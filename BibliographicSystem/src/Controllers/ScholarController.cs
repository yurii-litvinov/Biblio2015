using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using BootApp.Models;
using BootApp.Parsing;

namespace BibliographicSystem.Controllers
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
        public ActionResult AddArticle(string title, string info, string reference, string username)
        {
            ParseMethod parser = new ParseMethod();
            string authors = parser.GetAuthors(info);
            string year = parser.GetYear(info);
            string journal = parser.GetJournal(info);
            string publisher = parser.GetPublisher(info);

            Article art = new Article { title = title, author = authors, year = year, journal = journal, publisher = publisher };
            art.Type = "Article";
            art.UserName = username;
            db.Articles.Add(art);
            db.SaveChanges();

            return Redirect("/Home/Finish");
        }

        [HttpPost]
        public ActionResult DownloadBibTeX(string title, string info, string reference)
        {
            ParseMethod parser = new ParseMethod();
            string path = @"c:\bibFiles";
            //string path = directory + "\\BibFiles";
            string bibDescr = parser.FormBibTeX(info, title, reference);
            string name = parser.FormBibTeXName(info, title);
            Directory.CreateDirectory(path);
            path += "\\" + name + ".bib";
            if (!System.IO.File.Exists(path))
            {
                using (StreamWriter sw = System.IO.File.CreateText(path))
                    sw.WriteLine(bibDescr);
            }

            return Redirect("/Home/Finish");
        }
    }
}
