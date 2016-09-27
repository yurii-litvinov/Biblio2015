using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using BootApp.Models;

namespace BootApp.Parsing
{
    public class ParseMethod
    {
        public string GetQueryUrl(string query)
        {
            string url = "http://scholar.google.com/scholar?hl=en&q=";
            query = query.Replace(' ', '+');
            query = string.Concat(url, query);
            return query;
        }

        private string GetPageContent(string url)
        {
            string pageContent;
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1251"));
            pageContent = streamReader.ReadToEnd();
            streamReader.Close();
            return pageContent;
        }

        public string GetAuthors(string articleInfo)
        {
            List<string> authors = new List<string>();
            string authorsPattern = @"[A-Z]{1,}\s{1}[A-Za-z]{1,}|[А-Я]{1,}\s{1}[А-Яа-я]{1,}";
            Regex author = new Regex(authorsPattern);
            MatchCollection matches = author.Matches(articleInfo);
            foreach (Match match in matches)
            {
                authors.Add(match.Value);
            }

            string authorsString = "";
            for (int i = 0; i < authors.Count - 1; i++)
                authorsString += authors.ElementAt(i) + ",";
            authorsString += authors.Last();

            return authorsString;
        }

        public string GetYear(string articleInfo)
        {
            string yearPattern = @"\d{4}";
            Regex year = new Regex(yearPattern);
            Match yearMatch = year.Match(articleInfo);
            if (yearMatch.Success)
                return yearMatch.Value; 
            else
                return "no info";
        }

        public string GetJournal(string articleInfo)
        {
            List<int> dashRange = GetDashRange(articleInfo);
            if (dashRange.Last() - (dashRange.First() + 3) == 4)
                return "no info";
            else if (articleInfo.IndexOf("…", dashRange.First()) != -1)
                return "no info";
            else
            {
                string journalPattern = @"[^,]{1,}";
                Regex journal = new Regex(journalPattern);
                Match journalMatch = journal.Match(articleInfo, (dashRange.First() + 3));
                return journalMatch.Value;
            }
        }

        // magical "+3" here for skipping 3 positions to pass the " - " symbol sequence
        public string GetPublisher(string articleInfo)
        {
            List<int> dashRange = GetDashRange(articleInfo);
            if (dashRange.Contains(-1))
                return "no info";
            else
            {
                string publisherPattern = @".{1,}";
                Regex publisher = new Regex(publisherPattern);
                Match publisherMatch = publisher.Match(articleInfo, (dashRange.Last() + 3));

                return publisherMatch.Value;
            }
        }

        /// <summary>
        /// Method for getting dash range, which includes info about year and journal
        /// </summary>
        /// <param name="str"></param>
        private List<int> GetDashRange(string str)
        {
            List<int> range = new List<int>();
            string dashToFind = " - ";
            int firstEntry = str.IndexOf(dashToFind);
            range.Add(firstEntry);
            range.Add(str.IndexOf(dashToFind, (firstEntry + 3)));

            return range;
        }

        /// <summary>
        /// Forms a name for article in BibTeX 
        /// Contains first author, year and first word of the title
        /// </summary>
        /// <param name="articleInfo"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private string FormBibTeXName(string articleInfo, string title)
        {
            string authors = GetAuthors(articleInfo);
            string year = GetYear(articleInfo);

            string firstWordPattern = @"[A-Z]{1}[a-z]{1,}";
            Regex author = new Regex(firstWordPattern);
            Match authorMatch = author.Match(authors);
            Regex firstWordTitle = new Regex(firstWordPattern);
            Match titleMatch = firstWordTitle.Match(title);
            string name = authorMatch.Value + year + titleMatch.Value;

            return name.ToLower(); ;
        }

