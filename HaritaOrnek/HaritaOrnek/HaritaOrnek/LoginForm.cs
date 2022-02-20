using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaritaOrnek
{
    public partial class LoginForm : Form
    {
        TeslimatDurumForm teslimatDurumForm = new TeslimatDurumForm();
        SignUpForm signUpForm = new SignUpForm();

        public LoginForm()
        {
            InitializeComponent();
        }

      
  

        private void kayitBtn_Click_1(object sender, EventArgs e)
        {
            signUpForm.Visible = true;
            this.Visible = false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
            if (kullaniciTxt.Text == "" || sifreTxt.Text == "")
            {
                MessageBox.Show("Şifre veya kullanıcı adı boş geçilemez!");

            }
            else if (kullaniciTxt.Text == "emre" && sifreTxt.Text == "123")
            {
                MessageBox.Show("Giriş Başarılı!");
                teslimatDurumForm.Visible = true;
                this.Visible = false;
            }
            else
            {
                MessageBox.Show("Böyle bir kullanıcı bulunamadı!");
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
