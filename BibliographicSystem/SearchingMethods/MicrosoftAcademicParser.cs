using System;
using System.Collections.Generic;
using System.Web;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using BibliographicSystem.Models;
using System.Linq;

namespace BibliographicSystem.SearchingMethods
{
    /// <summary>
    /// class for parsing get requests from MicrosoftAcademic
    /// </summary>
    public class MicrosoftAcademicParser
    {
        public MicrosoftAcademicParser(UserQuery userQuery)
        {
            this.userQuery = userQuery;
            response = new Response();
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
        }

        /// <summary>
        /// returns list of articles
        /// </summary>
        /// <returns></returns>
        public List<CommonArticle> GetSearchResult()
        {
            var listOfArticles = new List<CommonArticle>();
            var expressions = GetListOfExpr();
            var count = (userQuery.Count == ""? "10" : userQuery.Count);
            foreach (var expression in expressions)
            {
                count = (Convert.ToInt32(count) - listOfArticles.Count).ToString();
                var responseStatusCode = MakeGetRequest(expression, count);

                if (responseStatusCode == HttpStatusCode.OK)
                {
                    listOfArticles.AddRange(response.entities.Select(CopyData));
                }
                
                if (listOfArticles.Count.ToString() == userQuery.Count)
                {
                    break;
                }
            }
            
            return listOfArticles;
        }

        /// <summary>
        /// makes get request to Microsoft Academic and returns response status code
        /// </summary>
        /// <param name="expression">valid query string</param>
        /// <param name="count">the number of articles, that the user wants to see</param>
        private HttpStatusCode MakeGetRequest(string expression, string count)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "a89fcc82bd8049738b2f76c667f0f52c");

            // Request parameters
            queryString["expr"] = expression;
            queryString["model"] = "latest";
            queryString["count"] = count;
            queryString["offset"] = "0";
            queryString["orderby"] = "";
            queryString["attributes"] = "Ti,Y,AA.AuN,AA.AuId,CC,E";
            var uri = "https://api.projectoxford.ai/academic/v1.0/evaluate?" + queryString;

