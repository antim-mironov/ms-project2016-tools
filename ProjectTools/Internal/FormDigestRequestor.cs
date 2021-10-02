using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace ProjectTools.Internal
{
    /// <summary>
    /// Can be used to request a new form digest from SharePoint. The digest can be sued to make further requests against SharePoint.
    /// </summary>
    internal class FormDigestRequestor
    {
        private const string FormDigestUrl = "/_api/contextinfo";
        private readonly Uri digestFormUri;
        private readonly ICredentials credential;
        private FormDigest formDigest;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDigestRequestor"/> class.
        /// </summary>
        /// <param name="pwaUrl">URL of the Project Web App.</param>
        internal FormDigestRequestor(string pwaUrl)
        {
            var url = pwaUrl.TrimEnd('/');
            this.digestFormUri = new Uri(url + FormDigestUrl);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDigestRequestor"/> class.
        /// </summary>
        /// <param name="pwaUrl">URL of the Project Web App.</param>
        /// <param name="credential">Credentials to be used for the request of the form digest.</param>
        internal FormDigestRequestor(string pwaUrl, ICredentials credential)
            : this(pwaUrl)
        {
            this.credential = credential;
        }

        /// <summary>
        /// Requests a new form digest from SharePoint.
        /// The retrieved digest will be stored in a internal variable. If this digest is still values it will be used and no new request will be made.
        /// </summary>
        /// <returns>The form digest.</returns>
        internal string GetFormDigest()
        {
            if (this.formDigest != null && !this.formDigest.HasExpired)
            {
                return this.formDigest.DigestValue;
            }

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(this.digestFormUri);
            request.Method = "POST";
            request.UserAgent = "NONISV|SharePoint|Custom Tool";
            request.ContentLength = 0;

            if (this.credential == null)
            {
                request.UseDefaultCredentials = true;
            }
            else
            {
                request.UseDefaultCredentials = false;
                request.Credentials = this.credential;
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            XDocument xml = XDocument.Load(response.GetResponseStream());
            var formDigestElement = xml.Descendants().Where(x => x.Name.LocalName == "FormDigestValue").FirstOrDefault();

            if (formDigestElement == null)
            {
                throw new InvalidOperationException("No form digest value was retrieved!");
            }

            int formDigestTimeOut = 0;
            var formDigestTimeOutElement = xml.Descendants().Where(x => x.Name.LocalName == "FormDigestTimeoutSeconds").FirstOrDefault();
            if (formDigestTimeOutElement != null)
            {
                formDigestTimeOut = int.Parse(formDigestTimeOutElement.Value);
            }

            this.formDigest = new FormDigest(formDigestElement.Value, formDigestTimeOut);

            return formDigestElement.Value;
        }
    }
}
