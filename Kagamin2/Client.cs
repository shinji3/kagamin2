//#define BETA15TEST

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Forms;

namespace Kagamin2
{
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
                        catch (Exception)
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
        /// ClientItem��̃N���C�A���g�ڑ����ԍX�V
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
                        if(cd.Id == Status.Gui.ClientItem[cnt].Text)
                            Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_TM_IDX].Text = cd.ClientTimeString;
                    }
                }
            }
        }
        /// <summary>
        /// KickItem��̃N���C�A���g�L�b�N���ԍX�V
        /// </summary>
        public void UpdateKickTime()
        {
            lock (Front.KickList)
            lock (Status.Gui.KickItem)
            {
                foreach (ListViewItem _item in Status.Gui.KickItem)
                {
                    // Kick��ԁ��������Ԃ��X�V
                    string[] str = Front.KickList[_item.Text].Split(',');
                    DateTime _now_tim = DateTime.Now;
                    DateTime _end_tim = DateTime.Parse(str[0]);
                    int con_cnt = int.Parse(str[1]);
                    if (con_cnt == 0 && _end_tim > _now_tim)
                    {
                        TimeSpan _duration = _end_tim - _now_tim;
                        _item.SubItems[1].Text = "�K����/�����܂�" + (long)_duration.TotalSeconds + "�b";    // clmKickViewState
                        _item.SubItems[0].ForeColor = Color.Red;
                    }
                    else if (con_cnt < 0)
                    {
                        _item.SubItems[1].Text = "�K����/������";   // clmKickViewState
                        _item.SubItems[0].ForeColor = Color.Red;
                    }
                    else
                    {
                        _item.SubItems[1].Text = "������";                                                  // clmKickViewState
                        _item.SubItems[0].ForeColor = Color.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// �q���ڑ��̒����烉���_����1��Ԃ�
        /// </summary>
        /// <returns></returns>
        public string GetKagamiList()
        {
            List<string> list = new List<string>();
            lock (ClientList)
            {
                foreach (ClientData cd in ClientList)
                {
                    if (cd.KagamiOK)
                    {
                        // "http://host��:port�ԍ�" �ŋl�߂�
                        string kagami = "http://" + cd.Ip + ":" + cd.KagamiPort.ToString();
                        list.Add(kagami);
                    }
                }
            }
            if (list.Count == 0)
                return "";

            Random ran = new Random();
            return list[ran.Next(list.Count)];
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
        /// �N���C�A���gUserAgent
        /// </summary>
        public string UserAgent;
        /// <summary>
        /// �q���|�[�g�ԍ�
        /// </summary>
        public int KagamiPort;
        /// <summary>
        /// �q���ڑ��m�FOK�t���O
        /// </summary>
        public bool KagamiOK;
        /// <summary>
        /// �X�g���[�����M�L���[
        /// </summary>
        public Queue<byte[]> StreamQueue = new Queue<byte[]>((int)Front.Sock.SockSendQueueSize);
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

        #endregion

        #region �R���X�g���N�^
        public ClientData(Status _status, string _id, Socket _sock, string _ua)
        {
            Status = _status;
            Id = _id;
            Sock = _sock;
            UserAgent = _ua;
            ClientStartTime = DateTime.Now;
            Ip = ((IPEndPoint)_sock.RemoteEndPoint).Address.ToString();
            if (UserAgent.IndexOf("/Port=") >= 0)
            {
                string port = UserAgent.Substring(UserAgent.IndexOf("/Port=") + 6);
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
            KagamiOK = false;
        }
        #endregion

        /// <summary>
        /// �N���C�A���g�փf�[�^���M
        /// </summary>
        public void Send()
        {
            ArrayList _red = new ArrayList();
            ArrayList _wrt = new ArrayList();
            ArrayList _err = new ArrayList();

            #region �q���ڑ��`�F�b�N�̃X���b�h����
            // ���|�[�g���킩��Ȃ�ꗥ�`�F�b�N�X���b�h����
            //if (Status.Client.Count >= Status.Connection)
            //if (Front.Opt.PriKagamin == true && KagamiPort != 0)
            if (KagamiPort != 0)
            {
#if !DEBUG
                // ���[�J���A�h���X�̎q���͏��O����
                if (Ip == "::1" ||                // IPv6 LoopBack
                    Ip.StartsWith("10.") ||       // ClassA
                    Ip.StartsWith("172.16.") ||   // ClassB
                    Ip.StartsWith("192.168.") ||  // ClassC
                    Ip.StartsWith("127."))        // LoopBack
                {
                    // ���O
                }
                else
                {
#endif
                Thread check = new Thread(new ThreadStart(KagaminCheck));
                check.Name = "KagaminCheck";
                check.Start();
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
                        Thread check = new Thread(new ThreadStart(SameIPCheck));
                        check.Name = "SameIPCheck";
                        check.Start();
                    }
                }
            }
            #endregion

            Front.AddLogData(0, Status, "[" + Id + "] �N���C�A���g�^�X�N���J�n���܂�");
            Front.AddLogData(0, Status, "[" + Id + "] User-Agent: " + UserAgent);
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
                    if (Sock == null || !Sock.Connected)
                    {
                        // �O���X���b�h���狭���ؒf����Ă�����
                        throw new Exception();
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

                    byte[] buf = StreamQueue.Dequeue();
                    if (buf != null)
                        Sock.Send(buf);
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
            IsAlive = false;
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
        private void KagaminCheck()
        {
            Socket sock_chk = new Socket(Socket.OSSupportsIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 10�b�҂��Ă���`�F�b�N�J�n
            Thread.Sleep(10000);

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

                sock_chk.SendTimeout = (int)Front.Sock.SockConnTimeout;       // Import�ڑ� �w�b�_�擾�v�����M�̃^�C���A�E�g�l
                sock_chk.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    // Import�ڑ� �w�b�_�擾������M�̃^�C���A�E�g�l

                // �ڑ�
                sock_chk.Connect(ephost);

                // �q���ɋU���������N�G�X�g�𑗐M
                // �N���C�A���g�ɋU�������w�b�_�𑗐M
                string reqMsg = "GET / HTTP/1.1\r\n" +
                    "Accept: */*\r\n" +
                    "User-Agent: " + Front.UserAgent + "/ConnCheck\r\n" +
                    "Host: " + ((IPEndPoint)sock_chk.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)sock_chk.RemoteEndPoint).Port + "\r\n" +
                    "Pragma: no-cache\r\n" +
                    "Content-Type: application/x-mms-framed\r\n\r\n";

                Encoding enc = Encoding.ASCII; // "euc-jp"
                byte[] reqBytes = enc.GetBytes(reqMsg);
                sock_chk.Send(reqBytes, reqBytes.Length, SocketFlags.None);

                // �����w�b�_��M
                byte[] tmp = new byte[9];
                byte[] tmp2 = new byte[3];
                sock_chk.Receive(tmp);  // "HTTP/1.x " �܂Ŏ�M
                sock_chk.Receive(tmp2); // HTTP StatusCode��3byte��M
                int http_status = int.Parse(Encoding.ASCII.GetString(tmp2));
                // StatusCode:200�ȊO��NG
                // 10�b��Ƀ`�F�b�N���Ĕ��l�͕��ʂ��肦�Ȃ��ł��傤�B�B
                if (http_status != 200)
                    throw new Exception();
                string chkMsg = "Content-Type: application/x-mms-framed";
                byte[] chkBytes = enc.GetBytes(chkMsg);
                byte[] ack = new byte[1];
                int i = 0;
                int count = 0;
                while (true)
                {
                    sock_chk.Receive(ack);
                    count++;
                    if (count > 5000)
                        throw new Exception();
                    if (ack[0].Equals(chkBytes[i])) i++; else i = 0;
                    if (i >= chkBytes.Length)
                        break;
                }
                // Contents-Type�̃`�F�b�N��OK�Ȃ̂ő��v�ł��傤�B
                // �R�l�N�V������؂炸�ɏI��
                sock_chk.Close();
                Front.AddLogData(0, Status, "�q���ڑ��`�F�b�NOK [" + Ip + ":" + KagamiPort + "]");
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
                    Front.AddLogData(1, Status, "�q���ڑ��`�F�b�NNG [[" + Id + "] " + Ip + "]");
                }
                sock_chk.Close();
            }
        }

        /// <summary>
        /// ����IP�ڑ������Ď�����
        /// �������𒴂��Ă�����ؒf����
        /// </summary>
        private void SameIPCheck()
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
