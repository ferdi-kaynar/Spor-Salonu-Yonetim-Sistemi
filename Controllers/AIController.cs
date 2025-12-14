using FitnessSalonYonetim.Models;
using FitnessSalonYonetim.Services;
using FitnessSalonYonetim.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitnessSalonYonetim.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AIController> _logger;
        private readonly FitnessAIService _aiService;

        public AIController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<AIController> logger,
            FitnessAIService aiService)
        {
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
            _aiService = aiService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> GetEgzersizOnerisi([FromForm] AIRequestViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                // Form'dan gelen veya kullanıcı profilinden gelen verileri kullan
                var boyValue = model.Boy != 0 ? model.Boy : user?.Boy ?? 170;
                var kiloValue = model.Kilo != 0 ? model.Kilo : user?.Kilo ?? 70;
                var cinsiyetValue = !string.IsNullOrEmpty(model.Cinsiyet) ? model.Cinsiyet : user?.Cinsiyet ?? "Belirtilmemiş";
                var yasValue = model.Yas ?? 30;
                var aktiviteSeviyesiValue = model.AktiviteSeviyesi ?? "Orta Aktif";

                // Fotoğraf yükleme işlemi (opsiyonel - gelecekte analiz için kullanılabilir)
                if (model.Foto != null && model.Foto.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "ai");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var uniqueFileName = $"{user?.Id}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(model.Foto.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Foto.CopyToAsync(fileStream);
                    }
                    
                    _logger.LogInformation("Kullanıcı fotoğrafı yüklendi: {FileName}", uniqueFileName);
                }

                // Gömülü AI modeli ile öneri oluştur
                var oneri = _aiService.GeneratePersonalizedPlan(
                    boyValue, 
                    kiloValue, 
                    cinsiyetValue, 
                    model.Hedef, 
                    yasValue, 
                    aktiviteSeviyesiValue
                );

                return Json(new AIResponseViewModel
                {
                    Success = true,
                    OneriHTML = oneri
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI öneri oluşturulurken hata");
                return Json(new AIResponseViewModel
                {
                    Success = false,
                    Message = "Bir hata oluştu: " + ex.Message
                });
            }
        }
    }
}
