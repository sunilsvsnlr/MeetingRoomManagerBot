using MeetingRoomManagerLUIS.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MeetingRoomManagerLUIS.HttpWrapper
{
    public class HttpCalls
    {
        HttpClient client;
        private string frameURI = string.Empty;
        public HttpCalls()
        {
            string serverPath = Utilities.ServerUrl;
            string hostedPath = Utilities.HostedService;
            frameURI = serverPath + "/" + hostedPath + "/";
            client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(15);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Uri baseAddress = new Uri(frameURI);
            client.BaseAddress = baseAddress;
        }

        public HttpResponseMessage Get(string url, out string errorMessage)
        {
            errorMessage = string.Empty;
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            else
            {
                errorMessage = response.Content.ReadAsStringAsync().Result;
                //log here
            }
            return null;
        }

        public HttpResponseMessage Delete(string url,out string errorMessage)
        {
            errorMessage = string.Empty;
            HttpResponseMessage response = client.DeleteAsync(url).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            else
            {
                errorMessage = response.Content.ReadAsStringAsync().Result;
                //log here
            }
            return null;
        }

        public T Get<T>(string uri, string userName, string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            byte[] authBytes = Encoding.UTF8.GetBytes((userName + ":" + password).ToCharArray());
            if (!client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(authBytes));
            }
            HttpResponseMessage response = client.GetAsync(uri).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.ReadAsAsync<T>().Result;
            }
            else
            {
                errorMessage = response.Content.ReadAsStringAsync().Result;
                return default(T);
                //log here
            }
        }

        public string Get(string uri, string userName, string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            byte[] authBytes = Encoding.UTF8.GetBytes((userName + ":" + password).ToCharArray());
            if (!client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(authBytes));
            }
            HttpResponseMessage response = client.GetAsync(uri).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                errorMessage = response.Content.ReadAsStringAsync().Result;
                return string.Empty;
                //log here
            }
        }

        public HttpResponseMessage Post<T>(string url, T postData, out string errorMessage)
        {
            errorMessage = string.Empty;
            HttpResponseMessage response = client.PostAsJsonAsync<T>(url, postData).Result;
            return response;
        }
    }
}
