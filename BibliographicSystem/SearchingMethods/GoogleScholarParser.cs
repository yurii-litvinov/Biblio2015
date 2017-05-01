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
    public class GoogleScholarParser : IParser
    {
        public UserQuery Query { get; set; }

        /// <summary>
        /// Main class method
        /// </summary>
        /// <param name="query"> aggregate information the user entered in the text boxes </param>
        /// <returns> List of articles from Google.Scholar </returns>
        public List<OutsideArticle> GetArticles()
        {
            var articles = new List<OutsideArticle>();

            /// с шагом 10 мы получаем новый набор страниц
            for (int page = 0; page < 100; page += 10)
            {
                // getting page content from scholar page (with given query)
                var queryURL = GetQueryUrl(Query.MainInput, page, Query.ExactPhrase, Query.Without, Query.Head, Query.Published, Query.Authors, Query.DateStart, Query.DateEnd);
                var pageContent = GetPageContent(queryURL);

                var doc = new HtmlDocument();
                doc.LoadHtml(pageContent);
                var root = doc.DocumentNode;

                for (var i = 1; i <= 10; i++)
                {
                    string title = "";
                    string reference = "";
                    string citation = "";
                    string info = "";

                    //*[@id="gs_ccl_results_results"]/div[1]
                    var biblioCheck = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div[2]");
                    if (biblioCheck != null)
                    {
                        // adding title of article //by xPath of title
                        title = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div[2]/h3/a").InnerText;

                        // adding info 
                        info = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div[2]/div[1]").InnerText;

                        // adding reference
                        var refCheck = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div[2]/h3/a");
                        reference = refCheck.GetAttributeValue("href", null);

                        // adding citiations amount
                        citation = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div[2]/div[3]/a[1]").InnerText;
                    }
                    else
                    {
                        info = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div/div[1]").InnerText;

                        var refCheck = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div/h3/a");
                        if (refCheck != null)
                        {
                            title = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div/h3/a").InnerText;
                            reference = refCheck.GetAttributeValue("href", null);
                            citation = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div/div[3]/a[1]").InnerText;
                        }
                        else
                        {
                            // case, when article do not has reference, but has a tag [citiation]/[book]
                            var titleMatchNode = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div/h3");
                            if (titleMatchNode != null)
                            {
                                var spanNode = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div/h3/span");
                                titleMatchNode.RemoveChild(spanNode);

                                title = titleMatchNode.InnerText;
                                reference = "empty";
                                citation = root.SelectSingleNode($"//*[@id='gs_ccl_results']/div[{i}]/div/div[2]/a[1]").InnerText;
                            }
                        }
                    }

                    var article = new OutsideArticle
                    {
                        Title = title,
                        Description = info,
                        CitationCount = citation.StartsWith("Cited by") ? Convert.ToInt32((citation.Split(' '))[2]) : 0,
                        Year = this.GetYearRefact(info),
                        References = new List<string>(),
                        Authors = this.GetAuthorsRefact(info),
                        Info = info,
                        Reference = reference,
                    };

                    if (reference != "empty")
                        article.References.Add(reference);

                    articles.Add(article);
                }
            }

            return articles;
        }

        private List<Author> GetAuthorsRefact(string description)
        {
            var author = new Regex(@"[A-Z]{1,}\s{1}[A-Za-z]{1,}|[А-Я]{1,}\s{1}[А-Яа-я]{1,}");
            var matches = author.Matches(description);
            var authors = (from Match match in matches select match.Value);
            return authors.Select(name => new Author { AuthorName = name, AuthorId = 0 }).ToList();
        }

        private int GetYearRefact(string articleInfo)
        {
            const string yearPattern = @"\d{4}";
            var year = new Regex(yearPattern);
            var yearMatch = year.Match(articleInfo);
            return yearMatch.Success ? Convert.ToInt32(yearMatch.Value) : 0;
        }

        #region функции для парсинга(устаревшие)

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

        #endregion

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

        private string GetPageContent(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
            var pageContent = streamReader.ReadToEnd();
            streamReader.Close();
            pageContent = HttpUtility.HtmlDecode(pageContent);
            return pageContent;

            //var request = (HttpWebRequest)WebRequest.Create(url);

            //try
            //{
            //    HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();
            //    httpWebResponse.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

            //    var response = (HttpWebResponse)request.GetResponse();
            //    var streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
            //    var pageContent = streamReader.ReadToEnd();
            //    streamReader.Close();
            //    pageContent = HttpUtility.HtmlDecode(pageContent);
            //    return pageContent;

            //}
            //catch (WebException err)
            //{
            //    var errorWebResponse = (HttpWebResponse)err.Response;
            //    Stream stream = errorWebResponse.GetResponseStream();
            //    var stream1= new StreamReader(errorWebResponse.GetResponseStream(), Encoding.GetEncoding("utf-8"));
            //    var pageContent1 = stream1.ReadToEnd();
            //    stream1.Close();
            //    pageContent1 = HttpUtility.HtmlDecode(pageContent1);
            //    return pageContent1;
            //}
        }

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
            var url = "http://scholar.google.com/scholar?start=" + page + "&hl=en&q=";
            query = string.Concat(url, query);
            if (head)
                query += "&as_occt = title";
            if (dateStart > 0)
                query += string.Concat("&as_ylo=", HttpUtility.UrlEncode(dateStart.ToString()));
            if (dateEnd > 0)
                query += string.Concat("&as_yhi=", HttpUtility.UrlEncode(dateEnd.ToString()));
            if (published != null)
                query += string.Concat("&as_publication=", HttpUtility.UrlEncode(published));
            return query;
        }
    }
}