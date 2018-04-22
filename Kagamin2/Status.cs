using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Drawing;

namespace Kagamin2
{
    /// <summary>
    /// �����ɍ쐬�����f�[�^�N���X
    /// �C���|�[�g�^�X�N�ƃG�N�X�|�[�g�^�X�N�̗�������Q�Ƃ����f�[�^
    /// </summary>
    public class Status
    {
        #region �����o�ϐ�

        /// <summary>
        /// �Ǘ���Kagami�N���X
        /// </summary>
        public Kagami Kagami;
        /// <summary>
        /// Import�ڑ���z�X�g���FPORT�ԍ�
        /// </summary>
        public string ImportURL;

        /// <summary>
        /// Import��z�X�g��
        /// </summary>
        public string ImportHost
        {
            get
            {
                string ImportHost = new Uri(ImportURL).DnsSafeHost;
                return Dns.GetHostAddresses(ImportHost)[0].ToString();
            }
        }

        /// <summary>
        /// Import��Port�ԍ�
        /// </summary>
        public int ImportPort
        {
            get
            {
                return new Uri(ImportURL).Port;
            }
        }

        /// <summary>
        /// ���p���ҋ@Port�ԍ�
        /// </summary>
        public int MyPort;

        /// <summary>
        /// �ʏ�ڑ��ő吔
        /// </summary>
        public int Connection;

        /// <summary>
        /// ���[�U��GUI�ォ��ݒ肵���ʏ�ڑ��ő吔
        /// </summary>
        public int Conn_UserSet;

        /// <summary>
        /// ���U�[�u�ڑ��ő吔
        /// </summary>
        public int Reserve;

        /// <summary>
        /// ���̋��ɐڑ����Ă���N���C�A���g�f�[�^
        /// </summary>
        public Client Client;

        /// <summary>
        /// ����/�O���ڑ����
        /// 0=���� 1=�O�� 2=Push�z�M
        /// </summary>
        public int Type;

        /// <summary>
        /// 0:pull�L��
        /// 1:pull����(push only)
        /// </summary>
        public bool DisablePull;

        /// <summary>
        /// �|�[�gListen��ԃt���O
        /// Push�z�M�|�[�g��Export�|�[�g�̐ؑւŗ��p
        /// </summary>
        public bool ListenPort;

        /// <summary>
        /// ���N����(�O���ڑ��҂����܂�)��true��ێ��B
        /// �����~����������false�ɂ���
        /// </summary>
        public bool RunStatus = true;

        /// <summary>
        /// Import�\�[�X�𐳏�Ɏ�M���Ă����true
        /// </summary>
        public bool ImportStatus = false;

        /// <summary>
        /// �f�[�^��M�o�C�g��
        /// (�R�b���Ƀ��Z�b�g)
        /// </summary>
        public int RecvByte = 0;

        #region GUI�֘A�����o
        public class GUI
        {
            /// <summary>
            /// kagamiView�o�͗p��Item
            /// </summary>
            public ListViewItem KagamiItem = new ListViewItem();

            /// <summary>
            /// clientView�o�͗p�̓������X�g
            /// </summary>
            public List<ListViewItem> ClientItem = new List<ListViewItem>();

            /// <summary>
            /// reserveView�o�͗p�̓������X�g
            /// </summary>
            public List<ListViewItem> ReserveItem = new List<ListViewItem>();

            /// <summary>
            /// kickView�o�͗p�̓������X�g
            /// </summary>
            public List<ListViewItem> KickItem = new List<ListViewItem>();

            /// <summary>
            /// logView�o�͗p�̓������X�g(�S���O�\���p)
            /// </summary>
            public List<ListViewItem> LogAllItem = new List<ListViewItem>();

            /// <summary>
            /// logView�o�͗p�̓������X�g(�d�v���O�̂ݕ\���p)
            /// </summary>
            public List<ListViewItem> LogImpItem = new List<ListViewItem>();

        }
        public GUI Gui = new GUI();
        #endregion

