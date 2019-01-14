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
    /// クライアント全体を管理するクラス
    /// </summary>
    public class Client
    {
        #region メンバ変数
        /// <summary>
        /// 鏡ステータス
        /// </summary>
        private Status Status;
        /// <summary>
        /// クライアントリスト
        /// </summary>
        private List<ClientData> ClientList = new List<ClientData>();
        /// <summary>
        /// クライアント管理ID
        /// </summary>
        private List<int> ClientID = new List<int>();
        /// <summary>
        /// 接続中クライアント数
        /// </summary>
        public int Count
        {
            get { return ClientList.Count; }
        }
        #endregion

        #region コンストラクタ
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
        /// クライアントへのデータ送信
        /// </summary>
        /// <param name="_status"></param>
        /// <param name="_sock"></param>
        /// <param name="_ua"></param>
        public void Send(Status _status, Socket _sock, string _ua)
        {
            //クライアントに管理IDを割り当てる
            int cnt;
            ClientData cd;
            try
            {
                // 同一ID二重捕捉を防ぐためlock
                lock (ClientID)
                {
                    //空きID検索
                    for (cnt = 0; cnt < ClientID.Count; cnt++)
                        if (cnt != ClientID[cnt])
                            break;
                    ClientID.Insert(cnt, cnt);
                }
            }
            catch
            { return; }

            //クライアント送信クラス生成
            try
            {
                //クライアントへデータ送信
                cd = new ClientData(_status, cnt.ToString("D3"), _sock, _ua);
            }
            catch
            {
                // クライアントID解放
                ClientID.Remove(cnt);
                return;
            }

            try
            {
                // 内部管理用ClientListに追加
                lock (ClientList)
                    ClientList.Add(cd);
                // ClientItemにデータ追加＋GUI更新
                Status.AddClient(cd);
                // クライアントへ送信開始
                cd.Send();
            }
            catch { }
            finally
            {
                // ClientItemから削除＋GUI更新
                Status.RemoveClient(cnt.ToString("D3"));
                // 内部管理用ClientListから削除
                lock (ClientList)
                    ClientList.Remove(cd);
                // 積極的メモリ解放
                cd = null;
                // クライアントID解放
                ClientID.Remove(cnt);
                // 念のため
                _sock.Close();
            }
        }

        /// <summary>
        /// クライアント強制切断
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
        /// ストリーム情報の書き込み
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
                            Front.AddLogData(1, Status, "ストリーム書き込みNG [" + cd.Ip + "] / Q:" + cd.StreamQueue.Count);
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// ClientItem上の状態更新
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
        /// KickItem上のクライアントキック時間更新
        /// </summary>
        public void UpdateKickTime()
        {
            lock (Status.Kick)
            lock (Status.Gui.KickItem)
            {
                foreach (System.Windows.Forms.ListViewItem _item in Status.Gui.KickItem)
                {
                    // Kick状態＆解除時間を更新

                    if (!Status.Kick.ContainsKey(_item.Text))
                    {//GUIには登録されているが内部管理には登録されていない場合
                     //通常はこない
                        continue;
                    }

                    
                    //接続試行回数
                    _item.SubItems[2].Text = Status.Kick[_item.Text].Cnt_out.ToString();
                    
                    //状態
                    if (Status.Kick[_item.Text].DenyEndTime == -1)
                    {//無制限の場合
                        _item.SubItems[1].Text = "規制中/無制限";    // clmKickViewState
                        _item.SubItems[0].ForeColor = System.Drawing.Color.Red;
                    }
                    else if (Status.Kick[_item.Text].KickFlag &&
                        Status.Kick[_item.Text].DenyTime.AddSeconds(Status.Kick[_item.Text].DenyEndTime) > DateTime.Now)
                    {//キック中の場合
                        TimeSpan _duration = Status.Kick[_item.Text].DenyTime.AddSeconds(Status.Kick[_item.Text].DenyEndTime) - DateTime.Now;
                        _item.SubItems[1].Text = "規制中/解除まで" + (long)_duration.TotalSeconds + "秒";    // clmKickViewState
                        _item.SubItems[0].ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {//キック中でない場合
#if DEBUG
                        Front.AddKickLog(Status, "UpdateKickTime1");
#endif
                        Status.Kick[_item.Text].KickFlag = false;
                        _item.SubItems[1].Text = "解除中";                                                  // clmKickViewState
                        _item.SubItems[0].ForeColor = System.Drawing.Color.Empty;
                    }
                }
            }
        }
    }

    /// <summary>
    /// １クライアント分のデータを管理するクラス
    /// </summary>
    public class ClientData
    {
        #region メンバ変数
        /// <summary>
        /// クライアントID
        /// </summary>
        public string Id;
        /// <summary>
        /// クライアントIP
        /// </summary>
        public string Ip;
        /// <summary>
        /// 子鏡ポート番号
        /// </summary>
        public int KagamiPort;
        /// <summary>
        /// 子鏡リンクレベル
        /// </summary>
        public int KagamiLink;
        /// <summary>
        /// リモートホスト
        /// </summary>
        public string Host;
        /// <summary>
        /// クライアントUserAgent
        /// </summary>
        public string UserAgent;
        /// <summary>
        /// ストリーム送信キュー
        /// </summary>
        public Queue<byte[]> StreamQueue = new Queue<byte[]>((int)Front.Sock.SockSendQueueSize);
        /// <summary>
        /// 子鏡の人数情報
        /// </summary>
        public string ConnInfo;
        /// <summary>
        /// 鏡間リンク管理クラス
        /// </summary>
        public KagamiLink KLink;
        /// <summary>
        /// クライアント接続時間
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
        /// クライアント生存フラグ
        /// </summary>
        public bool IsAlive = false;
        /// <summary>
        /// クライアント管理元ステータス
        /// </summary>
        private Status Status;
        /// <summary>
        /// クライアントソケット
        /// </summary>
        private Socket Sock;
        /// <summary>
        /// クライアント接続時刻
        /// </summary>
        private DateTime ClientStartTime;
        /// <summary>
        /// 次子鏡チェック時間
        /// </summary>
        private DateTime? KagamiCheck = null;
        /// <summary>
        /// 子鏡接続確認OKフラグ
        /// </summary>
        public bool KagamiOK;
        /// <summary>
        /// 子鏡チェック中フラグ
        /// </summary>
        private bool KagamiCheking = false;
        #endregion

        #region コンストラクタ
        public ClientData(Status _status, string _id, Socket _sock, string _ua)
        {
            ConnInfo = "";
            Status = _status;
            Id = _id;
            Sock = _sock;
            UserAgent = _ua;
            ClientStartTime = DateTime.Now;
            Ip = ((IPEndPoint)_sock.RemoteEndPoint).Address.ToString();

            #region 鏡ポート抽出
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
            #region 鏡リンク抽出
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
                        Front.AddLogData(0, Status, "[" + Id + "] 禁止ホストのためクライアントを拒否します [" + Host + "/Match:" + _host + "]");
                        Sock.Close();
                        return;
                    }
                }
                catch
                {
                }

                if (Host.Contains(_host))
                {
                    Front.AddLogData(0, Status, "[" + Id + "] 禁止ホストのためクライアントを拒否します [" + Host + "/Match:" + _host + "]");
                    Sock.Close();
                    return;
                }

            }
        }
        #endregion

        /// <summary>
        /// クライアントへデータ送信
        /// </summary>
        public void Send()
        {
            System.Collections.ArrayList _red = new System.Collections.ArrayList();
            System.Collections.ArrayList _wrt = new System.Collections.ArrayList();
            System.Collections.ArrayList _err = new System.Collections.ArrayList();

            #region 子鏡接続チェックのスレッド生成
            // 鏡ポートがわかるなら一律チェックスレッド生成
            //if (Status.Client.Count >= Status.Connection)
            //if (Front.Opt.PriKagamin == true && KagamiPort != 0)
            if (KagamiPort != 0)
            {
#if !DEBUG
                
                // ローカルアドレスの子鏡は除外する
                if (Ip.StartsWith("10.") ||       // ClassA
                    Ip.StartsWith("192.168.") ||  // ClassC
                    Ip.StartsWith("127."))        // LoopBack
                {
                    // 除外
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
            #region 同一IP接続チェックのスレッド生成
            // 同一IP接続は通常でも(即切断するが)起こりうる
            // なので別スレッドを作って設定秒後にチェックして複数接続したままなら切断することにする
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
                    // 接続数が指定値を超えているのでチェックスレッド生成
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

            Front.AddLogData(0, Status, "[" + Id + "] クライアントタスクを開始します");
            Front.AddLogData(0, Status, "[" + Id + "] User-Agent: " + UserAgent);
            //Front.AddLogData(0, Status, "[" + Id + "] Remote-Host: " + Host);
            IsAlive = true;
            while (true)
            {
                try
                {
                    if (Status.RunStatus == false || Status.ImportStatus == false)
                    {
                        // インポート側が切れた時
                        // ログ表示は面倒なので行わない
                        break;
                    }

                    _red.Clear(); _red.Add(Sock);
                    _wrt.Clear(); _wrt.Add(Sock);
                    _err.Clear(); _err.Add(Sock);
                    Socket.Select(_red, null, _err, 30);

                    // 受信データあり
                    // ストリーミング開始後の場合、受信データあり＝クライアント自己切断を示す
                    if (_red.Count > 0)
                    {
                        // 一応サイズの０チェック
                        if (Sock.Available == 0)
                        {
                            // 正常切断
                            Front.AddLogData(0, Status, "[" + Id + "] コネクションは切断されました(正常終了) [" + Ip + "]");
                            break;
                        }
                    }

                    // ソケットエラーはメッセージだけ出して無視
                    if (_err.Count > 0)
                        Front.AddLogData(1, Status, "[" + Id + "] ソケットエラー発生 [" + Ip + "]");

                    // 送信データが無ければ30ms waitしてcontinue
                    if (StreamQueue.Count == 0)
                    {
                        Thread.Sleep(30);
                        continue;
                    }

                    // 送信データ有るばあい、バッファ溢れチェック
                    if (Front.Sock.SockSendQueueSize < StreamQueue.Count)
                    {
                        Front.AddLogData(1, Status, "[" + Id + "] 書き込みバッファが溢れたため切断します [" + Ip + "] Q:" + Front.Sock.SockSendQueueSize);
                        Status.ExportError++;
                        break;
                    }
                    if (Status.Kick.ContainsKey(Ip))
                    {
                        if (Status.Kick[Ip].KickFlag)
                        {
                            Front.AddLogData(0, Status, "Kick対象になったため強制切断します。 [" + Ip + "]");
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
                    //強制切断ならエラーメッセージを変える
                    if (se.SocketErrorCode == SocketError.NotSocket)
                    {
                        Front.AddLogData(1, Status, "[" + Id + "] コネクションを強制切断しました [" + Ip + "]");
                    }
                    else
                    {
                        Front.AddLogData(1, Status, "[" + Id + "] コネクションは切断されました [" + Ip + "] wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString());
                        Status.ExportError++;
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (Sock == null || !Sock.Connected)
                    {
                        //他スレッドから切断されたとき
                        //内部エラーにはせずに切断
                        Front.AddLogData(1, Status, "[" + Id + "] コネクションを強制切断しました [" + Ip + "]");
                    }
                    else
                    {
                        // それ以外のエラー
                        Front.AddLogData(1, Status, "[" + Id + "] クライアント送信エラー(内部エラー:" + e.Message + "/Type:" + e.GetType().ToString() + "/Trace:" + e.StackTrace + ")");
                        Status.ExportError++;
                    }
                    break;
                }
            }

            Sock.Close();
            Front.AddLogData(0, Status, "[" + Id + "] クライアントタスクを終了します");
        }

        /// <summary>
        /// クライアント強制切断
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
        /// 子鏡接続をチェックするスレッド
        /// </summary>
        private void KagaminCheck(object sender)
        {

            KagamiCheking = true;
            Socket sock_chk = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bool _busy = false;
            // 10秒待ってからチェック開始
            Thread.Sleep(10000);

            if (Sock == null)
                return;
            // 10秒以内に接続が切れていた場合終了
            if (Sock.Connected == false)
            {
                return;
            }

            try
            {
                Front.AddLogData(0, Status, "子鏡接続チェックを開始します [" + Ip + ":" + KagamiPort + "]");

                // 子鏡ポートチェック用Socketの作成
                IPAddress hostadd = Dns.GetHostAddresses(Ip)[0];
                IPEndPoint ephost = new IPEndPoint(hostadd, KagamiPort);

                sock_chk.SendTimeout = (int)Front.Sock.SockConnTimeout * 3;       // Import接続 ヘッダ取得要求送信のタイムアウト値
                sock_chk.ReceiveTimeout = (int)Front.Sock.SockConnTimeout * 3;    // Import接続 ヘッダ取得応答受信のタイムアウト値

                // 接続
                sock_chk.Connect(ephost);

                // 子鏡に偽装したリクエストを送信
                // クライアントに偽装したヘッダを送信
                string reqMsg = "GET / HTTP/1.1\r\n" +
                    "Accept: */*\r\n" +
                    "User-Agent: " + Front.UserAgent + "/WebABC.ConnCheck \r\n" +
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

                // StatusCode:200以外はNG
                // 10秒後にチェックして美人は普通ありえないでしょう。。
                //初回検査時はKagamiCheckがnull。初回以外はビジーでもきらない
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

                    // HTTP応答ヘッダの終わりを検索
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
                {//ビジーの場合に人数情報が送られてこない場合前の情報から補正する(kagami/0.76+)
                    try
                    {
                        ConnInfo = ConnInfo.Split('/')[1].Split('+')[0] + "/" + ConnInfo.Split('/')[1];
                    }
                    catch
                    {
                        ConnInfo = "Busy";
                    }
                }


                // Contents-TypeのチェックもOKなので大丈夫でしょう。
                // コネクションを切らずに終了
                sock_chk.Close();

                //次回チェック時間。予測できないよう＆一斉にチェックにならないようにある程度ランダムに
                Random rd = new Random();
#if !DEBUG
                KagamiCheck = DateTime.Now.AddSeconds(rd.Next(60 * 3, 60 * 10));//3min～10min
#else
                KagamiCheck = DateTime.Now.AddSeconds(rd.Next(30, 90));//30sec～90sec
#endif


                if (string.IsNullOrEmpty(ConnInfo))
                    Front.AddLogData(0, Status, "子鏡接続チェックOK/次チェック" + ((TimeSpan)(KagamiCheck - DateTime.Now)).TotalSeconds.ToString("F0") + "秒後 [" + Ip + ":" + KagamiPort + "]");
                else
                    Front.AddLogData(0, Status, "子鏡接続チェックOK/次チェック" + ((TimeSpan)(KagamiCheck - DateTime.Now)).TotalSeconds.ToString("F0") + "秒後 [" + Ip + ":" + KagamiPort + " Con:" + ConnInfo + "]");
                KagamiOK = true;    // 子鏡転送対象にする


            }
            catch
            {
                // かがみん優先ONの場合 NGなら接続を切る
                // かがみん優先OFFの場合 NGでも切らないが、子鏡転送は行わない
                if (Front.Opt.PriKagamin)
                {
                    Sock.Close();
                    Front.AddLogData(1, Status, "子鏡接続チェックNGのため切断します [[" + Id + "] " + Ip + "]");
                }
                else
                {
                    //次回チェック時間。予測できないよう＆一斉にチェックにならないようにある程度ランダムに
                    //NGの場合は次チェックは長めに
                    Random rd = new Random();
#if !DEBUG
                KagamiCheck = DateTime.Now.AddSeconds(rd.Next(60 * 10, 60 * 20));//10min～20min
#else
                    KagamiCheck = DateTime.Now.AddSeconds(rd.Next(30, 90));//30sec～90sec
#endif

                    Front.AddLogData(1, Status, "子鏡接続チェックNG/次チェック" + ((TimeSpan)(KagamiCheck - DateTime.Now)).TotalSeconds.ToString("F0") + "秒後 [[" + Id + "] " + Ip + "]");
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
        /// 同一IP接続数を監視して
        /// 制限数を超えていたら切断する
        /// </summary>
        private void SameIPCheck(object sender)
        {
            // 3秒待つ
            Thread.Sleep(3000);
            // 3秒以内に接続が切れていたら終了
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
                Front.AddLogData(1, Status, "同一IPの同時接続超過のため切断します [[" + Id + "] " + Ip + "]");
                Sock.Close();
            }
        }
    }
}
