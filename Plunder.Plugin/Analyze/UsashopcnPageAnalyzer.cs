﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Plunder.Compoment;
using Plunder.Plugin.Compoment;
using Plunder.Analyze;
using HtmlAgilityPack;
using Site = Plunder.Compoment.Site;

namespace Plunder.Plugin.Analyze
{
    public class UsashopcnPageAnalyzer : IPageAnalyzer
    {
        public static string SiteId => SiteIndex.Usashopcn;
        public Site Site { get; }

        private readonly IEnumerable<FieldSelector> _fieldXPaths;

        public UsashopcnPageAnalyzer()
        {
            Site = SiteConfiguration.Instance.GetSite(SiteId);
            _fieldXPaths = new Dictionary<string, string> {
                { "Title","/html[1]/body[1]/div[2]/div[2]/div[1]/div[1]/div[2]/h2[1]"},
                { "Price","/html[1]/body[1]/div[2]/div[2]/div[1]/div[1]/div[2]/div[1]/div[2]/span[1]"},
                { "Description","/html[1]/body[1]/div[2]/div[2]/div[1]/div[2]/div[1]/div[2]"},
                { "PicUri","/html[1]/body[1]/div[2]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/img[1]/@src[1]"},
            }.Select(e => new FieldSelector() { FieldName = e.Key, Selector = e.Value }).ToList();
        }

        public PageResult Analyze(Request request, Response response)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(response.Content);
            var newRequests = FindNewRequest(doc, request, @"[\s\S]*", "/Product/Details/");//todo: regexPattern
            List<ResultField> resultFields = null;
            if (request.UrlType == UrlType.Extracting)
            {
                resultFields = XpathSelect(doc, _fieldXPaths);
                resultFields.Add(new ResultField() { Name = "Uri", Value = request.Url });
                resultFields.Add(new ResultField() { Name = "SiteName", Value = Site.Name });
                resultFields.Add(new ResultField() { Name = "SiteDomain", Value = Site.Domain });
                resultFields.Add(new ResultField() { Name = "ElapsedSecond", Value = response.MillisecondTime.ToString() });
                resultFields.Add(new ResultField() { Name = "Downloader", Value = response.Downloader });
                resultFields.Add(new ResultField() { Name = "CommentCount", Value = "0" });
            }

            var pageResult = new PageResult
            {
                Request = request,
                Response = response,
                NewRequests = newRequests,
                Channel = Channel.Product,
                Data = resultFields,
                Topic = Site.Topic
            };
            return pageResult;
        }

        private IEnumerable<Request> FindNewRequest(HtmlDocument doc, Request request, string newUrlPattern, string extractUrlPattern)
        {
            if (String.IsNullOrWhiteSpace(extractUrlPattern))
                return null;
            var newRegex = new Regex(newUrlPattern, RegexOptions.IgnoreCase);
            //var extractRegex = new Regex(extractUrlPattern, RegexOptions.IgnoreCase);
            var newRequests = new List<Request>();
            var dominRegex = new Regex(@"(?<=http://)[\w\.]+[^/]", RegexOptions.IgnoreCase); 

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                var href = link.GetAttributeValue("href", String.Empty).Trim();
                href = AbsoluteUrl(href);
                if (request.Url.Equals(href.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    continue;
                if (String.IsNullOrWhiteSpace(href) || !href.StartsWith("http"))
                    continue;
                if (!String.IsNullOrWhiteSpace(newUrlPattern) && !newRegex.IsMatch(href))
                    continue;
                if(href.Equals(request.Url))
                    continue;
                if(!dominRegex.Match(href).Value.Contains(Site.Domain))
                    continue;
                //var urlTye = extractRegex.IsMatch(href) ? UrlType.Extracting : UrlType.Navigation;
                var urlTye = href.IndexOf("/Product/Details/", StringComparison.CurrentCultureIgnoreCase) >= 0 ? UrlType.Extracting : UrlType.Navigation;
                newRequests.Add(new Request() { Url = href, UrlType = urlTye, SiteId = request.SiteId, HttpMethod = request.HttpMethod });
            }
            return newRequests;
        }

        private string AbsoluteUrl(string url)
        {
            if (url.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
                return url;
            return (new Uri(new Uri(String.Format("http://{0}/", Site.Domain)), url)).AbsoluteUri;
        }


        private List<ResultField> XpathSelect(HtmlDocument doc, IEnumerable<FieldSelector> selectors)
        {
            var fields = new List<ResultField>().ToList();
            foreach (var selector in selectors)
            {
                if(String.IsNullOrWhiteSpace(selector.Selector))
                    continue;
                var node = doc.DocumentNode.SelectSingleNode(selector.Selector);
                fields.Add(new ResultField { Name = selector.FieldName, Value = node == null ? null : node.InnerText.Trim() });
            }
            return fields;
        }
    }
}
