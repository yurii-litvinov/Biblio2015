using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace BibliographicSystem.Models
{

    public class AppContext : DbContext
    {
        //public AppContext()
          //  : base("DefaultConnection")
        //{ }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserInGroup> UserInGroups { get; set; }
        public DbSet<UsersArticle> UsersArticles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Article>().HasMany(c => c.Tags)
                .WithMany(s => s.Articles)
                .Map(t => t.MapLeftKey("ArticleId")
                .MapRightKey("TagId")
                .ToTable("ArticleTag"));
        }

        public void AddUserInGroup (int id, string name)
        {
            UserInGroup u = new UserInGroup { GroupId = id, UserName = name };
            this.UserInGroups.Add(u);
            this.SaveChanges();
        }

        private List<int> GroupIdByUser (string name)
        {
            List<int> groups = new List<int>();
            foreach (var t in this.UserInGroups.ToList())
            {
                if (t.UserName == name)
                {
                    groups.Add(t.GroupId);
                }
            }
            return groups;
        }

        public List<Group> GroupByUser (string name)
        {
            List<int> ids = GroupIdByUser(name);
            List<Group> groups = new List<Group>();
            foreach (var g in this.Groups.ToList())
            {
                if (ids.Contains(g.GroupId))
                {
                    groups.Add(g);
                }
            }
            return groups;
        }

        public List<Article> ArticleByGroup (int id)
        {
            List<Article> list = new List<Article>();
            foreach (var a in this.Articles.ToList())
            {
                if (a.GroupId == id)
                {
                    list.Add(a);
                }
            }
            return list;
        }

        public List<string> UsersByGroup (int id)
        {
            List<string> list = new List<string>();
            foreach (var u in this.UserInGroups.ToList())
            {
                if (u.GroupId == id)
                {
                    list.Add(u.UserName);
                }
            }
            return list;
        }

        public List<Article> SearchByTag(string tagName)
        {
            int tNumber = 0; 
            Tag tag = new Tag();
            foreach (var t in this.Tags)
            {
                if (t.TagName == tagName)
                {
                    tNumber = t.TagId;
                    tag = t;
                }
            }
            Article a = new Article();
            List<Article> list = new List<Article>();
            foreach (var art in this.Articles.ToList())
            {
                /*if (art.Tags.Contains(tag))
                {
                    list.Add(art);
                }*/
                foreach (var t in art.Tags)
                {
                    if (tagName == t.TagName)
                    {
                        list.Add(art);
                    }
                }
            }

            return list;
        }

        public List<Group> SearchGroupList (string name)
        {
            List<Group> list = new List<Group>();
            foreach (var g in this.Groups.ToList())
            {
                if (name == g.GroupName)
                {
                    list.Add(g);
                }
            }
            return list;
        }

    }

   
}