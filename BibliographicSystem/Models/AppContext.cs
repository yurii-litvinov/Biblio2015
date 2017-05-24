using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace BibliographicSystem.Models
{
    public class bibliodb : DbContext
    {
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

        public void AddUserInGroup(int id, string name)
        {
            UserInGroups.Add(new UserInGroup {GroupId = id, UserName = name});
            SaveChanges();
        }

        private List<int> GroupIdByUser(string name) =>
            (from item in UserInGroups.ToList() where item.UserName == name select item.GroupId).ToList();

        public List<Group> GroupByUser(string name) =>
            Groups.ToList().Where(g => GroupIdByUser(name).Contains(g.GroupId)).ToList();

        public List<Article> ArticleByGroup(int id) =>
            Articles.ToList().Where(a => a.GroupId == id).ToList();

        public List<string> UsersByGroup(int id) =>
            (from user in UserInGroups.ToList() where user.GroupId == id select user.UserName).ToList();

        public List<Article> SearchByTag(string tagName) =>
            (from art in Articles.ToList() from t in art.Tags where tagName == t.TagName select art).ToList();

        public List<Group> SearchGroupList(string name) =>
            Groups.ToList().Where(g => name == g.GroupName).ToList();
    }
}