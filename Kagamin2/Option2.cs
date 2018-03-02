using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Kagamin2
{
    public partial class Option2 : Form
    {
        public string Event;
        public string Port;
        public string Data;

        public Option2(string _event, string _port, string _data)
        {
            InitializeComponent();

            this.optWeek.Items.Clear();
            this.optWeek.Items.AddRange(Front.ScheduleWeekString);
            this.optTrfType.Items.Clear();
            this.optTrfType.Items.AddRange(Front.ScheduleTrfTypeString);
            this.optTrfUnit.Items.Clear();
            this.optTrfUnit.Items.AddRange(Front.ScheduleTrfUnitString);

            Event = _event;
            Port = _port;
            Data = _data;
            
            // ウインドウタイトル
            this.Text = Event + " - 詳細設定";

            // デフォルト値設定
            this.radioTime.Checked = true;
            this.optWeek.SelectedIndex = 0;
            this.optHour.Value = this.optHour.Minimum;
            this.optMin.Value = this.optMin.Minimum;
            this.optTrfType.SelectedIndex = 0;
            this.optTrfVal.Value = this.optTrfVal.Minimum;
            this.optTrfUnit.SelectedIndex = 0;
            this.optConn.Value = Front.Gui.Conn;
            this.optResv.Value = Front.Gui.Reserve;

            //トラフィック指定は首締め
            //radioTraffic.Enabled = false;

            //data形式
            //[起動条件],[曜日],[時],[分],[比較元転送量],[転送量値],[転送量単位],[通常枠],[リザ枠]
            string[] _str = _data.Split(',');
            if (_str.Length == 9)
            {
                try
                {
                    uint[] _val = new uint[9];
                    for (int cnt = 0; cnt < 9; cnt++)
                        _val[cnt] = uint.Parse(_str[cnt]);

                    if (_val[1] >= this.optWeek.Items.Count) _val[1] = 0;
                    if (_val[2] > this.optHour.Maximum) _val[2] = (uint)this.optHour.Minimum;
                    if (_val[3] > this.optMin.Maximum) _val[3] = (uint)this.optMin.Minimum;
                    if (_val[4] >= this.optTrfType.Items.Count) _val[4] = 0;
                    if (_val[5] > this.optTrfVal.Maximum) _val[5] = (uint)this.optTrfVal.Minimum;
                    if (_val[6] > this.optTrfUnit.Items.Count) _val[6] = 0;
                    if (_val[7] > this.optConn.Maximum) _val[7] = Front.Gui.Conn;
                    if (_val[8] > this.optResv.Maximum) _val[8] = Front.Gui.Reserve;

                    if (_val[0] == 0)
                        this.radioTime.Checked = true;
                    else
                        this.radioTraffic.Checked = true;
                    this.optWeek.SelectedIndex = (int)_val[1];
                    this.optHour.Value = _val[2];
                    this.optMin.Value = _val[3];
                    this.optTrfType.SelectedIndex = (int)_val[4];
                    this.optTrfVal.Value = _val[5];
                    this.optTrfUnit.SelectedIndex = (int)_val[6];
                    this.optConn.Value = _val[7];
                    this.optResv.Value = _val[8];
                }
                catch { }
            }

            // チェック状態更新
            radioTime_CheckedChanged(null, EventArgs.Empty);

            // イベント種別によって接続枠設定の有効/無効を切り替える
            switch (Event)
            {
                case "ポート待受開始":
                case "接続枠数変更":
                    //接続枠設定有効
                    grpConn.Enabled = true;
                    break;
                default:
                    //その他は接続枠設定無効
                    grpConn.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// 結果取得
        /// </summary>
        /// <returns></returns>
        public string GetResult()
        {
            return Data;
        }

        /// <summary>
        /// OKボタンを押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okBTN_Click(object sender, EventArgs e)
        {
            // Dataに退避して終了
            if (this.radioTime.Checked)
                Data = "0,";
            else
                Data = "1,";
            Data += optWeek.SelectedIndex.ToString() + ",";
            Data += optHour.Value.ToString() + ",";
            Data += optMin.Value.ToString() + ",";
            Data += optTrfType.SelectedIndex.ToString() + ",";
            Data += optTrfVal.Value.ToString() + ",";
            Data += optTrfUnit.SelectedIndex.ToString() + ",";
            Data += optConn.Value.ToString() + ",";
            Data += optResv.Value.ToString();
        }

        /// <summary>
        /// キャンセルボタンを押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelBTN_Click(object sender, EventArgs e)
        {
            // とくになにもなし
        }

        private void radioTime_CheckedChanged(object sender, EventArgs e)
        {
            if (radioTime.Checked)
            {
                // 時間指定が有効なとき
                this.optWeek.Enabled = true;
                this.optHour.Enabled = true;
                this.optMin.Enabled = true;
                this.optTrfType.Enabled = false;
                this.optTrfVal.Enabled = false;
                this.optTrfUnit.Enabled = false;
            }
            else
            {
                // 転送量指定が有効なとき
                this.optWeek.Enabled = false;
                this.optHour.Enabled = false;
                this.optMin.Enabled = false;
                this.optTrfType.Enabled = true;
                this.optTrfVal.Enabled = true;
                this.optTrfUnit.Enabled = true;
            }
        }
    }
}