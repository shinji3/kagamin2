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
        #region メンバ変数
        /// <summary>
        /// 鏡設定データ
        /// </summary>
        private Status Status = null;

        #endregion

        /// <summary>
        /// コンストラクタ
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
        /// エクスポートタスク
        /// </summary>
        private void ExportTask()
        {

            while (Status.RunStatus)
            {
                if (!Status.ImportStatus)
                {
                    // インポート接続が完了していない
                    Thread.Sleep(1000);
                    continue;
                }

                // Push配信ポートの解放待ち
                while (Status.ListenPort)
                    Thread.Sleep(100);
                // Import接続確立
                // Export待ち受け開始
                Front.AddLogData(0, Status, "エクスポートタスクを開始します");
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
                        Front.AddLogData(1, Status, "UPnPによりポートを開放しました。(Port:" + Status.MyPort.ToString() + ")");
                        lock (Front.UPnPPort)
                        {
                            Front.UPnPPort.Add(Status.MyPort);
                        }
                    }
                    else
                        Front.AddLogData(1, Status, "UPnPによるポート開放に失敗しました。(Port:" + Status.MyPort.ToString() + ")");
                }
                IPEndPoint _iep = new IPEndPoint(IPAddress.Any, Status.MyPort);
                System.Net.Sockets.TcpListener _listener = new System.Net.Sockets.TcpListener(_iep);
                // Listen開始
                try
                {
                    Status.ListenPort = true;
                    _listener.Start();
                }
                catch
                {
                    Front.AddLogData(1, Status, "エクスポートすることが出来ません。設定を確認して下さい。");
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
                        //Accept待ちのチェック
                        if (_listener.Pending() == true)
                        {
                            //Accept実施
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
                        //リスナーで待機しているクライアントをすべて切断する
                        while (_listener.Pending())
                        {
                            System.Net.Sockets.TcpClient _sock = _listener.AcceptTcpClient();
                            _sock.Close();
                        }
                    }
                    catch { }
                    // Listen停止
                    _listener.Stop();
                    Status.ListenPort = false;
                }
                #region comment
                /*form1のチェックに任せることにする
                if ((Front.UPnPPort.Contains(Status.MyPort)))
                {
                    //UPnPで閉じたら一瞬接続が切れる気がするので
                    //ほかに起動中のポートがあるときは閉じないほうがいいのだろうか
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
                            Front.AddLogData(1, Status, "UPnPによりポートを閉鎖しました。(Port:" + Status.MyPort.ToString() + ")");
                            lock (Front.UPnPPort)
                            {
                                Front.UPnPPort.Remove(Status.MyPort);
                            }
                        }
                        else if (!_tmp)
                        {
                            Front.AddLogData(1, Status, "UPnPによるポート閉鎖に失敗しました。(Port:" + Status.MyPort.ToString() + ")");
                            lock (Front.UPnPPort)
                            {
                                Front.UPnPPort.Remove(Status.MyPort);
                            }
                        }
                        else
                            Front.AddLogData(1, Status, "使用中ポートがあるためポート閉鎖を行いませんでした。(Port:" + Status.MyPort.ToString() + ")");
                   
                  }
                 
                }
                 */

                #endregion

                Front.AddLogData(0, Status, "エクスポートタスクを終了します");
            }
        }

        /// <summary>
        /// クライアント送信タスク
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

            sock.ReceiveTimeout = 1000;     //Export受信のタイムアウト値
            sock.SendTimeout = 500;         //Export送信のタイムアウト値

            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;

            try
            {
                Front.AddLogData(0, Status, _ip + "から接続要求");
                //Front.AddLogData(1, Status, "受付処理時間:" + sw.Elapsed.TotalMilliseconds.ToString());
                // Kick対象かチェック
                if (Status.IsKickCheck(sock) == false && !localcheck)
                {

                    Front.AddLogData(0, Status, "Kick対象のため接続を拒否します。 [" + _ip + "]");
                    #region Kick処理
                    bool _not_found = true;
                    // KickItemから該当IP検索
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
                        // 見つからなかった場合は新規追加
                        if (_not_found)
                            Status.AddKick(_ip, 1);
                    }
                    // Kick先には503を送る
                    // 相手のリクエストを受け取ってから返信
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
                            // 連打しないように応答返却を遅延させる
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            // ビジーメッセージ送信
                            string str = enc.GetString(reqBytes, 0, j);
                            Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");
                            sock.Send(enc.GetBytes(Front.BusyString));
                        }
                        else
                        {
                            // リクエストが長すぎる場合は何も送らず切る
                            // 連打しないように切断を遅延させる
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                        }
                    }
                    catch
                    {
                        // MMS(TCP)以外ならビジーメッセージ送信NG
                        if (enc.GetString(reqBytes, 12, 4) != "MMS ")
                            Front.AddLogData(1, Status, "ビジーメッセージが送信できませんでした。");
                    }
                    Status.BusyCounter++;
                    #endregion
                    sock.Close();
                    return;
                }

                //受付チェック


                _ua = AcceptUser(sock);

                //受付拒否された接続を切断
                if (_ua == "")
                {
                    sock.Close();
                    return;
                }
                //KAGAMI_LINKは何もせずに終わり
                if (_ua == "KAGAMI_LINK")
                {
                    return;
                }
                //エクスポート受付OK
                if (!localcheck)
                    Status.ExportCount++;
                //クライアントへデータ送信
                sock.SendTimeout = (int)Front.Sock.SockSendTimeout;   //Export送信のタイムアウト値
                Status.Client.Send(Status, sock, _ua);

            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "クライアント受付エラー(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")[" + _ip + ":" + _port + "]");
                sock.Close();
            }
            finally
            {

            }


        }

        /// <summary>
        /// 接続要求に来たクライアントの接続可否を判定し、
        /// 正常ならHTTPHeader/StreamHeaderを送信
        /// </summary>
        /// <param name="sock">クライアントのsocket</param>
        /// <returns>クライアントのUserAgent</returns>
        private string AcceptUser(System.Net.Sockets.Socket sock)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;
            bool localcheck = false;
            if (((IPEndPoint)sock.RemoteEndPoint).Address.ToString() == "127.0.0.1")
                localcheck = true;

            #region DenyIPチェック
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
                            Front.AddLogData(0, Status, "接続禁止IPのため拒否します [" + _ip + "]");
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            return "";


                        }
                    }
                    catch { }
                    if (_denyip.StartsWith(_ip))
                    {
                        Front.AddLogData(0, Status, "接続禁止IPのため拒否します [" + _ip + "]");
                        Thread.Sleep((int)Front.Sock.SockCloseDelay);
                        return "";
                    }
                }
            }
            #endregion
            try
            {
                #region プロトコルチェック+UserAgent取得
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

                //必要以上にガチガチに組んでますが、作者の趣味です＾＾
                //リクエストヘッダの改行コードは普通CR+LFだけど
                //WME配信簡易テストがLFのみで来るので両対応にする
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
                    // 12～16byte目に"MMS "でMMS(TCP)
                    enc = Front.GetCode(reqBytes, j);
                    if (enc.GetString(reqBytes, 12, 4).Equals("MMS "))
                    {
                        //MMSはリクエスト内容のログ出力しない
                        //NSPlayer接続に切り替えさせるため、クライアントには何も送らず切断。
                        Front.AddLogData(0, Status, "対応していないリクエスト(MMST)のため切断します [" + _ip + "]");
                    }
                    else
                    {
                        //受信できたところまでのリクエストMsgをログ出力
                        Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" +
                                     enc.GetString(reqBytes, 0, j) +
                                     "\r\nRecvReqMsg(Client)End-----");
                        if (e is SocketException)
                        {
                            SocketException se = (SocketException)e;
                            Front.AddLogData(1, Status, "リクエスト受信タイムアウト [" + _ip + "] (wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")");
                        }
                        else
                        {
                            Front.AddLogData(1, Status, "リクエスト受信タイムアウト [" + _ip + "] (" + e.Message + ")");
                        }
                    }
                    //MMS(TCP)はココでNG。または受信タイムアウト。なので、切断遅延はさせない。
                    //切断するためnull返却
                    return "";
                }

                //Front.AddLogData(1, Status, "1.1.1:" + sw.ElapsedMilliseconds.ToString());
                //sw.Reset();
                //sw.Start();
                if (i < 2)
                {
                    //受信できたところまでのリクエストMsgをログ出力
                    enc = Front.GetCode(reqBytes, j);
                    Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" +
                                 enc.GetString(reqBytes, 0, j) +
                                 "\r\nRecvReqMsg(Client)End-----");
                    Front.AddLogData(1, Status, "リクエスト長が長すぎるので切断します(over5KB) [" + _ip + "]");
                    // 連打しないように切断を遅延させる
                    Thread.Sleep((int)Front.Sock.SockCloseDelay);
                    //切断するためnull返却
                    return "";
                }

                enc = Front.GetCode(reqBytes, j);
                string str = enc.GetString(reqBytes, 0, j);
                if (str.IndexOf("\r\n") < 0)
                {
                    //改行コードがLFのみならCR+LFに置換する
                    str = str.Replace("\n", "\r\n");
                }
                if (str.IndexOf("\r\n") < 8)
                {
                    Front.AddLogData(0, Status, "対応していないリクエストのため切断します [" + _ip + "]");
                    Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");
                    //プロトコル取り出しが出来ないパターン
                    //切断するためnull返却
                    return "";
                }
                int _s = str.Split('\n')[0].LastIndexOf(' ') + 1;
                int _l = str.Split('\n')[0].LastIndexOf('/') - _s;
                string protocol = str.Substring(_s, _l);
                Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");

