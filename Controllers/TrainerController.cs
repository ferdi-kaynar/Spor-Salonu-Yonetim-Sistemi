using FitnessSalonYonetim.Data;
using FitnessSalonYonetim.Models;
using FitnessSalonYonetim.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessSalonYonetim.Controllers
{
    public class TrainerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<TrainerController> _logger;

        public TrainerController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TrainerController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // Eğitmen Giriş Sayfası
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [Authorize(Roles = "Trainer")]

        // Eğitmen Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.AntrenorId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.Musaitlikler)
                .FirstOrDefaultAsync(a => a.Id == user.AntrenorId);

            if (antrenor == null)
            {
                return NotFound();
            }

            // Bugünkü randevular
            var bugun = DateTime.Today;
            var bugunRandevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == antrenor.Id && r.RandevuTarihi.Date == bugun)
                .OrderBy(r => r.BaslangicSaati)
                .ToListAsync();

            // Yaklaşan randevular
            var yaklasanRandevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == antrenor.Id && r.RandevuTarihi > bugun)
                .OrderBy(r => r.RandevuTarihi)
                .ThenBy(r => r.BaslangicSaati)
                .Take(5)
                .ToListAsync();

            // Bekleyen randevular
            var bekleyenRandevularList = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == antrenor.Id && r.Durum == RandevuDurumu.Beklemede)
                .OrderBy(r => r.RandevuTarihi)
                .ThenBy(r => r.BaslangicSaati)
                .Take(10)
                .ToListAsync();

            // İstatistikler
            var toplamRandevu = await _context.Randevular
                .Where(r => r.AntrenorId == antrenor.Id)
                .CountAsync();

            var tamamlananRandevu = await _context.Randevular
                .Where(r => r.AntrenorId == antrenor.Id && r.Durum == RandevuDurumu.Tamamlandi)
                .CountAsync();

            var bekleyenRandevu = await _context.Randevular
                .Where(r => r.AntrenorId == antrenor.Id && r.Durum == RandevuDurumu.Beklemede)
                .CountAsync();

            var toplamUye = await _context.Randevular
                .Where(r => r.AntrenorId == antrenor.Id)
                .Select(r => r.UyeId)
                .Distinct()
                .CountAsync();

            ViewBag.Antrenor = antrenor;
            ViewBag.BugunRandevular = bugunRandevular;
            ViewBag.YaklasanRandevular = yaklasanRandevular;
            ViewBag.BekleyenRandevularList = bekleyenRandevularList;
            ViewBag.ToplamRandevu = toplamRandevu;
            ViewBag.TamamlananRandevu = tamamlananRandevu;
            ViewBag.BekleyenRandevu = bekleyenRandevu;
            ViewBag.ToplamUye = toplamUye;

            return View();
        }

        // Tüm Randevular
        public async Task<IActionResult> Randevular()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.AntrenorId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var randevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == user.AntrenorId)
                .OrderByDescending(r => r.RandevuTarihi)
                .ThenByDescending(r => r.BaslangicSaati)
                .ToListAsync();

            var antrenor = await _context.Antrenorler.FindAsync(user.AntrenorId);
            ViewBag.Antrenor = antrenor;

            return View(randevular);
        }

        // Müsaitlikler
        public async Task<IActionResult> Musaitlikler()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.AntrenorId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.Musaitlikler)
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == user.AntrenorId);

            if (antrenor == null)
            {
                return NotFound();
            }

            return View(antrenor);
        }

        // Profil
        public async Task<IActionResult> Profil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.AntrenorId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.Musaitlikler)
                .FirstOrDefaultAsync(a => a.Id == user.AntrenorId);

            if (antrenor == null)
            {
                return NotFound();
            }

            ViewBag.User = user;
            return View(antrenor);
        }

        // Üyelerim
        public async Task<IActionResult> Uyeler()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.AntrenorId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Bu eğitmenden randevu alan tüm üyeleri getir
            var uyeler = await _context.Randevular
                .Include(r => r.Uye)
                .Where(r => r.AntrenorId == user.AntrenorId)
                .GroupBy(r => r.UyeId)
                .Select(g => new
                {
                    Uye = g.First().Uye,
                    ToplamRandevu = g.Count(),
                    SonRandevu = g.Max(r => r.RandevuTarihi),
                    TamamlananRandevu = g.Count(r => r.Durum == RandevuDurumu.Tamamlandi)
                })
                .ToListAsync();

            var antrenor = await _context.Antrenorler.FindAsync(user.AntrenorId);
            ViewBag.Antrenor = antrenor;
            ViewBag.Uyeler = uyeler;

            return View();
        }

        // Randevu Onaylama
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOnayla(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.AntrenorId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Randevular));
            }

            // Sadece kendi randevusunu onaylayabilir
            if (randevu.AntrenorId != user.AntrenorId)
            {
                TempData["Error"] = "Bu randevuyu onaylama yetkiniz yok.";
                return RedirectToAction(nameof(Randevular));
            }

            randevu.Durum = RandevuDurumu.Onaylandi;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevu başarıyla onaylandı.";
            
            return RedirectToAction(nameof(Randevular));
        }

        // Randevu Reddetme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuReddet(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.AntrenorId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Randevular));
            }

            // Sadece kendi randevusunu reddedebilir
            if (randevu.AntrenorId != user.AntrenorId)
            {
                TempData["Error"] = "Bu randevuyu reddetme yetkiniz yok.";
                return RedirectToAction(nameof(Randevular));
            }

            randevu.Durum = RandevuDurumu.Reddedildi;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevu reddedildi.";
            
            return RedirectToAction(nameof(Randevular));
        }

        // Randevu Tamamlama
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuTamamla(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.AntrenorId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Randevular));
            }

            // Sadece kendi randevusunu tamamlayabilir
            if (randevu.AntrenorId != user.AntrenorId)
            {
                TempData["Error"] = "Bu randevuyu tamamlama yetkiniz yok.";
                return RedirectToAction(nameof(Randevular));
            }

            randevu.Durum = RandevuDurumu.Tamamlandi;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevu tamamlandı olarak işaretlendi.";
            
            return RedirectToAction(nameof(Randevular));
        }

        // Randevu İptal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuIptal(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.AntrenorId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                TempData["Error"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Randevular));
            }

            // Sadece kendi randevusunu iptal edebilir
            if (randevu.AntrenorId != user.AntrenorId)
            {
                TempData["Error"] = "Bu randevuyu iptal etme yetkiniz yok.";
                return RedirectToAction(nameof(Randevular));
            }

            randevu.Durum = RandevuDurumu.IptalEdildi;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Randevu iptal edildi.";
            
            return RedirectToAction(nameof(Randevular));
        }
    }
}

