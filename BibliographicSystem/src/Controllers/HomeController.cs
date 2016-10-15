using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BibliographicSystem.Models;

namespace BibliographicSystem.Controllers
{
    public class HomeController : Controller
    {
        AppContext db = new AppContext();
        List<Article> list = new List<Article>();
       
        //
        // GET: /Home/
        [HttpGet]
        public ActionResult Index()
        {
            ListsOfStuff list1 = new ListsOfStuff();
            list1.wrongDate = false;
            list1.wrongFile = false;
            return View(list1);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult CreateGroup()
        {
            return View();
        }

        public ActionResult AddArticleToGroup(int id)
        {
            AddingClass a = new AddingClass { UserName = User.Identity.Name, GroupId = id };
            return View(a);
        }

        [HttpPost]
        public ActionResult AddArticleToGroup(int idGroup, int idArticle)
        {
            db.Articles.Find(idArticle).GroupId = idGroup;
            db.SaveChanges();
            AddingClass a = new AddingClass { UserName = User.Identity.Name, GroupId = idGroup };
            return View("AddArticleToGroup", a);
        }

        [HttpPost]
        public ActionResult CreateGroup(string GroupName, string Theme)
        {
            if (GroupName == null) return View();
            Group g = new Group {GroupName = GroupName, Theme = Theme};
            db.Groups.Add(g);
            db.SaveChanges();
            UserInGroup ug = new UserInGroup {UserName = User.Identity.Name, GroupId = g.GroupId};
            db.UserInGroups.Add(ug);
            db.SaveChanges();
            return View("Finish");
        }

        public ActionResult GroupList()
        {
            return View(db.Groups.ToList());
        }

        [HttpPost]
        public ActionResult GroupList(string Search)
        {
            List<Group> list = db.SearchGroupList(Search);
            return View("GroupList", list);
        }

        public ActionResult Details(int id)
        {
            Article a = db.Articles.Find(id);
            return View(a);
        }

        [HttpPost]
        public ActionResult Details(int ArticleId, int ToMakeChoise)
        {
                db.Articles.Remove(db.Articles.Find(ArticleId));
                db.SaveChanges();
                return View("MainPage", db.Articles.ToList());
        }

        public ActionResult CorrectArticle(int id)
        {
            return View(db.Articles.Find(id));
        }

        [HttpPost]
        public ActionResult CorrectArticle(int ArticleId, string ArticleName, string Author, string TagList, string Year, string Publisher, string Journal, string Note)
        {
            Article a = db.Articles.Find(ArticleId);
            List<string> list = StringToList(TagList);
            List<Tag> lT = new List<Tag>();
            for (int i = 0; i < list.Count; i++)
            {
                Tag t = new Tag { TagName = list[i] };
                //lT.Add(t);
                //db.Tags.Add(t);
                addTag(t);
                db.SaveChanges();
            }
            for (int i = 0; i < list.Count; i++)
            {
                foreach (var t in db.Tags.ToList())
                {
                    if (t.TagName == list[i])
                    {
                        lT.Add(t);
                    }
                }
            }
            a.author = Author;
            a.journal = Journal;
            a.note = Note;
            a.publisher = Publisher;
            a.title = ArticleName;
            a.year = Year;
            a.Tags = lT;
            CreateBibFile(a);
            db.SaveChanges();
            return View("MainPage", db.Articles.ToList());
        }

        public ActionResult MainPage()
        {
            Group group = db.Groups.Find(1);
            CreateBibFile(group);
            return View(db.Articles.ToList());
        }

        [HttpPost]
        public ActionResult MainPage(string Search)
        {
            if(Search == "")
            {
                return View("MainPage", db.Articles.ToList());
            }
            list = db.SearchByTag(Search);
            //return RedirectToAction("SearchResult", list);
            return View("MainPage", list);
        }

        /*public ActionResult Search(string s)
        {
            if (s == "")
            {
                return View("MainPage", db.Articles.ToList());
            }
            list = db.SearchByTag(s);
            //return RedirectToAction("SearchResult", list);
            return View("MainPage", list);
        }*/

        public ActionResult GroupPage(int id)
        {
            Group g = db.Groups.Find(id);
            g.Articles = db.ArticleByGroup(id);
            g.Users = db.UsersByGroup(id);
            return View(g);
        }

       /* public ActionResult SearchResult()
        {
            return View(list);
        }*/

        public FileResult GetFile(int id)
        {
            Article a = db.Articles.Find(id);
            string filePath = "~/Files/" + a.Path;
            string fileType = "application/pdf";
            string fileName = a.Path;
            return File(filePath, fileType, fileName);
        }

        public FileResult GetBibFile(int id)
        {
            Article a = db.Articles.Find(id);
            string filePath = "~/BibFiles/" + a.title.Split('.') + ".bib";
            //string fileType = "BIB";
            string fileName = a.title.Split('.') + ".bib";

            return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        /*public FileResult GetBibFile1(string username)
        {
            string fileName = CreateBibFile(username);
            string filePath = "~/BibFiles/" + fileName;
            return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public FileResult GetBibFile(Group group)
        {
            string fileName = CreateBibFile(group);
            string filePath = "~/BibFiles/" + fileName;
            return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }*/

        public FileResult kek(int id)
        {
            Group group = db.Groups.Find(id);
            string name = group.GroupName;
            CreateBibFile(group);

            string fileName = group.GroupName + ".bib";
            string filePath = "~/BibFiles/" + fileName;
            return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);

        }

        public FileResult pls()
        {
            string name = User.Identity.Name;
            CreateBibFile(name);
            string fileName = name + ".bib";
            string filePath = "~/BibFiles/" + fileName;
            return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public string CreateBibFile(Article art)
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            string name = art.title + ".bib";
            string path = directory + "/BibFiles/" + name;
            System.IO.StreamWriter textFile = new System.IO.StreamWriter(@path);
            AddBib(textFile, art);
            textFile.Close();

            return name;
        }

        public string CreateBibFile(string username)
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            string name = username + ".bib";
            string path = directory + "/BibFiles/" + name;
            System.IO.StreamWriter textFile = new System.IO.StreamWriter(@path);
            foreach (var art in db.Articles.ToList())
            {
                if (art.UserName == username)
                {
                    AddBib(textFile, art);
                }
            }
            textFile.Close();
            return name;

        }

