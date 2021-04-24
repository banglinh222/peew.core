using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace peew.core
{
    public class Peew
    {
        private HttpClient client = new HttpClient();
        private string BaseUrl;

        public Peew(string domain)
        {
            BaseUrl = $"https://{domain}";
        }

        public string getMachineId()
        {
            var json = post($"{BaseUrl}/machine-id");
            return json.machineId;
        }

        public void upload(string filePath, string fileName, string machineId)
        {
            var data = new Dictionary<string, string>
            {
                { "fileName", fileName },
                { "machineId", machineId }
            };
            var postPolicy = post(BaseUrl, data);
            string url = postPolicy.url;
            Dictionary<string, string> fields = postPolicy.fields.ToObject<Dictionary<string, string>>();
            var result = post(url, fields, filePath);
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            File.Delete(filePath);
            Debug.WriteLine(filePath);
            Debug.WriteLine(fileName);
            OpenUrl($"{BaseUrl}/{fileName}?machineId={machineId}");
        }

        public dynamic post(string url, Dictionary<string, string> data = null)
        {
            var convertedData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return post(url, convertedData).Result;
        }

        public dynamic post(string url, Dictionary<string, string> data, string filePath)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            foreach(KeyValuePair<string, string> kv in data)
            {
                form.Add(new StringContent(kv.Value), kv.Key);
            }
            var fileStream = new FileStream(filePath, FileMode.Open);
            form.Add(new StreamContent(fileStream), "file");
            return post(url, form).Result;
        }

        public async Task<dynamic> post(string url, HttpContent data)
        {
            var response = await client.PostAsync(url, data).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Debug.WriteLine(responseString);
            var json = JsonConvert.DeserializeObject(responseString);
            return json;
        }

        public string MD5FileName()
        {
            string input = $"{System.Guid.NewGuid()}";
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
