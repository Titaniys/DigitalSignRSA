using System;
using System.Security.Cryptography;

namespace RSA
{
    class RSA
    {
        /// <summary>
        /// ������ n
        /// </summary>
        public readonly BigInt32 n;
        
        /// <summary>
        /// �������� ����
        /// </summary>
        public readonly BigInt32 e;
     
        /// <summary>
        /// �������� ����
        /// </summary>
        public readonly BigInt32 d;
        
        /// <summary>
        /// �����������. ���������� � ��������� �����. 
        /// </summary>
        public RSA()
        {
            Random uy = new Random();

            const int KeyOFLenght = 128; // ����� p � q
            BigInt32 p = BigInt32.GetPseudoPrime(KeyOFLenght, 10, uy);
            BigInt32 q = BigInt32.GetPseudoPrime(KeyOFLenght - 1, 10, uy);
            //Console.WriteLine("q = {0}, p = {1}", q, p);
            n = p * q;
            //Console.WriteLine("n= {0}", n);
            p--;
            q--;
            BigInt32 w = p * q;                    // ������� ������

            e = new BigInt32();                   // �������� ����
            
            do
            {
                e.GetRandomBits(KeyOFLenght * 2, uy);
            }
            while (BigInt32.NOD(e, w) != 1 && (e > n));

            d = BigInt32.Inverse(e, w);           // d - �������� ���� (�������� � � �� ������ w)
        }                                         // d - ���������� <=> ���(e,w) = 1  

        /// <summary>
        /// ��������� ��� �������� ������ ��������� �������� ����������� MD5
        /// </summary>
        /// <param name="data">�������� ������</param>
        /// <returns>128-������ ���</returns>
        private static byte[] MD5hash(byte[] data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(data);
        }   
        /// <summary>
        /// ������������ ��� ���������-Text
        /// </summary>
        /// <param name="Text">���������</param>
        /// <returns>�������</returns>
        public static BigInt32 CreateSignature(byte[]Text, BigInt32 d, BigInt32 n)
        {
            byte[] hash = MD5hash(Text);
            BigInt32 BI_Text = new BigInt32(hash);
            return BI_Text.modPow(d, n);
        }

        /// <summary>
        /// �������� �������� ������� Signature ������ Text
        /// </summary>
        /// <param name="Text">�����</param>
        /// <param name="Signature">�������</param>
        /// <param name="e">�������� ����</param>
        /// <param name="n">������</param>
        /// <returns>true ���� ������� �����, false � �������� ������</returns>
        public static bool VerifySignature(byte[] Text, BigInt32 Signature, BigInt32 e, BigInt32 n)
        {
            BigInt32 R = new BigInt32(MD5hash(Text));
            //Console.WriteLine(R);
            BigInt32 S = new BigInt32(Signature.modPow(e,n));
            //Console.WriteLine(S);
            if (R==S)
                return true;
            return false;
        }
    }
}
