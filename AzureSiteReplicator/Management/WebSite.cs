﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCLI;
using AzureSiteReplicator.Utils;
using Newtonsoft.Json.Linq;

namespace AzureSiteReplicator.Management
{
    public class WebSite
    {
        static WebSite()
        {
            HostSuffix = HostSuffix ?? ".azurewebsites.net";
        }

        private WebSite()
        {
        }

        public static string HostSuffix { get; set; }

        public string Name { get; set; }
        public string WebSpace { get; set; }
        public string State { get; set; }
        public bool Enabled { get; set; }
        public string[] HostNames { get; set; }
        public string SelfLink { get; set; }
        public string SiteMode { get; set; }
        public SiteProperties SiteProperties { get; set; }

        public static async Task<WebSite[]> GetAllAsync()
        {
            var tasks = new List<Task<WebSite[]>>();
            foreach (WebSpace webSpace in await AzureSiteReplicator.Management.WebSpace.GetAllAsync())
            {
                tasks.Add(WebSite.GetAllAsync(webSpace.Name));
            }

            var results = await Task.WhenAll<WebSite[]>(tasks.ToArray());
            var sites = new List<WebSite>();
            foreach (var result in results)
            {
                sites.AddRange(result);
            }

            return sites.ToArray(); 
        }

        public static async Task<WebSite[]> GetAllAsync(string webSpace)
        {
            string url = UriHelper.GetWebSitesUri(webSpace);
            return await RdfeHelper.GetAsAsync<WebSite[]>(url);
        }

        public static async Task<WebSite> GetAsync(string name, string webSpace)
        {
            string url = UriHelper.GetWebSiteUri(webSpace, name);
            return await RdfeHelper.GetAsAsync<WebSite>(url);
        }

        public static async Task CreateAsync(string name, string webSpace)
        {
            string url = UriHelper.GetWebSitesUri(webSpace, includesProperties: false);
            await RdfeHelper.PostAsync(url, new
            {
                Name = name,
                HostNames = new string[] { name + WebSite.HostSuffix }
            });
        }

        public static async Task DeleteAsync(string name, string webSpace)
        {
            string url = UriHelper.GetWebSiteUri(webSpace, name, includesProperties: false);
            await RdfeHelper.DeleteAsync(url);
        }

        public static async Task SyncWebSiteRepositoryAsync(string name, string webSpace)
        {
            string url = UriHelper.GetSyncWebSiteRepositoryUri(webSpace, name);
            await RdfeHelper.PostAsync(url, String.Empty);
        }

        public async Task SwapWebSiteSlots()
        {
            string url = UriHelper.GetSwapWebSiteSlotsUrl(WebSpace, Name);
            await RdfeHelper.PostAsync(url, String.Empty);
        }

        public async Task<string> GetPublishXml()
        {
            string url = UriHelper.GetWebSitePublishXmlUri(WebSpace, Name);
            return await RdfeHelper.GetAsAsync<string>(url);
        }

        public async Task<string[]> GetInstanceIds()
        {
            string url = UriHelper.GetWebSiteInstanceIdsUri(WebSpace, Name);
            return await RdfeHelper.GetAsAsync<string[]>(url);
        }

        public override string ToString()
        {
            return JObject.FromObject(this).ToString();
        }
    }
}
