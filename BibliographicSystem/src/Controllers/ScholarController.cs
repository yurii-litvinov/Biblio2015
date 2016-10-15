using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using BibliographicSystem.Models;

namespace BibliographicSystem.Controllers
{
    public class ScholarController : Controller
    {
        private readonly AppContext db = new AppContext();

        //
        // GET: /Scholar/

        public ActionResult SearchOnScholar()
        {
            var lists = new ListsOfStuff {ScholarArt = new List<ScholarArticle>()};
            return View("SearchOnScholar", lists);
        }

        [HttpPost]
        public ActionResult SearchOnScholar(string query)
        {
            var parsing = new ParseMethod.ParseMethod();
            var lists = new ListsOfStuff {ScholarArt = parsing.GetScholarArticlesByQuery(query)};
            return View("SearchOnScholar", lists);
        }

        [HttpPost]
        public ActionResult AddArticle(string title, string info, string reference, string username)
        {
            var parser = new ParseMethod.ParseMethod();
            var authors = parser.GetAuthors(info);
            var year = parser.GetYear(info);
            var journal = parser.GetJournal(info);
            var publisher = parser.GetPublisher(info);

            var art = new Article
            {
                title = title,
                author = authors,
                year = year,
                journal = journal,
                publisher = publisher,
                Type = "Article",
                UserName = username
            };
            db.Articles.Add(art);
            db.SaveChanges();

            return Redirect("/Home/Finish");
        }

        [HttpPost]
        public ActionResult DownloadBibTeX(string title, string info, string reference)
        {
            var parser = new ParseMethod.ParseMethod();
            var path = @"c:\bibFiles";
            var bibDescr = parser.FormBibTeX(info, title, reference);
            var name = parser.FormBibTeXName(info, title);
            Directory.CreateDirectory(path);
            path += "\\" + name + ".bib";
            if (!System.IO.File.Exists(path))
            {
                using (var sw = System.IO.File.CreateText(path))
                    sw.WriteLine(bibDescr);
            }
            return Redirect("/Home/Finish");
        }
    }
}
