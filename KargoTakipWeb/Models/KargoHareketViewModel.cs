namespace KargoTakipWeb.Models
{
    public class KargoHareketViewModel
    {
        public DateTime IslemZamani { get; set; }
        public string TakipKodu { get; set; }
        public string IslemYeri { get; set; }
        public string IslemYapanPersonel { get; set; }
        public string KargoDurumu { get; set; }
        public string Aciklama { get; set; }
    }
}