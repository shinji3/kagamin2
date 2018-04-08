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
        #region メンバ変数

        /// <summary>
        /// 各種情報管理
        /// </summary>
        private Status Status = null;
        
        /// <summary>
        /// Import用ソケット
        /// </summary>
        private Socket sock = null;

        /// <summary>
        /// IM接続/切断音再生用
        /// </summary>
        private System.Media.SoundPlayer player = null;

        /// <summary>
        /// Push配信でSetup要求受信済みフラグ
        /// </summary>
        private bool bSetup;

        /// <summary>
        /// Push配信でSetup要求を行ったIPアドレス
        /// </summary>
        private string SetupIp;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_import"></param>
        /// <param name="_port"></param>
        /// <param name="_connection"></param>
        /// <param name="_reserve"></param>
        public Import(Status _status)
        {

            Status = _status;

            // インポート制御タスク開始
            Thread th1 = new Thread(ImportMain);
            th1.Name = "ImportMain";
            th1.Start();

            // 帯域測定タスク開始
            Thread th2 = new Thread(BandTask);
            th2.Name = "BandTask";
            th2.Start();

        }

        /// <summary>
        /// インポート制御タスク
        /// </summary>
        private void ImportMain()
        {
            lock (this)
            {
                if (Status.Type != 0)
                    Front.AddLogData(0, Status, "外部接続待ちうけタスクを開始します");
                while(true)
                {
                    // 外部接続なら待ち受けを開始する
                    if (Status.Type != 0)
                    {
                        // WebエントランスからImportURLが「待機中」以外に書き換えられるか、
                        // WebエントランスからPush配信の利用要求(Type=1→2変更)が来るか、
                        // GUIから終了要求を受けるまで待機
                        while (Status.ImportURL == "待機中" && Status.Type == 1 && Status.RunStatus)
                            Thread.Sleep(1000);

                        // GUIからの終了要求を受信した場合、そのまま終了
                        if (Status.RunStatus == false)
                            break;

                        // 外部からPush配信の要求が来た時
                        if (Status.Type == 2)
                        {
                            if (!Front.Opt.EnablePush)
                            {
                                // HybridDSP側でガードしてるが念のため
                                // 外部接続に戻して再度待ち受けへ
                                Status.Type = 1;
                                continue;
                            }

                            // 直前の接続でExportポートの解放が済んで無い場合、解放待ち
                            while (Status.ListenPort)
                                Thread.Sleep(100);

                            // Push配信利用要求が来たので、ポートを開けて待つ。
                            Front.AddLogData(0, Status, "Push配信受付ポートを起動します");
                            IPEndPoint _iep = new IPEndPoint(Socket.OSSupportsIPv6 ? IPAddress.IPv6Any : IPAddress.Any, Status.MyPort);
                            TcpListener _listener = new TcpListener(_iep);
                            // Listen開始
                            try
                            {
                                Status.ListenPort = true;
                                _listener.Start();
                            }
                            catch
                            {
                                Front.AddLogData(1, Status, "Push配信ポートの待ち受けが出来ません。設定を確認して下さい。");
                                Status.Disc();
                                Status.RunStatus = false;
                                Status.ListenPort = false;
                                break;
                            }

                            int _timeout_cnt = 0;
                            bool _timeout_flg = false;
                            bSetup = false; // Setup未受信
                            try
                            {
                                // Push配信開始によってImportURLが「待機中」以外になるか
                                // WebエントランスからPush配信停止要求が来るか
                                // Push配信受付タイムアウトになるか、
                                // GUIからの切断要求が来るまでListen継続
                                while (Status.ImportURL == "待機中" && Status.Type == 2 && (_timeout_cnt <= 300 || !Front.Hp.UseHP) && Status.RunStatus)
                                {
                                    //Accept待ちのチェック
                                    if (_listener.Pending() == true)
                                    {
                                        //Accept実施
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
                                // 300秒を超えたらタイムアウトで外部接続状態に戻る
                                // 値はGUI上から変えられるようにしよう。後で。。
                                if (_timeout_cnt > 300 && Front.Hp.UseHP)
                                    _timeout_flg = true;
                            }
                            finally
                            {
                                if (_timeout_flg)
                                    Front.AddLogData(1, Status, "Push配信受付ポートを停止します(受付時間超過)");
                                else if (Status.Type != 2 || !Status.RunStatus)
                                    Front.AddLogData(1, Status, "Push配信受付ポートを停止します(停止要求)");
                                else
                                    Front.AddLogData(1, Status, "Push配信受付ポートを停止します(配信開始)");
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

                            // GUIからの停止要求なら外部待ち受け停止
                            if (!Status.RunStatus)
                                break;

                            // timeoutもしくはWebエントランスからの停止要求なら外部接続待ち受けに戻る
                            if (_timeout_flg || Status.Type != 2)
                            {
                                Status.Type = 1;
                                Front.AddLogData(1, Status, "外部接続待ち受け状態に戻ります");
                                continue;
                            }
                            // push配信開始
                        }

                        // 外部からの接続要求を受信
                        // …このログ出力は、HttpHybridに移動。
                        //Front.AddLogData(1, Status, "外部接続要求を受信しました");
                        //Front.AddLogData(1, Status, "URL=" + Status.ImportURL + " / コメント=" + Status.Comment);
                        // GUIの左パネルを更新
                        Event.EventUpdateKagami();

                    }
                    // インポート先へ接続
                    if (Status.Type != 2)
                    {
                        // 内側接続または外側接続
                        Front.AddLogData(0, Status, "インポートタスクを開始します");
                        ImportTask();
                        Front.AddLogData(0, Status, "インポートタスクを終了します");
                    }
                    else
                    {
                        // Push配信
                        Front.AddLogData(0, Status, "Push配信インポートタスクを開始します");
                        PushImportTask();
                        Front.AddLogData(0, Status, "Push配信インポートタスクを終了します");
                    }
                    // 内側接続なら終了
                    if (Status.Type == 0)
                    {
                        Status.RunStatus = false;
                        break;
                    }
                    // 外部接続でGUIの切断要求受信なら終了
                    if (Status.RunStatus == false)
                    {
                        break;
                    }

                    // 不要な情報はクリア
                    // ただしPush配信のときは待ちうけに戻っても状態保持のためクリアしない。
                    if (Status.Type == 2)
                    {
                        // Push配信でRunStatus正常ならPush配信要求待ちに戻る
                        Front.AddLogData(1, Status, "Push配信待ち受け状態に戻ります");
                        // 切断のための値設定
                        Status.ImportStatus = false;
                        Status.ImportURL = "待機中";
                        // 各種情報の消去
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
                        // 最大接続数をユーザ指定値に戻す
                        Status.Connection = Status.Conn_UserSet;
                    }
                    else
                    {
                        // 外側接続でRunStatus正常なら待ち受けに戻る
                        Front.AddLogData(1, Status, "外部接続待ち受け状態に戻ります");
                        Status.Disc();
                    }
                    // 外部接続待ちうけに戻る時は追加で以下も削除
                    Status.DataRspMsg10 = null;
                    Status.HeadRspMsg10 = null;
                    Status.HeadStream = null;
                    //念のため
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
                    Front.AddLogData(1, Status, "外部待ち受けタスクを終了します");
                // 起動中鏡リストから削除
                Front.Delete(Status.Kagami);
            }
        }

        /// <summary>
        /// インポートタスク
        /// </summary>
        private void ImportTask()
        {
            uint retry_max;
            uint retry_wait;
            // 内側・外側で再試行のタイミングを変える
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
            // 再接続ループ
            while (Status.RunStatus && Status.ImportURL != "待機中")
            {
                try
                {
                    // 速度情報クリア
                    Status.TrafficCount = 0;
                    Status.AverageDLSpeed = 0;
                    Status.MaxDLSpeed = 0;
                    Front.AddLogData(1, Status, "インポート：接続中…");
                    // ヘッダ取得要求送信
                    GetHeader();
                    Front.AddLogData(1, Status, "インポート：ヘッダ取得完了");
                    // データ取得要求送信
                    GetStream();
                    // インポート受信ループ開始
                    Front.AddLogData(1, Status, "インポートソースの取り込みを開始しました");
                    RecvStreamLoop();
                    Front.AddLogData(1, Status, "インポートソースの取り込みを終了しました");
                }
                catch (KagamiException ke)
                {
                    Front.AddLogData(ke.LogLv, Status, ke.Message);
                    if (ke.Message.IndexOf("リダイレクト") >= 0)
                    {
                        // リダイレクト要求による切断の場合、即再接続
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogData(1, Status, "インポートエラー(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")");
                }

                try
                {
                    // 接続が切れたのでリトライカウンタをカウントＵＰ。
                    // リトライカウンタのリセット契機は、ASF_STREAM_DATAを正常に受信できた時。
                    Status.RetryCounter++;
                    Front.AddLogData(0, Status, "接続カウント " + Status.RetryCounter + "回目終了");
                    sock.Close();
                }
                catch { }

                // 帯域情報＆最大接続数のリセット
                Status.MaxDLSpeed = 0;
                Status.AverageDLSpeed = 0;
                Status.TrafficCount = 0;
                Status.Connection = Status.Conn_UserSet;

                if (Status.RetryCounter >= retry_max)
                {
                    Front.AddLogData(0, Status, "再接続を終了します");
                    Status.ImportErrorContext = "";
                    break;
                }
                if (Status.RunStatus == false || Status.ImportURL == "待機中")
                {
                    // 切断要求による正常終了
                    break;
                }
                Front.AddLogData(0, Status, "再接続待ち(" + retry_wait + "sec)");
                for (int i = 0; i < retry_wait && Status.RunStatus == true && Status.ImportURL != "待機中"; i++)
                    Thread.Sleep(1000);
                if (Status.RunStatus == true && Status.ImportURL != "待機中")
                    Front.AddLogData(1, Status, "再接続を開始します");
                else
                    Front.AddLogData(1, Status, "再接続を終了します");
            }
        }

        #region インポート先から実際にデータを取得する処理

        /// <summary>
        /// 相手先へ接続し、ヘッダを受信したら切断する
        /// 取得したヘッダはStatus.Data.Header/Status.Data.Header2に保存しておく
        /// ヘッダ取得に失敗した場合はKagamiExceptionをthrowする
        /// </summary>
        /// <returns></returns>
        private void GetHeader()
        {
            if (Status.RunStatus == false)
            {
                throw new KagamiException("ヘッダー取得中に終了要求が発生しました");
            }
            try
            {
                //Socketの作成
                sock = new Socket(Socket.OSSupportsIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress hostadd = Dns.GetHostAddresses(Status.ImportHost)[0];
                IPEndPoint ephost = new System.Net.IPEndPoint(hostadd, Status.ImportPort);

                sock.SendTimeout = (int)Front.Sock.SockConnTimeout;       // Import接続 ヘッダ取得要求送信のタイムアウト値
                sock.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    // Import接続 ヘッダ取得応答受信のタイムアウト値

                //接続
                sock.Connect(ephost);
            }
            catch
            {
                sock.Close();
                // IM接続NG音が設定されていたら再生
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "インポート接続エラー";
                throw new KagamiException("インポート先に接続できませんでした");
            }

            try
            {
                //クライアントに偽装したヘッダを送信
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

                //リクエスト送信
                Front.AddLogDetail("SendReqMsg(Head)Sta-----\r\n" + reqMsg + "\r\nSendReqMsg(Head)End-----");
                sock.Send(reqBytes, reqBytes.Length, SocketFlags.None);

                //まずはHTTP応答ヘッダまで取得
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

                    // HTTP応答ヘッダの終わりを検索
                    if (ack[0].Equals(0x0d)) continue;  // '\r'
                    if (ack[0].Equals(ack_end[i])) i++; else i = 0;

                    //ack_endに入れた文字列と同じものが受信できたか判定
                    //文字が見つからず、受信したデータが50000バイトを超えたらエラー扱いにする
                    //ほとんどの場合、5000～6000バイトで見つかる
                    //50000まで行くとエラーの可能性大
                    if (i >= ack_end.Length)
                    {
                        // HTTP StatusCode取得
                        // 9～11文字目を取得
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
                            //HTTP StatusCode変換失敗
                            Status.ImportErrorContext = "ヘッダ取得エラー(HTTP応答ヘッダ異常)";
                            throw new KagamiException("ヘッダの取得中にエラーが発生しました(HTTP応答ヘッダ異常)");
                        }
                        if (http_status == 200)
                        {
                            break;
                        }
                        else if (http_status == 301 || http_status == 302)
                        {
                            // リダイレクト指示の場合は移動先をImportURLに再設定してからNG処理
                            string _reply = System.Text.Encoding.ASCII.GetString(ack_log, 0, count);
                            int _idx = _reply.IndexOf("Location: ");
                            if (_idx >= 0)
                            {
                                _idx += 10; // 移動先URL文字列先頭に移動
                                int _len = _reply.IndexOf("\r\n", _idx) - _idx;
                                if (_len > 0)
                                {
                                    string _redirURL = _reply.Substring(_idx, _len);
                                    string _orgURL = Status.ImportURL;
                                    Status.ImportURL = _redirURL;
                                    Event.EventUpdateKagami();
                                    Status.ImportErrorContext = "リダイレクト応答を受信しました[HTTPStatusCode=302]";
                                    throw new KagamiException("リダイレクトします[URL= " + _orgURL + " -> URL=" + _redirURL + "]");
                                }
                            }
                        }
                        Status.ImportErrorContext = "インポートソースはビジーです。[HTTPStatusCode=" + http_status + "]";
                        throw new KagamiException("インポートソースはビジーです。[HTTPStatusCode=" + http_status + "]");
                    }
                    else if (count >= 50000)
                    {
                        Front.AddLogDetail("RecvRspMsg(Head)Sta-----\r\n" +
                                     System.Text.Encoding.ASCII.GetString(ack_log,0,count) +
                                     "\r\nRecvRspMsg(Head)End-----");
                        Status.ImportErrorContext = "HTTPヘッダ取得エラー(HTTPHeader>50KBover)";
                        throw new KagamiException("HTTPヘッダの取得中にエラーが発生しました(HTTPHeader>50KBover)");
                    }
                }

                //ヘッダ上と下を繋げるメモリストリーム
                MemoryStream ms2;
                ms2 = new MemoryStream();

                // ASFヘッダメモ: type(2)+size(2)+seq(4)+unk(2)+szcfm(2)

                //ack_end = new byte[] { 0x00, 0x00, 0x01, 0x01 };
                byte[] ack_sta = new byte[] { 0x24, 0x48 };
                i = 0;
                int pos = 0;
                // ASF_HEADER始まりを探す
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
                        Status.ImportErrorContext = "ストリームヘッダ取得エラー(StreamHeader>50KBover)";
                        throw new KagamiException("ストリームヘッダの取得中にエラーが発生しました(StreamHeader>50KBover)");
                    }
                }
                if(Status.RunStatus == false)
                    throw new KagamiException("ヘッダー取得中に終了要求が発生しました");

                // blk_size取得
                int blk_size = 0;
                sock.Receive(ack);
                ms2.WriteByte(ack[0]);
                blk_size += ack[0];
                sock.Receive(ack);
                ms2.WriteByte(ack[0]);
                blk_size += (ack[0] << 8);
                // 残りのヘッダ取得
                int rsp_size = 0;
                ack = new byte[blk_size];
                while (Status.RunStatus && blk_size > rsp_size)
                {
                    rsp_size += sock.Receive(ack, rsp_size, blk_size - rsp_size, SocketFlags.None);
                }
                if (Status.RunStatus == false)
                    throw new KagamiException("ヘッダー取得中に終了要求が発生しました");

                ms2.Write(ack, 0, blk_size);
                Status.HeadStream = ms2.ToArray();

                bool error_flg = false;
                // size-cfmのチェック。異常でも突き進む？
                // ack[6-7]がsize-cfm
                if ((ack[6] + (ack[7] << 8)) != blk_size)
                {
                    Front.AddLogData(1, Status, "受信データはASFストリームではありません");
                    error_flg = true;
                }

                // 末尾の 00 00 01 01 チェック。異常でも突き進む。
                if (ack[blk_size - 4] != 0x00 ||
                    ack[blk_size - 3] != 0x00 ||
                    ack[blk_size - 2] != 0x01 ||
                    ack[blk_size - 1] != 0x01)
                {
                    Front.AddLogData(1, Status, "HeaderStreamが正しくない可能性があります。");
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
                    // ヘッダ解析テスト
                    //WMVヘッダはASFヘッダ type(2)+size(2)+seq(4)+unk(2)+szcfm(2)の後。posはtypeの終わりまでのoffset
                    CheckWMVHeader(Status.HeadStream, pos + 10);
                }
                catch { }
                sock.Close();

                // エクスポートの応答データ作成
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
                //ヘッダ上と下を繋げるメモリストリーム
                MemoryStream ms1;
                ms1 = new MemoryStream();
                count = enc.GetBytes(str).Length;
                ms1.Write(enc.GetBytes(str), 0, count);
                Status.HeadRspMsg10 = ms1.ToArray();
            }
            catch (KagamiException ke)
            {
                sock.Close();
                // IM接続NG音が設定されていたら再生
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                throw new KagamiException(ke.Message);
            }
            catch (SocketException se)
            {
                sock.Close();
                // IM接続NG音が設定されていたら再生
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "ヘッダ取得ソケットエラー";
                throw new KagamiException("ヘッダの取得中にエラーが発生しました(wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")");
            }
            catch (Exception e)
            {
                sock.Close();
                // IM接続NG音が設定されていたら再生
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "ヘッダ取得内部エラー";
                throw new KagamiException("ヘッダの取得中にエラーが発生しました(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")");
            }
        }

        /// <summary>
        /// WMVヘッダを解析してデータを取得する
        /// </summary>
        /// <param name="block"></param>
        /// <param name="idx"></param>
        private void CheckWMVHeader(byte[] block, int idx)
        {
            string key = "";
            int max = 0;
            int pos = idx; // 次Object先頭位置
            int num = 0;
            int obj = 0;
            string stream_prop = "";
            Status.MaxDLSpeed = 0;
            try
            {
                //先頭がASF_HEADER_OBJECTになっているかチェック
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
                             * ASF_File_Properties_Objectのビットレートは
                             * 全プロパティの和になってしまうので、マルチビットレートだと使えない。
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
                            // 受信ヘッダのマルチビットレート判定
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
                    // マルチビットレート用に帯域を計算し直す
                    // 帯域制限には最高ビットレートの値を適用
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
                    // 受信するストリームの決定及び最大ビットレートの計算
                    for (int cnt = 1; cnt <= Status.StreamSwitchCount; cnt++)
                    {
                        // 音声ストリームの中でビットレートが最大のものを選択
                        if ((Status.StreamType[cnt - 1] == 0) && (Status.StreamBitrate[cnt - 1] > max_audio))
                        {
                            Status.SelectedAudioRecord = cnt;
                            max_audio = Status.StreamBitrate[cnt - 1];
                        }
                        // 映像ストリームの中でビットレートが最大のものを選択
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
                        Status.Data.MaxDLSpeed = ex_max;    // Extended_Stream_Propertiesから取得できた場合
                    else
                    */
                    if (max > 0)
                        Status.MaxDLSpeed = max / 1000;       // Extended_Stream_Propertiesから取得できなかった場合、File_Propertiesから設定
                    else
                        Front.AddLogData(1, Status, "ビットレート取得NG");
                }
                else
                {
                    Front.AddLogDebug("WMVHeader[" + Status.MyPort + "]", "ASF_Header_Object CHECK NG");
                    Front.AddLogData(1, Status, "ビットレート取得NG");
                }
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "配信ビットレート取得内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
            }
        }

        /// <summary>
        /// 相手先へ接続し、DataStream本体のリクエストを送信する
        /// 受信処理はHTTP StatusCodeのみチェックし、以降の受信処理はRecvStreamに任せる
        /// DataStream受信NGの場合は、KagamiExceptionをthrowする
        /// </summary>
        private void GetStream()
        {
            if (Status.RunStatus == false)
                throw new KagamiException("ストリーム要求中に終了要求が発生しました");

            try
            {
                //Socketの作成
                sock = new Socket(Socket.OSSupportsIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress hostadd = Dns.GetHostAddresses(Status.ImportHost)[0];
                IPEndPoint ephost = new System.Net.IPEndPoint(hostadd, Status.ImportPort);

                sock.SendTimeout = (int)Front.Sock.SockConnTimeout;       //インポート接続 Stream本体要求タイムアウト
                sock.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    //インポート接続 Stream本体受信タイムアウト

                //接続
                sock.Connect(ephost);

                // WMVヘッダが正常に解析できなかった時
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
                    // ↓WMPのオプション→パフォーマンスで回線速度自動選択にした時、この値になる
                    "Pragma: LinkBW=2147483647, AccelBW=2147483647, AccelDuration=18000\r\n" +
                    // ↓回線速度を手動設定したときは、このようになる(256kbps指定時)
                    //"Pragma: LinkBW=240000\r\n" +
#if !DEBUG
                    "Pragma: kagami-port=" + Status.MyPort + "\r\n" +
#endif
                    "Accept-Language: ja, *;q=0.1\r\n" +
                    "Supported: com.microsoft.wm.srvppair, com.microsoft.wm.sswitch, com.microsoft.wm.predstrm, com.microsoft.wm.startupprofile\r\n" +
                    // stream-switch-countには含まれる全ストリームの数を設定
                    "Pragma: stream-switch-count=" + Status.StreamSwitchCount + "\r\n" +
                    // stream-switch-entryの記述方法は下記参照
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
                // memo:stream-switch-entry設定方法
                // ffff:X:Y
                // X: ストリーム番号
                // Y: 0:そのストリームを要求する
                //    1:？ videoストリームで設定されることあり。。
                //    2:そのストリームを要求しない
                // 例）ID:1=64K_AUDIO 2=128K_AUDIO 3=300K_VIDEO 4=600K_VIDEOの場合
                //    64K_AUDIO+300K_VIDEOを要求：stream-switch-entry=ffff:1:0 ffff:2:2 ffff:3:0 ffff:4:2
                //   128K_AUDIO+300K_VIDEOを要求：stream-switch-entry=ffff:1:2 ffff:2:0 ffff:3:0 ffff:4:2
                //   128K_AUDIO+600K_VIDEOを要求：stream-switch-entry=ffff:1:2 ffff:2:0 ffff:3:2 ffff:4:0
                // 気力があればその内マルチビットレートにも対応したいね。。

                //ヘッダの送信
                System.Text.Encoding enc = System.Text.Encoding.ASCII;
                byte[] reqBytes = enc.GetBytes(reqMsg);

                Front.AddLogDetail("SendReqMsg(Data)Sta-----\r\n" + reqMsg + "\r\nSendReqMsg(Data)End-----");
                sock.Send(reqBytes);


                //まずはHTTP応答ヘッダまで取得
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

                    // HTTP応答ヘッダの終わりを検索
                    if (ack[0].Equals(0x0d)) continue;  // '\r'
                    if (ack[0].Equals(ack_end[i])) i++; else i = 0;

                    //ack_endに入れた文字列と同じものが受信できたか判定
                    //文字が見つからず、受信したデータが50000バイトを超えたらエラー扱いにする
                    //ほとんどの場合、5000～6000バイトで見つかる
                    //50000まで行くとエラーの可能性大
                    if (i >= ack_end.Length)
                    {
                        // HTTP StatusCode取得
                        // 9～11文字目を取得
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
                            //HTTP StatusCode変換失敗
                            Status.ImportErrorContext = "ストリーム要求時エラー(HTTP応答ヘッダ異常)";
                            throw new KagamiException("ストリーム要求中にエラーが発生しました(HTTP応答ヘッダ異常)");
                        }
                        if (http_status == 200)
                        {
                            break;
                        }
                        else
                        {
                            Status.ImportErrorContext = "インポートソースビジー[" +http_status + "]";
                            throw new KagamiException("インポートソースはビジーです。[HTTPStatusCode=" + http_status + "]");
                        }
                    }
                    else if (count >= 50000)
                    {
                        Front.AddLogDetail("RecvRspMsg(Data)Sta-----\r\n" +
                                     System.Text.Encoding.ASCII.GetString(ack_log, 0, count) +
                                     "\r\nRecvRspMsg(Data)End-----");
                        Status.ImportErrorContext = "HTTPヘッダ取得エラー(HTTPHeader>50KBover)";
                        throw new KagamiException("HTTPヘッダの取得中にエラーが発生しました(HTTPHeader>50KBover)");
                    }
                }
                // エクスポートの応答データ作成
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
                //データ取得応答ヘッダを保持
                MemoryStream ms;
                ms = new MemoryStream();
                count = enc.GetBytes(str).Length;
                ms.Write(enc.GetBytes(str), 0, count);
                Status.DataRspMsg10 = ms.ToArray();
            }
            catch (KagamiException ke)
            {
                sock.Close();
                // IM接続NG音が設定されていたら再生
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                throw new KagamiException(ke.Message);
            }
            catch (SocketException se)
            {
                sock.Close();
                // IM接続NG音が設定されていたら再生
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "ストリーム要求時エラー:" + se.SocketErrorCode.ToString();
                throw new KagamiException("ストリーム要求中にエラーが発生しました(wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")");
            }
            catch (Exception e)
            {
                sock.Close();
                // IM接続NG音が設定されていたら再生
                if (File.Exists(Front.Opt.SndConnNgFile))
                    PlaySound(Front.Opt.SndConnNgFile);
                Status.ImportErrorContext = "ストリーム要求時内部エラー";
                throw new KagamiException("ストリーム要求中にエラーが発生しました(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")");
            }
        }

        /// <summary>
        /// インポート先からDataStream受信ループ処理
        /// 受信できなくなったらループ終了
        /// </summary>
        private void RecvStreamLoop()
        {
            // 受信データはbyte[]配列に格納して、
            // ASF1ブロック分取得できたら1ブロック毎にキューイングする方式にする
            MemoryStream ms = new MemoryStream();
            int recv_timeout = 0;
            int ava_size = 0;
            int blk_size = 0;
            int rsp_size = 0;
            byte[] recv = new byte[1];
            byte[] asf_head = new byte[4];
            byte[] blk = null;

            // ストリーム受信が正常に出来ていればtrue。
            // ImportStatusは外部から書き換えられるので、
            // TimeOut判定ではこちらを利用
            bool once_status = false;

            // 最初はStatus.ImportStatusはfalseになっている
            // ASF_STREAMING_DATA受信でtrueにする
            try
            {
                while (Status.RunStatus && Status.ImportURL != "待機中")
                {
                    // 中途半端に受信したブロックがあるか？
                    // rsp_sizeが0以外なら、中途半端に受信した状態になっている
                    if (rsp_size == 0)
                    {// 中途半端なデータは無い。先頭から新規に受信
                        ava_size = sock.Available;
                        // ASFブロック長を知るために最低4byte受信したい
                        if (ava_size > 4)
                        {
                            // 読み取り可能データが有ればタイムアウトカウンタをリセット
                            recv_timeout = 0;
                            // 4byte以上受信可能なら、まずtypeチェック
                            #region type 1byte目受信＆チェック
                            sock.Receive(recv);
                            if (recv[0] != 0x24)
                            {
                                #region type1異常
                                // typeが異常。とりあえずms2に退避
                                ms.WriteByte(recv[0]);
                                // ms2が1000byteを超えていたらユーザに送信
                                if (ms.Length > 1000)
                                {
                                    // 送信
                                    Status.Client.StreamWrite(ms.ToArray());
                                    // 一度解放して再捕捉しておく
                                    ms.Dispose();
                                    ms = new MemoryStream();
                                }
                                #endregion
                                // 再度最初から受信しなおし
                                continue;
                            }
                            #endregion
                            #region type 2byte目受信＆チェック
                            sock.Receive(recv);
                            switch (recv[0])
                            {
                                case 0x43: // ASF_STREAMING_CLEAR(0x2443)
                                case 0x45: // ASF_STREAMING_END_TRANS(0x2445)
                                case 0x48: // ASF_STREAMING_HEADER(0x2448)
                                    break;
                                case 0x44: // ASF_STREAMING_DATA(0x2444)
                                    // DATAブロックを受信したらImportStatus正常に遷移
                                    if (Status.ImportStatus == false)
                                    {
                                        Status.ImportStartTime = DateTime.Now;
                                        Status.ClientTime = DateTime.Now;
                                        Status.ImportStatus = true;
                                        Status.RetryCounter = 0;
                                        once_status = true;
                                        // IM接続完了音が設定されていたら再生
                                        if (File.Exists(Front.Opt.SndConnOkFile))
                                            PlaySound(Front.Opt.SndConnOkFile);
                                    }
                                    break;
                                default:
                                    #region type2異常
                                    // type異常。同じくms2に退避
                                    ms.WriteByte(0x24);
                                    ms.WriteByte(recv[0]);
                                    // ms2が1000byteを超えていたらユーザに送信
                                    if (ms.Length > 1000)
                                    {
                                        // 送信
                                        Status.Client.StreamWrite(ms.ToArray());
                                        // 一度解放して再捕捉しておく
                                        ms.Dispose();
                                        ms = new MemoryStream();
                                    }
                                    #endregion
                                    // 再度最初から受信しなおし
                                    continue;
                            }
                            #endregion
                            // type正常
                            #region type異常時の残骸があれば送信しておく
                            if (ms.Length > 0)
                            {
                                // 送信
                                Status.Client.StreamWrite(ms.ToArray());
                                // 一度解放して再捕捉しておく
                                ms.Dispose();
                                ms = new MemoryStream();
                            }
                            #endregion
                            // asf_headにtypeを書き込み
                            asf_head[0] = 0x24;
                            asf_head[1] = recv[0];

                            // ASF block sizeを取得1
                            sock.Receive(recv);
                            asf_head[2] = recv[0];
                            // ASF block sizeを取得2
                            sock.Receive(recv);
                            asf_head[3] = recv[0];
                            // block size算出
                            blk_size = (asf_head[3] << 8) + asf_head[2];

                            // blockサイズ分データが来てるかチェック
                            ava_size = sock.Available;
                            if (ava_size >= blk_size)
                            {// ASF1ブロック分のデータが来ている
                                blk = new byte[4 + blk_size];
                                blk[0] = asf_head[0];
                                blk[1] = asf_head[1];
                                blk[2] = asf_head[2];
                                blk[3] = asf_head[3];
                                rsp_size = 0;
                                // データ受信
                                while (blk_size > rsp_size)
                                    rsp_size += sock.Receive(blk, 4 + rsp_size, blk_size - rsp_size, SocketFlags.None);
                                // クライアントに送信
                                Status.Client.StreamWrite(blk);
                                rsp_size = 0;   // 中途半端データなし
                                blk = null;     // GCを早めるための明示的null設定
                            }
                            else
                            {// ASF1ブロック分のデータが来ていない
                                // 来てるところまで受信して、受信サイズをblk_lenに設定しておく
                                blk = new byte[4 + blk_size];
                                blk[0] = asf_head[0];
                                blk[1] = asf_head[1];
                                blk[2] = asf_head[2];
                                blk[3] = asf_head[3];
                                rsp_size = 0;
                                // 途中までのデータ受信
                                while (ava_size > rsp_size)
                                    rsp_size += sock.Receive(blk, 4 + rsp_size, ava_size - rsp_size, SocketFlags.None);
                                // rsp_sizeとblkを保持したまま再ループ
                            }
                        }
                        else
                        {
                            // 読み取り可能データが4byte以下しか無ければ
                            // タイムアウトカウンタをUP
                            Thread.Sleep(50);
                            recv_timeout++;
                            if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                                break;  //Timeout
                        }
                    }
                    else
                    {// 中途半端なデータが有る。足りないブロックを受信
                        ava_size = sock.Available;
                        if (ava_size > (blk_size - rsp_size))
                        {// 足りないブロックサイズ分のデータが全て来ている
                            // 読み取り可能データが有るのでタイムアウトカウンタをリセット
                            recv_timeout = 0;
                            while (blk_size > rsp_size)
                                rsp_size += sock.Receive(blk, 4 + rsp_size, blk_size - rsp_size, SocketFlags.None);
                            // クライアントに送信
                            Status.Client.StreamWrite(blk);
                            rsp_size = 0;   // 中途半端データなし
                            blk = null;     // GCを早めるための明示的null設定
                        }
                        else if (ava_size > 0)
                        {// データは来ているが、１ブロック分を形成するには至らない
                            // 読み取り可能データが有るのでタイムアウトカウンタをリセット
                            recv_timeout = 0;
                            // データ受信。１回だけ。
                            rsp_size += sock.Receive(blk, 4 + rsp_size, ava_size, SocketFlags.None);
                        }
                        else
                        {
                            // データがまったく来ていない
                            // タイムアウトカウンタをUP
                            Thread.Sleep(50);
                            recv_timeout++;
                            if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                                break;  //Timeout
                        }
                    }
                }
                // whileループ抜け
                // 終了理由の判定を行う
                if (Status.RunStatus == false || (Status.ImportStatus == false && once_status == true))
                {
                    // RunStatusがfalseになった(GUIからの切断)、または
                    // once_statusがtrue(正常受信中)にImportStatusがfalse(外部からの切断要求)を受けた場合

                    Front.AddLogData(1, Status, "インポート接続の切断要求を受信しました");
                    // ユーザ指示による切断では切断音を鳴らさない
                    //if (File.Exists(Front.Opt.SndDiscFile))
                    //    PlaySound(Front.Opt.SndDiscFile);
                }
                else
                {
                    Front.AddLogData(1, Status, "インポート先からの受信がタイムアウトしました");
                    Status.ImportErrorContext = "インポート受信タイムアウト";
                    Status.ImportError++;
                    // IM切断音が設定されていたら別スレッドで再生する
                    if (File.Exists(Front.Opt.SndDiscFile))
                        PlaySound(Front.Opt.SndDiscFile);
                }
            }
            catch (SocketException se)
            {
                Front.AddLogData(1, Status, "インポート受信エラー(wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")");
                Status.ImportErrorContext = "インポート受信エラー(wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString() + ")";
                Status.ImportError++;
                // IM切断音が設定されていたら別スレッドで再生する
                if (File.Exists(Front.Opt.SndDiscFile))
                    PlaySound(Front.Opt.SndDiscFile);
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "インポート受信エラー(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")");
                Status.ImportErrorContext = "インポート受信内部エラー";
                Status.ImportError++;
                // IM切断音が設定されていたら別スレッドで再生する
                if (File.Exists(Front.Opt.SndDiscFile))
                    PlaySound(Front.Opt.SndDiscFile);
            }
            finally
            {
                Front.AddLogData(0, Status, "エクスポートへの書き出しを終了します");
                Status.ImportStatus = false;
            }
        }

        #endregion

        #region Push配信関係
        /// <summary>
        /// Push配信リクエスト受付タスク
        /// </summary>
        private void PushReqTask(object obj)
        {
            Socket _sock = (Socket)obj;
            string _ua = "";

            _sock.SendTimeout = (int)Front.Sock.SockConnTimeout;       // Import接続 ヘッダ取得要求送信のタイムアウト値
            _sock.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    // Import接続 ヘッダ取得応答受信のタイムアウト値

            string _ip = ((IPEndPoint)_sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)_sock.RemoteEndPoint).Port;

            try
            {
                Front.AddLogData(0, Status, _ip + "から接続要求");

                #region Kick対象かチェック
                if (Status.IsKickCheck(_sock) == false)
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
                            // 連打しないように応答返却を遅延させる
                            Thread.Sleep((int)Front.Sock.SockCloseDelay);
                            // ビジーメッセージ送信
                            string str = _enc.GetString(reqBytes, 0, j);
                            Front.AddLogDetail("RecvReqMsg(Push)Sta-----\r\n" + str + "\r\nRecvReqMsg(Push)End-----");
                            Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + Front.BusyString + "\r\nSendRspMsg(Push)End-----"); 
                            _sock.Send(_enc.GetBytes(Front.BusyString));
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
                        if (_enc.GetString(reqBytes, 12, 4) != "MMS ")
                            Front.AddLogData(1, Status, "ビジーメッセージが送信できませんでした [" + _ip + "]");
                    }
                    #endregion
                    _sock.Close();
                    return;
                }
                #endregion

                while (true)
                {
                    // 相手からのリクエストデータ未受信なら、Keep-Aliveのため待機
                    while (Status.RunStatus && Status.ImportURL == "待機中" && Status.Type == 2 && _sock.Connected && _sock.Available == 0)
                        Thread.Sleep(500);

                    // Sock切断された場合
                    if (!_sock.Connected)
                    {
                        _sock.Close();
                        return;
                    }
                    // Sock切断以外の異常・他タスクから開始要求受信により切断するパターン
                    if (!Status.RunStatus || Status.ImportURL != "待機中" || Status.Type != 2)
                    {
                        // Setup投げたセッションとStart投げたセッションが別の場合、
                        // Setupセッションに408は返さないようにする
                        if (!bSetup || SetupIp != _ip)
                        {
                            // タイムアウト応答を送信して切断する
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
                                Front.AddLogData(1, Status, "タイムアウトメッセージが送信できませんでした [" + _ip + "]");
                            }
                        }
                        _sock.Close();
                        return;
                    }

                    //Setup受信済みなら、SetupしたIP以外の新規要求は弾く
                    if (bSetup && SetupIp != _ip)
                    {
                        Front.AddLogData(1, Status, "Setup要求元とは異なるIPアドレスからの接続を切断します [" + _ip + "]");
                        _sock.Close();
                        return;
                    }

                    //Push配信受付チェック
                    _ua = PushAcceptUser(_sock);

                    //受付拒否された接続を切断
                    if (_ua == "")
                    {
                        _sock.Close();
                        return;
                    }
                    else if (_ua == "push-setup")
                    {
                        //Push配信Setup要求
                        SetupIp = _ip;
                        bSetup = true;
                        continue;
                    }
                    else if (_ua != "")
                    {
                        // Push配信Start要求
                        // ->Setup要求受信済みチェック
                        if (bSetup == false)
                        {
                            Front.AddLogData(1, Status, "Push配信Setup要求前にPush配信Start要求を受信しました。");
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
                                Front.AddLogData(1, Status, "タイムアウトメッセージが送信できませんでした [" + _ip + "]");
                            }
                            _sock.Close();
                            return;
                        }
                        // Push配信受付OK
                        sock = _sock;
                        Status.SetUserIP = _ip;
                        Status.ImportURL = "push://" + _ip;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "Push配信リクエスト受付エラー(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")[" + _ip + ":" + _port + "]");
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
                    Front.AddLogData(1, Status, "タイムアウトメッセージが送信できませんでした [" + _ip + "]");
                }
                _sock.Close();
            }
        }

        /// <summary>
        /// 接続要求がPush配信開始リクエストか判定する。
        /// </summary>
        /// <param name="sock">要求元のsocket</param>
        /// <returns>要求元のUserAgent</returns>
        private string PushAcceptUser(Socket sock)
        {
            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
            int _port = ((IPEndPoint)sock.RemoteEndPoint).Port;

            try
            {
                #region プロトコルチェック+UserAgent取得
                string userAgent;
                //int priKagamiPort;
                System.Text.Encoding enc;

                char[] end = { '\n', '\n' };
                byte[] reqBytes = new byte[5000];
                int i = 0;
                int j = 0;

                //リクエストヘッダの改行コードは普通CR+LFだけどLFのみにも対応する
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
                        string _str = enc.GetString(reqBytes, 0, j);
                        Front.AddLogDetail("RecvReqMsg(Push)Sta-----\r\n" +
                                     _str +
                                     "\r\nRecvReqMsg(Push)End-----");
                        if (e is SocketException)
                        {
                            SocketException se = (SocketException)e;
                            Front.AddLogData(1, Status, "リクエスト受信タイムアウト [" + _ip + "] wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString());
                        }
                        else
                        {
                            Front.AddLogData(1, Status, "リクエスト受信タイムアウト [" + _ip + "] (内部エラー:" + e.Message + ")");
                        }
                    }
                    return "";
                }
                if (i < 2)
                {
                    //受信できたところまでのリクエストMsgをログ出力
                    enc = Front.GetCode(reqBytes, j);
                    Front.AddLogDetail("RecvReqMsg(Push)Sta-----\r\n" +
                                 enc.GetString(reqBytes, 0, j) +
                                 "\r\nRecvReqMsg(Push)End-----");
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
                // プロトコル取り出し
                string protocol = str.Substring(str.IndexOf("\r\n") - 8, 4);
                // Contextがあるなら取り出してみる
                // ただしPush配信Start要求なら取り出さない
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
                    Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + ackMsg + "\r\nSendRspMsg(Push)End-----");
                    sock.Send(enc.GetBytes(ackMsg));
                    //切断するためnull返却
                    return "";
                }
                /*
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
                 */
                #endregion

                // ブラウザからのアクセス
                if (Front.Opt.BrowserView == true &&
                    (userAgent.IndexOf("Mozilla") == 0 ||
                     userAgent.IndexOf("Opera") == 0))
                {
                    Front.AddLogData(0, Status, "Webブラウザからのアクセスです UA: " + userAgent);
                    Front.AddLogData(0, Status, "コネクションを切断します [" + _ip + "]");
                    //切断するためnull返却
                    return "";
                }
                else
                {
                    // Push配信判定
                    if (str.Contains("Content-Type: application/x-wms-pushsetup"))
                    {
                        //int pid = Environment.TickCount & 0xFFFF;
                        Front.AddLogData(0, Status, "Push配信Setup要求を受信しました UA: " + userAgent);
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
                        Front.AddLogData(0, Status, "Push配信Start要求を受信しました UA: " + userAgent);
                        return userAgent;
                    }
                    else
                    {
                        Front.AddLogData(0, Status, "WMEncoder以外での接続です UA: " + userAgent);
                        // 切断遅延
                        // ・・・Push配信中にIMが切れた時にクライアントが
                        //       大量に再接続に来るので切断遅延を入れる
                        Thread.Sleep((int)Front.Sock.SockCloseDelay);
                        // 美人メッセージ送信
                        // ・・・何も送らず切ったほうがいいかなぁ。。
                        //Front.AddLogDetail("SendRspMsg(Push)Sta-----\r\n" + Front.BusyString + "\r\nSendRspMsg(Push)End-----");
                        //sock.Send(enc.GetBytes(Front.BusyString));
                        Front.AddLogData(0, Status, "コネクションを切断します [" + _ip + "]");
                        return "";
                    }
                    /*
                     * リクエスト１発目
                    POST // HTTP/1.1
                    Content-Type: application/x-wms-pushsetup
                    X-Accept-Authentication: Negotiate, NTLM, Digest
                    User-Agent: WMEncoder/11.0.5721.5145
                    Host: localhost:8888
                    Content-Length: 16
                    Connection: Keep-Alive
                    Cache-Control: no-cache
                    Cookie: push-id=0

                     * 応答１発目
                    HTTP/1.1 204 No Content
                    Server: Servet-agent
                    Content-Length: 0
                    Date: Tue, 09 Jan 2007 10:02:58 GMT
                    Pragma: no-cache, timeout=60000
                    Cache-Control: no-cache
                    Set-Cookie: push-id=35201712
                    Supported: com.microsoft.wm.srvppair, com.microsoft.wm.sswitch, com.microsoft.wm.predstrm, com.microsoft.wm.fastcache, com.microsoft.wm.startupprofile
                     * 
                    サーバはKeepAliveしておくこと。
                     * 
                     * リクエスト２発目
                    POST /test HTTP/1.1
                    Content-Type: application/x-wms-pushstart
                    X-Accept-Authentication: NTLM, Digest
                    User-Agent: WMEncoder/9.0.0.3287
                    Host: 192.168.0.1:8000
                    Content-Length: 2147483647
                    Connection: Keep-Alive
                    Cache-Control: no-cache
                    Cookie: push-id=35201712
                    $H～以下ストリームデータ～
                     * 
                     * サーバ受信待ちタイムアウト
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
                Front.AddLogData(1, Status, "クライアント受付エラー(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")[" + _ip + "]");
                return "";
            }
            // ここには来ない
            //return "";
        }


        /// <summary>
        /// Push配信用インポートタスク
        /// </summary>
        private void PushImportTask()
        {
            Status.ImportStartTime = DateTime.Now;
            Status.BusyCounter = 0;
            Status.RetryCounter = 0;
            Status.ImportError = 0;
            Status.ExportError = 0;
            Status.ExportCount = 0;
            // 速度情報クリア
            Status.TrafficCount = 0;
            Status.AverageDLSpeed = 0;
            Status.MaxDLSpeed = 0;

            // エクスポートの応答データ作成
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
            //ヘッダ上と下を繋げるメモリストリーム
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
                // ASF HEAD検出
                j = 0;
                while (Status.RunStatus && Status.ImportURL != "待機中")
                {
                    sock.Receive(ack);
                    j++;
                    if (ack[0].Equals(asf_type[i])) i++; else i = 0;
                    if (i >= asf_type.Length)
                    {
                        // push配信ではASF HEAD BLOCKがpull配信と異なるので自作する
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
                        //先頭50KB内にASFヘッダ無し
                        throw new KagamiException("ストリームヘッダの取得中にエラーが発生しました(NotFoundASFHead at Top50KB)");
                    }
                }
                // ASF HEAD検出完了&自作HEADをms2に書き込み済み
                // ASF HEADの本体を受信してms2に書き込み
                j = 0;
                while (Status.RunStatus && Status.ImportURL != "待機中")
                {
                    sock.Receive(ack);
                    ms2.WriteByte(ack[0]);
                    j++;
                    if (ack[0].Equals(ack_end[i])) i++; else i = 0;

                    if (i >= ack_end.Length)
                    {
                        //ASF HEADの終わりを検出
                        //msの配列の長さの確認
                        Status.HeadStream = ms2.ToArray();
                        //Front.AddLogData(Status, "ヘッダ取得完了");
                        try
                        {
                            // ヘッダ解析テスト
                            CheckWMVHeader(Status.HeadStream, 12);
                        }
                        catch { }
                        break;
                    }
                    else if (j > 50000)
                    {
                        throw new KagamiException("ストリームヘッダの取得中にエラーが発生しました(StreamHeader>50KBover)");
                    }
                }

                // エクスポートの応答データ作成
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
                //ヘッダ上と下を繋げるメモリストリーム
                MemoryStream ms3;
                ms3 = new MemoryStream();
                count = enc.GetBytes(str).Length;
                ms3.Write(enc.GetBytes(str), 0, count);
                Status.HeadRspMsg10 = ms3.ToArray();

                if (Status.ImportURL == "待機中")
                {
                    Front.AddLogData(1, Status, "Push配信ストリームヘッダの取得中に終了要求を受けました");
                }
                else
                {
                    // ASF HEAD受信完了
                    // インポート受信ループ開始
                    Front.AddLogData(1, Status, "Push配信ソースの取り込みを開始しました");
                    RecvPushStreamLoop();
                    Front.AddLogData(1, Status, "Push配信ソースの取り込みを終了しました");
                }
            }
            catch (KagamiException ke)
            {
                Front.AddLogData(ke.LogLv, Status, ke.Message);
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "Push配信エラー(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")");
            }
            sock.Close();
            // 帯域情報＆最大接続数のリセット
            Status.MaxDLSpeed = 0;
            Status.AverageDLSpeed = 0;
            Status.TrafficCount = 0;
            Status.Connection = Status.Conn_UserSet;
        }

        /// <summary>
        /// インポート先からDataStream受信ループ処理
        /// 受信できなくなったらループ終了
        /// </summary>
        private void RecvPushStreamLoop()
        {
            // 受信データはbyte[]配列に格納して、
            // ASF1ブロック分取得できたら1ブロック毎にキューイングする方式にする
            MemoryStream ms = new MemoryStream();
            int recv_timeout = 0;
            int ava_size = 0;
            int blk_size = 0;
            int rsp_size = 0;
            uint seqno = 1;
            byte[] recv = new byte[1];
            byte[] asf_head = new byte[4];
            byte[] blk = null;
            bool end_flg = false; // ASF_STREAMING_END_TRANS受信フラグ

            // ストリーム受信が正常に出来ていればtrue。
            // ImportStatusは外部から書き換えられるので、
            // TimeOut判定ではこちらを利用
            bool once_status = false;

            // 最初はStatus.ImportStatusはfalseになっている
            // ASF_STREAMING_DATA受信でtrueにする
            try
            {
                while (Status.RunStatus && Status.ImportURL != "待機中")
                {
                    // 中途半端に受信したブロックがあるか？
                    // rsp_sizeが0以外なら、中途半端に受信した状態になっている
                    if (rsp_size == 0)
                    {// 中途半端なデータは無い。先頭から新規に受信
                        ava_size = sock.Available;
                        // ASFブロック長を知るために最低4byte受信したい
                        if (ava_size > 4)
                        {
                            // 読み取り可能データが有ればタイムアウトカウンタをリセット
                            recv_timeout = 0;
                            // 4byte以上受信可能なら、まずtypeチェック
                            #region type 1byte目受信＆チェック
                            sock.Receive(recv);
                            if (recv[0] != 0x24)
                            {
                                #region type1異常
                                if (end_flg)
                                {
                                    // end_flg == trueの時は停止⇒即再スタートの可能性あり
                                    // 本来はkeep-aliveで再スタート処理に進むが、ここでは切断する
                                    break;
                                }
                                else
                                {
                                    // typeが異常。とりあえずms2に退避
                                    ms.WriteByte(recv[0]);
                                    // ms2が1000byteを超えていたらユーザに送信
                                    if (ms.Length > 1000)
                                    {
                                        // 送信
                                        Status.Client.StreamWrite(ms.ToArray());
                                        // 一度解放して再捕捉しておく
                                        ms.Dispose();
                                        ms = new MemoryStream();
                                    }
                                }
                                #endregion
                                // 再度最初から受信しなおし
                                continue;
                            }
                            #endregion
                            #region type 2byte目受信＆チェック
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
                                    // DATAブロックを受信したらImportStatus正常に遷移
                                    if (Status.ImportStatus == false)
                                    {
                                        Status.ImportStartTime = DateTime.Now;
                                        Status.ClientTime = DateTime.Now;
                                        Status.ImportStatus = true;
                                        Status.RetryCounter = 0;
                                        once_status = true;
                                        // IM接続完了音が設定されていたら再生
                                        if (File.Exists(Front.Opt.SndConnOkFile))
                                            PlaySound(Front.Opt.SndConnOkFile);
                                    }
                                    break;
                                default:
                                    #region type2異常
                                    // type異常。同じくms2に退避
                                    ms.WriteByte(0x24);
                                    ms.WriteByte(recv[0]);
                                    // ms2が1000byteを超えていたらユーザに送信
                                    if (ms.Length > 1000)
                                    {
                                        // 送信
                                        Status.Client.StreamWrite(ms.ToArray());
                                        // 一度解放して再捕捉しておく
                                        ms.Dispose();
                                        ms = new MemoryStream();
                                    }
                                    #endregion
                                    // 再度最初から受信しなおし
                                    continue;
                            }
                            #endregion
                            // type正常
                            #region type異常時の残骸があれば送信しておく
                            if (ms.Length > 0)
                            {
                                // 送信
                                Status.Client.StreamWrite(ms.ToArray());
                                // 一度解放して再捕捉しておく
                                ms.Dispose();
                                ms = new MemoryStream();
                            }
                            #endregion
                            // seqno更新
                            seqno++;
                            // asf_headにtypeを書き込み
                            asf_head[0] = 0x24;
                            asf_head[1] = recv[0];

                            // ASF block sizeを取得1
                            sock.Receive(recv);
                            asf_head[2] = recv[0];
                            // ASF block sizeを取得2
                            sock.Receive(recv);
                            asf_head[3] = recv[0];
                            // block size算出
                            blk_size = (asf_head[3] << 8) + asf_head[2];

                            // blockサイズ分データが来てるかチェック
                            ava_size = sock.Available;
                            if (ava_size >= blk_size)
                            {// ASF1ブロック分のデータが来ている
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
                                    // END_TRANSの時はSeqNoしか増えない。
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
                                // データ受信
                                while (blk_size > rsp_size)
                                    rsp_size += sock.Receive(blk, 4 + rsp_size, blk_size - rsp_size, SocketFlags.None);
                                // クライアントに送信
                                Status.Client.StreamWrite(blk);
                                rsp_size = 0;   // 中途半端データなし
                                blk = null;     // GCを早めるための明示的null設定
                            }
                            else
                            {// ASF1ブロック分のデータが来ていない
                                // 来てるところまで受信して、受信サイズをblk_lenに設定しておく
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
                                // 途中までのデータ受信
                                while (ava_size > rsp_size)
                                    rsp_size += sock.Receive(blk, 4 + rsp_size, ava_size - rsp_size, SocketFlags.None);
                                // rsp_sizeとblkを保持したまま再ループ
                            }
                        }
                        else
                        {
                            // 読み取り可能データが4byte以下しか無ければ
                            // タイムアウトカウンタをUP
                            Thread.Sleep(50);
                            recv_timeout++;
                            if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                                break;  //Timeout
                        }
                    }
                    else
                    {// 中途半端なデータが有る。足りないブロックを受信
                        ava_size = sock.Available;
                        if (ava_size > (blk_size - rsp_size))
                        {// 足りないブロックサイズ分のデータが全て来ている
                            // 読み取り可能データが有るのでタイムアウトカウンタをリセット
                            recv_timeout = 0;
                            while (blk_size > rsp_size)
                                rsp_size += sock.Receive(blk, 4 + rsp_size, blk_size - rsp_size, SocketFlags.None);
                            // クライアントに送信
                            Status.Client.StreamWrite(blk);
                            rsp_size = 0;   // 中途半端データなし
                            blk = null;     // GCを早めるための明示的null設定
                        }
                        else if (ava_size > 0)
                        {// データは来ているが、１ブロック分を形成するには至らない
                            // 読み取り可能データが有るのでタイムアウトカウンタをリセット
                            recv_timeout = 0;
                            // データ受信。１回だけ。
                            rsp_size += sock.Receive(blk, 4 + rsp_size, ava_size, SocketFlags.None);
                        }
                        else
                        {
                            // データがまったく来ていない
                            // タイムアウトカウンタをUP
                            Thread.Sleep(50);
                            recv_timeout++;
                            if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                                break;  //Timeout
                        }
                    }
                }
                // whileループ抜け
                // 終了理由の判定を行う
                if (Status.RunStatus == false || (Status.ImportStatus == false && once_status == true))
                {
                    // RunStatusがfalseになった(GUIからの切断)、または
                    // once_statusがtrue(正常受信中)にImportStatusがfalse(外部からの切断要求)を受けた場合

                    Front.AddLogData(1, Status, "インポート接続の切断要求を受信しました");
                    // ユーザ指示による切断では切断音を鳴らさない
                    //if (File.Exists(Front.Opt.SndDiscFile))
                    //    PlaySound(Front.Opt.SndDiscFile);
                }
                else if (end_flg)
                {
                    Front.AddLogData(1, Status, "インポート先が配信を停止しました");
                    // エラーカウントは計上しない
                    // IM切断音が設定されていたら別スレッドで再生する
                    if (File.Exists(Front.Opt.SndDiscFile))
                        PlaySound(Front.Opt.SndDiscFile);
                }
                else // if (recv_timeout > Front.Sock.SockRecvTimeout / 50)
                {
                    Front.AddLogData(1, Status, "インポート先からの受信がタイムアウトしました");
                    Status.ImportError++;
                    // IM切断音が設定されていたら別スレッドで再生する
                    if (File.Exists(Front.Opt.SndDiscFile))
                        PlaySound(Front.Opt.SndDiscFile);
                }
            }
            catch (SocketException se)
            {
                Front.AddLogData(1, Status, "インポート受信エラー wsa:" + se.ErrorCode + "/" + se.SocketErrorCode.ToString());
                Status.ImportError++;
                // IM切断音が設定されていたら別スレッドで再生する
                if (File.Exists(Front.Opt.SndDiscFile))
                    PlaySound(Front.Opt.SndDiscFile);
            }
            catch (Exception e)
            {
                Front.AddLogData(1, Status, "インポート受信エラー(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")");
                Status.ImportError++;
                // IM切断音が設定されていたら別スレッドで再生する
                if (File.Exists(Front.Opt.SndDiscFile))
                    PlaySound(Front.Opt.SndDiscFile);
            }
            finally
            {
                Front.AddLogData(0, Status, "エクスポートへの書き出しを終了します");
                Status.ImportStatus = false;
            }
        }
        #endregion

        /// <summary>
        /// 帯域測定タスク
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

                // 平均ビットレートの計算
                // DLSpeedが0だったら、平均ビットレートに反映させない
                if (Status.CurrentDLSpeed != 0)
                {
                    if (Status.TrafficCount == 0)
                    {
                        Status.AverageDLSpeed = Status.CurrentDLSpeed;
                        Status.TrafficCount++;
                    }
                    else if (Status.TrafficCount < 10)    ///10回=30秒平均
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
        /// 指定されたwavファイルを非同期で再生する
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
