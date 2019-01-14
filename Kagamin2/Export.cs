using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace Kagamin2
{
    class Export
    {
        #region �����o�ϐ�
        /// <summary>
        /// ���ݒ�f�[�^
        /// </summary>
        private Status Status = null;

        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="_status"></param>
        public Export(Status _status)
        {

            Status = _status;
            Thread th = new Thread(ExportTask);
            th.Name = "ExportTask";
            th.Priority = ThreadPriority.BelowNormal;
            th.Start();
        }

        /// <summary>
        /// �G�N�X�|�[�g�^�X�N
        /// </summary>
        private void ExportTask()
        {

            while (Status.RunStatus)
            {
                if (!Status.ImportStatus)
                {
                    // �C���|�[�g�ڑ����������Ă��Ȃ�
                    Thread.Sleep(1000);
                    continue;
                }

                // Push�z�M�|�[�g�̉���҂�
                while (Status.ListenPort)
                    Thread.Sleep(100);
                // Import�ڑ��m��
                // Export�҂��󂯊J�n
                Front.AddLogData(0, Status, "�G�N�X�|�[�g�^�X�N���J�n���܂�");
                if (Front.Sock.upnp)
                {
                    bool _suc;
#if !DEBUG
                    _suc = JinkSoft.Utility.UPnP.UPnPClient.OpenFirewallPort(Status.MyPort);
#endif
#if DEBUG
                    _suc = JinkSoft.Utility.UPnP.UPnPClient.OpenFirewallPort("192.168.1.14", "192.168.1.4", Status.MyPort);
#endif
                    if (_suc)
                    {
                        Front.AddLogData(1, Status, "UPnP�ɂ��|�[�g���J�����܂����B(Port:" + Status.MyPort.ToString() + ")");
                        lock (Front.UPnPPort)
                        {
                            Front.UPnPPort.Add(Status.MyPort);
                        }
                    }
                    else
                        Front.AddLogData(1, Status, "UPnP�ɂ��|�[�g�J���Ɏ��s���܂����B(Port:" + Status.MyPort.ToString() + ")");
                }
                IPEndPoint _iep = new IPEndPoint(IPAddress.Any, Status.MyPort);
                System.Net.Sockets.TcpListener _listener = new System.Net.Sockets.TcpListener(_iep);
                // Listen�J�n
                try
                {
                    Status.ListenPort = true;
                    _listener.Start();
                }
                catch
                {
                    Front.AddLogData(1, Status, "�G�N�X�|�[�g���邱�Ƃ��o���܂���B�ݒ���m�F���ĉ������B");
                    Status.Disc();
                    Status.RunStatus = false;
                    Status.ListenPort = false;
                    if (Front.Sock.upnp)
                        if (JinkSoft.Utility.UPnP.UPnPClient.CloseFirewallPort(Status.MyPort))
                            Front.UPnPPort.Remove(Status.MyPort);

                    break;
                }

                try
                {
                    while (Status.RunStatus && Status.ImportStatus)
                    {
                        //Accept�҂��̃`�F�b�N
                        if (_listener.Pending() == true)
                        {
                            //Accept���{
                            System.Net.Sockets.Socket _sock = _listener.AcceptSocket();



                            Thread thread = new Thread(ClientTask);
                            thread.Start(_sock);
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                finally
                {
                    try
                    {
                        //���X�i�[�őҋ@���Ă���N���C�A���g�����ׂĐؒf����
                        while (_listener.Pending())
                        {
                            System.Net.Sockets.TcpClient _sock = _listener.AcceptTcpClient();
                            _sock.Close();
                        }
                    }
                    catch { }
                    // Listen��~
                    _listener.Stop();
                    Status.ListenPort = false;
                }
                #region comment
                /*form1�̃`�F�b�N�ɔC���邱�Ƃɂ���
                if ((Front.UPnPPort.Contains(Status.MyPort)))
                {
                    //UPnP�ŕ������u�ڑ����؂��C������̂�
                    //�ق��ɋN�����̃|�[�g������Ƃ��͕��Ȃ��ق��������̂��낤��
                    bool _suc;

                    bool _tmp = false;
                    lock (Front.KagamiList)
                    {
                        foreach (Kagami _k_tmp in Front.KagamiList)
                        {
                            if (_k_tmp.Status.ImportStatus)
                            {
                                _tmp = true;
                                break;
                            }
                        }
                    }
                    if (!_tmp)
                    {
#if !DEBUG
                        _suc = JinkSoft.Utility.UPnP.UPnPClient.CloseFirewallPort(Status.MyPort);
#endif
#if DEBUG
                        _suc = JinkSoft.Utility.UPnP.UPnPClient.CloseFirewallPort("192.168.1.14", "192.168.1.4", Status.MyPort);
#endif

                        if (_suc)
                        {
                            Front.AddLogData(1, Status, "UPnP�ɂ��|�[�g������܂����B(Port:" + Status.MyPort.ToString() + ")");
                            lock (Front.UPnPPort)
                            {
                                Front.UPnPPort.Remove(Status.MyPort);
                            }
                        }
                        else if (!_tmp)
                        {
                            Front.AddLogData(1, Status, "UPnP�ɂ��|�[�g���Ɏ��s���܂����B(Port:" + Status.MyPort.ToString() + ")");
                            lock (Front.UPnPPort)
                            {
                                Front.UPnPPort.Remove(Status.MyPort);
                            }
                        }
                        else
                            Front.AddLogData(1, Status, "�g�p���|�[�g�����邽�߃|�[�g�����s���܂���ł����B(Port:" + Status.MyPort.ToString() + ")");
                   
                  }
                 
                }
                 */

                #endregion

                Front.AddLogData(0, Status, "�G�N�X�|�[�g�^�X�N���I�����܂�");
            }
        }

        /// <summary>
        /// �N���C�A���g���M�^�X�N
        /// </summary>
        /// <param name="obj"></param>
        private void ClientTask(object obj)
        {
            Socket sock = (System.Net.Sockets.Socket)obj;
            string _ua = "";

            bool localcheck = false;
            if (((IPEndPoint)sock.RemoteEndPoint).Address.ToString() == "127.0.0.1")
                localcheck = true;
#if DEBUG
            localcheck=false;
#endif

            sock.ReceiveTimeout = 1000;     //Export��M�̃^�C���A�E�g�l
            sock.SendTimeout = 500;         //Export���M�̃^�C���A�E�g�l

            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;

            try
            {
                Front.AddLogData(0, Status, _ip + "����ڑ��v��");
                //Front.AddLogData(1, Status, "��t��������:" + sw.Elapsed.TotalMilliseconds.ToString());
                // Kick�Ώۂ��`�F�b�N
                if (Status.IsKickCheck(sock) == false && !localcheck)
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
                    Encoding enc = Encoding.ASCII; // "euc-jp"
                    int i = 0;
                    int j = 0;
                    try
                    {
                        for (; j < 5000; j++)
                        {
                            sock.Receive(reqBytes, j, 1, System.Net.Sockets.SocketFlags.None);
                            if (reqBytes[j] == '\r') continue;
                            if (reqBytes[j] == end[i]) i++; else i = 0;
                            if (i >= 2) break;
                        }
                        if (i >= 2)
                        {
                            // �A�ł��Ȃ��悤�ɉ����ԋp��x��������
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            // �r�W�[���b�Z�[�W���M
                            string str = enc.GetString(reqBytes, 0, j);
                            Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");
                            sock.Send(enc.GetBytes(Front.BusyString));
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
                        if (enc.GetString(reqBytes, 12, 4) != "MMS ")
                            Front.AddLogData(1, Status, "�r�W�[���b�Z�[�W�����M�ł��܂���ł����B");
                    }
                    Status.BusyCounter++;
                    #endregion
                    sock.Close();
                    return;
                }

                //��t�`�F�b�N


                _ua = AcceptUser(sock);

                //��t���ۂ��ꂽ�ڑ���ؒf
                if (_ua == "")
                {
                    sock.Close();
                    return;
                }
                //KAGAMI_LINK�͉��������ɏI���
                if (_ua == "KAGAMI_LINK")
                {
                    return;
                }
                //�G�N�X�|�[�g��tOK
                if (!localcheck)
                    Status.ExportCount++;
                //�N���C�A���g�փf�[�^���M
                sock.SendTimeout = (int)Front.Sock.SockSendTimeout;   //Export���M�̃^�C���A�E�g�l
                Status.Client.Send(Status, sock, _ua);

            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "�N���C�A���g��t�G���[(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")[" + _ip + ":" + _port + "]");
                sock.Close();
            }
            finally
            {

            }


        }

        /// <summary>
        /// �ڑ��v���ɗ����N���C�A���g�̐ڑ��ۂ𔻒肵�A
        /// ����Ȃ�HTTPHeader/StreamHeader�𑗐M
        /// </summary>
        /// <param name="sock">�N���C�A���g��socket</param>
        /// <returns>�N���C�A���g��UserAgent</returns>
        private string AcceptUser(System.Net.Sockets.Socket sock)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;
            bool localcheck = false;
            if (((IPEndPoint)sock.RemoteEndPoint).Address.ToString() == "127.0.0.1")
                localcheck = true;

            #region DenyIP�`�F�b�N
            foreach (string _denyip in Front.Acl.DenyHost)
            {
                if (string.IsNullOrEmpty(_denyip))
                    continue;
                IPAddress _out;
                if (IPAddress.TryParse(_denyip, out _out))
                {
                    try
                    {
                        Regex re = new Regex(_denyip);
                        if (re.IsMatch(_ip))
                        {
                            Front.AddLogData(0, Status, "�ڑ��֎~IP�̂��ߋ��ۂ��܂� [" + _ip + "]");
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            return "";


                        }
                    }
                    catch { }
                    if (_denyip.StartsWith(_ip))
                    {
                        Front.AddLogData(0, Status, "�ڑ��֎~IP�̂��ߋ��ۂ��܂� [" + _ip + "]");
                        Thread.Sleep((int)Front.Sock.SockCloseDelay);
                        return "";
                    }
                }
            }
            #endregion
            try
            {
                #region �v���g�R���`�F�b�N+UserAgent�擾
                string userAgent;
                string host = "";
                int priKagamiPort;//KagamiPort
#if DEBUG
                int KagamiLinkLevel = -1;//KagamiLink
#endif
                string authPass = "";
                System.Text.Encoding enc;

                char[] end = { '\n', '\n' };
                byte[] reqBytes = new byte[5000];
                int i = 0;
                int j = 0;

                //�K�v�ȏ�ɃK�`�K�`�ɑg��ł܂����A��҂̎�ł��O�O
                //���N�G�X�g�w�b�_�̉��s�R�[�h�͕���CR+LF������
                //WME�z�M�ȈՃe�X�g��LF�݂̂ŗ���̂ŗ��Ή��ɂ���
                try
                {
                    for (; j < 5000; j++)
                    {
                        sock.Receive(reqBytes, j, 1, System.Net.Sockets.SocketFlags.None);
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
                        Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" +
                                     enc.GetString(reqBytes, 0, j) +
                                     "\r\nRecvReqMsg(Client)End-----");
                        if (e is SocketException)
                        {
                            SocketException se = (SocketException)e;
                            Front.AddLogData(1, Status, "���N�G�X�g��M�^�C���A�E�g [" + _ip + "] (wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")");
                        }
                        else
                        {
                            Front.AddLogData(1, Status, "���N�G�X�g��M�^�C���A�E�g [" + _ip + "] (" + e.Message + ")");
                        }
                    }
                    //MMS(TCP)�̓R�R��NG�B�܂��͎�M�^�C���A�E�g�B�Ȃ̂ŁA�ؒf�x���͂����Ȃ��B
                    //�ؒf���邽��null�ԋp
                    return "";
                }

                //Front.AddLogData(1, Status, "1.1.1:" + sw.ElapsedMilliseconds.ToString());
                //sw.Reset();
                //sw.Start();
                if (i < 2)
                {
                    //��M�ł����Ƃ���܂ł̃��N�G�X�gMsg�����O�o��
                    enc = Front.GetCode(reqBytes, j);
                    Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" +
                                 enc.GetString(reqBytes, 0, j) +
                                 "\r\nRecvReqMsg(Client)End-----");
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
                if (str.IndexOf("\r\n") < 8)
                {
                    Front.AddLogData(0, Status, "�Ή����Ă��Ȃ����N�G�X�g�̂��ߐؒf���܂� [" + _ip + "]");
                    Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");
                    //�v���g�R�����o�����o���Ȃ��p�^�[��
                    //�ؒf���邽��null�ԋp
                    return "";
                }
                int _s = str.Split('\n')[0].LastIndexOf(' ') + 1;
                int _l = str.Split('\n')[0].LastIndexOf('/') - _s;
                string protocol = str.Substring(_s, _l);
                Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");

#region KAGAMI�����N
#if DEBUG
                if (protocol == "KAGAMI")
                {
                    sock.Send(enc.GetBytes(Front.KagamiLinkRes));
                    byte[] _byte = { 0x00, 0x08, 0x00, 0x02, 0x05, 0x00, 0x00, 0x00 };
                    sock.Send(_byte);

                    //Front.AddLogDebug("LINK", BitConverter.ToString(enc.GetBytes(Front.KagamiLinkRes)));
                    Front.AddLogData(0, Status, "���ԃ����N���J�n���܂�");
                    Status.IKLink = new KagamiLink(Status, sock);
                    return "KAGAMI_LINK";
                }
#endif
#endregion
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
                    sock.Send(enc.GetBytes(ackMsg));
                    //�ؒf���邽��null�ԋp
                    return "";
                }
#region DenyUA�`�F�b�N

                foreach (string _denyua in Front.Acl.DenyUA)
                {
                    if (string.IsNullOrEmpty(_denyua))
                        continue;
                    try
                    {
                        Regex re = new Regex(_denyua);

                        if (re.IsMatch(userAgent))
                        {

                            Front.AddLogData(0, Status, "�ڑ��֎~UA�̂��ߋ��ۂ��܂� [" + _ip + "/UA:" + userAgent + "]");
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            return "";

                        }
                    }
                    catch { }

                    if (userAgent.Contains(_denyua))
                    {
                        Front.AddLogData(0, Status, "�ڑ��֎~UA�̂��ߋ��ۂ��܂� [" + _ip + "/UA:" + userAgent + "]");
                        Thread.Sleep((int)Front.Sock.SockCloseDelay);
                        return "";

                    }

                }


#endregion
                if (Front.Sock.VirtualHost)
                {
                    try
                    {
                        host = str.Substring(str.IndexOf("Host: ") + 6, str.IndexOf("\r\n", str.IndexOf("Host: ")) - str.IndexOf("Host: ") - 6);
                        if (host.Contains(":"))
                        {
                            host = host.Remove(host.LastIndexOf(':'));
                        }
                    }
                    catch
                    {
                    }
                }
                if (Status.ExportAuth)
                {
                    try
                    {

                        authPass = str.Substring(str.IndexOf("Authorization: ") + 15, str.IndexOf("\r\n", str.IndexOf("Authorization: ")) - str.IndexOf("Authorization: ") - 15);
#if DEBUG
                        //Front.AddLogData(1, Status, "authPass(BASE64):" + authPass);
#endif
                        authPass = authPass.Replace("Basic ", "").Replace("basic ", "");
                        authPass = enc.GetString(Convert.FromBase64String(authPass));
#if DEBUG
                        //Front.AddLogData(1, Status, "authPass(ASCII):" + authPass);
#endif
                    }
                    catch //(Exception ex)
                    {
#if DEBUG
                        //Front.AddLogData(1, Status, ex.Message);
#endif

                    }
                }
                //Front.AddLogData(1, Status, "1.2:" + sw.ElapsedMilliseconds.ToString());
                //sw.Reset();
                //sw.Start();
                string _tempra = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
                if (!_tempra.Contains("192.168.") && !_tempra.Contains("127.0.0.1"))
                {
                    if (!host.Contains(Front.Hp.IpHTTP.Replace("http://", "")) && Front.Sock.VirtualHost)
                    {
                        Front.AddLogData(0, Status, "�z�X�g������v���Ȃ����ߐؒf���܂��B (Host:" + host + ")[" + _ip + "]");
                        Thread.Sleep((int)Front.Sock.SockCloseDelay);
                        return "";
                    }
#region sql
                    /*
                    else if (Front.Opt.MySQLEnable && Status.SQLOn)
                    {


                        if (conn != null)
                            conn.Close();

                        string connStr = String.Format("server={0};user id={1}; password={2}; database=" + Front.Opt.RearDB + "; pooling=false",
                            Front.Opt.HostMySQL, Front.Opt.UserMuSQL, Front.Opt.PassMySQL);

                        try
                        {
                            conn = new MySqlConnection(connStr);
                            conn.Open();

                        }
                        catch (MySqlException ex)
                        {
                            Front.AddLogData(0, Status, "Error connecting to the server: " + ex.Message);
                            //MessageBox.Show("Error connecting to the server: " + ex.Message);
                        }

                        MySqlDataReader reader = null;
                        string sql = "select " + Front.Opt.ReadRowMySQL + " from " + Front.Opt.ReadHyou +
                            " where " + Front.Opt.ReadRowMySQL + " = '" + host + "' and " + Front.Opt.ReadRowIPMySQL + "='" + _ip + "'";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);

                        Front.AddLogData(1, Status, "sql: " + sql);
                        try
                        {
                            reader = cmd.ExecuteReader();

                            if (!reader.Read())
                            {

                                Front.AddLogData(0, Status, "�z�X�g������v���Ȃ����ߐؒf���܂��B (Host:" + host + ")[" + _ip + "]");
                                Thread.Sleep((int)Front.Sock.SockCloseDelay);
                                reader.Close();
                                return "";

                            }
                            Front.AddLogData(0, Status, "Revsql:" + reader.GetString(0));
                            reader.Close();

                        }
                        catch (MySqlException ex)
                        {
                            Front.AddLogData(0, Status, "Failed to populate database list: " + ex.Message);
                            //MessageBox.Show("Failed to populate database list: " + ex.Message);
                        }
                        finally
                        {
                            if (reader != null) reader.Close();
                        }

                    
                    }*/
#endregion
                }
#region kagami-port
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
#endregion

#region kagami-link
#if DEBUG
                // �������N�����o��
                _idx = str.IndexOf("Pragma: kagami-link=");
                if (_idx > 0)
                {
                    _idx += 20;
                    int _len = str.IndexOf("\r\n", _idx) - _idx;
                    if (_len == 0)
                    {
                        KagamiLinkLevel = 0;
                    }
                    else
                    {
                        try
                        {
                            KagamiLinkLevel = int.Parse(str.Substring(_idx, _len));


                        }
                        catch
                        {
                            KagamiLinkLevel = 0;
                        }
                    }
                }
#if DEBUG
                Front.AddLogData(0, Status, "KagamiLink=" + KagamiLinkLevel);
#endif
#endif
#endregion


#endregion


                bool _browser = false;
                bool _mobile = false;
                if (Front.Opt.BrowserView)
                {
                    // PC�p�u���E�U
                    if (userAgent.IndexOf("Mozilla") == 0 ||
                        userAgent.IndexOf("Opera") == 0)
                    {
                        _browser = true;
                    }
                    // �g�уu���E�U
                    if (userAgent.Contains("UP.Browser") ||
                        userAgent.IndexOf("J-PHONE") == 0 ||
                        userAgent.IndexOf("Vodafone") == 0 ||
                        userAgent.IndexOf("SoftBank") == 0 ||
                        userAgent.IndexOf("DoCoMo") == 0)
                    {
                        _mobile = true;
                    }
                }
                if (_browser || _mobile)
                {
#region Mozilla����
                    string ackMsg = "";
                    string head = "";
                    string childMsg = "";
                    Front.AddLogData(0, Status, "Web�u���E�U����̃A�N�Z�X�ł� UA: " + userAgent);
                    //�Ƃ肠�����B�B�B�Ȍ����G���W���΍�B
                    if (str.IndexOf("GET /robot.txt") >= 0 ||
                        str.IndexOf("GET /robots.txt") >= 0)
                    {
                        // �����G���W����robot.txt�ɃA�N�Z�X���Ă������̉������b�Z�[�W
                        ackMsg = "HTTP/1.0 200 OK\r\n" +
                                 "Server: " + Front.AppName + "\r\n" +
                                 "Cache-Control: no-cache\r\n" +
                                 "Pragma: no-cache\r\n" +
                                 "Keep-Alive: timeout=1, max=0\r\n" +
                                 "Connection: close\r\n" +
                                 "Content-Type: text/plain\r\n\r\n" +
                                 "User-agent: *\r\n" +
                                 "Disallow: /\r\n";
                        enc = Encoding.ASCII;
                        sock.Send(enc.GetBytes(ackMsg));
                        Front.AddLogData(0, Status, "�R�l�N�V������ؒf���܂� [" + _ip + "]");
                        //�ؒf���邽��null�ԋp
                        return "";
                    }
                    if (Front.Opt.PriKagamin == true)
                    {
                        //�����݂�D�惂�[�h�̏ꍇ�A�q�������o�͂���
                        //ClientItem���狾�|�[�g�����o�����N���C�A���g�ꗗ���o��
                        lock (Status.Gui.ClientItem)
                        {
                            bool none_flg = true;
                            for (i = 0; i < Status.Gui.ClientItem.Count; i++)
                            {
                                string _tmpHost;
                                int _tmpPort;
                                try
                                {
                                    if (!Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.Contains("/Port=") &&
                                        (!Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.Contains("kagami") || !Front.Opt.PriKagamiexe))
                                        continue;
                                    _tmpHost = Status.Gui.ClientItem[i].SubItems[Front.clmCV_IP_IDX].Text;
                                    if (Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.Contains("/Port="))
                                        _tmpPort = int.Parse(Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.Substring(Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.IndexOf("/Port=") + 6));
                                    else
                                        _tmpPort = 0;
                                }
                                catch
                                {
                                    // UserAgent���狾�|�[�g�擾NG
                                    continue;
                                }
                                if (_tmpPort == 0)
                                    childMsg += "\r\n http://" + _tmpHost;
                                else
                                    childMsg += "\r\n http://" + _tmpHost + ":" + _tmpPort;
                                none_flg = false;
                            }
                            if (none_flg)
                                childMsg += " none";
                        }
                    }
                    try
                    {
                        StreamReader sr = new StreamReader(@"html/template_port.txt", Encoding.GetEncoding("Shift_JIS"));
                        head =
                            "HTTP/1.0 200 OK\r\n" +
                            "Server: " + Front.AppName + "\r\n" +
                            "Cache-Control: no-cache\r\n" +
                            "Pragma: no-cache\r\n" +
                            "Keep-Alive: timeout=1, max=0\r\n" +
                            "Connection: close\r\n";
                        if (!Front.Opt.BrowserViewMode || _mobile)
                            head += "Content-Type: text/plain\r\n\r\n";   // TEXT���[�h or �g�тȂ�TEXT�o��
                        else
                            head += "Content-Type: text/html\r\n\r\n";    // HTML���[�h
                        while (sr.Peek() > 0)
                        {
                            string TemplatePortInfo = sr.ReadLine();
                            ackMsg += TemplatePortInfo
                                .Replace("<TIME>", Status.ImportTimeString)
                                .Replace("<SRC_URL>", (Status.UrlVisible ? Status.ImportURL : "�ݒ肪��\���ɂȂ��Ă��܂�"))
                                .Replace("<COMMENT>", Status.Comment)
                                .Replace("<CONN>", Status.Client.Count.ToString())
                                .Replace("<MAXCONN>", Status.Connection + "+" + Status.Reserve)
                                .Replace("<CURRENT>", Status.CurrentDLSpeed + "Kbps (" + (int)(Status.CurrentDLSpeed / 8) + "KB/sec)")
                                .Replace("<BITRATE>", Status.MaxDLSpeed + "Kbps (" + (int)(Status.MaxDLSpeed / 8) + "KB/sec)")
                                .Replace("<BANDWIDTH>", (Front.BndWth.EnableBandWidth ? Status.LimitUPSpeed + "Kbps (" + (int)(Status.LimitUPSpeed / 8) + "KB/sec)" : "none"))
                                .Replace("<BUSYCOUNT>", Status.BusyCounter.ToString())
                                .Replace("<IMPORT_ERROR>", Status.ImportError.ToString())
                                .Replace("<EXPORT_ERROR>", Status.ExportError.ToString())
                                .Replace("<LIVE_URL>", Status.Url.ToString())
                                .Replace("<CHILD>", childMsg)
                                .Replace("<TRANS_COUNT>", Status.TransCount.ToString())
                                .Replace("<VERSION>", Front.AppName)
                                .Replace("<TITLE>", Status.ASFTitle)
                                .Replace("<AUTHOR>", Status.ASFAuthor)
                                .Replace("<COPYRIGHT>", Status.ASFCopyRight)
                                .Replace("<DESCRIPTION>", Status.ASFDescription)
                                .Replace("<RATING>", Status.ASFRating)
                                .Replace("<HEIGHT>", Status.MediaHeight.ToString())
                                .Replace("<WIDTH>", Status.MediaWidth.ToString())
                            ;
                            ackMsg += "\r\n";
                        }
                        sr.Dispose();

                        /*
                        if (!Front.Opt.BrowserViewMode || _mobile)
                        {
                            Regex r = new Regex("<.*?>", RegexOptions.Multiline);
                            ackMsg = head + ackMsg.Replace("<br>", "\r\n");
                            ackMsg = r.Replace(ackMsg, "");
                        }
                        else
                        {
                            ackMsg = head + ackMsg.Replace("\r\n", "<br>");
                        }
                        */
                    }
                    catch
                    {
                        // �e���v���[�g�Ǎ������s�����ꍇ�A���܂łǂ���̌`���ŏo�͂���
                        ackMsg =
                            "HTTP/1.0 200 OK\r\n" +
                            "Server: " + Front.AppName + "\r\n" +
                            "Cache-Control: no-cache\r\n" +
                            "Pragma: no-cache\r\n" +
                            "Keep-Alive: timeout=1, max=0\r\n" +
                            "Connection: close\r\n" +
                            "Content-Type: text/plain\r\n\r\n" +
                            "CurrentBitrate: " + Status.CurrentDLSpeed + "Kbps (" + (int)(Status.CurrentDLSpeed / 8) + "KB/sec)\r\n" +
                            "MaximumBitrate: " + Status.MaxDLSpeed + "Kbps (" + (int)(Status.MaxDLSpeed / 8) + "KB/sec)\r\n" +
                            "LimitUpBitrate: " + (Front.BndWth.EnableBandWidth ? Status.LimitUPSpeed + "Kbps (" + (int)(Status.LimitUPSpeed / 8) + "KB/sec)" : "none") + "\r\n" +
                            "CurrentConnection: " + Status.Client.Count + "/" + Status.Connection + "+" + Status.Reserve + "\r\n" +
                            "BusyCounter:       " + Status.BusyCounter + "\r\n" +
                            "ChildMirrorInfo:" + childMsg + "\r\n";
                    }
                    // �|�[�g��ԕ\����ShiftJIS�ōs���B
                    enc = Encoding.GetEncoding("Shift_JIS");
                    sock.Send(enc.GetBytes(head + ackMsg));
                    Front.AddLogData(0, Status, "�R�l�N�V������ؒf���܂� [" + _ip + "]");
#endregion
                    //�ؒf���邽��null�ԋp
                    return "";
                }
                else
                {


                    // Mozilla�ȊO�Ȃ���l�`�F�b�N����
#region ���l�`�F�b�N����
                    ///�����ւ̓]���������Ȃ��ꍇ�̓r�W�[����
                    if (userAgent.Contains("PriCheck") == true && Front.Opt.NotMyTrans == true)
                    {
                        Front.AddLogData(0, Status, "�]���s���̂��ߐڑ������ۂ��܂� [" + _ip + "]");
                        try
                        {
                            // �r�W�[�������M
                            sock.Send(enc.GetBytes(Front.BusyString));
                        }
                        catch
                        {
                            Front.AddLogData(1, Status, "�r�W�[���b�Z�[�W�����M�ł��܂���ł����B");
                        }
                        return "";
                    }
                    // �܂��́A���U�ڑ��o���邩�`�F�b�N
                    // ���U���X�g�ɊY��IP���~��Ԃő��݂��Ȃ��@�܂��́A
                    // �ő�ڑ��\���𒴂��Ă�ꍇ�A���Ȃ킿�A
                    //   ���݂̐ڑ�����(�ő�ʏ�ڑ����{�ő僊�U�ڑ���)�@�Ȃ�A���U�ڑ��s��
                    if (!Status.IsReserveList(_ip) ||
                        Status.Client.Count >= Status.Connection + Status.Reserve)
                    {
                        // ���U�ڑ��s�B�ʏ�ڑ��ł��邩�`�F�b�N
                        // ���݂̒ʏ�ڑ������ő�ʏ�ڑ����@�Ȃ�A�ʏ�ڑ��s�B���Ȃ킿�A
                        // ���݂̑��ڑ����|���݂̃��U�[�u�ڑ������ő�ʏ�ڑ����@�Ȃ�A�ʏ�ڑ��s�B
                        if (Status.Client.Count - Status.ReserveCount >= Status.Connection && !localcheck)
                        {
                            // ���U�ڑ����ʏ�ڑ����s�B

                            // �D�惂�[�h���l���������l����
                            // ���U�󂫂�����(�ő�ڑ��\���𒴂��Ă���) or
                            // �����݂�ڑ��ł�kagami.exe�ڑ��ł��Ȃ� or
                            // �����݂�ڑ������ǂ����݂�D��OFF or
                            // kagami.exe�ڑ�������kagami.exe�D��OFF
                            if ((Status.Connection + Status.Reserve) <= Status.Client.Count ||
                                (priKagamiPort == 0 && userAgent.IndexOf("kagami/") < 0) ||
                                (priKagamiPort != 0 && Front.Opt.PriKagamin == false) ||
                                (userAgent.IndexOf("kagami/") >= 0 && Front.Opt.PriKagamiexe == false))
                            {
#region ���l����
                                string _hostport = null;
                                uint _cont;
                                //
                                if (Front.Opt.TransKagamin && !userAgent.Contains("kagami/") && !userAgent.Contains("WebABC") && Status.TransWeb)
                                {
                                    if (userAgent.Contains("/PriCheck"))
                                    {
                                        try
                                        {
                                            _cont = uint.Parse(userAgent.Substring(userAgent.Length - 1, 1));
                                            if (_cont == 0)
                                            {
                                                Front.AddLogData(0, Status, "�]���K�w�I�[�o�[�̂��ߐڑ������ۂ��܂� [" + _ip + "]");
                                                try
                                                {
                                                    // �r�W�[�������M
                                                    sock.Send(enc.GetBytes(Front.BusyString));
                                                }
                                                catch
                                                {
                                                    Front.AddLogData(1, Status, "�r�W�[���b�Z�[�W�����M�ł��܂���ł����B");
                                                }
                                                return "";
                                            }
                                            _cont -= 1;

                                        }
                                        catch
                                        {
                                            _cont = Front.Opt.TransCont;
                                        }
                                    }
                                    else
                                    {
                                        _cont = Front.Opt.TransCont;
                                    }
                                    //MessageBox.Show((DateTime.Now - Front.temptrans).Minutes.ToString());
                                    if ((DateTime.Now - Front.temptrans).Seconds > 60)
                                    {
                                        _hostport = KagaminCheck302((int)_cont);
                                        Front.temphost = _hostport;
                                        Front.temptrans = DateTime.Now;

                                    }
                                    else
                                    {
                                        _hostport = Front.temphost;

                                    }
                                }
                                else
                                {
                                    _hostport = null;
                                }
                                if (_hostport != null && _hostport != "")
                                {
                                    string _temp = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
                                    //�ڑ������v���C�x�[�gIP�łȂ��A���A�]���悪�v���C�x�[�gIP�̏ꍇ
                                    //�v���C�x�[�gIP�ł͊O����Ȃ���Ȃ��̂ŁA�O���[�o��IP��Ԃ��悤�ɂ���B
                                    //MessageBox.Show(_temp);
                                    if ((!_temp.Contains("192.168.") && !_temp.Contains("127.0.0.1")) &&
                                        _hostport.Contains("192.168.") || _hostport.Contains("127.0.0.1"))
                                    {
                                        if (Front.GlobalIP == "")
                                        {
                                            Front.GetGlobalIP();
                                        }
                                        if (Front.GlobalIP != "")
                                        {
                                            Match index = Regex.Match(_hostport, @":\d{1,5}");
                                            _hostport = "http://" + Front.GlobalIP + ":" + _hostport.Substring(index.Index + 1, index.Length - 1);
                                        }
                                    }
                                    //301 Moved Permanently�͍P�v�I�Ȉړ��A302 Found�͈ꎞ�I
                                    //�ǂ�����v���C���[�̋����͓����͗l
                                    //WMP,GOM,winamp,VLC�Ń��_�C���N�g�m�F�BMPC�̓��_�C���N�g���Ȃ��d�l�̖͗l�B
                                    string reqMsg = "HTTP/1.0 301 Moved Permanently\r\n" +
                                                    "Location: " + _hostport + "\r\n" +
                                                    "User-Agent: " + Front.UserAgent + "\r\n" +
                                        /*"Host: " + ((IPEndPoint)sock.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)sock.RemoteEndPoint).Port + "\r\n" +*/
                                                    "Pragma: no-cache\r\n" +
                                                    "Content-Type: application/x-mms-framed\r\n\r\n";

                                    Thread.Sleep(1000);
                                    sock.Send(enc.GetBytes(reqMsg));
                                    Front.AddLogData(1, Status, "�r�W�[�̂��ߐڑ������ۂ��܂� [" + _ip + "]/�q���]������[" + _hostport + "]");
                                    Status.TransCount++;
                                    return "";
                                }
                                else
                                {
                                    Front.AddLogData(0, Status, "�r�W�[�̂��ߐڑ������ۂ��܂� [" + _ip + "]");
                                    try
                                    {
                                        bool flag = false;
                                        // �A�ł��Ȃ��悤�ɉ������M��x��������
                                        // �������AWebABC�͒x���������TimeOut�\���ɂȂ�̂ő��؂肷��B
                                        foreach (string tempra in Front.Acl.CheckUserAgent)
                                        {
                                            if (userAgent.Contains(tempra))
                                                flag = true;
                                        }
                                        if (!flag)
                                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                                        // �r�W�[�������M
                                        if (Front.Sock.ConnInfoSend)
                                        {
                                            sock.Send(enc.GetBytes(Front.BusyString.Replace("Pragma: client-id=0\r\n", "Pragma: client-id=0\r\nPragma: coninfo=" + Status.Client.Count + "/" + Status.Connection + "+" + Status.Reserve + "\r\n")));
                                        }
                                        else
                                        {
                                            sock.Send(enc.GetBytes(Front.BusyString));
                                        }
                                    }
                                    catch
                                    {
                                        Front.AddLogData(1, Status, "�r�W�[���b�Z�[�W�����M�ł��܂���ł����B");
                                    }
                                    Status.BusyCounter++;
#endregion
                                    //�ؒf���邽��null�ԋp
                                    return "";
                                }
                            }

                        }
                    }
#endregion

                    // ���l����Ȃ�
                    if (userAgent.IndexOf("NSPlayer") == 0)
                    {

                        if (Status.ExportAuth && Status.AuthID != "" && Status.AuthPass != "")
                        {
                            //�F�؃p�X���܂܂�Ă��Ȃ��܂��͈�v���Ȃ��ꍇ��401��Ԃ�
                            if (authPass == "" || authPass != Status.AuthID + ":" + Status.AuthPass)
                            {
                                Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                    (Front.AuthString()) +
                                    "\r\n\n\n" + "\r\nSendRspMsg(Client)End-----");
                                sock.Send(enc.GetBytes(Front.AuthString()));
                                if (authPass == "")
                                    Front.AddLogData(1, Status, "�F�ؗv�����b�Z�[�W�𑗐M���܂� [" + _ip + "]");
                                else
                                    Front.AddLogData(1, Status, "�F��NG�̂��ߐؒf���܂�[" + _ip + "/" + authPass + "])");

                                return "";
                            }
                            else
                            {
                                Front.AddLogData(1, Status, "�F�؊������܂���[" + _ip + "]");

                            }

                        }

                        if (str.IndexOf("x-mms-framed") > 0 || str.IndexOf("stream-switch") > 0 || Status.Type == 2)
                        {
#region NSPlayer�w�b�_���M�{�ڑ�����
                            try
                            {
                                // �����w�b�_���M
                                /* �����݂�̉����͕K��HTTP/1.0
                                if (str.IndexOf("HTTP/1.1") > -1)
                                {
                                    Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                                                enc.GetString(Status.DataRspMsg11) +
                                                                "\r\nSendRspMsg(Client)End-----");
                                    sock.Send(Status.DataRspMsg11);
                                }
                                else
                                {
                                */

                                bool flag = false;
                                foreach (string tempra in Front.Acl.CheckUserAgent)
                                {
                                    if (userAgent.Contains(tempra))
                                        flag = true;
                                }

                                if (userAgent.Contains("Kagamin2") && userAgent.Contains("ConnCheck"))
                                {
                                    flag = true;
                                }
                                string str1 = enc.GetString(Status.DataRspMsg10);

                                if (userAgent.Contains("kagami"))
                                {

                                    string bfr = str1.Substring(0, str1.IndexOf("X-Server:"));
                                    string aft = str1.Substring(str1.IndexOf("\r\n", str1.IndexOf("X-Server")) + 2);
                                    str1 = bfr + aft;

                                }



                                if (Front.Sock.ConnInfoSend && flag)
                                {
                                    str1 = str1.Substring(0, str1.Length - 4) + "\r\nPragma: coninfo=" +
                                            Status.Client.Count + "/" + Status.Connection + "+" + Status.Reserve +
                                            "\r\n\n\n";
                                }

                                Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                                            str1 +
                                                            "\r\nSendRspMsg(Client)End-----");
                                sock.Send(enc.GetBytes(str1));


                                //}
                                // �w�b�_�X�g���[�����M


                                sock.Send(Status.HeadStream);
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.Message);
                                Front.AddLogData(1, Status, "�w�b�_���MNG/�R�l�N�V������ؒf���܂�(���R:" + ex.Message + ") [" + _ip + "]");
                                return "";
                            }
#endregion
                            //�ڑ��ێ��̂���UserAgent�ԋp
                            //�q���ڑ��̏ꍇ�A�|�[�g�ԍ���t������UA��ԋp
                            //KagamiLink��0�ȏ�̏ꍇ�͋������N���t��
                            if (priKagamiPort != 0)
#if DEBUG
                                return KagamiLinkLevel > 0 ?
                                    userAgent + "/Port=" + priKagamiPort + "/Link=" + KagamiLinkLevel : userAgent + "/Port=" + priKagamiPort;
#else
                                return userAgent + "/Port=" + priKagamiPort;
#endif
                            else
                                return userAgent;
                        }
                        // x-mms-framed�܂���stream-switch�w�b�_�������ꍇ�A
                        // �w�b�_�̂ݑ����Ă����ɐؒf����
                        else
                        {
#region NSPlayer�w�b�_���M�{�ڑ��I��
                            //Front.AddLogData(Status, "�w�b�_���M�{�I���{1.1");
                            Front.AddLogData(0, Status, "�w�b�_���擾���邽�߂̐ڑ� UA: " + userAgent);
                            try
                            {
                                // �����w�b�_���M
                                /* �����݂�̉����͕K��HTTP/1.0
                                if (str.IndexOf("HTTP/1.1") > -1)
                                {
                                    Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                                                enc.GetString(Status.HeadRspMsg11) +
                                                                "\r\nSendRspMsg(Client)End-----");
                                    sock.Send(Status.HeadRspMsg11);
                                }
                                else
                                {
                                */

                                bool flag = false;
                                foreach (string tempra in Front.Acl.CheckUserAgent)
                                {
                                    if (userAgent.Contains(tempra))
                                        flag = true;
                                }
                                if (userAgent.Contains("Kagamin2") && userAgent.Contains("ConnCheck"))
                                {
                                    flag = true;
                                }

                                string str1 = enc.GetString(Status.HeadRspMsg10);

                                if (userAgent.Contains("kagami"))
                                {

                                    string bfr = str1.Substring(0, str1.IndexOf("X-Server:"));
                                    string aft = str1.Substring(str1.IndexOf("\r\n", str1.IndexOf("X-Server")) + 2);
                                    str1 = bfr + aft;

                                }
                                if (Front.Sock.ConnInfoSend && flag)
                                {
                                    str1 = str1.Substring(0, str1.Length - 4) + "\r\nPragma: coninfo=" +
                                            Status.Client.Count + "/" + Status.Connection + "+" + Status.Reserve +
                                            "\r\n\n\n";
                                }


                                Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                                            str1 +
                                                            "\r\nSendRspMsg(Client)End-----");
                                sock.Send(enc.GetBytes(str1));
                                //
                                // �w�b�_�X�g���[�����M
                                sock.Send(Status.HeadStream);
                            }
                            catch { }

                            Front.AddLogData(0, Status, "�R�l�N�V������ؒf���܂� [" + _ip + "]");
#endregion
                            //�ؒf���邽��null�ԋp
                            return "";

                        }
                    }
                    else
                    {

                        //WMP12�p
                        //�T�[�o�[���͌Œ�̕K�v����(Kagami�`�`���ƃT�[�o�[�G���[�ɂȂ�)
                        if (userAgent.Contains("Windows-Media-Player"))
                        {
                            string ackMsg = "HTTP/1.0 400 Bad Request\r\n" +
                                 "Server: Rex/11.0.5721.5251\r\n" +
                                 "Cache-Control: no-cache\r\n" +
                                 "Pragma: no-cache\r\n\r\n";


                            Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                ackMsg +
                                "\r\nSendRspMsg(Client)End-----");

                            sock.Send(enc.GetBytes(ackMsg));

                        }

                        //NSPlayer�ȊO�ł̐ڑ�
                        //�����ł�HTTP�G���[������Ԃ��Ă͂����Ȃ��B�����Ԃ����ɐؒf���邱�ƂŁA
                        //�N���C�A���g����UserAgent��NSPlayer�ɐ؂�ւ��čĐڑ����Ă���B
                        Front.AddLogData(0, Status, "NSPlayer�ȊO�ł̐ڑ��ł� UA: " + userAgent);
                        Front.AddLogData(0, Status, "�R�l�N�V������ؒf���܂� [" + _ip + "]");
                        //���Đڑ������邽�߁A�ؒf�x���͍s��Ȃ��B
                        //�ؒf���邽��null�ԋp
                        return "";
                    }
                }
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "�N���C�A���g��t�G���[(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")[" + _ip + "]");
                return "";
            }
            finally
            {

            }
            // �����ɂ͗��Ȃ�
            //return "";
        }
        /// <summary>
        /// �r�W�[���q���]���`�F�b�N
        /// </summary>
        /// <returns></returns>
        private string KagaminCheck302(int _cont)
        {
            bool notflag = false;
            string _firsthost = "";
            Socket sock_chk = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            lock (Status.Gui.ClientItem)
            {
                for (int i = 0; i < Status.Gui.ClientItem.Count; i++)
                {
                    string _tmpHost;
                    int _tmpPort;
                    try
                    {
                        if (!Status.Gui.ClientItem[i].SubItems[2].Text.Contains("/Port="))  // SubItems[2]:clmClientViewUA
                            continue;
                        //MessageBox.Show(Status.Gui.ClientItem[i].SubItems[3].Text.StartsWith("00:00:").ToString() + "\r\n" +
                        // Status.Gui.ClientItem[i].SubItems[3].Text.Substring(6, 2));
                        //�ڑ�����10sec�ȓ��̃N���C�A���g�̓X���[
                        if (Status.Gui.ClientItem[i].SubItems[3].Text.StartsWith("00:00:") &&
                            int.Parse(Status.Gui.ClientItem[i].SubItems[3].Text.Substring(6, 2)) <= 10)
                            continue;

                        _tmpHost = Status.Gui.ClientItem[i].SubItems[1].Text;               // SubItems[1]:clmClientViewIP
                        _tmpPort = int.Parse(Status.Gui.ClientItem[i].SubItems[2].Text.Substring(Status.Gui.ClientItem[i].SubItems[2].Text.IndexOf("/Port=") + 6)); // clmClientViewUA

                    }
                    catch
                    {
                        // UserAgent���狾�|�[�g�擾NG
                        continue;
                    }
                    try
                    {
                        //kgm_port = int.Parse(UserAgent.Substring(UserAgent.IndexOf("/Port=") + 6));

                        Front.AddLogData(0, Status, "�q���]���`�F�b�N���J�n���܂��B/�]���[��=" + _cont.ToString() + " /[" + _tmpHost + ":" + _tmpPort + "]");

                        // �q���|�[�g�`�F�b�N�pSocket�̍쐬
                        IPAddress hostadd = Dns.GetHostAddresses(_tmpHost)[0];
                        IPEndPoint ephost = new IPEndPoint(hostadd, _tmpPort);

                        sock_chk.SendTimeout = (int)Front.Sock.SockConnTimeout;       // Import�ڑ� �w�b�_�擾�v�����M�̃^�C���A�E�g�l
                        sock_chk.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    // Import�ڑ� �w�b�_�擾������M�̃^�C���A�E�g�l

                        // �ڑ�
                        sock_chk.Connect(ephost);

                        // �q���ɋU���������N�G�X�g�𑗐M
                        // �N���C�A���g�ɋU�������w�b�_�𑗐M
                        string reqMsg = "GET / HTTP/1.1\r\n" +
                            "Accept: */*\r\n" +
                            "User-Agent: " + Front.UserAgent + "/PriCheck" + _cont.ToString() + "\r\n" +
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
                        //http�X�e�[�^�X��200�ȊO
                        if (http_status != 200)
                        {
                            //�q���Ń��_�C���N�g���A���Ă����ꍇ�����̃z�X�g���L�^
                            if (http_status == 301 || http_status == 302)
                            {
                                _firsthost = "http://" + _tmpHost + ":" + _tmpPort;
                                notflag = true;
                                sock_chk.Close();
                                Front.AddLogData(1, Status, "�q���]���`�F�b�N�ē]��OK[" + _tmpHost + ":" + _tmpPort + "]");
                                continue;

                            }
                            //�r�W�[�Ȃ炻�̂܂�
                            else
                            {
                                sock_chk.Close();
                                Front.AddLogData(1, Status, "�q���]���`�F�b�NNG[" + _tmpHost + ":" + _tmpPort + "]");
                                continue;
                            }
                        }
                        /*
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
                         */
                        sock_chk.Close();
                        Front.AddLogData(1, Status, "�q���]���`�F�b�NOK [" + _tmpHost + ":" + _tmpPort + "]");
                        //�󂢂Ă�q����Ԃ�
                        return "http://" + _tmpHost + ":" + _tmpPort;
                    }
                    catch
                    {
                        sock_chk.Close();
                        Front.AddLogData(1, Status, "�q���]���`�F�b�NNG[" + _tmpHost + ":" + _tmpPort + "]");
                    }
                }
            }
            //�󂢂ĂȂ��ꍇ301��Ԃ����q����Ԃ�
            if (notflag == true)
                return _firsthost;
            else
                //301���Ȃ��ꍇnull�B�r�W�[���b�Z�[�W���M
                return null;
        }
    }
}
