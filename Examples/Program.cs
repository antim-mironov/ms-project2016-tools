using Microsoft.ProjectServer.Client;
using ProjectTools;
using System;

namespace Examples
{
    class Program
    {
        static void Main()
        {
            var pwaUrl = "https://my.server.dev/pwa";
            var projectId = Guid.Parse("12345678-ABCD-1234-ABCD-12345678ABCD");
            var siteUlr = "sites/SomeProjectSite";

            Example(pwaUrl, projectId, siteUlr);
            //ExampleWithProjectClientLibExtension(pwaUrl, projectId, siteUlr);
        }

        private static void Example(string pwaUrl, Guid projectId, string siteRelativeUrl)
        {
            var svc = new ProjectSiteConnector(pwaUrl);

            // In a case you want to provide specific credentials, use the overloaded constructor.
            //var svc = new ProjectSiteConnector(pwaUrl, new System.Net.NetworkCredential("domain\\userName", "password"));

            var result = svc.ConnectProject(projectId, siteRelativeUrl);
            Console.WriteLine(result);

            //Disconnect project from a site
            //var result12 = svc.DisconnectProject(projectId);
            //Console.WriteLine(result12);
        }

        private static void ExampleWithProjectClientLibExtension(string pwaUrl, Guid projectId, string siteRelativeUrl)
        {
            var projContext = new ProjectContext(pwaUrl);

            // Uncomment the following line in a case you need to provide credentials.
            //projContext.Credentials = new System.Net.NetworkCredential("domain\\userName", "password");

            projContext.Projects.ConnectProjectToSite(projectId, siteRelativeUrl);
        }
    }
}
