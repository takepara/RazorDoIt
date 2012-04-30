using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorDoIt.Models
{
    public class Account
    {
        [Key]
        public string UserName { get; set; }
        
        public OAuth OAuth { get; set; }
        public Twitter Twitter { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        
        public virtual ICollection<Template> Templates { get; set; }

        public Account()
        {
            OAuth = new OAuth();
            Twitter = new Twitter();
            Templates = new List<Template>();
        }
    }

    [ComplexType]
    public class OAuth
    {
        public string Token { get; set; }
        public string Verifier { get; set; }
        public string TokenSecret { get; set; }
    }

    [ComplexType]
    public class Twitter
    {
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}
