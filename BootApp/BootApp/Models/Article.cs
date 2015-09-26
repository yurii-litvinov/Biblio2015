using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace BootApp.Models
{
    public class Article
    {
        /*public int ArticleId { get; set; }
        public string ArticleTitle { get; set; }
        public string Author { get; set; }
        public string Adstract { get; set; }
        public string Type { get; set; }
        public DateTime DateOfPublish { get; set; }
        public string Editor { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public Article()
        {
            Tags = new List<Tag>();
        }*/

        public int ArticleId { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public string journal { get; set; }
        public string publisher { get; set; }
        //[StringLength(4, MinimumLength = 4, ErrorMessage = "Неверный формат")]
        public string year { get; set; }
        public string note { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public string UserName { get; set; }
        public int GroupId { get; set; }


        public virtual ICollection<Tag> Tags { get; set; }
        public Article()
        {
            Tags = new List<Tag>();
        }
    }
}