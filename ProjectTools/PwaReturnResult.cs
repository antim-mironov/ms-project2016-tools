using System;
using System.Linq;
using System.Xml.Linq;

namespace ProjectTools
{
    /// <summary>
    /// Represents the result of a request to connect a project to a site, or to disconnect a project from a site.
    /// </summary>
    public class PwaReturnResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PwaReturnResult"/> class.
        /// </summary>
        /// <param name="errorCode">Specifies the error code of the request result.</param>
        /// <param name="message">Specifies the message of the request result.</param>
        /// <param name="userInformation">Specifies any additional information of the request result.</param>
        internal PwaReturnResult(int errorCode, string message, string userInformation)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("The message cannot be empty!");
            }

            this.ErrorCode = errorCode;
            this.Message = message;
            this.UserInformation = userInformation;
        }

        /// <summary>
        /// Gets the error code of the result.
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Gets the message of the result.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets any additional user information of the result.
        /// </summary>
        public string UserInformation { get; }

        /// <summary>
        /// Gets a value indicating whether the result represents a successful request.
        /// </summary>
        public bool IsSuccessful
        {
            get { return !string.IsNullOrEmpty(this.Message) && this.Message.Equals("Success", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Overrides the default ToString method.
        /// </summary>
        /// <returns>String containing all property values of the request result.</returns>
        public override string ToString()
        {
            return $"ErrorCode: {this.ErrorCode} | Message: {this.Message} | UserInformation: {this.UserInformation}";
        }

        /// <summary>
        /// Parses the provided XML and creates a new instance of the <see cref="PwaReturnResult"/> class.
        /// </summary>
        /// <param name="xmlContent">The XML content shat shall be parsed.</param>
        /// <returns>Request result object.</returns>
        internal static PwaReturnResult Parse(string xmlContent)
        {
            XDocument xml = XDocument.Parse(xmlContent);
            int errorCode = 0;
            string message = string.Empty;
            string userInfo = string.Empty;

            var dataElements = xml.Descendants().Where(x => x.Name.LocalName == "DATA");
            foreach (var element in dataElements)
            {
                var elementValue = element.Value;
                var idAttrib = element.Attributes().FirstOrDefault(x => x.Name == "ID");
                if (idAttrib != null)
                {
                    if (idAttrib.Value == "idError")
                    {
                        errorCode = int.Parse(elementValue);
                    }
                    else if (idAttrib.Value == "idMessage")
                    {
                        message = elementValue;
                    }
                    else if (idAttrib.Value == "UserInformation")
                    {
                        userInfo = elementValue;
                    }
                }
            }

            return new PwaReturnResult(errorCode, message, userInfo);
        }
    }
}
