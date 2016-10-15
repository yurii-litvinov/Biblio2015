using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using BibliographicSystem.Models;
using HtmlAgilityPack;

namespace BibliographicSystem.ParseMethod
{
    public class ParseMethod
    {
        public string GetQueryUrl(string query)
        {
            const string url = "http://scholar.google.com/scholar?hl=en&q=";
            query = query.Replace(' ', '+');
            query = string.Concat(url, query);
            return query;
        }

        private string GetPageContent(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1251"));
            var pageContent = streamReader.ReadToEnd();
            streamReader.Close();
            return pageContent;
        }

        public string GetAuthors(string articleInfo)
        {
            var author = new Regex(@"[A-Z]{1,}\s{1}[A-Za-z]{1,}|[А-Я]{1,}\s{1}[А-Яа-я]{1,}");
            var matches = author.Matches(articleInfo);
            var authors = (from Match match in matches select match.Value).ToList();

            var authorsString = "";
            for (var i = 0; i < authors.Count - 1; i++)
                authorsString += authors.ElementAt(i) + ",";
            authorsString += authors.Last();

            return authorsString;
        }

        public string GetYear(string articleInfo)
        {
            const string yearPattern = @"\d{4}";
            var year = new Regex(yearPattern);
            var yearMatch = year.Match(articleInfo);
            return yearMatch.Success ? yearMatch.Value : "no info";
        }

        public string GetJournal(string articleInfo)
        {
            var dashRange = GetDashRange(articleInfo);
            if (dashRange.Last() - (dashRange.First() + 3) == 4)
                return "no info";
            if (articleInfo.IndexOf("…", dashRange.First(), StringComparison.Ordinal) != -1)
                return "no info";
            const string journalPattern = @"[^,]{1,}";
            var journal = new Regex(journalPattern);
            var journalMatch = journal.Match(articleInfo, (dashRange.First() + 3));
            return journalMatch.Value;
        }

        // magical "+3" here for skipping 3 positions to pass the " - " symbol sequence
        public string GetPublisher(string articleInfo)
        {
            var dashRange = GetDashRange(articleInfo);
            if (dashRange.Contains(-1))
                return "no info";
            const string publisherPattern = @".{1,}";
            var publisher = new Regex(publisherPattern);
            var publisherMatch = publisher.Match(articleInfo, (dashRange.Last() + 3));

            return publisherMatch.Value;
        }

        /// <summary>
        /// Method for getting dash range, which includes info about year and journal
        /// </summary>
        /// <param name="str"></param>
        private List<int> GetDashRange(string str)
        {
            var range = new List<int>();
            const string dashToFind = " - ";
            var firstEntry = str.IndexOf(dashToFind, StringComparison.Ordinal);
            range.Add(firstEntry);
            range.Add(str.IndexOf(dashToFind, firstEntry + 3, StringComparison.Ordinal));

            return range;
        }

        /// <summary>
        /// Forms a name for article in BibTeX 
        /// Contains first author, year and first word of the title
        /// </summary>
        /// <param name="articleInfo"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public string FormBibTeXName(string articleInfo, string title)
        {
            var authors = GetAuthors(articleInfo);
            var year = GetYear(articleInfo);

            const string firstWordPattern = @"[A-Z]{1}[a-z]{1,}";
            var author = new Regex(firstWordPattern);
            var authorMatch = author.Match(authors);
            var firstWordTitle = new Regex(firstWordPattern);
            var titleMatch = firstWordTitle.Match(title);
            var name = authorMatch.Value + year + titleMatch.Value;

            return name.ToLower();
        }

        public string FormBibTeX(string articleInfo, string title, string reference)
        {
            // getting needed info
            var authors = GetAuthors(articleInfo);
            var journal = GetJournal(articleInfo);
            var year = GetYear(articleInfo);
            var publisher = GetPublisher(articleInfo);
            var name = FormBibTeXName(articleInfo, title);

            // forming string file, to convert it into bibtex further
            var bibtex = "@article{" + name + ",\n" + "  title={" + title + "},\n" + "  author={";
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
            var pageContent = GetPageContent(query);

            // creating list of articles for "searhc on scholar" view
            var scholarArticles = new List<ScholarArticle>();

            var doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            for (var i = 1; i <= 10; i++)
            {
                var article = new ScholarArticle();
                var xPathBiblioCheck = $"//*[@id='gs_ccl_results']/div[{i}]/div[2]";
                //*[@id="gs_ccl_results_results"]/div[1]
                var biblioCheck = doc.DocumentNode.SelectSingleNode(xPathBiblioCheck);
                if (biblioCheck != null)
                {

                    var xPathRefCheck = $"//*[@id='gs_ccl_results']/div[{i}]/div[2]/h3/a";
                    var refCheck = doc.DocumentNode.SelectSingleNode(xPathRefCheck);

                    // adding title of article 
                    var xPathTitle = $"//*[@id='gs_ccl_results']/div[{i}]/div[2]/h3/a";
                    article.Title = doc.DocumentNode.SelectSingleNode(xPathTitle).InnerText;

                    // adding info 
                    var xPathInfo = $"//*[@id='gs_ccl_results']/div[{i}]/div[2]/div[1]";
                    article.Info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;

                    // adding reference
                    article.Reference = refCheck.GetAttributeValue("href", null);

                    // adding citiations amount
                    var xPathCitiations = $"//*[@id='gs_ccl_results']/div[{i}]/div[2]/div[3]/a[1]";
                    var citiationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                    article.Citiations = citiationsCheck.StartsWith("Cited by") ? citiationsCheck : "No citiations for this article. ";

                    scholarArticles.Insert(i - 1, article);
                }
                else
                {
                    var xPathRefCheck = $"//*[@id='gs_ccl_results']/div[{i}]/div/h3/a";
                    var refCheck = doc.DocumentNode.SelectSingleNode(xPathRefCheck);
                    if (refCheck != null)
                    {
                        var xPathTitle = $"//*[@id='gs_ccl_results']/div[{i}]/div/h3/a";
                        article.Title = doc.DocumentNode.SelectSingleNode(xPathTitle).InnerText;

                        var xPathInfo = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[1]";
                        article.Info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;

                        article.Reference = refCheck.GetAttributeValue("href", null);

                        var xPathCitiations = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[3]/a[1]";
                        var citiationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                        article.Citiations = citiationsCheck.StartsWith("Cited by") ? citiationsCheck : "No citiations for this article. ";

                        scholarArticles.Insert(i - 1, article);
                    }

                    else
                    {
                        // case, when article do not has reference, but has a tag [citiation]/[book]
                        var xPathTitleCheck = $"//*[@id='gs_ccl_results']/div[{i}]/div/h3";
                        var xPathSpanNode = $"//*[@id='gs_ccl_results']/div[{i}]/div/h3/span";
                        var titleMatchNode = doc.DocumentNode.SelectSingleNode(xPathTitleCheck);
                        var spanNode = doc.DocumentNode.SelectSingleNode(xPathSpanNode);
                        titleMatchNode.RemoveChild(spanNode);
                        article.Title = titleMatchNode.InnerText;

                        string xPathInfo = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[1]";
                        article.Info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;

                        article.Reference = "This article does not have a reference";

                        var xPathCitiations = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[2]/a[1]";
                        var citiationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                        article.Citiations = citiationsCheck.StartsWith("Cited by") ? citiationsCheck : "No citiations for this article. ";

                        scholarArticles.Insert(i - 1, article);
                    }
                }
            }

            return scholarArticles;
        }
    }
}