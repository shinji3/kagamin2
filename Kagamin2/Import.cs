using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace Kagamin2
{
    public class KagamiException : Exception
    {
        public KagamiException(string _str)
        {
            Message = _str;
            LogLv = 1;
        }
        /*
        public KagamiException(int _logLv, string _str)
        {
            Message = _str;
            LogLv = _logLv;
        }
        */
        public new string Message;
        public int LogLv;
    }

    class Import
    {
        #region �����o�ϐ�

        /// <summary>
        /// �e����Ǘ�
        /// </summary>
        private Status Status = null;
        
        /// <summary>
        /// Import�p�\�P�b�g
        /// </summary>
        private Socket sock = null;

        /// <summary>
        /// IM�ڑ�/�ؒf���Đ��p
        /// </summary>
        private System.Media.SoundPlayer player = null;

        /// <summary>
        /// Push�z�M��Setup�v����M�ς݃t���O
        /// </summary>
        private bool bSetup;

        /// <summary>
        /// Push�z�M��Setup�v�����s����IP�A�h���X
        /// </summary>
        private string SetupIp;
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="_import"></param>
        /// <param name="_port"></param>
        /// <param name="_connection"></param>
        /// <param name="_reserve"></param>
        public Import(Status _status)
        {

            Status = _status;

            // �C���|�[�g����^�X�N�J�n
            Thread th1 = new Thread(ImportMain);
            th1.Name = "ImportMain";
            th1.Start();

            // �ш摪��^�X�N�J�n
            Thread th2 = new Thread(BandTask);
            th2.Name = "BandTask";
            th2.Start();

        }

        /// <summary>
        /// �C���|�[�g����^�X�N
        /// </summary>
        private void ImportMain()
        {
            lock (this)
            {
                if (Status.Type != 0)
                    Front.AddLogData(0, Status, "�O���ڑ��҂������^�X�N���J�n���܂�");
                while(true)
                {
                    // �O���ڑ��Ȃ�҂��󂯂��J�n����
                    if (Status.Type != 0)
                    {
                        // Web�G���g�����X����ImportURL���u�ҋ@���v�ȊO�ɏ����������邩�A
                        // Web�G���g�����X����Push�z�M�̗��p�v��(Type=1��2�ύX)�����邩�A
                        // GUI����I���v�����󂯂�܂őҋ@
                        while (Status.ImportURL == "�ҋ@��" && Status.Type == 1 && Status.RunStatus)
                            Thread.Sleep(1000);

                        // GUI����̏I���v������M�����ꍇ�A���̂܂܏I��
                        if (Status.RunStatus == false)
                            break;

                        // �O������Push�z�M�̗v����������
                        if (Status.Type == 2)
                        {
                            if (!Front.Opt.EnablePush)
                            {
                                // HybridDSP���ŃK�[�h���Ă邪�O�̂���
                                // �O���ڑ��ɖ߂��čēx�҂��󂯂�
                                Status.Type = 1;
                                continue;
                            }

                            // ���O�̐ڑ���Export�|�[�g�̉�����ς�Ŗ����ꍇ�A����҂�
                            while (Status.ListenPort)
                                Thread.Sleep(100);

                            // Push�z�M���p�v���������̂ŁA�|�[�g���J���đ҂B
                            Front.AddLogData(0, Status, "Push�z�M��t�|�[�g���N�����܂�");
                            IPEndPoint _iep = new IPEndPoint(Socket.OSSupportsIPv6 ? IPAddress.IPv6Any : IPAddress.Any, Status.MyPort);
                            TcpListener _listener = new TcpListener(_iep);
                            // Listen�J�n
                            try
                            {
                                Status.ListenPort = true;
                                _listener.Start();
                            }
                            catch
                            {
                                Front.AddLogData(1, Status, "Push�z�M�|�[�g�̑҂��󂯂��o���܂���B�ݒ���m�F���ĉ������B");
                                Status.Disc();
                                Status.RunStatus = false;
                                Status.ListenPort = false;
                                break;
                            }

                            int _timeout_cnt = 0;
                            bool _timeout_flg = false;
                            bSetup = false; // Setup����M
                            try
                            {
                                // Push�z�M�J�n�ɂ����ImportURL���u�ҋ@���v�ȊO�ɂȂ邩
                                // Web�G���g�����X����Push�z�M��~�v�������邩
                                // Push�z�M��t�^�C���A�E�g�ɂȂ邩�A
                                // GUI����̐ؒf�v��������܂�Listen�p��
                                while (Status.ImportURL == "�ҋ@��" && Status.Type == 2 && (_timeout_cnt <= 300 || !Front.Hp.UseHP) && Status.RunStatus)
                                {
                                    //Accept�҂��̃`�F�b�N
                                    if (_listener.Pending() == true)
                                    {
                                        //Accept���{
                                        Socket _sock = _listener.AcceptSocket();
                                        Thread _th = new Thread(PushReqTask);
                                        _th.Start(_sock);
                                    }
                                    else
                                    {
                                        Thread.Sleep(1000);
                                        _timeout_cnt++;
                                    }
                                }
                                // 300�b�𒴂�����^�C���A�E�g�ŊO���ڑ���Ԃɖ߂�
                                // �l��GUI�ォ��ς�����悤�ɂ��悤�B��ŁB�B
                                if (_timeout_cnt > 300 && Front.Hp.UseHP)
                                    _timeout_flg = true;
                            }
                            finally
                            {
                                if (_timeout_flg)
                                    Front.AddLogData(1, Status, "Push�z�M��t�|�[�g���~���܂�(��t���Ԓ���)");
                                else if (Status.Type != 2 || !Status.RunStatus)
                                    Front.AddLogData(1, Status, "Push�z�M��t�|�[�g���~���܂�(��~�v��)");
                                else
                                    Front.AddLogData(1, Status, "Push�z�M��t�|�[�g���~���܂�(�z�M�J�n)");
                                try
                                {
                                    //���X�i�[�őҋ@���Ă���N���C�A���g�����ׂĐؒf����
                                    while (_listener.Pending())
                                    {
                                        TcpClient _sock = _listener.AcceptTcpClient();
                                        _sock.Close();
                                    }
                                }
                                catch { }
                                // Listen��~
                                _listener.Stop();
                                Status.ListenPort = false;
                            }

                            // GUI����̒�~�v���Ȃ�O���҂��󂯒�~
                            if (!Status.RunStatus)
                                break;

                            // timeout��������Web�G���g�����X����̒�~�v���Ȃ�O���ڑ��҂��󂯂ɖ߂�
                            if (_timeout_flg || Status.Type != 2)
                            {
                                Status.Type = 1;
                                Front.AddLogData(1, Status, "�O���ڑ��҂��󂯏�Ԃɖ߂�܂�");
                                continue;
                            }
                            // push�z�M�J�n
                        }

                        // �O������̐ڑ��v������M
                        // �c���̃��O�o�͂́AHttpHybrid�Ɉړ��B
                        //Front.AddLogData(1, Status, "�O���ڑ��v������M���܂���");
                        //Front.AddLogData(1, Status, "URL=" + Status.ImportURL + " / �R�����g=" + Status.Comment);
                        // GUI�̍��p�l�����X�V
                        Event.EventUpdateKagami();

                    }
                    // �C���|�[�g��֐ڑ�
                    if (Status.Type != 2)
                    {
                        // �����ڑ��܂��͊O���ڑ�
                        Front.AddLogData(0, Status, "�C���|�[�g�^�X�N���J�n���܂�");
                        ImportTask();
                        Front.AddLogData(0, Status, "�C���|�[�g�^�X�N���I�����܂�");
                    }
                    else
                    {
                        // Push�z�M
                        Front.AddLogData(0, Status, "Push�z�M�C���|�[�g�^�X�N���J�n���܂�");
                        PushImportTask();
                        Front.AddLogData(0, Status, "Push�z�M�C���|�[�g�^�X�N���I�����܂�");
                    }
                    // �����ڑ��Ȃ�I��
                    if (Status.Type == 0)
                    {
                        Status.RunStatus = false;
                        break;
                    }
                    // �O���ڑ���GUI�̐ؒf�v����M�Ȃ�I��
                    if (Status.RunStatus == false)
                    {
                        break;
                    }

                    // �s�v�ȏ��̓N���A
                    // ������Push�z�M�̂Ƃ��͑҂������ɖ߂��Ă���ԕێ��̂��߃N���A���Ȃ��B
                    if (Status.Type == 2)
                    {
                        // Push�z�M��RunStatus����Ȃ�Push�z�M�v���҂��ɖ߂�
                        Front.AddLogData(1, Status, "Push�z�M�҂��󂯏�Ԃɖ߂�܂�");
                        // �ؒf�̂��߂̒l�ݒ�
                        Status.ImportStatus = false;
                        Status.ImportURL = "�ҋ@��";
                        // �e����̏���
                        Status.BusyCounter = 0;
                        Status.RetryCounter = 0;
                        //Status.Comment = "";
                        //Status.Password = "";
                        //Status.SetUserIP = "";
                        //Status.Gui.ReserveItem.Clear();
                        Status.CurrentDLSpeed = 0;
                        Status.AverageDLSpeed = 0;
                        Status.TrafficCount = 0;
                        Status.MaxDLSpeed = 0;
                        // �ő�ڑ��������[�U�w��l�ɖ߂�
                        Status.Connection = Status.Conn_UserSet;
                    }
                    else
                    {
                        // �O���ڑ���RunStatus����Ȃ�҂��󂯂ɖ߂�
                        Front.AddLogData(1, Status, "�O���ڑ��҂��󂯏�Ԃɖ߂�܂�");
                        Status.Disc();
                    }
                    // �O���ڑ��҂������ɖ߂鎞�͒ǉ��ňȉ����폜
                    Status.DataRspMsg10 = null;
                    Status.HeadRspMsg10 = null;
                    Status.HeadStream = null;
                    //�O�̂���
                    try
                    {
                        if (sock != null)
                        {
                            sock.Close();
                            sock = null;
                        }
                    }
                    catch { }
                    Event.EventUpdateKagami();
                }
                if (Status.Type != 0)
                    Front.AddLogData(1, Status, "�O���҂��󂯃^�X�N���I�����܂�");
                // �N���������X�g����폜
                Front.Delete(Status.Kagami);
            }
        }

        /// <summary>
        /// �C���|�[�g�^�X�N
        /// </summary>
        private void ImportTask()
        {
            uint retry_max;
            uint retry_wait;
            // �����E�O���ōĎ��s�̃^�C�~���O��ς���
            if (Status.Type == 0)
            {
                retry_max = Front.Retry.InRetryTime;
                retry_wait = Front.Retry.InRetryInterval;
            }
            else
            {
                retry_max = Front.Retry.OutRetryTime;
                retry_wait = Front.Retry.OutRetryInterval;
            }

            Status.ImportStartTime = DateTime.Now;
            Status.BusyCounter = 0;
            Status.RetryCounter = 0;
            Status.ImportError = 0;
            Status.ExportError = 0;
            Status.ExportCount = 0;
            // �Đڑ����[�v
            while (Status.RunStatus && Status.ImportURL != "�ҋ@��")
            {
                try
                {
                    // ���x���N���A
                    Status.TrafficCount = 0;
                    Status.AverageDLSpeed = 0;
                    Status.MaxDLSpeed = 0;
                    Front.AddLogData(1, Status, "�C���|�[�g�F�ڑ����c");
                    // �w�b�_�擾�v�����M
                    GetHeader();
                    Front.AddLogData(1, Status, "�C���|�[�g�F�w�b�_�擾����");
                    // �f�[�^�擾�v�����M
                    GetStream();
                    // �C���|�[�g��M���[�v�J�n
                    Front.AddLogData(1, Status, "�C���|�[�g�\�[�X�̎�荞�݂��J�n���܂���");
                    RecvStreamLoop();
                    Front.AddLogData(1, Status, "�C���|�[�g�\�[�X�̎�荞�݂��I�����܂���");
                }
                catch (KagamiException ke)
                {
                    Front.AddLogData(ke.LogLv, Status, ke.Message);
                    if (ke.Message.IndexOf("���_�C���N�g") >= 0)
                    {
                        // ���_�C���N�g�v���ɂ��ؒf�̏ꍇ�A���Đڑ�
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogData(1, Status, "�C���|�[�g�G���[(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")");
                }

                try
                {
                    // �ڑ����؂ꂽ�̂Ń��g���C�J�E���^���J�E���g�t�o�B
                    // ���g���C�J�E���^�̃��Z�b�g�_�@�́AASF_STREAM_DATA�𐳏�Ɏ�M�ł������B
                    Status.RetryCounter++;
                    Front.AddLogData(0, Status, "�ڑ��J�E���g " + Status.RetryCounter + "��ڏI��");
                    sock.Close();
                }
                catch { }

                // �ш��񁕍ő�ڑ����̃��Z�b�g
                Status.MaxDLSpeed = 0;
                Status.AverageDLSpeed = 0;
                Status.TrafficCount = 0;
                Status.Connection = Status.Conn_UserSet;

                if (Status.RetryCounter >= retry_max)
                {
                    Front.AddLogData(0, Status, "�Đڑ����I�����܂�");
                    Status.ImportErrorContext = "";
                    break;
                }
                if (Status.RunStatus == false || Status.ImportURL == "�ҋ@��")
                {
                    // �ؒf�v���ɂ�鐳��I��
                    break;
                }
                Front.AddLogData(0, Status, "�Đڑ��҂�(" + retry_wait + "sec)");
                for (int i = 0; i < retry_wait && Status.RunStatus == true && Status.ImportURL != "�ҋ@��"; i++)
                    Thread.Sleep(1000);
                if (Status.RunStatus == true && Status.ImportURL != "�ҋ@��")
                    Front.AddLogData(1, Status, "�Đڑ����J�n���܂�");
                else
                    Front.AddLogData(1, Status, "�Đڑ����I�����܂�");
            }
        }

        #region �C���|�[�g�悩����ۂɃf�[�^���擾���鏈��

        /// <summary>
        /// �����֐ڑ����A�w�b�_����M������ؒf����
        /// �擾�����w�b�_��Status.Data.Header/Status.Data.Header2�ɕۑ����Ă���
        /// �w�b�_�擾�Ɏ��s�����ꍇ��KagamiException��throw����
        /// </summary>
        /// <returns></returns>
        private void GetHeader()
        {
            if (Status.RunStatus == false)
            {
                throw new KagamiException("�w�b�_�[�擾���ɏI���v�����������܂���");
            }
            try
            {
                //Socket�̍쐬
                sock = new Socket(Socket.OSSupportsIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress hostadd = Dns.GetHostAddresses(Status.ImportHost)[0];
                IPEndPoint ephost = new System.Net.IPEndPoint(hostadd, Status.ImportPort);

                sock.SendTimeout = (int)Front.Sock.SockConnTimeout;       // Import�ڑ� �w�b�_�擾�v�����M�̃^�C���A�E�g�l
                sock.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    // Import�ڑ� �w�b�_�擾������M�̃^�C���A�E�g�l

                //�ڑ�
                sock.Connect(ephost);
            }
            catch
            {
                sock.Close();
                // IM�ڑ�NG�����ݒ肳��Ă�����Đ�
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "�C���|�[�g�ڑ��G���[";
                throw new KagamiException("�C���|�[�g��ɐڑ��ł��܂���ł���");
            }

            try
            {
                //�N���C�A���g�ɋU�������w�b�_�𑗐M
                string reqMsg = "GET / HTTP/1.0\r\n" +
                        "Accept: */*\r\n" +
                        "User-Agent: " + Front.UserAgent + "\r\n" +
                        "Host: " + ((IPEndPoint)sock.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)sock.RemoteEndPoint).Port + "\r\n" +
                        "Pragma: no-cache\r\n" +
                        #if !DEBUG
                        "Pragma: kagami-port=" + Status.MyPort + "\r\n" +
                        #endif
                        "Content-Type: application/x-mms-framed\r\n\r\n";
                System.Text.Encoding enc = System.Text.Encoding.ASCII; // "euc-jp"
                byte[] reqBytes = enc.GetBytes(reqMsg);

                //���N�G�X�g���M
                Front.AddLogDetail("SendReqMsg(Head)Sta-----\r\n" + reqMsg + "\r\nSendReqMsg(Head)End-----");
                sock.Send(reqBytes, reqBytes.Length, SocketFlags.None);

                //�܂���HTTP�����w�b�_�܂Ŏ擾
                byte[] ack = new byte[1];
                byte[] ack_end ={ 0x0a, 0x0a }; // '\n', '\n'
                byte[] ack_log = new byte[50000];
                byte[] sts_code = new byte[3];
                int i = 0;
                int count = 0;
                while (Status.RunStatus)
                {
                    sock.Receive(ack);
                    ack_log[count] = ack[0];
                    count++;

                    // HTTP�����w�b�_�̏I��������
                    if (ack[0].Equals(0x0d)) continue;  // '\r'
                    if (ack[0].Equals(ack_end[i])) i++; else i = 0;

                    //ack_end�ɓ��ꂽ������Ɠ������̂���M�ł���������
                    //�����������炸�A��M�����f�[�^��50000�o�C�g�𒴂�����G���[�����ɂ���
                    //�قƂ�ǂ̏ꍇ�A5000�`6000�o�C�g�Ō�����
                    //50000�܂ōs���ƃG���[�̉\����
                    if (i >= ack_end.Length)
                    {
                        // HTTP StatusCode�擾
                        // 9�`11�����ڂ��擾
                        // 0123456789abcde
                        // HTTP/1.x 200 OK
                        int http_status = 0;
                        sts_code[0] = ack_log[9];
                        sts_code[1] = ack_log[10];
                        sts_code[2] = ack_log[11];
                        try
                        {
                            Front.AddLogDetail("RecvRspMsg(Head)Sta-----\r\n" +
                                         System.Text.Encoding.ASCII.GetString(ack_log, 0, count) +
                                         "\r\nRecvRspMsg(Head)End-----");
                            http_status = int.Parse(System.Text.Encoding.ASCII.GetString(sts_code));
                        }
                        catch
                        {
                            //HTTP StatusCode�ϊ����s
                            Status.ImportErrorContext = "�w�b�_�擾�G���[(HTTP�����w�b�_�ُ�)";
                            throw new KagamiException("�w�b�_�̎擾���ɃG���[���������܂���(HTTP�����w�b�_�ُ�)");
                        }
                        if (http_status == 200)
                        {
                            break;
                        }
                        else if (http_status == 301 || http_status == 302)
                        {
                            // ���_�C���N�g�w���̏ꍇ�͈ړ����ImportURL�ɍĐݒ肵�Ă���NG����
                            string _reply = System.Text.Encoding.ASCII.GetString(ack_log, 0, count);
                            int _idx = _reply.IndexOf("Location: ");
                            if (_idx >= 0)
                            {
                                _idx += 10; // �ړ���URL������擪�Ɉړ�
                                int _len = _reply.IndexOf("\r\n", _idx) - _idx;
                                if (_len > 0)
                                {
                                    string _redirURL = _reply.Substring(_idx, _len);
                                    string _orgURL = Status.ImportURL;
                                    Status.ImportURL = _redirURL;
                                    Event.EventUpdateKagami();
                                    Status.ImportErrorContext = "���_�C���N�g��������M���܂���[HTTPStatusCode=302]";
                                    throw new KagamiException("���_�C���N�g���܂�[URL= " + _orgURL + " -> URL=" + _redirURL + "]");
                                }
                            }
                        }
                        Status.ImportErrorContext = "�C���|�[�g�\�[�X�̓r�W�[�ł��B[HTTPStatusCode=" + http_status + "]";
                        throw new KagamiException("�C���|�[�g�\�[�X�̓r�W�[�ł��B[HTTPStatusCode=" + http_status + "]");
                    }
                    else if (count >= 50000)
                    {
                        Front.AddLogDetail("RecvRspMsg(Head)Sta-----\r\n" +
                                     System.Text.Encoding.ASCII.GetString(ack_log,0,count) +
                                     "\r\nRecvRspMsg(Head)End-----");
                        Status.ImportErrorContext = "HTTP�w�b�_�擾�G���[(HTTPHeader>50KBover)";
                        throw new KagamiException("HTTP�w�b�_�̎擾���ɃG���[���������܂���(HTTPHeader>50KBover)");
                    }
                }

                //�w�b�_��Ɖ����q���郁�����X�g���[��
                MemoryStream ms2;
                ms2 = new MemoryStream();

                // ASF�w�b�_����: type(2)+size(2)+seq(4)+unk(2)+szcfm(2)

                //ack_end = new byte[] { 0x00, 0x00, 0x01, 0x01 };
                byte[] ack_sta = new byte[] { 0x24, 0x48 };
                i = 0;
                int pos = 0;
                // ASF_HEADER�n�܂��T��
                while (Status.RunStatus)
                {
                    sock.Receive(ack);
                    ms2.WriteByte(ack[0]);
                    pos++;
                    if (ack[0].Equals(ack_sta[i])) i++; else i = 0;
                    if (i >= ack_sta.Length)
                    {
                        break;
                    }
                    else if (ms2.Length > 50000)
                    {
                        Status.ImportErrorContext = "�X�g���[���w�b�_�擾�G���[(StreamHeader>50KBover)";
                        throw new KagamiException("�X�g���[���w�b�_�̎擾���ɃG���[���������܂���(StreamHeader>50KBover)");
                    }
                }
                if(Status.RunStatus == false)
                    throw new KagamiException("�w�b�_�[�擾���ɏI���v�����������܂���");

                // blk_size�擾
                int blk_size = 0;
                sock.Receive(ack);
                ms2.WriteByte(ack[0]);
                blk_size += ack[0];
                sock.Receive(ack);
                ms2.WriteByte(ack[0]);
                blk_size += (ack[0] << 8);
                // �c��̃w�b�_�擾
                int rsp_size = 0;
                ack = new byte[blk_size];
                while (Status.RunStatus && blk_size > rsp_size)
                {
                    rsp_size += sock.Receive(ack, rsp_size, blk_size - rsp_size, SocketFlags.None);
                }
                if (Status.RunStatus == false)
                    throw new KagamiException("�w�b�_�[�擾���ɏI���v�����������܂���");

                ms2.Write(ack, 0, blk_size);
                Status.HeadStream = ms2.ToArray();

                bool error_flg = false;
                // size-cfm�̃`�F�b�N�B�ُ�ł��˂��i�ށH
                // ack[6-7]��size-cfm
                if ((ack[6] + (ack[7] << 8)) != blk_size)
                {
                    Front.AddLogData(1, Status, "��M�f�[�^��ASF�X�g���[���ł͂���܂���");
                    error_flg = true;
                }

                // ������ 00 00 01 01 �`�F�b�N�B�ُ�ł��˂��i�ށB
                if (ack[blk_size - 4] != 0x00 ||
                    ack[blk_size - 3] != 0x00 ||
                    ack[blk_size - 2] != 0x01 ||
                    ack[blk_size - 1] != 0x01)
                {
                    Front.AddLogData(1, Status, "HeaderStream���������Ȃ��\��������܂��B");
                    error_flg = true;
                }
                if (error_flg)
                {
                    Front.AddLogData(1, Status,
                        "asf-type:2448 size:" +
                        Status.HeadStream[pos + 0].ToString("X2") +
                        Status.HeadStream[pos + 1].ToString("X2") +
                        " seq:" +
                        Status.HeadStream[pos + 2].ToString("X2") +
                        Status.HeadStream[pos + 3].ToString("X2") +
                        Status.HeadStream[pos + 4].ToString("X2") +
                        Status.HeadStream[pos + 5].ToString("X2") +
                        " unk:" +
                        Status.HeadStream[pos + 6].ToString("X2") +
                        Status.HeadStream[pos + 7].ToString("X2") +
                        " sizecfm:" +
                        Status.HeadStream[pos + 8].ToString("X2") +
                        Status.HeadStream[pos + 9].ToString("X2") +
                        " wmv-guid:" +
                        Status.HeadStream[pos + 10].ToString("X2") +
                        Status.HeadStream[pos + 11].ToString("X2") +
                        Status.HeadStream[pos + 12].ToString("X2") +
                        Status.HeadStream[pos + 13].ToString("X2") +
                        Status.HeadStream[pos + 14].ToString("X2") +
                        Status.HeadStream[pos + 15].ToString("X2") +
                        Status.HeadStream[pos + 16].ToString("X2") +
                        Status.HeadStream[pos + 17].ToString("X2") +
                        Status.HeadStream[pos + 18].ToString("X2") +
                        Status.HeadStream[pos + 19].ToString("X2") +
                        Status.HeadStream[pos + 20].ToString("X2") +
                        Status.HeadStream[pos + 21].ToString("X2") +
                        Status.HeadStream[pos + 22].ToString("X2") +
                        Status.HeadStream[pos + 23].ToString("X2") +
                        Status.HeadStream[pos + 24].ToString("X2") +
                        Status.HeadStream[pos + 25].ToString("X2"));
                    Front.AddLogData(1, Status,
                        "tail-data=" +
                        ack[blk_size - 4].ToString("X2") +
                        ack[blk_size - 3].ToString("X2") +
                        ack[blk_size - 2].ToString("X2") +
                        ack[blk_size - 1].ToString("X2"));
                }
                try
                {
                    // �w�b�_��̓e�X�g
                    //WMV�w�b�_��ASF�w�b�_ type(2)+size(2)+seq(4)+unk(2)+szcfm(2)�̌�Bpos��type�̏I���܂ł�offset
                    CheckWMVHeader(Status.HeadStream, pos + 10);
                }
                catch { }
                sock.Close();

                // �G�N�X�|�[�g�̉����f�[�^�쐬
                string str =
                    "HTTP/1.0 200 OK\r\n" +
                    "Server: Rex/9.0.0.2980\r\n" +
                    "Cache-Control: no-cache\r\n" +
                    "Pragma: no-cache\r\n" +
                    "Pragma: client-id=3320437311\r\n" +
                    "Pragma: features=\"seekable,stridable\"\r\n" +
                    "X-Server: " + Front.AppName + "\r\n" +
                    "Content-Type: application/vnd.ms.wms-hdr.asfv1\r\n" +
                    "Content-Length: " + Status.HeadStream.Length + "\r\n\r\n";
                //�w�b�_��Ɖ����q���郁�����X�g���[��
                MemoryStream ms1;
                ms1 = new MemoryStream();
                count = enc.GetBytes(str).Length;
                ms1.Write(enc.GetBytes(str), 0, count);
                Status.HeadRspMsg10 = ms1.ToArray();
            }
            catch (KagamiException ke)
            {
                sock.Close();
                // IM�ڑ�NG�����ݒ肳��Ă�����Đ�
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                throw new KagamiException(ke.Message);
            }
            catch (SocketException se)
            {
                sock.Close();
                // IM�ڑ�NG�����ݒ肳��Ă�����Đ�
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "�w�b�_�擾�\�P�b�g�G���[";
                throw new KagamiException("�w�b�_�̎擾���ɃG���[���������܂���(wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")");
            }
            catch (Exception e)
            {
                sock.Close();
                // IM�ڑ�NG�����ݒ肳��Ă�����Đ�
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "�w�b�_�擾�����G���[";
                throw new KagamiException("�w�b�_�̎擾���ɃG���[���������܂���(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")");
            }
        }

        /// <summary>
        /// WMV�w�b�_����͂��ăf�[�^���擾����
        /// </summary>
        /// <param name="block"></param>
        /// <param name="idx"></param>
        private void CheckWMVHeader(byte[] block, int idx)
        {
            string key = "";
            int max = 0;
            int pos = idx; // ��Object�擪�ʒu
            int num = 0;
            int obj = 0;
            string stream_prop = "";
            Status.MaxDLSpeed = 0;
            try
            {
                //�擪��ASF_HEADER_OBJECT�ɂȂ��Ă��邩�`�F�b�N
                key = "";
                for (int guid = 0; guid < 16; guid++)
                    key += block[pos + guid].ToString("X2");

                if (Front.ASF_GUID.ContainsKey(key) && Front.ASF_GUID[key].Equals("ASF_Header_Object"))
                {
                    /*
                     * ASF Header Object Structure
                     * 0x10 : OBJ-ID
                     * 0x08 : OBJ-SIZE
                     * 0x04 : NumberOfHeaderObjects
                     * 0x02 : Reserve(01,02)
                     */
                    num = (block[pos + 0x18 + 3] << 24)
                        + (block[pos + 0x18 + 2] << 16)
                        + (block[pos + 0x18 + 1] << 8)
                        + (block[pos + 0x18]);
                    pos += 0x1e;
                    Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "ASF_Header_Object CHECK OK / NumOfObj=" + num);

                    //HeaderObjectLoop
                    for (obj = 0; obj < num; obj++)
                    {
                        key = "";
                        for (int guid = 0; guid < 16; guid++)
                            key += block[pos + guid].ToString("X2");

                        if (Front.ASF_GUID.ContainsKey(key))
                        {
                            Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "OBJID:[" + (obj + 1) + "]" + Front.ASF_GUID[key]);
                            if (Front.ASF_GUID[key].Equals("ASF_Header_Extension_Object"))
                            {
                                #region ASF_Header_Extension_Object
                                /*
                                 * ASF HeaderExtension Object Structure
                                 * 0x10 : OBJ-ID
                                 * 0x08 : OBJ-SIZE
                                 * 0x10 : Reserve1 GUID
                                 * 0x02 : Reserve2
                                 * 0x04 : HeaderExtensionSize
                                 */
                                int ex_siz = 0;
                                int ex_cur = 0;
                                int ex_len = 0;
                                ex_siz = (block[pos + 0x2a + 3] << 24)
                                       + (block[pos + 0x2a + 2] << 16)
                                       + (block[pos + 0x2a + 1] << 8)
                                       + (block[pos + 0x2a]);
                                pos += 0x2e;

                                //HeaderExtensionObjectLoop
                                for (ex_cur = 0; ex_cur < ex_siz; )
                                {
                                    key = "";
                                    for (int guid = 0; guid < 16; guid++)
                                        key += block[pos + guid].ToString("X2");

                                    if (Front.ASF_GUID.ContainsKey(key))
                                    {
                                        Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "EX-OBJID:" + Front.ASF_GUID[key]);

                                        if (Front.ASF_GUID[key].Equals("ASF_Extended_Stream_Properties_Object"))
                                        {
                                            #region ASF_Extended_Stream_Properties_Object
                                            /*
                                             * ASF Extended Stream Properties Object
                                             * 0x10 : OBJ-ID
                                             * 0x08 : OBJ-SIZE
                                             * 0x08 : StartTime
                                             * 0x08 : EndTime
                                             * 0x04 : DataBitrate <----
                                             * 0x04 : BufferSize
                                             * 0x04 : InitialBufferFullness
                                             * 0x04 : AlternateDataBitrate <---- Same As DataBitrate?
                                             * 0x04 : AlternateBufferSize
                                             * 0x04 : AlternateInitialBufferFullness
                                             * 0x04 : MaximumObjectSize
                                             * 0x04 : Flags
                                             * 0x02 : StreamNumber <----
                                             * 0x02 : StreamLanguageIdIndex
                                             * 0x08 : AverageTimePerFrame
                                             * 0x02 : StreamNameCount
                                             * 0x02 : PayloadExtensionSystemCount
                                             */
#if DEBUG
                                            int ex_max = (block[pos + 0x28 + 3] << 24)
                                                       + (block[pos + 0x28 + 2] << 16)
                                                       + (block[pos + 0x28 + 1] << 8)
                                                       + (block[pos + 0x28]);
                                            ex_max /= 1000;
                                            int strnum = (block[pos + 0x48 + 1] << 8)
                                                       + (block[pos + 0x48]);
                                            Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "STREAM-NUM=" + strnum + " / EX-BITRATE=" + ex_max + "kbps");
#endif
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "EX-OBJID:" + key);
                                    }
                                    // StandardStructure
                                    // 0x10 : HeaderExtension GUID
                                    // 0x08 : OBJ-SIZE
                                    //   :       :
                                    ex_len = (block[pos + 0x10 + 3] << 24)
                                           + (block[pos + 0x10 + 2] << 16)
                                           + (block[pos + 0x10 + 1] << 8)
                                           + (block[pos + 0x10]);
                                    ex_cur += ex_len;
                                    pos += ex_len;
                                }
                                #endregion
                            }
                            /*
                             * ASF_File_Properties_Object�̃r�b�g���[�g��
                             * �S�v���p�e�B�̘a�ɂȂ��Ă��܂��̂ŁA�}���`�r�b�g���[�g���Ǝg���Ȃ��B
                             */
                            else if (Front.ASF_GUID[key].Equals("ASF_File_Properties_Object"))
                            {
                                #region ASF_File_Properties_Object
                                /*
                                 * ASF File Properties Object
                                 * offset: len : name
                                 * 0x00 : 0x10 : OBJ-ID
                                 * 0x10 : 0x08 : OBJ-SIZE
                                 * 0x18 : 0x10 : FileID
                                 * 0x28 : 0x08 : FileSize
                                 * 0x30 : 0x08 : Creation-Date
                                 * 0x38 : 0x08 : Data-Packets-Count
                                 * 0x40 : 0x08 : Play-Duration
                                 * 0x48 : 0x08 : Send-Duration
                                 * 0x50 : 0x08 : Pre-roll
                                 * 0x58 : 0x04 : Broadcast & Seekable & ReserveFlags
                                 * 0x5c : 0x04 : Minimum Data Packets Size
                                 * 0x60 : 0x04 : Maximum Data Packets Size
                                 * 0x64 : 0x04 : Maximum Bitrate(Average Bitrate) <----
                                 */
                                // maximum bitrate
                                max = (block[pos + 0x64 + 3] << 24)
                                    + (block[pos + 0x64 + 2] << 16)
                                    + (block[pos + 0x64 + 1] << 8)
                                    + (block[pos + 100]);
                                max /= 1000;
                                Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "BITRATE=" + max + "kbps");
                                #endregion
                            }
                            // ��M�w�b�_�̃}���`�r�b�g���[�g����
                            else if (Front.ASF_GUID[key].Equals("ASF_Stream_Bitrate_Properties_Object"))
                            {
                                #region ASF_Stream_Bitrate_Properties_Object
                                /*
                                 * ASF Stream Bitrate Propertis Object
                                 * 0x10 : OBJ-ID
                                 * 0x08 : OBJ-SIZE
                                 */
                                Status.StreamSwitchCount = (block[pos + 25] << 8) + block[pos + 24];
                                Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "BitrateRecordsCount=" + Status.StreamSwitchCount);
                                Status.StreamType = new int[Status.StreamSwitchCount];
                                Status.StreamBitrate = new int[Status.StreamSwitchCount];
                                for (int _loop = 0; _loop < Status.StreamSwitchCount; _loop++)
                                {
                                    int tmp_no = (block[pos + 27 + _loop * 6] << 8) + block[pos + 26 + _loop * 6];
                                    int tmp_br = (block[pos + 31 + _loop * 6] << 24) + (block[pos + 30 + _loop * 6] << 16) + (block[pos + 29 + _loop * 6] << 8) + block[pos + 28 + _loop * 6];
                                    Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "Record=" + tmp_no + "/" + "Rate=" + tmp_br + "(kbps)");
                                    if ((0 == tmp_no) || (tmp_no > Status.StreamSwitchCount))
                                        continue;
                                    Status.StreamType[tmp_no - 1] = 0; // Audio
                                    Status.StreamBitrate[tmp_no - 1] = tmp_br;
                                }
                                #endregion
                            }
                            else if (Front.ASF_GUID[key].Equals("ASF_Stream_Properties_Object"))
                            {
                                #region ASF_Stream_Properties_Object
                                /*
                                 * ASF Stream Properties Object
                                 * 0x10 : OBJ-ID
                                 * 0x08 : OBJ-SIZE
                                 * 0x10 : StreamTypeGUID
                                 * 0x10 : ErrorCorrectionTypeGUID
                                 * 0x08 : TimeOffset
                                 * 0x04 : TypeSpecificDataLength
                                 * 0x04 : ErrorCorrectionDataLength
                                 * 0x02 : Flags:StreamNumber(7)+Reserved(8)+EncryptedContentFlag(1)
                                 * 0x04 : Reserved
                                 * vari : TypeSpecificData
                                 * vari : ErrorCorrectionData
                                 */
                                int str_num = block[pos + 0x48];
                                key = "";
                                for (int guid = 0; guid < 16; guid++)
                                    key += block[pos + 0x18 + guid].ToString("X2");
                                if (Front.ASF_GUID.ContainsKey(key))
                                {
                                    Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "STREAM-NUM=" + str_num + " / STREAM-TYPE=" + Front.ASF_GUID[key]);
                                    if (Front.ASF_GUID[key] == "ASF_Audio_Media")
                                        stream_prop += str_num.ToString() + "=0,";
                                    else if (Front.ASF_GUID[key] == "ASF_Video_Media")
                                        stream_prop += str_num.ToString() + "=1,";
                                    else
                                        stream_prop += str_num.ToString() + "=2,";
                                }
                                else
                                {
                                    Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "STREAM-NUM=" + str_num + " / STREAM-TYPE=" + key);
                                    stream_prop += str_num.ToString() + "=2,";
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "OBJID:" + key);
                        }
                        pos += (block[pos + 16 + 3] << 24) + (block[pos + 16 + 2] << 16) + (block[pos + 16 + 1] << 8) + block[pos + 16];
                    }
                    // �}���`�r�b�g���[�g�p�ɑш���v�Z������
                    // �ш搧���ɂ͍ō��r�b�g���[�g�̒l��K�p
                    foreach (string stream_type in stream_prop.Split(','))
                    {
                        if (stream_type == "")
                            continue;
                        string[] cnt_val = stream_type.Split('=');
                        int cnt = int.Parse(cnt_val[0]);
                        int val = int.Parse(cnt_val[1]);
                        if ((cnt == 0) || (cnt > Status.StreamSwitchCount))
                            continue;
                        Status.StreamType[cnt - 1] = val;
                    }
                    int max_audio = 0;
                    int max_video = 0;
                    // ��M����X�g���[���̌���y�эő�r�b�g���[�g�̌v�Z
                    for (int cnt = 1; cnt <= Status.StreamSwitchCount; cnt++)
                    {
                        // �����X�g���[���̒��Ńr�b�g���[�g���ő�̂��̂�I��
                        if ((Status.StreamType[cnt - 1] == 0) && (Status.StreamBitrate[cnt - 1] > max_audio))
                        {
                            Status.SelectedAudioRecord = cnt;
                            max_audio = Status.StreamBitrate[cnt - 1];
                        }
                        // �f���X�g���[���̒��Ńr�b�g���[�g���ő�̂��̂�I��
                        if ((Status.StreamType[cnt - 1] == 1) && (Status.StreamBitrate[cnt - 1] > max_video))
                        {
                            Status.SelectedVideoRecord = cnt;
                            max_video = Status.StreamBitrate[cnt - 1];
                        }
                    }
                    Front.AddLogDebug("MAXIMUM BITRATE", "AUDIO: STREAM-NUM=" + Status.SelectedAudioRecord + "/" + max_audio + "bps");
                    Front.AddLogDebug("MAXIMUM BITRATE", "VIDEO: STREAM-NUM=" + Status.SelectedVideoRecord + "/" + max_video + "bps");
                    max = max_audio + max_video;
                    /*
                    if (ex_max > 0)
                        Status.Data.MaxDLSpeed = ex_max;    // Extended_Stream_Properties����擾�ł����ꍇ
                    else
                    */
                    if (max > 0)
                        Status.MaxDLSpeed = max / 1000;       // Extended_Stream_Properties����擾�ł��Ȃ������ꍇ�AFile_Properties����ݒ�
                    else
                        Front.AddLogData(1, Status, "�r�b�g���[�g�擾NG");
                }
                else
                {
                    Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "ASF_Header_Object CHECK NG");
                    Front.AddLogData(1, Status, "�r�b�g���[�g�擾NG");
                }
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "�z�M�r�b�g���[�g�擾�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
            }
        }

        /// <summary>
        /// �����֐ڑ����ADataStream�{�̂̃��N�G�X�g�𑗐M����
        /// ��M������HTTP StatusCode�̂݃`�F�b�N���A�ȍ~�̎�M������RecvStream�ɔC����
        /// DataStream��MNG�̏ꍇ�́AKagamiException��throw����
        /// </summary>
        private void GetStream()
        {
            if (Status.RunStatus == false)
                throw new KagamiException("�X�g���[���v�����ɏI���v�����������܂���");

            try
            {
                //Socket�̍쐬
                sock = new Socket(Socket.OSSupportsIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress hostadd = Dns.GetHostAddresses(Status.ImportHost)[0];
                IPEndPoint ephost = new System.Net.IPEndPoint(hostadd, Status.ImportPort);

                sock.SendTimeout = (int)Front.Sock.SockConnTimeout;       //�C���|�[�g�ڑ� Stream�{�̗v���^�C���A�E�g
                sock.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    //�C���|�[�g�ڑ� Stream�{�̎�M�^�C���A�E�g

                //�ڑ�
                sock.Connect(ephost);

                // WMV�w�b�_������ɉ�͂ł��Ȃ�������
                if (Status.StreamSwitchCount == 0)
                {
                    Status.StreamSwitchCount = 2;
                    Status.SelectedAudioRecord = 1;
                    Status.SelectedVideoRecord = 2;
                }

                string reqMsg = "GET / HTTP/1.0\r\n" +
                    "Accept: */*\r\n" +
                    "User-Agent: " + Front.UserAgent + "\r\n" +
                    "Host: " + ((IPEndPoint)sock.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)sock.RemoteEndPoint).Port + "\r\n" +
                    "X-Accept-Authentication: Negotiate, NTLM, Digest, Basic\r\n" +
                    "Pragma: no-cache,rate=1.000,stream-time=0,stream-offset=4294967295:4294967295,packet-num=4294967295,max-duration=0\r\n" +
                    "Pragma: xPlayStrm=1\r\n" +
                    // ��WMP�̃I�v�V�������p�t�H�[�}���X�ŉ�����x�����I���ɂ������A���̒l�ɂȂ�
                    "Pragma: LinkBW=2147483647, AccelBW=2147483647, AccelDuration=18000\r\n" +
                    // ��������x���蓮�ݒ肵���Ƃ��́A���̂悤�ɂȂ�(256kbps�w�莞)
                    //"Pragma: LinkBW=240000\r\n" +
#if !DEBUG
                    "Pragma: kagami-port=" + Status.MyPort + "\r\n" +
#endif
                    "Accept-Language: ja, *;q=0.1\r\n" +
                    "Supported: com.microsoft.wm.srvppair, com.microsoft.wm.sswitch, com.microsoft.wm.predstrm, com.microsoft.wm.startupprofile\r\n" +
                    // stream-switch-count�ɂ͊܂܂��S�X�g���[���̐���ݒ�
                    "Pragma: stream-switch-count=" + Status.StreamSwitchCount + "\r\n" +
                    // stream-switch-entry�̋L�q���@�͉��L�Q��
                    "Pragma: stream-switch-entry=";

                for (int cnt = 1; cnt <= Status.StreamSwitchCount; cnt++)
                {
                    reqMsg += "ffff:" + cnt;
                    if (cnt == Status.SelectedAudioRecord || cnt == Status.SelectedVideoRecord)
                        reqMsg += ":0 ";
                    else
                        reqMsg += ":2 ";
                }
                reqMsg += "\r\n\r\n";
                // memo:stream-switch-entry�ݒ���@
                // ffff:X:Y
                // X: �X�g���[���ԍ�
                // Y: 0:���̃X�g���[����v������
                //    1:�H video�X�g���[���Őݒ肳��邱�Ƃ���B�B
                //    2:���̃X�g���[����v�����Ȃ�
                // ��jID:1=64K_AUDIO 2=128K_AUDIO 3=300K_VIDEO 4=600K_VIDEO�̏ꍇ
                //    64K_AUDIO+300K_VIDEO��v���Fstream-switch-entry=ffff:1:0 ffff:2:2 ffff:3:0 ffff:4:2
                //   128K_AUDIO+300K_VIDEO��v���Fstream-switch-entry=ffff:1:2 ffff:2:0 ffff:3:0 ffff:4:2
                //   128K_AUDIO+600K_VIDEO��v���Fstream-switch-entry=ffff:1:2 ffff:2:0 ffff:3:2 ffff:4:0
                // �C�͂�����΂��̓��}���`�r�b�g���[�g�ɂ��Ή��������ˁB�B

                //�w�b�_�̑��M
                System.Text.Encoding enc = System.Text.Encoding.ASCII;
                byte[] reqBytes = enc.GetBytes(reqMsg);

                Front.AddLogDetail("SendReqMsg(Data)Sta-----\r\n" + reqMsg + "\r\nSendReqMsg(Data)End-----");
                sock.Send(reqBytes);


                //�܂���HTTP�����w�b�_�܂Ŏ擾
                byte[] ack = new byte[1];
                byte[] ack_end ={ 0x0a, 0x0a }; // '\n', '\n'
                byte[] ack_log = new byte[50000];
                byte[] sts_code = new byte[3];
                int i = 0;
                int count = 0;
                while (Status.RunStatus)
                {
                    sock.Receive(ack);
                    ack_log[count] = ack[0];
                    count++;

                    // HTTP�����w�b�_�̏I��������
                    if (ack[0].Equals(0x0d)) continue;  // '\r'
                    if (ack[0].Equals(ack_end[i])) i++; else i = 0;

                    //ack_end�ɓ��ꂽ������Ɠ������̂���M�ł���������
                    //�����������炸�A��M�����f�[�^��50000�o�C�g�𒴂�����G���[�����ɂ���
                    //�قƂ�ǂ̏ꍇ�A5000�`6000�o�C�g�Ō�����
                    //50000�܂ōs���ƃG���[�̉\����
                    if (i >= ack_end.Length)
                    {
                        // HTTP StatusCode�擾
                        // 9�`11�����ڂ��擾
                        // 0123456789abcde
                        // HTTP/1.x 200 OK
                        int http_status = 0;
                        sts_code[0] = ack_log[9];
                        sts_code[1] = ack_log[10];
                        sts_code[2] = ack_log[11];
                        try
                        {
                            Front.AddLogDetail("RecvRspMsg(Data)Sta-----\r\n" +
                                         System.Text.Encoding.ASCII.GetString(ack_log, 0, count) +
                                         "\r\nRecvRspMsg(Data)End-----");
                            http_status = int.Parse(System.Text.Encoding.ASCII.GetString(sts_code));
                        }
                        catch
                        {
                            //HTTP StatusCode�ϊ����s
                            Status.ImportErrorContext = "�X�g���[���v�����G���[(HTTP�����w�b�_�ُ�)";
                            throw new KagamiException("�X�g���[���v�����ɃG���[���������܂���(HTTP�����w�b�_�ُ�)");
                        }
                        if (http_status == 200)
                        {
                            break;
                        }
                        else
                        {
                            Status.ImportErrorContext = "�C���|�[�g�\�[�X�r�W�[[" +http_status + "]";
                            throw new KagamiException("�C���|�[�g�\�[�X�̓r�W�[�ł��B[HTTPStatusCode=" + http_status + "]");
                        }
                    }
                    else if (count >= 50000)
                    {
                        Front.AddLogDetail("RecvRspMsg(Data)Sta-----\r\n" +
                                     System.Text.Encoding.ASCII.GetString(ack_log, 0, count) +
                                     "\r\nRecvRspMsg(Data)End-----");
                        Status.ImportErrorContext = "HTTP�w�b�_�擾�G���[(HTTPHeader>50KBover)";
                        throw new KagamiException("HTTP�w�b�_�̎擾���ɃG���[���������܂���(HTTPHeader>50KBover)");
                    }
                }
                // �G�N�X�|�[�g�̉����f�[�^�쐬
                string str =
                    "HTTP/1.0 200 OK\r\n" +
                    "Server: Rex/9.0.0.2980\r\n" +
                    "Cache-Control: no-cache\r\n" +
                    "Pragma: no-cache\r\n" +
                    "Pragma: client-id=3320437311\r\n" +
                    "Pragma: features=\"seekable,stridable\"\r\n" +
                    "X-Server: " + Front.AppName + "\r\n" +
                    "Connection: close\r\n" +
                    "Content-Type: application/x-mms-framed\r\n\r\n";
                //�f�[�^�擾�����w�b�_��ێ�
                MemoryStream ms;
                ms = new MemoryStream();
                count = enc.GetBytes(str).Length;
                ms.Write(enc.GetBytes(str), 0, count);
                Status.DataRspMsg10 = ms.ToArray();
            }
            catch (KagamiException ke)
            {
                sock.Close();
                // IM�ڑ�NG�����ݒ肳��Ă�����Đ�
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                throw new KagamiException(ke.Message);
            }
            catch (SocketException se)
            {
                sock.Close();
                // IM�ڑ�NG�����ݒ肳��Ă�����Đ�
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "�X�g���[���v�����G���[:" + se.SocketErrorCode.ToString();
                throw new KagamiException("�X�g���[���v�����ɃG���[���������܂���(wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")");
            }
            catch (Exception e)
            {
                sock.Close();
                // IM�ڑ�NG�����ݒ肳��Ă�����Đ�
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "�X�g���[���v���������G���[";
                throw new KagamiException("�X�g���[���v�����ɃG���[���������܂���(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")");
            }
        }

        /// <summary>
        /// �C���|�[�g�悩��DataStream��M���[�v����
        /// ��M�ł��Ȃ��Ȃ����烋�[�v�I��
        /// </summary>
        private void RecvStreamLoop()
        {
            // ��M�f�[�^��byte[]�z��Ɋi�[���āA
            // ASF1�u���b�N���擾�ł�����1�u���b�N���ɃL���[�C���O��������ɂ���
            MemoryStream ms = new MemoryStream();
            int recv_timeout = 0;
            int ava_size = 0;
            int blk_size = 0;
            int rsp_size = 0;
            byte[] recv = new byte[1];
            byte[] asf_head = new byte[4];
            byte[] blk = null;

            // �X�g���[����M������ɏo���Ă����true�B
            // ImportStatus�͊O�����珑����������̂ŁA
            // TimeOut����ł͂�����𗘗p
            bool once_status = false;

            // �ŏ���Status.ImportStatus��false�ɂȂ��Ă���
            // ASF_STREAMING_DATA��M��true�ɂ���
            try
            {
                while (Status.RunStatus && Status.ImportURL != "�ҋ@��")
                {
                    // ���r���[�Ɏ�M�����u���b�N�����邩�H
                    // rsp_size��0�ȊO�Ȃ�A���r���[�Ɏ�M������ԂɂȂ��Ă���
                    if (rsp_size == 0)
                    {// ���r���[�ȃf�[�^�͖����B�擪����V�K�Ɏ�M
                        ava_size = sock.Available;
                        // ASF�u���b�N����m�邽�߂ɍŒ�4byte��M������
                        if (ava_size > 4)
                        {
                            // �ǂݎ��\�f�[�^���L��΃^�C���A�E�g�J�E���^�����Z�b�g
                            recv_timeout = 0;
                            // 4byte�ȏ��M�\�Ȃ�A�܂�type�`�F�b�N
                            #region type 1byte�ڎ�M���`�F�b�N
                            sock.Receive(recv);
                            if (recv[0] != 0x24)
                            {
                                #region type1�ُ�
                                // type���ُ�B�Ƃ肠����ms2�ɑޔ�
                                ms.WriteByte(recv[0]);
                                // ms2��1000byte�𒴂��Ă����烆�[�U�ɑ��M
                                if (ms.Length > 1000)
                                {
                                    // ���M
                                    Status.Client.StreamWrite(ms.ToArray());
                                    // ��x������čĕߑ����Ă���
                                    ms.Dispose();
                                    ms = new MemoryStream();
                                }
                                #endregion
                                // �ēx�ŏ������M���Ȃ���
                                continue;
                            }
                            #endregion
                            #region type 2byte�ڎ�M���`�F�b�N
                            sock.Receive(recv);
                            switch (recv[0])
                            {
                                case 0x43: // ASF_STREAMING_CLEAR(0x2443)
                                case 0x45: // ASF_STREAMING_END_TRANS(0x2445)
                                case 0x48: // ASF_STREAMING_HEADER(0x2448)
                                    break;
                                case 0x44: // ASF_STREAMING_DATA(0x2444)
                                    // DATA�u���b�N����M������ImportStatus����ɑJ��
                                    if (Status.ImportStatus == false)
                                    {
                                        Status.ImportStartTime = DateTime.Now;
                                        Status.ClientTime = DateTime.Now;
                                        Status.ImportStatus = true;
                                        Status.RetryCounter = 0;
                                        once_status = true;
                                        // IM�ڑ����������ݒ肳��Ă�����Đ�
                                        if (File.Exists(Front.Opt.SndConnOkFile))
                                            PlaySound(Front.Opt.SndConnOkFile);
                                    }
                                    break;
                                default:
                                    #region type2�ُ�
                                    // type�ُ�B������ms2�ɑޔ�
                                    ms.WriteByte(0x24);
                                    ms.WriteByte(recv[0]);
                                    // ms2��1000byte�𒴂��Ă����烆�[�U�ɑ��M
                                    if (ms.Length > 1000)
                                    {
                                        // ���M
                                        Status.Client.StreamWrite(ms.ToArray());
                                        // ��x������čĕߑ����Ă���
                                        ms.Dispose();
                                        ms = new MemoryStream();
                                    }
                                    #endregion
                                    // �ēx�ŏ������M���Ȃ���
                                    continue;
                            }
                            #endregion
                            // type����
                            #region type�ُ펞�̎c�[������Α��M���Ă���
                            if (ms.Length > 0)
                            {
                                // ���M
                                Status.Client.StreamWrite(ms.ToArray());
                                // ��x������čĕߑ����Ă���
                                ms.Dispose();
                                ms = new MemoryStream();
                            }
                            #endregion
                            // asf_head��type����������
                            asf_head[0] = 0x24;
                            asf_head[1] = recv[0];

                            // ASF block size���擾1
                            sock.Receive(recv);
                            asf_head[2] = recv[0];
                            // ASF block size���擾2
                            sock.Receive(recv);
                            asf_head[3] = recv[0];
                            // block size�Z�o
                            blk_size = (asf_head[3] << 8) + asf_head[2];

                            // block�T�C�Y���f�[�^�����Ă邩�`�F�b�N
                            ava_size = sock.Available;
                            if (ava_size >= blk_size)
                            {// ASF1�u���b�N���̃f�[�^�����Ă���
                                blk = new byte[4 + blk_size];
                                blk[0] = asf_head[0];
                                blk[1] = asf_head[1];
                                blk[2] = asf_head[2];
                                blk[3] = asf_head[3];
                                rsp_size = 0;
                                // �f�[�^��M
                                while (blk_size > rsp_size)
                                    rsp_size += sock.Receive(blk, 4 + rsp_size, blk_size - rsp_size, SocketFlags.None);
                                // �N���C�A���g�ɑ��M
                                Status.Client.StreamWrite(blk);
                                rsp_size = 0;   // ���r���[�f�[�^�Ȃ�
                                blk = null;     // GC�𑁂߂邽�߂̖����Inull�ݒ�
                            }
                            else
                            {// ASF1�u���b�N���̃f�[�^�����Ă��Ȃ�
                                // ���Ă�Ƃ���܂Ŏ�M���āA��M�T�C�Y��blk_len�ɐݒ肵�Ă���
                                blk = new byte[4 + blk_size];
                                blk[0] = asf_head[0];
                                blk[1] = asf_head[1];
                                blk[2] = asf_head[2];
                                blk[3] = asf_head[3];
                                rsp_size = 0;
                                // �r���܂ł̃f�[�^��M
                                while (ava_size > rsp_size)
                                    rsp_size += sock.Receive(blk, 4 + rsp_size, ava_size - rsp_size, SocketFlags.None);
                                // rsp_size��blk��ێ������܂܍ă��[�v
                            }
                        }
                        else
                        {
                            // �ǂݎ��\�f�[�^��4byte�ȉ������������
                            // �^�C���A�E�g�J�E���^��UP
                            Thread.Sleep(50);
                            recv_timeout++;
                            if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                                break;  //Timeout
                        }
                    }
                    else
                    {// ���r���[�ȃf�[�^���L��B����Ȃ��u���b�N����M
                        ava_size = sock.Available;
                        if (ava_size > (blk_size - rsp_size))
                        {// ����Ȃ��u���b�N�T�C�Y���̃f�[�^���S�ė��Ă���
                            // �ǂݎ��\�f�[�^���L��̂Ń^�C���A�E�g�J�E���^�����Z�b�g
                            recv_timeout = 0;
                            while (blk_size > rsp_size)
                                rsp_size += sock.Receive(blk, 4 + rsp_size, blk_size - rsp_size, SocketFlags.None);
                            // �N���C�A���g�ɑ��M
                            Status.Client.StreamWrite(blk);
                            rsp_size = 0;   // ���r���[�f�[�^�Ȃ�
                            blk = null;     // GC�𑁂߂邽�߂̖����Inull�ݒ�
                        }
                        else if (ava_size > 0)
                        {// �f�[�^�͗��Ă��邪�A�P�u���b�N�����`������ɂ͎���Ȃ�
                            // �ǂݎ��\�f�[�^���L��̂Ń^�C���A�E�g�J�E���^�����Z�b�g
                            recv_timeout = 0;
                            // �f�[�^��M�B�P�񂾂��B
                            rsp_size += sock.Receive(blk, 4 + rsp_size, ava_size, SocketFlags.None);
                        }
                        else
                        {
                            // �f�[�^���܂��������Ă��Ȃ�
                            // �^�C���A�E�g�J�E���^��UP
                            Thread.Sleep(50);
                            recv_timeout++;
                            if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                                break;  //Timeout
                        }
                    }
                }
                // while���[�v����
                // �I�����R�̔�����s��
                if (Status.RunStatus == false || (Status.ImportStatus == false && once_status == true))
                {
                    // RunStatus��false�ɂȂ���(GUI����̐ؒf)�A�܂���
                    // once_status��true(�����M��)��ImportStatus��false(�O������̐ؒf�v��)���󂯂��ꍇ

                    Front.AddLogData(1, Status, "�C���|�[�g�ڑ��̐ؒf�v������M���܂���");
                    // ���[�U�w���ɂ��ؒf�ł͐ؒf����炳�Ȃ�
                    //if (File.Exists(Front.Opt.SndDiscFile))
                    //    PlaySound(Front.Opt.SndDiscFile);
                }
                else
                {
                    Front.AddLogData(1, Status, "�C���|�[�g�悩��̎�M���^�C���A�E�g���܂���");
                    Status.ImportErrorContext = "�C���|�[�g��M�^�C���A�E�g";
                    Status.ImportError++;
                    // IM�ؒf�����ݒ肳��Ă�����ʃX���b�h�ōĐ�����
                    if (File.Exists(Front.Opt.SndDiscFile))
                        PlaySound(Front.Opt.SndDiscFile);
                }
            }
            catch (SocketException se)
            {
                Front.AddLogData(1, Status, "�C���|�[�g��M�G���[(wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")");
                Status.ImportErrorContext = "�C���|�[�g��M�G���[(wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")";
                Status.ImportError++;
                // IM�ؒf�����ݒ肳��Ă�����ʃX���b�h�ōĐ�����
                if (File.Exists(Front.Opt.SndDiscFile))
                    PlaySound(Front.Opt.SndDiscFile);
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "�C���|�[�g��M�G���[(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")");
                Status.ImportErrorContext = "�C���|�[�g��M�����G���[";
                Status.ImportError++;
                // IM�ؒf�����ݒ肳��Ă�����ʃX���b�h�ōĐ�����
                if (File.Exists(Front.Opt.SndDiscFile))
                    PlaySound(Front.Opt.SndDiscFile);
            }
            finally
            {
                Front.AddLogData(0, Status, "�G�N�X�|�[�g�ւ̏����o�����I�����܂�");
                Status.ImportStatus = false;
            }
        }

        #endregion

        #region Push�z�M�֌W
        /// <summary>
        /// Push�z�M���N�G�X�g��t�^�X�N
        /// </summary>
        private void PushReqTask(object obj)
        {
            Socket _sock = (Socket)obj;
            string _ua = "";

            _sock.SendTimeout = (int)Front.Sock.SockConnTimeout;       // Import�ڑ� �w�b�_�擾�v�����M�̃^�C���A�E�g�l
            _sock.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    // Import�ڑ� �w�b�_�擾������M�̃^�C���A�E�g�l

            string _ip = ((IPEndPoint)_sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)_sock.RemoteEndPoint).Port;

            try
            {
                Front.AddLogData(0, Status, _ip + "����ڑ��v��");

                #region Kick�Ώۂ��`�F�b�N
                if (Status.IsKickCheck(_sock) == false)
                {
                    Front.AddLogData(0, Status, "Kick�Ώۂ̂��ߐڑ������ۂ��܂��B [" + _ip + "]");
                    #region Kick����
                    bool _not_found = true;
                    // KickItem����Y��IP����
                    lock (Status.Gui.KickItem)
                    {
                        foreach (ListViewItem _item in Status.Gui.KickItem)
                        {
                            if (_item.Text == _ip)   // clmKickViewIP
                            {
                                Event.EventUpdateKick(Status.Kagami, _item, 0);
                                _not_found = false;
                                break;
                            }
                        }
                        // ������Ȃ������ꍇ�͐V�K�ǉ�
                        if (_not_found)
                            Status.AddKick(_ip, 1);
                    }
                    // Kick��ɂ�503�𑗂�
                    // ����̃��N�G�X�g���󂯎���Ă���ԐM
                    char[] end = { '\n', '\n' };
                    byte[] reqBytes = new byte[5000];
                    Encoding _enc = Encoding.ASCII; // "euc-jp"
                    int i = 0;
                    int j = 0;
                    try
                    {
                        for (; j < 5000; j++)
                        {
                            _sock.Receive(reqBytes, j, 1, SocketFlags.None);
                            if (reqBytes[j] == '\r') continue;
                            if (reqBytes[j] == end[i]) i++; else i = 0;
                            if (i >= 2) break;
                        }
                        if (i >= 2)
                        {
                            // �A�ł��Ȃ��悤�ɉ����ԋp��x��������
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            // �r�W�[���b�Z�[�W���M
                            string str = _enc.GetString(reqBytes, 0, j);
                            Front.AddLogDetail("RecvReqMsg(Push)Sta-----\r\n" + str + "\r\nRecvReqMsg(Push)End-----");
                            Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + Front.BusyString + "\r\nSendRspMsg(Push)End-----"); 
                            _sock.Send(_enc.GetBytes(Front.BusyString));
                        }
                        else
                        {
                            // ���N�G�X�g����������ꍇ�͉������炸�؂�
                            // �A�ł��Ȃ��悤�ɐؒf��x��������
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                        }
                    }
                    catch
                    {
                        // MMS(TCP)�ȊO�Ȃ�r�W�[���b�Z�[�W���MNG
                        if (_enc.GetString(reqBytes, 12, 4) != "MMS ")
                            Front.AddLogData(1, Status, "�r�W�[���b�Z�[�W�����M�ł��܂���ł��� [" + _ip + "]");
                    }
                    #endregion
                    _sock.Close();
                    return;
                }
                #endregion

                while (true)
                {
                    // ���肩��̃��N�G�X�g�f�[�^����M�Ȃ�AKeep-Alive�̂��ߑҋ@
                    while (Status.RunStatus && Status.ImportURL == "�ҋ@��" && Status.Type == 2 && _sock.Connected && _sock.Available == 0)
                        Thread.Sleep(500);

                    // Sock�ؒf���ꂽ�ꍇ
                    if (!_sock.Connected)
                    {
                        _sock.Close();
                        return;
                    }
                    // Sock�ؒf�ȊO�ُ̈�E���^�X�N����J�n�v����M�ɂ��ؒf����p�^�[��
                    if (!Status.RunStatus || Status.ImportURL != "�ҋ@��" || Status.Type != 2)
                    {
                        // Setup�������Z�b�V������Start�������Z�b�V�������ʂ̏ꍇ�A
                        // Setup�Z�b�V������408�͕Ԃ��Ȃ��悤�ɂ���
                        if (!bSetup || SetupIp != _ip)
                        {
                            // �^�C���A�E�g�����𑗐M���Đؒf����
                            Encoding _enc = Encoding.ASCII;
                            string _ackMsg = "HTTP/1.1 408 Request Timeout\r\n" +
                                            "Server: Cougar/9.01.01.3862\r\n" +
                                            "Date: " + DateTime.Now.ToString("R") + "\r\n" +
                                            "Pragma: no-cache, timeout=60000\r\n" +
                                            "Set-Cookie: push-id=0\r\n" +
                                            "Supported: com.microsoft.wm.srvppair, com.microsoft.wm.sswitch, com.microsoft.wm.predstrm, com.microsoft.wm.fastcache, com.microsoft.wm.startupprofile\r\n" +
                                            "Connection: close\r\n\r\n";
                            Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + _ackMsg + "\r\nSendRspMsg(Push)End-----");
                            try
                            {
                                _sock.Send(_enc.GetBytes(_ackMsg));
                            }
                            catch
                            {
                                Front.AddLogData(1, Status, "�^�C���A�E�g���b�Z�[�W�����M�ł��܂���ł��� [" + _ip + "]");
                            }
                        }
                        _sock.Close();
                        return;
                    }

                    //Setup��M�ς݂Ȃ�ASetup����IP�ȊO�̐V�K�v���͒e��
                    if (bSetup && SetupIp != _ip)
                    {
                        Front.AddLogData(1, Status, "Setup�v�����Ƃ͈قȂ�IP�A�h���X����̐ڑ���ؒf���܂� [" + _ip + "]");
                        _sock.Close();
                        return;
                    }

                    //Push�z�M��t�`�F�b�N
                    _ua = PushAcceptUser(_sock);

                    //��t���ۂ��ꂽ�ڑ���ؒf
                    if (_ua == "")
                    {
                        _sock.Close();
                        return;
                    }
                    else if (_ua == "push-setup")
                    {
                        //Push�z�MSetup�v��
                        SetupIp = _ip;
                        bSetup = true;
                        continue;
                    }
                    else if (_ua != "")
                    {
                        // Push�z�MStart�v��
                        // ->Setup�v����M�ς݃`�F�b�N
                        if (bSetup == false)
                        {
                            Front.AddLogData(1, Status, "Push�z�MSetup�v���O��Push�z�MStart�v������M���܂����B");
                            Encoding _enc = Encoding.ASCII;
                            string _ackMsg = "HTTP/1.1 408 Request Timeout\r\n" +
                                            "Server: Cougar/9.01.01.3862\r\n" +
                                            "Date: " + DateTime.Now.ToString("R") + "\r\n" +
                                            "Pragma: no-cache, timeout=60000\r\n" +
                                            "Set-Cookie: push-id=0\r\n" +
                                            "Supported: com.microsoft.wm.srvppair, com.microsoft.wm.sswitch, com.microsoft.wm.predstrm, com.microsoft.wm.fastcache, com.microsoft.wm.startupprofile\r\n" +
                                            "Connection: close\r\n\r\n";
                            Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + _ackMsg + "\r\nSendRspMsg(Push)End-----");
                            try
                            {
                                _sock.Send(_enc.GetBytes(_ackMsg));
                            }
                            catch
                            {
                                Front.AddLogData(1, Status, "�^�C���A�E�g���b�Z�[�W�����M�ł��܂���ł��� [" + _ip + "]");
                            }
                            _sock.Close();
                            return;
                        }
                        // Push�z�M��tOK
                        sock = _sock;
                        Status.SetUserIP = _ip;
                        Status.ImportURL = "push://" + _ip;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "Push�z�M���N�G�X�g��t�G���[(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")[" + _ip + ":" + _port + "]");
                Encoding _enc = Encoding.ASCII;
                string _ackMsg = "HTTP/1.1 408 Request Timeout\r\n" +
                                "Server: Cougar/9.01.01.3862\r\n" +
                                "Date: " + DateTime.Now.ToString("R") + "\r\n" +
                                "Pragma: no-cache, timeout=60000\r\n" +
                                "Set-Cookie: push-id=0\r\n" +
                                "Supported: com.microsoft.wm.srvppair, com.microsoft.wm.sswitch, com.microsoft.wm.predstrm, com.microsoft.wm.fastcache, com.microsoft.wm.startupprofile\r\n" +
                                "Connection: close\r\n\r\n";
                Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + _ackMsg + "\r\nSendRspMsg(Push)End-----");
                try
                {
                    _sock.Send(_enc.GetBytes(_ackMsg));
                }
                catch
                {
                    Front.AddLogData(1, Status, "�^�C���A�E�g���b�Z�[�W�����M�ł��܂���ł��� [" + _ip + "]");
                }
                _sock.Close();
            }
        }

        /// <summary>
        /// �ڑ��v����Push�z�M�J�n���N�G�X�g�����肷��B
        /// </summary>
        /// <param name="sock">�v������socket</param>
        /// <returns>�v������UserAgent</returns>
        private string PushAcceptUser(Socket sock)
        {
            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;

            try
            {
                #region �v���g�R���`�F�b�N+UserAgent�擾
                string userAgent;
                //int priKagamiPort;
                System.Text.Encoding enc;

                char[] end = { '\n', '\n' };
                byte[] reqBytes = new byte[5000];
                int i = 0;
                int j = 0;

                //���N�G�X�g�w�b�_�̉��s�R�[�h�͕���CR+LF������LF�݂̂ɂ��Ή�����
                try
                {
                    for (; j < 5000; j++)
                    {
                        sock.Receive(reqBytes, j, 1, SocketFlags.None);
                        if (reqBytes[j] == '\r') continue;
                        if (reqBytes[j] == end[i]) i++; else i = 0;
                        if (i >= 2) break;
                    }
                }
                catch (Exception e)
                {
                    // MMS(TCP)Check
                    // 12�`16byte�ڂ�"MMS "��MMS(TCP)
                    enc = Front.GetCode(reqBytes, j);
                    if (enc.GetString(reqBytes, 12, 4).Equals("MMS "))
                    {
                        //MMS�̓��N�G�X�g���e�̃��O�o�͂��Ȃ�
                        //NSPlayer�ڑ��ɐ؂�ւ������邽�߁A�N���C�A���g�ɂ͉������炸�ؒf�B
                        Front.AddLogData(0, Status, "�Ή����Ă��Ȃ����N�G�X�g(MMST)�̂��ߐؒf���܂� [" + _ip + "]");
                    }
                    else
                    {
                        //��M�ł����Ƃ���܂ł̃��N�G�X�gMsg�����O�o��
                        string _str = enc.GetString(reqBytes, 0, j);
                        Front.AddLogDetail("RecvReqMsg(Push)Sta-----\r\n" +
                                     _str +
                                     "\r\nRecvReqMsg(Push)End-----");
                        if (e is SocketException)
                        {
                            SocketException se = (SocketException)e;
                            Front.AddLogData(1, Status, "���N�G�X�g��M�^�C���A�E�g [" + _ip + "] wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString());
                        }
                        else
                        {
                            Front.AddLogData(1, Status, "���N�G�X�g��M�^�C���A�E�g [" + _ip + "] (�����G���[:" + e.Message + ")");
                        }
                    }
                    return "";
                }
                if (i < 2)
                {
                    //��M�ł����Ƃ���܂ł̃��N�G�X�gMsg�����O�o��
                    enc = Front.GetCode(reqBytes, j);
                    Front.AddLogDetail("RecvReqMsg(Push)Sta-----\r\n" +
                                 enc.GetString(reqBytes, 0, j) +
                                 "\r\nRecvReqMsg(Push)End-----");
                    Front.AddLogData(1, Status, "���N�G�X�g������������̂Őؒf���܂�(over5KB) [" + _ip + "]");
                    // �A�ł��Ȃ��悤�ɐؒf��x��������
                    Thread.Sleep((int)Front.Sock.SockCloseDelay);
                    //�ؒf���邽��null�ԋp
                    return "";
                }

                enc = Front.GetCode(reqBytes, j);
                string str = enc.GetString(reqBytes, 0, j);
                if (str.IndexOf("\r\n") < 0)
                {
                    //���s�R�[�h��LF�݂̂Ȃ�CR+LF�ɒu������
                    str = str.Replace("\n", "\r\n");
                }
                // �v���g�R�����o��
                string protocol = str.Substring(str.IndexOf("\r\n") - 8, 4);
                // Context������Ȃ���o���Ă݂�
                // ������Push�z�MStart�v���Ȃ���o���Ȃ�
                if (sock.Available > 0 &&
                    str.IndexOf("Content-Type: application/x-wms-pushstart") < 0)
                {
                    try
                    {
                        int _len = sock.Available;
                        byte[] _dat = new byte[_len];
                        _len = sock.Receive(_dat);
                        if (_len > 0)
                        {
                            str += enc.GetString(_dat, 0, _len);
                        }
                    }
                    catch { }
                }
                Front.AddLogDetail("RecvReqMsg(Push)Sta-----\r\n" + str + "\r\nRecvReqMsg(Push)End-----");

                if (protocol != "HTTP")
                {
                    Front.AddLogData(0, Status, "�Ή����Ă��Ȃ����N�G�X�g(" + protocol + ")�̂��ߐؒf���܂� [" + _ip + "]");
                    //RTSP�Ƃ��̓R�R��NG�B�Ȃ̂Őؒf�x���͂����Ȃ��B
                    //�ؒf���邽��null�ԋp
                    return "";
                }

                // UserAgent�擾
                try
                {
                    userAgent = str.Substring(str.IndexOf("User-Agent: ") + 12, str.IndexOf("\r\n", str.IndexOf("User-Agent: ")) - str.IndexOf("User-Agent: ") - 12);
                }
                catch
                {
                    Front.AddLogData(0, Status, "UserAgent���s���Ȃ��ߐڑ������ۂ��܂� [" + _ip + "]");
                    // UserAgent�������Ȃ珈���p���o���Ȃ��̂ŁA400�𑗂�
                    // �A�ł��Ȃ��悤�ɉ������M��x��������
                    Thread.Sleep((int)Front.Sock.SockCloseDelay);
                    string ackMsg = "HTTP/1.0 400 Bad Request\r\n" +
                                    "Server: " + Front.AppName + "\r\n" +
                                    "Cache-Control: no-cache\r\n" +
                                    "Pragma: no-cache\r\n" +
                                    "Connection: close\r\n" +
                                    "Content-Type: text/html\r\n\r\n" +
                                    "<html><head><title>400 Bad Request</title></head>\r\n" +
                                    "<body><h1>Bad Request</h1>\r\n" +
                                    "Your client doesn't contain UserAgent information in the request.\r\n" +
                                    "</body></html>\r\n";
                    Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + ackMsg + "\r\nSendRspMsg(Push)End-----");
                    sock.Send(enc.GetBytes(ackMsg));
                    //�ؒf���邽��null�ԋp
                    return "";
                }
                /*
                // ���|�[�g�����o��
                int _idx = str.IndexOf("Pragma: kagami-port=");
                if (_idx < 0)
                {
                    priKagamiPort = 0;
                }
                else
                {
                    _idx += 20; // �|�[�g�ԍ��擪�Ɉړ�
                    int _len = str.IndexOf("\r\n", _idx) - _idx;
                    if (_len == 0)
                    {
                        priKagamiPort = 0;
                    }
                    else
                    {
                        try
                        {
                            priKagamiPort = int.Parse(str.Substring(_idx, _len));
                        }
                        catch
                        {
                            priKagamiPort = 0;
                        }
                    }
                }
                 */
                #endregion

                // �u���E�U����̃A�N�Z�X
                if (Front.Opt.BrowserView == true &&
                    (userAgent.IndexOf("Mozilla") == 0 ||
                     userAgent.IndexOf("Opera") == 0))
                {
                    Front.AddLogData(0, Status, "Web�u���E�U����̃A�N�Z�X�ł� UA: " + userAgent);
                    Front.AddLogData(0, Status, "�R�l�N�V������ؒf���܂� [" + _ip + "]");
                    //�ؒf���邽��null�ԋp
                    return "";
                }
                else
                {
                    // Push�z�M����
                    if (str.Contains("Content-Type: application/x-wms-pushsetup"))
                    {
                        //int pid = Environment.TickCount & 0xFFFF;
                        Front.AddLogData(0, Status, "Push�z�MSetup�v������M���܂��� UA: " + userAgent);
                        string ackMsg =
                            "HTTP/1.1 204 No Content\r\n" +
                            "Server: Cougar/9.01.01.3841\r\n" + // + Front.AppName + "\r\n" +
                            "Content-Length: 0\r\n" +
                            "Date: " + DateTime.Now.ToString("R") + "\r\n" +
                            "Pragma: no-cache, timeout=60000\r\n" +
                            "Cache-Control: no-cache\r\n" +
                            //"Set-Cookie: push-id=" + pid + "\r\n" +
                            "Set-Cookie: push-id=0\r\n" +
                            "Supported: com.microsoft.wm.srvppair, com.microsoft.wm.sswitch, com.microsoft.wm.predstrm, com.microsoft.wm.fastcache, com.microsoft.wm.startupprofile\r\n\r\n";
                        Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + ackMsg + "\r\nSendRspMsg(Push)End-----");
                        sock.Send(enc.GetBytes(ackMsg));
                        return "push-setup";
                    }
                    else if (str.Contains("Content-Type: application/x-wms-pushstart"))
                    {
                        Front.AddLogData(0, Status, "Push�z�MStart�v������M���܂��� UA: " + userAgent);
                        return userAgent;
                    }
                    else
                    {
                        Front.AddLogData(0, Status, "WMEncoder�ȊO�ł̐ڑ��ł� UA: " + userAgent);
                        // �ؒf�x��
                        // �E�E�EPush�z�M����IM���؂ꂽ���ɃN���C�A���g��
                        //       ��ʂɍĐڑ��ɗ���̂Őؒf�x��������
                        Thread.Sleep((int)Front.Sock.SockCloseDelay);
                        // ���l���b�Z�[�W���M
                        // �E�E�E�������炸�؂����ق����������Ȃ��B�B
                        //Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + Front.BusyString + "\r\nSendRspMsg(Push)End-----");
                        //sock.Send(enc.GetBytes(Front.BusyString));
                        Front.AddLogData(0, Status, "�R�l�N�V������ؒf���܂� [" + _ip + "]");
                        return "";
                    }
                    /*
                     * ���N�G�X�g�P����
                    POST // HTTP/1.1
                    Content-Type: application/x-wms-pushsetup
                    X-Accept-Authentication: Negotiate, NTLM, Digest
                    User-Agent: WMEncoder/11.0.5721.5145
                    Host: localhost:8888
                    Content-Length: 16
                    Connection: Keep-Alive
                    Cache-Control: no-cache
                    Cookie: push-id=0

                     * �����P����
                    HTTP/1.1 204 No Content
                    Server: Servet-agent
                    Content-Length: 0
                    Date: Tue, 09 Jan 2007 10:02:58 GMT
                    Pragma: no-cache, timeout=60000
                    Cache-Control: no-cache
                    Set-Cookie: push-id=35201712
                    Supported: com.microsoft.wm.srvppair, com.microsoft.wm.sswitch, com.microsoft.wm.predstrm, com.microsoft.wm.fastcache, com.microsoft.wm.startupprofile
                     * 
                    �T�[�o��KeepAlive���Ă������ƁB
                     * 
                     * ���N�G�X�g�Q����
                    POST /test HTTP/1.1
                    Content-Type: application/x-wms-pushstart
                    X-Accept-Authentication: NTLM, Digest
                    User-Agent: WMEncoder/9.0.0.3287
                    Host: 192.168.0.1:8000
                    Content-Length: 2147483647
                    Connection: Keep-Alive
                    Cache-Control: no-cache
                    Cookie: push-id=35201712
                    $H�`�ȉ��X�g���[���f�[�^�`
                     * 
                     * �T�[�o��M�҂��^�C���A�E�g
                    HTTP/1.1 408 Request Timeout
                    Server: Cougar/9.01.01.3862
                    Date: Fri, 17 Oct 2008 06:47:57 GMT
                    Pragma: no-cache, timeout=60000
                    Set-Cookie: push-id=2692261714
                    Supported: com.microsoft.wm.srvppair, com.microsoft.wm.sswitch, com.microsoft.wm.predstrm, com.microsoft.wm.fastcache, com.microsoft.wm.startupprofile
                    Connection: close
                     *
                     */
                }
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "�N���C�A���g��t�G���[(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")[" + _ip + "]");
                return "";
            }
            // �����ɂ͗��Ȃ�
            //return "";
        }


        /// <summary>
        /// Push�z�M�p�C���|�[�g�^�X�N
        /// </summary>
        private void PushImportTask()
        {
            Status.ImportStartTime = DateTime.Now;
            Status.BusyCounter = 0;
            Status.RetryCounter = 0;
            Status.ImportError = 0;
            Status.ExportError = 0;
            Status.ExportCount = 0;
            // ���x���N���A
            Status.TrafficCount = 0;
            Status.AverageDLSpeed = 0;
            Status.MaxDLSpeed = 0;

            // �G�N�X�|�[�g�̉����f�[�^�쐬
            string str =
                "HTTP/1.0 200 OK\r\n" +
                "Server: Rex/9.0.0.2980\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Pragma: no-cache\r\n" +
                "Pragma: client-id=3320437311\r\n" +
                "Pragma: features=\"seekable,stridable\"\r\n" +
                "X-Server: " + Front.AppName + "\r\n" +
                "Connection: close\r\n" +
                "Content-Type: application/x-mms-framed\r\n\r\n";
            //�w�b�_��Ɖ����q���郁�����X�g���[��
            MemoryStream ms1, ms2;
            ms1 = new MemoryStream();
            ms2 = new MemoryStream();
            Encoding enc = Encoding.ASCII;
            int count = enc.GetBytes(str).Length;
            ms1.Write(enc.GetBytes(str), 0, count);
            Status.DataRspMsg10 = ms1.ToArray();

            byte[] ack = new byte[1];
            byte[] asf_type = new byte[] { 0x24, 0x48 };    // ASF_HEADER
            byte[] asf_head = new byte[12];
            int len = 0;
            byte[] ack_end = new byte[] { 0x00, 0x00, 0x01, 0x01 };
            int i = 0;
            int j = 0;
            try
            {
                // ASF HEAD���o
                j = 0;
                while (Status.RunStatus && Status.ImportURL != "�ҋ@��")
                {
                    sock.Receive(ack);
                    j++;
                    if (ack[0].Equals(asf_type[i])) i++; else i = 0;
                    if (i >= asf_type.Length)
                    {
                        // push�z�M�ł�ASF HEAD BLOCK��pull�z�M�ƈقȂ�̂Ŏ��삷��
                        sock.Receive(ack);
                        len = ack[0];
                        sock.Receive(ack);
                        len += ack[0] * 0x100;
                        len += 8;
                        asf_head[0] = 0x24;
                        asf_head[1] = 0x48;
                        asf_head[2] = (byte)(len & 0x00FF);
                        asf_head[3] = (byte)((len & 0xFF00) >> 8);
                        asf_head[4] = 0x00;
                        asf_head[5] = 0x00;
                        asf_head[6] = 0x00;
                        asf_head[7] = 0x00;
                        asf_head[8] = 0x00;
                        asf_head[9] = 0x0C;
                        asf_head[10] = asf_head[2];
                        asf_head[11] = asf_head[3];
                        ms2.Write(asf_head, 0, 12);
                        i = 0;
                        break;
                    }
                    else if (j > 50000)
                    {
                        //�擪50KB����ASF�w�b�_����
                        throw new KagamiException("�X�g���[���w�b�_�̎擾���ɃG���[���������܂���(NotFoundASFHead at Top50KB)");
                    }
                }
                // ASF HEAD���o����&����HEAD��ms2�ɏ������ݍς�
                // ASF HEAD�̖{�̂���M����ms2�ɏ�������
                j = 0;
                while (Status.RunStatus && Status.ImportURL != "�ҋ@��")
                {
                    sock.Receive(ack);
                    ms2.WriteByte(ack[0]);
                    j++;
                    if (ack[0].Equals(ack_end[i])) i++; else i = 0;

                    if (i >= ack_end.Length)
                    {
                        //ASF HEAD�̏I�������o
                        //ms�̔z��̒����̊m�F
                        Status.HeadStream = ms2.ToArray();
                        //Front.AddLogData(Status, "�w�b�_�擾����");
                        try
                        {
                            // �w�b�_��̓e�X�g
                            CheckWMVHeader(Status.HeadStream, 12);
                        }
                        catch { }
                        break;
                    }
                    else if (j > 50000)
                    {
                        throw new KagamiException("�X�g���[���w�b�_�̎擾���ɃG���[���������܂���(StreamHeader>50KBover)");
                    }
                }

                // �G�N�X�|�[�g�̉����f�[�^�쐬
                str =
                    "HTTP/1.0 200 OK\r\n" +
                    "Server: Rex/9.0.0.2980\r\n" +
                    "Cache-Control: no-cache\r\n" +
                    "Pragma: no-cache\r\n" +
                    "Pragma: client-id=3320437311\r\n" +
                    "Pragma: features=\"seekable,stridable\"\r\n" +
                    "X-Server: " + Front.AppName + "\r\n" +
                    "Content-Type: application/vnd.ms.wms-hdr.asfv1\r\n" +
                    "Content-Length: " + Status.HeadStream.Length + "\r\n\r\n";
                //�w�b�_��Ɖ����q���郁�����X�g���[��
                MemoryStream ms3;
                ms3 = new MemoryStream();
                count = enc.GetBytes(str).Length;
                ms3.Write(enc.GetBytes(str), 0, count);
                Status.HeadRspMsg10 = ms3.ToArray();

                if (Status.ImportURL == "�ҋ@��")
                {
                    Front.AddLogData(1, Status, "Push�z�M�X�g���[���w�b�_�̎擾���ɏI���v�����󂯂܂���");
                }
                else
                {
                    // ASF HEAD��M����
                    // �C���|�[�g��M���[�v�J�n
                    Front.AddLogData(1, Status, "Push�z�M�\�[�X�̎�荞�݂��J�n���܂���");
                    RecvPushStreamLoop();
                    Front.AddLogData(1, Status, "Push�z�M�\�[�X�̎�荞�݂��I�����܂���");
                }
            }
            catch (KagamiException ke)
            {
                Front.AddLogData(ke.LogLv, Status, ke.Message);
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "Push�z�M�G���[(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")");
            }
            sock.Close();
            // �ш��񁕍ő�ڑ����̃��Z�b�g
            Status.MaxDLSpeed = 0;
            Status.AverageDLSpeed = 0;
            Status.TrafficCount = 0;
            Status.Connection = Status.Conn_UserSet;
        }

        /// <summary>
        /// �C���|�[�g�悩��DataStream��M���[�v����
        /// ��M�ł��Ȃ��Ȃ����烋�[�v�I��
        /// </summary>
        private void RecvPushStreamLoop()
        {
            // ��M�f�[�^��byte[]�z��Ɋi�[���āA
            // ASF1�u���b�N���擾�ł�����1�u���b�N���ɃL���[�C���O��������ɂ���
            MemoryStream ms = new MemoryStream();
            int recv_timeout = 0;
            int ava_size = 0;
            int blk_size = 0;
            int rsp_size = 0;
            uint seqno = 1;
            byte[] recv = new byte[1];
            byte[] asf_head = new byte[4];
            byte[] blk = null;
            bool end_flg = false; // ASF_STREAMING_END_TRANS��M�t���O

            // �X�g���[����M������ɏo���Ă����true�B
            // ImportStatus�͊O�����珑����������̂ŁA
            // TimeOut����ł͂�����𗘗p
            bool once_status = false;

            // �ŏ���Status.ImportStatus��false�ɂȂ��Ă���
            // ASF_STREAMING_DATA��M��true�ɂ���
            try
            {
                while (Status.RunStatus && Status.ImportURL != "�ҋ@��")
                {
                    // ���r���[�Ɏ�M�����u���b�N�����邩�H
                    // rsp_size��0�ȊO�Ȃ�A���r���[�Ɏ�M������ԂɂȂ��Ă���
                    if (rsp_size == 0)
                    {// ���r���[�ȃf�[�^�͖����B�擪����V�K�Ɏ�M
                        ava_size = sock.Available;
                        // ASF�u���b�N����m�邽�߂ɍŒ�4byte��M������
                        if (ava_size > 4)
                        {
                            // �ǂݎ��\�f�[�^���L��΃^�C���A�E�g�J�E���^�����Z�b�g
                            recv_timeout = 0;
                            // 4byte�ȏ��M�\�Ȃ�A�܂�type�`�F�b�N
                            #region type 1byte�ڎ�M���`�F�b�N
                            sock.Receive(recv);
                            if (recv[0] != 0x24)
                            {
                                #region type1�ُ�
                                if (end_flg)
                                {
                                    // end_flg == true�̎��͒�~�ˑ��ăX�^�[�g�̉\������
                                    // �{����keep-alive�ōăX�^�[�g�����ɐi�ނ��A�����ł͐ؒf����
                                    break;
                                }
                                else
                                {
                                    // type���ُ�B�Ƃ肠����ms2�ɑޔ�
                                    ms.WriteByte(recv[0]);
                                    // ms2��1000byte�𒴂��Ă����烆�[�U�ɑ��M
                                    if (ms.Length > 1000)
                                    {
                                        // ���M
                                        Status.Client.StreamWrite(ms.ToArray());
                                        // ��x������čĕߑ����Ă���
                                        ms.Dispose();
                                        ms = new MemoryStream();
                                    }
                                }
                                #endregion
                                // �ēx�ŏ������M���Ȃ���
                                continue;
                            }
                            #endregion
                            #region type 2byte�ڎ�M���`�F�b�N
                            sock.Receive(recv);
                            switch (recv[0])
                            {
                                case 0x43: // ASF_STREAMING_CLEAR(0x2443)
                                case 0x48: // ASF_STREAMING_HEADER(0x2448)
                                    break;
                                case 0x45: // ASF_STREAMING_END_TRANS(0x2445)
                                    end_flg = true;
                                    break;
                                case 0x44: // ASF_STREAMING_DATA(0x2444)
                                    // DATA�u���b�N����M������ImportStatus����ɑJ��
                                    if (Status.ImportStatus == false)
                                    {
                                        Status.ImportStartTime = DateTime.Now;
                                        Status.ClientTime = DateTime.Now;
                                        Status.ImportStatus = true;
                                        Status.RetryCounter = 0;
                                        once_status = true;
                                        // IM�ڑ����������ݒ肳��Ă�����Đ�
                                        if (File.Exists(Front.Opt.SndConnOkFile))
                                            PlaySound(Front.Opt.SndConnOkFile);
                                    }
                                    break;
                                default:
                                    #region type2�ُ�
                                    // type�ُ�B������ms2�ɑޔ�
                                    ms.WriteByte(0x24);
                                    ms.WriteByte(recv[0]);
                                    // ms2��1000byte�𒴂��Ă����烆�[�U�ɑ��M
                                    if (ms.Length > 1000)
                                    {
                                        // ���M
                                        Status.Client.StreamWrite(ms.ToArray());
                                        // ��x������čĕߑ����Ă���
                                        ms.Dispose();
                                        ms = new MemoryStream();
                                    }
                                    #endregion
                                    // �ēx�ŏ������M���Ȃ���
                                    continue;
                            }
                            #endregion
                            // type����
                            #region type�ُ펞�̎c�[������Α��M���Ă���
                            if (ms.Length > 0)
                            {
                                // ���M
                                Status.Client.StreamWrite(ms.ToArray());
                                // ��x������čĕߑ����Ă���
                                ms.Dispose();
                                ms = new MemoryStream();
                            }
                            #endregion
                            // seqno�X�V
                            seqno++;
                            // asf_head��type����������
                            asf_head[0] = 0x24;
                            asf_head[1] = recv[0];

                            // ASF block size���擾1
                            sock.Receive(recv);
                            asf_head[2] = recv[0];
                            // ASF block size���擾2
                            sock.Receive(recv);
                            asf_head[3] = recv[0];
                            // block size�Z�o
                            blk_size = (asf_head[3] << 8) + asf_head[2];

                            // block�T�C�Y���f�[�^�����Ă邩�`�F�b�N
                            ava_size = sock.Available;
                            if (ava_size >= blk_size)
                            {// ASF1�u���b�N���̃f�[�^�����Ă���
                                if (asf_head[1] != 0x45)
                                {
                                    blk_size += 8;
                                    blk = new byte[4 + blk_size];
                                    blk[0] = asf_head[0];
                                    blk[1] = asf_head[1];
                                    blk[2] = (byte)(blk_size & 0x00FF);
                                    blk[3] = (byte)((blk_size & 0xFF00) >> 8);
                                    blk[4] = (byte)(seqno & 0x000000FF);
                                    blk[5] = (byte)((seqno & 0x0000FF00) >> 8);
                                    blk[6] = (byte)((seqno & 0x00FF0000) >> 16);
                                    blk[7] = (byte)((seqno & 0xFF000000) >> 24);
                                    blk[8] = 0;
                                    blk[9] = 0;
                                    blk[10] = blk[2];
                                    blk[11] = blk[3];
                                    rsp_size = 8;
                                }
                                else
                                {
                                    // END_TRANS�̎���SeqNo���������Ȃ��B
                                    blk_size += 4;
                                    blk = new byte[4 + blk_size];
                                    blk[0] = asf_head[0];
                                    blk[1] = asf_head[1];
                                    blk[2] = (byte)(blk_size & 0x00FF);
                                    blk[3] = (byte)((blk_size & 0xFF00) >> 8);
                                    blk[4] = (byte)(seqno & 0x000000FF);
                                    blk[5] = (byte)((seqno & 0x0000FF00) >> 8);
                                    blk[6] = (byte)((seqno & 0x00FF0000) >> 16);
                                    blk[7] = (byte)((seqno & 0xFF000000) >> 24);
                                    rsp_size = 4;
                                }
                                // �f�[�^��M
                                while (blk_size > rsp_size)
                                    rsp_size += sock.Receive(blk, 4 + rsp_size, blk_size - rsp_size, SocketFlags.None);
                                // �N���C�A���g�ɑ��M
                                Status.Client.StreamWrite(blk);
                                rsp_size = 0;   // ���r���[�f�[�^�Ȃ�
                                blk = null;     // GC�𑁂߂邽�߂̖����Inull�ݒ�
                            }
                            else
                            {// ASF1�u���b�N���̃f�[�^�����Ă��Ȃ�
                                // ���Ă�Ƃ���܂Ŏ�M���āA��M�T�C�Y��blk_len�ɐݒ肵�Ă���
                                if (asf_head[1] != 0x45)
                                {
                                    blk_size += 8;
                                    blk = new byte[4 + blk_size];
                                    blk[0] = asf_head[0];
                                    blk[1] = asf_head[1];
                                    blk[2] = (byte)(blk_size & 0x00FF);
                                    blk[3] = (byte)((blk_size & 0xFF00) >> 8);
                                    blk[4] = (byte)(seqno & 0x000000FF);
                                    blk[5] = (byte)((seqno & 0x0000FF00) >> 8);
                                    blk[6] = (byte)((seqno & 0x00FF0000) >> 16);
                                    blk[7] = (byte)((seqno & 0xFF000000) >> 24);
                                    blk[8] = 0;
                                    blk[9] = 0;
                                    blk[10] = blk[2];
                                    blk[11] = blk[3];
                                    rsp_size = 8;
                                }
                                else
                                {
                                    blk_size += 4;
                                    blk = new byte[4 + blk_size];
                                    blk[0] = asf_head[0];
                                    blk[1] = asf_head[1];
                                    blk[2] = (byte)(blk_size & 0x00FF);
                                    blk[3] = (byte)((blk_size & 0xFF00) >> 8);
                                    blk[4] = (byte)(seqno & 0x000000FF);
                                    blk[5] = (byte)((seqno & 0x0000FF00) >> 8);
                                    blk[6] = (byte)((seqno & 0x00FF0000) >> 16);
                                    blk[7] = (byte)((seqno & 0xFF000000) >> 24);
                                    rsp_size = 4;
                                }
                                // �r���܂ł̃f�[�^��M
                                while (ava_size > rsp_size)
                                    rsp_size += sock.Receive(blk, 4 + rsp_size, ava_size - rsp_size, SocketFlags.None);
                                // rsp_size��blk��ێ������܂܍ă��[�v
                            }
                        }
                        else
                        {
                            // �ǂݎ��\�f�[�^��4byte�ȉ������������
                            // �^�C���A�E�g�J�E���^��UP
                            Thread.Sleep(50);
                            recv_timeout++;
                            if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                                break;  //Timeout
                        }
                    }
                    else
                    {// ���r���[�ȃf�[�^���L��B����Ȃ��u���b�N����M
                        ava_size = sock.Available;
                        if (ava_size > (blk_size - rsp_size))
                        {// ����Ȃ��u���b�N�T�C�Y���̃f�[�^���S�ė��Ă���
                            // �ǂݎ��\�f�[�^���L��̂Ń^�C���A�E�g�J�E���^�����Z�b�g
                            recv_timeout = 0;
                            while (blk_size > rsp_size)
                                rsp_size += sock.Receive(blk, 4 + rsp_size, blk_size - rsp_size, SocketFlags.None);
                            // �N���C�A���g�ɑ��M
                            Status.Client.StreamWrite(blk);
                            rsp_size = 0;   // ���r���[�f�[�^�Ȃ�
                            blk = null;     // GC�𑁂߂邽�߂̖����Inull�ݒ�
                        }
                        else if (ava_size > 0)
                        {// �f�[�^�͗��Ă��邪�A�P�u���b�N�����`������ɂ͎���Ȃ�
                            // �ǂݎ��\�f�[�^���L��̂Ń^�C���A�E�g�J�E���^�����Z�b�g
                            recv_timeout = 0;
                            // �f�[�^��M�B�P�񂾂��B
                            rsp_size += sock.Receive(blk, 4 + rsp_size, ava_size, SocketFlags.None);
                        }
                        else
                        {
                            // �f�[�^���܂��������Ă��Ȃ�
                            // �^�C���A�E�g�J�E���^��UP
                            Thread.Sleep(50);
                            recv_timeout++;
                            if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                                break;  //Timeout
                        }
                    }
                }
                // while���[�v����
                // �I�����R�̔�����s��
                if (Status.RunStatus == false || (Status.ImportStatus == false && once_status == true))
                {
                    // RunStatus��false�ɂȂ���(GUI����̐ؒf)�A�܂���
                    // once_status��true(�����M��)��ImportStatus��false(�O������̐ؒf�v��)���󂯂��ꍇ

                    Front.AddLogData(1, Status, "�C���|�[�g�ڑ��̐ؒf�v������M���܂���");
                    // ���[�U�w���ɂ��ؒf�ł͐ؒf����炳�Ȃ�
                    //if (File.Exists(Front.Opt.SndDiscFile))
                    //    PlaySound(Front.Opt.SndDiscFile);
                }
                else if (end_flg)
                {
                    Front.AddLogData(1, Status, "�C���|�[�g�悪�z�M���~���܂���");
                    // �G���[�J�E���g�͌v�サ�Ȃ�
                    // IM�ؒf�����ݒ肳��Ă�����ʃX���b�h�ōĐ�����
                    if (File.Exists(Front.Opt.SndDiscFile))
                        PlaySound(Front.Opt.SndDiscFile);
                }
                else // if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                {
                    Front.AddLogData(1, Status, "�C���|�[�g�悩��̎�M���^�C���A�E�g���܂���");
                    Status.ImportError++;
                    // IM�ؒf�����ݒ肳��Ă�����ʃX���b�h�ōĐ�����
                    if (File.Exists(Front.Opt.SndDiscFile))
                        PlaySound(Front.Opt.SndDiscFile);
                }
            }
            catch (SocketException se)
            {
                Front.AddLogData(1, Status, "�C���|�[�g��M�G���[ wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString());
                Status.ImportError++;
                // IM�ؒf�����ݒ肳��Ă�����ʃX���b�h�ōĐ�����
                if (File.Exists(Front.Opt.SndDiscFile))
                    PlaySound(Front.Opt.SndDiscFile);
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "�C���|�[�g��M�G���[(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")");
                Status.ImportError++;
                // IM�ؒf�����ݒ肳��Ă�����ʃX���b�h�ōĐ�����
                if (File.Exists(Front.Opt.SndDiscFile))
                    PlaySound(Front.Opt.SndDiscFile);
            }
            finally
            {
                Front.AddLogData(0, Status, "�G�N�X�|�[�g�ւ̏����o�����I�����܂�");
                Status.ImportStatus = false;
            }
        }
        #endregion

        /// <summary>
        /// �ш摪��^�X�N
        /// </summary>
        private void BandTask()
        {
            while (Status.RunStatus)
            {
                Status.CurrentDLSpeed = Status.RecvByte * 8 / 3000;
                Status.TotalDLSize += (ulong)Status.RecvByte;
                Status.TotalUPSize += (ulong)(Status.RecvByte * Status.Client.Count);
                Front.TotalDL += (ulong)Status.RecvByte;
                Front.TotalUP += (ulong)(Status.RecvByte * Status.Client.Count);
                Status.RecvByte = 0;

                // ���σr�b�g���[�g�̌v�Z
                // DLSpeed��0��������A���σr�b�g���[�g�ɔ��f�����Ȃ�
                if (Status.CurrentDLSpeed != 0)
                {
                    if (Status.TrafficCount == 0)
                    {
                        Status.AverageDLSpeed = Status.CurrentDLSpeed;
                        Status.TrafficCount++;
                    }
                    else if (Status.TrafficCount < 10)    ///10��=30�b����
                    {
                        int _totalDL = Status.AverageDLSpeed * Status.TrafficCount;
                        _totalDL += Status.CurrentDLSpeed;
                        Status.TrafficCount++;
                        Status.AverageDLSpeed = _totalDL / Status.TrafficCount;
                    }
                    else
                    {
                        int _totalDL = Status.AverageDLSpeed * (Status.TrafficCount - 1);
                        _totalDL += Status.CurrentDLSpeed;
                        Status.AverageDLSpeed = _totalDL / Status.TrafficCount;
                    }
                }
                Thread.Sleep(3000);
            }
        }

        /// <summary>
        /// �w�肳�ꂽwav�t�@�C����񓯊��ōĐ�����
        /// </summary>
        /// <param name="_wavfile"></param>
        private void PlaySound(string _wavfile)
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
            }
            player = new System.Media.SoundPlayer(_wavfile);
            player.Play();
        }
    }
}
