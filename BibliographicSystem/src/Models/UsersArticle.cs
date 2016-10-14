using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BootApp.Models
{
    public class UsersArticle
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public string UserName { get; set; }
    }
}