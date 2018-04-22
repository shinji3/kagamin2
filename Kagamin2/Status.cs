using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Drawing;

namespace Kagamin2
{
    /// <summary>
    /// 鏡毎に作成されるデータクラス
    /// インポートタスクとエクスポートタスクの両方から参照されるデータ
    /// </summary>
    public class Status
    {
        #region メンバ変数

        /// <summary>
        /// 管理元Kagamiクラス
        /// </summary>
        public Kagami Kagami;
        /// <summary>
        /// Import接続先ホスト名：PORT番号
        /// </summary>
        public string ImportURL;

        /// <summary>
        /// Import先ホスト名
        /// </summary>
        public string ImportHost
        {
            get
            {
                string ImportHost = new Uri(ImportURL).DnsSafeHost;
                return Dns.GetHostAddresses(ImportHost)[0].ToString();
            }
        }

        /// <summary>
        /// Import先Port番号
        /// </summary>
        public int ImportPort
        {
            get
            {
                return new Uri(ImportURL).Port;
            }
        }

        /// <summary>
        /// 鏡用自待機Port番号
        /// </summary>
        public int MyPort;

        /// <summary>
        /// 通常接続最大数
        /// </summary>
        public int Connection;

        /// <summary>
        /// ユーザがGUI上から設定した通常接続最大数
        /// </summary>
        public int Conn_UserSet;

        /// <summary>
        /// リザーブ接続最大数
        /// </summary>
        public int Reserve;

        /// <summary>
        /// この鏡に接続しているクライアントデータ
        /// </summary>
        public Client Client;

        /// <summary>
        /// 内側/外側接続種別
        /// 0=内側 1=外側 2=Push配信
        /// </summary>
        public int Type;

        /// <summary>
        /// 0:pull有効
        /// 1:pull無効(push only)
        /// </summary>
        public bool DisablePull;

        /// <summary>
        /// ポートListen状態フラグ
        /// Push配信ポートとExportポートの切替で利用
        /// </summary>
        public bool ListenPort;

        /// <summary>
        /// 鏡起動中(外部接続待ちを含む)はtrueを保持。
        /// 鏡を停止したい時にfalseにする
        /// </summary>
        public bool RunStatus = true;

        /// <summary>
        /// Importソースを正常に受信していればtrue
        /// </summary>
        public bool ImportStatus = false;

        /// <summary>
        /// データ受信バイト数
        /// (３秒毎にリセット)
        /// </summary>
        public int RecvByte = 0;

        #region GUI関連メンバ
        public class GUI
        {
            /// <summary>
            /// kagamiView出力用のItem
            /// </summary>
            public ListViewItem KagamiItem = new ListViewItem();

            /// <summary>
            /// clientView出力用の内部リスト
            /// </summary>
            public List<ListViewItem> ClientItem = new List<ListViewItem>();

            /// <summary>
            /// reserveView出力用の内部リスト
            /// </summary>
            public List<ListViewItem> ReserveItem = new List<ListViewItem>();

            /// <summary>
            /// kickView出力用の内部リスト
            /// </summary>
            public List<ListViewItem> KickItem = new List<ListViewItem>();

            /// <summary>
            /// logView出力用の内部リスト(全ログ表示用)
            /// </summary>
            public List<ListViewItem> LogAllItem = new List<ListViewItem>();

            /// <summary>
            /// logView出力用の内部リスト(重要ログのみ表示用)
            /// </summary>
            public List<ListViewItem> LogImpItem = new List<ListViewItem>();

        }
        public GUI Gui = new GUI();
        #endregion

