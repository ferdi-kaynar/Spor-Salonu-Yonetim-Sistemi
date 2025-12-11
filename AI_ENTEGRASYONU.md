# 🤖 Gömülü Yapay Zeka Modeli Dokümantasyonu

## Genel Bakış

Spor Salonu Yönetim ve Randevu Sistemi, **gömülü yapay zeka modeli** kullanarak kullanıcılara kişiselleştirilmiş egzersiz ve diyet önerileri sunar. Sistem, harici API bağımlılığı olmadan çalışır.

## 🎯 Özellikler

### 1. Kişiselleştirilmiş Fitness Planı
- ✅ BMI (Vücut Kitle İndeksi) hesaplama ve analiz
- ✅ İdeal kilo hesaplama
- ✅ Günlük kalori ihtiyacı hesaplama (Mifflin-St Jeor Denklemi)
- ✅ Hedef odaklı egzersiz programları
- ✅ Haftalık detaylı program
- ✅ Makro besin hedefleri

### 2. Kural Tabanlı AI Algoritması
- ✅ Cinsiyet bazlı öneriler
- ✅ BMI kategorisine göre uyarlanmış programlar
- ✅ Aktivite seviyesine göre kalori hesaplama
- ✅ Hedef odaklı egzersiz seçimi

### 3. Fotoğraf Yükleme
- ✅ Kullanıcı fotoğrafı saklama
- ✅ Güvenli dosya yönetimi
- ✅ Gelecekte görsel analiz için hazır

## 🧠 AI Model Algoritmaları

### 1. BMI Hesaplama

```csharp
BMI = Kilo (kg) / (Boy (m))²
```

**Kategoriler:**
- < 18.5: Zayıf
- 18.5 - 24.9: Normal
- 25.0 - 29.9: Fazla Kilolu
- 30.0 - 34.9: Obez (1. Derece)
- 35.0 - 39.9: Obez (2. Derece)
- ≥ 40.0: Morbid Obez

### 2. Günlük Kalori İhtiyacı (Mifflin-St Jeor)

**Erkekler için:**
```
BMR = (10 × kilo) + (6.25 × boy) - (5 × yaş) + 5
```

**Kadınlar için:**
```
BMR = (10 × kilo) + (6.25 × boy) - (5 × yaş) - 161
```

**Aktivite Çarpanları:**
- Hareketsiz: 1.2
- Az Aktif: 1.375
- Orta Aktif: 1.55
- Çok Aktif: 1.725
- Profesyonel Sporcu: 1.9

**Günlük Kalori = BMR × Aktivite Çarpanı**

### 3. İdeal Kilo Aralığı

```
Min = 18.5 × (Boy/100)²
Max = 24.9 × (Boy/100)²
```

### 4. Makro Besin Dağılımı

**Kilo Verme:**
- Protein: 1.6-2.0g/kg
- Karbonhidrat: 45%
- Yağ: 35%

**Kas Geliştirme:**
- Protein: 2.0-2.5g/kg
- Karbonhidrat: 50%
- Yağ: 30%

## 💻 Kod Yapısı

### FitnessAIService.cs (Gömülü Model)

```csharp
public class FitnessAIService
{
    // Ana öneri oluşturma
    public string GeneratePersonalizedPlan(...)
    
    // Yardımcı metodlar
    private decimal CalculateBMI(...)
    private int CalculateDailyCalories(...)
    private string GetBMICategory(...)
    private string GenerateWeeklyExercisePlan(...)
    private string GenerateNutritionPlan(...)
    private string GenerateMotivationTips(...)
}
```

### AIController.cs

```csharp
[Authorize]
public class AIController : Controller
{
    private readonly FitnessAIService _aiService;
    
    [HttpPost]
    public async Task<IActionResult> GetEgzersizOnerisi(AIRequestViewModel model)
    {
        var oneri = _aiService.GeneratePersonalizedPlan(...);
        return Json(new AIResponseViewModel { OneriHTML = oneri });
    }
}
```

## 📊 Örnek Çıktılar

### Kilo Verme Hedefi

**Girdi:**
- Boy: 175 cm, Kilo: 95 kg, Cinsiyet: Erkek, Yaş: 30
- Hedef: Kilo Verme

**Model Çıktısı:**
- BMI: 31.0 (Obez)
- İdeal Kilo: 57-76 kg
- Günlük Kalori: 2400 kcal (500 açık ile 1900 kcal)
- Haftalık Program: 5 gün kardiyovasküler + 2 gün direnç
- Protein: 152g/gün
- Tahmini Süre: 8-12 hafta

