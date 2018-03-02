using System;
using System.Collections.Generic;
using System.Text;

namespace Kagamin2
{
    /// <summary>
    /// 鏡管理クラス
    /// </summary>
    public class Kagami
    {
        #region メンバ変数
        public Status Status;
        private Import Import;
        private Export Export;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_importURL"></param>
        /// <param name="_myPort"></param>
        /// <param name="_connection"></param>
        /// <param name="_reserve"></param>
        public Kagami(string _importURL, int _myPort, int _connection, int _reserve)
        {
            Status = new Status(this, _importURL, _myPort, _connection, _reserve);
            Import = new Import(Status);
            Export = new Export(Status);
        }

        /// <summary>
        /// 鏡接続を終了する
        /// </summary>
        public void Stop()
        {
            Status.Disc();
            Status.RunStatus = false;
        }
    }
}
