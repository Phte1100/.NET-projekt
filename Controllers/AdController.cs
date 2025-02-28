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
            wwwRootPath = _hostEnvironment.WebRootPath;
        }

        // GET: Ad
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ads.Include(a => a.category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ad/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ad = await _context.Ads
                .Include(a => a.category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ad == null)
            {
                return NotFound();
            }

            return View(ad);
        }

        // GET: Ad/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Ad/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,ImageFile,status,CategoryId")] Ad ad)
        {
            if (ModelState.IsValid)
            {
                // kopiera filen till wwwroot/images
                if(ad.ImageFile != null)
{ 
                    // Generera unikt filnamn MED FILÄNDELSE
                    string fileName = Path.GetFileNameWithoutExtension(ad.ImageFile.FileName);
                    string extension = Path.GetExtension(ad.ImageFile.FileName);
                    ad.ImageName = fileName.Replace(" ", String.Empty) + DateTime.Now.ToString("yymmssfff") + extension; // Filändelse läggs till
                    
                    string path = Path.Combine(wwwRootPath, "images", ad.ImageName); // Spara filen med korrekt namn

                    // Lagra i filsystemet
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await ad.ImageFile.CopyToAsync(fileStream);
                        await Task.Delay(200); // Vänta 200ms innan vidare hantering
                    }

                    // Skapa miniatyrbild
                    CreateImageFiles(ad.ImageName);
                } 
                else
                {
                    ad.ImageName = "default.png";
                }


                _context.Add(ad);

                // skapad av användar
                ad.CreatedBy = User.Identity?.Name ?? "Unknown";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ad.CategoryId);
            return View(ad);
        }

        // GET: Ad/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ad = await _context.Ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ad.CategoryId);
            return View(ad);
        }

        // POST: Ad/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,ImageFile,status,CategoryId")] Ad ad)
        {
            if (id != ad.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdExists(ad.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ad.CategoryId);
            return View(ad);
        }

        // GET: Ad/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ad = await _context.Ads
                .Include(a => a.category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ad == null)
            {
                return NotFound();
            }

            return View(ad);
        }

        // POST: Ad/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad != null)
            {
                _context.Ads.Remove(ad);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdExists(int id)
        {
            return _context.Ads.Any(e => e.Id == id);
        }

private void CreateImageFiles(string fileName)
{
    string imagePath = Path.Combine(wwwRootPath, "images", fileName);
    string extension = Path.GetExtension(fileName).ToLower(); // Hämta filändelsen

    if (string.IsNullOrEmpty(extension))
    {
        return;
    }

    // Skapa ett nytt filnamn för JPEG
    string thumbFileName = $"thumb_{Path.GetFileNameWithoutExtension(fileName)}.jpg";
    string thumbPath = Path.Combine(wwwRootPath, "images", thumbFileName);

    try
    {
        Task.Delay(500).Wait(); // Vänta lite så att filen är helt sparad

        using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using var image = Image.Load(stream);

            // Anpassa bildstorlek (skapa en miniatyr)
            image.Mutate(x => x.Resize(image.Width / 4, image.Height / 4));

            // Spara som JPEG
            var jpegEncoder = new JpegEncoder
            {
                Quality = 80 // Justera kvaliteten mellan 0-100 (80 ger bra balans mellan storlek & kvalitet)
            };
            image.Save(thumbPath, jpegEncoder);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fel vid skapande av miniatyrbild: {ex.Message}");
    }
}}


}


