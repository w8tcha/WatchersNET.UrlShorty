/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty.Objects
{
    #region

    using System;

    #endregion

    /// <summary>
    /// The error log.
    /// </summary>
    public class ErrorLog
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLog"/> class. 
        ///   404 ErrorLog Item
        /// </summary>
        public ErrorLog()
        {
            this.ReqTime = DateTime.Now;
            this.RequestUrl = null;
            this.UserHostAddress = null;
            this.UserAgent = null;
            this.UrlReferrer = null;
            this.Browser = null;
            this.Platform = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Browser.
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// Gets or sets Platform.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets Request Time.
        /// </summary>
        public DateTime ReqTime { get; set; }

        /// <summary>
        /// Gets or sets RequestUrl.
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// Gets or sets UrlReferrer.
        /// </summary>
        public string UrlReferrer { get; set; }

        /// <summary>
        /// Gets or sets UserAgent.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets User Host Address.
        /// </summary>
        public string UserHostAddress { get; set; }

        #endregion
    }
}