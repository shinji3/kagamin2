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

        #region �R���X�g���N�^��HTTP�T�[�o�N���E��~���\�b�h
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public HttpHybrid()
        {
        }

        /// <summary>
        /// ���u����G���g�����X�N��
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
                MessageBox.Show("GlobalIP���擾�ł��܂���ł����B\r\n�l�b�g���[�N�ڑ����m�F���Ă��������B\r\nCause:" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("�J�n�ł��܂���ł����B\r\n���g�p�|�[�g���m�F���Ă��������B\r\nCause:" + e.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// ���u����G���g�����X��~
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
        /// �e���v���[�g�w�b�_
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
        /// �R���X�g���N�^
        /// </summary>
        public DateTimeHandler()
        {
            #region �e���v���[�g�ǂݍ���
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
                MessageBox.Show("�e���v���[�g�t�@�C����������܂���\r\nFILE:" + fe.FileName, "Error", MessageBoxButtons.OK);
                Application.Exit();
            }
            catch
            {
                MessageBox.Show("�e���v���[�g�ǂݍ��݂Ɏ��s���܂���", "Error", MessageBoxButtons.OK);
                Application.Exit();
            }

            // �Ǝ�Template
            // �����Ă����Ȃ��B
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
        /// �G���g�����X�ւ̃A�N�Z�X����
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public void HandleRequest(HTTPServerRequest request, HTTPServerResponse response)
        {
            int resp = 200; // http�����X�e�[�^�X(loging�p)
            response.KeepAlive = false;
            // �l�X�g���[�������G���[���������Â炢�̂Ō��₷����������
            // �A�N�Z�X�����`�F�b�N
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
            catch { } // DNS�t�������s�����瓧��

            if (_deny)
            {
                #region �A�N�Z�X�������̃����[�gIP����̐ڑ� 403 Forbidden
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
                    Front.AddLogDebug("HttpHybrid", "Forbidden�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                resp = 403;
                #endregion
            }
            else if (request.URI == "/" || request.URI.StartsWith("/index.html"))
            {
                #region /,/index.html�ւ̃��N�G�X�g
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        // �]���ʌv�Z
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
                    Front.AddLogDebug("HttpHybrid", "index.html�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/main.html"))
            {
                #region /main.html�ւ̃��N�G�X�g
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
                        // �]���ʌv�Z
                        string _ul_day, _dl_day, _ul_mon, _dl_mon;
                        _ul_day = ((ulong)(Front.Log.TrsUpDay + Front.TotalUP)).ToString("#,##0,,MB");
                        _dl_day = ((ulong)(Front.Log.TrsDlDay + Front.TotalDL)).ToString("#,##0,,MB");
                        _ul_mon = ((ulong)(Front.Log.TrsUpMon + Front.TotalUP)).ToString("#,##0,,MB");
                        _dl_mon = ((ulong)(Front.Log.TrsDlMon + Front.TotalDL)).ToString("#,##0,,MB");

                        // <TEMPLATE>��������܂Ń��[�v
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

                        // <�ҋ@���\��>��������܂Ń��[�v
                        while (TemplateMain.Count > i && TemplateMain[i] != "<�ҋ@���\��>")
                            i++;
                        i++;
                        // </�ҋ@���\��>��������܂Ń��[�v
                        while (TemplateMain.Count > i && TemplateMain[i] != "</�ҋ@���\��>")
                        {
                            // �ҋ@���̕\���p�^�O���擾
                            str_wai += TemplateMain[i] + "\r\n";
                            i++;
                        }

                        // <�ڑ����s���\��>��������܂Ń��[�v
                        while (TemplateMain.Count > i && TemplateMain[i] != "<�ڑ����s���\��>")
                            i++;
                        i++;
                        // </�ڑ����s���\��>��������܂Ń��[�v
                        while (TemplateMain.Count > i && TemplateMain[i] != "</�ڑ����s���\��>")
                        {
                            // �ڑ����s���̕\���p�^�O���擾
                            str_try += TemplateMain[i] + "\r\n";
                            i++;
                        }

                        // <�ڑ����\��>��������܂Ń��[�v
                        while (TemplateMain.Count > i && TemplateMain[i] != "<�ڑ����\��>")
                            i++;
                        i++;
                        // </�ڑ����\��>��������܂Ń��[�v
                        while (TemplateMain.Count > i && TemplateMain[i] != "</�ڑ����\��>")
                        {
                            // �ڑ����s���̕\���p�^�O���擾
                            str_con += TemplateMain[i] + "\r\n";
                            i++;
                        }

                        // </TEMPLATE>��������܂Ń��[�v
                        while (TemplateMain.Count > i && TemplateMain[i] != "</TEMPLATE>")
                            i++;
                        i++;

                        // �|�[�g�����[�v
                        foreach (Kagami k in Front.KagamiList)
                        {
                            // �����ڑ��̓G���g�����X�ɕ\�����Ȃ�
                            if (k.Status.Type == 0)
                                continue;

                            // <KAGAMI>��������܂Ń��[�v
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

                            // <KAGAMI>��������Ȃ�����
                            if (TemplateMain.Count <= i)
                                break;

                            i++;

                            if (k.Status.ImportURL == "�ҋ@��" && k.Status.Type != 2)
                            {//�ҋ@��
                                str_tmp = str_wai;
                            }
                            else if (!k.Status.ImportStatus || (k.Status.Type == 2 && k.Status.ImportURL == "�ҋ@��"))
                            {//Import�ɕ����͓����Ă��邪�AImport�t���O��false
                             //�܂��́APush�z�M�v���̑҂��󂯒�
                                str_tmp = str_try;
                            }
                            else
                            {//����ɉғ���
                                str_tmp = str_con;
                            }

                            // HTML���o��
                            tw.WriteLine(
                                str_tmp
                                .Replace("<COUNT>", count.ToString())
                                .Replace("<MY_URL>", Front.Hp.IpHTTP + ":" + k.Status.MyPort.ToString())
                                .Replace("<PORT>", k.Status.MyPort.ToString())
                                .Replace("<SRC_URL>", (k.Status.UrlVisible ? k.Status.ImportURL : "�ݒ肪��\���ɂȂ��Ă��܂�"))
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
                                .Replace("<LIVE_URLO>", ((k.Status.Url != "") ? "����URL" : ""))
                                .Replace("<ERROR>", k.Status.ImportErrorContext)
                                .Replace("<RETRY_COUNT>", (k.Status.Type == 2 ? "" : k.Status.RetryCounter.ToString() + "/" + Front.Retry.OutRetryTime.ToString()))
                            );
                            count++;
                        }

                        // �K�v��<KAGAMI>��u����������̏���
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
                    Front.AddLogDebug("HttpHybrid", "main.html�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/conn.html?"))
            {
                #region /conn.html�ւ̃��N�G�X�g
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

                        // ���݁[��[��
                        while (true)
                        {
                            if (dic.ContainsKey("open"))
                            {
                                #region �ڑ���ʕ\��
                                int port = int.Parse(dic["open"]);
                                Kagami k = Front.IndexOf(port);
                                if (k == null)
                                {
                                    comment = "���̃|�[�g�͐ڑ��\��Ԃł͂���܂���";
                                    break;
                                }
                                if (dic.ContainsKey("admin"))
                                {
                                    // �Ǘ��Ґڑ����[�h
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
                                    // �ʏ�ڑ����[�h
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
                                #region �ڑ��v����M
                                if (dic["Port"].Length == 0 || dic["Port"].Length >= 6)
                                {
                                    comment = "�|�[�g����ł�";
                                    break;
                                }
                                // �t�H�[�����͓��e�`�F�b�N
                                if (dic["mode"] != "pull" && dic["mode"] != "push")
                                {
                                    comment = "���̓G���[";
                                    break;
                                }
                                // push�z�M��address��disabled�ɂ���ƃu���E�U���瑗���Ă��Ȃ��̂ŁA
                                // push�z�M����dic["address"]�̃`�F�b�N�͍s��Ȃ�
                                if (dic["mode"] == "pull")
                                {
                                    if (dic["address"].Length == 0 ||
                                        dic["address"].IndexOf("<") != -1 ||
                                        dic["address"].IndexOf(">") != -1 ||
                                        dic["address"].IndexOf("\"") != -1)
                                    {
                                        comment = "���̓G���[/�s���ȃA�h���X�ł�";
                                        break;
                                    }
                                }

                                if (dic["comment"].IndexOf("<") != -1 ||
                                    dic["comment"].IndexOf(">") != -1 ||
                                    dic["comment"].IndexOf("\"") != -1 ||
                                    dic["comment"].IndexOf("+ADw-") != -1 ||    // UTF-7 XSS
                                    dic["comment"].IndexOf("+AD4-") != -1)      // UTF-7 XSS
                                {
                                    comment = "���̓G���[/�s���ȃR�����g�ł�";
                                    break;
                                }
                                if (dic["password"].Length == 0)
                                {
                                    comment = "���̓G���[/�p�X���[�h���ݒ�";
                                    break;
                                }

                                // ����URL�͑����Ă��Ă���΃`�F�b�N����
                                if (dic.ContainsKey("url") && dic["url"].Length > 0)
                                {
                                    if (!dic["url"].StartsWith(Front.Opt.OutUrl) ||
                                        dic["url"].IndexOf("<") != -1 ||
                                        dic["url"].IndexOf(">") != -1 ||
                                        dic["url"].IndexOf("\"") != -1 ||
                                        dic["url"].IndexOf("+ADw-") != -1 ||    // UTF-7 XSS
                                        dic["url"].IndexOf("+AD4-") != -1)     // UTF-7 XSS)
                                    {
                                        comment = "���̓G���[/�s���Ȏ���URL�ł�";
                                        break;
                                    }
                                }

                                //�Y���|�[�g���ڑ��\���`�F�b�N
                                Kagami k = Front.IndexOf(int.Parse(dic["Port"]));
                                if (k == null ||
                                    k.Status.ImportURL != "�ҋ@��" ||
                                    k.Status.Type == 2 ||
                                    ((Front.Pause || k.Status.Pause) && (!dic.ContainsKey("admin") || dic["admin"] == "" || dic["admin"] != Front.Opt.AdminPass)))
                                {
                                    comment = "���̃|�[�g�͐ڑ��\��Ԃł͂���܂���";
                                    break;
                                }

                                //�z�M��ʂŐU�蕪��
                                if (dic["mode"] == "pull")
                                {
                                    // pull�z�M
                                    if (k.Status.DisablePull)
                                    {
                                        comment = "Pull�z�M����";
                                        break;
                                    }
                                    string imp = "";
                                    //�C���|�[�g��z�X�g���擾
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
                                    // �ڑ������z�X�g�ƈ�v���邩�`�F�b�N
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
                                        comment = "�ڑ�����/���̃z�X�g�ւ̐ڑ��͋֎~����Ă��܂�";
                                        break;
                                    }
                                    // ����z�X�g�ڑ�������𒴂��Ă��Ȃ����`�F�b�N
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
                                        comment = "�ڑ�����/���̃z�X�g�ւ̐ڑ����������l�𒴂��Ă��܂�";
                                        break;
                                    }
                                    // �ݒ��IP�ƈ�v���邩�`�F�b�N
                                    if (Front.Acl.SetUserIpCheck)
                                    {
                                        // �ݒ�҂����[�J���A�h���X�Ȃ�ꗥ���e
                                        if (request.RemoteAddr.StartsWith("10.") ||         // ClassA
                                            request.RemoteAddr.StartsWith("172.16.") ||     // ClassB
                                            request.RemoteAddr.StartsWith("192.168.") ||    // ClassC
                                            request.RemoteAddr.StartsWith("127."))          // LoopBack
                                        {
                                            // ok
                                        }
                                        else
                                        {
                                            // �C���|�[�gURL��IP�A�h���X�����߂�
                                            System.Net.IPAddress hostadd;
                                            try
                                            {
                                                hostadd = System.Net.Dns.GetHostAddresses(imp)[0];
                                                if (hostadd.ToString() != request.RemoteAddr)
                                                {
                                                    comment = "�ڑ�����/�C���|�[�gURL�Ɛݒ��IP����v���܂���";
                                                    break;
                                                }
                                            }
                                            catch
                                            {
                                                comment = "�ڑ�����/�C���|�[�gURL����IP�A�h���X�ɕϊ��ł��܂���";
                                                break;
                                            }
                                        }
                                    }

                                    ////////////////////////////////
                                    // �`�F�b�NOK�B�ڑ����ݒ�J�n

                                    // ���U�[�u���͈�x���ׂď����ĐV�K�ݒ�
                                    Event.EventUpdateReserve(k, null, 2);   // GUI��ListView����ListViewItem�S�폜
                                    k.Status.Gui.ReserveItem.Clear();       // ReserveItem��ListViewItem�S�폜

                                    // ���U�[�u�Đݒ�
                                    if (dic["reserve"].Length > 0)
                                    {
                                        Regex r = new Regex(",");
                                        string[] reserve_list = r.Split(dic["reserve"], k.Status.Reserve + 1);
                                        for (int cnt = 0; cnt < k.Status.Reserve && cnt < reserve_list.Length; cnt++)
                                        {
                                            // IP�ɕϊ��ł��Ȃ��z�X�g���Ȃ�o�^�������
                                            try
                                            {
                                                System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(reserve_list[cnt])[0];
                                                k.Status.AddReserve(hostadd.ToString());
                                            }
                                            catch { }
                                        }
                                    }

                                    k.Status.Type = 1;  // pull�z�M
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
                                    Front.AddLogData(1, k.Status, "�O������ڑ��v������M���܂��� / �v����IP:" + request.RemoteAddr);
                                    Front.AddLogData(1, k.Status, "URL=" + k.Status.ImportURL + " / �R�����g=" + k.Status.Comment + " / ����URL=" + k.Status.Url);
                                    comment = "���M����";
                                }
                                else
                                {
                                    // push�z�M
                                    if (!Front.Opt.EnablePush)
                                    {
                                        comment = "Push�z�M����";
                                        break;
                                    }

                                    ////////////////////////////////
                                    // �`�F�b�NOK�B�ڑ����ݒ�J�n

                                    // ���U�[�u���͈�x���ׂď����ĐV�K�ݒ�
                                    Event.EventUpdateReserve(k, null, 2);   // GUI��ListView����ListViewItem�S�폜
                                    k.Status.Gui.ReserveItem.Clear();       // ReserveItem��ListViewItem�S�폜

                                    // ���U�[�u�Đݒ�
                                    if (dic["reserve"].Length > 0)
                                    {
                                        Regex r = new Regex(",");
                                        string[] reserve_list = r.Split(dic["reserve"], k.Status.Reserve + 1);
                                        for (int cnt = 0; cnt < k.Status.Reserve && cnt < reserve_list.Length; cnt++)
                                        {
                                            // IP�ɕϊ��ł��Ȃ��z�X�g���Ȃ�o�^�������
                                            try
                                            {
                                                System.Net.IPAddress hostadd = System.Net.Dns.GetHostAddresses(reserve_list[cnt])[0];
                                                k.Status.AddReserve(hostadd.ToString());
                                            }
                                            catch { }
                                        }
                                    }

                                    k.Status.Type = 2;  // push�z�M
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
                                    Front.AddLogData(1, k.Status, "�O������Push�z�M�҂��󂯎w������M���܂��� / �v����IP:" + request.RemoteAddr);
                                    Front.AddLogData(1, k.Status, "�R�����g=" + k.Status.Comment + " / ����URL=" + k.Status.Url);
                                    comment = "���M����";
                                }
                                #endregion // �ڑ��ݒ�v�����M
                                break;
                            }
                            comment = "�s���ȃG���[�ł�";
                            // �_�~�[���[�v�I��
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
                    Front.AddLogDebug("HttpHybrid", "conn.html�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/dis.html?"))
            {
                #region /dis.html�ւ̃��N�G�X�g
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
                            #region �ؒf��ʕ\��
                            int port = int.Parse(dic["dis"]);
                            foreach (string s in TemplateDis)
                            {
                                tw.WriteLine(s.Replace("<PORT>", port.ToString()));
                            }
                            #endregion
                        }
                        else if (dic.ContainsKey("password"))
                        {
                            #region �ؒf�v����M
                            Kagami k = Front.IndexOf(int.Parse(dic["Port"]));
                            // �N�����|�[�g���A
                            // push�z�M�̏ꍇ�͑ҋ@���E�ڑ����֌W�Ȃ��ؒf�\
                            // push�z�M�ȊO�̏ꍇ�́A�ҋ@���ȊO�Ȃ�ؒf�\
                            if (k != null && (k.Status.Type == 2 || k.Status.ImportURL != "�ҋ@��"))
                            {
                                if ((k.Status.Password == dic["password"]) ||
                                    (Front.Opt.AdminPass != "" && Front.Opt.AdminPass == dic["password"]))
                                {
                                    if (k.Status.Type == 2 && k.Status.ImportURL == "�ҋ@��")
                                        k.Status.Type = 1;  // Push�z�M�v���҂����̐ؒf�v��
                                    else
                                        k.Status.Disc();    // ����ڑ����̃C���|�[�g�ؒf
                                    Front.AddLogData(1, k.Status, "�O������ؒf�v������M���܂��� / �v����IP:" + request.RemoteAddr);
                                    comment = "���M����";
                                }
                                else
                                {
                                    comment = "�p�X���[�h����v���܂���";
                                }
                            }
                            else
                            {
                                comment = "���̃|�[�g�͐ؒf�\��Ԃł͂���܂���";
                            }
                            #endregion
                        }
                        else
                        {
                            comment = "�s���ȃG���[�ł�";
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
                    Front.AddLogDebug("HttpHybrid", "dis.html�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/auth.html?"))
            {
                #region /auth.html�ւ̃��N�G�X�g
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
                            #region �F�؉�ʕ\��
                            int port = int.Parse(dic["port"]);
                            Kagami k = Front.IndexOf(int.Parse(dic["port"]));
                            if (k != null && (k.Status.Type == 2 || k.Status.ImportURL != "�ҋ@��"))
                            {
                                foreach (string s in TemplateAuth)
                                    tw.WriteLine(s.Replace("<PORT>", port.ToString()));
                            }
                            else
                            {
                                comment = "���̃|�[�g�͐ڑ��\��Ԃł͂���܂���";
                            }
                            #endregion
                        }
                        else
                        {
                            comment = "�s���ȃG���[�ł�";
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
                    Front.AddLogDebug("HttpHybrid", "auth.html�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            else if (request.URI.StartsWith("/set.html?"))
            {
                #region /set.html�ւ̃��N�G�X�g
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

                        //�_�~�[���[�v
                        while (true)
                        {
                            if (dic.ContainsKey("port") &&
                                dic.ContainsKey("password") &&
                                !dic.ContainsKey("reserve"))
                            {
                                #region �ݒ�ύX��ʕ\��
                                int port = int.Parse(dic["port"]);
                                Kagami k = Front.IndexOf(port);
                                if (k == null || (k.Status.Type != 2 && k.Status.ImportURL == "�ҋ@��"))
                                {
                                    comment = "���̃|�[�g�͐ڑ��\��Ԃł͂���܂���";
                                    break;
                                }

                                if (k.Status.Password != dic["password"] &&
                                    (Front.Opt.AdminPass == "" || Front.Opt.AdminPass != dic["password"]))
                                {
                                    comment = "�p�X���[�h����v���܂���";
                                    break;
                                }

                                ///////////////////////
                                // �ݒ�ύX�J�n
                                // �ݒ�ς݃��U�[�u���X�g���J���}��؂蕶����ɂ���
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
                                #region �ݒ�ύX�v����M
                                int port = int.Parse(dic["port"]);
                                Kagami k = Front.IndexOf(port);
                                if (k == null ||
                                    (k.Status.Type != 2 && k.Status.ImportURL == "�ҋ@��"))
                                {
                                    comment = "���̃|�[�g�͐ڑ��\��Ԃł͂���܂���";
                                    break;
                                }
                                if (k.Status.Password != dic["password"] &&
                                    (Front.Opt.AdminPass == "" || Front.Opt.AdminPass != dic["password"]))
                                {
                                    comment = "�p�X���[�h����v���܂���";
                                    break;
                                }

                                // ���U�[�uIP���X�g�̐���`�F�b�N
                                // �󂯎�������X�g���A�������͍ő僊�U�[�u�o�^���̏��������̒l�Ń��[�v
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
                                    // DNS�G���[�̏ꍇ�X�V�m�f�ɂ���
                                    comment = "���̓G���[/�s���ȃ��U�[�uIP�ł�";
                                    break;
                                }

                                if (dic["new_password"].Length == 0)
                                {
                                    comment = "���̓G���[/�V�p�X���[�h����ł�";
                                    break;
                                }
                                if (dic["comment"].IndexOf("<") >= 0 ||
                                    dic["comment"].IndexOf(">") >= 0 ||
                                    dic["comment"].IndexOf("\"") >= 0 ||
                                    dic["comment"].IndexOf("+ADw-") >= 0 ||    // UTF-7 XSS
                                    dic["comment"].IndexOf("+AD4-") >= 0 ||    // UTF-7 XSS
                                    (dic["radio"] != "on" && dic["radio"] != "off"))
                                {
                                    comment = "���̓G���[/�s���ȃR�����g�ł�";
                                    break;
                                }

                                // ����URL�͑����Ă��Ă���΃`�F�b�N����
                                if (dic.ContainsKey("url") && dic["url"].Length > 0)
                                {
                                    if (!dic["url"].StartsWith(Front.Opt.OutUrl) ||
                                        dic["url"].IndexOf("<") != -1 ||
                                        dic["url"].IndexOf(">") != -1 ||
                                        dic["url"].IndexOf("\"") != -1 ||
                                        dic["url"].IndexOf("+ADw-") != -1 ||    // UTF-7 XSS
                                        dic["url"].IndexOf("+AD4-") != -1)     // UTF-7 XSS)
                                    {
                                        comment = "���̓G���[/�s���Ȏ���URL�ł�";
                                        break;
                                    }
                                }

                                /////////////////////
                                // �ݒ�ύX�J�n
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
                                // ���U�[�u���͈�x���ׂď����ĐV�K�ݒ�
                                Event.EventUpdateReserve(k, null, 2);   // GUI��ListView����ListViewItem�S�폜
                                k.Status.Gui.ReserveItem.Clear();       // ReserveItem��ListViewItem�S�폜
                                // �󂯎�������X�g���A�������͍ő僊�U�[�u�o�^���̏��������̒l�Ń��[�v
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
                                        // ��Ƀz�X�g���`�F�b�N���Ă邩�炱���ɂ͗��Ȃ��͂��B�B
                                        Front.AddLogDebug("HttpHybrid", "�ݒ�ύX�G���[(�����G���[:" + e.Message + "/Trace:" + e.StackTrace + ")");
                                    }
                                }
                                comment = "���M����";
                                #endregion
                                break;
                            }
                            comment = "�s���ȃG���[�ł�";
                            // �_�~�[���[�v�I��
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
                    Front.AddLogDebug("HttpHybrid", "set.html�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            // css�̓e���v���[�g���L��΃A�N�Z�X�\
            else if (request.URI.StartsWith("/kagamin.css") && TemplateCss.Count > 0)
            {
                #region /kagamin.css�ւ̃��N�G�X�g
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
                    Front.AddLogDebug("HttpHybrid", "kagamin.css�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            // admin.html�L�����̂݃A�N�Z�X�ł���
            else if (request.URI.StartsWith("/admin.html") && Front.Opt.EnableAdmin)
            {
                #region /admin.html�ւ̃A�N�Z�X
                response.ContentType = "text/html; charset=UTF-8";
                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                try
                {
                    using (Stream ostr = response.Send())
                    using (TextWriter tw = new StreamWriter(ostr))
                    {
                        // �_�~�[���[�v
                        while (true)
                        {
                            if (!request.URI.StartsWith("/admin.html?"))
                            {
                                //�N�G���Ȃ� - �F�؉�ʂ�
                                foreach (string s in TemplateAdminAuth)
                                    tw.WriteLine(s);
                                break;
                            }

                            string comment = "";
                            string str = request.URI.Replace("/admin.html?", "");
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            try
                            {
                                // �N�G������
                                str = HttpUtility.UrlDecode(str);
                                string[] str2 = str.Split('&');
                                foreach (string s in str2)
                                    dic.Add(s.Split('=')[0], s.Split('=')[1]);
                            }
                            catch
                            {
                                //�N�G������s�\ - �F�؉�ʂ�
                                foreach (string s in TemplateAdminAuth)
                                    tw.WriteLine(s);
                                break;
                            }

                            // �p�X���[�h�N�G�������邩�H
                            if (!dic.ContainsKey("password"))
                            {
                                //�p�X���[�h�Ȃ� - �F�؉�ʂ�
                                foreach (string s in TemplateAdminAuth)
                                    tw.WriteLine(s);
                                break;
                            }

                            // �p�X���[�h��v����
                            if (dic["password"] == "" || dic["password"] != Front.Opt.AdminPass)
                            {
                                //�p�X���[�h����A�܂��͕s��v
                                foreach (string s in TemplateAdminOk)
                                    tw.WriteLine(s
                                        .Replace("<Status>", "�p�X���[�h����v���܂���")
                                        .Replace("<ADMIN_PASS>", "")
                                    );
                                break;
                            }
                            if (!dic.ContainsKey("mode"))
                            {
                                #region �Ǘ���ʃg�b�v�\��
                                // main.html���p�N���A�ǉ��^�O��p�ӂ���
                                int i = 0;
                                int count = 1;

                                string str_dmt = "";    // ���N����ԃe���v���[�g
                                string str_wai = "";    // �ҋ@����ԃe���v���[�g
                                string str_try = "";    // ���s����ԃe���v���[�g
                                string str_con = "";    // �ڑ�����ԃe���v���[�g
                                string str_tmp = "";    // ���ۂɏo�͂���e���v���[�g����

                                // �]���ʌv�Z
                                string _ul_day, _dl_day, _ul_mon, _dl_mon;
                                _ul_day = ((ulong)(Front.Log.TrsUpDay + Front.TotalUP)).ToString("#,##0,,MB");
                                _dl_day = ((ulong)(Front.Log.TrsDlDay + Front.TotalDL)).ToString("#,##0,,MB");
                                _ul_mon = ((ulong)(Front.Log.TrsUpMon + Front.TotalUP)).ToString("#,##0,,MB");
                                _dl_mon = ((ulong)(Front.Log.TrsDlMon + Front.TotalDL)).ToString("#,##0,,MB");

                                // <TEMPLATE>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<TEMPLATE>")
                                {
                                    tw.WriteLine(TemplateAdminMain[i]
                                        .Replace("<VERSION>", Front.AppName)
                                        .Replace("<PAUSE>", Front.Pause ? "�ꎞ��~��" : "�����Ȃ�") // �Ǘ����[�h�ł͏�ԕ\��
                                        .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "�J�n��" : "��~��")
                                        .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)
                                        .Replace("<MAXCONN>", Front.Gui.Conn.ToString() + "+" + Front.Gui.Reserve.ToString())
                                        .Replace("<TRF_UP_DAY>", _ul_day)
                                        .Replace("<TRF_DL_DAY>", _dl_day)
                                        .Replace("<TRF_UP_MON>", _ul_mon)
                                        .Replace("<TRF_DL_MON>", _dl_mon)
                                    );
                                    i++;
                                }

                                #region �e���v���[�g�Ǎ�
                                // <���N���\��>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<���N���\��>")
                                    i++;
                                i++;
                                // </���N���\��>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</���N���\��>")
                                {
                                    // �|�[�g���X�g�ɓo�^����Ă��関�N���|�[�g�̕\���p�^�O���擾
                                    str_dmt += TemplateAdminMain[i] + "\r\n";
                                    i++;
                                }

                                // <�ҋ@���\��>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<�ҋ@���\��>")
                                    i++;
                                i++;
                                // </�ҋ@���\��>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</�ҋ@���\��>")
                                {
                                    // �ҋ@���̕\���p�^�O���擾
                                    str_wai += TemplateAdminMain[i] + "\r\n";
                                    i++;
                                }

                                // <�ڑ����s���\��>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<�ڑ����s���\��>")
                                    i++;
                                i++;
                                // </�ڑ����s���\��>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</�ڑ����s���\��>")
                                {
                                    // �ڑ����s���̕\���p�^�O���擾
                                    str_try += TemplateAdminMain[i] + "\r\n";
                                    i++;
                                }

                                // <�ڑ����\��>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<�ڑ����\��>")
                                    i++;
                                i++;
                                // </�ڑ����\��>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</�ڑ����\��>")
                                {
                                    // �ڑ����s���̕\���p�^�O���擾
                                    str_con += TemplateAdminMain[i] + "\r\n";
                                    i++;
                                }
                                #endregion

                                // </TEMPLATE>��������܂Ń��[�v
                                while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "</TEMPLATE>")
                                    i++;
                                i++;

                                Kagami k;
                                // �o�^�|�[�g�ƋN�����|�[�g�̂Q���ꏏ�Ƀ��[�v
                                for (int cnt = 0; cnt < Front.Gui.PortList.Count + Front.KagamiList.Count; cnt++)
                                {
                                    if (cnt < Front.Gui.PortList.Count)
                                    {
                                        // �ŏ��ɁA�o�^�|�[�g�̒����疢�N���|�[�g�̈ꗗ�o��
                                        k = Front.IndexOf(Front.Gui.PortList[cnt]);
                                        if (k != null)
                                            continue;  // �N�����|�[�g�͑ΏۊO
                                    }
                                    else
                                    {
                                        // ���̌�A�N�����|�[�g�̈ꗗ�o��
                                        k = Front.KagamiList[cnt - Front.Gui.PortList.Count];
                                    }

                                    // �Ǘ����[�h�̏ꍇ�A�����ڑ����\��
                                    /*
                                    // �����ڑ��̓G���g�����X�ɕ\�����Ȃ�
                                    if (k.Status.Type == 0)
                                        continue;
                                    */
                                    // <KAGAMI>��������܂Ń��[�v
                                    while (TemplateAdminMain.Count > i && TemplateAdminMain[i] != "<KAGAMI>")
                                    {
                                        tw.WriteLine(TemplateAdminMain[i]
                                            .Replace("<VERSION>", Front.AppName)
                                            .Replace("<PAUSE>", Front.Pause ? "�ꎞ��~��" : "�����Ȃ�") // �Ǘ����[�h�ł͏�ԕ\��
                                            .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "�J�n��" : "��~��")
                                            .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)
                                            .Replace("<MAXCONN>", Front.Gui.Conn.ToString() + "+" + Front.Gui.Reserve.ToString())
                                            .Replace("<TRF_UP_DAY>", _ul_day)
                                            .Replace("<TRF_DL_DAY>", _dl_day)
                                            .Replace("<TRF_UP_MON>", _ul_mon)
                                            .Replace("<TRF_DL_MON>", _dl_mon)
                                        );
                                        i++;
                                    }

                                    // <KAGAMI>��������Ȃ�����
                                    if (TemplateAdminMain.Count <= i)
                                        break;

                                    i++;

                                    if (k == null)
                                        str_tmp = str_dmt;  //���N��
                                    else if (k.Status.ImportURL == "�ҋ@��")
                                        str_tmp = str_wai;  //�ҋ@��
                                    else if (k.Status.ImportStatus == false)
                                        str_tmp = str_try;  //�ڑ����s��(ImportURL�ɕ����͓����Ă��邪�AImport�t���O��false)
                                    else
                                        str_tmp = str_con;  //����ɉғ���

                                    // HTML���o��
                                    if (k == null)
                                    {
                                        // ���N���|�[�g��k.Status�����܂Ȃ������̂݃T�|�[�g
                                        tw.WriteLine(
                                            str_tmp
                                            .Replace("<VERSION>", Front.AppName)
                                            .Replace("<PAUSE>", Front.Pause ? "�ꎞ��~��" : "�����Ȃ�") // �Ǘ����[�h�ł͏�ԕ\��
                                            .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "�J�n��" : "��~��")
                                            .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)       // �Ǘ����[�h�ǉ��^�O
                                            .Replace("<MAXCONN>", Front.Gui.Conn.ToString() + "+" + Front.Gui.Reserve.ToString())
                                            .Replace("<COUNT>", count.ToString())
                                            .Replace("<PORT>", Front.Gui.PortList[cnt].ToString())
                                        );
                                    }
                                    else
                                    {
                                        // �N�����|�[�g�̏�ԕ\��
                                        tw.WriteLine(
                                            str_tmp
                                            .Replace("<VERSION>", Front.AppName)
                                            .Replace("<PAUSE>", Front.Pause ? "�ꎞ��~��" : "�����Ȃ�") // �Ǘ����[�h�ł͏�ԕ\��
                                            .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "�J�n��" : "��~��")
                                            .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)       // �Ǘ����[�h�ǉ��^�O
                                            .Replace("<COUNT>", count.ToString())
                                            .Replace("<MY_URL>", Front.Hp.IpHTTP + ":" + k.Status.MyPort.ToString())
                                            .Replace("<PORT>", k.Status.MyPort.ToString())
                                            //.Replace("<SRC_URL>", (k.Status.UrlVisible ? k.Status.ImportURL : "�ݒ肪��\���ɂȂ��Ă��܂�"))
                                            .Replace("<SRC_URL>", k.Status.ImportURL) // �Ǘ����[�h�ł͏�ɕ\��
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

                                // �K�v��<KAGAMI>��u����������̏���
                                while (TemplateAdminMain.Count > i)
                                {
                                    tw.WriteLine(TemplateAdminMain[i]
                                        .Replace("<KAGAMI>", "")        // <KAGAMI>�^�O��NULL��
                                        .Replace("<VERSION>", Front.AppName)
                                        .Replace("<PAUSE>", Front.Pause ? "�ꎞ��~��" : "�����Ȃ�") // �Ǘ����[�h�ł͏�ԕ\��
                                        .Replace("<BAND>", Front.BndWth.EnableBandWidth ? "�J�n��" : "��~��")
                                        .Replace("<ADMIN_PASS>", Front.Opt.AdminPass)       // �Ǘ����[�h�ǉ��^�O
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
                                #region �ݒ�ύX
                                // �p�X���[�h�ƃ��[�h�͊m�F�ς݁B
                                // �|�[�g���󂯎��Ȃ�A�ʂŃ`�F�b�N���邱�ƁB
                                int _port, _conn, _resv;
                                Kagami _k;
                                switch (dic["mode"])
                                {
                                    case "start":
                                        #region �w��|�[�g�N��
                                        if (!dic.ContainsKey("port") || !dic.ContainsKey("conn"))
                                        {
                                            comment = "�K�v�ȃp�����[�^���ݒ肳��Ă��܂���";
                                            break;
                                        }
                                        try
                                        {
                                            _port = int.Parse(dic["port"]);
                                        }
                                        catch
                                        {
                                            comment = "�|�[�g��񂪈ُ�ł�";
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
                                            comment = "�ڑ��g�ݒ肪�ُ�ł�";
                                            break;
                                        }
                                        _k = Front.IndexOf(_port);
                                        if (_k != null)
                                        {
                                            comment = "���̃|�[�g�͊��ɋN�����ł�";
                                            break;
                                        }
                                        // �҂��󂯊J�n�ݒ�
                                        _k = new Kagami("", _port, _conn, _resv);
                                        // �ʑш�ݒ�̏ꍇ�A����ш��ݒ肵�Ă���
                                        if (Front.BndWth.BandStopMode == 2)
                                        {
                                            _k.Status.GUILimitUPSpeed = (int)Front.BndWth.BandStopValue;
                                            _k.Status.LimitUPSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                                        }
                                        Front.Add(_k);
                                        comment = "Port:" + dic["port"] + " �҂��󂯊J�n����";
                                        #endregion
                                        break;
                                    case "startall":
                                        #region �S�|�[�g�N��
                                        if (!dic.ContainsKey("conn"))
                                        {
                                            comment = "�K�v�ȃp�����[�^���ݒ肳��Ă��܂���";
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
                                            comment = "�ڑ��g�ݒ肪�ُ�ł�";
                                            break;
                                        }
                                        foreach (int _i in Front.Gui.PortList)
                                        {
                                            _k = Front.IndexOf(_i);
                                            if (_k == null)
                                            {
                                                // �҂��󂯊J�n�ݒ�
                                                _k = new Kagami("", _i, _conn, _resv);
                                                // �ʑш�ݒ�̏ꍇ�A����ш��ݒ肵�Ă���
                                                if (Front.BndWth.BandStopMode == 2)
                                                {
                                                    _k.Status.GUILimitUPSpeed = (int)Front.BndWth.BandStopValue;
                                                    _k.Status.LimitUPSpeed = Front.CnvLimit((int)Front.BndWth.BandStopValue, (int)Front.BndWth.BandStopUnit);
                                                }
                                                Front.Add(_k);
                                            }
                                        }
                                        //�f�t�H���g�ڑ������X�V
                                        Front.Gui.Conn = (uint)_conn;
                                        Front.Gui.Reserve = (uint)_resv;
                                        comment = "�S�|�[�g�҂��󂯊J�n����";
                                        #endregion
                                        break;
                                    case "stop":
                                        #region �w��|�[�g��~
                                        if (!dic.ContainsKey("port"))
                                        {
                                            comment = "�|�[�g�ԍ����w�肳��Ă��܂���";
                                            break;
                                        }
                                        try
                                        {
                                            _port = int.Parse(dic["port"]);
                                        }
                                        catch
                                        {
                                            comment = "�|�[�g�ԍ��ُ�";
                                            break;
                                        }
                                        _k = Front.IndexOf(_port);
                                        if (_k == null)
                                        {
                                            comment = "���̃|�[�g�͑҂��󂯒���Ԃł͂���܂���";
                                            break;
                                        }
                                        // �҂��󂯒�~�ݒ�
                                        _k.Status.Disc();
                                        _k.Status.RunStatus = false;
                                        comment = "Port:" + dic["port"] + " �҂��󂯒�~����";
                                        #endregion
                                        break;
                                    case "stopall":
                                        #region �S�|�[�g��~
                                        foreach (Kagami _k_tmp in Front.KagamiList)
                                        {
                                            _k_tmp.Status.Disc();
                                            _k_tmp.Status.RunStatus = false;
                                        }
                                        comment = "�S�|�[�g�҂��󂯒�~����";
                                        #endregion
                                        break;
                                    case "chg":
                                        #region �g���̕ύX
                                        if (!dic.ContainsKey("port") || !dic.ContainsKey("conn"))
                                        {
                                            comment = "�K�v�ȃp�����[�^���ݒ肳��Ă��܂���";
                                            break;
                                        }
                                        try
                                        {
                                            _port = int.Parse(dic["port"]);
                                        }
                                        catch
                                        {
                                            comment = "�|�[�g�ԍ��ُ�";
                                            break;
                                        }
                                        _k = Front.IndexOf(_port);
                                        if (_k == null)
                                        {
                                            comment = "���̃|�[�g�͖��N���ł�";
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
                                            comment = "�ڑ��g�ݒ肪�ُ�ł�";
                                            break;
                                        }
                                        _k.Status.Conn_UserSet = _conn;
                                        _k.Status.Reserve = _resv;
                                        Event.EventUpdateKagami();
                                        comment = "�g����ύX���܂���(PORT:" + _port + "/" + _conn + "+" + _resv + ")";
                                        #endregion
                                        break;
                                    case "pause":
                                        #region �V�K�ڑ��̈ꎞ��~�E�ꎞ��~����
                                        Front.Pause = !Front.Pause;
                                        Event.EventUpdateKagami();
                                        if (Front.Pause)
                                            comment = "�V�K�ڑ��̐������J�n���܂���";
                                        else
                                            comment = "�V�K�ڑ��̐������������܂���";
                                        #endregion
                                        break;
                                    case "band":
                                        #region �ш搧���̊J�n�E��~
                                        Front.BndWth.EnableBandWidth = !Front.BndWth.EnableBandWidth;
                                        Event.EventUpdateKagami();
                                        if (Front.BndWth.EnableBandWidth)
                                            comment = "�ш搧�����J�n���܂���";
                                        else
                                            comment = "�ш搧�����~���܂���";
                                        #endregion
                                        break;
                                    case "dis":
                                        #region �w��|�[�g�����ؒf
                                        if (!dic.ContainsKey("port"))
                                        {
                                            comment = "�|�[�g�ԍ����w�肳��Ă��܂���";
                                            break;
                                        }
                                        // �ؒf�̂��߂̒l�ݒ�
                                        try
                                        {
                                            _port = int.Parse(dic["port"]);
                                        }
                                        catch
                                        {
                                            comment = "�|�[�g�ԍ����ُ�ł�";
                                            break;
                                        }
                                        Kagami k = Front.IndexOf(_port);
                                        if (k == null || (k.Status.Type != 2 && k.Status.ImportURL == "�ҋ@��"))
                                        {
                                            comment = "���̃|�[�g�͐ؒf�\��Ԃł͂���܂���";
                                            break;
                                        }
                                        // �C���|�[�g�ؒf
                                        k.Status.Disc();
                                        comment = "Port:" + dic["port"] + " �ؒf����";
                                        #endregion
                                        break;
                                    case "disall":
                                        #region �S�|�[�g�����ؒf
                                        foreach (Kagami _k_tmp in Front.KagamiList)
                                        {
                                            // �C���|�[�g�ؒf
                                            _k_tmp.Status.Disc();
                                        }
                                        comment = "�S�|�[�g�ؒf����";
                                        #endregion
                                        break;
                                    default:
                                        comment = "�s���ȃ��[�h�ł�";
                                        break;
                                }
                                // ���b�Z�[�W�o��
                                foreach (string s in TemplateAdminOk)
                                    tw.WriteLine(s
                                        .Replace("<Status>", comment)
                                        .Replace("<ADMIN_PASS>", dic["password"])
                                    );
                                #endregion
                            }
                            // �_�~�[���[�v�I��
                            break;
                        }// end of while(true)
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "admin.html�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
            // info.html�L�����̂݃A�N�Z�X�ł���
            else if (request.URI.StartsWith("/info.html") && Front.Opt.EnableInfo)
            {
                #region /info.html�ւ̃��N�G�X�g
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
                            // �����ڑ��͕\�������Ȃ��c����ς�\������B
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
                    Front.AddLogDebug("HttpHybrid", "info.html�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
/*
            else if (request.URI.StartsWith("/rss.rdf") && Front.Opt.EnableRss)
            {
                #region /rss.rdf�ւ̃��N�G�X�g
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
                            // �����ڑ��͕\�������Ȃ�
                            if (k.Status.Type == 0)
                                continue;
                            tw.WriteLine("<item rdf:about=\"" + HPAddr + "#he" + now.ToString("yyyyMMdd") + "\">");
                            tw.WriteLine("<title>" + k.Status.MyPort + "</title>");
                            tw.WriteLine("<link>" + HPAddr + "</link>");
                            tw.WriteLine("<dc:date>" + now.ToString("yyyy-MM-dd") + "T" + now.ToString("HH:mm:ss") + "+09:00</dc:date>");
                            if (Front.Pause)
                            {
                                tw.WriteLine("<description>�V�K��t�������ł��B</description>");
                            }
                            if (k.Status.ImportURL == "�ҋ@��" && k.Status.Type != 2)
                            {
                                //�ҋ@��
                                tw.WriteLine("<description>�g�p�\�ł��B</description>");
                            }
                            else if (!k.Status.ImportStatus || (k.Status.Type == 2 && k.Status.ImportURL == "�ҋ@��"))
                            {//Import�ɕ����͓����Ă��邪�AImport�t���O��false
                                //�܂��́APush�z�M�v���̑҂��󂯒�
                                tw.WriteLine("<description>�ڑ����s���܂��̓v�b�V���z�M�ڑ��ҋ@���ł��B</description>");
                            }
                            else
                            {
                                //����ɉғ���
                                tw.WriteLine("<description>�g�p���ł��BCOMMENT:" + k.Status.Comment + "</description>");
                            }
                            tw.WriteLine("</item>");
                        }
                        tw.WriteLine("</rdf:RDF>");
                    }
                }
                catch (Exception e)
                {
                    Front.AddLogDebug("HttpHybrid", "rss.rdf�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
                }
                #endregion
            }
 */
            else
            {
                #region ���̑�URI�ւ̃��N�G�X�g
                bool _not_found = true;
                //�_�~�[���[�v
                while(true)
                {
                    // ���JDir���ݒ肳��ĂȂ����404
                    if (Front.Hp.PublicDir == "")
                        break;
                    
                    string _path = HttpUtility.UrlDecode(request.URI);
                    _path = Front.Hp.PublicDir + _path.Replace('\\', '/');
                    _path = Path.GetFullPath(_path);
                    if (!_path.StartsWith(Front.Hp.PublicDir))
                        break;

                    // ���΃p�X����t�@�C�����Ɗg���q���o��
                    string _ext = Path.GetExtension(_path);

                    // �t�@�C���`�F�b�N�B�������404�B
                    if (!File.Exists(_path))
                        break;

                    ////////////////////
                    #region �t�@�C���𔭌������̂ő��M����
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
                            // 1000byte���������M
                            // �ŏ��̂P��ڂ�ContentsType����
                            if (_remain > 0)
                            {
                                if (_remain > 1000)
                                    _read = fs.Read(_buf, 0, 1000);
                                else
                                    _read = fs.Read(_buf, 0, (int)_remain);
                                // ContentsType����
                                switch (_ext)
                                {
                                    #region �g���q���̔���
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
                                        //�Ή��O�̊g���q�̏ꍇ�A�o�C�i�����ǂ����̔�����s��
                                        //���炩�̕����R�[�h���������text/plain�ő��o
                                        Encoding enc = Front.GetCode(_buf, _read);
                                        if (enc == null)
                                            response.ContentType = "application/octet-stream";
                                        else
                                            response.ContentType = "text/plain";
                                        break;
                                }
                                // �P��ڏo��
                                response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
                                ostr.Write(_buf, 0, _read);
                                _remain -= _read;

                            }
                            // �Q��ڈȍ~�o��
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
                    // �_�~�[���[�v�I��
                    break;
                }
                if (_not_found)
                {
                    #region �t�@�C����������Ȃ����� 404 Not Found
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
                        Front.AddLogDebug("HttpHybrid", "NOT_FOUND�����G���[:" + e.Message + "/Trace:" + e.StackTrace);
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