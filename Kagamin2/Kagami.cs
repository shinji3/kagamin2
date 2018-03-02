using System;
using System.Collections.Generic;
using System.Text;

namespace Kagamin2
{
    /// <summary>
    /// ���Ǘ��N���X
    /// </summary>
    public class Kagami
    {
        #region �����o�ϐ�
        public Status Status;
        private Import Import;
        private Export Export;
        #endregion

        /// <summary>
        /// �R���X�g���N�^
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
        /// ���ڑ����I������
        /// </summary>
        public void Stop()
        {
            Status.Disc();
            Status.RunStatus = false;
        }
    }
}