        /*
         * Web Form���ێ������o
         */
        /// <summary>
        /// URL�\��ON/OFF
        /// </summary>
        public bool UrlVisible = true;
        /// <summary>
        /// �G���g�����X�ݒ�p�p�X���[�h
        /// </summary>
        public string Password = "";
        /// <summary>
        /// �G���g�����X�\���p�R�����g
        /// </summary>
        public string Comment = "";
        /// <summary>
        /// �O���ڑ����s�����l��IP�A�h���X
        /// </summary>
        public string SetUserIP = "";
        /// <summary>
        /// �����X���b�hURL
        /// </summary>
        public string Url = "";
        /// <summary>
        /// �q���ւ̃��_�C���N�g���e�t���O
        /// </summary>
        public bool EnableRedirectChild = false;
        /// <summary>
        /// �e�ւ̃��_�C���N�g���e�t���O
        /// </summary>
        public bool EnableRedirectParent = false;
        /// <summary>
        /// �V�K�ڑ������t���O(�|�[�g��)
        /// </summary>
        public bool Pause = false;

        /// <summary>
        /// �w�b�_�擾����/HTTP1.0
        /// </summary>
        public byte[] HeadRspMsg10 = null;
        /// <summary>
        /// �w�b�_�X�g���[�����
        /// </summary>
        public byte[] HeadStream = null;
        /// <summary>
        /// �f�[�^�擾����/HTTP1.0
        /// </summary>
        public byte[] DataRspMsg10 = null;
        /// <summary>
        /// StreamSwitchCount
        /// </summary>
        public int StreamSwitchCount = 0;
        /// <summary>
        /// StreamType
        /// �z��v�f�ԍ����X�g���[���ԍ�-1
        /// �l��0����Audio,1����Video,2���Ƃ��̑�������
        /// </summary>
        public int[] StreamType;
        /// <summary>
        /// StreamBitrate
        /// �z��v�f�ԍ����X�g���[���ԍ�-1
        /// �l�̓r�b�g���[�g(bps)
        /// </summary>
        public int[] StreamBitrate;
        /// <summary>
        /// ���̃C���|�[�g�ڑ��ŗ��p����
        /// Audio�X�g���[���̃X�g���[���ԍ�
        /// </summary>
        public int SelectedAudioRecord = 0;
        /// <summary>
        /// ���̃C���|�[�g�ڑ��ŗ��p����
        /// Video�X�g���[���̃X�g���[���ԍ�
        /// </summary>
        public int SelectedVideoRecord = 0;

        /*
         * ���v���
         */
        /// <summary>
        /// �r�W�[�J�E���^�[
        /// </summary>
        public int BusyCounter = 0;
        /// <summary>
        /// �C���|�[�g�G���[�J�E���^
        /// </summary>
        public int ImportError = 0;
        /// <summary>
        /// ����̃C���|�[�g�G���[�̓��e
        /// </summary>
        public string ImportErrorContext = "";
        /// <summary>
        /// �G�N�X�|�[�g�G���[�J�E���^
        /// </summary>
        public int ExportError = 0;
        /// <summary>
        /// �G�N�X�|�[�g�ڑ���
        /// </summary>
        public int ExportCount = 0;
        /// <summary>
        /// �C���|�[�g�ڑ����g���C�񐔃J�E���^
        /// </summary>
        public int RetryCounter = 0;
        /// <summary>
        /// �C���|�[�g�^�X�N�N������
        /// </summary>
        public DateTime ImportStartTime = DateTime.Now;
        /// <summary>
        /// �C���|�[�g�^�X�N�N������
        /// </summary>
        public TimeSpan ImportTime
        {
            get
            {
                return DateTime.Now - ImportStartTime;
            }
        }
        /// <summary>
        /// �C���|�[�g�^�X�N�N������(������)
        /// </summary>
        public string ImportTimeString
        {
            get
            {
                TimeSpan _duration = DateTime.Now - ImportStartTime;
                return String.Format("{0:D2}:{1:D2}:{2:D2}", _duration.Hours, _duration.Minutes, _duration.Seconds);
            }
        }
        /// <summary>
        /// �N���C�A���g�������C���|�[�g�ؒf�p
        /// �N���C�A���g������l�𖞂����Ă����ŏI����
        /// </summary>
        public DateTime ClientTime = DateTime.Now;

        /*
         * �ш搧��
         */
        /// <summary>
        /// �����ш�l(�ʐݒ莞��GUI�ݒ�l)
        /// </summary>
        public int GUILimitUPSpeed = 0;
        /// <summary>
        /// �����ш�n(kbps)
        /// </summary>
        public int LimitUPSpeed = 0;

