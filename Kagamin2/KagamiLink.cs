using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Kagamin2
{
    public class KagamiLink
    {

        private Status _status;
        /// <summary>
        /// 鏡間リンクソケット
        /// </summary>
        private Socket Sock = null;
        /// <summary>
        /// 鏡クライアントクラス
        /// </summary>
        private ClientData Client = null;
        /// <summary>
        /// UTF8
        /// </summary>
        private Encoding enc = Encoding.Default;
        /// <summary>
        /// 鏡間リンク確立フラグ
        /// </summary>
        private bool _connected = false;
        /// <summary>
        /// 鏡間リンクが確立してるかどうか
        /// </summary>
        public bool Connected
        {
            get { return _connected; }

        }
        /// <summary>
        /// 受信ループスレッド
        /// </summary>
        private Thread RecvingLoop;
        /// <summary>
        /// 鏡間リンク受信開始
        /// </summary>
        /// <param name="_s"></param>
        /// <param name="_sock"></param>
        public KagamiLink(Status _s, Socket _sock)
        {
            _connected = true;
            Sock = _sock;
            _status = _s;
            RecvingLoop = new Thread(Recving);
            RecvingLoop.IsBackground = true;
            RecvingLoop.Start();

        }
        /// <summary>
        /// 鏡クライアントへ鏡間リンクを接続に行く
        /// </summary>
        /// <param name="_s"></param>
        /// <param name="_client"></param>
        public KagamiLink(Status _s, ClientData _client)
        {
            _status = _s;
            Client = _client;
            Thread thread = new Thread(_connection);
            thread.Start();
        }
        /// <summary>
        /// 鏡間リンクコネクション開始プロトコル
        /// </summary>
        private string _connect_msg
        {
            get
            {
                return "CONNECT kagami:" + Client.Ip + ":" + Client.KagamiPort + " KAGAMI/1.0\r\n" +
                   "Host: " + Client.Ip + ":" + Client.KagamiPort + "\r\n" +
                   "User-Agent: " + Front.UserAgent + "/StatusChecker\r\n\r\n";
            }
        }
        /// <summary>
        /// 送られてきたチャットを他の鏡へ転送する
        /// </summary>
        /// <param name="_msg"></param>
        private void TransChat(byte[] _msg)
        {
            if (_status.IKLink != this)
            {
                _status.IKLink.Send(_msg);
            }
            _status.Client.TransChat(this, _msg);

        }

        private void _connection()
        {
            Front.AddLogDebug("kagamilink", "鏡リンクスレッド開始");
            Thread.Sleep(10000);
            Front.AddLogData(0, _status, "鏡間リンクを開始します");

            try
            {
                //Socketの作成
                Sock = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                    System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                IPAddress hostadd = Dns.GetHostAddresses(Client.Ip)[0];


                IPEndPoint ephost = new System.Net.IPEndPoint(hostadd, Client.KagamiPort);

                Sock.SendTimeout = (int)Front.Sock.SockConnTimeout;       // Import接続 ヘッダ取得要求送信のタイムアウト値
                Sock.ReceiveTimeout = (int)Front.Sock.SockConnTimeout;    // Import接続 ヘッダ取得応答受信のタイムアウト値

                //接続
                Sock.Connect(ephost);
                Sock.Send(enc.GetBytes(_connect_msg));

                #region KAGAMI応答受信
                //まずはHTTP応答ヘッダまで取得
                byte[] ack = new byte[1];
                byte[] ack_end = { 0x0a, 0x0a }; // '\n', '\n'
                byte[] ack_log = new byte[50000];
                byte[] sts_code = new byte[3];

                int i = 0;
                int count = 0;
                while (_status.RunStatus)
                {
                    Sock.Receive(ack);
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
                        // KAGAMI StatusCode取得
                        // 9～11文字目を取得
                        // 0123456789abcde
                        // KAGAMI/1.x 200 OK
                        int http_status = 0;
                        sts_code[0] = ack_log[11];
                        sts_code[1] = ack_log[12];
                        sts_code[2] = ack_log[13];

                        try
                        {
                            Front.AddLogDetail("RecvRspMsg(Head)Sta-----\r\n" +
                                         System.Text.Encoding.ASCII.GetString(ack_log, 0, count) +
                                         "\r\nRecvRspMsg(Head)End-----");

                            http_status = int.Parse(System.Text.Encoding.ASCII.GetString(sts_code));
                        }
                        catch
                        {
                            Front.AddLogData(0, _status, "鏡間リンクに失敗しました(StatusCode異常)");
                            return;
                        }
                        if (http_status == 200)
                        {
                            break;
                        }
                        else
                        {
                            Front.AddLogData(0, _status, "鏡間リンクを停止しました/Status=" + http_status);
                            return;
                        }                        


                    }
                    else if (count >= 50000)
                    {
                        Front.AddLogData(0, _status, "鏡間リンクに失敗しました(50KB>OVER)");
                        Front.AddLogDetail("RecvRspMsg(Head)Sta-----\r\n" +
                                     System.Text.Encoding.ASCII.GetString(ack_log, 0, count) +
                                     "\r\nRecvRspMsg(Head)End-----");
                        return;

                    }
                }
                #endregion
                _connected = true;
                RecvingLoop = new Thread(Recving);
                RecvingLoop.IsBackground = true;
                RecvingLoop.Start();
            }
            catch (Exception ex)
            {
                Front.AddLogData(1, _status, "鏡間リンクの確立に失敗しました(" + ex.Message + ")");
                _connected = false;
                return;
            }



        }
        //接続状況を送信
        private void ConSend()
        {

        }
        private void Send(string _content)
        {
            Send(enc.GetBytes(_content));

        }
        public void Send(byte[] _byte)
        {
            try
            {
                Sock.Send(_byte);
                Front.AddLogDebug("LINK_SEND", "送信データ:" + BitConverter.ToString(_byte) + "\r\n");
            }
            catch (Exception ex)
            {
                _connected = false;
                Front.AddLogData(0, _status, "鏡間リンクが切断されました/" + ex.Message);
            }
        }
        /// <summary>
        /// 接続情報の解析(未実装)
        /// </summary>
        /// <param name="_recv"></param>
        private void TransIni(byte[] _recv)
        {

        }
        private DateTime _dt = DateTime.Now;
        private void Recving()
        {
            Front.AddLogDebug("LINK_RECV", "受信ループ開始");

            //メッセージ全体の長さ。受信完了時に0にする
            int _size = 0;
            byte[] recv = new byte[1];//受信用バッファ
            int _count = 0;//受信オフセット
            byte[] buffer = null;
            while (_status.ImportStatus && _connected)
            {

                if (Sock.Available > 0)
                {

                    //BufferSize
                    if (_size > 0)
                    {//メッセージ途中からの場合
                        Sock.Receive(recv);
                        buffer[_count] = recv[0];
                        _count++;

                        //受信完了
                        if (_count == _size)
                        {
                            _size = 0;


                            int kind = buffer[2] << 2 | buffer[3];

                            Front.AddLogDebug("LINK_RECV", "RECV:" + enc.GetString(buffer));
                            switch (kind)
                            {
                                case 2:
                                    Front.AddLogData(0, _status, "鏡間リンク完了");
                                    break;
                                case 9:
                                    int len = buffer[4] >> 2 | buffer[5];

                                    string _msg = enc.GetString(buffer, 9, buffer.Length - 9);
                                    Front.AddLogData(0, _status, "チャットを受信:" + _msg);
                                    TransChat(buffer);
                                    Front.AddLogDebug("LINK_RECV", "メッセージ:" + BitConverter.ToString(buffer));
                                    break;
                                case 7:
                                    //Front.AddLogData(0, _status, "接続確認を受信");
                                    break;
                                case 8:
                                    TransIni(buffer);
                                    break;
                                default:
                                    Front.AddLogData(0, _status, "未実装の鏡間リンクメッセージです(ProtcolNo=" + kind + ")");
                                    Front.AddLogDebug("LINK_RECV", "未実装メッセージ:" + BitConverter.ToString(buffer));
                                    break;

                            }

                            buffer = null;
                            _count = 0;
                        }
                    }
                    else
                    {//メッセージ最初からの場合
                        if (Sock.Available < 2)
                            continue;//最低バイト数来てなかったらスルー
                        byte[] msg_size = new byte[2];
                        Sock.Receive(msg_size, 0, 2, SocketFlags.None);
                        _size = msg_size[0] << 2 | msg_size[1];

                        buffer = new byte[_size];
                        buffer[0] = msg_size[0];
                        buffer[1] = msg_size[1];
                        _count = 2;
                    }


                }
                else
                {
                    if ((DateTime.Now - _dt).TotalSeconds > 30)
                    {
                        //定常通信?メッセージ
                        byte[] _byte = { 0x00, 0x04, 0x00, 0x07 };
                        Send(_byte);
                        _dt = DateTime.Now;

                    }
                    Thread.Sleep(500);
                }

            }
            Front.AddLogDebug("LINK_RECV", "受信ループ終了");

        }

    }
}
