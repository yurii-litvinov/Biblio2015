using BibliographicSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BibliographicSystem.SearchingMethods;

namespace BibliographicSystem.Controllers
{
    public class MicrosoftAcademicController : Controller
    {
        // GET: MicrosoftAcademic
        public ActionResult Index() => View();

        private readonly AppContext db = new AppContext();
        //
        // GET: /Scholar/

        /// <summary>
        /// Initial page method
        /// </summary>
        /// <param name="query"> Always == null </param>
        /// <returns> Page for Microsoft Academic searching </returns>
        public ActionResult SearchOnMicrosoftAcademic(string query) => View("SearchOnMicrosoftAcademic", "");

        public PartialViewResult SearchResult(string query, int count, string author, int year)
        {
            query += "";
            if (query.Length == 0)
            {
                return PartialView("SearchResult", new List<MicrosoftAcademicArticle>());
            }

            var parser = new MicrosoftAcademicParser(query);
            var listOfArticles = parser.GetSearchResult();
            return PartialView("SearchResult", listOfArticles);
        }


    }
}