#region KAGAMIリンク
#if DEBUG
                if (protocol == "KAGAMI")
                {
                    sock.Send(enc.GetBytes(Front.KagamiLinkRes));
                    byte[] _byte = { 0x00, 0x08, 0x00, 0x02, 0x05, 0x00, 0x00, 0x00 };
                    sock.Send(_byte);

                    //Front.AddLogDebug("LINK", BitConverter.ToString(enc.GetBytes(Front.KagamiLinkRes)));
                    Front.AddLogData(0, Status, "鏡間リンクを開始します");
                    Status.IKLink = new KagamiLink(Status, sock);
                    return "KAGAMI_LINK";
                }
#endif
#endregion
                if (protocol != "HTTP")
                {
                    Front.AddLogData(0, Status, "対応していないリクエスト(" + protocol + ")のため切断します [" + _ip + "]");
                    //RTSPとかはココでNG。なので切断遅延はさせない。
                    //切断するためnull返却
                    return "";
                }

                // UserAgent取得
                try
                {
                    userAgent = str.Substring(str.IndexOf("User-Agent: ") + 12, str.IndexOf("\r\n", str.IndexOf("User-Agent: ")) - str.IndexOf("User-Agent: ") - 12);
                }
                catch
                {
                    Front.AddLogData(0, Status, "UserAgentが不明なため接続を拒否します [" + _ip + "]");
                    // UserAgentが無いなら処理継続出来ないので、400を送る
                    // 連打しないように応答送信を遅延させる
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
                    //切断するためnull返却
                    return "";
                }
