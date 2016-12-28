using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using BibliographicSystem.Models;
using HtmlAgilityPack;

namespace BibliographicSystem.SearchingMethods
{
    /// <summary>
    /// Class for parsing Google.Scholar 
    /// </summary>
    public class GoogleScholarParser
    {
        private string GetQueryUrl(string query, 
            int page,
            string exactPhrase = null,
            string without = null,
            bool head = false,
            string published = null,
            string author = null,
            int dateStart = int.MinValue,
            int dateEnd = int.MinValue)
        {
            query = HttpUtility.UrlEncode(query);
            if (!string.IsNullOrEmpty(exactPhrase))
                query += '+' + HttpUtility.UrlEncode('"' + exactPhrase + '"');
            if (!string.IsNullOrEmpty(without))
                query += '+' + HttpUtility.UrlEncode('-' + without);
            if (!string.IsNullOrEmpty(author))
                query += '+' + HttpUtility.UrlEncode("author:" + author);
            var url = "http://scholar.google.com/scholar?start="+ page + "&hl=en&q=";
            query = string.Concat(url, query);
            if (head)
                query += "&as_occt = title";
            if (dateStart  > 0)
                query += string.Concat("&as_ylo=", HttpUtility.UrlEncode(dateStart.ToString()));
            if (dateEnd > 0)
                query += string.Concat("&as_yhi=", HttpUtility.UrlEncode(dateEnd.ToString()));
            if (published != null)
                query += string.Concat("&as_publication=", HttpUtility.UrlEncode(published));
            return query;
        }
        