        /*
         * Web Form情報保持メンバ
         */
        /// <summary>
        /// URL表示ON/OFF
        /// </summary>
        public bool UrlVisible = true;
        /// <summary>
        /// エントランス設定用パスワード
        /// </summary>
        public string Password = "";
        /// <summary>
        /// エントランス表示用コメント
        /// </summary>
        public string Comment = "";
        /// <summary>
        /// 外部接続を行った人のIPアドレス
        /// </summary>
        public string SetUserIP = "";
        /// <summary>
        /// 実況スレッドURL
        /// </summary>
        public string Url = "";
        /// <summary>
        /// 子鏡へのリダイレクト許容フラグ
        /// </summary>
        public bool EnableRedirectChild = false;
        /// <summary>
        /// 親へのリダイレクト許容フラグ
        /// </summary>
        public bool EnableRedirectParent = false;
        /// <summary>
        /// 新規接続制限フラグ(ポート個別)
        /// </summary>
        public bool Pause = false;

        /// <summary>
        /// ヘッダ取得応答/HTTP1.0
        /// </summary>
        public byte[] HeadRspMsg10 = null;
        /// <summary>
        /// ヘッダストリーム情報
        /// </summary>
        public byte[] HeadStream = null;
        /// <summary>
        /// データ取得応答/HTTP1.0
        /// </summary>
        public byte[] DataRspMsg10 = null;
        /// <summary>
        /// StreamSwitchCount
        /// </summary>
        public int StreamSwitchCount = 0;
        /// <summary>
        /// StreamType
        /// 配列要素番号がストリーム番号-1
        /// 値が0だとAudio,1だとVideo,2だとその他を示す
        /// </summary>
        public int[] StreamType;
        /// <summary>
        /// StreamBitrate
        /// 配列要素番号がストリーム番号-1
        /// 値はビットレート(bps)
        /// </summary>
        public int[] StreamBitrate;
        /// <summary>
        /// このインポート接続で利用する
        /// Audioストリームのストリーム番号
        /// </summary>
        public int SelectedAudioRecord = 0;
        /// <summary>
        /// このインポート接続で利用する
        /// Videoストリームのストリーム番号
        /// </summary>
        public int SelectedVideoRecord = 0;

        /*
         * 統計情報
         */
        /// <summary>
        /// ビジーカウンター
        /// </summary>
        public int BusyCounter = 0;
        /// <summary>
        /// インポートエラーカウンタ
        /// </summary>
        public int ImportError = 0;
        /// <summary>
        /// 直後のインポートエラーの内容
        /// </summary>
        public string ImportErrorContext = "";
        /// <summary>
        /// エクスポートエラーカウンタ
        /// </summary>
        public int ExportError = 0;
        /// <summary>
        /// エクスポート接続回数
        /// </summary>
        public int ExportCount = 0;
        /// <summary>
        /// インポート接続リトライ回数カウンタ
        /// </summary>
        public int RetryCounter = 0;
        /// <summary>
        /// インポートタスク起動時刻
        /// </summary>
        public DateTime ImportStartTime = DateTime.Now;
        /// <summary>
        /// インポートタスク起動時間
        /// </summary>
        public TimeSpan ImportTime
        {
            get
            {
                return DateTime.Now - ImportStartTime;
            }
        }
        /// <summary>
        /// インポートタスク起動時間(文字列)
        /// </summary>
        public string ImportTimeString
        {
            get
            {
                TimeSpan _duration = DateTime.Now - ImportStartTime;
                return String.Format("{0:D2}:{1:D2}:{2:D2}", _duration.Hours, _duration.Minutes, _duration.Seconds);
            }
        }
        /// <summary>
        /// クライアント数制限インポート切断用
        /// クライアント数が基準値を満たしていた最終時刻
        /// </summary>
        public DateTime ClientTime = DateTime.Now;

        /*
         * 帯域制限
         */
        /// <summary>
        /// 制限帯域値(個別設定時のGUI設定値)
        /// </summary>
        public int GUILimitUPSpeed = 0;
        /// <summary>
        /// 制限帯域地(kbps)
        /// </summary>
        public int LimitUPSpeed = 0;