        /*
         * �]���ʁE�]�����x���
         */
        /// <summary>
        /// ���݂̉���]�����x�����l(Kbps)
        /// </summary>
        public int CurrentDLSpeed = 0;
        /// <summary>
        /// �C���|�[�g�ڑ��ォ��̕��ω���]�����x�����l(Kbps)
        /// </summary>
        public int AverageDLSpeed = 0;
        /// <summary>
        /// �g���t�B�b�N���W��(AveDLSpeed�v�Z�p)
        /// </summary>
        public int TrafficCount = 0;
        /// <summary>
        /// �C���|�[�g���\���̍ő�ш�(Kbps)
        /// </summary>
        public int MaxDLSpeed = 0;
        /// <summary>
        /// ���݂̐ڑ��ł̃G�N�X�|�[�g��ւ̍��vUpSize
        /// </summary>
        public ulong TotalUPSize = 0;
        /// <summary>
        /// ���݂̐ڑ��ł̃C���|�[�g������̍��vDownSize
        /// </summary>
        public ulong TotalDLSize = 0;

        #endregion

        #region �R���X�g���N�^
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="_kagami"></param>
        /// <param name="_importURL"></param>
        /// <param name="_myPort"></param>
        /// <param name="_connection"></param>
        /// <param name="_reserve"></param>
        public Status(Kagami _kagami, string _importURL, int _myPort, int _connection, int _reserve)
        {
            Kagami = _kagami;
            ImportURL = _importURL;
            ImportURL.ToLower();
            MyPort = _myPort;
            Conn_UserSet = _connection;
            Connection = _connection;
            Reserve = _reserve;
            DisablePull = false;

            // ImportURL����Ȃ�O���ڑ����[�h�ŋN��
            if (ImportURL == "")
            {
                ImportURL = "�ҋ@��";
                if (Front.Hp.UseHP)
                {
                    Type = 1; // �O���ڑ�
                }
                else if (Front.Opt.EnablePush)
                {
                    Type = 2; // Push�z�M
                }
            }
            else
            {
                Type = 0; // �����ڑ�
            }

            Gui.KagamiItem.Text = MyPort.ToString();                            // clmKagamiViewPort
            Gui.KagamiItem.SubItems.Add(ImportURL);                             // clmKagamiViewImport
            Gui.KagamiItem.SubItems.Add("0/" + Connection + "+" + Reserve);     // clmKagamiViewConn

            Client = new Client(this);
        }
        #endregion

        /// <summary>
        /// �C���|�[�g���y�ёS�N���C�A���g��ؒf���܂��B
        /// �O���ڑ��̏ꍇ�͑Ҏ󂯏�Ԃɖ߂�܂��B
        /// </summary>
        public void Disc()
        {
            if (Type == 0)
            {
                // �����ڑ��̏ꍇ�|�[�g��~
                ImportStatus = false;
                RunStatus = false;
            }
            else
            {
                // �ؒf�̂��߂̒l�ݒ�
                ImportStatus = false;
                ImportURL = "�ҋ@��";
                // �e����̏���
                BusyCounter = 0;
                RetryCounter = 0;
                Comment = "";
                Password = "";
                SetUserIP = "";
                Gui.ReserveItem.Clear();
                CurrentDLSpeed = 0;
                AverageDLSpeed = 0;
                TrafficCount = 0;
                MaxDLSpeed = 0;
                // �ő�ڑ��������[�U�w��l�ɖ߂�
                Connection = Conn_UserSet;
            }
        }

        #region �N���C�A���g�ǉ�/�폜�֌W
        /// <summary>
        /// ClientItem�Ƀf�[�^�ǉ��{GUI�X�V
        /// </summary>
        /// <param name="_cd"></param>
        public void AddClient(ClientData _cd)
        {
            string _hostname = "";
            string _hostip = "";
            ListViewItem _item = new ListViewItem();

            try { _hostname = Dns.GetHostEntry(_cd.Ip).HostName; }
            catch { _hostname = _cd.Ip; }

            if (Front.Opt.EnableResolveHost)
                _hostip = _hostname;
            else
                _hostip = _cd.Ip;

            // clientView.Columns�Ɠ������K�v!!
            _item.Text = _cd.Id;                // 0:clmClientViewID
            _item.SubItems.Add(_hostip);        // 1:clmClientViewIpHost
            _item.SubItems.Add(_cd.UserAgent);  // 2:clmClientViewUA
            _item.SubItems.Add("00:00:00");     // 3:clmClientViewTime
            _item.SubItems.Add(_cd.Ip);         // 4:clmClientView_internal_IP
            _item.SubItems.Add(_hostname);      // 5:clmClientView_internal_Host
            // ClientItem�ɒǉ�
            lock (Gui.ClientItem)
                Gui.ClientItem.Add(_item);
            // �N���C�A���g�ڑ��ʒm
            Event.EventUpdateClient(Kagami, _item, 0);
        }

