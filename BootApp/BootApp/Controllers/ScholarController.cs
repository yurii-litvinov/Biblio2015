using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net;
using System.IO;

namespace BootApp.Controllers
{
    public class ScholarController : Controller
    {
        //
        // GET: /Scholar/

        public string GetScholarPage()
        {
            WebRequest req = WebRequest.Create("https://scholar.google.ru");
            req.Method = "POST";
            req.GetRequestStream();
            string html;
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    html = reader.ReadToEnd();
                }
            }
            StringWriter writer = new StringWriter();
            return HttpUtility.HtmlEncode(html);
        }
    }
}
