using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureSiteReplicator;

namespace AzureSiteReplicator.Management
{
    partial class Program
    {
        static void Main(string[] args)
        {
            try
            {
                /*
                // args[0] is publishing settings file.
                string profile = @"C:\Users\mitian\Downloads\Visual Studio Ultimate with MSDN-MSDN Dev_Test Pay-As-You-Go-5-8-2014-credentials.publishsettings"; // args[0];

                // this will deserialize and save as global variable 
                PublishSettings.LoadFrom(profile);
                var temp = PublishProfile.Current.Subscriptions[0];
                PublishProfile.Current.Subscriptions[0] = PublishProfile.Current.Subscriptions[1];
                PublishProfile.Current.Subscriptions[1] = temp;
                */

                PublishProfile profile = new PublishProfile();
                profile.SchemaVersion = "2.0";
                profile.PublishMethod = "AzureServiceManagementAPI";
                profile.Subscriptions = new PublishProfile.Subscription[]
                {
                    new PublishProfile.Subscription
                    {
                        Id = "b104f328-8cc0-4fb6-bade-cfe78a6d3ae6",
                        ManagementCertificate = "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",
                        ServiceManagementUrl = "https://management.core.windows.net",
                        Name="Visual Studio Ultimate with MSDN"
                    }
                };
                BasicTests();
                //KuduTests();
                //GitTests();
                //TfsTests();
                //GitHubTests();
                //BitbucketTests();
                //CodePlexTests();
                //DropboxTests();
                //ManagementTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}