using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web;
using System.Web.Script.Serialization;

namespace Common.Helpers
{
    public class RestHelper
    {
        public T Post<T, M>(M parameterModel, string url)

        {
            T responseData;
            var wi = (WindowsIdentity)HttpContext.Current.User.Identity;
            var wic = wi.Impersonate();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UseDefaultCredentials = true;
            request.Method = HttpMethod.Post.ToString();
            request.ContentType = "application/json; charset=utf-8";
            JavaScriptSerializer jScriptSerializer = new JavaScriptSerializer();
            string json = jScriptSerializer.Serialize(parameterModel);
            StreamWriter writerObject = new StreamWriter(request.GetRequestStream());
            writerObject.Write(json); writerObject.Close();
            string response = GetResponse(request, wic);

            // deserialize the response content          
            responseData = jScriptSerializer.Deserialize<T>(response);
            return responseData;



        }
        public T Put<T, M>(M parameterModel, string url)
        {
            var wi = (WindowsIdentity)HttpContext.Current.User.Identity;
            var wic = wi.Impersonate();
            T responseData;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UseDefaultCredentials = true;
            request.Method = HttpMethod.Put.ToString();
            request.ContentType = "application/json; charset=utf-8";
            JavaScriptSerializer jScriptSerializer = new JavaScriptSerializer();
            string json = jScriptSerializer.Serialize(parameterModel);
            StreamWriter writerObject = new StreamWriter(request.GetRequestStream());
            writerObject.Write(json);
            writerObject.Close();
            string response = GetResponse(request, wic);

            // deserialize the response content          
            responseData = jScriptSerializer.Deserialize<T>(response);
            return responseData;

        }
        public T Get<T>(string url) where T : class
        {
            T responseData = null;
            try
            {
                var wi = (WindowsIdentity)HttpContext.Current.User.Identity;
                var wic = wi.Impersonate();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UseDefaultCredentials = true;
                request.Method = HttpMethod.Get.ToString();
                request.ContentType = "application/json; charset=utf-8";
                string response = GetResponse(request, wic);

                // deserialize the response content     
                JavaScriptSerializer jScriptSerializer = new JavaScriptSerializer();
                jScriptSerializer.MaxJsonLength = Int32.MaxValue;
                // deserialize the response content          
                responseData = jScriptSerializer.Deserialize<T>(response);
            }
            catch (Exception e)
            {

            }
            return responseData;
        }

        private string GetResponse(HttpWebRequest request, WindowsImpersonationContext wic)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string responseContent = string.Empty;
            string responseStatatusCode = string.Empty;
            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                responseStatatusCode = response.StatusCode.ToString();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream);
                        responseContent = reader.ReadToEnd();
                        return responseContent;
                    }
                }
            }
            catch (WebException webException)
            {
                wic.Undo();
                using (var reader = new StreamReader(webException.Response.GetResponseStream()))
                {
                    responseContent = reader.ReadToEnd();
                }
                return responseContent;
            }
            return responseContent;
        }

    }
}