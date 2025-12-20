# KAYNAR Spor Salonu Yönetim Sistemi

ASP.NET Core MVC ile geliştirilmiş spor salonu yönetim ve randevu sistemi.

## Özellikler

- Salon & Antrenör Yönetimi
- Randevu Sistemi (otomatik kontrol + onay)
- Rol Sistemi (Admin/Trainer/Üye)
- REST API + LINQ filtreleme
- Gömülü AI Modeli (kişiselleştirilmiş fitness planı)
- Modern UI (Bootstrap 5)

## Teknolojiler

ASP.NET Core 6.0 | Entity Framework | Identity | SQL Server | Bootstrap 5 | jQuery

## Kurulum

```bash
git clone [repository-url]
cd FitnessSalonYonetim
dotnet restore
dotnet ef database update
dotnet run
admin giriş: admin@gmail.com şifre 123456
```


## API Örnekleri

```
GET /api/Api/Antrenorler?uzmanlik=Yoga
GET /api/Api/UygunAntrenorler?tarih=2024-12-15&saat=09:00
GET /api/Api/Randevular?durum=1
GET /api/Api/Istatistikler
```

## Proje Yapısı

```
Controllers/ Models/ Views/ Data/ wwwroot/
```

## Geliştirme

```bash
dotnet ef migrations add [Name]
dotnet ef database update
```

---

**Sakarya Üniversitesi - Web Programlama Dersi**  
Ferdi Kaynar | ferdi.kaynar@ogr.sakarya.edu.tr | 2025
