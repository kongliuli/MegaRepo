using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json;

namespace xfyun.Helper
{
    public class HttpHelper
    {
        private readonly string _appId;
        private readonly string _secret;
        private readonly ApiAuthAlgorithm _apiAuthAlgorithm;

        public HttpHelper(string appId,string secret)
        {
            _appId=appId;
            _secret=secret;
            _apiAuthAlgorithm=new ApiAuthAlgorithm();
        }

        public async Task<string> SendAsyncPostRequest(string url,Dictionary<string,string> postData)
        {
            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(postData);
            var timespan = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = new ApiAuthAlgorithm().GetSignature(_appId,_secret,timespan);
            client.DefaultRequestHeaders.Add("appId",_appId);
            client.DefaultRequestHeaders.Add("timestamp",timespan.ToString());
            client.DefaultRequestHeaders.Add("signature",signature);

            try
            {
                HttpResponseMessage response = await client.PostAsync(url,content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch(Exception ex)
            {
                // Handle exception
                return ex.Message;
            }
        }

        public string SendPostRequest(string url,Dictionary<string,string> postData)
        {
            try
            {
                using var client = new HttpClient();
                var postDataJson = JsonConvert.SerializeObject(postData); // 假设你使用的是Newtonsoft.Json
                var content = new StringContent(postDataJson,Encoding.UTF8,"application/json");
                var timespan = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var signature = new ApiAuthAlgorithm().GetSignature(_appId,_secret,timespan);

                client.DefaultRequestHeaders.Add("appId",_appId);
                client.DefaultRequestHeaders.Add("timestamp",timespan.ToString());
                client.DefaultRequestHeaders.Add("signature",signature);

                // 确保Content-Type头正确设置为application/json
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.PostAsync(url,content).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch(AggregateException ae) when(ae.InnerException is HttpRequestException)
            {
                // 捕获HttpRequestException异常
                return $"Request failed: {ae.InnerException.Message}";
            }
            catch(TaskCanceledException)
            {
                // 捕获由于超时导致的异常
                return "The request was canceled due to a timeout.";
            }
            catch(Exception ex)
            {
                // 捕获其他所有异常
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