### Kas Geliştirme Hedefi

**Girdi:**
- Boy: 180 cm, Kilo: 70 kg, Cinsiyet: Erkek, Yaş: 25
- Hedef: Kas Geliştirme

**Model Çıktısı:**
- BMI: 21.6 (Normal)
- Günlük Kalori: 2800 kcal (500 fazla ile 3300 kcal)
- Haftalık Program: 6 gün split antrenman
- Protein: 154-175g/gün
- Tahmini Süre: 12-16 hafta

## 📱 Kullanım

### Endpoint
```
POST /AI/GetEgzersizOnerisi
```

### Request (Form Data)
```json
{
    "Boy": 175,
    "Kilo": 80.5,
    "Cinsiyet": "Erkek",
    "Hedef": "Kilo Verme",
    "Yas": 30,
    "AktiviteSeviyesi": "Orta Aktif",
    "Foto": [file] // Opsiyonel
}
```

### Response
```json
{
    "success": true,
    "oneriHTML": "<div>...</div>"
}
```

## 🎨 Frontend Kullanımı

```javascript
$('#aiForm').submit(function (e) {
    e.preventDefault();
    var formData = new FormData(this);
    
    $.ajax({
        url: '/AI/GetEgzersizOnerisi',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            $('#resultContent').html(response.oneriHTML);
            $('#resultCard').fadeIn();
        }
    });
});
```

## ⚡ Performans

- **Hız**: ~50-100ms (API çağrısı yok)
- **Maliyet**: $0 (harici servis kullanmıyor)
- **Offline**: İnternet bağlantısı gerektirmez
- **Ölçeklenebilirlik**: Sınırsız kullanıcı

## 🔒 Güvenlik

### Dosya Yükleme Güvenliği
```csharp
// Dosya tipi kontrolü
var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

// Dosya boyutu kontrolü (max 5MB)
if (foto.Length > 5 * 1024 * 1024)
    return BadRequest("Dosya çok büyük");
```

### Input Validasyonu
```csharp
[Range(100, 250)]
public int Boy { get; set; }

[Range(30, 300)]
public decimal Kilo { get; set; }
```

## 🧪 Test Senaryoları

### Test 1: Normal BMI - Kas Geliştirme
```
Boy: 180, Kilo: 75, Hedef: Kas Geliştirme
Beklenen: Split program, kalori fazlası, yüksek protein
```

### Test 2: Yüksek BMI - Kilo Verme
```
Boy: 170, Kilo: 95, Hedef: Kilo Verme
Beklenen: Kardiyovasküler ağırlıklı, kalori açığı
```

### Test 3: Düşük BMI - Genel Sağlık
```
Boy: 175, Kilo: 60, Hedef: Genel Sağlık
Beklenen: Dengeli program, normal beslenme
```

## 🚀 Gelecek Geliştirmeler

- [ ] ML.NET ile makine öğrenmesi modeli
- [ ] Fotoğraf analizi (görsel işleme)
- [ ] İlerleme takibi ve öneri optimizasyonu
- [ ] Kullanıcı geri bildirimleri ile model iyileştirme
- [ ] Diyet uyumluluğu skoru

## 📚 Bilimsel Temel

Model aşağıdaki bilimsel yöntemlere dayanır:

1. **Mifflin-St Jeor Denklemi** (1990) - En doğru BMR hesaplama
2. **BMI Kategorileri** - WHO standartları
3. **Protein İhtiyacı** - ISSN (International Society of Sports Nutrition) önerileri
4. **Progresif Yüklenme** - Spor bilimi prensipleri

## 🐛 Sorun Giderme

### Problem: Plan oluşturulmuyor

**Çözüm:**
1. Form alanlarının dolu olduğunu kontrol edin
2. Boy ve Kilo geçerli aralıkta olmalı
3. Hedef seçilmiş olmalı

### Problem: Fotoğraf yüklenmiyor

**Çözüm:**
1. Dosya tipi JPG/PNG olmalı
2. Maksimum 5MB boyut
3. wwwroot/uploads/ai klasörü oluşturulmuş olmalı

---

**Son Güncelleme:** 11 Aralık 2025  
**Versiyon:** 2.0 (Gömülü Model)  
**Teknoloji:** Kural Tabanlı AI + Bilimsel Formüller
