using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

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

                TcpListener _listener = TcpListener.Create(Status.MyPort);
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
                            Socket _sock = _listener.AcceptSocket();
                            Thread _th = new Thread(ClientTask);
                            _th.Start(_sock);
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
                            TcpClient _sock = _listener.AcceptTcpClient();
                            _sock.Close();
                        }
                    }
                    catch { }
                    // Listen��~
                    _listener.Stop();
                    Status.ListenPort = false;
                }
                Front.AddLogData(0, Status, "�G�N�X�|�[�g�^�X�N���I�����܂�");
            }
        }

        /// <summary>
        /// �N���C�A���g���M�^�X�N
        /// </summary>
        /// <param name="obj"></param>
        private void ClientTask(object obj)
        {
            Socket sock = (Socket)obj;
            string _ua = "";

            sock.ReceiveTimeout = 1000;     //Export��M�̃^�C���A�E�g�l
            sock.SendTimeout = 500;         //Export���M�̃^�C���A�E�g�l

            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;

            try
            {
                Front.AddLogData(0, Status, _ip + "����ڑ��v��");
                // Kick�Ώۂ��`�F�b�N
                if (Status.IsKickCheck(sock) == false)
                {
                    Front.AddLogData(0, Status, "Kick�Ώۂ̂��ߐڑ������ۂ��܂��B [" + _ip + "]");
                    #region Kick����
                    bool _not_found = true;
                    // KickItem����Y��IP����
                    lock (Status.Gui.KickItem)
                    {
                        foreach(ListViewItem _item in Status.Gui.KickItem)
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
                            Status.AddKick(_ip,1);
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
                            sock.Receive(reqBytes, j, 1, SocketFlags.None);
                            if (reqBytes[j] == '\r') continue;
                            if (reqBytes[j] == end[i]) i++; else i = 0;
                            if (i >= 2) break;
                        }
                        if (i >= 2)
                        {
                            // �A�ł��Ȃ��悤�ɉ����ԋp��x��������
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            // ���肩��̎�M�f�[�^�ۑ�
                            string str = enc.GetString(reqBytes, 0, j);
                            Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");
                            // �r�W�[���b�Z�[�W���M
                            Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" + Front.BusyString + "\r\nSendRspMsg(Client)End-----");
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
                            Front.AddLogData(1, Status, "�r�W�[���b�Z�[�W�����M�ł��܂���ł��� [" + _ip + "]");
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

                //�G�N�X�|�[�g��tOK
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

        }

        /// <summary>
        /// �ڑ��v���ɗ����N���C�A���g�̐ڑ��ۂ𔻒肵�A
        /// ����Ȃ�HTTPHeader/StreamHeader�𑗐M
        /// </summary>
        /// <param name="sock">�N���C�A���g��socket</param>
        /// <returns>�N���C�A���g��UserAgent</returns>
        private string AcceptUser(Socket sock)
        {
            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;

            try
            {
                #region �v���g�R���`�F�b�N+UserAgent�擾
                string userAgent;
                int priKagamiPort;
                Encoding enc;

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
                        sock.Receive(reqBytes, j, 1, SocketFlags.None);
                        if (reqBytes[j] == '\r') continue;
                        if (reqBytes[j] == end[i]) i++; else i = 0;
                        if (i >= 2) break;
                    }
                }
                catch(Exception e)
                {
                    // MMS(TCP)Check
                    // 12�`16byte�ڂ�"MMS "��MMS(TCP)
                    enc = Front.GetCode(reqBytes,j);
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
                            Front.AddLogData(1, Status, "���N�G�X�g��M�^�C���A�E�g [" + _ip + "] wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString());
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
                if (i < 2)
                {
                    //��M�ł����Ƃ���܂ł̃��N�G�X�gMsg�����O�o��
                    if (j > 0)
                    {
                        enc = Front.GetCode(reqBytes, j);
                        Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" +
                                     enc.GetString(reqBytes, 0, j) +
                                     "\r\nRecvReqMsg(Client)End-----");
                    }
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

                string protocol = str.Substring(str.IndexOf("\r\n") - 8, 4);
                Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");

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
                if(_browser||_mobile)
                {
                    #region Mozilla����
                    string ackMsg = "";
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
                                    if (!Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.Contains("/Port="))
                                        continue;
                                    _tmpHost = Status.Gui.ClientItem[i].SubItems[Front.clmCV_IP_IDX].Text;
                                    _tmpPort = int.Parse(Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.Substring(Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.IndexOf("/Port=") + 6));
                                }
                                catch
                                {
                                    // UserAgent���狾�|�[�g�擾NG
                                    continue;
                                }
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
                        ackMsg =
                            "HTTP/1.0 200 OK\r\n" +
                            "Server: " + Front.AppName + "\r\n" +
                            "Cache-Control: no-cache\r\n" +
                            "Pragma: no-cache\r\n" +
                            "Keep-Alive: timeout=1, max=0\r\n" +
                            "Connection: close\r\n";
                        if (!Front.Opt.BrowserViewMode || _mobile)
                            ackMsg += "Content-Type: text/plain\r\n\r\n";   // TEXT���[�h or �g�тȂ�TEXT�o��
                        else
                            ackMsg += "Content-Type: text/html\r\n\r\n";    // HTML���[�h
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
                                .Replace("<VERSION>", Front.AppName)
                            ;
                            ackMsg += "\r\n";
                        }
                        sr.Dispose();
                        if (!Front.Opt.BrowserViewMode || _mobile)
                        {
                            Regex r = new Regex("<.*?>", RegexOptions.Multiline);
                            ackMsg = ackMsg.Replace("<br>", "\r\n");
                            ackMsg = r.Replace(ackMsg, "");
                        }
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
                    sock.Send(enc.GetBytes(ackMsg));
                    Front.AddLogData(0, Status, "�R�l�N�V������ؒf���܂� [" + _ip + "]");
                    #endregion
                    //�ؒf���邽��null�ԋp
                    return "";
                }
                else
                {
                    // Mozilla�ȊO�Ȃ���l�`�F�b�N����
                    #region ���l�`�F�b�N����
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
                        if (Status.Client.Count - Status.ReserveCount >= Status.Connection)
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
                                #region ���l�E���_�C���N�g����
                                try
                                {
                                    bool busy_flg = true;   // Busy�����M
                                    // WebABC�̏ꍇ�̓��_�C���N�g���ؒf�x�����������؂肷��B
                                    if (userAgent.Contains("WebABC"))
                                    {
                                        Front.AddLogData(0, Status, "�r�W�[�̂��ߐڑ������ۂ��܂� [" + _ip + "]");
                                        Front.AddLogDetail(
                                            "SendRspMsg(Client)Sta-----\r\n" +
                                            Front.BusyString +
                                            "\r\nSendRspMsg(Client)End-----"); 
                                        sock.Send(enc.GetBytes(Front.BusyString));
                                        busy_flg = false;   // Busy���M�ς�
                                    }
                                    //�q���ւ̃��_�C���N�g���s���Ƃ�
                                    if (busy_flg && Status.EnableRedirectChild)
                                    {
                                        //�q�������W
                                        //�v���C�x�[�gIP���Ԃ邱�Ƃ͖����̂Ń`�F�b�N�s�v
                                        string _redirUrl = Status.Client.GetKagamiList();
                                        //�q����񂪗L��΃��_�C���N�g
                                        if (_redirUrl.Length > 0)
                                        {
                                            string _redirStr = "HTTP/1.0 302 Found\r\n" +
                                                "Location: " + _redirUrl + "\r\n" +
                                                "Connection: close\r\n" +
                                                "Content-Type: text/html\r\n\r\n" +
                                                "<html><head>\r\n" +
                                                "<title>302 Found</title>\r\n" +
                                                "</head><body>\r\n" +
                                                "<h1>Found</h1>\r\n" +
                                                "<p>The document has moved <a href=\"" + _redirUrl + "\">here</a>.</p>\r\n" +
                                                "</body></html>";
                                            Front.AddLogData(0, Status, "�r�W�[�̂��ߐڑ������ۂ��܂� [" + _ip + "] / �q���_�C���N�g���� URL=" + _redirUrl);
                                            Front.AddLogDetail(
                                                "SendRspMsg(Client)Sta-----\r\n" +
                                                _redirStr +
                                                "\r\nSendRspMsg(Client)End-----");
                                            // �]������ɂȂ�Ȃ��悤302�������M��x��������
                                            Thread.Sleep(1000);
                                            sock.Send(enc.GetBytes(_redirStr));
                                            busy_flg = false;   // Redirect���M�ς�
                                        }
                                    }
                                    //�e�ւ̃��_�C���N�g���s���Ƃ�
                                    if (busy_flg && Status.EnableRedirectParent && Status.Type != 2)
                                    {
                                        if (Status.ImportHost == "::1" ||               // IPv6 LoopBack
                                            Status.ImportHost.StartsWith("10.") ||      // ClassA
                                            Status.ImportHost.StartsWith("172.16.") ||  // ClassB
                                            Status.ImportHost.StartsWith("192.168.") || // ClassC
                                            Status.ImportHost.StartsWith("127."))       // LoopBack
                                        {
                                            // �e�����[�J���A�h���X�Ȃ�]��Skip
                                        }
                                        else
                                        {
                                            string _redirUrl = "http://" + (Status.ImportHost.Contains(":") ? "[" + Status.ImportHost +  "]" : Status.ImportHost) + ":" + Status.ImportPort;
                                            string _redirStr = "HTTP/1.0 302 Found\r\n" +
                                                "Location: " + _redirUrl + "\r\n" +
                                                "Connection: close\r\n" +
                                                "Content-Type: text/html\r\n\r\n" +
                                                "<html><head>\r\n" +
                                                "<title>302 Found</title>\r\n" +
                                                "</head><body>\r\n" +
                                                "<h1>Found</h1>\r\n" +
                                                "<p>The document has moved <a href=\"" + _redirUrl + "\">here</a>.</p>\r\n" +
                                                "</body></html>";
                                            Front.AddLogData(0, Status, "�r�W�[�̂��ߐڑ������ۂ��܂� [" + _ip + "] / �e���_�C���N�g���� URL=" + _redirUrl);
                                            Front.AddLogDetail(
                                                "SendRspMsg(Client)Sta-----\r\n" +
                                                _redirStr +
                                                "\r\nSendRspMsg(Client)End-----");
                                            // �]������ɂȂ�Ȃ��悤302�������M��x��������
                                            Thread.Sleep(1000);
                                            sock.Send(enc.GetBytes(_redirStr));
                                            busy_flg = false;   // Redirect���M�ς�
                                        }
                                    }
                                    if (busy_flg)
                                    {
                                        Front.AddLogData(0, Status, "�r�W�[�̂��ߐڑ������ۂ��܂� [" + _ip + "]");
                                        Front.AddLogDetail(
                                            "SendRspMsg(Client)Sta-----\r\n" +
                                            Front.BusyString +
                                            "\r\nSendRspMsg(Client)End-----");
                                        // �A�ł��Ȃ��悤�ɉ������M��x��������
                                        Thread.Sleep((int)Front.Sock.SockCloseDelay);
                                        // �r�W�[�������M
                                        sock.Send(enc.GetBytes(Front.BusyString));
                                    }
                                }
                                catch
                                {
                                    Front.AddLogData(1, Status, "�r�W�[���b�Z�[�W�����M�ł��܂���ł��� [" + _ip + "]");
                                }
                                Status.BusyCounter++;
                                #endregion
                                //�ؒf���邽��null�ԋp
                                return "";
                            }
                        }
                    }
                    #endregion

                    // ���l����Ȃ�
                    if (userAgent.IndexOf("NSPlayer") == 0 || userAgent.IndexOf("Windows-Media-Player") == 0)
                    {
                        if (str.IndexOf("x-mms-framed") > 0 || str.IndexOf("stream-switch") > 0)
                        {
                            #region NSPlayer�w�b�_���M�{�ڑ�����
                            try
                            {
                                // �����w�b�_���M
                                Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                                            enc.GetString(Status.DataRspMsg10) +
                                                            "\r\nSendRspMsg(Client)End-----");
                                sock.Send(Status.DataRspMsg10);
                                // �w�b�_�X�g���[�����M
                                sock.Send(Status.HeadStream);
                            }
                            catch
                            {
                                Front.AddLogData(1, Status, "�w�b�_���MNG/�R�l�N�V������ؒf���܂� [" + _ip + "]");
                                return "";
                            }
                            #endregion
                            //�ڑ��ێ��̂���UserAgent�ԋp
                            //�q���ڑ��̏ꍇ�A�|�[�g�ԍ���t������UA��ԋp
                            if (priKagamiPort != 0)
                                return userAgent + "/Port=" + priKagamiPort;
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
                                Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                                            enc.GetString(Status.HeadRspMsg10) +
                                                            "\r\nSendRspMsg(Client)End-----");
                                sock.Send(Status.HeadRspMsg10);
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
            // �����ɂ͗��Ȃ�
            //return "";
        }


    }
}
