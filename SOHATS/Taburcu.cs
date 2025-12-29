using SOHATS.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOHATS
{
    public partial class Taburcu : Form
    {
        public Taburcu(FormControl formControl,string tutar,string dosyano,List<sevk> sevkler)
        {
            InitializeComponent();
            this.formControl = formControl;
            this.tutar = tutar;
            this.dosyano = dosyano;
            this.sevkler = sevkler;    
        }

        FormControl formControl;
        string tutar;
        string dosyano;
        List<sevk> sevkler;

        DatabaseControl databaseControl = new DatabaseControl();

        private void btnGiris_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Taburcu_Load(object sender, EventArgs e)
        {
            txtDosyaNo.Text = dosyano ?? "";
            txtTutar.Text = tutar ?? "0 TL";
            
            if (sevkler != null && sevkler.Count > 0)
            {
                dtpSevkTarihi.Text = sevkler[0].sevktarihi.ToShortDateString();
            }
            else
            {
                dtpSevkTarihi.Value = DateTime.Today;
            }
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if(cbOdeme.Text == "")
            {
                MessageBox.Show("Lütfen bir ödeme yöntemi giriniz");
                return;
            }
            
            if (sevkler == null || sevkler.Count == 0)
            {
                MessageBox.Show("Sevk bilgisi bulunamadı!");
                return;
            }
            
            try
            {
                int dosyaNoInt;
                if (!int.TryParse(txtDosyaNo.Text, out dosyaNoInt))
                {
                    MessageBox.Show("Geçersiz dosya numarası!");
                    return;
                }
                
                cikis cikis = new cikis()
                {
                    id = databaseControl.GetCikisId(),
                    dosyano = dosyaNoInt,
                    sevktarihi = dtpSevkTarihi.Value.ToShortDateString(),
                    cikissaati = DateTime.Now,
                    odeme = cbOdeme.Text,
                    toplamtutar = txtTutar.Text ?? "0"
                };
                databaseControl.AddCikis(cikis);

                foreach(sevk sevk in sevkler)
                {
                    sevk s = databaseControl.GetSevkId(sevk.id);
                    if (s != null && s.dosyano != null)
                    {
                        s.taburcu = "True";
                        databaseControl.UpdateSevk(s, true);
                    }
                }

                MessageBox.Show("Çıkışınız Tamamlanmıştır");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Taburcu işlemi sırasında hata oluştu: " + ex.Message, "Hata", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
