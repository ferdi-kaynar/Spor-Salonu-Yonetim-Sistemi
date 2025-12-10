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

    public class AIImageGenerationViewModel
    {
        [Required(ErrorMessage = "Hedef vücut tipi seçimi zorunludur.")]
        public string HedefVucutTipi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")]
        public string Cinsiyet { get; set; } = string.Empty;

        public string? EgzersizTuru { get; set; }

        public int? Sure { get; set; } // Kaç ay sonra
    }

    public class AIResponseViewModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? OneriHTML { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImagePrompt { get; set; }
    }
}

