using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace BoardGamesExtractor
{
    public static class GetWebPages
    {
        public static string GetWebPage(string Address, Encoding enc, out bool res, out string msg)
        {
            res = true; msg = "";
            string html = "";
            try
            {
                HttpWebRequest proxy_request = (HttpWebRequest)WebRequest.Create(Address);
                proxy_request.Method = "GET";
                proxy_request.ContentType = "application/x-www-form-urlencoded";
                proxy_request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/532.5 (KHTML, like Gecko) Chrome/4.0.249.89 Safari/532.5";
                proxy_request.KeepAlive = true;
                HttpWebResponse resp = proxy_request.GetResponse() as HttpWebResponse;
                using (StreamReader sr = new StreamReader(resp.GetResponseStream(), enc /*Encoding.GetEncoding(1251)*/))
                    html = sr.ReadToEnd();
                html = html.Trim();
            }
            catch (Exception ex)
            {
                res = false;
                msg = ex.Message;
            }
            return html;
        }
    }
}