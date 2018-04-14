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

                TcpListener _listener = TcpListener.Create(Status.MyPort);
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
                        //リスナーで待機しているクライアントをすべて切断する
                        while (_listener.Pending())
                        {
                            TcpClient _sock = _listener.AcceptTcpClient();
                            _sock.Close();
                        }
                    }
                    catch { }
                    // Listen停止
                    _listener.Stop();
                    Status.ListenPort = false;
                }
                Front.AddLogData(0, Status, "エクスポートタスクを終了します");
            }
        }

        /// <summary>
        /// クライアント送信タスク
        /// </summary>
        /// <param name="obj"></param>
        private void ClientTask(object obj)
        {
            Socket sock = (Socket)obj;
            string _ua = "";

            sock.ReceiveTimeout = 1000;     //Export受信のタイムアウト値
            sock.SendTimeout = 500;         //Export送信のタイムアウト値

            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;

            try
            {
                Front.AddLogData(0, Status, _ip + "から接続要求");
                // Kick対象かチェック
                if (Status.IsKickCheck(sock) == false)
                {
                    Front.AddLogData(0, Status, "Kick対象のため接続を拒否します。 [" + _ip + "]");
                    #region Kick処理
                    bool _not_found = true;
                    // KickItemから該当IP検索
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
                        // 見つからなかった場合は新規追加
                        if (_not_found)
                            Status.AddKick(_ip,1);
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
                            sock.Receive(reqBytes, j, 1, SocketFlags.None);
                            if (reqBytes[j] == '\r') continue;
                            if (reqBytes[j] == end[i]) i++; else i = 0;
                            if (i >= 2) break;
                        }
                        if (i >= 2)
                        {
                            // 連打しないように応答返却を遅延させる
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            // 相手からの受信データ保存
                            string str = enc.GetString(reqBytes, 0, j);
                            Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");
                            // ビジーメッセージ送信
                            Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" + Front.BusyString + "\r\nSendRspMsg(Client)End-----");
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
                            Front.AddLogData(1, Status, "ビジーメッセージが送信できませんでした [" + _ip + "]");
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

                //エクスポート受付OK
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

        }

        /// <summary>
        /// 接続要求に来たクライアントの接続可否を判定し、
        /// 正常ならHTTPHeader/StreamHeaderを送信
        /// </summary>
        /// <param name="sock">クライアントのsocket</param>
        /// <returns>クライアントのUserAgent</returns>
        private string AcceptUser(Socket sock)
        {
            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;

            try
            {
                #region プロトコルチェック+UserAgent取得
                string userAgent;
                int priKagamiPort;
                Encoding enc;

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
                        sock.Receive(reqBytes, j, 1, SocketFlags.None);
                        if (reqBytes[j] == '\r') continue;
                        if (reqBytes[j] == end[i]) i++; else i = 0;
                        if (i >= 2) break;
                    }
                }
                catch(Exception e)
                {
                    // MMS(TCP)Check
                    // 12～16byte目に"MMS "でMMS(TCP)
                    enc = Front.GetCode(reqBytes,j);
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
                            Front.AddLogData(1, Status, "リクエスト受信タイムアウト [" + _ip + "] wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString());
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
                if (i < 2)
                {
                    //受信できたところまでのリクエストMsgをログ出力
                    if (j > 0)
                    {
                        enc = Front.GetCode(reqBytes, j);
                        Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" +
                                     enc.GetString(reqBytes, 0, j) +
                                     "\r\nRecvReqMsg(Client)End-----");
                    }
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

                string protocol = str.Substring(str.IndexOf("\r\n") - 8, 4);
                Front.AddLogDetail("RecvReqMsg(Client)Sta-----\r\n" + str + "\r\nRecvReqMsg(Client)End-----");

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
                if(_browser||_mobile)
                {
                    #region Mozilla処理
                    string ackMsg = "";
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
                                    if (!Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.Contains("/Port="))
                                        continue;
                                    _tmpHost = Status.Gui.ClientItem[i].SubItems[Front.clmCV_IP_IDX].Text;
                                    _tmpPort = int.Parse(Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.Substring(Status.Gui.ClientItem[i].SubItems[Front.clmCV_UA_IDX].Text.IndexOf("/Port=") + 6));
                                }
                                catch
                                {
                                    // UserAgentから鏡ポート取得NG
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
                            ackMsg += "Content-Type: text/plain\r\n\r\n";   // TEXTモード or 携帯ならTEXT出力
                        else
                            ackMsg += "Content-Type: text/html\r\n\r\n";    // HTMLモード
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
                    sock.Send(enc.GetBytes(ackMsg));
                    Front.AddLogData(0, Status, "コネクションを切断します [" + _ip + "]");
                    #endregion
                    //切断するためnull返却
                    return "";
                }
                else
                {
                    // Mozilla以外なら美人チェックする
                    #region 美人チェック処理
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
                        if (Status.Client.Count - Status.ReserveCount >= Status.Connection)
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
                                #region 美人・リダイレクト処理
                                try
                                {
                                    bool busy_flg = true;   // Busy未送信
                                    // WebABCの場合はリダイレクトも切断遅延もせず即切りする。
                                    if (userAgent.Contains("WebABC"))
                                    {
                                        Front.AddLogData(0, Status, "ビジーのため接続を拒否します [" + _ip + "]");
                                        Front.AddLogDetail(
                                            "SendRspMsg(Client)Sta-----\r\n" +
                                            Front.BusyString +
                                            "\r\nSendRspMsg(Client)End-----"); 
                                        sock.Send(enc.GetBytes(Front.BusyString));
                                        busy_flg = false;   // Busy送信済み
                                    }
                                    //子鏡へのリダイレクトを行うとき
                                    if (busy_flg && Status.EnableRedirectChild)
                                    {
                                        //子鏡情報収集
                                        //プライベートIPが返ることは無いのでチェック不要
                                        string _redirUrl = Status.Client.GetKagamiList();
                                        //子鏡情報が有ればリダイレクト
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
                                            Front.AddLogData(0, Status, "ビジーのため接続を拒否します [" + _ip + "] / 子リダイレクト応答 URL=" + _redirUrl);
                                            Front.AddLogDetail(
                                                "SendRspMsg(Client)Sta-----\r\n" +
                                                _redirStr +
                                                "\r\nSendRspMsg(Client)End-----");
                                            // 転送合戦にならないよう302応答送信を遅延させる
                                            Thread.Sleep(1000);
                                            sock.Send(enc.GetBytes(_redirStr));
                                            busy_flg = false;   // Redirect送信済み
                                        }
                                    }
                                    //親へのリダイレクトを行うとき
                                    if (busy_flg && Status.EnableRedirectParent && Status.Type != 2)
                                    {
                                        if (Status.ImportHost == "::1" ||               // IPv6 LoopBack
                                            Status.ImportHost.StartsWith("10.") ||      // ClassA
                                            Status.ImportHost.StartsWith("172.16.") ||  // ClassB
                                            Status.ImportHost.StartsWith("192.168.") || // ClassC
                                            Status.ImportHost.StartsWith("127."))       // LoopBack
                                        {
                                            // 親がローカルアドレスなら転送Skip
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
                                            Front.AddLogData(0, Status, "ビジーのため接続を拒否します [" + _ip + "] / 親リダイレクト応答 URL=" + _redirUrl);
                                            Front.AddLogDetail(
                                                "SendRspMsg(Client)Sta-----\r\n" +
                                                _redirStr +
                                                "\r\nSendRspMsg(Client)End-----");
                                            // 転送合戦にならないよう302応答送信を遅延させる
                                            Thread.Sleep(1000);
                                            sock.Send(enc.GetBytes(_redirStr));
                                            busy_flg = false;   // Redirect送信済み
                                        }
                                    }
                                    if (busy_flg)
                                    {
                                        Front.AddLogData(0, Status, "ビジーのため接続を拒否します [" + _ip + "]");
                                        Front.AddLogDetail(
                                            "SendRspMsg(Client)Sta-----\r\n" +
                                            Front.BusyString +
                                            "\r\nSendRspMsg(Client)End-----");
                                        // 連打しないように応答送信を遅延させる
                                        Thread.Sleep((int)Front.Sock.SockCloseDelay);
                                        // ビジー応答送信
                                        sock.Send(enc.GetBytes(Front.BusyString));
                                    }
                                }
                                catch
                                {
                                    Front.AddLogData(1, Status, "ビジーメッセージが送信できませんでした [" + _ip + "]");
                                }
                                Status.BusyCounter++;
                                #endregion
                                //切断するためnull返却
                                return "";
                            }
                        }
                    }
                    #endregion

                    // 美人じゃない
                    if (userAgent.IndexOf("NSPlayer") == 0 || userAgent.IndexOf("Windows-Media-Player") == 0)
                    {
                        if (str.IndexOf("x-mms-framed") > 0 || str.IndexOf("stream-switch") > 0)
                        {
                            #region NSPlayerヘッダ送信＋接続持続
                            try
                            {
                                // 応答ヘッダ送信
                                Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                                            enc.GetString(Status.DataRspMsg10) +
                                                            "\r\nSendRspMsg(Client)End-----");
                                sock.Send(Status.DataRspMsg10);
                                // ヘッダストリーム送信
                                sock.Send(Status.HeadStream);
                            }
                            catch
                            {
                                Front.AddLogData(1, Status, "ヘッダ送信NG/コネクションを切断します [" + _ip + "]");
                                return "";
                            }
                            #endregion
                            //接続保持のためUserAgent返却
                            //子鏡接続の場合、ポート番号を付加したUAを返却
                            if (priKagamiPort != 0)
                                return userAgent + "/Port=" + priKagamiPort;
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
                                Front.AddLogDetail("SendRspMsg(Client)Sta-----\r\n" +
                                                            enc.GetString(Status.HeadRspMsg10) +
                                                            "\r\nSendRspMsg(Client)End-----");
                                sock.Send(Status.HeadRspMsg10);
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
            // ここには来ない
            //return "";
        }


    }
}
