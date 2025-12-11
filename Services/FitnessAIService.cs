using System.Text;

namespace FitnessSalonYonetim.Services
{
    /// <summary>
    /// Gömülü Fitness AI Model Servisi
    /// Kural tabanlı yapay zeka algoritması ile kişiye özel egzersiz ve beslenme önerileri
    /// </summary>
    public class FitnessAIService
    {
        /// <summary>
        /// Kişisel bilgilere göre detaylı egzersiz ve beslenme planı oluşturur
        /// </summary>
        public string GeneratePersonalizedPlan(
            int boy, 
            decimal kilo, 
            string cinsiyet, 
            string hedef, 
            int? yas = null, 
            string? aktiviteSeviyesi = null)
        {
            var sb = new StringBuilder();
            
            // BMI Hesaplama ve Analiz
            var bmi = CalculateBMI(kilo, boy);
            var bmiKategorisi = GetBMICategory(bmi);
            var idealKilo = CalculateIdealWeight(boy, cinsiyet);
            var dailyCalories = CalculateDailyCalories(kilo, boy, yas ?? 30, cinsiyet, aktiviteSeviyesi ?? "Orta");
            
            // Kişisel Değerlendirme
            sb.AppendLine("<div class='alert alert-primary'>");
            sb.AppendLine("<h4><i class='bi bi-person-check'></i> Kişisel Değerlendirme</h4>");
            sb.AppendLine("<ul class='mb-0'>");
            sb.AppendLine($"<li><strong>Boy:</strong> {boy} cm</li>");
            sb.AppendLine($"<li><strong>Kilo:</strong> {kilo} kg</li>");
            sb.AppendLine($"<li><strong>Cinsiyet:</strong> {cinsiyet}</li>");
            if (yas.HasValue) sb.AppendLine($"<li><strong>Yaş:</strong> {yas}</li>");
            sb.AppendLine($"<li><strong>BMI:</strong> {bmi:F1} ({bmiKategorisi})</li>");
            sb.AppendLine($"<li><strong>İdeal Kilo Aralığı:</strong> {idealKilo.min:F0} - {idealKilo.max:F0} kg</li>");
            sb.AppendLine($"<li><strong>Günlük Kalori İhtiyacı:</strong> {dailyCalories} kcal</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");

            // Hedef Analizi
            sb.AppendLine($"<h4 class='mt-4'><i class='bi bi-target'></i> Hedefiniz: {hedef}</h4>");
            sb.AppendLine(GenerateGoalAnalysis(hedef, bmi, kilo, idealKilo));

            // Haftalık Egzersiz Programı
            sb.AppendLine("<h4 class='mt-4'><i class='bi bi-calendar-week'></i> Haftalık Egzersiz Programı</h4>");
            sb.AppendLine(GenerateWeeklyExercisePlan(hedef, bmiKategorisi, cinsiyet));

            // Beslenme Planı
            sb.AppendLine("<h4 class='mt-4'><i class='bi bi-egg-fried'></i> Beslenme Planı</h4>");
            sb.AppendLine(GenerateNutritionPlan(hedef, dailyCalories, kilo));

            // Motivasyon ve İpuçları
            sb.AppendLine("<h4 class='mt-4'><i class='bi bi-lightbulb'></i> Motivasyon ve İpuçları</h4>");
            sb.AppendLine(GenerateMotivationTips(hedef, bmiKategorisi));

            // Önemli Uyarılar
            sb.AppendLine("<div class='alert alert-warning mt-4'>");
            sb.AppendLine("<h5><i class='bi bi-exclamation-triangle'></i> Dikkat Edilmesi Gerekenler</h5>");
            sb.AppendLine("<ul class='mb-0'>");
            sb.AppendLine("<li>Egzersiz programına başlamadan önce doktor kontrolünden geçin</li>");
            sb.AppendLine("<li>Ağrı hissettiğinizde egzersizi durdurun</li>");
            sb.AppendLine("<li>Isınma ve soğuma hareketlerini atlamayın</li>");
            sb.AppendLine("<li>Yeterli su tüketimine dikkat edin (günde 2-3 litre)</li>");
            sb.AppendLine("<li>Düzenli uyku uyuyun (7-9 saat)</li>");
            sb.AppendLine("<li>Ani kilo değişimlerinden kaçının (haftada max 0.5-1 kg)</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='alert alert-info mt-3'>");
            sb.AppendLine("<strong><i class='bi bi-robot'></i> Yapay Zeka Notu:</strong> ");
            sb.AppendLine("Bu plan, gömülü fitness AI modelimiz tarafından kişisel bilgilerinize göre oluşturulmuştur. ");
            sb.AppendLine("Daha detaylı ve kişiselleştirilmiş program için profesyonel antrenör desteği almanızı öneririz.");
            sb.AppendLine("</div>");

            return sb.ToString();
        }

