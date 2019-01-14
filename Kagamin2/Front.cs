using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Kagamin2
{
    /// <summary>
    /// スケジュール起動用アイテム
    /// </summary>
    public class SCHEDULE
    {
        /// <summary>
        /// スケジュール有効フラグ
        /// </summary>
        public bool Enable;
        /// <summary>
        /// スケジュールイベント
        /// </summary>
        public uint Event;
        /// <summary>
        /// スケジュールポート
        /// 0=全ポート
        /// </summary>
        public uint Port;
        /// <summary>
        /// スケジュール開始条件
        /// 0=時間 1=転送量
        /// </summary>
        public uint StartType;
        /// <summary>
        /// スケジュール開始時間(曜日)
        /// </summary>
        public uint Week;
        /// <summary>
        /// スケジュール開始時間(時)
        /// </summary>
        public uint Hour;
        /// <summary>
        /// スケジュール開始時間(分)
        /// </summary>
        public uint Min;
        /// <summary>
        /// 比較元転送量種別
        /// 0=日間転送量 1=月間転送量
        /// </summary>
        public uint TrfType;
        /// <summary>
        /// 比較転送量値
        /// </summary>
        public uint TrfValue;
        /// <summary>
        /// 比較転送量単位
        /// 0=MB 1=GB
        /// </summary>
        public uint TrfUnit;
        /// <summary>
        /// 転送量指定のイベント実行済みフラグ
        /// </summary>
        public bool ExecTrf;
        /// <summary>
        /// 接続枠(通常枠)
        /// </summary>
        public uint Conn;
        /// <summary>
        /// 接続枠(リザーブ枠)
        /// </summary>
        public uint Resv;

    }

    #region Import/ExportタスクからGUIへのイベント通知ハンドラ

    /// <summary>
    /// Form更新イベントハンドラ用イベントデータクラス
    /// </summary>
    public class KagamiEvent : EventArgs
    {
        /// <summary>
        /// 更新対象Kagami
        /// </summary>
        public Kagami Kagami;
        /// <summary>
        /// 更新対象Item
        /// </summary>
        public ListViewItem Item;
        /// <summary>
        /// 更新モード。
        /// ハンドラ種別毎に利用方法はさまざま。
        /// </summary>
        public int Mode;
#if DEBUG
        /// <summary>
        /// イベント通知元スレッドID(デバッグ用)
        /// </summary>
        public int ThreadId;
#endif
    }

    /// <summary>
    /// staticイベントハンドラ
    /// </summary>
    static public class Event
    {
        /// <summary>
        /// 鏡状態変化通知ハンドラ
        /// </summary>
        public static event EventHandler UpdateKagami;
        /// <summary>
        /// クライアント接続切断通知ハンドラ
        /// </summary>
        public static event EventHandler UpdateClient;
        /// <summary>
        /// リザーブ登録状態変化通知ハンドラ
        /// </summary>
        public static event EventHandler UpdateReserve;
        /// <summary>
        /// クライアントキック通知ハンドラ
        /// </summary>
        public static event EventHandler UpdateKick;
        /// <summary>
        /// ログ出力通知ハンドラ
        /// </summary>
        public static event EventHandler UpdateLog;

        /// <summary>
        /// 鏡状態変化通知
        /// </summary>
        public static void EventUpdateKagami()
        {
            UpdateKagami(null, EventArgs.Empty);
        }

        /// <summary>
        /// クライアント接続切断通知
        /// </summary>
        /// <param name="_kagami"></param>
        /// <param name="_item"></param>
        /// <param name="_mode"></param>
        public static void EventUpdateClient(Kagami _kagami, ListViewItem _item, int _mode)
        {
            if (_kagami == null)
                return;

            KagamiEvent e = new KagamiEvent();
            e.Kagami = _kagami;
            e.Item = _item;
            e.Mode = _mode;
#if DEBUG
            e.ThreadId = System.AppDomain.GetCurrentThreadId();
#endif
            if (UpdateClient != null)
                UpdateClient(null, e);
        }
        /// <summary>
        /// リザーブ登録状態変化通知
        /// _mode=0:追加 1:削除 2:全削除
        /// </summary>
        /// <param name="_kagami"></param>
        /// <param name="_item"></param>
        /// <param name="_mode"></param>
        public static void EventUpdateReserve(Kagami _kagami, ListViewItem _item, int _mode)
        {
            if (_kagami == null)
                return;

            KagamiEvent e = new KagamiEvent();
            e.Kagami = _kagami;
            e.Item = _item;
            e.Mode = _mode;
#if DEBUG
            e.ThreadId = System.AppDomain.GetCurrentThreadId();
#endif
            if (UpdateReserve != null)
                UpdateReserve(null, e);
        }

        /// <summary>
        /// クライアントキック通知
        /// </summary>
        /// <param name="_kagami"></param>
        public static void EventUpdateKick(Kagami _kagami,ListViewItem _item, int _mode)
        {
            if (_kagami == null)
                return;

            KagamiEvent e = new KagamiEvent();
            e.Kagami = _kagami;
            e.Item = _item;
            e.Mode = _mode;
#if DEBUG
            e.ThreadId = System.AppDomain.GetCurrentThreadId();
#endif
            if (UpdateKick != null)
                UpdateKick(null, e);
        }

        /// <summary>
        /// ログ出力通知
        /// </summary>
        /// <param name="_logLv"></param>
        /// <param name="_kagami"></param>
        /// <param name="_item"></param>
        public static void EventUpdateLog(int _logLv, Kagami _kagami, ListViewItem _item)
        {
            if (_kagami == null)
                return;

            KagamiEvent e = new KagamiEvent();
            e.Kagami = _kagami;
            e.Item = _item;
            e.Mode = _logLv;
#if DEBUG
            e.ThreadId = System.AppDomain.GetCurrentThreadId();
#endif
            if (UpdateLog != null)
                UpdateLog(null, e);
        }
    }
    #endregion

    /// <summary>
    /// staticフロント部
    /// </summary>
    static public class Front
    {
        #region 設定を保存するメンバ変数
        /// <summary>
        /// Form関連データ
        /// </summary>
        public struct FORM
        {
            /// <summary>
            /// 前回終了時のウインドウ表示位置X
            /// </summary>
            public int X;
            /// <summary>
            /// 前回終了時のウインドウ表示位置Y
            /// </summary>
            public int Y;
            /// <summary>
            /// 前回終了時のウインドウ幅W
            /// </summary>
            public int W;
            /// <summary>
            /// 前回終了時のウインドウ高H
            /// </summary>
            public int H;
            /// <summary>
            /// 前回終了時のsplitter1の幅
            /// </summary>
            public uint SplitDistance1;
            /// <summary>
            /// 前回終了時のsplitter2の幅
            /// </summary>
            public uint SplitDistance2;
            /// <summary>
            /// LeftPanelの縮小OnOff
            /// </summary>
            public bool LeftPanelCollapsed;
            /// <summary>
            /// kagamiViewの各カラム幅(カンマ区切り)
            /// </summary>
            public string KagamiListColumn;
            /// <summary>
            /// monAllViewの各カラム幅(カンマ区切り)
            /// </summary>
            public string MonAllViewColumn;
            /// <summary>
            /// monViewの各カラム幅(カンマ区切り)
            /// </summary>
            public string MonViewColumn;
            /// <summary>
            /// clientViewの各カラム幅(カンマ区切り)
            /// </summary>
            public string ClientViewColumn;
            /// <summary>
            /// reserveViewの各カラム幅(カンマ区切り)
            /// </summary>
            public string ResvViewColumn;
            /// <summary>
            /// kickViewの各カラム幅(カンマ区切り)
            /// </summary>
            public string KickViewColumn;
            /// <summary>
            /// logViewの各カラム幅(カンマ区切り)
            /// </summary>
            public string LogViewColumn;
            /// <summary>
            /// スケジュールの各カラム幅(カンマ区切り)
            /// </summary>
            public string ScheduleColumn;
            /// <summary>
            /// モニタ上の速度表示単位
            /// 0:kbps 1:KB/s
            /// </summary>
            public uint monViewUnit;
            /// <summary>
            /// アイコンインデックス
            /// </summary>
            public uint IconIndex;
        }
        static public FORM Form;

        /// <summary>
        /// GUI上のデータ
        /// </summary>
        public struct GUI
        {
            /// <summary>
            /// GUI上のデフォルトImportURL
            /// </summary>
            public string ImportURL;
            /// <summary>
            /// お気に入りImportURLリスト
            /// </summary>
            public List<string> FavoriteList;
            /// <summary>
            /// お気に入りのリザーブリスト
            /// </summary>
            public List<string> ReserveList;
            /// <summary>
            /// 各ポートの接続数設定保存用
            /// </summary>
            public Dictionary<int, int> Conn_NumList;
            /// <summary>
            /// 各ポートのリザーブ数設定保存用
            /// </summary>
            public Dictionary<int, int> Reserve_NumList;
            /// <summary>
            /// GUI上のポートリスト
            /// </summary>
            public List<int> PortList;
            /// <summary>
            /// スケジュール接続数セット
            /// </summary>
            public List<int> SetList;
            /// <summary>
            /// GUI上のデフォルト通常接続数
            /// </summary>
            public uint Conn;
            /// <summary>
            /// GUI上のデフォルトリザーブ枠数
            /// </summary>
            public uint Reserve;
        }
        static public GUI Gui;

        /// <summary>
        /// エントランス情報
        /// </summary>
        public struct HP
        {
            /// <summary>
            /// 鏡置き場エントランス使用有無
            /// </summary>
            public bool UseHP;
            /// <summary>
            /// エントランス表示用ホスト名
            /// </summary>
            public string IpHTTP;
            /// <summary>
            /// エントランス表示用ポート番号
            /// </summary>
            public uint PortHTTP;
            /// <summary>
            /// エントランス公開Dir
            /// </summary>
            public string PublicDir;
            /// <summary>
            /// グローバルIPを取得するための確認君URL
            /// </summary>
            public string IPCheckURL;
            /// <summary>
            /// 確認君のHTMLソース内で、
            /// グローバルIPが書いてある行番号
            /// </summary>
            public uint IPCheckLine;
            /// <summary>
            /// 起動時に自動的にエントランスを起動するか
            /// </summary>
            public bool HPstartON;
            /// <summary>
            /// 起動時にポート待ち受けをするかどうか
            /// </summary>
            public bool PortstartON;
            /// <summary>
            /// 起動時に待機状態にするポートリスト
            /// </summary>
            public List<int> StartPortList;

            /// <summary>
            /// HP認証のユーザー名
            /// </summary>
            public string AuthUser;
            /// <summary>
            /// HP認証のパスワード
            /// </summary>
            public string AuthPass;
            /// <summary>
            /// HP_Basic認証を行うか
            /// </summary>
            public bool AuthEnable;
            public bool allh;
            public bool porth;
        }
        static public HP Hp;

        /// <summary>
        /// ソケット関連データ
        /// </summary>
        public struct SOCK
        {
            /// <summary>
            /// ソケット接続タイムアウト時間(ms)
            /// </summary>
            public uint SockConnTimeout;
            /// <summary>
            /// インポート受信タイムアウト時間(ms)
            /// </summary>
            public uint SockRecvTimeout;
            /// <summary>
            /// エクスポート送信タイムアウト時間(ms)
            /// </summary>
            public uint SockSendTimeout;
            /// <summary>
            /// エクスポート送信キュー閾値
            /// </summary>
            public uint SockSendQueueSize;
            /// <summary>
            /// ソケット切断までの遅延時間(ms)
            /// </summary>
            public uint SockCloseDelay;
            /// <summary>
            /// UPnP有効フラグ
            /// </summary>
            public bool upnp;
            /// <summary>
            /// ホスト限定
            /// </summary>
            public bool VirtualHost;
            /// <summary>
            /// ビジーチェッカー人数情報送信
            /// </summary>
            public bool ConnInfoSend;
        }
        static public SOCK Sock;

        /// <summary>
        /// リトライ回数データ
        /// </summary>
        public struct RETRY
        {
            /// <summary>
            /// 内側接続リトライ回数
            /// </summary>
            public uint InRetryTime;
            /// <summary>
            /// 内側接続再接続待ち時間(秒)
            /// </summary>
            public uint InRetryInterval;
            /// <summary>
            /// 外側接続リトライ回数
            /// </summary>
            public uint OutRetryTime;
            /// <summary>
            /// 外側接続再接続待ち時間(秒)
            /// </summary>
            public uint OutRetryInterval;
        }
        static public RETRY Retry;

        /// <summary>
        /// 各種オプション機能フラグ
        /// </summary>
        public struct OPT
        {
            /// <summary>
            /// kagami.exe優先モード有効フラグ
            /// </summary>
            public bool PriKagamiexe;
            /// <summary>
            /// かがみん優先モード有効フラグ
            /// </summary>
            public bool PriKagamin;
            /// <summary>
            /// BrowserViewモード有効フラグ
            /// </summary>
            public bool BrowserView;
            /// <summary>
            /// ブラウザビュー表示モード(false=TEXT,true=HTML)
            /// </summary>
            public bool BrowserViewMode;
            /// <summary>
            /// タスクトレイBalloonTip表示有効フラグ
            /// </summary>
            public bool BalloonTip;
            /// <summary>
            /// プッシュ配信サーバ機能有効フラグ
            /// </summary>
            public bool EnablePush;
            /// <summary>
            /// /info.html有効フラグ
            /// </summary>
            public bool EnableInfo;
            /// <summary>
            /// /admin.html有効フラグ
            /// </summary>
            public bool EnableAdmin;
            /// <summary>
            /// 管理者モードパスワード
            /// </summary>
            public string AdminPass;
            /// <summary>
            /// WMPコンポーネントプレビューON
            /// </summary>
            public bool ViewWMP;
/*
            /// <summary>
            /// /rss.rdf有効フラグ
            /// </summary>
            public bool EnableRss;
            /// <summary>
            /// RSS配信時のタイトル
            /// </summary>
            public string RssTitle;
            /// <summary>
            /// RSS配信時のURL
            /// </summary>
            public string RssUrl;
 */
            /// <summary>
            /// IM接続OK時再生音
            /// </summary>
            public string SndConnOkFile;
            /// <summary>
            /// IM接続NG時再生音
            /// </summary>
            public string SndConnNgFile;
            /// <summary>
            /// IM切断時再生音
            /// </summary>
            public string SndDiscFile;
            ///<summary>
            /// 実況URL制限
            ///</summary>
            public List<string> OutUrl;

            public string OutUrlstr
            {
                get
                {
                    string str = "";
                    foreach (string _str in OutUrl)
                        str += _str+",";
                    if (str.Length >= 1)
                        str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
                    return str;
                }
            }
            public bool OutUrlContain(string _c)
            {
                foreach (string _str in OutUrl)
                {
                    try
                    {
                        Regex re = new Regex(_str);
                        if (re.IsMatch(_c))
                            return true;
                    }
                    catch { }
                    if (_c.StartsWith(_str))
                        return true;
                }
                return false;

            }

            /// <summary>
            /// アプリケーションネーム。バージョン後に付記
            /// </summary>
            public string AppName;

            /// <summary>
            /// 子鏡へ転送を行うか
            /// </summary>
            public bool TransKagamin;
            /// <summary>
            /// 自分への転送を許可しないか
            /// </summary>
            public bool NotMyTrans;
            /// <summary>
            /// 転送階層深さ
            /// </summary>
            public uint TransCont;
            /// <summary>
            /// インポートのリダイレクトを許可するか
            /// </summary>
            public bool ImportRedirect;
            /// <summary>
            /// 最小化時にタスクトレイへ入れるか
            /// </summary>
            public bool InTrayOn;

            /*
            /// <summary>
            /// MySQLのホスト
            /// </summary>
            public string HostMySQL;
            /// <summary>
            /// MySQLのユーザー名
            /// </summary>
            public string UserMuSQL;
            /// <summary>
            /// MySQLのパスワード
            /// </summary>
            public string PassMySQL;
            /// <summary>
            /// MySQL呼び出し列
            /// </summary>
            public string ReadRowMySQL;
            /// <summary>
            /// MySQL呼び出し列(IP)
            /// </summary>
            public string ReadRowIPMySQL;
            /// <summary>
            /// MySQL連携有効
            /// </summary>
            public bool MySQLEnable;
            /// <summary>
            /// 接続データベース名
            /// </summary>
            public string RearDB;
            /// <summary>
            /// 読み出し表名
            /// </summary>
            public string ReadHyou;
            */
            /// <summary>
            /// 日別転送量リセット時刻
            /// </summary>
            public string ResetDayTime;
            /// <summary>
            /// IP確認不可時にエラーを出すか(true=on)
            /// </summary>
            public bool NotDialogIPError;
            /// <summary>
            /// Web側Kick設定有効
            /// </summary>
            public bool WebKick;
            /// <summary>
            /// エクスポート認証ID
            /// </summary>
            public string AuthID;
            /// <summary>
            /// エクスポード認証パスワード
            /// </summary>
            public string AuthPass;
            /// <summary>
            /// Webよりエクスポート認証を有効にするか
            /// </summary>
            public bool AuthWebSet;
        }
        static public OPT Opt;

        /// <summary>
        /// キック関連データ
        /// </summary>
        public struct KICK
        {
            /// <summary>
            /// Kick検査期間(秒)
            /// </summary>
            public uint KickCheckSecond;
            /// <summary>
            /// Kick開始閾値
            /// </summary>
            public uint KickCheckTime;
            /// <summary>
            /// Kick期間(秒)
            /// </summary>
            public uint KickDenyTime;
        }
        static public KICK Kick;

        /// <summary>
        /// ログファイル関連データ
        /// </summary>
        public struct LOG
        {
            /// <summary>
            /// HPアクセスログ
            /// </summary>
            public string HpLogFile;
            /// <summary>
            /// 鏡ログ
            /// </summary>
            public string KagamiLogFile;
            /// <summary>
            /// 詳細ログ出力フラグ
            /// </summary>
            public bool LogDetail;

            ///
            /// 以下のトラヒックは前回終了した時点での日間/月間トラヒック。
            /// 次回起動時に日/月が変わってた場合破棄する。
            /// 同じ日/月ならTotalUP/TotalDLと合わせてGUI上に表示する。
            /// TotalUP/TotalDLはあくまでそのセッションにおける転送量。
            /// ただしTotalUP/TotalDLは毎日０時にTrsUpMon/TrsDlMonに退避後０クリアする。
            /// 

            /// <summary>
            /// UP転送量/day
            /// </summary>
            public ulong TrsUpDay;
            /// <summary>
            /// DL転送量/day
            /// </summary>
            public ulong TrsDlDay;
            /// <summary>
            /// UP転送量/mon
            /// </summary>
            public ulong TrsUpMon;
            /// <summary>
            /// DL転送量/mon
            /// </summary>
            public ulong TrsDlMon;
            /// <summary>
            /// 最終更新日時
            /// </summary>
            public string LastUpdate;
        }
        static public LOG Log;

        /// <summary>
        /// アクセス制限系データ
        /// </summary>
        public struct ACL
        {
            /// <summary>
            /// HPアクセス禁止リモートホスト
            /// </summary>
            public List<string> HpDenyRemoteHost;

            /// <summary>
            /// 接続禁止ImportURLリスト
            /// </summary>
            public List<string> DenyImportURL;

            /// <summary>
            /// WebABCUA
            /// </summary>
            public List<string> CheckUserAgent;

            /// <summary>
            /// 制限ログイン失敗回数
            /// </summary>
            public uint FailureCount;
            /// <summary>
            ///クローズ設置向け設定パスワード
            /// </summary>
            public string ClosedPassward;
            /// <summary>
            /// クローズ失敗設定コメント
            /// </summary>
            public string ClosedComment;
            /// <summary>
            /// 同一ImportURL接続制限
            /// 0:制限なし 1～:指定数まで許可
            /// </summary>
            public uint LimitSameImportURL;

            ///<summary>
            /// 長時間インポート接続自動切断
            /// 0:自動切断しない 1～:指定時間(分)経過後自動切断
            /// </summary>
            public uint ImportOutTime;

            ///<summary>
            /// クライアント数制限有効フラグ
            /// </summary>
            public bool ClientOutCheck;
            ///<summary>
            /// 制限クライアント数
            /// </summary>
            public uint ClientOutNum;
            ///<summary>
            /// 制限クライアント時間
            /// </summary>
            public uint ClientOutTime;
            /// <summary>
            /// 設定者のIPをクライアント数に含めるか
            /// true:含めない false:含む
            /// </summary>
            public bool ClientNotIPCheck;

            /// <summary>
            /// 同一クライアント接続制限
            /// 0:制限なし 1～:指定数まで許可
            /// </summary>
            public uint LimitSameClient;
            /// <summary>
            /// 転送量制限有効フラグ
            /// </summary>
            public bool StopFlag;
            /// <summary>
            /// DL転送量含む
            /// </summary>
            public bool DLContainFlag;
            /// <summary>
            /// 制限転送量（MB）
            /// </summary>
            public uint StopMB;
            /// <summary>
            /// 制限転送量単位(0=MB,1=GB,2=TB)
            /// </summary>
            public uint StopMBSelect;
            /// <summary>
            /// 日別選択(Day=0,Week=1,Month=2)
            /// </summary>
            ///public uint StopDaySelect;
            /// <summary>
            /// 転送量到達後動作指定
            /// </summary>
            public uint StopSelect;
            /// <summary>
            /// 過去24時間のトラフィック。分単位
            /// </summary>
            public long[] TrafficTFHour;
            /// <summary>
            /// 過去２４時間基準有効(0=無効,1=一時ファイル保存,制限無効,2= 1と制限有効)
            /// </summary>
            public uint TrafficHour;
            /// <summary>
            /// 土日割り当て増加有効
            /// </summary>
            ///public bool SunDayUP;
            /// <summary>
            /// インポートURLと設定者IPの一致チェック有効フラグ
            /// </summary>
            public bool SetUserIpCheck;
            /// <summary>
            /// 待機中ポートがあれば開放しない
            /// </summary>
            public bool PortFullOnlyCheck;
            /// <summary>
            /// 接続禁止ホスト
            /// </summary>
            public List<string> DenyHost;
            /// <summary>
            /// 接続禁止UA
            /// </summary>
            public List<string> DenyUA;
        }
        static public ACL Acl;

        /// <summary>
        /// 帯域制限関連データ
        /// </summary>
        public struct BNDWTH
        {
            /// <summary>
            /// 帯域制限実行中フラグ
            /// </summary>
            public bool EnableBandWidth;
            /// <summary>
            /// 帯域制限モード種別
            /// </summary>
            public uint BandStopMode;
            /// <summary>
            /// 帯域制限値
            /// </summary>
            public uint BandStopValue;
            /// <summary>
            /// 帯域制限値単位
            /// </summary>
            public uint BandStopUnit;
            /// <summary>
            /// 帯域制限時のリザーブ数考慮フラグ
            /// </summary>
            public bool BandStopResv;
            /// <summary>
            /// 起動時に待機するポートの帯域制限値
            /// </summary>
            public Dictionary<int,int> PortBandWidth;
        }
        static public BNDWTH BndWth;

        /// <summary>
        /// スケジュール起動
        /// </summary>
        static public List<SCHEDULE> ScheduleItem = new List<SCHEDULE>();
        #endregion

        #region 設定を保存しないメンバ
        /// <summary>
        /// アプリケーション名
        /// </summary>
        static public string AppName;
        /// <summary>
        /// 全体のCPU使用率
        /// </summary>
        static public PerformanceCounter CPU_ALL;
        /// <summary>
        /// かがみんのCPU使用率
        /// </summary>
        static public PerformanceCounter CPU_APP;
        /// <summary>
        /// かがみん起動時刻
        /// </summary>
        static public DateTime StartTime = DateTime.Now;
        /// <summary>
        /// グローバルIP
        /// </summary>
        static public string GlobalIP = "";
        /// <summary>
        /// 次回グローバルIPを取得出来る時間
        /// </summary>
        static public DateTime GlobalIPGetTime = DateTime.Now;
        /// <summary>
        /// 新規接続制限フラグ
        /// </summary>
        static public bool Pause = false;
        /// <summary>
        /// UserAgent
        /// </summary>
        static public string UserAgent = "";
        /// <summary>
        /// ログ出力モード
        /// 0:すべてのログ表示
        /// 1:重要ログのみ表示
        /// </summary>
        static public int LogMode = 0;
        // clientView,ClientItem内ListViewItemのIndex
        /// <summary>
        /// clmClientViewID.DisplayIndex
        /// </summary>
        static public int clmCV_ID_IDX = 0;
        /// <summary>
        /// clmClientViewIpHost.DisplayIndex
        /// </summary>
        static public int clmCV_IH_IDX = 0;
        /// <summary>
        /// clmClientViewUA.DisplayIndex
        /// </summary>
        static public int clmCV_UA_IDX = 0;
        /// <summary>
        /// clmClientViewTime.DisplayIndex
        /// </summary>
        static public int clmCV_TM_IDX = 0;
        /// <summary>
        /// clmClientView_internal_IP.DisplayIndex
        /// </summary>
        static public int clmCV_IP_IDX = 0;
        /// <summary>
        /// clmClientView_internal_HOST.DisplayIndex
        /// </summary>
        static public int clmCV_HO_IDX = 0;
        /// <summary>
        /// clmClientViewBuffer.DisplayIndex
        /// </summary>
        static public int clmCV_BU_IDX = 0;
        /// <summary>
        /// 鏡状態ディスプレイインデックス
        /// </summary>
        static public int clmCV_KI_IDX = 0;
        /// <summary>
        /// 1スレッドで送信を行うくらいクライアント数
        /// </summary>
        static public int ClientToThread = 10;
        /// <summary>
        /// 鏡インスタンス一覧
        /// </summary>
        static public List<Kagami> KagamiList = new List<Kagami>();

        /// <summary>
        /// KickIP管理リスト(Statusごとに切り替え)
        /// </summary>
        ///static public Dictionary<String, String> KickList = new Dictionary<string,string>();

        /// <summary>
        /// 総UP転送量
        /// </summary>
        static public ulong TotalUP = 0;
        /// <summary>
        /// 総DL転送量
        /// </summary>
        static public ulong TotalDL = 0;
        ///<summary>
        /// 日別UP転送量
        /// </summary>
        static public ulong DayUP = 0;
        /// <summary>
        /// 24時間累計転送量
        /// </summary>
        static public ulong HourUP = 0;
        /// <summary>
        /// 設定転送量(MB)
        /// </summary>
        ///static public ulong SetTraffic = 0;
        /// <summary>
        /// 
        /// </summary>
        static public ulong DayUPSet = 0;
        /// <summary>
        /// 分単位転送量(MB)0-59sec
        /// </summary>
        static public uint MinUPSet = 0;
        /// <summary>
        /// トラフィック起動管理用
        /// </summary>
        static public bool Traffic = false;
        /// <summary>
        /// 土日増加割合(標準30%)
        /// </summary>
        ///static public uint SunUPPer = 30;
        /// <summary>
        /// admin/dis指定回数失敗アクセス制限用
        /// </summary>
        static public List<string> FailureLogin = new List<string>();
        /// <summary>
        /// ログイン失敗DenyIP
        /// </summary>
        static public List<string> DenyLogin = new List<string>();
        /// <summary>
        /// UPnPで閉じる必要のあるポート
        /// </summary>
        static public List<int> UPnPPort = new List<int>();
        /// <summary>
        /// HP開始状態フラグ
        /// </summary>
        static public bool HPStart = false;
        /// <summary>
        /// 認証ダイアログ排他使用フラグ(true=Import,false=Export)
        /// </summary>
        static public bool AuthDiagflag = true;
        /// <summary>
        /// HP用HybridDSPインスタンス
        /// </summary>
        static public HttpHybrid WebEntrance = new HttpHybrid();

        // ASF GUID
        static public Dictionary<string, string> ASF_GUID = new Dictionary<string, string>();
        /// <summary>
        /// 美人メッセージ
        /// </summary>
        static public string BusyString = "HTTP/1.0 503 Service Unavailable\r\n" +
            "Server: Rex/10.0.0.3650\r\n" +
            "Cache-Control: no-cache\r\n" +
            "Pragma: no-cache\r\n" +
            "Pragma: client-id=0\r\n" +
            "X-Server: " + Front.AppName + "\r\n" +
            "Content-Type: text/html\r\n\r\n" +
            "<html><head><title>503 Service Unavailable</title></head>\r\n" +
            "<body><h1>503 Service Unavailable</h1></body></html>\r\n";
        static public string AuthString_ = "HTTP/1.0 401 Authorization Required\r\n" +
            "Date: DATE_NOW_REPLACE \r\n" +
            "Server: Rex/10.0.0.3650\r\n" +
            "WWW-Authenticate: Basic realm=\"Secret File\"\r\n" +
            "Cache-Control: no-cache\r\n" +
            "Connection: close\r\n" +
            "Content-Type: text/html; charset=iso-8859-1\r\n" +
            "<!DOCTYPE HTML PUBLIC \" -//IETF//DTD HTML 2.0//EN\">\r\n"+
            "<html><head>\r\n" +
            "<title>401 Authorization Required</title>\r\n" +
            "</head><body>\r\n" +
            "<h1>Authorization Required</h1>\r\n" +
            "<p>This server could not verify that you\r\n" +
            "are authorized to access the document requested.</p>\r\n";
        static public string KagamiLinkRes
        {
            get
            {
                return "KAGAMI/1.0 200 OK\r\n" + "Server: " + AppName + "\r\n\r\n";
            }
        }
        static public string AuthString()
        {
            return AuthString_.Replace("DATE_NOW_REPLACE", DateTime.Now.ToString("r"));


        }
        /// <summary>
        /// 帯域制限方式
        /// </summary>
        static public string[] BandStopTypeString = {
            "全ポート合計で設定",
            "１ポート辺りで設定",
            "ポート毎に個別設定" };
        /// <summary>
        /// 帯域制限値単位
        /// </summary>
        static public string[] BandStopUnitString = {
            "KB/s",
            "MB/s",
            "Kbps",
            "Mbps" };
        /// <summary>
        /// スケジュール起動イベント
        /// </summary>
        static public string[] ScheduleEventString = {
            "エントランスサーバ起動",
            "エントランスサーバ停止",
            "新規受付開始",
            "新規受付停止",
            "強制切断",
            "ポート待ち受け開始",
            "ポート待ち受け停止",
        　　"鏡終了後待受停止",
            "接続枠数変更"};
        static public string[] ScheduleWeekString = {
            "日曜",
            "月曜",
            "火曜",
            "水曜",
            "木曜",
            "金曜",
            "土曜",
            "毎日",
            "平日",
            "土日"
        };
        static public string[] ScheduleTrfTypeString = {
            "日間総転送量",
            "月間総転送量"
        };
        static public string[] ScheduleTrfUnitString = {
            "MB",
            "GB"
        };
        /// <summary>
        /// 転送量到達フラグ
        /// </summary>
        static public bool Attainment = false;
        
        /// <summary>
        /// 最終子鏡転送チェック時間
        /// </summary>
        static public DateTime temptrans = DateTime.Now;
        /// <summary>
        /// 最終転送ホスト
        /// </summary>
        static public string temphost = "";
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        static Front()
        {
            CPU_ALL = new PerformanceCounter();
            CPU_ALL.CategoryName = "Processor";
            CPU_ALL.CounterName = "% Processor Time";
            CPU_ALL.InstanceName = "_Total";
            CPU_APP = new PerformanceCounter();
            CPU_APP.CategoryName = "Process";
            CPU_APP.CounterName = "% Processor Time";
#if DEBUG
            CPU_APP.InstanceName = "Kagamin2.vshost";
#else
            CPU_APP.InstanceName = "Kagamin2";
#endif
            #region ASF_HEADER_GUIDの設定
            // Top-Lv
            ASF_GUID.Add("3026B2758E66CF11A6D900AA0062CE6C", "ASF_Header_Object");
            ASF_GUID.Add("3626B2758E66CF11A6D900AA0062CE6C", "ASF_Data_Object");
            ASF_GUID.Add("90080033B1E5CF1189F400A0C90349CB", "ASF_Simple_Index_Object");
            ASF_GUID.Add("D329E2D6DA35D111903400A0C90349BE", "ASF_Index_Object");
            ASF_GUID.Add("F803B1FEAD12644C840F2A1D2F7AD48C", "ASF_Media_Object_Index_Object");
            ASF_GUID.Add("D03FB73C4A0C0348953DEDF7B6228F0C", "ASF_Timecode_Index_Object");
            // Header-Object
            ASF_GUID.Add("A1DCAB8C47A9CF118EE400C00C205365", "ASF_File_Properties_Object");
            ASF_GUID.Add("9107DCB7B7A9CF118EE600C00C205365", "ASF_Stream_Properties_Object");
            ASF_GUID.Add("B503BF5F2EA9CF118EE300C00C205365", "ASF_Header_Extension_Object");
            ASF_GUID.Add("4052D1861D31D011A3A400A0C90348F6", "ASF_Codec_List_Object");
            ASF_GUID.Add("301AFB1E620BD011A39B00A0C90348F6", "ASF_Script_Command_Object");
            ASF_GUID.Add("01CD87F451A9CF118EE600C00C205365", "ASF_Marker_Object");
            ASF_GUID.Add("DC29E2D6DA35D111903400A0C90349BE", "ASF_Bitrate_Mutual_Exclusion_Object");
            ASF_GUID.Add("3526B2758E66CF11A6D900AA0062CE6C", "ASF_Error_Correction_Object");
            ASF_GUID.Add("3326B2758E66CF11A6D900AA0062CE6C", "ASF_Content_Description_Object");
            ASF_GUID.Add("40A4D0D207E3D21197F000A0C95EA850", "ASF_Extended_Content_Description_Object");
            ASF_GUID.Add("FAB3112223BDD211B4B700A0C955FC6E", "ASF_Content_Branding_Object");
            ASF_GUID.Add("CE75F87B8D46D1118D82006097C9A2B2", "ASF_Stream_Bitrate_Properties_Object");
            ASF_GUID.Add("FBB3112223BDD211B4B700A0C955FC6E", "ASF_Content_Encryption_Object");
            ASF_GUID.Add("14E68A292226174CB935DAE07EE9289C", "ASF_Extended_Content_Encryption_Object");
            ASF_GUID.Add("FCB3112223BDD211B4B700A0C955FC6E", "ASF_Digital_Signature_Object");
            ASF_GUID.Add("74D40618DFCA0945A4BA9AABCB96AAE8", "ASF_Padding_Object");
            // Header-Extension-Object
            ASF_GUID.Add("CBA5E61472C632438399A96952065B5A", "ASF_Extended_Stream_Properties_Object");
            ASF_GUID.Add("CF4986A0754770468A166E35357566CD", "ASF_Advanced_Mutual_Exclusion_Object");
            ASF_GUID.Add("405A46D1795A3843B71BE36B8FD6C249", "ASF_Group_Mutual_Exclusion_Object");
            ASF_GUID.Add("5BD1FED4D3884F4581F0ED5C45999E24", "ASF_Stream_Prioritization_Object");
            ASF_GUID.Add("E60996A67B51D211B6AF00C04FD908E9", "ASF_Bandwidth_Sharing_Object");
            ASF_GUID.Add("A946437CE0EFFC4BB229393EDE415C85", "ASF_Language_List_Object");
            ASF_GUID.Add("EACBF8C5AF5B77488467AA8C44FA4CCA", "ASF_Metadata_Object");
            ASF_GUID.Add("941C23449894D149A1411D134E457054", "ASF_Metadata_Library_Object");
            ASF_GUID.Add("DF29E2D6DA35D111903400A0C90349BE", "ASF_Index_Parameters_Object");
            ASF_GUID.Add("AD3B206B113FE448ACA8D7613DE2CFA7", "ASF_Media_Object_Index_Parameters_Object");
            ASF_GUID.Add("6D495EF597975D4B8C8B604DFE9BFB24", "ASF_Timecode_Index_Parameters_Object");
            ASF_GUID.Add("338505438169E6499B74AD12CB86D58C", "ASF_Advanced_Content_Encryption_Object");
            // StreamType-Object
            ASF_GUID.Add("409E69F84D5BCF11A8FD00805F5C442B", "ASF_Audio_Media");
            ASF_GUID.Add("C0EF19BC4D5BCF11A8FD00805F5C442B", "ASF_Video_Media");
            ASF_GUID.Add("C0CFDA59E659D011A3AC00A0C90348F6", "ASF_Command_Media");
            ASF_GUID.Add("00E11BB64E5BCF11A8FD00805F5C442B", "ASF_JFIF_Media");
            ASF_GUID.Add("E07D903515E4CF11A91700805F5C442B", "ASF_Degradable_JPEG_Media");
            ASF_GUID.Add("2C22BD911CF27A498B6D5AA86BFC0185", "ASF_File_Transfer_Media");
            ASF_GUID.Add("E265FB3AEF47F240AC2C70A90D71D343", "ASF_Binary_Media");
            #endregion
        }

        #region 設定の読み書き
        /// <summary>
        /// 設定読み出し
        /// </summary>
        static public void LoadSetting()
        {


            // ウインドウ位置・サイズが読み出せなかった場合の初期値計算
            int x = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - Form.W) / 2; if (x < 0) x = 0;
            int y = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - Form.H) / 2; if (y < 0) y = 0;
            int w = Form.W;
            int h = Form.H;

            string iniFile = System.Windows.Forms.Application.StartupPath + "/setting.ini";
            StringBuilder sb = new StringBuilder(1024);

            IniFileHandler.GetPrivateProfileString(                         "ACL",      "DENY_HP_IP",           "",     sb, (uint)sb.Capacity, iniFile);    Acl.HpDenyRemoteHost = new List<string>(sb.ToString().Split(','));
            IniFileHandler.GetPrivateProfileString("ACL", "DENY_CLIENT_IP", "", sb, (uint)sb.Capacity, iniFile); Acl.DenyHost = new List<string>(sb.ToString().Split(','));
            IniFileHandler.GetPrivateProfileString("ACL", "DENY_CLIENT_UA", "", sb, (uint)sb.Capacity, iniFile); Acl.DenyUA = new List<string>(sb.ToString().Split(','));

            IniFileHandler.GetPrivateProfileString("ACL", "CHECK_UA", "WebABC,JikkyoServerChecker,StatusChecker", sb, (uint)sb.Capacity, iniFile); Acl.CheckUserAgent = new List<string>(sb.ToString().Split(','));

            IniFileHandler.GetPrivateProfileString(                         "ACL",      "DENY_IMPORT_URL",      "127.0.0.1,localhost", sb, (uint)sb.Capacity, iniFile); Acl.DenyImportURL = new List<string>(sb.ToString().Split(','));
            IniFileHandler.GetPrivateProfileString("ACL", "CLOSED_PASSWORD", "", sb, (uint)sb.Capacity, iniFile); Acl.ClosedPassward = sb.ToString();
            IniFileHandler.GetPrivateProfileString("ACL", "CLOSED_COMMENT", "", sb, (uint)sb.Capacity, iniFile); Acl.ClosedComment = sb.ToString();
            Acl.LimitSameImportURL  = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_SAME_IMPORT",    2,      iniFile);
            Acl.SetUserIpCheck = IniFileHandler.GetPrivateProfileInt("ACL", "SET_USER_IP_CHECK", 0, iniFile) != 0 ? true : false;
            Acl.ImportOutTime       = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_IMPORT_OUT",     0,      iniFile);
            Acl.ClientOutCheck      = IniFileHandler.GetPrivateProfileInt(  "ACL",      "ENABLE_CLIENT_OUT",    0,      iniFile) != 0 ? true : false;
            Acl.ClientNotIPCheck = IniFileHandler.GetPrivateProfileInt("ACL", "ENABLE_CLIENT_NOTIP", 0, iniFile) != 0 ? true : false;
            Acl.PortFullOnlyCheck = IniFileHandler.GetPrivateProfileInt("ACL", "PORT_FULL_ONLY", 0, iniFile) != 0 ? true : false;

            Acl.StopFlag = IniFileHandler.GetPrivateProfileInt("ACL", "ENABLE_STOPMB", 0, iniFile) != 0 ? true : false;
            Acl.DLContainFlag = IniFileHandler.GetPrivateProfileInt("ACL", "ENABLE_CONDL", 0, iniFile) != 0 ? true : false;
            Acl.StopSelect = IniFileHandler.GetPrivateProfileInt("ACL", "STOP_SYSTEM", 0, iniFile);
            Acl.StopMB = IniFileHandler.GetPrivateProfileInt("ACL", "STOP_MB", 25600, iniFile);
            Acl.StopMBSelect = IniFileHandler.GetPrivateProfileInt("ACL", "MB_SELECT", 1, iniFile);
            //Acl.StopDaySelect = IniFileHandler.GetPrivateProfileInt("ACL", "DAY_SELECT", 0, iniFile);
            //Acl.SunDayUP = IniFileHandler.GetPrivateProfileInt("ACL", "SUNDAY_UP", 0, iniFile) != 0 ? true : false;

            Acl.ClientOutNum        = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_CLIENT_NUM",     0,      iniFile);
            Acl.ClientOutTime       = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_CLIENT_TIME",    10,     iniFile);
            Acl.LimitSameClient     = IniFileHandler.GetPrivateProfileInt(  "ACL",      "LIMIT_SAME_CLIENT",    0,      iniFile);
            Acl.FailureCount = IniFileHandler.GetPrivateProfileInt("ACL", "LIMIT_LOGIN_COUNT", 5, iniFile);

            BndWth.EnableBandWidth  = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "ENABLE",               0,      iniFile) != 0 ? true : false;
            BndWth.BandStopMode     = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "MODE",                 0,      iniFile);
            BndWth.BandStopValue    = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "VALUE",                5000,   iniFile);
            BndWth.BandStopUnit     = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "UNIT",                 0,      iniFile);
            BndWth.BandStopResv     = IniFileHandler.GetPrivateProfileInt(  "BNDWTH",   "RESV_FLAG",            0,      iniFile) != 0 ? true : false;

            // 座標とサイズはマイナスがありえる？
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "X",                    x.ToString(),   sb, (uint)sb.Capacity, iniFile);    try { Form.X = int.Parse(sb.ToString()); } catch { Form.X = 0; }
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "Y",                    y.ToString(),   sb, (uint)sb.Capacity, iniFile);    try { Form.Y = int.Parse(sb.ToString()); } catch { Form.Y = 0; }
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "W",                    w.ToString(),   sb, (uint)sb.Capacity, iniFile);    try { Form.W = int.Parse(sb.ToString()); } catch { Form.W = 600; }
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "H",                    h.ToString(),   sb, (uint)sb.Capacity, iniFile);    try { Form.H = int.Parse(sb.ToString()); } catch { Form.H = 400; }
            Form.SplitDistance1     = IniFileHandler.GetPrivateProfileInt(  "FORM",     "SPLIT1",               190,    iniFile);
            Form.SplitDistance2     = IniFileHandler.GetPrivateProfileInt(  "FORM",     "SPLIT2",               150,    iniFile);
            Form.LeftPanelCollapsed = IniFileHandler.GetPrivateProfileInt(  "FORM",     "LP_COLLAPSED",         1,      iniFile) != 0 ? true : false;
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_KAGAMI",           "",     sb, (uint)sb.Capacity, iniFile);    Form.KagamiListColumn   = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_MONALL",           "",     sb, (uint)sb.Capacity, iniFile);    Form.MonAllViewColumn   = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_MON",              "",     sb, (uint)sb.Capacity, iniFile);    Form.MonViewColumn      = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_CLIENT",           "",     sb, (uint)sb.Capacity, iniFile);    Form.ClientViewColumn   = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_RESERVE",          "",     sb, (uint)sb.Capacity, iniFile);    Form.ResvViewColumn     = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_KICK",             "",     sb, (uint)sb.Capacity, iniFile);    Form.KickViewColumn     = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_LOG",              "",     sb, (uint)sb.Capacity, iniFile);    Form.LogViewColumn      = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "FORM",     "CLM_SCHEDULE",         "",     sb, (uint)sb.Capacity, iniFile);    Form.ScheduleColumn     = sb.ToString();
            Form.monViewUnit        = IniFileHandler.GetPrivateProfileInt(  "FORM",     "MON_UNIT",             0,      iniFile);
            Form.IconIndex          = IniFileHandler.GetPrivateProfileInt(  "FORM",     "ICON",                 0,      iniFile);
            if (Form.IconIndex >= 3) Form.IconIndex = 0;    // 念のためガード

            IniFileHandler.GetPrivateProfileString(                         "GUI",      "IMPORT_URL",           "",     sb, (uint)sb.Capacity, iniFile);    Gui.ImportURL = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "GUI",      "FAVORITE_LIST",        "",     sb, (uint)sb.Capacity, iniFile);    Gui.FavoriteList = new List<string>(sb.ToString().Split(','));
            IniFileHandler.GetPrivateProfileString(                         "GUI",      "RESERVE_LIST",         "",     sb, (uint)sb.Capacity, iniFile);    Gui.ReserveList = new List<string>(sb.ToString().Split(','));
            IniFileHandler.GetPrivateProfileString(                         "GUI",      "PORT_LIST",            "8080", sb, (uint)sb.Capacity, iniFile);
            Gui.PortList = new List<int>();
            foreach (string s in sb.ToString().Split(','))
            {
                try
                {
                    int i = int.Parse(s);
                    Gui.PortList.Add(i);
                }
                catch { }
            }
 
            IniFileHandler.GetPrivateProfileString("HP", "START_PORTLIST", "", sb, (uint)sb.Capacity, iniFile);

            Hp.StartPortList = new List<int>();
            foreach (string s in sb.ToString().Split(','))
            {
                try
                {
                    int i = int.Parse(s);
                    Hp.StartPortList.Add(i);
                }
                catch { }
            }

            IniFileHandler.GetPrivateProfileString("GUI", "SET_LIST", "", sb, (uint)sb.Capacity, iniFile);
            Gui.SetList = new List<int>();
            foreach (string s in sb.ToString().Split(','))
            {
                try
                {
                    int i = int.Parse(s);
                    Gui.SetList.Add(i);
                }
                catch
                {
                    Gui.SetList.Clear();

                    break;
                }
            }

            IniFileHandler.GetPrivateProfileString("GUI", "BAND_PORT_WIDTH", "", sb, (uint)sb.Capacity, iniFile);
            BndWth.PortBandWidth = new Dictionary<int,int>();
            string[] _str1= sb.ToString().Split(',');
            for (int i = 0; i < _str1.Length; i++)
            {
                try
                {
                    int j = int.Parse(_str1[i]);
                    BndWth.PortBandWidth.Add(j, int.Parse(_str1[++i]));

                }
                catch
                {
                    BndWth.PortBandWidth.Clear();
                    break;
                }
            }

            IniFileHandler.GetPrivateProfileString("GUI", "PORT_CONN", "", sb, (uint)sb.Capacity, iniFile);
            Gui.Conn_NumList = new Dictionary<int, int>();
            _str1 = null;
            _str1 = sb.ToString().Split(',');
            for (int i = 0; i < _str1.Length; i++)
            {
                try
                {
                    int j = int.Parse(_str1[i]);
                    Gui.Conn_NumList.Add(j, int.Parse(_str1[++i]));

                }
                catch
                {
                    Gui.Conn_NumList.Clear();
                    break;
                }
            }

            IniFileHandler.GetPrivateProfileString("GUI", "PORT_RESERVE", "", sb, (uint)sb.Capacity, iniFile);
            Gui.Reserve_NumList = new Dictionary<int, int>();
            _str1 = null;
            _str1 = sb.ToString().Split(',');
            for (int i = 0; i < _str1.Length; i++)
            {
                try
                {
                    int j = int.Parse(_str1[i]);
                    Gui.Reserve_NumList.Add(j, int.Parse(_str1[++i]));

                }
                catch
                {
                    Gui.Reserve_NumList.Clear();
                    break;
                }
            }

            Gui.Conn                = IniFileHandler.GetPrivateProfileInt(  "GUI",      "CONN",                 50,     iniFile);
            Gui.Reserve             = IniFileHandler.GetPrivateProfileInt(  "GUI",      "RESV",                 3,      iniFile);

            Hp.UseHP                = IniFileHandler.GetPrivateProfileInt(  "HP",       "USE_HP",               0,      iniFile) != 0 ? true : false;
            Hp.AuthEnable = IniFileHandler.GetPrivateProfileInt("HP", "AUTH_ENABLE", 0, iniFile) != 0 ? true : false;
            IniFileHandler.GetPrivateProfileString("HP", "AUTH_PASS", "", sb, (uint)sb.Capacity, iniFile); Hp.AuthPass = sb.ToString();
            IniFileHandler.GetPrivateProfileString("HP", "AUTH_USER", "", sb, (uint)sb.Capacity, iniFile); Hp.AuthUser = sb.ToString();
 
            IniFileHandler.GetPrivateProfileString(                         "HP",       "HOSTNAME",             "http://localhost", sb, (uint)sb.Capacity, iniFile);    Hp.IpHTTP = sb.ToString();
            Hp.PortHTTP             = IniFileHandler.GetPrivateProfileInt(  "HP",       "PORT",                 8888,   iniFile);
            IniFileHandler.GetPrivateProfileString(                         "HP",       "PUBLIC_DIR",           "",     sb, (uint)sb.Capacity, iniFile);    Hp.PublicDir = sb.ToString();
