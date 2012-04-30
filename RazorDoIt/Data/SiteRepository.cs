using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Database;
using System.Data.Entity.Infrastructure;
using System.Linq;
using RazorDoIt.Data.EfCodeFirst;
using RazorDoIt.Models;

namespace RazorDoIt.Data
{
    public interface ISiteRepository
    {
        Account GetAccount(string userName);
        Account SaveAccount(Account account);

        Template GetTemplate(int id);
        Template GetTemplate(string key);
        Template SaveTemplate(Template template);

        Tag GetTag(int id);
        Tag GetTag(string keyword);

        IEnumerable<Template> FindRecentTemplates();
        int SaveChanges();
    }

    public class SiteRepository : ISiteRepository, IDisposable
    {
        private SiteContext _db;
        private static bool _started;
        public static void Bootstrap()
        {
            if (_started)
                return;

            _started = true;

            var drop = bool.Parse(ConfigurationManager.AppSettings["DropDatabase"] ?? "false");
            if(drop)
                DbDatabase.SetInitializer(new DropCreateDatabaseIfModelChanges<SiteContext>());
            else
                DbDatabase.SetInitializer(new CreateDatabaseIfNotExists<SiteContext>());
        }

        public SiteRepository()
        {
            _db = new SiteContext();
            // RelationデータをProxyを作成せずに管理する。
            // だからIncludeが必須。
            // Attachはうまくいく!!
            ((IObjectContextAdapter)_db).ObjectContext.ContextOptions.ProxyCreationEnabled = false;

        }

        public Account GetAccount(string userName)
        {
            var query = from account in _db.Accounts
                        where account.UserName == userName
                        select account;

            return query.FirstOrDefault();
        }

        public Account SaveAccount(Account account)
        {
            var now = DateTime.Now;
            account.UpdateAt = now;

            var existAccount = GetAccount(account.UserName);
            if (existAccount != null)
            {
                account.CreateAt = existAccount.CreateAt;

                var entry = _db.Entry(existAccount);
                entry.OriginalValues.SetValues(existAccount);
                entry.CurrentValues.SetValues(account);
            }
            else
            {
                account.CreateAt = now;
                _db.Accounts.Add(account);
            }

            return account;
        }

        private IQueryable<Template> QueryTemplate()
        {
            return from template in _db.Templates.Include(m => m.Account).Include(m => m.Tags)
                   select template;
        }

        public Template GetTemplate(int id)
        {
            var query = from template in QueryTemplate()
                        where template.Id == id
                        select template;

            return query.FirstOrDefault();
        }

        public Template GetTemplate(string key)
        {
            var query = from template in QueryTemplate()
                        where template.UrlKey == key
                        select template;

            return query.FirstOrDefault();
        }

        public IEnumerable<Template> FindRecentTemplates()
        {
            var query = from template in QueryTemplate().AsNoTracking()
                        orderby template.UpdateAt descending
                        select template;

            return query.Take(20).ToList();
        }

        public Template SaveTemplate(Template template)
        {
            var now = DateTime.Now;
            template.UpdateAt = now;
            var existTemplate = GetTemplate(template.Id);
            if (existTemplate != null)
            {
                var entry = _db.Entry(existTemplate);
                entry.OriginalValues.SetValues(existTemplate);
                entry.CurrentValues.SetValues(template);

                // 今持ってるTagを一旦全部消す
                existTemplate.Tags.Clear();
            }
            else
            {
                template.CreateAt = now;
                _db.Templates.Add(template);
            }

            foreach (var tag in template.Tags)
            {
                tag.Templates.Add(template);
                _db.Tags.Add(tag);
            }

            return template;
        }

        public Tag GetTag(int id)
        {
            var query = from tag in _db.Tags
                        where tag.Id == id
                        select tag;

            return query.FirstOrDefault();
        }

        public Tag GetTag(string keyword)
        {
            var query = from tag in _db.Tags
                        where tag.Keyword == keyword
                        select tag;

            return query.FirstOrDefault();
        }

        public int SaveChanges()
        {
            return _db.SaveChanges();
        }

        #region disposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SiteRepository()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_db != null)
                {
                    if (_db.Database.Connection.State != ConnectionState.Closed)
                        _db.Database.Connection.Close();

                    _db.Dispose();
                    _db = null;
                }
            }
        }
        #endregion
    }
}
