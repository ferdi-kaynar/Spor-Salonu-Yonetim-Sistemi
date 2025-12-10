namespace FitnessSalonYonetim.Models
{
    // Antrenör ve Hizmet arasında many-to-many ilişki için ara tablo
    public class AntrenorHizmet
    {
        public int AntrenorId { get; set; }
        public virtual Antrenor Antrenor { get; set; }

        public int HizmetId { get; set; }
        public virtual Hizmet Hizmet { get; set; }
    }
}

