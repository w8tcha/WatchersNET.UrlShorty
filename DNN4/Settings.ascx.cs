/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty
{
    #region

    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.UserControls;

    #endregion

    /// <summary>
    /// The settings.
    /// </summary>
    public partial class Settings : ModuleSettingsBase
    {
        #region Constants and Fields

        /// <summary>
        /// The dsh comm opt.
        /// </summary>
        protected SectionHeadControl dshCommOpt;

        /// <summary>
        /// The lbl tag mode.
        /// </summary>
        protected LabelControl lblTagMode;

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the module settings
        /// </summary>
        public override void LoadSettings()
        {
            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.LoadModuleSettings();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Save the module settings
        /// </summary>
        public override void UpdateSettings()
        {
            try
            {
                this.SaveChanges();

                this.Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on init.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnInit(EventArgs e)
        {
            this.InitializeComponent();
            base.OnInit(e);
        }

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
            this.cBUseCustomizeUrl.Text = Localization.GetString("cBUseCustomizeUrl.Text", this.LocalResourceFile);
            this.UseAnalyticsUrl.Text = Localization.GetString("UseAnalyticsUrl.Text", this.LocalResourceFile);
            this.cBUseErrorLog.Text = Localization.GetString("cBUseErrorLog.Text", this.LocalResourceFile);
            this.cBAllowUserEdit.Text = Localization.GetString("cBAllowUserEdit.Text", this.LocalResourceFile);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dDLHashReuse.Items.Add(
                new ListItem(Localization.GetString("NewItem.Text", this.LocalResourceFile), "new"));
            this.dDLHashReuse.Items.Add(
                new ListItem(Localization.GetString("UserItem.Text", this.LocalResourceFile), "user"));
            this.dDLHashReuse.Items.Add(
                new ListItem(Localization.GetString("ReUseItem.Text", this.LocalResourceFile), "reuse"));

            this.dDLHashReuse.SelectedIndex = 1;
        }

        /// <summary>
        /// Loads the Settings
        /// </summary>
        private void LoadModuleSettings()
        {
            // Setting HashReuse Options
            if (!string.IsNullOrEmpty((string)this.TabModuleSettings["HashReuseMode"]))
            {
                try
                {
                    this.dDLHashReuse.SelectedValue = (string)this.TabModuleSettings["HashReuseMode"];
                }
                catch (Exception)
                {
                    this.dDLHashReuse.SelectedValue = "user";
                }
            }
            else
            {
                this.dDLHashReuse.SelectedValue = "user";
            }

            // Setting UseCustomizeUrl Options
            if (!string.IsNullOrEmpty((string)this.TabModuleSettings["UseCustomizeUrl"]))
            {
                bool bResult;
                if (bool.TryParse((string)this.TabModuleSettings["UseCustomizeUrl"], out bResult))
                {
                    this.cBUseCustomizeUrl.Checked = bResult;
                }
            }

            // Setting UseAnalyticsUrl Options
            if (!string.IsNullOrEmpty((string)this.TabModuleSettings["UseAnalyticsUrl"]))
            {
                bool bResult;
                if (bool.TryParse((string)this.TabModuleSettings["UseAnalyticsUrl"], out bResult))
                {
                    this.UseAnalyticsUrl.Checked = bResult;
                }
            }

            // Setting AllowUserEdit Options
            if (!string.IsNullOrEmpty((string)this.TabModuleSettings["AllowUserEdit"]))
            {
                bool bResult;
                if (bool.TryParse((string)this.TabModuleSettings["AllowUserEdit"], out bResult))
                {
                    this.cBAllowUserEdit.Checked = bResult;
                }
            }

            // Setting UseErrorLog Options
            string sPortalKey = string.Format("URLSHORTY#{0}#", this.PortalSettings.PortalId);

            if (!string.IsNullOrEmpty((string)this.PortalSettings.HostSettings[string.Format("{0}UseErrorLog", sPortalKey)]))
            {
                bool bResult;
                if (bool.TryParse(
                    (string)this.PortalSettings.HostSettings[string.Format("{0}UseErrorLog", sPortalKey)], out bResult))
                {
                    this.cBUseErrorLog.Checked = bResult;
                }
            }
        }

        /// <summary>
        /// Save the Settings to Database
        /// </summary>
        private void SaveChanges()
        {
            ModuleController objModules = new ModuleController();

            // Setting HashReuse Options
            objModules.UpdateTabModuleSetting(this.TabModuleId, "HashReuseMode", this.dDLHashReuse.SelectedValue);

            // Setting UseCustomizeUrl Options
            objModules.UpdateTabModuleSetting(
                this.TabModuleId, "UseCustomizeUrl", this.cBUseCustomizeUrl.Checked.ToString());

            // Setting UseAnalyticsUrl Options
            objModules.UpdateTabModuleSetting(
                this.TabModuleId, "UseAnalyticsUrl", this.UseAnalyticsUrl.Checked.ToString());

            // Setting AllowUserEdit Options
            objModules.UpdateTabModuleSetting(
                this.TabModuleId, "AllowUserEdit", this.cBAllowUserEdit.Checked.ToString());

            // Setting UseErrorLog Options
            string sPortalKey = string.Format("URLSHORTY#{0}#", this.PortalSettings.PortalId);

            HostSettingsController hostSettingsCtrl = new HostSettingsController();

            hostSettingsCtrl.UpdateHostSetting(
                string.Format("{0}UseErrorLog", sPortalKey), this.cBUseErrorLog.Checked.ToString());
        }

        #endregion
    }
}