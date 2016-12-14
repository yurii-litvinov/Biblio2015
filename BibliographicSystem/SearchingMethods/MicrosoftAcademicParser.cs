using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;

using System.IO;
using System.Net;
using Newtonsoft.Json;
using BibliographicSystem.Models;


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
            rootObject = new Response();
        }

        #region классы для десериализации json
        public class AA
        {
            public string AuN { get; set; }
            public long AuId { get; set; }
        }

        public class Entity
        {
            public double prob { get; set; }
            public string Ti { get; set; }
            public int Y { get; set; }
            public int CC { get; set; }
            public string E { get; set; }
            public List<AA> AA { get; set; }
        }

        public class Response
        {
            public string expr { get; set; }
            public List<Entity> entities { get; set; }
        }

        public class ResponseConverter : Newtonsoft.Json.Converters.CustomCreationConverter<Response>
        {
            public override Response Create(Type objectType)
            {
                return new Response();
            }
        }

        public class MetadataConverter : Newtonsoft.Json.Converters.CustomCreationConverter<Extended>
        {
            public override Extended Create(Type objectType)
            {
                return new Extended();
            }
        }

        /// <summary>
        /// for deserialization extended metadata
        /// </summary>
        public class S
        {
            public int Ty { get; set; }
            public string U { get; set; }
        }

        /// <summary>
        /// for deserialization extended metadata
        /// </summary>
        public class Extended
        {
            public string DN { get; set; }
            public string D { get; set; }
            public IList<S> S { get; set; }
            public string VFN { get; set; }
            public int V { get; set; }
            public int I { get; set; }
            public int FP { get; set; }
            public int LP { get; set; }
            public string DOI { get; set; }
        }

        #endregion

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
        public List<MicrosoftAcademicArticle> GetSearchResult()
        {
            var listOfArticles = new List<MicrosoftAcademicArticle>();
            var expressions = GetListOfExpr();
            var count = (userQuery.Count == ""? "10" : userQuery.Count);
            foreach (var expression in expressions)
            {

                count = (Convert.ToInt32(count) - listOfArticles.Count).ToString();
                MakeGetRequest(expression, count);

                foreach (var element in rootObject.entities)
                {
                    var article = CopyData(element);
                    listOfArticles.Add(article);
                }

                if (listOfArticles.Count.ToString() == userQuery.Count)
                {
                    break;
                }
            }
            
            return listOfArticles;

        }

        /// <summary>
        /// makes get request to Microsoft Academic
        /// </summary>
        /// <param name="expression">valid query string</param>
        /// <param name="count">the number of articles, that the user wants to see</param>
        private void MakeGetRequest(string expression, string count)
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
                rootObject = JsonConvert.DeserializeObject<Response>(json, new ResponseConverter());
            }
        }

        /// <summary>
        /// returns the list of query string which are not contrary Query Expression Syntax
        /// </summary>
        /// <returns></returns>
        private List<string> GetListOfExpr()
        {
            var listOfExpr = new List<string>();
            var expression = "And(";

            if (userQuery.Authors.Length != 0)
            {
                var inputtedAuthors = userQuery.Authors.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                expression += "Or(";
                foreach (var author in inputtedAuthors)
                {
                    expression += "Composite(AA.AuN='" + author + "'),";
                }

                expression = expression.Remove(expression.Length - 1, 1);
                expression += "),";
            }
            
            if (userQuery.MainInput.Length != 0)
            {
                var inputtedWords = userQuery.MainInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                expression += "Or(";
                foreach (var word in inputtedWords)
                {
                    expression += "W='" + word + "',";
                }

                expression = expression.Remove(expression.Length - 1, 1);
                expression += ")";
            }

            if (userQuery.Year.Length != 0)
            {
                expression += ",Y=" + userQuery.Year;
            }

            expression += ")";
            listOfExpr.Add(expression);

            /// replace "And" to "Or"
            expression = "Or" + expression.Remove(0, 3);
            listOfExpr.Add(expression);

            return listOfExpr;
        }

        /// <summary>
        /// creates an article, copies the required data from deserialized object, and returns this article
        /// </summary>
        /// <param name="entity">deserialized object</param>
        /// <returns></returns>
        private MicrosoftAcademicArticle CopyData(Entity entity)
        {
            Extended extendedMetadata = JsonConvert.DeserializeObject<Extended>(entity.E, new MetadataConverter());
            var article = new MicrosoftAcademicArticle
            {
                Year = entity.Y,
                CitationCount = entity.CC,
                ExtendedMetadata = entity.E,
                Title = extendedMetadata.DN,
            };

            article.References = new List<string>();
            if (extendedMetadata.S != null)
            {
                foreach (var reference in extendedMetadata.S)
                {
                    article.References.Add(reference.U);
                }
            }

            article.Authors = new List<Author>();
            if (entity.AA != null)
            {
                foreach (var author in entity.AA)
                {
                    article.Authors.Add(new Author { AuthorName = author.AuN, AuthorId = author.AuId });
                }
            }

            article.Description = extendedMetadata.D == null ? "¯\\_(ツ)_/¯" : extendedMetadata.D;
            return article;
        }

        private UserQuery userQuery;
        private Response rootObject;
    }
}