using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BibliographicSystem.Models;

namespace BibliographicSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppContext db = new AppContext();
        private List<Article> list = new List<Article>();
        
        // GET: /Home/
        [HttpGet]
        public ActionResult Index()
        {
            var newList = new AddingToSystem
            {
                WrongDate = false,
                WrongFile = false
            };
            return View(newList);
        }

        public ActionResult About() => View();

        public ActionResult Contacts() => View();

        public ActionResult CreateGroup() => View();

        public ActionResult AddArticleToGroup(int id) =>
            View(new AddingToGroup { UserName = User.Identity.Name, GroupId = id });

        [HttpPost]
        public ActionResult AddArticleToGroup(int idGroup, int idArticle)
        {
            db.Articles.Find(idArticle).GroupId = idGroup;
            db.SaveChanges();
            return View("AddArticleToGroup", new AddingToGroup { UserName = User.Identity.Name, GroupId = idGroup });
        }

        [HttpPost]
        public ActionResult CreateGroup(string groupName, string theme)
        {
            if (groupName == null)
                return View();
            var group = new Group { GroupName = groupName, Theme = theme };
            db.Groups.Add(group);
            db.SaveChanges();
            db.UserInGroups.Add(new UserInGroup { UserName = User.Identity.Name, GroupId = group.GroupId });
            db.SaveChanges();
            return View("Finish");
        }

        public ActionResult GroupList() => View(db.Groups.ToList());

        [HttpPost]
        public ActionResult GroupList(string search) => View("GroupList", db.SearchGroupList(search));

        public ActionResult Details(int id) => View(db.Articles.Find(id));

        [HttpPost]
        public ActionResult Details(int articleId, int toMakeChoise)
        {
            db.Articles.Remove(db.Articles.Find(articleId));
            db.SaveChanges();
            return View("MainPage", db.Articles.ToList());
        }

        public ActionResult CorrectArticle(int id) => View(db.Articles.Find(id));

        [HttpPost]
        public ActionResult CorrectArticle(int articleId, string articleName, string author, string tagList, string year, string publisher, string journal, string note)
        {
            var article = db.Articles.Find(articleId);
            var newList = StringToList(tagList);
            var listTag = new List<Tag>();
            foreach (var tag in newList)
            {
                AddTag(new Tag { TagName = tag });
                db.SaveChanges();
            }

            foreach (var tag in newList)
            {
                listTag.AddRange(db.Tags.ToList().Where(t => t.TagName == tag));
            }

            article.Author = author;
            article.Journal = journal;
            article.Note = note;
            article.Publisher = publisher;
            article.Title = articleName;
            article.Year = year;
            article.Tags = listTag;
            CreateBibFile(article);
            db.SaveChanges();
            return View("MainPage", db.Articles.ToList());
        }

        public ActionResult MainPage()
        {
            var group = db.Groups.Find(1);
            CreateBibFile(group);
            return View(db.Articles.ToList());
        }

        [HttpPost]
        public ActionResult MainPage(string search)
        {
            if (search == string.Empty)
                return View("MainPage", db.Articles.ToList());
            list = db.SearchByTag(search);
            return View("MainPage", list);
        }

        public ActionResult GroupPage(int id)
        {
            var group = db.Groups.Find(id);
            group.Articles = db.ArticleByGroup(id);
            group.Users = db.UsersByGroup(id);
            return View(group);
        }

        public FileResult GetFile(int id)
        {
            var article = db.Articles.Find(id);
            var filePath = "~/Files/" + article.Path;
            var fileType = "application/pdf";
            var fileName = article.Path;
            return File(filePath, fileType, fileName);
        }

        public FileResult GetBibFile(int id)
        {
            var article = db.Articles.Find(id);
            var filePath = "~/BibFiles/" + article.Title.Split('.') + ".bib";
            var fileName = article.Title.Split('.') + ".bib";
            return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public string CreateBibFile(Article art)
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            var name = art.Title + ".bib";
            var path = directory + "/BibFiles/" + name;
            var textFile = new StreamWriter(path);
            AddBib(textFile, art);
            textFile.Close();
            return name;
        }

        public string CreateBibFile(string username)
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            var name = username + ".bib";
            var path = directory + "/BibFiles/" + name;
            var textFile = new StreamWriter(path);
            var x = db.Articles.ToList();
            foreach (var art in x)
            {
                if (art.UserName == username)
                    AddBib(textFile, art);
            }

            textFile.Close();
            return name;
        }

        public string CreateBibFile(Group group)
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            var name = group.GroupName + ".bib";
            var path = directory + "/BibFiles/" + name;
            var textFile = new StreamWriter(path);
            var articles = db.ArticleByGroup(group.GroupId);
            foreach (var art in articles)
            {
                AddBib(textFile, art);
            }

            textFile.Close();
            return name;
        }

        [HttpPost]
        public ActionResult Index(
            IEnumerable<HttpPostedFileBase> fileUpload,
            string articleName,
            string tagList,
            string author, 
            string year, 
            string journal, 
            string publisher, 
            string note)
        {
            if (articleName == string.Empty)
                return View(new AddingToSystem());

            if ((DateTime.Now.Year <= Convert.ToInt32(year)) || (Convert.ToInt32(year) <= 1500))
            {
                return View(new AddingToSystem
                {
                    ArticleName = articleName,
                    Author = author,
                    Journal = journal,
                    Note = note,
                    Publisher = publisher,
                    TagList = tagList,
                    WrongDate = true
                });
            }

            foreach (var file in fileUpload)
            {
                var filename = string.Empty;
                if (file != null)
                {
                    const string AllowFormat = "pdf";
                    var fileExt = Path.GetExtension(file.FileName)?.Substring(1);
                    if (fileExt != AllowFormat)
                    {
                        return View(new AddingToSystem
                        {
                            ArticleName = articleName,
                            Author = author,
                            Journal = journal,
                            Note = note,
                            Publisher = publisher,
                            TagList = tagList,
                            Year = year,
                            WrongFile = true
                        });
                    }

                    filename = Path.GetFileName(file.FileName);
                    file.SaveAs(Server.MapPath("~/Files/" + filename));
                }

                var newList = StringToList(tagList);
                var tags = new List<Tag>();
                foreach (var tag in newList)
                {
                    AddTag(new Tag { TagName = tag });
                    db.SaveChanges();
                }

                foreach (var item in newList)
                {
                    tags.AddRange(db.Tags.ToList().Where(t => t.TagName == item));
                }

                var type = Request.Form["TypeT"];
                var groupId = Convert.ToInt32(Request.Form["GroupT"]);

                var article = new Article { Title = articleName, Path = filename, Tags = tags, Author = author, Type = type,
                    Journal = journal, Note = note, Publisher = publisher, Year = year, UserName = User.Identity.Name, GroupId = groupId };
                db.Articles.Add(article);
                CreateBibFile(article);
                db.SaveChanges();
                var usersArticle = new UsersArticle { UserName = User.Identity.Name, ArticleId = article.ArticleId };
                db.UsersArticles.Add(usersArticle);
                db.SaveChanges();
            }

            return RedirectToAction("Finish");
        }

        public ActionResult ErrorOccured() => View();

        public ActionResult Finish() => View();

        /// <summary>
        /// Interpret string to list of words (Sequences, separated by spaces)
        /// </summary>
        /// <param name="str">  </param>
        /// <returns> List of words in given string </returns>
        public List<string> StringToList(string str)
        {
            var newList = new List<string>();
            var word = string.Empty;
            foreach (var character in str)
            {
                if (character != ' ')
                {
                    word = word + character;
                }
                else
                {
                    newList.Add(word);
                    word = string.Empty;
                }
            }

            newList.Add(word);
            return newList;
        }

        public void AddTag(Tag tag)
        {
            bool add = true;
            foreach (var t in db.Tags.ToList())
            {
                if (tag.TagName == t.TagName)
                    add = false;
            }

            if (add)
                db.Tags.Add(tag);
        }

        public void WideIn(int id) => db.AddUserInGroup(id, User.Identity.Name);

        [HttpPost]
        public ActionResult GroupPage(string wade, int groupId)
        {
            switch (wade)
            {
                case "-1":
                    var user = new UserInGroup();
                    foreach (var group in db.UserInGroups.ToList())
                    {
                        if (group.UserName == User.Identity.Name && group.GroupId == groupId)
                            user = group;
                    }

                    db.UserInGroups.Remove(user);
                    db.SaveChanges();
                    break;
                case "-2":
                    db.AddUserInGroup(groupId, User.Identity.Name);
                    db.SaveChanges();
                    break;
                default:
                    db.Articles.Find(Convert.ToInt32(wade)).GroupId = 0;
                    db.SaveChanges();
                    break;
            }

            var model = db.Groups.Find(groupId);
            model.Articles = db.ArticleByGroup(groupId);
            model.Users = db.UsersByGroup(groupId);
            return View("GroupPage", model);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private void AddBib(TextWriter textFile, Article art)
        {
            switch (art.Type)
            {
                case "Статья":
                    textFile.WriteLine("@ARTICLE{");
                    break;
                case "Книга":
                    textFile.WriteLine("@BOOK{");
                    break;
            }

            textFile.WriteLine("author = {" + art.Author + "},");
            textFile.WriteLine("title = {«" + art.Title + "»},");

            if (art.Publisher != string.Empty)
                textFile.WriteLine("publisher = {" + art.Publisher + "},");

            if (art.Journal != string.Empty)
                textFile.WriteLine("journal = {" + art.Journal + "},");

            if (art.Year != string.Empty)
                textFile.WriteLine("year = {" + art.Year + "},");

            textFile.WriteLine("note = {" + art.Note + "},");
            textFile.WriteLine("}");
        }
    }
}