        private decimal CalculateBMI(decimal kilo, int boy)
        {
            var boyMetre = boy / 100m;
            return kilo / (boyMetre * boyMetre);
        }

        private string GetBMICategory(decimal bmi)
        {
            if (bmi < 18.5m) return "Zayıf";
            if (bmi < 25m) return "Normal";
            if (bmi < 30m) return "Fazla Kilolu";
            if (bmi < 35m) return "Obez (1. Derece)";
            if (bmi < 40m) return "Obez (2. Derece)";
            return "Morbid Obez";
        }

        private (decimal min, decimal max) CalculateIdealWeight(int boy, string cinsiyet)
        {
            var boyMetre = boy / 100m;
            var minBMI = 18.5m;
            var maxBMI = 24.9m;
            
            return (minBMI * boyMetre * boyMetre, maxBMI * boyMetre * boyMetre);
        }

        private int CalculateDailyCalories(decimal kilo, int boy, int yas, string cinsiyet, string aktiviteSeviyesi)
        {
            // Mifflin-St Jeor Denklemi
            decimal bmr;
            if (cinsiyet.ToLower() == "erkek")
                bmr = (10 * kilo) + (6.25m * boy) - (5 * yas) + 5;
            else
                bmr = (10 * kilo) + (6.25m * boy) - (5 * yas) - 161;

            // Aktivite çarpanı
            var aktiviteCarpani = aktiviteSeviyesi.ToLower() switch
            {
                "hareketsiz" => 1.2m,
                "az aktif" => 1.375m,
                "orta aktif" => 1.55m,
                "çok aktif" => 1.725m,
                "sporcu" => 1.9m,
                _ => 1.55m
            };

            return (int)(bmr * aktiviteCarpani);
        }

        private string GenerateGoalAnalysis(string hedef, decimal bmi, decimal kilo, (decimal min, decimal max) idealKilo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class='card bg-light'>");
            sb.AppendLine("<div class='card-body'>");
            
            switch (hedef.ToLower())
            {
                case "kilo verme":
                case "zayıflama":
                    var kiloFarki = kilo - idealKilo.max;
                    sb.AppendLine("<p><strong>Analiz:</strong> Kilo verme hedefi için sağlıklı bir program oluşturulmuştur.</p>");
                    if (kiloFarki > 0)
                        sb.AppendLine($"<p>İdeal kilonuza ulaşmak için yaklaşık <strong>{kiloFarki:F0} kg</strong> vermeniz önerilir.</p>");
                    sb.AppendLine("<p><strong>Tahmini Süre:</strong> 8-12 hafta (haftada 0.5-1 kg ile)</p>");
                    sb.AppendLine("<p><strong>Odak:</strong> Kalori açığı + Kardiyovasküler egzersizler</p>");
                    break;

                case "kas geliştirme":
                case "kas kazanma":
                    sb.AppendLine("<p><strong>Analiz:</strong> Kas geliştirme için progresif yüklenme ve yüksek protein diyeti gereklidir.</p>");
                    sb.AppendLine("<p><strong>Tahmini Süre:</strong> 12-16 hafta (görünür sonuçlar için)</p>");
                    sb.AppendLine("<p><strong>Odak:</strong> Kalori fazlası + Ağırlık antrenmanı</p>");
                    break;

                case "esneklik":
                case "yoga":
                    sb.AppendLine("<p><strong>Analiz:</strong> Esneklik ve mobilite geliştirme programı.</p>");
                    sb.AppendLine("<p><strong>Tahmini Süre:</strong> 6-8 hafta (gelişme için)</p>");
                    sb.AppendLine("<p><strong>Odak:</strong> Düzenli germe + Yoga/Pilates</p>");
                    break;

                default:
                    sb.AppendLine("<p><strong>Analiz:</strong> Genel sağlık ve fitness için dengeli bir program.</p>");
                    sb.AppendLine("<p><strong>Tahmini Süre:</strong> Sürekli yaşam tarzı</p>");
                    sb.AppendLine("<p><strong>Odak:</strong> Dengeli egzersiz + Sağlıklı beslenme</p>");
                    break;
            }
            
            sb.AppendLine("</div></div>");
            return sb.ToString();
        }

        private string GenerateWeeklyExercisePlan(string hedef, string bmiKategorisi, string cinsiyet)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class='table-responsive'>");
            sb.AppendLine("<table class='table table-bordered table-hover'>");
            sb.AppendLine("<thead class='table-primary'>");
            sb.AppendLine("<tr><th>Gün</th><th>Egzersiz</th><th>Süre</th><th>Notlar</th></tr>");
            sb.AppendLine("</thead><tbody>");

