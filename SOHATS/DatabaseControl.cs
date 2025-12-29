using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SOHATS.DB;

namespace SOHATS
{
    class DatabaseControl
    {
        public DatabaseControl()
        {

        }

        public List<bool> kullaniciGirisi(string kullaniciAdi, string sifre)
        {
            // Null veya boş kontrol
            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                return new List<bool> { false, false, true };
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    kullanici kullanici = context.kullanici
                        .FirstOrDefault(p => p.username == kullaniciAdi && p.sifre == sifre);

                    if (kullanici == null)
                    {
                        return new List<bool> { false, false, true };
                    }

                    bool yetki = false;
                    if (!string.IsNullOrEmpty(kullanici.yetki))
                    {
                        bool.TryParse(kullanici.yetki, out yetki);
                    }

                    return new List<bool> { true, yetki, true };
                }
            }
            catch (System.Data.Entity.Core.EntityException)
            {
                return new List<bool> { false, false, false };
            }
            catch (Exception)
            {
                return new List<bool> { false, false, false };
            }
        }

        public sevk GetSevk(string dosyaNo)
        {
            if (string.IsNullOrEmpty(dosyaNo))
            {
                return new sevk { dosyano = null };
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.sevk.FirstOrDefault(p => p.dosyano == dosyaNo);
                    return result ?? new sevk { dosyano = null };
                }
            }
            catch (Exception)
            {
                return new sevk { dosyano = null };
            }
        }

        public sevk GetSevkLast(string dosyaNo)
        {
            if (string.IsNullOrEmpty(dosyaNo))
            {
                return new sevk { dosyano = null };
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var sevkler = context.sevk.Where(p => p.dosyano == dosyaNo).ToList();
                    if (sevkler == null || !sevkler.Any())
                    {
                        return new sevk { dosyano = null };
                    }
                    return sevkler.OrderByDescending(s => s.id).FirstOrDefault() ?? new sevk { dosyano = null };
                }
            }
            catch (Exception)
            {
                return new sevk { dosyano = null };
            }
        }

        public sevk GetSevkId(int id)
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.sevk.FirstOrDefault(p => p.id == id);
                    return result ?? new sevk { dosyano = null };
                }
            }
            catch (Exception)
            {
                return new sevk { dosyano = null };
            }
        }

        public List<DateTime> GetOncekiİslemler(string dosyaNo)
        {
            if (string.IsNullOrEmpty(dosyaNo))
            {
                return new List<DateTime>();
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var sorgu = context.sevk.Where(p => p.dosyano == dosyaNo).ToList();
                    if (sorgu == null || !sorgu.Any())
                    {
                        return new List<DateTime>();
                    }
                    return sorgu.Select(p => p.sevktarihi).Distinct().ToList();
                }
            }
            catch (Exception)
            {
                return new List<DateTime>();
            }
        }

        public hasta GetHasta(string dosyaNo)
        {
            if (string.IsNullOrEmpty(dosyaNo))
            {
                return null;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    int dosya;
                    if (!int.TryParse(dosyaNo, out dosya))
                    {
                        return null;
                    }
                    return context.hasta.FirstOrDefault(p => p.dosyano == dosya);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public int GetKullaniciKodu()
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    if (!context.kullanici.Any())
                    {
                        return 0;
                    }
                    return context.kullanici.Max(p => p.kodu);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public kullanici GetKullanici(string kullaniciKodu)
        {
            if (string.IsNullOrEmpty(kullaniciKodu))
            {
                return null;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    return context.kullanici.FirstOrDefault(p => p.username == kullaniciKodu);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<kullanici> GetKullanici(bool doktor)
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.kullanici
                        .Where(p => p.unvan != null && p.unvan.ToUpper() == "DOKTOR")
                        .ToList();
                    return result ?? new List<kullanici>();
                }
            }
            catch (Exception)
            {
                return new List<kullanici>();
            }
        }

        public List<islem> GetIslem()
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    return context.islem.ToList() ?? new List<islem>();
                }
            }
            catch (Exception)
            {
                return new List<islem>();
            }
        }

        public islem GetFiyat(string islemAdi)
        {
            if (string.IsNullOrEmpty(islemAdi))
            {
                return new islem { birimFiyat = "0" };
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.islem.FirstOrDefault(p => p.islemAdi == islemAdi);
                    return result ?? new islem { birimFiyat = "0" };
                }
            }
            catch (Exception)
            {
                return new islem { birimFiyat = "0" };
            }
        }

        public sevk Sira(string poliklinik, DateTime sevkTarihi)
        {
            if (string.IsNullOrEmpty(poliklinik))
            {
                return new sevk { sira = "0" };
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    List<sevk> sevkler = context.sevk
                        .Where(p => p.poliklinik == poliklinik && p.sevktarihi == sevkTarihi)
                        .ToList();

                    if (sevkler == null || !sevkler.Any())
                    {
                        return new sevk { sira = "0" };
                    }

                    // En son sırayı bul
                    int maxSira = 0;
                    foreach (sevk s in sevkler)
                    {
                        int currentSira;
                        if (int.TryParse(s.sira, out currentSira) && currentSira > maxSira)
                        {
                            maxSira = currentSira;
                        }
                    }
                    return new sevk { sira = maxSira.ToString() };
                }
            }
            catch (Exception)
            {
                return new sevk { sira = "0" };
            }
        }

        public bool AddSevk(sevk sevk)
        {
            if (sevk == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    context.sevk.Add(sevk);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sevk eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<sevk> GetYapilanTahlilİslemler(string dosyaNo, DateTime tarih)
        {
            if (string.IsNullOrEmpty(dosyaNo))
            {
                return new List<sevk>();
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.sevk
                        .Where(p => p.dosyano == dosyaNo && p.sevktarihi == tarih)
                        .ToList();
                    return result ?? new List<sevk>();
                }
            }
            catch (Exception)
            {
                return new List<sevk>();
            }
        }

        public List<string> GetUnvan()
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.kullanici
                        .Where(x => x.unvan != null)
                        .Select(x => x.unvan)
                        .Distinct()
                        .ToList();
                    return result ?? new List<string>();
                }
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public List<kullanici> GetKullanici()
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    return context.kullanici.ToList() ?? new List<kullanici>();
                }
            }
            catch (Exception)
            {
                return new List<kullanici>();
            }
        }

        public bool DeleteKullanici(kullanici kullanici1)
        {
            if (kullanici1 == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var existing = context.kullanici.Find(kullanici1.kodu);
                    if (existing != null)
                    {
                        context.kullanici.Remove(existing);
                        context.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanıcı silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UpdateKullanici(kullanici kullanici1)
        {
            if (kullanici1 == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var entity = context.Entry(kullanici1);
                    entity.State = EntityState.Modified;
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanıcı güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UpdateSevk(sevk s, bool durum)
        {
            if (s == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var entity = context.Entry(s);
                    entity.State = EntityState.Modified;
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sevk güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool AddKullanici(kullanici kullanici1)
        {
            if (kullanici1 == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    context.kullanici.Add(kullanici1);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanıcı eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public poliklinik GetPoliklinik(string adi)
        {
            if (string.IsNullOrEmpty(adi))
            {
                return new poliklinik { poliklinikadi = null };
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.poliklinik.FirstOrDefault(p => p.poliklinikadi == adi);
                    return result ?? new poliklinik { poliklinikadi = null };
                }
            }
            catch (Exception)
            {
                return new poliklinik { poliklinikadi = null };
            }
        }

        public List<poliklinik> GetPoliklinik(bool aktif)
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.poliklinik
                        .Where(x => x.durum != null && x.durum.ToUpper() == "TRUE")
                        .ToList();
                    return result ?? new List<poliklinik>();
                }
            }
            catch (Exception)
            {
                return new List<poliklinik>();
            }
        }

        public bool AddPoliklinik(poliklinik poliklinik)
        {
            if (poliklinik == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    context.poliklinik.Add(poliklinik);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Poliklinik eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UpdatePoliklinik(poliklinik polik)
        {
            if (polik == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var entity = context.Entry(polik);
                    entity.State = EntityState.Modified;
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Poliklinik güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool DeleteHasta(hasta hasta)
        {
            if (hasta == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var existing = context.hasta.Find(hasta.tckimlikno);
                    if (existing != null)
                    {
                        context.hasta.Remove(existing);
                        context.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hasta silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool DeletePoliklinik(poliklinik polik)
        {
            if (polik == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var existing = context.poliklinik.Find(polik.id);
                    if (existing != null)
                    {
                        context.poliklinik.Remove(existing);
                        context.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Poliklinik silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public kullanici DoktorAdi(int Kodu)
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.kullanici.FirstOrDefault(p => p.kodu == Kodu);
                    return result ?? new kullanici { ad = "Bilinmiyor", soyad = "" };
                }
            }
            catch (Exception)
            {
                return new kullanici { ad = "Bilinmiyor", soyad = "" };
            }
        }

        public int GetYeniDosyaNumarasi()
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    if (!context.hasta.Any())
                    {
                        return 1;
                    }
                    var maxDosyaNo = context.hasta.Max(p => p.dosyano);
                    return maxDosyaNo + 1;
                }
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public bool addHasta(hasta hasta)
        {
            if (hasta == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    context.hasta.Add(hasta);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                MessageBox.Show("Bu TC Kimlik numarası ile kayıtlı hasta zaten mevcut!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hasta eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UpdateHasta(hasta hasta)
        {
            if (hasta == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var entity = context.Entry(hasta);
                    entity.State = EntityState.Modified;
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hasta güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool DeleteIslem(sevk sevk)
        {
            if (sevk == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var existing = context.sevk.Find(sevk.id);
                    if (existing != null)
                    {
                        context.sevk.Remove(existing);
                        context.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İşlem silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public hasta GetHastaKimlikNo(string kimlikNo)
        {
            if (string.IsNullOrEmpty(kimlikNo))
            {
                return new hasta { tckimlikno = "0" };
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.hasta.FirstOrDefault(p => p.tckimlikno == kimlikNo);
                    return result ?? new hasta { tckimlikno = "0" };
                }
            }
            catch (Exception)
            {
                return new hasta { tckimlikno = "0" };
            }
        }

        public hasta GetHastaKurumSicilNo(string kurumSicilNo)
        {
            if (string.IsNullOrEmpty(kurumSicilNo))
            {
                return new hasta { tckimlikno = "0" };
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    var result = context.hasta.FirstOrDefault(p => p.kurumsicilno == kurumSicilNo);
                    return result ?? new hasta { tckimlikno = "0" };
                }
            }
            catch (Exception)
            {
                return new hasta { tckimlikno = "0" };
            }
        }

        public hasta GetHastaDosyaNo(string dosyaNo)
        {
            if (string.IsNullOrEmpty(dosyaNo))
            {
                return new hasta { tckimlikno = "0" };
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    int dosya;
                    if (!int.TryParse(dosyaNo, out dosya))
                    {
                        return new hasta { tckimlikno = "0" };
                    }
                    var result = context.hasta.FirstOrDefault(p => p.dosyano == dosya);
                    return result ?? new hasta { tckimlikno = "0" };
                }
            }
            catch (Exception)
            {
                return new hasta { tckimlikno = "0" };
            }
        }

        public List<hasta> GetHastaAdSoyad(string ad, string soyad, bool durum)
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    // Null veya boş kontrol
                    ad = ad ?? "";
                    soyad = soyad ?? "";

                    List<hasta> result;
                    if (durum)
                    {
                        result = context.hasta
                            .Where(p => (p.ad != null && p.ad.Contains(ad)) && 
                                       (p.soyad != null && p.soyad.Contains(soyad)))
                            .ToList();
                    }
                    else
                    {
                        result = context.hasta
                            .Where(p => (p.ad != null && p.ad.Contains(ad)) || 
                                       (p.soyad != null && p.soyad.Contains(soyad)))
                            .ToList();
                    }
                    return result ?? new List<hasta>();
                }
            }
            catch (Exception)
            {
                return new List<hasta>();
            }
        }

        public bool AddCikis(cikis cikis)
        {
            if (cikis == null)
            {
                return false;
            }

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    context.cikis.Add(cikis);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Çıkış kaydı eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public int GetCikisId()
        {
            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    if (!context.cikis.Any())
                    {
                        return 1;
                    }
                    return context.cikis.Max(p => p.id) + 1;
                }
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public List<RaporTaburcu> GetTaburcu(DateTime baslangic, DateTime bitis)
        {
            List<RaporTaburcu> raporTaburcu = new List<RaporTaburcu>();

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    List<sevk> sevks = context.sevk
                        .Where(t => t.sevktarihi > baslangic && t.sevktarihi < bitis)
                        .ToList();

                    if (sevks == null || !sevks.Any())
                    {
                        return raporTaburcu;
                    }

                    foreach (sevk sevk in sevks)
                    {
                        try
                        {
                            RaporTaburcu hasta = new RaporTaburcu();
                            hasta.Dosyano = sevk.dosyano ?? "";

                            var hastaInfo = this.GetHasta(sevk.dosyano);
                            if (hastaInfo != null)
                            {
                                hasta.Ad = hastaInfo.ad ?? "";
                                hasta.Soyad = hastaInfo.soyad ?? "";
                            }
                            else
                            {
                                hasta.Ad = "";
                                hasta.Soyad = "";
                            }

                            hasta.Sevktarihi = sevk.sevktarihi;
                            hasta.Poliklinik = sevk.poliklinik ?? "";

                            int drKod;
                            if (!string.IsNullOrEmpty(sevk.drkod) && int.TryParse(sevk.drkod.Trim(), out drKod))
                            {
                                var doktor = this.DoktorAdi(drKod);
                                if (doktor != null)
                                {
                                    hasta.Doktoradi = doktor.ad ?? "";
                                    hasta.Doktorsoyad = doktor.soyad ?? "";
                                }
                                else
                                {
                                    hasta.Doktoradi = "";
                                    hasta.Doktorsoyad = "";
                                }
                            }
                            else
                            {
                                hasta.Doktoradi = "";
                                hasta.Doktorsoyad = "";
                            }

                            raporTaburcu.Add(hasta);
                        }
                        catch (Exception)
                        {
                            // Bu kayıt atlanır, diğerlerine devam edilir
                            continue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return raporTaburcu;
            }

            return raporTaburcu;
        }

        public List<RaporTaburcu> GetTaburcu(bool durum, DateTime baslangic, DateTime bitis)
        {
            List<RaporTaburcu> raporTaburcu = new List<RaporTaburcu>();

            try
            {
                using (SOHATSEntities context = new SOHATSEntities())
                {
                    string durumStr = durum.ToString();
                    List<sevk> sevks = context.sevk
                        .Where(p => p.sevktarihi > baslangic &&
                                   p.sevktarihi < bitis &&
                                   p.taburcu == durumStr)
                        .ToList();

                    if (sevks == null || !sevks.Any())
                    {
                        return raporTaburcu;
                    }

                    foreach (sevk sevk in sevks)
                    {
                        try
                        {
                            RaporTaburcu hasta = new RaporTaburcu();
                            hasta.Dosyano = sevk.dosyano ?? "";

                            var hastaInfo = this.GetHasta(sevk.dosyano);
                            if (hastaInfo != null)
                            {
                                hasta.Ad = hastaInfo.ad ?? "";
                                hasta.Soyad = hastaInfo.soyad ?? "";
                            }
                            else
                            {
                                hasta.Ad = "";
                                hasta.Soyad = "";
                            }

                            hasta.Sevktarihi = sevk.sevktarihi;
                            hasta.Poliklinik = sevk.poliklinik ?? "";

                            int drKod;
                            if (!string.IsNullOrEmpty(sevk.drkod) && int.TryParse(sevk.drkod.Trim(), out drKod))
                            {
                                var doktor = this.DoktorAdi(drKod);
                                if (doktor != null)
                                {
                                    hasta.Doktoradi = doktor.ad ?? "";
                                    hasta.Doktorsoyad = doktor.soyad ?? "";
                                }
                                else
                                {
                                    hasta.Doktoradi = "";
                                    hasta.Doktorsoyad = "";
                                }
                            }
                            else
                            {
                                hasta.Doktoradi = "";
                                hasta.Doktorsoyad = "";
                            }

                            raporTaburcu.Add(hasta);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return raporTaburcu;
            }

            return raporTaburcu;
        }
    }
}
