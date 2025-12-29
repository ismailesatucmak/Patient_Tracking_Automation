using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace SOHATS
{
    /// <summary>
    /// Veritabanı dosyasını otomatik oluşturur ve başlangıç verilerini yükler
    /// </summary>
    public static class DatabaseInitializer
    {
        private static readonly string DbFileName = "SOHATS.mdf";
        private static readonly string LogFileName = "SOHATS_log.ldf";

        public static void Initialize()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string mdfPath = Path.Combine(appPath, DbFileName);
            string ldfPath = Path.Combine(appPath, LogFileName);

            // Veritabanı dosyası zaten varsa, başlatmaya gerek yok
            if (File.Exists(mdfPath))
            {
                return;
            }

            try
            {
                // LocalDB'ye bağlanarak veritabanını oluştur
                CreateDatabase(mdfPath, ldfPath);
                
                // Tabloları ve başlangıç verilerini oluştur
                CreateTablesAndSeedData(mdfPath);
                
                MessageBox.Show("Veritabanı başarıyla oluşturuldu!\n\nVarsayılan giriş bilgileri:\nKullanıcı Adı: MUDUR\nŞifre: 123456789", 
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı oluşturulurken hata oluştu:\n{ex.Message}\n\nLütfen SQL Server LocalDB'nin yüklü olduğundan emin olun.", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CreateDatabase(string mdfPath, string ldfPath)
        {
            string masterConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True";
            
            using (SqlConnection connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();
                
                string createDbSql = $@"
                    CREATE DATABASE [SOHATS]
                    ON PRIMARY (
                        NAME = N'SOHATS',
                        FILENAME = N'{mdfPath}',
                        SIZE = 8192KB,
                        MAXSIZE = UNLIMITED,
                        FILEGROWTH = 65536KB
                    )
                    LOG ON (
                        NAME = N'SOHATS_log',
                        FILENAME = N'{ldfPath}',
                        SIZE = 8192KB,
                        MAXSIZE = 2048GB,
                        FILEGROWTH = 65536KB
                    )";

                using (SqlCommand command = new SqlCommand(createDbSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Veritabanını detach et (dosya bazlı kullanım için)
                string detachSql = @"
                    ALTER DATABASE [SOHATS] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    EXEC sp_detach_db 'SOHATS', 'true';";
                
                using (SqlCommand command = new SqlCommand(detachSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void CreateTablesAndSeedData(string mdfPath)
        {
            string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={mdfPath};Initial Catalog=SOHATS;Integrated Security=True";
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Tabloları oluştur
                ExecuteNonQuery(connection, GetCreateTablesScript());
                
                // Başlangıç verilerini ekle
                ExecuteNonQuery(connection, GetSeedDataScript());
            }
        }

        private static void ExecuteNonQuery(SqlConnection connection, string sql)
        {
            // GO ifadelerini ayır ve her birini ayrı çalıştır
            string[] batches = sql.Split(new[] { "\r\nGO\r\n", "\nGO\n", "\r\nGO", "GO\r\n", "\nGO", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string batch in batches)
            {
                string trimmedBatch = batch.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedBatch) && trimmedBatch.ToUpper() != "GO")
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand(trimmedBatch, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException)
                    {
                        // Bazı hatalar göz ardı edilebilir (örn: tablo zaten var)
                    }
                }
            }
        }

        private static string GetCreateTablesScript()
        {
            return @"
-- cikis tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'cikis')
CREATE TABLE [dbo].[cikis](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [dosyano] [int] NOT NULL,
    [sevktarihi] [nvarchar](10) NOT NULL,
    [cikissaati] [datetime] NULL,
    [odeme] [nvarchar](20) NULL,
    [toplamtutar] [nvarchar](20) NULL,
    CONSTRAINT [PK_cikis] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

-- hasta tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'hasta')
CREATE TABLE [dbo].[hasta](
    [tckimlikno] [nvarchar](11) NOT NULL,
    [dosyano] [int] IDENTITY(1,1) NOT NULL,
    [ad] [nvarchar](15) NULL,
    [soyad] [nvarchar](15) NULL,
    [dogumyeri] [nvarchar](15) NULL,
    [dogumtarihi] [datetime] NULL,
    [babaadi] [nvarchar](15) NULL,
    [anneadi] [nvarchar](15) NULL,
    [cinsiyet] [nvarchar](5) NULL,
    [kangrubu] [nvarchar](5) NULL,
    [medenihal] [nvarchar](5) NULL,
    [adres] [nvarchar](255) NULL,
    [tel] [nvarchar](11) NULL,
    [kurumsicilno] [nvarchar](10) NULL,
    [kurumadi] [nvarchar](50) NULL,
    [yakintel] [nvarchar](11) NULL,
    [yakinkurumsicilno] [nvarchar](10) NULL,
    [yakinkurumadi] [nvarchar](50) NULL,
    CONSTRAINT [PK_hasta] PRIMARY KEY CLUSTERED ([tckimlikno] ASC)
)
GO

-- islem tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'islem')
CREATE TABLE [dbo].[islem](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [islemAdi] [nvarchar](50) NULL,
    [birimFiyat] [nvarchar](50) NULL,
    CONSTRAINT [PK_islem] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

-- kullanici tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'kullanici')
CREATE TABLE [dbo].[kullanici](
    [kodu] [int] IDENTITY(1,1) NOT NULL,
    [ad] [nvarchar](20) NULL,
    [soyad] [nvarchar](20) NULL,
    [sifre] [nvarchar](20) NULL,
    [yetki] [nvarchar](5) NULL,
    [evtel] [nvarchar](11) NULL,
    [ceptel] [nvarchar](11) NULL,
    [adres] [nvarchar](255) NULL,
    [unvan] [nvarchar](15) NULL,
    [isebaslama] [datetime] NULL,
    [maas] [nvarchar](20) NULL,
    [dogumyeri] [nvarchar](50) NULL,
    [annead] [nvarchar](20) NULL,
    [babaad] [nvarchar](20) NULL,
    [cinsiyet] [nvarchar](5) NULL,
    [kangrubu] [nvarchar](10) NULL,
    [medenihal] [nvarchar](10) NULL,
    [dogumtarihi] [datetime] NULL,
    [tckimlikno] [nvarchar](11) NULL,
    [username] [nvarchar](50) NULL,
    CONSTRAINT [PK_kullanici] PRIMARY KEY CLUSTERED ([kodu] ASC)
)
GO

-- poliklinik tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'poliklinik')
CREATE TABLE [dbo].[poliklinik](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [poliklinikadi] [nvarchar](50) NULL,
    [durum] [nvarchar](5) NULL,
    [aciklama] [nvarchar](255) NULL,
    CONSTRAINT [PK_poliklinik] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

-- sevk tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'sevk')
CREATE TABLE [dbo].[sevk](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [sevktarihi] [datetime] NOT NULL,
    [dosyano] [nvarchar](50) NOT NULL,
    [poliklinik] [nvarchar](50) NULL,
    [saat] [nvarchar](10) NULL,
    [yapilanislem] [nvarchar](50) NULL,
    [drkod] [char](10) NULL,
    [miktar] [char](3) NULL,
    [birimfiyat] [nvarchar](20) NULL,
    [sira] [nvarchar](3) NULL,
    [toplamtutar] [nvarchar](50) NULL,
    [taburcu] [nvarchar](50) NULL,
    CONSTRAINT [PK_sevk] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO
";
        }

        private static string GetSeedDataScript()
        {
            return @"
-- Kullanıcılar (varsayılan admin ve test kullanıcıları)
SET IDENTITY_INSERT [dbo].[kullanici] ON
GO

IF NOT EXISTS (SELECT * FROM [dbo].[kullanici] WHERE [username] = 'MUDUR')
INSERT [dbo].[kullanici] ([kodu], [ad], [soyad], [sifre], [yetki], [evtel], [ceptel], [adres], [unvan], [isebaslama], [maas], [dogumyeri], [annead], [babaad], [cinsiyet], [kangrubu], [medenihal], [dogumtarihi], [tckimlikno], [username]) 
VALUES (1, N'Admin', N'Kullanıcı', N'123456789', N'true', N'02121234567', N'05001234567', N'Merkez', N'Doktor', CAST(N'2020-01-01T00:00:00.000' AS DateTime), N'10000', N'İstanbul', N'Ayşe', N'Mehmet', N'BAY', N'A Rh+', N'EVLİ', CAST(N'1980-01-01T00:00:00.000' AS DateTime), N'12345678901', N'MUDUR')
GO

IF NOT EXISTS (SELECT * FROM [dbo].[kullanici] WHERE [username] = 'user')
INSERT [dbo].[kullanici] ([kodu], [ad], [soyad], [sifre], [yetki], [evtel], [ceptel], [adres], [unvan], [isebaslama], [maas], [dogumyeri], [annead], [babaad], [cinsiyet], [kangrubu], [medenihal], [dogumtarihi], [tckimlikno], [username]) 
VALUES (2, N'Test', N'Kullanıcı', N'123', N'false', N'02129876543', N'05009876543', N'Şube', N'Sağlık Personel', CAST(N'2021-01-01T00:00:00.000' AS DateTime), N'5000', N'Ankara', N'Fatma', N'Ali', N'BAYAN', N'B Rh+', N'BEKAR', CAST(N'1990-06-15T00:00:00.000' AS DateTime), N'98765432109', N'user')
GO

SET IDENTITY_INSERT [dbo].[kullanici] OFF
GO

-- İşlemler
SET IDENTITY_INSERT [dbo].[islem] ON
GO

IF NOT EXISTS (SELECT * FROM [dbo].[islem] WHERE [islemAdi] = 'a tahlil')
INSERT [dbo].[islem] ([id], [islemAdi], [birimFiyat]) VALUES (1, N'a tahlil', N'15')
GO

IF NOT EXISTS (SELECT * FROM [dbo].[islem] WHERE [islemAdi] = 'b tahlil')
INSERT [dbo].[islem] ([id], [islemAdi], [birimFiyat]) VALUES (2, N'b tahlil', N'30')
GO

IF NOT EXISTS (SELECT * FROM [dbo].[islem] WHERE [islemAdi] = 'c tahlil')
INSERT [dbo].[islem] ([id], [islemAdi], [birimFiyat]) VALUES (3, N'c tahlil', N'60')
GO

IF NOT EXISTS (SELECT * FROM [dbo].[islem] WHERE [islemAdi] = 'muayene')
INSERT [dbo].[islem] ([id], [islemAdi], [birimFiyat]) VALUES (4, N'muayene', N'50')
GO

IF NOT EXISTS (SELECT * FROM [dbo].[islem] WHERE [islemAdi] = 'kan tahlili')
INSERT [dbo].[islem] ([id], [islemAdi], [birimFiyat]) VALUES (5, N'kan tahlili', N'15')
GO

SET IDENTITY_INSERT [dbo].[islem] OFF
GO

-- Poliklinikler
SET IDENTITY_INSERT [dbo].[poliklinik] ON
GO

IF NOT EXISTS (SELECT * FROM [dbo].[poliklinik] WHERE [poliklinikadi] = 'Poliklinik 1')
INSERT [dbo].[poliklinik] ([id], [poliklinikadi], [durum], [aciklama]) VALUES (1, N'Poliklinik 1', N'True', N'Genel Poliklinik')
GO

IF NOT EXISTS (SELECT * FROM [dbo].[poliklinik] WHERE [poliklinikadi] = 'Poliklinik 2')
INSERT [dbo].[poliklinik] ([id], [poliklinikadi], [durum], [aciklama]) VALUES (2, N'Poliklinik 2', N'True', N'Dahiliye')
GO

IF NOT EXISTS (SELECT * FROM [dbo].[poliklinik] WHERE [poliklinikadi] = 'Poliklinik 3')
INSERT [dbo].[poliklinik] ([id], [poliklinikadi], [durum], [aciklama]) VALUES (3, N'Poliklinik 3', N'True', N'Çocuk Sağlığı')
GO

IF NOT EXISTS (SELECT * FROM [dbo].[poliklinik] WHERE [poliklinikadi] = 'KBB')
INSERT [dbo].[poliklinik] ([id], [poliklinikadi], [durum], [aciklama]) VALUES (4, N'KBB', N'True', N'Kulak Burun Boğaz')
GO

SET IDENTITY_INSERT [dbo].[poliklinik] OFF
GO

-- Örnek hasta
SET IDENTITY_INSERT [dbo].[hasta] ON
GO

IF NOT EXISTS (SELECT * FROM [dbo].[hasta] WHERE [tckimlikno] = '11111111111')
INSERT [dbo].[hasta] ([tckimlikno], [dosyano], [ad], [soyad], [dogumyeri], [dogumtarihi], [babaadi], [anneadi], [cinsiyet], [kangrubu], [medenihal], [adres], [tel], [kurumsicilno], [kurumadi], [yakintel], [yakinkurumsicilno], [yakinkurumadi]) 
VALUES (N'11111111111', 1, N'Örnek', N'Hasta', N'İstanbul', CAST(N'1985-05-15T00:00:00.000' AS DateTime), N'Ahmet', N'Ayşe', N'BAY', N'A Rh+', N'EVLİ', N'Test Adresi', N'05001112233', NULL, NULL, NULL, NULL, NULL)
GO

SET IDENTITY_INSERT [dbo].[hasta] OFF
GO
";
        }
    }
}
