using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using BibliographicSystem.Models;
using BibliographicSystem.SearchingMethods;

namespace BibliographicSystem.Controllers
{
    /// <summary>
    /// Controller for searching on Google.Scholar
    /// </summary>
    public class ScholarController : Controller
    {
        private readonly AppContext db = new AppContext();
        //
        // GET: /Scholar/

        /// <summary>
        /// Initial page method
        /// </summary>
        /// <param name="query"> Always == null </param>
        /// <returns> Page for Scholar searching </returns>
        public ActionResult SearchOnScholar(string query)
            => View("SearchOnScholar", "");

        /// <summary>
        /// Method for handling Ajax query to search on Scholar
        /// </summary>
        /// <param name="query"> String to find </param>
        /// <param name="number"> Number of articles to find</param>
        /// <param name="exactPhrase"> Article should contains this phrase </param>
        /// <param name="without"> Articles should not contains this words </param>
        /// <param name="head"> Is searching only in article head </param>
        /// <param name="published"> Journal, where the article was published </param>
        /// <param name="author"> Author of article </param>
        /// <param name="dateStart"> Since date </param>
        /// <param name="dateEnd"> Till date </param>
        /// <returns> Block of Scholar articles </returns>
        public PartialViewResult SearchOnScholarResult(string query = "",
            int number = 10,
            string exactPhrase = null,
            string without = null,
            bool head = false,
            string published = null,
            string author = null,
            int dateStart = int.MinValue,
            int dateEnd = int.MinValue)
        {
            if (query.Length == 0)
                return PartialView("SearchOnScholarResult", new List<ScholarArticle>());
            var parsing = new GoogleScholarParser();
            try
            {
                var resultList = new List<ScholarArticle>();
                for (var i = 0; i < number; i += 10)
                {
                    var parsedArticles = resultList.Count;
                    resultList.AddRange(parsing.GetScholarArticlesByQuery(query, i, exactPhrase, without, head, published, author, dateStart, dateEnd));
                    if (resultList.Count == parsedArticles)
                        break;
                }
                if (resultList.Count > number)
                    resultList.RemoveRange(number, resultList.Count - number);
                if (resultList.Count != 0)
                    return PartialView("SearchOnScholarResult", resultList);
                
                return PartialView("SearchOnScholarResult", new List<ScholarArticle>());
            }
            catch (NullReferenceException)
            {
                return PartialView("SearchOnScholarResult", new List<ScholarArticle>());
            }
        }

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
    }
}
