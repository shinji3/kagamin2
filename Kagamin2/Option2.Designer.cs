namespace Kagamin2
{
    partial class Option2
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.optWeek = new System.Windows.Forms.ComboBox();
            this.optHour = new System.Windows.Forms.NumericUpDown();
            this.optMin = new System.Windows.Forms.NumericUpDown();
            this.radioTime = new System.Windows.Forms.RadioButton();
            this.radioTraffic = new System.Windows.Forms.RadioButton();
            this.optTrfType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.optTrfVal = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.grpStartTiming = new System.Windows.Forms.GroupBox();
            this.optTrfUnit = new System.Windows.Forms.ComboBox();
            this.grpConn = new System.Windows.Forms.GroupBox();
            this.optResv = new System.Windows.Forms.NumericUpDown();
            this.optConn = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cancelBTN = new System.Windows.Forms.Button();
            this.okBTN = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.optHour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.optMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.optTrfVal)).BeginInit();
            this.grpStartTiming.SuspendLayout();
            this.grpConn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.optResv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.optConn)).BeginInit();
            this.SuspendLayout();
            // 
            // optWeek
            // 
            this.optWeek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.optWeek.FormattingEnabled = true;
            this.optWeek.Items.AddRange(new object[] {
            "日曜",
            "月曜",
            "火曜",
            "水曜",
            "木曜",
            "金曜",
            "土曜",
            "毎日",
            "平日",
            "土日"});
            this.optWeek.Location = new System.Drawing.Point(22, 35);
            this.optWeek.Name = "optWeek";
            this.optWeek.Size = new System.Drawing.Size(59, 20);
            this.optWeek.TabIndex = 0;
            // 
            // optHour
            // 
            this.optHour.Location = new System.Drawing.Point(87, 36);
            this.optHour.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.optHour.Name = "optHour";
            this.optHour.Size = new System.Drawing.Size(38, 19);
            this.optHour.TabIndex = 1;
            this.optHour.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.optHour.Value = new decimal(new int[] {
            23,
            0,
            0,
            0});
            // 
            // optMin
            // 
            this.optMin.Location = new System.Drawing.Point(146, 36);
            this.optMin.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.optMin.Name = "optMin";
            this.optMin.Size = new System.Drawing.Size(38, 19);
            this.optMin.TabIndex = 2;
            this.optMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.optMin.Value = new decimal(new int[] {
            59,
            0,
            0,
            0});
            // 
            // radioTime
            // 
            this.radioTime.AutoSize = true;
            this.radioTime.Checked = true;
            this.radioTime.Location = new System.Drawing.Point(6, 18);
            this.radioTime.Name = "radioTime";
            this.radioTime.Size = new System.Drawing.Size(71, 16);
            this.radioTime.TabIndex = 3;
            this.radioTime.TabStop = true;
            this.radioTime.Text = "日時指定";
            this.radioTime.UseVisualStyleBackColor = true;
            this.radioTime.CheckedChanged += new System.EventHandler(this.radioTime_CheckedChanged);
            // 
            // radioTraffic
            // 
            this.radioTraffic.AutoSize = true;
            this.radioTraffic.Location = new System.Drawing.Point(6, 62);
            this.radioTraffic.Name = "radioTraffic";
            this.radioTraffic.Size = new System.Drawing.Size(98, 16);
            this.radioTraffic.TabIndex = 4;
            this.radioTraffic.TabStop = true;
            this.radioTraffic.Text = "UP転送量指定";
            this.radioTraffic.UseVisualStyleBackColor = true;
            // 
            // optTrfType
            // 
            this.optTrfType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.optTrfType.FormattingEnabled = true;
            this.optTrfType.Items.AddRange(new object[] {
            "日間総転送量",
            "月間総転送量"});
            this.optTrfType.Location = new System.Drawing.Point(22, 79);
            this.optTrfType.Name = "optTrfType";
            this.optTrfType.Size = new System.Drawing.Size(103, 20);
            this.optTrfType.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(127, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "時";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(187, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "分";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(128, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "が";
            // 
            // optTrfVal
            // 
            this.optTrfVal.Location = new System.Drawing.Point(146, 80);
            this.optTrfVal.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.optTrfVal.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.optTrfVal.Name = "optTrfVal";
            this.optTrfVal.Size = new System.Drawing.Size(64, 19);
            this.optTrfVal.TabIndex = 9;
            this.optTrfVal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.optTrfVal.Value = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(257, 83);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "を超えた時";
            // 
            // grpStartTiming
            // 
            this.grpStartTiming.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpStartTiming.Controls.Add(this.optTrfUnit);
            this.grpStartTiming.Controls.Add(this.radioTime);
            this.grpStartTiming.Controls.Add(this.label4);
            this.grpStartTiming.Controls.Add(this.optWeek);
            this.grpStartTiming.Controls.Add(this.optTrfVal);
            this.grpStartTiming.Controls.Add(this.optHour);
            this.grpStartTiming.Controls.Add(this.label3);
            this.grpStartTiming.Controls.Add(this.optMin);
            this.grpStartTiming.Controls.Add(this.label2);
            this.grpStartTiming.Controls.Add(this.radioTraffic);
            this.grpStartTiming.Controls.Add(this.label1);
            this.grpStartTiming.Controls.Add(this.optTrfType);
            this.grpStartTiming.Location = new System.Drawing.Point(4, 4);
            this.grpStartTiming.Name = "grpStartTiming";
            this.grpStartTiming.Size = new System.Drawing.Size(316, 108);
            this.grpStartTiming.TabIndex = 11;
            this.grpStartTiming.TabStop = false;
            this.grpStartTiming.Text = "実行条件";
            // 
            // optTrfUnit
            // 
            this.optTrfUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.optTrfUnit.FormattingEnabled = true;
            this.optTrfUnit.Items.AddRange(new object[] {
            "MB",
            "GB"});
            this.optTrfUnit.Location = new System.Drawing.Point(212, 79);
            this.optTrfUnit.Name = "optTrfUnit";
            this.optTrfUnit.Size = new System.Drawing.Size(43, 20);
            this.optTrfUnit.TabIndex = 12;
            // 
            // grpConn
            // 
            this.grpConn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConn.Controls.Add(this.optResv);
            this.grpConn.Controls.Add(this.optConn);
            this.grpConn.Controls.Add(this.label6);
            this.grpConn.Controls.Add(this.label5);
            this.grpConn.Location = new System.Drawing.Point(4, 118);
            this.grpConn.Name = "grpConn";
            this.grpConn.Size = new System.Drawing.Size(316, 45);
            this.grpConn.TabIndex = 12;
            this.grpConn.TabStop = false;
            this.grpConn.Text = "枠設定";
            // 
            // optResv
            // 
            this.optResv.Location = new System.Drawing.Point(154, 19);
            this.optResv.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.optResv.Name = "optResv";
            this.optResv.Size = new System.Drawing.Size(42, 19);
            this.optResv.TabIndex = 11;
            this.optResv.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.optResv.Value = new decimal(new int[] {
            999,
            0,
            0,
            0});
            // 
            // optConn
            // 
            this.optConn.Location = new System.Drawing.Point(50, 18);
            this.optConn.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.optConn.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.optConn.Name = "optConn";
            this.optConn.Size = new System.Drawing.Size(42, 19);
            this.optConn.TabIndex = 10;
            this.optConn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.optConn.Value = new decimal(new int[] {
            999,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(98, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 1;
            this.label6.Text = "リザーブ枠";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "通常枠";
            // 
            // cancelBTN
            // 
            this.cancelBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBTN.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBTN.Location = new System.Drawing.Point(238, 169);
            this.cancelBTN.Name = "cancelBTN";
            this.cancelBTN.Size = new System.Drawing.Size(82, 24);
            this.cancelBTN.TabIndex = 13;
            this.cancelBTN.Text = "キャンセル(&C)";
            this.cancelBTN.Click += new System.EventHandler(this.cancelBTN_Click);
            // 
            // okBTN
            // 
            this.okBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okBTN.AutoSize = true;
            this.okBTN.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okBTN.Location = new System.Drawing.Point(150, 169);
            this.okBTN.Name = "okBTN";
            this.okBTN.Size = new System.Drawing.Size(82, 24);
            this.okBTN.TabIndex = 12;
            this.okBTN.Text = "OK(&O)";
            this.okBTN.Click += new System.EventHandler(this.okBTN_Click);
            // 
            // Option2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 197);
            this.Controls.Add(this.cancelBTN);
            this.Controls.Add(this.okBTN);
            this.Controls.Add(this.grpConn);
            this.Controls.Add(this.grpStartTiming);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Option2";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "詳細設定";
            ((System.ComponentModel.ISupportInitialize)(this.optHour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.optMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.optTrfVal)).EndInit();
            this.grpStartTiming.ResumeLayout(false);
            this.grpStartTiming.PerformLayout();
            this.grpConn.ResumeLayout(false);
            this.grpConn.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.optResv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.optConn)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox optWeek;
        private System.Windows.Forms.NumericUpDown optHour;
        private System.Windows.Forms.NumericUpDown optMin;
        private System.Windows.Forms.RadioButton radioTime;
        private System.Windows.Forms.RadioButton radioTraffic;
        private System.Windows.Forms.ComboBox optTrfType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown optTrfVal;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox grpStartTiming;
        private System.Windows.Forms.ComboBox optTrfUnit;
        private System.Windows.Forms.GroupBox grpConn;
        private System.Windows.Forms.NumericUpDown optConn;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown optResv;
        private System.Windows.Forms.Button cancelBTN;
        private System.Windows.Forms.Button okBTN;
    }
}