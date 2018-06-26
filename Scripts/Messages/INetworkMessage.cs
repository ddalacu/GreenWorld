using System;
using System.Text;

namespace GreenProject.Messages
{
    /// <summary>
    /// Diac Paul
    /// </summary>
    public interface INetworkMessage
    {
        byte[] SerializeData();
        void DeserializeData(byte[] data);
    }

    public static class MessageExtensions
    {
        //https://www.codeproject.com/Articles/34309/Convert-String-to-bit-Integer
        private static Int64 GetInt64HashCode(string strText)
        {
            Int64 hashCode = 0;
            if (!string.IsNullOrEmpty(strText))
            {
                //Unicode Encode Covering all characterset
                byte[] byteContents = Encoding.Unicode.GetBytes(strText);
                System.Security.Cryptography.SHA256 hash =
                    new System.Security.Cryptography.SHA256CryptoServiceProvider();
                byte[] hashText = hash.ComputeHash(byteContents);
                //32Byte hashText separate
                //hashCodeStart = 0~7  8Byte
                //hashCodeMedium = 8~23  8Byte
                //hashCodeEnd = 24~31  8Byte
                //and Fold
                Int64 hashCodeStart = BitConverter.ToInt64(hashText, 0);
                Int64 hashCodeMedium = BitConverter.ToInt64(hashText, 8);
                Int64 hashCodeEnd = BitConverter.ToInt64(hashText, 24);
                hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            }
            return (hashCode);
        }

        public static long GetMessageUniqueIdentifier<T>() where T : INetworkMessage
        {
            return GetInt64HashCode(typeof(T).FullName);
        }
    }

}
