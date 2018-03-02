using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Kagamin2
{
    /// <summary>
    /// �X�P�W���[���N���p�A�C�e��
    /// </summary>
    public class SCHEDULE
    {
        /// <summary>
        /// �X�P�W���[���L���t���O
        /// </summary>
        public bool Enable;
        /// <summary>
        /// �X�P�W���[���C�x���g
        /// </summary>
        public uint Event;
        /// <summary>
        /// �X�P�W���[���|�[�g
        /// 0=�S�|�[�g
        /// </summary>
        public uint Port;
        /// <summary>
        /// �X�P�W���[���J�n����
        /// 0=���� 1=�]����
        /// </summary>
        public uint StartType;
        /// <summary>
        /// �X�P�W���[���J�n����(�j��)
        /// </summary>
        public uint Week;
        /// <summary>
        /// �X�P�W���[���J�n����(��)
        /// </summary>
        public uint Hour;
        /// <summary>
        /// �X�P�W���[���J�n����(��)
        /// </summary>
        public uint Min;
        /// <summary>
        /// ��r���]���ʎ��
        /// 0=���ԓ]���� 1=���ԓ]����
        /// </summary>
        public uint TrfType;
        /// <summary>
        /// ��r�]���ʒl
        /// </summary>
        public uint TrfValue;
        /// <summary>
        /// ��r�]���ʒP��
        /// 0=MB 1=GB
        /// </summary>
        public uint TrfUnit;
        /// <summary>
        /// �]���ʎw��̃C�x���g���s�ς݃t���O
        /// </summary>
        public bool ExecTrf;
        /// <summary>
        /// �ڑ��g(�ʏ�g)
        /// </summary>
        public uint Conn;
        /// <summary>
        /// �ڑ��g(���U�[�u�g)
        /// </summary>
        public uint Resv;
    }

    #region Import/Export�^�X�N����GUI�ւ̃C�x���g�ʒm�n���h��

    /// <summary>
    /// Form�X�V�C�x���g�n���h���p�C�x���g�f�[�^�N���X
    /// </summary>
    public class KagamiEvent : EventArgs
    {
        /// <summary>
        /// �X�V�Ώ�Kagami
        /// </summary>
        public Kagami Kagami;
        /// <summary>
        /// �X�V�Ώ�Item
        /// </summary>
        public ListViewItem Item;
        /// <summary>
        /// �X�V���[�h�B
        /// �n���h����ʖ��ɗ��p���@�͂��܂��܁B
        /// </summary>
        public int Mode;
#if DEBUG
        /// <summary>
        /// �C�x���g�ʒm���X���b�hID(�f�o�b�O�p)
        /// </summary>
        public int ThreadId;
#endif
    }

    /// <summary>
    /// static�C�x���g�n���h��
    /// </summary>
    static public class Event
    {
        /// <summary>
        /// ����ԕω��ʒm�n���h��
        /// </summary>
        public static event EventHandler UpdateKagami;
        /// <summary>
        /// �N���C�A���g�ڑ��ؒf�ʒm�n���h��
        /// </summary>
        public static event EventHandler UpdateClient;
        /// <summary>
        /// ���U�[�u�o�^��ԕω��ʒm�n���h��
        /// </summary>
        public static event EventHandler UpdateReserve;
        /// <summary>
        /// �N���C�A���g�L�b�N�ʒm�n���h��
        /// </summary>
        public static event EventHandler UpdateKick;
        /// <summary>
        /// ���O�o�͒ʒm�n���h��
        /// </summary>
        public static event EventHandler UpdateLog;

        /// <summary>
        /// ����ԕω��ʒm
        /// </summary>
        public static void EventUpdateKagami()
        {
            UpdateKagami(null, EventArgs.Empty);
        }

        /// <summary>
        /// �N���C�A���g�ڑ��ؒf�ʒm
        /// </summary>
        /// <param name="_kagami"></param>
        /// <param name="_item"></param>
        /// <param name="_mode"></param>
        public static void EventUpdateClient(Kagami _kagami, ListViewItem _item, int _mode)
        {
            if (_kagami == null)
                return;

            KagamiEvent e = new KagamiEvent();
            e.Kagami = _kagami;
            e.Item = _item;
            e.Mode = _mode;
#if DEBUG
            e.ThreadId = System.AppDomain.GetCurrentThreadId();
#endif
            if (UpdateClient != null)
                UpdateClient(null, e);
        }
        /// <summary>
        /// ���U�[�u�o�^��ԕω��ʒm
        /// _mode=0:�ǉ� 1:�폜 2:�S�폜
        /// </summary>
        /// <param name="_kagami"></param>
        /// <param name="_item"></param>
        /// <param name="_mode"></param>
        public static void EventUpdateReserve(Kagami _kagami, ListViewItem _item, int _mode)
        {
            if (_kagami == null)
                return;

            KagamiEvent e = new KagamiEvent();
            e.Kagami = _kagami;
            e.Item = _item;
            e.Mode = _mode;
#if DEBUG
            e.ThreadId = System.AppDomain.GetCurrentThreadId();
#endif
            if (UpdateReserve != null)
                UpdateReserve(null, e);
        }

        /// <summary>
        /// �N���C�A���g�L�b�N�ʒm
        /// </summary>
        /// <param name="_kagami"></param>
        public static void EventUpdateKick(Kagami _kagami,ListViewItem _item, int _mode)
        {
            if (_kagami == null)
                return;

            KagamiEvent e = new KagamiEvent();
            e.Kagami = _kagami;
            e.Item = _item;
            e.Mode = _mode;
#if DEBUG
            e.ThreadId = System.AppDomain.GetCurrentThreadId();
#endif
            if (UpdateKick != null)
                UpdateKick(null, e);
        }

        /// <summary>
        /// ���O�o�͒ʒm
        /// </summary>
        /// <param name="_logLv"></param>
        /// <param name="_kagami"></param>
        /// <param name="_item"></param>
        public static void EventUpdateLog(int _logLv, Kagami _kagami, ListViewItem _item)
        {
            if (_kagami == null)
                return;

            KagamiEvent e = new KagamiEvent();
            e.Kagami = _kagami;
            e.Item = _item;
            e.Mode = _logLv;
#if DEBUG
            e.ThreadId = System.AppDomain.GetCurrentThreadId();
#endif
            if (UpdateLog != null)
                UpdateLog(null, e);
        }
    }
    #endregion

    /// <summary>
    /// static�t�����g��
    /// </summary>
    static public class Front
    {
        #region �ݒ��ۑ����郁���o�ϐ�
        /// <summary>
        /// Form�֘A�f�[�^
        /// </summary>
        public struct FORM
        {
            /// <summary>
            /// �O��I�����̃E�C���h�E�\���ʒuX
            /// </summary>
            public int X;
            /// <summary>
            /// �O��I�����̃E�C���h�E�\���ʒuY
            /// </summary>
            public int Y;
            /// <summary>
            /// �O��I�����̃E�C���h�E��W
            /// </summary>
            public int W;
            /// <summary>
            /// �O��I�����̃E�C���h�E��H
            /// </summary>
            public int H;
            /// <summary>
            /// �O��I������splitter1�̕�
            /// </summary>
            public uint SplitDistance1;
            /// <summary>
            /// �O��I������splitter2�̕�
            /// </summary>
            public uint SplitDistance2;
            /// <summary>
            /// LeftPanel�̏k��OnOff
            /// </summary>
            public bool LeftPanelCollapsed;
            /// <summary>
            /// kagamiView�̊e�J������(�J���}��؂�)
            /// </summary>
            public string KagamiListColumn;
            /// <summary>
            /// monAllView�̊e�J������(�J���}��؂�)
            /// </summary>
            public string MonAllViewColumn;
            /// <summary>
            /// monView�̊e�J������(�J���}��؂�)
            /// </summary>
            public string MonViewColumn;
            /// <summary>
            /// clientView�̊e�J������(�J���}��؂�)
            /// </summary>
            public string ClientViewColumn;
            /// <summary>
            /// reserveView�̊e�J������(�J���}��؂�)
            /// </summary>
            public string ResvViewColumn;
            /// <summary>
            /// kickView�̊e�J������(�J���}��؂�)
            /// </summary>
            public string KickViewColumn;
            /// <summary>
            /// logView�̊e�J������(�J���}��؂�)
            /// </summary>
            public string LogViewColumn;
            /// <summary>
            /// �X�P�W���[���̊e�J������(�J���}��؂�)
            /// </summary>
            public string ScheduleColumn;
            /// <summary>
            /// ���j�^��̑��x�\���P��
            /// 0:kbps 1:KB/s
            /// </summary>
            public uint monViewUnit;
            /// <summary>
            /// �A�C�R���C���f�b�N�X
            /// </summary>
            public uint IconIndex;
            /// <summary>
            /// �ŏ������Ƀ^�X�N�g���C�ɏ풓����
            /// </summary>
            public bool EnableTrayIcon;
        }
        static public FORM Form;

        /// <summary>
        /// GUI��̃f�[�^
        /// </summary>
        public struct GUI
        {
            /// <summary>
            /// GUI��̃f�t�H���gImportURL
            /// </summary>
            public string ImportURL;
            /// <summary>
            /// ���C�ɓ���ImportURL���X�g
            /// </summary>
            public List<string> FavoriteList;
            /// <summary>
            /// GUI��̃|�[�g���X�g
            /// </summary>
            public List<int> PortList;
            /// <summary>
            /// GUI��̃f�t�H���g�ʏ�ڑ���
            /// </summary>
            public uint Conn;
            /// <summary>
            /// GUI��̃f�t�H���g���U�[�u�g��
            /// </summary>
            public uint Reserve;
        }
        static public GUI Gui;

        /// <summary>
        /// �G���g�����X���
        /// </summary>
        public struct HP
        {
            /// <summary>
            /// ���u����G���g�����X�g�p�L��
            /// </summary>
            public bool UseHP;
            /// <summary>
            /// �G���g�����X�\���p�z�X�g��
            /// </summary>
            public string IpHTTP;
            /// <summary>
            /// �G���g�����X�\���p�|�[�g�ԍ�
            /// </summary>
            public uint PortHTTP;
            /// <summary>
            /// �G���g�����X���JDir
            /// </summary>
            public string PublicDir;
            /// <summary>
            /// �O���[�o��IP���擾���邽�߂̊m�F�NURL
            /// </summary>
            public string IPCheckURL;
            /// <summary>
            /// �m�F�N��HTML�\�[�X���ŁA
            /// �O���[�o��IP�������Ă���s�ԍ�
            /// </summary>
            public uint IPCheckLine;
        }
        static public HP Hp;

        /// <summary>
        /// �\�P�b�g�֘A�f�[�^
        /// </summary>
        public struct SOCK
        {
            /// <summary>
            /// �\�P�b�g�ڑ��^�C���A�E�g����(ms)
            /// </summary>
            public uint SockConnTimeout;
            /// <summary>
            /// �C���|�[�g��M�^�C���A�E�g����(ms)
            /// </summary>
            public uint SockRecvTimeout;
            /// <summary>
            /// �G�N�X�|�[�g���M�^�C���A�E�g����(ms)
            /// </summary>
            public uint SockSendTimeout;
            /// <summary>
            /// �G�N�X�|�[�g���M�L���[臒l
            /// </summary>
            public uint SockSendQueueSize;
            /// <summary>
            /// �\�P�b�g�ؒf�܂ł̒x������(ms)
            /// </summary>
            public uint SockCloseDelay;
        }
        static public SOCK Sock;

        /// <summary>
        /// ���g���C�񐔃f�[�^
        /// </summary>
        public struct RETRY
        {
            /// <summary>
            /// �����ڑ����g���C��
            /// </summary>
            public uint InRetryTime;
            /// <summary>
            /// �����ڑ��Đڑ��҂�����(�b)
            /// </summary>
            public uint InRetryInterval;
            /// <summary>
            /// �O���ڑ����g���C��
            /// </summary>
            public uint OutRetryTime;
            /// <summary>
            /// �O���ڑ��Đڑ��҂�����(�b)
            /// </summary>
            public uint OutRetryInterval;
        }
        static public RETRY Retry;

        /// <summary>
        /// �e��I�v�V�����@�\�t���O
        /// </summary>
        public struct OPT
        {
            /// <summary>
            /// kagami.exe�D�惂�[�h�L���t���O
            /// </summary>
            public bool PriKagamiexe;
            /// <summary>
            /// �����݂�D�惂�[�h�L���t���O
            /// </summary>
            public bool PriKagamin;
            /// <summary>
            /// BrowserView���[�h�L���t���O
            /// </summary>
            public bool BrowserView;
            /// <summary>
            /// �u���E�U�r���[�\�����[�h(false=TEXT,true=HTML)
            /// </summary>
            public bool BrowserViewMode;
            /// <summary>
            /// �^�X�N�g���CBalloonTip�\���L���t���O
            /// </summary>
            public bool BalloonTip;
            /// <summary>
            /// �v�b�V���z�M�T�[�o�@�\�L���t���O
            /// </summary>
            public bool EnablePush;
            /// <summary>
            /// /info.html�L���t���O
            /// </summary>
            public bool EnableInfo;
            /// <summary>
            /// /admin.html�L���t���O
            /// </summary>
            public bool EnableAdmin;
            /// <summary>
            /// �Ǘ��҃��[�h�p�X���[�h
            /// </summary>
            public string AdminPass;
/*
            /// <summary>
            /// /rss.rdf�L���t���O
            /// </summary>
            public bool EnableRss;
            /// <summary>
            /// RSS�z�M���̃^�C�g��
            /// </summary>
            public string RssTitle;
            /// <summary>
            /// RSS�z�M����URL
            /// </summary>
            public string RssUrl;
 */
            /// <summary>
            /// IM�ڑ�OK���Đ���
            /// </summary>
            public string SndConnOkFile;
            /// <summary>
            /// IM�ڑ�NG���Đ���
            /// </summary>
            public string SndConnNgFile;
            /// <summary>
            /// IM�ؒf���Đ���
            /// </summary>
            public string SndDiscFile;
            ///<summary>
            /// ����URL����
            ///</summary>
            public string OutUrl;
            /// <summary>
            /// �A�v���P�[�V�����l�[���B�o�[�W������ɕt�L
            /// </summary>
            public string AppName;
            /// <summary>
            /// �h���C���������L���t���O
            /// </summary>
            public bool EnableResolveHost;
        }
        static public OPT Opt;

        /// <summary>
        /// �L�b�N�֘A�f�[�^
        /// </summary>
        public struct KICK
        {
            /// <summary>
            /// Kick��������(�b)
            /// </summary>
            public uint KickCheckSecond;
            /// <summary>
            /// Kick�J�n臒l
            /// </summary>
            public uint KickCheckTime;
            /// <summary>
            /// Kick����(�b)
            /// </summary>
            public uint KickDenyTime;
        }
        static public KICK Kick;

        /// <summary>
        /// ���O�t�@�C���֘A�f�[�^
        /// </summary>
        public struct LOG
        {
            /// <summary>
            /// HP�A�N�Z�X���O
            /// </summary>
            public string HpLogFile;
            /// <summary>
            /// �����O
            /// </summary>
            public string KagamiLogFile;
            /// <summary>
            /// �ڍ׃��O�o�̓t���O
            /// </summary>
            public bool LogDetail;

            ///
            /// �ȉ��̃g���q�b�N�͑O��I���������_�ł̓���/���ԃg���q�b�N�B
            /// ����N�����ɓ�/�����ς���Ă��ꍇ�j������B
            /// ������/���Ȃ�TotalUP/TotalDL�ƍ��킹��GUI��ɕ\������B
            /// TotalUP/TotalDL�͂����܂ł��̃Z�b�V�����ɂ�����]���ʁB
            /// ������TotalUP/TotalDL�͖����O����TrsUpMon/TrsDlMon�ɑޔ���O�N���A����B
            /// 

            /// <summary>
            /// UP�]����/day
            /// </summary>
            public ulong TrsUpDay;
            /// <summary>
            /// DL�]����/day
            /// </summary>
            public ulong TrsDlDay;
            /// <summary>
            /// UP�]����/mon
            /// </summary>
            public ulong TrsUpMon;
            /// <summary>
            /// DL�]����/mon
            /// </summary>
            public ulong TrsDlMon;
            /// <summary>
            /// �ŏI�X�V����
            /// </summary>
            public string LastUpdate;
        }
        static public LOG Log;

        /// <summary>
        /// �A�N�Z�X�����n�f�[�^
        /// </summary>
        public struct ACL
        {
            /// <summary>
            /// HP�A�N�Z�X�֎~�����[�g�z�X�g
            /// </summary>
            public List<string> HpDenyRemoteHost;

            /// <summary>
            /// �ڑ��֎~ImportURL���X�g
            /// </summary>
            public List<string> DenyImportURL;

            /// <summary>
            /// ����ImportURL�ڑ�����
            /// 0:�����Ȃ� 1�`:�w�萔�܂ŋ���
            /// </summary>
            public uint LimitSameImportURL;

            ///<summary>
            /// �����ԃC���|�[�g�ڑ������ؒf
            /// 0:�����ؒf���Ȃ� 1�`:�w�莞��(��)�o�ߌ㎩���ؒf
            /// </summary>
            public uint ImportOutTime;

            ///<summary>
            /// �N���C�A���g�������L���t���O
            /// </summary>
            public bool ClientOutCheck;
            ///<summary>
            /// �����N���C�A���g��
            /// </summary>
            public uint ClientOutNum;
            ///<summary>
            /// �����N���C�A���g����
            /// </summary>
            public uint ClientOutTime;
            /// <summary>
            /// �ݒ�҂�IP���N���C�A���g���Ɋ܂߂邩
            /// true:�܂߂Ȃ� false:�܂�
            /// </summary>
            public bool ClientNotIPCheck;

            /// <summary>
            /// ����N���C�A���g�ڑ�����
            /// 0:�����Ȃ� 1�`:�w�萔�܂ŋ���
            /// </summary>
            public uint LimitSameClient;

            /// <summary>
            /// �ҋ@���|�[�g������Ύ���������Ȃ�
            /// </summary>
            public bool PortFullOnlyCheck;

            /// <summary>
            /// �C���|�[�gURL�Ɛݒ��IP�̈�v�`�F�b�N�L���t���O
            /// </summary>
            public bool SetUserIpCheck;
        }
        static public ACL Acl;

        /// <summary>
        /// �ш搧���֘A�f�[�^
        /// </summary>
        public struct BNDWTH
        {
            /// <summary>
            /// �ш搧�����s���t���O
            /// </summary>
            public bool EnableBandWidth;
            /// <summary>
            /// �ш搧�����[�h���
            /// </summary>
            public uint BandStopMode;
            /// <summary>
            /// �ш搧���l
            /// </summary>
            public uint BandStopValue;
            /// <summary>
            /// �ш搧���l�P��
            /// </summary>
            public uint BandStopUnit;
            /// <summary>
            /// �ш搧�����̃��U�[�u���l���t���O
            /// </summary>
            public bool BandStopResv;
        }
        static public BNDWTH BndWth;

        /// <summary>
        /// �X�P�W���[���N��
        /// </summary>
        static public List<SCHEDULE> ScheduleItem = new List<SCHEDULE>();
        #endregion

        #region �ݒ��ۑ����Ȃ������o
        /// <summary>
        /// �A�v���P�[�V������
        /// </summary>
        static public string AppName;
        /// <summary>
        /// �S�̂�CPU�g�p��
        /// </summary>
        static public PerformanceCounter CPU_ALL;
        /// <summary>
        /// �����݂��CPU�g�p��
        /// </summary>
        static public PerformanceCounter CPU_APP;
        /// <summary>
        /// �����݂�N������
        /// </summary>
        static public DateTime StartTime = DateTime.Now;
        /// <summary>
        /// �O���[�o��IP
        /// </summary>
        static public string GlobalIP = "";
        /// <summary>
        /// ����O���[�o��IP���擾�o���鎞��
        /// </summary>
        static public DateTime GlobalIPGetTime = DateTime.Now;
        /// <summary>
        /// �V�K�ڑ������t���O
        /// </summary>
        static public bool Pause = false;
        /// <summary>
        /// UserAgent
        /// </summary>
        static public string UserAgent = "";
        /// <summary>
        /// ���O�o�̓��[�h
        /// 0:���ׂẴ��O�\��
        /// 1:�d�v���O�̂ݕ\��
        /// </summary>
        static public int LogMode = 0;

        // clientView,ClientItem��ListViewItem��Index
        /// <summary>
        /// clmClientViewID.DisplayIndex
        /// </summary>
        static public int clmCV_ID_IDX = 0;
        /// <summary>
        /// clmClientViewIpHost.DisplayIndex
        /// </summary>
        static public int clmCV_IH_IDX = 0;
        /// <summary>
        /// clmClientViewUA.DisplayIndex
        /// </summary>
        static public int clmCV_UA_IDX = 0;
        /// <summary>
        /// clmClientViewTime.DisplayIndex
        /// </summary>
        static public int clmCV_TM_IDX = 0;
        /// <summary>
        /// clmClientView_internal_IP.DisplayIndex
        /// </summary>
        static public int clmCV_IP_IDX = 0;
        /// <summary>
        /// clmClientView_internal_HOST.DisplayIndex
        /// </summary>
        static public int clmCV_HO_IDX = 0;

        /// <summary>
        /// ���C���X�^���X�ꗗ
        /// </summary>
        static public List<Kagami> KagamiList = new List<Kagami>();

        /// <summary>
        /// KickIP�Ǘ����X�g
        /// </summary>
        static public Dictionary<String, String> KickList = new Dictionary<string,string>();

        /// <summary>
        /// ��UP�]����
        /// </summary>
        static public ulong TotalUP = 0;
        /// <summary>
        /// ��DL�]����
        /// </summary>
        static public ulong TotalDL = 0;

        /// <summary>
        /// HP�J�n��ԃt���O
        /// </summary>
        static public bool HPStart = false;
        /// <summary>
        /// HP�pHybridDSP�C���X�^���X
        /// </summary>
        static public HttpHybrid WebEntrance = new HttpHybrid();

        // ASF GUID
        static public Dictionary<string, string> ASF_GUID = new Dictionary<string, string>();

        /// <summary>
        /// ���l���b�Z�[�W
        /// </summary>
        static public string BusyString = "HTTP/1.0 503 Service Unavailable\r\n" +
            "Server: Rex/10.0.0.3650\r\n" +
            "Cache-Control: no-cache\r\n" +
            "Pragma: no-cache\r\n" +
            "Pragma: client-id=0\r\n" +
            "X-Server: " + Front.AppName + "\r\n" +
            "Content-Type: text/html\r\n\r\n" +
            "<html><head><title>503 Service Unavailable</title></head>\r\n" +
            "<body><h1>503 Service Unavailable</h1></body></html>\r\n";
        /// <summary>
        /// �ш搧������
        /// </summary>
        static public string[] BandStopTypeString = {
            "�S�|�[�g���v�Őݒ�",
            "�P�|�[�g�ӂ�Őݒ�",
            "�|�[�g���Ɍʐݒ�" };
        /// <summary>
        /// �ш搧���l�P��
        /// </summary>
        static public string[] BandStopUnitString = {
            "KB/s",
            "MB/s",
            "Kbps",
            "Mbps" };
        /// <summary>
        /// �X�P�W���[���N���C�x���g
        /// </summary>
        static public string[] ScheduleEventString = {
            "�G���g�����X�N��",
            "�G���g�����X��~",
            "�V�K��t�J�n",
            "�V�K��t��~",
            "�����ؒf",
            "�|�[�g�Ҏ�J�n",
            "�|�[�g�Ҏ��~",
            "�ڑ��g���ύX"
        };
        static public string[] ScheduleWeekString = {
            "���j",
            "���j",
            "�Ηj",
            "���j",
            "�ؗj",
            "���j",
            "�y�j",
            "����",
            "����",
            "�y��"
        };
        static public string[] ScheduleTrfTypeString = {
            "���ԑ��]����",
            "���ԑ��]����"
        };
        static public string[] ScheduleTrfUnitString = {
            "MB",
            "GB"
        };
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        static Front()
        {
            CPU_ALL = new PerformanceCounter();
            CPU_ALL.CategoryName = "Processor";
            CPU_ALL.CounterName = "% Processor Time";
            CPU_ALL.InstanceName = "_Total";
            CPU_APP = new PerformanceCounter();
            CPU_APP.CategoryName = "Process";
            CPU_APP.CounterName = "% Processor Time";
#if DEBUG
            CPU_APP.InstanceName = "Kagamin2.vshost";
#else
            CPU_APP.InstanceName = "Kagamin2";
#endif
            #region ASF_HEADER_GUID�̐ݒ�
            // Top-Lv
            ASF_GUID.Add("3026B2758E66CF11A6D900AA0062CE6C", "ASF_Header_Object");
            ASF_GUID.Add("3626B2758E66CF11A6D900AA0062CE6C", "ASF_Data_Object");
            ASF_GUID.Add("90080033B1E5CF1189F400A0C90349CB", "ASF_Simple_Index_Object");
            ASF_GUID.Add("D329E2D6DA35D111903400A0C90349BE", "ASF_Index_Object");
            ASF_GUID.Add("F803B1FEAD12644C840F2A1D2F7AD48C", "ASF_Media_Object_Index_Object");
            ASF_GUID.Add("D03FB73C4A0C0348953DEDF7B6228F0C", "ASF_Timecode_Index_Object");
            // Header-Object
            ASF_GUID.Add("A1DCAB8C47A9CF118EE400C00C205365", "ASF_File_Properties_Object");
            ASF_GUID.Add("9107DCB7B7A9CF118EE600C00C205365", "ASF_Stream_Properties_Object");
            ASF_GUID.Add("B503BF5F2EA9CF118EE300C00C205365", "ASF_Header_Extension_Object");
            ASF_GUID.Add("4052D1861D31D011A3A400A0C90348F6", "ASF_Codec_List_Object");
            ASF_GUID.Add("301AFB1E620BD011A39B00A0C90348F6", "ASF_Script_Command_Object");
            ASF_GUID.Add("01CD87F451A9CF118EE600C00C205365", "ASF_Marker_Object");
            ASF_GUID.Add("DC29E2D6DA35D111903400A0C90349BE", "ASF_Bitrate_Mutual_Exclusion_Object");
            ASF_GUID.Add("3526B2758E66CF11A6D900AA0062CE6C", "ASF_Error_Correction_Object");
            ASF_GUID.Add("3326B2758E66CF11A6D900AA0062CE6C", "ASF_Content_Description_Object");
            ASF_GUID.Add("40A4D0D207E3D21197F000A0C95EA850", "ASF_Extended_Content_Description_Object");
            ASF_GUID.Add("FAB3112223BDD211B4B700A0C955FC6E", "ASF_Content_Branding_Object");
            ASF_GUID.Add("CE75F87B8D46D1118D82006097C9A2B2", "ASF_Stream_Bitrate_Properties_Object");
            ASF_GUID.Add("FBB3112223BDD211B4B700A0C955FC6E", "ASF_Content_Encryption_Object");
            ASF_GUID.Add("14E68A292226174CB935DAE07EE9289C", "ASF_Extended_Content_Encryption_Object");
            ASF_GUID.Add("FCB3112223BDD211B4B700A0C955FC6E", "ASF_Digital_Signature_Object");
            ASF_GUID.Add("74D40618DFCA0945A4BA9AABCB96AAE8", "ASF_Padding_Object");
            // Header-Extension-Object
            ASF_GUID.Add("CBA5E61472C632438399A96952065B5A", "ASF_Extended_Stream_Properties_Object");
            ASF_GUID.Add("CF4986A0754770468A166E35357566CD", "ASF_Advanced_Mutual_Exclusion_Object");
            ASF_GUID.Add("405A46D1795A3843B71BE36B8FD6C249", "ASF_Group_Mutual_Exclusion_Object");
            ASF_GUID.Add("5BD1FED4D3884F4581F0ED5C45999E24", "ASF_Stream_Prioritization_Object");
            ASF_GUID.Add("E60996A67B51D211B6AF00C04FD908E9", "ASF_Bandwidth_Sharing_Object");
            ASF_GUID.Add("A946437CE0EFFC4BB229393EDE415C85", "ASF_Language_List_Object");
            ASF_GUID.Add("EACBF8C5AF5B77488467AA8C44FA4CCA", "ASF_Metadata_Object");
            ASF_GUID.Add("941C23449894D149A1411D134E457054", "ASF_Metadata_Library_Object");
            ASF_GUID.Add("DF29E2D6DA35D111903400A0C90349BE", "ASF_Index_Parameters_Object");
            ASF_GUID.Add("AD3B206B113FE448ACA8D7613DE2CFA7", "ASF_Media_Object_Index_Parameters_Object");
            ASF_GUID.Add("6D495EF597975D4B8C8B604DFE9BFB24", "ASF_Timecode_Index_Parameters_Object");
            ASF_GUID.Add("338505438169E6499B74AD12CB86D58C", "ASF_Advanced_Content_Encryption_Object");
            // StreamType-Object
            ASF_GUID.Add("409E69F84D5BCF11A8FD00805F5C442B", "ASF_Audio_Media");
            ASF_GUID.Add("C0EF19BC4D5BCF11A8FD00805F5C442B", "ASF_Video_Media");
            ASF_GUID.Add("C0CFDA59E659D011A3AC00A0C90348F6", "ASF_Command_Media");
            ASF_GUID.Add("00E11BB64E5BCF11A8FD00805F5C442B", "ASF_JFIF_Media");
            ASF_GUID.Add("E07D903515E4CF11A91700805F5C442B", "ASF_Degradable_JPEG_Media");
            ASF_GUID.Add("2C22BD911CF27A498B6D5AA86BFC0185", "ASF_File_Transfer_Media");
            ASF_GUID.Add("E265FB3AEF47F240AC2C70A90D71D343", "ASF_Binary_Media");
            #endregion
        }

        #region �ݒ�̓ǂݏ���
        /// <summary>
        /// �ݒ�ǂݏo��
        /// </summary>
        static public void LoadSetting()
        {
            // �E�C���h�E�ʒu�E�T�C�Y���ǂݏo���Ȃ������ꍇ�̏����l�v�Z
            int x = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - Form.W) / 2; if (x < 0) x = 0;
            int y = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - Form.H) / 2; if (y < 0) y = 0;
            int w = Form.W;
            int h = Form.H;

            string iniFile = System.Windows.Forms.Application.StartupPath + "/setting.ini";
            StringBuilder sb = new StringBuilder(1024);

            IniFileHandler.GetPrivateProfileString(                         "ACL",      "DENY_HP_IP",           "",     sb, (uint)sb.Capacity, iniFile);    Acl.HpDenyRemoteHost = new List<string>(sb.ToString().Split(','));
            IniFileHandler.GetPrivateProfileString(                         "ACL",      "DENY_IMPORT_URL",      "127.0.0.1,localhost", sb, (uint)sb.Capacity, iniFile); Acl.DenyImportURL = new List<string>(sb.ToString().Split(','));
            Acl.LimitSameImportURL  = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_SAME_IMPORT",    2,      iniFile);
            Acl.ImportOutTime       = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_IMPORT_OUT",     0,      iniFile);
            Acl.ClientOutCheck      = IniFileHandler.GetPrivateProfileInt(  "ACL",      "ENABLE_CLIENT_OUT",    0,      iniFile) != 0 ? true : false;
            Acl.ClientOutNum        = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_CLIENT_NUM",     0,      iniFile);
            Acl.ClientOutTime       = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_CLIENT_TIME",    10,     iniFile);
            Acl.ClientNotIPCheck    = IniFileHandler.GetPrivateProfileInt(  "ACL",      "ENABLE_CLIENT_NOTIP",  0,      iniFile) != 0 ? true : false;
            Acl.LimitSameClient     = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_SAME_CLIENT",    0,      iniFile);
            Acl.PortFullOnlyCheck   = IniFileHandler.GetPrivateProfileInt(  "ACL",      "PORT_FULL_ONLY",       0,      iniFile) != 0 ? true : false;
            Acl.SetUserIpCheck      = IniFileHandler.GetPrivateProfileInt(  "ACL",      "SET_USER_IP_CHECK",    0,      iniFile) != 0 ? true : false;

            BndWth.EnableBandWidth  = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "ENABLE",               0,      iniFile) != 0 ? true : false;
            BndWth.BandStopMode     = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "MODE",                 0,      iniFile);
            BndWth.BandStopValue    = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "VALUE",                5000,   iniFile);
            BndWth.BandStopUnit     = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "UNIT",                 0,      iniFile);
            BndWth.BandStopResv     = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "RESV_FLAG",            0,      iniFile) != 0 ? true : false;

            // ���W�ƃT�C�Y�̓}�C�i�X�����肦��H
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "X",                    x.ToString(),   sb, (uint)sb.Capacity, iniFile);    try { Form.X = int.Parse(sb.ToString()); } catch { Form.X = 0; }
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "Y",                    y.ToString(),   sb, (uint)sb.Capacity, iniFile);    try { Form.Y = int.Parse(sb.ToString()); } catch { Form.Y = 0; }
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "W",                    w.ToString(),   sb, (uint)sb.Capacity, iniFile);    try { Form.W = int.Parse(sb.ToString()); } catch { Form.W = 600; }
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "H",                    h.ToString(),   sb, (uint)sb.Capacity, iniFile);    try { Form.H = int.Parse(sb.ToString()); } catch { Form.H = 400; }
            Form.SplitDistance1     = IniFileHandler.GetPrivateProfileInt(  "FORM",     "SPLIT1",               190,    iniFile);
            Form.SplitDistance2     = IniFileHandler.GetPrivateProfileInt(  "FORM",     "SPLIT2",               195,    iniFile);
            Form.LeftPanelCollapsed = IniFileHandler.GetPrivateProfileInt(  "FORM",     "LP_COLLAPSED",         1,      iniFile) != 0 ? true : false;
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_KAGAMI",           "",     sb, (uint)sb.Capacity, iniFile);    Form.KagamiListColumn   = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_MONALL",           "",     sb, (uint)sb.Capacity, iniFile);    Form.MonAllViewColumn   = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_MON",              "",     sb, (uint)sb.Capacity, iniFile);    Form.MonViewColumn      = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_CLIENT",           "",     sb, (uint)sb.Capacity, iniFile);    Form.ClientViewColumn   = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_RESERVE",          "",     sb, (uint)sb.Capacity, iniFile);    Form.ResvViewColumn     = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_KICK",             "",     sb, (uint)sb.Capacity, iniFile);    Form.KickViewColumn     = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_LOG",              "",     sb, (uint)sb.Capacity, iniFile);    Form.LogViewColumn      = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_SCHEDULE",         "",     sb, (uint)sb.Capacity, iniFile);    Form.ScheduleColumn     = sb.ToString();
            Form.monViewUnit        = IniFileHandler.GetPrivateProfileInt(  "FORM",     "MON_UNIT",             0,      iniFile);
            Form.IconIndex          = IniFileHandler.GetPrivateProfileInt(  "FORM",     "ICON",                 0,      iniFile);
            if (Form.IconIndex >= 3) Form.IconIndex = 0;    // �O�̂��߃K�[�h
            Form.EnableTrayIcon     = IniFileHandler.GetPrivateProfileInt(  "FORM",     "ENABLE_TRAY_ICON",     1,      iniFile) != 0 ? true : false;


            IniFileHandler.GetPrivateProfileString(                         "GUI",      "IMPORT_URL",           "",     sb, (uint)sb.Capacity, iniFile);    Gui.ImportURL = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "GUI",      "FAVORITE_LIST",        "",     sb, (uint)sb.Capacity, iniFile);    Gui.FavoriteList = new List<string>(sb.ToString().Split(','));
            IniFileHandler.GetPrivateProfileString(                         "GUI",      "PORT_LIST",            "8080", sb, (uint)sb.Capacity, iniFile);
            Gui.PortList = new List<int>();
            foreach (string s in sb.ToString().Split(','))
            {
                try
                {
                    int i = int.Parse(s);
                    Gui.PortList.Add(i);
                }
                catch { }
            }
            Gui.Conn                = IniFileHandler.GetPrivateProfileInt(  "GUI",      "CONN",                 50,     iniFile);
            Gui.Reserve             = IniFileHandler.GetPrivateProfileInt(  "GUI",      "RESV",                 3,      iniFile);

            Hp.UseHP                = IniFileHandler.GetPrivateProfileInt(  "HP",       "USE_HP",               0,      iniFile) != 0 ? true : false;
            IniFileHandler.GetPrivateProfileString(                         "HP",       "HOSTNAME",             "http://localhost", sb, (uint)sb.Capacity, iniFile);    Hp.IpHTTP = sb.ToString();
            Hp.PortHTTP             = IniFileHandler.GetPrivateProfileInt(  "HP",       "PORT",                 8888,   iniFile);
            IniFileHandler.GetPrivateProfileString(                         "HP",       "PUBLIC_DIR",           "",     sb, (uint)sb.Capacity, iniFile);    Hp.PublicDir = sb.ToString();
