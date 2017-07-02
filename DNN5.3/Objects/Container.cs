/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty.Objects
{
    #region

    using System;

    #endregion

    /// <summary>
    /// Container for the ShortURL object
    /// </summary>
    public class Container
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class. 
        ///   Container for the ShortURL object
        /// </summary>
        public Container()
        {
            this.CreateDate = DateTime.Now;
            this.CreatedBy = "Unknown";
            this.CreatedUser = "-1";
            this.RealUrl = null;
            this.ShortenedUrl = "Unknown";
            this.Clicked = 0;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Clicked.
        /// </summary>
        public int Clicked { get; set; }

        /// <summary>
        /// Gets or sets CreateDate.
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets CreatedBy.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets CreatedUser.
        /// </summary>
        public string CreatedUser { get; set; }

        /// <summary>
        /// Gets or sets RealUrl.
        /// </summary>
        public string RealUrl { get; set; }

        /// <summary>
        /// Gets or sets ShortenedUrl.
        /// </summary>
        public string ShortenedUrl { get; set; }

        #endregion
    }
}