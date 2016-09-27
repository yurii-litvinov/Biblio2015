using System.Collections.Generic;
using System.Web.Mvc;
using BootApp.Models;
using BootApp.Parsing;

namespace BootApp.Controllers
{
    public class ScholarController : Controller
    {
        AppContext db = new AppContext();

        //
        // GET: /Scholar/

       

        public ActionResult SearchOnScholar()
        {
            ListsOfStuff lists = new ListsOfStuff();
            lists.ScholarArt = new List<ScholarArticle>();
            return View("SearchOnScholar", lists);
        }

        [HttpPost]
        public ActionResult SearchOnScholar(string Query)
        {
            ParseMethod parsing = new ParseMethod();
            ListsOfStuff lists = new ListsOfStuff();
            lists.ScholarArt = parsing.GetScholarArticlesByQuery(Query);
            return View("SearchOnScholar", lists);
        }

        [HttpPost]
        //public ActionResult AddArticle(string title, string info, string reference)
        public ActionResult AddArticle(string title, string info, string reference)
        {
            //ViewBag.Text = "Success";
            ParseMethod parser = new ParseMethod();
            string authors = parser.GetAuthors(info);
            string year = parser.GetYear(info);
            string journal = parser.GetJournal(info);
            string publisher = parser.GetPublisher(info);

            Article art = new Article { title = title, author = authors, year = year, journal = journal, publisher = publisher };
            //db.Articles.Add(art);
            //db.SaveChanges();

            return RedirectToAction("Home/Finish");
        }
    }
}
