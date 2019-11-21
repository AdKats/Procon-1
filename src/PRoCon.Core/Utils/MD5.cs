using System;
using System.Text;

namespace PRoCon.Core.Utils
{
    public static class MD5
    {

        static readonly System.Security.Cryptography.MD5 Hasher = System.Security.Cryptography.MD5.Create();

        public static String File(String path)
        {
            if (System.IO.File.Exists(path))
            {
                return Data(System.IO.File.ReadAllBytes(path));
            }
            return string.Empty;
        }

        public static String Data(byte[] data)
        {
            StringBuilder stringifyHash = new StringBuilder();

            byte[] hash = Hasher.ComputeHash(data);
            for (int x = 0; x < hash.Length; x++)
            {
                stringifyHash.Append(hash[x].ToString("x2"));
            }

            return stringifyHash.ToString();
        }

        public static String String(String data)
        {
            return MD5.Data(Encoding.ASCII.GetBytes(data));
        }

        public static Guid Guid(String data)
        {
            return new Guid(Hasher.ComputeHash(Encoding.ASCII.GetBytes(data)));
        }
    }
}
