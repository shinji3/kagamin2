//#define PLUS
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Threading;
using System.Xml.Serialization;
using System.Diagnostics;

namespace Kagamin2
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// イベントハンドラのスレッド間通信用delegate
        /// </summary>
        /// <param name="_ke"></param>
        delegate void EventHandlerDelegate(KagamiEvent _ke);

        #region メンバ変数
        /// <summary>
        /// 左パネルの更新が必要な時true
        /// </summary>
        bool LeftFlag = true;
        /// <summary>
        /// 前回の使用中ポート数。
        /// BalloonTipのポート状態変更検出用。
        /// </summary>
        int LastActPortNum = 0;
        /// <summary>
        /// 自動終了有効フラグ
        /// </summary>
        bool EnableAutoExit = false;
        /// <summary>
        /// 自動シャットダウン有効フラグ
        /// </summary>
        bool EnableAutoShutdown = false;
        /// <summary>
        /// 自動終了時に終了確認をskipするためのフラグ
        /// </summary>
        bool AskFormClose = true;
        /// <summary>
        /// シャットダウン処理中フラグ
        /// </summary>
        bool ExecShutdown = false;
        /// <summary>
        /// 帯域制限スレッド
        /// </summary>
        Thread BandTh = null;
        /// <summary>
        /// 即時帯域再計算フラグ
        /// </summary>
        bool BandFlag = false;
        /// <summary>
        /// 選択中ポートの鏡参照を返却
        /// </summary>
        Kagami SelectedKagami
        {
            get
            {
                try
                {
                    if (myPort.Text != "")
                        return Front.IndexOf(int.Parse(myPort.Text));
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 現在時刻,スケジュール起動で利用
        /// 「時:分」形式
        /// </summary>
        public string Time = "";
        /// <summary>
        /// monViewで使用
        /// kbps⇔KB/s切替
        /// 0:kbps 1:KB/s
        /// </summary>
        public uint Unit = 0;

        /// <summary>
        /// TaskbarRestartMsgID
        /// </summary>
        private Int32 _uTaskbarRestartMsg = 0;
        #endregion

#if OVERLOAD
        private int overload_cnt =1;
#endif

        #region コンストラクタ
        public Form1(string[] argv)
        {
            try
            {
                InitializeComponent();
            }
            catch
            {
                MessageBox.Show("コンポーネントの初期化に失敗しました\r\n環境を確認してください.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            // イベント通知ハンドラの登録
            Event.UpdateKagami += new EventHandler(Event_UpdateGUI);
            Event.UpdateClient += new EventHandler(Event_UpdateClient);
            Event.UpdateReserve += new EventHandler(Event_UpdateReserve);
            Event.UpdateKick += new EventHandler(Event_UpdateKick);
            Event.UpdateLog += new EventHandler(Event_UpdateLog);

            Front.AppName = "Kagamin2/1.3.8";
            this.Text = Front.AppName;

            // StatusBar
            toolStripCPU.Spring = true;

            // clmClientViewのIndex退避
            Front.clmCV_ID_IDX = clmClientViewID.DisplayIndex;
            Front.clmCV_IH_IDX = clmClientViewIpHost.DisplayIndex;
            Front.clmCV_UA_IDX = clmClientViewUA.DisplayIndex;
            Front.clmCV_TM_IDX = clmClientViewTime.DisplayIndex;
            Front.clmCV_IP_IDX = clientView.Columns.Count + 0; // internal-0
            Front.clmCV_HO_IDX = clientView.Columns.Count + 1; // internal-1

            // monViewに項目設定
            monViewInit();

            // monAllViewに項目設定
            monAllViewInit();
            
            // StatusBar EX,IM,CPU表示
            statusBarUpdate();

            // 設定読込前にウインドウサイズを仮設定
            Front.Form.W = this.Width;
            Front.Form.H = this.Height;

            // ファイル→Frontに保存値を読み込み
            Front.LoadSetting();

            if(Front.Opt.AppName != "")
                Front.AppName += Front.Opt.AppName;
            Front.UserAgent = "NSPlayer/11.0.5721.5145 " + Front.AppName;

            // Front→GUIに反映
            LoadSetting();

            // アイコン設定
            // resファイルから指定できないので実行ファイルから拾うという荒業。。
            IntPtr[] sIcon = new IntPtr[1];
            IntPtr[] lIcon = new IntPtr[1];
            NativeMethods.ExtractIconEx(@"Kagamin2.exe", (int)Front.Form.IconIndex, lIcon, sIcon, 1);
            if (lIcon[0] != IntPtr.Zero)
                NativeMethods.DestroyIcon(lIcon[0]);
            if (sIcon[0] != IntPtr.Zero)
            {
                this.Icon = (Icon)Icon.FromHandle(sIcon[0]).Clone();
                NativeMethods.DestroyIcon(sIcon[0]);
            }

            // タスクトレイアイコンの設定
            TaskTrayIcon.Icon = this.Icon;
            TaskTrayIcon.BalloonTipTitle = Front.AppName;
            TaskTrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TaskTrayIcon.BalloonTipText = "";
            TaskTrayIcon.Text = Front.AppName;
            TaskTrayIcon.Visible = false;

            // コマンドライン引数の処理
            if (argv.Length >= 1)
            {
                string url = argv[0];
                if (url.StartsWith("mms://"))
                {
                    url = url.Replace("mms://", "http://");
                }
                else if (url.StartsWith("ttp://"))
                {
                    url = "h" + url;
                }
                else if (!url.StartsWith("http://"))
                {
                    url = "http://" + url;
                }
                importURL.Text = url;
                //このタイミングは無理矢理だけどね。。
                connBTN_Click((object)null, EventArgs.Empty);
            }
        }

        #endregion

        #region イベントハンドラ処理
        /// <summary>
        /// 鏡状態変化通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateGUI(object sender, EventArgs e)
        {
            // LeftPanel更新＆即時帯域再計算を行う
            LeftFlag = true;
            BandFlag = true;
        }
        /// <summary>
        /// クライアント接続切断通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateClient(object sender, EventArgs e)
        {
            // LeftPanel更新を行う
            LeftFlag = true;
            if (e != EventArgs.Empty)
            {
                // clientView,reserveViewの更新を行う
                KagamiEvent _ke = (KagamiEvent)e;
                clientViewUpdate(_ke);
            }
        }
        /// <summary>
        /// リザーブ登録削除通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateReserve(object sender, EventArgs e)
        {
            if (e != EventArgs.Empty)
            {
                // reserveViewの更新を行う
                KagamiEvent _ke = (KagamiEvent)e;
                reserveViewUpdate(_ke);
            }
        }
        
        /// <summary>
        /// クライアントキック通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateKick(object sender, EventArgs e)
        {
            if (e != EventArgs.Empty)
            {
                // kickViewの更新を行う
                KagamiEvent _ke = (KagamiEvent)e;
                kickViewUpdate(_ke);
            }
        }
        /// <summary>
        /// ログ出力通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Event_UpdateLog(object sender, EventArgs e)
        {
            if (e != EventArgs.Empty)
            {
                // logViewの更新を行う
                KagamiEvent _ke = (KagamiEvent)e;
                logViewUpdate(_ke);
            }
        }
        #endregion

        #region 設定の読み書き
        /// <summary>
        /// Front→GUIに設定反映
        /// 起動時と、オプション設定完了時に呼ばれる。
        /// </summary>
        private void LoadSetting()
        {
            // ポート一覧に設定反映
            myPort.Items.Clear();
            foreach (int i in Front.Gui.PortList)
                myPort.Items.Add(i.ToString());

            // 現在のGUIのポートが未起動なら反映したリストの先頭値に設定
            if (this.SelectedKagami == null)
                if (myPort.Items.Count > 0)
                    myPort.Text = myPort.Items[0].ToString();

            // お気に入りインポートURLを右クリックメニューに反映
            while (ImportUrlRClick.Items.Count > 7)
                ImportUrlRClick.Items.RemoveAt(7);
            foreach (string s in Front.Gui.FavoriteList)
            {
                if (s.Length > 0)
                {
                    ToolStripMenuItem _tsmi = new ToolStripMenuItem();
                    _tsmi.Text = s;
                    _tsmi.Click += PasteFavoriteURL;
                    ImportUrlRClick.Items.Add(_tsmi);
                }
            }

            // 帯域制限設定
            if (bndStopLabel.Text == "ポート毎に個別設定" && Front.BandStopTypeString[Front.BndWth.BandStopMode] == "ポート毎に個別設定")
            {
                bndStopUnit.Text = Front.BandStopUnitString[Front.BndWth.BandStopUnit];
                bndStopNumAudit();
            }
            else
            {
                bndStopLabel.Text = Front.BandStopTypeString[Front.BndWth.BandStopMode];
                //bndStopNum.Enabled = (Front.BandStopTypeString[Front.BndWth.BandStopMode] == "ポート毎に個別設定") ? true : false;
                bndStopNum.Value = Front.BndWth.BandStopValue;
                bndStopUnit.Text = Front.BandStopUnitString[Front.BndWth.BandStopUnit];
            }

            // 設定にあわせてStatusBarをAudit
            statusBarAudit();
        }
        /// <summary>
        /// GUI→Frontに設定反映
        /// …が、メインフォームからFrontに設定反映するのはForm1_FormClosingで実施。
        /// </summary>
        private void SaveSetting()
        {
            // とくになし。
        }
        #endregion

        #region フォーム全体の制御
        /// <summary>
        /// フォーム読込時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // ちらつき対策 - メインフォームのダブルバッファリング
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // ちらつき対策 - ListViewのダブルバッファリング
            LV_SetDoubleBuffer(kagamiView.Handle);
            LV_SetDoubleBuffer(monView.Handle);
            LV_SetDoubleBuffer(monAllView.Handle);
            LV_SetDoubleBuffer(clientView.Handle);
            LV_SetDoubleBuffer(reserveView.Handle);
            LV_SetDoubleBuffer(kickView.Handle);
            LV_SetDoubleBuffer(logView.Handle);

            // ウインドウ表示位置・状態リカバリー
            this.Location = new Point(Front.Form.X, Front.Form.Y);
            this.Size = new Size(Front.Form.W, Front.Form.H);

            // スプリッターの位置をリカバリー
            if (Front.Form.SplitDistance1 >= 0) splitContainer1.SplitterDistance = (int)Front.Form.SplitDistance1;
            //if (Front.Form.SplitDistance2 >= 0) splitContainer2.SplitterDistance = (int)Front.Form.SplitDistance2;

            // 左パネルのOn/Offをリカバリー
            splitContainer1.Panel1Collapsed = Front.Form.LeftPanelCollapsed;
            // 帯域制限のOn/Offリカバリー
            if (Front.BndWth.EnableBandWidth)
                StartBandWidth();

            // 日間・月間転送量のリカバリー
            // LastUpdateと比較して変化してたらクリアする
            if (Front.Log.LastUpdate != DateTime.Now.ToString("YYYYMMDD"))
            {
                // 日が違う
                Front.Log.TrsUpDay = 0;
                Front.Log.TrsDlDay = 0;
                if (!Front.Log.LastUpdate.StartsWith(DateTime.Now.ToString("YYYYMM")))
                {
                    // 月が違う
                    Front.Log.TrsUpMon = 0;
                    Front.Log.TrsDlMon = 0;
                }
            }

            // ステータスバー状態AUDIT
            statusBarAudit();

            // 各ListViewのカラム幅をリカバリー
            #region 各ListViewのカラム幅をリカバリー
            string[] _clm;
            _clm = Front.Form.KagamiListColumn.Split(',');
            if (_clm.Length == 3)
            {
                try
                {
                    clmKgmViewPort.Width = int.Parse(_clm[0]);
                    clmKgmViewImport.Width = int.Parse(_clm[1]);
                    clmKgmViewConn.Width = int.Parse(_clm[2]);
                }
                catch { }
            }
            _clm = Front.Form.MonAllViewColumn.Split(',');
            if (_clm.Length == 2)
            {
                try
                {
                    clmMonAllView1.Width = int.Parse(_clm[0]);
                    clmMonAllView2.Width = int.Parse(_clm[1]);
                }
                catch { }
            }
            _clm = Front.Form.MonViewColumn.Split(',');
            if (_clm.Length == 2)
            {
                try
                {
                    clmMonView1.Width = int.Parse(_clm[0]);
                    clmMonView2.Width = int.Parse(_clm[1]);
                }
                catch { }
            }
            _clm = Front.Form.ClientViewColumn.Split(',');
            if (_clm.Length == 4)
            {
                try
                {
                    clmClientViewID.Width = int.Parse(_clm[0]);
                    clmClientViewIpHost.Width = int.Parse(_clm[1]);
                    clmClientViewUA.Width = int.Parse(_clm[2]);
                    clmClientViewTime.Width = int.Parse(_clm[3]);
                }
                catch { }
            }
            _clm = Front.Form.ResvViewColumn.Split(',');
            if (_clm.Length == 2)
            {
                try
                {
                    clmResvViewIP.Width = int.Parse(_clm[0]);
                    clmResvViewStatus.Width = int.Parse(_clm[1]);
                }
                catch { }
            }
            _clm = Front.Form.KickViewColumn.Split(',');
            if (_clm.Length == 3)
            {
                try
                {
                    clmKickViewIP.Width = int.Parse(_clm[0]);
                    clmKickViewStatus.Width = int.Parse(_clm[1]);
                    clmKickViewCount.Width = int.Parse(_clm[2]);
                }
                catch { }
            }
            _clm = Front.Form.LogViewColumn.Split(',');
            if (_clm.Length == 2)
            {
                try
                {
                    clmLogView1.Width = int.Parse(_clm[0]);
                    clmLogView2.Width = int.Parse(_clm[1]);
                }
                catch { }
            }
            _clm = null;
            #endregion

            // ClientViewRClickのドメイン解決有効切り替え
            ClientResolveHostMenu.Checked = Front.Opt.EnableResolveHost;

            // monViewの帯域速度表示単位
            Unit = Front.Form.monViewUnit;
            monViewUpdate(null);
            
            // ImportURL/最大通常接続数/最大リザーブ接続数を終了時点でのGUI上の値でリカバリー
            importURL.Text = Front.Gui.ImportURL;
            connNum.Value = Front.Gui.Conn;
            resvNum.Value = Front.Gui.Reserve;

            // スケジュール起動用時間設定
            DateTime _dtNow = DateTime.Now;
            Time = _dtNow.ToString("HH:mm");
            // 周期更新用タイマー開始
            timer1.Enabled = true;
#if !PLUS
            MonModeChgMenu.Visible = false;
#endif
        }
        /// <summary>
        /// 指定ListViewにダブルバッファリング指示
        /// </summary>
        /// <param name="_handle"></param>
        private void LV_SetDoubleBuffer(IntPtr _handle)
        {
            /*
            private const int LVM_FIRST = 0x1000;
            private const int LVM_SETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 54);
            private const int LVM_GETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 55);
            private const int LVS_EX_DOUBLEBUFFER = 0x00010000;
            */
            int _style = NativeMethods.SendMessage(_handle, 0x1037, 0, (IntPtr)0);
            _style |= 0x10000;
            NativeMethods.SendMessage(_handle, 0x1036, 0, (IntPtr)_style);
        }

        /// <summary>
        /// フォームを閉じる直前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 終了確認
            // 強制終了・シャットダウン以外なら確認する
            if (e.CloseReason != CloseReason.TaskManagerClosing &&
                e.CloseReason != CloseReason.WindowsShutDown &&
                AskFormClose)
            {
                if (MessageBox.Show("アプリケーションを終了しますか？", "確認", MessageBoxButtons.YesNo).Equals(DialogResult.No))
                {
                    e.Cancel = true;
                    return;
                }
            }
            // 周期更新タイマー停止
            timer1.Enabled = false;

            // Webエントランス停止
            Front.WebEntrance.Stop();
            // 全鏡停止
            Front.AllStop();
            // 帯域制限スレッド停止
            if (BandTh != null && BandTh.IsAlive)
                BandTh.Abort();

            // ウインドウ情報をDataに保存
            // ウインドウ位置
            Front.Form.X = this.Location.X;
            Front.Form.Y = this.Location.Y;
            Front.Form.W = this.Size.Width;
            Front.Form.H = this.Size.Height;
            // スプリッタ位置
            Front.Form.SplitDistance1 = (uint)splitContainer1.SplitterDistance;
            Front.Form.SplitDistance2 = (uint)splitContainer2.SplitterDistance;
            // 左パネルのON/OFF状態
            Front.Form.LeftPanelCollapsed = splitContainer1.Panel1Collapsed;
            // 各ListViewのカラム幅を保存
            Front.Form.KagamiListColumn = "" + clmKgmViewPort.Width + "," + clmKgmViewImport.Width + "," + clmKgmViewConn.Width;
            Front.Form.MonAllViewColumn = "" + clmMonAllView1.Width + "," + clmMonAllView2.Width;
            Front.Form.MonViewColumn = "" + clmMonView1.Width + "," + clmMonView2.Width;
            Front.Form.ClientViewColumn = "" + clmClientViewID.Width + "," + clmClientViewIpHost.Width + "," + clmClientViewUA.Width + "," + clmClientViewTime.Width;
            Front.Form.ResvViewColumn = "" + clmResvViewIP.Width + "," + clmResvViewStatus.Width;
            Front.Form.KickViewColumn = "" + clmKickViewIP.Width + "," + clmKickViewStatus.Width + "," + clmKickViewCount.Width;
            Front.Form.LogViewColumn = "" + clmLogView1.Width + "," + clmLogView2.Width;
            // monViewの帯域速度表示単位
            Front.Form.monViewUnit = Unit;
            // ImportURL/最大通常接続数/最大リザーブ接続数を終了時点でのGUI上の値で保存
            if (importURL.Text == "待機中")
                Front.Gui.ImportURL = "";
            else
                Front.Gui.ImportURL = importURL.Text;
            Front.Gui.Conn = (uint)connNum.Value;
            Front.Gui.Reserve = (uint)resvNum.Value;
            // 今回の総転送量を加算
            Front.Log.TrsUpDay += Front.TotalUP;
            Front.Log.TrsDlDay += Front.TotalDL;
            Front.Log.TrsUpMon += Front.TotalUP;
            Front.Log.TrsDlMon += Front.TotalDL;
            Front.Log.LastUpdate = DateTime.Now.ToString("YYYYMMDD");
            // Front→ファイルに保存値を書き込み
            Front.SaveSetting();
        }

        /// <summary>
        /// ウインドウサイズ変更（最大化・最小化含む）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            //最小化でタスクトレイに格納
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (Front.Form.EnableTrayIcon)
                {
                    this.Visible = false;
                    this.TaskTrayIcon.Visible = true;
                }
            }
        }
        /// <summary>
        /// ウインドウサイズ変更開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            // 効果ないけど、一応、ね。
            tabKagami.SuspendLayout();
        }
        /// <summary>
        /// ウインドウサイズ変更完了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            tabKagami.ResumeLayout();
        }
        /// <summary>
        /// ハンドル生成時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            // タスクトレイのアイコン復活のためのメッセージ登録
            _uTaskbarRestartMsg = NativeMethods.RegisterWindowMessage("TaskbarCreated");
            base.OnHandleCreated(e);
        }
        /// <summary>
        /// ウインドウプロシージャ
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // タスクトレイのアイコンをセット
            if (m.Msg == _uTaskbarRestartMsg && _uTaskbarRestartMsg != 0)
            {
                if (this.TaskTrayIcon.Visible)
                {
                    this.TaskTrayIcon.Visible = false;
                    this.TaskTrayIcon.Visible = true;
                }
            }
            base.WndProc(ref m);
        }
        #endregion

        #region ステータスバー
        /// <summary>
        /// 左パネル縮小OnOff制御
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripLeftPanelOnOff_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();
            splitContainer1.Panel1Collapsed = !splitContainer1.Panel1Collapsed;
            Front.Form.LeftPanelCollapsed = splitContainer1.Panel1Collapsed;
            statusBarAudit();
            this.ResumeLayout();
        }
        /// <summary>
        /// 帯域制限OnOff制御
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripBandStart_Click(object sender, EventArgs e)
        {
            Front.BndWth.EnableBandWidth = !Front.BndWth.EnableBandWidth;
            statusBarAudit();
            if (Front.BndWth.EnableBandWidth)
                StartBandWidth();
            else
                StopBandWidth();
            monViewUpdate(this.SelectedKagami);
        }
        /// <summary>
        /// 鏡置き場エントランスOnOff制御
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripHPStart_Click(object sender, EventArgs e)
        {
            Front.HPStart = !Front.HPStart;
            statusBarAudit();
            if (Front.HPStart)
            {
                if (Front.WebEntrance.Start((int)Front.Hp.PortHTTP) == false)
                {
                    MessageBox.Show("エントランス起動に失敗しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Front.HPStart = false;
                    statusBarAudit();
                    return;
                }
                Front.AddLogDebug("Webエントランス", "エントランスを起動しました");
                return;
            }
            else
            {
                Front.AddLogDebug("Webエントランス", "エントランスを停止しました");
                Front.WebEntrance.Stop();
            }
        }
        /// <summary>
        /// 新規IM接続制限OnOff制御
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripHPPause_Click(object sender, EventArgs e)
        {
            Front.Pause = !Front.Pause;
            statusBarAudit();
        }
        /// <summary>
        /// 自動終了ボタン制御
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripAutoExit_Click(object sender, EventArgs e)
        {
            EnableAutoExit = !EnableAutoExit;
            statusBarAudit();
        }
        /// <summary>
        /// 自動シャットダウンボタン制御
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripAutoShutdown_Click(object sender, EventArgs e)
        {
            if (ExecShutdown == false)
            {
                EnableAutoShutdown = !EnableAutoShutdown;
            }
            else
            {
                //shutdown stop
                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = "shutdown.exe";
                psInfo.CreateNoWindow = true;
                psInfo.UseShellExecute = false;
                psInfo.Arguments = "-a";
                Process.Start(psInfo);
                //shutdown開始用ボタンに変更
                toolStripAutoShutdown.BorderStyle = Border3DStyle.Raised;
                toolStripAutoShutdown.Text = "自動電断";
                ExecShutdown = false;
                EnableAutoShutdown = false;
                AskFormClose = true;
            }
            statusBarAudit();
        }
        /// <summary>
        /// ステータスバーのボタン表示状態を更新する
        /// </summary>
        private void statusBarAudit()
        {
            // 凸スタイル:Raised、凹スタイル:Sunken

            // 左パネル表示ボタン
            if (splitContainer1.Panel1Collapsed)
                toolStripLeftPanelOnOff.BorderStyle = Border3DStyle.Raised; // OFF
            else
                toolStripLeftPanelOnOff.BorderStyle = Border3DStyle.Sunken; // ON

            // 帯域制限ボタン
            if (Front.BndWth.EnableBandWidth)
                toolStripBandStart.BorderStyle = Border3DStyle.Sunken; // ON
            else
                toolStripBandStart.BorderStyle = Border3DStyle.Raised; // OFF

            if (Front.Hp.UseHP)
            {
                // エントランス起動ボタン
                toolStripCPU.Spring = false;
                toolStripCPU.Width = toolStripEXNum.Width;
                toolStripHPStart.Visible = true;
                toolStripHPPause.Visible = true;
                toolStripCPU.Spring = true;
                if (Front.HPStart)
                    toolStripHPStart.BorderStyle = Border3DStyle.Sunken; // ON
                else
                    toolStripHPStart.BorderStyle = Border3DStyle.Raised; // OFF

                // 新規受付停止ボタン
                if (Front.Pause)
                    toolStripHPPause.BorderStyle = Border3DStyle.Sunken; // ON
                else
                    toolStripHPPause.BorderStyle = Border3DStyle.Raised; // OFF
            }
            else
            {
                toolStripCPU.Spring = false;
                toolStripCPU.Width = toolStripEXNum.Width;
                toolStripHPStart.Visible = false;
                toolStripHPPause.Visible = false;
                toolStripCPU.Spring = true;
            }

            // 自動終了ボタン
            if (EnableAutoExit)
                toolStripAutoExit.BorderStyle = Border3DStyle.Sunken;   // ON
            else
                toolStripAutoExit.BorderStyle = Border3DStyle.Raised;   // OFF

            // 自動シャットダウンボタン
            if (EnableAutoShutdown && !ExecShutdown)
                toolStripAutoShutdown.BorderStyle = Border3DStyle.Sunken; // ON
            else
                toolStripAutoShutdown.BorderStyle = Border3DStyle.Raised; // OFF
        }
        /// <summary>
        /// ステータスバーのEX,IM,CPU情報を更新する
        /// </summary>
        private void statusBarUpdate()
        {
            // 総EX数,総IM数
            int cnt_ex = 0, cnt_im = 0;
            foreach (Kagami _k in Front.KagamiList)
            {
                if (_k.Status.ImportStatus)
                    cnt_im++;
                cnt_ex += _k.Status.Client.Count;
            }
            toolStripEXNum.Text = "EX " + cnt_ex.ToString();
            toolStripIMNum.Text = "IM " + cnt_im.ToString();
            toolStripCPU.Text = "CPU " + monAllView.Items[0].SubItems[1].Text;
        }
        /// <summary>
        /// ステータスバー上でのToolTip表示用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripStatus_MouseHover(object sender, EventArgs e)
        {
            ToolStripStatusLabel _tssl = (ToolStripStatusLabel)sender;
            Point pos = MousePosition;
            pos.X -= this.Location.X; pos.X += 4;
            pos.Y -= this.Location.Y; pos.Y += 2;

            toolTip1.Show(_tssl.ToolTipText, this, pos);
        }
        /// <summary>
        /// ステータスバー上でのToolTip表示用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripStatus_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(this);
        }
        #endregion

        #region タスクトレイ

        #region タスクトレイアイコンのイベント
        /// <summary>
        /// タスクトレイアイコンのクリック/ダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskTrayIcon_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Left)
                ExitTaskTray();
        }
        /// <summary>
        /// タスクトレイから復帰
        /// </summary>
        private void ExitTaskTray()
        {
            this.Visible = true;
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.Activate();
            this.TaskTrayIcon.Visible = false;
        }
        /// <summary>
        /// タスクトレイアイコンにマウスを載せた時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskTrayIcon_MouseMove(object sender, MouseEventArgs e)
        {
            showTaskTrayTip();
        }

        /// <summary>
        /// タスクトレイアイコンにBalloonTip表示
        /// </summary>
        private void showTaskTrayTip()
        {
            if (!Front.Opt.BalloonTip)
                return;

            this.TaskTrayIcon.BalloonTipText = "";
            StringBuilder sb = new StringBuilder(1024);
            foreach (Kagami _k in Front.KagamiList)
            {
                sb.AppendLine(
                    "PORT:" + _k.Status.MyPort.ToString("####0") + " " +
                    _k.Status.Client.Count.ToString("#0") + "/" +
                    _k.Status.Connection.ToString("#0") + "(+" + _k.Status.Reserve.ToString() + ") " +
                    _k.Status.ImportURL
                    );
                sb.AppendLine(
                    "    " + _k.Status.Comment
                    );
            }
            if (sb.Length == 0)
            {
                this.TaskTrayIcon.BalloonTipText = "起動中ポートはありません.";
            }
            else
            {
                //最後の改行は削除
                sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
                this.TaskTrayIcon.BalloonTipText = sb.ToString();
            }
            this.TaskTrayIcon.ShowBalloonTip(10000);
        }
        #endregion

        #region タスクトレイメニューのイベント
        /// <summary>
        /// ウインドウ位置リセット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RstWndPos_Click(object sender, EventArgs e)
        {
            ExitTaskTray();
            int x = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - this.Width) / 2; if (x < 0) x = 0;
            int y = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - this.Height) / 2; if (y < 0) y = 0;
            this.Location = new Point(x, y);
        }
        /// <summary>
        /// 終了メニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrayMenuExit_Click(object sender, EventArgs e)
        {
            ExitTaskTray();
            this.Close();
        }
        #endregion

        #endregion

        #region 左パネル
        /// <summary>
        /// kagamiView内の一覧からダブルクリックで選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void kagamiView_DoubleClick(object sender, EventArgs e)
        {
            for (int i = 0; i < kagamiView.Items.Count; i++)
            {
                if (kagamiView.Items[i].Selected)
                {
                    if (clmKgmViewPort.DisplayIndex == 0)
                        myPort.Text = kagamiView.Items[i].Text;
                    else
                        myPort.Text = kagamiView.Items[i].SubItems[clmKgmViewPort.DisplayIndex].Text;
                    myPort_StateChanged();
                    return;
                }
            }
        }
        /// <summary>
        /// kagamiView右クリックメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void kagamiViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // 選択中アイテムがあるかチェックして、無ければDisable
            for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
            {
                if (kagamiView.Items[cnt].Selected)
                {
                    Kagami _k;
                    if (clmKgmViewPort.DisplayIndex == 0)
                        _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].Text));
                    else
                        _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].SubItems[clmKgmViewPort.DisplayIndex].Text));
                    if (_k == null)
                        continue;
                    KgmIpCopyMenu.Enabled = true;
                    KgmImIpCopyMenu.Enabled = true;
                    if (_k.Status.Type == 0)
                    {
                        KgmImPauseMenu.Visible = false;
                        KgmImDiscMenu.Visible = false;
                    }
                    else
                    {
                        KgmImPauseMenu.Visible = true;
                        KgmImPauseMenu.Enabled = true;
                        KgmImPauseMenu.Checked = _k.Status.Pause;
                        KgmImDiscMenu.Visible = true;
                        KgmImDiscMenu.Enabled = true;
                    }
                    return;
                }
            }
            KgmIpCopyMenu.Enabled = false;
            KgmImIpCopyMenu.Enabled = false;
            KgmImPauseMenu.Visible = false;
            KgmImDiscMenu.Visible = false;
        }
        /// <summary>
        /// 鏡URLコピーメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KgmIpCopyMenu_Click(object sender, EventArgs e)
        {
            // kagamiViewは複数選択は出来ないことにしているので
            // １つ見つけたら終わり
            string _url;
            if (Front.Hp.UseHP && Front.Hp.IpHTTP.Length > 0)
                _url = "http://" + Front.Hp.IpHTTP + ":";
            else
                _url = "http://" + Front.GlobalIP + ":";
            for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
            {
                if (kagamiView.Items[cnt].Selected)
                {
                    if (clmKgmViewPort.DisplayIndex == 0)
                        _url += kagamiView.Items[cnt].Text;
                    else
                        _url += kagamiView.Items[cnt].SubItems[clmKgmViewPort.DisplayIndex].Text;
                    Clipboard.SetText(_url);
                    return;
                }
            }
        }
        /// <summary>
        /// ImportURLコピーメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KgmImIpCopyMenu_Click(object sender, EventArgs e)
        {
            // kagamiViewは複数選択は出来ないことにしているので
            // １つ見つけたら終わり
            for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
            {
                if (kagamiView.Items[cnt].Selected)
                {
                    if (clmKgmViewImport.DisplayIndex == 0)
                        Clipboard.SetText(kagamiView.Items[cnt].Text);
                    else
                        Clipboard.SetText(kagamiView.Items[cnt].SubItems[clmKgmViewImport.DisplayIndex].Text);
                    return;
                }
            }
            return;
        }
        /// <summary>
        /// 接続制限(ポート個別)メニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KgmImPauseMenu_Click(object sender, EventArgs e)
        {
            // kagamiViewは複数選択は出来ないことにしているので
            // １つ見つけたら終わり
            try
            {
                for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
                {
                    if (kagamiView.Items[cnt].Selected)
                    {
                        Kagami _k;
                        if (clmKgmViewPort.DisplayIndex == 0)
                            _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].Text));
                        else
                            _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].SubItems[clmKgmViewPort.DisplayIndex].Text));
                        if (_k == null)
                            continue;
                        _k.Status.Pause = !_k.Status.Pause;
                        Event.EventUpdateKagami();
                        return;
                    }
                }
            }
            catch { }
            return;
        }
        /// <summary>
        /// Import強制切断メニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KgmImDiscMenu_Click(object sender, EventArgs e)
        {
            // kagamiViewは複数選択は出来ないことにしているので
            // １つ見つけたら終わり
            try
            {
                for (int cnt = 0; cnt < kagamiView.Items.Count; cnt++)
                {
                    if (kagamiView.Items[cnt].Selected)
                    {
                        Kagami _k;
                        if (clmKgmViewPort.DisplayIndex == 0)
                            _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].Text));
                        else
                            _k = Front.IndexOf(int.Parse(kagamiView.Items[cnt].SubItems[clmKgmViewPort.DisplayIndex].Text));
                        if (_k == null)
                            continue;
                        _k.Status.Disc();
                        return;
                    }
                }
            }
            catch { }
            return;
        }
        /// <summary>
        /// monAllViewに初期アイテムを追加する
        /// </summary>
        private void monAllViewInit()
        {
            monAllView.Items.Add("全CPU使用率");
            monAllView.Items.Add("鏡CPU使用率");
            monAllView.Items.Add("総UP帯域");
            monAllView.Items.Add("総DL帯域");
            monAllView.Items.Add("UP転送量/日");
            monAllView.Items.Add("DL転送量/日");
            monAllView.Items.Add("UP転送量/月");
            monAllView.Items.Add("DL転送量/月");
            for (int cnt = 0; cnt < monAllView.Items.Count; cnt++)
                monAllView.Items[cnt].SubItems.Add("");
            monAllViewUpdate();
        }
        /// <summary>
        /// 全体モニタの更新
        /// </summary>
        private void monAllViewUpdate()
        {
            //総up,down転送速度の計算
            int _up = 0, _down = 0;
            foreach (Kagami _k in Front.KagamiList)
            {
                _up += _k.Status.CurrentDLSpeed * _k.Status.Client.Count;
                _down += _k.Status.CurrentDLSpeed;
            }

            //総up,down転送量の計算
            //ulong _ul_day, _dl_day, _ul_mon, _dl_mon;
            string _ul_day, _dl_day, _ul_mon, _dl_mon;

            _ul_day = ((ulong)(Front.Log.TrsUpDay + Front.TotalUP)).ToString("#,##0,, [Mbyte]");
            _dl_day = ((ulong)(Front.Log.TrsDlDay + Front.TotalDL)).ToString("#,##0,, [Mbyte]");
            _ul_mon = ((ulong)(Front.Log.TrsUpMon + Front.TotalUP)).ToString("#,##0,, [Mbyte]");
            _dl_mon = ((ulong)(Front.Log.TrsDlMon + Front.TotalDL)).ToString("#,##0,, [Mbyte]");

            int idx = 0;
            // 全体のCPU使用率
            try
            {
                monAllView.Items[idx].SubItems[1].Text = System.Math.Round(Front.CPU_ALL.NextValue(), 1).ToString("0.0") + "%";
            }
            catch
            {
                monAllView.Items[idx].SubItems[1].Text = "取得NG";
            }
            idx++;
            // 鏡のCPU使用率
            try
            {
                monAllView.Items[idx].SubItems[1].Text = System.Math.Round(Front.CPU_APP.NextValue(), 1).ToString("0.0") + "%";
            }
            catch
            {
                monAllView.Items[idx].SubItems[1].Text = "取得NG";
            }
            idx++;
            // 総UP帯域
            monAllView.Items[idx].SubItems[1].Text = Unit == 0 ? _up.ToString("#,##0") + " [kbps]" : (_up/8).ToString("#,##0") + " [KB/s]"; idx++;
            // 総DOWN帯域
            monAllView.Items[idx].SubItems[1].Text = Unit == 0 ? _down.ToString("#,##0") + " [kbps]" : (_down/8).ToString("#,##0") + " [KB/s]"; idx++;
            // 総UP転送量/日
            monAllView.Items[idx].SubItems[1].Text = _ul_day; idx++;
            // 総DL転送量/日
            monAllView.Items[idx].SubItems[1].Text = _dl_day; idx++;
            // 総UP転送量/月
            monAllView.Items[idx].SubItems[1].Text = _ul_mon; idx++;
            // 総DL転送量/月
            monAllView.Items[idx].SubItems[1].Text = _dl_mon; idx++;
        }
        /// <summary>
        /// monAllViewのダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monAllView_DoubleClick(object sender, EventArgs e)
        {
            // kbps⇔KB/s切替
            if (Unit == 0)
                Unit = 1;
            else
                Unit = 0;
            // 即更新
            monAllViewUpdate();
            monViewUpdate(this.SelectedKagami);
        }
        #endregion
        
        #region 右パネル上部
        /// <summary>
        /// インポートURLでキー入力した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importURL_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter入力なら接続処理を行う
            if (e.KeyChar == (char)Keys.Enter)
            {
                connBTN_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        #region ImportURL右クリックメニュー
        /// <summary>
        /// お気に入りへ登録メニュー選択時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFavoriteMenu_Click(object sender, EventArgs e)
        {
            // 登録済みチェック
            if (Front.Gui.FavoriteList.Contains(importURL.Text))
                return;

            // 右クリックメニューに反映
            ToolStripMenuItem _tsmi = new ToolStripMenuItem();
            _tsmi.Text = importURL.Text;
            _tsmi.Click += PasteFavoriteURL;
            ImportUrlRClick.Items.Add(_tsmi);

            // 設定ファイルに反映
            Front.Gui.FavoriteList.Add(importURL.Text);
            Front.SaveSetting();
        }

        /// <summary>
        /// お気に入りをImportURLへ貼り付け
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasteFavoriteURL(object sender, EventArgs e)
        {
            ToolStripMenuItem _tsmi = (ToolStripMenuItem)sender;
            try
            {
                if (!Front.IndexOf(int.Parse(myPort.Text)).Status.RunStatus)
                    importURL.Text = _tsmi.Text;
            }
            catch
            {
                // 状態チェック失敗
                importURL.Text = _tsmi.Text;
            }
        }

        private void ImportUrlCutMenu_Click(object sender, EventArgs e)
        {
            importURL.Cut();
        }

        private void ImportUrlCopyMenu_Click(object sender, EventArgs e)
        {
            importURL.Copy();
        }

        private void ImportUrlPasteMenu_Click(object sender, EventArgs e)
        {
            importURL.Paste();
        }

        private void ImportUrlEraseMenu_Click(object sender, EventArgs e)
        {
            if (importURL.SelectionLength > 0)
                importURL.Text = importURL.Text.Remove(importURL.SelectionStart, importURL.SelectionLength);
        }
        #endregion

        /// <summary>
        /// 鏡ポート番号欄でキー入力した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter入力なら接続処理を行う
            if (e.KeyChar == (char)Keys.Enter)
            {
                connBTN_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }
        /// <summary>
        /// 接続ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connBTN_Click(object sender, EventArgs e)
        {
            int _port;

            if (!Front.Hp.UseHP && !Front.Opt.EnablePush && importURL.Text == "")
            {
                MessageBox.Show("インポートURLを入力してください。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try { _port = int.Parse(myPort.Text); }
            catch { MessageBox.Show("ポート番号が異常です。\r\n65535以下の数値を設定してください。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            connBTN.Enabled = false;
            if(Front.IndexOf(_port) == null)
            {
                logView.Items.Clear();
                Kagami _k = new Kagami(importURL.Text, _port, (int)connNum.Value, (int)resvNum.Value);
                // 個別帯域設定の場合、上限帯域を設定しておく
                if(Front.BndWth.BandStopMode == 2)
                {
                    _k.Status.GUILimitUPSpeed = (int)Front.BndWth.BandStopValue;
                    _k.Status.LimitUPSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                }
                Front.Add(_k);
            }
        }

        /// <summary>
        /// 切断ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void discBTN_Click(object sender, EventArgs e)
        {
            discBTN.Enabled = false;
            Kagami _k = this.SelectedKagami;
            if (_k != null)
                _k.Status.RunStatus = false;
        }

        /// <summary>
        /// 接続・切断ボタン状態オーディット
        /// </summary>
        private void ButtonAudit()
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                if (connBTN.Enabled == false)
                {
                    // ポート未接続でconnボタン無効：切断完了
                    connBTN.Enabled = true;
                    discBTN.Enabled = false;
                    // 待機中/接続中→未接続になったのでGUI表示更新
                    myPort_StateChanged();
                }
                else
                {
                    // 未接続状態でconnボタン有効：Idle中
                }
                // TitleBar更新
                this.Text = Front.AppName;
            }
            else
            {
                if (_k.Status.RunStatus == true)
                {
                    if (discBTN.Enabled == false)
                    {
                        // ポート接続状態でdiscボタン無効：接続完了
                        connBTN.Enabled = false;
                        discBTN.Enabled = true;
                        // 未接続→待機中になったのでGUI表示更新
                        myPort_StateChanged();
                    }
                    else
                    {
                        // ポート接続状態でdiscボタン有効：Running中
                        if (importURL.Text != _k.Status.ImportURL)
                            importURL.Text = _k.Status.ImportURL;
                    }
                }
                else
                {
                    // ポート接続状態でRunStatus==false：切断処理中
                }
                // TitleBar更新
                this.Text = "EX " + _k.Status.Client.Count + "/" + _k.Status.Connection + "+" + _k.Status.Reserve
                    + " PORT:" + _k.Status.MyPort + " " + Front.AppName;
            }

        }

        /// <summary>
        /// 消去ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearBTN_Click(object sender, EventArgs e)
        {
            if(importURL.Enabled)
                importURL.Text = "";
        }

        /// <summary>
        /// 鏡ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyMyUrlBTN_Click(object sender, EventArgs e)
        {
            if (Front.GetGlobalIP())
                Clipboard.SetText("http://" + Front.GlobalIP + ":" + myPort.Text + "/");
        }

        /// <summary>
        /// 設ボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optBTN_Click(object sender, EventArgs e)
        {
            Option optDlg = new Option();
            // 設定画面表示
            optDlg.ShowDialog();
            // 新設定をロード
            LoadSetting();
        }

        /// <summary>
        /// 自ポート番号を変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myPort_TextChanged(object sender, EventArgs e)
        {
            myPort_StateChanged();
        }
        /// <summary>
        /// ポート状態変更
        /// </summary>
        private void myPort_StateChanged()
        {
            ButtonAudit();

            Kagami _k = this.SelectedKagami;

            //右パネル上部の状態を修正
            if (_k != null)
            {
                //起動中ポート
                importURL.Text = _k.Status.ImportURL;
                importURL.Enabled = false;
                connNum.Value = _k.Status.Conn_UserSet;
                resvNum.Value = _k.Status.Reserve;
            }
            else
            {
                //未起動ポート
                if (importURL.Text == "待機中")
                    importURL.Text = "";
                importURL.Enabled = true;
            }
            //tabKagamiのタブ状態を修正
            tabKagami_SelectedIndexChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// 最大通常接続数を変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connNum_ValueChanged(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.Conn_UserSet = (int)connNum.Value;
                if (Front.BndWth.EnableBandWidth)
                {
                    // 帯域制限中は人数減らしは即反映。
                    // 人数増加は帯域制限タスクでの再計算契機に行う。
                    if (_k.Status.Connection > connNum.Value)
                        _k.Status.Connection = (int)connNum.Value;
                }
                else
                {
                    _k.Status.Connection = (int)connNum.Value;
                }
                LeftFlag = true;
                BandFlag = true;
            }
            else
            {
                Front.Gui.Conn = (uint)connNum.Value;
            }
        }
        /// <summary>
        /// 最大通常接続数を変更（キー入力）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connNum_KeyUp(object sender, KeyEventArgs e)
        {
            connNum_ValueChanged(sender, (EventArgs)e);
        }

        /// <summary>
        /// 最大リザーブ接続数を変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resvNum_ValueChanged(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.Reserve = (int)resvNum.Value;
                LeftFlag = true;
                BandFlag = true;
            }
            else
            {
                Front.Gui.Reserve = (uint)resvNum.Value;
            }
        }
        /// <summary>
        /// 最大リザーブ接続数を変更（キー入力）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resvNum_KeyUp(object sender, KeyEventArgs e)
        {
            resvNum_ValueChanged(sender, (EventArgs)e);
        }
        #endregion

        /// <summary>
        /// tabKagamiを手動で切り替えた時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabKagami_SelectedIndexChanged(object sender, EventArgs e)
        {
            //選択中タブのみ更新
            Kagami _k = this.SelectedKagami;
            switch (this.tabKagami.SelectedTab.Text)
            {
                case "モニタ":
                    //周期再描画と同じ処理
                    monViewUpdate(_k);
                    //個別帯域制限の場合、表示内容AUDIT
                    if (Front.BandStopTypeString[Front.BndWth.BandStopMode] == "ポート毎に個別設定")
                        bndStopNumAudit();
                    break;
                case "クライアント":
                    if (_k != null)
                    {
                        //新ポートのClientItemを一括設定
                        clientView.BeginUpdate();
                        clientView.Items.Clear();
                        _k.Status.Client.UpdateClientTime();
                        clientView.Items.AddRange(_k.Status.Gui.ClientItem.ToArray());
                        clientView.EndUpdate();
                    }
                    break;
                case "リザーブ":
                    if (_k != null)
                    {
                        reserveView.BeginUpdate();
                        reserveView.Items.Clear();
                        reserveView.Items.AddRange(_k.Status.Gui.ReserveItem.ToArray());
                        // 全登録IPの状態AUDIT
                        // 登録IPが被らない状態のリスト作成
                        List<string> _ip_list = new List<string>();
                        lock (_k.Status.Gui.ReserveItem)
                        {
                            foreach (ListViewItem _item in _k.Status.Gui.ReserveItem)
                            {
                                if (!_ip_list.Contains(_item.Text))    // clmResvViewIP
                                    _ip_list.Add(_item.Text);
                            }
                        }
                        foreach (string _ip in _ip_list)
                        {
                            AuditReserve(_k, _ip);
                        }
                        reserveView.EndUpdate();
                    }
                    break;
                case "キック":
                    if (_k != null)
                    {
                        kickView.BeginUpdate();
                        kickView.Items.Clear();
                        _k.Status.Client.UpdateKickTime();
                        kickView.Items.AddRange(_k.Status.Gui.KickItem.ToArray());
                        kickView.EndUpdate();
                    }
                    break;
                case "ログ":
                    if (_k != null)
                    {
                        //LogItemから一括設定
                        logView.BeginUpdate();
                        logView.Items.Clear();
                        if (Front.LogMode == 0)
                            logView.Items.AddRange(_k.Status.Gui.LogAllItem.ToArray());
                        else
                            logView.Items.AddRange(_k.Status.Gui.LogImpItem.ToArray());
                        // オートスクロール
                        if (logAutoScroll.Checked && logView.Items.Count > 0)
                            logView.EnsureVisible(logView.Items.Count - 1);
                        logView.EndUpdate();
                    }
                    break;
                default:
                    throw new Exception("not implemented.");
            }
        }

        #region モニタタブ

        #region monViewの処理
        /// <summary>
        /// monViewに初期アイテムを追加する
        /// </summary>
        private void monViewInit()
        {
            monView.Items.Add("IM状態");
            monView.Items.Add("IM接続時間");
            monView.Items.Add("EX接続数");
            monView.Items.Add("帯域制限");
            monView.Items.Add("UP帯域");
            monView.Items.Add("DOWN帯域");
            monView.Items.Add("UP転送量");
            monView.Items.Add("DOWN転送量");
            monView.Items.Add("ビジーカウンタ");
            monView.Items.Add("IM不調回数");
            monView.Items.Add("EX不調回数");
            monView.Items.Add("EX接続回数");
            monView.Items.Add("コメント");
            monView.Items.Add("IM設定者IP");
            monView.Items.Add("実況URL");
            monView.Items.Add("Redirect/子");
            monView.Items.Add("Redirect/親");
#if PLUS
            monView.Items.Add("Mode");
#endif
            for (int cnt = 0; cnt < monView.Items.Count; cnt++)
                monView.Items[cnt].SubItems.Add("");
            monViewUpdate(null);
        }
        /// <summary>
        /// 周期的に呼ばれて、
        /// monViewの内容を更新する
        /// </summary>
        private void monViewUpdate(Kagami _k)
        {
            monView.BeginUpdate();
            if (_k != null)
            {
                int idx = 0;

                //up,down転送量の計算
                string _ul, _dl;
                //if (_k.Status.TotalUPSize > 10 * 1000 * 1000)
                    _ul = _k.Status.TotalUPSize.ToString("#,##0,,") + " [Mbyte]";
                //else
                //    _ul = _k.Status.TotalUPSize.ToString("#,##0,") + " [kbyte]";
                //if (_k.Status.TotalDLSize > 10 * 1000 * 1000)
                    _dl = _k.Status.TotalDLSize.ToString("#,##0,,") + " [Mbyte]";
                //else
                //    _dl = _k.Status.TotalDLSize.ToString("#,##0,") + " [kbyte]";

                // IM状態
                monView.Items[idx].SubItems[1].Text = _k.Status.ImportURL != "待機中" ? (_k.Status.ImportStatus ? "正常" : "接続試行中...") : "待機中"; idx++;
                // IM接続時間
                monView.Items[idx].SubItems[1].Text = _k.Status.ImportStatus ? _k.Status.ImportTimeString : "-"; idx++;
                // EX接続数
                monView.Items[idx].SubItems[1].Text = _k.Status.Client.Count + "/" + _k.Status.Connection + "+" + _k.Status.Reserve; idx++;
                // 帯域制限
                monView.Items[idx].SubItems[1].Text = Front.BndWth.EnableBandWidth ? "開始中: " + _k.Status.LimitUPSpeed + " [kbps]" : "停止中"; idx++;
                // UP帯域
                monView.Items[idx].SubItems[1].Text = Unit == 0 ? (_k.Status.CurrentDLSpeed * _k.Status.Client.Count).ToString("#,##0") + " [kbps]" : (_k.Status.CurrentDLSpeed/8 * _k.Status.Client.Count).ToString("#,##0") + " [KB/s]"; idx++;
                // DOWN帯域
                monView.Items[idx].SubItems[1].Text = Unit == 0 ? _k.Status.CurrentDLSpeed.ToString("#,##0") + " [kbps]" : (_k.Status.CurrentDLSpeed/8).ToString("#,##0") + " [KB/s]"; idx++;
                // UP転送量
                monView.Items[idx].SubItems[1].Text = _ul; idx++;
                // DOWN転送量
                monView.Items[idx].SubItems[1].Text = _dl; idx++;
                // ビジーカウンタ
                monView.Items[idx].SubItems[1].Text = _k.Status.BusyCounter.ToString(); idx++;
                // IM不調回数
                monView.Items[idx].SubItems[1].Text = _k.Status.ImportError.ToString(); idx++;
                // EX不調回数
                monView.Items[idx].SubItems[1].Text = _k.Status.ExportError.ToString(); idx++;
                // EX接続回数
                monView.Items[idx].SubItems[1].Text = _k.Status.ExportCount.ToString(); idx++;
                // コメント
                monView.Items[idx].SubItems[1].Text = _k.Status.Comment; idx++;
                // 接続設定者IP
                monView.Items[idx].SubItems[1].Text = _k.Status.SetUserIP; idx++;
                // 実況URL
                monView.Items[idx].SubItems[1].Text = _k.Status.Url; idx++;
                // Redirect/子
                monView.Items[idx].SubItems[1].Text = _k.Status.EnableRedirectChild == true ? "有効" : "無効"; idx++;
                // Redirect/親
                monView.Items[idx].SubItems[1].Text = _k.Status.EnableRedirectParent == true ? "有効" : "無効"; idx++;
#if PLUS
                // Mode
                monView.Items[idx].SubItems[1].Text = _k.Status.DisablePull ? "Pull無効/Push有効" : Front.Opt.EnablePush ? "Pull有効/Push有効" : "Pull有効/Push無効"; idx++;
#endif
            }
            else
            {
                int idx = 0;
                // IM状態
                monView.Items[idx].SubItems[1].Text = "未接続"; idx++;
                // IM接続時間
                monView.Items[idx].SubItems[1].Text = "-"; idx++;
                // EX接続数
                monView.Items[idx].SubItems[1].Text = "-"; idx++;
                // 帯域制限
                monView.Items[idx].SubItems[1].Text = (Front.BndWth.EnableBandWidth ? "開始中" : "停止中"); idx++;
                // UP帯域
                monView.Items[idx].SubItems[1].Text = Unit == 0 ? "0 [kbps]" : "0 [KB/s]"; idx++;
                // DOWN帯域
                monView.Items[idx].SubItems[1].Text = Unit == 0 ? "0 [kbps]" : "0 [KB/s]"; idx++;
                // UP転送量
                monView.Items[idx].SubItems[1].Text = "0 [kbyte]"; idx++;
                // DOWN転送量
                monView.Items[idx].SubItems[1].Text = "0 [kbyte]"; idx++;
                // ビジーカウンタ
                monView.Items[idx].SubItems[1].Text = "0"; idx++;
                // IM不調回数
                monView.Items[idx].SubItems[1].Text = "0"; idx++;
                // EX不調回数
                monView.Items[idx].SubItems[1].Text = "0"; idx++;
                // EX接続回数
                monView.Items[idx].SubItems[1].Text = "0"; idx++;
                // コメント
                monView.Items[idx].SubItems[1].Text = ""; idx++;
                // 接続設定者IP
                monView.Items[idx].SubItems[1].Text = ""; idx++;
                // 実況URL
                monView.Items[idx].SubItems[1].Text = ""; idx++;
                // Redirect/子
                monView.Items[idx].SubItems[1].Text = "-"; idx++;
                // Redirect/親
                monView.Items[idx].SubItems[1].Text = "-"; idx++;
#if PLUS
                // Mode
                monView.Items[idx].SubItems[1].Text = Front.Opt.EnablePush ? "Push有効" : "Push無効"; idx++;
#endif
            }
            monView.EndUpdate();
        }
        /// <summary>
        /// monViewのダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monView_DoubleClick(object sender, EventArgs e)
        {
            ListView _monView = (ListView)sender;
            Kagami _k = this.SelectedKagami;
            switch (_monView.FocusedItem.Text)
            {
                case "UP帯域":
                case "DOWN帯域":
                    // kbps⇔KB/s切替
                    this.Unit = (uint)(this.Unit == 0 ? 1 : 0);
                    break;
                case "Redirect/子":
                    if (_k != null)
                    {
                        _k.Status.EnableRedirectChild = !_k.Status.EnableRedirectChild;
                        MonRedirCMenu.Checked = _k.Status.EnableRedirectChild;
                    }
                    break;
                case "Redirect/親":
                    if (_k != null)
                    {
                        _k.Status.EnableRedirectParent = !_k.Status.EnableRedirectParent;
                        MonRedirPMenu.Checked = _k.Status.EnableRedirectParent;
                    }
                    break;
                case "Mode":
                    if (_k != null)
                    {
                        if (Front.Opt.EnablePush)
                        {
                            _k.Status.DisablePull = !_k.Status.DisablePull;
                        }
                    }
                    break;
                default:
                    return;
            }
            // 即更新
            monAllViewUpdate();
            monViewUpdate(this.SelectedKagami);
        }

        /// <summary>
        /// モニタタブで右クリックメニューを開こうとした時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonViewRClick_Opening(object sender, CancelEventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                MonRedirCMenu.Checked = false;
                MonRedirPMenu.Checked = false;
                MonImIpCopyMenu.Enabled = false;
                MonRedirCMenu.Enabled = false;
                MonRedirPMenu.Enabled = false;
                MonModeChgMenu.Enabled = false;
            }
            else
            {
                MonImIpCopyMenu.Enabled = false;
                foreach (ListViewItem _item in monView.Items)
                {
                    if (_item.Text == "IM設定者IP")
                    {
                        if (_item.SubItems[1].Text != "")
                        {
                            MonImIpCopyMenu.Enabled = true;
                            break;
                        }
                    }
                }
                MonRedirCMenu.Enabled = true;
                MonRedirPMenu.Enabled = true;
                MonRedirCMenu.Checked = _k.Status.EnableRedirectChild;
                MonRedirPMenu.Checked = _k.Status.EnableRedirectParent;
                MonModeChgMenu.Enabled = Front.Opt.EnablePush;
            }
        }

        /// <summary>
        /// kbps⇔KB/s切り替えメニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonUnitChgMenu_Click(object sender, EventArgs e)
        {
            // kbps⇔KB/s切替
            this.Unit = (uint)(this.Unit == 0 ? 1 : 0);
            // 即更新
            monAllViewUpdate();
            monViewUpdate(this.SelectedKagami);
        }

        /// <summary>
        /// IM設定者IPコピーメニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonImIpCopyMenu_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem _item in monView.Items)
            {
                if (_item.Text == "IM設定者IP")
                {
                    if (_item.SubItems[1].Text != "")
                    {
                        Clipboard.SetText(_item.SubItems[1].Text);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Redirect/子メニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonRedirCMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.EnableRedirectChild = !_k.Status.EnableRedirectChild;
                MonRedirCMenu.Checked = _k.Status.EnableRedirectChild;
            }
            // 即更新
            monViewUpdate(this.SelectedKagami);
        }

        /// <summary>
        /// Redirect/親メニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonRedirPMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.EnableRedirectParent = !_k.Status.EnableRedirectParent;
                MonRedirPMenu.Checked = _k.Status.EnableRedirectParent;
            }
            // 即更新
            monViewUpdate(this.SelectedKagami);
        }

        /// <summary>
        /// Mode切り替えメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonModeChgMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.DisablePull = !_k.Status.DisablePull;
            }
            // 即更新
            monViewUpdate(this.SelectedKagami);
        }
        #endregion

        #region 帯域制限ダイアログの処理
        /// <summary>
        /// 帯域制限値変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bndStopNum_ValueChanged(object sender, EventArgs e)
        {
            if (Front.BandStopTypeString[Front.BndWth.BandStopMode] != "ポート毎に個別設定")
            {
                Front.BndWth.BandStopValue = (uint)bndStopNum.Value;
            }
            else
            {
                Kagami _k = this.SelectedKagami;
                if (_k != null)
                {
                    _k.Status.GUILimitUPSpeed = (int)bndStopNum.Value;
                    _k.Status.LimitUPSpeed = Front.CnvLimit((int)bndStopNum.Value, (int)Front.BndWth.BandStopUnit);
                }
            }
        }

        /// <summary>
        /// 個別帯域制限値変更（キー入力）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bndStopNum_KeyUp(object sender, KeyEventArgs e)
        {
            bndStopNum_ValueChanged(sender, (EventArgs)e);
        }

        /// <summary>
        /// 設定変更時・ポート切替時などに
        /// モニタタブ上の帯域制限値を再設定する
        /// (帯域制限方式:ポート毎個別設定用)
        /// </summary>
        private void bndStopNumAudit()
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                // デフォルト値
                bndStopNum.Value = Front.BndWth.BandStopValue;
            }
            else
            {
                switch (Front.BandStopUnitString[Front.BndWth.BandStopUnit])
                {
                    case "KB/s":
                        bndStopNum.Value = _k.Status.LimitUPSpeed / 8;
                        break;
                    case "MB/s":
                        bndStopNum.Value = _k.Status.LimitUPSpeed / 8000;
                        break;
                    case "Kbps":
                        bndStopNum.Value = _k.Status.LimitUPSpeed;
                        break;
                    case "Mbps":
                        bndStopNum.Value = _k.Status.LimitUPSpeed / 1000;
                        break;
                    default:
                        throw new Exception("not implemented.");
                }
            }
        }
        #endregion

        #endregion

        #region クライアントタブ
        /// <summary>
        /// クライアントの接続・切断時に起動される
        /// clientView管理スレッド以外から呼ばれたら、スレッド間通信する
        /// 通知元ポートとGUI上の選択ポートが一致すれば、
        /// clientViewへのアイテム追加・削除処理と、reserveViewの状態更新を行う
        /// </summary>
        /// <param name="_ke"></param>
        void clientViewUpdate(KagamiEvent _ke)
        {
            if (clientView.InvokeRequired)
            {
                if (clientView.IsHandleCreated)
                {
                    //コントロール管理元スレッド以外はこっち
                    //コントロール管理元スレッドに非同期Invoke通信で更新要求
                    EventHandlerDelegate _dlg = new EventHandlerDelegate(clientViewUpdate);
                    this.BeginInvoke(_dlg, new object[] { _ke });
                    //this.Invoke(_dlg, new object[] { _ke });
                }
            }
            else
            {
                // コントロール管理元スレッドはこっち
                Kagami _k = this.SelectedKagami;
                if (_k != null && _k == _ke.Kagami && !clientView.IsDisposed)
                {
                    if (_ke.Mode == 0)
                    {
                        // クライアント接続通知
                        // 通知されたアイテムをclientViewに追加
                        // 選択ポート変更による二重登録エラーを防ぐため
                        // 登録前に確認しておく
                        if (!clientView.Items.Contains(_ke.Item))
                            clientView.Items.Add(_ke.Item);
                    }
                    else
                    {
                        // クライアント切断通知
                        // 通知されたアイテムをclientViewから削除
                        // 念のため、登録済みであることを確認しておく
                        if (clientView.Items.Contains(_ke.Item))
                            clientView.Items.Remove(_ke.Item);
                    }
                    // reserveView状態更新
                    if (Front.clmCV_IP_IDX == 0)
                        AuditReserve(_k, _ke.Item.Text);
                    else
                        AuditReserve(_k, _ke.Item.SubItems[Front.clmCV_IP_IDX].Text);
                }
            }
        }
        /// <summary>
        /// クライアントタブで右クリックメニューを開こうとした時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // 選択中アイテムがあるかチェックして、無ければDisable
            // ただしClientResolveHostMenuは常にEnable
            for (int cnt = 0; cnt < clientView.Items.Count; cnt++)
            {
                if (clientView.Items[cnt].Selected)
                {
                    //ClientViewRClick.Enabled = true;
                    ClientDiscMenu.Enabled = true;
                    ClientKickDefMenu.Enabled = true;
                    ClientKickSubMenu.Enabled = true;
                    ClientAddResvMenu.Enabled = true;
                    ClientIPCopyMenu.Enabled = true;
                    ClientResolveHostMenu.Enabled = true;
                    return;
                }
            }
            //ClientViewRClick.Enabled = false;
            ClientDiscMenu.Enabled = false;
            ClientKickDefMenu.Enabled = false;
            ClientKickSubMenu.Enabled = false;
            ClientAddResvMenu.Enabled = false;
            ClientIPCopyMenu.Enabled = false;
            ClientResolveHostMenu.Enabled = true;
            return;
        }

        /// <summary>
        /// クライアント切断メニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientDiscMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;

            try
            {
                //後ろから切ってく
                for (int cnt = clientView.Items.Count - 1; cnt >= 0; cnt--)
                {
                    if (clientView.Items[cnt].Selected)
                    {
                        //強制切断
                        if (Front.clmCV_ID_IDX == 0)
                            _k.Status.Client.Disc(clientView.Items[cnt].Text);
                        else
                            _k.Status.Client.Disc(clientView.Items[cnt].SubItems[Front.clmCV_ID_IDX].Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("ClientDiscMenu", "内部エラー Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// クライアントタブ上でキックメニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientKickMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;
            try
            {
                // Tagプロパティにキック時間が設定されている
                int _deny_tim = int.Parse((string)(((ToolStripMenuItem)sender).Tag));
                // 標準キックはTag情報に０が設定されているので、ユーザ設定値に置き換える
                if (_deny_tim == 0)
                    _deny_tim = (int)Front.Kick.KickDenyTime;

                //後ろから蹴ってく
                for (int cnt = clientView.Items.Count - 1; cnt >= 0; cnt--)
                {
                    if (clientView.Items[cnt].Selected == false)
                        continue;

                    //内部管理KickListへの登録
                    if (Front.clmCV_IP_IDX == 0)
                        Front.AddKickUser(clientView.Items[cnt].Text, _deny_tim);
                    else
                        Front.AddKickUser(clientView.Items[cnt].SubItems[Front.clmCV_IP_IDX].Text, _deny_tim);

                    //GUI表示用KickItemに未登録なら登録する
                    lock (_k.Status.Gui.KickItem)
                    {
                        int cnt2;
                        for (cnt2 = 0; cnt2 < _k.Status.Gui.KickItem.Count; cnt2++)
                        {
                            if (clmKickViewIP.DisplayIndex == 0)
                            {
                                if (_k.Status.Gui.KickItem[cnt2].Text == clientView.Items[cnt].Text)
                                    break;
                            }
                            else
                            {
                                if (_k.Status.Gui.KickItem[cnt2].SubItems[clmKickViewIP.DisplayIndex].Text == clientView.Items[cnt].SubItems[Front.clmCV_IP_IDX].Text)
                                    break;
                            }
                        }
                        if (cnt2 >= _k.Status.Gui.KickItem.Count)
                        {
                            if (Front.clmCV_IP_IDX == 0)
                                _k.Status.AddKick(clientView.Items[cnt].Text, 0);
                            else
                                _k.Status.AddKick(clientView.Items[cnt].SubItems[Front.clmCV_IP_IDX].Text, 0);
                        }
                    }
                    //接続中なら強制切断
                    if (clientView.Items[cnt].Selected)
                    {
                        if (Front.clmCV_ID_IDX == 0)
                            _k.Status.Client.Disc(clientView.Items[cnt].Text);
                        else
                            _k.Status.Client.Disc(clientView.Items[cnt].SubItems[Front.clmCV_ID_IDX].Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("ClientKickMenu", "内部エラー Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// リザーブ登録メニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientAddResvMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;

            try
            {
                //選択中の全クライアントをリザーブ登録
                lock(_k.Status.Gui.ClientItem)
                    foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                        if (_item.Selected)
                            if (Front.clmCV_IP_IDX == 0)
                            {
                                _k.Status.AddReserve(_item.Text);
                                AuditReserve(_k, _item.Text);
                            }
                            else
                            {
                                _k.Status.AddReserve(_item.SubItems[Front.clmCV_IP_IDX].Text);
                                AuditReserve(_k, _item.SubItems[Front.clmCV_IP_IDX].Text);
                            }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("ClientAddResvMenu", "内部エラー Trace:" + ex.StackTrace);
            }
        }
        /// <summary>
        /// IPアドレスコピーメニューのクリック
        /// クライアント/キック/リザーブ共通処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IPCopyMenu_Click(object sender, EventArgs e)
        {
            string copy_string = "";
            ToolStripMenuItem _tsmi = (ToolStripMenuItem)sender;
            switch (_tsmi.Name)
            {
                case "ClientIPCopyMenu":
                    if (Front.clmCV_IP_IDX == 0)
                    {
                        foreach (ListViewItem _item in clientView.Items)
                            if (_item.Selected)
                                copy_string += _item.Text + Environment.NewLine;
                    }
                    else
                    {
                        foreach (ListViewItem _item in clientView.Items)
                            if (_item.Selected)
                                copy_string += _item.SubItems[Front.clmCV_IP_IDX].Text + Environment.NewLine;
                    }
                    break;
                case "KickIPCopyMenu":
                    if (clmKickViewIP.DisplayIndex == 0)
                    {
                        foreach (ListViewItem _item in kickView.Items)
                            if (_item.Selected)
                                copy_string += _item.Text + Environment.NewLine;
                    }
                    else
                    {
                        foreach (ListViewItem _item in kickView.Items)
                            if (_item.Selected)
                                copy_string += _item.SubItems[clmKickViewIP.DisplayIndex].Text + Environment.NewLine;
                    }
                    break;
                case "ResvIPCopyMenu":
                    if (clmResvViewIP.DisplayIndex == 0)
                    {
                        foreach (ListViewItem _item in reserveView.Items)
                            if (_item.Selected)
                                copy_string += _item.Text + Environment.NewLine;
                    }
                    else
                    {
                        foreach (ListViewItem _item in reserveView.Items)
                            if (_item.Selected)
                                copy_string += _item.SubItems[clmResvViewIP.DisplayIndex].Text + Environment.NewLine;
                    }
                    break;
                default:
                    throw new Exception("not implemented.");
            }
            if (copy_string.Length > Environment.NewLine.Length)
                copy_string = copy_string.Remove(copy_string.Length - Environment.NewLine.Length); // 余計な末尾の改行消し
            Clipboard.SetText(copy_string);
        }
        /// <summary>
        /// ドメイン解決有効/無効切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientResolveHostMenu_Click(object sender, EventArgs e)
        {
            Front.Opt.EnableResolveHost = ClientResolveHostMenu.Checked;
            int _target = Front.Opt.EnableResolveHost ? Front.clmCV_HO_IDX : Front.clmCV_IP_IDX;
            // 全鏡のクライアントリスト上のFront.clmCV_IH_IDXカラム内の値を
            // _targetカラムの値に書き換える
            lock (Front.KagamiList)
                foreach (Kagami _k in Front.KagamiList)
                    lock (_k.Status.Gui.ClientItem)
                        if (Front.clmCV_IH_IDX == 0)
                            // clmCV_IH_IDX==0なら_targe==0はありえない
                            foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                                _item.Text = _item.SubItems[_target].Text;
                        else if (_target == 0)
                            // _targe==0ならclmCV_IH_IDX==0はありえない
                            foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                                _item.SubItems[Front.clmCV_IH_IDX].Text = _item.Text;
                        else
                            foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                                _item.SubItems[Front.clmCV_IH_IDX].Text = _item.SubItems[_target].Text;
        }
        #endregion

        #region リザーブタブ
        /// <summary>
        /// リザーブ登録状態が変化したら起動される
        /// </summary>
        /// <param name="_ke"></param>
        void reserveViewUpdate(KagamiEvent _ke)
        {
            if (reserveView.InvokeRequired)
            {
                if (reserveView.IsHandleCreated)
                {
                    //コントロール管理元スレッド以外はこっち
                    //コントロール管理元スレッドに非同期Invoke通信で更新要求
                    EventHandlerDelegate _dlg = new EventHandlerDelegate(reserveViewUpdate);
                    this.BeginInvoke(_dlg, new object[] { _ke });
                    //this.Invoke(_dlg, new object[] { _ke });
                }
            }
            else
            {
                // コントロール管理元スレッドはこっち
                // GUI上の選択ポートと通知元ポートが一緒ならreserveViewのアイテム追加・削除
                Kagami _k = this.SelectedKagami;
                if (_k != null && _k == _ke.Kagami && !reserveView.IsDisposed)
                {
                    switch (_ke.Mode)
                    {
                        case 0: // 追加
                            // reserveViewにItemを追加
                            if (!reserveView.Items.Contains(_ke.Item))
                                reserveView.Items.Add(_ke.Item);
                            break;
                        case 1: // 削除
                            // reserveViewからItemを削除
                            if (reserveView.Items.Contains(_ke.Item))
                                reserveView.Items.Remove(_ke.Item);
                            break;
                        case 2: // 全削除
                            reserveView.Items.Clear();
                            break;
                        default:
                            break;
                    }
                    // リザーブ状態AUDIT
                    if (_ke.Item != null)   // 全削除の時はnullがありうる
                    {
                        if (clmResvViewIP.DisplayIndex == 0)
                            AuditReserve(_k, _ke.Item.Text);
                        else
                            AuditReserve(_k, _ke.Item.SubItems[clmResvViewIP.DisplayIndex].Text);
                    }
                }
            }
        }

        /// <summary>
        /// リザーブ追加ボタンのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addResvBTN_Click(object sender, EventArgs e)
        {
            if (addResvHost.Text == "")
                return;

            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                MessageBox.Show("ポートを起動してから追加してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                //リザーブ追加
                System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(addResvHost.Text)[0];
                _k.Status.AddReserve(hostadd.ToString());
                //表示更新
                addResvHost.Text = "";
                //リザーブ状態AUDIT
                AuditReserve(_k, hostadd.ToString());
            }
            catch
            {
                MessageBox.Show("ホスト名からIPアドレスに変換できませんでした", "DNS Error", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// リザーブ登録ホスト入力欄でキー入力した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addResvHost_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter入力なら追加処理を行う
            if (e.KeyChar == (char)Keys.Enter)
            {
                addResvBTN_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        /// <summary>
        /// リザーブタブで右クリックメニューを開こうとした時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReserveViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // 選択中アイテムがあるかチェックして、無ければDisable
            for (int cnt = 0; cnt < reserveView.Items.Count; cnt++)
            {
                if (reserveView.Items[cnt].Selected)
                {
                    //ReserveViewRClick.Enabled = true;
                    ResvDelMenu.Enabled = true;
                    ResvIPCopyMenu.Enabled = true;
                    return;
                }
            }
            //ReserveViewRClick.Enabled = false;
            ResvDelMenu.Enabled = false;
            ResvIPCopyMenu.Enabled = false;
            return;
        }

        /// <summary>
        /// リザーブ登録削除メニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResvDelMenu_Click(object sender, EventArgs e)
        {
            //後ろから順にReserveItemを削除
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                lock (_k.Status.Gui.ReserveItem)
                {
                    for (int cnt = reserveView.Items.Count - 1; cnt >= 0; cnt--)
                    {
                        if (reserveView.Items[cnt].Selected)
                        {
                            ListViewItem _item = reserveView.Items[cnt];
                            // ReserveItemから削除
                            if (_k.Status.Gui.ReserveItem.Contains(_item))
                            {
                                _k.Status.Gui.ReserveItem.Remove(_item);
                                if (clmResvViewIP.DisplayIndex == 0)
                                    AuditReserve(_k, _item.Text);
                                else
                                    AuditReserve(_k, _item.SubItems[clmResvViewIP.DisplayIndex].Text);
                            }
                        }
                    }
                }
            }
            //GUI上のリストをReserveItemにあわせる
            myPort_StateChanged();
        }

        /// <summary>
        /// 対象IPのリザーブ登録状態を現在の接続数にあわせる
        /// </summary>
        /// <param name="_k"></param>
        /// <param name="_ip"></param>
        public void AuditReserve(Kagami _k, string _ip)
        {
            if (_k == null)
                return;

            //ClientViewの更新も行うのでReserve登録0でも続行
            //if (_k.Status.Gui.ReserveItem.Count == 0)
            //    return;

            int r_cnt = 0; // リザーブリスト上での該当IP数
            int c_cnt = 0; // クライアントリスト上での該当IP数

            lock (_k.Status.Gui.ReserveItem)
            lock (_k.Status.Gui.ClientItem)
            {
                // 対象IPがリザーブリストに何個存在するかチェック
                if (clmResvViewIP.DisplayIndex == 0)
                {
                    foreach (ListViewItem _item in _k.Status.Gui.ReserveItem)
                        if (_item.Text == _ip)
                            r_cnt++;
                }
                else
                {
                    foreach (ListViewItem _item in _k.Status.Gui.ReserveItem)
                        if (_item.SubItems[clmResvViewIP.DisplayIndex].Text == _ip)
                            r_cnt++;
                }
                

                // 対象IPがクライアントリストに何個存在するかチェック
                // r_cntの値分、見つかった_itemを赤文字にする
                if (Front.clmCV_IP_IDX == 0)
                {
                    foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                        if (_item.Text == _ip)
                        {
                            c_cnt++;
                            if (r_cnt > 0)
                            {
                                r_cnt--;
                                _item.SubItems[0].ForeColor = System.Drawing.Color.LimeGreen;
                            }
                            else
                            {
                                _item.SubItems[0].ResetStyle();
                            }
                        }
                }
                else
                {
                    foreach (ListViewItem _item in _k.Status.Gui.ClientItem)
                        if (_item.SubItems[Front.clmCV_IP_IDX].Text == _ip)
                        {
                            c_cnt++;
                            if (r_cnt > 0)
                            {
                                r_cnt--;
                                _item.SubItems[0].ForeColor = System.Drawing.Color.LimeGreen;
                            }
                            else
                            {
                                _item.SubItems[0].ResetStyle();
                            }
                        }
                }
                

                // 対象IPの接続数分○に設定し、
                // それ以降は全て×にする
                foreach (ListViewItem _item in _k.Status.Gui.ReserveItem)
                {
                    if (_item.Text == _ip)  // clmResvViewIP
                    {
                        if (c_cnt > 0)
                        {
                            _item.SubItems[1].Text = "○";  // clmResvViewStatus
                            _item.SubItems[0].ForeColor = System.Drawing.Color.LimeGreen;
                            c_cnt--;
                        }
                        else
                        {
                            _item.SubItems[1].Text = "×";  // clmResvViewStatus
                            _item.SubItems[0].ForeColor = System.Drawing.Color.Red;
                        }
                    }
                }
            }
            // ついでにリザーブ登録有りでKick対象に登録されてたら解除する
            if (c_cnt > 0)
                lock (Front.KickList)
                    if (Front.KickList.ContainsKey(_ip))
                        Front.KickList[_ip] = DateTime.Now.AddSeconds(Front.Kick.KickDenyTime).ToString() + ",1";
        }

        #endregion

        #region キックタブ
        /// <summary>
        /// キックユーザが追加された時に呼ばれる
        /// 今開いてるタブがキックタブならリストに追加
        /// kickView管理スレッド以外から呼ばれたら、スレッド間通信する
        /// </summary>
        /// <param name="_ke"></param>
        void kickViewUpdate(KagamiEvent _ke)
        {
            if (kickView.InvokeRequired)
            {
                if (kickView.IsHandleCreated)
                {
                    //コントロール管理元スレッド以外はこっち
                    //コントロール管理元スレッドに非同期Invoke通信で更新要求
                    EventHandlerDelegate _dlg = new EventHandlerDelegate(kickViewUpdate);
                    this.BeginInvoke(_dlg, new object[] { _ke });
                    //this.Invoke(_dlg, new object[] { _ke });
                }
            }
            else
            {
                // コントロール管理元スレッドはこっち
                // GUI上選択中のポートが、イベント発生ポートと同じかチェック
                // GUI上選択中のタブが、キックタブかチェック
                Kagami _k = this.SelectedKagami;
                if (_k != null && _k == _ke.Kagami)
                {
                    // kickViewに未登録のアイテムで通知してきたら新規登録
                    // 登録済みアイテムの通知ならカウントを＋１インクリメントする
                    if (!kickView.Items.Contains(_ke.Item))
                    {
                        //新規登録
                        if (!kickView.IsDisposed)
                            kickView.Items.Add(_ke.Item);
                    }
                    else
                    {
                        //カウンタインクリメント
                        if (clmKickViewCount.DisplayIndex == 0)
                            _ke.Item.Text = (int.Parse(_ke.Item.Text) + 1).ToString();
                        else
                            _ke.Item.SubItems[clmKickViewCount.DisplayIndex].Text
                                = (int.Parse(_ke.Item.SubItems[clmKickViewCount.DisplayIndex].Text) + 1).ToString();
                    }
                }
            }
        }
        /// <summary>
        /// キック追加ボタンのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addKickBTN_Click(object sender, EventArgs e)
        {
            if (addKickHost.Text == "")
                return;

            Kagami _k = this.SelectedKagami;
            if (_k == null)
            {
                MessageBox.Show("ポートを起動してから追加してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // リストに登録するだけで、規制は開始しない。
            try
            {
                //キック追加
                System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(addKickHost.Text)[0];
                _k.Status.AddKick(hostadd.ToString(),0);
                //表示更新
                addKickHost.Text = "";
                myPort_StateChanged();
            }
            catch
            {
                MessageBox.Show("ホスト名からIPアドレスに変換できませんでした", "DNS Error", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// キック登録ホスト入力欄でキー入力した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addKickHost_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter入力なら追加処理を行う
            if (e.KeyChar == (char)Keys.Enter)
            {
                addKickBTN_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        /// <summary>
        /// キックタブ上で右クリックメニューを開こうとした時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KickViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // 選択中アイテムがあるかチェックして、無ければDisable
            // ただしすべて解除、すべて解除＆リスト消去はそのまま。
            for (int cnt = 0; cnt < kickView.Items.Count; cnt++)
            {
                if (kickView.Items[cnt].Selected)
                {
                    KickAddSubMenu.Enabled = true;
                    KickSelDelMenu.Enabled = true;
                    KickSelDelClearMenu.Enabled = true;
                    KickIPCopyMenu.Enabled = true;
                    return;
                }
            }
            KickAddSubMenu.Enabled = false;
            KickSelDelMenu.Enabled = false;
            KickSelDelClearMenu.Enabled = false;
            KickIPCopyMenu.Enabled = false;
            return;
        }

        /// <summary>
        /// キック追加メニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KickAddMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;

            try
            {
                // Tagプロパティにキック時間が設定されている
                int _deny_tim = int.Parse((string)(((ToolStripMenuItem)sender).Tag));
                // 後ろから順に追加
                for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                {
                    if (kickView.Items[cnt].Selected == true)
                    {
                        if (clmKickViewIP.DisplayIndex == 0)
                            Front.AddKickUser(kickView.Items[cnt].Text, _deny_tim);
                        else
                            Front.AddKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.DisplayIndex].Text, _deny_tim);
                    }
                }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("KickAddMenu", "内部エラー Trace:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// キック解除メニューのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KickDelMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k == null)
                return;

            try
            {
                switch (((ToolStripMenuItem)sender).Name)
                {
                    case "KickSelDelMenu":  // 選択中のKick解除
                        for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                            if (kickView.Items[cnt].Selected == true)
                                Front.DelKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.Index].Text);
                        break;

                    case "KickAllDelMenu":  // すべてのKick解除
                        for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                            Front.DelKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.Index].Text);
                        break;

                    case "KickSelDelClearMenu": // 選択中のKick解除＋クリア
                        for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                        {
                            if (kickView.Items[cnt].Selected == true)
                            {
                                if (clmKickViewIP.DisplayIndex == 0)
                                    Front.DelKickUser(kickView.Items[cnt].Text);
                                else
                                    Front.DelKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.DisplayIndex].Text);
                                lock (_k.Status.Gui.KickItem)
                                {
                                    for (int cnt2 = _k.Status.Gui.KickItem.Count - 1; cnt2 >= 0; cnt2--)
                                    {
                                        if (clmKickViewIP.DisplayIndex == 0)
                                        {
                                            if (_k.Status.Gui.KickItem[cnt2].Text == kickView.Items[cnt].Text)
                                                _k.Status.Gui.KickItem.RemoveAt(cnt2);
                                        }
                                        else
                                        {
                                            if (_k.Status.Gui.KickItem[cnt2].SubItems[clmKickViewIP.DisplayIndex].Text == kickView.Items[cnt].SubItems[clmKickViewIP.DisplayIndex].Text)
                                                _k.Status.Gui.KickItem.RemoveAt(cnt2);
                                        }
                                    }
                                }
                                kickView.Items[cnt].Remove();
                            }
                        }
                        break;

                    case "KickAllDelClearMenu": // すべてのKick解除＋クリア
                        for (int cnt = kickView.Items.Count - 1; cnt >= 0; cnt--)
                        {
                            if (clmKickViewIP.DisplayIndex == 0)
                                Front.DelKickUser(kickView.Items[cnt].Text);
                            else
                                Front.DelKickUser(kickView.Items[cnt].SubItems[clmKickViewIP.DisplayIndex].Text);
                        }
                        _k.Status.Gui.KickItem.Clear();
                        kickView.Items.Clear();
                        break;
                    default:
                        throw new Exception("not implemented.");
                }
            }
            catch (Exception ex)
            {
                Front.AddLogDebug("KickDelMenu", "内部エラー Trace:" + ex.StackTrace);
            }
        }
        #endregion

        #region ログタブ
        /// <summary>
        /// logViewアイテム追加
        /// logView管理スレッド以外から呼ばれたら、スレッド間通信する
        /// </summary>
        /// <param name="_ke"></param>
        void logViewUpdate(KagamiEvent _ke)
        {
            if (logView.InvokeRequired)
            {
                //コントロール管理元スレッド以外はこっち
                //コントロール管理元スレッドに非同期Invoke通信で更新要求
                if (logView.IsHandleCreated)
                {
                    try
                    {
                        EventHandlerDelegate _dlg = new EventHandlerDelegate(logViewUpdate);
                        this.BeginInvoke(_dlg, new object[] { _ke });
                        //this.Invoke(_dlg, new object[] { _ke });
                    }
                    catch { }
                }
            }
            else
            {
                // コントロール管理元スレッドはこっち
                // GUI上選択中のポートが、イベント発生ポートと同じかチェック
                // GUI上選択中のタブが、ログタブかチェック
                Kagami _k = this.SelectedKagami;
                if (_k == _ke.Kagami || _k == null)
                {
                    // ログ表示モード判定
                    // とりあえず２パターンしかないのでコレで。。
                    if (_ke.Mode >= Front.LogMode)
                    {
                        // ログを追加
                        try
                        {
                            if (!logView.IsDisposed)
                                logView.Items.Add(_ke.Item);
                        }
                        catch { }   // 二重登録NG系は無視
                        // 自動スクロール
                        if (logView.Items.Count != 0 && this.logAutoScroll.Checked)
                            logView.EnsureVisible(logView.Items.Count - 1);
                    }
                }
            }
        }
        /// <summary>
        /// ログ出力モード変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logMode_CheckedChanged(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;

            if (logModeAll.Checked)
                Front.LogMode = 0;
            else
                Front.LogMode = 1;
            // ログ内容の表示切替
            if (_k != null)
            {
                logView.BeginUpdate();
                logView.Items.Clear();
                if (logModeAll.Checked)
                    logView.Items.AddRange(_k.Status.Gui.LogAllItem.ToArray());
                else
                    logView.Items.AddRange(_k.Status.Gui.LogImpItem.ToArray());
                // 自動スクロール
                if (logView.Items.Count != 0 && this.logAutoScroll.Checked)
                    logView.EnsureVisible(logView.Items.Count - 1);
                logView.EndUpdate();
            }
        }
        /// <summary>
        /// ログタブで右クリックメニューを開こうとしている時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogViewRClick_Opening(object sender, CancelEventArgs e)
        {
            // 特に何もせずメニューを表示
        }
        /// <summary>
        /// 右クリックメニューで「このポートのログクリア」を選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogClearMenu_Click(object sender, EventArgs e)
        {
            Kagami _k = this.SelectedKagami;
            if (_k != null)
            {
                _k.Status.Gui.LogAllItem.Clear();
                _k.Status.Gui.LogImpItem.Clear();
                logView.Items.Clear();
            }
        }
        /// <summary>
        /// 右クリックメニューで「全ポートのログクリア」を選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogClearAllMenu_Click(object sender, EventArgs e)
        {
            foreach (Kagami _k in Front.KagamiList)
            {
                lock (_k.Status.Gui)
                {
                    _k.Status.Gui.LogAllItem.Clear();
                    _k.Status.Gui.LogImpItem.Clear();
                }
            }
            logView.Items.Clear();
        }

        #endregion

        #region スケジュール起動＋周期再描画＋自動終了監視
        /// <summary>
        /// 周期再描画＋自動終了監視＋スケジュール起動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            int _tmp_port_num = 0;
            Kagami _k;

            #region 左パネルの更新
            // kagamiView更新/上部ボタンAUDIT/ステータスバーAUDITはLeftFlagがONの時のみ行う
            if (LeftFlag)
            {
                LeftFlag = false;
                #region kagamiViewの更新
                kagamiView.BeginUpdate();
                kagamiView.Items.Clear();
                lock (Front.KagamiList)
                {
                    foreach (Kagami _k_tmp in Front.KagamiList)
                    {
                        // ImportURLを再設定
                        if (clmKgmViewImport.DisplayIndex == 0)
                            _k_tmp.Status.Gui.KagamiItem.Text = _k_tmp.Status.ImportURL;
                        else
                            _k_tmp.Status.Gui.KagamiItem.SubItems[clmKgmViewImport.DisplayIndex].Text = _k_tmp.Status.ImportURL;

                        // ポート毎接続制限中ならポート番号を赤文字にする
                        if (_k_tmp.Status.Pause)
                            _k_tmp.Status.Gui.KagamiItem.SubItems[clmKgmViewPort.DisplayIndex].ForeColor = System.Drawing.Color.Red;
                        else
                            _k_tmp.Status.Gui.KagamiItem.SubItems[clmKgmViewPort.DisplayIndex].ResetStyle();

                        // 待機中ポート＆帯域制限停止時は最大接続数をユーザ定義値に再設定
                        if (!_k_tmp.Status.ImportStatus || BandTh == null || !BandTh.IsAlive)
                            _k_tmp.Status.Connection = _k_tmp.Status.Conn_UserSet;

                        // 接続者数を再設定
                        if (clmKgmViewConn.DisplayIndex == 0)
                            _k_tmp.Status.Gui.KagamiItem.Text = _k_tmp.Status.Client.Count + "/" + _k_tmp.Status.Connection + "+" + _k_tmp.Status.Reserve;
                        else
                            _k_tmp.Status.Gui.KagamiItem.SubItems[clmKgmViewConn.DisplayIndex].Text = _k_tmp.Status.Client.Count + "/" + _k_tmp.Status.Connection + "+" + _k_tmp.Status.Reserve;

                        kagamiView.Items.Add(_k_tmp.Status.Gui.KagamiItem);
                        if (_k_tmp.Status.ImportURL != "待機中")
                            _tmp_port_num++;
                    }
                }
                kagamiView.EndUpdate();
                #endregion

                // 接続・切断ボタン状態AUDIT
                ButtonAudit();

                // Web上から設定が変更された場合用
                // Pauseボタン & 帯域制限ボタンOnOff状態AUDIT
                statusBarAudit();

                // インポート接続数(クライアント数じゃないよ)が変化したらTaskTrayTip表示
                if (LastActPortNum != _tmp_port_num)
                {
                    LastActPortNum = _tmp_port_num;
                    if (TaskTrayIcon.Visible)
                        showTaskTrayTip();
                }
            }// end of if(LeftFlag)

            // monAllViewは常に更新
            monAllViewUpdate();

            // StatusBarのEX,IM,CPU使用率も常に更新
            statusBarUpdate();
            #endregion

            #region 右パネルの更新
            // RightPanelは選択中タブ内のフォームのみ更新
            _k = this.SelectedKagami;
            if (_k != null)
            {// 選択中ポートで鏡起動中
                switch (tabKagami.SelectedTab.Text)
                {
                    case "モニタ":
                        monViewUpdate(_k);
                        break;
                    case "クライアント":
                        if (_k.Status.Gui.ClientItem.Count > 0)
                        {
                            clientView.BeginUpdate();
                            _k.Status.Client.UpdateClientTime();
                            clientView.EndUpdate();
                        }
                        break;
                    case "リザーブ":
                        break;
                    case "キック":
                        if (_k.Status.Gui.KickItem.Count > 0)
                        {
                            kickView.BeginUpdate();
                            _k.Status.Client.UpdateKickTime();
                            kickView.EndUpdate();
                        }
                        break;
                    case "ログ":
                        break;
                    default:
                        throw new Exception("not implemented.");
                }
            }
            else
            {// 選択中ポートで鏡未起動
            }
            #endregion

            #region アプリケーションの自動終了
            if (EnableAutoExit && LastActPortNum == 0)
            {
                //TaskTrayから復帰
                this.Visible = true;
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.TaskTrayIcon.Visible = false;
                AskFormClose = false;
                timer1.Stop();
                this.Close();
                return;
            }
            #endregion

            #region 自動シャットダウン
            if (EnableAutoShutdown && LastActPortNum == 0 && ExecShutdown == false)
            {
                //TaskTrayから復帰
                this.Visible = true;
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.TaskTrayIcon.Visible = false;
                //shutdown中止用ボタンに変更
                toolStripAutoShutdown.BorderStyle = Border3DStyle.Raised; // 凸ボタン
                toolStripAutoShutdown.Text = "電断中止";
                ExecShutdown = true;
                AskFormClose = false;
                //Shutdown.exe起動
                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = "shutdown.exe";
                psInfo.CreateNoWindow = true;
                psInfo.UseShellExecute = false;
                psInfo.Arguments = "-s -f -c \"かがみん2からのシャットダウン要求...\"";
                try
                {
                    Process.Start(psInfo);
                }
                catch
                {
                    MessageBox.Show("shutdown.exeの実行に失敗しました.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //shutdown開始用ボタンに変更
                    toolStripAutoShutdown.BorderStyle = Border3DStyle.Raised;
                    toolStripAutoShutdown.Text = "自動電断";
                    ExecShutdown = false;
                    EnableAutoShutdown = false;
                    AskFormClose = true;
                    statusBarAudit();
                }
            }
            #endregion

            #region 長時間インポート自動切断＋少クライアント数自動切断
            if (Front.Acl.ImportOutTime > 0 || Front.Acl.ClientOutCheck)
            {
                bool empty_port = false; // 空きポートなし
                lock (Front.KagamiList)
                {
                    foreach (Kagami _k_tmp in Front.KagamiList)
                    {
                        // 正常に接続できていないポートは対象外
                        if (!_k_tmp.Status.ImportStatus)
                        {
                            empty_port = true; // 空きポート有り
                            continue;
                        }

                        // 内側接続は対象外
                        if (_k_tmp.Status.Type == 0)
                            continue;

                        //Import制限時間オーバー
                        if (Front.Acl.ImportOutTime > 0 && _k_tmp.Status.ImportTime.TotalMinutes >= Front.Acl.ImportOutTime)
                        {
                            if (!Front.Acl.PortFullOnlyCheck || !empty_port)
                            {
                                //インポート切断
                                _k_tmp.Status.Disc();
                                Front.AddLogData(1, _k_tmp.Status, "長時間インポート接続の制限時間に達したため外部待受状態に戻ります(接続時間:" + Front.Acl.ImportOutTime + "分)");
                            }
                        }

                        //Client数が設定数より多くなったら時間リセット
                        if (_k_tmp.Status.Client.Count > Front.Acl.ClientOutNum)
                        {
                            //設定者IPを含まない設定になっている場合
                            if (Front.Acl.ClientNotIPCheck)
                            {
                                int num = 0;
                                lock (_k_tmp.Status.Gui.ClientItem)
                                {
                                    for (int cnt = 0; cnt < _k_tmp.Status.Gui.ClientItem.Count; cnt++)
                                    {
                                        if (_k_tmp.Status.Gui.ClientItem[cnt].SubItems[Front.clmCV_IP_IDX].Text == _k_tmp.Status.SetUserIP)
                                            num++;
                                    }
                                }
                                if (_k_tmp.Status.Client.Count - num > Front.Acl.ClientOutNum)
                                    _k_tmp.Status.ClientTime = DateTime.Now;
                            }
                            else
                            {
                                _k_tmp.Status.ClientTime = DateTime.Now;
                            }
                        }
                        //時間が設定時間を越えると待ち受けに戻る
                        if (Front.Acl.ClientOutCheck && (DateTime.Now - _k_tmp.Status.ClientTime).TotalMinutes >= Front.Acl.ClientOutTime)
                        {
                            if (!Front.Acl.PortFullOnlyCheck || !empty_port)
                            {
                                //インポート切断
                                _k_tmp.Status.Disc();
                                Front.AddLogData(1, _k_tmp.Status, "クライアント数制限の制限時間に達したため外部受付状態に戻ります(" + Front.Acl.ClientOutNum + "人以下/" + Front.Acl.ClientOutTime + "分)");
                            }
                        }
                    }
                }
            }
            /* */
            #endregion

            #region スケジュールイベント
            string _time = DateTime.Now.ToString("HH:mm");
            bool _time_chg = false;
            int _week = (int)DateTime.Now.DayOfWeek;
            if (Time != _time)
            {
                Time = _time;
                _time_chg = true;
                // 時刻00:00(=日付が変わったとき)
                if (_time == "00:00")
                {
                    // 月間トラヒックに退避して情報クリア
                    Front.Log.TrsUpMon += Front.TotalUP;
                    Front.Log.TrsDlMon += Front.TotalDL;
                    Front.TotalUP = 0;
                    Front.TotalDL = 0;
                    Front.Log.TrsUpDay = 0;
                    Front.Log.TrsDlDay = 0;
                    if (DateTime.Now.Day == 1)
                    {
                        // 毎月1日の00:00
                        Front.Log.TrsUpMon = 0;
                        Front.Log.TrsDlMon = 0;
                    }
                    // 転送量指定のイベント実行済みフラグをクリアする
                    foreach (SCHEDULE _item in Front.ScheduleItem)
                    {
                        if (_item.StartType == 1)
                        {
                            switch (_item.TrfType)
                            {
                                case 0:
                                    _item.ExecTrf = false;
                                    break;
                                case 1:
                                default:
                                    if (DateTime.Now.Day == 1)
                                        _item.ExecTrf = false;
                                    break;
                            }
                        }
                    }
                    // 毎日0時に設定ファイル自動保存
                    Front.SaveSetting();
                }
            }
            
            // スケジュールイベントの総チェック
            foreach (SCHEDULE _item in Front.ScheduleItem)
            {
                // 無効スケジュールイベントはSkip
                if (_item.Enable == false)
                    continue;

                // スケジュールイベントタイプでチェック内容分岐
                switch (_item.StartType)
                {
                    case 0: // イベントタイプ=時間指定
                        // 時間変化していなければSkip
                        if (_time_chg)
                            continue;
                        // 時間不一致はSkip
                        string _start_time = _item.Hour.ToString("D2") + ":" + _item.Min.ToString("D2");
                        if (Time != _start_time)
                            continue;
                        // 曜日チェック
                        // 単一曜日指定で曜日不一致はSkip
                        if (_item.Week < 7 && _item.Week != _week)
                            continue;
                        // 平日指定で土日はSkip
                        if (_item.Week == 8 && (_week == 0 || _week >= 6))
                            continue;
                        // 土日指定で土日以外はSkip
                        if (_item.Week == 9 && _week != 0 && _week != 6)
                            continue;
                        break;

                    case 1: // イベントタイプ=転送量指定
                        ulong _trf = (ulong)(_item.TrfValue * (_item.TrfUnit == 0 ? 1000 * 1000 : 1000 * 1000 * 1000));
                        // イベント実行済みならSkip
                        if (_item.ExecTrf)
                            continue;
                        switch (_item.TrfType)
                        {
                            case 0: // 日間転送量
                                // 日間転送量がスケジュール設定値より小さければSkip
                                if (_trf > (Front.Log.TrsUpDay + Front.TotalUP))
                                    continue;
                                break;
                            case 1: // 月間転送量
                                // 月間転送量がスケジュール設定値より小さければSkip
                                if (_trf > (Front.Log.TrsUpMon + Front.TotalUP))
                                    continue;
                                break;
                            default:
                                continue;
                        }
                        _item.ExecTrf = true;
                        break;

                    default:
                        continue;
                }
                //
                // スケジュールに一致するアイテムを発見
                //

                List<int> _port_list = new List<int>();
                if (_item.Port == 0)
                    foreach (string s in myPort.Items)
                        _port_list.Add(int.Parse(s));
                else
                    _port_list.Add((int)_item.Port);

                switch (Front.ScheduleEventString[_item.Event])
                {
                    case "エントランス起動":
                        if (Front.HPStart == false)
                            toolStripHPStart_Click(sender, EventArgs.Empty);
                        break;

                    case "エントランス停止":
                        if (Front.HPStart == true)
                            toolStripHPStart_Click(sender, EventArgs.Empty);
                        break;

                    case "新規受付開始":
                        Front.Pause = false;
                        break;

                    case "新規受付停止":
                        Front.Pause = true;
                        break;

                    case "強制切断":
                        foreach (int _port in _port_list)
                        {
                            // 指定ポート切断
                            _k = Front.IndexOf(_port);
                            if (_k != null)
                            {
                                _k.Status.Disc();
                            }
                        }
                        break;

                    case "ポート待受開始":
                        foreach (int _port in _port_list)
                        {
                            _k = Front.IndexOf(_port);
                            if (_k == null)
                            {
                                // 未起動ポートを起動
                                _k = new Kagami("", (int)_port, (int)_item.Conn, (int)_item.Resv);
                                // 帯域制限起動中の場合、statusの追加設定
                                if (Front.BndWth.EnableBandWidth)
                                {
                                    _k.Status.GUILimitUPSpeed = (int)Front.BndWth.BandStopValue;
                                    _k.Status.LimitUPSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                                }
                                Front.Add(_k);
                            }
                        }
                        break;

                    case "ポート待受停止":
                        foreach (int _port in _port_list)
                        {
                            _k = Front.IndexOf(_port);
                            if (_k != null)
                            {
                                // 指定ポートが起動中なので、停止する
                                _k.Status.RunStatus = false;
                            }
                        }
                        break;

                    case "接続枠数変更":
                        foreach (int _port in _port_list)
                        {
                            _k = Front.IndexOf(_port);
                            if (_k != null)
                            {
                                _k.Status.Conn_UserSet = (int)_item.Conn;
                                _k.Status.Reserve = (int)_item.Resv;
                            }
                        }
                        break;

                    default:
                        throw new Exception("not implemented.");
                }
                // 全スケジュール完了後
                // ステータスバーのAUDIT実施
                statusBarAudit();
            }
            #endregion

#if OVERLOAD
            //エクスポート接続過負荷試験ツール
            _k = this.SelectedKagami;
            timer1.Interval = 300;
            if (_k != null && _k.Status.ImportStatus)
            {
                overload_cnt++;
                if (overload_cnt > 1000) overload_cnt = 1;
                Status _status = new Status(null, _k.Status.ImportURL, _k.Status.MyPort+overload_cnt, _k.Status.Conn_UserSet, _k.Status.Reserve);
                Import _import = new Import(_status);

                Thread _th = new Thread(overload);
                _th.Start((object)_status);
            }
#endif

        }//end of timer1_Tick
        #endregion

        #region 帯域制限
        /// <summary>
        /// 帯域制限開始
        /// </summary>
        private void StartBandWidth()
        {
            if (BandTh != null)
                return; // 二重起動ガード
            try
            {
                Front.AddLogDebug("帯域制限状態変更", "帯域制限タスクを開始します");
                BandTh = new Thread(BandWidth);
                BandTh.Name = "BandWidth";
                BandTh.Start();
            }
            catch
            {
                MessageBox.Show("帯域制限タスクが開始できません", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Front.BndWth.EnableBandWidth = false;
                statusBarAudit();
                return;
            }
        }
        /// <summary>
        /// 帯域制限停止
        /// </summary>
        private void StopBandWidth()
        {
            if (BandTh == null)
                return; // 二重停止ガード

            try
            {
                Front.AddLogDebug("帯域制限状態変更", "帯域制限タスクを停止します");
                BandTh.Abort();
                BandTh = null;
            }
            catch
            {
                MessageBox.Show("帯域制限タスクが停止できません", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Front.BndWth.EnableBandWidth = true;
                statusBarAudit();
                return;
            }
            //元々の最大接続数に戻す
            lock (Front.KagamiList)
                foreach (Kagami _k in Front.KagamiList)
                    _k.Status.Connection = _k.Status.Conn_UserSet;
            LeftFlag = true;
        }
        /// <summary>
        /// 帯域制限タスク
        /// </summary>
        private void BandWidth()
        {
            int TotalClient = 0;
            int LimitSpeed = 0;
            Front.AddLogDebug("帯域制限", "帯域制限を開始します");
            while (true)
            {
                try
                {
                    Front.AddLogDebug("帯域制限", "---帯域制限計算開始---");
                    switch (Front.BndWth.BandStopMode)
                    {
                        case 0: // 全ポート合計での制限値指定
                            // 各ポートの接続可能数比率で分配する
                            TotalClient = 0;
                            LimitSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                            //全ポートの接続可能数を計算
                            lock (Front.KagamiList)
                            {
                                foreach (Kagami _k in Front.KagamiList)
                                {
                                    if (Front.BndWth.BandStopResv)
                                        TotalClient += _k.Status.Conn_UserSet + _k.Status.Reserve;
                                    else
                                        TotalClient += _k.Status.Conn_UserSet;
                                }
                            }
                            Front.AddLogDebug("帯域制限", "UP帯域上限値=" + LimitSpeed + "Kbps");
                            Front.AddLogDebug("帯域制限", "全体の接続可能数=" + TotalClient);
                            //各鏡に制限帯域値をセット
                            lock (Front.KagamiList)
                            {
                                foreach (Kagami _k in Front.KagamiList)
                                {
                                    if (Front.BndWth.BandStopResv)
                                    {
                                        _k.Status.LimitUPSpeed = LimitSpeed * (_k.Status.Conn_UserSet + _k.Status.Reserve) / TotalClient;
                                    }
                                    else
                                        _k.Status.LimitUPSpeed = LimitSpeed * _k.Status.Conn_UserSet / TotalClient;
                                }
                            }
                            break;
                        case 1: // １ポート単位での制限値指定
                            //各鏡に制限帯域値をセット
                            LimitSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                            lock (Front.KagamiList)
                                foreach (Kagami _k in Front.KagamiList)
                                    _k.Status.LimitUPSpeed = LimitSpeed;
                            break;
                        case 2: // ポート毎に個別指定する
                            // LimitUPSpeed設定済み
                            break;
                        default:
                            break;
                    }

                    lock (Front.KagamiList)
                    {
                        //制限帯域値を元に最大接続人数を計算しセットする
                        //セットされた値は、Kbps単位で計算されています
                        //帯域に余裕があっても、GUI上で設定した通常接続最大数は超えないように設定する。
                        foreach (Kagami _k in Front.KagamiList)
                        {
                            Front.AddLogDebug("帯域制限", "PORT=" + _k.Status.MyPort + "/PORT毎UP帯域上限値=" + _k.Status.LimitUPSpeed + "kbps");
                            if (_k.Status.MaxDLSpeed == 0 && _k.Status.CurrentDLSpeed == 0)
                            {
                                // 申告帯域も実測値も取得できない場合は最大人数設定
                                _k.Status.Connection = _k.Status.Conn_UserSet;
                                Front.AddLogDebug("帯域制限", "PORT=" + _k.Status.MyPort + "/DL帯域0の為ユーザ値を使用/conn=inf/user=" + _k.Status.Conn_UserSet);
                            }
                            else
                            {
                                int conn_old = _k.Status.Connection;
                                int conn = 0;
                                if ((_k.Status.MaxDLSpeed == 0) || (_k.Status.AverageDLSpeed * 0.9 > _k.Status.MaxDLSpeed))
                                {
                                    // インポート接続時に申告速度が取得出来なかった場合、または
                                    // 実測値平均速度×0.9＞申告速度の場合、実測値で制限する
                                    conn = (int)((_k.Status.LimitUPSpeed * 0.9) / _k.Status.AverageDLSpeed);//0.9は余裕値
                                }
                                else
                                {
                                    // インポート接続時に申告した最大帯域が使える場合、最大帯域で制限する
                                    conn = (int)((_k.Status.LimitUPSpeed * 0.9) / _k.Status.MaxDLSpeed);//0.9は余裕値
                                }
                                // リザーブ考慮の有無
                                if (Front.BndWth.BandStopResv)
                                    conn -= _k.Status.Reserve;
                                if (conn < 0) { conn = 0; }

                                if (conn < _k.Status.Conn_UserSet)
                                {
                                    _k.Status.Connection = conn;
                                    Front.AddLogDebug("帯域制限", "PORT=" + _k.Status.MyPort + "/制限値に達した為自動計算値を使用/conn=" + conn + "/user=" + _k.Status.Conn_UserSet);
                                }
                                else
                                {
                                    _k.Status.Connection = _k.Status.Conn_UserSet;
                                    Front.AddLogDebug("帯域制限", "PORT=" + _k.Status.MyPort + "/制限値に達しない為ユーザ値を使用/conn=" + conn + "/user=" + _k.Status.Conn_UserSet);
                                }
                                if (conn != conn_old)
                                    LeftFlag = true;
                            }
                        }
                    }
                    Front.AddLogDebug("帯域制限", "---帯域制限計算終了---");
                    // 即時再計算フラグが立つか、10秒経過するまで待つ
                    for (int cnt = 0; !BandFlag && cnt < 10; cnt++)
                        Thread.Sleep(1000);
                    BandFlag = false;
                }
                catch (ThreadAbortException e)
                {
                    Front.AddLogDebug("帯域制限", "帯域制限を終了します:" + e.Message);
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("帯域制限", "帯域制限スレッド内で例外発生:" + e.Message + "/Trace:" + e.StackTrace + ")");
                }
            }
        }
        #endregion

#if OVERLOAD
        /// <summary>
        /// エクスポート接続過負荷試験ツール
        /// </summary>
        private void overload(object obj)
        {
            Status _status = (Status)obj;
            while (_status.ImportStatus == false)
                Thread.Sleep(1000);

            // 10秒未満でランダム待機
            Random _ran = new Random();
            Thread.Sleep(_ran.Next(1000, 10000));

            _status.Disc();
        }
#endif
    }
}