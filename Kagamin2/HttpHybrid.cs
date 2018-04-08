using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using HybridDSP.Net.HTTP;
using System.IO;
using System.Text.RegularExpressions;

using System.Web;
using System.Net;

namespace Kagamin2
{
    public class HttpHybrid
    {
        int Port;

        HTTPServer server;

        #region コンストラクタ＆HTTPサーバ起動・停止メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HttpHybrid()
        {
        }

        /// <summary>
        /// 鏡置き場エントランス起動
        /// </summary>
        /// <param name="_port"></param>
        /// <returns></returns>
        public bool Start(int _port)
        {
            Port = _port;
            RequestHandlerFactory factory = new RequestHandlerFactory();
            try
            {
                Front.GetGlobalIP();
            }
            catch(Exception e)
            {
                MessageBox.Show("GlobalIPが取得できませんでした。\r\nネットワーク接続を確認してください。\r\nCause:" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                server = new HTTPServer(factory, Port);
                server.Start();
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show("開始できませんでした。\r\n未使用ポートか確認してください。\r\nCause:" + e.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 鏡置き場エントランス停止
        /// </summary>
        public void Stop()
        {
            try
            {
                if(server!=null)
                    server.Stop();
            }
            catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        public interface IHTTPRequestHandler
        {
            void HandleRequest(HTTPServerRequest request, HTTPServerResponse response);
        }

        /// <summary>
        /// 
        /// </summary>
        public interface IHTTPRequestHandlerFactory
        {
            IHTTPRequestHandler CreateRequestHandler(HTTPServerRequest request);
        }
        #endregion
    }


    class DateTimeHandler : IHTTPRequestHandler
    {

        /// <summary>
        /// テンプレートヘッダ
        /// </summary>
        List<string> TemplateMain = new List<string>();
        List<string> TemplateConn = new List<string>();
        List<string> TemplateDis = new List<string>();
        List<string> TemplateFrame = new List<string>();
        List<string> TemplateOk = new List<string>();
        List<string> TemplateAuth = new List<string>();
        List<string> TemplateSet = new List<string>();
        List<string> TemplateAdminAuth = new List<string>();
        List<string> TemplateAdminMain = new List<string>();
        List<string> TemplateAdminOk = new List<string>();
        List<string> TemplateCss = new List<string>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DateTimeHandler()
        {
            #region テンプレート読み込み
            try
            {
                StreamReader sr = new StreamReader(@"html/template_main.html", Encoding.UTF8);
                TemplateMain.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateMain.Add(sr.ReadLine());
                }
                sr.Close();

                sr = new StreamReader(@"html/template_conn.html", Encoding.UTF8);
                TemplateConn.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateConn.Add(sr.ReadLine());
                }
                sr.Close();

                sr = new StreamReader(@"html/template_dis.html", Encoding.UTF8);
                TemplateDis.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateDis.Add(sr.ReadLine());
                }
                sr.Close();

                sr = new StreamReader(@"html/template_frame.html", Encoding.UTF8);
                TemplateFrame.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateFrame.Add(sr.ReadLine());
                }
                sr.Close();

                sr = new StreamReader(@"html/template_ok.html", Encoding.UTF8);
                TemplateOk.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateOk.Add(sr.ReadLine());
                }
                sr.Close();

                sr = new StreamReader(@"html/template_auth.html", Encoding.UTF8);
                TemplateAuth.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateAuth.Add(sr.ReadLine());
                }
                sr.Close();

                sr = new StreamReader(@"html/template_set.html", Encoding.UTF8);
                TemplateSet.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateSet.Add(sr.ReadLine());
                }
                sr.Close();

                sr = new StreamReader(@"html/template_admin_auth.html", Encoding.UTF8);
                TemplateAdminAuth.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateAdminAuth.Add(sr.ReadLine());
                }
                sr.Close();

