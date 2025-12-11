using System.ComponentModel.DataAnnotations;

namespace FitnessSalonYonetim.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter olmalıdır.", MinimumLength = 2)]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter olmalıdır.", MinimumLength = 2)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        [Display(Name = "Cinsiyet")]
        public string? Cinsiyet { get; set; }

        [Display(Name = "Boy (cm)")]
        [Range(0, 300, ErrorMessage = "Boy 0-300 cm arasında olmalıdır.")]
        public int? Boy { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(0, 500, ErrorMessage = "Kilo 0-500 kg arasında olmalıdır.")]
        public decimal? Kilo { get; set; }

        [StringLength(500)]
        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Display(Name = "Profil Resmi")]
        public string? ProfilResmi { get; set; }
    }
}

