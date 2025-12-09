using FitnessSalonYonetim.Models;
using FitnessSalonYonetim.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;
using System.Text;

namespace FitnessSalonYonetim.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AIController> _logger;
        private readonly string? _openAIKey;

        public AIController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<AIController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
            _openAIKey = _configuration["OpenAI:ApiKey"];
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

                // BMI hesapla
                var bmi = CalculateBMI(kiloValue, boyValue);

                string oneri;

                // OpenAI API kullanımı (eğer API key varsa)
                if (!string.IsNullOrEmpty(_openAIKey) && _openAIKey != "OPENAI_API_KEY_BURAYA_GIRILECEK")
                {
                    oneri = await GetOpenAIRecommendation(boyValue, kiloValue, cinsiyetValue, model.Hedef, model.Yas, model.AktiviteSeviyesi, bmi, model.Foto);
                }
                else
                {
                    // Fallback: Yerel algoritma
                    oneri = GenerateEgzersizOnerisi(boyValue, kiloValue, cinsiyetValue, model.Hedef, bmi);
                }

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

        [HttpPost]
        public async Task<IActionResult> GenerateMotivationImage([FromForm] AIImageGenerationViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(_openAIKey) || _openAIKey == "OPENAI_API_KEY_BURAYA_GIRILECEK")
                {
                    return Json(new AIResponseViewModel
                    {
                        Success = false,
                        Message = "OpenAI API anahtarı yapılandırılmamış. Lütfen sistem yöneticisiyle iletişime geçin."
                    });
                }

                var imageUrl = await GenerateDALLEImage(model);

                return Json(new AIResponseViewModel
                {
                    Success = true,
                    ImageUrl = imageUrl,
                    Message = "Görsel başarıyla oluşturuldu!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DALL-E görsel oluşturulurken hata");
                return Json(new AIResponseViewModel
                {
                    Success = false,
                    Message = "Görsel oluşturulurken hata: " + ex.Message
                });
            }
        }

        private async Task<string> GetOpenAIRecommendation(
            int boy, decimal kilo, string cinsiyet, string hedef,
            int? yas, string? aktiviteSeviyesi, decimal bmi, IFormFile? foto)
        {
            try
            {
                var apiKey = new ApiKeyCredential(_openAIKey!);
                var chatClient = new ChatClient("gpt-4", apiKey);

                // Prompt oluştur
                var prompt = $@"
Bir fitness uzmanı olarak, aşağıdaki bilgilere sahip kişi için detaylı bir egzersiz ve beslenme planı oluştur:

Kişisel Bilgiler:
- Boy: {boy} cm
- Kilo: {kilo} kg
- Cinsiyet: {cinsiyet}
- BMI: {bmi:F2} ({GetBMICategory(bmi)})
{(yas.HasValue ? $"- Yaş: {yas}" : "")}
{(!string.IsNullOrEmpty(aktiviteSeviyesi) ? $"- Aktivite Seviyesi: {aktiviteSeviyesi}" : "")}

Hedef: {hedef}

Lütfen aşağıdaki başlıklar altında detaylı bir plan hazırla:

1. Kişisel Değerlendirme
2. Önerilen Egzersiz Programı (haftalık program)
3. Beslenme Önerileri (örnek günlük menü)
4. Motivasyon ve İpuçları
5. Dikkat Edilmesi Gerekenler

Cevabını HTML formatında, ul, li, strong, h4, h5 etiketleri kullanarak ver. Div veya p etiketi kullanma.
";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("Sen profesyonel bir fitness uzmanı ve diyetisyensin. Kişiye özel, bilimsel temelli, uygulanabilir programlar oluşturuyorsun."),
                    new UserChatMessage(prompt)
                };

                var response = await chatClient.CompleteChatAsync(messages);
                var content = response.Value.Content[0].Text;

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI API çağrısı başarısız");
                // Fallback
                return GenerateEgzersizOnerisi(boy, kilo, cinsiyet, hedef, bmi);
            }
        }

        private async Task<string> GenerateDALLEImage(AIImageGenerationViewModel model)
        {
            var apiKey = new ApiKeyCredential(_openAIKey!);
            var imageClient = new ImageClient("dall-e-3", apiKey);

            var cinsiyetPrompt = model.Cinsiyet.ToLower() switch
            {
                "erkek" => "athletic male",
                "kadın" => "athletic female",
                _ => "athletic person"
            };

            var vucutTipiPrompt = model.HedefVucutTipi.ToLower() switch
            {
                "kaslı" => "muscular, well-defined body",
                "zayıf" => "lean, toned physique",
                "fit" => "fit and healthy body",
                "atletik" => "athletic, strong physique",
                _ => "healthy, fit body"
            };

            var egzersizPrompt = !string.IsNullOrEmpty(model.EgzersizTuru)
                ? $"doing {model.EgzersizTuru}"
                : "in a fitness pose";

            var prompt = $@"A photorealistic image of a {cinsiyetPrompt} with {vucutTipiPrompt}, 
{egzersizPrompt}, in a modern gym setting, professional fitness photography style, 
motivational, high quality, natural lighting, healthy and strong appearance";

            var options = new ImageGenerationOptions
            {
                Size = GeneratedImageSize.W1024xH1024,
                Quality = GeneratedImageQuality.Standard,
                Style = GeneratedImageStyle.Natural
            };

            var imageGeneration = await imageClient.GenerateImageAsync(prompt, options);
            var imageUrl = imageGeneration.Value.ImageUri.ToString();

            return imageUrl;
        }

        private decimal CalculateBMI(decimal? kilo, int? boy)
        {
            if (!kilo.HasValue || !boy.HasValue || boy.Value == 0)
                return 0;

            var boyMetre = boy.Value / 100m;
            return kilo.Value / (boyMetre * boyMetre);
        }

        private string GenerateEgzersizOnerisi(int boy, decimal kilo, string cinsiyet, string hedef, decimal bmi)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"<h4>Kişisel Bilgileriniz:</h4>");
            sb.AppendLine($"<ul>");
            sb.AppendLine($"<li><strong>Boy:</strong> {boy} cm</li>");
            sb.AppendLine($"<li><strong>Kilo:</strong> {kilo} kg</li>");
            sb.AppendLine($"<li><strong>Cinsiyet:</strong> {cinsiyet}</li>");
            sb.AppendLine($"<li><strong>BMI (Vücut Kitle İndeksi):</strong> {bmi:F2}</li>");
            sb.AppendLine($"<li><strong>BMI Kategorisi:</strong> {GetBMICategory(bmi)}</li>");
            sb.AppendLine($"</ul>");

            sb.AppendLine($"<h4>Hedefiniz: {hedef}</h4>");

            // Hedefe göre öneriler
            switch (hedef.ToLower())
            {
                case "kilo verme":
                    sb.AppendLine("<h5>Önerilen Egzersizler:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li><strong>Kardiyovasküler Egzersizler:</strong> Haftada 5 gün, her seansta 30-45 dakika tempolu yürüyüş, koşu veya bisiklet</li>");
                    sb.AppendLine("<li><strong>HIIT (Yüksek Yoğunluklu Interval Antrenmanı):</strong> Haftada 3 gün, 20-30 dakika</li>");
                    sb.AppendLine("<li><strong>Direnç Antrenmanı:</strong> Haftada 2-3 gün, tüm vücut egzersizleri</li>");
                    sb.AppendLine("<li><strong>Yüzme:</strong> Haftada 2-3 gün, 30-40 dakika</li>");
                    sb.AppendLine("</ul>");
                    sb.AppendLine("<h5>Beslenme Önerileri:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li>Kalori açığı oluşturun (günlük ihtiyacınızdan 500 kalori az)</li>");
                    sb.AppendLine("<li>Bol miktarda protein tüketin (1.6-2.2g/kg vücut ağırlığı)</li>");
                    sb.AppendLine("<li>İşlenmiş gıdalardan kaçının</li>");
                    sb.AppendLine("<li>Bol su için (günde en az 2-3 litre)</li>");
                    sb.AppendLine("<li>Sabah kahvaltısını atlamamaya özen gösterin</li>");
                    sb.AppendLine("</ul>");
                    break;

                case "kas geliştirme":
                case "kas kazanma":
                    sb.AppendLine("<h5>Önerilen Egzersizler:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li><strong>Ağırlık Antrenmanı:</strong> Haftada 4-6 gün, split program (göğüs-triceps, sırt-biceps, bacak-omuz)</li>");
                    sb.AppendLine("<li><strong>Bileşik Hareketler:</strong> Squat, deadlift, bench press, overhead press</li>");
                    sb.AppendLine("<li><strong>İzolasyon Hareketleri:</strong> Belirli kas gruplarını hedefleyen egzersizler</li>");
                    sb.AppendLine("<li><strong>Progresif Yüklenme:</strong> Haftalık olarak ağırlık veya tekrar sayısını artırın</li>");
                    sb.AppendLine("</ul>");
                    sb.AppendLine("<h5>Beslenme Önerileri:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li>Kalori fazlası oluşturun (günlük ihtiyacınızdan 300-500 kalori fazla)</li>");
                    sb.AppendLine("<li>Yüksek protein alımı (2-2.5g/kg vücut ağırlığı)</li>");
                    sb.AppendLine("<li>Karbonhidratları ihmal etmeyin (antrenman öncesi ve sonrası)</li>");
                    sb.AppendLine("<li>Sağlıklı yağlar tüketin (omega-3, avokado, fındık)</li>");
                    sb.AppendLine("<li>Günde 5-6 öğün yemek yiyin</li>");
                    sb.AppendLine("</ul>");
                    break;

                case "zayıflama":
                    sb.AppendLine("<h5>Önerilen Egzersizler:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li><strong>Karışık Antrenman:</strong> Kardiyovasküler + Direnç antrenmanı kombinasyonu</li>");
                    sb.AppendLine("<li><strong>Tempolu Yürüyüş:</strong> Günde 45-60 dakika</li>");
                    sb.AppendLine("<li><strong>Yüzme veya Su Egzersizleri:</strong> Eklemlere nazik, etkili kalori yakımı</li>");
                    sb.AppendLine("<li><strong>Düşük Etkili Aerobik:</strong> Step, eliptik, bisiklet</li>");
                    sb.AppendLine("</ul>");
                    sb.AppendLine("<h5>Beslenme Önerileri:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li>Dengeli bir kalori açığı (günlük 500 kalori)</li>");
                    sb.AppendLine("<li>Porsiyon kontrolü yapın</li>");
                    sb.AppendLine("<li>Lifli gıdalar tüketin (tam tahıllar, sebzeler)</li>");
                    sb.AppendLine("<li>Düzenli öğün saatleri belirleyin</li>");
                    sb.AppendLine("</ul>");
                    break;

                case "esneklik":
                case "yoga":
                    sb.AppendLine("<h5>Önerilen Egzersizler:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li><strong>Yoga:</strong> Haftada 3-5 gün, Hatha veya Vinyasa yoga</li>");
                    sb.AppendLine("<li><strong>Pilates:</strong> Haftada 2-3 gün</li>");
                    sb.AppendLine("<li><strong>Statik Germe Egzersizleri:</strong> Her antrenman sonrası 10-15 dakika</li>");
                    sb.AppendLine("<li><strong>Dinamik Germe:</strong> Antrenman öncesi ısınma</li>");
                    sb.AppendLine("</ul>");
                    sb.AppendLine("<h5>Ek Öneriler:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li>Nefes egzersizleri yapın</li>");
                    sb.AppendLine("<li>Meditasyon ve mindfulness pratiği</li>");
                    sb.AppendLine("<li>Düzenli uyku (7-9 saat)</li>");
                    sb.AppendLine("</ul>");
                    break;

                case "genel sağlık":
                case "fitness":
                default:
                    sb.AppendLine("<h5>Önerilen Egzersizler:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li><strong>Kardiyovasküler:</strong> Haftada 3-4 gün, 30 dakika</li>");
                    sb.AppendLine("<li><strong>Direnç Antrenmanı:</strong> Haftada 2-3 gün, tüm vücut</li>");
                    sb.AppendLine("<li><strong>Esneklik Çalışmaları:</strong> Haftada 2-3 gün, yoga veya germe</li>");
                    sb.AppendLine("<li><strong>Aktif Yaşam:</strong> Günlük 10.000 adım hedefi</li>");
                    sb.AppendLine("</ul>");
                    sb.AppendLine("<h5>Beslenme Önerileri:</h5>");
                    sb.AppendLine("<ul>");
                    sb.AppendLine("<li>Dengeli ve çeşitli beslenme</li>");
                    sb.AppendLine("<li>Taze meyve ve sebze tüketin</li>");
                    sb.AppendLine("<li>Yeterli protein, karbonhidrat ve yağ dengesi</li>");
                    sb.AppendLine("<li>Bol su için (günde 2-3 litre)</li>");
                    sb.AppendLine("</ul>");
                    break;
            }

            sb.AppendLine("<div class='alert alert-info mt-3'>");
            sb.AppendLine("<strong><i class='bi bi-info-circle'></i> Not:</strong> Bu öneriler genel bilgilendirme amaçlıdır. Kişiye özel program için mutlaka bir fitness uzmanı veya diyetisyenle görüşün.");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='alert alert-warning mt-2'>");
            sb.AppendLine("<strong><i class='bi bi-lightbulb'></i> İpucu:</strong> Yapay zeka destekli detaylı öneriler için OpenAI API anahtarını yapılandırın.");
            sb.AppendLine("</div>");

            return sb.ToString();
        }

        private string GetBMICategory(decimal bmi)
        {
            if (bmi < 18.5m)
                return "Zayıf";
            else if (bmi < 25m)
                return "Normal Kilolu";
            else if (bmi < 30m)
                return "Fazla Kilolu";
            else
                return "Obez";
        }
    }
}