        public string CreateBibFile(Group group)
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            string name = group.GroupName + ".bib";
            string path = directory + "/BibFiles/" + name;
            System.IO.StreamWriter textFile = new System.IO.StreamWriter(@path);
            List<Article> list = db.ArticleByGroup(group.GroupId);
            foreach(var art in list)
            {
                AddBib(textFile, art);
            }
            textFile.Close();

            return name;
        }

        private void AddBib(System.IO.StreamWriter textFile, Article art)
        {
            if (art.Type == "Статья")
            {
                textFile.WriteLine("@ARTICLE{");
            }
            else if (art.Type == "Книга")
            {
                textFile.WriteLine("@BOOK{");
            }
            textFile.WriteLine("author = {" + art.author + "},");
            textFile.WriteLine("title = {«" + art.title + "»},");
            if (art.publisher != "")
            {
                textFile.WriteLine("publisher = {" + art.publisher + "},");
            }
          
            if (art.journal != "")
            {
                textFile.WriteLine("journal = {" + art.journal + "},");
            }
            if (art.year != "")
            {
                textFile.WriteLine("year = {" + art.year + "},");
            }
            textFile.WriteLine("note = {" + art.note + "},");
            textFile.WriteLine("}");

        }

        [HttpPost]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> fileUpload, string ArticleName, string TagList, 
            string Author, string Year, string Journal, string Publisher, string Note)
        {
            if (ArticleName == "") 
            { 
                ListsOfStuff list1 = new ListsOfStuff();
                return View(list1); 
            }
            if ((DateTime.Now.Year <= Convert.ToInt32(Year)) || (Convert.ToInt32(Year) <= 1500))
            {
                ListsOfStuff list1 = new ListsOfStuff();
                list1.ArticleName = ArticleName;
                list1.Author = Author;
                list1.Journal = Journal;
                list1.Note = Note;
                list1.Publisher = Publisher;
                list1.TagList = TagList;
                list1.wrongDate = true;
                return View(list1);
            }
            foreach (var file in fileUpload)
            {
                string filename = "";
                if (file != null)
                {
                    string allowFormat = "pdf";
                    var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                    if (fileExt != allowFormat)
                    {
                        ListsOfStuff list1 = new ListsOfStuff();
                        list1.ArticleName = ArticleName;
                        list1.Author = Author;
                        list1.Journal = Journal;
                        list1.Note = Note;
                        list1.Publisher = Publisher;
                        list1.TagList = TagList;
                        list1.Year = Year;
                        list1.wrongFile = true;
                        return View(list1);
                    }
                    //string path = AppDomain.CurrentDomain.BaseDirectory + "UploadedFiles/";
                    filename = System.IO.Path.GetFileName(file.FileName);
                    file.SaveAs(Server.MapPath("~/Files/" + filename));
                }
                List<string> list = StringToList(TagList);
                List<Tag> lT = new List<Tag>();
                for (int i = 0; i < list.Count; i++)
                {
                    Tag t = new Tag { TagName = list[i] };
                    //lT.Add(t);
                    //db.Tags.Add(t);
                    addTag(t);
                    db.SaveChanges();
                }
                for (int i = 0; i < list.Count; i++)
                {
                    foreach(var t in db.Tags.ToList())
                    {
                        if (t.TagName == list[i])
                        {
                            lT.Add(t);
                        }
                    }
                }
                
                string tt = Request.Form["TypeT"].ToString();
                int tg = Convert.ToInt32(Request.Form["GroupT"].ToString());
                
                Article a1 = new Article { title = ArticleName, Path = filename, Tags = lT, author = Author, Type = tt,  journal = Journal, note = Note, publisher = Publisher, year = Year, UserName = User.Identity.Name, GroupId = tg };
                db.Articles.Add(a1);
                CreateBibFile(a1);
                db.SaveChanges();
                UsersArticle ua = new UsersArticle { UserName = User.Identity.Name, ArticleId = a1.ArticleId};
                db.UsersArticles.Add(ua);
                db.SaveChanges();
            }

            return RedirectToAction("Finish");
        }

        public ActionResult ErrorOccured()
        {
            return View();
        }

        public ActionResult Finish()
        {
            return View();
        }

        public List<string> StringToList(string s)
        {
            List<string> list = new List<string>();
            int count = s.Length;
            string u = "";
            for (int i = 0; i < count; i++)
            {
                if (s[i] != ' ')
                {
                    u = u + s[i];
                }
                else
                {
                    list.Add(u);
                    u = "";
                }
            }
            list.Add(u);
            return list;
        }

        public void addTag(Tag tag)
        {
            bool add = true;
            foreach (var t in db.Tags.ToList())
            {
                if (tag.TagName == t.TagName)
                    add = false;
            }
            if (add)
            {
                db.Tags.Add(tag);
            }
        }

        
        public void WideIn(int Id)
        {
            db.AddUserInGroup(Id, User.Identity.Name);
        }

        [HttpPost]
        public ActionResult GroupPage(string Wade, int GroupId)
        {
            if (Wade == "-1")
            {
                UserInGroup u = new UserInGroup();
                foreach(var g in db.UserInGroups.ToList())
                {
                    if (g.UserName == User.Identity.Name && g.GroupId == GroupId)
                        u = g;
                }
                db.UserInGroups.Remove(u);
                db.SaveChanges();
            }
            else if (Wade == "-2")
            {
                db.AddUserInGroup(GroupId, User.Identity.Name);
                db.SaveChanges();
            }
            else
            {
                Article a = db.Articles.Find(Convert.ToInt32(Wade));
                a.GroupId = 0;
                db.SaveChanges();
            }
            Group m = db.Groups.Find(GroupId);
            m.Articles = db.ArticleByGroup(GroupId);
            m.Users = db.UsersByGroup(GroupId);
            return View("GroupPage", m);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
