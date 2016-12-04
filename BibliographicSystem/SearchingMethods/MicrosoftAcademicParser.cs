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
        public MicrosoftAcademicParser(string query)
        {
            this.query = query;
            rootObject = new RootObject();
        }


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
            public List<AA> AA { get; set; }
        }

        public class RootObject
        {
            public string expr { get; set; }
            public List<Entity> entities { get; set; }
        }

        public class RootConverter : Newtonsoft.Json.Converters.CustomCreationConverter<RootObject>
        {
            public override RootObject Create(Type objectType)
            {
                return new RootObject();
            }
        }

        public List<MicrosoftAcademicArticle> GetSearchResult()
        {
            var listOfArticles = new List<MicrosoftAcademicArticle>();
            MakeGetRequest();
            foreach (var element in rootObject.entities)
            {
                listOfArticles.Add(new MicrosoftAcademicArticle { Title = element.Ti, Year = element.Y });
            }

            return listOfArticles;

        }

        public void MakeGetRequest()
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "a89fcc82bd8049738b2f76c667f0f52c");

            // Request parameters
            queryString["expr"] = "Composite(AA.AuN=='jaime teevan')";
            queryString["model"] = "latest";
            queryString["count"] = "10";
            queryString["offset"] = "0";
            queryString["orderby"] = "";
            queryString["attributes"] = "";
            var uri = "https://api.projectoxford.ai/academic/v1.0/evaluate?" + queryString;

            var response = client.GetAsync(uri).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                rootObject = JsonConvert.DeserializeObject<RootObject>(json, new RootConverter());
            }
        }

        private string query;
        private RootObject rootObject;
    }
}