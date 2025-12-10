using System.ComponentModel.DataAnnotations;

namespace FitnessSalonYonetim.Models
{
    public class AntrenorMusaitlik
    {
        public int Id { get; set; }

        [Required]
        public int AntrenorId { get; set; }

        [Required(ErrorMessage = "Gün seçimi zorunludur.")]
        [Display(Name = "Gün")]
        public DayOfWeek Gun { get; set; }

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan BaslangicSaati { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan BitisSaati { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        // Navigation Property
        public virtual Antrenor Antrenor { get; set; }
    }
}