            switch (hedef.ToLower())
            {
                case "kilo verme":
                case "zayıflama":
                    sb.AppendLine("<tr><td><strong>Pazartesi</strong></td><td>Kardiyovasküler (Koşu/Yürüyüş)</td><td>45 dk</td><td>Orta tempo</td></tr>");
                    sb.AppendLine("<tr><td><strong>Salı</strong></td><td>HIIT</td><td>30 dk</td><td>Yüksek yoğunluk</td></tr>");
                    sb.AppendLine("<tr><td><strong>Çarşamba</strong></td><td>Direnç Antrenmanı</td><td>40 dk</td><td>Tüm vücut</td></tr>");
                    sb.AppendLine("<tr><td><strong>Perşembe</strong></td><td>Yüzme/Bisiklet</td><td>45 dk</td><td>Düşük etkili</td></tr>");
                    sb.AppendLine("<tr><td><strong>Cuma</strong></td><td>Kardiyovasküler</td><td>45 dk</td><td>Orta-yüksek tempo</td></tr>");
                    sb.AppendLine("<tr><td><strong>Cumartesi</strong></td><td>Aktif Dinlenme (Yoga)</td><td>30 dk</td><td>Hafif germe</td></tr>");
                    sb.AppendLine("<tr><td><strong>Pazar</strong></td><td>Dinlenme</td><td>-</td><td>Tam dinlenme</td></tr>");
                    break;

                case "kas geliştirme":
                case "kas kazanma":
                    sb.AppendLine("<tr><td><strong>Pazartesi</strong></td><td>Göğüs + Triceps</td><td>60 dk</td><td>Bench press, dips</td></tr>");
                    sb.AppendLine("<tr><td><strong>Salı</strong></td><td>Sırt + Biceps</td><td>60 dk</td><td>Deadlift, pull-ups</td></tr>");
                    sb.AppendLine("<tr><td><strong>Çarşamba</strong></td><td>Dinlenme/Kardiyovasküler</td><td>20 dk</td><td>Hafif tempo</td></tr>");
                    sb.AppendLine("<tr><td><strong>Perşembe</strong></td><td>Bacak</td><td>60 dk</td><td>Squat, leg press</td></tr>");
                    sb.AppendLine("<tr><td><strong>Cuma</strong></td><td>Omuz + Karın</td><td>50 dk</td><td>Military press, abs</td></tr>");
                    sb.AppendLine("<tr><td><strong>Cumartesi</strong></td><td>Tam Vücut (Hafif)</td><td>45 dk</td><td>Düşük ağırlık</td></tr>");
                    sb.AppendLine("<tr><td><strong>Pazar</strong></td><td>Dinlenme</td><td>-</td><td>Kaslar için kritik</td></tr>");
                    break;

                case "esneklik":
                case "yoga":
                    sb.AppendLine("<tr><td><strong>Pazartesi</strong></td><td>Hatha Yoga</td><td>45 dk</td><td>Temel pozlar</td></tr>");
                    sb.AppendLine("<tr><td><strong>Salı</strong></td><td>Dinamik Germe</td><td>30 dk</td><td>Tüm vücut</td></tr>");
                    sb.AppendLine("<tr><td><strong>Çarşamba</strong></td><td>Pilates</td><td>45 dk</td><td>Core odaklı</td></tr>");
                    sb.AppendLine("<tr><td><strong>Perşembe</strong></td><td>Vinyasa Yoga</td><td>45 dk</td><td>Akışkan hareketler</td></tr>");
                    sb.AppendLine("<tr><td><strong>Cuma</strong></td><td>Statik Germe</td><td>30 dk</td><td>Derin germe</td></tr>");
                    sb.AppendLine("<tr><td><strong>Cumartesi</strong></td><td>Yin Yoga</td><td>60 dk</td><td>Pasif germe</td></tr>");
                    sb.AppendLine("<tr><td><strong>Pazar</strong></td><td>Meditasyon</td><td>20 dk</td><td>Zihin-beden</td></tr>");
                    break;

                default:
                    sb.AppendLine("<tr><td><strong>Pazartesi</strong></td><td>Kardiyovasküler</td><td>30 dk</td><td>Koşu/Yürüyüş</td></tr>");
                    sb.AppendLine("<tr><td><strong>Salı</strong></td><td>Direnç (Üst vücut)</td><td>40 dk</td><td>Push hareketleri</td></tr>");
                    sb.AppendLine("<tr><td><strong>Çarşamba</strong></td><td>Yoga/Germe</td><td>30 dk</td><td>Esneklik</td></tr>");
                    sb.AppendLine("<tr><td><strong>Perşembe</strong></td><td>Kardiyovasküler</td><td>30 dk</td><td>Bisiklet/Yüzme</td></tr>");
                    sb.AppendLine("<tr><td><strong>Cuma</strong></td><td>Direnç (Alt vücut)</td><td>40 dk</td><td>Squat, lunges</td></tr>");
                    sb.AppendLine("<tr><td><strong>Cumartesi</strong></td><td>Aktif Hobi</td><td>60 dk</td><td>Spor/Doğa yürüyüşü</td></tr>");
                    sb.AppendLine("<tr><td><strong>Pazar</strong></td><td>Dinlenme</td><td>-</td><td>Tam dinlenme</td></tr>");
                    break;
            }

