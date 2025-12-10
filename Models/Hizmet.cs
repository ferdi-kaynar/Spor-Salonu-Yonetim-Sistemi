using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessSalonYonetim.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Hizmet adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Hizmet Adı")]
        public string Ad { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Süre zorunludur.")]
        [Range(15, 480, ErrorMessage = "Süre 15 ile 480 dakika arasında olmalıdır.")]
        [Display(Name = "Süre (dakika)")]
        public int Sure { get; set; }

        [Required(ErrorMessage = "Ücret zorunludur.")]
        [Range(0, 999999, ErrorMessage = "Ücret 0 ile 999999 arasında olmalıdır.")]
        [Display(Name = "Ücret")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Ucret { get; set; }

        [Required(ErrorMessage = "Hizmet türü zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Hizmet Türü")]
        public string HizmetTuru { get; set; } // Fitness, Yoga, Pilates, Kişisel Antrenörlük, vb.

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
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}

