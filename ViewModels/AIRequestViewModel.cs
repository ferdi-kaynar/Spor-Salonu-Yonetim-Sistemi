using System.ComponentModel.DataAnnotations;

namespace FitnessSalonYonetim.ViewModels
{
    public class AIRequestViewModel
    {
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır.")]
        public int Boy { get; set; }

        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır.")]
        public decimal Kilo { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")]
        public string Cinsiyet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hedef seçimi zorunludur.")]
        public string Hedef { get; set; } = string.Empty;

        public int? Yas { get; set; }

        public string? AktiviteSeviyesi { get; set; }

        public string? SaglikDurumu { get; set; }

        public IFormFile? Foto { get; set; }
    }


    public class AIResponseViewModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? OneriHTML { get; set; }
    }
}