#region DenyUAチェック

                foreach (string _denyua in Front.Acl.DenyUA)
                {
                    if (string.IsNullOrEmpty(_denyua))
                        continue;
                    try
                    {
                        Regex re = new Regex(_denyua);

                        if (re.IsMatch(userAgent))
                        {

                            Front.AddLogData(0, Status, "接続禁止UAのため拒否します [" + _ip + "/UA:" + userAgent + "]");
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            return "";

                        }
                    }
                    catch { }

                    if (userAgent.Contains(_denyua))
                    {
                        Front.AddLogData(0, Status, "接続禁止UAのため拒否します [" + _ip + "/UA:" + userAgent + "]");
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
                        Front.AddLogData(0, Status, "ホスト名が一致しないため切断します。 (Host:" + host + ")[" + _ip + "]");
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

                                Front.AddLogData(0, Status, "ホスト名が一致しないため切断します。 (Host:" + host + ")[" + _ip + "]");
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
                // 鏡ポートを取り出す
                int _idx = str.IndexOf("Pragma: kagami-port=");
                if (_idx < 0)
                {
                    priKagamiPort = 0;
                }
                else
                {
                    _idx += 20; // ポート番号先頭に移動
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
                // 鏡リンクを取り出す
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
                    // PC用ブラウザ
                    if (userAgent.IndexOf("Mozilla") == 0 ||
                        userAgent.IndexOf("Opera") == 0)
                    {
                        _browser = true;
                    }
                    // 携帯ブラウザ
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
#region Mozilla処理
                    string ackMsg = "";
                    string head = "";
                    string childMsg = "";
                    Front.AddLogData(0, Status, "Webブラウザからのアクセスです UA: " + userAgent);
                    //とりあえず。。。な検索エンジン対策。
                    if (str.IndexOf("GET /robot.txt") >= 0 ||
                        str.IndexOf("GET /robots.txt") >= 0)
                    {
                        // 検索エンジンがrobot.txtにアクセスしてきた時の応答メッセージ
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
                        Front.AddLogData(0, Status, "コネクションを切断します [" + _ip + "]");
                        //切断するためnull返却
                        return "";
                    }
                    if (Front.Opt.PriKagamin == true)
                    {
                        //かがみん優先モードの場合、子鏡情報を出力する
                        //ClientItemから鏡ポートを取り出せたクライアント一覧を出力
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
                                    // UserAgentから鏡ポート取得NG
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
                            head += "Content-Type: text/plain\r\n\r\n";   // TEXTモード or 携帯ならTEXT出力
                        else
                            head += "Content-Type: text/html\r\n\r\n";    // HTMLモード
                        while (sr.Peek() > 0)
                        {
                            string TemplatePortInfo = sr.ReadLine();
                            ackMsg += TemplatePortInfo
                                .Replace("<TIME>", Status.ImportTimeString)
                                .Replace("<SRC_URL>", (Status.UrlVisible ? Status.ImportURL : "設定が非表示になっています"))
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
                        // テンプレート読込が失敗した場合、今までどおりの形式で出力する
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
                    // ポート状態表示はShiftJISで行う。
                    enc = Encoding.GetEncoding("Shift_JIS");
                    sock.Send(enc.GetBytes(head + ackMsg));
                    Front.AddLogData(0, Status, "コネクションを切断します [" + _ip + "]");
#endregion
                    //切断するためnull返却
                    return "";
                }
                else
                {


                    // Mozilla以外なら美人チェックする
#region 美人チェック処理
                    ///自分への転送を許可しない場合はビジー応答
                    if (userAgent.Contains("PriCheck") == true && Front.Opt.NotMyTrans == true)
                    {
                        Front.AddLogData(0, Status, "転送不許可のため接続を拒否します [" + _ip + "]");
                        try
                        {
                            // ビジー応答送信
                            sock.Send(enc.GetBytes(Front.BusyString));
                        }
                        catch
                        {
                            Front.AddLogData(1, Status, "ビジーメッセージが送信できませんでした。");
                        }
                        return "";
                    }
                    // まずは、リザ接続出来るかチェック
                    // リザリストに該当IPが×状態で存在しない　または、
                    // 最大接続可能数を超えてる場合、すなわち、
                    //   現在の接続数≧(最大通常接続数＋最大リザ接続数)　なら、リザ接続不可
                    if (!Status.IsReserveList(_ip) ||
                        Status.Client.Count >= Status.Connection + Status.Reserve)
                    {
                        // リザ接続不可。通常接続できるかチェック
                        // 現在の通常接続数≧最大通常接続数　なら、通常接続不可。すなわち、
                        // 現在の総接続数－現在のリザーブ接続数≧最大通常接続数　なら、通常接続不可。
                        if (Status.Client.Count - Status.ReserveCount >= Status.Connection && !localcheck)
                        {
                            // リザ接続も通常接続も不可。

                            // 優先モードを考慮した美人判定
                            // リザ空きが無い(最大接続可能数を超えている) or
                            // かがみん接続でもkagami.exe接続でもない or
                            // かがみん接続だけどかがみん優先OFF or
                            // kagami.exe接続だけどkagami.exe優先OFF
                            if ((Status.Connection + Status.Reserve) <= Status.Client.Count ||
                                (priKagamiPort == 0 && userAgent.IndexOf("kagami/") < 0) ||
                                (priKagamiPort != 0 && Front.Opt.PriKagamin == false) ||
                                (userAgent.IndexOf("kagami/") >= 0 && Front.Opt.PriKagamiexe == false))
                            {
#region 美人処理
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
                                                Front.AddLogData(0, Status, "転送階層オーバーのため接続を拒否します [" + _ip + "]");
                                                try
                                                {
                                                    // ビジー応答送信
                                                    sock.Send(enc.GetBytes(Front.BusyString));
                                                }
                                                catch
                                                {
                                                    Front.AddLogData(1, Status, "ビジーメッセージが送信できませんでした。");
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
                                    //接続元がプライベートIPでなく、かつ、転送先がプライベートIPの場合
                                    //プライベートIPでは外からつながらないので、グローバルIPを返すようにする。
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
                                    //301 Moved Permanentlyは恒久的な移動、302 Foundは一時的
                                    //どちらもプレイヤーの挙動は同じ模様
                                    //WMP,GOM,winamp,VLCでリダイレクト確認。MPCはリダイレクトしない仕様の模様。
                                    string reqMsg = "HTTP/1.0 301 Moved Permanently\r\n" +
                                                    "Location: " + _hostport + "\r\n" +
                                                    "User-Agent: " + Front.UserAgent + "\r\n" +
                                        /*"Host: " + ((IPEndPoint)sock.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)sock.RemoteEndPoint).Port + "\r\n" +*/
                                                    "Pragma: no-cache\r\n" +
                                                    "Content-Type: application/x-mms-framed\r\n\r\n";

                                    Thread.Sleep(1000);
                                    sock.Send(enc.GetBytes(reqMsg));
                                    Front.AddLogData(1, Status, "ビジーのため接続を拒否します [" + _ip + "]/子鏡転送完了[" + _hostport + "]");
                                    Status.TransCount++;
                                    return "";
                                }
                                else
                                {
                                    Front.AddLogData(0, Status, "ビジーのため接続を拒否します [" + _ip + "]");
                                    try
                                    {
                                        bool flag = false;
                                        // 連打しないように応答送信を遅延させる
                                        // ただし、WebABCは遅延させるとTimeOut表示になるので即切りする。
                                        foreach (string tempra in Front.Acl.CheckUserAgent)
                                        {
                                            if (userAgent.Contains(tempra))
                                                flag = true;
                                        }
                                        if (!flag)
                                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                                        // ビジー応答送信
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
                                        Front.AddLogData(1, Status, "ビジーメッセージが送信できませんでした。");
                                    }
                                    Status.BusyCounter++;
#endregion
                                    //切断するためnull返却
                                    return "";
                                }
                            }

                        }
                    }
#endregion

                    // 美人じゃない
                    if (userAgent.IndexOf("NSPlayer") == 0)
                    {

                        if (Status.ExportAuth && Status.AuthID != "" && Status.AuthPass != "")
                        {
                            //認証パスが含まれていないまたは一致しない場合は401を返す
                            if (authPass == "" || authPass != Status.AuthID + ":" + Status.AuthPass)
                            {
                                Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                    (Front.AuthString()) +
                                    "\r\n\n\n" + "\r\nSendRspMsg(Client)End-----");
                                sock.Send(enc.GetBytes(Front.AuthString()));
                                if (authPass == "")
                                    Front.AddLogData(1, Status, "認証要求メッセージを送信します [" + _ip + "]");
                                else
                                    Front.AddLogData(1, Status, "認証NGのため切断します[" + _ip + "/" + authPass + "])");

                                return "";
                            }
                            else
                            {
                                Front.AddLogData(1, Status, "認証完了しました[" + _ip + "]");

                            }

                        }

                        if (str.IndexOf("x-mms-framed") > 0 || str.IndexOf("stream-switch") > 0 || Status.Type == 2)
                        {
#region NSPlayerヘッダ送信＋接続持続
                            try
                            {
                                // 応答ヘッダ送信
                                /* かがみんの応答は必ずHTTP/1.0
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
                                // ヘッダストリーム送信


                                sock.Send(Status.HeadStream);
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.Message);
                                Front.AddLogData(1, Status, "ヘッダ送信NG/コネクションを切断します(理由:" + ex.Message + ") [" + _ip + "]");
                                return "";
                            }
#endregion
                            //接続保持のためUserAgent返却
                            //子鏡接続の場合、ポート番号を付加したUAを返却
                            //KagamiLinkが0以上の場合は鏡リンクも付加
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
                        // x-mms-framedまたはstream-switchヘッダが無い場合、
                        // ヘッダのみ送ってすぐに切断する
                        else
                        {
#region NSPlayerヘッダ送信＋接続終了
                            //Front.AddLogData(Status, "ヘッダ送信＋終了＋1.1");
                            Front.AddLogData(0, Status, "ヘッダを取得するための接続 UA: " + userAgent);
                            try
                            {
                                // 応答ヘッダ送信
                                /* かがみんの応答は必ずHTTP/1.0
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
                                // ヘッダストリーム送信
                                sock.Send(Status.HeadStream);
                            }
                            catch { }

                            Front.AddLogData(0, Status, "コネクションを切断します [" + _ip + "]");
#endregion
                            //切断するためnull返却
                            return "";

                        }
                    }
                    else
                    {

                        //WMP12用
                        //サーバー名は固定の必要あり(Kagami～～だとサーバーエラーになる)
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

                        //NSPlayer以外での接続
                        //ここではHTTPエラー応答を返してはいけない。何も返さずに切断することで、
                        //クライアント側はUserAgentをNSPlayerに切り替えて再接続してくる。
                        Front.AddLogData(0, Status, "NSPlayer以外での接続です UA: " + userAgent);
                        Front.AddLogData(0, Status, "コネクションを切断します [" + _ip + "]");
                        //即再接続させるため、切断遅延は行わない。
                        //切断するためnull返却
                        return "";
                    }
                }
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "クライアント受付エラー(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")[" + _ip + "]");
                return "";
            }
            finally
            {

            }
            // ここには来ない
            //return "";
        }
        /// <summary>
        /// ビジー時子鏡転送チェック
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
                        //接続時間10sec以内のクライアントはスルー
                        if (Status.Gui.ClientItem[i].SubItems[3].Text.StartsWith("00:00:") &&
                            int.Parse(Status.Gui.ClientItem[i].SubItems[3].Text.Substring(6, 2)) <= 10)
                            continue;

                        _tmpHost = Status.Gui.ClientItem[i].SubItems[1].Text;               // SubItems[1]:clmClientViewIP
                        _tmpPort = int.Parse(Status.Gui.ClientItem[i].SubItems[2].Text.Substring(Status.Gui.ClientItem[i].SubItems[2].Text.IndexOf("/Port=") + 6)); // clmClientViewUA

                    }
                    catch
                    {
                        // UserAgentから鏡ポート取得NG
                        continue;
                    }
                    try
                    {
                        //kgm_port = int.Parse(UserAgent.Substring(UserAgent.IndexOf("/Port=") + 6));

                        Front.AddLogData(0, Status, "子鏡転送チェックを開始します。/転送深さ=" + _cont.ToString() + " /[" + _tmpHost + ":" + _tmpPort + "]");

                        // 子鏡ポートチェック用Socketの作成
                        IPAddress hostadd = Dns.GetHostAddresses(_tmpHost)[0];
                        IPEndPoint ephost = new IPEndPoint(hostadd, _tmpPort);

                        sock_chk.SendTimeout = (int)Front.Sock.SockConnTimeout;       // Import接続 ヘッダ取得要求送信のタイムアウト値
                        sock_chk.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    // Import接続 ヘッダ取得応答受信のタイムアウト値

                        // 接続
                        sock_chk.Connect(ephost);

                        // 子鏡に偽装したリクエストを送信
                        // クライアントに偽装したヘッダを送信
                        string reqMsg = "GET / HTTP/1.1\r\n" +
                            "Accept: */*\r\n" +
                            "User-Agent: " + Front.UserAgent + "/PriCheck" + _cont.ToString() + "\r\n" +
                            "Host: " + ((IPEndPoint)sock_chk.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)sock_chk.RemoteEndPoint).Port + "\r\n" +
                            "Pragma: no-cache\r\n" +
                            "Content-Type: application/x-mms-framed\r\n\r\n";

                        System.Text.Encoding enc = System.Text.Encoding.ASCII; // "euc-jp"
                        byte[] reqBytes = enc.GetBytes(reqMsg);
                        sock_chk.Send(reqBytes, reqBytes.Length, System.Net.Sockets.SocketFlags.None);

                        // 応答ヘッダ受信
                        byte[] tmp = new byte[9];
                        byte[] tmp2 = new byte[3];
                        sock_chk.Receive(tmp);  // "HTTP/1.x " まで受信
                        sock_chk.Receive(tmp2); // HTTP StatusCodeの3byte受信
                        int http_status = int.Parse(System.Text.Encoding.ASCII.GetString(tmp2));
                        //httpステータスが200以外
                        if (http_status != 200)
                        {
                            //子鏡でリダイレクトが帰ってきた場合そこのホストを記録
                            if (http_status == 301 || http_status == 302)
                            {
                                _firsthost = "http://" + _tmpHost + ":" + _tmpPort;
                                notflag = true;
                                sock_chk.Close();
                                Front.AddLogData(1, Status, "子鏡転送チェック再転送OK[" + _tmpHost + ":" + _tmpPort + "]");
                                continue;

                            }
                            //ビジーならそのまま
                            else
                            {
                                sock_chk.Close();
                                Front.AddLogData(1, Status, "子鏡転送チェックNG[" + _tmpHost + ":" + _tmpPort + "]");
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
                        Front.AddLogData(1, Status, "子鏡転送チェックOK [" + _tmpHost + ":" + _tmpPort + "]");
                        //空いてる子鏡を返す
                        return "http://" + _tmpHost + ":" + _tmpPort;
                    }
                    catch
                    {
                        sock_chk.Close();
                        Front.AddLogData(1, Status, "子鏡転送チェックNG[" + _tmpHost + ":" + _tmpPort + "]");
                    }
                }
            }
            //空いてない場合301を返した子鏡を返す
            if (notflag == true)
                return _firsthost;
            else
                //301もない場合null。ビジーメッセージ送信
                return null;
        }
    }
}
