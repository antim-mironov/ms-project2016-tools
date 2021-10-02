using System;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using ProjectTools;

namespace Examples
{
    /// <summary>
    /// Shows an example how you could utilize the connector library through extension methods.
    /// </summary>
    public static class ProjectClientExtensions
    {
        public static PwaReturnResult ConnectProjectToSite(this ProjectCollection projectCollection, Guid projectId, string relativeSiteUrl)
        {
            return ConnectProjectToSite(projectCollection.Context, projectId, relativeSiteUrl);
        }

        public static PwaReturnResult ConnectProjectToSite(this PublishedProject project, string relativeSiteUrl)
        {
            return ConnectProjectToSite(project.Context, project.Id, relativeSiteUrl);
        }

        public static PwaReturnResult ConnectProjectToSite(this DraftProject project, string relativeSiteUrl)
        {
            return ConnectProjectToSite(project.Context, project.Id, relativeSiteUrl);
        }

        private static PwaReturnResult ConnectProjectToSite(ClientRuntimeContext ctx, Guid projectId, string relativeSiteUrl)
        {
            var svc = new ProjectSiteConnector(ctx.Url, ctx.Credentials);
            return svc.ConnectProject(projectId, relativeSiteUrl);
        }
    }
}