        private string GetPageContent(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
            var pageContent = streamReader.ReadToEnd();
            streamReader.Close();
            pageContent = HttpUtility.HtmlDecode(pageContent);
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

        public List<Author> GetAuthorsList(string articleInfo)
        {
            var author = new Regex(@"[A-Z]{1,}\s{1}[A-Za-z]{1,}|[А-Я]{1,}\s{1}[А-Яа-я]{1,}");
            var matches = author.Matches(articleInfo);
            var authors = (from Match match in matches select match.Value).ToList();
            var result = new List<Author>();
            foreach (var name in authors)
            {
                result.Add(new Author { AuthorName = name, AuthorId = 0 });
            }
            return result;
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
        /// Forms a name for article in BibTeX.
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

        /// <summary>
        /// Main class method
        /// </summary>
        /// <param name="query"> String from text box </param>
        /// <param name="page"> Number of page to find </param>
        /// <param name="exactPhrase"> Article should contains this phrase </param>
        /// <param name="without"> Articles should not contains this words </param>
        /// <param name="head"> Is searching only in article head </param>
        /// <param name="published"> Journal, where the article was published </param>
        /// <param name="author"> Author of article </param>
        /// <param name="dateStart"> Since date </param>
        /// <param name="dateEnd"> Till date </param>
        /// <returns> List of articles from Google.Scholar </returns>
        public List<CommonArticle> GetScholarArticlesByQuery(string query, 
            int page,
            string exactPhrase = null,
            string without = null,
            bool head = false,
            string published = null,
            string author = null,
            int dateStart = int.MinValue,
            int dateEnd = int.MinValue)
        {
            // getting page content from scholar page (with given query)
            query = GetQueryUrl(query, page, exactPhrase, without, head, published, author, dateStart, dateEnd);
            var pageContent = GetPageContent(query);

            // creating list of articles for "search on scholar" view
            var scholarArticles = new List<CommonArticle>();

            var doc = new HtmlDocument();
            doc.LoadHtml(pageContent);
            for (var i = 1; i <= 11; i++)
            {
                var article = new CommonArticle();
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

                    //adding info, description
                    var xPathInfo = $"//*[@id='gs_ccl_results']/div[{i}]/div[2]/div[2]";
                    var info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;
                    if (info != null)
                    {
                        var xPathDescription = $"//*[@id='gs_ccl_results']/div[{i}]/div[2]/div[1]";
                        var description = doc.DocumentNode.SelectSingleNode(xPathDescription).InnerText;
                        article.Description = description;
                    }
                    else
                    {
                        xPathInfo = $"//*[@id='gs_ccl_results']/div[{i}]/div[2]/div[1]";
                        info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;
                    }
                    article.Year = Convert.ToInt32(GetYear(info));
                    article.Authors = GetAuthorsList(info);

                    // adding reference
                    article.References.Add(doc.DocumentNode.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div[1]/div/div/a").GetAttributeValue("href", null));
                    //article.References.Add(refCheck.GetAttributeValue("href", null));

                    // adding citations amount
                    var xPathCitations = $"//*[@id='gs_ccl_results']/div[{i}]/div[2]/div[3]/a[1]";
                    var citationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitations).InnerText;
                    var citations = citationsCheck.StartsWith("Cited by") ? citationsCheck : "";
                    article.CitationCount = citations == "" ? 0 : Convert.ToInt32(Regex.Replace(citations, @"[^\d]+", ""));
                    scholarArticles.Add(article);
                }
                else
                {
                    var xPathRefCheck = $"//*[@id='gs_ccl_results']/div[{i}]/div/h3/a";
                    var refCheck = doc.DocumentNode.SelectSingleNode(xPathRefCheck);
                    if (refCheck != null)
                    {
                        var xPathTitle = $"//*[@id='gs_ccl_results']/div[{i}]/div/h3/a";
                        article.Title = doc.DocumentNode.SelectSingleNode(xPathTitle).InnerText;
                        
                        var xPathInfo = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[2]";
                        var info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;
                        if (info != null)
                        {
                            var xPathDescription = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[1]";
                            var description = doc.DocumentNode.SelectSingleNode(xPathDescription).InnerText;
                            article.Description = description;
                        }
                        else
                        {
                            xPathInfo = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[1]";
                            info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;
                        }
                        article.Year = Convert.ToInt32(GetYear(info));
                        article.Authors = GetAuthorsList(info);
                        //article.References.Add(refCheck.GetAttributeValue("href", null));

                        var xPathCitiations = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[3]/a[1]";
                        var citationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitiations).InnerText;
                        var citations = citationsCheck.StartsWith("Cited by") ? citationsCheck : "";
                        article.CitationCount = citations == "" ? 0 : Convert.ToInt32(Regex.Replace(citations, @"[^\d]+", ""));
                        scholarArticles.Add(article);
                    }
                    else
                    {
                        // case, when article do not has reference, but has a tag [citation]/[book]
                        var xPathTitleCheck = $"//*[@id='gs_ccl_results']/div[{i}]/div/h3";
                        var xPathSpanNode = $"//*[@id='gs_ccl_results']/div[{i}]/div/h3/span";
                        var titleMatchNode = doc.DocumentNode.SelectSingleNode(xPathTitleCheck);
                        if (titleMatchNode != null)
                        {
                            var spanNode = doc.DocumentNode.SelectSingleNode(xPathSpanNode);
                            titleMatchNode.RemoveChild(spanNode);
                            article.Title = titleMatchNode.InnerText;

                            var xPathInfo = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[2]";
                            var info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;
                            if (info != null)
                            {
                                var xPathDescription = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[1]";
                                var description = doc.DocumentNode.SelectSingleNode(xPathDescription).InnerText;
                                article.Description = description;
                            }
                            else
                            {
                                xPathInfo = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[1]";
                                info = doc.DocumentNode.SelectSingleNode(xPathInfo).InnerText;
                            }
                            article.Year = Convert.ToInt32(GetYear(info));
                            article.Authors = GetAuthorsList(info);

                            var xPathCitations = $"//*[@id='gs_ccl_results']/div[{i}]/div/div[2]/a[1]";
                            var citationsCheck = doc.DocumentNode.SelectSingleNode(xPathCitations).InnerText;
                            var citations = citationsCheck.StartsWith("Cited by") ? citationsCheck : "No citations for this article. ";
                            article.CitationCount = citations == "No citations for this article. " ? 0 : Convert.ToInt32(Regex.Replace(citations, @"[^\d]+", ""));
                            scholarArticles.Add(article);
                        }
                    }
                }
            }

            return scholarArticles;
        }
    }
}