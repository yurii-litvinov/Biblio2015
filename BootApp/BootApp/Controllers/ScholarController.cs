using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Text;
using HtmlAgilityPack;
using BootApp.Models;

namespace BootApp.Controllers
{
    public class ScholarController : Controller
    {
        //List<ScholarArticle> articles = new List<ScholarArticle>();

        //
        // GET: /Scholar/

        public string GetQueryUrl(string query)
        {
            string url = "http://scholar.google.ru/scholar?q=";
            query = query.Replace(' ', '+');
            query = string.Concat(url, query);
            return query;
        }

        public List<ScholarArticle> GetScholarArticlesByQuery(string query)
        {
            // Getting page content from scholar page (with given query)
            query = GetQueryUrl(query);
            string pageContent;
            var request = (HttpWebRequest)WebRequest.Create(query);
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1251"));
            pageContent = streamReader.ReadToEnd();
            streamReader.Close();

            // creating list of articles 
            List<ScholarArticle> articles = new List<ScholarArticle>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            for (int i = 1; i <= 10; i++)
            {
                ScholarArticle article = new ScholarArticle();
                string xPathBiblioCheck = string.Format("//*[@id='gs_ccl']/div[{0}]/div[2]", i);
                HtmlNode biblioCheck = doc.DocumentNode.SelectSingleNode(xPathBiblioCheck);
                if (biblioCheck != null)
                {

                    string xPathRefCheck = string.Format("//*[@id='gs_ccl']/div[{0}]/div[2]/h3/a", i);
                    HtmlNode refCheck = doc.DocumentNode.SelectSingleNode(xPathRefCheck);

                    // adding title of article 
                    string xPathTitle = string.Format("//*[@id='gs_ccl']/div[{0}]/div[2]/h3/a", i);
                    article.Title = doc.DocumentNode.SelectSingleNode(xPathTitle).InnerText;

                    // adding info 
                    string xPathInfo = string.Format("//*[@id='gs_ccl']/div[{0}]/div[2]/div[1]", i);
                    article.Info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;

                    // adding reference
                    article.Reference = refCheck.GetAttributeValue("href", null);

                    // adding citiations amount
                    string xPathCitiations = string.Format("//*[@id='gs_ccl']/div[{0}]/div[2]/div[3]/a[1]", i);
                    string citiationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                    if (citiationsCheck.StartsWith("Цитируется"))
                        article.Citiations = citiationsCheck;
                    else
                        article.Citiations = "Эта статья нигде не цитируется. ";

                    articles.Insert(i - 1, article);
                }
                else
                {
                    string xPathRefCheck = string.Format("//*[@id='gs_ccl']/div[{0}]/div/h3/a", i);
                    HtmlNode refCheck = doc.DocumentNode.SelectSingleNode(xPathRefCheck);
                    if (refCheck != null)
                    {
                        string xPathTitle = string.Format("//*[@id='gs_ccl']/div[{0}]/div/h3/a", i);
                        article.Title = doc.DocumentNode.SelectSingleNode(xPathTitle).InnerText;

                        string xPathInfo = string.Format("//*[@id='gs_ccl']/div[{0}]/div/div[1]", i);
                        article.Info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;

                        article.Reference = refCheck.GetAttributeValue("href", null);

                        string xPathCitiations = string.Format("//*[@id='gs_ccl']/div[{0}]/div/div[3]/a[1]", i);
                        string citiationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                        if (citiationsCheck.StartsWith("Цитируется"))
                            article.Citiations = citiationsCheck;
                        else
                            article.Citiations = "Эта статья нигде не цитируется. ";

                        articles.Insert(i - 1, article);
                    }

                    else
                    {
                        // case, when article do not has reference, but has a tag [citiation]
                        string xPathTitleCheck = string.Format("//*[@id='gs_ccl']/div[{0}]/div/h3", i);
                        string xPathSpanNode = string.Format("//*[@id='gs_ccl']/div[{0}]/div/h3/span", i);
                        HtmlNode titleMatchNode = doc.DocumentNode.SelectSingleNode(xPathTitleCheck);
                        HtmlNode spanNode = doc.DocumentNode.SelectSingleNode(xPathSpanNode);
                        titleMatchNode.RemoveChild(spanNode);
                        article.Title = titleMatchNode.InnerText;

                        string xPathInfo = string.Format("//*[@id='gs_ccl']/div[{0}]/div/div[1]", i);
                        article.Info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;

                        article.Reference = "This article does not have a reference";

                        string xPathCitiations = string.Format("//*[@id='gs_ccl']/div[{0}]/div/div[2]/a[1]", i);
                        string citiationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                        if (citiationsCheck.StartsWith("Цитируется"))
                            article.Citiations = citiationsCheck;
                        else
                            article.Citiations = "Эта статья нигде не цитируется. ";

                        articles.Insert(i - 1, article);
                    }
                }
            }

            return articles;
        }

        public ActionResult SearchOnScholar()
        {
            List<ScholarArticle> articles = new List<ScholarArticle>();
            return View("SearchOnScholar", articles);
        }

        [HttpPost]
        public ActionResult SearchOnScholar(string Query)
        {
            List<ScholarArticle> articles = GetScholarArticlesByQuery(Query);
            return View("SearchOnScholar", articles);
        }
    }
}
