/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    using WatchersNET.DNN.Modules.UrlShorty.Objects;

    #endregion

    /// <summary>
    /// The error history.
    /// </summary>
    public partial class ErrorHistory : ModuleSettingsBase
    {
        #region Methods

        /// <summary>
        /// Close Page
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ClosePage(object sender, EventArgs e)
        {
            this.Response.Redirect(Globals.NavigateURL(), true);
        }

        /// <summary>
        /// Empty Error Log
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void EmptyLog(object sender, EventArgs e)
        {
            DataController.DeleteAllErrorsFromDb();

            this.LoadErrorLog();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            this.InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // Languages
            this.btnEmptyLog.Text = Localization.GetString("btnEmptyLog.Text", this.LocalResourceFile);
            this.btnClose.Text = Localization.GetString("btnClose.Text", this.LocalResourceFile);

            this.LoadErrorLog();

            this.RegisterJs();

            ((CDefault)this.Page).AddStyleSheet(
                "jQueryUi", "//ajax.googleapis.com/ajax/libs/jqueryui/1/themes/south-street/jquery-ui.css");

            ((CDefault)this.Page).AddStyleSheet("jQueryJqgrid", this.ResolveUrl("css/ui.jqgrid.css"));
        }

        /// <summary>
        /// Load the Full 404 Error Log
        /// </summary>
        private void LoadErrorLog()
        {
            List<ErrorLog> logList;

            try
            {
                logList = DataController.RetrieveErrorList();
            }
            catch (Exception)
            {
                logList = new List<ErrorLog>();
            }

            var historyGridScript = new StringBuilder();

            historyGridScript.Append("jQuery(document).ready(function(){");

            // Clear Grid Data if PostBack
            historyGridScript.Append("jQuery('#grdErrorLog').clearGridData();");

            historyGridScript.Append("var grid = jQuery('#grdErrorLog').jqGrid({");

            historyGridScript.Append("datatype: 'local',");

            historyGridScript.Append("autowidth : true,");
            historyGridScript.Append("height : '100%',");
            historyGridScript.Append("rowNum: 10,");
            historyGridScript.Append("rowList:[10,20,50,100],");
            historyGridScript.Append("sortname: 'ReqTime',");
            historyGridScript.Append("sortorder: 'desc',");
            historyGridScript.Append("viewrecords: true,");

            historyGridScript.Append("pager: jQuery('#pager'),");

            historyGridScript.AppendFormat("colNames:[{0}],", Localization.GetString("Columns.Text", this.LocalResourceFile));

            historyGridScript.Append("colModel:[");
            historyGridScript.Append("{name:'ReqTime',index:'ReqTime', sorttype:'int', align:'center',width:100 },");
            historyGridScript.Append("{name:'RequestUrl', index:'RequestUrl', sorttype:'link', sorttype:'text',width:400},");
            historyGridScript.Append(
                "{name:'UserHostAdress', index:'UserHostAdress',sorttype:'text', sorttype:'text',width:70},");
            historyGridScript.Append("{name:'UserAgent', index:'UserAgent',sorttype:'text',width:130 },");
            historyGridScript.Append("{name:'UrlReferrer', index:'UrlReferrer', align:'center', sorttype:'text',width:200},");
            historyGridScript.Append("{name:'Browser', index:'Browser', align:'center', sorttype:'text',width:70},");
            historyGridScript.Append("{name:'Platform', index:'Platform', align:'center', sorttype:'text',width:55},");
            historyGridScript.Append("],");

            historyGridScript.AppendFormat("caption: '{0}',", Localization.GetString("Caption.Text", this.LocalResourceFile));
            historyGridScript.AppendFormat("emptyrecords: '{0}',", Localization.GetString("NoRecords.Text", this.LocalResourceFile));

            historyGridScript.Append("});");

            historyGridScript.Append("var myData = [");

            foreach (ErrorLog errorLog in logList)
            {
                historyGridScript.AppendFormat(
                        "{{ReqTime:'{0}', RequestUrl:'{1}'\n,UserHostAdress:'{2}',UserAgent:'{3}',UrlReferrer:'{4}',Browser:'{5}',Platform:'{6}'}},", 
                        errorLog.ReqTime,
                        Utils.CleanString(errorLog.RequestUrl.Trim()),
                        errorLog.UserHostAddress,
                        errorLog.UserAgent,
                        Utils.CleanString(errorLog.UrlReferrer.Trim()), 
                        errorLog.Browser, 
                        errorLog.Platform);
            }

            historyGridScript.Append("];");
            historyGridScript.Append("for(var i=0;i<=myData.length;i++)");
            historyGridScript.Append("{grid.addRowData(i + 1, myData[i]);}");

            historyGridScript.Append("grid.trigger('reloadGrid',[{page:1}]);");

            historyGridScript.Append("});");

            ScriptManager.RegisterStartupScript(
                this, typeof(Page), Guid.NewGuid().ToString(), historyGridScript.ToString(), true);
        }

        /// <summary>
        /// Register the Java Script for the Grid.
        /// </summary>
        private void RegisterJs()
        {
            Type csType = typeof(Page);

            // Register jQuery
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

        #endregion
    }
}