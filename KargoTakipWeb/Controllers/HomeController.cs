using KargoTakipWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace KargoTakipWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;

        // Veritabaný baðlantý yolunu (appsettings.json'dan) alýyoruz
        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        // Sayfa ilk açýldýðýnda çalýþacak metot
        public IActionResult Index()
        {
            return View();
        }

        // Kullanýcý butona bastýðýnda çalýþacak metot
        [HttpPost]
        public IActionResult Index(string takipKodu)
        {
            List<KargoHareketViewModel> hareketler = new List<KargoHareketViewModel>();
            string connectionString = _config.GetConnectionString("KargoDbBaglantisi");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Bizim yazdýðýmýz Stored Procedure'ü çaðýrýyoruz
                using (SqlCommand cmd = new SqlCommand("sp_KargoGecmisiSorgula", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Kullanýcýnýn girdiði kodu parametre olarak ekliyoruz
                    cmd.Parameters.AddWithValue("@GelenTakipKodu", takipKodu ?? "");

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hareketler.Add(new KargoHareketViewModel
                            {
                                // SQL'deki sütun isimleriyle eþleþtiriyoruz
                                IslemZamani = Convert.ToDateTime(reader["Ýþlem Zamaný"]),
                                TakipKodu = reader["Takip Kodu"].ToString(),
                                IslemYeri = reader["Ýþlem Yeri (Þube)"].ToString(),
                                IslemYapanPersonel = reader["Ýþlemi Yapan Personel"].ToString(),
                                KargoDurumu = reader["Kargo Durumu"].ToString(),
                                Aciklama = reader["Detay/Açýklama"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.ArananKod = takipKodu;
            // Verileri ekrana (View'a) gönderiyoruz
            return View(hareketler);
        }

        // ÞUBE RAPORLARI SAYFASI ÝÇÝN YENÝ METOT
        public IActionResult Istatistikler()
        {
            List<SubeIstatistikViewModel> raporlar = new List<SubeIstatistikViewModel>();
            string connectionString = _config.GetConnectionString("KargoDbBaglantisi");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Raporumuzdaki 1. Sorunun o meþhur JOIN ve GROUP BY'lý SQL Sorgusu
                string query = @"SELECT S.SubeAdi, COUNT(K.KargoID) AS ToplamCikanKargo, SUM(F.Toplam_Tutar) AS ToplamGelir
                                 FROM Subeler S
                                 INNER JOIN Kargolar K ON S.SubeID = K.CikisSubeID
                                 INNER JOIN Faturalar F ON K.KargoID = F.KargoID
                                 GROUP BY S.SubeAdi";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            raporlar.Add(new SubeIstatistikViewModel
                            {
                                SubeAdi = reader["SubeAdi"].ToString(),
                                ToplamKargoSayisi = Convert.ToInt32(reader["ToplamCikanKargo"]),
                                // Eðer para kýsmý boþ(null) gelirse hata vermesin diye kontrol yapýyoruz:
                                ToplamCiro = reader["ToplamGelir"] != DBNull.Value ? Convert.ToDecimal(reader["ToplamGelir"]) : 0
                            });
                        }
                    }
                }
            }
            return View(raporlar);
        }

        // TÜM TABLOLARI DÝNAMÝK GÖSTEREN SÝHÝRLÝ METOT
        public IActionResult TabloGoster(string id = "Kargolar")
        {
            // Güvenlik: Sadece bizim belirlediðimiz tablolara girilebilsin (SQL Injection Korumasý)
            var izinVerilenTablolar = new List<string> { "Musteriler", "Subeler", "Personeller", "Araclar", "Kargolar", "KargoHareketleri", "Faturalar", "Teslimatlar", "KargoDurumTanimi" };

            // Eðer adres çubuðuna saçma sapan bir tablo adý yazýlýrsa, varsayýlan olarak Kargolar'ý aç
            if (!izinVerilenTablolar.Contains(id))
            {
                id = "Kargolar";
            }

            DataTable dt = new DataTable();
            string connectionString = _config.GetConnectionString("KargoDbBaglantisi");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Tablo adýný dinamik olarak SQL sorgusuna yerleþtiriyoruz
                string query = $"SELECT * FROM {id}";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        // Gelen tüm kolon ve satýrlarý DataTable içine doldur
                        da.Fill(dt);
                    }
                }
            }

            // Hangi tabloda olduðumuzu ve tüm tablo listesini ekrana (View'a) gönderiyoruz
            ViewBag.AktifTablo = id;
            ViewBag.TabloListesi = izinVerilenTablolar;

            return View(dt);
       }

        // 1. KARGO EKLEME SAYFASINI AÇAN METOT (GET)
        [HttpGet]
        public IActionResult KargoEkle()
        {
            List<SelectListItem> musteriler = new List<SelectListItem>();
            List<SelectListItem> subeler = new List<SelectListItem>();
            string connectionString = _config.GetConnectionString("KargoDbBaglantisi");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                // Müþterileri Çek
                using (SqlCommand cmd = new SqlCommand("SELECT MusteriID, Ad + ' ' + Soyad AS AdSoyad FROM Musteriler", con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        musteriler.Add(new SelectListItem { Value = reader["MusteriID"].ToString(), Text = reader["AdSoyad"].ToString() });
                    }
                }
                // Þubeleri Çek
                using (SqlCommand cmd = new SqlCommand("SELECT SubeID, SubeAdi FROM Subeler", con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subeler.Add(new SelectListItem { Value = reader["SubeID"].ToString(), Text = reader["SubeAdi"].ToString() });
                    }
                }
            }

            ViewBag.Musteriler = musteriler;
            ViewBag.Subeler = subeler;
            return View();
        }

        // 2. FORMDAN GELEN VERÝLERÝ VERÝTABANINA YAZAN METOT (POST)
        [HttpPost]
        public IActionResult KargoEkle(int GondericiID, int AliciID, int CikisSubeID, int VarisSubeID, decimal Desi, decimal Agirlik, string KargoTipi)
        {
            // Otomatik, benzersiz bir Takip Kodu üretiyoruz (Örn: TR-20260307123045)
            string takipKodu = "TR-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string connectionString = _config.GetConnectionString("KargoDbBaglantisi");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                // Kargolar tablosuna yeni kaydý ekleyen SQL (INSERT INTO)
                string query = @"INSERT INTO Kargolar (GondericiID, AliciID, CikisSubeID, VarisSubeID, TakipKodu, Desi, Agirlik, KargoTipi) 
                                 VALUES (@gId, @aId, @cSId, @vSId, @kod, @desi, @agirlik, @tip);
                                 SELECT SCOPE_IDENTITY();"; // Eklenen kargonun ID'sini anýnda geri alýyoruz

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@gId", GondericiID);
                    cmd.Parameters.AddWithValue("@aId", AliciID);
                    cmd.Parameters.AddWithValue("@cSId", CikisSubeID);
                    cmd.Parameters.AddWithValue("@vSId", VarisSubeID);
                    cmd.Parameters.AddWithValue("@kod", takipKodu);
                    cmd.Parameters.AddWithValue("@desi", Desi);
                    cmd.Parameters.AddWithValue("@agirlik", Agirlik);
                    cmd.Parameters.AddWithValue("@tip", KargoTipi);

                    // Kargo eklendi, þimdi o kargoya ait ilk "Kargo Kabul Edildi" hareketini girelim
                    int yeniKargoId = Convert.ToInt32(cmd.ExecuteScalar());
                    string hareketQuery = @"INSERT INTO KargoHareketleri (KargoID, IslemSubeID, IslemPersonelID, DurumID, Aciklama) 
                        VALUES (@kId, @sId, (SELECT TOP 1 PersonelID FROM Personeller WHERE CalistigiSubeID = @sId), 1, 'Kargo þubeden teslim alýndý ve sisteme girildi.')";
                    using (SqlCommand hCmd = new SqlCommand(hareketQuery, con))
                    {
                        hCmd.Parameters.AddWithValue("@kId", yeniKargoId);
                        hCmd.Parameters.AddWithValue("@sId", CikisSubeID);
                        hCmd.ExecuteNonQuery();
                    }
                }
            }
            // Ýþlem bitince ana sayfadaki tabloya yönlendir ve girdiði kodu aratsýn
            return RedirectToAction("Index");
        }


    }
}