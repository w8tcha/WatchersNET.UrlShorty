<%@ Control Language="c#" AutoEventWireup="True" Codebehind="UrlShorty.ascx.cs" Inherits="WatchersNET.DNN.Modules.UrlShorty.UrlShorty" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="UrlShorty">
  <h1>WatchersNET.UrlShorty - <em>Short Your URL & Share</em></h1>
  <h2>Domain: <asp:Label ID="lblDomainName" runat="server" /></h2>
   <div>
    <p><asp:TextBox id="txtRealUrl" runat="server" TextMode="Multiline" Height="100px" Width="520px" /></p>
    <asp:Panel ID="AnalyticsPanel" CssClass="AnalyticsPanelDiv" Visible="false" runat="server">
      <h2><asp:Literal id="AnalyticsHeader" runat="server" Text="Google Analytics URL Builder"></asp:Literal></h2>
      <asp:Label id="AnalyticsDescription" runat="server" CssClass="BoldWhiteText"></asp:Label>
      <table cellspacing="2" cellpadding="2" class="AnalyticsTable">
       <tr>
         <td><asp:Literal id="CampaignNameLabel" runat="server" Text="Campaign Name:"></asp:Literal></td>
         <td><asp:TextBox ID="CampaignName" runat="server"></asp:TextBox></td>
       </tr>
       <tr>
         <td><asp:Literal id="CampaignSourceLabel" runat="server" Text="Campaign Source:"></asp:Literal></td>
         <td><asp:TextBox ID="CampaignSource" runat="server"></asp:TextBox></td>
       </tr>
       <tr>
         <td><asp:Literal id="CampaignMediumLabel" runat="server" Text="Campaign Medium:"></asp:Literal></td>
         <td><asp:DropDownList ID="CampaignMedium" runat="server"></asp:DropDownList><asp:TextBox ID="CampaignMediumOther" runat="server"></asp:TextBox></td>
       </tr>
       <tr>
         <td><asp:Literal id="CampaignTermLabel" runat="server" Text="Campaign Term:"></asp:Literal></td>
         <td><asp:TextBox ID="CampaignTerm" runat="server"></asp:TextBox></td>
       </tr>
       <tr>
         <td><asp:Literal id="CampaignContentLabel" runat="server" Text="Campaign Content:"></asp:Literal></td>
         <td><asp:TextBox ID="CampaignContent" runat="server"></asp:TextBox></td>
       </tr>
      </table>
    </asp:Panel>
    <asp:Label ID="lblDomainName2" runat="server" Visible="false" CssClass="BlackText" />&nbsp;<asp:TextBox id="txtCustomUrl" runat="server" Visible="false"></asp:TextBox>
    <asp:Button id="btnCustomize" runat="server" Text="Customize" OnClick="CustomizeShortUrl" CssClass="CustomizeButton" />
    <asp:Button id="btnSubmit" runat="server" Text="Create Short URL" OnClick="GenerateShortUrl" />
  </div>
  <div>
  <asp:Label ID="lblLinkInfo" runat="server" Text="Your new shortened URL is :" Visible="false" /> <asp:HyperLink CssClass="ShortLink" id="lnkShortUrl" runat="server" Font-Bold="true" />
  <asp:Button ID="btnCopyClip" runat="server" Text="Copy to Clipboard" Visible="false" Style="padding:5px;font-size:76%;color:#fff;" />
  </div>
</div>
<asp:PlaceHolder ID="QrContainer" runat="server" Visible="false">
  <div class="UrlShortyCodeContainer">
    <asp:Image ID="imgQrCode" runat="server" Visible="false" />
  </div>
  <br />
</asp:PlaceHolder>
<div class="UrlUserHistory">
  <input id="inpHide" type="hidden" value="" runat="server" />  
  <table id="grdUrlHistory" class="scroll"></table>
  <div id="pager" class="scroll" style="text-align:center;"></div> 
  <asp:Button id="btnEditurl" runat="server" OnClick="EditUrls" Text="Edit Selected URL" Style="padding:6px;margin-top:6px;font-size:100%;color:#fff;" Visible="false" />
  &nbsp;<asp:Button id="btnDeleteUrl" runat="server" OnClick="DeleteUrls" Text="Delete Selected URL" Style="padding:6px;margin-top:6px;font-size:100%;color:#fff;" Visible="false" />
</div>

<asp:Panel id="panEditUrl" runat="server" Visible="false">
  <hr />
  <h2><asp:Label id="lblHeaderUpdate" runat="server" Text="Update an Existing URL"></asp:Label></h2>
  <asp:Label ID="lblEditUrl" runat="server"></asp:Label>&nbsp;
  <asp:TextBox id="txtEditUrl" runat="server" Width="500"></asp:TextBox>
  <asp:Button id="btnEditUrlNow" runat="server" OnClick="EditUrlNow" CssClass="SmallButton" Text="Save Url" Style="padding:6px;margin-top:6px;font-size:100%;color:#fff;" />&nbsp;
  <asp:Button id="btnEditUrlCancel" runat="server" OnClick="EditUrlCancel" CssClass="SmallButton" Text="Cancel Edit" Style="padding:6px;margin-top:6px;font-size:100%;color:#fff;" />
  <br />
</asp:Panel>

<div id="ErrorDialog" style="display:none">
  <asp:Label ID="lblDialogInfo" runat="server"></asp:Label>
</div>