        public string FormBibTeX(string articleInfo, string title, string reference)
        {
            // getting needed info
            string authors = GetAuthors(articleInfo);
            string journal = GetJournal(articleInfo);
            string year = GetYear(articleInfo);
            string publisher = GetPublisher(articleInfo);
            string name = FormBibTeXName(articleInfo, title);

            // forming string file, to convert it into bibtex further
            string bibtex = "@article{" + name + ",\n" + "  title={" + title + "},\n" + "  author={";
            bibtex += authors + "},\n";
            if (journal != "no info")
                bibtex += "  journal={" + journal + "},\n";

            if (publisher != "no info" && !publisher.Contains("."))
            {
                bibtex += "  year={" + year + "},\n";
                bibtex += "  publisher={" + publisher + "}\n}";
            }
            else if (publisher != "no info" && publisher.Contains("."))
            {
                bibtex += "  year={" + year + "},\n";
                bibtex += "  url={" + reference + "},\n";
                bibtex += "  medium={electronic resource}\n}";
            }
            else if (publisher == "no info")
                bibtex += "  year={" + year + "}\n}";

            return bibtex;
        }

        public List<ScholarArticle> GetScholarArticlesByQuery(string query)
        {
            // getting page content from scholar page (with given query)
            query = GetQueryUrl(query);
            string pageContent = GetPageContent(query);

            // creating list of articles for "searhc on scholar" view
            List<ScholarArticle> scholarArticles = new List<ScholarArticle>();
            // creating list of articles to operate 
            List<Article> articles = new List<Article>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            for (int i = 1; i <= 10; i++)
            {
                ScholarArticle article = new ScholarArticle();
                string xPathBiblioCheck = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div[2]", i);
                //*[@id="gs_ccl_results_results"]/div[1]
                HtmlNode biblioCheck = doc.DocumentNode.SelectSingleNode(xPathBiblioCheck);
                if (biblioCheck != null)
                {

                    string xPathRefCheck = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div[2]/h3/a", i);
                    HtmlNode refCheck = doc.DocumentNode.SelectSingleNode(xPathRefCheck);

                    // adding title of article 
                    string xPathTitle = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div[2]/h3/a", i);
                    article.Title = doc.DocumentNode.SelectSingleNode(xPathTitle).InnerText;

                    // adding info 
                    string xPathInfo = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div[2]/div[1]", i);
                    article.Info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;

                    // adding reference
                    article.Reference = refCheck.GetAttributeValue("href", null);

                    // adding citiations amount
                    string xPathCitiations = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div[2]/div[3]/a[1]", i);
                    string citiationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                    if (citiationsCheck.StartsWith("Cited by"))
                        article.Citiations = citiationsCheck;
                    else
                        article.Citiations = "No citiations for this article. ";
                    
                    scholarArticles.Insert(i - 1, article);
                }
                else
                {
                    string xPathRefCheck = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div/h3/a", i);
                    HtmlNode refCheck = doc.DocumentNode.SelectSingleNode(xPathRefCheck);
                    if (refCheck != null)
                    {
                        string xPathTitle = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div/h3/a", i);
                        article.Title = doc.DocumentNode.SelectSingleNode(xPathTitle).InnerText;

                        string xPathInfo = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div/div[1]", i);
                        article.Info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;

                        article.Reference = refCheck.GetAttributeValue("href", null);

                        string xPathCitiations = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div/div[3]/a[1]", i);
                        string citiationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                        if (citiationsCheck.StartsWith("Cited by"))
                            article.Citiations = citiationsCheck;
                        else
                            article.Citiations = "No citiations for this article. ";

                        scholarArticles.Insert(i - 1, article);
                    }

                    else
                    {
                        // case, when article do not has reference, but has a tag [citiation]/[book]
                        string xPathTitleCheck = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div/h3", i);
                        string xPathSpanNode = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div/h3/span", i);
                        HtmlNode titleMatchNode = doc.DocumentNode.SelectSingleNode(xPathTitleCheck);
                        HtmlNode spanNode = doc.DocumentNode.SelectSingleNode(xPathSpanNode);
                        titleMatchNode.RemoveChild(spanNode);
                        article.Title = titleMatchNode.InnerText;

                        string xPathInfo = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div/div[1]", i);
                        article.Info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;

                        article.Reference = "This article does not have a reference";

                        string xPathCitiations = string.Format("//*[@id='gs_ccl_results']/div[{0}]/div/div[2]/a[1]", i);
                        string citiationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                        if (citiationsCheck.StartsWith("Cited by"))
                            article.Citiations = citiationsCheck;
                        else
                            article.Citiations = "No citiations for this article. ";

                        scholarArticles.Insert(i - 1, article);
                    }
                }
            }

            return scholarArticles;
        }
    }
}