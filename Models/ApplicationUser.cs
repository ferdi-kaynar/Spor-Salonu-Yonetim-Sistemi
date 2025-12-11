using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FitnessSalonYonetim.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        [StringLength(10)]
        [Display(Name = "Cinsiyet")]
        public string? Cinsiyet { get; set; } // Erkek, Kadın, Diğer

        [Display(Name = "Boy (cm)")]
        [Range(0, 300)]
        public int? Boy { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(0, 500)]
        public decimal? Kilo { get; set; }

    [StringLength(500)]
    [Display(Name = "Adres")]
    public string? Adres { get; set; }

    [Display(Name = "Profil Resmi")]
    public string? ProfilResmi { get; set; }

    [Display(Name = "Kayıt Tarihi")]
    public DateTime KayitTarihi { get; set; } = DateTime.Now;

        // Eğitmen İlişkisi (Eğer kullanıcı aynı zamanda eğitmense)
        [Display(Name = "Eğitmen")]
        public int? AntrenorId { get; set; }
        public virtual Antrenor? Antrenor { get; set; }

        // Navigation Properties
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        [Display(Name = "Ad Soyad")]
        public string AdSoyad => $"{Ad} {Soyad}";
    }
}

