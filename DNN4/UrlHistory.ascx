<%@ Control Language="c#" AutoEventWireup="True" Codebehind="UrlHistory.ascx.cs" Inherits="WatchersNET.DNN.Modules.UrlShorty.UrlHistory" %>

<div class="UrlHistory">
  <table id="grdUrlHistory" class="scroll"></table>
  <div id="pager" class="scroll" style="text-align:center;"></div> 
  <br />
  <input id="inpHide" type="hidden" value="" runat="server" />  
  <asp:Button id="btnEditurl" runat="server" OnClick="EditUrls" Text="Edit Selected URL" Style="padding:6px;margin-top:6px;font-size:100%;color:#fff;" />
  &nbsp;<asp:Button id="btnDeleteUrl" runat="server" OnClick="DeleteUrls" Text="Delete Selected URL" Style="padding:6px;font-size:100%;color:#fff;" />&nbsp;
  <asp:Button id="btnExportList" OnClick="ExportList" runat="server" Text="Export List" Style="padding:6px;font-size:100%;color:#fff;" />&nbsp;
  <asp:Button runat="server" id="lnkClose" cssclass="CommandButton" OnClick="ClosePage" Style="padding:6px;font-size:100%;color:#fff;" />

  <asp:Panel id="panEditUrl" runat="server" Visible="false">
    <hr />
    <h2><asp:Label id="lblHeaderUpdate" runat="server" Text="Update an Existing URL"></asp:Label></h2>
    <asp:Label ID="lblEditUrl" runat="server"></asp:Label> &nbsp;
    <asp:TextBox id="txtEditUrl" runat="server" Width="500"></asp:TextBox>
    <asp:Button id="btnEditUrlNow" runat="server" OnClick="EditUrlNow" CssClass="SmallButton" Text="Save Url" Style="padding:6px;margin-top:6px;font-size:100%;color:#fff;" />&nbsp;
  <asp:Button id="btnEditUrlCancel" runat="server" OnClick="EditUrlCancel" CssClass="SmallButton" Text="Cancel" Style="padding:6px;margin-top:6px;font-size:100%;color:#fff;" />
  </asp:Panel>
  
  <asp:Panel ID="panExport" runat="server" Visible="false">
    <hr />
    <p><asp:DropDownList id="cboFolders" Runat="server" CssClass="NormalTextBox" Width="300"></asp:DropDownList></p>
    <p><asp:TextBox id="txtExportName" runat="server" Text="FullUrlList.xml" Width="300"></asp:TextBox></p>
    <asp:Button id="btnExportNow" runat="server" OnClick="ExportListNow" Text="Export Now"></asp:Button> <asp:Button id="btnExportCancel" OnClick="ExportListCancel" runat="server" Text="Cancel"></asp:Button>
  </asp:Panel>  
</div>
