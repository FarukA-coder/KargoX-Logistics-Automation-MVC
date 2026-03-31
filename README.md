# 📦 KargoX - Lojistik ve Kargo Takip Otomasyonu

Bu proje, C# ASP.NET Core MVC ve MS SQL Server kullanılarak baştan sona (Full-Stack) geliştirilmiş, **ileri düzey veritabanı mimarisine sahip** kapsamlı bir lojistik ve kargo takip otomasyonudur.

## 🗄️ İleri Düzey Veritabanı Mimarisi (Projenin Odak Noktası)
Bu proje standart bir CRUD uygulamasının ötesine geçerek, veri bütünlüğünü ve güvenliğini veritabanı (SQL Server) katmanında sağlamak üzere tasarlanmıştır. Veritabanı 3. Normal Form (3NF) kurallarına göre normalize edilmiştir.

* **Sanal Tablolar (VIEW):** Kullanıcılara karmaşık ID'ler yerine anlaşılır metinler sunmak amacıyla, 5 farklı tabloyu `INNER JOIN` ile birleştiren (`vw_KargoGenelDurum`) sanal kargo takip ekranları oluşturulmuştur.
* **Saklı Yordamlar (STORED PROCEDURE):** Dış uygulamalardan girilen "Takip Kodu" parametresini alarak, o kargoya ait tüm geçmiş işlem hareketlerini kronolojik olarak getiren parametrik yordamlar (`sp_KargoGecmisiSorgula`) yazılmıştır.
* **Tetikleyiciler (TRIGGER) ile Veri Güvenliği:** Teslimatı tamamlanmış kargoların üzerine yanlışlıkla veya kötü niyetle yeni bir işlem kaydı girilmesini engelleyen ve işlemi otomatik iptal eden (`ROLLBACK TRANSACTION`) sistem bekçisi (`trg_TeslimEdilmisKargoyuKoru`) kodlanmıştır.
* **Karmaşık Sorgular ve Raporlama:** Şube performanslarını, toplam kargo çıkışlarını ve elde edilen ciroları hesaplamak için `GROUP BY`, `SUM`, `COUNT` gibi Aggregate fonksiyonları kullanılarak dinamik rapor sorguları hazırlanmıştır.
* **Büyük Veri Simülasyonu (T-SQL Döngüleri):** Sistem performansını test etmek amacıyla; T-SQL `WHILE` döngüleri, `NEWID()`, `CHECKSUM` ve `CASE` yapıları kullanılarak veritabanına tek seferde **5000 adet tamamen gerçekçi (Ahmet Yılmaz vb.) müşteri verisi** ve şubelere dağıtılmış 500 personel verisi otomatik olarak eklenmiştir.

## 💻 Kullanılan Teknolojiler
* **Backend:** C# ASP.NET Core MVC
* **Veritabanı:** MS SQL Server (T-SQL, Stored Procedures, Triggers, Views)
* **Frontend:** HTML5, CSS3, Bootstrap 5 (Tamamen Responsive)
* **Mimari:** Katmanlı Mimari (MVC), İlişkisel Veritabanı (RDBMS) Tasarımı

## 🚀 Kurulum ve Çalıştırma
Projeyi kendi bilgisayarınızda test etmek için:
1. Depoyu klonlayın.
2. Proje dizinindeki `Veritabani_Scriptleri` klasörünü açın.
3. Öncelikle **`1_Tablo_Kurulumu.sql`** dosyasını SQL Server'da çalıştırarak veritabanı iskeletini oluşturun.
4. Ardından **`2_Gercekci_5000_Veri.sql`** dosyasını çalıştırarak sistemi test verileriyle doldurun.
5. (Opsiyonel) **`3_Ileri_Duzey_Nesneler.sql`** dosyası ile Trigger, View ve Procedure'leri sisteme dahil edin.
6. Visual Studio üzerinden projeyi derleyip çalıştırın.

---
*Geliştirici: Faruk Akmeşe | Veritabanı Yönetim Sistemleri Projesi*
