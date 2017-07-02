<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ErrorHistory.ascx.cs" Inherits="WatchersNET.DNN.Modules.UrlShorty.ErrorHistory" %>

<div class="ErrorHistory">
  <table id="grdErrorLog" class="scroll"></table>
  <div id="pager" class="scroll" style="text-align:center;"></div> 
  <br />
  <asp:Button id="btnEmptyLog" OnClick="EmptyLog" runat="server" Text="Empty Log" Style="padding:6px;font-size:100%;color:#fff;" />&nbsp;
  <asp:Button runat="server" id="btnClose" cssclass="CommandButton" OnClick="ClosePage" Style="padding:6px;font-size:100%;color:#fff;" />
</div>