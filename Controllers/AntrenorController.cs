using FitnessSalonYonetim.Data;
using FitnessSalonYonetim.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessSalonYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AntrenorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AntrenorController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Antrenor
        public async Task<IActionResult> Index()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.Salon)
                .ToListAsync();
            return View(antrenorler);
        }

        // GET: Antrenor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.Musaitlikler)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (antrenor == null)
            {
                return NotFound();
            }

            return View(antrenor);
        }

        // GET: Antrenor/Create
        public IActionResult Create()
        {
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad");
            ViewData["Hizmetler"] = _context.Hizmetler.ToList();
            return View();
        }

        // POST: Antrenor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ad,Soyad,Email,Telefon,UzmanlikAlanlari,Biyografi,Aktif,SalonId")] Antrenor antrenor, List<int> selectedHizmetler)
        {
            if (ModelState.IsValid)
            {
                _context.Add(antrenor);
                await _context.SaveChangesAsync();

                // Hizmet ilişkilerini ekle
                if (selectedHizmetler != null && selectedHizmetler.Any())
                {
                    foreach (var hizmetId in selectedHizmetler)
                    {
                        _context.AntrenorHizmetler.Add(new AntrenorHizmet
                        {
                            AntrenorId = antrenor.Id,
                            HizmetId = hizmetId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Antrenör başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", antrenor.SalonId);
            ViewData["Hizmetler"] = _context.Hizmetler.ToList();
            return View(antrenor);
        }

        // GET: Antrenor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorHizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (antrenor == null)
            {
                return NotFound();
            }
            
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", antrenor.SalonId);
            ViewData["Hizmetler"] = _context.Hizmetler.ToList();
            ViewData["SelectedHizmetler"] = antrenor.AntrenorHizmetler.Select(ah => ah.HizmetId).ToList();
            return View(antrenor);
        }

        // POST: Antrenor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Soyad,Email,Telefon,UzmanlikAlanlari,Biyografi,ProfilResmi,Aktif,SalonId")] Antrenor antrenor, List<int> selectedHizmetler)
        {
            if (id != antrenor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenor);

                    // Mevcut hizmet ilişkilerini temizle
                    var mevcutIliskiler = _context.AntrenorHizmetler.Where(ah => ah.AntrenorId == id);
                    _context.AntrenorHizmetler.RemoveRange(mevcutIliskiler);

                    // Yeni hizmet ilişkilerini ekle
                    if (selectedHizmetler != null && selectedHizmetler.Any())
                    {
                        foreach (var hizmetId in selectedHizmetler)
                        {
                            _context.AntrenorHizmetler.Add(new AntrenorHizmet
                            {
                                AntrenorId = id,
                                HizmetId = hizmetId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Antrenör başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.Id))
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
            
            ViewData["SalonId"] = new SelectList(_context.Salonlar, "Id", "Ad", antrenor.SalonId);
            ViewData["Hizmetler"] = _context.Hizmetler.ToList();
            return View(antrenor);
        }

        // GET: Antrenor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (antrenor == null)
            {
                return NotFound();
            }

            return View(antrenor);
        }

        // POST: Antrenor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                _context.Antrenorler.Remove(antrenor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Antrenör başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.Id == id);
        }

        // Müsaitlik Yönetimi
        [HttpGet]
        public async Task<IActionResult> Musaitlik(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.Musaitlikler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null)
            {
                return NotFound();
            }

            return View(antrenor);
        }

        [HttpPost]
        public async Task<IActionResult> MusaitlikEkle(int antrenorId, DayOfWeek gun, TimeSpan baslangicSaati, TimeSpan bitisSaati)
        {
            var musaitlik = new AntrenorMusaitlik
            {
                AntrenorId = antrenorId,
                Gun = gun,
                BaslangicSaati = baslangicSaati,
                BitisSaati = bitisSaati,
                Aktif = true
            };

            _context.AntrenorMusaitlikler.Add(musaitlik);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Müsaitlik eklendi.";
            return RedirectToAction("Musaitlik", new { id = antrenorId });
        }

        [HttpPost]
        public async Task<IActionResult> MusaitlikSil(int id, int antrenorId)
        {
            var musaitlik = await _context.AntrenorMusaitlikler.FindAsync(id);
            if (musaitlik != null)
            {
                _context.AntrenorMusaitlikler.Remove(musaitlik);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Müsaitlik silindi.";
            }

            return RedirectToAction("Musaitlik", new { id = antrenorId });
        }
    }
}


