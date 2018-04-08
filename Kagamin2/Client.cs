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
                        catch (Exception)
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
        /// ClientItem上のクライアント接続時間更新
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
        /// KickItem上のクライアントキック時間更新
        /// </summary>
        public void UpdateKickTime()
        {
            lock (Front.KickList)
            lock (Status.Gui.KickItem)
            {
                foreach (ListViewItem _item in Status.Gui.KickItem)
                {
                    // Kick状態＆解除時間を更新
                    string[] str = Front.KickList[_item.Text].Split(',');
                    DateTime _now_tim = DateTime.Now;
                    DateTime _end_tim = DateTime.Parse(str[0]);
                    int con_cnt = int.Parse(str[1]);
                    if (con_cnt == 0 && _end_tim > _now_tim)
                    {
                        TimeSpan _duration = _end_tim - _now_tim;
                        _item.SubItems[1].Text = "規制中/解除まで" + (long)_duration.TotalSeconds + "秒";    // clmKickViewState
                        _item.SubItems[0].ForeColor = Color.Red;
                    }
                    else if (con_cnt < 0)
                    {
                        _item.SubItems[1].Text = "規制中/無期限";   // clmKickViewState
                        _item.SubItems[0].ForeColor = Color.Red;
                    }
                    else
                    {
                        _item.SubItems[1].Text = "解除中";                                                  // clmKickViewState
                        _item.SubItems[0].ForeColor = Color.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// 子鏡接続の中からランダムに1つを返す
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
                        // "http://host名:port番号" で詰める
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
        /// クライアントUserAgent
        /// </summary>
        public string UserAgent;
        /// <summary>
        /// 子鏡ポート番号
        /// </summary>
        public int KagamiPort;
        /// <summary>
        /// 子鏡接続確認OKフラグ
        /// </summary>
        public bool KagamiOK;
        /// <summary>
        /// ストリーム送信キュー
        /// </summary>
        public Queue<byte[]> StreamQueue = new Queue<byte[]>((int)Front.Sock.SockSendQueueSize);
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

        #endregion

        #region コンストラクタ
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
        /// クライアントへデータ送信
        /// </summary>
        public void Send()
        {
            ArrayList _red = new ArrayList();
            ArrayList _wrt = new ArrayList();
            ArrayList _err = new ArrayList();

            #region 子鏡接続チェックのスレッド生成
            // 鏡ポートがわかるなら一律チェックスレッド生成
            //if (Status.Client.Count >= Status.Connection)
            //if (Front.Opt.PriKagamin == true && KagamiPort != 0)
            if (KagamiPort != 0)
            {
#if !DEBUG
                // ローカルアドレスの子鏡は除外する
                if (Ip == "::1" ||                // IPv6 LoopBack
                    Ip.StartsWith("10.") ||       // ClassA
                    Ip.StartsWith("172.16.") ||   // ClassB
                    Ip.StartsWith("192.168.") ||  // ClassC
                    Ip.StartsWith("127."))        // LoopBack
                {
                    // 除外
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
                        Thread check = new Thread(new ThreadStart(SameIPCheck));
                        check.Name = "SameIPCheck";
                        check.Start();
                    }
                }
            }
            #endregion

            Front.AddLogData(0, Status, "[" + Id + "] クライアントタスクを開始します");
            Front.AddLogData(0, Status, "[" + Id + "] User-Agent: " + UserAgent);
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
                    if (Sock == null || !Sock.Connected)
                    {
                        // 外部スレッドから強制切断されていた時
                        throw new Exception();
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

                    byte[] buf = StreamQueue.Dequeue();
                    if (buf != null)
                        Sock.Send(buf);
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
            IsAlive = false;
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
        private void KagaminCheck()
        {
            Socket sock_chk = new Socket(Socket.OSSupportsIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 10秒待ってからチェック開始
            Thread.Sleep(10000);

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

                sock_chk.SendTimeout = (int)Front.Sock.SockConnTimeout;       // Import接続 ヘッダ取得要求送信のタイムアウト値
                sock_chk.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    // Import接続 ヘッダ取得応答受信のタイムアウト値

                // 接続
                sock_chk.Connect(ephost);

                // 子鏡に偽装したリクエストを送信
                // クライアントに偽装したヘッダを送信
                string reqMsg = "GET / HTTP/1.1\r\n" +
                    "Accept: */*\r\n" +
                    "User-Agent: " + Front.UserAgent + "/ConnCheck\r\n" +
                    "Host: " + ((IPEndPoint)sock_chk.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)sock_chk.RemoteEndPoint).Port + "\r\n" +
                    "Pragma: no-cache\r\n" +
                    "Content-Type: application/x-mms-framed\r\n\r\n";

                Encoding enc = Encoding.ASCII; // "euc-jp"
                byte[] reqBytes = enc.GetBytes(reqMsg);
                sock_chk.Send(reqBytes, reqBytes.Length, SocketFlags.None);

                // 応答ヘッダ受信
                byte[] tmp = new byte[9];
                byte[] tmp2 = new byte[3];
                sock_chk.Receive(tmp);  // "HTTP/1.x " まで受信
                sock_chk.Receive(tmp2); // HTTP StatusCodeの3byte受信
                int http_status = int.Parse(Encoding.ASCII.GetString(tmp2));
                // StatusCode:200以外はNG
                // 10秒後にチェックして美人は普通ありえないでしょう。。
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
                // Contents-TypeのチェックもOKなので大丈夫でしょう。
                // コネクションを切らずに終了
                sock_chk.Close();
                Front.AddLogData(0, Status, "子鏡接続チェックOK [" + Ip + ":" + KagamiPort + "]");
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
                    Front.AddLogData(1, Status, "子鏡接続チェックNG [[" + Id + "] " + Ip + "]");
                }
                sock_chk.Close();
            }
        }

        /// <summary>
        /// 同一IP接続数を監視して
        /// 制限数を超えていたら切断する
        /// </summary>
        private void SameIPCheck()
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
