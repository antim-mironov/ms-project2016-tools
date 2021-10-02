using System;

namespace ProjectTools.Internal
{
    /// <summary>
    /// Represents SharePoint form digest value with expiration date.
    /// </summary>
    internal class FormDigest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormDigest"/> class.
        /// </summary>
        /// <param name="value">The digest value.</param>
        /// <param name="timeOutInSeconds">The digest timeout in seconds.</param>
        internal FormDigest(string value, int timeOutInSeconds)
        {
            this.DigestValue = value;
            this.ExpirationUtc = DateTime.UtcNow.AddSeconds(timeOutInSeconds);
        }

        /// <summary>
        /// Gets the form digest.
        /// </summary>
        internal string DigestValue { get; }

        /// <summary>
        /// Gets the digest expiration date and time (UTC).
        /// </summary>
        internal DateTime ExpirationUtc { get; }

        /// <summary>
        /// Gets a value indicating whether the form digest has expired.
        /// </summary>
        internal bool HasExpired
        {
            get
            {
                return DateTime.UtcNow >= this.ExpirationUtc;
            }
        }
    }
}
