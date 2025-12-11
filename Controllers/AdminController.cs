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

            TempData["Success"] = $"{antrenor.AdSoyad} başarıyla {(antrenor.Aktif ? "aktif" : "pasif")} edildi.";
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

        // Üye ekleme (GET)
        [HttpGet]
        public IActionResult UyeEkle()
        {
            return View(new RegisterViewModel());
        }

        // Üye ekleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeEkle(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    KayitTarihi = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Member rolü ata
                    await _userManager.AddToRoleAsync(user, "Member");
                    
                    TempData["Success"] = $"{user.AdSoyad} başarıyla eklendi.";
                    return RedirectToAction(nameof(Uyeler));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Üye düzenleme (GET)
        [HttpGet]
        public async Task<IActionResult> UyeDuzenle(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var uye = await _userManager.FindByIdAsync(id);
            if (uye == null)
            {
                return NotFound();
            }

            // Admin kullanıcısını düzenlemeye izin verme
            if (await _userManager.IsInRoleAsync(uye, "Admin"))
            {
                TempData["ErrorMessage"] = "Admin kullanıcısı düzenlenemez!";
                return RedirectToAction(nameof(Uyeler));
            }

            var model = new ProfileViewModel
            {
                Ad = uye.Ad,
                Soyad = uye.Soyad,
                Email = uye.Email,
                Telefon = uye.PhoneNumber,
                DogumTarihi = uye.DogumTarihi,
                Cinsiyet = uye.Cinsiyet,
                Boy = uye.Boy,
                Kilo = uye.Kilo,
                Adres = uye.Adres,
                ProfilResmi = uye.ProfilResmi
            };

            ViewBag.UserId = id;
            return View(model);
        }

        // Üye düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeDuzenle(string id, ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = id;
                return View(model);
            }

            var uye = await _userManager.FindByIdAsync(id);
            if (uye == null)
            {
                return NotFound();
            }

            // Admin kullanıcısını düzenlemeye izin verme
            if (await _userManager.IsInRoleAsync(uye, "Admin"))
            {
                TempData["ErrorMessage"] = "Admin kullanıcısı düzenlenemez!";
                return RedirectToAction(nameof(Uyeler));
            }

            uye.Ad = model.Ad;
            uye.Soyad = model.Soyad;
            uye.PhoneNumber = model.Telefon;
            uye.DogumTarihi = model.DogumTarihi;
            uye.Cinsiyet = model.Cinsiyet;
            uye.Boy = model.Boy;
            uye.Kilo = model.Kilo;
            uye.Adres = model.Adres;

            var result = await _userManager.UpdateAsync(uye);

            if (result.Succeeded)
            {
                TempData["Success"] = $"{uye.AdSoyad} başarıyla güncellendi.";
                return RedirectToAction(nameof(Uyeler));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.UserId = id;
            return View(model);
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
                TempData["Success"] = $"{uye.AdSoyad} başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Uyeler));
        }

        // Üye Şifre Değiştirme (GET)
        [HttpGet]
        public async Task<IActionResult> UyeSifreDegistir(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var uye = await _userManager.FindByIdAsync(id);
            if (uye == null)
            {
                return NotFound();
            }

            ViewBag.UserId = id;
            ViewBag.UserName = uye.AdSoyad;
            return View(new AdminChangePasswordViewModel());
        }

        // Üye Şifre Değiştirme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeSifreDegistir(string id, AdminChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                ViewBag.UserId = id;
                ViewBag.UserName = user?.AdSoyad;
                return View(model);
            }

            var uye = await _userManager.FindByIdAsync(id);
            if (uye == null)
            {
                return NotFound();
            }

            // Eski şifreyi sıfırla ve yeni şifre ata
            var token = await _userManager.GeneratePasswordResetTokenAsync(uye);
            var result = await _userManager.ResetPasswordAsync(uye, token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = $"{uye.AdSoyad} kullanıcısının şifresi başarıyla değiştirildi.";
                return RedirectToAction(nameof(Uyeler));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.UserId = id;
            ViewBag.UserName = uye.AdSoyad;
            return View(model);
        }

        // Eğitmen ekleme (GET)
        [HttpGet]
        public async Task<IActionResult> EgitmenEkle()
        {
            ViewBag.Salonlar = await _context.Salonlar.Where(s => s.Aktif).ToListAsync();
            return View(new Antrenor());
        }

        // Eğitmen ekleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EgitmenEkle(Antrenor model)
        {
            if (ModelState.IsValid)
            {
                model.Aktif = true;
                _context.Antrenorler.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{model.AdSoyad} başarıyla eklendi.";
                return RedirectToAction(nameof(Egitmenler));
            }

            ViewBag.Salonlar = await _context.Salonlar.Where(s => s.Aktif).ToListAsync();
            return View(model);
        }

        // Eğitmen düzenleme (GET)
        [HttpGet]
        public async Task<IActionResult> EgitmenDuzenle(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null)
            {
                return NotFound();
            }

            ViewBag.Salonlar = await _context.Salonlar.Where(s => s.Aktif).ToListAsync();
            return View(antrenor);
        }

        // Eğitmen düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EgitmenDuzenle(int id, Antrenor model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"{model.AdSoyad} başarıyla güncellendi.";
                    return RedirectToAction(nameof(Egitmenler));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Antrenorler.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.Salonlar = await _context.Salonlar.Where(s => s.Aktif).ToListAsync();
            return View(model);
        }

        // Eğitmen silme
        [HttpPost]
        public async Task<IActionResult> EgitmenSil(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null)
            {
                return NotFound();
            }

            // İlişkili randevuları kontrol et
            var randevuVarMi = await _context.Randevular.AnyAsync(r => r.AntrenorId == id);
            if (randevuVarMi)
            {
                TempData["ErrorMessage"] = "Bu eğitmene ait randevular olduğu için silinemez. Önce pasif yapabilirsiniz.";
                return RedirectToAction(nameof(Egitmenler));
            }

            _context.Antrenorler.Remove(antrenor);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{antrenor.AdSoyad} başarıyla silindi.";
            return RedirectToAction(nameof(Egitmenler));
        }
    }
}

