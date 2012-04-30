using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Microsoft.Web.Mvc;

namespace RazorDoIt.Models
{
    public class Template
    {
        public int Id { get; set; }
        public string UrlKey { get; set; }
        [Required]
        public string Title { get; set; }
        [Required, AllowHtml, MaxLength(int.MaxValue)]
        public string Razor { get; set; }
        [MaxLength(32)]
        public string Language { get; set; }
        [DatabaseGenerated(DatabaseGenerationOption.None)]
        public int ParentId { get; set; }
        public Account Account { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }

        [NotMapped]
        public string Response { get; set; }
        [NotMapped]
        public string PageName { get; set; }
        [NotMapped]
        public string TagsValue { get; set; }
        [NotMapped]
        public IEnumerable<string> Languages { get; set; }

        public Template()
        {
            Tags = new List<Tag>();
            Languages = new List<string> {"cshtml", "vbhtml"};
        }

        public Template(Template original):this()
        {
            ModelCopier.CopyModel(original, this);
            //Id = original.Id;
            //ParentId = original.ParentId;
            Title = original.Title ?? "Razor do it!";
            //TagsValue = original.TagsValue;
            //Razor = original.Razor;
            //UrlKey = original.UrlKey;
            //CreateAt = original.CreateAt;
            //Account = original.Account;
        }
    }

    public class Tag
    {
        public int Id { get; set; }
        public string Keyword { get; set; }

        public virtual ICollection<Template> Templates { get; set; }

        public Tag()
        {
            Templates = new List<Template>();
        }
    }
}
