using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;

namespace Neo.Consensus
{
    /// <summary>
    /// ��ʶ���������ģ���¼��ǰ��ʶ���Ϣ
    /// </summary>
    public interface IConsensusContext : IDisposable
    {
        //public const uint Version = 0;
        /// <summary>
        /// ������ʶ����״̬
        /// </summary>
        ConsensusState State { get; set; }
        /// <summary>
        /// ��һ��block��hash
        /// </summary>
        UInt256 PrevHash { get; }
        /// <summary>
        /// �᰸block������߶�
        /// </summary>
        uint BlockIndex { get; }
        /// <summary>
        /// ��ǰ��ͼ�ı��
        /// </summary>
        byte ViewNumber { get; }
        /// <summary>
        /// ���ֹ�ʶ�ڵ�Ĺ�Կ�б�
        /// </summary>
        ECPoint[] Validators { get; }
        /// <summary>
        /// ��ǰ�ڵ��ţ���Validators���������
        /// </summary>
        int MyIndex { get; }
        /// <summary>
        /// ���ֹ�ʶ���鳤���
        /// </summary>
        uint PrimaryIndex { get; }
        /// <summary>
        /// ��ǰ�᰸blockʱ���
        /// </summary>
        uint Timestamp { get; set; }
        /// <summary>
        /// ��ǰ�᰸block��nonce
        /// </summary>
        ulong Nonce { get; set; }
        /// <summary>
        /// ��ǰ�᰸block��NextConsensus, ָ����һ�ֹ�ʶ�ڵ�
        /// </summary>
        UInt160 NextConsensus { get; set; }
        /// <summary>
        /// ��ǰ�᰸block�Ľ���hash�б�
        /// </summary>
        UInt256[] TransactionHashes { get; set; }
        /// <summary>
        /// ��ǰ�᰸block�Ľ���
        /// </summary>
        Dictionary<UInt256, Transaction> Transactions { get; set; }
        /// <summary>
        /// ����յ����᰸block��ǩ������
        /// </summary>
        byte[][] Signatures { get; set; }
        /// <summary>
        /// �յ��ĸ��ڵ�������ͼ��ţ���Ҫ���ڸı���ͼ�����С���������ÿһλ��Ӧÿ����֤�˽ڵ��ڴ�����ͼ��š�����֤��������Ӧ��ŵġ�
        /// </summary>
        byte[] ExpectedView { get; set; }
        /// <summary>
        /// ��͹�ʶ�ڵ㰲ȫ��ֵ���������ڸ���ֵ����ʶ���̽������
        /// </summary>
        int M { get; }
        /// <summary>
        /// ǰ���������ͷ
        /// </summary>
        Header PrevHeader { get; }
        /// <summary>
        /// �ж��Ƿ����ָ����ϣֵ�Ľ���
        /// </summary>
        /// <param name="hash">���׵Ĺ�ϣֵ</param>
        /// <returns>�ж��Ƿ����</returns>
        bool ContainsTransaction(UInt256 hash);
        /// <summary>
        /// ��ָ֤���Ľ����Ƿ�Ϸ�
        /// </summary>
        /// <param name="tx">ָ���Ľ���</param>
        /// <returns>�Ϸ����׷���true</returns>
        bool VerifyTransaction(Transaction tx);
        /// <summary>
        /// ������ͼ
        /// </summary>
        /// <param name="view_number">�µ���ͼ���</param>
        void ChangeView(byte view_number);
        /// <summary>
        /// ��������
        /// </summary>
        /// <returns>�´���������</returns>
        Block CreateBlock();

        //void Dispose();
        /// <summary>
        /// �����鳤���
        /// </summary>
        /// <param name="view_number">��ǰ��ͼ���</param>
        /// <returns>�µ��鳤���</returns>
        uint GetPrimaryIndex(byte view_number);
        /// <summary>
        /// ����ChangeView��Ϣ
        /// </summary>
        /// <returns>��ʶ��Ϣ(ChangeView)</returns>
        ConsensusPayload MakeChangeView();
        /// <summary>
        /// ����һ��ֻ��������ͷ�������н������ݵĿ�����
        /// </summary>
        /// <returns>ֻ��������ͷ������</returns>
        Block MakeHeader();
        /// <summary>
        /// ǩ������ͷ
        /// </summary>
        void SignHeader();
        /// <summary>
        /// ����PrepareRequset�Ĺ�ʶ��ϢConsensusPayload��
        /// </summary>
        /// <returns>PrepareRequset��Ϣ</returns>
        ConsensusPayload MakePrepareRequest();
        /// <summary>
        /// ����PrepareResponse�Ĺ�ʶ��ϢConsensusPayload��
        /// </summary>
        /// <param name="signature">���᰸block��ǩ��</param>
        /// <returns>��ʶ��Ϣ</returns>
        ConsensusPayload MakePrepareResponse(byte[] signature);
        /// <summary>
        /// ��������������
        /// </summary>
        void Reset();
        /// <summary>
        /// ����᰸block������
        /// </summary>
        void Fill();
        /// <summary>
        /// �յ�PrepareRequest����У��PrepareRequest�������᰸block����
        /// </summary>
        /// <returns>��֤�Ϸ��Ժ󷵻�true</returns>
        bool VerifyRequest();
    }
}
