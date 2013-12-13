﻿using Rareform.Validation;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Espera.Services
{
    public static class FogBugzService
    {
        public static async Task SubmitReport(string message)
        {
            if (message == null)
                Throw.ArgumentNullException(() => message);

            string url = "https://espera.fogbugz.com/scoutSubmit.asp";
            string userName = "Dennis Daume";
            string project = "Espera";
            string area = "CrashReports";
            string body = message;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            string parameters = string.Empty;
            parameters += "Description=" + HttpUtility.UrlEncode(body);
            parameters += "&ScoutUserName=" + HttpUtility.UrlEncode(userName);
            parameters += "&ScoutProject=" + HttpUtility.UrlEncode(project);
            parameters += "&ScoutArea=" + HttpUtility.UrlEncode(area);
            parameters += "&ForceNewBug=" + "1";

            byte[] bytes = Encoding.ASCII.GetBytes(parameters);
            request.ContentLength = bytes.Length;

            using (Stream os = request.GetRequestStream())
            {
                await os.WriteAsync(bytes, 0, bytes.Length);
            }

            using (await request.GetResponseAsync())
            { }
        }
    }
}