using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using moment5.Models;
using projekt.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.AspNetCore.Authorization;

namespace projekt.Controllers
{
    public class AdController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string wwwRootPath;

        public AdController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;

            // Uppdaterad sökväg för att hantera wwwroot/wwwroot
            wwwRootPath = Path.Combine(_hostEnvironment.WebRootPath, "wwwroot");
            Console.WriteLine($"WWW Root Path: {wwwRootPath}");
        }

        // GET: Ad
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            var ads = await _context.Ads
                .Include(a => a.Images)
                .Include(a => a.category)
                .ToListAsync();

            if (categoryId.HasValue)
            {
                ads = ads.Where(a => a.CategoryId == categoryId).ToList();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                string lowerSearch = searchString.ToLower();

                ads = ads.Where(a =>
                    a.Title.ToLower().Contains(lowerSearch) ||
                    a.Description.ToLower().Contains(lowerSearch) ||
                    (a.category != null && a.category.Name.ToLower().Contains(lowerSearch)) ||
                    (a.CreatedBy != null && a.CreatedBy.ToLower().Contains(lowerSearch))
                ).ToList();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

            return View(ads);
        }

        // GET: Ad/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Ad/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,ImageFiles,status,CategoryId")] Ad ad)
        {
            if (ModelState.IsValid)
            {
                ad.Images = new List<AdImage>();

                if (ad.ImageFiles != null && ad.ImageFiles.Any())
                {
                    // Se till att images-katalogen finns
                    string imageDir = Path.Combine(wwwRootPath, "images");
                    Directory.CreateDirectory(imageDir);

                    foreach (var imageFile in ad.ImageFiles)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                        string extension = Path.GetExtension(imageFile.FileName);
                        string uniqueFileName = $"{fileName.Replace(" ", string.Empty)}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
                        string filePath = Path.Combine(imageDir, uniqueFileName);

                        Console.WriteLine($"📂 Sparar bild till: {filePath}");

                        try
                        {
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fileStream);
                            }

                            Console.WriteLine($"✅ Bild sparad: {filePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Fel vid bildspara: {ex.Message}");
                        }

                        // Lägg till bilden i databasen
                        ad.Images.Add(new AdImage { ImageName = uniqueFileName });
                    }
                }
                else
                {
                    ad.Images.Add(new AdImage { ImageName = "default.png" });
                }

                ad.CreatedBy = User.Identity?.Name ?? "Okänd";
                _context.Add(ad);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Annonsen har skapats!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ad.CategoryId);
            return View(ad);
        }

        // POST: Ad/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ad = await _context.Ads.Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == id);

            if (ad != null)
            {
                string imageDir = Path.Combine(wwwRootPath, "images");

                foreach (var image in ad.Images)
                {
                    string filePath = Path.Combine(imageDir, image.ImageName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Ads.Remove(ad);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Annonsen har raderats!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.AdImages.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            string imageDir = Path.Combine(wwwRootPath, "images");
            string filePath = Path.Combine(imageDir, image.ImageName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.AdImages.Remove(image);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bilden har raderats!";
            return RedirectToAction("Edit", new { id = image.AdId });
        }

        private void CreateImageFiles(string fileName)
        {
            string imageDir = Path.Combine(wwwRootPath, "images");
            string imagePath = Path.Combine(imageDir, fileName);
            string extension = Path.GetExtension(fileName).ToLower();

            if (string.IsNullOrEmpty(extension))
            {
                return;
            }

            string thumbFileName = $"thumb_{Path.GetFileNameWithoutExtension(fileName)}.jpg";
            string thumbPath = Path.Combine(imageDir, thumbFileName);

            try
            {
                Task.Delay(500).Wait();

                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using var image = Image.Load(stream);
                    image.Mutate(x => x.Resize(image.Width / 4, image.Height / 4));

                    var jpegEncoder = new JpegEncoder { Quality = 80 };
                    image.Save(thumbPath, jpegEncoder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Fel vid bildhantering]: {ex.Message}");
            }
        }
    }
}
