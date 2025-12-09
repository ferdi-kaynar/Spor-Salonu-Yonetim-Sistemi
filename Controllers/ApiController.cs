using FitnessSalonYonetim.Data;
using FitnessSalonYonetim.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessSalonYonetim.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Api/Antrenorler
        [HttpGet("Antrenorler")]
        public async Task<ActionResult<IEnumerable<object>>> GetAntrenorler([FromQuery] string? uzmanlik = null, [FromQuery] int? salonId = null)
        {
            var query = _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .Where(a => a.Aktif)
                .AsQueryable();

            // LINQ filtreleme
            if (!string.IsNullOrEmpty(uzmanlik))
            {
                query = query.Where(a => a.UzmanlikAlanlari.Contains(uzmanlik));
            }

            if (salonId.HasValue)
            {
                query = query.Where(a => a.SalonId == salonId.Value);
            }

            var antrenorler = await query
                .Select(a => new
                {
                    a.Id,
                    a.Ad,
                    a.Soyad,
                    AdSoyad = a.Ad + " " + a.Soyad,
                    a.Email,
                    a.Telefon,
                    a.UzmanlikAlanlari,
                    a.Biyografi,
                    Salon = new { a.Salon.Id, a.Salon.Ad },
                    Hizmetler = a.AntrenorHizmetler.Select(ah => new
                    {
                        ah.Hizmet.Id,
                        ah.Hizmet.Ad,
                        ah.Hizmet.Sure,
                        ah.Hizmet.Ucret
                    }).ToList()
                })
                .ToListAsync();

            return Ok(antrenorler);
        }

        // GET: api/Api/Antrenor/5
        [HttpGet("Antrenor/{id}")]
        public async Task<ActionResult<object>> GetAntrenor(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.Musaitlikler)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.Ad,
                    a.Soyad,
                    AdSoyad = a.Ad + " " + a.Soyad,
                    a.Email,
                    a.Telefon,
                    a.UzmanlikAlanlari,
                    a.Biyografi,
                    Salon = new { a.Salon.Id, a.Salon.Ad },
                    Hizmetler = a.AntrenorHizmetler.Select(ah => new
                    {
                        ah.Hizmet.Id,
                        ah.Hizmet.Ad,
                        ah.Hizmet.Sure,
                        ah.Hizmet.Ucret
                    }).ToList(),
                    Musaitlikler = a.Musaitlikler.Select(m => new
                    {
                        m.Id,
                        m.Gun,
                        GunAdi = m.Gun.ToString(),
                        m.BaslangicSaati,
                        m.BitisSaati
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (antrenor == null)
            {
                return NotFound();
            }

            return Ok(antrenor);
        }

        // GET: api/Api/UygunAntrenorler
        [HttpGet("UygunAntrenorler")]
        public async Task<ActionResult<IEnumerable<object>>> GetUygunAntrenorler([FromQuery] DateTime tarih, [FromQuery] TimeSpan saat, [FromQuery] int? hizmetId = null)
        {
            var gun = tarih.DayOfWeek;

            var query = _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.Musaitlikler)
                .Where(a => a.Aktif)
                .AsQueryable();

            // Hizmet filtresi
            if (hizmetId.HasValue)
            {
                query = query.Where(a => a.AntrenorHizmetler.Any(ah => ah.HizmetId == hizmetId.Value));
            }

            // Müsaitlik kontrolü - LINQ sorgusu
            var uygunAntrenorler = await query
                .Where(a => a.Musaitlikler.Any(m => m.Gun == gun && 
                                                     m.BaslangicSaati <= saat && 
                                                     m.BitisSaati >= saat.Add(TimeSpan.FromHours(1)) &&
                                                     m.Aktif))
                .Select(a => new
                {
                    a.Id,
                    a.Ad,
                    a.Soyad,
                    AdSoyad = a.Ad + " " + a.Soyad,
                    a.UzmanlikAlanlari,
                    Salon = new { a.Salon.Id, a.Salon.Ad },
                    Hizmetler = a.AntrenorHizmetler.Select(ah => new
                    {
                        ah.Hizmet.Id,
                        ah.Hizmet.Ad,
                        ah.Hizmet.Sure,
                        ah.Hizmet.Ucret
                    }).ToList()
                })
                .ToListAsync();

            // Çakışma kontrolü için randevuları kontrol et
            var uygunlar = new List<object>();
            foreach (var antrenor in uygunAntrenorler)
            {
                var randevuVar = await _context.Randevular
                    .AnyAsync(r => r.AntrenorId == antrenor.Id &&
                                  r.RandevuTarihi.Date == tarih.Date &&
                                  r.BaslangicSaati < saat.Add(TimeSpan.FromHours(1)) &&
                                  r.BitisSaati > saat &&
                                  r.Durum != RandevuDurumu.IptalEdildi &&
                                  r.Durum != RandevuDurumu.Reddedildi);

                if (!randevuVar)
                {
                    uygunlar.Add(antrenor);
                }
            }

            return Ok(uygunlar);
        }

        // GET: api/Api/Randevular
        [HttpGet("Randevular")]
        public async Task<ActionResult<IEnumerable<object>>> GetRandevular(
            [FromQuery] DateTime? baslangicTarihi = null, 
            [FromQuery] DateTime? bitisTarihi = null,
            [FromQuery] int? antrenorId = null,
            [FromQuery] RandevuDurumu? durum = null)
        {
            var query = _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .AsQueryable();

            // LINQ filtreleme
            if (baslangicTarihi.HasValue)
            {
                query = query.Where(r => r.RandevuTarihi >= baslangicTarihi.Value);
            }

            if (bitisTarihi.HasValue)
            {
                query = query.Where(r => r.RandevuTarihi <= bitisTarihi.Value);
            }

            if (antrenorId.HasValue)
            {
                query = query.Where(r => r.AntrenorId == antrenorId.Value);
            }

            if (durum.HasValue)
            {
                query = query.Where(r => r.Durum == durum.Value);
            }

            var randevular = await query
                .OrderBy(r => r.RandevuTarihi)
                .ThenBy(r => r.BaslangicSaati)
                .Select(r => new
                {
                    r.Id,
                    Uye = new { r.Uye.Id, r.Uye.Ad, r.Uye.Soyad, AdSoyad = r.Uye.Ad + " " + r.Uye.Soyad },
                    Antrenor = new { r.Antrenor.Id, r.Antrenor.Ad, r.Antrenor.Soyad, AdSoyad = r.Antrenor.Ad + " " + r.Antrenor.Soyad },
                    Hizmet = new { r.Hizmet.Id, r.Hizmet.Ad, r.Hizmet.Sure, r.Hizmet.Ucret },
                    r.RandevuTarihi,
                    r.BaslangicSaati,
                    r.BitisSaati,
                    r.Durum,
                    DurumAdi = r.Durum.ToString(),
                    r.Ucret,
                    r.Notlar,
                    r.OlusturmaTarihi
                })
                .ToListAsync();

            return Ok(randevular);
        }

        // GET: api/Api/Hizmetler
        [HttpGet("Hizmetler")]
        public async Task<ActionResult<IEnumerable<object>>> GetHizmetler([FromQuery] string? tur = null, [FromQuery] int? salonId = null)
        {
            var query = _context.Hizmetler
                .Include(h => h.Salon)
                .Where(h => h.Aktif)
                .AsQueryable();

            // LINQ filtreleme
            if (!string.IsNullOrEmpty(tur))
            {
                query = query.Where(h => h.HizmetTuru.Contains(tur));
            }

            if (salonId.HasValue)
            {
                query = query.Where(h => h.SalonId == salonId.Value);
            }

            var hizmetler = await query
                .Select(h => new
                {
                    h.Id,
                    h.Ad,
                    h.Aciklama,
                    h.Sure,
                    h.Ucret,
                    h.HizmetTuru,
                    Salon = new { h.Salon.Id, h.Salon.Ad }
                })
                .ToListAsync();

            return Ok(hizmetler);
        }

        // GET: api/Api/Salonlar
        [HttpGet("Salonlar")]
        public async Task<ActionResult<IEnumerable<object>>> GetSalonlar()
        {
            var salonlar = await _context.Salonlar
                .Where(s => s.Aktif)
                .Select(s => new
                {
                    s.Id,
                    s.Ad,
                    s.Adres,
                    s.Telefon,
                    s.AcilisSaati,
                    s.KapanisSaati,
                    s.Aciklama,
                    HizmetSayisi = s.Hizmetler.Count(h => h.Aktif),
                    AntrenorSayisi = s.Antrenorler.Count(a => a.Aktif)
                })
                .ToListAsync();

            return Ok(salonlar);
        }

        // GET: api/Api/Istatistikler
        [HttpGet("Istatistikler")]
        public async Task<ActionResult<object>> GetIstatistikler()
        {
            var toplamSalon = await _context.Salonlar.CountAsync(s => s.Aktif);
            var toplamAntrenor = await _context.Antrenorler.CountAsync(a => a.Aktif);
            var toplamUye = await _context.Users.CountAsync();
            var toplamRandevu = await _context.Randevular.CountAsync();
            var bekleyenRandevuSayisi = await _context.Randevular.CountAsync(r => r.Durum == RandevuDurumu.Beklemede);
            var onaylananRandevuSayisi = await _context.Randevular.CountAsync(r => r.Durum == RandevuDurumu.Onaylandi);
            
            // En çok randevu alan antrenörler - LINQ sorgusu
            var enCokRandevuAlanAntrenorler = await _context.Randevular
                .Where(r => r.Durum == RandevuDurumu.Tamamlandi)
                .GroupBy(r => new { r.AntrenorId, r.Antrenor.Ad, r.Antrenor.Soyad })
                .Select(g => new
                {
                    AntrenorId = g.Key.AntrenorId,
                    Ad = g.Key.Ad,
                    Soyad = g.Key.Soyad,
                    AdSoyad = g.Key.Ad + " " + g.Key.Soyad,
                    RandevuSayisi = g.Count()
                })
                .OrderByDescending(x => x.RandevuSayisi)
                .Take(5)
                .ToListAsync();

            // En popüler hizmetler - LINQ sorgusu
            var enPopulerHizmetler = await _context.Randevular
                .GroupBy(r => new { r.HizmetId, r.Hizmet.Ad })
                .Select(g => new
                {
                    HizmetId = g.Key.HizmetId,
                    HizmetAd = g.Key.Ad,
                    RandevuSayisi = g.Count()
                })
                .OrderByDescending(x => x.RandevuSayisi)
                .Take(5)
                .ToListAsync();

            var istatistikler = new
            {
                ToplamSalon = toplamSalon,
                ToplamAntrenor = toplamAntrenor,
                ToplamUye = toplamUye,
                ToplamRandevu = toplamRandevu,
                BekleyenRandevuSayisi = bekleyenRandevuSayisi,
                OnaylananRandevuSayisi = onaylananRandevuSayisi,
                EnCokRandevuAlanAntrenorler = enCokRandevuAlanAntrenorler,
                EnPopulerHizmetler = enPopulerHizmetler
            };

            return Ok(istatistikler);
        }
    }
}

