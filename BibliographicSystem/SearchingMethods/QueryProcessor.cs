using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BibliographicSystem.Models;

namespace BibliographicSystem.SearchingMethods
{
    /// <summary>
    /// a class for storing precise user request
    /// </summary>
    public class UserQuery
    {
        public string MainInput { get; set; }
        public string Authors { get; set; }
        public string Year { get; set; }
        public string Count { get; set; }
        public string ExactPhrase { get; set; }   //Article should contains this phrase
        public string Without { get; set; }      //Articles should not contains this words 
        public bool Head { get; set; }          //Is searching only in article head 
        public string Published { get; set; }       //Journal, where the article was published 
        public int DateStart { get; set; }
        public int DateEnd { get; set; }
    }


    public class QueryProcessor
    {
        public QueryProcessor(UserQuery query)
        {
            this.userQuery = query;
            this.parsers = new List<IParser>()
            {
                new GoogleScholarParser() { Query = query },
                new MicrosoftAcademicParser() { Query = query}
            };

        }

        public IEnumerable<OutsideArticle> GetSearchResult()
        {
            var articles = new List<OutsideArticle>();
            parsers.ForEach((pars) => articles.AddRange(pars.GetArticles()));
            return articles;
        }

        private UserQuery userQuery;
        private List<IParser> parsers;
    }
}