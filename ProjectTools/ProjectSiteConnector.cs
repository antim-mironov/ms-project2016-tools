using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.SharePoint.Client;
using ProjectTools.Internal;

namespace ProjectTools
{
    /// <summary>
    /// Can be used to connect a project to a site collection and disconnect a project from a site collection.
    /// </summary>
    public class ProjectSiteConnector
    {
        private readonly ICredentials credential;
        private readonly Uri pwaUri;
        private readonly FormDigestRequestor formDigestSvc;
        private Guid rootSiteId = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectSiteConnector"/> class.
        /// </summary>
        /// <param name="pwaUrl">URL of the Project Web App.</param>
        public ProjectSiteConnector(string pwaUrl)
        {
            var url = pwaUrl.TrimEnd('/');
            this.pwaUri = new Uri(url);
            this.formDigestSvc = new FormDigestRequestor(pwaUrl);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectSiteConnector"/> class.
        /// </summary>
        /// <param name="pwaUrl">URL of the Project Web App.</param>
        /// <param name="credential">Credentials to be used for the requests.</param>
        public ProjectSiteConnector(string pwaUrl, ICredentials credential)
            : this(pwaUrl)
        {
            this.credential = credential;
            this.formDigestSvc = new FormDigestRequestor(pwaUrl, credential);
        }

        /// <summary>
        /// Connects a project to a site collection.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="relativeSiteUrl">Relative site collection URL.</param>
        /// <returns>The result from the request.</returns>
        public PwaReturnResult ConnectProject(Guid projectId, string relativeSiteUrl)
        {
            if (!Uri.TryCreate(relativeSiteUrl, UriKind.Relative, out _))
            {
                throw new ArgumentException("The provided site URL shall be relative!");
            }

            Guid idWSSServerUID = this.GetRootSiteId();
            var body = this.BuildRequestBody(projectId, relativeSiteUrl, idWSSServerUID);
            var result = this.SendPostRequest(body);

            return result;
        }

        /// <summary>
        /// Disconnects a project from its connected site collection.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>The result from the request.</returns>
        public PwaReturnResult DisconnectProject(Guid projectId)
        {
            var body = this.BuildRequestBody(projectId, string.Empty, Guid.Empty);
            var result = this.SendPostRequest(body);

            return result;
        }

        private string BuildRequestBody(Guid projectId, string relativeSiteUrl, Guid idWSSServerUID)
        {
            StringBuilder pwaCallBackArguments = new StringBuilder();
            pwaCallBackArguments.Append("<PWACALLBACK>");
            pwaCallBackArguments.Append("<INPUT NAME=\"idInformational\"/>");
            pwaCallBackArguments.Append("<INPUT NAME=\"idOperation\">EditWeb</INPUT>");
            pwaCallBackArguments.Append($"<INPUT NAME=\"idProjectUID\">{projectId}</INPUT>");

            if (string.IsNullOrEmpty(relativeSiteUrl))
            {
                pwaCallBackArguments.Append("<INPUT NAME=\"idProjectName\"/>");
            }
            else
            {
                pwaCallBackArguments.Append($"<INPUT NAME=\"idProjectName\">{relativeSiteUrl.Trim('/')}</INPUT>");
            }

            pwaCallBackArguments.Append($"<INPUT NAME=\"idWSSServerUID\">{idWSSServerUID}</INPUT>");
            pwaCallBackArguments.Append("<INPUT NAME=\"idWSSWebFullURL\"/>");
            pwaCallBackArguments.Append("<INPUT NAME=\"idNewMode\"/>");
            pwaCallBackArguments.Append("</PWACALLBACK>");

            var formDigest = this.formDigestSvc.GetFormDigest();

            StringBuilder body = new StringBuilder();
            body.Append("__REQUESTDIGEST=");
            body.Append(Uri.EscapeDataString(formDigest));
            body.Append("&PWAXMLData=");
            body.Append(Uri.EscapeDataString(pwaCallBackArguments.ToString()));

            return body.ToString();
        }

        private PwaReturnResult SendPostRequest(string requestContent)
        {
            var requestUri = new Uri(this.pwaUri.AbsoluteUri + "/_layouts/15/pwa/Admin/ManageWSS.aspx");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
            request.Method = "POST";
            request.UserAgent = "NONISV|SharePoint|Custom Tool";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("PJ_SERVER_CALLBACK", "1");

            if (this.credential == null)
            {
                request.UseDefaultCredentials = true;
            }
            else
            {
                request.UseDefaultCredentials = false;
                request.Credentials = this.credential;
            }

            StreamWriter sw = new StreamWriter(request.GetRequestStream());
            sw.Write(requestContent);
            sw.Flush();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            var responseContent = sr.ReadToEnd();

            return PwaReturnResult.Parse(responseContent);
        }

        private Guid GetRootSiteId()
        {
            if (this.rootSiteId != Guid.Empty)
            {
                return this.rootSiteId;
            }

            var rootSiteUri = new Uri(this.pwaUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped));

            using (ClientContext ctx = new ClientContext(rootSiteUri))
            {
                if (this.credential != null)
                {
                    ctx.Credentials = this.credential;
                }

                ctx.Load(ctx.Site, x => x.Id);
                ctx.ExecuteQuery();
                this.rootSiteId = ctx.Site.Id;
            }

            return this.rootSiteId;
        }
    }
}
