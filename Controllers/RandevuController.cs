using FitnessSalonYonetim.Data;
using FitnessSalonYonetim.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessSalonYonetim.Controllers
{
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RandevuController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Randevu
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            IQueryable<Randevu> randevular;

            if (User.IsInRole("Admin"))
            {
                randevular = _context.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet);
            }
            else
            {
                randevular = _context.Randevular
                    .Where(r => r.UyeId == user!.Id)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet);
            }

            return View(await randevular.OrderByDescending(r => r.RandevuTarihi).ToListAsync());
        }

        // GET: Randevu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (randevu == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && randevu.UyeId != user!.Id)
            {
                return Forbid();
            }

            return View(randevu);
        }

        // GET: Randevu/Create
        public IActionResult Create()
        {
            ViewData["Hizmetler"] = new SelectList(_context.Hizmetler.Where(h => h.Aktif), "Id", "Ad");
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetAntrenorler(int hizmetId)
        {
            var antrenorler = await _context.AntrenorHizmetler
                .Where(ah => ah.HizmetId == hizmetId && ah.Antrenor.Aktif)
                .Select(ah => new
                {
                    id = ah.AntrenorId,
                    text = ah.Antrenor.Ad + " " + ah.Antrenor.Soyad
                })
                .ToListAsync();

            return Json(antrenorler);
        }

        [HttpGet]
        public async Task<JsonResult> GetMusaitSaatler(int antrenorId, DateTime tarih)
        {
            var gun = tarih.DayOfWeek;
            
            // Antrenörün o güne ait müsaitlik saatlerini al
            var musaitlikler = await _context.AntrenorMusaitlikler
                .Where(m => m.AntrenorId == antrenorId && m.Gun == gun && m.Aktif)
                .ToListAsync();

            if (!musaitlikler.Any())
            {
                return Json(new List<object>());
            }

            // O gün için mevcut randevuları al
            var mevcutRandevular = await _context.Randevular
                .Where(r => r.AntrenorId == antrenorId && 
                           r.RandevuTarihi.Date == tarih.Date &&
                           r.Durum != RandevuDurumu.IptalEdildi &&
                           r.Durum != RandevuDurumu.Reddedildi)
                .ToListAsync();

            var musaitSaatler = new List<object>();

            foreach (var musaitlik in musaitlikler)
            {
                var baslangic = musaitlik.BaslangicSaati;
                var bitis = musaitlik.BitisSaati;
                
                // Her yarım saatlik dilimi kontrol et
                while (baslangic.Add(TimeSpan.FromMinutes(30)) <= bitis)
                {
                    var saatBitis = baslangic.Add(TimeSpan.FromMinutes(60));
                    
                    // Bu saatte randevu var mı kontrol et
                    var randevuVar = mevcutRandevular.Any(r =>
                        (r.BaslangicSaati < saatBitis && r.BitisSaati > baslangic));

                    if (!randevuVar)
                    {
                        musaitSaatler.Add(new
                        {
                            value = baslangic.ToString(@"hh\:mm"),
                            text = baslangic.ToString(@"hh\:mm")
                        });
                    }

                    baslangic = baslangic.Add(TimeSpan.FromMinutes(30));
                }
            }

            return Json(musaitSaatler);
        }

        // POST: Randevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HizmetId,AntrenorId,RandevuTarihi,BaslangicSaati,Notlar")] Randevu randevu)
        {
            var user = await _userManager.GetUserAsync(User);
            randevu.UyeId = user!.Id;

            // Hizmet bilgilerini al
            var hizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
            if (hizmet == null)
            {
                ModelState.AddModelError("", "Hizmet bulunamadı.");
                ViewData["Hizmetler"] = new SelectList(_context.Hizmetler.Where(h => h.Aktif), "Id", "Ad");
                return View(randevu);
            }

            randevu.BitisSaati = randevu.BaslangicSaati.Add(TimeSpan.FromMinutes(hizmet.Sure));
            randevu.Ucret = hizmet.Ucret;
            randevu.Durum = RandevuDurumu.Beklemede;

            // Müsaitlik kontrolü
            var gun = randevu.RandevuTarihi.DayOfWeek;
            var musaitMi = await _context.AntrenorMusaitlikler
                .AnyAsync(m => m.AntrenorId == randevu.AntrenorId &&
                              m.Gun == gun &&
                              m.BaslangicSaati <= randevu.BaslangicSaati &&
                              m.BitisSaati >= randevu.BitisSaati &&
                              m.Aktif);

            if (!musaitMi)
            {
                ModelState.AddModelError("", "Seçilen antrenör bu saatte müsait değil.");
                ViewData["Hizmetler"] = new SelectList(_context.Hizmetler.Where(h => h.Aktif), "Id", "Ad");
                return View(randevu);
            }

            // Çakışma kontrolü
            var cakismaVar = await _context.Randevular
                .AnyAsync(r => r.AntrenorId == randevu.AntrenorId &&
                              r.RandevuTarihi.Date == randevu.RandevuTarihi.Date &&
                              r.BaslangicSaati < randevu.BitisSaati &&
                              r.BitisSaati > randevu.BaslangicSaati &&
                              r.Durum != RandevuDurumu.IptalEdildi &&
                              r.Durum != RandevuDurumu.Reddedildi);

            if (cakismaVar)
            {
                ModelState.AddModelError("", "Bu saatte başka bir randevu mevcut. Lütfen başka bir saat seçiniz.");
                ViewData["Hizmetler"] = new SelectList(_context.Hizmetler.Where(h => h.Aktif), "Id", "Ad");
                return View(randevu);
            }

            _context.Add(randevu);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevunuz başarıyla oluşturuldu. Onay beklemektedir.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Randevu/Iptal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iptal(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && randevu.UyeId != user!.Id)
            {
                return Forbid();
            }

            randevu.Durum = RandevuDurumu.IptalEdildi;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevu iptal edildi.";
            
            return RedirectToAction(nameof(Index));
        }

        // Admin için onay/red işlemleri
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            randevu.Durum = RandevuDurumu.Onaylandi;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevu onaylandı.";
            
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            randevu.Durum = RandevuDurumu.Reddedildi;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevu reddedildi.";
            
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tamamla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            randevu.Durum = RandevuDurumu.Tamamlandi;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevu tamamlandı olarak işaretlendi.";
            
            return RedirectToAction(nameof(Index));
        }
    }
}

