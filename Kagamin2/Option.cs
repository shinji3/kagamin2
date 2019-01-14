using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace Kagamin2
{
    partial class Option : Form
    {
        public Option()
        {
            InitializeComponent();

            /*
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            */

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.VersionLabel.Text = Front.AppName;

            this.optBandType.Items.Clear();
            this.optBandType.Items.AddRange(Front.BandStopTypeString);
            this.optBandUnit.Items.Clear();
            this.optBandUnit.Items.AddRange(Front.BandStopUnitString);
            this.clmScheduleEvent.Items.Clear();
            this.clmScheduleEvent.Items.AddRange(Front.ScheduleEventString);
            /*
            this.optStopSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            this.optStopSystem.DropDownStyle = ComboBoxStyle.DropDownList;
            
             */
            // Front→オプション画面に反映
            LoadSetting();

        }
        private void Option_Load(object sender, EventArgs e)
        {
            // DataGridViewのカラム幅リカバリ
            string[] _clm;
            _clm = Front.Form.ScheduleColumn.Split(',');
            if (_clm.Length == 4)
            {
                try
                {
                    clmScheduleEnable.Width = int.Parse(_clm[0]);
                    clmScheduleEvent.Width = int.Parse(_clm[1]);
                    clmSchedulePort.Width = int.Parse(_clm[2]);
                    clmScheduleOption.Width = int.Parse(_clm[3]);
                }
                catch { }
            }
            _clm = null;
        }

        private void Option_FormClosing(object sender, FormClosingEventArgs e)
        {
            // カラムサイズ保存
            Front.Form.ScheduleColumn = "" + clmScheduleEnable.Width + "," + clmScheduleEvent.Width + "," + clmSchedulePort.Width + "," + clmScheduleOption.Width;
        }
        #region 設定の読み書き
        /// <summary>
        /// Front→GUIに設定を反映
        /// FormLoad時の１回だけ起動される
        /// </summary>
        private void LoadSetting()
        {
            // GUI上のポート一覧とスケジュール設定ポートリストに反映
            optPortList.Text = "";
            clmSchedulePort.Items.Clear();
            foreach (int _i in Front.Gui.PortList)
            {
                optPortList.Text += _i.ToString() + Environment.NewLine;
                clmSchedulePort.Items.Add(_i.ToString());
            }
            foreach (SCHEDULE i in Front.ScheduleItem)
            {
                if (!clmSchedulePort.Items.Contains(i.Port) && i.Port != 0)
                    clmSchedulePort.Items.Add(i.Port.ToString());
            }
            // スケジュール設定ポートリストに「ALL」を追加
            clmSchedulePort.Items.Add("ALL");

            #region 基本設定
            //自動再試行
            optInSecond.Value = Front.Retry.InRetryInterval;
            optInTime.Value = Front.Retry.InRetryTime;
            optOutSecond.Value = Front.Retry.OutRetryInterval;
            optOutTime.Value = Front.Retry.OutRetryTime;

            //帯域制限
            if (optBandType.Items.Count > Front.BndWth.BandStopMode)
                optBandType.SelectedIndex = (int)Front.BndWth.BandStopMode;
            optBandValue.Value = Front.BndWth.BandStopValue;
            if (optBandUnit.Items.Count > Front.BndWth.BandStopUnit)
                optBandUnit.SelectedIndex = (int)Front.BndWth.BandStopUnit;
            optBandReserve.Checked = Front.BndWth.BandStopResv;

            //連打キック
            optKickSecond.Value = Front.Kick.KickCheckSecond;
            optKickTime.Value = Front.Kick.KickCheckTime;
            optKickDenyTime.Value = Front.Kick.KickDenyTime;

            //ログ出力
            optKagamiLogFile.Text = Front.Log.KagamiLogFile;
            optKagamiDetailLog.Checked = Front.Log.LogDetail;
            optHpLogFile.Text = Front.Log.HpLogFile;
            #endregion

            #region 鏡置き場
            //エントランス
            if (Front.Hp.UseHP)
            {
                optUseHP.Checked = true;
                groupKgm1.Enabled = true;
                groupKgm2.Enabled = true;
            }
            else
            {
                optUseHP.Checked = false;
                groupKgm1.Enabled = false;
                groupKgm2.Enabled = false;
            }
            foreach (string _str in Front.Acl.DenyHost)
            {
                if (!string.IsNullOrEmpty(_str))
                    optDenyHost.Text += _str + "\r\n";
            }
            foreach (string _str in Front.Acl.DenyUA)
            {
                if (!string.IsNullOrEmpty(_str))
                    optDenyUA.Text += _str + "\r\n";
            }

            optHPAddr.Text = Front.Hp.IpHTTP;
            optHPPort.Text = Front.Hp.PortHTTP.ToString();
            optHPDir.Text = Front.Hp.PublicDir;
            optHPDenyList.Clear();
            //エントランス接続制限
            foreach (string s in Front.Acl.HpDenyRemoteHost)
                optHPDenyList.Text += s + Environment.NewLine;
            if (Front.DenyLogin != null)
            {
                foreach (string s in Front.DenyLogin)
                    optHPDenyList.Text += s + "(LoginDeny)" + Environment.NewLine;
            }
            if (optHPDenyList.Text.Length >= Environment.NewLine.Length)
                optHPDenyList.Text = optHPDenyList.Text.Remove(optHPDenyList.Text.Length - Environment.NewLine.Length); // 余計な末尾の改行消し

            //インポート接続制限
            foreach (string s in Front.Acl.DenyImportURL)
                optDenyList.Text += s + Environment.NewLine;
            if (optDenyList.Text.Length >= Environment.NewLine.Length)
                optDenyList.Text = optDenyList.Text.Remove(optDenyList.Text.Length - Environment.NewLine.Length); // 余計な末尾の改行消し
            if (Front.Acl.LimitSameImportURL == 0)
            {
                optSameImportIPKick.Checked = false;
                optSameImportIPNum.Enabled = false;
            }
            else
            {
                optSameImportIPKick.Checked = true;
                optSameImportIPNum.Enabled = true;
                optSameImportIPNum.Value = Front.Acl.LimitSameImportURL;
            }
            if (Front.Acl.ImportOutTime == 0)
            {
                optImportKick.Checked = false;
                optImportOutTime.Enabled = false;
            }
            else
            {
                optImportKick.Checked = true;
                optImportOutTime.Enabled = true;
                optImportOutTime.Value = Front.Acl.ImportOutTime;
            }
            ///インポートURLと設定者IPの一致チェック
            optSetUserIpCheck.Checked = Front.Acl.SetUserIpCheck;

            optClientTimeKick.Checked = Front.Acl.ClientOutCheck;
            optClientNum.Enabled = Front.Acl.ClientOutCheck;
            optClientNum.Value = Front.Acl.ClientOutNum;
            optClientOutTime.Enabled = Front.Acl.ClientOutCheck;
            optClientOutTime.Value = Front.Acl.ClientOutTime;
            if (Front.Acl.ClientOutCheck == true)
            {
                optClientNotIPEnable.Enabled = true;
                optClientNotIPEnable.Checked = Front.Acl.ClientNotIPCheck;
            }
            else
            {
                optClientNotIPEnable.Enabled = false;
                optClientNotIPEnable.Checked = false;
            }
            ///待機中ポートがあれば自動解放しない
            optPortFullOnly.Checked = Front.Acl.PortFullOnlyCheck;

            optAuthEnable.Checked = Front.Hp.AuthEnable;
            optAuthPass.Text = Front.Hp.AuthPass;
            optAuthUser.Text = Front.Hp.AuthUser;
            #endregion

            #region 詳細設定
            //通信関連
            optConnTimeOut.Value = Front.Sock.SockConnTimeout;
            optRecvTimeOut.Value = Front.Sock.SockRecvTimeout;
            optSendTimeOut.Value = Front.Sock.SockSendTimeout;
            optSendQueueSize.Value = Front.Sock.SockSendQueueSize;
            optSockCloseDelay.Value = Front.Sock.SockCloseDelay;
            optUPnPEnable.Checked = Front.Sock.upnp;
            optVirtualHost.Checked = Front.Sock.VirtualHost;
            optBusyConSend.Checked = Front.Sock.ConnInfoSend;
            //その他詳細
            optBalloonTip.Checked = Front.Opt.BalloonTip;
            optBrowser.Checked = Front.Opt.BrowserView;
            if (Front.Opt.BrowserViewMode == false)
                optBrowserTextMode.Checked = true;
            else
                optBrowserHtmlMode.Checked = true;
            optPriKagamiexe.Checked = Front.Opt.PriKagamiexe;
            optPriKagamin.Checked = Front.Opt.PriKagamin;
            if (Front.Acl.LimitSameClient == 0)
            {
                optSameClientKick.Checked = false;
                optSameClientLabel.Enabled = false;
                optSameClientNum.Value = 1;
                optSameClientNum.Enabled = false;
            }
            else
            {
                optSameClientKick.Checked = true;
                optSameClientLabel.Enabled = true;
                optSameClientNum.Value = Front.Acl.LimitSameClient;
                optSameClientNum.Enabled = true;
            }
            optEnablePush.Checked = Front.Opt.EnablePush;
            optEnableInfo.Checked = Front.Opt.EnableInfo;
            optEnableAdmin.Checked = Front.Opt.EnableAdmin;
            optAdminPass.Text = Front.Opt.AdminPass;

            opttranskagamin.Checked = Front.Opt.TransKagamin;
            optTransPerEnable.Checked = Front.Opt.NotMyTrans;
            optEnableImportRedirect.Checked = Front.Opt.ImportRedirect;
            optInTray.Checked = Front.Opt.InTrayOn;
            optWebKick.Checked = Front.Opt.WebKick;

            if (Front.Opt.EnableAdmin)
            {
                optAdminLabel.Enabled = true;
                optAdminPass.Enabled = true;
            }
            else
            {
                optAdminLabel.Enabled = false;
                optAdminPass.Enabled = false;
            }
            // サウンド再生
            optSndConnOK.Text = Front.Opt.SndConnOkFile;
            optSndConnNG.Text = Front.Opt.SndConnNgFile;
            optSndDisc.Text = Front.Opt.SndDiscFile;
            #endregion

            #region 転送量制限
            /*optStopCheck.Checked = Front.Acl.StopFlag;
            groupBox3.Enabled = optStopCheck.Checked;
            optDLCheck.Checked = Front.Acl.DLContainFlag;
            optStopSystem.SelectedIndex = (int)Front.Acl.StopSelect;
            optStopSelect.SelectedIndex = (int)Front.Acl.StopMBSelect;

            if (Front.Acl.TrafficHour == 2)
                optHourTraffic.CheckState = CheckState.Checked;
            if (Front.Acl.TrafficHour == 1)
                optHourTraffic.CheckState = CheckState.Indeterminate;
            if (Front.Acl.TrafficHour == 0)
                optHourTraffic.CheckState = CheckState.Unchecked;

            try
            {
                if (Front.Acl.StopMBSelect == 0)
                {
                    optStopMB.Value = Front.Acl.StopMB;
                }
                else if (Front.Acl.StopMBSelect == 1)
                {
                    optStopMB.Value = Front.Acl.StopMB / 1024;
                }
                else if (Front.Acl.StopMBSelect == 2)
                {
                    optStopMB.Value = Front.Acl.StopMB / (1024 * 1024);
                }
                else
                {
                    MessageBox.Show("単位が不明です。(" + Front.Acl.StopMBSelect.ToString() + ")");
                    Front.Acl.StopMBSelect = 0;
                    Front.Acl.StopMB = 25600;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                Front.Acl.StopMBSelect = 0;
                Front.Acl.StopMB = 25600;
            }
            */
            #endregion
            #region スケジュール起動
            for (int _num = 0; _num < Front.ScheduleItem.Count; _num++)
            {
                string _port = "";
                string _set = "";
                // パラメータチェック
                if (Front.ScheduleItem[_num].Event >= clmScheduleEvent.Items.Count) Front.ScheduleItem[_num].Event = 0;
                if (Front.ScheduleItem[_num].Port == 0)
                    _port = "ALL";// 全ポート指定
                else if (clmSchedulePort.Items.Contains(Front.ScheduleItem[_num].Port.ToString()))
                    _port = Front.ScheduleItem[_num].Port.ToString();// 特定ポート指定
                _set = Front.ScheduleItem[_num].StartType.ToString() + "," +
                    Front.ScheduleItem[_num].Week.ToString() + "," +
                    Front.ScheduleItem[_num].Hour.ToString() + "," +
                    Front.ScheduleItem[_num].Min.ToString() + "," +
                    Front.ScheduleItem[_num].TrfType.ToString() + "," +
                    Front.ScheduleItem[_num].TrfValue.ToString() + "," +
                    Front.ScheduleItem[_num].TrfUnit.ToString() + "," +
                    Front.ScheduleItem[_num].Conn.ToString() + "," +
                    Front.ScheduleItem[_num].Resv.ToString();
                scheduleDataView.Rows.Add(
                    Front.ScheduleItem[_num].Enable,
                    clmScheduleEvent.Items[(int)Front.ScheduleItem[_num].Event].ToString(),
                    _port,
                    "",
                    _set
                    );
                resetClmScheduleOption(_num);
            }
            #endregion
            #region その他
            ClosedEnable.Checked = (Front.Acl.ClosedPassward == "") ? false : true;
            optPrivatePassword.Text = Front.Acl.ClosedPassward;
            optPrivateComment.Text = Front.Acl.ClosedComment;

            optStartHP.Checked = Front.Hp.HPstartON;
            optStartPort.Checked = Front.Hp.PortstartON;

            optLoinOut.Checked = (Front.Acl.FailureCount == 0) ? false : true;
            optLoginCount.Value = Front.Acl.FailureCount;

            /*
            optpasssql.Text = Front.Opt.PassMySQL;
            optsqlhost.Text = Front.Opt.HostMySQL;
            optpasssql.Text = Front.Opt.PassMySQL;
            mySQLOn.Checked = Front.Opt.MySQLEnable;
            optDateBaseName.Text = Front.Opt.RearDB;
            optReadCulum.Text = Front.Opt.ReadRowMySQL;
            optReadIP.Text = Front.Opt.ReadRowIPMySQL;
            optHyou.Text = Front.Opt.ReadHyou;
            optusersql.Text = Front.Opt.UserMuSQL;
            */
             
            optWMPEnable.Checked = Front.Opt.ViewWMP;

            ClosedEnable_CheckedChanged(null, EventArgs.Empty);
            optLoinOut_CheckedChanged(null, EventArgs.Empty);
            optStartHP_CheckedChanged(null, EventArgs.Empty);
            AuthIDText.Text = Front.Opt.AuthID;
            AuthPassText.Text = Front.Opt.AuthPass;
            optWebExport.Checked = Front.Opt.AuthWebSet;
            for (int i = 0; i < optStartPortList.Items.Count; i++)
            {
                foreach (int _port in Front.Hp.StartPortList)
                {
                    if (_port == int.Parse(optStartPortList.Items[i].ToString()))
                        optStartPortList.SetItemChecked(i, true);
                }
            }
            #endregion
            optAuthEnable_CheckedChanged(null, null);
        }

        /// <summary>
        /// GUI→Frontに設定を反映
        /// </summary>
        private void SaveSetting()
        {
            // 登録ポート一覧からスケジュールとFrontに設定を反映
            //Frontにはカンマ区切りに直して設定する
            clmSchedulePort.Items.Clear();
            Front.Gui.PortList.Clear();
            using (StringReader sr = new StringReader(optPortList.Text))
            {
                while (sr.Peek() > 0)
                {
                    string s = sr.ReadLine();
                    if (s.Length > 0)
                    {
                        try
                        {
                            int i = int.Parse(s);
                            Front.Gui.PortList.Add(i);
                            clmSchedulePort.Items.Add(s);
                        }
                        catch { }
                    }
                }
            }
            // スケジュール設定のポートリストに「ALL」を追加
            clmSchedulePort.Items.Add("ALL");

            #region 基本設定
            //自動再試行
            Front.Retry.InRetryInterval = (uint)optInSecond.Value;
            Front.Retry.InRetryTime = (uint)optInTime.Value;
            Front.Retry.OutRetryInterval = (uint)optOutSecond.Value;
            Front.Retry.OutRetryTime = (uint)optOutTime.Value;

            //帯域制限
            if (Front.BandStopTypeString[optBandType.SelectedIndex] == "ポート毎に個別設定")
            {
                // 個別設定
                if (Front.BandStopTypeString[Front.BndWth.BandStopMode] != "ポート毎に個別設定")
                {
                    //旧:個別設定以外→新:個別設定に変更になった場合
                    //設定された制限値をすべての起動中鏡に設定
                    foreach (Kagami _k in Front.KagamiList)
                    {
                        _k.Status.GUILimitUPSpeed = (int)optBandValue.Value;
                        _k.Status.LimitUPSpeed = Front.CnvLimit((int)optBandValue.Value, optBandUnit.SelectedIndex);
                    }
                }
            }
            else
            {
                // 個別設定以外
            }
            Front.BndWth.BandStopMode = (uint)optBandType.SelectedIndex;
            Front.BndWth.BandStopValue = (uint)optBandValue.Value;
            Front.BndWth.BandStopUnit = (uint)optBandUnit.SelectedIndex;
            Front.BndWth.BandStopResv = optBandReserve.Checked;

            //連打キック
            Front.Kick.KickCheckSecond = (uint)optKickSecond.Value;
            Front.Kick.KickCheckTime = (uint)optKickTime.Value;
            Front.Kick.KickDenyTime = (uint)optKickDenyTime.Value;

            //ログ出力
            Front.Log.KagamiLogFile = optKagamiLogFile.Text;
            Front.Log.LogDetail = optKagamiDetailLog.Checked;
            Front.Log.HpLogFile = optHpLogFile.Text;

            Front.Acl.DenyHost.Clear();
            foreach (string _str in optDenyHost.Lines)
            {
                if (!string.IsNullOrEmpty(_str))
                    Front.Acl.DenyHost.Add(_str);
            }
            Front.Acl.DenyUA.Clear();
            foreach (string _str in optDenyUA.Lines)
            {
                if (!string.IsNullOrEmpty(_str))
                    Front.Acl.DenyUA.Add(_str);
            }

            #endregion

            #region 鏡置き場
            //エントランス
            Front.Hp.UseHP = optUseHP.Checked;
            Front.Hp.IpHTTP = optHPAddr.Text;
            Front.Hp.PublicDir = optHPDir.Text;
            try
            {
                Front.Hp.PortHTTP = uint.Parse(optHPPort.Text);
            }
            catch
            {
                MessageBox.Show("HP公開ポート番号が異常です。\r\n65535以下の数値を設定してください。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //エントランス接続制限
            using (StringReader sr = new StringReader(optHPDenyList.Text))
            {
                try
                {
                    string str = "";
                    Front.Acl.HpDenyRemoteHost.Clear();
                    while (sr.Peek() > 0)
                    {
                        str = sr.ReadLine();
                        if (str.Length > 0 && Front.Acl.HpDenyRemoteHost.IndexOf(str) == -1)
                            if (Front.DenyLogin.Contains(str))
                                continue;
                            Front.Acl.HpDenyRemoteHost.Add(str);
                    }
                }
                catch { }
            }

            //インポート接続制限
            using (StringReader sr = new StringReader(optDenyList.Text))
            {
                try
                {
                    string str = "";
                    Front.Acl.DenyImportURL.Clear();
                    while (sr.Peek() > 0)
                    {
                        str = sr.ReadLine();
                        if (str.Length > 0 && Front.Acl.DenyImportURL.IndexOf(str) == -1)
                            Front.Acl.DenyImportURL.Add(str);
                    }
                }
                catch { }
            }
            if (optSameImportIPKick.Checked)
                Front.Acl.LimitSameImportURL = (uint)optSameImportIPNum.Value;
            else
                Front.Acl.LimitSameImportURL = 0;

            if (optImportKick.Checked)
                Front.Acl.ImportOutTime = (uint)optImportOutTime.Value;
            else
                Front.Acl.ImportOutTime = 0;

            if (optClientTimeKick.Checked)
            {
                Front.Acl.ClientOutCheck = true;
                Front.Acl.ClientOutNum = (uint)optClientNum.Value;
                Front.Acl.ClientOutTime = (uint)optClientOutTime.Value;
                Front.Acl.ClientNotIPCheck = optClientNotIPEnable.Checked;
            }
            else
            {
                Front.Acl.ClientOutCheck = false;
                Front.Acl.ClientOutNum = 0;
                Front.Acl.ClientOutTime = 10;
                Front.Acl.ClientNotIPCheck = false;
            }
            ///インポートURLと設定者IPの一致チェック
            Front.Acl.SetUserIpCheck = optSetUserIpCheck.Checked;
            ///待機中ポートがあれば自動解放しない
            Front.Acl.PortFullOnlyCheck = optPortFullOnly.Checked;
            Front.Hp.AuthEnable = optAuthEnable.Checked;
            Front.Hp.AuthPass = optAuthPass.Text;
            Front.Hp.AuthUser = optAuthUser.Text;
            #endregion
            #region 転送量制限
            /*
            Front.Acl.StopFlag = optStopCheck.Checked;
            Front.Acl.DLContainFlag = optDLCheck.Checked;
            Front.Acl.StopMBSelect = (uint)optStopSelect.SelectedIndex;

            Front.Acl.StopSelect = (uint)optStopSystem.SelectedIndex;
            if (optHourTraffic.CheckState == CheckState.Checked)
                Front.Acl.TrafficHour = 2;
            if (optHourTraffic.CheckState == CheckState.Indeterminate)
                Front.Acl.TrafficHour = 1;
            if (optHourTraffic.CheckState == CheckState.Unchecked)
            {
                Front.Acl.TrafficHour = 0;
                Front.Traffic = false;
            }
            try
            {
                if (Front.Acl.StopMBSelect == 0)
                {
                    Front.Acl.StopMB = (uint)optStopMB.Value;
                }
                else if (Front.Acl.StopMBSelect == 1)
                {
                    Front.Acl.StopMB = (uint)optStopMB.Value * 1024;
                }
                else if (Front.Acl.StopMBSelect == 2)
                {
                    Front.Acl.StopMB = (uint)optStopMB.Value * (1024 * 1024);
                }
                else
                {
                    MessageBox.Show("単位が不明です。(" + Front.Acl.StopMBSelect.ToString() + ")");
                    Front.Acl.StopMBSelect = 0;
                    Front.Acl.StopMB = 25600;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Front.Acl.StopMBSelect = 0;
                Front.Acl.StopMB = 25600;
            }*/
            #endregion
            #region 詳細設定
            //通信関連
            Front.Sock.SockConnTimeout = (uint)optConnTimeOut.Value;
            Front.Sock.SockRecvTimeout = (uint)optRecvTimeOut.Value;
            Front.Sock.SockSendTimeout = (uint)optSendTimeOut.Value;
            Front.Sock.SockSendQueueSize = (uint)optSendQueueSize.Value;
            Front.Sock.SockCloseDelay = (uint)optSockCloseDelay.Value;
            Front.Sock.upnp = optUPnPEnable.Checked;
            Front.Sock.VirtualHost = optVirtualHost.Checked;
            Front.Sock.ConnInfoSend = optBusyConSend.Checked;
            //その他詳細
            Front.Opt.BalloonTip = optBalloonTip.Checked;
            Front.Opt.BrowserView = optBrowser.Checked;
            Front.Opt.BrowserViewMode = optBrowserHtmlMode.Checked;
            Front.Opt.PriKagamiexe = optPriKagamiexe.Checked;
            Front.Opt.PriKagamin = optPriKagamin.Checked;
            Front.Acl.LimitSameClient = optSameClientKick.Checked ? (uint)optSameClientNum.Value : 0;
            Front.Opt.EnablePush = optEnablePush.Checked;
            Front.Opt.EnableInfo = optEnableInfo.Checked;
            Front.Opt.EnableAdmin = optEnableAdmin.Checked;
            Front.Opt.AdminPass = optAdminPass.Text;
            Front.Opt.SndConnOkFile = optSndConnOK.Text;
            Front.Opt.SndConnNgFile = optSndConnNG.Text;
            Front.Opt.SndDiscFile = optSndDisc.Text;
            Front.Opt.NotMyTrans = optTransPerEnable.Checked;
            Front.Opt.TransKagamin = opttranskagamin.Checked;
            Front.Opt.ImportRedirect = optEnableImportRedirect.Checked;
            Front.Opt.InTrayOn = optInTray.Checked;
            Front.Opt.WebKick = optWebKick.Checked;
            Front.Opt.AuthWebSet = optWebExport.Checked;
            if (!Front.Opt.EnablePush)
            {
                lock (Front.KagamiList)
                {
                    foreach (Kagami _k in Front.KagamiList)
                        _k.Status.PushOnly = false;
                }
            }
            #endregion

            #region スケジュール起動
            Front.ScheduleItem.Clear();
            for (int _num = 0; _num < scheduleDataView.Rows.Count - 1; _num++)
            {
                // 入力チェック
                if (//scheduleDataView[clmScheduleWeek.DisplayIndex, _num].Value == null ||
                    //scheduleDataView[clmScheduleHour.DisplayIndex, _num].Value == null ||
                    //scheduleDataView[clmScheduleMin.DisplayIndex, _num].Value == null ||
                    scheduleDataView[clmScheduleEvent.DisplayIndex, _num].Value == null)
                {
                    continue;
                }
                // ポート入力必須のイベントは、ポート番号の入力チェック
                string _port;
                switch (scheduleDataView[clmScheduleEvent.DisplayIndex, _num].Value.ToString())
                {
                    case "強制切断":
                    case "ポート待ち受け開始":
                    case "ポート待ち受け停止":
                    case "鏡終了後待受停止":
                    case "転送量制限値変更":
                        if (scheduleDataView[clmSchedulePort.DisplayIndex, _num].Value == null)
                            continue;
                        _port = (string)scheduleDataView[clmSchedulePort.DisplayIndex, _num].Value;
                        break;
                    default:
                        // とりあえずALLで登録しておく
                        _port = "ALL";
                        break;
                }

                // データ追加
                SCHEDULE _item = new SCHEDULE();
                if (scheduleDataView[clmScheduleEnable.DisplayIndex, _num].Value != null)
                    _item.Enable = (bool)scheduleDataView[clmScheduleEnable.DisplayIndex, _num].Value;
                else
                    _item.Enable = false;
                _item.Event = (uint)clmScheduleEvent.Items.IndexOf(scheduleDataView[clmScheduleEvent.DisplayIndex, _num].Value);
                if (_port == "ALL")
                    _item.Port = 0;
                else
                    _item.Port = uint.Parse(_port);
                string[] _str = scheduleDataView[clmScheduleData.DisplayIndex, _num].Value.ToString().Split(',');
                if (_str.Length == 9)
                {
                    _item.StartType = uint.Parse(_str[0]);
                    _item.Week = uint.Parse(_str[1]);
                    _item.Hour = uint.Parse(_str[2]);
                    _item.Min = uint.Parse(_str[3]);
                    _item.TrfType = uint.Parse(_str[4]);
                    _item.TrfValue = uint.Parse(_str[5]);
                    _item.TrfUnit = uint.Parse(_str[6]);
                    _item.ExecTrf = false;
                    _item.Conn = uint.Parse(_str[7]);
                    _item.Resv = uint.Parse(_str[8]);
                }
                else
                {
                    throw new Exception("illigal schedule data");
                }
                Front.ScheduleItem.Add(_item);
            }
            #endregion
            #region その他
            if (optPrivatePassword.Text != "")
            {
                Front.Acl.ClosedPassward = (ClosedEnable.Checked == false) ? "" : optPrivatePassword.Text;
            }
            else if (optPrivatePassword.Text == "" && ClosedEnable.Checked)
            {
                MessageBox.Show("非公開用接続設定パスワードが空白です。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Front.Acl.ClosedPassward = "";
            }

            Front.Acl.ClosedComment = optPrivateComment.Text;

            Front.Hp.HPstartON = optStartHP.Checked;
            Front.Hp.PortstartON = optStartPort.Checked;

            Front.Acl.FailureCount = (optLoinOut.Checked == false) ? 0 : (uint)optLoginCount.Value;

            Front.Hp.StartPortList.Clear();
            /*
            Front.Opt.PassMySQL = optpasssql.Text;
            Front.Opt.HostMySQL = optsqlhost.Text;
            Front.Opt.UserMuSQL = optusersql.Text;
            Front.Opt.MySQLEnable = mySQLOn.Checked;
            Front.Opt.ReadRowMySQL = optReadCulum.Text;
            Front.Opt.RearDB = optDateBaseName.Text;
            Front.Opt.ReadHyou = optHyou.Text;
            Front.Opt.ReadRowIPMySQL = optReadIP.Text;
            */
            
            foreach (string _port in optStartPortList.CheckedItems)
            {
                
                Front.Hp.StartPortList.Add(int.Parse(_port));
            }

            Front.Opt.ViewWMP = optWMPEnable.Checked;
            Front.Opt.AuthID = AuthIDText.Text;
            Front.Opt.AuthPass = AuthPassText.Text;
            Front.Opt.AuthWebSet = optWebExport.Checked;
            #endregion

        }
        #endregion
        /// <summary>
        /// OKボタンをクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okBTN_Click(object sender, EventArgs e)
        {
            // 設定の適用
            optApplyBTN_Click(sender, e);
        }
        /// <summary>
        /// 適用ボタンをクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optApplyBTN_Click(object sender, EventArgs e)
        {
            // GUI→Frontに設定反映
            SaveSetting();
            // Front→iniファイルに保存
            Front.SaveSetting();
        }
        #region 基本設定
        /// <summary>
        /// 鏡ログの参照ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optRefKagamiBTN_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (optKagamiLogFile.Text != "")
                sfd.FileName = optKagamiLogFile.Text;
            else
                sfd.FileName = "log.txt";
            sfd.InitialDirectory = @".\";
            sfd.Filter = "LOGファイル(*.txt)|*.txt|すべてのファイル(*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.Title = "保存先のログファイル名を入力してください";
            sfd.RestoreDirectory = true;
            sfd.OverwritePrompt = true;
            sfd.CheckPathExists = false;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                optKagamiLogFile.Text = sfd.FileName;
            }
        }
        #endregion

        #region 鏡置き場設定
        /// <summary>
        /// HP公開機能のON/OFF制御
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optUseHP_CheckedChanged(object sender, EventArgs e)
        {
            if (optUseHP.Checked)
            {
                groupKgm1.Enabled = true;
                groupKgm2.Enabled = true;
            }
            else
            {
                groupKgm1.Enabled = false;
                groupKgm2.Enabled = false;
            }
        }        /// <summary>
        /// HP公開用Dir参照ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optRefHpDirBTN_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "HP公開用フォルダを指定してください";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                optHPDir.Text = fbd.SelectedPath;
            }
        }
        /// <summary>
        /// HPログの参照ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optRefHpLogBTN_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (optHpLogFile.Text != "")
                sfd.FileName = optHpLogFile.Text;
            else
                sfd.FileName = "log.txt";
            sfd.InitialDirectory = @".\";
            sfd.Filter = "LOGファイル(*.txt)|*.txt|すべてのファイル(*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.Title = "保存先のログファイル名を入力してください";
            sfd.RestoreDirectory = true;
            sfd.OverwritePrompt = true;
            sfd.CheckPathExists = false;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                optHpLogFile.Text = sfd.FileName;
            }
        }
        /// <summary>
        /// 同一インポートURLキック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optSameImportIPKick_CheckedChanged(object sender, EventArgs e)
        {
            if (optSameImportIPKick.Checked)
                optSameImportIPNum.Enabled = true;
            else
                optSameImportIPNum.Enabled = false;
        }
        /// <summary>
        /// 長時間インポート接続自動切断
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optImportKick_CheckedChanged(object sender, EventArgs e)
        {
            if (optImportKick.Checked)
                optImportOutTime.Enabled = true;
            else
                optImportOutTime.Enabled = false;
        }
        /// <summary>
        /// 視聴者数自動切断
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optClientTimeKick_CheckedChanged(object sender, EventArgs e)
        {
            if (optClientTimeKick.Checked)
            {
                optClientNum.Enabled = true;
                optClientOutTime.Enabled = true;
                optClientNotIPEnable.Enabled = true;
            }
            else
            {
                optClientNum.Enabled = false;
                optClientOutTime.Enabled = false;
                optClientNotIPEnable.Enabled = false;
            }
        }
        #endregion

        #region 詳細設定
        /// <summary>
        /// 同一クライアント切断のチェック状態変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optSameClientKick_CheckedChanged(object sender, EventArgs e)
        {
            if (optSameClientKick.Checked)
            {
                optSameClientLabel.Enabled = true;
                optSameClientNum.Enabled = true;
            }
            else
            {
                optSameClientLabel.Enabled = false;
                optSameClientNum.Enabled = false;
            }
        }

        /// <summary>
        /// Adminモードのチェック状態変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optEnableAdmin_CheckedChanged(object sender, EventArgs e)
        {
            if (optEnableAdmin.Checked)
            {
                optAdminLabel.Enabled = true;
                optAdminPass.Enabled = true;
            }
            else
            {
                optAdminLabel.Enabled = false;
                optAdminPass.Enabled = false;
            }
        }

        /// <summary>
        /// インポート接続OK時の音 参照ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optRefSndConnOkBTN_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (optSndConnOK.Text != "")
                ofd.FileName = optSndConnOK.Text;
            ofd.Title = "インポート接続完了時に再生するwaveファイルを選択してください";
            ofd.InitialDirectory = @".\";
            ofd.Filter = "WAVEファイル(*.wav)|*.wav|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.CheckFileExists = true;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
                optSndConnOK.Text = ofd.FileName;
        }

        /// <summary>
        /// インポート接続タイムアウト時の音 参照ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optRefSndConnNgBTN_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (optSndConnNG.Text != "")
                ofd.FileName = optSndConnNG.Text;
            ofd.Title = "インポート接続NG時に再生するwaveファイルを選択してください";
            ofd.InitialDirectory = @".\";
            ofd.Filter = "WAVEファイル(*.wav)|*.wav|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.CheckFileExists = true;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
                optSndConnNG.Text = ofd.FileName;
        }

        /// <summary>
        /// インポート切断時の音 参照ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optRefSndDiscBTN_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (optSndDisc.Text != "")
                ofd.FileName = optSndDisc.Text;
            ofd.Title = "インポート切断時に再生するwaveファイルを選択してください";
            ofd.InitialDirectory = @".\";
            ofd.Filter = "WAVEファイル(*.wav)|*.wav|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            ofd.CheckFileExists = true;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
                optSndDisc.Text = ofd.FileName;
        }
        #endregion

        #region スケジュール登録タブ
        /// <summary>
        /// セルのフォーカスが移った時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduleDataView_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            // フォーカス先がComboBoxならF4キー送信して即選択メニューを開く
            DataGridView dgv = (DataGridView)sender;
            if (dgv.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn)
            {
                //SendKeysはメインウインドウ側に送るので、
                //オプション画面をモーダルダイアログ化すると利かない。。
                //SendKeys.Send("{F4}");
                //とりあえず編集もーど。
                try
                {
                    dgv.BeginEdit(false);
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// 新しい行を追加した時のデフォルト値設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduleDataView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            /*
            e.Row.Cells[clmScheduleEnable.DisplayIndex].Value = false;
            e.Row.Cells[clmScheduleHour.DisplayIndex].Value = clmScheduleHour.Items[0];
            e.Row.Cells[clmScheduleMin.DisplayIndex].Value = clmScheduleMin.Items[0];
            e.Row.Cells[clmScheduleEvent.DisplayIndex].Value = clmScheduleEvent.Items[0];
            e.Row.Cells[clmSchedulePort.DisplayIndex].Value = clmSchedulePort.Items[0];
            */
            e.Row.Cells[clmScheduleData.DisplayIndex].Value = "0,0,0,0,0,1,0," + Front.Gui.Conn + "," + Front.Gui.Reserve;
        }
        /// <summary>
        /// セルの入力完了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduleDataView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == clmScheduleEvent.DisplayIndex)
            {
                if (scheduleDataView[e.ColumnIndex, e.RowIndex].Value != null)
                {
                    switch (scheduleDataView[e.ColumnIndex, e.RowIndex].Value.ToString())
                    {
                        case "強制切断":
                        case "ポート待受開始":
                        case "ポート待受停止":
                        case "接続枠数変更":
                        case "鏡終了後待受停止":
                            scheduleDataView[clmSchedulePort.DisplayIndex, e.RowIndex].ReadOnly = false;
                            break;
                        default:
                            scheduleDataView[clmSchedulePort.DisplayIndex, e.RowIndex].Value = "ALL";
                            scheduleDataView[clmSchedulePort.DisplayIndex, e.RowIndex].ReadOnly = true;
                            break;
                    }
                }
                else
                {
                    scheduleDataView[clmSchedulePort.DisplayIndex, e.RowIndex].ReadOnly = false;
                }
            }
            // clmScheduleOptionの文字列再設定
            resetClmScheduleOption(e.RowIndex);
        }
           /// <summary>
        /// 指定行数のclmScheduleOptionの文字列再設定
        /// </summary>
        /// <param name="row"></param>
        private void resetClmScheduleOption(int _row)
        {
            string[] _str = scheduleDataView[clmScheduleData.DisplayIndex, _row].Value.ToString().Split(',');
            if (_str.Length == 9)
            {
                if (scheduleDataView[clmScheduleEvent.DisplayIndex, _row].Value != null)
                {
                    string _evt = scheduleDataView[clmScheduleEvent.DisplayIndex, _row].Value.ToString();
                    string _set = "";
                    switch (_str[0])
                    {
                        case "0":
                            _set = Front.ScheduleWeekString[int.Parse(_str[1])] + " " + (_str[2].Length == 1 ? "0" : "") + _str[2] + ":" + (_str[3].Length == 1 ? "0" : "") + _str[3];
                            // イベント種別によって接続枠設定の有効/無効を切り替える
                            switch (_evt)
                            {
                                case "ポート待受開始":
                                case "接続枠数変更":
                                    //接続枠設定有効
                                    _set += " <" + _str[7] + "+" + _str[8] + ">";
                                    break;
                                default:
                                    //その他は接続枠設定無効
                                    break;
                            }
                            break;
                        case "1":
                            _set = _str[5] + Front.ScheduleTrfUnitString[int.Parse(_str[6])] + (_str[4] == "0" ? "/日以上" : "/月以上");
                            // イベント種別によって接続枠設定の有効/無効を切り替える
                            switch (_evt)
                            {
                                case "ポート待受開始":
                                case "接続枠数変更":
                                    //接続枠設定有効
                                    _set += " <" + _str[7] + "+" + _str[8] + ">";
                                    break;
                                default:
                                    //その他は接続枠設定無効
                                    break;
                            }
                            break;
                        default:
                            _set = "error";
                            break;
                    }
                    scheduleDataView[clmScheduleOption.DisplayIndex, _row].Value = _set;
                    return;
                }
            }
            scheduleDataView[clmScheduleOption.DisplayIndex, _row].Value = "";
            return;
        }


    
        private void dataGridViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
            try
            {
                DataGridViewCell curCell = scheduleDataView.CurrentCell;
                switch (scheduleDataView[curCell.ColumnIndex, curCell.RowIndex].Value.ToString())
                {
                    case "強制切断":
                    case "ポート待ち受け開始":
                    case "ポート待ち受け停止":
                    case "鏡終了後待受停止":
                        scheduleDataView[clmSchedulePort.DisplayIndex, curCell.RowIndex].ReadOnly = false;
                        break;
                    case "転送量制限値変更":
                        scheduleDataView[clmSchedulePort.DisplayIndex, curCell.RowIndex].ReadOnly = false;
                        clmSchedulePort.Items.Clear();
                        break;
                    default:
                        scheduleDataView[clmSchedulePort.DisplayIndex, curCell.RowIndex].Value = "ALL";
                        scheduleDataView[clmSchedulePort.DisplayIndex, curCell.RowIndex].ReadOnly = true;
                        break;
                }
            }
            catch
            {

            }
             */
        }

                
            
        
        #endregion
        private void scheduleDataView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            /*
            DataGridView dgv = (DataGridView)sender;
            int i;
            //該当する列か調べる
            if (dgv.Columns[e.ColumnIndex].Name == "clmSchedulePort" &&
                dgv.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn)
            {
                DataGridViewComboBoxColumn cbc =
                    (DataGridViewComboBoxColumn)dgv.Columns[e.ColumnIndex];
                //コンボボックスの項目に追加する
                try
                {
                    if (!cbc.Items.Contains(e.FormattedValue) && int.TryParse(e.FormattedValue.ToString(), out i))
                    {
                        cbc.Items.Add(e.FormattedValue);
                    }
                }
                catch
                {
                }


            }
             */
        }
        private void scheduleDataView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            /*
            if (e.Control is DataGridViewComboBoxEditingControl)
            {
                DataGridView dgv;
                try
                {
                    //該当する列か調べる
                    dgv = (DataGridView)sender;
                    if (dgv.CurrentCell.OwningColumn.Name == "clmSchedulePort")
                    {
                        //編集のために表示されているコントロールを取得
                        DataGridViewComboBoxEditingControl cb =
                            (DataGridViewComboBoxEditingControl)e.Control;
                        cb.DropDownStyle = ComboBoxStyle.DropDown;
                    }


                    //該当する列か調べる
                    if (dgv.CurrentCell.OwningColumn.Name == "clmScheduleEvent")
                    {
                        //編集のために表示されているコントロールを取得
                        this.dataGridViewComboBox =
                            (DataGridViewComboBoxEditingControl)e.Control;
                        //SelectedIndexChangedイベントハンドラを追加
                        this.dataGridViewComboBox.SelectedIndexChanged +=
                            new EventHandler(dataGridViewComboBox_SelectedIndexChanged);
                    }

                }
                catch
                {
                }
            }*/

        }

        private void optClientNotIPEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (optBrowser.Checked)
            {
                optBrowserHtmlMode.Enabled = true;
                optBrowserTextMode.Enabled = true;
            }
            else
            {
                optBrowserHtmlMode.Enabled = false;
                optBrowserTextMode.Enabled = false;
            }

        }
        /*
        private void optStopCheck_CheckedChanged(object sender, EventArgs e)
        {
            groupBox3.Enabled = optStopCheck.Checked;
            optDLCheck.Enabled = optStopCheck.Checked;
            if (!optDLCheck.Enabled)
                optDLCheck.Checked = false;
            optHourTraffic.Enabled = optStopCheck.Checked;
            if (!optHourTraffic.Enabled)
                optHourTraffic.Checked = false;
        }
        */
        private void StopClearBTN_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("転送量をリセットしますか？", "確認",
                 MessageBoxButtons.YesNo,
                 MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Front.DayUPSet = Front.TotalUP / (1024 * 1024);
                if (Front.Acl.DLContainFlag)
                    Front.DayUPSet += (Front.TotalDL / (1024 * 1024));
                Front.Attainment = false;
                for (int i = 0; i < 1440; i++)
                {
                    Front.Acl.TrafficTFHour[i] = 0;

                }
            }
        }

        private void UPnPTestBTN_Click(object sender, EventArgs e)
        {
            bool _suc;
            int _port;

            try
            {
                if (Front.Gui.PortList.Count != 0 && !Front.IndexOf(Front.Gui.PortList[0]).Status.ImportStatus)
                    _port = Front.Gui.PortList[0];
                else
                    _port = 65500;
            }
            catch
            {
                _port = Front.Gui.PortList[0];
            }
#if DEBUG
            _suc = JinkSoft.Utility.UPnP.UPnPClient.OpenFirewallPort("192.168.1.14", "192.168.1.4", _port);
#endif
#if !DEBUG
            _suc = JinkSoft.Utility.UPnP.UPnPClient.OpenFirewallPort(65500);
#endif
            if (_suc)
                MessageBox.Show("UPnPによるポート開放に成功しました(Port:" + _port.ToString() + ")", "開放成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("UPnPによるポート開放に失敗しました(Port:" + _port.ToString() + ")", "開放失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
#if DEBUG
            _suc = JinkSoft.Utility.UPnP.UPnPClient.CloseFirewallPort("192.168.1.14", "192.168.1.4", 65500);
#endif
#if !DEBUG
            _suc = JinkSoft.Utility.UPnP.UPnPClient.CloseFirewallPort(65500);
#endif
            if (_suc)
                MessageBox.Show("UPnPによるポート閉鎖に成功しました(Port:" + _port.ToString() + ")", "閉鎖成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("UPnPによるポート閉鎖に失敗しました(Port:" + _port.ToString() + ")", "閉鎖失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void scheduleDataView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //詳細設定セルの編集開始で詳細設定ウィンドウを開く
            if (e.ColumnIndex == clmScheduleOption.DisplayIndex)
            {
                // 詳細設定セルの編集自体はキャンセルしておく
                e.Cancel = true;
                // イベントが設定してあるかチェック
                if (scheduleDataView[clmScheduleEvent.DisplayIndex, e.RowIndex].Value != null)
                {
                    if (scheduleDataView[clmSchedulePort.DisplayIndex, e.RowIndex].Value == null)
                        scheduleDataView[clmSchedulePort.DisplayIndex, e.RowIndex].Value = "";
                    if (scheduleDataView[clmScheduleData.DisplayIndex, e.RowIndex].Value == null)
                        scheduleDataView[clmScheduleData.DisplayIndex, e.RowIndex].Value = "";
                    Option2 optDlg = new Option2(
                        scheduleDataView[clmScheduleEvent.DisplayIndex, e.RowIndex].Value.ToString(),
                        scheduleDataView[clmSchedulePort.DisplayIndex, e.RowIndex].Value.ToString(),
                        scheduleDataView[clmScheduleData.DisplayIndex, e.RowIndex].Value.ToString()
                    );
                    // 設定画面表示
                    optDlg.ShowDialog();
                    // 設定の反映
                    scheduleDataView[clmScheduleData.DisplayIndex, e.RowIndex].Value = optDlg.GetResult();
                    resetClmScheduleOption(e.RowIndex);
                }
            }
        }

        private void ClosedEnable_CheckedChanged(object sender, EventArgs e)
        {
            optPrivateComment.Enabled = ClosedEnable.Checked;
            optPrivatePassword.Enabled = ClosedEnable.Checked;
        }

        private void optStartHP_CheckedChanged(object sender, EventArgs e)
        {
            optStartPort.Enabled = optStartHP.Checked;
            if (!optStartHP.Checked)
                optStartPort.Checked = false;
            optStartPortList.Enabled = optStartPort.Checked;
            optStartPortList.Items.Clear();
            foreach (string _temp in optPortList.Text.Split('\n'))
            {
                if (_temp == "")
                    continue;
                optStartPortList.Items.Add(_temp);
            }
        }

        private void optLoinOut_CheckedChanged(object sender, EventArgs e)
        {
            optLoginCount.Enabled = optLoinOut.Checked;
        }

        private void optAuthEnable_CheckedChanged(object sender, EventArgs e)
        {
            optAuthPass.Enabled = optAuthEnable.Checked;
            optAuthUser.Enabled = optAuthEnable.Checked;
        }








    }
}
