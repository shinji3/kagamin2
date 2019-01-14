using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Kagamin2
{
    public partial class AuthDiag : Form
    {
        private Status k;

        public AuthDiag(Status _k)
        {
            k = _k;
            if (Front.AuthDiagflag)
                this.Text = _k.ImportHost + "へ接続中";
            else
                this.Text = "認証情報入力";
                    
            InitializeComponent();
        }

        private void OKBTN_Click(object sender, EventArgs e)
        {
            if (authid.Text == "" || authpass.Text == "")
            {
                MessageBox.Show("IDまたはパスワードが入力されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Front.AuthDiagflag)
            {
                k.ImportAuthID = authid.Text;
                k.ImportAuthPass = authpass.Text;
            }
            else
            {
                k.AuthID = authid.Text;
                k.AuthPass = authpass.Text;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ExitBTN_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

    }
}
