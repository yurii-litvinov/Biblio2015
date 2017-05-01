using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    interface IParser
    {
        List<OutsideArticle> GetArticles();

        UserQuery Query { get; set; }
    }
}
