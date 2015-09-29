using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BootApp.Models
{
    public class ListsOfStuff
    {
        public bool wrongFile { get; set; }
        public bool wrongDate { get; set; }

        public string ArticleName {get; set;}
        public string TagList {get; set;}
        public string Author {get;set;}
        public string Year { get; set; }
        public string Journal { get; set; }
        public string Publisher{ get; set; }
        public string Note { get; set; }
        AppContext db = new AppContext();
        public List<Group> GroupsByUserName (string name)
        {
            return db.GroupByUser(name);
        }

        public List<Article> ArticlesByUser (string name)
        {
            List<Article> list = new List<Article>();
            foreach (var a in db.UsersArticles.ToList())
            {
                if (name == a.UserName)
                {
                    foreach (var art in db.Articles.ToList())
                    {
                        if (a.ArticleId == art.ArticleId)
                            list.Add(art);
                    }
                }
            }
            return list;
        }

        public List<string> TypesOfArticle()
        {
            List<string> list = new List<string> { "Книга", "Статья", "Другой тип" };
            return list;
        }
    }

    public class AddingClass
    {
        public int GroupId { get; set; }
        public string UserName { get; set; }
        ListsOfStuff lists = new ListsOfStuff();

        public List<Article> ArticlesToAdd()
        {
            List<Article> list = new List<Article>();
            foreach (var l in lists.ArticlesByUser(UserName))
            {
                if (l.GroupId == 0)
                {
                    list.Add(l);
                }
            }
            return list;
        }
    }
}