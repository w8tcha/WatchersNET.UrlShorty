<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="WatchersNET.DNN.Modules.UrlShorty.Settings" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="URL" Src="~/controls/URLControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>

<asp:panel id="pnlSettings" runat="server">
<dnn:sectionhead id="dshCommOpt" runat="server" cssclass="Head" includerule="True" isExpanded="True" resourcekey="lCommOpt" section="tblCommOpt" />
  <table id="tblCommOpt" runat="server" style="margin-left:20px; width:100%;">
    <tr>
      <td valign="top">
        <dnn:label id="lblHashReuse" runat="server"  ResourceKey="lblHashReuse" controlname="dDLHashReuse" suffix=":" CssClass="SubHead"></dnn:label>
      </td>
      <td>
        <asp:DropDownList id="dDLHashReuse" runat="server">
        </asp:DropDownList>
      </td>
    </tr>
    <tr>
      <td></td>
      <td><asp:CheckBox id="cBUseCustomizeUrl" runat="server" Text="Enable Customizing Urls?" Checked="true" /></td>
    </tr>
    <tr>
      <td></td>
      <td><asp:CheckBox id="UseAnalyticsUrl" runat="server" Text="Enable Google Analytics URL Builder?" /></td>
    </tr>
    <tr>
      <td></td>
      <td><asp:CheckBox id="cBUseErrorLog" runat="server" Text="Enable 404 Error Logging?" Checked="true" /></td>
    </tr>
    <tr>
      <td></td>
      <td><asp:CheckBox id="cBAllowUserEdit" runat="server" Text="Allow User to Edit/Delete Url?" /></td>
    </tr>
  </table>
</asp:panel>