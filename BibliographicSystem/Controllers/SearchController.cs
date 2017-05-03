using System.IO;
using System.Collections.Generic;
using System.Web.Mvc;
using BibliographicSystem.Models;
using BibliographicSystem.SearchingMethods;

namespace BibliographicSystem.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search

        /// <summary>
        /// Initial page method
        /// </summary>
        /// <param name="query"> Always == null </param>
        /// <returns> Page for searching </returns>
        public ActionResult Search(string query) => View("Search", "");

        /// <summary>
        /// shows the search result
        /// </summary>
        /// <param name="query">reference words</param>
        /// <param name="count">number of articles</param>
        /// <param name="authors">authors of article</param>
        /// <param name="year">the year of publication of article</param>
        /// <returns></returns>
        public PartialViewResult SearchResult(string query = "", string count = "10", string authors = "", string year = "")
        {
            if (query.Length + authors.Length + year.Length == 0)
            {
                return PartialView("SearchResult", new List<OutsideArticle>());
            }

            var userQuery = new UserQuery { MainInput = query.ToLower(), Authors = authors.ToLower(), Year = year, Count = count };
            var processor = new QueryProcessor(userQuery);
            var articles = processor.GetSearchResult();
            return PartialView("SearchResult", articles);

            //if (query.Length == 0)
            //    return PartialView("SearchOnScholarResult", new List<ScholarArticle>());
            //var parsing = new GoogleScholarParser();
            //try
            //{
            //    var resultList = new List<ScholarArticle>();
            //    for (var i = 0; i < number; i += 10)
            //    {
            //        var parsedArticles = resultList.Count;
            //        resultList.AddRange(parsing.GetScholarArticlesByQuery(query, i, exactPhrase, without, head, published, author, dateStart, dateEnd));
            //        if (resultList.Count == parsedArticles)
            //            break;
            //    }
            //    if (resultList.Count > number)
            //        resultList.RemoveRange(number, resultList.Count - number);
            //    if (resultList.Count != 0)
            //        return PartialView("SearchOnScholarResult", resultList);

            //    ModelState.AddModelError("Empty list", "Ничего не найдено");
            //    return PartialView("SearchOnScholarResult", new List<ScholarArticle>());
            //}
            //catch (NullReferenceException)
            //{
            //    ModelState.AddModelError("Empty list", "Ничего не найдено");
            //    return PartialView("SearchOnScholarResult", new List<ScholarArticle>());
            //}
        }

        public ViewResult ShowCaptcha()
        {
            return View("Captcha");
        }

        //public ActionResult Index()
        //{
        //    return View();
        //}


        [HttpPost]
        public ActionResult AddArticle(string title, string info, string reference, string username)
        {
            var parser = new GoogleScholarParser();
            var authors = parser.GetAuthors(info);
            var year = parser.GetYear(info);
            var journal = parser.GetJournal(info);
            var publisher = parser.GetPublisher(info);

            var art = new Article
            {
                Title = title,
                Author = authors,
                Year = year,
                Journal = journal,
                Publisher = publisher,
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
            var parser = new GoogleScholarParser();
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

        private readonly AppContext db = new AppContext();
    }
}