        /// <summary>
        /// �w�肳�ꂽ�N���C�A���gID�̃f�[�^��ClientItem����폜�{GUI�X�V
        /// �Ώ�ID�̃N���C�A���g��������Ȃ���Ή������Ȃ�
        /// </summary>
        /// <param name="_id"></param>
        public void RemoveClient(string _id)
        {
            lock (Gui.ClientItem)
            {
                foreach (ListViewItem _item in Gui.ClientItem)
                {
                    if (_item.Text == _id)  // clmClientViewID
                    {
                        // ClientItem����폜
                        Gui.ClientItem.Remove(_item);
                        // �N���C�A���g�ؒf�ʒm
                        Event.EventUpdateClient(Kagami, _item, 1);
                        break;
                    }
                }
            }
        }
        #endregion

        #region ���U�[�u�֘A
        /// <summary>
        /// �w�肵��IP��ReserveItem�ɐV�K�o�^����
        /// </summary>
        /// <param name="_ip"></param>
        public void AddReserve(string _ip)
        {
            // ip���������`�����̃`�F�b�N�͌Ăяo�����ł���Ă������ƁB
            try
            {
                ListViewItem _item = new ListViewItem(_ip);
                _item.SubItems.Add("�~");
                _item.SubItems[0].ForeColor = Color.Red;
                // GUI�ɒǉ�
                Event.EventUpdateReserve(Kagami, _item, 0);
                // �����Ǘ�ReserveItem�ɒǉ�
                lock (Gui.ReserveItem)
                    Gui.ReserveItem.Add(_item);
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("AddReserve", "�����G���[ Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// �w�肵��IP��ReserveItem����폜����
        /// </summary>
        /// <param name="_ip"></param>
        public void RemoveReserve(string _ip)
        {
            lock (Gui.ReserveItem)
            {
                foreach (ListViewItem _item in Gui.ReserveItem)
                {
                    if (_item.Text == _ip)
                    {
                        // GUI����폜
                        Event.EventUpdateReserve(Kagami, _item, 1);
                        //ReserveItem����폜
                        Gui.ReserveItem.Remove(_item);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// �w��IP���~��ԂŃ��U�[�u�o�^����Ă����true
        /// </summary>
        /// <param name="_ip"></param>
        /// <returns></returns>
        public bool IsReserveList(string _ip)
        {
            lock (Gui.ReserveItem)
                foreach (ListViewItem _item in Gui.ReserveItem)
                    if (_item.Text == _ip)                      // clmReserveViewIP
                        if (_item.SubItems[1].Text == "�~")     // clmResvViewStatus
                            return true;
            return false;
        }

        /// <summary>
        /// ���U�[�uIP���X�g��Ł��ɂȂ��Ă���A�C�e���̐���ԋp
        /// </summary>
        /// <returns></returns>
        public int ReserveCount
        {
            get
            {
                int cnt = 0;
                lock (Gui.ReserveItem)
                {
                    foreach (ListViewItem _item in Gui.ReserveItem)
                    {
                        if (_item.SubItems[1].Text == "��") // clmResvViewStatus
                            cnt++;
                    }
                }
                return cnt;
            }
            set
            {
            }
        }
        #endregion

        #region �L�b�N�֘A����
        /// <summary>
        /// �L�b�NIP��Status.Gui.KickItem(=Form1.kickView)�ɐV�K�o�^
        /// Front.KickList�ɖ��o�^�Ȃ炻�����ɂ��o�^����
        /// GUI�ɂ��ʒm����
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_cnt"></param>
        public void AddKick(string _ip, int _cnt)
        {
            // ip���������`�����̃`�F�b�N�͌Ăяo�����ł���Ă������ƁB
            try
            {
                // Front.KickList�ւ̓o�^
                if (!Front.KickList.ContainsKey(_ip))
                    Front.KickList.Add(_ip, DateTime.Now + ",1");
                // KickItem�ւ̓o�^
                ListViewItem _item = new ListViewItem();
                _item.Text = _ip;
                _item.SubItems.Add("-");                // clmKickViewStatus
                _item.SubItems.Add(_cnt.ToString());    // clmKickViewCount
                Gui.KickItem.Add(_item);
                // GUI�ւ̔��f
                Event.EventUpdateKick(Kagami, _item, 0);
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("AddKick", "�����G���[ Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Kick�Ώۂ�IP�����肷��BKick�ΏۂŖ������true�ԋp
        /// </summary>
        /// <param name="sock"></param>
        /// <returns></returns>
        public bool IsKickCheck(Socket sock)
        {
            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();

            //���U�[�u�o�^����Ă����Kick�`�F�b�N����
            lock (Gui.ReserveItem)
                foreach (ListViewItem _item in Gui.ReserveItem)
                    if (_item.Text == _ip)
                        return true;

            //Kick�`�F�b�N���X�g�ɓo�^�ς݂��`�F�b�N
            if (Front.KickList.ContainsKey(_ip))
            {
                //�����I�����ԁA�A���ڑ��񐔂��擾
                string[] str = Front.KickList[_ip].Split(',');
                DateTime _end_tim = DateTime.Parse(str[0]);
                int _con_cnt = int.Parse(str[1]);
                Front.AddLogDebug("KICK�`�F�b�N", "KickCheckCount:" + str[1]);
                //�A���ڑ��񐔂��ݒ�񐔂𒴂������`�F�b�N
                if (_con_cnt > Front.Kick.KickCheckTime)
                {
                    //�������ԓ��ɒ������̂��`�F�b�N
                    if (DateTime.Now < _end_tim)
                    {
                        //�������ԓ��ɒ������̂Ńu���b�N�J�n
                        //�u���b�N�I�����Ԃ�ݒ�
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickDenyTime).ToString() + ",0";
                        Front.AddLogDebug("KICK�`�F�b�N", "KickCheckResult:KickStart");
                        return false;
                    }
                    else
                    {
                        //�������Ԓ��ߌ�ɒ������̂ōŏ�����J�E���g���Ȃ���
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickCheckSecond).ToString() + ",1";
                        Front.AddLogDebug("KICK�`�F�b�N", "KickCheckResult:CountReset");
                        return true;
                    }
                }
                //�u���b�N��
                else if (_con_cnt == 0)
                {
                    // �u���b�N���Ԓ��̃A�N�Z�X���`�F�b�N
                    if (DateTime.Now < _end_tim)
                    {
                        //���ێ��ԓ�
                        Front.AddLogDebug("KICK�`�F�b�N", "KickCheckResult:KickPeriodNow");
                        return false;
                    }
                    else
                    {
                        //���ێ��Ԓ��߁A�n�߂���J�E���g���Ȃ���
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickCheckSecond).ToString() + ",1";
                        Front.AddLogDebug("KICK�`�F�b�N", "KickCheckResult:KickPeriodEnd");
                        return true;
                    }
                }
                //�������u���b�N��
                else if (_con_cnt < 0)
                {
                    return false;
                }
                //�ݒ�񐔂��z����O
                else
                {
                    //�������Ԃ𒴂������`�F�b�N
                    if (DateTime.Now < _end_tim)
                    {
                        //�������Ԃ𒴂��Ă��Ȃ���ΘA���ڑ��񐔃J�E���g�A�b�v
                        Front.KickList[_ip] = _end_tim.ToString() + "," + (_con_cnt + 1);
                        Front.AddLogDebug("KICK�`�F�b�N", "KickCheckResult:CountUp");
                        return true;
                    }
                    else
                    {
                        //�������Ԃ𒴂��Ă���΍ŏ�����J�E���g�A�b�v
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickCheckSecond).ToString() + ",1";
                        Front.AddLogDebug("KICK�`�F�b�N", "KickCheckResult:CountReset");
                        return true;
                    }
                }
            }
            else
            {
                // �V�K��Kick�`�F�b�N���X�g�ɓo�^
                // �����I�����Ԃ����X�g�o�^����
                Front.AddLogDebug("KICK�`�F�b�N", "KickCheckResult:AddNewHost");
                Front.KickList.Add(_ip, DateTime.Now.AddSeconds(Front.Kick.KickCheckSecond).ToString() + ",1");
                return true;
            }
            // �����ɂ͗��Ȃ�
        }

        #endregion

    }
}
