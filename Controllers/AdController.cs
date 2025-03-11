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
            wwwRootPath = _hostEnvironment.WebRootPath;
        }

        // GET: Ad
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            var ads = _context.Ads
                .Include(a => a.Images)
                .Include(a => a.category)
                .Where(a => a.Status); // Visa endast aktiva annonser

            if (categoryId.HasValue)
            {
                ads = ads.Where(a => a.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                string lowerSearch = searchString.ToLower(); // Konvertera söktermen till gemener

                ads = ads.Where(a =>
                    a.Title.ToLower().Contains(lowerSearch) ||
                    a.Description.ToLower().Contains(lowerSearch) ||
                    (a.category != null && a.category.Name.ToLower().Contains(lowerSearch)) || // Kontrollera att category inte är null
                    (a.CreatedBy != null && a.CreatedBy.ToLower().Contains(lowerSearch)) // Kontrollera att CreatedBy inte är null
                );
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

            return View(await ads.ToListAsync());
        }


        // GET: Ad/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ad = await _context.Ads
                .Include(a => a.Images)
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
public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,ImageFiles,status,CategoryId")] Ad ad)
{
    if (ModelState.IsValid)
    {
        ad.Images = new List<AdImage>();

        if (ad.ImageFiles != null && ad.ImageFiles.Any())
        {
            foreach (var imageFile in ad.ImageFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                string extension = Path.GetExtension(imageFile.FileName);
                string uniqueFileName = $"{fileName.Replace(" ", string.Empty)}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
                string filePath = Path.Combine(wwwRootPath, "images", uniqueFileName);

                // Spara bilden
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Skapa miniatyr
                CreateImageFiles(uniqueFileName);


                // Lägg till i databasen
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
        return RedirectToAction(nameof(Index));
    }

    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ad.CategoryId);
    return View(ad);
}

[Authorize]
[HttpGet]
public async Task<IActionResult> Buy(int id)
{
    var ad = await _context.Ads.FindAsync(id);
    if (ad == null)
    {
        return NotFound();
    }

    return View("Details", ad);
}


[Authorize]
[HttpGet]
public IActionResult BuyRedirect(int id)
{
    if (!User.Identity?.IsAuthenticated ?? true) 
    {
        return RedirectToPage("/Identity/Account/Login", new { returnUrl = Url.Action("Buy", "Ad", new { id }) });
    }

    return RedirectToAction("Details", new { id });
}



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int id, string? returnUrl = null)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Identity/Account/Login", new { returnUrl = Url.Action("Details", new { id }) });
            }

            var ad = await _context.Ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            if (!ad.Status)
            {
                return BadRequest("Denna vara är redan såld.");
            }

            ad.Status = false;
            ad.Buyer = User.Identity?.Name ?? "Okänd köpare";
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }



        // GET: Ad/Edit/5
public async Task<IActionResult> Edit(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var ad = await _context.Ads
        .Include(a => a.Images)
        .FirstOrDefaultAsync(m => m.Id == id);

    if (ad == null)
    {
        return NotFound();
    }

    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", ad.CategoryId);
    return View(ad);
}

[Authorize] // Kräver att användaren är inloggad
public async Task<IActionResult> MyAds()
{
    var userName = User.Identity?.Name;

    if (userName == null)
    {
        return RedirectToAction("Login", "Account");
    }

    var myAds = await _context.Ads
        .Include(a => a.Images)
        .Include(a => a.category)
        .Where(a => a.CreatedBy == userName)
        .ToListAsync();

    return View(myAds);
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
    string extension = Path.GetExtension(fileName).ToLower();

    if (string.IsNullOrEmpty(extension))
    {
        return;
    }

    string thumbFileName = $"thumb_{Path.GetFileNameWithoutExtension(fileName)}.jpg";
    string thumbPath = Path.Combine(wwwRootPath, "images", thumbFileName);

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
}}


}


