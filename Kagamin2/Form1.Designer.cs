namespace Kagamin2
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.TaskTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.TaskTrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RstWndPos = new System.Windows.Forms.ToolStripMenuItem();
            this.TrayMenuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.ReserveViewRClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ResvDelMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ResvIPCopyMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.FavReserveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddReserveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DelReserveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.copyMyUrlBTN = new System.Windows.Forms.Button();
            this.clearBTN = new System.Windows.Forms.Button();
            this.discBTN = new System.Windows.Forms.Button();
            this.connBTN = new System.Windows.Forms.Button();
            this.resvNum = new System.Windows.Forms.NumericUpDown();
            this.connNum = new System.Windows.Forms.NumericUpDown();
            this.myPort = new System.Windows.Forms.ComboBox();
            this.importURL = new System.Windows.Forms.TextBox();
            this.ImportUrlRClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AddFavoriteMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.ImportUrlCutMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportUrlCopyMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportUrlPasteMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportUrlEraseMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.addResvHost = new System.Windows.Forms.TextBox();
            this.addKickHost = new System.Windows.Forms.TextBox();
            this.optBTN = new System.Windows.Forms.Button();
            this.ClientViewRClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ClientDiscMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ClientKickDefMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ClientKickSubMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ClientKick1minMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ClientKick5minMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ClientKick10minMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ClientKick30minMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ClientKick1hourMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ClientKickLimited = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ClientAddResvMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ClientIPCopyMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripLeftPanelOnOff = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripHPStart = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripHPPause = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripBandStart = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripAutoExit = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripAutoShutdown = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripEXNum = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripIMNum = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripAC = new System.Windows.Forms.ToolStripStatusLabel();
            this.KickViewRClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.KickAddSubMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KickAdd1minMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KickAdd5minMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KickAdd10minMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KickAdd30minMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KickAdd1hourMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KickAddLimited = new System.Windows.Forms.ToolStripMenuItem();
            this.KickSelDelMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KickSelDelClearMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.KickAllDelMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KickAllDelClearMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.KickIPCopyMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.LogViewRClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.LogClearMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.LogClearAllMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.LogCopyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LogIPCopyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.kagamiView = new System.Windows.Forms.ListView();
            this.clmKgmViewPort = new System.Windows.Forms.ColumnHeader();
            this.clmKgmViewImport = new System.Windows.Forms.ColumnHeader();
            this.clmKgmViewConn = new System.Windows.Forms.ColumnHeader();
            this.kagamiViewRClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.KgmIpCopyMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KgmImIpCopyMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KgmImPauseMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.KgmImDiscMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.monAllView = new System.Windows.Forms.ListView();
            this.clmMonAllView1 = new System.Windows.Forms.ColumnHeader();
            this.clmMonAllView2 = new System.Windows.Forms.ColumnHeader();
            this.tabKagami = new System.Windows.Forms.TabControl();
            this.tabPageKgm1 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.bndStopUnit = new System.Windows.Forms.Label();
            this.bndStopNum = new System.Windows.Forms.NumericUpDown();
            this.bndStopLabel = new System.Windows.Forms.Label();
            this.monView = new System.Windows.Forms.ListView();
            this.clmMonView1 = new System.Windows.Forms.ColumnHeader();
            this.clmMonView2 = new System.Windows.Forms.ColumnHeader();
            this.tabPageKgm2 = new System.Windows.Forms.TabPage();
            this.clientView = new System.Windows.Forms.ListView();
            this.clmClientViewID = new System.Windows.Forms.ColumnHeader();
            this.clmClientViewBuffer = new System.Windows.Forms.ColumnHeader();
            this.clmClientViewIP = new System.Windows.Forms.ColumnHeader();
            this.clmClientViewUA = new System.Windows.Forms.ColumnHeader();
            this.clmClientViewTime = new System.Windows.Forms.ColumnHeader();
            this.columnViewHost = new System.Windows.Forms.ColumnHeader();
            this.clmClientViewConnInfo = new System.Windows.Forms.ColumnHeader();
            this.tabPageKgm3 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.addResvBTN = new System.Windows.Forms.Button();
            this.reserveView = new System.Windows.Forms.ListView();
            this.clmResvViewIP = new System.Windows.Forms.ColumnHeader();
            this.clmResvViewStatus = new System.Windows.Forms.ColumnHeader();
            this.tabPageKgm4 = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.addKickBTN = new System.Windows.Forms.Button();
            this.kickView = new System.Windows.Forms.ListView();
            this.clmKickViewIP = new System.Windows.Forms.ColumnHeader();
            this.clmKickViewStatus = new System.Windows.Forms.ColumnHeader();
            this.clmKickViewCount = new System.Windows.Forms.ColumnHeader();
            this.tabPageKgm5 = new System.Windows.Forms.TabPage();
            this.logView = new System.Windows.Forms.ListView();
            this.clmLogView1 = new System.Windows.Forms.ColumnHeader();
            this.clmLogView2 = new System.Windows.Forms.ColumnHeader();
            this.logAutoScroll = new System.Windows.Forms.CheckBox();
            this.logModeImp = new System.Windows.Forms.RadioButton();
            this.logModeAll = new System.Windows.Forms.RadioButton();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.PreviewWindowsMediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            this.optnonWMPLabel = new System.Windows.Forms.Label();
            this.MonViewRClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MonUnitChgMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.MonImIpCopyMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.MonStoppingKagamiCMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.MonModeChgMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CopyMyIPMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MyDDNSCopyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.ExportAuthMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TaskTrayMenu.SuspendLayout();
            this.ReserveViewRClick.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resvNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.connNum)).BeginInit();
            this.ImportUrlRClick.SuspendLayout();
            this.ClientViewRClick.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.KickViewRClick.SuspendLayout();
            this.LogViewRClick.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.kagamiViewRClick.SuspendLayout();
            this.tabKagami.SuspendLayout();
            this.tabPageKgm1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bndStopNum)).BeginInit();
            this.tabPageKgm2.SuspendLayout();
            this.tabPageKgm3.SuspendLayout();
            this.tabPageKgm4.SuspendLayout();
            this.tabPageKgm5.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewWindowsMediaPlayer)).BeginInit();
            this.MonViewRClick.SuspendLayout();
            this.OptionClick.SuspendLayout();
            this.SuspendLayout();
            // 
            // TaskTrayIcon
            // 
            this.TaskTrayIcon.ContextMenuStrip = this.TaskTrayMenu;
            this.TaskTrayIcon.Visible = true;
            this.TaskTrayIcon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskTrayIcon_MouseMove);
            this.TaskTrayIcon.Click += new System.EventHandler(this.TaskTrayIcon_Click);
            this.TaskTrayIcon.DoubleClick += new System.EventHandler(this.TaskTrayIcon_Click);
            // 
            // TaskTrayMenu
            // 
            this.TaskTrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RstWndPos,
            this.TrayMenuExit});
            this.TaskTrayMenu.Name = "TaskTrayMenu";
            this.TaskTrayMenu.ShowImageMargin = false;
            this.TaskTrayMenu.Size = new System.Drawing.Size(172, 48);
            this.TaskTrayMenu.Text = "TaskTrayMenu";
            // 
            // RstWndPos
            // 
            this.RstWndPos.Name = "RstWndPos";
            this.RstWndPos.Size = new System.Drawing.Size(171, 22);
            this.RstWndPos.Text = "ウインドウ位置初期化";
            this.RstWndPos.Click += new System.EventHandler(this.RstWndPos_Click);
            // 
            // TrayMenuExit
            // 
            this.TrayMenuExit.Name = "TrayMenuExit";
            this.TrayMenuExit.Size = new System.Drawing.Size(171, 22);
            this.TrayMenuExit.Text = "終了(&X)";
            this.TrayMenuExit.Click += new System.EventHandler(this.TrayMenuExit_Click);
            // 
            // ReserveViewRClick
            // 
            this.ReserveViewRClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ResvDelMenu,
            this.ResvIPCopyMenu,
            this.toolStripSeparator9,
            this.FavReserveMenuItem});
            this.ReserveViewRClick.Name = "ReserveRClick";
            this.ReserveViewRClick.ShowImageMargin = false;
            this.ReserveViewRClick.Size = new System.Drawing.Size(166, 76);
            this.ReserveViewRClick.Opening += new System.ComponentModel.CancelEventHandler(this.ReserveViewRClick_Opening);
            // 
            // ResvDelMenu
            // 
            this.ResvDelMenu.Name = "ResvDelMenu";
            this.ResvDelMenu.Size = new System.Drawing.Size(165, 22);
            this.ResvDelMenu.Text = "削除(&D)";
            this.ResvDelMenu.Click += new System.EventHandler(this.ResvDelMenu_Click);
            // 
            // ResvIPCopyMenu
            // 
            this.ResvIPCopyMenu.Name = "ResvIPCopyMenu";
            this.ResvIPCopyMenu.Size = new System.Drawing.Size(165, 22);
            this.ResvIPCopyMenu.Text = "IPアドレスコピー(&C)";
            this.ResvIPCopyMenu.Click += new System.EventHandler(this.IPCopyMenu_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(162, 6);
            // 
            // FavReserveMenuItem
            // 
            this.FavReserveMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddReserveMenuItem,
            this.DelReserveMenuItem,
            this.toolStripSeparator10});
            this.FavReserveMenuItem.Name = "FavReserveMenuItem";
            this.FavReserveMenuItem.Size = new System.Drawing.Size(165, 22);
            this.FavReserveMenuItem.Text = "お気に入り";
            // 
            // AddReserveMenuItem
            // 
            this.AddReserveMenuItem.Name = "AddReserveMenuItem";
            this.AddReserveMenuItem.Size = new System.Drawing.Size(100, 22);
            this.AddReserveMenuItem.Text = "追加";
            this.AddReserveMenuItem.Click += new System.EventHandler(this.AddReserveMenuItem_Click);
            // 
            // DelReserveMenuItem
            // 
            this.DelReserveMenuItem.Name = "DelReserveMenuItem";
            this.DelReserveMenuItem.Size = new System.Drawing.Size(100, 22);
            this.DelReserveMenuItem.Text = "削除";
            this.DelReserveMenuItem.Click += new System.EventHandler(this.DelReserveMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(97, 6);
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 200;
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 200;
            this.toolTip1.ReshowDelay = 40;
            // 
            // copyMyUrlBTN
            // 
            this.copyMyUrlBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.copyMyUrlBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.copyMyUrlBTN.Location = new System.Drawing.Point(653, 0);
            this.copyMyUrlBTN.Name = "copyMyUrlBTN";
            this.copyMyUrlBTN.Size = new System.Drawing.Size(21, 23);
            this.copyMyUrlBTN.TabIndex = 7;
            this.copyMyUrlBTN.Text = "鏡";
            this.toolTip1.SetToolTip(this.copyMyUrlBTN, "自分のグローバルIPを取得して、\r\n「http://自グローバルIP:自鏡ポート/」の形式で\r\nクリップボードにコピーします");
            this.copyMyUrlBTN.UseVisualStyleBackColor = true;
            this.copyMyUrlBTN.MouseDown += new System.Windows.Forms.MouseEventHandler(this.copyMyUrlBTN_Click);
            // 
            // clearBTN
            // 
            this.clearBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.clearBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clearBTN.Location = new System.Drawing.Point(631, 0);
            this.clearBTN.Name = "clearBTN";
            this.clearBTN.Size = new System.Drawing.Size(21, 23);
            this.clearBTN.TabIndex = 6;
            this.clearBTN.Text = "消";
            this.toolTip1.SetToolTip(this.clearBTN, "接続先URLの欄を消去します。");
            this.clearBTN.UseVisualStyleBackColor = true;
            this.clearBTN.Click += new System.EventHandler(this.clearBTN_Click);
            // 
            // discBTN
            // 
            this.discBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.discBTN.Enabled = false;
            this.discBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.discBTN.Location = new System.Drawing.Point(589, 0);
            this.discBTN.Name = "discBTN";
            this.discBTN.Size = new System.Drawing.Size(43, 23);
            this.discBTN.TabIndex = 5;
            this.discBTN.Text = "切断";
            this.toolTip1.SetToolTip(this.discBTN, "配信先から切断します。\r\n外部接続要求待ちうけ状態もキャンセルします。");
            this.discBTN.UseVisualStyleBackColor = true;
            this.discBTN.Click += new System.EventHandler(this.discBTN_Click);
            // 
            // connBTN
            // 
            this.connBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.connBTN.Location = new System.Drawing.Point(547, 0);
            this.connBTN.Name = "connBTN";
            this.connBTN.Size = new System.Drawing.Size(42, 23);
            this.connBTN.TabIndex = 4;
            this.connBTN.Text = "接続";
            this.toolTip1.SetToolTip(this.connBTN, "配信先に接続します。\r\n\r\n空欄のまま接続ボタンを押した場合は、\r\n外部からの接続要求待ち受けモードに移行します。");
            this.connBTN.UseVisualStyleBackColor = true;
            this.connBTN.Click += new System.EventHandler(this.connBTN_Click);
            // 
            // resvNum
            // 
            this.resvNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resvNum.Location = new System.Drawing.Point(506, 1);
            this.resvNum.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.resvNum.Name = "resvNum";
            this.resvNum.Size = new System.Drawing.Size(42, 20);
            this.resvNum.TabIndex = 3;
            this.toolTip1.SetToolTip(this.resvNum, "最大リザーブ接続数。\r\n右端のコントロールで数字を調整するか、\r\n数値を直接書き換えることで接続数を変更します。");
            this.resvNum.ValueChanged += new System.EventHandler(this.resvNum_ValueChanged);
            this.resvNum.KeyUp += new System.Windows.Forms.KeyEventHandler(this.resvNum_KeyUp);
            // 
            // connNum
            // 
            this.connNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connNum.Location = new System.Drawing.Point(462, 1);
            this.connNum.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.connNum.Name = "connNum";
            this.connNum.Size = new System.Drawing.Size(42, 20);
            this.connNum.TabIndex = 2;
            this.toolTip1.SetToolTip(this.connNum, "最大通常接続数。\r\n右端のコントロールで数字を調整するか、\r\n数値を直接書き換えることで接続数を変更します。");
            this.connNum.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.connNum.ValueChanged += new System.EventHandler(this.connNum_ValueChanged);
            this.connNum.KeyUp += new System.Windows.Forms.KeyEventHandler(this.connNum_KeyUp);
            // 
            // myPort
            // 
            this.myPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.myPort.FormattingEnabled = true;
            this.myPort.Location = new System.Drawing.Point(405, 1);
            this.myPort.MaxLength = 5;
            this.myPort.Name = "myPort";
            this.myPort.Size = new System.Drawing.Size(55, 21);
            this.myPort.TabIndex = 1;
            this.myPort.Text = "8888";
            this.toolTip1.SetToolTip(this.myPort, "鏡ポートとして起動する自ポート番号。\r\n設定から、使用するポートを予め登録出来ます。\r\n直接数値を入力して使用する事も出来ます。");
            this.myPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.myPort_KeyPress);
            this.myPort.TextChanged += new System.EventHandler(this.myPort_TextChanged);
            // 
            // importURL
            // 
            this.importURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.importURL.ContextMenuStrip = this.ImportUrlRClick;
            this.importURL.Location = new System.Drawing.Point(0, 1);
            this.importURL.Name = "importURL";
            this.importURL.Size = new System.Drawing.Size(404, 20);
            this.importURL.TabIndex = 0;
            this.toolTip1.SetToolTip(this.importURL, "接続先URLを\r\n「 http://hostname:port/ 」の形式で入力します。\r\n\r\n空欄のまま接続ボタンを押した場合は、\r\n外部からの接続要求待ち受け" +
                    "モードに移行します。");
            this.importURL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.importURL_KeyPress);
            // 
            // ImportUrlRClick
            // 
            this.ImportUrlRClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddFavoriteMenu,
            this.toolStripSeparator6,
            this.ImportUrlCutMenu,
            this.ImportUrlCopyMenu,
            this.ImportUrlPasteMenu,
            this.ImportUrlEraseMenu,
            this.toolStripSeparator7});
            this.ImportUrlRClick.Name = "ImportTextBoxRClick";
            this.ImportUrlRClick.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.ImportUrlRClick.ShowImageMargin = false;
            this.ImportUrlRClick.Size = new System.Drawing.Size(148, 126);
            // 
            // AddFavoriteMenu
            // 
            this.AddFavoriteMenu.Name = "AddFavoriteMenu";
            this.AddFavoriteMenu.Size = new System.Drawing.Size(147, 22);
            this.AddFavoriteMenu.Text = "お気に入りへ登録";
            this.AddFavoriteMenu.Click += new System.EventHandler(this.AddFavoriteMenu_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(144, 6);
            // 
            // ImportUrlCutMenu
            // 
            this.ImportUrlCutMenu.Name = "ImportUrlCutMenu";
            this.ImportUrlCutMenu.Size = new System.Drawing.Size(147, 22);
            this.ImportUrlCutMenu.Text = "切り取り(&T)";
            this.ImportUrlCutMenu.Click += new System.EventHandler(this.ImportUrlCutMenu_Click);
            // 
            // ImportUrlCopyMenu
            // 
            this.ImportUrlCopyMenu.Name = "ImportUrlCopyMenu";
            this.ImportUrlCopyMenu.Size = new System.Drawing.Size(147, 22);
            this.ImportUrlCopyMenu.Text = "コピー(&C)";
            this.ImportUrlCopyMenu.Click += new System.EventHandler(this.ImportUrlCopyMenu_Click);
            // 
            // ImportUrlPasteMenu
            // 
            this.ImportUrlPasteMenu.Name = "ImportUrlPasteMenu";
            this.ImportUrlPasteMenu.Size = new System.Drawing.Size(147, 22);
            this.ImportUrlPasteMenu.Text = "貼り付け(&P)";
            this.ImportUrlPasteMenu.Click += new System.EventHandler(this.ImportUrlPasteMenu_Click);
            // 
            // ImportUrlEraseMenu
            // 
            this.ImportUrlEraseMenu.Name = "ImportUrlEraseMenu";
            this.ImportUrlEraseMenu.Size = new System.Drawing.Size(147, 22);
            this.ImportUrlEraseMenu.Text = "削除(&D)";
            this.ImportUrlEraseMenu.Click += new System.EventHandler(this.ImportUrlEraseMenu_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(144, 6);
            // 
            // addResvHost
            // 
            this.addResvHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.addResvHost.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.addResvHost.Location = new System.Drawing.Point(80, 7);
            this.addResvHost.Name = "addResvHost";
            this.addResvHost.Size = new System.Drawing.Size(354, 20);
            this.addResvHost.TabIndex = 1;
            this.toolTip1.SetToolTip(this.addResvHost, "リザーブ登録するホストの\r\nIPアドレスまたはFQDN名を入力");
            this.addResvHost.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.addResvHost_KeyPress);
            // 
            // addKickHost
            // 
            this.addKickHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.addKickHost.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.addKickHost.Location = new System.Drawing.Point(80, 7);
            this.addKickHost.Name = "addKickHost";
            this.addKickHost.Size = new System.Drawing.Size(354, 20);
            this.addKickHost.TabIndex = 1;
            this.toolTip1.SetToolTip(this.addKickHost, "キック登録するホストの\r\nIPアドレスまたはFQDN名を入力");
            this.addKickHost.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.addKickHost_KeyPress);
            // 
            // optBTN
            // 
            this.optBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.optBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.optBTN.Location = new System.Drawing.Point(675, 0);
            this.optBTN.Name = "optBTN";
            this.optBTN.Size = new System.Drawing.Size(21, 23);
            this.optBTN.TabIndex = 8;
            this.optBTN.Text = "設";
            this.toolTip1.SetToolTip(this.optBTN, "設定ウインドウを開きます");
            this.optBTN.UseVisualStyleBackColor = true;
            this.optBTN.Click += new System.EventHandler(this.optBTN_Click);
            // 
            // ClientViewRClick
            // 
            this.ClientViewRClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClientDiscMenu,
            this.toolStripSeparator3,
            this.ClientKickDefMenu,
            this.ClientKickSubMenu,
            this.toolStripSeparator2,
            this.ClientAddResvMenu,
            this.toolStripSeparator4,
            this.ClientIPCopyMenu});
            this.ClientViewRClick.Name = "ClientRClick";
            this.ClientViewRClick.ShowImageMargin = false;
            this.ClientViewRClick.Size = new System.Drawing.Size(166, 132);
            this.ClientViewRClick.Opening += new System.ComponentModel.CancelEventHandler(this.ClientViewRClick_Opening);
            // 
            // ClientDiscMenu
            // 
            this.ClientDiscMenu.Name = "ClientDiscMenu";
            this.ClientDiscMenu.Size = new System.Drawing.Size(165, 22);
            this.ClientDiscMenu.Text = "切断(&D)";
            this.ClientDiscMenu.Click += new System.EventHandler(this.ClientDiscMenu_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(162, 6);
            // 
            // ClientKickDefMenu
            // 
            this.ClientKickDefMenu.Name = "ClientKickDefMenu";
            this.ClientKickDefMenu.Size = new System.Drawing.Size(165, 22);
            this.ClientKickDefMenu.Tag = "0";
            this.ClientKickDefMenu.Text = "標準キック(&K)";
            this.ClientKickDefMenu.Click += new System.EventHandler(this.ClientKickMenu_Click);
            // 
            // ClientKickSubMenu
            // 
            this.ClientKickSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClientKick1minMenu,
            this.ClientKick5minMenu,
            this.ClientKick10minMenu,
            this.ClientKick30minMenu,
            this.ClientKick1hourMenu,
            this.ClientKickLimited});
            this.ClientKickSubMenu.Name = "ClientKickSubMenu";
            this.ClientKickSubMenu.Size = new System.Drawing.Size(165, 22);
            this.ClientKickSubMenu.Text = "時間指定キック";
            // 
            // ClientKick1minMenu
            // 
            this.ClientKick1minMenu.Name = "ClientKick1minMenu";
            this.ClientKick1minMenu.Size = new System.Drawing.Size(143, 22);
            this.ClientKick1minMenu.Tag = "60";
            this.ClientKick1minMenu.Text = "1分キック";
            this.ClientKick1minMenu.Click += new System.EventHandler(this.ClientKickMenu_Click);
            // 
            // ClientKick5minMenu
            // 
            this.ClientKick5minMenu.Name = "ClientKick5minMenu";
            this.ClientKick5minMenu.Size = new System.Drawing.Size(143, 22);
            this.ClientKick5minMenu.Tag = "300";
            this.ClientKick5minMenu.Text = "5分キック";
            this.ClientKick5minMenu.Click += new System.EventHandler(this.ClientKickMenu_Click);
            // 
            // ClientKick10minMenu
            // 
            this.ClientKick10minMenu.Name = "ClientKick10minMenu";
            this.ClientKick10minMenu.Size = new System.Drawing.Size(143, 22);
            this.ClientKick10minMenu.Tag = "600";
            this.ClientKick10minMenu.Text = "10分キック";
            this.ClientKick10minMenu.Click += new System.EventHandler(this.ClientKickMenu_Click);
            // 
            // ClientKick30minMenu
            // 
            this.ClientKick30minMenu.Name = "ClientKick30minMenu";
            this.ClientKick30minMenu.Size = new System.Drawing.Size(143, 22);
            this.ClientKick30minMenu.Tag = "1800";
            this.ClientKick30minMenu.Text = "30分キック";
            this.ClientKick30minMenu.Click += new System.EventHandler(this.ClientKickMenu_Click);
            // 
            // ClientKick1hourMenu
            // 
            this.ClientKick1hourMenu.Name = "ClientKick1hourMenu";
            this.ClientKick1hourMenu.Size = new System.Drawing.Size(143, 22);
            this.ClientKick1hourMenu.Tag = "3600";
            this.ClientKick1hourMenu.Text = "1時間キック";
            this.ClientKick1hourMenu.Click += new System.EventHandler(this.ClientKickMenu_Click);
            // 
            // ClientKickLimited
            // 
            this.ClientKickLimited.Name = "ClientKickLimited";
            this.ClientKickLimited.Size = new System.Drawing.Size(143, 22);
            this.ClientKickLimited.Tag = "-1";
            this.ClientKickLimited.Text = "無制限";
            this.ClientKickLimited.Click += new System.EventHandler(this.ClientKickMenu_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(162, 6);
            // 
            // ClientAddResvMenu
            // 
            this.ClientAddResvMenu.Name = "ClientAddResvMenu";
            this.ClientAddResvMenu.Size = new System.Drawing.Size(165, 22);
            this.ClientAddResvMenu.Text = "リザーブ登録(&R)";
            this.ClientAddResvMenu.Click += new System.EventHandler(this.ClientAddResvMenu_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(162, 6);
            // 
            // ClientIPCopyMenu
            // 
            this.ClientIPCopyMenu.Name = "ClientIPCopyMenu";
            this.ClientIPCopyMenu.Size = new System.Drawing.Size(165, 22);
            this.ClientIPCopyMenu.Text = "IPアドレスコピー(&C)";
            this.ClientIPCopyMenu.Click += new System.EventHandler(this.IPCopyMenu_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLeftPanelOnOff,
            this.toolStripHPStart,
            this.toolStripHPPause,
            this.toolStripBandStart,
            this.toolStripAutoExit,
            this.toolStripAutoShutdown,
            this.toolStripEXNum,
            this.toolStripIMNum,
            this.toolStripStatusLabel1,
            this.toolStripAC});
            this.statusStrip1.Location = new System.Drawing.Point(0, 377);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(696, 27);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripLeftPanelOnOff
            // 
            this.toolStripLeftPanelOnOff.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripLeftPanelOnOff.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripLeftPanelOnOff.Name = "toolStripLeftPanelOnOff";
            this.toolStripLeftPanelOnOff.Size = new System.Drawing.Size(60, 22);
            this.toolStripLeftPanelOnOff.Text = "左パネル";
            this.toolStripLeftPanelOnOff.ToolTipText = "左パネルを表示します。";
            this.toolStripLeftPanelOnOff.MouseHover += new System.EventHandler(this.toolStripStatus_MouseHover);
            this.toolStripLeftPanelOnOff.MouseLeave += new System.EventHandler(this.toolStripStatus_MouseLeave);
            this.toolStripLeftPanelOnOff.Click += new System.EventHandler(this.toolStripLeftPanelOnOff_Click);
            // 
            // toolStripHPStart
            // 
            this.toolStripHPStart.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripHPStart.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripHPStart.Name = "toolStripHPStart";
            this.toolStripHPStart.Size = new System.Drawing.Size(60, 22);
            this.toolStripHPStart.Text = "鏡置き場";
            this.toolStripHPStart.ToolTipText = "鏡置き場エントランスを起動します。";
            this.toolStripHPStart.MouseHover += new System.EventHandler(this.toolStripStatus_MouseHover);
            this.toolStripHPStart.MouseLeave += new System.EventHandler(this.toolStripStatus_MouseLeave);
            this.toolStripHPStart.Click += new System.EventHandler(this.toolStripHPStart_Click);
            // 
            // toolStripHPPause
            // 
            this.toolStripHPPause.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripHPPause.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripHPPause.Name = "toolStripHPPause";
            this.toolStripHPPause.Size = new System.Drawing.Size(60, 22);
            this.toolStripHPPause.Text = "接続制限";
            this.toolStripHPPause.ToolTipText = "鏡置き場エントランスからの\r\n新規接続を一時的に制限します。";
            this.toolStripHPPause.MouseHover += new System.EventHandler(this.toolStripStatus_MouseHover);
            this.toolStripHPPause.MouseLeave += new System.EventHandler(this.toolStripStatus_MouseLeave);
            this.toolStripHPPause.Click += new System.EventHandler(this.toolStripHPPause_Click);
            // 
            // toolStripBandStart
            // 
            this.toolStripBandStart.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripBandStart.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripBandStart.Name = "toolStripBandStart";
            this.toolStripBandStart.Size = new System.Drawing.Size(60, 22);
            this.toolStripBandStart.Text = "帯域制限";
            this.toolStripBandStart.ToolTipText = "帯域制限を開始します。";
            this.toolStripBandStart.MouseHover += new System.EventHandler(this.toolStripStatus_MouseHover);
            this.toolStripBandStart.MouseLeave += new System.EventHandler(this.toolStripStatus_MouseLeave);
            this.toolStripBandStart.Click += new System.EventHandler(this.toolStripBandStart_Click);
            // 
            // toolStripAutoExit
            // 
            this.toolStripAutoExit.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripAutoExit.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripAutoExit.Name = "toolStripAutoExit";
            this.toolStripAutoExit.Size = new System.Drawing.Size(60, 22);
            this.toolStripAutoExit.Text = "自動終了";
            this.toolStripAutoExit.ToolTipText = "全ポートが終了または待機状態になった時、\r\nアプリケーションを終了します。";
            this.toolStripAutoExit.MouseHover += new System.EventHandler(this.toolStripStatus_MouseHover);
            this.toolStripAutoExit.MouseLeave += new System.EventHandler(this.toolStripStatus_MouseLeave);
            this.toolStripAutoExit.Click += new System.EventHandler(this.toolStripAutoExit_Click);
            // 
            // toolStripAutoShutdown
            // 
            this.toolStripAutoShutdown.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripAutoShutdown.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripAutoShutdown.Margin = new System.Windows.Forms.Padding(0, 3, 5, 2);
            this.toolStripAutoShutdown.Name = "toolStripAutoShutdown";
            this.toolStripAutoShutdown.Size = new System.Drawing.Size(60, 22);
            this.toolStripAutoShutdown.Text = "自動電断";
            this.toolStripAutoShutdown.ToolTipText = "全ポートが終了または待機状態になった時、\r\nコンピュータをシャットダウンします。";
            this.toolStripAutoShutdown.MouseHover += new System.EventHandler(this.toolStripStatus_MouseHover);
            this.toolStripAutoShutdown.MouseLeave += new System.EventHandler(this.toolStripStatus_MouseLeave);
            this.toolStripAutoShutdown.Click += new System.EventHandler(this.toolStripAutoShutdown_Click);
            // 
            // toolStripEXNum
            // 
            this.toolStripEXNum.AutoSize = false;
            this.toolStripEXNum.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripEXNum.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripEXNum.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripEXNum.Name = "toolStripEXNum";
            this.toolStripEXNum.Size = new System.Drawing.Size(48, 22);
            this.toolStripEXNum.Text = "EX 000";
            this.toolStripEXNum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripEXNum.ToolTipText = "全ポート合計EX接続数";
            this.toolStripEXNum.MouseHover += new System.EventHandler(this.toolStripStatus_MouseHover);
            this.toolStripEXNum.MouseLeave += new System.EventHandler(this.toolStripStatus_MouseLeave);
            // 
            // toolStripIMNum
            // 
            this.toolStripIMNum.AutoSize = false;
            this.toolStripIMNum.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripIMNum.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripIMNum.Name = "toolStripIMNum";
            this.toolStripIMNum.Size = new System.Drawing.Size(48, 22);
            this.toolStripIMNum.Text = "IM 000";
            this.toolStripIMNum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripIMNum.ToolTipText = "全ポート合計IM接続数";
            this.toolStripIMNum.MouseHover += new System.EventHandler(this.toolStripStatus_MouseHover);
            this.toolStripIMNum.MouseLeave += new System.EventHandler(this.toolStripStatus_MouseLeave);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabel1.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(64, 22);
            this.toolStripStatusLabel1.Text = "CPU 100%";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripStatusLabel1.ToolTipText = "全CPU使用率";
            // 
            // toolStripAC
            // 
            this.toolStripAC.AutoSize = false;
            this.toolStripAC.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripAC.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripAC.Name = "toolStripAC";
            this.toolStripAC.Size = new System.Drawing.Size(90, 22);
            this.toolStripAC.Text = "Power 100%";
            this.toolStripAC.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripAC.ToolTipText = "バッテリー残量";
            this.toolStripAC.MouseHover += new System.EventHandler(this.toolStripStatus_MouseHover);
            this.toolStripAC.MouseLeave += new System.EventHandler(this.toolStripStatus_MouseLeave);
            // 
            // KickViewRClick
            // 
            this.KickViewRClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.KickAddSubMenu,
            this.KickSelDelMenu,
            this.KickSelDelClearMenu,
            this.toolStripSeparator1,
            this.KickAllDelMenu,
            this.KickAllDelClearMenu,
            this.toolStripSeparator5,
            this.KickIPCopyMenu});
            this.KickViewRClick.Name = "KickViewRClick";
            this.KickViewRClick.ShowImageMargin = false;
            this.KickViewRClick.Size = new System.Drawing.Size(184, 148);
            this.KickViewRClick.Opening += new System.ComponentModel.CancelEventHandler(this.KickViewRClick_Opening);
            // 
            // KickAddSubMenu
            // 
            this.KickAddSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.KickAdd1minMenu,
            this.KickAdd5minMenu,
            this.KickAdd10minMenu,
            this.KickAdd30minMenu,
            this.KickAdd1hourMenu,
            this.KickAddLimited});
            this.KickAddSubMenu.Name = "KickAddSubMenu";
            this.KickAddSubMenu.Size = new System.Drawing.Size(183, 22);
            this.KickAddSubMenu.Text = "キック登録";
            // 
            // KickAdd1minMenu
            // 
            this.KickAdd1minMenu.Name = "KickAdd1minMenu";
            this.KickAdd1minMenu.Size = new System.Drawing.Size(112, 22);
            this.KickAdd1minMenu.Tag = "60";
            this.KickAdd1minMenu.Text = "1分";
            this.KickAdd1minMenu.Click += new System.EventHandler(this.KickAddMenu_Click);
            // 
            // KickAdd5minMenu
            // 
            this.KickAdd5minMenu.Name = "KickAdd5minMenu";
            this.KickAdd5minMenu.Size = new System.Drawing.Size(112, 22);
            this.KickAdd5minMenu.Tag = "300";
            this.KickAdd5minMenu.Text = "5分";
            this.KickAdd5minMenu.Click += new System.EventHandler(this.KickAddMenu_Click);
            // 
            // KickAdd10minMenu
            // 
            this.KickAdd10minMenu.Name = "KickAdd10minMenu";
            this.KickAdd10minMenu.Size = new System.Drawing.Size(112, 22);
            this.KickAdd10minMenu.Tag = "600";
            this.KickAdd10minMenu.Text = "10分";
            this.KickAdd10minMenu.Click += new System.EventHandler(this.KickAddMenu_Click);
            // 
            // KickAdd30minMenu
            // 
            this.KickAdd30minMenu.Name = "KickAdd30minMenu";
            this.KickAdd30minMenu.Size = new System.Drawing.Size(112, 22);
            this.KickAdd30minMenu.Tag = "1800";
            this.KickAdd30minMenu.Text = "30分";
            this.KickAdd30minMenu.Click += new System.EventHandler(this.KickAddMenu_Click);
            // 
            // KickAdd1hourMenu
            // 
            this.KickAdd1hourMenu.Name = "KickAdd1hourMenu";
            this.KickAdd1hourMenu.Size = new System.Drawing.Size(112, 22);
            this.KickAdd1hourMenu.Tag = "3600";
            this.KickAdd1hourMenu.Text = "1時間";
            this.KickAdd1hourMenu.Click += new System.EventHandler(this.KickAddMenu_Click);
            // 
            // KickAddLimited
            // 
            this.KickAddLimited.Name = "KickAddLimited";
            this.KickAddLimited.Size = new System.Drawing.Size(112, 22);
            this.KickAddLimited.Tag = "-1";
            this.KickAddLimited.Text = "無制限";
            this.KickAddLimited.Click += new System.EventHandler(this.KickAddMenu_Click);
            // 
            // KickSelDelMenu
            // 
            this.KickSelDelMenu.Name = "KickSelDelMenu";
            this.KickSelDelMenu.Size = new System.Drawing.Size(183, 22);
            this.KickSelDelMenu.Text = "キック解除";
            this.KickSelDelMenu.Click += new System.EventHandler(this.KickDelMenu_Click);
            // 
            // KickSelDelClearMenu
            // 
            this.KickSelDelClearMenu.Name = "KickSelDelClearMenu";
            this.KickSelDelClearMenu.Size = new System.Drawing.Size(183, 22);
            this.KickSelDelClearMenu.Text = "キック解除＆リスト消去";
            this.KickSelDelClearMenu.Click += new System.EventHandler(this.KickDelMenu_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(180, 6);
            // 
            // KickAllDelMenu
            // 
            this.KickAllDelMenu.Name = "KickAllDelMenu";
            this.KickAllDelMenu.Size = new System.Drawing.Size(183, 22);
            this.KickAllDelMenu.Text = "すべて解除";
            this.KickAllDelMenu.Click += new System.EventHandler(this.KickDelMenu_Click);
            // 
            // KickAllDelClearMenu
            // 
            this.KickAllDelClearMenu.Name = "KickAllDelClearMenu";
            this.KickAllDelClearMenu.Size = new System.Drawing.Size(183, 22);
            this.KickAllDelClearMenu.Text = "すべて解除＆リスト消去";
            this.KickAllDelClearMenu.Click += new System.EventHandler(this.KickDelMenu_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(180, 6);
            // 
            // KickIPCopyMenu
            // 
            this.KickIPCopyMenu.Name = "KickIPCopyMenu";
            this.KickIPCopyMenu.Size = new System.Drawing.Size(183, 22);
            this.KickIPCopyMenu.Text = "IPアドレスコピー(&C)";
            this.KickIPCopyMenu.Click += new System.EventHandler(this.IPCopyMenu_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // LogViewRClick
            // 
            this.LogViewRClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LogClearMenu,
            this.LogClearAllMenu,
            this.toolStripSeparator11,
            this.LogCopyMenuItem,
            this.LogIPCopyMenuItem});
            this.LogViewRClick.Name = "LogViewRClick";
            this.LogViewRClick.ShowImageMargin = false;
            this.LogViewRClick.Size = new System.Drawing.Size(202, 98);
            this.LogViewRClick.Opening += new System.ComponentModel.CancelEventHandler(this.LogViewRClick_Opening);
            // 
            // LogClearMenu
            // 
            this.LogClearMenu.Name = "LogClearMenu";
            this.LogClearMenu.Size = new System.Drawing.Size(201, 22);
            this.LogClearMenu.Text = "このポートのログクリア(&C)";
            this.LogClearMenu.Click += new System.EventHandler(this.LogClearMenu_Click);
            // 
            // LogClearAllMenu
            // 
            this.LogClearAllMenu.Name = "LogClearAllMenu";
            this.LogClearAllMenu.Size = new System.Drawing.Size(201, 22);
            this.LogClearAllMenu.Text = "全ポートのログクリア(&A)";
            this.LogClearAllMenu.Click += new System.EventHandler(this.LogClearAllMenu_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(198, 6);
            // 
            // LogCopyMenuItem
            // 
            this.LogCopyMenuItem.Name = "LogCopyMenuItem";
            this.LogCopyMenuItem.Size = new System.Drawing.Size(201, 22);
            this.LogCopyMenuItem.Text = "この行のログをコピー";
            this.LogCopyMenuItem.Click += new System.EventHandler(this.LogCopyMenuItem_Click);
            // 
            // LogIPCopyMenuItem
            // 
            this.LogIPCopyMenuItem.Name = "LogIPCopyMenuItem";
            this.LogIPCopyMenuItem.Size = new System.Drawing.Size(201, 22);
            this.LogIPCopyMenuItem.Text = "この行のIPをコピー";
            this.LogIPCopyMenuItem.Click += new System.EventHandler(this.LogIPCopyMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabKagami);
            this.splitContainer1.Size = new System.Drawing.Size(696, 352);
            this.splitContainer1.SplitterDistance = 190;
            this.splitContainer1.TabIndex = 9;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.kagamiView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.monAllView);
            this.splitContainer2.Size = new System.Drawing.Size(190, 352);
            this.splitContainer2.SplitterDistance = 179;
            this.splitContainer2.TabIndex = 1;
            // 
            // kagamiView
            // 
            this.kagamiView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmKgmViewPort,
            this.clmKgmViewImport,
            this.clmKgmViewConn});
            this.kagamiView.ContextMenuStrip = this.kagamiViewRClick;
            this.kagamiView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kagamiView.FullRowSelect = true;
            this.kagamiView.GridLines = true;
            this.kagamiView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.kagamiView.Location = new System.Drawing.Point(0, 0);
            this.kagamiView.MultiSelect = false;
            this.kagamiView.Name = "kagamiView";
            this.kagamiView.Size = new System.Drawing.Size(190, 179);
            this.kagamiView.TabIndex = 0;
            this.kagamiView.UseCompatibleStateImageBehavior = false;
            this.kagamiView.View = System.Windows.Forms.View.Details;
            this.kagamiView.DoubleClick += new System.EventHandler(this.kagamiView_DoubleClick);
            // 
            // clmKgmViewPort
            // 
            this.clmKgmViewPort.Text = "Port";
            this.clmKgmViewPort.Width = 32;
            // 
            // clmKgmViewImport
            // 
            this.clmKgmViewImport.Text = "Import";
            this.clmKgmViewImport.Width = 100;
            // 
            // clmKgmViewConn
            // 
            this.clmKgmViewConn.Text = "Conn";
            this.clmKgmViewConn.Width = 48;
            // 
            // kagamiViewRClick
            // 
            this.kagamiViewRClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.KgmIpCopyMenu,
            this.KgmImIpCopyMenu,
            this.KgmImPauseMenu,
            this.KgmImDiscMenu});
            this.kagamiViewRClick.Name = "kagamiViewRClick";
            this.kagamiViewRClick.ShowCheckMargin = true;
            this.kagamiViewRClick.ShowImageMargin = false;
            this.kagamiViewRClick.Size = new System.Drawing.Size(178, 92);
            this.kagamiViewRClick.Opening += new System.ComponentModel.CancelEventHandler(this.kagamiViewRClick_Opening);
            // 
            // KgmIpCopyMenu
            // 
            this.KgmIpCopyMenu.Name = "KgmIpCopyMenu";
            this.KgmIpCopyMenu.Size = new System.Drawing.Size(177, 22);
            this.KgmIpCopyMenu.Text = "鏡URLコピー";
            this.KgmIpCopyMenu.Click += new System.EventHandler(this.KgmIpCopyMenu_Click);
            // 
            // KgmImIpCopyMenu
            // 
            this.KgmImIpCopyMenu.Name = "KgmImIpCopyMenu";
            this.KgmImIpCopyMenu.Size = new System.Drawing.Size(177, 22);
            this.KgmImIpCopyMenu.Text = "ImportURLコピー";
            this.KgmImIpCopyMenu.Click += new System.EventHandler(this.KgmImIpCopyMenu_Click);
            // 
            // KgmImPauseMenu
            // 
            this.KgmImPauseMenu.Name = "KgmImPauseMenu";
            this.KgmImPauseMenu.Size = new System.Drawing.Size(177, 22);
            this.KgmImPauseMenu.Text = "接続制限";
            this.KgmImPauseMenu.Click += new System.EventHandler(this.KgmImPauseMenu_Click);
            // 
            // KgmImDiscMenu
            // 
            this.KgmImDiscMenu.Name = "KgmImDiscMenu";
            this.KgmImDiscMenu.Size = new System.Drawing.Size(177, 22);
            this.KgmImDiscMenu.Text = "強制解放";
            this.KgmImDiscMenu.Click += new System.EventHandler(this.KgmImDiscMenu_Click);
            // 
            // monAllView
            // 
            this.monAllView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmMonAllView1,
            this.clmMonAllView2});
            this.monAllView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.monAllView.FullRowSelect = true;
            this.monAllView.GridLines = true;
            this.monAllView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.monAllView.Location = new System.Drawing.Point(0, 0);
            this.monAllView.MultiSelect = false;
            this.monAllView.Name = "monAllView";
            this.monAllView.Size = new System.Drawing.Size(190, 169);
            this.monAllView.TabIndex = 0;
            this.monAllView.UseCompatibleStateImageBehavior = false;
            this.monAllView.View = System.Windows.Forms.View.Details;
            this.monAllView.DoubleClick += new System.EventHandler(this.monAllView_DoubleClick);
            // 
            // clmMonAllView1
            // 
            this.clmMonAllView1.Text = "項目";
            this.clmMonAllView1.Width = 79;
            // 
            // clmMonAllView2
            // 
            this.clmMonAllView2.Text = "値";
            this.clmMonAllView2.Width = 101;
            // 
            // tabKagami
            // 
            this.tabKagami.Controls.Add(this.tabPageKgm1);
            this.tabKagami.Controls.Add(this.tabPageKgm2);
            this.tabKagami.Controls.Add(this.tabPageKgm3);
            this.tabKagami.Controls.Add(this.tabPageKgm4);
            this.tabKagami.Controls.Add(this.tabPageKgm5);
            this.tabKagami.Controls.Add(this.tabPage1);
            this.tabKagami.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabKagami.Location = new System.Drawing.Point(0, 0);
            this.tabKagami.Name = "tabKagami";
            this.tabKagami.SelectedIndex = 0;
            this.tabKagami.Size = new System.Drawing.Size(502, 352);
            this.tabKagami.TabIndex = 0;
            this.tabKagami.SelectedIndexChanged += new System.EventHandler(this.tabKagami_SelectedIndexChanged);
            // 
            // tabPageKgm1
            // 
            this.tabPageKgm1.Controls.Add(this.label8);
            this.tabPageKgm1.Controls.Add(this.bndStopUnit);
            this.tabPageKgm1.Controls.Add(this.bndStopNum);
            this.tabPageKgm1.Controls.Add(this.bndStopLabel);
            this.tabPageKgm1.Controls.Add(this.monView);
            this.tabPageKgm1.Location = new System.Drawing.Point(4, 22);
            this.tabPageKgm1.Name = "tabPageKgm1";
            this.tabPageKgm1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageKgm1.Size = new System.Drawing.Size(483, 326);
            this.tabPageKgm1.TabIndex = 0;
            this.tabPageKgm1.Text = "モニタ";
            this.tabPageKgm1.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(0, 7);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 22);
            this.label8.TabIndex = 0;
            this.label8.Text = "帯域制限：";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bndStopUnit
            // 
            this.bndStopUnit.AutoSize = true;
            this.bndStopUnit.Location = new System.Drawing.Point(242, 11);
            this.bndStopUnit.Name = "bndStopUnit";
            this.bndStopUnit.Size = new System.Drawing.Size(31, 13);
            this.bndStopUnit.TabIndex = 3;
            this.bndStopUnit.Text = "KB/s";
            // 
            // bndStopNum
            // 
            this.bndStopNum.Location = new System.Drawing.Point(174, 8);
            this.bndStopNum.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.bndStopNum.Name = "bndStopNum";
            this.bndStopNum.Size = new System.Drawing.Size(65, 20);
            this.bndStopNum.TabIndex = 2;
            this.bndStopNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.bndStopNum.Value = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.bndStopNum.ValueChanged += new System.EventHandler(this.bndStopNum_ValueChanged);
            this.bndStopNum.KeyUp += new System.Windows.Forms.KeyEventHandler(this.bndStopNum_KeyUp);
            // 
            // bndStopLabel
            // 
            this.bndStopLabel.AutoSize = true;
            this.bndStopLabel.Location = new System.Drawing.Point(66, 11);
            this.bndStopLabel.Name = "bndStopLabel";
            this.bndStopLabel.Size = new System.Drawing.Size(104, 13);
            this.bndStopLabel.TabIndex = 1;
            this.bndStopLabel.Text = "ポート毎に個別設定";
            // 
            // monView
            // 
            this.monView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.monView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmMonView1,
            this.clmMonView2});
            this.monView.FullRowSelect = true;
            this.monView.GridLines = true;
            this.monView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.monView.Location = new System.Drawing.Point(3, 34);
            this.monView.MultiSelect = false;
            this.monView.Name = "monView";
            this.monView.Size = new System.Drawing.Size(481, 288);
            this.monView.TabIndex = 4;
            this.monView.UseCompatibleStateImageBehavior = false;
            this.monView.View = System.Windows.Forms.View.Details;
            this.monView.DoubleClick += new System.EventHandler(this.monView_DoubleClick);
            // 
            // clmMonView1
            // 
            this.clmMonView1.Text = "項目";
            this.clmMonView1.Width = 77;
            // 
            // clmMonView2
            // 
            this.clmMonView2.Text = "値";
            this.clmMonView2.Width = 163;
            // 
            // tabPageKgm2
            // 
            this.tabPageKgm2.Controls.Add(this.clientView);
            this.tabPageKgm2.Location = new System.Drawing.Point(4, 22);
            this.tabPageKgm2.Name = "tabPageKgm2";
            this.tabPageKgm2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageKgm2.Size = new System.Drawing.Size(483, 326);
            this.tabPageKgm2.TabIndex = 1;
            this.tabPageKgm2.Text = "クライアント";
            this.tabPageKgm2.UseVisualStyleBackColor = true;
            // 
            // clientView
            // 
            this.clientView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmClientViewID,
            this.clmClientViewBuffer,
            this.clmClientViewIP,
            this.clmClientViewUA,
            this.clmClientViewTime,
            this.columnViewHost,
            this.clmClientViewConnInfo});
            this.clientView.ContextMenuStrip = this.ClientViewRClick;
            this.clientView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clientView.FullRowSelect = true;
            this.clientView.GridLines = true;
            this.clientView.Location = new System.Drawing.Point(3, 3);
            this.clientView.Name = "clientView";
            this.clientView.Size = new System.Drawing.Size(477, 320);
            this.clientView.TabIndex = 0;
            this.clientView.UseCompatibleStateImageBehavior = false;
            this.clientView.View = System.Windows.Forms.View.Details;
            this.clientView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.clientView_ColumnClick);
            // 
            // clmClientViewID
            // 
            this.clmClientViewID.Text = "ID";
            this.clmClientViewID.Width = 26;
            // 
            // clmClientViewBuffer
            // 
            this.clmClientViewBuffer.Text = "Buffer";
            this.clmClientViewBuffer.Width = 41;
            // 
            // clmClientViewIP
            // 
            this.clmClientViewIP.Text = "IP";
            this.clmClientViewIP.Width = 90;
            // 
            // clmClientViewUA
            // 
            this.clmClientViewUA.Text = "UserAgent";
            this.clmClientViewUA.Width = 137;
            // 
            // clmClientViewTime
            // 
            this.clmClientViewTime.Text = "Time";
            this.clmClientViewTime.Width = 52;
            // 
            // columnViewHost
            // 
            this.columnViewHost.Text = "リモートホスト";
            this.columnViewHost.Width = 151;
            // 
            // clmClientViewConnInfo
            // 
            this.clmClientViewConnInfo.Text = "鏡状態";
            this.clmClientViewConnInfo.Width = 49;
            // 
            // tabPageKgm3
            // 
            this.tabPageKgm3.Controls.Add(this.label1);
            this.tabPageKgm3.Controls.Add(this.addResvBTN);
            this.tabPageKgm3.Controls.Add(this.addResvHost);
            this.tabPageKgm3.Controls.Add(this.reserveView);
            this.tabPageKgm3.Location = new System.Drawing.Point(4, 22);
            this.tabPageKgm3.Name = "tabPageKgm3";
            this.tabPageKgm3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageKgm3.Size = new System.Drawing.Size(494, 326);
            this.tabPageKgm3.TabIndex = 2;
            this.tabPageKgm3.Text = "リザーブ";
            this.tabPageKgm3.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(1, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "リザーブ追加：";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // addResvBTN
            // 
            this.addResvBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addResvBTN.Location = new System.Drawing.Point(441, 7);
            this.addResvBTN.Name = "addResvBTN";
            this.addResvBTN.Size = new System.Drawing.Size(50, 22);
            this.addResvBTN.TabIndex = 2;
            this.addResvBTN.Text = "追加";
            this.addResvBTN.UseVisualStyleBackColor = true;
            this.addResvBTN.Click += new System.EventHandler(this.addResvBTN_Click);
            // 
            // reserveView
            // 
            this.reserveView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.reserveView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmResvViewIP,
            this.clmResvViewStatus});
            this.reserveView.ContextMenuStrip = this.ReserveViewRClick;
            this.reserveView.FullRowSelect = true;
            this.reserveView.GridLines = true;
            this.reserveView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.reserveView.Location = new System.Drawing.Point(3, 34);
            this.reserveView.Name = "reserveView";
            this.reserveView.Size = new System.Drawing.Size(495, 288);
            this.reserveView.TabIndex = 3;
            this.reserveView.UseCompatibleStateImageBehavior = false;
            this.reserveView.View = System.Windows.Forms.View.Details;
            // 
            // clmResvViewIP
            // 
            this.clmResvViewIP.Text = "IP";
            this.clmResvViewIP.Width = 90;
            // 
            // clmResvViewStatus
            // 
            this.clmResvViewStatus.Text = "状態";
            this.clmResvViewStatus.Width = 48;
            // 
            // tabPageKgm4
            // 
            this.tabPageKgm4.Controls.Add(this.label7);
            this.tabPageKgm4.Controls.Add(this.addKickBTN);
            this.tabPageKgm4.Controls.Add(this.addKickHost);
            this.tabPageKgm4.Controls.Add(this.kickView);
            this.tabPageKgm4.Location = new System.Drawing.Point(4, 22);
            this.tabPageKgm4.Name = "tabPageKgm4";
            this.tabPageKgm4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageKgm4.Size = new System.Drawing.Size(494, 326);
            this.tabPageKgm4.TabIndex = 3;
            this.tabPageKgm4.Text = "キック";
            this.tabPageKgm4.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(3, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 22);
            this.label7.TabIndex = 0;
            this.label7.Text = "キック追加：";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // addKickBTN
            // 
            this.addKickBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addKickBTN.Location = new System.Drawing.Point(441, 7);
            this.addKickBTN.Name = "addKickBTN";
            this.addKickBTN.Size = new System.Drawing.Size(50, 22);
            this.addKickBTN.TabIndex = 2;
            this.addKickBTN.Text = "追加";
            this.addKickBTN.UseVisualStyleBackColor = true;
            this.addKickBTN.Click += new System.EventHandler(this.addKickBTN_Click);
            // 
            // kickView
            // 
            this.kickView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.kickView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmKickViewIP,
            this.clmKickViewStatus,
            this.clmKickViewCount});
            this.kickView.ContextMenuStrip = this.KickViewRClick;
            this.kickView.FullRowSelect = true;
            this.kickView.GridLines = true;
            this.kickView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.kickView.Location = new System.Drawing.Point(3, 34);
            this.kickView.Name = "kickView";
            this.kickView.Size = new System.Drawing.Size(495, 288);
            this.kickView.TabIndex = 3;
            this.kickView.UseCompatibleStateImageBehavior = false;
            this.kickView.View = System.Windows.Forms.View.Details;
            // 
            // clmKickViewIP
            // 
            this.clmKickViewIP.Text = "IP";
            this.clmKickViewIP.Width = 90;
            // 
            // clmKickViewStatus
            // 
            this.clmKickViewStatus.Text = "状態";
            this.clmKickViewStatus.Width = 168;
            // 
            // clmKickViewCount
            // 
            this.clmKickViewCount.Text = "弾いた回数";
            this.clmKickViewCount.Width = 70;
            // 
            // tabPageKgm5
            // 
            this.tabPageKgm5.Controls.Add(this.logView);
            this.tabPageKgm5.Controls.Add(this.logAutoScroll);
            this.tabPageKgm5.Controls.Add(this.logModeImp);
            this.tabPageKgm5.Controls.Add(this.logModeAll);
            this.tabPageKgm5.Location = new System.Drawing.Point(4, 22);
            this.tabPageKgm5.Name = "tabPageKgm5";
            this.tabPageKgm5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageKgm5.Size = new System.Drawing.Size(494, 326);
            this.tabPageKgm5.TabIndex = 5;
            this.tabPageKgm5.Text = "ログ";
            this.tabPageKgm5.UseVisualStyleBackColor = true;
            // 
            // logView
            // 
            this.logView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.logView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmLogView1,
            this.clmLogView2});
            this.logView.ContextMenuStrip = this.LogViewRClick;
            this.logView.FullRowSelect = true;
            this.logView.GridLines = true;
            this.logView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.logView.Location = new System.Drawing.Point(3, 34);
            this.logView.MultiSelect = false;
            this.logView.Name = "logView";
            this.logView.Size = new System.Drawing.Size(495, 288);
            this.logView.TabIndex = 3;
            this.logView.UseCompatibleStateImageBehavior = false;
            this.logView.View = System.Windows.Forms.View.Details;
            // 
            // clmLogView1
            // 
            this.clmLogView1.Text = "時刻";
            this.clmLogView1.Width = 82;
            // 
            // clmLogView2
            // 
            this.clmLogView2.Text = "メッセージ";
            this.clmLogView2.Width = 250;
            // 
            // logAutoScroll
            // 
            this.logAutoScroll.Appearance = System.Windows.Forms.Appearance.Button;
            this.logAutoScroll.Checked = true;
            this.logAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logAutoScroll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.logAutoScroll.Location = new System.Drawing.Point(243, 3);
            this.logAutoScroll.Name = "logAutoScroll";
            this.logAutoScroll.Size = new System.Drawing.Size(114, 24);
            this.logAutoScroll.TabIndex = 2;
            this.logAutoScroll.Text = "自動スクロール";
            this.logAutoScroll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.logAutoScroll.UseVisualStyleBackColor = true;
            // 
            // logModeImp
            // 
            this.logModeImp.Appearance = System.Windows.Forms.Appearance.Button;
            this.logModeImp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.logModeImp.Location = new System.Drawing.Point(123, 3);
            this.logModeImp.Name = "logModeImp";
            this.logModeImp.Size = new System.Drawing.Size(114, 24);
            this.logModeImp.TabIndex = 1;
            this.logModeImp.Tag = "";
            this.logModeImp.Text = "重要なログのみ表示";
            this.logModeImp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.logModeImp.UseVisualStyleBackColor = true;
            this.logModeImp.CheckedChanged += new System.EventHandler(this.logMode_CheckedChanged);
            // 
            // logModeAll
            // 
            this.logModeAll.Appearance = System.Windows.Forms.Appearance.Button;
            this.logModeAll.Checked = true;
            this.logModeAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.logModeAll.Location = new System.Drawing.Point(3, 3);
            this.logModeAll.Name = "logModeAll";
            this.logModeAll.Size = new System.Drawing.Size(114, 24);
            this.logModeAll.TabIndex = 0;
            this.logModeAll.TabStop = true;
            this.logModeAll.Tag = "";
            this.logModeAll.Text = "全てのログを表示";
            this.logModeAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.logModeAll.UseVisualStyleBackColor = true;
            this.logModeAll.CheckedChanged += new System.EventHandler(this.logMode_CheckedChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.PreviewWindowsMediaPlayer);
            this.tabPage1.Controls.Add(this.optnonWMPLabel);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(494, 326);
            this.tabPage1.TabIndex = 6;
            this.tabPage1.Text = "プレビュー";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // PreviewWindowsMediaPlayer
            // 
            this.PreviewWindowsMediaPlayer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PreviewWindowsMediaPlayer.Enabled = true;
            this.PreviewWindowsMediaPlayer.Location = new System.Drawing.Point(3, 3);
            this.PreviewWindowsMediaPlayer.Name = "PreviewWindowsMediaPlayer";
            this.PreviewWindowsMediaPlayer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("PreviewWindowsMediaPlayer.OcxState")));
            this.PreviewWindowsMediaPlayer.Size = new System.Drawing.Size(488, 320);
            this.PreviewWindowsMediaPlayer.TabIndex = 0;
            // 
            // optnonWMPLabel
            // 
            this.optnonWMPLabel.AutoSize = true;
            this.optnonWMPLabel.Location = new System.Drawing.Point(6, 13);
            this.optnonWMPLabel.Name = "optnonWMPLabel";
            this.optnonWMPLabel.Size = new System.Drawing.Size(130, 13);
            this.optnonWMPLabel.TabIndex = 1;
            this.optnonWMPLabel.Text = "設定が無効になっています";
            // 
            // MonViewRClick
            // 
            this.MonViewRClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MonUnitChgMenu,
            this.MonImIpCopyMenu,
            this.MonStoppingKagamiCMenu,
            this.MonModeChgMenu});
            this.MonViewRClick.Name = "MonViewRClick";
            this.MonViewRClick.Size = new System.Drawing.Size(195, 92);
            // 
            // MonUnitChgMenu
            // 
            this.MonUnitChgMenu.Name = "MonUnitChgMenu";
            this.MonUnitChgMenu.Size = new System.Drawing.Size(194, 22);
            this.MonUnitChgMenu.Text = "kbps⇔KB/s 切り替え";
            this.MonUnitChgMenu.Click += new System.EventHandler(this.MonUnitChgMenu_Click);
            // 
            // MonImIpCopyMenu
            // 
            this.MonImIpCopyMenu.Name = "MonImIpCopyMenu";
            this.MonImIpCopyMenu.Size = new System.Drawing.Size(194, 22);
            this.MonImIpCopyMenu.Text = "IM設定者IPをコピー";
            this.MonImIpCopyMenu.Click += new System.EventHandler(this.MonImIpCopyMenu_Click);
            // 
            // MonStoppingKagamiCMenu
            // 
            this.MonStoppingKagamiCMenu.Name = "MonStoppingKagamiCMenu";
            this.MonStoppingKagamiCMenu.Size = new System.Drawing.Size(194, 22);
            this.MonStoppingKagamiCMenu.Text = "鏡終了後停止";
            this.MonStoppingKagamiCMenu.Click += new System.EventHandler(this.MonStoppingKagamiCMenu_Click);
            // 
            // MonModeChgMenu
            // 
            this.MonModeChgMenu.Name = "MonModeChgMenu";
            this.MonModeChgMenu.Size = new System.Drawing.Size(194, 22);
            this.MonModeChgMenu.Text = "Push配信専用";
            this.MonModeChgMenu.Click += new System.EventHandler(this.MonModeChgMenu_Click);
            // 
            // OptionClick
            // 
            this.OptionClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CopyMyIPMenuItem,
            this.MyDDNSCopyMenuItem,
            this.toolStripSeparator8,
            this.ExportAuthMenuItem});
            this.OptionClick.Name = "OptionClick";
            this.OptionClick.Size = new System.Drawing.Size(385, 76);
            // 
            // CopyMyIPMenuItem
            // 
            this.CopyMyIPMenuItem.Name = "CopyMyIPMenuItem";
            this.CopyMyIPMenuItem.Size = new System.Drawing.Size(384, 22);
            this.CopyMyIPMenuItem.Text = "http://自グローバルIP:ポートをクリップボードへコピー";
            this.CopyMyIPMenuItem.Click += new System.EventHandler(this.CopyMyIPMenuItem_Click);
            // 
            // MyDDNSCopyMenuItem
            // 
            this.MyDDNSCopyMenuItem.Name = "MyDDNSCopyMenuItem";
            this.MyDDNSCopyMenuItem.Size = new System.Drawing.Size(384, 22);
            this.MyDDNSCopyMenuItem.Text = "http://自DDNS:ポートをクリップボードへコピー";
            this.MyDDNSCopyMenuItem.Click += new System.EventHandler(this.MyDDNSCopyMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(381, 6);
            // 
            // ExportAuthMenuItem
            // 
            this.ExportAuthMenuItem.Name = "ExportAuthMenuItem";
            this.ExportAuthMenuItem.Size = new System.Drawing.Size(384, 22);
            this.ExportAuthMenuItem.Text = "エクスポート認証";
            this.ExportAuthMenuItem.Click += new System.EventHandler(this.ExportAuthMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 404);
            this.Controls.Add(this.optBTN);
            this.Controls.Add(this.copyMyUrlBTN);
            this.Controls.Add(this.clearBTN);
            this.Controls.Add(this.importURL);
            this.Controls.Add(this.discBTN);
            this.Controls.Add(this.myPort);
            this.Controls.Add(this.connBTN);
            this.Controls.Add(this.connNum);
            this.Controls.Add(this.resvNum);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "Kagamiｎ2/x.x.x";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResizeBegin += new System.EventHandler(this.Form1_ResizeBegin);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            this.TaskTrayMenu.ResumeLayout(false);
            this.ReserveViewRClick.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.resvNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.connNum)).EndInit();
            this.ImportUrlRClick.ResumeLayout(false);
            this.ClientViewRClick.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.KickViewRClick.ResumeLayout(false);
            this.LogViewRClick.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.kagamiViewRClick.ResumeLayout(false);
            this.tabKagami.ResumeLayout(false);
            this.tabPageKgm1.ResumeLayout(false);
            this.tabPageKgm1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bndStopNum)).EndInit();
            this.tabPageKgm2.ResumeLayout(false);
            this.tabPageKgm3.ResumeLayout(false);
            this.tabPageKgm3.PerformLayout();
            this.tabPageKgm4.ResumeLayout(false);
            this.tabPageKgm4.PerformLayout();
            this.tabPageKgm5.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewWindowsMediaPlayer)).EndInit();
            this.MonViewRClick.ResumeLayout(false);
            this.OptionClick.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon TaskTrayIcon;
        private System.Windows.Forms.ContextMenuStrip TaskTrayMenu;
        private System.Windows.Forms.ToolStripMenuItem TrayMenuExit;
        private System.Windows.Forms.ContextMenuStrip ReserveViewRClick;
        private System.Windows.Forms.ToolStripMenuItem ResvDelMenu;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip ClientViewRClick;
        private System.Windows.Forms.ToolStripMenuItem ClientDiscMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem ClientKickDefMenu;
        private System.Windows.Forms.ToolStripMenuItem ClientKickSubMenu;
        private System.Windows.Forms.ToolStripMenuItem ClientKick1minMenu;
        private System.Windows.Forms.ToolStripMenuItem ClientKick5minMenu;
        private System.Windows.Forms.ToolStripMenuItem ClientKick10minMenu;
        private System.Windows.Forms.ToolStripMenuItem ClientKick30minMenu;
        private System.Windows.Forms.ToolStripMenuItem ClientKick1hourMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ClientAddResvMenu;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ContextMenuStrip KickViewRClick;
        private System.Windows.Forms.ToolStripMenuItem KickAddSubMenu;
        private System.Windows.Forms.ToolStripMenuItem KickAdd1minMenu;
        private System.Windows.Forms.ToolStripMenuItem KickAdd5minMenu;
        private System.Windows.Forms.ToolStripMenuItem KickAdd10minMenu;
        private System.Windows.Forms.ToolStripMenuItem KickAdd30minMenu;
        private System.Windows.Forms.ToolStripMenuItem KickAdd1hourMenu;
        private System.Windows.Forms.ToolStripMenuItem KickSelDelMenu;
        private System.Windows.Forms.ToolStripMenuItem KickSelDelClearMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem KickAllDelMenu;
        private System.Windows.Forms.ToolStripMenuItem KickAllDelClearMenu;
        private System.Windows.Forms.ToolStripStatusLabel toolStripIMNum;
        private System.Windows.Forms.ToolStripStatusLabel toolStripAC;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripLeftPanelOnOff;
        private System.Windows.Forms.ToolStripStatusLabel toolStripAutoExit;
        private System.Windows.Forms.ToolStripStatusLabel toolStripAutoShutdown;
        private System.Windows.Forms.ContextMenuStrip LogViewRClick;
        private System.Windows.Forms.ToolStripMenuItem LogClearMenu;
        private System.Windows.Forms.ToolStripMenuItem LogClearAllMenu;
        private System.Windows.Forms.ToolStripMenuItem RstWndPos;
        private System.Windows.Forms.ToolStripMenuItem ResvIPCopyMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem ClientIPCopyMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem KickIPCopyMenu;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView kagamiView;
        private System.Windows.Forms.ColumnHeader clmKgmViewPort;
        private System.Windows.Forms.ColumnHeader clmKgmViewImport;
        private System.Windows.Forms.ColumnHeader clmKgmViewConn;
        private System.Windows.Forms.ListView monAllView;
        private System.Windows.Forms.ColumnHeader clmMonAllView1;
        private System.Windows.Forms.ColumnHeader clmMonAllView2;
        private System.Windows.Forms.Button copyMyUrlBTN;
        private System.Windows.Forms.Button clearBTN;
        private System.Windows.Forms.Button discBTN;
        private System.Windows.Forms.Button connBTN;
        private System.Windows.Forms.NumericUpDown resvNum;
        private System.Windows.Forms.NumericUpDown connNum;
        private System.Windows.Forms.ComboBox myPort;
        private System.Windows.Forms.TextBox importURL;
        private System.Windows.Forms.TabControl tabKagami;
        private System.Windows.Forms.TabPage tabPageKgm1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label bndStopUnit;
        private System.Windows.Forms.NumericUpDown bndStopNum;
        private System.Windows.Forms.Label bndStopLabel;
        private System.Windows.Forms.ListView monView;
        private System.Windows.Forms.ColumnHeader clmMonView1;
        private System.Windows.Forms.ColumnHeader clmMonView2;
        private System.Windows.Forms.TabPage tabPageKgm2;
        private System.Windows.Forms.ListView clientView;
        private System.Windows.Forms.ColumnHeader clmClientViewID;
        private System.Windows.Forms.ColumnHeader clmClientViewIP;
        private System.Windows.Forms.ColumnHeader clmClientViewUA;
        private System.Windows.Forms.ColumnHeader clmClientViewTime;
        private System.Windows.Forms.TabPage tabPageKgm3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button addResvBTN;
        private System.Windows.Forms.TextBox addResvHost;
        private System.Windows.Forms.ListView reserveView;
        private System.Windows.Forms.ColumnHeader clmResvViewIP;
        private System.Windows.Forms.ColumnHeader clmResvViewStatus;
        private System.Windows.Forms.TabPage tabPageKgm4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button addKickBTN;
        private System.Windows.Forms.TextBox addKickHost;
        private System.Windows.Forms.ListView kickView;
        private System.Windows.Forms.ColumnHeader clmKickViewIP;
        private System.Windows.Forms.ColumnHeader clmKickViewStatus;
        private System.Windows.Forms.ColumnHeader clmKickViewCount;
        private System.Windows.Forms.TabPage tabPageKgm5;
        private System.Windows.Forms.ListView logView;
        private System.Windows.Forms.ColumnHeader clmLogView1;
        private System.Windows.Forms.ColumnHeader clmLogView2;
        private System.Windows.Forms.CheckBox logAutoScroll;
        private System.Windows.Forms.RadioButton logModeImp;
        private System.Windows.Forms.RadioButton logModeAll;
        private System.Windows.Forms.Button optBTN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripHPStart;
        private System.Windows.Forms.ToolStripStatusLabel toolStripHPPause;
        private System.Windows.Forms.ToolStripStatusLabel toolStripBandStart;
        private System.Windows.Forms.ToolStripStatusLabel toolStripEXNum;
        private System.Windows.Forms.ContextMenuStrip ImportUrlRClick;
        private System.Windows.Forms.ToolStripMenuItem AddFavoriteMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem ImportUrlCutMenu;
        private System.Windows.Forms.ToolStripMenuItem ImportUrlCopyMenu;
        private System.Windows.Forms.ToolStripMenuItem ImportUrlPasteMenu;
        private System.Windows.Forms.ToolStripMenuItem ImportUrlEraseMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ColumnHeader columnViewHost;
        private System.Windows.Forms.ToolStripMenuItem KickAddLimited;
        private System.Windows.Forms.ToolStripMenuItem ClientKickLimited;
        private System.Windows.Forms.TabPage tabPage1;
        private AxWMPLib.AxWindowsMediaPlayer PreviewWindowsMediaPlayer;
        private System.Windows.Forms.Label optnonWMPLabel;
        private System.Windows.Forms.ContextMenuStrip MonViewRClick;
        private System.Windows.Forms.ToolStripMenuItem MonUnitChgMenu;
        private System.Windows.Forms.ToolStripMenuItem MonImIpCopyMenu;
        private System.Windows.Forms.ToolStripMenuItem MonStoppingKagamiCMenu;
        private System.Windows.Forms.ToolStripMenuItem MonModeChgMenu;
        private System.Windows.Forms.ContextMenuStrip kagamiViewRClick;
        private System.Windows.Forms.ToolStripMenuItem KgmIpCopyMenu;
        private System.Windows.Forms.ToolStripMenuItem KgmImIpCopyMenu;
        private System.Windows.Forms.ToolStripMenuItem KgmImPauseMenu;
        private System.Windows.Forms.ToolStripMenuItem KgmImDiscMenu;
        private System.Windows.Forms.ContextMenuStrip OptionClick;
        private System.Windows.Forms.ToolStripMenuItem CopyMyIPMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MyDDNSCopyMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem ExportAuthMenuItem;
        private System.Windows.Forms.ColumnHeader clmClientViewBuffer;
        private System.Windows.Forms.ColumnHeader clmClientViewConnInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem FavReserveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddReserveMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem DelReserveMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem LogCopyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem LogIPCopyMenuItem;

    }
}

