using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using System.Collections;
using System.Diagnostics;
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
                string str = "";
                Match index = Regex.Match(ImportURL, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:");
                if (index.Success)
                {
                    str = ImportURL.Substring(index.Index, index.Length - 1);
                }
                else
                {
                    index = Regex.Match(ImportURL, @"http:\/\/[a-z0-9A-Z._-]+:");
                    if (index.Success)
                    {
                        str = ImportURL.Substring(index.Index + 7, index.Length - 8);
                    }
                    index = Regex.Match(ImportURL, @"mms:\/\/[a-z0-9A-Z._-]+:");
                    if (index.Success)
                    {
                        str = ImportURL.Substring(index.Index + 6, index.Length - 7);
                    }
                    if (str == "")
                    {
                        str = ImportURL.Split(':')[0];
                    }
                    if (str == "localhost")
                        str = "127.0.0.1";
                }
                return str;
            }
        }

        /// <summary>
        /// Import先Port番号
        /// </summary>
        public int ImportPort
        {
            get
            {
                Match index = Regex.Match(ImportURL, @":\d{1,5}");
                return int.Parse(ImportURL.Substring(index.Index + 1, index.Length - 1));
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
        /// 接続最大数履歴
        /// </summary>
        public int ConnectionMax = 0;
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
        /// Kagami.exe方式リザーブを使う(未実装)
        /// </summary>
        public bool KagamiexeReserve;
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
        /// trueの場合鏡が終了したときに外部待ち受けも終了する
        /// </summary>
        public bool ListenStop = false;
        /// <summary>
        /// 新規接続制限フラグ(ポート個別)
        /// </summary>
        public bool Pause = false;
        /// <summary>
        /// Importソースを正常に受信していればtrue
        /// </summary>
        public bool ImportStatus = false;
        /// <summary>
        /// Push配信専用フラグ
        /// </summary>
        public bool PushOnly = false;
        /// <summary>
        /// データ受信バイト数
        /// (３秒毎にリセット)
        /// </summary>
        public int RecvByte = 0;
        /// <summary>
        /// 鏡間リンク管理クラス(サーバ)
        /// </summary>
        public KagamiLink IKLink = null;
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
        #region キック管理クラス
        /// <summary>
        /// エクスポートに対するキック管理を行うクラス
        /// </summary>
        public class KICK
        {
            #region メンバー変数
            /// <summary>
            /// IP
            /// </summary>
            public string IP;
            /// <summary>
            /// 最初の接続をした時間
            /// </summary>
            public DateTime StartTime;
            /// <summary>
            /// 接続試行回数
            /// </summary>
            public int Cnt ;
            /// <summary>
            /// 接続拒否回数
            /// </summary>
            public int Cnt_out;
            /// <summary>
            /// 拒否開始時刻
            /// </summary>
            public DateTime DenyTime;
            /// <summary>
            /// 拒否終了時間(秒)
            /// 無制限の場合は-1
            /// </summary>
            public int DenyEndTime;
            
            /// <summary>
            /// 拒否中フラグ
            /// </summary>
            public bool KickFlag;
            /// <summary>
            /// 自動キックフラグ.falseの場合手動登録
            /// </summary>
            public bool AutoKick;

            /// <summary>
            /// 検査終了時刻
            /// </summary>
            public DateTime ResetTime
            {
                get { return StartTime.AddSeconds(Front.Kick.KickCheckSecond); }

            }


            #endregion
        }
        #endregion

        public Dictionary<string,KICK> Kick = new Dictionary<string,KICK>();
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
        /// 認証ID
        /// </summary>
        public string AuthID = "";
        /// <summary>
        /// 認証Pass
        /// </summary>
        public string AuthPass = "";
        /// <summary>
        /// インポート認証時の要表示ラベル
        /// </summary>
        public string ImportAuthLabel = "";
        /// <summary>
        /// インポートの認証ID
        /// </summary>
        public string ImportAuthID = "";
        /// <summary>
        /// インポートの認証パス
        /// </summary>
        public string ImportAuthPass = "";

        /// <summary>
        /// WEB優先子鏡転送許可
        /// </summary>
        public bool TransWeb = Front.Opt.TransKagamin;
        /// <summary>
        /// ヘッダ取得応答/HTTP1.0
        /// </summary>
        public byte[] HeadRspMsg10 = null;
        /// <summary>
        /// ヘッダ取得応答/HTTP1.1
        /// </summary>
        //public byte[] HeadRspMsg11 = null;
        /// <summary>
        /// ヘッダストリーム情報
        /// </summary>
        public byte[] HeadStream = null;
        /// <summary>
        /// データ取得応答/HTTP1.0
        /// </summary>
        public byte[] DataRspMsg10 = null;
        /// <summary>
        /// データ取得応答/HTTP1.1
        /// </summary>
        //public byte[] DataRspMsg11 = null;
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
        /// <summary>
        /// ストリームヘッダに記述されてるTitle
        /// </summary>
        public string ASFTitle = "";
        /// <summary>
        /// ストリームヘッダに記述されてるAuthor
        /// </summary>
        public string ASFAuthor = "";
        /// <summary>
        /// ストリームヘッダに記述されてるCopyRight
        /// </summary>
        public string ASFCopyRight = "";
        /// <summary>
        /// ストリームヘッダに記述されてるDescription
        /// </summary>
        public string ASFDescription = "";
        /// <summary>
        /// ストリームヘッダに記述されてるRating
        /// </summary>
        public string ASFRating = "";
        /// <summary>
        /// 解像度縦
        /// </summary>
        public int MediaHeight = 0;
        /// <summary>
        /// 解像度横
        /// </summary>
        public int MediaWidth = 0;
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
        /// 子鏡転送回数
        /// </summary>
        public int TransCount = 0;
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
        /// 動画最大ビットレート(Kbps)
        /// </summary>
        public int MaxVideoBitRate = 0;
        /// <summary>
        /// 音声最大ビットレート(Kbps)
        /// </summary>
        public int MaxAudioBitRate = 0;
        /// <summary>
        /// 選択中のビットレート(Videoマルチビットレート用Kbps)
        /// </summary>
        public int NowBitRateVideo = 0;
        /// <summary>
        /// 選択中のビットレート(Audioマルチビットレート用Kbps)
        /// </summary>
        public int NowBitRateAudio = 0;
        /// <summary>
        /// 選択中のビットレートのID
        /// </summary>
        public int MultiID = 0;
        /// <summary>
        /// マルチビットレート切り替え用(true=切り替えリトライ)
        /// </summary>
        public bool SelectMulti = false;
        /// <summary>
        /// マルチビットレート切り替え一時用(true=切り替えリトライ)
        /// </summary>
        public bool tempMulti = false;

        /// <summary>
        /// 現在の接続でのエクスポート先への合計UpSize
        /// </summary>
        public ulong TotalUPSize = 0;
        /// <summary>
        /// 現在の接続でのインポート元からの合計DownSize
        /// </summary>
        public ulong TotalDLSize = 0;
        /// <summary>
        /// クライアントに認証を要求するか
        /// </summary>
        public bool ExportAuth = false;
        ////
        //public bool VirtualHost = false;
        //public bool SQLOn = false;
        ////
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
            AuthID = Front.Opt.AuthID;
            AuthPass = Front.Opt.AuthPass;
            // ImportURLが空なら外側接続モードで起動
            if (ImportURL == "")
            {
                ImportURL = "待機中";
                Type = 1; // 外側接続
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
                Kick.Clear();
                Gui.KickItem.Clear();
                CurrentDLSpeed = 0;
                AverageDLSpeed = 0;
                TrafficCount = 0;
                MaxDLSpeed = 0;
                // 最大接続数をユーザ指定値に戻す
                Connection = Conn_UserSet;
                Event.EventUpdateKick(Kagami, null, 2);
            }
        }
        /// <summary>
        /// マルチビットレート(video)のIDを一つ進めます。
        /// SelectedAudioRecord,Status.SelectedVideoRecordを所定のIDへ変更して
        /// 再接続をします。
        /// </summary>
        /// <returns>ビットレート</returns>
        public void MultiVideo()
        {
            bool select = false;
            /*
            if (SelectedVideoRecord + 1 > StreamSwitchCount)
            {
                for (int cnt = 1; cnt <= StreamSwitchCount; cnt++)
                {
                    if ((StreamType[cnt - 1] == 1))
                    {
                        SelectedVideoRecord = cnt;
                        select = true;
                        SelectMulti = true;
                        tempMulti = true;
                        NowBitRateVideo = StreamBitrate[cnt-1] / 1000;
                    }
                }
                return;


            }*/
            int _temp = SelectedVideoRecord;
            for (int cnt = SelectedVideoRecord+1; cnt <= StreamSwitchCount; cnt++)
            {
                if ((StreamType[cnt-1] == 1))
                {
                    if (_temp == cnt)
                        return;
                    SelectedVideoRecord = cnt;
                    select = true;
                    SelectMulti = true;
                    tempMulti = true;
                    NowBitRateVideo = StreamBitrate[cnt-1] / 1000;
                    return;
                }
            }
            if (!select)
            {
                for (int cnt = 1; cnt <= StreamSwitchCount; cnt++)
                {
                    if ((StreamType[cnt - 1] == 1))
                    {
                        if (_temp == cnt)
                            return;
                        SelectedVideoRecord = cnt;
                        select = true;
                        SelectMulti = true;
                        tempMulti = true;
                        NowBitRateVideo = StreamBitrate[cnt - 1] / 1000;
                        return;
                    }
                }

            }

        }
        /// <summary>
        /// マルチビットレート(audio)のIDを一つ進めます。
        /// SelectedAudioRecord,Status.SelectedVideoRecordを所定のIDへ変更して
        /// 再接続をします。
        /// </summary>
        /// <returns>ビットレート</returns>
        public void Multiaudio()
        {
            bool select = false;
            /*
            if (SelectedAudioRecord + 1 > StreamSwitchCount)
            {
                for (int cnt = 1; cnt <= StreamSwitchCount; cnt++)
                {
                    if ((StreamType[cnt - 1] == 0))
                    {
                        SelectedAudioRecord = cnt;
                        select = true;
                        SelectMulti = true;
                        tempMulti = true;
                        NowBitRateAudio = StreamBitrate[cnt - 1] / 1000;
                    }
                }
                return;


            }*/
            int _temp = SelectedAudioRecord;
            for (int cnt = SelectedAudioRecord+1; cnt <= StreamSwitchCount; cnt++)
            {
                if ((StreamType[cnt-1] == 0))
                {
                    if (_temp == cnt)
                        return;
                    SelectedAudioRecord = cnt;
                    select = true;
                    SelectMulti = true;
                    tempMulti = true;
                    NowBitRateAudio= StreamBitrate[cnt-1] / 1000;
                    return;
                }
            }
            if (!select)
            {
                for (int cnt = 1; cnt <= StreamSwitchCount; cnt++)
                {
                    if ((StreamType[cnt - 1] == 0))
                    {
                        if (_temp == cnt)
                            return;
                        SelectedAudioRecord = cnt;
                        select = true;
                        SelectMulti = true;
                        tempMulti = true;
                        NowBitRateAudio= StreamBitrate[cnt - 1] / 1000;
                        return;
                    }
                }

            }


        }
        #region クライアント追加/削除関係
        /// <summary>
        /// ClientItemにデータ追加＋GUI更新
        /// </summary>
        /// <param name="_cd"></param>
        public void AddClient(ClientData _cd)
        {
            ListViewItem _item = new ListViewItem();
            _item.Text = _cd.Id;                // clmClientViewID
            _item.SubItems.Add("100%");         //Buffer
            _item.SubItems.Add(_cd.Ip);         // clmClientViewIP

            try
            {
                lock (Gui.ReserveItem)
                {
                    foreach (ListViewItem _itemRes in Gui.ReserveItem)
                    {
                        if (_itemRes.Text == _cd.Ip)
                            _item.SubItems[0].ForeColor = System.Drawing.Color.Blue;
                    }
                }
            }
            catch
            {
            }
            _item.SubItems.Add(_cd.UserAgent);  // clmClientViewUA
            //_item.SubItems.Add("");             // Work

            _item.SubItems.Add("00:00:00");     // clmClientViewTime

            _item.SubItems.Add(_cd.Host);       //clmClientViewHost
            _item.SubItems.Add(_cd.ConnInfo);             //KagamiInfo
            //_item.SubItems.Add("");             // FQDN
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
                _item.SubItems[0].ForeColor = System.Drawing.Color.Red;
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
        /// キックIPを新規登録。登録済みの場合更新
        /// </summary>
        /// <param name="_ip">設定するIP</param>
        /// <param name="_cnt">拒否する時間.0の場合は登録のみ。-1の場合は無制限</param>
        /// <param name="_user">ユーザー登録フラグ</param>
        public void AddKick(string _ip, int _time, bool _user)
        {
            lock (Kick)
                lock (Gui.KickItem)
                {
                    // ipが正しい形式かのチェックは呼び出し元でやっておくこと。


                    try
                    {
                        //キックアイテムに含まれているか
                        if (!CheckKickItem(_ip))
                        {//含まれていない場合GUIに新規登録

                            //内部Kick管理にない場合登録
                            if (!Kick.ContainsKey(_ip))
                            {
                                Kick[_ip] = new KICK();
                                Kick[_ip].IP = _ip;

                                if (_time != 0)//0以外の場合拒否中に
                                    Kick[_ip].KickFlag = true;
                                else//0の場合登録のみ
                                    Kick[_ip].KickFlag = false;
#if DEBUG
                                Front.AddKickLog(Kagami.Status, "addkick1");
#endif
                                Kick[_ip].StartTime = DateTime.Now;
                                Kick[_ip].Cnt = 0;
                                Kick[_ip].AutoKick = false;//登録されてない場合は手動登録しかない
                                Kick[_ip].DenyTime = DateTime.Now;
                                Kick[_ip].DenyEndTime = _time;
                            }
                            else if(_user)//登録済みの場合で手動登録の場合
                            {
                                Kick[_ip].AutoKick = _user;
                                ResetKick(_ip, _time);
                            }

                            
                            // KickItemへの登録
                            ListViewItem _item = new ListViewItem();
                            _item.Text = _ip;

                            if (_time == 0)
                                _item.SubItems.Add("-");                // clmKickViewStatus
                            else
                                _item.SubItems.Add(_time.ToString());

                            _item.SubItems.Add("1");
                            Gui.KickItem.Add(_item);
                            Event.EventUpdateKick(Kagami, _item, 0);

                            Kagami.Status.Client.UpdateKickTime();
                        }
                        else
                        {
                            if (_user)//手動登録の場合
                            {
                                Kick[_ip].AutoKick = _user;
                                ResetKick(_ip, _time);

                            }
                            foreach (ListViewItem _item in Gui.KickItem)
                            {
                                Event.EventUpdateKick(Kagami, _item, 0);

                            }

                        }

                        // GUIへの反映

                    }
                    catch (Exception ex)
                    {
                        Front.AddLogDebug("AddKick", "内部エラー Trace:" + ex.StackTrace);
                    }
                }
        }
        /// <summary>
        /// ハッシュに登録済みのIPに拒否時間をセットする
        /// 拒否時間が1以上の場合キック中にフラグを切り替える
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_denytime"></param>
        /// <returns></returns>
        private void ResetKick(string _ip, int _denytime)
        {
            Kick[_ip].DenyTime = DateTime.Now;
            Kick[_ip].DenyEndTime = _denytime;
            if (_denytime != 0)//0以外の場合拒否中に
                Kick[_ip].KickFlag = true;
            else//0の場合登録のみ
                Kick[_ip].KickFlag = false;
        }

        /// <summary>
        /// KickItem中にIPがあるかどうか
        /// </summary>
        /// <param name="_ip"></param>
        /// <returns></returns>
        private bool CheckKickItem(string _ip)
        {
            foreach (ListViewItem _item in Gui.KickItem)
            {
                if (_item.Text == _ip)
                    return true;

            }
            return false;

        }
        /// <summary>
        /// 手動キック登録
        /// </summary>
        /// <param name="_ip">IP</param>
        /// <param name="_time">拒否時間</param>
        public void AddKick(string _ip, int _time)
        {
            AddKick(_ip, _time, true);

        }
        /// <summary>
        /// 自動キック登録
        /// </summary>
        /// <param name="_ip">IP</param>
        public void AddKick(string _ip)
        {
            AddKick(_ip, (int)Front.Kick.KickDenyTime, false);

        }
        
        /// <summary>
        /// 内部キック管理からキックIPを解除.GUIは関係なし
        /// </summary>
        /// <param name="_ip">解除するIP</param>
        public void DelKick(string _ipl)
        {
            lock (Kick)
            {
                // ipが正しい形式かのチェックは呼び出し元でやっておくこと。
                try
                {
                    if (!Kick.ContainsKey(_ipl))
                        return;//含まれていない場合は何もしない

                    

                    Kick[_ipl].KickFlag = false;
                    Kick[_ipl].DenyEndTime = 0;
                    Kick[_ipl].Cnt = 0;

                }
                catch (Exception ex)
                {
                    Front.AddLogDebug("DelKick", "内部エラー Trace:" + ex.StackTrace);
                }

            }
        }
        /// <summary>
        /// Kick対象のIPか判定する。Kick対象で無ければtrue返却
        /// </summary>
        /// <param name="sock"></param>
        /// <returns></returns>
        public bool IsKickCheck(System.Net.Sockets.Socket sock)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                //IPアドレス
                string _ip = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();

                //リザーブ登録されていればKickチェック無し.キック管理も初期化
                lock (Gui.ReserveItem)
                    foreach (ListViewItem _item in Gui.ReserveItem)
                        if (_item.Text == _ip)
                        {
                            if (Kick.ContainsKey(_ip))
                            {
                                Kick[_ip].DenyEndTime = 0;
                                Kick[_ip].Cnt = 0;
                                Kick[_ip].KickFlag = false;
                            }
                            return true;

                        }



                //KickList中にIPがあるかチェック
                //含まれてない場合キック対象ではない
                if (!Kick.ContainsKey(_ip))
                {
                    //なければ新規登録
                    KICK Kick_Client = new KICK();
                    Kick_Client.IP = _ip;
                    Kick_Client.KickFlag = false;
                    Kick_Client.StartTime = DateTime.Now;
                    Kick_Client.Cnt = 1;
                    Kick_Client.Cnt_out = 0;
                    Kick_Client.AutoKick = false;
#if DEBUG
                    Front.AddKickLog(Kagami.Status, "Iskickccheck1");
#endif
                    Kick_Client.DenyTime = DateTime.Now;
                    Kick_Client.DenyEndTime = 0; ;

                    //ハッシュに登録
                    Kick[_ip] = Kick_Client;
                    return true;
                }
                else
                {//含まれてる場合

                    //KICK _kick = Kick[_ip];

                    //キック中の場合
                    if (Kick[_ip].KickFlag)
                    {

                            //キック時間無制限
                            if (Kick[_ip].DenyEndTime < 0)
                            {
                                Kick[_ip].Cnt++;
                                Kick[_ip].Cnt_out++;
                                return false;
                            }

                            //拒否終了時刻を過ぎている場合リセット
                            if (Kick[_ip].DenyTime.AddSeconds(Kick[_ip].DenyEndTime) < DateTime.Now)
                            {
                                Kick[_ip].KickFlag = false;
                                Kick[_ip].StartTime = DateTime.Now;
                                Kick[_ip].Cnt = 0;
#if DEBUG
                                Front.AddKickLog(Kagami.Status, "Iskickccheck2");
#endif

                                return true;
                            }
                            else
                            {//拒否時間中の場合接続試行回数を増やし終了
                                //Kick[_ip].Cnt++;
                                Kick[_ip].Cnt_out++;
                                return false;

                            }
                        

                    }//キック中でない場合
                    else
                    {


                        try
                        {
                            //りせっと時間に達していない
                            if (Kick[_ip].ResetTime > DateTime.Now)
                            {
                                //キック回数に到達の場合キック開始
                                if (Kick[_ip].Cnt >= Front.Kick.KickCheckTime)
                                {
                                    Kick[_ip].DenyTime = DateTime.Now;
                                    Kick[_ip].DenyEndTime = (int)Front.Kick.KickDenyTime;
                                    Kick[_ip].Cnt = 0;
                                    Kick[_ip].Cnt_out++;
                                    Kick[_ip].KickFlag = true;
                                    AddKick(_ip);



                                    return false;

                                }
                                else//キック回数に到達していない場合は回数を+1
                                {
                                    Kick[_ip].Cnt++;

                                    return true;

                                }

                            }
                            else//リセット時間に達しているリセット
                            {
                                Kick[_ip].Cnt = 0;
                                Kick[_ip].StartTime = DateTime.Now;
                                Kick[_ip].KickFlag = false;
                                return true;
                            }
                        }
                        finally
                        {

                        }


                    }

                }
            }
            finally
            {
                sw.Stop();
                //Front.AddLogData(1, this, "1:" + sw.ElapsedMilliseconds);

            }
        }

        #endregion

    }

}
