/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty
{
    #region

    using System;
    using System.Net;
    using System.Threading;
    using System.Web.UI;

    using WatchersNET.DNN.Modules.UrlShorty.Objects;

    #endregion

    /// <summary>
    /// The redirect.
    /// </summary>
    public partial class Redirect : Page
    {
        #region Methods

        /// <summary>
        /// The page_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {
            /*if (Utils.HasValue(Request.QueryString["page"])) // checks in case ISAPIRewrite is being used
            {
                oShortUrl = DataController.RetrieveUrlFromDatabase(Request.QueryString["page"]);
            }
            else // using IIS Custom Errors
            {
                oShortUrl = DataController.RetrieveUrlFromDatabase(Request.Url.ToString().Substring(Request.Url.ToString().LastIndexOf("/") + 1));
            }*/

            // using IIS Custom Errors
            Container oShortUrl =
                DataController.RetrieveUrlFromDatabase(
                    this.Request.Url.ToString().Substring(this.Request.Url.ToString().LastIndexOf("/", StringComparison.Ordinal) + 1));

            if (oShortUrl != null && !string.IsNullOrEmpty(oShortUrl.RealUrl))
            {
                /*this.Response.StatusCode = (int)HttpStatusCode.MovedPermanently;
                this.Response.Redirect(oShortUrl.RealUrl);
                this.Response.End();*/

                this.Response.Clear();
                this.Response.Status = "301 Moved Permanently";
                this.Response.AddHeader("Location", oShortUrl.RealUrl);
                this.Response.End();
            }
            else
            {
                this.Response.StatusCode = (int)HttpStatusCode.NotFound;
                this.Response.Redirect(
                    string.Format(
                        "404.aspx?Language={0}&Page={1}", Thread.CurrentThread.CurrentCulture.Name, this.Request.Url));
            }
        }

        #endregion
    }
}