using System.Collections.Generic;
using System.Linq;
using BibliographicSystem.Controllers;
using BibliographicSystem.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BibliographicSystemTests.Controllers
{
    [TestClass]
    public class ScholarControllerTests
    {
        /// <summary>
        /// Test for empty query
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest()
        {
            var controller = new ScholarController();
            var view = controller.SearchOnScholarResult();
            var list = (List<ScholarArticle>) view.Model;
            Assert.AreEqual(0, list.Count);            
        }

        /// <summary>
        /// Usual search test
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest2()
        {
            var controller = new ScholarController();
            var view = controller.SearchOnScholarResult("journal", 22);
            var list = (List<ScholarArticle>)view.Model;
            Assert.AreEqual(22, list.Count);
            var info= list.Select(article => article.Description).ToList();
            var count = info.Select(inf => inf.ToLower()).Count(lowerHead => lowerHead.Contains("journal"));
            Assert.IsTrue(count > 10);
        }

        /// <summary>
        /// Test for bug fixed
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest3()
        {
            var controller = new ScholarController();
            var view = controller.SearchOnScholarResult("kill", 3);
            var list = (List<ScholarArticle>)view.Model;
            Assert.AreEqual(3, list.Count);
            var info = list.Select(article => article.Description).ToList();
            var count = info.Select(inf => inf.ToLower()).Count(lowerHead => lowerHead.Contains("kill"));
            Assert.IsTrue(count > 0);
        }

        /// <summary>
        /// Test for russian letters
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest4()
        {
            var controller = new ScholarController();
            var view = controller.SearchOnScholarResult("задача");
            var list = (List<ScholarArticle>)view.Model;
            Assert.AreEqual(10, list.Count);
            var heads = list.Select(article => article.Title).ToList();
            var count = heads.Select(head => head.ToLower()).Count(lowerHead => lowerHead.Contains("задача"));
            Assert.IsTrue(count > 0);
        }

        /// <summary>
        /// Test for query with spaces
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest5()
        {
            var controller = new ScholarController();
            var view = controller.SearchOnScholarResult("теорема Коши");
            var list = (List<ScholarArticle>)view.Model;
            Assert.AreEqual(10, list.Count);
            var heads = list.Select(article => article.Title).ToList();
            var count = heads.Select(head => head.ToLower()).Count(lowerHead => lowerHead.Contains("теорема коши"));
            Assert.IsTrue(count > 0);
        }

        /// <summary>
        /// Test for query with spaces
        /// </summary>
        [TestMethod]
        public void SearchOnScholarResultTest6()
        {
            var controller = new ScholarController();
            var view = controller.SearchOnScholarResult("Перельман гипотеза Пуанкаре", 14);
            var list = (List<ScholarArticle>)view.Model;
            Assert.AreEqual(14, list.Count);
            var info = list.Select(article => article.Title).ToList();
            var count = info.Select(inf => inf.ToLower()).Count(lowerHead => lowerHead.Contains("перельман") || lowerHead.Contains("гипотеза") || lowerHead.Contains("пуанкаре"));
            Assert.IsTrue(count > 2);
        }
    }
}