using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DingTalkLib
{
    public interface IDingTalkClient
    {
        public string UriStr
        {
            get; set;
        }

        public string Token
        {
            get; set;
        }

        public string SecretInfo
        {
            get; set;
        }

        public string TotalUri
        {
            get; set;
        }
        public MessageType msgtype
        {
            get; set;
        }

        public DingTalkAtSetting? at
        {
            get; set;
        }
    }

    public class MarkDownDingTalkClient:IDingTalkClient
    {
        public MarkDownDingTalkClient(MessageContent markDown,string uriStr,string token,string secretInfo,DingTalkAtSetting? at,MessageType msgtype = MessageType.Markdown)
        {
            MarkDown=markDown??throw new ArgumentNullException(nameof(markDown));
            UriStr=uriStr??throw new ArgumentNullException(nameof(uriStr));
            Token=token??throw new ArgumentNullException(nameof(token));
            SecretInfo=secretInfo??throw new ArgumentNullException(nameof(secretInfo));
            this.msgtype=msgtype;
            this.at=at;
        }

        public MessageContent MarkDown
        {
            get; set;
        }

        public Task<HttpResponseMessage> Response
        {
            get; set;
        }
        #region 继承自接口
        public string UriStr
        {
            get; set;
        }
        public string Token
        {
            get; set;
        }

        public string SecretInfo
        {
            get; set;
        }
        public MessageType msgtype
        {
            get; set;
        }
        public DingTalkAtSetting? at
        {
            get; set;
        }
        public string TotalUri
        {
            get;
            set;
        }
        #endregion


        public MarkDownDingTalkClient IGetPostUri()
        {
            string sign = "";
            long timestamp = (long)(DateTime.UtcNow-new DateTime(1970,1,1)).TotalMilliseconds;

            string secret = "SECac5f99d07087195fecc94ac270bbf5d76b8da57a5e4499c047474b851e71fcc2";
            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);

            string string_to_sign = $"{timestamp}\n{secret}";
            byte[] stringToSignBytes = Encoding.UTF8.GetBytes(string_to_sign);

            using(var hmac = new HMACSHA256(secretBytes))
            {
                byte[] hmacCode = hmac.ComputeHash(stringToSignBytes);
                sign=Uri.EscapeDataString(Convert.ToBase64String(hmacCode));
            }

            TotalUri=$"{UriStr}?access_token={Token}&timestamp={timestamp}&sign={sign}";
            return this;
        }

        public MarkDownDingTalkClient SendMessageAsync()
        {
            var client = new HttpClient();
            var value = new
            {
                msgtype = "markdown",
                markdown = MarkDown,
                at = at
            };
            var content = new StringContent(JsonSerializer.Serialize(value),Encoding.UTF8,"application/json");
            var response = client.PostAsync(TotalUri,content).Result;

            if(response.IsSuccessStatusCode)
            {
                // 处理成功的响应
                var responseContent = response.Content.ReadAsStringAsync().Result;

            }
            else
            {
            }
            return this;
        }

    }

    public enum MessageType
    {
        Text, Markdown, ActionCard, FeedCard, Link
    }


}