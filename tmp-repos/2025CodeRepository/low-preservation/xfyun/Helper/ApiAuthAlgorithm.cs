using System.Security.Cryptography;
using System.Text;

namespace xfyun.Helper
{
    public class ApiAuthAlgorithm
    {
        private static readonly byte[] MD5_TABLE = {
        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
        0x38, 0x39, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66
        };

        public string GetSignature(string appId,string secret,long timestamp)
        {
            string auth = Md5(appId+timestamp);
            return HmacSha1Encrypt(auth,secret);
        }

        private string Md5(string cipherText)
        {
            using(var md5 = MD5.Create())
            {
                byte[] data = Encoding.UTF8.GetBytes(cipherText);
                byte[] hash = md5.ComputeHash(data);

                StringBuilder hashStringBuilder = new StringBuilder();
                for(int i = 0;i<hash.Length;i++)
                {
                    hashStringBuilder.Append(MD5_TABLE[hash[i]>>4&0x0F]);
                    hashStringBuilder.Append(MD5_TABLE[hash[i]&0x0F]);
                }
                return hashStringBuilder.ToString();
            }
        }

        private string HmacSha1Encrypt(string encryptText,string encryptKey)
        {
            using(var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(encryptKey)))
            {
                byte[] encryptedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(encryptText));
                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }
}
