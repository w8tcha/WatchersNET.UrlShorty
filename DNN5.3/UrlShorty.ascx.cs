/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty
{
    #region

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;

    using WatchersNET.DNN.Modules.UrlShorty.Objects;

    #endregion

    /// <summary>
    /// The url shorty.
    /// </summary>
    public partial class UrlShorty : ModuleSettingsBase, IActionable
    {
        //// Settings
        #region Constants and Fields

        /// <summary>
        ///   Setting AllowUserEdit Options
        /// </summary>
        private bool bAllowUserEdit;

        /// <summary>
        ///   Use Customize Function
        /// </summary>
        private bool bUseCustomizeUrl = true;

        /// <summary>
        /// use Analytics Url
        /// </summary>
        private bool useAnalyticsUrl;

        /// <summary>
        ///   The HashReuse Mode
        /// </summary>
        private string sHashReuseMode;

        #endregion

        ////
        #region Properties

        /// <summary>
        ///   Gets the Add Url History and 404 Error Log Page to the Module Actions
        /// </summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection actions = new ModuleActionCollection
                    {
                        {
                            this.GetNextActionID(), "URL History", ModuleActionType.AddContent, string.Empty, string.Empty, 
                            this.EditUrl("History"), false, SecurityAccessLevel.Edit }, 
                        {
                            this.GetNextActionID(), "404 Error Log", ModuleActionType.AddContent, string.Empty, 
                            string.Empty, this.EditUrl("ErrorLog"), false, SecurityAccessLevel.Edit }
                    };

                return actions;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Show or Hide Customize Url Field
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void CustomizeShortUrl(object sender, EventArgs e)
        {
            if (this.txtCustomUrl.Visible)
            {
                this.lblDomainName2.Visible = false;
                this.txtCustomUrl.Visible = false;
                this.btnCustomize.Text = Localization.GetString("btnCustomize.Text", this.LocalResourceFile);
            }
            else
            {
                this.lblDomainName2.Text = string.Format("{0}/", this.lblDomainName.Text);

                this.lblDomainName2.Visible = true;
                this.txtCustomUrl.Visible = true;
                this.btnCustomize.Text = Localization.GetString("btnCancel.Text", this.LocalResourceFile);
            }
        }

        /// <summary>
        /// Delete Selected Url
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void DeleteUrls(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.inpHide.Value))
            {
                string sDeleteUrl = this.inpHide.Value.Substring(this.inpHide.Value.LastIndexOf("/", StringComparison.Ordinal) + 1);

                DataController.DeleteUrlFromDatabase(sDeleteUrl);

                this.LoadUserHistory();
            }

            this.inpHide.Value = string.Empty;
        }

        /// <summary>
        /// Cancel Edit
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void EditUrlCancel(object sender, EventArgs e)
        {
            this.panEditUrl.Visible = false;
            this.btnEditurl.Visible = true;

            this.lblEditUrl.Text = string.Empty;
            this.txtEditUrl.Text = string.Empty;

            this.inpHide.Value = string.Empty;
        }

        /// <summary>
        /// Save Edited Selected Url
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void EditUrlNow(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtEditUrl.Text) || !this.txtEditUrl.Text.StartsWith("http"))
            {
                return;
            }

            string sEditUrl = this.lblEditUrl.Text.Substring(this.lblEditUrl.Text.LastIndexOf("/", StringComparison.Ordinal) + 1);

            Container editContainer = DataController.RetrieveUrlFromDatabase(Utils.Clean(sEditUrl));

            editContainer.RealUrl = this.txtEditUrl.Text;
            editContainer.CreateDate = DateTime.Now;

            DataController.UpdateUrl(editContainer);

            this.panEditUrl.Visible = false;
            this.btnEditurl.Visible = true;

            this.lblEditUrl.Text = string.Empty;
            this.txtEditUrl.Text = string.Empty;

            this.inpHide.Value = string.Empty;

            this.LoadUserHistory();
        }

        /// <summary>
        /// Edit Selected Url
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void EditUrls(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.inpHide.Value))
            {
                string sEditUrl = this.inpHide.Value.Substring(this.inpHide.Value.LastIndexOf("/", StringComparison.Ordinal) + 1);

                Container editContainer = DataController.RetrieveUrlFromDatabase(Utils.Clean(sEditUrl));

                if (editContainer != null)
                {
                    this.panEditUrl.Visible = true;

                    this.btnEditurl.Visible = false;

                    this.lblEditUrl.Text = this.inpHide.Value;
                    this.txtEditUrl.Text = editContainer.RealUrl;
                }
            }

            this.inpHide.Value = string.Empty;
        }

        /// <summary>
        /// Generate ShortUrl
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void GenerateShortUrl(object sender, EventArgs e)
        {
            this.txtRealUrl.Text = this.txtRealUrl.Text.Trim();

            if (string.IsNullOrEmpty(this.txtRealUrl.Text) || !this.txtRealUrl.Text.StartsWith("http"))
            {
                return;
            }

            UserInfo userinfo = UserController.GetCurrentUserInfo();

            string sUserName = userinfo != null ? userinfo.UserID.ToString() : "Guest";

            bool bUseCustom = false;
            string sCustomUrl = null;

            // If Custom Url
            if (this.txtCustomUrl.Visible && !string.IsNullOrEmpty(this.txtCustomUrl.Text))
            {
                sCustomUrl = Utils.CleanCustomUrl(this.txtCustomUrl.Text);

                this.txtCustomUrl.Text = sCustomUrl;

                string sCustomUrlFull = string.Format("http://{0}{1}", this.lblDomainName2.Text, sCustomUrl);

                try
                {
                    bUseCustom = Utils.IsValidCustomUrl(sCustomUrlFull);
                }
                catch (Exception)
                {
                    bUseCustom = false;
                }

                if (!bUseCustom)
                {
                    this.lblDialogInfo.Text = Localization.GetString("lblDialogInfo.Text", this.LocalResourceFile);

                    // Message Error Already taken
                    ScriptManager.RegisterStartupScript(
                        this.Page, 
                        this.Page.GetType(), 
                        string.Format("DialogOpen{0}", Guid.NewGuid()), 
                        "jQuery('#ErrorDialog').dialog('open');", 
                        true);
                    return;
                }
            }

            Container oShortUrl = new Container
                {
                    RealUrl = this.txtRealUrl.Text, 
                    ShortenedUrl = DataController.UniqueShortUrl(), 
                    CreateDate = DateTime.Now, 
                    CreatedBy = HttpContext.Current.Request.UserHostAddress, 
                    CreatedUser = sUserName, 
                    Clicked = 0
                };

            // Now Replace Unique Short Url with Custom Url
            if (bUseCustom && !string.IsNullOrEmpty(sCustomUrl))
            {
                oShortUrl.ShortenedUrl = sCustomUrl;
            }

            oShortUrl.ShortenedUrl = DataController.AddUrlToDatabase(oShortUrl, this.sHashReuseMode, bUseCustom);

            // Show QR Code
            this.QrContainer.Visible = true;
            this.imgQrCode.Visible = true;
            this.imgQrCode.ImageUrl = string.Format(
                "{0}?data={1}", this.ResolveUrl("QrCode.ashx"), oShortUrl.ShortenedUrl);

            oShortUrl.ShortenedUrl = Utils.PublicShortUrl(
                oShortUrl.ShortenedUrl, Globals.GetDomainName(this.Request, true));

            this.lnkShortUrl.NavigateUrl = oShortUrl.ShortenedUrl;

            // Remove "http://www" or "http://"
            if (oShortUrl.ShortenedUrl.StartsWith("http://www"))
            {
                oShortUrl.ShortenedUrl = oShortUrl.ShortenedUrl.Replace("http://www.", string.Empty);
            }
            else if (oShortUrl.ShortenedUrl.StartsWith("http://"))
            {
                oShortUrl.ShortenedUrl = oShortUrl.ShortenedUrl.Replace("http://", string.Empty);
            }

            // Append Google Analytics URL
            if (this.useAnalyticsUrl)
            {
                oShortUrl.ShortenedUrl = this.GenerateAnalyticsUrl(oShortUrl.ShortenedUrl);
            }

            this.lnkShortUrl.Text = oShortUrl.ShortenedUrl;
            this.txtRealUrl.Text = oShortUrl.ShortenedUrl;

            this.lblLinkInfo.Visible = true;

            this.btnCopyClip.Visible = true;

            this.LoadUserHistory();

            this.lblDialogInfo.Text =
                string.Format(
                    Localization.GetString("lblDialogInfoCopy.Text", this.LocalResourceFile), this.txtRealUrl.Text);

            var zeroClipBoardScript = new StringBuilder();

            zeroClipBoardScript.AppendFormat(
                "ZeroClipboard.setMoviePath( '{0}' );var clip = new ZeroClipboard.Client(); clip.setText('{1}');clip.glue('{2}');",
                this.ResolveUrl("js/ZeroClipboard.swf"),
                this.txtRealUrl.Text,
                this.btnCopyClip.ClientID);

            zeroClipBoardScript.Append(
                "clip.show();clip.addEventListener( 'mouseDown', function(client) {{jQuery('#ErrorDialog').dialog('open') }});");

            ScriptManager.RegisterStartupScript(
                this.Page,
                this.Page.GetType(),
                string.Format("CopyToClipBoard{0}", Guid.NewGuid()),
                zeroClipBoardScript.ToString(),
                true);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            this.InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Generate Analytics Url
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// Returns the Complete URL
        /// </returns>
        private string GenerateAnalyticsUrl(string url)
        {
            var analyticsUrlQuery = HttpUtility.ParseQueryString(string.Empty);

            var sCampaignSourceText = Localization.GetString("CampaignSource.Text", this.LocalResourceFile);
            var sCampaignMediumText = Localization.GetString("CampaignMedium.Text", this.LocalResourceFile);
            var sCampaignTermText = Localization.GetString("CampaignTerm.Text", this.LocalResourceFile);
            var sCampaignContentText = Localization.GetString("CampaignContent.Text", this.LocalResourceFile);
            var sCampaignNameText = Localization.GetString("CampaignName.Text", this.LocalResourceFile);

            if (!string.IsNullOrEmpty(this.CampaignSource.Text))
            {
                if (!this.CampaignSource.Text.Equals(sCampaignSourceText))
                {
                    analyticsUrlQuery.Add("utm_source", Server.HtmlEncode(this.CampaignSource.Text));
                }
            }

            if (this.CampaignMedium.SelectedIndex.Equals(3))
            {
                if (!string.IsNullOrEmpty(this.CampaignMediumOther.Text))
                {
                    if (!this.CampaignMediumOther.Text.Equals(sCampaignMediumText))
                    {
                        analyticsUrlQuery.Add("utm_medium", Server.HtmlEncode(this.CampaignMediumOther.Text));
                    }
                }
            }
            else
            {
                analyticsUrlQuery.Add("utm_medium", Server.HtmlEncode(this.CampaignMedium.SelectedValue));
            }


            if (!string.IsNullOrEmpty(this.CampaignTerm.Text))
            {
                if (!this.CampaignTerm.Text.Equals(sCampaignTermText))
                {
                    analyticsUrlQuery.Add("utm_term", Server.HtmlEncode(this.CampaignTerm.Text));
                }
            }

            if (!string.IsNullOrEmpty(this.CampaignContent.Text))
            {
                if (!this.CampaignContent.Text.Equals(sCampaignContentText))
                {
                    analyticsUrlQuery.Add("utm_content", Server.HtmlEncode(this.CampaignContent.Text));
                }
            }

            if (!string.IsNullOrEmpty(this.CampaignName.Text))
            {
                if (!this.CampaignName.Text.Equals(sCampaignNameText))
                {
                    analyticsUrlQuery.Add("utm_campaign", Server.HtmlEncode(this.CampaignName.Text));
                }
            }

            return analyticsUrlQuery.Count > 0 ? string.Format("{0}?{1}", url, analyticsUrlQuery) : url;
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblDomainName.Text = Globals.GetDomainName(this.Request, true);

            var shortText = Localization.GetString("ShortText.Text", this.LocalResourceFile);

            this.lblHeaderUpdate.Text = Localization.GetString("lblHeaderUpdate.Text", this.LocalResourceFile);
            this.btnEditUrlNow.Text = Localization.GetString("btnEditUrlNow.Text", this.LocalResourceFile);
            this.btnEditUrlCancel.Text = Localization.GetString("btnEditUrlCancel.Text", this.LocalResourceFile);
            this.btnEditurl.Text = Localization.GetString("btnEditurl.Text", this.LocalResourceFile);
            this.btnDeleteUrl.Text = Localization.GetString("btnDeleteUrl.Text", this.LocalResourceFile);
            this.AnalyticsHeader.Text = Localization.GetString("AnalyticsHeader.Text", this.LocalResourceFile);
            this.AnalyticsDescription.Text = Localization.GetString("AnalyticsDescription.Text", this.LocalResourceFile);

            this.txtRealUrl.Attributes["onfocus"] = string.Format(
                "if (this.value == '{0}') {{this.value = '';}}", shortText);
            this.txtRealUrl.Attributes["onblur"] = string.Format(
                "if (this.value == '') {{this.value = '{0}';}}", shortText);
            this.txtRealUrl.Text = shortText;

            this.btnCopyClip.Attributes.Add("data-clipboard-target", this.txtRealUrl.ClientID);

            this.CampaignSourceLabel.Text = Localization.GetString("CampaignSourceLabel.Text", this.LocalResourceFile);
            this.CampaignMediumLabel.Text = Localization.GetString("CampaignMediumLabel.Text", this.LocalResourceFile);
            this.CampaignTermLabel.Text = Localization.GetString("CampaignTermLabel.Text", this.LocalResourceFile);
            this.CampaignContentLabel.Text = Localization.GetString("CampaignContentLabel.Text", this.LocalResourceFile);
            this.CampaignNameLabel.Text = Localization.GetString("CampaignNameLabel.Text", this.LocalResourceFile);

            var sCampaignSourceText = Localization.GetString("CampaignSource.Text", this.LocalResourceFile);
            var sCampaignMediumText = Localization.GetString("CampaignMedium.Text", this.LocalResourceFile);
            var sCampaignTermText = Localization.GetString("CampaignTerm.Text", this.LocalResourceFile);
            var sCampaignContentText = Localization.GetString("CampaignContent.Text", this.LocalResourceFile);
            var sCampaignNameText = Localization.GetString("CampaignName.Text", this.LocalResourceFile);

            this.CampaignSource.Attributes["onfocus"] = string.Format(
                "if (this.value == '{0}') {{this.value = '';}}", sCampaignSourceText);
            this.CampaignSource.Attributes["onblur"] = string.Format(
                "if (this.value == '') {{this.value = '{0}';}}", sCampaignSourceText);
            this.CampaignSource.Text = sCampaignSourceText;

            this.CampaignMedium.Items.Add(new ListItem("cpc", "cpc"));
            this.CampaignMedium.Items.Add(new ListItem("Banner", "banner"));
            this.CampaignMedium.Items.Add(new ListItem("Email", "email"));
            this.CampaignMedium.Items.Add(new ListItem(Localization.GetString("Other.Text", this.LocalResourceFile), "other"));

            this.CampaignMediumOther.Attributes["onfocus"] = string.Format(
                "if (this.value == '{0}') {{this.value = '';}}", sCampaignMediumText);
            this.CampaignMediumOther.Attributes["onblur"] = string.Format(
                "if (this.value == '') {{this.value = '{0}';}}", sCampaignMediumText);
            this.CampaignMediumOther.Text = sCampaignMediumText;

            this.CampaignTerm.Attributes["onfocus"] = string.Format(
                "if (this.value == '{0}') {{this.value = '';}}", sCampaignTermText);
            this.CampaignTerm.Attributes["onblur"] = string.Format(
                "if (this.value == '') {{this.value = '{0}';}}", sCampaignTermText);
            this.CampaignTerm.Text = sCampaignTermText;

            this.CampaignContent.Attributes["onfocus"] = string.Format(
                "if (this.value == '{0}') {{this.value = '';}}", sCampaignContentText);
            this.CampaignContent.Attributes["onblur"] = string.Format(
                "if (this.value == '') {{this.value = '{0}';}}", sCampaignContentText);
            this.CampaignContent.Text = sCampaignContentText;

            this.CampaignName.Attributes["onfocus"] = string.Format(
                "if (this.value == '{0}') {{this.value = '';}}", sCampaignNameText);
            this.CampaignName.Attributes["onblur"] = string.Format(
                "if (this.value == '') {{this.value = '{0}';}}", sCampaignNameText);
            this.CampaignName.Text = sCampaignNameText;

            // Language
            this.lblLinkInfo.Text = Localization.GetString("lblLinkInfo.Text", this.LocalResourceFile);
            this.btnSubmit.Text = Localization.GetString("btnSubmit.Text", this.LocalResourceFile);
            this.btnCustomize.Text = Localization.GetString("btnCustomize.Text", this.LocalResourceFile);

            this.LoadModulSettings();

            this.LoadUserHistory();

            this.RegisterJs();

            ((CDefault)this.Page).AddStyleSheet(
                "jQueryUi", "//ajax.googleapis.com/ajax/libs/jqueryui/1/themes/south-street/jquery-ui.css"); 
            
            ((CDefault)this.Page).AddStyleSheet("jQueryJqgrid", this.ResolveUrl("css/ui.jqgrid.css"));

            this.btnCustomize.Visible = this.bUseCustomizeUrl;

            this.AnalyticsPanel.Visible = this.useAnalyticsUrl;

            if (HttpContext.Current.Request.IsAuthenticated)
            {
                return;
            }

            this.btnCustomize.Visible = false;
            this.AnalyticsPanel.Visible = false;
        }

        /// <summary>
        /// Load the Setting for this Module
        /// </summary>
        private void LoadModulSettings()
        {
            ModuleController objModuleController = new ModuleController();

            Hashtable moduleSettings = objModuleController.GetTabModuleSettings(this.TabModuleId);

            // Setting HashReuse Options
            try
            {
                this.sHashReuseMode = (string)moduleSettings["HashReuseMode"];
            }
            catch (Exception)
            {
                this.sHashReuseMode = "user";
            }

            // Setting AllowUserEdit Options
            if (!string.IsNullOrEmpty((string)moduleSettings["AllowUserEdit"]))
            {
                bool bResult;
                if (bool.TryParse((string)moduleSettings["AllowUserEdit"], out bResult))
                {
                    this.bAllowUserEdit = bResult;
                }
            }

            // Setting UseCustomizeUrl Options
            if (!string.IsNullOrEmpty((string)moduleSettings["UseCustomizeUrl"]))
            {
                bool bResult;
                if (bool.TryParse((string)moduleSettings["UseCustomizeUrl"], out bResult))
                {
                    this.bUseCustomizeUrl = bResult;
                }
            }

            // Setting UseAnalyticsUrl Options
            if (!string.IsNullOrEmpty((string)moduleSettings["UseAnalyticsUrl"]))
            {
                bool bResult;
                if (bool.TryParse((string)moduleSettings["UseAnalyticsUrl"], out bResult))
                {
                    this.useAnalyticsUrl = bResult;
                }
            }
        }

        /// <summary>
        /// Load Url History for the Current User
        /// </summary>
        private void LoadUserHistory()
        {
            if (!HttpContext.Current.Request.IsAuthenticated)
            {
                return;
            }

            UserInfo userinfo = UserController.GetCurrentUserInfo();

            List<Container> urlList = DataController.RetrieveUserUrlList(userinfo.UserID);

            foreach (Container urlContainer in urlList)
            {
                urlContainer.ShortenedUrl = Utils.PublicShortUrl(
                    urlContainer.ShortenedUrl, Globals.GetDomainName(this.Request, true));

                // Remove "http://www" or "http://"
                if (urlContainer.ShortenedUrl.StartsWith("http://www"))
                {
                    urlContainer.ShortenedUrl = urlContainer.ShortenedUrl.Replace("http://www.", string.Empty);
                }
                else if (urlContainer.ShortenedUrl.StartsWith("http://"))
                {
                    urlContainer.ShortenedUrl = urlContainer.ShortenedUrl.Replace("http://", string.Empty);
                }

                UserController userController = new UserController();

                try
                {
                    urlContainer.CreatedUser =
                        userController.GetUser(this.PortalId, int.Parse(urlContainer.CreatedUser)).Username;
                }
                catch (Exception)
                {
                    urlContainer.CreatedUser = "Guest";
                }
            }

            StringBuilder sbHistoryGrd = new StringBuilder();

            sbHistoryGrd.Append("jQuery(document).ready(function(){");

            // Clear Grid Data if PostBack
            sbHistoryGrd.Append("jQuery('#grdUrlHistory').clearGridData();");

            sbHistoryGrd.Append("var grid = jQuery('#grdUrlHistory').jqGrid({");

            sbHistoryGrd.Append("datatype: 'local',");

            sbHistoryGrd.Append("autowidth : true,");
            sbHistoryGrd.Append("height : '100%',");
            sbHistoryGrd.Append("rowNum: 10,");
            sbHistoryGrd.Append("rowList:[10,20,50,100],");
            sbHistoryGrd.Append("sortname: 'CreateDate',");
            sbHistoryGrd.Append("sortorder: 'desc',");
            sbHistoryGrd.Append("viewrecords: true,");
            sbHistoryGrd.Append("editurl: '',");

            sbHistoryGrd.Append("pager: jQuery('#pager'),");

            sbHistoryGrd.AppendFormat("colNames:[{0}],", Localization.GetString("Columns.Text", this.LocalResourceFile));

            sbHistoryGrd.Append("colModel:[");
            sbHistoryGrd.Append(
                "{name:'Clicked',index:'Clicked', width:55, sorttype:'int', editable:false, align:'center'},");
            sbHistoryGrd.Append(
                "{name:'RealUrl', index:'RealUrl', sorttype:'link', width:500, editable:true, sorttype:'text'},");
            sbHistoryGrd.Append(
                "{name:'ShortenedUrl', index:'ShortenedUrl',sorttype:'link',editable:false, width:240, sorttype:'text'},");
            sbHistoryGrd.Append("{name:'Created', index:'Created',sorttype:'text', editable:false, width:125},");
            sbHistoryGrd.Append("{name:'CreateDate', index:'CreateDate',sorttype:'int', editable:false, hidden:true},");
            sbHistoryGrd.Append("],");

            sbHistoryGrd.AppendFormat("caption: '{0}',", Localization.GetString("Caption.Text", this.LocalResourceFile));
            sbHistoryGrd.AppendFormat("emptyrecords: '{0}',", Localization.GetString("NoRecords.Text", this.LocalResourceFile));

            sbHistoryGrd.Append("});");

            // Delete Entries
            if (this.bAllowUserEdit)
            {
                sbHistoryGrd.AppendFormat("jQuery('#{0}').click( function() {{", this.btnDeleteUrl.ClientID);

                sbHistoryGrd.Append("var id; id = jQuery('#grdUrlHistory').jqGrid('getGridParam','selrow');");
                sbHistoryGrd.Append("if (id){");
                sbHistoryGrd.Append("var ret = jQuery('#grdUrlHistory').jqGrid('getRowData',id);");

                sbHistoryGrd.AppendFormat(" document.getElementById('{0}').value = ret.ShortenedUrl;", this.inpHide.ClientID);

                sbHistoryGrd.Append("} else { alert('Please select row');}");

                sbHistoryGrd.Append("});");

                // Edit Helper
                sbHistoryGrd.AppendFormat("jQuery('#{0}').click( function() {{", this.btnEditurl.ClientID);

                sbHistoryGrd.Append("var id; id = jQuery('#grdUrlHistory').jqGrid('getGridParam','selrow');");
                sbHistoryGrd.Append("if (id){");
                sbHistoryGrd.Append("var ret = jQuery('#grdUrlHistory').jqGrid('getRowData',id);");

                sbHistoryGrd.AppendFormat(" document.getElementById('{0}').value = ret.ShortenedUrl;", this.inpHide.ClientID);

                sbHistoryGrd.Append("} else { alert('Please select row');}");

                sbHistoryGrd.Append("});");

                this.btnDeleteUrl.Visible = true;
                this.btnEditurl.Visible = true;
            }
            else
            {
                this.btnDeleteUrl.Visible = false;
                this.btnEditurl.Visible = false;
            }

            sbHistoryGrd.Append("var myData = [");

            foreach (Container container in urlList)
            {
                TimeSpan ts = new TimeSpan(container.CreateDate.Ticks);
                long ticks = ts.Ticks;

                sbHistoryGrd.AppendFormat(
                    "{{Clicked:'{0}', RealUrl:'{1}',ShortenedUrl:'{2}',Created:'{3}', CreateDate:'{4}'}},",
                    container.Clicked,
                    container.RealUrl,
                    container.ShortenedUrl,
                    Utils.ReFormatDateTime(container.CreateDate),
                    ticks);
            }

            sbHistoryGrd.Append("];");
            sbHistoryGrd.Append("for(var i=0;i<=myData.length;i++)");
            sbHistoryGrd.Append("{grid.addRowData(i + 1, myData[i]);}");

            sbHistoryGrd.Append("grid.trigger('reloadGrid',[{page:1}]);");

            sbHistoryGrd.Append("});");

            ScriptManager.RegisterStartupScript(
                this.Page, 
                this.Page.GetType(), 
                string.Format("UrlHistoryGrid{0}", Guid.NewGuid()), 
                sbHistoryGrd.ToString(), 
                true);
        }

        /// <summary>
        /// Register the Java Script for the Grid.
        /// </summary>
        private void RegisterJs()
        {
            Type csType = typeof(Page);

            // Register jQuery
            // jQuery.RequestRegistration();
            if (HttpContext.Current.Items["jquery_registered"] == null)
            {
                ScriptManager.RegisterClientScriptInclude(
                    this, csType, "jquery", "//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js");

                HttpContext.Current.Items.Add("jquery_registered", "true");
            }

            ScriptManager.RegisterClientScriptInclude(
               this, csType, "jQueryMigrate", "http://code.jquery.com/jquery-migrate-1.2.1.min.js");

            ScriptManager.RegisterClientScriptInclude(
                this, csType, "jQueryUi", "//ajax.googleapis.com/ajax/libs/jqueryui/1/jquery-ui.min.js");

            string sLanguageCode;
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;

                sLanguageCode = currentCulture.TwoLetterISOLanguageName.ToLowerInvariant();
            }
            catch (Exception)
            {
                sLanguageCode = "en";
            }

            ScriptManager.RegisterClientScriptInclude(
                this, csType, "jqGridLang", this.ResolveUrl(string.Format("js/i18n/grid.locale-{0}.js", sLanguageCode)));

            ScriptManager.RegisterClientScriptInclude(
                this, csType, "jqGrid", this.ResolveUrl("js/jquery.jqGrid.min.js"));

            ScriptManager.RegisterClientScriptInclude(
                this, csType, "jquery.zclip", this.ResolveUrl("js/jquery.zclip.min.js"));

            StringBuilder sbDialogJs = new StringBuilder();

            sbDialogJs.Append("jQuery('#ErrorDialog').dialog({");
            sbDialogJs.Append("autoOpen: false,");
            sbDialogJs.Append("resizable: false,");

            // sbDialogJs.Append(string.Format("title: '{0}',", Localization.GetString("ErrorTitle.Text", LocalResourceFile)));
            sbDialogJs.Append("title: 'Info',");
            sbDialogJs.Append("buttons: { \"OK\": function () { jQuery(this).dialog(\"close\");  } },");
            sbDialogJs.Append("open: function (type, data) {");
            sbDialogJs.Append("jQuery(this).parent().appendTo(\"form\");");
            sbDialogJs.Append("}");
            sbDialogJs.Append("});");

            ScriptManager.RegisterStartupScript(
                this.Page, 
                this.Page.GetType(), 
                string.Format("DialogJs{0}", Guid.NewGuid()), 
                sbDialogJs.ToString(), 
                true);
        }

        #endregion
    }
}