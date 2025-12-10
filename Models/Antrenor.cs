using System.ComponentModel.DataAnnotations;

namespace FitnessSalonYonetim.Models
{
    public class Antrenor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(200)]
        [Display(Name = "Uzmanlık Alanları")]
        public string UzmanlikAlanlari { get; set; } // Örn: "Kas Geliştirme, Kilo Verme, Yoga"

        [StringLength(500)]
        [Display(Name = "Biyografi")]
        public string? Biyografi { get; set; }

        [Display(Name = "Profil Resmi")]
        public string? ProfilResmi { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        // Foreign Keys
        [Required(ErrorMessage = "Salon seçimi zorunludur.")]
        [Display(Name = "Salon")]
        public int SalonId { get; set; }

        // Navigation Properties
        [Display(Name = "Salon")]
        public virtual Salon Salon { get; set; }
        public virtual ICollection<AntrenorHizmet> AntrenorHizmetler { get; set; } = new List<AntrenorHizmet>();
        public virtual ICollection<AntrenorMusaitlik> Musaitlikler { get; set; } = new List<AntrenorMusaitlik>();
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
        
        // Eğitmen kullanıcı hesabı
        public virtual ApplicationUser? KullaniciHesabi { get; set; }

        [Display(Name = "Ad Soyad")]
        public string AdSoyad => $"{Ad} {Soyad}";
    }
}