#if DEBUG
            IniFileHandler.GetPrivateProfileString(                         "HP",       "IP_CHECK_URL",         "http://kagami.homelinux.net/ip.cgi",  sb, (uint)sb.Capacity, iniFile);    Hp.IPCheckURL = sb.ToString();
#else
            IniFileHandler.GetPrivateProfileString(                         "HP",       "IP_CHECK_URL",         "http://taruo.net/ip/",                sb, (uint)sb.Capacity, iniFile);    Hp.IPCheckURL = sb.ToString();
#endif
            Hp.IPCheckLine          = IniFileHandler.GetPrivateProfileInt(  "HP",       "IP_CHECK_LINE",        4,      iniFile);

            Hp.HPstartON = IniFileHandler.GetPrivateProfileInt("HP", "HP_START", 0, iniFile) != 0 ? true : false;
            Hp.PortstartON = IniFileHandler.GetPrivateProfileInt("HP", "PORT_START", 0, iniFile) != 0 ? true : false;

            Hp.allh = IniFileHandler.GetPrivateProfileInt("HP", "ALL_H", 0, iniFile) != 0 ? true : false;
            Hp.porth = IniFileHandler.GetPrivateProfileInt("HP", "PORT_H", 0, iniFile) != 0 ? true : false;

            Kick.KickCheckSecond    = IniFileHandler.GetPrivateProfileInt(  "KICK",     "CHECK_SECOND",         10,     iniFile);
            Kick.KickCheckTime      = IniFileHandler.GetPrivateProfileInt(  "KICK",     "CHECK_TIME",           7,      iniFile);
            Kick.KickDenyTime       = IniFileHandler.GetPrivateProfileInt(  "KICK",     "DENY_TIME",            60,     iniFile);

            IniFileHandler.GetPrivateProfileString("LOG", "FILE1", "", sb, (uint)sb.Capacity, iniFile); Log.HpLogFile = sb.ToString();
            IniFileHandler.GetPrivateProfileString("LOG", "FILE2", "", sb, (uint)sb.Capacity, iniFile); Log.KagamiLogFile = sb.ToString();
            Log.LogDetail = IniFileHandler.GetPrivateProfileInt("LOG", "ENABLE_DETAIL", 0, iniFile) != 0 ? true : false;
            IniFileHandler.GetPrivateProfileString("LOG", "TRS_UP_DAY", "0", sb, (uint)sb.Capacity, iniFile); Log.TrsUpDay = ulong.Parse(sb.ToString());
            IniFileHandler.GetPrivateProfileString("LOG", "TRS_DL_DAY", "0", sb, (uint)sb.Capacity, iniFile); Log.TrsDlDay = ulong.Parse(sb.ToString());
            IniFileHandler.GetPrivateProfileString("LOG", "TRS_UP_MON", "0", sb, (uint)sb.Capacity, iniFile); Log.TrsUpMon = ulong.Parse(sb.ToString());
            IniFileHandler.GetPrivateProfileString("LOG", "TRS_DL_MON", "0", sb, (uint)sb.Capacity, iniFile); Log.TrsDlMon = ulong.Parse(sb.ToString());
            IniFileHandler.GetPrivateProfileString("LOG", "LAST_UPDATE", "", sb, (uint)sb.Capacity, iniFile); Log.LastUpdate = sb.ToString();

            Opt.BrowserView         = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_BROWSER_VIEW",  1,      iniFile) != 0 ? true : false;
            Opt.BrowserViewMode     = IniFileHandler.GetPrivateProfileInt(  "OPT",      "BROWSER_MODE",         0,      iniFile) != 0 ? true : false;
            Opt.PriKagamiexe        = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_KAGAMIEXE",     0,      iniFile) != 0 ? true : false;
            Opt.PriKagamin          = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_KAGAMIN",       0,      iniFile) != 0 ? true : false;
            Opt.BalloonTip          = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_BALLOONTIP",    0,      iniFile) != 0 ? true : false;
            Opt.EnablePush          = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_PUSH",          0,      iniFile) != 0 ? true : false;
            Opt.EnableInfo          = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_INFO",          0,      iniFile) != 0 ? true : false;
            Opt.EnableAdmin         = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_ADMIN",         0,      iniFile) != 0 ? true : false;
            Opt.WebKick             = IniFileHandler.GetPrivateProfileInt(  "OPT",      "ENABLE_WEBKICK",       1, iniFile) != 0 ? true : false;
            Opt.AuthWebSet          = IniFileHandler.GetPrivateProfileInt(  "OPT",      "AUTH_WEB", 0, iniFile) != 0 ? true : false;
            Opt.NotDialogIPError = IniFileHandler.GetPrivateProfileInt("OPT", "IP_ERROR", 1, iniFile) != 0 ? true : false;
            IniFileHandler.GetPrivateProfileString("OPT", "RESET_TIME", "00:00", sb, (uint)sb.Capacity, iniFile); Opt.ResetDayTime = sb.ToString();
            IniFileHandler.GetPrivateProfileString("OPT", "AUTH_ID", "", sb, (uint)sb.Capacity, iniFile); Opt.AuthID = sb.ToString();
            IniFileHandler.GetPrivateProfileString("OPT", "AUTH_PASSWORD", "", sb, (uint)sb.Capacity, iniFile); Opt.AuthPass = sb.ToString();

            Opt.ViewWMP = IniFileHandler.GetPrivateProfileInt("OPT", "ENABLE_WMP", 0, iniFile) != 0 ? true : false;


            Acl.TrafficHour = IniFileHandler.GetPrivateProfileInt("ACL", "ENABLE_HOUR", 0, iniFile);
            Opt.TransKagamin = IniFileHandler.GetPrivateProfileInt("OPT", "ENABLE_TRANS", 0, iniFile) != 0 ? true : false;
            Opt.NotMyTrans = IniFileHandler.GetPrivateProfileInt("OPT", "NOT_TRANS", 0, iniFile) != 0 ? true : false;
            Opt.ImportRedirect = IniFileHandler.GetPrivateProfileInt("OPT", "IMPORT_REDIRECT", 0, iniFile) != 0 ? true : false;

            Opt.TransCont = IniFileHandler.GetPrivateProfileInt("OPT", "TRANS_CONT", 3, iniFile);
            Opt.InTrayOn = IniFileHandler.GetPrivateProfileInt("OPT", "ENABLE_INTRAY", 1, iniFile) != 0 ? true : false;

            IniFileHandler.GetPrivateProfileString(                         "OPT",      "ADMIN_PASS",           "",     sb, (uint)sb.Capacity, iniFile);    Opt.AdminPass = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "SOUND_CONN_OK",        "",     sb, (uint)sb.Capacity, iniFile);    Opt.SndConnOkFile = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "SOUND_CONN_NG",        "",     sb, (uint)sb.Capacity, iniFile);    Opt.SndConnNgFile = sb.ToString();
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "SOUND_DISC",           "",     sb, (uint)sb.Capacity, iniFile);    Opt.SndDiscFile = sb.ToString();
             //IniFileHandler.GetPrivateProfileString(                         "OPT",      "OUT_URL",              "http://live24.2ch.net/test/read.cgi/livevenus", sb, (uint)sb.Capacity, iniFile); Opt.OutUrl = sb.ToString();
            IniFileHandler.GetPrivateProfileString("OPT", "OUT_URL", "http://live24.2ch.net/test/read.cgi/livevenus,http://jbbs.livedoor.jp", sb, (uint)sb.Capacity, iniFile);Opt.OutUrl = new List<string>(sb.ToString().Split(','));
            IniFileHandler.GetPrivateProfileString(                         "OPT",      "APP_NAME",             "DER",     sb, (uint)sb.Capacity, iniFile);    Opt.AppName = sb.ToString();

            
            /*IniFileHandler.GetPrivateProfileString("SQL", "HOST", "", sb, (uint)sb.Capacity, iniFile); Opt.HostMySQL = sb.ToString();
            IniFileHandler.GetPrivateProfileString("SQL", "USER", "", sb, (uint)sb.Capacity, iniFile); Opt.UserMuSQL = sb.ToString();
            IniFileHandler.GetPrivateProfileString("SQL", "PASSWORD", "", sb, (uint)sb.Capacity, iniFile); Opt.PassMySQL = sb.ToString();
            IniFileHandler.GetPrivateProfileString("SQL", "READ", "", sb, (uint)sb.Capacity, iniFile); Opt.ReadRowMySQL = sb.ToString();
            IniFileHandler.GetPrivateProfileString("SQL", "READ_IP", "", sb, (uint)sb.Capacity, iniFile); Opt.ReadRowIPMySQL = sb.ToString();
            IniFileHandler.GetPrivateProfileString("SQL", "DB", "", sb, (uint)sb.Capacity, iniFile); Opt.RearDB = sb.ToString();
            IniFileHandler.GetPrivateProfileString("SQL", "HYOU", "", sb, (uint)sb.Capacity, iniFile); Opt.ReadHyou = sb.ToString();
            
            Opt.MySQLEnable = IniFileHandler.GetPrivateProfileInt("SQL", "MYSQL_ENABLE", 0, iniFile) != 0 ? true : false;
            */
            Retry.InRetryInterval   = IniFileHandler.GetPrivateProfileInt(  "RETRY",    "IN_INTERVAL",          10,     iniFile);
            Retry.InRetryTime       = IniFileHandler.GetPrivateProfileInt(  "RETRY",    "IN_TIME",              5,      iniFile);
            Retry.OutRetryInterval  = IniFileHandler.GetPrivateProfileInt(  "RETRY",    "OUT_INTERVAL",         10,     iniFile);
            Retry.OutRetryTime      = IniFileHandler.GetPrivateProfileInt(  "RETRY",    "OUT_TIME",             5,      iniFile);

            Sock.SockConnTimeout    = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "CONN_TO",              2000,   iniFile);
            Sock.SockRecvTimeout    = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "RECV_TO",              10000,  iniFile);
            Sock.SockSendTimeout    = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "SEND_TO",              2000,   iniFile);
            Sock.SockSendQueueSize  = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "QUEUE_SIZE",           200,    iniFile);
            Sock.SockCloseDelay     = IniFileHandler.GetPrivateProfileInt(  "SOCK",     "CLOSE_DELAY",          5000,   iniFile);
            Sock.upnp = IniFileHandler.GetPrivateProfileInt("SOCK", "UPNP_ENABLE", 0, iniFile) == 0 ? false : true;
            Sock.VirtualHost = IniFileHandler.GetPrivateProfileInt("SOCK", "HOST_ENABLE", 0, iniFile) == 0 ? false : true;
            Sock.ConnInfoSend = IniFileHandler.GetPrivateProfileInt("SOCK", "CONINFO_ENABLE", 0, iniFile) == 0 ? false : true;

            uint _sch_num = IniFileHandler.GetPrivateProfileInt("SCH", "NUM", 0, iniFile);
            for (uint _num = 0; _num < _sch_num; _num++)
            {
                IniFileHandler.GetPrivateProfileString("SCH", _num.ToString(), "", sb, (uint)sb.Capacity, iniFile);
                string[] _data = sb.ToString().Split(',');
                if (_data.Length == 12)
                {
                    try
                    {
                        SCHEDULE _item = new SCHEDULE();
                        _item.Enable = _data[0] != "0" ? true : false;
                        _item.Event = uint.Parse(_data[1]);
                        _item.Port = uint.Parse(_data[2]);
                        _item.StartType = uint.Parse(_data[3]);
                        _item.Week = uint.Parse(_data[4]);
                        _item.Hour = uint.Parse(_data[5]);
                        _item.Min = uint.Parse(_data[6]);
                        _item.TrfType = uint.Parse(_data[7]);
                        _item.TrfValue = uint.Parse(_data[8]);
                        _item.TrfUnit = uint.Parse(_data[9]);
                        _item.Conn = uint.Parse(_data[10]);
                        _item.Resv = uint.Parse(_data[11]);
                        _item.ExecTrf = false;
                        ScheduleItem.Add(_item);
                    }
                    catch { }
                }
            }
            Front.Acl.TrafficHour = 1;
        }

        /// <summary>
        /// 設定保存
        /// </summary>
        static public void SaveSetting()
        {
            string iniFile = System.Windows.Forms.Application.StartupPath + "/setting.ini";
            string str = "";

            // List<string> → カンマ区切りstring変換
            foreach (string s in Acl.HpDenyRemoteHost)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("ACL", "DENY_HP_IP", str, iniFile);

            // List<string> → カンマ区切りstring変換
            str = "";
            foreach (string s in Acl.DenyImportURL)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("ACL", "DENY_IMPORT_URL", str, iniFile);

            // List<string> → カンマ区切りstring変換
            str = "";
            foreach (string s in Acl.DenyHost)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("ACL", "DENY_CLIENT_IP", str, iniFile);

            // List<string> → カンマ区切りstring変換
            str = "";
            foreach (string s in Acl.DenyUA)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("ACL", "DENY_CLIENT_UA", str, iniFile);

            // List<string> → カンマ区切りstring変換
            str = "";
            foreach (Kagami _k in Front.KagamiList)
            {
                if (_k != null)
                {
                    str += _k.Status.MyPort.ToString() + ",";
                    str += _k.Status.GUILimitUPSpeed.ToString() + ",";

                }
            }
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("GUI", "BAND_PORT_WIDTH", str, iniFile);

            // List<string> → カンマ区切りstring変換
            str = "";
            foreach (Kagami _k in Front.KagamiList)
            {
                if (_k != null)
                {
                    str += _k.Status.MyPort.ToString() + ",";
                    str += _k.Status.Conn_UserSet.ToString() + ",";

                }
            }
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("GUI", "PORT_CONN", str, iniFile);


            // List<string> → カンマ区切りstring変換
            str = "";
            foreach (Kagami _k in Front.KagamiList)
            {
                if (_k != null)
                {
                    str += _k.Status.MyPort.ToString() + ",";
                    str += _k.Status.Reserve.ToString() + ",";

                }
            }
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("GUI", "PORT_RESERVE", str, iniFile);

            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_SAME_IMPORT", Acl.LimitSameImportURL.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_IMPORT_OUT", Acl.ImportOutTime.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "ENABLE_CLIENT_OUT", Acl.ClientOutCheck ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "ENABLE_CLIENT_NOTIP", Acl.ClientNotIPCheck ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_CLIENT_NUM", Acl.ClientOutNum.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_CLIENT_TIME", Acl.ClientOutTime.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_SAME_CLIENT", Acl.LimitSameClient.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "LIMIT_LOGIN_COUNT", Acl.FailureCount.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "CLOSED_PASSWORD", Acl.ClosedPassward, iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "CLOSED_COMMENT", Acl.ClosedComment, iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "ENABLE_HOUR", Acl.TrafficHour.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "PORT_FULL_ONLY", Acl.PortFullOnlyCheck ? "1" : "0", iniFile);

            IniFileHandler.WritePrivateProfileString("ACL", "ENABLE_STOPMB", Acl.StopFlag ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "ENABLE_CONDL", Acl.DLContainFlag?"1":"0", iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "STOP_SYSTEM", Acl.StopSelect.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "STOP_MB", Acl.StopMB.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "MB_SELECT", Acl.StopMBSelect.ToString(), iniFile);
            ///IniFileHandler.WritePrivateProfileString("ACL", "DAY_SELECT", Acl.StopDaySelect.ToString(), iniFile);
            ///IniFileHandler.WritePrivateProfileString("ACL", "SUNDAY_UP", Acl.SunDayUP ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("ACL", "SET_USER_IP_CHECK", Acl.SetUserIpCheck ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("BNDWTH", "ENABLE", BndWth.EnableBandWidth ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("BNDWTH", "MODE", BndWth.BandStopMode.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("BNDWTH", "VALUE", BndWth.BandStopValue.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("BNDWTH", "UNIT", BndWth.BandStopUnit.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("BNDWTH", "RESV_FLAG", BndWth.BandStopResv ? "1" : "0", iniFile);

            IniFileHandler.WritePrivateProfileString("FORM", "X", Form.X.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "Y", Form.Y.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "W", Form.W.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "H", Form.H.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "SPLIT1", Form.SplitDistance1.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "SPLIT2", Form.SplitDistance2.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "LP_COLLAPSED", Form.LeftPanelCollapsed ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_KAGAMI", Form.KagamiListColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_MONALL", Form.MonAllViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_MON", Form.MonViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_CLIENT", Form.ClientViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_RESERVE", Form.ResvViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_KICK", Form.KickViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_LOG", Form.LogViewColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "CLM_SCHEDULE", Form.ScheduleColumn, iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "MON_UNIT", Form.monViewUnit.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("FORM", "ICON", Form.IconIndex.ToString(), iniFile);

            // List<string> → カンマ区切りstring変換
            str = "";
            foreach (string s in Gui.FavoriteList)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("GUI", "FAVORITE_LIST", str, iniFile);

            // List<string> → カンマ区切りstring変換
            str = "";
            foreach (string s in Gui.ReserveList)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("GUI", "RESERVE_LIST", str, iniFile);


            // List<int> → カンマ区切りstring変換
            str = "";
            foreach (int s in Gui.PortList)
                str += s.ToString() + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("GUI", "PORT_LIST", str, iniFile);

            // List<int> → カンマ区切りstring変換
            /*str = "";
            string str1 = "";
            foreach (int s in Gui.PortList)
            {
                Kagami k = IndexOf(s);
                str += k.Status.VirtualHost + ",";
                str1 += k.Status.SQLOn ? "1" : "0";

            }
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            if (str1.Length >= 1)
                str1 = str1.Remove(str1.Length - 1);
            IniFileHandler.WritePrivateProfileString("PORT", "VIRTUAL_HOST", str, iniFile);
            IniFileHandler.WritePrivateProfileString("PORT", "SQL_ON", str1, iniFile);
            */
            // List<int> → カンマ区切りstring変換
            str = "";
            foreach (int s in Hp.StartPortList)
                str += s.ToString() + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("HP", "START_PORTLIST", str, iniFile);

            str = "";
            for (int i = 0; i < Gui.PortList.Count; i++)
            {
                str += Gui.PortList[i].ToString() + ",";
                try
                {
                    str += Front.IndexOf(Gui.PortList[i]).Status.Conn_UserSet.ToString() + ",";
                }
                catch
                {
                    str += Gui.Conn + ",";
                }
            }
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("GUI", "SET_LIST", str, iniFile);

            IniFileHandler.WritePrivateProfileString("GUI", "IMPORT_URL", Gui.ImportURL, iniFile);
            IniFileHandler.WritePrivateProfileString("GUI", "CONN", Gui.Conn.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("GUI", "RESV", Gui.Reserve.ToString(), iniFile);

            IniFileHandler.WritePrivateProfileString("HP", "USE_HP", Hp.UseHP ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "HOSTNAME", Hp.IpHTTP, iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "PORT", Hp.PortHTTP.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "PUBLIC_DIR", Hp.PublicDir, iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "IP_CHECK_URL", Hp.IPCheckURL, iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "IP_CHECK_LINE", Hp.IPCheckLine.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "HP_START", Hp.HPstartON ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "PORT_START", Hp.PortstartON ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "AUTH_ENABLE", Hp.AuthEnable ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "AUTH_USER", Hp.AuthUser, iniFile);
            IniFileHandler.WritePrivateProfileString("HP", "AUTH_PASS", Hp.AuthPass, iniFile);

            IniFileHandler.WritePrivateProfileString("KICK", "CHECK_SECOND", Kick.KickCheckSecond.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("KICK", "CHECK_TIME", Kick.KickCheckTime.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("KICK", "DENY_TIME", Kick.KickDenyTime.ToString(), iniFile);

            IniFileHandler.WritePrivateProfileString("LOG", "FILE1", Log.HpLogFile, iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "FILE2", Log.KagamiLogFile, iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "ENABLE_DETAIL", Log.LogDetail ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "TRS_UP_DAY", Log.TrsUpDay.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "TRS_DL_DAY", Log.TrsDlDay.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "TRS_UP_MON", Log.TrsUpMon.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "TRS_DL_MON", Log.TrsDlMon.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("LOG", "LAST_UPDATE", Log.LastUpdate, iniFile);

            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_BROWSER_VIEW", Opt.BrowserView ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "BROWSER_MODE", Opt.BrowserViewMode ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_KAGAMIEXE", Opt.PriKagamiexe ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_KAGAMIN", Opt.PriKagamin ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_BALLOONTIP", Opt.BalloonTip ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_PUSH", Opt.EnablePush ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_INFO", Opt.EnableInfo ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_ADMIN", Opt.EnableAdmin ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "IMPORT_REDIRECT", Opt.ImportRedirect ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ADMIN_PASS", Opt.AdminPass, iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "SOUND_CONN_OK", Opt.SndConnOkFile, iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "SOUND_CONN_NG", Opt.SndConnNgFile, iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "SOUND_DISC", Opt.SndDiscFile, iniFile);
            //IniFileHandler.WritePrivateProfileString("OPT", "OUT_URL", Opt.OutUrl, iniFile);

            // List<string> → カンマ区切りstring変換
            str = "";
            foreach (string s in Opt.OutUrl)
                str += s + ",";
            if (str.Length >= 1)
                str = str.Remove(str.Length - 1); // 余計な末尾のカンマ消し
            IniFileHandler.WritePrivateProfileString("OPT", "OUT_URL", str, iniFile);


            IniFileHandler.WritePrivateProfileString("OPT", "APP_NAME", Opt.AppName, iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_INTRAY", Opt.InTrayOn ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_WEBKICK", Opt.WebKick ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "AUTH_ID", Opt.AuthID, iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "AUTH_PASSWORD", Opt.AuthPass, iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "AUTH_WEB", Opt.AuthWebSet ? "1" : "0", iniFile);

            IniFileHandler.WritePrivateProfileString("OPT", "RESET_TIME",Opt.ResetDayTime , iniFile);

            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_WMP", Opt.ViewWMP ? "1" : "0", iniFile);

            IniFileHandler.WritePrivateProfileString("OPT", "ENABLE_TRANS", Opt.TransKagamin ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "NOT_TRANS", Opt.NotMyTrans ? "1" : "0", iniFile);

            IniFileHandler.WritePrivateProfileString("OPT", "IP_ERROR", Opt.NotDialogIPError ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "AUTH_WEB", Opt.AuthWebSet ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("OPT", "TRANS_CONT", Opt.TransCont.ToString(), iniFile);

            IniFileHandler.WritePrivateProfileString("RETRY", "IN_INTERVAL", Retry.InRetryInterval.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("RETRY", "IN_TIME", Retry.InRetryTime.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("RETRY", "OUT_INTERVAL", Retry.OutRetryInterval.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("RETRY", "OUT_TIME", Retry.OutRetryTime.ToString(), iniFile);

            IniFileHandler.WritePrivateProfileString("SOCK", "CONN_TO", Sock.SockConnTimeout.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "RECV_TO", Sock.SockRecvTimeout.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "SEND_TO", Sock.SockSendTimeout.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "QUEUE_SIZE", Sock.SockSendQueueSize.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "CLOSE_DELAY", Sock.SockCloseDelay.ToString(), iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "UPNP_ENABLE", Sock.upnp ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "HOST_ENABLE", Sock.VirtualHost ? "1" : "0", iniFile);
            IniFileHandler.WritePrivateProfileString("SOCK", "CONINFO_ENABLE", Sock.ConnInfoSend ? "1" : "0", iniFile);

            int _sch_num = ScheduleItem.Count;
            IniFileHandler.WritePrivateProfileString("SCH", "NUM", _sch_num.ToString(), iniFile);
            for (int _num = 0; _num < _sch_num; _num++)
            {
                string _s = (ScheduleItem[_num].Enable ? "1" : "0")
                    + "," + ScheduleItem[_num].Event
                    + "," + ScheduleItem[_num].Port
                    + "," + ScheduleItem[_num].StartType
                    + "," + ScheduleItem[_num].Week
                    + "," + ScheduleItem[_num].Hour
                    + "," + ScheduleItem[_num].Min
                    + "," + ScheduleItem[_num].TrfType
                    + "," + ScheduleItem[_num].TrfValue
                    + "," + ScheduleItem[_num].TrfUnit
                    + "," + ScheduleItem[_num].Conn
                    + "," + ScheduleItem[_num].Resv;
                IniFileHandler.WritePrivateProfileString("SCH", _num.ToString(), _s, iniFile);
            }

            /* 読み書きサンプル
            
            // キーと値を書き加える
            IniFileHandler.WritePrivateProfileString("アプリ1", "キー1", "ハロー", @"c:\sample.ini");
            IniFileHandler.WritePrivateProfileString("アプリ1", "キー2", "1234", @"c:\sample.ini");
            IniFileHandler.WritePrivateProfileString("アプリ2", "キー1", "good morning", @"c:\sample.ini");

            // 文字列を読み出す
            StringBuilder sb = new StringBuilder(1024);
            IniFileHandler.GetPrivateProfileString("アプリ1", "キー1", "default", sb, (uint)sb.Capacity, @"c:\sample.ini");
            Console.WriteLine("アプリ1セクションに含まれるキー1の値: {0}", sb.ToString());

            // 整数値を読み出す
            uint resultValue = IniFileHandler.GetPrivateProfileInt("アプリ1", "キー2", 0, @"c:\sample.ini");
            Console.WriteLine("アプリ1セクションに含まれるキー2の値: {0}", resultValue);

            // 指定セクションのキーの一覧を得る
            byte[] ar1 = new byte[1024];
            uint resultSize1 = IniFileHandler.GetPrivateProfileStringByByteArray("アプリ1", null, "default", ar1, (uint)ar1.Length, @"c:\sample.ini");
            string result1 = System.Text.Encoding.Default.GetString(ar1, 0, (int)resultSize1 - 1);
            string[] keys = result1.Split('\0');
            foreach (string key in keys)
            {
                Console.WriteLine("アプリ1セクションに含まれるキー名: {0}", key);
            }

            // 指定ファイルのセクションの一覧を得る
            byte[] ar2 = new byte[1024];
            uint resultSize2 = IniFileHandler.GetPrivateProfileStringByByteArray(null, null, "default", ar2, (uint)ar2.Length, @"c:\sample.ini");
            string result2 = System.Text.Encoding.Default.GetString(ar2, 0, (int)resultSize2 - 1);
            string[] sections = result2.Split('\0');
            foreach (string section in sections)
            {
                Console.WriteLine("このファイルに含まれるセクション名: {0}", section);
            }

            // 1つのキーと値のペアを削除する
            IniFileHandler.WritePrivateProfileString("アプリ2", "キー1", null, @"c:\sample.ini");

            // 指定セクション内の全てのキーと値のペアを削除する
            IniFileHandler.WritePrivateProfileString("アプリ1", null, null, @"c:\sample.ini");
            */
        }
        #endregion

        #region 鏡の追加/削除/読出
        /// <summary>
        /// 鏡リストにデータ追加
        /// </summary>
        /// <param name="_k"></param>
        static public void Add(Kagami _k)
        {
            lock (KagamiList)
            {
                // 単にAddするんじゃなく、Port毎にSortされるように追加
                int cnt;
                for (cnt = 0; cnt < KagamiList.Count; cnt++)
                    if (KagamiList[cnt].Status.MyPort > _k.Status.MyPort)
                        break;
                KagamiList.Insert(cnt, _k);
            }
            // GUI更新
            Event.EventUpdateKagami();
        }

        /// <summary>
        /// 鏡リストからデータ削除
        /// </summary>
        /// <param name="_k"></param>
        static public void Delete(Kagami _k)
        {
            if (_k == null)
                return;
            // Add完了前にRemoveされた時を考慮して
            // 念のためリトライ処理を入れておく
            while (true)
            {
                try
                {
                    lock (KagamiList)
                    {
                        _k.Status.RunStatus = false;
                        KagamiList.Remove(_k);
                    }
                    // GUI更新
                    Event.EventUpdateKagami();
                    break;
                }
                catch
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
        }

        /// <summary>
        /// 全鏡接続を終了する
        /// </summary>
        static public void AllStop()
        {
            while (true)
            {
                try
                {
                    foreach (Kagami _k in KagamiList)
                    {
                        _k.Stop();
                    }
                    break;
                }
                catch
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
        }

        /// <summary>
        /// 指定されたポートの鏡参照クラスを返却
        /// </summary>
        /// <param name="_port">ポート番号</param>
        /// <returns></returns>
        static public Kagami IndexOf(int _port)
        {
            lock (KagamiList)
            {
                foreach (Kagami k in KagamiList)
                {
                    if (k.Status.MyPort == _port)
                        return k;
                }
                return null;
            }
        }
        #endregion

        #region クライアントのキック登録
        /*
        /// <summary>
        /// KickList上のユーザをKick開始状態にする。
        /// KickListにない場合は追加する。
        /// </summary>
        /// <param name="_ip">KickするIP</param>
        /// <param name="_deny_tim">Kick時間(秒)</param>
        static public void AddKickUser(string _ip, int _deny_tim)
        {
            lock (KickList)
            {
                if (_deny_tim != -1)
                {
                    if (KickList.ContainsKey(_ip) == true)
                        KickList[_ip] = DateTime.Now.AddSeconds(_deny_tim).ToString() + ",0";
                    else
                        KickList.Add(_ip, DateTime.Now.AddSeconds(_deny_tim).ToString() + ",0");
                }
                else
                {
                    if (KickList.ContainsKey(_ip) == true)
                        KickList[_ip] = "-1,0";
                    else
                        KickList.Add(_ip, "-1" + ",0");
                }
            }

        }
        /// <summary>
        /// KickList上のユーザをKick開始状態にする。
        /// KickListにない場合は追加する。
        /// 時間無制限
        /// </summary>
        /// <param name="_ip">KickするIP</param>
        static public void AddKickUser(string _ip)
        {
            AddKickUser(_ip, -1);
        }
        /// <summary>
        /// KickList上のユーザをKick解除状態にする。
        /// KickListにない場合は追加する。
        /// </summary>
        /// <param name="_ip">Kick解除するIP</param>
        static public void DelKickUser(string _ip)
        {
            lock (KickList)
            {
                if (KickList.ContainsKey(_ip) == true)
                    KickList[_ip] = DateTime.Now.ToString() + ",1";
                else
                    KickList.Add(_ip, DateTime.Now.ToString() + ",1");
            }
        }
         */
        #endregion

        /// <summary>
        /// グローバルIPを取得して保持する
        /// </summary>
        static public bool GetGlobalIP()
        {
            if (Front.Hp.IPCheckURL == "" || Front.Hp.IPCheckLine <= 0)
            {
                if (Front.Opt.NotDialogIPError)
                    MessageBox.Show("グローバルIP取得ページの設定が正しくありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            // 1分以内の再取得はスキップする
            if (GlobalIP != "" && GlobalIPGetTime > DateTime.Now)
                return true;
            //WebClientの作成
            System.Net.WebClient wc = new System.Net.WebClient();
            //文字コードを指定
            wc.Encoding = System.Text.Encoding.GetEncoding(51932);

            string str;
            try
            {
                //HTMLソースをダウンロードする
                str = wc.DownloadString(Front.Hp.IPCheckURL);
            }
            catch
            {
                //後始末
                wc.Dispose();
                if (Front.Opt.NotDialogIPError)
                    MessageBox.Show("IPアドレス確認用ホストにつながりませんでした" + Environment.NewLine +
                                    "URL:" + Front.Hp.IPCheckURL + Environment.NewLine,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            //後始末
            wc.Dispose();
            try
            {
                //指定された行を取り出す
                string line = str.Split('\n')[Front.Hp.IPCheckLine - 1];
                //IPアドレスらしきものを検索
                Match index = Regex.Match(line, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
                if (index.Success)
                {
                    GlobalIP = line.Substring(index.Index, index.Length);
                    GlobalIPGetTime = DateTime.Now.AddSeconds(60);   // 次回取得可能なのは１分後
                    return true;
                }
                else
                {
                    if (Front.Opt.NotDialogIPError)
                        MessageBox.Show("IPアドレスが取得できませんでした\r\n" +
                                        "URL:" + Front.Hp.IPCheckURL + "\r\n" +
                                        "LINE:" + Front.Hp.IPCheckLine + "\r\n" +
                                        "STRING:" + line + "\r\n",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// 文字コードを判別する
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        static public System.Text.Encoding GetCode(byte[] bytes)
        {
            return GetCode(bytes, bytes.Length);
        }
        /// <summary>
        /// 文字コードを判別する
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        static public System.Text.Encoding GetCode(byte[] bytes,int len)
        {
            const byte bESC = 0x1B;
            const byte bAT = 0x40;
            const byte bDollar = 0x24;
            const byte bAnd = 0x26;
            const byte bOP = 0x28;    //(
            const byte bB = 0x42;
            const byte bD = 0x44;
            const byte bJ = 0x4A;
            const byte bI = 0x49;

            int binary = 0;
            int ucs2 = 0;
            int sjis = 0;
            int euc = 0;
            int utf8 = 0;
            byte b1, b2;

            for (int i = 0; i < len; i++)
            {
                if (bytes[i] <= 0x06 || bytes[i] == 0x7F || bytes[i] == 0xFF)
                {
                    //'binary'
                    binary++;
                    if (len - 1 > i && bytes[i] == 0x00
                        && i > 0 && bytes[i - 1] <= 0x7F)
                    {
                        //smells like raw unicode
                        ucs2++;
                    }
                }
            }

            if (binary > 0)
            {
                if (ucs2 > 0)
                    //JIS
                    //ucs2(Unicode)
                    return System.Text.Encoding.Unicode;
                else
                    //binary
                    return null;
            }

            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];

                if (b1 == bESC)
                {
                    if (b2 >= 0x80)
                        //not Japanese
                        //ASCII
                        return System.Text.Encoding.ASCII;
                    else if (len - 2 > i &&
                        b2 == bDollar && bytes[i + 2] == bAT)
                        //JIS_0208 1978
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 2 > i &&
                        b2 == bDollar && bytes[i + 2] == bB)
                        //JIS_0208 1983
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 5 > i &&
                        b2 == bAnd && bytes[i + 2] == bAT && bytes[i + 3] == bESC &&
                        bytes[i + 4] == bDollar && bytes[i + 5] == bB)
                        //JIS_0208 1990
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 3 > i &&
                        b2 == bDollar && bytes[i + 2] == bOP && bytes[i + 3] == bD)
                        //JIS_0212
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 2 > i &&
                        b2 == bOP && (bytes[i + 2] == bB || bytes[i + 2] == bJ))
                        //JIS_ASC
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    else if (len - 2 > i &&
                        b2 == bOP && bytes[i + 2] == bI)
                        //JIS_KANA
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                }
            }

            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((b1 >= 0x81 && b1 <= 0x9F) || (b1 >= 0xE0 && b1 <= 0xFC)) &&
                    ((b2 >= 0x40 && b2 <= 0x7E) || (b2 >= 0x80 && b2 <= 0xFC)))
                {
                    sjis += 2;
                    i++;
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((b1 >= 0xA1 && b1 <= 0xFE) && (b2 >= 0xA1 && b2 <= 0xFE)) ||
                    (b1 == 0x8E && (b2 >= 0xA1 && b2 <= 0xDF)))
                {
                    euc += 2;
                    i++;
                }
                else if (len - 2 > i &&
                    b1 == 0x8F && (b2 >= 0xA1 && b2 <= 0xFE) &&
                    (bytes[i + 2] >= 0xA1 && bytes[i + 2] <= 0xFE))
                {
                    euc += 3;
                    i += 2;
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if ((b1 >= 0xC0 && b1 <= 0xDF) && (b2 >= 0x80 && b2 <= 0xBF))
                {
                    utf8 += 2;
                    i++;
                }
                else if (len - 2 > i &&
                    (b1 >= 0xE0 && b1 <= 0xEF) && (b2 >= 0x80 && b2 <= 0xBF) &&
                    (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF))
                {
                    utf8 += 3;
                    i += 2;
                }
            }

            if (euc > sjis && euc > utf8)
                //EUC
                return System.Text.Encoding.GetEncoding(51932);
            else if (sjis > euc && sjis > utf8)
                //SJIS
                return System.Text.Encoding.GetEncoding(932);
            else if (utf8 > euc && utf8 > sjis)
                //UTF8
                return System.Text.Encoding.UTF8;

            // Unknown... ASCII
            return System.Text.Encoding.ASCII;
        }
        /// <summary>
        /// 帯域をKbpsで統一する
        /// </summary>
        /// <param name="_limit"></param>
        /// <param name="_unit"></param>
        /// <returns></returns>
        static public int CnvLimit(int _limit, int _unit)
        {
            switch (BandStopUnitString[_unit])
            {
                case "KB/s":
                    return _limit * 8;
                case "MB/s":
                    return _limit * 8000;
                case "Kbps":
                    return _limit;
                case "Mbps":
                    return _limit * 1000;
                default:
                    throw new Exception("not implemented.");
            }
        }
        /// <summary>
        /// 文字列連結メソッド(StringBuilder)
        /// </summary>
        /// <param name="_appendstr"></param>
        /// <returns></returns>
        static public string AppendStringBuilder(params string[] _appendstr)
        {
            StringBuilder strBil = new StringBuilder();
            strBil.Length = 0;

            for (int i = 0; i < _appendstr.Length; i++)
            {
                strBil.Append(_appendstr[i]);
            }
            string _r = strBil.ToString();
            strBil.Length = 0;
            
            return _r;


        }


        #region ログ出力
        private class Logp
        {
            public int LogLv;
            public Status Status;
            public string Contents;

            public Logp(int _log, Status _status, string _con)
            {
                LogLv = _log;
                Status = _status;
                Contents = _con;
            }

        }
        /// <summary>
        /// 通常のログ出力
        /// logLv=0 一般ログ出力
        /// logLv=1 重要ログ出力
        /// logLv=2 ログファイル書き出しのみ
        /// 注意:
        /// オブジェクトをlockしたまま本メソッドを起動し、
        /// GUIスレッドでも同一オブジェクトがlock待ちとなった場合、
        /// GUIスレッドへのInvoke通信が始まらずに
        /// デッドロックになる可能性があるので注意すること
        /// </summary>
        /// <param name="_logLv">重要度</param>
        /// <param name="_status">発生元Status</param>
        /// <param name="_content">出力メッセージ</param>
        static public void AddLogData(int _logLv, Status _status, string _content)
        {
            //ThreadPool.QueueUserWorkItem(new WaitCallback(AddLogData), new Logp(_logLv, _status, _content));

            DateTime _dtNow = DateTime.Now;
#if DEBUG
            System.Diagnostics.Trace.WriteLine("■NORMAL" + _logLv + _dtNow.ToString("[MM/dd HH:mm:ss]") + "[" + _status.MyPort + "]" + _content);
#endif

            try
            {
                // GUI出力用LogItemへの出力
                ListViewItem _item = new ListViewItem();
                _item.Text = _dtNow.ToString("MM/dd HH:mm:ss");
                _item.SubItems.Add(_content);
                // 一般ログ
                if (_logLv < 2)
                {
                    lock (_status.Gui.LogAllItem)
                    {
                        if (_status.Gui.LogAllItem.Count > 500)
                            _status.Gui.LogAllItem.RemoveRange(0, _status.Gui.LogAllItem.Count - 500);
                        _status.Gui.LogAllItem.Add(_item);
                    }
                }
                // 重要ログ
                if (_logLv == 1)
                {
                    lock (_status.Gui.LogImpItem)
                    {
                        if (_status.Gui.LogImpItem.Count > 500)
                            _status.Gui.LogImpItem.RemoveRange(0, _status.Gui.LogImpItem.Count - 500);
                        _status.Gui.LogImpItem.Add(_item);
                    }
                }
                // GUI更新
                // 出力したいログのLvが、現在のGUI表示モードより上ならGUI更新
                if ((_logLv >= LogMode && _logLv != 2))
                    Event.EventUpdateLog(_logLv, _status.Kagami, _item);

                // ログファイルへの出力
                lock (Front.Log.KagamiLogFile)
                {
                    if (Front.Log.KagamiLogFile.Length != 0)
                    {
                        /*
                        Regex y = new Regex("yyyy");
                        Regex ym = new Regex("yyyymm");
                        Regex ymd = new Regex("yyyymmdd");
                        string logFile = y.Replace(ym.Replace(ymd.Replace(Front.Log.KagamiLogFile, _dtNow.ToString("yyyyMMdd")), _dtNow.ToString("yyyyMM")), _dtNow.ToString("yyyy"));
                        */
                        string logFile = FilePathTrans(Front.Log.KagamiLogFile);

                        if (!Directory.Exists(logFile))
                        {
                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(logFile));
                            }
                            catch { }
                        }
                        try
                        {
                            StreamWriter log = new StreamWriter(logFile, true);
                            string str = _dtNow.ToString() + " [" + _status.MyPort + "] " + _content;
                            log.WriteLine(str);
                            log.Close();
                        }
                        catch (DirectoryNotFoundException dnfe)
                        {
                            dnfe.ToString();    // warning対策
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("ログ書き込みエラー:" + _content + "\r\nErrorMsg:" + e.Message + "\r\nTrace:" + e.StackTrace);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ログ書き込みエラー:" + _content + "\r\nErrorMsg:" + e.Message + "\r\nTrace:" + e.StackTrace);
            }

        }
        static public string FilePathTrans(string _path)
        {
            DateTime _dtNow = DateTime.Now;
            return _path.Replace("yyyy", _dtNow.ToString("yyyy"))
                        .Replace("mm", _dtNow.ToString("MM"))
                        .Replace("dd", _dtNow.ToString("dd"))
                        .Replace("hh", _dtNow.ToString("HH"))
                        .Replace("MM", _dtNow.ToString("mm"))
                        .Replace("ss", _dtNow.ToString("ss"));
        }
        static private void AddLogData(object obj)
        {
            int _logLv = ((Logp)obj).LogLv;
            Status _status = ((Logp)obj).Status;
            string _content = ((Logp)obj).Contents;

          
        }
        static public void DiscClient(string ip,Kagami k)
        {
            foreach (ListViewItem temp in k.Status.Gui.ClientItem)
            {
                if (temp.SubItems[1].Text == ip)
                {
                    k.Status.Client.Disc(temp.SubItems[0].Text);
                }

            }


        }
        /// <summary>
        /// 詳細ログ出力
        /// オプションで詳細ログ出力ONの時のみ有効、ログファイルに直接出力
        /// </summary>
        /// <param name="_content">出力メッセージ</param>
        static public void AddLogDetail(string _content)
        {
            if (Front.Log.LogDetail)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(AddLogDetail), _content);
            }
        }
        static private void AddLogDetail(object obj)
        {
            string _content = obj.ToString();
            DateTime _dtNow = DateTime.Now;

#if DEBUG
            System.Diagnostics.Trace.WriteLine("■DETAIL" + _dtNow.ToString("[MM/dd HH:mm:ss]") + _content);
#endif
            lock (Front.Log.KagamiLogFile)
            {
                if (Front.Log.KagamiLogFile.Length != 0)
                {
                    Regex ym = new Regex("yyyymm");
                    Regex ymd = new Regex("yyyymmdd");
                    string logFile = ym.Replace(ymd.Replace(Front.Log.KagamiLogFile, _dtNow.ToString("yyyyMMdd")), _dtNow.ToString("yyyyMM"));
                    try
                    {
                        StreamWriter log = new StreamWriter(logFile, true);
                        log.WriteLine(_content);
                        log.Close();
                    }
                    catch (DirectoryNotFoundException dnfe)
                    {
                        dnfe.ToString(); // warning対策
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("ログ書き込みエラー:" + _content + "\r\nErrorMsg:" + e.Message + "\r\nTrace:" + e.StackTrace);
                    }
                }
            }


        }

        static public void AddKickLog(Status _status, string _place)
        {
         /*   
            if (_status.Kick.KickFlag[0] == null)
                AddLogData(1, _status, "KickFlag=null");
            else
                AddLogData(1, _status, _place + "により" + _status.Kick.KickFlag[0].ToString() + "に変更されました");
           */ 
        }
        /// <summary>
        /// デバッグ専用メッセージ出力
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_content"></param>
        static public void AddLogDebug(string _pos, string _content)
        {
            #if DEBUG
            DateTime _dtNow = DateTime.Now;
            if (_pos == "帯域制限" || _pos == "KICKチェック")
                return;
            System.Diagnostics.Trace.WriteLine("■DEBUG" + _dtNow.ToString("[MM/dd HH:mm:ss]") + "[" + _pos + "]" + _content);
            #endif
        }
        #endregion

    }//end of class Front
}// end of namespace



/// <summary>
/// INIファイル読み書き用
/// </summary>
class IniFileHandler {
	[DllImport("kernel32.dll")]
	public static extern uint 
		GetPrivateProfileString(string lpAppName, 
		string lpKeyName, string lpDefault, 
		StringBuilder lpReturnedString, uint nSize, 
		string lpFileName);

    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringA")]
	public static extern uint 
		GetPrivateProfileStringByByteArray(string lpAppName, 
		string lpKeyName, string lpDefault, 
		byte [] lpReturnedString, uint nSize, 
		string lpFileName);

    [DllImport("kernel32.dll")]
	public static extern uint 
		GetPrivateProfileInt( string lpAppName, 
		string lpKeyName, int nDefault, string lpFileName );

    [DllImport("kernel32.dll")]
	public static extern uint WritePrivateProfileString(
		string lpAppName,
		string lpKeyName,
		string lpString,
		string lpFileName);
}
/// <summary>
/// その他Win32NativeMethods用
/// </summary>
class NativeMethods
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessage(
        IntPtr hWnd,
        int msg,
        int wParam,
        IntPtr lParam);


    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern uint ExtractIconEx(
        [MarshalAs(UnmanagedType.LPTStr)]
　　　　string lpszFile,
        int nIconIndex,
        [MarshalAs(UnmanagedType.LPArray)]
　　　　IntPtr[] phiconLarge,
        [MarshalAs(UnmanagedType.LPArray)]
　　　　IntPtr[] phiconSmall,
        uint nIcons
    );

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool DestroyIcon(
        IntPtr hIcon);

    [DllImport("user32.Dll", CharSet = CharSet.Auto)]
    public static extern Int32 RegisterWindowMessage(
        [MarshalAs(UnmanagedType.LPTStr)] String lpString);

    /*
    [DllImport("shell32.dll")]
    public static extern IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbSizeFileInfo,
        uint uFlags);

    // SHGetFileInfo関数で使用するフラグ
    public const uint SHGFI_ICON = 0x100; // アイコン・リソースの取得
    public const uint SHGFI_LARGEICON = 0x0; // 大きいアイコン
    public const uint SHGFI_SMALLICON = 0x1; // 小さいアイコン

    // SHGetFileInfo関数で使用する構造体
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };
    */
}