        /*
         * 転送量・転送速度情報
         */
        /// <summary>
        /// 現在の下り転送速度実測値(Kbps)
        /// </summary>
        public int CurrentDLSpeed = 0;
        /// <summary>
        /// インポート接続後からの平均下り転送速度実測値(Kbps)
        /// </summary>
        public int AverageDLSpeed = 0;
        /// <summary>
        /// トラフィック収集回数(AveDLSpeed計算用)
        /// </summary>
        public int TrafficCount = 0;
        /// <summary>
        /// インポート元申告の最大帯域(Kbps)
        /// </summary>
        public int MaxDLSpeed = 0;
        /// <summary>
        /// 現在の接続でのエクスポート先への合計UpSize
        /// </summary>
        public ulong TotalUPSize = 0;
        /// <summary>
        /// 現在の接続でのインポート元からの合計DownSize
        /// </summary>
        public ulong TotalDLSize = 0;

        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_kagami"></param>
        /// <param name="_importURL"></param>
        /// <param name="_myPort"></param>
        /// <param name="_connection"></param>
        /// <param name="_reserve"></param>
        public Status(Kagami _kagami, string _importURL, int _myPort, int _connection, int _reserve)
        {
            Kagami = _kagami;
            ImportURL = _importURL;
            ImportURL.ToLower();
            MyPort = _myPort;
            Conn_UserSet = _connection;
            Connection = _connection;
            Reserve = _reserve;
            DisablePull = false;

            // ImportURLが空なら外側接続モードで起動
            if (ImportURL == "")
            {
                ImportURL = "待機中";
                if (Front.Hp.UseHP)
                {
                    Type = 1; // 外側接続
                }
                else if (Front.Opt.EnablePush)
                {
                    Type = 2; // Push配信
                }
            }
            else
            {
                Type = 0; // 内側接続
            }

            Gui.KagamiItem.Text = MyPort.ToString();                            // clmKagamiViewPort
            Gui.KagamiItem.SubItems.Add(ImportURL);                             // clmKagamiViewImport
            Gui.KagamiItem.SubItems.Add("0/" + Connection + "+" + Reserve);     // clmKagamiViewConn

            Client = new Client(this);
        }
        #endregion

        /// <summary>
        /// インポート元及び全クライアントを切断します。
        /// 外側接続の場合は待受け状態に戻ります。
        /// </summary>
        public void Disc()
        {
            if (Type == 0)
            {
                // 内側接続の場合ポート停止
                ImportStatus = false;
                RunStatus = false;
            }
            else
            {
                // 切断のための値設定
                ImportStatus = false;
                ImportURL = "待機中";
                // 各種情報の消去
                BusyCounter = 0;
                RetryCounter = 0;
                Comment = "";
                Password = "";
                SetUserIP = "";
                Gui.ReserveItem.Clear();
                CurrentDLSpeed = 0;
                AverageDLSpeed = 0;
                TrafficCount = 0;
                MaxDLSpeed = 0;
                // 最大接続数をユーザ指定値に戻す
                Connection = Conn_UserSet;
            }
        }

        #region クライアント追加/削除関係
        /// <summary>
        /// ClientItemにデータ追加＋GUI更新
        /// </summary>
        /// <param name="_cd"></param>
        public void AddClient(ClientData _cd)
        {
            string _hostname = "";
            string _hostip = "";
            ListViewItem _item = new ListViewItem();

            try { _hostname = Dns.GetHostEntry(_cd.Ip).HostName; }
            catch { _hostname = _cd.Ip; }

            if (Front.Opt.EnableResolveHost)
                _hostip = _hostname;
            else
                _hostip = _cd.Ip;

            // clientView.Columnsと同期が必要!!
            _item.Text = _cd.Id;                // 0:clmClientViewID
            _item.SubItems.Add(_hostip);        // 1:clmClientViewIpHost
            _item.SubItems.Add(_cd.UserAgent);  // 2:clmClientViewUA
            _item.SubItems.Add("00:00:00");     // 3:clmClientViewTime
            _item.SubItems.Add(_cd.Ip);         // 4:clmClientView_internal_IP
            _item.SubItems.Add(_hostname);      // 5:clmClientView_internal_Host
            // ClientItemに追加
            lock (Gui.ClientItem)
                Gui.ClientItem.Add(_item);
            // クライアント接続通知
            Event.EventUpdateClient(Kagami, _item, 0);
        }

