//#define BETA15TEST

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace Kagamin2
{
    /*
    public class ClientEvent : EventArgs
    {
        public Status Status;
        public int Id;
        public Socket Sock;
        public string UserAgent;

        /// <summary>
        /// 0=Add
        /// 1=Delete
        /// </summary>
        public int EventType;
    }
    */

    /// <summary>
    /// �N���C�A���g�S�̂��Ǘ�����N���X
    /// </summary>
    public class Client
    {
        #region �����o�ϐ�
        /// <summary>
        /// ���X�e�[�^�X
        /// </summary>
        private Status Status;
        /// <summary>
        /// �N���C�A���g���X�g
        /// </summary>
        private List<ClientData> ClientList = new List<ClientData>();
        /// <summary>
        /// �N���C�A���g�Ǘ�ID
        /// </summary>
        private List<int> ClientID = new List<int>();
        /// <summary>
        /// �ڑ����N���C�A���g��
        /// </summary>
        public int Count
        {
            get { return ClientList.Count; }
        }
        #endregion

        #region �R���X�g���N�^
        public Client(Status _status)
        {
            Status = _status;
        }
        #endregion

        public void TransChat(KagamiLink kl,byte[] chat)
        {
            lock (ClientList)
            {
                foreach (var _client in ClientList)
                {
                    if (_client.KLink != kl)
                        _client.KLink.Send(chat);

                }
            }
        }
        /// <summary>
        /// �N���C�A���g�ւ̃f�[�^���M
        /// </summary>
        /// <param name="_status"></param>
        /// <param name="_sock"></param>
        /// <param name="_ua"></param>
        public void Send(Status _status, Socket _sock, string _ua)
        {
            //�N���C�A���g�ɊǗ�ID�����蓖�Ă�
            int cnt;
            ClientData cd;
            try
            {
                // ����ID��d�ߑ���h������lock
                lock (ClientID)
                {
                    //��ID����
                    for (cnt = 0; cnt < ClientID.Count; cnt++)
                        if (cnt != ClientID[cnt])
                            break;
                    ClientID.Insert(cnt, cnt);
                }
            }
            catch
            { return; }

            //�N���C�A���g���M�N���X����
            try
            {
                //�N���C�A���g�փf�[�^���M
                cd = new ClientData(_status, cnt.ToString("D3"), _sock, _ua);
            }
            catch
            {
                // �N���C�A���gID���
                ClientID.Remove(cnt);
                return;
            }

            try
            {
                // �����Ǘ��pClientList�ɒǉ�
                lock (ClientList)
                    ClientList.Add(cd);
                // ClientItem�Ƀf�[�^�ǉ��{GUI�X�V
                Status.AddClient(cd);
                // �N���C�A���g�֑��M�J�n
                cd.Send();
            }
            catch { }
            finally
            {
                // ClientItem����폜�{GUI�X�V
                Status.RemoveClient(cnt.ToString("D3"));
                // �����Ǘ��pClientList����폜
                lock (ClientList)
                    ClientList.Remove(cd);
                // �ϋɓI���������
                cd = null;
                // �N���C�A���gID���
                ClientID.Remove(cnt);
                // �O�̂���
                _sock.Close();
            }
        }

        /// <summary>
        /// �N���C�A���g�����ؒf
        /// </summary>
        /// <param name="_id"></param>
        public void Disc(string _id)
        {
            lock (ClientList)
                foreach (ClientData cd in ClientList)
                    if (_id == cd.Id)
                        cd.Disc();
        }

        /// <summary>
        /// �X�g���[�����̏�������
        /// </summary>
        /// <param name="_byte"></param>
        public void StreamWrite(byte[] _byte)
        {
            if (_byte.Length <= 0)
                return;
            Status.RecvByte += _byte.Length;
            try
            {
                lock (ClientList)
                {
                    foreach (ClientData cd in ClientList)
                    {
                        try
                        {
                            if (cd.IsAlive)
                                cd.StreamQueue.Enqueue(_byte);
                        }
                        catch
                        {
                            cd.Disc();
                            cd.StreamQueue.Clear();
                            Front.AddLogData(1, Status, "�X�g���[����������NG [" + cd.Ip + "] / Q:" + cd.StreamQueue.Count);
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// ClientItem��̏�ԍX�V
        /// </summary>
        public void UpdateClientTime()
        {

            lock (Status.Gui.ClientItem)
                lock (ClientList)
                {
                    foreach (ClientData cd in ClientList)
                    {
                        for (int cnt = 0; cnt < Status.Gui.ClientItem.Count; cnt++)
                        {
                            if (cd.Id == Status.Gui.ClientItem[cnt].Text)
                            {
                                Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_TM_IDX].Text = cd.ClientTimeString;
                                Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_BU_IDX].Text = ((Front.Sock.SockSendQueueSize - (double)cd.StreamQueue.Count) / Front.Sock.SockSendQueueSize * 100).ToString("F1") + "%";
                                Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_KI_IDX].Text = cd.ConnInfo;
                                if (Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_HO_IDX].Text == "Resolving")
                                    Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_HO_IDX].Text = cd.Host;
                            }
                        }
                    }
                }
        }
        /// <summary>
        /// KickItem��̃N���C�A���g�L�b�N���ԍX�V
        /// </summary>
        public void UpdateKickTime()
        {
            lock (Status.Kick)
            lock (Status.Gui.KickItem)
            {
                foreach (System.Windows.Forms.ListViewItem _item in Status.Gui.KickItem)
                {
                    // Kick��ԁ��������Ԃ��X�V

                    if (!Status.Kick.ContainsKey(_item.Text))
                    {//GUI�ɂ͓o�^����Ă��邪�����Ǘ��ɂ͓o�^����Ă��Ȃ��ꍇ
                     //�ʏ�͂��Ȃ�
                        continue;
                    }

                    
                    //�ڑ����s��
                    _item.SubItems[2].Text = Status.Kick[_item.Text].Cnt_out.ToString();
                    
                    //���
                    if (Status.Kick[_item.Text].DenyEndTime == -1)
                    {//�������̏ꍇ
                        _item.SubItems[1].Text = "�K����/������";    // clmKickViewState
                        _item.SubItems[0].ForeColor = System.Drawing.Color.Red;
                    }
                    else if (Status.Kick[_item.Text].KickFlag &&
                        Status.Kick[_item.Text].DenyTime.AddSeconds(Status.Kick[_item.Text].DenyEndTime) > DateTime.Now)
                    {//�L�b�N���̏ꍇ
                        TimeSpan _duration = Status.Kick[_item.Text].DenyTime.AddSeconds(Status.Kick[_item.Text].DenyEndTime) - DateTime.Now;
                        _item.SubItems[1].Text = "�K����/�����܂�" + (long)_duration.TotalSeconds + "�b";    // clmKickViewState
                        _item.SubItems[0].ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {//�L�b�N���łȂ��ꍇ
#if DEBUG
                        Front.AddKickLog(Status, "UpdateKickTime1");
#endif
                        Status.Kick[_item.Text].KickFlag = false;
                        _item.SubItems[1].Text = "������";                                                  // clmKickViewState
                        _item.SubItems[0].ForeColor = System.Drawing.Color.Empty;
                    }
                }
            }
        }
    }

    /// <summary>
    /// �P�N���C�A���g���̃f�[�^���Ǘ�����N���X
    /// </summary>
    public class ClientData
    {
        #region �����o�ϐ�
        /// <summary>
        /// �N���C�A���gID
        /// </summary>
        public string Id;
        /// <summary>
        /// �N���C�A���gIP
        /// </summary>
        public string Ip;
        /// <summary>
        /// �q���|�[�g�ԍ�
        /// </summary>
        public int KagamiPort;
        /// <summary>
        /// �q�������N���x��
        /// </summary>
        public int KagamiLink;
        /// <summary>
        /// �����[�g�z�X�g
        /// </summary>
        public string Host;
        /// <summary>
        /// �N���C�A���gUserAgent
        /// </summary>
        public string UserAgent;
        /// <summary>
        /// �X�g���[�����M�L���[
        /// </summary>
        public Queue<byte[]> StreamQueue = new Queue<byte[]>((int)Front.Sock.SockSendQueueSize);
        /// <summary>
        /// �q���̐l�����
        /// </summary>
        public string ConnInfo;
        /// <summary>
        /// ���ԃ����N�Ǘ��N���X
        /// </summary>
        public KagamiLink KLink;
        /// <summary>
        /// �N���C�A���g�ڑ�����
        /// </summary>
        public string ClientTimeString
        {
            get
            {
                TimeSpan _duration = DateTime.Now - ClientStartTime;
                return String.Format("{0:D2}:{1:D2}:{2:D2}", _duration.Hours, _duration.Minutes, _duration.Seconds);
            }
        }
        /// <summary>
        /// �N���C�A���g�����t���O
        /// </summary>
        public bool IsAlive = false;
        /// <summary>
        /// �N���C�A���g�Ǘ����X�e�[�^�X
        /// </summary>
        private Status Status;
        /// <summary>
        /// �N���C�A���g�\�P�b�g
        /// </summary>
        private Socket Sock;
        /// <summary>
        /// �N���C�A���g�ڑ�����
        /// </summary>
        private DateTime ClientStartTime;
        /// <summary>
        /// ���q���`�F�b�N����
        /// </summary>
        private DateTime? KagamiCheck = null;
        /// <summary>
        /// �q���ڑ��m�FOK�t���O
        /// </summary>
        public bool KagamiOK;
        /// <summary>
        /// �q���`�F�b�N���t���O
        /// </summary>
        private bool KagamiCheking = false;
        #endregion

        #region �R���X�g���N�^
        public ClientData(Status _status, string _id, Socket _sock, string _ua)
        {
            ConnInfo = "";
            Status = _status;
            Id = _id;
            Sock = _sock;
            UserAgent = _ua;
            ClientStartTime = DateTime.Now;
            Ip = ((IPEndPoint)_sock.RemoteEndPoint).Address.ToString();

            #region ���|�[�g���o
            if (UserAgent.IndexOf("/Port=") >= 0)
            {
                //NSPlayer/11.0.5721.5145 Kagamin2/2.1.4/Port=8888/Link=5
                string port = UserAgent.Substring(UserAgent.IndexOf("/Port=") + 6, UserAgent.Contains("/Link=") ?
                    UserAgent.LastIndexOf('/') - (UserAgent.IndexOf("/Port=") + 6) : UserAgent.Length - (UserAgent.IndexOf("/Port=") + 6));
                if (port != "")
                {
                    KagamiPort = int.Parse(port);
                    if (KagamiPort > 0xffff)
                        KagamiPort = 0;
                }
                else
                {
                    KagamiPort = 0;
                }
            }
            else
            {
                KagamiPort = 0;
            }
            #endregion
            #region �������N���o
#if DEBUG
            if (UserAgent.Contains("/Link="))
            {
                string link = UserAgent.Substring(UserAgent.IndexOf("/Link=") + 6);

                if (link != "")
                {
                    KagamiLink = int.Parse(link);
                }
                else
                {
                    KagamiLink = 0;
                }
            }
            else
            {
                KagamiLink = 0;
            }
#endif
            #endregion

            KagamiOK = false;

            Dns.BeginGetHostEntry(Ip, new AsyncCallback(GetHost), null);
            Host = "Resolving";

            if (KagamiPort > 0 && KagamiLink > 0)
            {
                this.KLink = new KagamiLink(_status, this);

            }
            
        }

        private void GetHost(IAsyncResult ar)
        {
            try
            {
                Host = Dns.EndGetHostEntry(ar).HostName;
            }
            catch
            {
                Host = "Resolve Error";
                return;
            }

            foreach (string _host in Front.Acl.DenyHost)
            {
                if (string.IsNullOrEmpty(_host))
                    continue;
                try
                {
                    System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(_host);
                    if (re.IsMatch(Host))
                    {
                        Front.AddLogData(0, Status, "[" + Id + "] �֎~�z�X�g�̂��߃N���C�A���g�����ۂ��܂� [" + Host + "/Match:" + _host + "]");
                        Sock.Close();
                        return;
                    }
                }
                catch
                {
                }

                if (Host.Contains(_host))
                {
                    Front.AddLogData(0, Status, "[" + Id + "] �֎~�z�X�g�̂��߃N���C�A���g�����ۂ��܂� [" + Host + "/Match:" + _host + "]");
                    Sock.Close();
                    return;
                }

            }
        }
        #endregion

        /// <summary>
        /// �N���C�A���g�փf�[�^���M
        /// </summary>
        public void Send()
        {
            System.Collections.ArrayList _red = new System.Collections.ArrayList();
            System.Collections.ArrayList _wrt = new System.Collections.ArrayList();
            System.Collections.ArrayList _err = new System.Collections.ArrayList();

            #region �q���ڑ��`�F�b�N�̃X���b�h����
            // ���|�[�g���킩��Ȃ�ꗥ�`�F�b�N�X���b�h����
            //if (Status.Client.Count >= Status.Connection)
            //if (Front.Opt.PriKagamin == true && KagamiPort != 0)
            if (KagamiPort != 0)
            {
#if !DEBUG
                
                // ���[�J���A�h���X�̎q���͏��O����
                if (Ip.StartsWith("10.") ||       // ClassA
                    Ip.StartsWith("192.168.") ||  // ClassC
                    Ip.StartsWith("127."))        // LoopBack
                {
                    // ���O
                }
                else
                {
#endif
                ThreadPool.QueueUserWorkItem(new WaitCallback(KagaminCheck));
                /*
                Thread check = new Thread(new ThreadStart(KagaminCheck));
                check.Name = "KagaminCheck";
                check.Start();
                */
#if !DEBUG
                }
#endif
            }
            #endregion
            #region ����IP�ڑ��`�F�b�N�̃X���b�h����
            // ����IP�ڑ��͒ʏ�ł�(���ؒf���邪)�N���肤��
            // �Ȃ̂ŕʃX���b�h������Đݒ�b��Ƀ`�F�b�N���ĕ����ڑ������܂܂Ȃ�ؒf���邱�Ƃɂ���
            if (Front.Acl.LimitSameClient > 0)
            {
                lock (Status.Gui.ClientItem)
                {
                    int num = 0;
                    for (int cnt = 0; cnt < Status.Gui.ClientItem.Count; cnt++)
                    {
                        if (Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_IP_IDX].Text == Ip)
                            num++;
                    }
                    // �ڑ������w��l�𒴂��Ă���̂Ń`�F�b�N�X���b�h����
                    if (num > Front.Acl.LimitSameClient)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(SameIPCheck));
                        /*
                        Thread check = new Thread(new ThreadStart(SameIPCheck));
                        check.Name = "SameIPCheck";
                        check.Start();
                        */
                    }
                }
            }
            #endregion

            Front.AddLogData(0, Status, "[" + Id + "] �N���C�A���g�^�X�N���J�n���܂�");
            Front.AddLogData(0, Status, "[" + Id + "] User-Agent: " + UserAgent);
            //Front.AddLogData(0, Status, "[" + Id + "] Remote-Host: " + Host);
            IsAlive = true;
            while (true)
            {
                try
                {
                    if (Status.RunStatus == false || Status.ImportStatus == false)
                    {
                        // �C���|�[�g�����؂ꂽ��
                        // ���O�\���͖ʓ|�Ȃ̂ōs��Ȃ�
                        break;
                    }

                    _red.Clear(); _red.Add(Sock);
                    _wrt.Clear(); _wrt.Add(Sock);
                    _err.Clear(); _err.Add(Sock);
                    Socket.Select(_red, null, _err, 30);

                    // ��M�f�[�^����
                    // �X�g���[�~���O�J�n��̏ꍇ�A��M�f�[�^���聁�N���C�A���g���Ȑؒf������
                    if (_red.Count > 0)
                    {
                        // �ꉞ�T�C�Y�̂O�`�F�b�N
                        if (Sock.Available == 0)
                        {
                            // ����ؒf
                            Front.AddLogData(0, Status, "[" + Id + "] �R�l�N�V�����͐ؒf����܂���(����I��) [" + Ip + "]");
                            break;
                        }
                    }

                    // �\�P�b�g�G���[�̓��b�Z�[�W�����o���Ė���
                    if (_err.Count > 0)
                        Front.AddLogData(1, Status, "[" + Id + "] �\�P�b�g�G���[���� [" + Ip + "]");

                    // ���M�f�[�^���������30ms wait����continue
                    if (StreamQueue.Count == 0)
                    {
                        Thread.Sleep(30);
                        continue;
                    }

                    // ���M�f�[�^�L��΂����A�o�b�t�@���`�F�b�N
                    if (Front.Sock.SockSendQueueSize < StreamQueue.Count)
                    {
                        Front.AddLogData(1, Status, "[" + Id + "] �������݃o�b�t�@����ꂽ���ߐؒf���܂� [" + Ip + "] Q:" + Front.Sock.SockSendQueueSize);
                        Status.ExportError++;
                        break;
                    }
                    if (Status.Kick.ContainsKey(Ip))
                    {
                        if (Status.Kick[Ip].KickFlag)
                        {
                            Front.AddLogData(0, Status, "Kick�ΏۂɂȂ������ߋ����ؒf���܂��B [" + Ip + "]");
                            break;
                        }
                    }
                    byte[] buf = StreamQueue.Dequeue();
                    if (buf != null)
                        Sock.Send(buf);

                    if (KagamiPort != 0)
                    {
                        if (KagamiCheck.HasValue)
                            if (!KagamiCheking)
                                if (KagamiCheck < DateTime.Now)
                                    ThreadPool.QueueUserWorkItem(KagaminCheck);

                    }

                }
                catch (SocketException se)
                {
                    //�����ؒf�Ȃ�G���[���b�Z�[�W��ς���
                    if (se.SocketErrorCode == SocketError.NotSocket)
                    {
                        Front.AddLogData(1, Status, "[" + Id + "] �R�l�N�V�����������ؒf���܂��� [" + Ip + "]");
                    }
                    else
                    {
                        Front.AddLogData(1, Status, "[" + Id + "] �R�l�N�V�����͐ؒf����܂��� [" + Ip + "] wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString());
                        Status.ExportError++;
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (Sock == null || !Sock.Connected)
                    {
                        //���X���b�h����ؒf���ꂽ�Ƃ�
                        //�����G���[�ɂ͂����ɐؒf
                        Front.AddLogData(1, Status, "[" + Id + "] �R�l�N�V�����������ؒf���܂��� [" + Ip + "]");
                    }
                    else
                    {
                        // ����ȊO�̃G���[
                        Front.AddLogData(1, Status, "[" + Id + "] �N���C�A���g���M�G���[(�����G���[:" + e.Message + "/Type:" + e.GetType().ToString() + "/Trace:" + e.StackTrace + ")");
                        Status.ExportError++;
                    }
                    break;
                }
            }

            Sock.Close();
            Front.AddLogData(0, Status, "[" + Id + "] �N���C�A���g�^�X�N���I�����܂�");
        }

        /// <summary>
        /// �N���C�A���g�����ؒf
        /// </summary>
        public void Disc()
        {
            try
            {
                IsAlive = false;
                if (Sock != null)
                {
                    Sock.Close();
                    //Sock.Disconnect(false);
                }
            }
            catch { }
            finally { Sock = null; }
        }

        /// <summary>
        /// �q���ڑ����`�F�b�N����X���b�h
        /// </summary>
        private void KagaminCheck(object sender)
        {

            KagamiCheking = true;
            Socket sock_chk = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bool _busy = false;
            // 10�b�҂��Ă���`�F�b�N�J�n
            Thread.Sleep(10000);

            if (Sock == null)
                return;
            // 10�b�ȓ��ɐڑ����؂�Ă����ꍇ�I��
            if (Sock.Connected == false)
            {
                return;
            }

            try
            {
                Front.AddLogData(0, Status, "�q���ڑ��`�F�b�N���J�n���܂� [" + Ip + ":" + KagamiPort + "]");

                // �q���|�[�g�`�F�b�N�pSocket�̍쐬
                IPAddress hostadd = Dns.GetHostAddresses(Ip)[0];
                IPEndPoint ephost = new IPEndPoint(hostadd, KagamiPort);

                sock_chk.SendTimeout = (int)Front.Sock.SockConnTimeout * 3;       // Import�ڑ� �w�b�_�擾�v�����M�̃^�C���A�E�g�l
                sock_chk.ReceiveTimeout = (int)Front.Sock.SockConnTimeout * 3;    // Import�ڑ� �w�b�_�擾������M�̃^�C���A�E�g�l

                // �ڑ�
                sock_chk.Connect(ephost);

                // �q���ɋU���������N�G�X�g�𑗐M
                // �N���C�A���g�ɋU�������w�b�_�𑗐M
                string reqMsg = "GET / HTTP/1.1\r\n" +
                    "Accept: */*\r\n" +
                    "User-Agent: " + Front.UserAgent + "/WebABC.ConnCheck \r\n" +
                    "Host: " + ((IPEndPoint)sock_chk.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)sock_chk.RemoteEndPoint).Port + "\r\n" +
                    "Pragma: no-cache\r\n" +
                    "Content-Type: application/x-mms-framed\r\n\r\n";

                System.Text.Encoding enc = System.Text.Encoding.ASCII; // "euc-jp"
                byte[] reqBytes = enc.GetBytes(reqMsg);
                sock_chk.Send(reqBytes, reqBytes.Length, System.Net.Sockets.SocketFlags.None);

                // �����w�b�_��M
                byte[] tmp = new byte[9];
                byte[] tmp2 = new byte[3];
                sock_chk.Receive(tmp);  // "HTTP/1.x " �܂Ŏ�M
                sock_chk.Receive(tmp2); // HTTP StatusCode��3byte��M
                int http_status = int.Parse(System.Text.Encoding.ASCII.GetString(tmp2));

                // StatusCode:200�ȊO��NG
                // 10�b��Ƀ`�F�b�N���Ĕ��l�͕��ʂ��肦�Ȃ��ł��傤�B�B
                //���񌟍�����KagamiCheck��null�B����ȊO�̓r�W�[�ł�����Ȃ�
                if (http_status != 200 && !KagamiCheck.HasValue)
                    throw new Exception();
                else if (http_status != 200)
                    _busy = true;

                string chkMsg = "Content-Type: application/x-mms-frame";
                byte[] chkBytes = enc.GetBytes(chkMsg);
                byte[] ack = new byte[1];
                byte[] ack_log = new byte[50000];
                byte[] ack_end = { 0x0a, 0x0a }; // '\n', '\n'

                int j = 0;
                int count = 0;

                while (true)
                {
                    sock_chk.Receive(ack);
                    ack_log[count] = ack[0];
                    count++;

                    // HTTP�����w�b�_�̏I��������
                    if (ack[0].Equals(0x0d)) continue;  // '\r'
                    if (ack[0].Equals(ack_end[j])) j++; else j = 0;

                    if (count > 5000)
                        throw new Exception();
                    if (j >= ack_end.Length)
                        break;

                }


                string str = enc.GetString(ack_log);
                if (!str.Contains(chkMsg) && !KagamiCheck.HasValue)
                    throw new Exception();

                if (str.Contains("coninfo="))
                    ConnInfo = str.Substring(str.IndexOf("coninfo=") + 8, str.IndexOf("\r\n", str.IndexOf("coninfo=") + 8) - str.IndexOf("coninfo=") - 8).TrimEnd();
                else if (!string.IsNullOrEmpty(ConnInfo) && _busy)
                {//�r�W�[�̏ꍇ�ɐl����񂪑����Ă��Ȃ��ꍇ�O�̏�񂩂�␳����(kagami/0.76+)
                    try
                    {
                        ConnInfo = ConnInfo.Split('/')[1].Split('+')[0] + "/" + ConnInfo.Split('/')[1];
                    }
                    catch
                    {
                        ConnInfo = "Busy";
                    }
                }


                // Contents-Type�̃`�F�b�N��OK�Ȃ̂ő��v�ł��傤�B
                // �R�l�N�V������؂炸�ɏI��
                sock_chk.Close();

                //����`�F�b�N���ԁB�\���ł��Ȃ��悤����ĂɃ`�F�b�N�ɂȂ�Ȃ��悤�ɂ�����x�����_����
                Random rd = new Random();
#if !DEBUG
                KagamiCheck = DateTime.Now.AddSeconds(rd.Next(60 * 3, 60 * 10));//3min�`10min
#else
                KagamiCheck = DateTime.Now.AddSeconds(rd.Next(30, 90));//30sec�`90sec
#endif


                if (string.IsNullOrEmpty(ConnInfo))
                    Front.AddLogData(0, Status, "�q���ڑ��`�F�b�NOK/���`�F�b�N" + ((TimeSpan)(KagamiCheck - DateTime.Now)).TotalSeconds.ToString("F0") + "�b�� [" + Ip + ":" + KagamiPort + "]");
                else
                    Front.AddLogData(0, Status, "�q���ڑ��`�F�b�NOK/���`�F�b�N" + ((TimeSpan)(KagamiCheck - DateTime.Now)).TotalSeconds.ToString("F0") + "�b�� [" + Ip + ":" + KagamiPort + " Con:" + ConnInfo + "]");
                KagamiOK = true;    // �q���]���Ώۂɂ���


            }
            catch
            {
                // �����݂�D��ON�̏ꍇ NG�Ȃ�ڑ���؂�
                // �����݂�D��OFF�̏ꍇ NG�ł��؂�Ȃ����A�q���]���͍s��Ȃ�
                if (Front.Opt.PriKagamin)
                {
                    Sock.Close();
                    Front.AddLogData(1, Status, "�q���ڑ��`�F�b�NNG�̂��ߐؒf���܂� [[" + Id + "] " + Ip + "]");
                }
                else
                {
                    //����`�F�b�N���ԁB�\���ł��Ȃ��悤����ĂɃ`�F�b�N�ɂȂ�Ȃ��悤�ɂ�����x�����_����
                    //NG�̏ꍇ�͎��`�F�b�N�͒��߂�
                    Random rd = new Random();
#if !DEBUG
                KagamiCheck = DateTime.Now.AddSeconds(rd.Next(60 * 10, 60 * 20));//10min�`20min
#else
                    KagamiCheck = DateTime.Now.AddSeconds(rd.Next(30, 90));//30sec�`90sec
#endif

                    Front.AddLogData(1, Status, "�q���ڑ��`�F�b�NNG/���`�F�b�N" + ((TimeSpan)(KagamiCheck - DateTime.Now)).TotalSeconds.ToString("F0") + "�b�� [[" + Id + "] " + Ip + "]");
                    KagamiOK = false;
                }
                sock_chk.Close();
            }
            finally
            {
                KagamiCheking = false;
            }
        }

        /// <summary>
        /// ����IP�ڑ������Ď�����
        /// �������𒴂��Ă�����ؒf����
        /// </summary>
        private void SameIPCheck(object sender)
        {
            // 3�b�҂�
            Thread.Sleep(3000);
            // 3�b�ȓ��ɐڑ����؂�Ă�����I��
            if (Sock.Connected == false)
            {
                return;
            }

            int num = 0;
            lock (Status.Gui.ClientItem)
            {
                for (int cnt = 0; cnt < Status.Gui.ClientItem.Count; cnt++)
                {
                    if (Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_IP_IDX].Text == Ip)
                        num++;
                }
            }
            if (num > Front.Acl.LimitSameClient)
            {
                Front.AddLogData(1, Status, "����IP�̓����ڑ����߂̂��ߐؒf���܂� [[" + Id + "] " + Ip + "]");
                Sock.Close();
            }
        }
    }
}
