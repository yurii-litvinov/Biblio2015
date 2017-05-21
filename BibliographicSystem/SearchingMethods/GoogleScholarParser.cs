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
        /// <summary>
        /// class constructor
        /// </summary>
        public GoogleScholarParser()
        {
            this.articles = new List<OutsideArticle>();
            this.IsSuccessful = true;
            this.problem = new Problem
            {
                Name = Problems.NoProblem,
                Content = ""
            };
        }

        public UserQuery Query { get; set; }

        public bool IsSuccessful { get; set; }

        public Problem GetProblem() => this.problem;

        public List<OutsideArticle> GetArticles() => this.articles;

        public void RequestArticles()
        {
            var articles = new List<OutsideArticle>();

            /// с шагом 10 мы получаем новый набор страниц
            for (int page = 0; page < 10; page += 10)
            {
                // getting page content from scholar page (with given query)
                var queryURL = GetQueryUrl(Query.MainInput, page, Query.ExactPhrase, Query.Without, Query.Head, Query.Published, Query.Authors, Query.DateStart, Query.DateEnd);
                var pageContent = GetPageContent(queryURL);
                var doc = new HtmlDocument();
                doc.LoadHtml(pageContent);
                var root = doc.DocumentNode;

                var captchaCheck = root.SelectSingleNode($"//*[@id='gs_res_bdy']/div[1]");
                if (captchaCheck == null)
                {
                    var captcha = root.SelectSingleNode("/html[1]/body[1]/div[1]/div[6]");
                    if (captcha != null)
                    {
                        this.IsSuccessful = false;
                        this.problem = new Problem
                        {
                            Name = Problems.GoogleScholarCaptcha,
                            Content = captcha.InnerHtml.Replace("6LfFDwUTAAAAAIyC8IeC3aGLqVpvrB6ZpkfmAibj", "6LfaXx8UAAAAAAMZOnRTw_CNOGUHZdr3mEUYUl2H")
                        };

                        return;
                    }

                    var captcha2 = root.SelectSingleNode("/html[1]/body[1]/div[1]");
                    if (captcha2 != null)
                    {
                        this.IsSuccessful = false;
                        this.problem = new Problem
                        {
                            Name = Problems.GoogleScholarCaptcha,
                            Content = captcha2.InnerHtml.Replace("6LfwuyUTAAAAAOAmoS0fdqijC2PbbdH4kjq62Y1b", "6LfaXx8UAAAAAAMZOnRTw_CNOGUHZdr3mEUYUl2H")
                        };

                        return;
                    }
                }

                var nodes = root.SelectSingleNode($"//*[@id='gs_ccl_results']").ChildNodes.Where(node => node.Name == "div");
                
                /// Case when a user profile appears
                if (nodes.Count() == 11)
                    nodes = nodes.Skip(1);

                foreach (var node in nodes)
                {
                    var n = node.LastChild.ChildNodes;
                    var title = n[0].InnerText;
                    var info = n[1].InnerText;
                    var references = new List<string>();
                    var citation = "";
                    var description = "";
                    try
                    {
                        citation = n[3].InnerText;
                        description = n[2].InnerText;
                        references.Add(n[0].LastChild.GetAttributeValue("href", null));
                    }
                    catch (ArgumentOutOfRangeException err)
                    {
                        // case, when article do not has reference, but has a tag [citiation]/[book] 
                        citation = n[2].InnerText;
                        description = "no description";
                    }

                    /// case when article has a bibliographic reference on the right
                    if (node.ChildNodes.Count == 2)
                        references.Add(node.FirstChild.LastChild.LastChild.LastChild.GetAttributeValue("href", null));

                    var article = new OutsideArticle()
                    {
                        From = "GS",
                        Title = title,
                        Info = info,
                        References = references,
                        Authors = GetAuthorsRefact(info),
                        Year = GetYearRefact(info),
                        Description = description,
                        CitationCount = citation.StartsWith("Cited by") ? Convert.ToInt32((citation.Split(' '))[2]) : 0
                    };

                    articles.Add(article);
                }


                this.articles = articles;
            }
        }

        private List<OutsideArticle> articles;
        private Problem problem;

        private List<Author> GetAuthorsRefact(string info)
        {
            var author = new Regex(@"[A-Z]{1,}\s{1}[A-Za-z]{1,}|[А-Я]{1,}\s{1}[А-Яа-я]{1,}");
            var matches = author.Matches(info);
            var authors = (from Match match in matches select match.Value);
            return authors.Select(name => new Author { AuthorName = name, AuthorId = 0 }).ToList();
        }

        private int GetYearRefact(string info)
        {
            const string yearPattern = @"\d{4}";
            var year = new Regex(yearPattern);
            var yearMatch = year.Match(info);
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
            try
            {
                //httpWebResponse.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                var response = (HttpWebResponse)request.GetResponse();
                var streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                var pageContent = streamReader.ReadToEnd();
                streamReader.Close();
                pageContent = HttpUtility.HtmlDecode(pageContent);
                return pageContent;
            }
            catch (WebException err)
            {
                var errorWebResponse = (HttpWebResponse)err.Response;
                var streamReader = new StreamReader(errorWebResponse.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                var pageContent = streamReader.ReadToEnd();
                streamReader.Close();
                pageContent = HttpUtility.HtmlDecode(pageContent);
                return pageContent;
            }
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