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
    public partial class KullaniciEkle : Form
    {
        public KullaniciEkle(AnaForm anaForm, FormControl formControl)
        {
            InitializeComponent();

            this.anaForm = anaForm;
            this.formControl = formControl;
        }
        
        AnaForm anaForm;
        FormControl formControl;
        DatabaseControl databaseControl = new DatabaseControl();

        private void KulTanit_Load(object sender, EventArgs e)
        {
            List<kullanici> kullanicis = databaseControl.GetKullanici();
            if (kullanicis != null)
            {
                foreach (var kullanci in kullanicis)
                {
                    if (kullanci != null && !string.IsNullOrEmpty(kullanci.username))
                    {
                        cbKullaniciKodu.Items.Add(kullanci.username);
                    }
                }
            }
        }

        private void btnYeniKullanici_Click(object sender, EventArgs e)
        {
            KullaniciSayfasi kullaniciPage = new KullaniciSayfasi();
            kullaniciPage.btnGuncelle.Text = "Kaydet";
            this.Close();
            formControl.Open(kullaniciPage);

        }

        private void cbKullaniciKodu_SelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbKullaniciKodu.Text))
            {
                return;
            }
            
            string kullaniciKodu = cbKullaniciKodu.Text;
            kullanici kullanicis = databaseControl.GetKullanici(kullaniciKodu);
            
            if (kullanicis != null)
            {
                formControl.Open(new KullaniciSayfasi(kullanicis));
            }
            else
            {
                MessageBox.Show("Kullanıcı bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
