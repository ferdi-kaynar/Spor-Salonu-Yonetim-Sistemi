using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessSalonYonetim.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public string UyeId { get; set; }

        [Required(ErrorMessage = "Antrenör seçimi zorunludur.")]
        public int AntrenorId { get; set; }

        [Required(ErrorMessage = "Hizmet seçimi zorunludur.")]
        public int HizmetId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.Date)]
        public DateTime RandevuTarihi { get; set; }

        [Required(ErrorMessage = "Randevu saati zorunludur.")]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan BaslangicSaati { get; set; }

        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan BitisSaati { get; set; }

        [Required]
        [Display(Name = "Durum")]
        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Beklemede;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Ücret")]
        public decimal Ucret { get; set; }

        [StringLength(500)]
        [Display(Name = "Notlar")]
        public string? Notlar { get; set; }

        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // Navigation Properties
        [Display(Name = "Üye")]
        public virtual ApplicationUser Uye { get; set; }

        [Display(Name = "Antrenör")]
        public virtual Antrenor Antrenor { get; set; }

        [Display(Name = "Hizmet")]
        public virtual Hizmet Hizmet { get; set; }
    }

    public enum RandevuDurumu
    {
        [Display(Name = "Beklemede")]
        Beklemede = 0,
        [Display(Name = "Onaylandı")]
        Onaylandi = 1,
        [Display(Name = "Reddedildi")]
        Reddedildi = 2,
        [Display(Name = "Tamamlandı")]
        Tamamlandi = 3,
        [Display(Name = "İptal Edildi")]
        IptalEdildi = 4
    }
}

