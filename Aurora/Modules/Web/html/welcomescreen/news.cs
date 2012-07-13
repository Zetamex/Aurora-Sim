﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aurora.Framework;
using Aurora.Framework.Servers.HttpServer;
using OpenMetaverse;

namespace Aurora.Modules.Web
{
    public class NewsWelcomeScreenPage : IWebInterfacePage
    {
        public string[] FilePath
        {
            get
            {
                return new[]
                       {
                           "html/welcomescreen/news.html",
                           "html/news_list.html"
                       };
            }
        }

        public bool RequiresAuthentication { get { return false; } }
        public bool RequiresAdminAuthentication { get { return false; } }

        public Dictionary<string, object> Fill(WebInterface webInterface, string filename, OSHttpRequest httpRequest,
            OSHttpResponse httpResponse, Dictionary<string, object> requestParameters, ITranslator translator)
        {
            IGenericsConnector connector = Aurora.DataManager.DataManager.RequestPlugin<IGenericsConnector>();
            var vars = new Dictionary<string, object>();

            vars.Add("News", translator.GetTranslatedString("News"));
            vars.Add("NewsDateText", translator.GetTranslatedString("NewsDateText"));
            vars.Add("NewsTitleText", translator.GetTranslatedString("NewsTitleText"));
            vars.Add("CurrentPageText", translator.GetTranslatedString("CurrentPageText"));

            uint amountPerQuery = 10;
            int start = httpRequest.Query.ContainsKey("Start") ? int.Parse(httpRequest.Query["Start"].ToString()) : 0;
            uint count = (uint)connector.GetGenericCount(UUID.Zero, "WebGridNews");
            int maxPages = (int)(count / amountPerQuery) - 1;

            if (start == -1)
                start = (int)(maxPages < 0 ? 0 : maxPages);

            vars.Add("CurrentPage", start);
            vars.Add("NextOne", start + 1 > maxPages ? start : start + 1);
            vars.Add("BackOne", start - 1 < 0 ? 0 : start - 1);

            var newsItems = connector.GetGenerics<GridNewsItem>(UUID.Zero, "WebGridNews");
            if (newsItems.Count == 0)
                newsItems.Add(GridNewsItem.NoNewsItem);
            vars.Add("NewsList", newsItems.ConvertAll<Dictionary<string, object>>(item => item.ToDictionary()));

            return vars;
        }
    }
}
