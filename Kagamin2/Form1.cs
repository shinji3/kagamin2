//#define PLUS
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Threading;
using System.Xml.Serialization;
using System.Diagnostics;

namespace Kagamin2
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// �C�x���g�n���h���̃X���b�h�ԒʐM�pdelegate
        /// </summary>
        /// <param name="_ke"></param>
        delegate void EventHandlerDelegate(KagamiEvent _ke);

        #region �����o�ϐ�
        /// <summary>
        /// ���p�l���̍X�V���K�v�Ȏ�true
        /// </summary>
        bool LeftFlag = true;
        /// <summary>
        /// �O��̎g�p���|�[�g���B
        /// BalloonTip�̃|�[�g��ԕύX���o�p�B
        /// </summary>
        int LastActPortNum = 0;
        /// <summary>
        /// �����I���L���t���O
        /// </summary>
        bool EnableAutoExit = false;
        /// <summary>
        /// �����V���b�g�_�E���L���t���O
        /// </summary>
        bool EnableAutoShutdown = false;
        /// <summary>
        /// �����I�����ɏI���m�F��skip���邽�߂̃t���O
        /// </summary>
        bool AskFormClose = true;
        /// <summary>
        /// �V���b�g�_�E���������t���O
        /// </summary>
        bool ExecShutdown = false;
        /// <summary>
        /// �ш搧���X���b�h
        /// </summary>
        Thread BandTh = null;
        /// <summary>
        /// �����ш�Čv�Z�t���O
        /// </summary>
        bool BandFlag = false;
        /// <summary>
        /// �I�𒆃|�[�g�̋��Q�Ƃ�ԋp
        /// </summary>
        Kagami SelectedKagami
        {
            get
            {
                try
                {
                    if (myPort.Text != "")
                        return Front.IndexOf(int.Parse(myPort.Text));
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// ���ݎ���,�X�P�W���[���N���ŗ��p
        /// �u��:���v�`��
        /// </summary>
        public string Time = "";
        /// <summary>
        /// monView�Ŏg�p
        /// kbps��KB/s�ؑ�
        /// 0:kbps 1:KB/s
        /// </summary>
        public uint Unit = 0;

        /// <summary>
        /// TaskbarRestartMsgID
        /// </summary>
        private Int32 _uTaskbarRestartMsg = 0;
        #endregion

#if OVERLOAD
        private int overload_cnt =1;
#endif

        #region �R���X�g���N�^
        public Form1(string[] argv)
        {
            try
            {
                InitializeComponent();
            }
            catch
            {
                MessageBox.Show("�R���|�[�l���g�̏������Ɏ��s���܂���\r\n�����m�F���Ă�������.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            // �C�x���g�ʒm�n���h���̓o�^
            Event.UpdateKagami += new EventHandler(Event_UpdateGUI);
            Event.UpdateClient += new EventHandler(Event_UpdateClient);
            Event.UpdateReserve += new EventHandler(Event_UpdateReserve);
            Event.UpdateKick += new EventHandler(Event_UpdateKick);
            Event.UpdateLog += new EventHandler(Event_UpdateLog);

            Front.AppName = "Kagamin2/1.3.8";
            this.Text = Front.AppName;

            // StatusBar
            toolStripCPU.Spring = true;

            // clmClientView��Index�ޔ�
            Front.clmCV_ID_IDX = clmClientViewID.DisplayIndex;
            Front.clmCV_IH_IDX = clmClientViewIpHost.DisplayIndex;
            Front.clmCV_UA_IDX = clmClientViewUA.DisplayIndex;
            Front.clmCV_TM_IDX = clmClientViewTime.DisplayIndex;
            Front.clmCV_IP_IDX = clientView.Columns.Count + 0; // internal-0
            Front.clmCV_HO_IDX = clientView.Columns.Count + 1; // internal-1

            // monView�ɍ��ڐݒ�
            monViewInit();

            // monAllView�ɍ��ڐݒ�
            monAllViewInit();
            
            // StatusBar EX,IM,CPU�\��
            statusBarUpdate();

            // �ݒ�Ǎ��O�ɃE�C���h�E�T�C�Y�����ݒ�
            Front.Form.W = this.Width;
            Front.Form.H = this.Height;

            // �t�@�C����Front�ɕۑ��l��ǂݍ���
            Front.LoadSetting();

            if(Front.Opt.AppName != "")
                Front.AppName += Front.Opt.AppName;
            Front.UserAgent = "NSPlayer/11.0.5721.5145 " + Front.AppName;

            // Front��GUI�ɔ��f
            LoadSetting();

            // �A�C�R���ݒ�
            // res�t�@�C������w��ł��Ȃ��̂Ŏ��s�t�@�C������E���Ƃ����r�ƁB�B
            IntPtr[] sIcon = new IntPtr[1];
            IntPtr[] lIcon = new IntPtr[1];
            NativeMethods.ExtractIconEx(@"Kagamin2.exe", (int)Front.Form.IconIndex, lIcon, sIcon, 1);
            if (lIcon[0] != IntPtr.Zero)
                NativeMethods.DestroyIcon(lIcon[0]);
            if (sIcon[0] != IntPtr.Zero)
            {
                this.Icon = (Icon)Icon.FromHandle(sIcon[0]).Clone();
                NativeMethods.DestroyIcon(sIcon[0]);
            }

            // �^�X�N�g���C�A�C�R���̐ݒ�
            TaskTrayIcon.Icon = this.Icon;
            TaskTrayIcon.BalloonTipTitle = Front.AppName;
            TaskTrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TaskTrayIcon.BalloonTipText = "";
            TaskTrayIcon.Text = Front.AppName;
            TaskTrayIcon.Visible = false;

            // �R�}���h���C�������̏���
            if (argv.Length >= 1)
            {
                string url = argv[0];
                if (url.StartsWith("mms://"))
                {
                    url = url.Replace("mms://", "http://");
                }
                else if (url.StartsWith("ttp://"))
                {
                    url = "h" + url;
                }
                else if (!url.StartsWith("http://"))
                {
                    url = "http://" + url;
                }
                importURL.Text = url;
                //���̃^�C�~���O�͖���������ǂˁB�B
                connBTN_Click((object)null, EventArgs.Empty);
            }
        }

        #endregion

        #region �C�x���g�n���h������
        /// <summary>
        /// ����ԕω��ʒm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateGUI(object sender, EventArgs e)
        {
            // LeftPanel�X�V�������ш�Čv�Z���s��
            LeftFlag = true;
            BandFlag = true;
        }
        /// <summary>
        /// �N���C�A���g�ڑ��ؒf�ʒm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateClient(object sender, EventArgs e)
        {
            // LeftPanel�X�V���s��
            LeftFlag = true;
            if (e != EventArgs.Empty)
            {
                // clientView,reserveView�̍X�V���s��
                KagamiEvent _ke = (KagamiEvent)e;
                clientViewUpdate(_ke);
            }
        }
        /// <summary>
        /// ���U�[�u�o�^�폜�ʒm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateReserve(object sender, EventArgs e)
        {
            if (e != EventArgs.Empty)
            {
                // reserveView�̍X�V���s��
                KagamiEvent _ke = (KagamiEvent)e;
                reserveViewUpdate(_ke);
            }
        }
        
        /// <summary>
        /// �N���C�A���g�L�b�N�ʒm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateKick(object sender, EventArgs e)
        {
            if (e != EventArgs.Empty)
            {
                // kickView�̍X�V���s��
                KagamiEvent _ke = (KagamiEvent)e;
                kickViewUpdate(_ke);
            }
        }
        /// <summary>
        /// ���O�o�͒ʒm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateLog(object sender, EventArgs e)
        {
            if (e != EventArgs.Empty)
            {
                // logView�̍X�V���s��
                KagamiEvent _ke = (KagamiEvent)e;
                logViewUpdate(_ke);
            }
        }
        #endregion

        #region �ݒ�̓ǂݏ���
        /// <summary>
        /// Front��GUI�ɐݒ蔽�f
        /// �N�����ƁA�I�v�V�����ݒ芮�����ɌĂ΂��B
        /// </summary>
        private void LoadSetting()
        {
            // �|�[�g�ꗗ�ɐݒ蔽�f
            myPort.Items.Clear();
            foreach (int i in Front.Gui.PortList)
                myPort.Items.Add(i.ToString());

            // ���݂�GUI�̃|�[�g�����N���Ȃ甽�f�������X�g�̐擪�l�ɐݒ�
            if (this.SelectedKagami == null)
                if (myPort.Items.Count > 0)
                    myPort.Text = myPort.Items[0].ToString();

            // ���C�ɓ���C���|�[�gURL���E�N���b�N���j���[�ɔ��f
            while (ImportUrlRClick.Items.Count > 7)
                ImportUrlRClick.Items.RemoveAt(7);
            foreach (string s in Front.Gui.FavoriteList)
            {
                if (s.Length > 0)
                {
                    ToolStripMenuItem _tsmi = new ToolStripMenuItem();
                    _tsmi.Text = s;
                    _tsmi.Click += PasteFavoriteURL;
                    ImportUrlRClick.Items.Add(_tsmi);
                }
            }

            // �ш搧���ݒ�
            if (bndStopLabel.Text == "�|�[�g���Ɍʐݒ�" && Front.BandStopTypeString[Front.BndWth.BandStopMode] == "�|�[�g���Ɍʐݒ�")
            {
                bndStopUnit.Text = Front.BandStopUnitString[Front.BndWth.BandStopUnit];
                bndStopNumAudit();
            }
            else
            {
                bndStopLabel.Text = Front.BandStopTypeString[Front.BndWth.BandStopMode];
                //bndStopNum.Enabled = (Front.BandStopTypeString[Front.BndWth.BandStopMode] == "�|�[�g���Ɍʐݒ�") ? true : false;
                bndStopNum.Value = Front.BndWth.BandStopValue;
                bndStopUnit.Text = Front.BandStopUnitString[Front.BndWth.BandStopUnit];
            }

            // �ݒ�ɂ��킹��StatusBar��Audit
            statusBarAudit();
        }
        /// <summary>
        /// GUI��Front�ɐݒ蔽�f
        /// �c���A���C���t�H�[������Front�ɐݒ蔽�f����̂�Form1_FormClosing�Ŏ��{�B
        /// </summary>
        private void SaveSetting()
        {
            // �Ƃ��ɂȂ��B
        }
        #endregion

        #region �t�H�[���S�̂̐���
        /// <summary>
        /// �t�H�[���Ǎ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // ������΍� - ���C���t�H�[���̃_�u���o�b�t�@�����O
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // ������΍� - ListView�̃_�u���o�b�t�@�����O
            LV_SetDoubleBuffer(kagamiView.Handle);
            LV_SetDoubleBuffer(monView.Handle);
            LV_SetDoubleBuffer(monAllView.Handle);
            LV_SetDoubleBuffer(clientView.Handle);
            LV_SetDoubleBuffer(reserveView.Handle);
            LV_SetDoubleBuffer(kickView.Handle);
            LV_SetDoubleBuffer(logView.Handle);

            // �E�C���h�E�\���ʒu�E��ԃ��J�o���[
            this.Location = new Point(Front.Form.X, Front.Form.Y);
            this.Size = new Size(Front.Form.W, Front.Form.H);

            // �X�v���b�^�[�̈ʒu�����J�o���[
            if (Front.Form.SplitDistance1 >= 0) splitContainer1.SplitterDistance = (int)Front.Form.SplitDistance1;
            //if (Front.Form.SplitDistance2 >= 0) splitContainer2.SplitterDistance = (int)Front.Form.SplitDistance2;

            // ���p�l����On/Off�����J�o���[
            splitContainer1.Panel1Collapsed = Front.Form.LeftPanelCollapsed;
            // �ш搧����On/Off���J�o���[
            if (Front.BndWth.EnableBandWidth)
                StartBandWidth();

            // ���ԁE���ԓ]���ʂ̃��J�o���[
            // LastUpdate�Ɣ�r���ĕω����Ă���N���A����
            if (Front.Log.LastUpdate != DateTime.Now.ToString("YYYYMMDD"))
            {
                // �����Ⴄ
                Front.Log.TrsUpDay = 0;
                Front.Log.TrsDlDay = 0;
                if (!Front.Log.LastUpdate.StartsWith(DateTime.Now.ToString("YYYYMM")))
                {
                    // �����Ⴄ
                    Front.Log.TrsUpMon = 0;
                    Front.Log.TrsDlMon = 0;
                }
            }

            // �X�e�[�^�X�o�[���AUDIT
            statusBarAudit();

            // �eListView�̃J�����������J�o���[
            #region �eListView�̃J�����������J�o���[
            string[] _clm;
            _clm = Front.Form.KagamiListColumn.Split(',');
            if (_clm.Length == 3)
            {
                try
                {
                    clmKgmViewPort.Width = int.Parse(_clm[0]);
                    clmKgmViewImport.Width = int.Parse(_clm[1]);
                    clmKgmViewConn.Width = int.Parse(_clm[2]);
                }
                catch { }
            }
            _clm = Front.Form.MonAllViewColumn.Split(',');
            if (_clm.Length == 2)
            {
                try
                {
                    clmMonAllView1.Width = int.Parse(_clm[0]);
                    clmMonAllView2.Width = int.Parse(_clm[1]);
                }
                catch { }
            }
            _clm = Front.Form.MonViewColumn.Split(',');
            if (_clm.Length == 2)
            {
                try
                {
                    clmMonView1.Width = int.Parse(_clm[0]);
                    clmMonView2.Width = int.Parse(_clm[1]);
                }
                catch { }
            }
            _clm = Front.Form.ClientViewColumn.Split(',');
            if (_clm.Length == 4)
            {
                try
                {
                    clmClientViewID.Width = int.Parse(_clm[0]);
                    clmClientViewIpHost.Width = int.Parse(_clm[1]);
                    clmClientViewUA.Width = int.Parse(_clm[2]);
                    clmClientViewTime.Width = int.Parse(_clm[3]);
                }
                catch { }
            }
            _clm = Front.Form.ResvViewColumn.Split(',');
            if (_clm.Length == 2)
            {
                try
                {
                    clmResvViewIP.Width = int.Parse(_clm[0]);
                    clmResvViewStatus.Width = int.Parse(_clm[1]);
                }
                catch { }
            }
            _clm = Front.Form.KickViewColumn.Split(',');
            if (_clm.Length == 3)
            {
                try
                {
                    clmKickViewIP.Width = int.Parse(_clm[0]);
                    clmKickViewStatus.Width = int.Parse(_clm[1]);
                    clmKickViewCount.Width = int.Parse(_clm[2]);
                }
                catch { }
            }
            _clm = Front.Form.LogViewColumn.Split(',');
            if (_clm.Length == 2)
            {
                try
                {
                    clmLogView1.Width = int.Parse(_clm[0]);
                    clmLogView2.Width = int.Parse(_clm[1]);
                }
                catch { }
            }
            _clm = null;
            #endregion

            // ClientViewRClick�̃h���C�������L���؂�ւ�
            ClientResolveHostMenu.Checked = Front.Opt.EnableResolveHost;

            // monView�̑ш摬�x�\���P��
            Unit = Front.Form.monViewUnit;
            monViewUpdate(null);
            
            // ImportURL/�ő�ʏ�ڑ���/�ő僊�U�[�u�ڑ������I�����_�ł�GUI��̒l�Ń��J�o���[
            importURL.Text = Front.Gui.ImportURL;
            connNum.Value = Front.Gui.Conn;
            resvNum.Value = Front.Gui.Reserve;

            // �X�P�W���[���N���p���Ԑݒ�
            DateTime _dtNow = DateTime.Now;
            Time = _dtNow.ToString("HH:mm");
            // �����X�V�p�^�C�}�[�J�n
            timer1.Enabled = true;
#if !PLUS
            MonModeChgMenu.Visible = false;
#endif
        }
        /// <summary>
        /// �w��ListView�Ƀ_�u���o�b�t�@�����O�w��
        /// </summary>
        /// <param name="_handle"></param>
        private void LV_SetDoubleBuffer(IntPtr _handle)
        {
            /*
            private const int LVM_FIRST = 0x1000;
            private const int LVM_SETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 54);
            private const int LVM_GETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 55);
            private const int LVS_EX_DOUBLEBUFFER = 0x00010000;
            */
            int _style = NativeMethods.SendMessage(_handle, 0x1037, 0, (IntPtr)0);
            _style |= 0x10000;
            NativeMethods.SendMessage(_handle, 0x1036, 0, (IntPtr)_style);
        }

        /// <summary>
        /// �t�H�[������钼�O
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // �I���m�F
            // �����I���E�V���b�g�_�E���ȊO�Ȃ�m�F����
            if (e.CloseReason != CloseReason.TaskManagerClosing &&
                e.CloseReason != CloseReason.WindowsShutDown &&
                AskFormClose)
            {
                if (MessageBox.Show("�A�v���P�[�V�������I�����܂����H", "�m�F", MessageBoxButtons.YesNo).Equals(DialogResult.No))
                {
                    e.Cancel = true;
                    return;
                }
            }
            // �����X�V�^�C�}�[��~
            timer1.Enabled = false;

            // Web�G���g�����X��~
            Front.WebEntrance.Stop();
            // �S����~
            Front.AllStop();
            // �ш搧���X���b�h��~
            if (BandTh != null && BandTh.IsAlive)
                BandTh.Abort();

            // �E�C���h�E����Data�ɕۑ�
            // �E�C���h�E�ʒu
            Front.Form.X = this.Location.X;
            Front.Form.Y = this.Location.Y;
            Front.Form.W = this.Size.Width;
            Front.Form.H = this.Size.Height;
            // �X�v���b�^�ʒu
            Front.Form.SplitDistance1 = (uint)splitContainer1.SplitterDistance;
            Front.Form.SplitDistance2 = (uint)splitContainer2.SplitterDistance;
            // ���p�l����ON/OFF���
            Front.Form.LeftPanelCollapsed = splitContainer1.Panel1Collapsed;
            // �eListView�̃J��������ۑ�
            Front.Form.KagamiListColumn = "" + clmKgmViewPort.Width + "," + clmKgmViewImport.Width + "," + clmKgmViewConn.Width;
            Front.Form.MonAllViewColumn = "" + clmMonAllView1.Width + "," + clmMonAllView2.Width;
            Front.Form.MonViewColumn = "" + clmMonView1.Width + "," + clmMonView2.Width;
            Front.Form.ClientViewColumn = "" + clmClientViewID.Width + "," + clmClientViewIpHost.Width + "," + clmClientViewUA.Width + "," + clmClientViewTime.Width;
            Front.Form.ResvViewColumn = "" + clmResvViewIP.Width + "," + clmResvViewStatus.Width;
            Front.Form.KickViewColumn = "" + clmKickViewIP.Width + "," + clmKickViewStatus.Width + "," + clmKickViewCount.Width;
            Front.Form.LogViewColumn = "" + clmLogView1.Width + "," + clmLogView2.Width;
            // monView�̑ш摬�x�\���P��
            Front.Form.monViewUnit = Unit;
            // ImportURL/�ő�ʏ�ڑ���/�ő僊�U�[�u�ڑ������I�����_�ł�GUI��̒l�ŕۑ�
            if (importURL.Text == "�ҋ@��")
                Front.Gui.ImportURL = "";
            else
                Front.Gui.ImportURL = importURL.Text;
            Front.Gui.Conn = (uint)connNum.Value;
            Front.Gui.Reserve = (uint)resvNum.Value;
            // ����̑��]���ʂ����Z
            Front.Log.TrsUpDay += Front.TotalUP;
            Front.Log.TrsDlDay += Front.TotalDL;
            Front.Log.TrsUpMon += Front.TotalUP;
            Front.Log.TrsDlMon += Front.TotalDL;
            Front.Log.LastUpdate = DateTime.Now.ToString("YYYYMMDD");
            // Front���t�@�C���ɕۑ��l����������
            Front.SaveSetting();
        }

        /// <summary>
        /// �E�C���h�E�T�C�Y�ύX�i�ő剻�E�ŏ����܂ށj
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            //�ŏ����Ń^�X�N�g���C�Ɋi�[
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (Front.Form.EnableTrayIcon)
                {
                    this.Visible = false;
                    this.TaskTrayIcon.Visible = true;
                }
            }
        }
        /// <summary>
        /// �E�C���h�E�T�C�Y�ύX�J�n
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            // ���ʂȂ����ǁA�ꉞ�A�ˁB
            tabKagami.SuspendLayout();
        }
        /// <summary>
        /// �E�C���h�E�T�C�Y�ύX����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            tabKagami.ResumeLayout();
        }
        /// <summary>
        /// �n���h��������
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            // �^�X�N�g���C�̃A�C�R�������̂��߂̃��b�Z�[�W�o�^
            _uTaskbarRestartMsg = NativeMethods.RegisterWindowMessage("TaskbarCreated");
            base.OnHandleCreated(e);
        }
        /// <summary>
        /// �E�C���h�E�v���V�[�W��
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // �^�X�N�g���C�̃A�C�R�����Z�b�g
            if (m.Msg == _uTaskbarRestartMsg && _uTaskbarRestartMsg != 0)
            {
                if (this.TaskTrayIcon.Visible)
                {
                    this.TaskTrayIcon.Visible = false;
                    this.TaskTrayIcon.Visible = true;
                }
            }
            base.WndProc(ref m);
        }
        #endregion

        #region �X�e�[�^�X�o�[
        /// <summary>
        /// ���p�l���k��OnOff����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripLeftPanelOnOff_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();
            splitContainer1.Panel1Collapsed = !splitContainer1.Panel1Collapsed;
            Front.Form.LeftPanelCollapsed = splitContainer1.Panel1Collapsed;
            statusBarAudit();
            this.ResumeLayout();
        }
        /// <summary>
        /// �ш搧��OnOff����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripBandStart_Click(object sender, EventArgs e)
        {
            Front.BndWth.EnableBandWidth = !Front.BndWth.EnableBandWidth;
            statusBarAudit();
            if (Front.BndWth.EnableBandWidth)
                StartBandWidth();
            else
                StopBandWidth();
            monViewUpdate(this.SelectedKagami);
        }
        /// <summary>
        /// ���u����G���g�����XOnOff����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripHPStart_Click(object sender, EventArgs e)
        {
            Front.HPStart = !Front.HPStart;
            statusBarAudit();
            if (Front.HPStart)
            {
                if (Front.WebEntrance.Start((int)Front.Hp.PortHTTP) == false)
                {
                    MessageBox.Show("�G���g�����X�N���Ɏ��s���܂���", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Front.HPStart = false;
                    statusBarAudit();
                    return;
                }
                Front.AddLogDebug("Web�G���g�����X", "�G���g�����X���N�����܂���");
                return;
            }
            else
            {
                Front.AddLogDebug("Web�G���g�����X", "�G���g�����X���~���܂���");
                Front.WebEntrance.Stop();
            }
        }
        /// <summary>
        /// �V�KIM�ڑ�����OnOff����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripHPPause_Click(object sender, EventArgs e)
        {
            Front.Pause = !Front.Pause;
            statusBarAudit();
        }
        /// <summary>
        /// �����I���{�^������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripAutoExit_Click(object sender, EventArgs e)
        {
            EnableAutoExit = !EnableAutoExit;
            statusBarAudit();
        }
        /// <summary>
        /// �����V���b�g�_�E���{�^������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripAutoShutdown_Click(object sender, EventArgs e)
        {
            if (ExecShutdown == false)
            {
                EnableAutoShutdown = !EnableAutoShutdown;
            }
            else
            {
                //shutdown stop
                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = "shutdown.exe";
                psInfo.CreateNoWindow = true;
                psInfo.UseShellExecute = false;
                psInfo.Arguments = "-a";
                Process.Start(psInfo);
                //shutdown�J�n�p�{�^���ɕύX
                toolStripAutoShutdown.BorderStyle = Border3DStyle.Raised;
                toolStripAutoShutdown.Text = "�����d�f";
                ExecShutdown = false;
                EnableAutoShutdown = false;
                AskFormClose = true;
            }
            statusBarAudit();
        }
        /// <summary>
        /// �X�e�[�^�X�o�[�̃{�^���\����Ԃ��X�V����
        /// </summary>
        private void statusBarAudit()
        {
            // �ʃX�^�C��:Raised�A���X�^�C��:Sunken

            // ���p�l���\���{�^��
            if (splitContainer1.Panel1Collapsed)
                toolStripLeftPanelOnOff.BorderStyle = Border3DStyle.Raised; // OFF
            else
                toolStripLeftPanelOnOff.BorderStyle = Border3DStyle.Sunken; // ON

            // �ш搧���{�^��
            if (Front.BndWth.EnableBandWidth)
                toolStripBandStart.BorderStyle = Border3DStyle.Sunken; // ON
            else
                toolStripBandStart.BorderStyle = Border3DStyle.Raised; // OFF

            if (Front.Hp.UseHP)
            {
                // �G���g�����X�N���{�^��
                toolStripCPU.Spring = false;
                toolStripCPU.Width = toolStripEXNum.Width;
                toolStripHPStart.Visible = true;
                toolStripHPPause.Visible = true;
                toolStripCPU.Spring = true;
                if (Front.HPStart)
                    toolStripHPStart.BorderStyle = Border3DStyle.Sunken; // ON
                else
                    toolStripHPStart.BorderStyle = Border3DStyle.Raised; // OFF

                // �V�K��t��~�{�^��
                if (Front.Pause)
                    toolStripHPPause.BorderStyle = Border3DStyle.Sunken; // ON
                else
                    toolStripHPPause.BorderStyle = Border3DStyle.Raised; // OFF
            }
            else
            {
                toolStripCPU.Spring = false;
                toolStripCPU.Width = toolStripEXNum.Width;
                toolStripHPStart.Visible = false;
                toolStripHPPause.Visible = false;
                toolStripCPU.Spring = true;
            }

            // �����I���{�^��
            if (EnableAutoExit)
                toolStripAutoExit.BorderStyle = Border3DStyle.Sunken;   // ON
            else
                toolStripAutoExit.BorderStyle = Border3DStyle.Raised;   // OFF

            // �����V���b�g�_�E���{�^��
            if (EnableAutoShutdown && !ExecShutdown)
                toolStripAutoShutdown.BorderStyle = Border3DStyle.Sunken; // ON
            else
                toolStripAutoShutdown.BorderStyle = Border3DStyle.Raised; // OFF
        }
        /// <summary>
        /// �X�e�[�^�X�o�[��EX,IM,CPU�����X�V����
        /// </summary>
        private void statusBarUpdate()
        {
            // ��EX��,��IM��
            int cnt_ex = 0, cnt_im = 0;
            foreach (Kagami _k in Front.KagamiList)
            {
                if (_k.Status.ImportStatus)
                    cnt_im++;
                cnt_ex += _k.Status.Client.Count;
            }
            toolStripEXNum.Text = "EX " + cnt_ex.ToString();
            toolStripIMNum.Text = "IM " + cnt_im.ToString();
            toolStripCPU.Text = "CPU " + monAllView.Items[0].SubItems[1].Text;
        }
        /// <summary>
        /// �X�e�[�^�X�o�[��ł�ToolTip�\���p
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripStatus_MouseHover(object sender, EventArgs e)
        {
            ToolStripStatusLabel _tssl = (ToolStripStatusLabel)sender;
            Point pos = MousePosition;
            pos.X -= this.Location.X; pos.X += 4;
            pos.Y -= this.Location.Y; pos.Y += 2;

            toolTip1.Show(_tssl.ToolTipText, this, pos);
        }
        /// <summary>
        /// �X�e�[�^�X�o�[��ł�ToolTip�\���p
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripStatus_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(this);
        }
        #endregion

        #region �^�X�N�g���C

        #region �^�X�N�g���C�A�C�R���̃C�x���g
        /// <summary>
        /// �^�X�N�g���C�A�C�R���̃N���b�N/�_�u���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskTrayIcon_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Left)
                ExitTaskTray();
        }
        /// <summary>
        /// �^�X�N�g���C���畜�A
        /// </summary>
        private void ExitTaskTray()
        {
            this.Visible = true;
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.Activate();
            this.TaskTrayIcon.Visible = false;
        }
        /// <summary>
        /// �^�X�N�g���C�A�C�R���Ƀ}�E�X���ڂ�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskTrayIcon_MouseMove(object sender, MouseEventArgs e)
        {
            showTaskTrayTip();
        }

        /// <summary>
        /// �^�X�N�g���C�A�C�R����BalloonTip�\��
        /// </summary>
        private void showTaskTrayTip()
        {
            if (!Front.Opt.BalloonTip)
                return;

            this.TaskTrayIcon.BalloonTipText = "";
            StringBuilder sb = new StringBuilder(1024);
            foreach (Kagami _k in Front.KagamiList)
            {
                sb.AppendLine(
                    "PORT:" + _k.Status.MyPort.ToString("####0") + " " +
                    _k.Status.Client.Count.ToString("#0") + "/" +
                    _k.Status.Connection.ToString("#0") + "(+" + _k.Status.Reserve.ToString() + ") " +
                    _k.Status.ImportURL
                    );
                sb.AppendLine(
                    "    " + _k.Status.Comment
                    );
            }
            if (sb.Length == 0)
            {
                this.TaskTrayIcon.BalloonTipText = "�N�����|�[�g�͂���܂���.";
            }
            else
            {
                //�Ō�̉��s�͍폜
                sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
                this.TaskTrayIcon.BalloonTipText = sb.ToString();
            }
            this.TaskTrayIcon.ShowBalloonTip(10000);
        }
        #endregion

        #region �^�X�N�g���C���j���[�̃C�x���g
        /// <summary>
        /// �E�C���h�E�ʒu���Z�b�g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RstWndPos_Click(object sender, EventArgs e)
        {
            ExitTaskTray();
            int x = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - this.Width) / 2; if (x < 0) x = 0;
            int y = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - this.Height) / 2; if (y < 0) y = 0;
            this.Location = new Point(x, y);
        }
        /// <summary>
        /// �I�����j���[
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrayMenuExit_Click(object sender, EventArgs e)
        {
            ExitTaskTray();
            this.Close();
        }
        #endregion

        #endregion

        #region ���p�l��
        /// <summary>
        /// kagamiView���̈ꗗ����_�u���N���b�N�őI��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void kagamiView_DoubleClick(object sender, EventArgs e)
        {
            for (int i = 0; i < kagamiView.Items.Count; i++)
            {
                if (kagamiView.Items[i].Selected)
                {
                    if (clmKgmViewPort.DisplayIndex == 0)
                        myPort.Text = kagamiView.Items[i].Text;
                    else
                        myPort.Text = kagamiView.Items[i].SubItems[clmKgmViewPort.DisplayIndex].Text;
                    myPort_StateChanged();
                    return;
                }
            }
        }
        /// <summary>
        /// kagamiView�E�N���b�N���j���[
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void kagamiViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // �I�𒆃A�C�e�������邩�`�F�b�N���āA�������Disable
            for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
            {
                if (kagamiView.Items[cnt].Selected)
                {
                    Kagami _k;
                    if (clmKgmViewPort.DisplayIndex == 0)
                        _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].Text));
                    else
                        _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].SubItems[clmKgmViewPort.DisplayIndex].Text));
                    if (_k == null)
                        continue;
                    KgmIpCopyMenu.Enabled = true;
                    KgmImIpCopyMenu.Enabled = true;
                    if (_k.Status.Type == 0)
                    {
                        KgmImPauseMenu.Visible = false;
                        KgmImDiscMenu.Visible = false;
                    }
                    else
                    {
                        KgmImPauseMenu.Visible = true;
                        KgmImPauseMenu.Enabled = true;
                        KgmImPauseMenu.Checked = _k.Status.Pause;
                        KgmImDiscMenu.Visible = true;
                        KgmImDiscMenu.Enabled = true;
                    }
                    return;
                }
            }
            KgmIpCopyMenu.Enabled = false;
            KgmImIpCopyMenu.Enabled = false;
            KgmImPauseMenu.Visible = false;
            KgmImDiscMenu.Visible = false;
        }
        /// <summary>
        /// ��URL�R�s�[���j���[
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KgmIpCopyMenu_Click(object sender, EventArgs e)
        {
            // kagamiView�͕����I���͏o���Ȃ����Ƃɂ��Ă���̂�
            // �P��������I���
            string _url;
            if (Front.Hp.UseHP && Front.Hp.IpHTTP.Length > 0)
                _url = "http://" + Front.Hp.IpHTTP + ":";
            else
                _url = "http://" + Front.GlobalIP + ":";
            for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
            {
                if (kagamiView.Items[cnt].Selected)
                {
                    if (clmKgmViewPort.DisplayIndex == 0)
                        _url += kagamiView.Items[cnt].Text;
                    else
                        _url += kagamiView.Items[cnt].SubItems[clmKgmViewPort.DisplayIndex].Text;
                    Clipboard.SetText(_url);
                    return;
                }
            }
        }
        /// <summary>
        /// ImportURL�R�s�[���j���[
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KgmImIpCopyMenu_Click(object sender, EventArgs e)
        {
            // kagamiView�͕����I���͏o���Ȃ����Ƃɂ��Ă���̂�
            // �P��������I���
            for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
            {
                if (kagamiView.Items[cnt].Selected)
                {
                    if (clmKgmViewImport.DisplayIndex == 0)
                        Clipboard.SetText(kagamiView.Items[cnt].Text);
                    else
                        Clipboard.SetText(kagamiView.Items[cnt].SubItems[clmKgmViewImport.DisplayIndex].Text);
                    return;
                }
            }
            return;
        }
        /// <summary>
        /// �ڑ�����(�|�[�g��)���j���[
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KgmImPauseMenu_Click(object sender, EventArgs e)
        {
            // kagamiView�͕����I���͏o���Ȃ����Ƃɂ��Ă���̂�
            // �P��������I���
            try
            {
                for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
                {
                    if (kagamiView.Items[cnt].Selected)
                    {
                        Kagami _k;
                        if (clmKgmViewPort.DisplayIndex == 0)
                            _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].Text));
                        else
                            _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].SubItems[clmKgmViewPort.DisplayIndex].Text));
                        if (_k == null)
                            continue;
                        _k.Status.Pause = !_k.Status.Pause;
                        Event.EventUpdateKagami();
                        return;
                    }
                }
            }
            catch { }
            return;
        }
        /// <summary>
        /// Import�����ؒf���j���[
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KgmImDiscMenu_Click(object sender, EventArgs e)
        {
            // kagamiView�͕����I���͏o���Ȃ����Ƃɂ��Ă���̂�
            // �P��������I���
            try
            {
                for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
                {
                    if (kagamiView.Items[cnt].Selected)
                    {
                        Kagami _k;
                        if (clmKgmViewPort.DisplayIndex == 0)
                            _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].Text));
                        else
                            _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].SubItems[clmKgmViewPort.DisplayIndex].Text));
                        if (_k == null)
                            continue;
                        _k.Status.Disc();
                        return;
                    }
                }
            }
            catch { }
            return;
        }
        /// <summary>
        /// monAllView�ɏ����A�C�e����ǉ�����
        /// </summary>
        private void monAllViewInit()
        {
            monAllView.Items.Add("�SCPU�g�p��");
            monAllView.Items.Add("��CPU�g�p��");
            monAllView.Items.Add("��UP�ш�");
            monAllView.Items.Add("��DL�ш�");
            monAllView.Items.Add("UP�]����/��");
            monAllView.Items.Add("DL�]����/��");
            monAllView.Items.Add("UP�]����/��");
            monAllView.Items.Add("DL�]����/��");
            for (int cnt = 0; cnt < monAllView.Items.Count; cnt++)
                monAllView.Items[cnt].SubItems.Add("");
            monAllViewUpdate();
        }
        /// <summary>
        /// �S�̃��j�^�̍X�V
        /// </summary>
        private void monAllViewUpdate()
        {
            //��up,down�]�����x�̌v�Z
            int _up = 0, _down = 0;
            foreach (Kagami _k in Front.KagamiList)
            {
                _up += _k.Status.CurrentDLSpeed * _k.Status.Client.Count;
                _down += _k.Status.CurrentDLSpeed;
            }

            //��up,down�]���ʂ̌v�Z
            //ulong _ul_day, _dl_day, _ul_mon, _dl_mon;
            string _ul_day, _dl_day, _ul_mon, _dl_mon;

            _ul_day = ((ulong)(Front.Log.TrsUpDay + Front.TotalUP)).ToString("#,##0,, [Mbyte]");
            _dl_day = ((ulong)(Front.Log.TrsDlDay + Front.TotalDL)).ToString("#,##0,, [Mbyte]");
            _ul_mon = ((ulong)(Front.Log.TrsUpMon + Front.TotalUP)).ToString("#,##0,, [Mbyte]");
            _dl_mon = ((ulong)(Front.Log.TrsDlMon + Front.TotalDL)).ToString("#,##0,, [Mbyte]");

            int idx = 0;
            // �S�̂�CPU�g�p��
            try
            {
                monAllView.Items[idx].SubItems[1].Text = System.Math.Round(Front.CPU_ALL.NextValue(), 1).ToString("0.0") + "%";
            }
            catch
            {
                monAllView.Items[idx].SubItems[1].Text = "�擾NG";
            }
            idx++;
            // ����CPU�g�p��
            try
            {
                monAllView.Items[idx].SubItems[1].Text = System.Math.Round(Front.CPU_APP.NextValue(), 1).ToString("0.0") + "%";
            }
            catch
            {
                monAllView.Items[idx].SubItems[1].Text = "�擾NG";
            }
            idx++;
            // ��UP�ш�
            monAllView.Items[idx].SubItems[1].Text = Unit == 0 ? _up.ToString("#,##0") + " [kbps]" : (_up/8).ToString("#,##0") + " [KB/s]"; idx++;
            // ��DOWN�ш�
            monAllView.Items[idx].SubItems[1].Text = Unit == 0 ? _down.ToString("#,##0") + " [kbps]" : (_down/8).ToString("#,##0") + " [KB/s]"; idx++;
            // ��UP�]����/��
            monAllView.Items[idx].SubItems[1].Text = _ul_day; idx++;
            // ��DL�]����/��
            monAllView.Items[idx].SubItems[1].Text = _dl_day; idx++;
            // ��UP�]����/��
            monAllView.Items[idx].SubItems[1].Text = _ul_mon; idx++;
            // ��DL�]����/��
            monAllView.Items[idx].SubItems[1].Text = _dl_mon; idx++;
        }
        /// <summary>
        /// monAllView�̃_�u���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monAllView_DoubleClick(object sender, EventArgs e)
        {
            // kbps��KB/s�ؑ�
            if (Unit == 0)
                Unit = 1;
            else
                Unit = 0;
            // ���X�V
            monAllViewUpdate();
            monViewUpdate(this.SelectedKagami);
        }
        #endregion
        
        #region �E�p�l���㕔
        /// <summary>
        /// �C���|�[�gURL�ŃL�[���͂�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importURL_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter���͂Ȃ�ڑ��������s��
            if (e.KeyChar == (char)Keys.Enter)
            {
                connBTN_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        #region ImportURL�E�N���b�N���j���[
        /// <summary>
        /// ���C�ɓ���֓o�^���j���[�I����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFavoriteMenu_Click(object sender, EventArgs e)
        {
            // �o�^�ς݃`�F�b�N
            if (Front.Gui.FavoriteList.Contains(importURL.Text))
                return;

            // �E�N���b�N���j���[�ɔ��f
            ToolStripMenuItem _tsmi = new ToolStripMenuItem();
            _tsmi.Text = importURL.Text;
            _tsmi.Click += PasteFavoriteURL;
            ImportUrlRClick.Items.Add(_tsmi);

            // �ݒ�t�@�C���ɔ��f
            Front.Gui.FavoriteList.Add(importURL.Text);
            Front.SaveSetting();
        }

        /// <summary>
        /// ���C�ɓ����ImportURL�֓\��t��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasteFavoriteURL(object sender, EventArgs e)
        {
            ToolStripMenuItem _tsmi = (ToolStripMenuItem)sender;
            try
            {
                if (!Front.IndexOf(int.Parse(myPort.Text)).Status.RunStatus)
                    importURL.Text = _tsmi.Text;
            }
            catch
            {
                // ��ԃ`�F�b�N���s
                importURL.Text = _tsmi.Text;
            }
        }

        private void ImportUrlCutMenu_Click(object sender, EventArgs e)
        {
            importURL.Cut();
        }

        private void ImportUrlCopyMenu_Click(object sender, EventArgs e)
        {
            importURL.Copy();
        }

        private void ImportUrlPasteMenu_Click(object sender, EventArgs e)
        {
            importURL.Paste();
        }

        private void ImportUrlEraseMenu_Click(object sender, EventArgs e)
        {
            if (importURL.SelectionLength > 0)
                importURL.Text = importURL.Text.Remove(importURL.SelectionStart, importURL.SelectionLength);
        }
        #endregion

        /// <summary>
        /// ���|�[�g�ԍ����ŃL�[���͂�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter���͂Ȃ�ڑ��������s��
            if (e.KeyChar == (char)Keys.Enter)
            {
                connBTN_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }
        /// <summary>
        /// �ڑ��{�^������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connBTN_Click(object sender, EventArgs e)
        {
            int _port;

            if (!Front.Hp.UseHP && !Front.Opt.EnablePush && importURL.Text == "")
            {
                MessageBox.Show("�C���|�[�gURL����͂��Ă��������B", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try { _port = int.Parse(myPort.Text); }
            catch { MessageBox.Show("�|�[�g�ԍ����ُ�ł��B\r\n65535�ȉ��̐��l��ݒ肵�Ă��������B", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            connBTN.Enabled = false;
            if(Front.IndexOf(_port) == null)
            {
                logView.Items.Clear();
                Kagami _k = new Kagami(importURL.Text, _port, (int)connNum.Value, (int)resvNum.Value);
                // �ʑш�ݒ�̏ꍇ�A����ш��ݒ肵�Ă���
                if(Front.BndWth.BandStopMode == 2)
                {
                    _k.Status.GUILimitUPSpeed = (int)Front.BndWth.BandStopValue;
                    _k.Status.LimitUPSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                }
                Front.Add(_k);
            }
        }

        /// <summary>
        /// �ؒf�{�^������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void discBTN_Click(object sender, EventArgs e)
        {
            discBTN.Enabled = false;
            Kagami _k = this.SelectedKagami;
            if (_k != null)
                _k.Status.RunStatus = false;
        }

        /// <summary>
        /// �ڑ��E�ؒf�{�^����ԃI�[�f�B�b�g
        /// </summary>
        private void ButtonAudit()
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                if (connBTN.Enabled == false)
                {
                    // �|�[�g���ڑ���conn�{�^�������F�ؒf����
                    connBTN.Enabled = true;
                    discBTN.Enabled = false;
                    // �ҋ@��/�ڑ��������ڑ��ɂȂ����̂�GUI�\���X�V
                    myPort_StateChanged();
                }
                else
                {
                    // ���ڑ���Ԃ�conn�{�^���L���FIdle��
                }
                // TitleBar�X�V
                this.Text = Front.AppName;
            }
            else
            {
                if (_k.Status.RunStatus == true)
                {
                    if (discBTN.Enabled == false)
                    {
                        // �|�[�g�ڑ���Ԃ�disc�{�^�������F�ڑ�����
                        connBTN.Enabled = false;
                        discBTN.Enabled = true;
                        // ���ڑ����ҋ@���ɂȂ����̂�GUI�\���X�V
                        myPort_StateChanged();
                    }
                    else
                    {
                        // �|�[�g�ڑ���Ԃ�disc�{�^���L���FRunning��
                        if (importURL.Text != _k.Status.ImportURL)
                            importURL.Text = _k.Status.ImportURL;
                    }
                }
                else
                {
                    // �|�[�g�ڑ���Ԃ�RunStatus==false�F�ؒf������
                }
                // TitleBar�X�V
                this.Text = "EX " + _k.Status.Client.Count + "/" + _k.Status.Connection + "+" + _k.Status.Reserve
                    + " PORT:" + _k.Status.MyPort + " " + Front.AppName;
            }

        }

        /// <summary>
        /// �����{�^������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearBTN_Click(object sender, EventArgs e)
        {
            if(importURL.Enabled)
                importURL.Text = "";
        }

        /// <summary>
        /// ���{�^������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyMyUrlBTN_Click(object sender, EventArgs e)
        {
            if (Front.GetGlobalIP())
                Clipboard.SetText("http://" + Front.GlobalIP + ":" + myPort.Text + "/");
        }

        /// <summary>
        /// �݃{�^������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optBTN_Click(object sender, EventArgs e)
        {
            Option optDlg = new Option();
            // �ݒ��ʕ\��
            optDlg.ShowDialog();
            // �V�ݒ�����[�h
            LoadSetting();
        }

        /// <summary>
        /// ���|�[�g�ԍ���ύX������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myPort_TextChanged(object sender, EventArgs e)
        {
            myPort_StateChanged();
        }
        /// <summary>
        /// �|�[�g��ԕύX
        /// </summary>
        private void myPort_StateChanged()
        {
            ButtonAudit();

            Kagami _k = this.SelectedKagami;

            //�E�p�l���㕔�̏�Ԃ��C��
            if (_k != null)
            {
                //�N�����|�[�g
                importURL.Text = _k.Status.ImportURL;
                importURL.Enabled = false;
                connNum.Value = _k.Status.Conn_UserSet;
                resvNum.Value = _k.Status.Reserve;
            }
            else
            {
                //���N���|�[�g
                if (importURL.Text == "�ҋ@��")
                    importURL.Text = "";
                importURL.Enabled = true;
            }
            //tabKagami�̃^�u��Ԃ��C��
            tabKagami_SelectedIndexChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// �ő�ʏ�ڑ�����ύX
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connNum_ValueChanged(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.Conn_UserSet = (int)connNum.Value;
                if (Front.BndWth.EnableBandWidth)
                {
                    // �ш搧�����͐l�����炵�͑����f�B
                    // �l�������͑ш搧���^�X�N�ł̍Čv�Z�_�@�ɍs���B
                    if (_k.Status.Connection > connNum.Value)
                        _k.Status.Connection = (int)connNum.Value;
                }
                else
                {
                    _k.Status.Connection = (int)connNum.Value;
                }
                LeftFlag = true;
                BandFlag = true;
            }
            else
            {
                Front.Gui.Conn = (uint)connNum.Value;
            }
        }
        /// <summary>
        /// �ő�ʏ�ڑ�����ύX�i�L�[���́j
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connNum_KeyUp(object sender, KeyEventArgs e)
        {
            connNum_ValueChanged(sender, (EventArgs)e);
        }

        /// <summary>
        /// �ő僊�U�[�u�ڑ�����ύX
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resvNum_ValueChanged(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.Reserve = (int)resvNum.Value;
                LeftFlag = true;
                BandFlag = true;
            }
            else
            {
                Front.Gui.Reserve = (uint)resvNum.Value;
            }
        }
        /// <summary>
        /// �ő僊�U�[�u�ڑ�����ύX�i�L�[���́j
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resvNum_KeyUp(object sender, KeyEventArgs e)
        {
            resvNum_ValueChanged(sender, (EventArgs)e);
        }
        #endregion

        /// <summary>
        /// tabKagami���蓮�Ő؂�ւ������̏���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabKagami_SelectedIndexChanged(object sender, EventArgs e)
        {
            //�I�𒆃^�u�̂ݍX�V
            Kagami _k = this.SelectedKagami;
            switch (this.tabKagami.SelectedTab.Text)
            {
                case "���j�^":
                    //�����ĕ`��Ɠ�������
                    monViewUpdate(_k);
                    //�ʑш搧���̏ꍇ�A�\�����eAUDIT
                    if (Front.BandStopTypeString[Front.BndWth.BandStopMode] == "�|�[�g���Ɍʐݒ�")
                        bndStopNumAudit();
                    break;
                case "�N���C�A���g":
                    if (_k != null)
                    {
                        //�V�|�[�g��ClientItem���ꊇ�ݒ�
                        clientView.BeginUpdate();
                        clientView.Items.Clear();
                        _k.Status.Client.UpdateClientTime();
                        clientView.Items.AddRange(_k.Status.Gui.ClientItem.ToArray());
                        clientView.EndUpdate();
                    }
                    break;
                case "���U�[�u":
                    if (_k != null)
                    {
                        reserveView.BeginUpdate();
                        reserveView.Items.Clear();
                        reserveView.Items.AddRange(_k.Status.Gui.ReserveItem.ToArray());
                        // �S�o�^IP�̏��AUDIT
                        // �o�^IP�����Ȃ���Ԃ̃��X�g�쐬
                        List<string> _ip_list = new List<string>();
                        lock (_k.Status.Gui.ReserveItem)
                        {
                            foreach (ListViewItem _item in _k.Status.Gui.ReserveItem)
                            {
                                if (!_ip_list.Contains(_item.Text))    // clmResvViewIP
                                    _ip_list.Add(_item.Text);
                            }
                        }
                        foreach (string _ip in _ip_list)
                        {
                            AuditReserve(_k, _ip);
                        }
                        reserveView.EndUpdate();
                    }
                    break;
                case "�L�b�N":
                    if (_k != null)
                    {
                        kickView.BeginUpdate();
                        kickView.Items.Clear();
                        _k.Status.Client.UpdateKickTime();
                        kickView.Items.AddRange(_k.Status.Gui.KickItem.ToArray());
                        kickView.EndUpdate();
                    }
                    break;
                case "���O":
                    if (_k != null)
                    {
                        //LogItem����ꊇ�ݒ�
                        logView.BeginUpdate();
                        logView.Items.Clear();
                        if (Front.LogMode == 0)
                            logView.Items.AddRange(_k.Status.Gui.LogAllItem.ToArray());
                        else
                            logView.Items.AddRange(_k.Status.Gui.LogImpItem.ToArray());
                        // �I�[�g�X�N���[��
                        if (logAutoScroll.Checked && logView.Items.Count > 0)
                            logView.EnsureVisible(logView.Items.Count - 1);
                        logView.EndUpdate();
                    }
                    break;
                default:
                    throw new Exception("not implemented.");
            }
        }

        #region ���j�^�^�u

        #region monView�̏���
        /// <summary>
        /// monView�ɏ����A�C�e����ǉ�����
        /// </summary>
        private void monViewInit()
        {
            monView.Items.Add("IM���");
            monView.Items.Add("IM�ڑ�����");
            monView.Items.Add("EX�ڑ���");
            monView.Items.Add("�ш搧��");
            monView.Items.Add("UP�ш�");
            monView.Items.Add("DOWN�ш�");
            monView.Items.Add("UP�]����");
            monView.Items.Add("DOWN�]����");
            monView.Items.Add("�r�W�[�J�E���^");
            monView.Items.Add("IM�s����");
            monView.Items.Add("EX�s����");
            monView.Items.Add("EX�ڑ���");
            monView.Items.Add("�R�����g");
            monView.Items.Add("IM�ݒ��IP");
            monView.Items.Add("����URL");
            monView.Items.Add("Redirect/�q");
            monView.Items.Add("Redirect/�e");
#if PLUS
            monView.Items.Add("Mode");
#endif
            for (int cnt = 0; cnt < monView.Items.Count; cnt++)
                monView.Items[cnt].SubItems.Add("");
            monViewUpdate(null);
        }
        /// <summary>
        /// �����I�ɌĂ΂�āA
        /// monView�̓��e���X�V����
        /// </summary>
        private void monViewUpdate(Kagami _k)
        {
            monView.BeginUpdate();
            if (_k != null)
            {
                int idx = 0;

                //up,down�]���ʂ̌v�Z
                string _ul, _dl;
                //if (_k.Status.TotalUPSize > 10 * 1000 * 1000)
                    _ul = _k.Status.TotalUPSize.ToString("#,##0,,") + " [Mbyte]";
                //else
                //    _ul = _k.Status.TotalUPSize.ToString("#,##0,") + " [kbyte]";
                //if (_k.Status.TotalDLSize > 10 * 1000 * 1000)
                    _dl = _k.Status.TotalDLSize.ToString("#,##0,,") + " [Mbyte]";
                //else
                //    _dl = _k.Status.TotalDLSize.ToString("#,##0,") + " [kbyte]";

                // IM���
                monView.Items[idx].SubItems[1].Text = _k.Status.ImportURL != "�ҋ@��" ? (_k.Status.ImportStatus ? "����" : "�ڑ����s��...") : "�ҋ@��"; idx++;
                // IM�ڑ�����
                monView.Items[idx].SubItems[1].Text = _k.Status.ImportStatus ? _k.Status.ImportTimeString : "-"; idx++;
                // EX�ڑ���
                monView.Items[idx].SubItems[1].Text = _k.Status.Client.Count + "/" + _k.Status.Connection + "+" + _k.Status.Reserve; idx++;
                // �ш搧��
                monView.Items[idx].SubItems[1].Text = Front.BndWth.EnableBandWidth ? "�J�n��: " + _k.Status.LimitUPSpeed + " [kbps]" : "��~��"; idx++;
                // UP�ш�
                monView.Items[idx].SubItems[1].Text = Unit == 0 ? (_k.Status.CurrentDLSpeed * _k.Status.Client.Count).ToString("#,##0") + " [kbps]" : (_k.Status.CurrentDLSpeed/8 * _k.Status.Client.Count).ToString("#,##0") + " [KB/s]"; idx++;
                // DOWN�ш�
                monView.Items[idx].SubItems[1].Text = Unit == 0 ? _k.Status.CurrentDLSpeed.ToString("#,##0") + " [kbps]" : (_k.Status.CurrentDLSpeed/8).ToString("#,##0") + " [KB/s]"; idx++;
                // UP�]����
                monView.Items[idx].SubItems[1].Text = _ul; idx++;
                // DOWN�]����
                monView.Items[idx].SubItems[1].Text = _dl; idx++;
                // �r�W�[�J�E���^
                monView.Items[idx].SubItems[1].Text = _k.Status.BusyCounter.ToString(); idx++;
                // IM�s����
                monView.Items[idx].SubItems[1].Text = _k.Status.ImportError.ToString(); idx++;
                // EX�s����
                monView.Items[idx].SubItems[1].Text = _k.Status.ExportError.ToString(); idx++;
                // EX�ڑ���
                monView.Items[idx].SubItems[1].Text = _k.Status.ExportCount.ToString(); idx++;
                // �R�����g
                monView.Items[idx].SubItems[1].Text = _k.Status.Comment; idx++;
                // �ڑ��ݒ��IP
                monView.Items[idx].SubItems[1].Text = _k.Status.SetUserIP; idx++;
                // ����URL
                monView.Items[idx].SubItems[1].Text = _k.Status.Url; idx++;
                // Redirect/�q
                monView.Items[idx].SubItems[1].Text = _k.Status.EnableRedirectChild == true ? "�L��" : "����"; idx++;
                // Redirect/�e
                monView.Items[idx].SubItems[1].Text = _k.Status.EnableRedirectParent == true ? "�L��" : "����"; idx++;
#if PLUS
                // Mode
                monView.Items[idx].SubItems[1].Text = _k.Status.DisablePull ? "Pull����/Push�L��" : Front.Opt.EnablePush ? "Pull�L��/Push�L��" : "Pull�L��/Push����"; idx++;
#endif
            }
            else
            {
                int idx = 0;
                // IM���
                monView.Items[idx].SubItems[1].Text = "���ڑ�"; idx++;
                // IM�ڑ�����
                monView.Items[idx].SubItems[1].Text = "-"; idx++;
                // EX�ڑ���
                monView.Items[idx].SubItems[1].Text = "-"; idx++;
                // �ш搧��
                monView.Items[idx].SubItems[1].Text = (Front.BndWth.EnableBandWidth ? "�J�n��" : "��~��"); idx++;
                // UP�ш�
                monView.Items[idx].SubItems[1].Text = Unit == 0 ? "0 [kbps]" : "0 [KB/s]"; idx++;
                // DOWN�ш�
                monView.Items[idx].SubItems[1].Text = Unit == 0 ? "0 [kbps]" : "0 [KB/s]"; idx++;
                // UP�]����
                monView.Items[idx].SubItems[1].Text = "0 [kbyte]"; idx++;
                // DOWN�]����
                monView.Items[idx].SubItems[1].Text = "0 [kbyte]"; idx++;
                // �r�W�[�J�E���^
                monView.Items[idx].SubItems[1].Text = "0"; idx++;
                // IM�s����
                monView.Items[idx].SubItems[1].Text = "0"; idx++;
                // EX�s����
                monView.Items[idx].SubItems[1].Text = "0"; idx++;
                // EX�ڑ���
                monView.Items[idx].SubItems[1].Text = "0"; idx++;
                // �R�����g
                monView.Items[idx].SubItems[1].Text = ""; idx++;
                // �ڑ��ݒ��IP
                monView.Items[idx].SubItems[1].Text = ""; idx++;
                // ����URL
                monView.Items[idx].SubItems[1].Text = ""; idx++;
                // Redirect/�q
                monView.Items[idx].SubItems[1].Text = "-"; idx++;
                // Redirect/�e
                monView.Items[idx].SubItems[1].Text = "-"; idx++;
#if PLUS
                // Mode
                monView.Items[idx].SubItems[1].Text = Front.Opt.EnablePush ? "Push�L��" : "Push����"; idx++;
#endif
            }
            monView.EndUpdate();
        }
        /// <summary>
        /// monView�̃_�u���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monView_DoubleClick(object sender, EventArgs e)
        {
            ListView _monView = (ListView)sender;
            Kagami _k = this.SelectedKagami;
            switch (_monView.FocusedItem.Text)
            {
                case "UP�ш�":
                case "DOWN�ш�":
                    // kbps��KB/s�ؑ�
                    this.Unit = (uint)(this.Unit == 0 ? 1 : 0);
                    break;
                case "Redirect/�q":
                    if (_k != null)
                    {
                        _k.Status.EnableRedirectChild = !_k.Status.EnableRedirectChild;
                        MonRedirCMenu.Checked = _k.Status.EnableRedirectChild;
                    }
                    break;
                case "Redirect/�e":
                    if (_k != null)
                    {
                        _k.Status.EnableRedirectParent = !_k.Status.EnableRedirectParent;
                        MonRedirPMenu.Checked = _k.Status.EnableRedirectParent;
                    }
                    break;
                case "Mode":
                    if (_k != null)
                    {
                        if (Front.Opt.EnablePush)
                        {
                            _k.Status.DisablePull = !_k.Status.DisablePull;
                        }
                    }
                    break;
                default:
                    return;
            }
            // ���X�V
            monAllViewUpdate();
            monViewUpdate(this.SelectedKagami);
        }

        /// <summary>
        /// ���j�^�^�u�ŉE�N���b�N���j���[���J�����Ƃ�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonViewRClick_Opening(object sender, CancelEventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                MonRedirCMenu.Checked = false;
                MonRedirPMenu.Checked = false;
                MonImIpCopyMenu.Enabled = false;
                MonRedirCMenu.Enabled = false;
                MonRedirPMenu.Enabled = false;
                MonModeChgMenu.Enabled = false;
            }
            else
            {
                MonImIpCopyMenu.Enabled = false;
                foreach (ListViewItem _item in monView.Items)
                {
                    if (_item.Text == "IM�ݒ��IP")
                    {
                        if (_item.SubItems[1].Text != "")
                        {
                            MonImIpCopyMenu.Enabled = true;
                            break;
                        }
                    }
                }
                MonRedirCMenu.Enabled = true;
                MonRedirPMenu.Enabled = true;
                MonRedirCMenu.Checked = _k.Status.EnableRedirectChild;
                MonRedirPMenu.Checked = _k.Status.EnableRedirectParent;
                MonModeChgMenu.Enabled = Front.Opt.EnablePush;
            }
        }

        /// <summary>
        /// kbps��KB/s�؂�ւ����j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonUnitChgMenu_Click(object sender, EventArgs e)
        {
            // kbps��KB/s�ؑ�
            this.Unit = (uint)(this.Unit == 0 ? 1 : 0);
            // ���X�V
            monAllViewUpdate();
            monViewUpdate(this.SelectedKagami);
        }

        /// <summary>
        /// IM�ݒ��IP�R�s�[���j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonImIpCopyMenu_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem _item in monView.Items)
            {
                if (_item.Text == "IM�ݒ��IP")
                {
                    if (_item.SubItems[1].Text != "")
                    {
                        Clipboard.SetText(_item.SubItems[1].Text);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Redirect/�q���j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonRedirCMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.EnableRedirectChild = !_k.Status.EnableRedirectChild;
                MonRedirCMenu.Checked = _k.Status.EnableRedirectChild;
            }
            // ���X�V
            monViewUpdate(this.SelectedKagami);
        }

        /// <summary>
        /// Redirect/�e���j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonRedirPMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.EnableRedirectParent = !_k.Status.EnableRedirectParent;
                MonRedirPMenu.Checked = _k.Status.EnableRedirectParent;
            }
            // ���X�V
            monViewUpdate(this.SelectedKagami);
        }

        /// <summary>
        /// Mode�؂�ւ����j���[
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonModeChgMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.DisablePull = !_k.Status.DisablePull;
            }
            // ���X�V
            monViewUpdate(this.SelectedKagami);
        }
        #endregion

        #region �ш搧���_�C�A���O�̏���
        /// <summary>
        /// �ш搧���l�ύX
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bndStopNum_ValueChanged(object sender, EventArgs e)
        {
            if (Front.BandStopTypeString[Front.BndWth.BandStopMode] != "�|�[�g���Ɍʐݒ�")
            {
                Front.BndWth.BandStopValue = (uint)bndStopNum.Value;
            }
            else
            {
                Kagami _k = this.SelectedKagami;
                if (_k != null)
                {
                    _k.Status.GUILimitUPSpeed = (int)bndStopNum.Value;
                    _k.Status.LimitUPSpeed = Front.CnvLimit((int)bndStopNum.Value, (int)Front.BndWth.BandStopUnit);
                }
            }
        }

        /// <summary>
        /// �ʑш搧���l�ύX�i�L�[���́j
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bndStopNum_KeyUp(object sender, KeyEventArgs e)
        {
            bndStopNum_ValueChanged(sender, (EventArgs)e);
        }

        /// <summary>
        /// �ݒ�ύX���E�|�[�g�ؑ֎��Ȃǂ�
        /// ���j�^�^�u��̑ш搧���l���Đݒ肷��
        /// (�ш搧������:�|�[�g���ʐݒ�p)
        /// </summary>
        private void bndStopNumAudit()
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                // �f�t�H���g�l
                bndStopNum.Value = Front.BndWth.BandStopValue;
            }
            else
            {
                switch (Front.BandStopUnitString[Front.BndWth.BandStopUnit])
                {
                    case "KB/s":
                        bndStopNum.Value = _k.Status.LimitUPSpeed / 8;
                        break;
                    case "MB/s":
                        bndStopNum.Value = _k.Status.LimitUPSpeed / 8000;
                        break;
                    case "Kbps":
                        bndStopNum.Value = _k.Status.LimitUPSpeed;
                        break;
                    case "Mbps":
                        bndStopNum.Value = _k.Status.LimitUPSpeed / 1000;
                        break;
                    default:
                        throw new Exception("not implemented.");
                }
            }
        }
        #endregion

        #endregion

        #region �N���C�A���g�^�u
        /// <summary>
        /// �N���C�A���g�̐ڑ��E�ؒf���ɋN�������
        /// clientView�Ǘ��X���b�h�ȊO����Ă΂ꂽ��A�X���b�h�ԒʐM����
        /// �ʒm���|�[�g��GUI��̑I���|�[�g����v����΁A
        /// clientView�ւ̃A�C�e���ǉ��E�폜�����ƁAreserveView�̏�ԍX�V���s��
        /// </summary>
        /// <param name="_ke"></param>
        void clientViewUpdate(KagamiEvent _ke)
        {
            if (clientView.InvokeRequired)
            {
                if (clientView.IsHandleCreated)
                {
                    //�R���g���[���Ǘ����X���b�h�ȊO�͂�����
                    //�R���g���[���Ǘ����X���b�h�ɔ񓯊�Invoke�ʐM�ōX�V�v��
                    EventHandlerDelegate _dlg = new EventHandlerDelegate(clientViewUpdate);
                    this.BeginInvoke(_dlg, new object[] { _ke });
                    //this.Invoke(_dlg, new object[] { _ke });
                }
            }
            else
            {
                // �R���g���[���Ǘ����X���b�h�͂�����
                Kagami _k = this.SelectedKagami;
                if (_k != null && _k == _ke.Kagami && !clientView.IsDisposed)
                {
                    if (_ke.Mode == 0)
                    {
                        // �N���C�A���g�ڑ��ʒm
                        // �ʒm���ꂽ�A�C�e����clientView�ɒǉ�
                        // �I���|�[�g�ύX�ɂ���d�o�^�G���[��h������
                        // �o�^�O�Ɋm�F���Ă���
                        if (!clientView.Items.Contains(_ke.Item))
                            clientView.Items.Add(_ke.Item);
                    }
                    else
                    {
                        // �N���C�A���g�ؒf�ʒm
                        // �ʒm���ꂽ�A�C�e����clientView����폜
                        // �O�̂��߁A�o�^�ς݂ł��邱�Ƃ��m�F���Ă���
                        if (clientView.Items.Contains(_ke.Item))
                            clientView.Items.Remove(_ke.Item);
                    }
                    // reserveView��ԍX�V
                    if (Front.clmCV_IP_IDX == 0)
                        AuditReserve(_k, _ke.Item.Text);
                    else
                        AuditReserve(_k, _ke.Item.SubItems[Front.clmCV_IP_IDX].Text);
                }
            }
        }
        /// <summary>
        /// �N���C�A���g�^�u�ŉE�N���b�N���j���[���J�����Ƃ�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // �I�𒆃A�C�e�������邩�`�F�b�N���āA�������Disable
            // ������ClientResolveHostMenu�͏��Enable
            for (int cnt = 0; cnt < clientView.Items.Count; cnt++)
            {
                if (clientView.Items[cnt].Selected)
                {
                    //ClientViewRClick.Enabled = true;
                    ClientDiscMenu.Enabled = true;
                    ClientKickDefMenu.Enabled = true;
                    ClientKickSubMenu.Enabled = true;
                    ClientAddResvMenu.Enabled = true;
                    ClientIPCopyMenu.Enabled = true;
                    ClientResolveHostMenu.Enabled = true;
                    return;
                }
            }
            //ClientViewRClick.Enabled = false;
            ClientDiscMenu.Enabled = false;
            ClientKickDefMenu.Enabled = false;
            ClientKickSubMenu.Enabled = false;
            ClientAddResvMenu.Enabled = false;
            ClientIPCopyMenu.Enabled = false;
            ClientResolveHostMenu.Enabled = true;
            return;
        }

        /// <summary>
        /// �N���C�A���g�ؒf���j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientDiscMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;

            try
            {
                //��납��؂��Ă�
                for (int cnt = clientView.Items.Count - 1; cnt >= 0; cnt--)
                {
                    if (clientView.Items[cnt].Selected)
                    {
                        //�����ؒf
                        if (Front.clmCV_ID_IDX == 0)
                            _k.Status.Client.Disc(clientView.Items[cnt].Text);
                        else
                            _k.Status.Client.Disc(clientView.Items[cnt].SubItems[Front.clmCV_ID_IDX].Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("ClientDiscMenu", "�����G���[ Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// �N���C�A���g�^�u��ŃL�b�N���j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientKickMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;
            try
            {
                // Tag�v���p�e�B�ɃL�b�N���Ԃ��ݒ肳��Ă���
                int _deny_tim = int.Parse((string)(((ToolStripMenuItem)sender).Tag));
                // �W���L�b�N��Tag���ɂO���ݒ肳��Ă���̂ŁA���[�U�ݒ�l�ɒu��������
                if (_deny_tim == 0)
                    _deny_tim = (int)Front.Kick.KickDenyTime;

                //��납��R���Ă�
                for (int cnt = clientView.Items.Count - 1; cnt >= 0; cnt--)
                {
                    if (clientView.Items[cnt].Selected == false)
                        continue;

                    //�����Ǘ�KickList�ւ̓o�^
                    if (Front.clmCV_IP_IDX == 0)
                        Front.AddKickUser(clientView.Items[cnt].Text, _deny_tim);
                    else
                        Front.AddKickUser(clientView.Items[cnt].SubItems[Front.clmCV_IP_IDX].Text, _deny_tim);

                    //GUI�\���pKickItem�ɖ��o�^�Ȃ�o�^����
                    lock (_k.Status.Gui.KickItem)
                    {
                        int cnt2;
                        for (cnt2 = 0; cnt2 < _k.Status.Gui.KickItem.Count; cnt2++)
                        {
                            if (clmKickViewIP.DisplayIndex == 0)
                            {
                                if (_k.Status.Gui.KickItem[cnt2].Text == clientView.Items[cnt].Text)
                                    break;
                            }
                            else
                            {
                                if (_k.Status.Gui.KickItem[cnt2].SubItems[clmKickViewIP.DisplayIndex].Text == clientView.Items[cnt].SubItems[Front.clmCV_IP_IDX].Text)
                                    break;
                            }
                        }
                        if (cnt2 >= _k.Status.Gui.KickItem.Count)
                        {
                            if (Front.clmCV_IP_IDX == 0)
                                _k.Status.AddKick(clientView.Items[cnt].Text, 0);
                            else
                                _k.Status.AddKick(clientView.Items[cnt].SubItems[Front.clmCV_IP_IDX].Text, 0);
                        }
                    }
                    //�ڑ����Ȃ狭���ؒf
                    if (clientView.Items[cnt].Selected)
                    {
                        if (Front.clmCV_ID_IDX == 0)
                            _k.Status.Client.Disc(clientView.Items[cnt].Text);
                        else
                            _k.Status.Client.Disc(clientView.Items[cnt].SubItems[Front.clmCV_ID_IDX].Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("ClientKickMenu", "�����G���[ Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// ���U�[�u�o�^���j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientAddResvMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;

            try
            {
                //�I�𒆂̑S�N���C�A���g�����U�[�u�o�^
                lock(_k.Status.Gui.ClientItem)
                    foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                        if (_item.Selected)
                            if (Front.clmCV_IP_IDX == 0)
                            {
                                _k.Status.AddReserve(_item.Text);
                                AuditReserve(_k, _item.Text);
                            }
                            else
                            {
                                _k.Status.AddReserve(_item.SubItems[Front.clmCV_IP_IDX].Text);
                                AuditReserve(_k, _item.SubItems[Front.clmCV_IP_IDX].Text);
                            }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("ClientAddResvMenu", "�����G���[ Trace:" + ex.StackTrace);
            }
        }
        /// <summary>
        /// IP�A�h���X�R�s�[���j���[�̃N���b�N
        /// �N���C�A���g/�L�b�N/���U�[�u���ʏ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IPCopyMenu_Click(object sender, EventArgs e)
        {
            string copy_string = "";
            ToolStripMenuItem _tsmi = (ToolStripMenuItem)sender;
            switch (_tsmi.Name)
            {
                case "ClientIPCopyMenu":
                    if (Front.clmCV_IP_IDX == 0)
                    {
                        foreach (ListViewItem _item in clientView.Items)
                            if (_item.Selected)
                                copy_string += _item.Text + Environment.NewLine;
                    }
                    else
                    {
                        foreach (ListViewItem _item in clientView.Items)
                            if (_item.Selected)
                                copy_string += _item.SubItems[Front.clmCV_IP_IDX].Text + Environment.NewLine;
                    }
                    break;
                case "KickIPCopyMenu":
                    if (clmKickViewIP.DisplayIndex == 0)
                    {
                        foreach (ListViewItem _item in kickView.Items)
                            if (_item.Selected)
                                copy_string += _item.Text + Environment.NewLine;
                    }
                    else
                    {
                        foreach (ListViewItem _item in kickView.Items)
                            if (_item.Selected)
                                copy_string += _item.SubItems[clmKickViewIP.DisplayIndex].Text + Environment.NewLine;
                    }
                    break;
                case "ResvIPCopyMenu":
                    if (clmResvViewIP.DisplayIndex == 0)
                    {
                        foreach (ListViewItem _item in reserveView.Items)
                            if (_item.Selected)
                                copy_string += _item.Text + Environment.NewLine;
                    }
                    else
                    {
                        foreach (ListViewItem _item in reserveView.Items)
                            if (_item.Selected)
                                copy_string += _item.SubItems[clmResvViewIP.DisplayIndex].Text + Environment.NewLine;
                    }
                    break;
                default:
                    throw new Exception("not implemented.");
            }
            if (copy_string.Length > Environment.NewLine.Length)
                copy_string = copy_string.Remove(copy_string.Length - Environment.NewLine.Length); // �]�v�Ȗ����̉��s����
            Clipboard.SetText(copy_string);
        }
        /// <summary>
        /// �h���C�������L��/�����؂�ւ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientResolveHostMenu_Click(object sender, EventArgs e)
        {
            Front.Opt.EnableResolveHost = ClientResolveHostMenu.Checked;
            int _target = Front.Opt.EnableResolveHost ? Front.clmCV_HO_IDX : Front.clmCV_IP_IDX;
            // �S���̃N���C�A���g���X�g���Front.clmCV_IH_IDX�J�������̒l��
            // _target�J�����̒l�ɏ���������
            lock (Front.KagamiList)
                foreach (Kagami _k in Front.KagamiList)
                    lock (_k.Status.Gui.ClientItem)
                        if (Front.clmCV_IH_IDX == 0)
                            // clmCV_IH_IDX==0�Ȃ�_targe==0�͂��肦�Ȃ�
                            foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                                _item.Text = _item.SubItems[_target].Text;
                        else if (_target == 0)
                            // _targe==0�Ȃ�clmCV_IH_IDX==0�͂��肦�Ȃ�
                            foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                                _item.SubItems[Front.clmCV_IH_IDX].Text = _item.Text;
                        else
                            foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                                _item.SubItems[Front.clmCV_IH_IDX].Text = _item.SubItems[_target].Text;
        }
        #endregion

        #region ���U�[�u�^�u
        /// <summary>
        /// ���U�[�u�o�^��Ԃ��ω�������N�������
        /// </summary>
        /// <param name="_ke"></param>
        void reserveViewUpdate(KagamiEvent _ke)
        {
            if (reserveView.InvokeRequired)
            {
                if (reserveView.IsHandleCreated)
                {
                    //�R���g���[���Ǘ����X���b�h�ȊO�͂�����
                    //�R���g���[���Ǘ����X���b�h�ɔ񓯊�Invoke�ʐM�ōX�V�v��
                    EventHandlerDelegate _dlg = new EventHandlerDelegate(reserveViewUpdate);
                    this.BeginInvoke(_dlg, new object[] { _ke });
                    //this.Invoke(_dlg, new object[] { _ke });
                }
            }
            else
            {
                // �R���g���[���Ǘ����X���b�h�͂�����
                // GUI��̑I���|�[�g�ƒʒm���|�[�g���ꏏ�Ȃ�reserveView�̃A�C�e���ǉ��E�폜
                Kagami _k = this.SelectedKagami;
                if (_k != null && _k == _ke.Kagami && !reserveView.IsDisposed)
                {
                    switch (_ke.Mode)
                    {
                        case 0: // �ǉ�
                            // reserveView��Item��ǉ�
                            if (!reserveView.Items.Contains(_ke.Item))
                                reserveView.Items.Add(_ke.Item);
                            break;
                        case 1: // �폜
                            // reserveView����Item���폜
                            if (reserveView.Items.Contains(_ke.Item))
                                reserveView.Items.Remove(_ke.Item);
                            break;
                        case 2: // �S�폜
                            reserveView.Items.Clear();
                            break;
                        default:
                            break;
                    }
                    // ���U�[�u���AUDIT
                    if (_ke.Item != null)   // �S�폜�̎���null�����肤��
                    {
                        if (clmResvViewIP.DisplayIndex == 0)
                            AuditReserve(_k, _ke.Item.Text);
                        else
                            AuditReserve(_k, _ke.Item.SubItems[clmResvViewIP.DisplayIndex].Text);
                    }
                }
            }
        }

        /// <summary>
        /// ���U�[�u�ǉ��{�^���̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addResvBTN_Click(object sender, EventArgs e)
        {
            if (addResvHost.Text == "")
                return;

            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                MessageBox.Show("�|�[�g���N�����Ă���ǉ����Ă�������", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                //���U�[�u�ǉ�
                System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(addResvHost.Text)[0];
                _k.Status.AddReserve(hostadd.ToString());
                //�\���X�V
                addResvHost.Text = "";
                //���U�[�u���AUDIT
                AuditReserve(_k, hostadd.ToString());
            }
            catch
            {
                MessageBox.Show("�z�X�g������IP�A�h���X�ɕϊ��ł��܂���ł���", "DNS Error", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// ���U�[�u�o�^�z�X�g���͗��ŃL�[���͂�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addResvHost_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter���͂Ȃ�ǉ��������s��
            if (e.KeyChar == (char)Keys.Enter)
            {
                addResvBTN_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        /// <summary>
        /// ���U�[�u�^�u�ŉE�N���b�N���j���[���J�����Ƃ�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReserveViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // �I�𒆃A�C�e�������邩�`�F�b�N���āA�������Disable
            for (int cnt = 0; cnt < reserveView.Items.Count; cnt++)
            {
                if (reserveView.Items[cnt].Selected)
                {
                    //ReserveViewRClick.Enabled = true;
                    ResvDelMenu.Enabled = true;
                    ResvIPCopyMenu.Enabled = true;
                    return;
                }
            }
            //ReserveViewRClick.Enabled = false;
            ResvDelMenu.Enabled = false;
            ResvIPCopyMenu.Enabled = false;
            return;
        }

        /// <summary>
        /// ���U�[�u�o�^�폜���j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResvDelMenu_Click(object sender, EventArgs e)
        {
            //��납�珇��ReserveItem���폜
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                lock (_k.Status.Gui.ReserveItem)
                {
                    for (int cnt = reserveView.Items.Count - 1; cnt >= 0; cnt--)
                    {
                        if (reserveView.Items[cnt].Selected)
                        {
                            ListViewItem _item = reserveView.Items[cnt];
                            // ReserveItem����폜
                            if (_k.Status.Gui.ReserveItem.Contains(_item))
                            {
                                _k.Status.Gui.ReserveItem.Remove(_item);
                                if (clmResvViewIP.DisplayIndex == 0)
                                    AuditReserve(_k, _item.Text);
                                else
                                    AuditReserve(_k, _item.SubItems[clmResvViewIP.DisplayIndex].Text);
                            }
                        }
                    }
                }
            }
            //GUI��̃��X�g��ReserveItem�ɂ��킹��
            myPort_StateChanged();
        }

        /// <summary>
        /// �Ώ�IP�̃��U�[�u�o�^��Ԃ����݂̐ڑ����ɂ��킹��
        /// </summary>
        /// <param name="_k"></param>
        /// <param name="_ip"></param>
        public void AuditReserve(Kagami _k, string _ip)
        {
            if (_k == null)
                return;

            //ClientView�̍X�V���s���̂�Reserve�o�^0�ł����s
            //if (_k.Status.Gui.ReserveItem.Count == 0)
            //    return;

            int r_cnt = 0; // ���U�[�u���X�g��ł̊Y��IP��
            int c_cnt = 0; // �N���C�A���g���X�g��ł̊Y��IP��

            lock (_k.Status.Gui.ReserveItem)
            lock (_k.Status.Gui.ClientItem)
            {
                // �Ώ�IP�����U�[�u���X�g�ɉ����݂��邩�`�F�b�N
                if (clmResvViewIP.DisplayIndex == 0)
                {
                    foreach (ListViewItem _item in _k.Status.Gui.ReserveItem)
                        if (_item.Text == _ip)
                            r_cnt++;
                }
                else
                {
                    foreach (ListViewItem _item in _k.Status.Gui.ReserveItem)
                        if (_item.SubItems[clmResvViewIP.DisplayIndex].Text == _ip)
                            r_cnt++;
                }
                

                // �Ώ�IP���N���C�A���g���X�g�ɉ����݂��邩�`�F�b�N
                // r_cnt�̒l���A��������_item��ԕ����ɂ���
                if (Front.clmCV_IP_IDX == 0)
                {
                    foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                        if (_item.Text == _ip)
                        {
                            c_cnt++;
                            if (r_cnt > 0)
                            {
                                r_cnt--;
                                _item.SubItems[0].ForeColor = System.Drawing.Color.LimeGreen;
                            }
                            else
                            {
                                _item.SubItems[0].ResetStyle();
                            }
                        }
                }
                else
                {
                    foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                        if (_item.SubItems[Front.clmCV_IP_IDX].Text == _ip)
                        {
                            c_cnt++;
                            if (r_cnt > 0)
                            {
                                r_cnt--;
                                _item.SubItems[0].ForeColor = System.Drawing.Color.LimeGreen;
                            }
                            else
                            {
                                _item.SubItems[0].ResetStyle();
                            }
                        }
                }
                

                // �Ώ�IP�̐ڑ��������ɐݒ肵�A
                // ����ȍ~�͑S�ā~�ɂ���
                foreach (ListViewItem _item in _k.Status.Gui.ReserveItem)
                {
                    if (_item.Text == _ip)  // clmResvViewIP
                    {
                        if (c_cnt > 0)
                        {
                            _item.SubItems[1].Text = "��";  // clmResvViewStatus
                            _item.SubItems[0].ForeColor = System.Drawing.Color.LimeGreen;
                            c_cnt--;
                        }
                        else
                        {
                            _item.SubItems[1].Text = "�~";  // clmResvViewStatus
                            _item.SubItems[0].ForeColor = System.Drawing.Color.Red;
                        }
                    }
                }
            }
            // ���łɃ��U�[�u�o�^�L���Kick�Ώۂɓo�^����Ă����������
            if (c_cnt > 0)
                lock (Front.KickList)
                    if (Front.KickList.ContainsKey(_ip))
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickDenyTime).ToString() + ",1";
        }

        #endregion

        #region �L�b�N�^�u
        /// <summary>
        /// �L�b�N���[�U���ǉ����ꂽ���ɌĂ΂��
        /// ���J���Ă�^�u���L�b�N�^�u�Ȃ烊�X�g�ɒǉ�
        /// kickView�Ǘ��X���b�h�ȊO����Ă΂ꂽ��A�X���b�h�ԒʐM����
        /// </summary>
        /// <param name="_ke"></param>
        void kickViewUpdate(KagamiEvent _ke)
        {
            if (kickView.InvokeRequired)
            {
                if (kickView.IsHandleCreated)
                {
                    //�R���g���[���Ǘ����X���b�h�ȊO�͂�����
                    //�R���g���[���Ǘ����X���b�h�ɔ񓯊�Invoke�ʐM�ōX�V�v��
                    EventHandlerDelegate _dlg = new EventHandlerDelegate(kickViewUpdate);
                    this.BeginInvoke(_dlg, new object[] { _ke });
                    //this.Invoke(_dlg, new object[] { _ke });
                }
            }
            else
            {
                // �R���g���[���Ǘ����X���b�h�͂�����
                // GUI��I�𒆂̃|�[�g���A�C�x���g�����|�[�g�Ɠ������`�F�b�N
                // GUI��I�𒆂̃^�u���A�L�b�N�^�u���`�F�b�N
                Kagami _k = this.SelectedKagami;
                if (_k != null && _k == _ke.Kagami)
                {
                    // kickView�ɖ��o�^�̃A�C�e���Œʒm���Ă�����V�K�o�^
                    // �o�^�ς݃A�C�e���̒ʒm�Ȃ�J�E���g���{�P�C���N�������g����
                    if (!kickView.Items.Contains(_ke.Item))
                    {
                        //�V�K�o�^
                        if (!kickView.IsDisposed)
                            kickView.Items.Add(_ke.Item);
                    }
                    else
                    {
                        //�J�E���^�C���N�������g
                        if (clmKickViewCount.DisplayIndex == 0)
                            _ke.Item.Text = (int.Parse(_ke.Item.Text) + 1).ToString();
                        else
                            _ke.Item.SubItems[clmKickViewCount.DisplayIndex].Text
                                = (int.Parse(_ke.Item.SubItems[clmKickViewCount.DisplayIndex].Text) + 1).ToString();
                    }
                }
            }
        }
        /// <summary>
        /// �L�b�N�ǉ��{�^���̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addKickBTN_Click(object sender, EventArgs e)
        {
            if (addKickHost.Text == "")
                return;

            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                MessageBox.Show("�|�[�g���N�����Ă���ǉ����Ă�������", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // ���X�g�ɓo�^���邾���ŁA�K���͊J�n���Ȃ��B
            try
            {
                //�L�b�N�ǉ�
                System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(addKickHost.Text)[0];
                _k.Status.AddKick(hostadd.ToString(),0);
                //�\���X�V
                addKickHost.Text = "";
                myPort_StateChanged();
            }
            catch
            {
                MessageBox.Show("�z�X�g������IP�A�h���X�ɕϊ��ł��܂���ł���", "DNS Error", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// �L�b�N�o�^�z�X�g���͗��ŃL�[���͂�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addKickHost_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter���͂Ȃ�ǉ��������s��
            if (e.KeyChar == (char)Keys.Enter)
            {
                addKickBTN_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        /// <summary>
        /// �L�b�N�^�u��ŉE�N���b�N���j���[���J�����Ƃ�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KickViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // �I�𒆃A�C�e�������邩�`�F�b�N���āA�������Disable
            // ���������ׂĉ����A���ׂĉ��������X�g�����͂��̂܂܁B
            for (int cnt = 0; cnt < kickView.Items.Count; cnt++)
            {
                if (kickView.Items[cnt].Selected)
                {
                    KickAddSubMenu.Enabled = true;
                    KickSelDelMenu.Enabled = true;
                    KickSelDelClearMenu.Enabled = true;
                    KickIPCopyMenu.Enabled = true;
                    return;
                }
            }
            KickAddSubMenu.Enabled = false;
            KickSelDelMenu.Enabled = false;
            KickSelDelClearMenu.Enabled = false;
            KickIPCopyMenu.Enabled = false;
            return;
        }

        /// <summary>
        /// �L�b�N�ǉ����j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KickAddMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;

            try
            {
                // Tag�v���p�e�B�ɃL�b�N���Ԃ��ݒ肳��Ă���
                int _deny_tim = int.Parse((string)(((ToolStripMenuItem)sender).Tag));
                // ��납�珇�ɒǉ�
                for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                {
                    if (kickView.Items[cnt].Selected == true)
                    {
                        if (clmKickViewIP.DisplayIndex == 0)
                            Front.AddKickUser(kickView.Items[cnt].Text, _deny_tim);
                        else
                            Front.AddKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.DisplayIndex].Text, _deny_tim);
                    }
                }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("KickAddMenu", "�����G���[ Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// �L�b�N�������j���[�̃N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KickDelMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;

            try
            {
                switch (((ToolStripMenuItem)sender).Name)
                {
                    case "KickSelDelMenu":  // �I�𒆂�Kick����
                        for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                            if (kickView.Items[cnt].Selected == true)
                                Front.DelKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.Index].Text);
                        break;

                    case "KickAllDelMenu":  // ���ׂĂ�Kick����
                        for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                            Front.DelKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.Index].Text);
                        break;

                    case "KickSelDelClearMenu": // �I�𒆂�Kick�����{�N���A
                        for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                        {
                            if (kickView.Items[cnt].Selected == true)
                            {
                                if (clmKickViewIP.DisplayIndex == 0)
                                    Front.DelKickUser(kickView.Items[cnt].Text);
                                else
                                    Front.DelKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.DisplayIndex].Text);
                                lock (_k.Status.Gui.KickItem)
                                {
                                    for (int cnt2 = _k.Status.Gui.KickItem.Count - 1; cnt2 >= 0; cnt2--)
                                    {
                                        if (clmKickViewIP.DisplayIndex == 0)
                                        {
                                            if (_k.Status.Gui.KickItem[cnt2].Text == kickView.Items[cnt].Text)
                                                _k.Status.Gui.KickItem.RemoveAt(cnt2);
                                        }
                                        else
                                        {
                                            if (_k.Status.Gui.KickItem[cnt2].SubItems[clmKickViewIP.DisplayIndex].Text == kickView.Items[cnt].SubItems[clmKickViewIP.DisplayIndex].Text)
                                                _k.Status.Gui.KickItem.RemoveAt(cnt2);
                                        }
                                    }
                                }
                                kickView.Items[cnt].Remove();
                            }
                        }
                        break;

                    case "KickAllDelClearMenu": // ���ׂĂ�Kick�����{�N���A
                        for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                        {
                            if (clmKickViewIP.DisplayIndex == 0)
                                Front.DelKickUser(kickView.Items[cnt].Text);
                            else
                                Front.DelKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.DisplayIndex].Text);
                        }
                        _k.Status.Gui.KickItem.Clear();
                        kickView.Items.Clear();
                        break;
                    default:
                        throw new Exception("not implemented.");
                }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("KickDelMenu", "�����G���[ Trace:" + ex.StackTrace);
            }
        }
        #endregion

        #region ���O�^�u
        /// <summary>
        /// logView�A�C�e���ǉ�
        /// logView�Ǘ��X���b�h�ȊO����Ă΂ꂽ��A�X���b�h�ԒʐM����
        /// </summary>
        /// <param name="_ke"></param>
        void logViewUpdate(KagamiEvent _ke)
        {
            if (logView.InvokeRequired)
            {
                //�R���g���[���Ǘ����X���b�h�ȊO�͂�����
                //�R���g���[���Ǘ����X���b�h�ɔ񓯊�Invoke�ʐM�ōX�V�v��
                if (logView.IsHandleCreated)
                {
                    try
                    {
                        EventHandlerDelegate _dlg = new EventHandlerDelegate(logViewUpdate);
                        this.BeginInvoke(_dlg, new object[] { _ke });
                        //this.Invoke(_dlg, new object[] { _ke });
                    }
                    catch { }
                }
            }
            else
            {
                // �R���g���[���Ǘ����X���b�h�͂�����
                // GUI��I�𒆂̃|�[�g���A�C�x���g�����|�[�g�Ɠ������`�F�b�N
                // GUI��I�𒆂̃^�u���A���O�^�u���`�F�b�N
                Kagami _k = this.SelectedKagami;
                if (_k == _ke.Kagami || _k == null)
                {
                    // ���O�\�����[�h����
                    // �Ƃ肠�����Q�p�^�[�������Ȃ��̂ŃR���ŁB�B
                    if (_ke.Mode >= Front.LogMode)
                    {
                        // ���O��ǉ�
                        try
                        {
                            if (!logView.IsDisposed)
                                logView.Items.Add(_ke.Item);
                        }
                        catch { }   // ��d�o�^NG�n�͖���
                        // �����X�N���[��
                        if (logView.Items.Count != 0 && this.logAutoScroll.Checked)
                            logView.EnsureVisible(logView.Items.Count - 1);
                    }
                }
            }
        }
        /// <summary>
        /// ���O�o�̓��[�h�ύX
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logMode_CheckedChanged(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;

            if (logModeAll.Checked)
                Front.LogMode = 0;
            else
                Front.LogMode = 1;
            // ���O���e�̕\���ؑ�
            if (_k != null)
            {
                logView.BeginUpdate();
                logView.Items.Clear();
                if (logModeAll.Checked)
                    logView.Items.AddRange(_k.Status.Gui.LogAllItem.ToArray());
                else
                    logView.Items.AddRange(_k.Status.Gui.LogImpItem.ToArray());
                // �����X�N���[��
                if (logView.Items.Count != 0 && this.logAutoScroll.Checked)
                    logView.EnsureVisible(logView.Items.Count - 1);
                logView.EndUpdate();
            }
        }
        /// <summary>
        /// ���O�^�u�ŉE�N���b�N���j���[���J�����Ƃ��Ă��鎞
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // ���ɉ����������j���[��\��
        }
        /// <summary>
        /// �E�N���b�N���j���[�Łu���̃|�[�g�̃��O�N���A�v��I��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogClearMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.Gui.LogAllItem.Clear();
                _k.Status.Gui.LogImpItem.Clear();
                logView.Items.Clear();
            }
        }
        /// <summary>
        /// �E�N���b�N���j���[�Łu�S�|�[�g�̃��O�N���A�v��I��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogClearAllMenu_Click(object sender, EventArgs e)
        {
            foreach (Kagami _k in Front.KagamiList)
            {
                lock (_k.Status.Gui)
                {
                    _k.Status.Gui.LogAllItem.Clear();
                    _k.Status.Gui.LogImpItem.Clear();
                }
            }
            logView.Items.Clear();
        }

        #endregion

        #region �X�P�W���[���N���{�����ĕ`��{�����I���Ď�
        /// <summary>
        /// �����ĕ`��{�����I���Ď��{�X�P�W���[���N��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            int _tmp_port_num = 0;
            Kagami _k;

            #region ���p�l���̍X�V
            // kagamiView�X�V/�㕔�{�^��AUDIT/�X�e�[�^�X�o�[AUDIT��LeftFlag��ON�̎��̂ݍs��
            if (LeftFlag)
            {
                LeftFlag = false;
                #region kagamiView�̍X�V
                kagamiView.BeginUpdate();
                kagamiView.Items.Clear();
                lock (Front.KagamiList)
                {
                    foreach (Kagami _k_tmp in Front.KagamiList)
                    {
                        // ImportURL���Đݒ�
                        if (clmKgmViewImport.DisplayIndex == 0)
                            _k_tmp.Status.Gui.KagamiItem.Text = _k_tmp.Status.ImportURL;
                        else
                            _k_tmp.Status.Gui.KagamiItem.SubItems[clmKgmViewImport.DisplayIndex].Text = _k_tmp.Status.ImportURL;

                        // �|�[�g���ڑ��������Ȃ�|�[�g�ԍ���ԕ����ɂ���
                        if (_k_tmp.Status.Pause)
                            _k_tmp.Status.Gui.KagamiItem.SubItems[clmKgmViewPort.DisplayIndex].ForeColor = System.Drawing.Color.Red;
                        else
                            _k_tmp.Status.Gui.KagamiItem.SubItems[clmKgmViewPort.DisplayIndex].ResetStyle();

                        // �ҋ@���|�[�g���ш搧����~���͍ő�ڑ��������[�U��`�l�ɍĐݒ�
                        if (!_k_tmp.Status.ImportStatus || BandTh == null || !BandTh.IsAlive)
                            _k_tmp.Status.Connection = _k_tmp.Status.Conn_UserSet;

                        // �ڑ��Ґ����Đݒ�
                        if (clmKgmViewConn.DisplayIndex == 0)
                            _k_tmp.Status.Gui.KagamiItem.Text = _k_tmp.Status.Client.Count + "/" + _k_tmp.Status.Connection + "+" + _k_tmp.Status.Reserve;
                        else
                            _k_tmp.Status.Gui.KagamiItem.SubItems[clmKgmViewConn.DisplayIndex].Text = _k_tmp.Status.Client.Count + "/" + _k_tmp.Status.Connection + "+" + _k_tmp.Status.Reserve;

                        kagamiView.Items.Add(_k_tmp.Status.Gui.KagamiItem);
                        if (_k_tmp.Status.ImportURL != "�ҋ@��")
                            _tmp_port_num++;
                    }
                }
                kagamiView.EndUpdate();
                #endregion

                // �ڑ��E�ؒf�{�^�����AUDIT
                ButtonAudit();

                // Web�ォ��ݒ肪�ύX���ꂽ�ꍇ�p
                // Pause�{�^�� & �ш搧���{�^��OnOff���AUDIT
                statusBarAudit();

                // �C���|�[�g�ڑ���(�N���C�A���g������Ȃ���)���ω�������TaskTrayTip�\��
                if (LastActPortNum != _tmp_port_num)
                {
                    LastActPortNum = _tmp_port_num;
                    if (TaskTrayIcon.Visible)
                        showTaskTrayTip();
                }
            }// end of if(LeftFlag)

            // monAllView�͏�ɍX�V
            monAllViewUpdate();

            // StatusBar��EX,IM,CPU�g�p������ɍX�V
            statusBarUpdate();
            #endregion

            #region �E�p�l���̍X�V
            // RightPanel�͑I�𒆃^�u���̃t�H�[���̂ݍX�V
            _k = this.SelectedKagami;
            if (_k != null)
            {// �I�𒆃|�[�g�ŋ��N����
                switch (tabKagami.SelectedTab.Text)
                {
                    case "���j�^":
                        monViewUpdate(_k);
                        break;
                    case "�N���C�A���g":
                        if (_k.Status.Gui.ClientItem.Count > 0)
                        {
                            clientView.BeginUpdate();
                            _k.Status.Client.UpdateClientTime();
                            clientView.EndUpdate();
                        }
                        break;
                    case "���U�[�u":
                        break;
                    case "�L�b�N":
                        if (_k.Status.Gui.KickItem.Count > 0)
                        {
                            kickView.BeginUpdate();
                            _k.Status.Client.UpdateKickTime();
                            kickView.EndUpdate();
                        }
                        break;
                    case "���O":
                        break;
                    default:
                        throw new Exception("not implemented.");
                }
            }
            else
            {// �I�𒆃|�[�g�ŋ����N��
            }
            #endregion

            #region �A�v���P�[�V�����̎����I��
            if (EnableAutoExit && LastActPortNum == 0)
            {
                //TaskTray���畜�A
                this.Visible = true;
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.TaskTrayIcon.Visible = false;
                AskFormClose = false;
                timer1.Stop();
                this.Close();
                return;
            }
            #endregion

            #region �����V���b�g�_�E��
            if (EnableAutoShutdown && LastActPortNum == 0 && ExecShutdown == false)
            {
                //TaskTray���畜�A
                this.Visible = true;
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.TaskTrayIcon.Visible = false;
                //shutdown���~�p�{�^���ɕύX
                toolStripAutoShutdown.BorderStyle = Border3DStyle.Raised; // �ʃ{�^��
                toolStripAutoShutdown.Text = "�d�f���~";
                ExecShutdown = true;
                AskFormClose = false;
                //Shutdown.exe�N��
                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = "shutdown.exe";
                psInfo.CreateNoWindow = true;
                psInfo.UseShellExecute = false;
                psInfo.Arguments = "-s -f -c \"�����݂�2����̃V���b�g�_�E���v��...\"";
                try
                {
                    Process.Start(psInfo);
                }
                catch
                {
                    MessageBox.Show("shutdown.exe�̎��s�Ɏ��s���܂���.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //shutdown�J�n�p�{�^���ɕύX
                    toolStripAutoShutdown.BorderStyle = Border3DStyle.Raised;
                    toolStripAutoShutdown.Text = "�����d�f";
                    ExecShutdown = false;
                    EnableAutoShutdown = false;
                    AskFormClose = true;
                    statusBarAudit();
                }
            }
            #endregion

            #region �����ԃC���|�[�g�����ؒf�{���N���C�A���g�������ؒf
            if (Front.Acl.ImportOutTime > 0 || Front.Acl.ClientOutCheck)
            {
                bool empty_port = false; // �󂫃|�[�g�Ȃ�
                lock (Front.KagamiList)
                {
                    foreach (Kagami _k_tmp in Front.KagamiList)
                    {
                        // ����ɐڑ��ł��Ă��Ȃ��|�[�g�͑ΏۊO
                        if (!_k_tmp.Status.ImportStatus)
                        {
                            empty_port = true; // �󂫃|�[�g�L��
                            continue;
                        }

                        // �����ڑ��͑ΏۊO
                        if (_k_tmp.Status.Type == 0)
                            continue;

                        //Import�������ԃI�[�o�[
                        if (Front.Acl.ImportOutTime > 0 && _k_tmp.Status.ImportTime.TotalMinutes >= Front.Acl.ImportOutTime)
                        {
                            if (!Front.Acl.PortFullOnlyCheck || !empty_port)
                            {
                                //�C���|�[�g�ؒf
                                _k_tmp.Status.Disc();
                                Front.AddLogData(1, _k_tmp.Status, "�����ԃC���|�[�g�ڑ��̐������ԂɒB�������ߊO���Ҏ��Ԃɖ߂�܂�(�ڑ�����:" + Front.Acl.ImportOutTime + "��)");
                            }
                        }

                        //Client�����ݒ萔��葽���Ȃ����玞�ԃ��Z�b�g
                        if (_k_tmp.Status.Client.Count > Front.Acl.ClientOutNum)
                        {
                            //�ݒ��IP���܂܂Ȃ��ݒ�ɂȂ��Ă���ꍇ
                            if (Front.Acl.ClientNotIPCheck)
                            {
                                int num = 0;
                                lock (_k_tmp.Status.Gui.ClientItem)
                                {
                                    for (int cnt = 0; cnt < _k_tmp.Status.Gui.ClientItem.Count; cnt++)
                                    {
                                        if (_k_tmp.Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_IP_IDX].Text == _k_tmp.Status.SetUserIP)
                                            num++;
                                    }
                                }
                                if (_k_tmp.Status.Client.Count - num > Front.Acl.ClientOutNum)
                                    _k_tmp.Status.ClientTime = DateTime.Now;
                            }
                            else
                            {
                                _k_tmp.Status.ClientTime = DateTime.Now;
                            }
                        }
                        //���Ԃ��ݒ莞�Ԃ��z����Ƒ҂��󂯂ɖ߂�
                        if (Front.Acl.ClientOutCheck && (DateTime.Now - _k_tmp.Status.ClientTime).TotalMinutes >= Front.Acl.ClientOutTime)
                        {
                            if (!Front.Acl.PortFullOnlyCheck || !empty_port)
                            {
                                //�C���|�[�g�ؒf
                                _k_tmp.Status.Disc();
                                Front.AddLogData(1, _k_tmp.Status, "�N���C�A���g�������̐������ԂɒB�������ߊO����t��Ԃɖ߂�܂�(" + Front.Acl.ClientOutNum + "�l�ȉ�/" + Front.Acl.ClientOutTime + "��)");
                            }
                        }
                    }
                }
            }
            /* */
            #endregion

            #region �X�P�W���[���C�x���g
            string _time = DateTime.Now.ToString("HH:mm");
            bool _time_chg = false;
            int _week = (int)DateTime.Now.DayOfWeek;
            if (Time != _time)
            {
                Time = _time;
                _time_chg = true;
                // ����00:00(=���t���ς�����Ƃ�)
                if (_time == "00:00")
                {
                    // ���ԃg���q�b�N�ɑޔ����ď��N���A
                    Front.Log.TrsUpMon += Front.TotalUP;
                    Front.Log.TrsDlMon += Front.TotalDL;
                    Front.TotalUP = 0;
                    Front.TotalDL = 0;
                    Front.Log.TrsUpDay = 0;
                    Front.Log.TrsDlDay = 0;
                    if (DateTime.Now.Day == 1)
                    {
                        // ����1����00:00
                        Front.Log.TrsUpMon = 0;
                        Front.Log.TrsDlMon = 0;
                    }
                    // �]���ʎw��̃C�x���g���s�ς݃t���O���N���A����
                    foreach (SCHEDULE _item in Front.ScheduleItem)
                    {
                        if (_item.StartType == 1)
                        {
                            switch (_item.TrfType)
                            {
                                case 0:
                                    _item.ExecTrf = false;
                                    break;
                                case 1:
                                default:
                                    if (DateTime.Now.Day == 1)
                                        _item.ExecTrf = false;
                                    break;
                            }
                        }
                    }
                    // ����0���ɐݒ�t�@�C�������ۑ�
                    Front.SaveSetting();
                }
            }
            
            // �X�P�W���[���C�x���g�̑��`�F�b�N
            foreach (SCHEDULE _item in Front.ScheduleItem)
            {
                // �����X�P�W���[���C�x���g��Skip
                if (_item.Enable == false)
                    continue;

                // �X�P�W���[���C�x���g�^�C�v�Ń`�F�b�N���e����
                switch (_item.StartType)
                {
                    case 0: // �C�x���g�^�C�v=���Ԏw��
                        // ���ԕω����Ă��Ȃ����Skip
                        if (_time_chg)
                            continue;
                        // ���ԕs��v��Skip
                        string _start_time = _item.Hour.ToString("D2") + ":" + _item.Min.ToString("D2");
                        if (Time != _start_time)
                            continue;
                        // �j���`�F�b�N
                        // �P��j���w��ŗj���s��v��Skip
                        if (_item.Week < 7 && _item.Week != _week)
                            continue;
                        // �����w��œy����Skip
                        if (_item.Week == 8 && (_week == 0 || _week >= 6))
                            continue;
                        // �y���w��œy���ȊO��Skip
                        if (_item.Week == 9 && _week != 0 && _week != 6)
                            continue;
                        break;

                    case 1: // �C�x���g�^�C�v=�]���ʎw��
                        ulong _trf = (ulong)(_item.TrfValue * (_item.TrfUnit == 0 ? 1000 * 1000 : 1000 * 1000 * 1000));
                        // �C�x���g���s�ς݂Ȃ�Skip
                        if (_item.ExecTrf)
                            continue;
                        switch (_item.TrfType)
                        {
                            case 0: // ���ԓ]����
                                // ���ԓ]���ʂ��X�P�W���[���ݒ�l��菬�������Skip
                                if (_trf > (Front.Log.TrsUpDay + Front.TotalUP))
                                    continue;
                                break;
                            case 1: // ���ԓ]����
                                // ���ԓ]���ʂ��X�P�W���[���ݒ�l��菬�������Skip
                                if (_trf > (Front.Log.TrsUpMon + Front.TotalUP))
                                    continue;
                                break;
                            default:
                                continue;
                        }
                        _item.ExecTrf = true;
                        break;

                    default:
                        continue;
                }
                //
                // �X�P�W���[���Ɉ�v����A�C�e���𔭌�
                //

                List<int> _port_list = new List<int>();
                if (_item.Port == 0)
                    foreach (string s in myPort.Items)
                        _port_list.Add(int.Parse(s));
                else
                    _port_list.Add((int)_item.Port);

                switch (Front.ScheduleEventString[_item.Event])
                {
                    case "�G���g�����X�N��":
                        if (Front.HPStart == false)
                            toolStripHPStart_Click(sender, EventArgs.Empty);
                        break;

                    case "�G���g�����X��~":
                        if (Front.HPStart == true)
                            toolStripHPStart_Click(sender, EventArgs.Empty);
                        break;

                    case "�V�K��t�J�n":
                        Front.Pause = false;
                        break;

                    case "�V�K��t��~":
                        Front.Pause = true;
                        break;

                    case "�����ؒf":
                        foreach (int _port in _port_list)
                        {
                            // �w��|�[�g�ؒf
                            _k = Front.IndexOf(_port);
                            if (_k != null)
                            {
                                _k.Status.Disc();
                            }
                        }
                        break;

                    case "�|�[�g�Ҏ�J�n":
                        foreach (int _port in _port_list)
                        {
                            _k = Front.IndexOf(_port);
                            if (_k == null)
                            {
                                // ���N���|�[�g���N��
                                _k = new Kagami("", (int)_port, (int)_item.Conn, (int)_item.Resv);
                                // �ш搧���N�����̏ꍇ�Astatus�̒ǉ��ݒ�
                                if (Front.BndWth.EnableBandWidth)
                                {
                                    _k.Status.GUILimitUPSpeed = (int)Front.BndWth.BandStopValue;
                                    _k.Status.LimitUPSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                                }
                                Front.Add(_k);
                            }
                        }
                        break;

                    case "�|�[�g�Ҏ��~":
                        foreach (int _port in _port_list)
                        {
                            _k = Front.IndexOf(_port);
                            if (_k != null)
                            {
                                // �w��|�[�g���N�����Ȃ̂ŁA��~����
                                _k.Status.RunStatus = false;
                            }
                        }
                        break;

                    case "�ڑ��g���ύX":
                        foreach (int _port in _port_list)
                        {
                            _k = Front.IndexOf(_port);
                            if (_k != null)
                            {
                                _k.Status.Conn_UserSet = (int)_item.Conn;
                                _k.Status.Reserve = (int)_item.Resv;
                            }
                        }
                        break;

                    default:
                        throw new Exception("not implemented.");
                }
                // �S�X�P�W���[��������
                // �X�e�[�^�X�o�[��AUDIT���{
                statusBarAudit();
            }
            #endregion

#if OVERLOAD
            //�G�N�X�|�[�g�ڑ��ߕ��׎����c�[��
            _k = this.SelectedKagami;
            timer1.Interval = 300;
            if (_k != null && _k.Status.ImportStatus)
            {
                overload_cnt++;
                if (overload_cnt > 1000) overload_cnt = 1;
                Status _status = new Status(null, _k.Status.ImportURL, _k.Status.MyPort+overload_cnt, _k.Status.Conn_UserSet, _k.Status.Reserve);
                Import _import = new Import(_status);

                Thread _th = new Thread(overload);
                _th.Start((object)_status);
            }
#endif

        }//end of timer1_Tick
        #endregion

        #region �ш搧��
        /// <summary>
        /// �ш搧���J�n
        /// </summary>
        private void StartBandWidth()
        {
            if (BandTh != null)
                return; // ��d�N���K�[�h
            try
            {
                Front.AddLogDebug("�ш搧����ԕύX", "�ш搧���^�X�N���J�n���܂�");
                BandTh = new Thread(BandWidth);
                BandTh.Name = "BandWidth";
                BandTh.Start();
            }
            catch
            {
                MessageBox.Show("�ш搧���^�X�N���J�n�ł��܂���", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Front.BndWth.EnableBandWidth = false;
                statusBarAudit();
                return;
            }
        }
        /// <summary>
        /// �ш搧����~
        /// </summary>
        private void StopBandWidth()
        {
            if (BandTh == null)
                return; // ��d��~�K�[�h

            try
            {
                Front.AddLogDebug("�ш搧����ԕύX", "�ш搧���^�X�N���~���܂�");
                BandTh.Abort();
                BandTh = null;
            }
            catch
            {
                MessageBox.Show("�ш搧���^�X�N����~�ł��܂���", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Front.BndWth.EnableBandWidth = true;
                statusBarAudit();
                return;
            }
            //���X�̍ő�ڑ����ɖ߂�
            lock (Front.KagamiList)
                foreach (Kagami _k in Front.KagamiList)
                    _k.Status.Connection = _k.Status.Conn_UserSet;
            LeftFlag = true;
        }
        /// <summary>
        /// �ш搧���^�X�N
        /// </summary>
        private void BandWidth()
        {
            int TotalClient = 0;
            int LimitSpeed = 0;
            Front.AddLogDebug("�ш搧��", "�ш搧�����J�n���܂�");
            while (true)
            {
                try
                {
                    Front.AddLogDebug("�ш搧��", "---�ш搧���v�Z�J�n---");
                    switch (Front.BndWth.BandStopMode)
                    {
                        case 0: // �S�|�[�g���v�ł̐����l�w��
                            // �e�|�[�g�̐ڑ��\���䗦�ŕ��z����
                            TotalClient = 0;
                            LimitSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                            //�S�|�[�g�̐ڑ��\�����v�Z
                            lock (Front.KagamiList)
                            {
                                foreach (Kagami _k in Front.KagamiList)
                                {
                                    if (Front.BndWth.BandStopResv)
                                        TotalClient += _k.Status.Conn_UserSet + _k.Status.Reserve;
                                    else
                                        TotalClient += _k.Status.Conn_UserSet;
                                }
                            }
                            Front.AddLogDebug("�ш搧��", "UP�ш����l=" + LimitSpeed + "Kbps");
                            Front.AddLogDebug("�ш搧��", "�S�̂̐ڑ��\��=" + TotalClient);
                            //�e���ɐ����ш�l���Z�b�g
                            lock (Front.KagamiList)
                            {
                                foreach (Kagami _k in Front.KagamiList)
                                {
                                    if (Front.BndWth.BandStopResv)
                                    {
                                        _k.Status.LimitUPSpeed = LimitSpeed * (_k.Status.Conn_UserSet + _k.Status.Reserve) / TotalClient;
                                    }
                                    else
                                        _k.Status.LimitUPSpeed = LimitSpeed * _k.Status.Conn_UserSet / TotalClient;
                                }
                            }
                            break;
                        case 1: // �P�|�[�g�P�ʂł̐����l�w��
                            //�e���ɐ����ш�l���Z�b�g
                            LimitSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                            lock (Front.KagamiList)
                                foreach (Kagami _k in Front.KagamiList)
                                    _k.Status.LimitUPSpeed = LimitSpeed;
                            break;
                        case 2: // �|�[�g���Ɍʎw�肷��
                            // LimitUPSpeed�ݒ�ς�
                            break;
                        default:
                            break;
                    }

                    lock (Front.KagamiList)
                    {
                        //�����ш�l�����ɍő�ڑ��l�����v�Z���Z�b�g����
                        //�Z�b�g���ꂽ�l�́AKbps�P�ʂŌv�Z����Ă��܂�
                        //�ш�ɗ]�T�������Ă��AGUI��Őݒ肵���ʏ�ڑ��ő吔�͒����Ȃ��悤�ɐݒ肷��B
                        foreach (Kagami _k in Front.KagamiList)
                        {
                            Front.AddLogDebug("�ш搧��", "PORT=" + _k.Status.MyPort + "/PORT��UP�ш����l=" + _k.Status.LimitUPSpeed + "kbps");
                            if (_k.Status.MaxDLSpeed == 0 && _k.Status.CurrentDLSpeed == 0)
                            {
                                // �\���ш�������l���擾�ł��Ȃ��ꍇ�͍ő�l���ݒ�
                                _k.Status.Connection = _k.Status.Conn_UserSet;
                                Front.AddLogDebug("�ш搧��", "PORT=" + _k.Status.MyPort + "/DL�ш�0�̈׃��[�U�l���g�p/conn=inf/user=" + _k.Status.Conn_UserSet);
                            }
                            else
                            {
                                int conn_old = _k.Status.Connection;
                                int conn = 0;
                                if ((_k.Status.MaxDLSpeed == 0) || (_k.Status.AverageDLSpeed * 0.9 > _k.Status.MaxDLSpeed))
                                {
                                    // �C���|�[�g�ڑ����ɐ\�����x���擾�o���Ȃ������ꍇ�A�܂���
                                    // �����l���ϑ��x�~0.9���\�����x�̏ꍇ�A�����l�Ő�������
                                    conn = (int)((_k.Status.LimitUPSpeed * 0.9) / _k.Status.AverageDLSpeed);//0.9�͗]�T�l
                                }
                                else
                                {
                                    // �C���|�[�g�ڑ����ɐ\�������ő�ш悪�g����ꍇ�A�ő�ш�Ő�������
                                    conn = (int)((_k.Status.LimitUPSpeed * 0.9) / _k.Status.MaxDLSpeed);//0.9�͗]�T�l
                                }
                                // ���U�[�u�l���̗L��
                                if (Front.BndWth.BandStopResv)
                                    conn -= _k.Status.Reserve;
                                if (conn < 0) { conn = 0; }

                                if (conn < _k.Status.Conn_UserSet)
                                {
                                    _k.Status.Connection = conn;
                                    Front.AddLogDebug("�ш搧��", "PORT=" + _k.Status.MyPort + "/�����l�ɒB�����׎����v�Z�l���g�p/conn=" + conn + "/user=" + _k.Status.Conn_UserSet);
                                }
                                else
                                {
                                    _k.Status.Connection = _k.Status.Conn_UserSet;
                                    Front.AddLogDebug("�ш搧��", "PORT=" + _k.Status.MyPort + "/�����l�ɒB���Ȃ��׃��[�U�l���g�p/conn=" + conn + "/user=" + _k.Status.Conn_UserSet);
                                }
                                if (conn != conn_old)
                                    LeftFlag = true;
                            }
                        }
                    }
                    Front.AddLogDebug("�ш搧��", "---�ш搧���v�Z�I��---");
                    // �����Čv�Z�t���O�������A10�b�o�߂���܂ő҂�
                    for (int cnt = 0; !BandFlag && cnt < 10; cnt++)
                        Thread.Sleep(1000);
                    BandFlag = false;
                }
                catch (ThreadAbortException e)
                {
                    Front.AddLogDebug("�ш搧��", "�ш搧�����I�����܂�:" + e.Message);
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("�ш搧��", "�ш搧���X���b�h���ŗ�O����:" + e.Message + "/Trace:" + e.StackTrace + ")");
                }
            }
        }
        #endregion

#if OVERLOAD
        /// <summary>
        /// �G�N�X�|�[�g�ڑ��ߕ��׎����c�[��
        /// </summary>
        private void overload(object obj)
        {
            Status _status = (Status)obj;
            while (_status.ImportStatus == false)
                Thread.Sleep(1000);

            // 10�b�����Ń����_���ҋ@
            Random _ran = new Random();
            Thread.Sleep(_ran.Next(1000, 10000));

            _status.Disc();
        }
#endif
    }
}