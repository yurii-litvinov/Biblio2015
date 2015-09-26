using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BootApp.Models
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Theme { get; set; }
        public List<string> Users { get; set; }
        public List<Article> Articles { get; set; }
    }

    public class UserInGroup
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string UserName { get; set; }
    }
}