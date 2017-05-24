using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BibliographicSystem.Models;

namespace BibliographicSystem.SearchingMethods
{
    public enum Problems { NoProblem, GoogleScholarCaptcha };

    /// <summary>
    /// Class that represents the web response problem
    /// </summary>
    public class Problem
    {
        public Problems Name { get; set; }
        public string Content { get; set; }
    }

    /// <summary>
    /// a class for storing precise user request
    /// </summary>
    public class UserQuery
    {
        public string MainInput { get; set; }
        public string Authors { get; set; }
        public string Year { get; set; }
        public string Count { get; set; }
        //Article should contains this phrase
        public string ExactPhrase { get; set; }
        //Articles should not contains this words 
        public string Without { get; set; }
        //Is searching only in article head 
        public bool Head { get; set; }
        //Journal, where the article was published 
        public string Published { get; set; }
        public int DateStart { get; set; }
        public int DateEnd { get; set; }
    }

    public class QueryProcessor
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="query"> User query composed of textbox data </param>
        public QueryProcessor(UserQuery query)
        {
            this.userQuery = query;
            this.parsers = new List<IParser>()
            {
                new GoogleScholarParser() { Query = query },
                new MicrosoftAcademicParser() { Query = query}
            };
        }

        /// <summary>
        /// Requests articles from different systems
        /// </summary>
        public void DownloadArticles() => parsers.ForEach(pars => pars.RequestArticles());

        /// <summary>
        /// Verify the success of the request
        /// </summary>
        /// <returns></returns>
        public bool IsSuccess() => parsers.TrueForAll(pars => pars.IsSuccessful);

        /// <summary>
        /// Returns the status of a successful request to the system
        /// </summary>
        public void SetSuccessfulState() => parsers.ForEach(pars => pars.IsSuccessful = true);

        /// <summary>
        /// Returns messages about problems that occurred during the query process
        /// </summary>
        /// <returns></returns>
        public string GetProblems() => this.parsers.Aggregate("", (acc, pars) => pars.GetProblem().Content + acc);

        /// <summary>
        /// Returns articles after processing
        /// </summary>
        /// <returns> articles from different bibliographic systems </returns>
        public List<OutsideArticle> GetSearchResult() => parsers.SelectMany(pars => pars.GetArticles()).ToList();

        private UserQuery userQuery;
        private List<IParser> parsers;
    }
}