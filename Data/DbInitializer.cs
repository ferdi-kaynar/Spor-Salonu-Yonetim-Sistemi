using FitnessSalonYonetim.Models;
using Microsoft.AspNetCore.Identity;

namespace FitnessSalonYonetim.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Database oluştur
            context.Database.EnsureCreated();

            // Roller
            string[] roleNames = { "Admin", "Member", "Trainer" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Admin kullanıcı
            var adminEmail = "admin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Ad = "Admin",
                    Soyad = "Yönetici",
                    EmailConfirmed = true,
                    KayitTarihi = DateTime.Now
                };
                
                var result = await userManager.CreateAsync(adminUser, "admin");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Örnek Salon
            if (!context.Salonlar.Any())
            {
                var salon = new Salon
                {
                    Ad = "Kaynar Spor Salonu",
                    Adres = "Sakarya Üniversitesi Kampüsü",
                    Telefon = "0264-123-4567",
                    AcilisSaati = new TimeSpan(6, 0, 0),
                    KapanisSaati = new TimeSpan(23, 0, 0),
                    Aciklama = "Modern ekipmanlar ve profesyonel antrenörlerle hizmetinizdeyiz.",
                    Aktif = true
                };
                context.Salonlar.Add(salon);
                await context.SaveChangesAsync();

                // Örnek Hizmetler
                var hizmetler = new List<Hizmet>
                {
                    new Hizmet
                    {
                        Ad = "Fitness Antrenmanı",
                        Aciklama = "Kişiye özel fitness programı",
                        Sure = 60,
                        Ucret = 150,
                        HizmetTuru = "Fitness",
                        SalonId = salon.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Yoga Dersi",
                        Aciklama = "Rahatlatıcı yoga seansı",
                        Sure = 45,
                        Ucret = 100,
                        HizmetTuru = "Yoga",
                        SalonId = salon.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Pilates",
                        Aciklama = "Core güçlendirme egzersizleri",
                        Sure = 45,
                        Ucret = 120,
                        HizmetTuru = "Pilates",
                        SalonId = salon.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Kişisel Antrenörlük",
                        Aciklama = "Birebir özel antrenman",
                        Sure = 60,
                        Ucret = 250,
                        HizmetTuru = "Kişisel Antrenörlük",
                        SalonId = salon.Id,
                        Aktif = true
                    }
                };
                context.Hizmetler.AddRange(hizmetler);
                await context.SaveChangesAsync();

                // Örnek Antrenörler
                var antrenorler = new List<Antrenor>
                {
                    new Antrenor
                    {
                        Ad = "Ahmet",
                        Soyad = "Yılmaz",
                        Email = "ahmet@kaynar.com",
                        Telefon = "0555-111-2233",
                        UzmanlikAlanlari = "Kas Geliştirme, Kilo Verme, Fitness",
                        Biyografi = "10 yıllık deneyimli fitness antrenörü",
                        SalonId = salon.Id,
                        Aktif = true
                    },
                    new Antrenor
                    {
                        Ad = "Ayşe",
                        Soyad = "Demir",
                        Email = "ayse@kaynar.com",
                        Telefon = "0555-444-5566",
                        UzmanlikAlanlari = "Yoga, Pilates, Esneklik",
                        Biyografi = "Sertifikalı yoga ve pilates eğitmeni",
                        SalonId = salon.Id,
                        Aktif = true
                    },
                    new Antrenor
                    {
                        Ad = "Mehmet",
                        Soyad = "Kaya",
                        Email = "mehmet@kaynar.com",
                        Telefon = "0555-777-8899",
                        UzmanlikAlanlari = "Kişisel Antrenörlük, Beslenme, Rehabilitasyon",
                        Biyografi = "Spor bilimleri uzmanı ve kişisel antrenör",
                        SalonId = salon.Id,
                        Aktif = true
                    }
                };
                context.Antrenorler.AddRange(antrenorler);
                await context.SaveChangesAsync();

                // Eğitmenler için kullanıcı hesapları oluştur
                var trainerUsers = new List<ApplicationUser>();
                for (int i = 0; i < antrenorler.Count; i++)
                {
                    var antrenor = antrenorler[i];
                    var trainerEmail = antrenor.Email;
                    var existingTrainer = await userManager.FindByEmailAsync(trainerEmail);
                    
                    if (existingTrainer == null)
                    {
                        var trainerUser = new ApplicationUser
                        {
                            UserName = trainerEmail,
                            Email = trainerEmail,
                            Ad = antrenor.Ad,
                            Soyad = antrenor.Soyad,
                            EmailConfirmed = true,
                            KayitTarihi = DateTime.Now,
                            AntrenorId = antrenor.Id
                        };
                        
                        var result = await userManager.CreateAsync(trainerUser, "trainer123");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(trainerUser, "Trainer");
                            trainerUsers.Add(trainerUser);
                        }
                    }
                }

                // Antrenör-Hizmet ilişkileri
                var antrenorHizmetler = new List<AntrenorHizmet>
                {
                    new AntrenorHizmet { AntrenorId = antrenorler[0].Id, HizmetId = hizmetler[0].Id },
                    new AntrenorHizmet { AntrenorId = antrenorler[0].Id, HizmetId = hizmetler[3].Id },
                    new AntrenorHizmet { AntrenorId = antrenorler[1].Id, HizmetId = hizmetler[1].Id },
                    new AntrenorHizmet { AntrenorId = antrenorler[1].Id, HizmetId = hizmetler[2].Id },
                    new AntrenorHizmet { AntrenorId = antrenorler[2].Id, HizmetId = hizmetler[0].Id },
                    new AntrenorHizmet { AntrenorId = antrenorler[2].Id, HizmetId = hizmetler[3].Id }
                };
                context.AntrenorHizmetler.AddRange(antrenorHizmetler);
                await context.SaveChangesAsync();

                // Antrenör müsaitlikleri
                foreach (var antrenor in antrenorler)
                {
                    for (int i = 1; i <= 5; i++) // Pazartesi - Cuma
                    {
                        var musaitlik = new AntrenorMusaitlik
                        {
                            AntrenorId = antrenor.Id,
                            Gun = (DayOfWeek)i,
                            BaslangicSaati = new TimeSpan(9, 0, 0),
                            BitisSaati = new TimeSpan(18, 0, 0),
                            Aktif = true
                        };
                        context.AntrenorMusaitlikler.Add(musaitlik);
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}

