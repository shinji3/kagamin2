namespace Kagamin2
{
    partial class AuthDiag
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OKBTN = new System.Windows.Forms.Button();
            this.ExitBTN = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.authid = new System.Windows.Forms.TextBox();
            this.authpass = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // OKBTN
            // 
            this.OKBTN.Location = new System.Drawing.Point(168, 102);
            this.OKBTN.Name = "OKBTN";
            this.OKBTN.Size = new System.Drawing.Size(75, 23);
            this.OKBTN.TabIndex = 0;
            this.OKBTN.Text = "OK";
            this.OKBTN.UseVisualStyleBackColor = true;
            this.OKBTN.Click += new System.EventHandler(this.OKBTN_Click);
            // 
            // ExitBTN
            // 
            this.ExitBTN.Location = new System.Drawing.Point(278, 102);
            this.ExitBTN.Name = "ExitBTN";
            this.ExitBTN.Size = new System.Drawing.Size(75, 23);
            this.ExitBTN.TabIndex = 1;
            this.ExitBTN.Text = "中止";
            this.ExitBTN.UseVisualStyleBackColor = true;
            this.ExitBTN.Click += new System.EventHandler(this.ExitBTN_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "認証情報を入力してください";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "ユーザー名";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "パスワード";
            // 
            // authid
            // 
            this.authid.Location = new System.Drawing.Point(75, 35);
            this.authid.Name = "authid";
            this.authid.Size = new System.Drawing.Size(278, 19);
            this.authid.TabIndex = 5;
            // 
            // authpass
            // 
            this.authpass.Location = new System.Drawing.Point(75, 60);
            this.authpass.Name = "authpass";
            this.authpass.PasswordChar = '●';
            this.authpass.Size = new System.Drawing.Size(278, 19);
            this.authpass.TabIndex = 6;
            // 
            // AuthDiag
            // 
            this.AcceptButton = this.OKBTN;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ExitBTN;
            this.ClientSize = new System.Drawing.Size(365, 137);
            this.Controls.Add(this.authpass);
            this.Controls.Add(this.authid);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ExitBTN);
            this.Controls.Add(this.OKBTN);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AuthDiag";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "認証情報入力";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKBTN;
        private System.Windows.Forms.Button ExitBTN;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox authid;
        private System.Windows.Forms.TextBox authpass;
    }
}