                sr = new StreamReader(@"html/template_admin_main.html", Encoding.UTF8);
                TemplateAdminMain.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateAdminMain.Add(sr.ReadLine());
                }
                sr.Close();

                sr = new StreamReader(@"html/template_admin_ok.html", Encoding.UTF8);
                TemplateAdminOk.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateAdminOk.Add(sr.ReadLine());
                }
                sr.Close();
            }
            catch (FileNotFoundException fe)
            {
                MessageBox.Show("テンプレートファイルが見つかりません\r\nFILE:" + fe.FileName, "Error", MessageBoxButtons.OK);
                Application.Exit();
            }
            catch
            {
                MessageBox.Show("テンプレート読み込みに失敗しました", "Error", MessageBoxButtons.OK);
                Application.Exit();
            }

            // 独自Template
            // 無くても問題ない。
            try
            {
                StreamReader sr = new StreamReader(@"html/template_kagamin.css", Encoding.UTF8);
                TemplateCss.Clear();
                while (sr.Peek() > -1)
                {
                    TemplateCss.Add(sr.ReadLine());
                }
                sr.Close();
            }
            catch
            {
                TemplateCss.Clear();
            }

            #endregion
        }

        /// <summary>
        /// エントランスへのアクセス処理
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public void HandleRequest(HTTPServerRequest request, HTTPServerResponse response)
        {
            int resp = 200; // http応答ステータス(loging用)
            response.KeepAlive = false;
            // ネストが深すぎ＆エラー処理が見づらいので見やすく書き直し
            // アクセス制限チェック
            bool _deny = false;
            try
            {
                string _resolve = Dns.GetHostEntry(request.RemoteAddr).HostName.ToLower();
                foreach (string _deny_host in Front.Acl.HpDenyRemoteHost)
                {
                    if (_deny_host == "")
                       continue;
                    if (_resolve.StartsWith(_deny_host.ToLower()) ||
                        _resolve.EndsWith(_deny_host.ToLower()))
                        _deny = true;
                }
            }
            catch { } // DNS逆引き失敗したら透過

            if (_deny)
            {
                #region アクセス制限中のリモートIPからの接続 403 Forbidden
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_FORBIDDEN;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        tw.WriteLine("<html><head><title>403 Forbidden</title></head>\r\n" +
                            "<body><h1>403 Forbidden</h1></body></html>");
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "Forbidden内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                resp = 403;
                #endregion
            }
            else if (request.URI == "/" || request.URI.StartsWith("/index.html"))
            {
                #region /,/index.htmlへのリクエスト
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        // 転送量計算
                        string _ul_day, _dl_day, _ul_mon, _dl_mon;
                        _ul_day = ((ulong)(Front.Log.TrsUpDay + Front.TotalUP)).ToString("#,##0,,MB");
                        _dl_day = ((ulong)(Front.Log.TrsDlDay + Front.TotalDL)).ToString("#,##0,,MB");
                        _ul_mon = ((ulong)(Front.Log.TrsUpMon + Front.TotalUP)).ToString("#,##0,,MB");
                        _dl_mon = ((ulong)(Front.Log.TrsDlMon + Front.TotalDL)).ToString("#,##0,,MB");

                        foreach (string s in TemplateFrame)
                            tw.WriteLine(s
                                .Replace("<GlobalIP>", Front.GlobalIP)
                                .Replace("<VERSION>", Front.AppName)
                                .Replace("<TRF_UP_DAY>", _ul_day)
                                .Replace("<TRF_DL_DAY>", _dl_day)
                                .Replace("<TRF_UP_MON>", _ul_mon)
                                .Replace("<TRF_DL_MON>", _dl_mon)
                                );
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "index.html内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/main.html"))
            {
                #region /main.htmlへのリクエスト
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    int i = 0;
                    int count = 1;

                    string str_wai = "";
                    string str_try = "";
                    string str_con = "";
                    string str_tmp = "";
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        // 転送量計算
                        string _ul_day, _dl_day, _ul_mon, _dl_mon;
                        _ul_day = ((ulong)(Front.Log.TrsUpDay + Front.TotalUP)).ToString("#,##0,,MB");
                        _dl_day = ((ulong)(Front.Log.TrsDlDay + Front.TotalDL)).ToString("#,##0,,MB");
                        _ul_mon = ((ulong)(Front.Log.TrsUpMon + Front.TotalUP)).ToString("#,##0,,MB");
                        _dl_mon = ((ulong)(Front.Log.TrsDlMon + Front.TotalDL)).ToString("#,##0,,MB");

                        // <TEMPLATE>が見つかるまでループ
                        while (TemplateMain.Count > i && TemplateMain[i] != "<TEMPLATE>")
                        {
                            tw.WriteLine(TemplateMain[i]
                                .Replace("<GlobalIP>", Front.GlobalIP)
                                .Replace("<VERSION>", Front.AppName)
                                .Replace("<TRF_UP_DAY>", _ul_day)
                                .Replace("<TRF_DL_DAY>", _dl_day)
                                .Replace("<TRF_UP_MON>", _ul_mon)
                                .Replace("<TRF_DL_MON>", _dl_mon)
                                );
                            i++;
                        }

                        // <待機中表示>が見つかるまでループ
                        while (TemplateMain.Count > i && TemplateMain[i] != "<待機中表示>")
                            i++;
                        i++;
                        // </待機中表示>が見つかるまでループ
                        while (TemplateMain.Count > i && TemplateMain[i] != "</待機中表示>")
                        {
                            // 待機中の表示用タグを取得
                            str_wai += TemplateMain[i] + "\r\n";
                            i++;
                        }

                        // <接続試行中表示>が見つかるまでループ
                        while (TemplateMain.Count > i && TemplateMain[i] != "<接続試行中表示>")
                            i++;
                        i++;
                        // </接続試行中表示>が見つかるまでループ
                        while (TemplateMain.Count > i && TemplateMain[i] != "</接続試行中表示>")
                        {
                            // 接続試行中の表示用タグを取得
                            str_try += TemplateMain[i] + "\r\n";
                            i++;
                        }

                        // <接続中表示>が見つかるまでループ
                        while (TemplateMain.Count > i && TemplateMain[i] != "<接続中表示>")
                            i++;
                        i++;
                        // </接続中表示>が見つかるまでループ
                        while (TemplateMain.Count > i && TemplateMain[i] != "</接続中表示>")
                        {
                            // 接続試行中の表示用タグを取得
                            str_con += TemplateMain[i] + "\r\n";
                            i++;
                        }

                        // </TEMPLATE>が見つかるまでループ
                        while (TemplateMain.Count > i && TemplateMain[i] != "</TEMPLATE>")
                            i++;
                        i++;

                        // ポート毎ループ
                        foreach (Kagami k in Front.KagamiList)
                        {
                            // 内側接続はエントランスに表示しない
                            if (k.Status.Type == 0)
                                continue;

                            // <KAGAMI>が見つかるまでループ
                            while (TemplateMain.Count > i && TemplateMain[i] != "<KAGAMI>")
                            {
                                tw.WriteLine(TemplateMain[i]
                                    .Replace("<GlobalIP>", Front.GlobalIP)
                                    .Replace("<VERSION>", Front.AppName)
                                    .Replace("<TRF_UP_DAY>", _ul_day)
                                    .Replace("<TRF_DL_DAY>", _dl_day)
                                    .Replace("<TRF_UP_MON>", _ul_mon)
                                    .Replace("<TRF_DL_MON>", _dl_mon)
                                    );
                                i++;
                            }

                            // <KAGAMI>が見つからなかった
                            if (TemplateMain.Count <= i)
                                break;

                            i++;

                            if (k.Status.ImportURL == "待機中" && k.Status.Type != 2)
                            {//待機中
                                str_tmp = str_wai;
                            }
                            else if (!k.Status.ImportStatus || (k.Status.Type == 2 && k.Status.ImportURL == "待機中"))
                            {//Importに文字は入っているが、Importフラグがfalse
                             //または、Push配信要求の待ち受け中
                                str_tmp = str_try;
                            }
                            else
                            {//正常に稼動中
                                str_tmp = str_con;
                            }

                            // HTMLを出力
                            tw.WriteLine(
                                str_tmp
                                .Replace("<COUNT>", count.ToString())
                                .Replace("<MY_URL>", Front.Hp.IpHTTP + ":" + k.Status.MyPort.ToString())
                                .Replace("<PORT>", k.Status.MyPort.ToString())
                                .Replace("<SRC_URL>", (k.Status.UrlVisible ? k.Status.ImportURL : "設定が非表示になっています"))
                                .Replace("<CONN>", k.Status.Client.Count.ToString())
                                .Replace("<MAXCONN>", k.Status.Connection.ToString() + ((k.Status.Reserve != 0) ? "+" + k.Status.Reserve.ToString() : ""))
                                .Replace("<COMMENT>", k.Status.Comment)
                                .Replace("<PAUSE>", (Front.Pause||k.Status.Pause) ? "disabled" : "")
                                .Replace("<BANDWIDTH>", Front.BndWth.EnableBandWidth ?
                                        (k.Status.LimitUPSpeed >= 1000 ?
                                            (k.Status.LimitUPSpeed / 1000).ToString() + "Mbps" :
                                            k.Status.LimitUPSpeed.ToString() + "Kbps"
                                        ) : "-")
                                .Replace("<BITRATE>", k.Status.MaxDLSpeed.ToString() + "Kbps")
                                .Replace("<BUSYCOUNT>", k.Status.BusyCounter.ToString())
                                .Replace("<IMPORT_ERROR>", k.Status.ImportError.ToString())
                                .Replace("<EXPORT_ERROR>", k.Status.ExportError.ToString())
                                .Replace("<TIME>", k.Status.ImportTimeString)
                                .Replace("<LIVE_URL>", k.Status.Url)
                                .Replace("<LIVE_URLO>", ((k.Status.Url != "") ? "実況URL" : ""))
                                .Replace("<ERROR>", k.Status.ImportErrorContext)
                                .Replace("<RETRY_COUNT>", (k.Status.Type == 2 ? "" : k.Status.RetryCounter.ToString() + "/" + Front.Retry.OutRetryTime.ToString()))
                            );
                            count++;
                        }

                        // 必要分<KAGAMI>を置き換えた後の処理
                        while (TemplateMain.Count > i)
                        {
                            if (TemplateMain[i].IndexOf("<KAGAMI>") > -1)
                            {
                                tw.WriteLine(TemplateMain[i].Replace("<KAGAMI>", ""));
                            }
                            else
                            {
                                tw.WriteLine(TemplateMain[i]
                                    .Replace("<GlobalIP>", Front.GlobalIP)
                                    .Replace("<VERSION>", Front.AppName)
                                    .Replace("<TRF_UP_DAY>", _ul_day)
                                    .Replace("<TRF_DL_DAY>", _dl_day)
                                    .Replace("<TRF_UP_MON>", _ul_mon)
                                    .Replace("<TRF_DL_MON>", _dl_mon)
                                    );
                            }
                            i++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "main.html内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/conn.html?"))
            {
                #region /conn.htmlへのリクエスト
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        string comment = "";
                        string prm = request.URI.Replace("/conn.html?", "");
                        prm = HttpUtility.UrlDecode(prm);

                        Dictionary<string, string> dic = new Dictionary<string, string>();

                        foreach (string s in prm.Split('&'))
                            dic.Add(s.Split('=')[0], s.Split('=')[1]);

                        // だみーるーぷ
                        while (true)
                        {
                            if (dic.ContainsKey("open"))
                            {
                                #region 接続画面表示
                                int port = int.Parse(dic["open"]);
                                Kagami k = Front.IndexOf(port);
                                if (k == null)
                                {
                                    comment = "そのポートは接続可能状態ではありません";
                                    break;
                                }
                                if (dic.ContainsKey("admin"))
                                {
                                    // 管理者接続モード
                                    foreach (string s in TemplateConn)
                                    {
                                        tw.WriteLine(s
                                            .Replace("<PORT>", port.ToString())
                                            .Replace("<RESERVE>", k.Status.Reserve.ToString())
                                            .Replace("<ADMIN_PASS>", dic["admin"])
                                            .Replace("<ONLOAD>", k.Status.DisablePull ? "onload=\"push()\"" : "onload=\"pull()\"")
                                            .Replace("<ENABLE_PULL>", k.Status.DisablePull ? "disabled" : "checked=\"checked\"")
                                            .Replace("<ENABLE_PUSH>", k.Status.DisablePull ? "checked=\"checked\"" : Front.Opt.EnablePush ? "" : "disabled")
                                            .Replace("<LIVEOUT_URL>", Front.Opt.OutUrl)
                                        );
                                    }
                                }
                                else
                                {
                                    // 通常接続モード
                                    foreach (string s in TemplateConn)
                                    {
                                        tw.WriteLine(s
                                            .Replace("<PORT>", port.ToString())
                                            .Replace("<RESERVE>", k.Status.Reserve.ToString())
                                            .Replace("<ADMIN_PASS>", "")
                                            .Replace("<ONLOAD>", k.Status.DisablePull ? "onload=\"push()\"" : "onload=\"pull()\"")
                                            .Replace("<ENABLE_PULL>", k.Status.DisablePull ? "disabled" : "checked=\"checked\"")
                                            .Replace("<ENABLE_PUSH>", k.Status.DisablePull ? "checked=\"checked\"" : Front.Opt.EnablePush ? "" : "disabled")
                                            .Replace("<LIVEOUT_URL>", Front.Opt.OutUrl)
                                        );
                                    }
                                }
                                #endregion
                                break;
                            }
                            else if (dic.ContainsKey("password"))
                            {
                                #region 接続要求受信
                                if (dic["Port"].Length == 0 || dic["Port"].Length >= 6)
                                {
                                    comment = "ポートが空です";
                                    break;
                                }
                                // フォーム入力内容チェック
                                if (dic["mode"] != "pull" && dic["mode"] != "push")
                                {
                                    comment = "入力エラー";
                                    break;
                                }
                                // push配信でaddressをdisabledにするとブラウザから送られてこないので、
                                // push配信時はdic["address"]のチェックは行わない
                                if (dic["mode"] == "pull")
                                {
                                    if (dic["address"].Length == 0 ||
                                        dic["address"].IndexOf("<") != -1 ||
                                        dic["address"].IndexOf(">") != -1 ||
                                        dic["address"].IndexOf("\"") != -1)
                                    {
                                        comment = "入力エラー/不正なアドレスです";
                                        break;
                                    }
                                }

                                if (dic["comment"].IndexOf("<") != -1 ||
                                    dic["comment"].IndexOf(">") != -1 ||
                                    dic["comment"].IndexOf("\"") != -1 ||
                                    dic["comment"].IndexOf("+ADw-") != -1 ||    // UTF-7 XSS
                                    dic["comment"].IndexOf("+AD4-") != -1)      // UTF-7 XSS
                                {
                                    comment = "入力エラー/不正なコメントです";
                                    break;
                                }
                                if (dic["password"].Length == 0)
                                {
                                    comment = "入力エラー/パスワード未設定";
                                    break;
                                }

                                // 実況URLは送られてきていればチェックする
                                if (dic.ContainsKey("url") && dic["url"].Length > 0)
                                {
                                    if (!dic["url"].StartsWith(Front.Opt.OutUrl) ||
                                        dic["url"].IndexOf("<") != -1 ||
                                        dic["url"].IndexOf(">") != -1 ||
                                        dic["url"].IndexOf("\"") != -1 ||
                                        dic["url"].IndexOf("+ADw-") != -1 ||    // UTF-7 XSS
                                        dic["url"].IndexOf("+AD4-") != -1)     // UTF-7 XSS)
                                    {
                                        comment = "入力エラー/不正な実況URLです";
                                        break;
                                    }
                                }

                                //該当ポートが接続可能かチェック
                                Kagami k = Front.IndexOf(int.Parse(dic["Port"]));
                                if (k == null ||
                                    k.Status.ImportURL != "待機中" ||
                                    k.Status.Type == 2 ||
                                    ((Front.Pause || k.Status.Pause) && (!dic.ContainsKey("admin") || dic["admin"] == "" || dic["admin"] != Front.Opt.AdminPass)))
                                {
                                    comment = "そのポートは接続可能状態ではありません";
                                    break;
                                }

                                //配信種別で振り分け
                                if (dic["mode"] == "pull")
                                {
                                    // pull配信
                                    if (k.Status.DisablePull)
                                    {
                                        comment = "Pull配信無効";
                                        break;
                                    }
                                    string imp = "";
                                    //インポート先ホスト名取得
                                    Match index = Regex.Match(dic["address"], @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:");
                                    if (index.Success)
                                    {
                                        imp = dic["address"].Substring(index.Index, index.Length - 1);
                                    }
                                    else
                                    {
                                        index = Regex.Match(dic["address"], @"http:\/\/[a-z0-9A-Z._-]+:");
                                        if (index.Success)
                                        {
                                            imp = dic["address"].Substring(index.Index + 7, index.Length - 8);
                                            imp = imp.Replace("http://", "");
                                        }
                                        index = Regex.Match(dic["address"], @"mms:\/\/[a-z0-9A-Z._-]+:");
                                        if (index.Success)
                                        {
                                            imp = dic["address"].Substring(index.Index + 6, index.Length - 7);
                                            imp = imp.Replace("mms://", "");
                                        }
                                        if (imp == "")
                                        {
                                            imp = dic["address"].Split(':')[0];
                                        }
                                    }
                                    // 接続制限ホストと一致するかチェック
                                    _deny = false;
                                    foreach (string sss in Front.Acl.DenyImportURL)
                                    {
                                        if (sss == "")
                                            continue;
                                        if (imp.IndexOf(sss) > -1)
                                            _deny = true;
                                    }
                                    if (_deny)
                                    {
                                        comment = "接続制限/そのホストへの接続は禁止されています";
                                        break;
                                    }
                                    // 同一ホスト接続上限数を超えていないかチェック
                                    int count = 0;
                                    lock (Front.KagamiList)
                                    {
                                        foreach (Kagami kk in Front.KagamiList)
                                        {
                                            if (kk.Status.ImportHost.IndexOf(imp) > -1)
                                            {
                                                count++;
                                            }
                                        }
                                    }
                                    if (Front.Acl.LimitSameImportURL > 0 && count >= Front.Acl.LimitSameImportURL)
                                    {
                                        comment = "接続制限/そのホストへの接続数が制限値を超えています";
                                        break;
                                    }
                                    // 設定者IPと一致するかチェック
                                    if (Front.Acl.SetUserIpCheck)
                                    {
                                        // 設定者がローカルアドレスなら一律許容
                                        if (request.RemoteAddr.StartsWith("10.") ||         // ClassA
                                            request.RemoteAddr.StartsWith("172.16.") ||     // ClassB
                                            request.RemoteAddr.StartsWith("192.168.") ||    // ClassC
                                            request.RemoteAddr.StartsWith("127."))          // LoopBack
                                        {
                                            // ok
                                        }
                                        else
                                        {
                                            // インポートURLのIPアドレスを求める
                                            System.Net.IPAddress hostadd;
                                            try
                                            {
                                                hostadd = System.Net.Dns.GetHostAddresses(imp)[0];
                                                if (hostadd.ToString() != request.RemoteAddr)
                                                {
                                                    comment = "接続制限/インポートURLと設定者IPが一致しません";
                                                    break;
                                                }
                                            }
                                            catch
                                            {
                                                comment = "接続制限/インポートURLからIPアドレスに変換できません";
                                                break;
                                            }
                                        }
                                    }

                                    ////////////////////////////////
                                    // チェックOK。接続情報設定開始

                                    // リザーブ情報は一度すべて消して新規設定
                                    Event.EventUpdateReserve(k, null, 2);   // GUIのListViewからListViewItem全削除
                                    k.Status.Gui.ReserveItem.Clear();       // ReserveItemのListViewItem全削除

                                    // リザーブ再設定
                                    if (dic["reserve"].Length > 0)
                                    {
                                        Regex r = new Regex(",");
                                        string[] reserve_list = r.Split(dic["reserve"], k.Status.Reserve + 1);
                                        for (int cnt = 0; cnt < k.Status.Reserve && cnt < reserve_list.Length; cnt++)
                                        {
                                            // IPに変換できないホスト名なら登録あきらめ
                                            try
                                            {
                                                System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(reserve_list[cnt])[0];
                                                k.Status.AddReserve(hostadd.ToString());
                                            }
                                            catch { }
                                        }
                                    }

                                    k.Status.Type = 1;  // pull配信
                                    k.Status.ImportURL = dic["address"];
                                    k.Status.Password = dic["password"];
                                    k.Status.Comment = dic["comment"];
                                    k.Status.UrlVisible = (dic["radio"] == "on" ? true : false);
                                    if (dic.ContainsKey("redir_p"))
                                        k.Status.EnableRedirectParent = dic["redir_p"] == "on" ? true : false;
                                    if (dic.ContainsKey("redir_c"))
                                        k.Status.EnableRedirectChild = dic["redir_c"] == "on" ? true : false;
                                    k.Status.SetUserIP = request.RemoteAddr;
                                    if (dic.ContainsKey("url"))
                                        k.Status.Url = dic["url"];
                                    else
                                        k.Status.Url = "";
                                    Event.EventUpdateKagami();
                                    Front.AddLogData(1, k.Status, "外部から接続要求を受信しました / 要求元IP:" + request.RemoteAddr);
                                    Front.AddLogData(1, k.Status, "URL=" + k.Status.ImportURL + " / コメント=" + k.Status.Comment + " / 実況URL=" + k.Status.Url);
                                    comment = "送信完了";
                                }
                                else
                                {
                                    // push配信
                                    if (!Front.Opt.EnablePush)
                                    {
                                        comment = "Push配信無効";
                                        break;
                                    }

                                    ////////////////////////////////
                                    // チェックOK。接続情報設定開始

                                    // リザーブ情報は一度すべて消して新規設定
                                    Event.EventUpdateReserve(k, null, 2);   // GUIのListViewからListViewItem全削除
                                    k.Status.Gui.ReserveItem.Clear();       // ReserveItemのListViewItem全削除

                                    // リザーブ再設定
                                    if (dic["reserve"].Length > 0)
                                    {
                                        Regex r = new Regex(",");
                                        string[] reserve_list = r.Split(dic["reserve"], k.Status.Reserve + 1);
                                        for (int cnt = 0; cnt < k.Status.Reserve && cnt < reserve_list.Length; cnt++)
                                        {
                                            // IPに変換できないホスト名なら登録あきらめ
                                            try
                                            {
                                                System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(reserve_list[cnt])[0];
                                                k.Status.AddReserve(hostadd.ToString());
                                            }
                                            catch { }
                                        }
                                    }

                                    k.Status.Type = 2;  // push配信
                                    //k.Status.ImportURL = dic["address"];
                                    k.Status.Password = dic["password"];
                                    k.Status.Comment = dic["comment"];
                                    k.Status.UrlVisible = (dic["radio"] == "on" ? true : false);
                                    if (dic.ContainsKey("redir_p"))
                                        k.Status.EnableRedirectParent = dic["redir_p"] == "on" ? true : false;
                                    if (dic.ContainsKey("redir_c"))
                                        k.Status.EnableRedirectChild = dic["redir_c"] == "on" ? true : false;
                                    k.Status.SetUserIP = request.RemoteAddr;
                                    if (dic.ContainsKey("url"))
                                        k.Status.Url = dic["url"];
                                    else
                                        k.Status.Url = "";
                                    Event.EventUpdateKagami();
                                    Front.AddLogData(1, k.Status, "外部からPush配信待ち受け指示を受信しました / 要求元IP:" + request.RemoteAddr);
                                    Front.AddLogData(1, k.Status, "コメント=" + k.Status.Comment + " / 実況URL=" + k.Status.Url);
                                    comment = "送信完了";
                                }
                                #endregion // 接続設定要求送信
                                break;
                            }
                            comment = "不明なエラーです";
                            // ダミーループ終了
                            break;
                        }
                        if (comment != "")
                        {
                            foreach (string s in TemplateOk)
                                tw.WriteLine(s.Replace("<Status>", comment));
                        }
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "conn.html内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/dis.html?"))
            {
                #region /dis.htmlへのリクエスト
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {

                        string comment = "";
                        string str = request.URI.Replace("/dis.html?", "");
                        str = HttpUtility.UrlDecode(str);

                        string[] str2 = new string[4];
                        str2 = str.Split('&');
                        Dictionary<string, string> dic = new Dictionary<string, string>();

                        foreach (string s in str2)
                        {
                            dic.Add(s.Split('=')[0], s.Split('=')[1]);
                        }

                        if (dic.ContainsKey("dis"))
                        {
                            #region 切断画面表示
                            int port = int.Parse(dic["dis"]);
                            foreach (string s in TemplateDis)
                            {
                                tw.WriteLine(s.Replace("<PORT>", port.ToString()));
                            }
                            #endregion
                        }
                        else if (dic.ContainsKey("password"))
                        {
                            #region 切断要求受信
                            Kagami k = Front.IndexOf(int.Parse(dic["Port"]));
                            // 起動中ポートかつ、
                            // push配信の場合は待機中・接続中関係なく切断可能
                            // push配信以外の場合は、待機中以外なら切断可能
                            if (k != null && (k.Status.Type == 2 || k.Status.ImportURL != "待機中"))
                            {
                                if ((k.Status.Password == dic["password"]) ||
                                    (Front.Opt.AdminPass != "" && Front.Opt.AdminPass == dic["password"]))
                                {
                                    if (k.Status.Type == 2 && k.Status.ImportURL == "待機中")
                                        k.Status.Type = 1;  // Push配信要求待ち中の切断要求
                                    else
                                        k.Status.Disc();    // 正常接続中のインポート切断
                                    Front.AddLogData(1, k.Status, "外部から切断要求を受信しました / 要求元IP:" + request.RemoteAddr);
                                    comment = "送信完了";
                                }
                                else
                                {
                                    comment = "パスワードが一致しません";
                                }
                            }
                            else
                            {
                                comment = "そのポートは切断可能状態ではありません";
                            }
                            #endregion
                        }
                        else
                        {
                            comment = "不明なエラーです";
                        }
                        if (comment != "")
                        {
                            foreach (string s in TemplateOk)
                                tw.WriteLine(s.Replace("<Status>", comment));
                        }
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "dis.html内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/auth.html?"))
            {
                #region /auth.htmlへのリクエスト
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {

                        string comment = "";
                        string str = request.URI.Replace("/auth.html?", "");
                        str = HttpUtility.UrlDecode(str);

                        string[] str2 = new string[4];
                        str2 = str.Split('&');
                        Dictionary<string, string> dic = new Dictionary<string, string>();

                        foreach (string s in str2)
                        {
                            dic.Add(s.Split('=')[0], s.Split('=')[1]);
                        }
                        if (dic.ContainsKey("port"))
                        {
                            #region 認証画面表示
                            int port = int.Parse(dic["port"]);
                            Kagami k = Front.IndexOf(int.Parse(dic["port"]));
                            if (k != null && (k.Status.Type == 2 || k.Status.ImportURL != "待機中"))
                            {
                                foreach (string s in TemplateAuth)
                                    tw.WriteLine(s.Replace("<PORT>", port.ToString()));
                            }
                            else
                            {
                                comment = "そのポートは接続可能状態ではありません";
                            }
                            #endregion
                        }
                        else
                        {
                            comment = "不明なエラーです";
                        }
                        if (comment != "")
                        {
                            foreach (string s in TemplateOk)
                                tw.WriteLine(s.Replace("<Status>", comment));
                        }
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "auth.html内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/set.html?"))
            {
                #region /set.htmlへのリクエスト
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {

                        string comment = "";
                        string str = request.URI.Replace("/set.html?", "");
                        str = HttpUtility.UrlDecode(str);

                        string[] str2 = new string[4];
                        str2 = str.Split('&');
                        Dictionary<string, string> dic = new Dictionary<string, string>();

                        foreach (string s in str2)
                            dic.Add(s.Split('=')[0], s.Split('=')[1]);

                        //ダミーループ
                        while (true)
                        {
                            if (dic.ContainsKey("port") &&
                                dic.ContainsKey("password") &&
                                !dic.ContainsKey("reserve"))
                            {
                                #region 設定変更画面表示
                                int port = int.Parse(dic["port"]);
                                Kagami k = Front.IndexOf(port);
                                if (k == null || (k.Status.Type != 2 && k.Status.ImportURL == "待機中"))
                                {
                                    comment = "そのポートは接続可能状態ではありません";
                                    break;
                                }

                                if (k.Status.Password != dic["password"] &&
                                    (Front.Opt.AdminPass == "" || Front.Opt.AdminPass != dic["password"]))
                                {
                                    comment = "パスワードが一致しません";
                                    break;
                                }

                                ///////////////////////
                                // 設定変更開始
                                // 設定済みリザーブリストをカンマ区切り文字列にする
                                string reserve_list = "";
                                foreach (ListViewItem r in k.Status.Gui.ReserveItem)
                                    reserve_list += r.Text + ",";
                                if (reserve_list.Length > 0)
                                    reserve_list = reserve_list.Remove(reserve_list.Length - 1);

                                foreach (string s in TemplateSet)
                                {
                                    tw.WriteLine(s
                                        .Replace("<PORT>", port.ToString())
                                        .Replace("<PASSWORD>", k.Status.Password)
                                        .Replace("<COMMENT>", k.Status.Comment)
                                        .Replace("<URL_ON>", (k.Status.UrlVisible == true ? "checked=\"checked\"" : ""))
                                        .Replace("<URL_OFF>", (k.Status.UrlVisible == false ? "checked=\"checked\"" : ""))
                                        .Replace("<REDIR_P_ON>", (k.Status.EnableRedirectParent == true ? "checked=\"checked\"" : ""))
                                        .Replace("<REDIR_P_OFF>", (k.Status.EnableRedirectParent == false ? "checked=\"checked\"" : ""))
                                        .Replace("<REDIR_C_ON>", (k.Status.EnableRedirectChild == true ? "checked=\"checked\"" : ""))
                                        .Replace("<REDIR_C_OFF>", (k.Status.EnableRedirectChild == false ? "checked=\"checked\"" : ""))
                                        .Replace("<RESERVE_LIST>", reserve_list)
                                        .Replace("<RESERVE_NUM>", k.Status.Reserve.ToString())
                                        .Replace("<LIVE_URL>", k.Status.Url)
                                        .Replace("<LIVEOUT_URL>", Front.Opt.OutUrl)
                                    );
                                }
                                #endregion
                                break;
                            }
                            else if (dic.ContainsKey("port") &&
                                dic.ContainsKey("password") &&
                                dic.ContainsKey("reserve") &&
                                dic.ContainsKey("comment") &&
                                dic.ContainsKey("radio"))
                            {
                                #region 設定変更要求受信
                                int port = int.Parse(dic["port"]);
                                Kagami k = Front.IndexOf(port);
                                if (k == null ||
                                    (k.Status.Type != 2 && k.Status.ImportURL == "待機中"))
                                {
                                    comment = "そのポートは接続可能状態ではありません";
                                    break;
                                }
                                if (k.Status.Password != dic["password"] &&
                                    (Front.Opt.AdminPass == "" || Front.Opt.AdminPass != dic["password"]))
                                {
                                    comment = "パスワードが一致しません";
                                    break;
                                }

                                // リザーブIPリストの正常チェック
                                // 受け取ったリスト数、もしくは最大リザーブ登録数の小さい方の値でループ
                                Regex r = new Regex(",");
                                string[] reserve_list = r.Split(dic["reserve"], k.Status.Reserve + 1);
                                try
                                {
                                    for (int r_cnt = 0; r_cnt < reserve_list.Length && r_cnt < k.Status.Reserve; r_cnt++)
                                    {
                                        if (reserve_list[r_cnt] != "")
                                        {
                                            System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(reserve_list[r_cnt])[0];
                                        }
                                    }
                                }
                                catch
                                {
                                    // DNSエラーの場合更新ＮＧにする
                                    comment = "入力エラー/不正なリザーブIPです";
                                    break;
                                }

                                if (dic["new_password"].Length == 0)
                                {
                                    comment = "入力エラー/新パスワードが空です";
                                    break;
                                }
                                if (dic["comment"].IndexOf("<") >= 0 ||
                                    dic["comment"].IndexOf(">") >= 0 ||
                                    dic["comment"].IndexOf("\"") >= 0 ||
                                    dic["comment"].IndexOf("+ADw-") >= 0 ||    // UTF-7 XSS
                                    dic["comment"].IndexOf("+AD4-") >= 0 ||    // UTF-7 XSS
                                    (dic["radio"] != "on" && dic["radio"] != "off"))
                                {
                                    comment = "入力エラー/不正なコメントです";
                                    break;
                                }

                                // 実況URLは送られてきていればチェックする
                                if (dic.ContainsKey("url") && dic["url"].Length > 0)
                                {
                                    if (!dic["url"].StartsWith(Front.Opt.OutUrl) ||
                                        dic["url"].IndexOf("<") != -1 ||
                                        dic["url"].IndexOf(">") != -1 ||
                                        dic["url"].IndexOf("\"") != -1 ||
                                        dic["url"].IndexOf("+ADw-") != -1 ||    // UTF-7 XSS
                                        dic["url"].IndexOf("+AD4-") != -1)     // UTF-7 XSS)
                                    {
                                        comment = "入力エラー/不正な実況URLです";
                                        break;
                                    }
                                }

                                /////////////////////
                                // 設定変更開始
                                k.Status.Password = dic["new_password"];
                                k.Status.Comment = dic["comment"];
                                k.Status.UrlVisible = (dic["radio"] == "on" ? true : false);
                                if (dic.ContainsKey("redir_p"))
                                    k.Status.EnableRedirectParent = dic["redir_p"] == "on" ? true : false;
                                if (dic.ContainsKey("redir_c"))
                                    k.Status.EnableRedirectChild = dic["redir_c"] == "on" ? true : false;
                                if (dic.ContainsKey("url"))
                                    k.Status.Url = dic["url"];
                                else
                                    k.Status.Url = "";
                                // リザーブ情報は一度すべて消して新規設定
                                Event.EventUpdateReserve(k, null, 2);   // GUIのListViewからListViewItem全削除
                                k.Status.Gui.ReserveItem.Clear();       // ReserveItemのListViewItem全削除
                                // 受け取ったリスト数、もしくは最大リザーブ登録数の小さい方の値でループ
                                for (int r_cnt = 0; r_cnt < reserve_list.Length && r_cnt < k.Status.Reserve; r_cnt++)
                                {
                                    try
                                    {
                                        if (reserve_list[r_cnt] != "")
                                        {
                                            System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(reserve_list[r_cnt])[0];
                                            k.Status.AddReserve(hostadd.ToString());
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        // 先にホスト名チェックしてるからここには来ないはず。。
                                        Front.AddLogDebug("HttpHybrid", "設定変更エラー(内部エラー:" + e.Message + "/Trace:" + e.StackTrace + ")");
                                    }
                                }
                                comment = "送信完了";
                                #endregion
                                break;
                            }
                            comment = "不明なエラーです";
                            // ダミーループ終了
                            break;
                        }
                        if (comment != "")
                        {
                            foreach (string s in TemplateOk)
                                tw.WriteLine(s.Replace("<Status>", comment));
                        }
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "set.html内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            // cssはテンプレートが有ればアクセス可能
            else if (request.URI.StartsWith("/kagamin.css") && TemplateCss.Count > 0)
            {
                #region /kagamin.cssへのリクエスト
                response.ContentType = "text/css";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        foreach (string s in TemplateCss)
                            tw.WriteLine(s);
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "kagamin.css内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            // admin.html有効時のみアクセスできる
            else if (request.URI.StartsWith("/admin.html") && Front.Opt.EnableAdmin)
            {
                #region /admin.htmlへのアクセス
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        // ダミーループ
                        while (true)
                        {
                            if (!request.URI.StartsWith("/admin.html?"))
                            {
                                //クエリなし - 認証画面へ
                                foreach (string s in TemplateAdminAuth)
                                    tw.WriteLine(s);
                                break;
                            }

                            string comment = "";
                            string str = request.URI.Replace("/admin.html?", "");
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            try
                            {
                                // クエリ分解
                                str = HttpUtility.UrlDecode(str);
                                string[] str2 = str.Split('&');
                                foreach (string s in str2)
                                    dic.Add(s.Split('=')[0], s.Split('=')[1]);
                            }
                            catch
                            {
                                //クエリ分解不能 - 認証画面へ
                                foreach (string s in TemplateAdminAuth)
                                    tw.WriteLine(s);
                                break;
                            }

                            // パスワードクエリがあるか？
                            if (!dic.ContainsKey("password"))
                            {
                                //パスワードなし - 認証画面へ
                                foreach (string s in TemplateAdminAuth)
                                    tw.WriteLine(s);
                                break;
                            }

                            // パスワード一致判定
                            if (dic["password"] == "" || dic["password"] != Front.Opt.AdminPass)
                            {
                                //パスワードが空、または不一致
                                foreach (string s in TemplateAdminOk)
                                    tw.WriteLine(s
                                        .Replace("<Status>", "パスワードが一致しません")
                                        .Replace("<ADMIN_PASS>", "")
                                    );
                                break;
                            }
                            if (!dic.ContainsKey("mode"))
                            {
                                #region 管理画面トップ表示
                                // main.htmlをパクリつつ、追加タグを用意する
                                int i = 0;
                                int count = 1;

                                string str_dmt = "";    // 未起動状態テンプレート
                                string str_wai = "";    // 待機中状態テンプレート
                                string str_try = "";    // 試行中状態テンプレート
                                string str_con = "";    // 接続中状態テンプレート
                                string str_tmp = "";    // 実際に出力するテンプレート書式

                                // 転送量計算
                                string _ul_day, _dl_day, _ul_mon, _dl_mon;
                                _ul_day = ((ulong)(Front.Log.TrsUpDay + Front.TotalUP)).ToString("#,##0,,MB");
                                _dl_day = ((ulong)(Front.Log.TrsDlDay + Front.TotalDL)).ToString("#,##0,,MB");
                                _ul_mon = ((ulong)(Front.Log.TrsUpMon + Front.TotalUP)).ToString("#,##0,,MB");
                                _dl_mon = ((ulong)(Front.Log.TrsDlMon + Front.TotalDL)).ToString("#,##0,,MB");

                                // <TEMPLATE>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<TEMPLATE>")
                                {
                                    tw.WriteLine(TemplateAdminMain[i]
                                        .Replace("<VERSION>", Front.AppName)
                                        .Replace("<PAUSE>", Front.Pause ? "一時停止中" : "制限なし") // 管理モードでは状態表示
                                        .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "開始中" : "停止中")
                                        .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)
                                        .Replace("<MAXCONN>", Front.Gui.Conn.ToString() + "+" + Front.Gui.Reserve.ToString())
                                        .Replace("<TRF_UP_DAY>", _ul_day)
                                        .Replace("<TRF_DL_DAY>", _dl_day)
                                        .Replace("<TRF_UP_MON>", _ul_mon)
                                        .Replace("<TRF_DL_MON>", _dl_mon)
                                    );
                                    i++;
                                }

                                #region テンプレート読込
                                // <未起動表示>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<未起動表示>")
                                    i++;
                                i++;
                                // </未起動表示>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</未起動表示>")
                                {
                                    // ポートリストに登録されている未起動ポートの表示用タグを取得
                                    str_dmt += TemplateAdminMain[i] + "\r\n";
                                    i++;
                                }

                                // <待機中表示>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<待機中表示>")
                                    i++;
                                i++;
                                // </待機中表示>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</待機中表示>")
                                {
                                    // 待機中の表示用タグを取得
                                    str_wai += TemplateAdminMain[i] + "\r\n";
                                    i++;
                                }

                                // <接続試行中表示>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<接続試行中表示>")
                                    i++;
                                i++;
                                // </接続試行中表示>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</接続試行中表示>")
                                {
                                    // 接続試行中の表示用タグを取得
                                    str_try += TemplateAdminMain[i] + "\r\n";
                                    i++;
                                }

                                // <接続中表示>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<接続中表示>")
                                    i++;
                                i++;
                                // </接続中表示>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</接続中表示>")
                                {
                                    // 接続試行中の表示用タグを取得
                                    str_con += TemplateAdminMain[i] + "\r\n";
                                    i++;
                                }
                                #endregion

                                // </TEMPLATE>が見つかるまでループ
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</TEMPLATE>")
                                    i++;
                                i++;

                                Kagami k;
                                // 登録ポートと起動中ポートの２つを一緒にループ
                                for (int cnt = 0; cnt < Front.Gui.PortList.Count + Front.KagamiList.Count; cnt++)
                                {
                                    if (cnt < Front.Gui.PortList.Count)
                                    {
                                        // 最初に、登録ポートの中から未起動ポートの一覧出力
                                        k = Front.IndexOf(Front.Gui.PortList[cnt]);
                                        if (k != null)
                                            continue;  // 起動中ポートは対象外
                                    }
                                    else
                                    {
                                        // その後、起動中ポートの一覧出力
                                        k = Front.KagamiList[cnt - Front.Gui.PortList.Count];
                                    }

                                    // 管理モードの場合、内側接続も表示
                                    /*
                                    // 内側接続はエントランスに表示しない
                                    if (k.Status.Type == 0)
                                        continue;
                                    */
                                    // <KAGAMI>が見つかるまでループ
                                    while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<KAGAMI>")
                                    {
                                        tw.WriteLine(TemplateAdminMain[i]
                                            .Replace("<VERSION>", Front.AppName)
                                            .Replace("<PAUSE>", Front.Pause ? "一時停止中" : "制限なし") // 管理モードでは状態表示
                                            .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "開始中" : "停止中")
                                            .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)
                                            .Replace("<MAXCONN>", Front.Gui.Conn.ToString() + "+" + Front.Gui.Reserve.ToString())
                                            .Replace("<TRF_UP_DAY>", _ul_day)
                                            .Replace("<TRF_DL_DAY>", _dl_day)
                                            .Replace("<TRF_UP_MON>", _ul_mon)
                                            .Replace("<TRF_DL_MON>", _dl_mon)
                                        );
                                        i++;
                                    }

                                    // <KAGAMI>が見つからなかった
                                    if (TemplateAdminMain.Count <= i)
                                        break;

                                    i++;

                                    if (k == null)
                                        str_tmp = str_dmt;  //未起動
                                    else if (k.Status.ImportURL == "待機中")
                                        str_tmp = str_wai;  //待機中
                                    else if (k.Status.ImportStatus == false)
                                        str_tmp = str_try;  //接続試行中(ImportURLに文字は入っているが、Importフラグがfalse)
                                    else
                                        str_tmp = str_con;  //正常に稼動中

                                    // HTMLを出力
                                    if (k == null)
                                    {
                                        // 未起動ポートはk.Statusが絡まない部分のみサポート
                                        tw.WriteLine(
                                            str_tmp
                                            .Replace("<VERSION>", Front.AppName)
                                            .Replace("<PAUSE>", Front.Pause ? "一時停止中" : "制限なし") // 管理モードでは状態表示
                                            .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "開始中" : "停止中")
                                            .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)       // 管理モード追加タグ
                                            .Replace("<MAXCONN>", Front.Gui.Conn.ToString() + "+" + Front.Gui.Reserve.ToString())
                                            .Replace("<COUNT>", count.ToString())
                                            .Replace("<PORT>", Front.Gui.PortList[cnt].ToString())
                                        );
                                    }
                                    else
                                    {
                                        // 起動中ポートの状態表示
                                        tw.WriteLine(
                                            str_tmp
                                            .Replace("<VERSION>", Front.AppName)
                                            .Replace("<PAUSE>", Front.Pause ? "一時停止中" : "制限なし") // 管理モードでは状態表示
                                            .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "開始中" : "停止中")
                                            .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)       // 管理モード追加タグ
                                            .Replace("<COUNT>", count.ToString())
                                            .Replace("<MY_URL>", Front.Hp.IpHTTP + ":" + k.Status.MyPort.ToString())
                                            .Replace("<PORT>", k.Status.MyPort.ToString())
                                            //.Replace("<SRC_URL>", (k.Status.UrlVisible ? k.Status.ImportURL : "設定が非表示になっています"))
                                            .Replace("<SRC_URL>", k.Status.ImportURL) // 管理モードでは常に表示
                                            .Replace("<CONN>", k.Status.Client.Count.ToString())
                                            .Replace("<MAXCONN>", k.Status.Connection.ToString() + ((k.Status.Reserve != 0) ? "+" + k.Status.Reserve.ToString() : ""))
                                            .Replace("<COMMENT>", k.Status.Comment)
                                            .Replace("<BANDWIDTH>", Front.BndWth.EnableBandWidth ?
                                                    (k.Status.LimitUPSpeed >= 1000 ?
                                                        (k.Status.LimitUPSpeed / 1000).ToString() + "Mbps" :
                                                        k.Status.LimitUPSpeed.ToString() + "Kbps"
                                                    ) : "-")
                                            .Replace("<BITRATE>", k.Status.MaxDLSpeed.ToString() + "Kbps")
                                            .Replace("<BUSYCOUNT>", k.Status.BusyCounter.ToString())
                                            .Replace("<TIME>", k.Status.ImportTimeString)
                                        );
                                    }
                                    count++;
                                }

                                // 必要分<KAGAMI>を置き換えた後の処理
                                while (TemplateAdminMain.Count > i)
                                {
                                    tw.WriteLine(TemplateAdminMain[i]
                                        .Replace("<KAGAMI>", "")        // <KAGAMI>タグをNULL化
                                        .Replace("<VERSION>", Front.AppName)
                                        .Replace("<PAUSE>", Front.Pause ? "一時停止中" : "制限なし") // 管理モードでは状態表示
                                        .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "開始中" : "停止中")
                                        .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)       // 管理モード追加タグ
                                        .Replace("<MAXCONN>", Front.Gui.Conn.ToString() + "+" + Front.Gui.Reserve.ToString())
                                        .Replace("<TRF_UP_DAY>", _ul_day)
                                        .Replace("<TRF_DL_DAY>", _dl_day)
                                        .Replace("<TRF_UP_MON>", _ul_mon)
                                        .Replace("<TRF_DL_MON>", _dl_mon)
                                    );
                                    i++;
                                }

                                #endregion
                            }
                            else
                            {
                                #region 設定変更
                                // パスワードとモードは確認済み。
                                // ポートを受け取るなら、個別でチェックすること。
                                int _port, _conn, _resv;
                                Kagami _k;
                                switch (dic["mode"])
                                {
                                    case "start":
                                        #region 指定ポート起動
                                        if (!dic.ContainsKey("port") || !dic.ContainsKey("conn"))
                                        {
                                            comment = "必要なパラメータが設定されていません";
                                            break;
                                        }
                                        try
                                        {
                                            _port = int.Parse(dic["port"]);
                                        }
                                        catch
                                        {
                                            comment = "ポート情報が異常です";
                                            break;
                                        }
                                        try
                                        {
                                            string[] _str = dic["conn"].Split('+');
                                            _conn = int.Parse(_str[0]);
                                            _resv = int.Parse(_str[1]);
                                        }
                                        catch
                                        {
                                            comment = "接続枠設定が異常です";
                                            break;
                                        }
                                        _k = Front.IndexOf(_port);
                                        if (_k != null)
                                        {
                                            comment = "そのポートは既に起動中です";
                                            break;
                                        }
                                        // 待ち受け開始設定
                                        _k = new Kagami("", _port, _conn, _resv);
                                        // 個別帯域設定の場合、上限帯域を設定しておく
                                        if (Front.BndWth.BandStopMode == 2)
                                        {
                                            _k.Status.GUILimitUPSpeed = (int)Front.BndWth.BandStopValue;
                                            _k.Status.LimitUPSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                                        }
                                        Front.Add(_k);
                                        comment = "Port:" + dic["port"] + " 待ち受け開始完了";
                                        #endregion
                                        break;
                                    case "startall":
                                        #region 全ポート起動
                                        if (!dic.ContainsKey("conn"))
                                        {
                                            comment = "必要なパラメータが設定されていません";
                                            break;
                                        }
                                        try
                                        {
                                            string[] _str = dic["conn"].Split('+');
                                            _conn = int.Parse(_str[0]);
                                            _resv = int.Parse(_str[1]);
                                            if (_conn < 0 || _resv < 0)
                                                throw new Exception();
                                        }
                                        catch
                                        {
                                            comment = "接続枠設定が異常です";
                                            break;
                                        }
                                        foreach (int _i in Front.Gui.PortList)
                                        {
                                            _k = Front.IndexOf(_i);
                                            if (_k == null)
                                            {
                                                // 待ち受け開始設定
                                                _k = new Kagami("", _i, _conn, _resv);
                                                // 個別帯域設定の場合、上限帯域を設定しておく
                                                if (Front.BndWth.BandStopMode == 2)
                                                {
                                                    _k.Status.GUILimitUPSpeed = (int)Front.BndWth.BandStopValue;
                                                    _k.Status.LimitUPSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                                                }
                                                Front.Add(_k);
                                            }
                                        }
                                        //デフォルト接続数を更新
                                        Front.Gui.Conn = (uint)_conn;
                                        Front.Gui.Reserve = (uint)_resv;
                                        comment = "全ポート待ち受け開始完了";
                                        #endregion
                                        break;
                                    case "stop":
                                        #region 指定ポート停止
                                        if (!dic.ContainsKey("port"))
                                        {
                                            comment = "ポート番号が指定されていません";
                                            break;
                                        }
                                        try
                                        {
                                            _port = int.Parse(dic["port"]);
                                        }
                                        catch
                                        {
                                            comment = "ポート番号異常";
                                            break;
                                        }
                                        _k = Front.IndexOf(_port);
                                        if (_k == null)
                                        {
                                            comment = "そのポートは待ち受け中状態ではありません";
                                            break;
                                        }
                                        // 待ち受け停止設定
                                        _k.Status.Disc();
                                        _k.Status.RunStatus = false;
                                        comment = "Port:" + dic["port"] + " 待ち受け停止完了";
                                        #endregion
                                        break;
                                    case "stopall":
                                        #region 全ポート停止
                                        foreach (Kagami _k_tmp in Front.KagamiList)
                                        {
                                            _k_tmp.Status.Disc();
                                            _k_tmp.Status.RunStatus = false;
                                        }
                                        comment = "全ポート待ち受け停止完了";
                                        #endregion
                                        break;
                                    case "chg":
                                        #region 枠数の変更
                                        if (!dic.ContainsKey("port") || !dic.ContainsKey("conn"))
                                        {
                                            comment = "必要なパラメータが設定されていません";
                                            break;
                                        }
                                        try
                                        {
                                            _port = int.Parse(dic["port"]);
                                        }
                                        catch
                                        {
                                            comment = "ポート番号異常";
                                            break;
                                        }
                                        _k = Front.IndexOf(_port);
                                        if (_k == null)
                                        {
                                            comment = "そのポートは未起動です";
                                            break;
                                        }
                                        try
                                        {
                                            string[] _str = dic["conn"].Split('+');
                                            _conn = int.Parse(_str[0]);
                                            _resv = int.Parse(_str[1]);
                                        }
                                        catch
                                        {
                                            comment = "接続枠設定が異常です";
                                            break;
                                        }
                                        _k.Status.Conn_UserSet = _conn;
                                        _k.Status.Reserve = _resv;
                                        Event.EventUpdateKagami();
                                        comment = "枠数を変更しました(PORT:" + _port + "/" + _conn + "+" + _resv + ")";
                                        #endregion
                                        break;
                                    case "pause":
                                        #region 新規接続の一時停止・一時停止解除
                                        Front.Pause = !Front.Pause;
                                        Event.EventUpdateKagami();
                                        if (Front.Pause)
                                            comment = "新規接続の制限を開始しました";
                                        else
                                            comment = "新規接続の制限を解除しました";
                                        #endregion
                                        break;
                                    case "band":
                                        #region 帯域制限の開始・停止
                                        Front.BndWth.EnableBandWidth = !Front.BndWth.EnableBandWidth;
                                        Event.EventUpdateKagami();
                                        if (Front.BndWth.EnableBandWidth)
                                            comment = "帯域制限を開始しました";
                                        else
                                            comment = "帯域制限を停止しました";
                                        #endregion
                                        break;
                                    case "dis":
                                        #region 指定ポート強制切断
                                        if (!dic.ContainsKey("port"))
                                        {
                                            comment = "ポート番号が指定されていません";
                                            break;
                                        }
                                        // 切断のための値設定
                                        try
                                        {
                                            _port = int.Parse(dic["port"]);
                                        }
                                        catch
                                        {
                                            comment = "ポート番号が異常です";
                                            break;
                                        }
                                        Kagami k = Front.IndexOf(_port);
                                        if (k == null || (k.Status.Type != 2 && k.Status.ImportURL == "待機中"))
                                        {
                                            comment = "そのポートは切断可能状態ではありません";
                                            break;
                                        }
                                        // インポート切断
                                        k.Status.Disc();
                                        comment = "Port:" + dic["port"] + " 切断完了";
                                        #endregion
                                        break;
                                    case "disall":
                                        #region 全ポート強制切断
                                        foreach (Kagami _k_tmp in Front.KagamiList)
                                        {
                                            // インポート切断
                                            _k_tmp.Status.Disc();
                                        }
                                        comment = "全ポート切断完了";
                                        #endregion
                                        break;
                                    default:
                                        comment = "不明なモードです";
                                        break;
                                }
                                // メッセージ出力
                                foreach (string s in TemplateAdminOk)
                                    tw.WriteLine(s
                                        .Replace("<Status>", comment)
                                        .Replace("<ADMIN_PASS>", dic["password"])
                                    );
                                #endregion
                            }
                            // ダミーループ終了
                            break;
                        }// end of while(true)
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "admin.html内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            // info.html有効時のみアクセスできる
            else if (request.URI.StartsWith("/info.html") && Front.Opt.EnableInfo)
            {
                #region /info.htmlへのリクエスト
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {

                        tw.WriteLine("<html><head></head><body>");
                        TimeSpan _duration = DateTime.Now - Front.StartTime;
                        tw.WriteLine("UPTIME:" + (long)_duration.TotalSeconds + "<br>");
                        tw.WriteLine("CPU_ALL:" + (int)Front.CPU_ALL.NextValue() + "<br>");
                        tw.WriteLine("CPU_APP:" + (int)Front.CPU_APP.NextValue() + "<br>");
                        tw.WriteLine("TOTAL_UP:" + Front.TotalUP.ToString() + "<br>");
                        tw.WriteLine("TOTAL_DL:" + Front.TotalDL.ToString() + "<br>");
                        foreach (Kagami k in Front.KagamiList)
                        {
                            // 内側接続は表示させない…やっぱり表示する。
                            //if (k.Status.Type == 0)
                            //    continue;
                            tw.WriteLine("<br>");
                            tw.WriteLine("PORT:" + k.Status.MyPort + "<br>");
                            tw.WriteLine("TITLE:" + k.Status.Comment + "<br>");
                            tw.WriteLine("UP:" + k.Status.TotalUPSize + "<br>");
                            tw.WriteLine("DL:" + k.Status.TotalDLSize + "<br>");
                            tw.WriteLine("CONN:" + k.Status.Client.Count + "<br>");
                        }
                        tw.WriteLine("</body></html>");
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "info.html内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
/*
            else if (request.URI.StartsWith("/rss.rdf") && Front.Opt.EnableRss)
            {
                #region /rss.rdfへのリクエスト
                response.ContentType = "application/xml; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    DateTime now = DateTime.Now;
                    string HPAddr = Front.Opt.RssUrl;
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        tw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                        tw.WriteLine("<rdf:RDF");
                        tw.WriteLine("xmlns=\"http://purl.org/rss/1.0/\"");
                        tw.WriteLine("xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"");
                        tw.WriteLine("xmlns:dc=\"http://purl.org/dc/elements/1.1/\"");
                        tw.WriteLine("xmlns:admin=\"http://webns.net/mvcb/\"");
                        tw.WriteLine("xml:lang=\"ja\">");
                        tw.WriteLine("<channel rdf:about=\"" + Front.Hp.IpHTTP + ":" + Front.Hp.PortHTTP + request.URI + "\">");
                        tw.WriteLine("<title>" + Front.Opt.RssTitle + "</title>");
                        tw.WriteLine("<link>\"" + HPAddr + "\"</link>");
                        tw.WriteLine("<dc:date>" + now.ToString("yyyy-MM-dd") + "T" + now.ToString("HH:mm:ss") + "+09:00</dc:date>");
                        tw.WriteLine("<description>" + Front.Opt.RssTitle + "RSS</description>");
                        tw.WriteLine("<admin:generatorAgent rdf:resource=\"" + HPAddr + "/?v=" + Front.AppName + "\" />");
                        tw.WriteLine("<items>");
                        tw.WriteLine("<rdf:Seq>");
                        foreach (Kagami k in Front.KagamiList)
                        {
                            if (k.Status.Type == 0)
                                continue;
                            tw.WriteLine(" <rdf:li rdf:resource=\"" + HPAddr + "#he" + now.ToString("yyyyMMdd") + "\"/>");
                        }
                        tw.WriteLine("</rdf:Seq>");
                        tw.WriteLine("</items>");
                        tw.WriteLine("</channel>");
                        foreach (Kagami k in Front.KagamiList)
                        {
                            // 内側接続は表示させない
                            if (k.Status.Type == 0)
                                continue;
                            tw.WriteLine("<item rdf:about=\"" + HPAddr + "#he" + now.ToString("yyyyMMdd") + "\">");
                            tw.WriteLine("<title>" + k.Status.MyPort + "</title>");
                            tw.WriteLine("<link>" + HPAddr + "</link>");
                            tw.WriteLine("<dc:date>" + now.ToString("yyyy-MM-dd") + "T" + now.ToString("HH:mm:ss") + "+09:00</dc:date>");
                            if (Front.Pause)
                            {
                                tw.WriteLine("<description>新規受付制限中です。</description>");
                            }
                            if (k.Status.ImportURL == "待機中" && k.Status.Type != 2)
                            {
                                //待機中
                                tw.WriteLine("<description>使用可能です。</description>");
                            }
                            else if (!k.Status.ImportStatus || (k.Status.Type == 2 && k.Status.ImportURL == "待機中"))
                            {//Importに文字は入っているが、Importフラグがfalse
                                //または、Push配信要求の待ち受け中
                                tw.WriteLine("<description>接続試行中またはプッシュ配信接続待機中です。</description>");
                            }
                            else
                            {
                                //正常に稼動中
                                tw.WriteLine("<description>使用中です。COMMENT:" + k.Status.Comment + "</description>");
                            }
                            tw.WriteLine("</item>");
                        }
                        tw.WriteLine("</rdf:RDF>");
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "rss.rdf内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
 */
            else
            {
                #region その他URIへのリクエスト
                bool _not_found = true;
                //ダミーループ
                while(true)
                {
                    // 公開Dirが設定されてなければ404
                    if (Front.Hp.PublicDir == "")
                        break;
                    
                    string _path = HttpUtility.UrlDecode(request.URI);
                    _path = Front.Hp.PublicDir + _path.Replace('\\', '/');
                    _path = Path.GetFullPath(_path);
                    if (!_path.StartsWith(Front.Hp.PublicDir))
                        break;

                    // 相対パスからファイル名と拡張子取り出し
                    string _ext = Path.GetExtension(_path);

                    // ファイルチェック。無ければ404。
                    if (!File.Exists(_path))
                        break;

                    ////////////////////
                    #region ファイルを発見したので送信する
                    _not_found = false;
                    resp = 200;
                    try
                    {
                        using (FileStream fs = new FileStream(_path, FileMode.Open))
                        using (Stream ostr = response.Send())
                        {
                            byte[] _buf = new byte[1000];
                            long _remain = response.ContentLength = fs.Length;
                            int _read = 0;
                            // 1000byteずつ分割送信
                            // 最初の１回目はContentsType判定
                            if (_remain > 0)
                            {
                                if (_remain > 1000)
                                    _read = fs.Read(_buf, 0, 1000);
                                else
                                    _read = fs.Read(_buf, 0, (int)_remain);
                                // ContentsType判定
                                switch (_ext)
                                {
                                    #region 拡張子毎の判定
                                    case "htm":
                                    case "html":
                                        response.ContentType = "text/html;";
                                        break;
                                    case "txt":
                                        response.ContentType = "text/plain";
                                        break;
                                    case "css":
                                        response.ContentType = "text/css";
                                        break;
                                    case "zip":
                                        response.ContentType = "application/zip";
                                        break;
                                    case "bin":
                                    case "exe":
                                    case "lzh":
                                    case "dll":
                                        response.ContentType = "application/octet-stream";
                                        break;
                                    case "pdf":
                                        response.ContentType = "application/pdf";
                                        break;
                                    case "ogg":
                                        response.ContentType = "application/ogg";
                                        break;
                                    case "mp3":
                                        response.ContentType = "audio/mpeg";
                                        break;
                                    case "jpg":
                                    case "jpeg":
                                        response.ContentType = "image/jpeg";
                                        break;
                                    case "gif":
                                        response.ContentType = "image/gif";
                                        break;
                                    case "png":
                                        response.ContentType = "image/png";
                                        break;
                                    case "bmp":
                                        response.ContentType = "image/bmp";
                                        break;
                                    case "ico":
                                        //response.ContentType = "image/x-icon";
                                        response.ContentType = "image/vnd.microsoft.icon";
                                        break;
                                    case "asf":
                                    case "asx":
                                        response.ContentType = "video/x-ms-asf";
                                        break;
                                    case "avi":
                                        response.ContentType = "video/x-msvideo";
                                        break;
                                    case "flv":
                                        response.ContentType = "video/x-flv";
                                        break;
                                    case "wmv":
                                        response.ContentType = "video/x-ms-wmv";
                                        break;
                                    #endregion
                                    default:
                                        //対応外の拡張子の場合、バイナリかどうかの判定を行う
                                        //何らかの文字コードが得られればtext/plainで送出
                                        Encoding enc = Front.GetCode(_buf, _read);
                                        if (enc == null)
                                            response.ContentType = "application/octet-stream";
                                        else
                                            response.ContentType = "text/plain";
                                        break;
                                }
                                // １回目出力
                                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                                ostr.Write(_buf, 0, _read);
                                _remain -= _read;

                            }
                            // ２回目以降出力
                            while (_remain > 0)
                            {
                                if (_remain > 1000)
                                    _read = fs.Read(_buf, 0, 1000);
                                else
                                    _read = fs.Read(_buf, 0, (int)_remain);
                                ostr.Write(_buf, 0, _read);
                                _remain -= _read;
                            }
                        }//endof using
                    }
                    catch { }
                    #endregion
                    // ダミーループ終了
                    break;
                }
                if (_not_found)
                {
                    #region ファイルが見つからなかった 404 Not Found
                    try
                    {
                        response.ContentType = "text/html;";
                        response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_FOUND;
                        using (Stream ostr = response.Send())
                        using (TextWriter tw = new StreamWriter(ostr))
                        {
                            tw.WriteLine("<html><head><title>404 NOT FOUND</title></head>\r\n" +
                                "<body><h1>404 NOT FOUND</h1></body></html>");
                        }
                    }
                    catch (Exception e)
                    {
                        Front.AddLogDebug("HttpHybrid", "NOT_FOUND内部エラー:" + e.Message + "/Trace:" + e.StackTrace);
                    }
                    resp = 404;
                    #endregion
                }
                #endregion
            }
            // HP AccessLog
            try
            {
                if (Front.Log.HpLogFile.Length != 0)
                {
                    lock (Front.Log.HpLogFile)
                    {
                        DateTime _dtNow = DateTime.Now;
                        Regex ym = new Regex("yyyymm");
                        Regex ymd = new Regex("yyyymmdd");
                        StreamWriter log = new StreamWriter(ym.Replace(ymd.Replace(Front.Log.HpLogFile, _dtNow.ToString("yyyyMMdd")), _dtNow.ToString("yyyyMM")), true);
                        string str = request.RemoteAddr + " - - [" + _dtNow.ToString("yyyy/MM/dd HH:mm:ss") + "] \"" + request.Method + " "
                            + request.URI + " " + request.Version + "\" " + resp.ToString("D3") + " - "
                            + (request.Has("Referer") ? "\"" + request.Get("Referer") + "\" " : "- ")
                            + (request.Has("User-Agent") ? "\"" + request.Get("User-Agent") + "\"" : "-");
                        log.WriteLine(str);
                        log.Close();
                    }
                }
            }
            catch { }
        }// end of HandleRequest
    }//end of Class DateTimeHandler

    class RequestHandlerFactory : IHTTPRequestHandlerFactory
    {
        public IHTTPRequestHandler CreateRequestHandler(HTTPServerRequest request)
        {
            return new DateTimeHandler();
        }
    }
}