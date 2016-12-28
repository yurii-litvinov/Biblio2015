using BibliographicSystem.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using BibliographicSystem.SearchingMethods;
using static BibliographicSystem.SearchingMethods.MicrosoftAcademicParser;

namespace BibliographicSystem.Controllers
{
    public class MicrosoftAcademicController : Controller
    {
        // GET: MicrosoftAcademic

        /// <summary>
        /// Initial page method
        /// </summary>
        /// <param name="query"> Always == null </param>
        /// <returns> Page for Microsoft Academic searching </returns>
        public ActionResult SearchOnMicrosoftAcademic(string query) => View("SearchOnMicrosoftAcademic", "");

        /// <summary>
        /// shows the search result
        /// </summary>
        /// <param name="query">reference words</param>
        /// <param name="count">number of articles</param>
        /// <param name="authors">authors of article</param>
        /// <param name="year">the year of publication of article</param>
        /// <returns></returns>
        public PartialViewResult SearchResult(string query = "", string count = "", string authors = "", string year = "")
        {
            if (query.Length + authors.Length + year.Length == 0)
            {
                return PartialView("SearchResult", new List<CommonArticle>());
            }

            var userQuery = new UserQuery { MainInput = query.ToLower(), Authors = authors.ToLower(), Year = year, Count = count };
            var parser = new MicrosoftAcademicParser(userQuery);
            var listOfArticles = parser.GetSearchResult();
            return PartialView("SearchResult", listOfArticles);
        }
    }
}