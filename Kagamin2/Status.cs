using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using System.Collections;
using System.Diagnostics;
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
                string str = "";
                Match index = Regex.Match(ImportURL, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:");
                if (index.Success)
                {
                    str = ImportURL.Substring(index.Index, index.Length - 1);
                }
                else
                {
                    index = Regex.Match(ImportURL, @"http:\/\/[a-z0-9A-Z._-]+:");
                    if (index.Success)
                    {
                        str = ImportURL.Substring(index.Index + 7, index.Length - 8);
                    }
                    index = Regex.Match(ImportURL, @"mms:\/\/[a-z0-9A-Z._-]+:");
                    if (index.Success)
                    {
                        str = ImportURL.Substring(index.Index + 6, index.Length - 7);
                    }
                    if (str == "")
                    {
                        str = ImportURL.Split(':')[0];
                    }
                    if (str == "localhost")
                        str = "127.0.0.1";
                }
                return str;
            }
        }

        /// <summary>
        /// Import��Port�ԍ�
        /// </summary>
        public int ImportPort
        {
            get
            {
                Match index = Regex.Match(ImportURL, @":\d{1,5}");
                return int.Parse(ImportURL.Substring(index.Index + 1, index.Length - 1));
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
        /// �ڑ��ő吔����
        /// </summary>
        public int ConnectionMax = 0;
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
        /// Kagami.exe�������U�[�u���g��(������)
        /// </summary>
        public bool KagamiexeReserve;
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
        /// true�̏ꍇ�����I�������Ƃ��ɊO���҂��󂯂��I������
        /// </summary>
        public bool ListenStop = false;
        /// <summary>
        /// �V�K�ڑ������t���O(�|�[�g��)
        /// </summary>
        public bool Pause = false;
        /// <summary>
        /// Import�\�[�X�𐳏�Ɏ�M���Ă����true
        /// </summary>
        public bool ImportStatus = false;
        /// <summary>
        /// Push�z�M��p�t���O
        /// </summary>
        public bool PushOnly = false;
        /// <summary>
        /// �f�[�^��M�o�C�g��
        /// (�R�b���Ƀ��Z�b�g)
        /// </summary>
        public int RecvByte = 0;
        /// <summary>
        /// ���ԃ����N�Ǘ��N���X(�T�[�o)
        /// </summary>
        public KagamiLink IKLink = null;
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
        #region �L�b�N�Ǘ��N���X
        /// <summary>
        /// �G�N�X�|�[�g�ɑ΂���L�b�N�Ǘ����s���N���X
        /// </summary>
        public class KICK
        {
            #region �����o�[�ϐ�
            /// <summary>
            /// IP
            /// </summary>
            public string IP;
            /// <summary>
            /// �ŏ��̐ڑ�����������
            /// </summary>
            public DateTime StartTime;
            /// <summary>
            /// �ڑ����s��
            /// </summary>
            public int Cnt ;
            /// <summary>
            /// �ڑ����ۉ�
            /// </summary>
            public int Cnt_out;
            /// <summary>
            /// ���ۊJ�n����
            /// </summary>
            public DateTime DenyTime;
            /// <summary>
            /// ���ۏI������(�b)
            /// �������̏ꍇ��-1
            /// </summary>
            public int DenyEndTime;
            
            /// <summary>
            /// ���ے��t���O
            /// </summary>
            public bool KickFlag;
            /// <summary>
            /// �����L�b�N�t���O.false�̏ꍇ�蓮�o�^
            /// </summary>
            public bool AutoKick;

            /// <summary>
            /// �����I������
            /// </summary>
            public DateTime ResetTime
            {
                get { return StartTime.AddSeconds(Front.Kick.KickCheckSecond); }

            }


            #endregion
        }
        #endregion

        public Dictionary<string,KICK> Kick = new Dictionary<string,KICK>();
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
        /// �F��ID
        /// </summary>
        public string AuthID = "";
        /// <summary>
        /// �F��Pass
        /// </summary>
        public string AuthPass = "";
        /// <summary>
        /// �C���|�[�g�F�؎��̗v�\�����x��
        /// </summary>
        public string ImportAuthLabel = "";
        /// <summary>
        /// �C���|�[�g�̔F��ID
        /// </summary>
        public string ImportAuthID = "";
        /// <summary>
        /// �C���|�[�g�̔F�؃p�X
        /// </summary>
        public string ImportAuthPass = "";

        /// <summary>
        /// WEB�D��q���]������
        /// </summary>
        public bool TransWeb = Front.Opt.TransKagamin;
        /// <summary>
        /// �w�b�_�擾����/HTTP1.0
        /// </summary>
        public byte[] HeadRspMsg10 = null;
        /// <summary>
        /// �w�b�_�擾����/HTTP1.1
        /// </summary>
        //public byte[] HeadRspMsg11 = null;
        /// <summary>
        /// �w�b�_�X�g���[�����
        /// </summary>
        public byte[] HeadStream = null;
        /// <summary>
        /// �f�[�^�擾����/HTTP1.0
        /// </summary>
        public byte[] DataRspMsg10 = null;
        /// <summary>
        /// �f�[�^�擾����/HTTP1.1
        /// </summary>
        //public byte[] DataRspMsg11 = null;
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
        /// <summary>
        /// �X�g���[���w�b�_�ɋL�q����Ă�Title
        /// </summary>
        public string ASFTitle = "";
        /// <summary>
        /// �X�g���[���w�b�_�ɋL�q����Ă�Author
        /// </summary>
        public string ASFAuthor = "";
        /// <summary>
        /// �X�g���[���w�b�_�ɋL�q����Ă�CopyRight
        /// </summary>
        public string ASFCopyRight = "";
        /// <summary>
        /// �X�g���[���w�b�_�ɋL�q����Ă�Description
        /// </summary>
        public string ASFDescription = "";
        /// <summary>
        /// �X�g���[���w�b�_�ɋL�q����Ă�Rating
        /// </summary>
        public string ASFRating = "";
        /// <summary>
        /// �𑜓x�c
        /// </summary>
        public int MediaHeight = 0;
        /// <summary>
        /// �𑜓x��
        /// </summary>
        public int MediaWidth = 0;
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
        /// �q���]����
        /// </summary>
        public int TransCount = 0;
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
        /// ����ő�r�b�g���[�g(Kbps)
        /// </summary>
        public int MaxVideoBitRate = 0;
        /// <summary>
        /// �����ő�r�b�g���[�g(Kbps)
        /// </summary>
        public int MaxAudioBitRate = 0;
        /// <summary>
        /// �I�𒆂̃r�b�g���[�g(Video�}���`�r�b�g���[�g�pKbps)
        /// </summary>
        public int NowBitRateVideo = 0;
        /// <summary>
        /// �I�𒆂̃r�b�g���[�g(Audio�}���`�r�b�g���[�g�pKbps)
        /// </summary>
        public int NowBitRateAudio = 0;
        /// <summary>
        /// �I�𒆂̃r�b�g���[�g��ID
        /// </summary>
        public int MultiID = 0;
        /// <summary>
        /// �}���`�r�b�g���[�g�؂�ւ��p(true=�؂�ւ����g���C)
        /// </summary>
        public bool SelectMulti = false;
        /// <summary>
        /// �}���`�r�b�g���[�g�؂�ւ��ꎞ�p(true=�؂�ւ����g���C)
        /// </summary>
        public bool tempMulti = false;

        /// <summary>
        /// ���݂̐ڑ��ł̃G�N�X�|�[�g��ւ̍��vUpSize
        /// </summary>
        public ulong TotalUPSize = 0;
        /// <summary>
        /// ���݂̐ڑ��ł̃C���|�[�g������̍��vDownSize
        /// </summary>
        public ulong TotalDLSize = 0;
        /// <summary>
        /// �N���C�A���g�ɔF�؂�v�����邩
        /// </summary>
        public bool ExportAuth = false;
        ////
        //public bool VirtualHost = false;
        //public bool SQLOn = false;
        ////
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
            AuthID = Front.Opt.AuthID;
            AuthPass = Front.Opt.AuthPass;
            // ImportURL����Ȃ�O���ڑ����[�h�ŋN��
            if (ImportURL == "")
            {
                ImportURL = "�ҋ@��";
                Type = 1; // �O���ڑ�
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
                Kick.Clear();
                Gui.KickItem.Clear();
                CurrentDLSpeed = 0;
                AverageDLSpeed = 0;
                TrafficCount = 0;
                MaxDLSpeed = 0;
                // �ő�ڑ��������[�U�w��l�ɖ߂�
                Connection = Conn_UserSet;
                Event.EventUpdateKick(Kagami, null, 2);
            }
        }
        /// <summary>
        /// �}���`�r�b�g���[�g(video)��ID����i�߂܂��B
        /// SelectedAudioRecord,Status.SelectedVideoRecord�������ID�֕ύX����
        /// �Đڑ������܂��B
        /// </summary>
        /// <returns>�r�b�g���[�g</returns>
        public void MultiVideo()
        {
            bool select = false;
            /*
            if (SelectedVideoRecord + 1 > StreamSwitchCount)
            {
                for (int cnt = 1; cnt <= StreamSwitchCount; cnt++)
                {
                    if ((StreamType[cnt - 1] == 1))
                    {
                        SelectedVideoRecord = cnt;
                        select = true;
                        SelectMulti = true;
                        tempMulti = true;
                        NowBitRateVideo = StreamBitrate[cnt-1] / 1000;
                    }
                }
                return;


            }*/
            int _temp = SelectedVideoRecord;
            for (int cnt = SelectedVideoRecord+1; cnt <= StreamSwitchCount; cnt++)
            {
                if ((StreamType[cnt-1] == 1))
                {
                    if (_temp == cnt)
                        return;
                    SelectedVideoRecord = cnt;
                    select = true;
                    SelectMulti = true;
                    tempMulti = true;
                    NowBitRateVideo = StreamBitrate[cnt-1] / 1000;
                    return;
                }
            }
            if (!select)
            {
                for (int cnt = 1; cnt <= StreamSwitchCount; cnt++)
                {
                    if ((StreamType[cnt - 1] == 1))
                    {
                        if (_temp == cnt)
                            return;
                        SelectedVideoRecord = cnt;
                        select = true;
                        SelectMulti = true;
                        tempMulti = true;
                        NowBitRateVideo = StreamBitrate[cnt - 1] / 1000;
                        return;
                    }
                }

            }

        }
        /// <summary>
        /// �}���`�r�b�g���[�g(audio)��ID����i�߂܂��B
        /// SelectedAudioRecord,Status.SelectedVideoRecord�������ID�֕ύX����
        /// �Đڑ������܂��B
        /// </summary>
        /// <returns>�r�b�g���[�g</returns>
        public void Multiaudio()
        {
            bool select = false;
            /*
            if (SelectedAudioRecord + 1 > StreamSwitchCount)
            {
                for (int cnt = 1; cnt <= StreamSwitchCount; cnt++)
                {
                    if ((StreamType[cnt - 1] == 0))
                    {
                        SelectedAudioRecord = cnt;
                        select = true;
                        SelectMulti = true;
                        tempMulti = true;
                        NowBitRateAudio = StreamBitrate[cnt - 1] / 1000;
                    }
                }
                return;


            }*/
            int _temp = SelectedAudioRecord;
            for (int cnt = SelectedAudioRecord+1; cnt <= StreamSwitchCount; cnt++)
            {
                if ((StreamType[cnt-1] == 0))
                {
                    if (_temp == cnt)
                        return;
                    SelectedAudioRecord = cnt;
                    select = true;
                    SelectMulti = true;
                    tempMulti = true;
                    NowBitRateAudio= StreamBitrate[cnt-1] / 1000;
                    return;
                }
            }
            if (!select)
            {
                for (int cnt = 1; cnt <= StreamSwitchCount; cnt++)
                {
                    if ((StreamType[cnt - 1] == 0))
                    {
                        if (_temp == cnt)
                            return;
                        SelectedAudioRecord = cnt;
                        select = true;
                        SelectMulti = true;
                        tempMulti = true;
                        NowBitRateAudio= StreamBitrate[cnt - 1] / 1000;
                        return;
                    }
                }

            }


        }
        #region �N���C�A���g�ǉ�/�폜�֌W
        /// <summary>
        /// ClientItem�Ƀf�[�^�ǉ��{GUI�X�V
        /// </summary>
        /// <param name="_cd"></param>
        public void AddClient(ClientData _cd)
        {
            ListViewItem _item = new ListViewItem();
            _item.Text = _cd.Id;                // clmClientViewID
            _item.SubItems.Add("100%");         //Buffer
            _item.SubItems.Add(_cd.Ip);         // clmClientViewIP

            try
            {
                lock (Gui.ReserveItem)
                {
                    foreach (ListViewItem _itemRes in Gui.ReserveItem)
                    {
                        if (_itemRes.Text == _cd.Ip)
                            _item.SubItems[0].ForeColor = System.Drawing.Color.Blue;
                    }
                }
            }
            catch
            {
            }
            _item.SubItems.Add(_cd.UserAgent);  // clmClientViewUA
            //_item.SubItems.Add("");             // Work

            _item.SubItems.Add("00:00:00");     // clmClientViewTime

            _item.SubItems.Add(_cd.Host);       //clmClientViewHost
            _item.SubItems.Add(_cd.ConnInfo);             //KagamiInfo
            //_item.SubItems.Add("");             // FQDN
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
                _item.SubItems[0].ForeColor = System.Drawing.Color.Red;
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
        /// �L�b�NIP��V�K�o�^�B�o�^�ς݂̏ꍇ�X�V
        /// </summary>
        /// <param name="_ip">�ݒ肷��IP</param>
        /// <param name="_cnt">���ۂ��鎞��.0�̏ꍇ�͓o�^�̂݁B-1�̏ꍇ�͖�����</param>
        /// <param name="_user">���[�U�[�o�^�t���O</param>
        public void AddKick(string _ip, int _time, bool _user)
        {
            lock (Kick)
                lock (Gui.KickItem)
                {
                    // ip���������`�����̃`�F�b�N�͌Ăяo�����ł���Ă������ƁB


                    try
                    {
                        //�L�b�N�A�C�e���Ɋ܂܂�Ă��邩
                        if (!CheckKickItem(_ip))
                        {//�܂܂�Ă��Ȃ��ꍇGUI�ɐV�K�o�^

                            //����Kick�Ǘ��ɂȂ��ꍇ�o�^
                            if (!Kick.ContainsKey(_ip))
                            {
                                Kick[_ip] = new KICK();
                                Kick[_ip].IP = _ip;

                                if (_time != 0)//0�ȊO�̏ꍇ���ے���
                                    Kick[_ip].KickFlag = true;
                                else//0�̏ꍇ�o�^�̂�
                                    Kick[_ip].KickFlag = false;
#if DEBUG
                                Front.AddKickLog(Kagami.Status, "addkick1");
#endif
                                Kick[_ip].StartTime = DateTime.Now;
                                Kick[_ip].Cnt = 0;
                                Kick[_ip].AutoKick = false;//�o�^����ĂȂ��ꍇ�͎蓮�o�^�����Ȃ�
                                Kick[_ip].DenyTime = DateTime.Now;
                                Kick[_ip].DenyEndTime = _time;
                            }
                            else if(_user)//�o�^�ς݂̏ꍇ�Ŏ蓮�o�^�̏ꍇ
                            {
                                Kick[_ip].AutoKick = _user;
                                ResetKick(_ip, _time);
                            }

                            
                            // KickItem�ւ̓o�^
                            ListViewItem _item = new ListViewItem();
                            _item.Text = _ip;

                            if (_time == 0)
                                _item.SubItems.Add("-");                // clmKickViewStatus
                            else
                                _item.SubItems.Add(_time.ToString());

                            _item.SubItems.Add("1");
                            Gui.KickItem.Add(_item);
                            Event.EventUpdateKick(Kagami, _item, 0);

                            Kagami.Status.Client.UpdateKickTime();
                        }
                        else
                        {
                            if (_user)//�蓮�o�^�̏ꍇ
                            {
                                Kick[_ip].AutoKick = _user;
                                ResetKick(_ip, _time);

                            }
                            foreach (ListViewItem _item in Gui.KickItem)
                            {
                                Event.EventUpdateKick(Kagami, _item, 0);

                            }

                        }

                        // GUI�ւ̔��f

                    }
                    catch (Exception ex)
                    {
                        Front.AddLogDebug("AddKick", "�����G���[ Trace:" + ex.StackTrace);
                    }
                }
        }
        /// <summary>
        /// �n�b�V���ɓo�^�ς݂�IP�ɋ��ێ��Ԃ��Z�b�g����
        /// ���ێ��Ԃ�1�ȏ�̏ꍇ�L�b�N���Ƀt���O��؂�ւ���
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_denytime"></param>
        /// <returns></returns>
        private void ResetKick(string _ip, int _denytime)
        {
            Kick[_ip].DenyTime = DateTime.Now;
            Kick[_ip].DenyEndTime = _denytime;
            if (_denytime != 0)//0�ȊO�̏ꍇ���ے���
                Kick[_ip].KickFlag = true;
            else//0�̏ꍇ�o�^�̂�
                Kick[_ip].KickFlag = false;
        }

        /// <summary>
        /// KickItem����IP�����邩�ǂ���
        /// </summary>
        /// <param name="_ip"></param>
        /// <returns></returns>
        private bool CheckKickItem(string _ip)
        {
            foreach (ListViewItem _item in Gui.KickItem)
            {
                if (_item.Text == _ip)
                    return true;

            }
            return false;

        }
        /// <summary>
        /// �蓮�L�b�N�o�^
        /// </summary>
        /// <param name="_ip">IP</param>
        /// <param name="_time">���ێ���</param>
        public void AddKick(string _ip, int _time)
        {
            AddKick(_ip, _time, true);

        }
        /// <summary>
        /// �����L�b�N�o�^
        /// </summary>
        /// <param name="_ip">IP</param>
        public void AddKick(string _ip)
        {
            AddKick(_ip, (int)Front.Kick.KickDenyTime, false);

        }
        
        /// <summary>
        /// �����L�b�N�Ǘ�����L�b�NIP������.GUI�͊֌W�Ȃ�
        /// </summary>
        /// <param name="_ip">��������IP</param>
        public void DelKick(string _ipl)
        {
            lock (Kick)
            {
                // ip���������`�����̃`�F�b�N�͌Ăяo�����ł���Ă������ƁB
                try
                {
                    if (!Kick.ContainsKey(_ipl))
                        return;//�܂܂�Ă��Ȃ��ꍇ�͉������Ȃ�

                    

                    Kick[_ipl].KickFlag = false;
                    Kick[_ipl].DenyEndTime = 0;
                    Kick[_ipl].Cnt = 0;

                }
                catch (Exception ex)
                {
                    Front.AddLogDebug("DelKick", "�����G���[ Trace:" + ex.StackTrace);
                }

            }
        }
        /// <summary>
        /// Kick�Ώۂ�IP�����肷��BKick�ΏۂŖ������true�ԋp
        /// </summary>
        /// <param name="sock"></param>
        /// <returns></returns>
        public bool IsKickCheck(System.Net.Sockets.Socket sock)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                //IP�A�h���X
                string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();

                //���U�[�u�o�^����Ă����Kick�`�F�b�N����.�L�b�N�Ǘ���������
                lock (Gui.ReserveItem)
                    foreach (ListViewItem _item in Gui.ReserveItem)
                        if (_item.Text == _ip)
                        {
                            if (Kick.ContainsKey(_ip))
                            {
                                Kick[_ip].DenyEndTime = 0;
                                Kick[_ip].Cnt = 0;
                                Kick[_ip].KickFlag = false;
                            }
                            return true;

                        }



                //KickList����IP�����邩�`�F�b�N
                //�܂܂�ĂȂ��ꍇ�L�b�N�Ώۂł͂Ȃ�
                if (!Kick.ContainsKey(_ip))
                {
                    //�Ȃ���ΐV�K�o�^
                    KICK Kick_Client = new KICK();
                    Kick_Client.IP = _ip;
                    Kick_Client.KickFlag = false;
                    Kick_Client.StartTime = DateTime.Now;
                    Kick_Client.Cnt = 1;
                    Kick_Client.Cnt_out = 0;
                    Kick_Client.AutoKick = false;
#if DEBUG
                    Front.AddKickLog(Kagami.Status, "Iskickccheck1");
#endif
                    Kick_Client.DenyTime = DateTime.Now;
                    Kick_Client.DenyEndTime = 0; ;

                    //�n�b�V���ɓo�^
                    Kick[_ip] = Kick_Client;
                    return true;
                }
                else
                {//�܂܂�Ă�ꍇ

                    //KICK _kick = Kick[_ip];

                    //�L�b�N���̏ꍇ
                    if (Kick[_ip].KickFlag)
                    {

                            //�L�b�N���Ԗ�����
                            if (Kick[_ip].DenyEndTime < 0)
                            {
                                Kick[_ip].Cnt++;
                                Kick[_ip].Cnt_out++;
                                return false;
                            }

                            //���ۏI���������߂��Ă���ꍇ���Z�b�g
                            if (Kick[_ip].DenyTime.AddSeconds(Kick[_ip].DenyEndTime) < DateTime.Now)
                            {
                                Kick[_ip].KickFlag = false;
                                Kick[_ip].StartTime = DateTime.Now;
                                Kick[_ip].Cnt = 0;
#if DEBUG
                                Front.AddKickLog(Kagami.Status, "Iskickccheck2");
#endif

                                return true;
                            }
                            else
                            {//���ێ��Ԓ��̏ꍇ�ڑ����s�񐔂𑝂₵�I��
                                //Kick[_ip].Cnt++;
                                Kick[_ip].Cnt_out++;
                                return false;

                            }
                        

                    }//�L�b�N���łȂ��ꍇ
                    else
                    {


                        try
                        {
                            //�肹���Ǝ��ԂɒB���Ă��Ȃ�
                            if (Kick[_ip].ResetTime > DateTime.Now)
                            {
                                //�L�b�N�񐔂ɓ��B�̏ꍇ�L�b�N�J�n
                                if (Kick[_ip].Cnt >= Front.Kick.KickCheckTime)
                                {
                                    Kick[_ip].DenyTime = DateTime.Now;
                                    Kick[_ip].DenyEndTime = (int)Front.Kick.KickDenyTime;
                                    Kick[_ip].Cnt = 0;
                                    Kick[_ip].Cnt_out++;
                                    Kick[_ip].KickFlag = true;
                                    AddKick(_ip);



                                    return false;

                                }
                                else//�L�b�N�񐔂ɓ��B���Ă��Ȃ��ꍇ�͉񐔂�+1
                                {
                                    Kick[_ip].Cnt++;

                                    return true;

                                }

                            }
                            else//���Z�b�g���ԂɒB���Ă��郊�Z�b�g
                            {
                                Kick[_ip].Cnt = 0;
                                Kick[_ip].StartTime = DateTime.Now;
                                Kick[_ip].KickFlag = false;
                                return true;
                            }
                        }
                        finally
                        {

                        }


                    }

                }
            }
            finally
            {
                sw.Stop();
                //Front.AddLogData(1, this, "1:" + sw.ElapsedMilliseconds);

            }
        }

        #endregion

    }

}
