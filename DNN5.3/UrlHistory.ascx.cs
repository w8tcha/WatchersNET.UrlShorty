/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    using WatchersNET.DNN.Modules.UrlShorty.Objects;

    #endregion

    /// <summary>
    /// The url history.
    /// </summary>
    public partial class UrlHistory : ModuleSettingsBase
    {
        #region Constants and Fields

        /// <summary>
        /// The url list.
        /// </summary>
        private List<Container> urlList;

        #endregion

        #region Methods

        /// <summary>
        /// Close Page
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void ClosePage(object sender, EventArgs e)
        {
            this.Response.Redirect(Globals.NavigateURL(), true);
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
                string sDeleteUrl =
                    this.inpHide.Value.Substring(this.inpHide.Value.LastIndexOf("/", StringComparison.Ordinal) + 1);

                DataController.DeleteUrlFromDatabase(sDeleteUrl);

                this.LoadHistory();
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

            string sEditUrl =
                this.lblEditUrl.Text.Substring(this.lblEditUrl.Text.LastIndexOf("/", StringComparison.Ordinal) + 1);

            Container editContainer = DataController.RetrieveUrlFromDatabase(Utils.Clean(sEditUrl));

            editContainer.RealUrl = this.txtEditUrl.Text;
            editContainer.CreateDate = DateTime.Now;

            DataController.UpdateUrl(editContainer);

            this.panEditUrl.Visible = false;
            this.btnEditurl.Visible = true;

            this.lblEditUrl.Text = string.Empty;
            this.txtEditUrl.Text = string.Empty;

            this.inpHide.Value = string.Empty;

            this.LoadHistory();
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
                string sEditUrl =
                    this.inpHide.Value.Substring(this.inpHide.Value.LastIndexOf("/", StringComparison.Ordinal) + 1);

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
        /// Show Export Panel
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void ExportList(object sender, EventArgs e)
        {
            this.panExport.Visible = true;
        }

        /// <summary>
        /// Hide Export Panel
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void ExportListCancel(object sender, EventArgs e)
        {
            this.panExport.Visible = false;
        }

        /// <summary>
        /// Export Url History as List
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void ExportListNow(object sender, EventArgs e)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Container>));

            string sXmlFile = this.txtExportName.Text;

            if (string.IsNullOrEmpty(sXmlFile))
            {
                // If Input File Name is Empty
                sXmlFile = "FullUrlList.xml";
            }
            else if (!sXmlFile.EndsWith(".xml"))
            {
                // Make Sure File Extension is xml
                sXmlFile += ".xml";
            }

            string sFile = Path.Combine(
                this.PortalSettings.HomeDirectoryMapPath, this.cboFolders.SelectedValue + sXmlFile);

            TextWriter tr = new StreamWriter(sFile);
            serializer.Serialize(tr, this.urlList);

            tr.Close();

            this.panExport.Visible = false;

            this.RunjQueryCode(string.Format("alert('{0}- Exported');", sXmlFile));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            this.InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Get the jQuery code.
        /// </summary>
        /// <param name="jsCodetoRun">
        /// The JS code to run.
        /// </param>
        /// <returns>
        /// Returns the complete jQuery code block
        /// </returns>
        private static string GetjQueryCode(string jsCodetoRun)
        {
            var script = new StringBuilder();

            script.AppendLine("jQuery(document).ready(function() {");
            script.AppendLine(jsCodetoRun);
            script.AppendLine(" });");

            return script.ToString();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblHeaderUpdate.Text = Localization.GetString("lblHeaderUpdate.Text", this.LocalResourceFile);
            this.btnEditUrlNow.Text = Localization.GetString("btnEditUrlNow.Text", this.LocalResourceFile);
            this.btnEditUrlCancel.Text = Localization.GetString("btnEditUrlCancel.Text", this.LocalResourceFile);
            this.btnEditurl.Text = Localization.GetString("btnEditurl.Text", this.LocalResourceFile);
            this.btnDeleteUrl.Text = Localization.GetString("btnDeleteUrl.Text", this.LocalResourceFile);
            this.lnkClose.Text = Localization.GetString("btnClose.Text", this.LocalResourceFile);

            this.LoadHistory();

            this.RegisterJs();

            ((CDefault)this.Page).AddStyleSheet(
                "jQueryUi", "//ajax.googleapis.com/ajax/libs/jqueryui/1/themes/south-street/jquery-ui.css");
            
            ((CDefault)this.Page).AddStyleSheet("jQueryJqgrid", this.ResolveUrl("css/ui.jqgrid.css"));
        }

        /// <summary>
        /// Load the Full Url History
        /// </summary>
        private void LoadHistory()
        {
            this.urlList = DataController.RetrieveUrlList();

            foreach (Container urlContainer in this.urlList)
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

            sbHistoryGrd.Append("pager: jQuery('#pager'),");

            sbHistoryGrd.AppendFormat("colNames:[{0}],", Localization.GetString("Columns.Text", this.LocalResourceFile));

            sbHistoryGrd.Append("colModel:[");
            sbHistoryGrd.Append("{name:'Clicked',index:'Clicked', width:55, sorttype:'int', align:'center'},");
            sbHistoryGrd.Append("{name:'RealUrl', index:'RealUrl', sorttype:'link', width:500, sorttype:'text'},");
            sbHistoryGrd.Append(
                "{name:'ShortenedUrl', index:'ShortenedUrl',sorttype:'link', width:240, sorttype:'text'},");
            sbHistoryGrd.Append("{name:'Created', index:'Created',sorttype:'text', width:125},");
            sbHistoryGrd.Append("{name:'CreatedBy', index:'CreatedBy', align:'center', width:80, sorttype:'float'},");
            sbHistoryGrd.Append("{name:'CreatedUser', index:'CreatedUser', align:'center', width:90, sorttype:'text'},");
            sbHistoryGrd.Append("{name:'CreateDate', index:'CreateDate',sorttype:'int', hidden:true},");
            sbHistoryGrd.Append("],");

            sbHistoryGrd.AppendFormat("caption: '{0}',", Localization.GetString("Caption.Text", this.LocalResourceFile));
            sbHistoryGrd.AppendFormat(
                "emptyrecords: '{0}',", Localization.GetString("NoRecords.Text", this.LocalResourceFile));

            sbHistoryGrd.Append("});");

            // sbHistoryGrd.Append("jQuery('#grdUrlHistory').jqGrid('navGrid','#pager',{edit:false,add:false,del:false, refresh:false});");

            // Delete Helper
            sbHistoryGrd.AppendFormat("jQuery('#{0}').click( function() {{", this.btnDeleteUrl.ClientID);

            sbHistoryGrd.Append("var id; id = jQuery('#grdUrlHistory').jqGrid('getGridParam','selrow');");
            sbHistoryGrd.Append("if (id){");
            sbHistoryGrd.Append("var ret = jQuery('#grdUrlHistory').jqGrid('getRowData',id);");

            sbHistoryGrd.AppendFormat(
                " document.getElementById('{0}').value = ret.ShortenedUrl;", this.inpHide.ClientID);

            sbHistoryGrd.Append("} else { alert('Please select row');}");

            sbHistoryGrd.Append("});");

            // Edit Helper
            sbHistoryGrd.AppendFormat("jQuery('#{0}').click( function() {{", this.btnEditurl.ClientID);

            sbHistoryGrd.Append("var id; id = jQuery('#grdUrlHistory').jqGrid('getGridParam','selrow');");
            sbHistoryGrd.Append("if (id){");
            sbHistoryGrd.Append("var ret = jQuery('#grdUrlHistory').jqGrid('getRowData',id);");

            sbHistoryGrd.AppendFormat(
                " document.getElementById('{0}').value = ret.ShortenedUrl;", this.inpHide.ClientID);

            sbHistoryGrd.Append("} else { alert('Please select row');}");

            sbHistoryGrd.Append("});");

            /////////////
            sbHistoryGrd.Append("var myData = [");

            foreach (Container container in this.urlList)
            {
                TimeSpan ts = new TimeSpan(container.CreateDate.Ticks);
                long ticks = ts.Ticks;

                sbHistoryGrd.AppendFormat(
                    "{{Clicked:'{5}', RealUrl:'{0}',ShortenedUrl:'{1}',Created:'{2}',CreatedBy:'{3}',CreatedUser:'{4}',CreateDate:'{6}'}},",
                    Utils.CleanString(container.RealUrl.Trim()),
                    container.ShortenedUrl,
                    Utils.ReFormatDateTime(container.CreateDate),
                    container.CreatedBy,
                    container.CreatedUser,
                    container.Clicked,
                    ticks);
            }

            sbHistoryGrd.Append("];");
            sbHistoryGrd.Append("for(var i=0;i<=myData.length;i++)");
            sbHistoryGrd.Append("{grid.addRowData(i + 1, myData[i]);}");

            sbHistoryGrd.Append("grid.trigger('reloadGrid',[{page:1}]);");

            sbHistoryGrd.Append("});");

            ScriptManager.RegisterStartupScript(
                this, typeof(Page), Guid.NewGuid().ToString(), sbHistoryGrd.ToString(), true);
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

            string languageCode;
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;

                languageCode = currentCulture.TwoLetterISOLanguageName.ToLowerInvariant();
            }
            catch (Exception)
            {
                languageCode = "en";
            }

            ScriptManager.RegisterClientScriptInclude(
                this, csType, "jqGridLang", this.ResolveUrl(string.Format("js/i18n/grid.locale-{0}.js", languageCode)));

            ScriptManager.RegisterClientScriptInclude(
                this, csType, "jqGrid", this.ResolveUrl("js/jquery.jqGrid.min.js"));
        }

        /// <summary>
        /// Run the jQuery code.
        /// </summary>
        /// <param name="jsCodetoRun">The JS code to run.</param>
        private void RunjQueryCode(string jsCodetoRun)
        {
            ScriptManager requestSm = ScriptManager.GetCurrent(this.Page);

            if (requestSm != null && requestSm.IsInAsyncPostBack)
            {
                ScriptManager.RegisterClientScriptBlock(
                    this, typeof(Page), Guid.NewGuid().ToString(), GetjQueryCode(jsCodetoRun), true);
            }
            else
            {
                this.Page.ClientScript.RegisterClientScriptBlock(
                    typeof(Page), Guid.NewGuid().ToString(), GetjQueryCode(jsCodetoRun), true);
            }
        }

        #endregion
    }
}