        /// <summary>
        /// 指定されたクライアントIDのデータをClientItemから削除＋GUI更新
        /// 対象IDのクライアントが見つからなければ何もしない
        /// </summary>
        /// <param name="_id"></param>
        public void RemoveClient(string _id)
        {
            lock (Gui.ClientItem)
            {
                foreach (ListViewItem _item in Gui.ClientItem)
                {
                    if (_item.Text == _id)  // clmClientViewID
                    {
                        // ClientItemから削除
                        Gui.ClientItem.Remove(_item);
                        // クライアント切断通知
                        Event.EventUpdateClient(Kagami, _item, 1);
                        break;
                    }
                }
            }
        }
        #endregion

        #region リザーブ関連
        /// <summary>
        /// 指定したIPをReserveItemに新規登録する
        /// </summary>
        /// <param name="_ip"></param>
        public void AddReserve(string _ip)
        {
            // ipが正しい形式かのチェックは呼び出し元でやっておくこと。
            try
            {
                ListViewItem _item = new ListViewItem(_ip);
                _item.SubItems.Add("×");
                _item.SubItems[0].ForeColor = Color.Red;
                // GUIに追加
                Event.EventUpdateReserve(Kagami, _item, 0);
                // 内部管理ReserveItemに追加
                lock (Gui.ReserveItem)
                    Gui.ReserveItem.Add(_item);
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("AddReserve", "内部エラー Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 指定したIPをReserveItemから削除する
        /// </summary>
        /// <param name="_ip"></param>
        public void RemoveReserve(string _ip)
        {
            lock (Gui.ReserveItem)
            {
                foreach (ListViewItem _item in Gui.ReserveItem)
                {
                    if (_item.Text == _ip)
                    {
                        // GUIから削除
                        Event.EventUpdateReserve(Kagami, _item, 1);
                        //ReserveItemから削除
                        Gui.ReserveItem.Remove(_item);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 指定IPが×状態でリザーブ登録されていればtrue
        /// </summary>
        /// <param name="_ip"></param>
        /// <returns></returns>
        public bool IsReserveList(string _ip)
        {
            lock (Gui.ReserveItem)
                foreach (ListViewItem _item in Gui.ReserveItem)
                    if (_item.Text == _ip)                      // clmReserveViewIP
                        if (_item.SubItems[1].Text == "×")     // clmResvViewStatus
                            return true;
            return false;
        }

        /// <summary>
        /// リザーブIPリスト上で○になっているアイテムの数を返却
        /// </summary>
        /// <returns></returns>
        public int ReserveCount
        {
            get
            {
                int cnt = 0;
                lock (Gui.ReserveItem)
                {
                    foreach (ListViewItem _item in Gui.ReserveItem)
                    {
                        if (_item.SubItems[1].Text == "○") // clmResvViewStatus
                            cnt++;
                    }
                }
                return cnt;
            }
            set
            {
            }
        }
        #endregion

        #region キック関連処理
        /// <summary>
        /// キックIPをStatus.Gui.KickItem(=Form1.kickView)に新規登録
        /// Front.KickListに未登録ならそっちにも登録する
        /// GUIにも通知する
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_cnt"></param>
        public void AddKick(string _ip, int _cnt)
        {
            // ipが正しい形式かのチェックは呼び出し元でやっておくこと。
            try
            {
                // Front.KickListへの登録
                if (!Front.KickList.ContainsKey(_ip))
                    Front.KickList.Add(_ip, DateTime.Now + ",1");
                // KickItemへの登録
                ListViewItem _item = new ListViewItem();
                _item.Text = _ip;
                _item.SubItems.Add("-");                // clmKickViewStatus
                _item.SubItems.Add(_cnt.ToString());    // clmKickViewCount
                Gui.KickItem.Add(_item);
                // GUIへの反映
                Event.EventUpdateKick(Kagami, _item, 0);
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("AddKick", "内部エラー Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Kick対象のIPか判定する。Kick対象で無ければtrue返却
        /// </summary>
        /// <param name="sock"></param>
        /// <returns></returns>
        public bool IsKickCheck(Socket sock)
        {
            string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();

            //リザーブ登録されていればKickチェック無し
            lock (Gui.ReserveItem)
                foreach (ListViewItem _item in Gui.ReserveItem)
                    if (_item.Text == _ip)
                        return true;

            //Kickチェックリストに登録済みかチェック
            if (Front.KickList.ContainsKey(_ip))
            {
                //検査終了時間、連続接続回数を取得
                string[] str = Front.KickList[_ip].Split(',');
                DateTime _end_tim = DateTime.Parse(str[0]);
                int _con_cnt = int.Parse(str[1]);
                Front.AddLogDebug("KICKチェック", "KickCheckCount:" + str[1]);
                //連続接続回数が設定回数を超えたかチェック
                if (_con_cnt > Front.Kick.KickCheckTime)
                {
                    //検査期間内に超えたのかチェック
                    if (DateTime.Now < _end_tim)
                    {
                        //検査期間内に超えたのでブロック開始
                        //ブロック終了時間を設定
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickDenyTime).ToString() + ",0";
                        Front.AddLogDebug("KICKチェック", "KickCheckResult:KickStart");
                        return false;
                    }
                    else
                    {
                        //検査期間超過後に超えたので最初からカウントしなおし
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickCheckSecond).ToString() + ",1";
                        Front.AddLogDebug("KICKチェック", "KickCheckResult:CountReset");
                        return true;
                    }
                }
                //ブロック中
                else if (_con_cnt == 0)
                {
                    // ブロック期間中のアクセスかチェック
                    if (DateTime.Now < _end_tim)
                    {
                        //拒否時間内
                        Front.AddLogDebug("KICKチェック", "KickCheckResult:KickPeriodNow");
                        return false;
                    }
                    else
                    {
                        //拒否時間超過、始めからカウントしなおし
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickCheckSecond).ToString() + ",1";
                        Front.AddLogDebug("KICKチェック", "KickCheckResult:KickPeriodEnd");
                        return true;
                    }
                }
                //無期限ブロック中
                else if (_con_cnt < 0)
                {
                    return false;
                }
                //設定回数を越える前
                else
                {
                    //検査期間を超えたかチェック
                    if (DateTime.Now < _end_tim)
                    {
                        //検査期間を超えていなければ連続接続回数カウントアップ
                        Front.KickList[_ip] = _end_tim.ToString() + "," + (_con_cnt + 1);
                        Front.AddLogDebug("KICKチェック", "KickCheckResult:CountUp");
                        return true;
                    }
                    else
                    {
                        //検査期間を超えていれば最初からカウントアップ
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickCheckSecond).ToString() + ",1";
                        Front.AddLogDebug("KICKチェック", "KickCheckResult:CountReset");
                        return true;
                    }
                }
            }
            else
            {
                // 新規にKickチェックリストに登録
                // 検査終了時間をリスト登録する
                Front.AddLogDebug("KICKチェック", "KickCheckResult:AddNewHost");
                Front.KickList.Add(_ip, DateTime.Now.AddSeconds(Front.Kick.KickCheckSecond).ToString() + ",1");
                return true;
            }
            // ここには来ない
        }

        #endregion

    }
}
