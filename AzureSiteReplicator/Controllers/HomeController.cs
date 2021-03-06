﻿using AzureSiteReplicator.Contracts;
using AzureSiteReplicator.Data;
using AzureSiteReplicator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureSiteReplicator.Controllers
{
    public class HomeController : Controller
    {
        private ReplicationInfoModel _model;

        public ActionResult Index()
        {
            List<SiteStatusModel> statuses = new List<SiteStatusModel>();
            foreach (var site in Replicator.Instance.ConfigRepository.Sites)
            {
                statuses.Add(new SiteStatusModel(site.Status));
            }

            _model = new ReplicationInfoModel()
            {
                AllSites = Replicator.Instance.PublishXmlRepository.Sites,
                Checked = new List<bool>(Replicator.Instance.PublishXmlRepository.Sites.Count),
                SiteStatuses = statuses,
                SkipFiles = Replicator.Instance.ConfigRepository.Config.SkipRules
            };

            return View(_model);
        }

        [HttpGet]
        public JsonResult SiteStatuses()
        {
            List<SiteStatusModel> statuses = new List<SiteStatusModel>();
            foreach (var site in Replicator.Instance.PublishXmlRepository.Sites)
            {
                statuses.Add(new SiteStatusModel(site.Status));
            }

            return Json(statuses, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase unused)
        {
            //if (file.ContentLength > 0)
            //{
            //    try
            //    {
            //        PublishSettings settings = new PublishSettings(file.InputStream);
            //        string fileName = settings.SiteName + ".publishSettings";
            //        var path = Path.Combine(Environment.Instance.SiteReplicatorPath, fileName);
                    
            //        file.SaveAs(path);

            //        Replicator.Instance.ConfigRepository.AddSite(path);

            //        // Trigger a deployment since we just added a new target site
            //        Replicator.Instance.TriggerDeployment();
            //    }
            //    catch (IOException)
            //    {
            //        // todo: error handling
            //    }
            //}

            foreach (var file in Directory.EnumerateFiles(Environment.Instance.SiteReplicatorPath))
            {
                if (file.EndsWith(".publishSettings"))
                {
                    System.IO.File.Delete(file);
                }
            }

            foreach (string siteName in HttpContext.Request.Form.Keys)
            {
                var src = Replicator.Instance.PublishXmlRepository.Sites.First(s => s.Name == siteName).FilePath + ".publishSettings";
                var dst = Path.Combine(Environment.Instance.SiteReplicatorPath, Path.GetFileName(src));
                System.IO.File.Copy(src, dst);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Apply(string[] siteNames)
        {
            foreach (var name in siteNames)
            {
                Console.WriteLine(name);
            }
            for (int i = 0; i < _model.Checked.Count; i++)
            {
                if (_model.Checked[i])
                {
                    
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public FileResult LogFile(string siteName)
        {
            byte[] fileBytes = null;
            using (LogFile logFile = new LogFile(siteName, true))
            {
                // Not calling File.ReadAllBytes because it can cause a sharing violation if
                // the log file is still being written to.
                using (Stream fileStream = FileHelper.FileSystem.File.Open(
                    logFile.FilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    fileBytes = ReadStreamToBytes(fileStream);
                }

                return File(
                    fileBytes,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    "deploy.log");
            }
        }

        [HttpPost]
        public HttpResponseMessage SkipRules(IList<SkipRule> skipRules)
        {
            if (skipRules == null)
            {
                Replicator.Instance.ConfigRepository.Config.ClearSkips();
            }
            else
            {
                Replicator.Instance.ConfigRepository.Config.SetSkips(skipRules.ToList());
            }

            Replicator.Instance.ConfigRepository.Config.Save();
            Replicator.Instance.TriggerDeployment();

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete]
        public HttpResponseMessage Site(string name)
        {
            FindSiteOrThrow(name);
            Replicator.Instance.ConfigRepository.RemoveSite(name);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> SyncSite(string name)
        {
            Site site = FindSiteOrThrow(name);
            await Replicator.Instance.PublishContentToSingleSite(site);
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        public JsonResult TestSkipRules(List<SkipRule> skipRules)
        {
            List<string> pathsToBeSkipped = null;

            if (skipRules == null)
            {
                pathsToBeSkipped = new List<string>();
            }
            else
            {
                WebDeployHelper helper = new WebDeployHelper();
                pathsToBeSkipped = helper.TestSkipRule(
                    skipRules,
                    Environment.Instance.ContentPath);
            }

            return Json(pathsToBeSkipped);
        }

        private Site FindSiteOrThrow(string name)
        {
            Site site = Replicator.Instance.ConfigRepository.Sites.FirstOrDefault(s =>
            {
                return string.Equals(name, s.Name, StringComparison.OrdinalIgnoreCase);
            });

            if (site == null)
            {
                throw new HttpException(
                    (int)HttpStatusCode.NotFound,
                    string.Format("Could not find the site '{0}'", name));
            }

            return site;
        }

        private byte[] ReadStreamToBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}