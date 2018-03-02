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
            
            // �E�C���h�E�^�C�g��
            this.Text = Event + " - �ڍאݒ�";

            // �f�t�H���g�l�ݒ�
            this.radioTime.Checked = true;
            this.optWeek.SelectedIndex = 0;
            this.optHour.Value = this.optHour.Minimum;
            this.optMin.Value = this.optMin.Minimum;
            this.optTrfType.SelectedIndex = 0;
            this.optTrfVal.Value = this.optTrfVal.Minimum;
            this.optTrfUnit.SelectedIndex = 0;
            this.optConn.Value = Front.Gui.Conn;
            this.optResv.Value = Front.Gui.Reserve;

            //�g���t�B�b�N�w��͎����
            //radioTraffic.Enabled = false;

            //data�`��
            //[�N������],[�j��],[��],[��],[��r���]����],[�]���ʒl],[�]���ʒP��],[�ʏ�g],[���U�g]
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

            // �`�F�b�N��ԍX�V
            radioTime_CheckedChanged(null, EventArgs.Empty);

            // �C�x���g��ʂɂ���Đڑ��g�ݒ�̗L��/������؂�ւ���
            switch (Event)
            {
                case "�|�[�g�Ҏ�J�n":
                case "�ڑ��g���ύX":
                    //�ڑ��g�ݒ�L��
                    grpConn.Enabled = true;
                    break;
                default:
                    //���̑��͐ڑ��g�ݒ薳��
                    grpConn.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// ���ʎ擾
        /// </summary>
        /// <returns></returns>
        public string GetResult()
        {
            return Data;
        }

        /// <summary>
        /// OK�{�^�����������Ƃ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okBTN_Click(object sender, EventArgs e)
        {
            // Data�ɑޔ����ďI��
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
        /// �L�����Z���{�^�����������Ƃ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelBTN_Click(object sender, EventArgs e)
        {
            // �Ƃ��ɂȂɂ��Ȃ�
        }

        private void radioTime_CheckedChanged(object sender, EventArgs e)
        {
            if (radioTime.Checked)
            {
                // ���Ԏw�肪�L���ȂƂ�
                this.optWeek.Enabled = true;
                this.optHour.Enabled = true;
                this.optMin.Enabled = true;
                this.optTrfType.Enabled = false;
                this.optTrfVal.Enabled = false;
                this.optTrfUnit.Enabled = false;
            }
            else
            {
                // �]���ʎw�肪�L���ȂƂ�
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