            var response = client.GetAsync(uri).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                this.response = JsonConvert.DeserializeObject<Response>(json, new ResponseConverter());
            }

            return response.StatusCode;
        }

        /// <summary>
        /// returns the list of query string which are not contrary Query Expression Syntax
        /// </summary>
        /// <returns></returns>
        private List<string> GetListOfExpr()
        {
            var authorsInQuery = "";
            if (userQuery.Authors.Length != 0)
            {
                var inputtedAuthors = userQuery.Authors.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var author in inputtedAuthors)
                {
                    var fullName = author.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    authorsInQuery += ",Composite(AA.AuN=='";
                    authorsInQuery = fullName.Aggregate(authorsInQuery, (current, word) => current + (word + ' '));
                    authorsInQuery = authorsInQuery.Remove(authorsInQuery.Length - 1);
                    authorsInQuery += "')";
                }

                authorsInQuery = authorsInQuery.Substring(1);  // without ,
                authorsInQuery = '(' + authorsInQuery + ')';
            }

            var wordsInQuery = "";
            if (userQuery.MainInput.Length != 0)
            {
                var inputtedWords = userQuery.MainInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                wordsInQuery = inputtedWords.Aggregate(wordsInQuery, (current, word) => current + ",W=='" + word + "'");
                wordsInQuery = wordsInQuery.Substring(1);  // without ,
                wordsInQuery = '(' + wordsInQuery + ')';
            }    

            var orWordsInQuery = "";
            var andWordsInQuery = "";
            if (wordsInQuery.Length != 0)
            {
                orWordsInQuery = "Or" + wordsInQuery + ",";
                andWordsInQuery = "And" + wordsInQuery + ",";
            }

            var orAuthorsInQuery = "";
            var andAuthorsInQuery = "";
            if (authorsInQuery.Length != 0)
            {
                orAuthorsInQuery = "Or" + authorsInQuery + ",";
                andAuthorsInQuery = "And" + authorsInQuery + ",";
            }

            var yearInQuery = "";
            if (userQuery.Year.Length != 0)
            {
                yearInQuery = "Y=" + userQuery.Year + ",";
            }

            var and = andAuthorsInQuery + andWordsInQuery + yearInQuery;
            var or = orAuthorsInQuery + orWordsInQuery + yearInQuery;
            and = and.Remove(and.Length - 1);
            or = or.Remove(or.Length - 1);

            var listOfExpr = new List<string>();
            listOfExpr.Add("And(" + and + ")");
            listOfExpr.Add("And(" + or + ")");
            listOfExpr.Add("Or(" + and + ")");
            listOfExpr.Add("Or(" + or + ")");
            return listOfExpr;
        }

        /// <summary>
        /// creates an article, copies the required data from deserialized object, and returns this article
        /// </summary>
        /// <param name="entity">deserialized object</param>
        /// <returns></returns>
        private CommonArticle CopyData(Entity entity)
        {
            var extendedMetadata = JsonConvert.DeserializeObject<Extended>(entity.E, new MetadataConverter());
            var article = new CommonArticle
            {
                Year = entity.Y,
                CitationCount = entity.CC,
                ExtendedMetadata = entity.E,
                Title = extendedMetadata.DN,
                References = new List<string>(),
                Authors = new List<Author>()
            };

            if (extendedMetadata.S != null)
            {
                foreach (var reference in extendedMetadata.S)
                {
                    article.References.Add(reference.U);
                }
            }

            if (entity.AA != null)
            {
                foreach (var author in entity.AA)
                {
                    article.Authors.Add(new Author{ AuthorName = author.AuN, AuthorId = author.AuId });
                }
            }

            article.Description = extendedMetadata.D ?? "¯\\_(ツ)_/¯";
            return article;
        }

        private UserQuery userQuery;
        private Response response;

        #region классы для десериализации json
        /// <summary>
        /// author of article
        /// </summary>
        private class AA
        {
            /// <summary>
            /// author name
            /// </summary>
            public string AuN { get; set; }
            /// <summary>
            /// author Id
            /// </summary>
            public long AuId { get; set; }
        }

        /// <summary>
        /// this class represents an article
        /// </summary>
        private class Entity
        {
            public double prob { get; set; }
            /// <summary>
            /// title of article
            /// </summary>
            public string Ti { get; set; }
            /// <summary>
            /// the year of publication of article
            /// </summary>
            public int Y { get; set; }
            /// <summary>
            /// citation count
            /// </summary>
            public int CC { get; set; }
            /// <summary>
            /// Extended metadata
            /// </summary>
            public string E { get; set; }
            /// <summary>
            /// authors
            /// </summary>
            public List<AA> AA { get; set; }
        }

        /// <summary>
        /// this class represents response message from Microsoft Academic Api
        /// </summary>
        private class Response
        {
            public string expr { get; set; }
            public List<Entity> entities { get; set; }
        }

        /// <summary>
        /// converter for json deserialization
        /// </summary>
        private class ResponseConverter : Newtonsoft.Json.Converters.CustomCreationConverter<Response>
        {
            public override Response Create(Type objectType) => new Response();
        }

        /// <summary>
        /// converter for json deserialization
        /// </summary>
        private class MetadataConverter : Newtonsoft.Json.Converters.CustomCreationConverter<Extended>
        {
            public override Extended Create(Type objectType) => new Extended();
        }

        /// <summary>
        /// link to article
        /// </summary>
        private class S
        {
            /// <summary>
            /// the link type
            /// </summary>
            public int Ty { get; set; }
            /// <summary>
            /// the link itself
            /// </summary>
            public string U { get; set; }
        }

        /// <summary>
        /// this class represents extended metadata of article
        /// </summary>
        private class Extended
        {
            /// <summary>
            /// name of article
            /// </summary>
            public string DN { get; set; }
            /// <summary>
            /// description
            /// </summary>
            public string D { get; set; }
            /// <summary>
            /// list of references
            /// </summary>
            public IList<S> S { get; set; }
            /// <summary>
            /// Venue Full Name - full name of the Journal or Conference
            /// </summary>
            public string VFN { get; set; }
            /// <summary>
            /// journal volume
            /// </summary>
            public int V { get; set; }
            /// <summary>
            /// journal issue
            /// </summary>
            public int I { get; set; }
            /// <summary>
            /// first page of paper
            /// </summary>
            public int FP { get; set; }
            /// <summary>
            /// last page of paper
            /// </summary>
            public int LP { get; set; }
            /// <summary>
            /// Digital Object Identifier
            /// </summary>
            public string DOI { get; set; }
        }

        #endregion
    }
}