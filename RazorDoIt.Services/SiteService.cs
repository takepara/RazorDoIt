using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
using RazorDoIt.Data;
using RazorDoIt.Models;
using TweetSharp;

namespace RazorDoIt.Services
{
    public static class FeedExtensions
    {
        public static IEnumerable<SyndicationItem> ToFeed(this IEnumerable<Template> templates, string baseUrl)
        {
            return from template in templates
                   select
                       new SyndicationItem(
                           template.Title, 
                           template.Razor,
                           new Uri(baseUrl + template.UrlKey), 
                           template.UrlKey,
                           new DateTimeOffset(template.UpdateAt.Value));
        }
    }

    public class SiteService
    {
        ISiteRepository _repository = new SiteRepository();

        public static string TwitterConsumerKey
        {
            get { return ConfigurationManager.AppSettings["twitterConsumerKey"]; }
        }

        public static string TwitterConsumerSecret
        {
            get
            {
                return ConfigurationManager.AppSettings["twitterConsumerSecret"];

            }
        }

        public Template GetTemplate(string urlKey)
        {
            var template = (string.IsNullOrEmpty(urlKey)
                            ? null
                            : _repository.GetTemplate(urlKey)) ?? new Template();
            template.TagsValue = template.Tags != null
                                 ? string.Join(" ", template.Tags.Select(t => t.Keyword.Trim())) 
                                 : "";
            return template;
        }

        public IEnumerable<Template> FindRecentTemplates()
        {
            return _repository.FindRecentTemplates();
        }

        public string SaveTemplate(string userName, Template template)
        {
            var account = GetAccount(userName);
            using (var ts = new TransactionScope())
            {
                var genLength = 4;
                while (string.IsNullOrEmpty(template.UrlKey))
                {
                    var key = KeyGenerator.Generate(genLength++);
                    if (GetTemplate(key).Id == 0)
                    {
                        template.UrlKey = key;
                    }
                }


                // Accountはログインで追加されてるはず。
                template.Account = account;

                // Templateはここで保存
                // ※なかでTagの保存もやってるよ。
                foreach (var tag in (template.TagsValue ?? "").Split(' ')
                                                              .Select(t => t.Trim())
                                                              .Distinct()
                                                              .Where(t => !string.IsNullOrEmpty(t)))
                {
                    template.Tags.Add(new Tag { Keyword = tag });
                }

                _repository.SaveTemplate(template);
                _repository.SaveChanges();

                ts.Complete();
            }

            return template.UrlKey;
        }

        public Account UpdateAccount(Models.OAuth oauth, Models.Twitter twitter)
        {
            var account = new Account
                              {
                                  UserName = twitter.ScreenName,
                                  OAuth = oauth,
                                  Twitter = twitter
                              };
            var result = _repository.SaveAccount(account);
            _repository.SaveChanges();

            return result;
        }

        public string GetTwitterAuthUri(string returnPath)
        {
            var twitter = new TwitterService(TwitterConsumerKey, TwitterConsumerSecret);
            var token = twitter.GetRequestToken(returnPath);
            var uri = twitter.GetAuthorizationUri(token);

            return uri.ToString();
        }

        public Account GetAccount(string userName)
        {
            var account = _repository.GetAccount(userName);

            return account;
        }

        // これ、ひどい。いいのかこんなコトして。
        // Twitter REST Apiのドキュメントには確かにHTTPとは書かれてるけど。
        // ライブラリがHTTPSにしてるのをバチコンとしても宜しいものなんだろうか．．．。
        private const string RestApiAuthority = "http://api.twitter.com";
        private static void ChangeApiAccessHttpsToHttp(TwitterService twitter)
        {
            var info = twitter.GetType().GetField("_oauth",
                                                           BindingFlags.Instance | BindingFlags.NonPublic);
            if (info != null)
            {
                var _oauth = info.GetValue(twitter) as Hammock.RestClient;
                if (_oauth != null)
                {
                    _oauth.Authority = RestApiAuthority;
                }
            }
        }
        public Account SignIn(string token, string verifier)
        {
            var requestToken = new OAuthRequestToken { Token = token };
            var twitter = new TwitterService(TwitterConsumerKey, TwitterConsumerSecret);
            ChangeApiAccessHttpsToHttp(twitter);

            var accessToken = twitter.GetAccessToken(requestToken, verifier);
            twitter.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);
            var user = twitter.VerifyCredentials();
            if (string.IsNullOrWhiteSpace(user.ScreenName))
                return null;

            // 保持しておくのはRequestTokenじゃなくてAccessTokenなんだぜ！
            // 一日潰れたぜ！
            var oauth = new OAuth
            {
                Token = accessToken.Token,
                TokenSecret = accessToken.TokenSecret,
                Verifier = verifier,
            };

            var twitterAccount = new Twitter
            {
                Name = user.Name,
                ScreenName = user.ScreenName,
                ProfileImageUrl = user.ProfileImageUrl
            };

            return UpdateAccount(oauth, twitterAccount);
        }
    }
}
