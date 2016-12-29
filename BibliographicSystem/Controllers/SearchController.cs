using BibliographicSystem.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using BibliographicSystem.SearchingMethods;
using static BibliographicSystem.SearchingMethods.MicrosoftAcademicParser;
using System;

namespace BibliographicSystem.Controllers
{
    public class SearchController : Controller
    {
        public ActionResult Search(string query) => View("Search", "");

        public PartialViewResult SearchResult(string query = "",
            int number = 10,
            string authors = null,
            int dateStart = int.MinValue,
            int dateEnd = int.MaxValue,
            string exactPhrase = null,
            string without = null,
            bool head = false,
            string published = null)
        {
            var googleParser = new GoogleScholarParser();
            try
            {
                var listOfGoogleScholarArticles = new List<CommonArticle>();
                for (var i = 0; i < number; i += 10)
                {
                    var parsedArticles = listOfGoogleScholarArticles.Count;
                    listOfGoogleScholarArticles.AddRange(googleParser.GetScholarArticlesByQuery(query, i, exactPhrase, without, head, published, authors, dateStart, dateEnd));
                    if (listOfGoogleScholarArticles.Count == parsedArticles)
                        break;
                }

                string year = dateStart.ToString();
                var userQuery = new UserQuery { MainInput = query.ToLower(), Authors = authors.ToLower(), Year = year, Count = number.ToString() };
                var microsoftAcademicParser = new MicrosoftAcademicParser(userQuery);
                var listOfMicrosoftArticles = microsoftAcademicParser.GetSearchResult();

                if (listOfGoogleScholarArticles.Count == 0 && listOfMicrosoftArticles.Count == 0)
                {
                    ModelState.AddModelError("Empty list", "Ничего не найдено");
                    return PartialView("SearchResult", new List<CommonArticle>());
                }

                /*if (listOfGoogleScholarArticles.Count > number)
                    listOfGoogleScholarArticles.RemoveRange(number, listOfGoogleScholarArticles.Count - number);
                if (listOfMicrosoftArticles.Count > number)
                    listOfMicrosoftArticles.RemoveRange(number, listOfMicrosoftArticles.Count - number);*/

                listOfGoogleScholarArticles.AddRange(listOfMicrosoftArticles);

                return PartialView("SearchResult", listOfGoogleScholarArticles);
            }
            catch (NullReferenceException)
            {
                ModelState.AddModelError("Empty list", "Ничего не найдено");
                return PartialView("SearchResult", new List<CommonArticle>());
            }
        }
    }
}
