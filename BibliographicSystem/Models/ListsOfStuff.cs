using System.Collections.Generic;
using System.Linq;

namespace BibliographicSystem.Models
{
    public class ListsOfStuff
    {
        public bool WrongFile { get; set; }
        public bool WrongDate { get; set; }

        public string ArticleName {get; set;}
        public string TagList {get; set;}
        public string Author {get;set;}
        public string Year { get; set; }
        public string Journal { get; set; }
        public string Publisher{ get; set; }
        public string Note { get; set; }

        private readonly AppContext db = new AppContext();

        public List<Group> GroupsByUserName (string name) => db.GroupByUser(name);

        public List<Article> ArticlesByUser (string name) =>
            (from a in db.UsersArticles.ToList() where name == a.UserName from art in db.Articles.ToList() where a.ArticleId == art.ArticleId select art).ToList();

        public List<string> TypesOfArticle() => new List<string> { "Книга", "Статья", "Другой тип" };
        
    }

    public class AddingClass
    {
        public int GroupId { get; set; }
        public string UserName { get; set; }
        private readonly ListsOfStuff lists = new ListsOfStuff();

        public List<Article> ArticlesToAdd() => 
            lists.ArticlesByUser(UserName).Where(l => l.GroupId == 0).ToList();
    }
}