            sb.AppendLine("</tbody></table></div>");
            return sb.ToString();
        }

        private string GenerateNutritionPlan(string hedef, int dailyCalories, decimal kilo)
        {
            var sb = new StringBuilder();
            var protein = kilo * (hedef.ToLower().Contains("kas") ? 2.2m : 1.6m);
            
            sb.AppendLine("<div class='row'>");
            
            // Makro Besinler
            sb.AppendLine("<div class='col-md-6'>");
            sb.AppendLine("<h5>Günlük Makro Hedefler</h5>");
            sb.AppendLine("<ul>");
            sb.AppendLine($"<li><strong>Kalori:</strong> {dailyCalories} kcal</li>");
            sb.AppendLine($"<li><strong>Protein:</strong> {protein:F0}g ({protein * 4}kcal)</li>");
            
            var kalanKalori = dailyCalories - (protein * 4);
            if (hedef.ToLower().Contains("kas"))
            {
                sb.AppendLine($"<li><strong>Karbonhidrat:</strong> ~{kalanKalori * 0.5m / 4:F0}g</li>");
                sb.AppendLine($"<li><strong>Yağ:</strong> ~{kalanKalori * 0.5m / 9:F0}g</li>");
            }
            else
            {
                sb.AppendLine($"<li><strong>Karbonhidrat:</strong> ~{kalanKalori * 0.45m / 4:F0}g</li>");
                sb.AppendLine($"<li><strong>Yağ:</strong> ~{kalanKalori * 0.35m / 9:F0}g</li>");
            }
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");

            // Örnek Günlük Menü
            sb.AppendLine("<div class='col-md-6'>");
            sb.AppendLine("<h5>Örnek Günlük Menü</h5>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li><strong>Kahvaltı:</strong> Yumurta, tam tahıllı ekmek, avokado</li>");
            sb.AppendLine("<li><strong>Ara Öğün:</strong> Meyve, fındık (30g)</li>");
            sb.AppendLine("<li><strong>Öğle:</strong> Tavuk/Balık, kinoa, sebze</li>");
            sb.AppendLine("<li><strong>Ara Öğün:</strong> Protein shake, muz</li>");
            sb.AppendLine("<li><strong>Akşam:</strong> Kırmızı et/Hindi, patates, salata</li>");
            sb.AppendLine("<li><strong>Gece:</strong> Yoğurt veya cottage cheese</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div>");

            // Beslenme İpuçları
            sb.AppendLine("<div class='mt-3'>");
            sb.AppendLine("<h5>Beslenme İpuçları</h5>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Her öğünde protein tüketin</li>");
            sb.AppendLine("<li>Antrenman öncesi karbonhidrat, sonrası protein ağırlıklı beslenin</li>");
            sb.AppendLine("<li>İşlenmiş gıdalardan kaçının</li>");
            sb.AppendLine("<li>Günde 2-3 litre su için</li>");
            sb.AppendLine("<li>Öğünleri atlamayın, düzenli beslenin</li>");
            sb.AppendLine("<li>Lifli gıdalara yer verin (sebze, meyve, tam tahıl)</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");

            return sb.ToString();
        }

        private string GenerateMotivationTips(string hedef, string bmiKategorisi)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class='alert alert-success'>");
            sb.AppendLine("<ul class='mb-0'>");
            sb.AppendLine("<li><strong>Sabırlı Olun:</strong> Sonuçlar 4-6 hafta içinde görünmeye başlar</li>");
            sb.AppendLine("<li><strong>Tutarlı Olun:</strong> Haftada 3-4 gün düzenli egzersiz</li>");
            sb.AppendLine("<li><strong>İlerle Kaydet:</strong> Kilo, ölçüler ve fotoğraflarla takip edin</li>");
            sb.AppendLine("<li><strong>Kendinizi Ödüllendirin:</strong> Küçük başarıları kutlayın</li>");
            sb.AppendLine("<li><strong>Destek Alın:</strong> Arkadaş veya antrenörle çalışın</li>");
            sb.AppendLine("<li><strong>Esneklik Gösterin:</strong> %80 kuralı - her zaman mükemmel olmak zorunda değilsiniz</li>");
            sb.AppendLine("<li><strong>Dinlenmeyi İhmal Etmeyin:</strong> Kas gelişimi dinlenme sırasında olur</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");

            return sb.ToString();
        }
    }
}