#if DEBUG
            IniFileHandler.GetPrivateProfileString(                         "HP",       "IP_CHECK_URL",         "http://kagami.homelinux.net/ip.cgi",  sb, (uint)sb.Capacity, iniFile);    Hp.IPCheckURL = sb.ToString();
#else
            IniFileHandler.GetPrivateProfileString(                         "HP",       "IP_CHECK_URL",         "http://taruo.net/ip/",                sb, (uint)sb.Capacity, iniFile);    Hp.IPCheckURL = sb.ToString();
#endif
            Hp.IPCheckLine          = IniFileHandler.GetPrivateProfileInt(  "HP",       "IP_CHECK_LINE",        4,      iniFile);

            Kick.KickCheckSecond    = IniFileHandler.GetPrivateProfileInt(  "KICK",     "CHECK_SECOND",         10,     iniFile);
            Kick.KickCheckTime      = IniFileHandler.GetPrivateProfileInt(  "KICK",     "CHECK_TIME",           7,      iniFile);
            Kick.KickDenyTime       = IniFileHandler.GetPrivateProfileInt(  "KICK",     "DENY_TIME",            60,     iniFile);
            
            IniFileHandler.GetPrivateProfileString(                         "LOG",      "FILE1",                "",     sb, (uint)sb.Capacity, iniFile);    Log.HpLogFile = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "LOG",      "FILE2",                "",     sb, (uint)sb.Capacity, iniFile);    Log.KagamiLogFile = sb.ToString();
            Log.LogDetail           = IniFileHandler.GetPrivateProfileInt(  "LOG",      "ENABLE_DETAIL",        0,      iniFile) != 0 ? true : false;
            IniFileHandler.GetPrivateProfileString(                         "LOG",      "TRS_UP_DAY",           "0",    sb, (uint)sb.Capacity, iniFile);    Log.TrsUpDay = ulong.Parse(sb.ToString());
            IniFileHandler.GetPrivateProfileString(                         "LOG",      "TRS_DL_DAY",           "0",    sb, (uint)sb.Capacity, iniFile);    Log.TrsDlDay = ulong.Parse(sb.ToString());
            IniFileHandler.GetPrivateProfileString(                         "LOG",      "TRS_UP_MON",           "0",    sb, (uint)sb.Capacity, iniFile);    Log.TrsUpMon = ulong.Parse(sb.ToString());
            IniFileHandler.GetPrivateProfileString(                         "LOG",      "TRS_DL_MON",           "0",    sb, (uint)sb.Capacity, iniFile);    Log.TrsDlMon = ulong.Parse(sb.ToString());
            IniFileHandler.GetPrivateProfileString(                         "LOG",      "LAST_UPDATE",          "",     sb, (uint)sb.Capacity, iniFile);    Log.LastUpdate = sb.ToString();

            Opt.BrowserView         = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_BROWSER_VIEW",  1,      iniFile) != 0 ? true : false;
            Opt.BrowserViewMode     = IniFileHandler.GetPrivateProfileInt(  "OPT",      "BROWSER_MODE",         0,      iniFile) != 0 ? true : false;
            Opt.PriKagamiexe        = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_KAGAMIEXE",     0,      iniFile) != 0 ? true : false;
            Opt.PriKagamin          = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_KAGAMIN",       0,      iniFile) != 0 ? true : false;
            Opt.BalloonTip          = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_BALLOONTIP",    0,      iniFile) != 0 ? true : false;
            Opt.EnablePush          = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_PUSH",          0,      iniFile) != 0 ? true : false;
            Opt.EnableInfo          = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_INFO",          0,      iniFile) != 0 ? true : false;
            Opt.EnableAdmin         = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_ADMIN",         0,      iniFile) != 0 ? true : false;
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "ADMIN_PASS",           "",     sb, (uint)sb.Capacity, iniFile);    Opt.AdminPass = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "SOUND_CONN_OK",        "",     sb, (uint)sb.Capacity, iniFile);    Opt.SndConnOkFile = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "SOUND_CONN_NG",        "",     sb, (uint)sb.Capacity, iniFile);    Opt.SndConnNgFile = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "SOUND_DISC",           "",     sb, (uint)sb.Capacity, iniFile);    Opt.SndDiscFile = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "OUT_URL",              "http://live24.2ch.net/test/read.cgi/livevenus", sb, (uint)sb.Capacity, iniFile); Opt.OutUrl = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "APP_NAME",             "",     sb, (uint)sb.Capacity, iniFile);    Opt.AppName = sb.ToString();
            Opt.EnableResolveHost   = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_RESOLVE_HOST",  0,      iniFile) != 0 ? true : false;

            Retry.InRetryInterval   = IniFileHandler.GetPrivateProfileInt(  "RETRY",    "IN_INTERVAL",          10,     iniFile);
            Retry.InRetryTime       = IniFileHandler.GetPrivateProfileInt(  "RETRY",    "IN_TIME",              5,      iniFile);
            Retry.OutRetryInterval  = IniFileHandler.GetPrivateProfileInt(  "RETRY",    "OUT_INTERVAL",         10,     iniFile);
            Retry.OutRetryTime      = IniFileHandler.GetPrivateProfileInt(  "RETRY",    "OUT_TIME",             5,      iniFile);

            Sock.SockConnTimeout    = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "CONN_TO",              2000,   iniFile);
            Sock.SockRecvTimeout    = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "RECV_TO",              10000,  iniFile);
            Sock.SockSendTimeout    = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "SEND_TO",              2000,   iniFile);
            Sock.SockSendQueueSize  = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "QUEUE_SIZE",           200,    iniFile);
            Sock.SockCloseDelay     = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "CLOSE_DELAY",          5000,   iniFile);

            uint _sch_num            = IniFileHandler.GetPrivateProfileInt( "SCH",      "NUM",                  0,      iniFile);
            for (uint _num = 0; _num < _sch_num; _num++)
            {
                IniFileHandler.GetPrivateProfileString("SCH", _num.ToString(), "", sb, (uint)sb.Capacity, iniFile);
                string[] _data        = sb.ToString().Split(',');
                if (_data.Length == 12)
                {
                    try
                    {
                        SCHEDULE _item = new SCHEDULE();
                        _item.Enable = _data[0] != "0" ? true : false;
                        _item.Event = uint.Parse(_data[1]);
                        _item.Port = uint.Parse(_data[2]);
                        _item.StartType = uint.Parse(_data[3]);
                        _item.Week = uint.Parse(_data[4]);
                        _item.Hour = uint.Parse(_data[5]);
                        _item.Min = uint.Parse(_data[6]);
                        _item.TrfType = uint.Parse(_data[7]);
                        _item.TrfValue = uint.Parse(_data[8]);
                        _item.TrfUnit = uint.Parse(_data[9]);
                        _item.Conn = uint.Parse(_data[10]);
                        _item.Resv = uint.Parse(_data[11]);
                        _item.ExecTrf = false;
                        ScheduleItem.Add(_item);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// �ݒ�ۑ�
        /// </summary>
        static public void SaveSetting()
        {
            string iniFile = System.Windows.Forms.Application.StartupPath + "/setting.ini";
            string str = "";

            // List<string> �� �J���}��؂�string�ϊ�
            foreach (string s in Acl.HpDenyRemoteHost)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // �]�v�Ȗ����̃J���}����
            IniFileHandler.WritePrivateProfileString("ACL", "DENY_HP_IP", str, iniFile);

            // List<string> �� �J���}��؂�string�ϊ�
            str = "";
            foreach (string s in Acl.DenyImportURL)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // �]�v�Ȗ����̃J���}����
            IniFileHandler.WritePrivateProfileString("ACL", "DENY_IMPORT_URL", str, iniFile);

            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_SAME_IMPORT", Acl.LimitSameImportURL.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_IMPORT_OUT", Acl.ImportOutTime.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "ENABLE_CLIENT_OUT", Acl.ClientOutCheck ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "PORT_FULL_ONLY", Acl.PortFullOnlyCheck ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "ENABLE_CLIENT_NOTIP", Acl.ClientNotIPCheck ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_CLIENT_NUM", Acl.ClientOutNum.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_CLIENT_TIME", Acl.ClientOutTime.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_SAME_CLIENT", Acl.LimitSameClient.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "SET_USER_IP_CHECK", Acl.SetUserIpCheck ? "1" : "0", iniFile);

            IniFileHandler.WritePrivateProfileString("BNDWTH", "ENABLE", BndWth.EnableBandWidth ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("BNDWTH", "MODE", BndWth.BandStopMode.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("BNDWTH", "VALUE", BndWth.BandStopValue.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("BNDWTH", "UNIT", BndWth.BandStopUnit.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("BNDWTH", "RESV_FLAG", BndWth.BandStopResv ? "1" : "0", iniFile);

            IniFileHandler.WritePrivateProfileString("FORM", "X", Form.X.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "Y", Form.Y.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "W", Form.W.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "H", Form.H.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "SPLIT1", Form.SplitDistance1.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "SPLIT2", Form.SplitDistance2.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "LP_COLLAPSED", Form.LeftPanelCollapsed ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_KAGAMI", Form.KagamiListColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_MONALL", Form.MonAllViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_MON", Form.MonViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_CLIENT", Form.ClientViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_RESERVE", Form.ResvViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_KICK", Form.KickViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_LOG", Form.LogViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_SCHEDULE", Form.ScheduleColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "MON_UNIT", Form.monViewUnit.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "ICON", Form.IconIndex.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "ENABLE_TRAY_ICON", Form.EnableTrayIcon ? "1" : "0", iniFile);

            // List<string> �� �J���}��؂�string�ϊ�
            str = "";
            foreach (string s in Gui.FavoriteList)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // �]�v�Ȗ����̃J���}����
            IniFileHandler.WritePrivateProfileString("GUI", "FAVORITE_LIST", str, iniFile);

            // List<int> �� �J���}��؂�string�ϊ�
            str = "";
            foreach (int s in Gui.PortList)
                str += s.ToString() + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // �]�v�Ȗ����̃J���}����
            IniFileHandler.WritePrivateProfileString("GUI", "PORT_LIST",    str,                    iniFile);
            IniFileHandler.WritePrivateProfileString("GUI", "IMPORT_URL",   Gui.ImportURL,          iniFile);
            IniFileHandler.WritePrivateProfileString("GUI", "CONN",         Gui.Conn.ToString(),    iniFile);
            IniFileHandler.WritePrivateProfileString("GUI", "RESV",         Gui.Reserve.ToString(), iniFile);

            IniFileHandler.WritePrivateProfileString("HP", "USE_HP",                Hp.UseHP ? "1" : "0",       iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "HOSTNAME",              Hp.IpHTTP,                  iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "PORT",                  Hp.PortHTTP.ToString(),     iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "PUBLIC_DIR",            Hp.PublicDir,               iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "IP_CHECK_URL",          Hp.IPCheckURL,              iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "IP_CHECK_LINE",         Hp.IPCheckLine.ToString(),  iniFile);

            IniFileHandler.WritePrivateProfileString("KICK", "CHECK_SECOND",        Kick.KickCheckSecond.ToString(),    iniFile);
            IniFileHandler.WritePrivateProfileString("KICK", "CHECK_TIME",          Kick.KickCheckTime.ToString(),      iniFile);
            IniFileHandler.WritePrivateProfileString("KICK", "DENY_TIME",           Kick.KickDenyTime.ToString(),       iniFile);

            IniFileHandler.WritePrivateProfileString("LOG", "FILE1",                Log.HpLogFile,              iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "FILE2",                Log.KagamiLogFile,          iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "ENABLE_DETAIL",        Log.LogDetail ? "1" : "0",  iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "TRS_UP_DAY",           Log.TrsUpDay.ToString(),    iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "TRS_DL_DAY",           Log.TrsDlDay.ToString(),    iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "TRS_UP_MON",           Log.TrsUpMon.ToString(),    iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "TRS_DL_MON",           Log.TrsDlMon.ToString(),    iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "LAST_UPDATE",          Log.LastUpdate,             iniFile);

            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_BROWSER_VIEW",  Opt.BrowserView     ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "BROWSER_MODE",         Opt.BrowserViewMode ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_KAGAMIEXE",     Opt.PriKagamiexe    ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_KAGAMIN",       Opt.PriKagamin      ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_BALLOONTIP",    Opt.BalloonTip      ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_PUSH",          Opt.EnablePush      ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_INFO",          Opt.EnableInfo      ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_ADMIN",         Opt.EnableAdmin     ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ADMIN_PASS",           Opt.AdminPass,                   iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "SOUND_CONN_OK",        Opt.SndConnOkFile,               iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "SOUND_CONN_NG",        Opt.SndConnNgFile,               iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "SOUND_DISC",           Opt.SndDiscFile,                 iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "OUT_URL",              Opt.OutUrl,                      iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "APP_NAME",             Opt.AppName,                     iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_RESOLVE_HOST",  Opt.EnableResolveHost ? "1" : "0", iniFile);

            IniFileHandler.WritePrivateProfileString("RETRY", "IN_INTERVAL",    Retry.InRetryInterval.ToString(),   iniFile);
            IniFileHandler.WritePrivateProfileString("RETRY", "IN_TIME",        Retry.InRetryTime.ToString(),       iniFile);
            IniFileHandler.WritePrivateProfileString("RETRY", "OUT_INTERVAL",   Retry.OutRetryInterval.ToString(),  iniFile);
            IniFileHandler.WritePrivateProfileString("RETRY", "OUT_TIME",       Retry.OutRetryTime.ToString(),      iniFile);

            IniFileHandler.WritePrivateProfileString("SOCK", "CONN_TO",     Sock.SockConnTimeout.ToString(),    iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "RECV_TO",     Sock.SockRecvTimeout.ToString(),    iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "SEND_TO",     Sock.SockSendTimeout.ToString(),    iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "QUEUE_SIZE",  Sock.SockSendQueueSize.ToString(),  iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "CLOSE_DELAY", Sock.SockCloseDelay.ToString(),     iniFile);

            int _sch_num = ScheduleItem.Count;
            IniFileHandler.WritePrivateProfileString("SCH", "NUM", _sch_num.ToString(), iniFile);
            for (int _num = 0; _num < _sch_num; _num++)
            {
                string _s = (ScheduleItem[_num].Enable ? "1" : "0")
                    + "," + ScheduleItem[_num].Event
                    + "," + ScheduleItem[_num].Port
                    + "," + ScheduleItem[_num].StartType
                    + "," + ScheduleItem[_num].Week
                    + "," + ScheduleItem[_num].Hour
                    + "," + ScheduleItem[_num].Min
                    + "," + ScheduleItem[_num].TrfType
                    + "," + ScheduleItem[_num].TrfValue
                    + "," + ScheduleItem[_num].TrfUnit
                    + "," + ScheduleItem[_num].Conn
                    + "," + ScheduleItem[_num].Resv;
                IniFileHandler.WritePrivateProfileString("SCH", _num.ToString(), _s, iniFile);
            }

            /* �ǂݏ����T���v��
            
            // �L�[�ƒl������������
            IniFileHandler.WritePrivateProfileString("�A�v��1", "�L�[1", "�n���[", @"c:\sample.ini");
            IniFileHandler.WritePrivateProfileString("�A�v��1", "�L�[2", "1234", @"c:\sample.ini");
            IniFileHandler.WritePrivateProfileString("�A�v��2", "�L�[1", "good morning", @"c:\sample.ini");

            // �������ǂݏo��
            StringBuilder sb = new StringBuilder(1024);
            IniFileHandler.GetPrivateProfileString("�A�v��1", "�L�[1", "default", sb, (uint)sb.Capacity, @"c:\sample.ini");
            Console.WriteLine("�A�v��1�Z�N�V�����Ɋ܂܂��L�[1�̒l: {0}", sb.ToString());

            // �����l��ǂݏo��
            uint resultValue = IniFileHandler.GetPrivateProfileInt("�A�v��1", "�L�[2", 0, @"c:\sample.ini");
            Console.WriteLine("�A�v��1�Z�N�V�����Ɋ܂܂��L�[2�̒l: {0}", resultValue);

            // �w��Z�N�V�����̃L�[�̈ꗗ�𓾂�
            byte[] ar1 = new byte[1024];
            uint resultSize1 = IniFileHandler.GetPrivateProfileStringByByteArray("�A�v��1", null, "default", ar1, (uint)ar1.Length, @"c:\sample.ini");
            string result1 = System.Text.Encoding.Default.GetString(ar1, 0, (int)resultSize1 - 1);
            string[] keys = result1.Split('\0');
            foreach (string key in keys)
            {
                Console.WriteLine("�A�v��1�Z�N�V�����Ɋ܂܂��L�[��: {0}", key);
            }

            // �w��t�@�C���̃Z�N�V�����̈ꗗ�𓾂�
            byte[] ar2 = new byte[1024];
            uint resultSize2 = IniFileHandler.GetPrivateProfileStringByByteArray(null, null, "default", ar2, (uint)ar2.Length, @"c:\sample.ini");
            string result2 = System.Text.Encoding.Default.GetString(ar2, 0, (int)resultSize2 - 1);
            string[] sections = result2.Split('\0');
            foreach (string section in sections)
            {
                Console.WriteLine("���̃t�@�C���Ɋ܂܂��Z�N�V������: {0}", section);
            }

            // 1�̃L�[�ƒl�̃y�A���폜����
            IniFileHandler.WritePrivateProfileString("�A�v��2", "�L�[1", null, @"c:\sample.ini");

            // �w��Z�N�V�������̑S�ẴL�[�ƒl�̃y�A���폜����
            IniFileHandler.WritePrivateProfileString("�A�v��1", null, null, @"c:\sample.ini");
            */
        }
        #endregion

        #region ���̒ǉ�/�폜/�Ǐo
        /// <summary>
        /// �����X�g�Ƀf�[�^�ǉ�
        /// </summary>
        /// <param name="_k"></param>
        static public void Add(Kagami _k)
        {
            lock (KagamiList)
            {
                // �P��Add����񂶂�Ȃ��APort����Sort�����悤�ɒǉ�
                int cnt;
                for (cnt = 0; cnt < KagamiList.Count; cnt++)
                    if (KagamiList[cnt].Status.MyPort > _k.Status.MyPort)
                        break;
                KagamiList.Insert(cnt, _k);
            }
            // GUI�X�V
            Event.EventUpdateKagami();
        }

        /// <summary>
        /// �����X�g����f�[�^�폜
        /// </summary>
        /// <param name="_k"></param>
        static public void Delete(Kagami _k)
        {
            if (_k == null)
                return;
            // Add�����O��Remove���ꂽ�����l������
            // �O�̂��߃��g���C���������Ă���
            while (true)
            {
                try
                {
                    lock (KagamiList)
                    {
                        _k.Status.RunStatus = false;
                        KagamiList.Remove(_k);
                    }
                    // GUI�X�V
                    Event.EventUpdateKagami();
                    break;
                }
                catch
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
        }

        /// <summary>
        /// �S���ڑ����I������
        /// </summary>
        static public void AllStop()
        {
            while (true)
            {
                try
                {
                    foreach (Kagami _k in KagamiList)
                    {
                        _k.Stop();
                    }
                    break;
                }
                catch
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
        }

        /// <summary>
        /// �w�肳�ꂽ�|�[�g�̋��Q�ƃN���X��ԋp
        /// </summary>
        /// <param name="_port">�|�[�g�ԍ�</param>
        /// <returns></returns>
        static public Kagami IndexOf(int _port)
        {
            lock (KagamiList)
            {
                foreach (Kagami k in KagamiList)
                {
                    if (k.Status.MyPort == _port)
                        return k;
                }
                return null;
            }
        }
        #endregion

        #region �N���C�A���g�̃L�b�N�o�^
        /// <summary>
        /// KickList��̃��[�U��Kick�J�n��Ԃɂ���B
        /// KickList�ɂȂ��ꍇ�͒ǉ�����B
        /// </summary>
        /// <param name="_ip">Kick����IP</param>
        /// <param name="_deny_tim">Kick����(�b) -1�Ŗ�����</param>
        static public void AddKickUser(string _ip, int _deny_tim)
        {
            lock (KickList)
            {
                if (KickList.ContainsKey(_ip) == true)
                {
                    // KickList�o�^�ς�
                    if (_deny_tim >= 0)
                    {
                        // �����t��Kick
                        KickList[_ip] = DateTime.Now.AddSeconds(_deny_tim).ToString() + ",0";
                    }
                    else
                    {
                        // ������Kick
                        KickList[_ip] = DateTime.Now.ToString() + ",-1";
                    }
                }
                else
                {
                    // KickList�V�K�o�^
                    if (_deny_tim >= 0)
                    {
                        //�����t��Kick
                        KickList.Add(_ip, DateTime.Now.AddSeconds(_deny_tim).ToString() + ",0");
                    }
                    else
                    {
                        // ������Kick
                        KickList.Add(_ip, DateTime.Now.ToString() + ",-1");
                    }
                }
            }
        }

        /// <summary>
        /// KickList��̃��[�U��Kick������Ԃɂ���B
        /// KickList�ɂȂ��ꍇ�͒ǉ�����B
        /// </summary>
        /// <param name="_ip">Kick��������IP</param>
        static public void DelKickUser(string _ip)
        {
            lock (KickList)
            {
                if (KickList.ContainsKey(_ip) == true)
                    KickList[_ip] = DateTime.Now.ToString() + ",1";
                else
                    KickList.Add(_ip, DateTime.Now.ToString() + ",1");
            }
        }
        #endregion

        /// <summary>
        /// �O���[�o��IP���擾���ĕێ�����
        /// </summary>
        static public bool GetGlobalIP()
        {
            if (Front.Hp.IPCheckURL == "" || Front.Hp.IPCheckLine <= 0)
            {
                MessageBox.Show("�O���[�o��IP�擾�y�[�W�̐ݒ肪����������܂���", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            // 1���ȓ��̍Ď擾�̓X�L�b�v����
            if (GlobalIP != "" && GlobalIPGetTime > DateTime.Now)
                return true;
            //WebClient�̍쐬
            System.Net.WebClient wc = new System.Net.WebClient();
            //�����R�[�h���w��
            wc.Encoding = System.Text.Encoding.GetEncoding(51932);

            string str;
            try
            {
                //HTML�\�[�X���_�E�����[�h����
                str = wc.DownloadString(Front.Hp.IPCheckURL);
            }
            catch
            {
                //��n��
                wc.Dispose();
                MessageBox.Show("IP�A�h���X�m�F�p�z�X�g�ɂȂ���܂���ł���" + Environment.NewLine +
                                "URL:" + Front.Hp.IPCheckURL + Environment.NewLine,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            //��n��
            wc.Dispose();
            try
            {
                //�w�肳�ꂽ�s�����o��
                string line = str.Split('\n')[Front.Hp.IPCheckLine - 1];
                //IP�A�h���X�炵�����̂�����
                Match index = Regex.Match(line, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
                if (index.Success)
                {
                    GlobalIP = line.Substring(index.Index, index.Length);
                    GlobalIPGetTime = DateTime.Now.AddSeconds(60);   // ����擾�\�Ȃ̂͂P����
                    return true;
                }
                else
                {
                    MessageBox.Show("IP�A�h���X���擾�ł��܂���ł���\r\n" +
                                    "URL:" + Front.Hp.IPCheckURL + "\r\n" +
                                    "LINE:" + Front.Hp.IPCheckLine + "\r\n" +
                                    "STRING:" + line + "\r\n",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// �����R�[�h�𔻕ʂ���
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        static public System.Text.Encoding GetCode(byte[] bytes)
        {
            return GetCode(bytes, bytes.Length);
        }
        /// <summary>
        /// �����R�[�h�𔻕ʂ���
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        static public System.Text.Encoding GetCode(byte[] bytes,int len)
        {
            const byte bESC = 0x1B;
            const byte bAT = 0x40;
            const byte bDollar = 0x24;
            const byte bAnd = 0x26;
            const byte bOP = 0x28;    //(
            const byte bB = 0x42;
            const byte bD = 0x44;
            const byte bJ = 0x4A;
            const byte bI = 0x49;

            int binary = 0;
            int ucs2 = 0;
            int sjis = 0;
            int euc = 0;
            int utf8 = 0;
            byte b1, b2;

            for (int i = 0; i < len; i++)
            {
                if (bytes[i] <= 0x06 || bytes[i] == 0x7F || bytes[i] == 0xFF)
                {
                    //'binary'
                    binary++;
                    if (len - 1 > i && bytes[i] == 0x00
                        && i > 0 && bytes[i - 1] <= 0x7F)
                    {
                        //smells like raw unicode
                        ucs2++;
                    }
                }
            }

            if (binary > 0)
            {
                if (ucs2 > 0)
                    //JIS
                    //ucs2(Unicode)
                    return System.Text.Encoding.Unicode;
                else
                    //binary
                    return null;
            }

            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];

                if (b1 == bESC)
                {
                    if (b2 >= 0x80)
                        //not Japanese
                        //ASCII
                        return System.Text.Encoding.ASCII;
                    else if (len - 2 > i &&
                        b2 == bDollar && bytes[i + 2] == bAT)
                        //JIS_0208 1978
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 2 > i &&
                        b2 == bDollar && bytes[i + 2] == bB)
                        //JIS_0208 1983
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 5 > i &&
                        b2 == bAnd && bytes[i + 2] == bAT && bytes[i + 3] == bESC &&
                        bytes[i + 4] == bDollar && bytes[i + 5] == bB)
                        //JIS_0208 1990
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 3 > i &&
                        b2 == bDollar && bytes[i + 2] == bOP && bytes[i + 3] == bD)
                        //JIS_0212
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 2 > i &&
                        b2 == bOP && (bytes[i + 2] == bB || bytes[i + 2] == bJ))
                        //JIS_ASC
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 2 > i &&
                        b2 == bOP && bytes[i + 2] == bI)
                        //JIS_KANA
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                }
            }

            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((b1 >= 0x81 && b1 <= 0x9F) || (b1 >= 0xE0 && b1 <= 0xFC)) &&
                    ((b2 >= 0x40 && b2 <= 0x7E) || (b2 >= 0x80 && b2 <= 0xFC)))
                {
                    sjis += 2;
                    i++;
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((b1 >= 0xA1 && b1 <= 0xFE) && (b2 >= 0xA1 && b2 <= 0xFE)) ||
                    (b1 == 0x8E && (b2 >= 0xA1 && b2 <= 0xDF)))
                {
                    euc += 2;
                    i++;
                }
                else if (len - 2 > i &&
                    b1 == 0x8F && (b2 >= 0xA1 && b2 <= 0xFE) &&
                    (bytes[i + 2] >= 0xA1 && bytes[i + 2] <= 0xFE))
                {
                    euc += 3;
                    i += 2;
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if ((b1 >= 0xC0 && b1 <= 0xDF) && (b2 >= 0x80 && b2 <= 0xBF))
                {
                    utf8 += 2;
                    i++;
                }
                else if (len - 2 > i &&
                    (b1 >= 0xE0 && b1 <= 0xEF) && (b2 >= 0x80 && b2 <= 0xBF) &&
                    (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF))
                {
                    utf8 += 3;
                    i += 2;
                }
            }

            if (euc > sjis && euc > utf8)
                //EUC
                return System.Text.Encoding.GetEncoding(51932);
            else if (sjis > euc && sjis > utf8)
                //SJIS
                return System.Text.Encoding.GetEncoding(932);
            else if (utf8 > euc && utf8 > sjis)
                //UTF8
                return System.Text.Encoding.UTF8;

            // Unknown... ASCII
            return System.Text.Encoding.ASCII;
        }
        /// <summary>
        /// �ш��Kbps�œ��ꂷ��
        /// </summary>
        /// <param name="_limit"></param>
        /// <param name="_unit"></param>
        /// <returns></returns>
        static public int CnvLimit(int _limit, int _unit)
        {
            switch (BandStopUnitString[_unit])
            {
                case "KB/s":
                    return _limit * 8;
                case "MB/s":
                    return _limit * 8000;
                case "Kbps":
                    return _limit;
                case "Mbps":
                    return _limit * 1000;
                default:
                    throw new Exception("not implemented.");
            }
        }

        #region ���O�o��
        /// <summary>
        /// �ʏ�̃��O�o��
        /// logLv=0 ��ʃ��O�o��
        /// logLv=1 �d�v���O�o��
        /// 
        /// ����:
        /// �I�u�W�F�N�g��lock�����܂ܖ{���\�b�h���N�����A
        /// GUI�X���b�h�ł�����I�u�W�F�N�g��lock�҂��ƂȂ����ꍇ�A
        /// GUI�X���b�h�ւ�Invoke�ʐM���n�܂炸��
        /// �f�b�h���b�N�ɂȂ�\��������̂Œ��ӂ��邱��
        /// </summary>
        /// <param name="_logLv">�d�v�x</param>
        /// <param name="_status">������Status</param>
        /// <param name="_content">�o�̓��b�Z�[�W</param>
        static public void AddLogData(int _logLv, Status _status, string _content)
        {
            DateTime _dtNow = DateTime.Now;
            #if DEBUG
            System.Diagnostics.Trace.WriteLine("��NORMAL" + _logLv + _dtNow.ToString("[MM/dd HH:mm:ss]") + "[" + _status.MyPort + "]" + _content);
            #endif

            try
            {
                // GUI�o�͗pLogItem�ւ̏o��
                ListViewItem _item = new ListViewItem();
                _item.Text = _dtNow.ToString("MM/dd HH:mm:ss");
                _item.SubItems.Add(_content);
                // ��ʃ��O
                lock (_status.Gui.LogAllItem)
                {
                    if (_status.Gui.LogAllItem.Count > 500)
                        _status.Gui.LogAllItem.RemoveRange(0, _status.Gui.LogAllItem.Count - 500);
                    _status.Gui.LogAllItem.Add(_item);
                }
                // �d�v���O
                if (_logLv > 0)
                {
                    lock (_status.Gui.LogImpItem)
                    {
                        if (_status.Gui.LogImpItem.Count > 500)
                            _status.Gui.LogImpItem.RemoveRange(0, _status.Gui.LogImpItem.Count - 500);
                        _status.Gui.LogImpItem.Add(_item);
                    }
                }
                // GUI�X�V
                // �o�͂��������O��Lv���A���݂�GUI�\�����[�h����Ȃ�GUI�X�V
                if (_logLv >= LogMode)
                    Event.EventUpdateLog(_logLv,_status.Kagami, _item);

                // ���O�t�@�C���ւ̏o��
                lock (Front.Log.KagamiLogFile)
                {
                    if (Front.Log.KagamiLogFile.Length != 0)
                    {
                        Regex ym = new Regex("yyyymm");
                        Regex ymd = new Regex("yyyymmdd");
                        string logFile = ym.Replace(ymd.Replace(Front.Log.KagamiLogFile, _dtNow.ToString("yyyyMMdd")), _dtNow.ToString("yyyyMM"));
                        try
                        {
                            StreamWriter log = new StreamWriter(logFile, true);
                            string str = _dtNow.ToString() + " [" + _status.MyPort + "] " + _content;
                            log.WriteLine(str);
                            log.Close();
                        }
                        catch (DirectoryNotFoundException dnfe)
                        {
                            dnfe.ToString();    // warning�΍�
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("���O�������݃G���[:" + _content + "\r\nErrorMsg:" + e.Message + "\r\nTrace:" + e.StackTrace);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("���O�������݃G���[:" + _content + "\r\nErrorMsg:" + e.Message + "\r\nTrace:" + e.StackTrace);
            }

        }
        /// <summary>
        /// �ڍ׃��O�o��
        /// �I�v�V�����ŏڍ׃��O�o��ON�̎��̂ݗL���A���O�t�@�C���ɒ��ڏo��
        /// </summary>
        /// <param name="_content">�o�̓��b�Z�[�W</param>
        static public void AddLogDetail(string _content)
        {
            DateTime _dtNow = DateTime.Now;
            if (Front.Log.LogDetail)
            {
                #if DEBUG
                System.Diagnostics.Trace.WriteLine("��DETAIL" + _dtNow.ToString("[MM/dd HH:mm:ss]") + _content);
                #endif
                lock (Front.Log.KagamiLogFile)
                {
                    if (Front.Log.KagamiLogFile.Length != 0)
                    {
                        Regex ym = new Regex("yyyymm");
                        Regex ymd = new Regex("yyyymmdd");
                        string logFile = ym.Replace(ymd.Replace(Front.Log.KagamiLogFile, _dtNow.ToString("yyyyMMdd")), _dtNow.ToString("yyyyMM"));
                        try
                        {
                            StreamWriter log = new StreamWriter(logFile, true);
                            log.WriteLine(_content);
                            log.Close();
                        }
                        catch (DirectoryNotFoundException dnfe)
                        {
                            dnfe.ToString(); // warning�΍�
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("���O�������݃G���[:" + _content + "\r\nErrorMsg:" + e.Message + "\r\nTrace:" + e.StackTrace);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �f�o�b�O��p���b�Z�[�W�o��
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_content"></param>
        static public void AddLogDebug(string _pos, string _content)
        {
            #if DEBUG
            DateTime _dtNow = DateTime.Now;
            if (_pos == "�ш搧��" || _pos == "KICK�`�F�b�N")
                return;
            System.Diagnostics.Trace.WriteLine("��DEBUG" + _dtNow.ToString("[MM/dd HH:mm:ss]") + "[" + _pos + "]" + _content);
            #endif
        }
        #endregion

    }//end of class Front
}// end of namespace


/// <summary>
/// INI�t�@�C���ǂݏ����p
/// </summary>
class IniFileHandler {
	[DllImport("kernel32.dll")]
	public static extern uint 
		GetPrivateProfileString(string lpAppName, 
		string lpKeyName, string lpDefault, 
		StringBuilder lpReturnedString, uint nSize, 
		string lpFileName);

    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringA")]
	public static extern uint 
		GetPrivateProfileStringByByteArray(string lpAppName, 
		string lpKeyName, string lpDefault, 
		byte [] lpReturnedString, uint nSize, 
		string lpFileName);

    [DllImport("kernel32.dll")]
	public static extern uint 
		GetPrivateProfileInt( string lpAppName, 
		string lpKeyName, int nDefault, string lpFileName );

    [DllImport("kernel32.dll")]
	public static extern uint WritePrivateProfileString(
		string lpAppName,
		string lpKeyName,
		string lpString,
		string lpFileName);
}
/// <summary>
/// ���̑�Win32NativeMethods�p
/// </summary>
class NativeMethods
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessage(
        IntPtr hWnd,
        int msg,
        int wParam,
        IntPtr lParam);


    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern uint ExtractIconEx(
        [MarshalAs(UnmanagedType.LPTStr)]
�@�@�@�@string lpszFile,
        int nIconIndex,
        [MarshalAs(UnmanagedType.LPArray)]
�@�@�@�@IntPtr[] phiconLarge,
        [MarshalAs(UnmanagedType.LPArray)]
�@�@�@�@IntPtr[] phiconSmall,
        uint nIcons
    );

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool DestroyIcon(
        IntPtr hIcon);

    [DllImport("user32.Dll", CharSet = CharSet.Auto)]
    public static extern Int32 RegisterWindowMessage(
        [MarshalAs(UnmanagedType.LPTStr)] String lpString);

    /*
    [DllImport("shell32.dll")]
    public static extern IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbSizeFileInfo,
        uint uFlags);

    // SHGetFileInfo�֐��Ŏg�p����t���O
    public const uint SHGFI_ICON = 0x100; // �A�C�R���E���\�[�X�̎擾
    public const uint SHGFI_LARGEICON = 0x0; // �傫���A�C�R��
    public const uint SHGFI_SMALLICON = 0x1; // �������A�C�R��

    // SHGetFileInfo�֐��Ŏg�p����\����
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };
    */
}
