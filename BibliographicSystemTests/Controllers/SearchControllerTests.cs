using System.Collections.Generic;
using System.Linq;
using BibliographicSystem.Controllers;
using BibliographicSystem.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BibliographicSystemTests.Controllers
{
    [TestClass]
    public class SearchControllerTests
    {
        /// <summary>
        /// Test for empty query
        /// </summary>
        [TestMethod]
        public void SearchResultTest()
        {
            var controller = new SearchController();
            var view = controller.SearchResult();
            var list = (List<OutsideArticle>)view.Model;
            Assert.AreEqual(0, list.Count);
        }

        /// <summary>
        /// Test for bug fixed(Case when a user profile appears)
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest3()
        {
            var controller = new SearchController();
            var view = controller.SearchResult("kill");
            var list = (List<OutsideArticle>)view.Model;
            var gsArticles = list.Where(article => article.From == "GS");
            var info = gsArticles.Select(article => article.Info).ToList();
            var count = info.Select(inf => inf.ToLower()).Count(lowerHead => lowerHead.Contains("kill"));
            Assert.IsTrue(count > 0);
        }

        /// <summary>
        /// Test for russian letters
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest4()
        {
            var controller = new SearchController();
            var view = controller.SearchResult("задача");
            var list = (List<OutsideArticle>)view.Model;
            var gsArticles = list.Where(article => article.From == "GS");
            var heads = gsArticles.Select(article => article.Title).ToList();
            var count = heads.Select(head => head.ToLower()).Count(lowerHead => lowerHead.Contains("задача"));
            Assert.IsTrue(count > 0);
        }

        /// <summary>
        /// Test for query with spaces
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest5()
        {
            var controller = new SearchController();
            var view = controller.SearchResult("теорема Коши");
            var list = (List<OutsideArticle>)view.Model;
            var gsArticles = list.Where(article => article.From == "GS");
            var heads = gsArticles.Select(article => article.Title).ToList();
            var count = heads.Select(head => head.ToLower()).Count(lowerHead => lowerHead.Contains("теорема коши"));
            Assert.IsTrue(count > 0);
        }

        /// <summary>
        /// Test for query with spaces
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest6()
        {
            var controller = new SearchController();
            var view = controller.SearchResult("Перельман гипотеза Пуанкаре");
            var list = (List<OutsideArticle>)view.Model;
            var gsArticles = list.Where(article => article.From == "GS");
            var info = gsArticles.Select(article => article.Title).ToList();
            var count = info.Select(inf => inf.ToLower()).Count(lowerHead => lowerHead.Contains("перельман") || lowerHead.Contains("гипотеза") || lowerHead.Contains("пуанкаре"));
            Assert.IsTrue(count > 2);
        }
    }
}