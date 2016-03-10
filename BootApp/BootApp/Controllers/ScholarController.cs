using System.Collections.Generic;
using System.Web.Mvc;
using BootApp.Models;
using BootApp.Parsing;

namespace BootApp.Controllers
{
    public class ScholarController : Controller
    {
        //List<ScholarArticle> articles = new List<ScholarArticle>();

        //
        // GET: /Scholar/

       

        public ActionResult SearchOnScholar()
        {
            List<ScholarArticle> articles = new List<ScholarArticle>();
            return View("SearchOnScholar", articles);
        }

        [HttpPost]
        public ActionResult SearchOnScholar(string Query)
        {
            ParseMethod parsing = new ParseMethod();
            List<ScholarArticle> articles = parsing.GetScholarArticlesByQuery(Query);
            return View("SearchOnScholar", articles);
        }
    }
}
