using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace peew
{
    public class MachineId
    {
        private static readonly HttpClient client = new HttpClient();
        public static async Task<string> get()
        {
            var response = await client.PostAsync("https://dev.peew.me/machine-id", null).ConfigureAwait(false);
            string str_json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            dynamic json = JsonConvert.DeserializeObject(str_json);
            return json.machineId;
        }
    }
}
