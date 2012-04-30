using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace RazorDoIt
{
    public enum FeedFormat
    {
        Atom,
        Rss
    }

    public class FeedResult : ActionResult
    {
        private Dictionary<FeedFormat, string> _feedContentType = new Dictionary<FeedFormat, string>()
        {
            {FeedFormat.Atom, "application/atom+xml"},
            {FeedFormat.Rss, "application/rss+xml"}
        };

        public SyndicationFeed Feed { get; set; }
        public FeedFormat Format { get; set; }
        public FeedResult() : this(new SyndicationFeed(), FeedFormat.Atom) { }
        public FeedResult(SyndicationFeed feed, FeedFormat format)
        {
            Feed = feed;
            Format = format;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = _feedContentType[Format];

            SyndicationFeedFormatter formatter = null;
            if (Format == FeedFormat.Atom)
                formatter = new Atom10FeedFormatter(Feed);
            else 
                formatter = new Rss20FeedFormatter(Feed);
            using (var writer = XmlWriter.Create(context.HttpContext.Response.Output))
            {
                formatter.WriteTo(writer);
            }
        }
    }
}
