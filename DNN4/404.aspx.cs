/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty
{
    #region

    using System;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    using WatchersNET.DNN.Modules.UrlShorty.Objects;

    #endregion

    /// <summary>
    /// The DNN 404 Page.
    /// </summary>
    public partial class Dnn404 : Page
    {
        #region Constants and Fields

        /// <summary>
        /// The _portal settings.
        /// </summary>
        private readonly PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();

        #endregion

        #region Properties

        /// <summary>
        ///   Gets the Name for the Current Resource file name
        /// </summary>
        private string SResXFile
        {
            get
            {
                string[] page = this.Request.ServerVariables["SCRIPT_NAME"].Split('/');

                string fileRoot = string.Format(
                    "{0}/{1}/{2}.resx",
                    this.TemplateSourceDirectory,
                    Localization.LocalResourceDirectory,
                    page[page.GetUpperBound(0)]);

                return fileRoot;
            }
        }

        #endregion

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
            if (!string.IsNullOrEmpty(this.Request.QueryString["Page"]))
            {
                const string ErrorCode = "?404;";

                string sRequestedUrl = this.Request.QueryString["Page"];

                if (sRequestedUrl.Contains(ErrorCode))
                {
                    sRequestedUrl = sRequestedUrl.Substring(sRequestedUrl.IndexOf(ErrorCode) + ErrorCode.Length);
                }

                this.lRequestUrl.Text = sRequestedUrl;
            }

            this.imagePortal.AlternateText = "Portal Logo";
            if (!string.IsNullOrEmpty(this._portalSettings.LogoFile))
            {
                this.imagePortal.ImageUrl = this._portalSettings.HomeDirectory + this._portalSettings.LogoFile;
            }
            else
            {
                this.imagePortal.Visible = false;
            }

            this.SetLanguage();

            // Setting UseErrorLog Options
            string sPortalKey = string.Format("URLSHORTY#{0}#", this._portalSettings.PortalId);

            if (string.IsNullOrEmpty((string)this._portalSettings.HostSettings[sPortalKey + "UseErrorLog"]))
            {
                return;
            }

            bool result;

            if (
                !bool.TryParse(
                    (string)this._portalSettings.HostSettings[string.Format("{0}UseErrorLog", sPortalKey)], out result))
            {
                return;
            }

            if (result)
            {
                this.GenerateLogEntry();
            }
        }

        /// <summary>
        /// Create an Entry in the DB
        /// </summary>
        private void GenerateLogEntry()
        {
            string sReferer;
            try
            {
                sReferer = HttpContext.Current.Request.UrlReferrer.ToString();
            }
            catch (Exception)
            {
                sReferer = string.Empty;
            }

            ErrorLog logEntry = new ErrorLog
                {
                    ReqTime = DateTime.Now, 
                    RequestUrl = this.lRequestUrl.Text, 
                    UserHostAddress = HttpContext.Current.Request.UserHostAddress, 
                    UserAgent = HttpContext.Current.Request.UserAgent ?? string.Empty, 
                    UrlReferrer = sReferer, 
                    Browser =
                        string.Format(
                            "{0} {1}", 
                            HttpContext.Current.Request.Browser.Browser, 
                            HttpContext.Current.Request.Browser.Version), 
                    Platform = HttpContext.Current.Request.Browser.Platform
                };

            DataController.AddErrorToDatabase(logEntry);
        }

        /// <summary>
        /// Fill Labels with ML Text
        /// </summary>
        private void SetLanguage()
        {
            string language = null;

            if (!string.IsNullOrEmpty(this.Request.QueryString["language"]))
            {
                language = this.Request.QueryString["language"];
            }

            this.lErrorHeader.Text = Localization.GetString("lErrorHeader.Text", this.SResXFile, language);
            this.lErrorMessage.Text = Localization.GetString("lErrorMessage.Text", this.SResXFile, language);
            this.lReasonInfo.Text = Localization.GetString("lReasonInfo.Text", this.SResXFile, language);

            HtmlGenericControl ulReasons = new HtmlGenericControl("ul");

            HtmlGenericControl liReason1 = new HtmlGenericControl("li")
                {
                   InnerText = Localization.GetString("liReason1.Text", this.SResXFile, language) 
                };
            HtmlGenericControl liReason2 = new HtmlGenericControl("li")
                {
                   InnerText = Localization.GetString("liReason2.Text", this.SResXFile, language) 
                };
            HtmlGenericControl liReason3 = new HtmlGenericControl("li")
                {
                   InnerText = Localization.GetString("liReason3.Text", this.SResXFile, language) 
                };

            ulReasons.Controls.Add(liReason1);
            ulReasons.Controls.Add(liReason2);
            ulReasons.Controls.Add(liReason3);

            this.panReasons.Controls.Add(ulReasons);

            HtmlGenericControl spanHead = new HtmlGenericControl("span")
                {
                   InnerText = Localization.GetString("spanHead.Text", this.SResXFile, language) 
                };
            HtmlGenericControl spanFoot = new HtmlGenericControl("span")
                {
                   InnerText = Localization.GetString("spanFoot.Text", this.SResXFile, language) 
                };

            HtmlAnchor linkMainPage = new HtmlAnchor
                {
                    Title = Localization.GetString("linkMainPage.Text", this.SResXFile, language), 
                    InnerText = Localization.GetString("linkMainPage.Text", this.SResXFile, language), 
                    HRef = Globals.NavigateURL(this._portalSettings.HomeTabId)
                };

            this.panMainPage.Controls.Add(spanHead);
            this.panMainPage.Controls.Add(linkMainPage);
            this.panMainPage.Controls.Add(spanFoot);

            this.lApologize.Text = Localization.GetString("lApologize.Text", this.SResXFile, language);

            this.lSiteName.Text = this._portalSettings.PortalName;
        }

        #endregion
    }
}