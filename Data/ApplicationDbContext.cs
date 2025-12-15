using FitnessSalonYonetim.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessSalonYonetim.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<AntrenorHizmet> AntrenorHizmetler { get; set; }
        public DbSet<AntrenorMusaitlik> AntrenorMusaitlikler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AntrenorHizmet için composite key tanımı
            modelBuilder.Entity<AntrenorHizmet>()
                .HasKey(ah => new { ah.AntrenorId, ah.HizmetId });

            modelBuilder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Antrenor)
                .WithMany(a => a.AntrenorHizmetler)
                .HasForeignKey(ah => ah.AntrenorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Hizmet)
                .WithMany(h => h.AntrenorHizmetler)
                .HasForeignKey(ah => ah.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Randevu ilişkileri
            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Uye)
                .WithMany(u => u.Randevular)
                .HasForeignKey(r => r.UyeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Antrenor)
                .WithMany(a => a.Randevular)
                .HasForeignKey(r => r.AntrenorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Diğer ilişkiler
            modelBuilder.Entity<Hizmet>()
                .HasOne(h => h.Salon)
                .WithMany(s => s.Hizmetler)
                .HasForeignKey(h => h.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Antrenor>()
                .HasOne(a => a.Salon)
                .WithMany(s => s.Antrenorler)
                .HasForeignKey(a => a.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AntrenorMusaitlik>()
                .HasOne(am => am.Antrenor)
                .WithMany(a => a.Musaitlikler)
                .HasForeignKey(am => am.AntrenorId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser için decimal precision ayarları
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Kilo)
                .HasPrecision(5, 2); // 123.45 kg formatında

            // ApplicationUser - Antrenor ilişkisi (Eğitmen kullanıcı hesabı)
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Antrenor)
                .WithOne(a => a.KullaniciHesabi)
                .HasForeignKey<ApplicationUser>(u => u.AntrenorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}


