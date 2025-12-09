using FitnessSalonYonetim.Data;
using FitnessSalonYonetim.Models;
using FitnessSalonYonetim.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessSalonYonetim.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // Admin Giriş Sayfası
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Admin Giriş İşlemi
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Email ile kullanıcıyı bul
                var user = await _userManager.FindByEmailAsync(model.Email);
                
                if (user != null)
                {
                    // Admin rolü kontrolü
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    if (!isAdmin)
                    {
                        ModelState.AddModelError(string.Empty, "Bu sayfa sadece admin kullanıcıları içindir.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Admin giriş yaptı: {UserName}", user.UserName);
                        return RedirectToAction(nameof(Dashboard));
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Geçersiz admin email veya şifre.");
                return View(model);
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Dashboard()
        {
            var toplamSalon = await _context.Salonlar.CountAsync();
            var toplamAntrenor = await _context.Antrenorler.CountAsync();
            var toplamUye = await _context.Users.CountAsync();
            var toplamRandevu = await _context.Randevular.CountAsync();
            var bekleyenRandevuSayisi = await _context.Randevular
                .CountAsync(r => r.Durum == RandevuDurumu.Beklemede);
            var onaylananRandevuSayisi = await _context.Randevular
                .CountAsync(r => r.Durum == RandevuDurumu.Onaylandi);
            var tamamlananRandevuSayisi = await _context.Randevular
                .CountAsync(r => r.Durum == RandevuDurumu.Tamamlandi);

            ViewBag.ToplamSalon = toplamSalon;
            ViewBag.ToplamAntrenor = toplamAntrenor;
            ViewBag.ToplamUye = toplamUye;
            ViewBag.ToplamRandevu = toplamRandevu;
            ViewBag.BekleyenRandevuSayisi = bekleyenRandevuSayisi;
            ViewBag.OnaylananRandevuSayisi = onaylananRandevuSayisi;
            ViewBag.TamamlananRandevuSayisi = tamamlananRandevuSayisi;

            // Son randevular
            var sonRandevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.OlusturmaTarihi)
                .Take(10)
                .ToListAsync();

            // Bekleyen randevular
            var bekleyenRandevularList = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.Durum == RandevuDurumu.Beklemede)
                .OrderBy(r => r.RandevuTarihi)
                .ThenBy(r => r.BaslangicSaati)
                .Take(10)
                .ToListAsync();

            ViewBag.BekleyenRandevularList = bekleyenRandevularList;

            return View(sonRandevular);
        }

        // Eğitmen Yönetimi
        public async Task<IActionResult> Egitmenler()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .OrderBy(a => a.Ad)
                .ToListAsync();

            return View(antrenorler);
        }

        // Eğitmen aktif/pasif durumu değiştir
        [HttpPost]
        public async Task<IActionResult> ToggleEgitmen(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null)
            {
                return NotFound();
            }

            antrenor.Aktif = !antrenor.Aktif;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{antrenor.AdSoyad} başarıyla {(antrenor.Aktif ? "aktif" : "pasif")} edildi.";
            return RedirectToAction(nameof(Egitmenler));
        }

        // Üye Yönetimi
        public async Task<IActionResult> Uyeler()
        {
            var uyeler = await _context.Users
                .Include(u => u.Randevular)
                .OrderByDescending(u => u.KayitTarihi)
                .ToListAsync();

            // Her üyenin rolünü kontrol et
            var uyelerVeRoller = new List<(ApplicationUser uye, bool isAdmin)>();
            foreach (var uye in uyeler)
            {
                var isAdmin = await _userManager.IsInRoleAsync(uye, "Admin");
                uyelerVeRoller.Add((uye, isAdmin));
            }

            return View(uyelerVeRoller);
        }

        // Üye detayları
        public async Task<IActionResult> UyeDetay(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var uye = await _context.Users
                .Include(u => u.Randevular)
                    .ThenInclude(r => r.Antrenor)
                .Include(u => u.Randevular)
                    .ThenInclude(r => r.Hizmet)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uye == null)
            {
                return NotFound();
            }

            return View(uye);
        }

        // Üye silme
        [HttpPost]
        public async Task<IActionResult> UyeSil(string id)
        {
            var uye = await _userManager.FindByIdAsync(id);
            if (uye == null)
            {
                return NotFound();
            }

            // Admin kullanıcısını silme
            if (await _userManager.IsInRoleAsync(uye, "Admin"))
            {
                TempData["ErrorMessage"] = "Admin kullanıcısı silinemez!";
                return RedirectToAction(nameof(Uyeler));
            }

            var result = await _userManager.DeleteAsync(uye);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"{uye.AdSoyad} başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Uyeler));
        }
    }
}

