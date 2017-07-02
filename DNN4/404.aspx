<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="404.aspx.cs" Inherits="WatchersNET.DNN.Modules.UrlShorty.Dnn404" %>

<%
    Response.Status = "404 Not Found";
%>
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <title>Page Not Found</title>
   <script src="http://ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js" type="text/javascript"></script>
   <style type="text/css">
     #MainContainer {position: absolute; z-index: 8;width: 100%;height: 100%;top: 0px;left: 0px;background: black;
                     font-family: Verdana, Arial, Helvetica, sans-serif;font-size: smaller;color: black;text-align: justify}
     #MainContainer #ErrorFrame {position:relative;width: 100%;height: 100%;top: 0px;left: 0px;}
     #MainContainer #ErrorFrame #CenterDiv {position: absolute;width: 650px;height: 350px;top: 50%;left: 50%;background: white}
     #MainContainer #ErrorFrame #CenterDiv #Header { font-family: Impact, Verdana, Arial;font-size: 42px;font-weight: bold;color: #9eda29;position: absolute;left: 15px;top: 10px}
     #MainContainer #ErrorFrame #CenterDiv #Logo {position: absolute;right: 2px;top: 2px;opacity:0.2;filter:alpha(opacity=20)}
     #MainContainer #ErrorFrame #CenterDiv #Message { position: absolute;left: 25px;top: 85px;font-size: 1.2em;}
     #MainContainer #ErrorFrame #CenterDiv #Footer{font-weight: bold;position: absolute;left: 25px;}
   </style>
  </head>
  <body> 
    <script type="text/javascript">
    jQuery(document).ready(function () {
      jQuery("#MainContainer").width(jQuery(window).width()).height(jQuery(document).height());
      jQuery("#MainContainer #ErrorFrame").width(jQuery(window).width()).height(jQuery(window).height());
      jQuery("#MainContainer #ErrorFrame #CenterDiv").css({
            "margin-left": "-" + (jQuery("#MainContainer #ErrorFrame #CenterDiv").width() / 2) + "px",
            "margin-top": "-" + (jQuery("#MainContainer #ErrorFrame #CenterDiv").height() / 2) + "px"
        });
        jQuery("#MainContainer #ErrorFrame #CenterDiv #Message").css({
            "width": (jQuery("#MainContainer #ErrorFrame #CenterDiv").width() - 50) + "px",
        });
        jQuery("#MainContainer #ErrorFrame #CenterDiv #Footer").css({
            "width": (jQuery("#MainContainer #ErrorFrame #CenterDiv").width() - 50) + "px",
            "top": (jQuery("#MainContainer #ErrorFrame #CenterDiv #Message").height() + 55 + 30) + "px",
      });
    });
    </script>
    <form id="Form1" runat="server">
      <div id="MainContainer">
        <div id="ErrorFrame">
          <div id="CenterDiv">
            <div id="Header">
              <asp:Label id="lErrorHeader" runat="server" Text="ERROR"></asp:Label>
            </div>
            <div id="Logo">
              <asp:Image id="imagePortal" runat="server" />
            </div>
            <div id="Message">
              <asp:Label id="lErrorMessage" runat="server" Text="An error occured while processing your request."></asp:Label>
              <p><asp:Label id="lRequestUrl" runat="server" Font-Italic="true"></asp:Label></p>
              <p><strong><asp:Label id="lReasonInfo" runat="server" Text="Possible reasons:"></asp:Label></strong></p>
              <asp:Panel ID="panReasons" runat="server"></asp:Panel>
            </div>
            <div id="Footer">
              <asp:Panel id="panMainPage" runat="server"></asp:Panel>
              <p><asp:Label id="lApologize" runat="server" Text="We apologize for the problem"></asp:Label></p>
              <p><em><asp:Label ID="lSiteName" runat="server" Text=""></asp:Label></em></p>
            </div>
          </div>
        </div>
      </div>
    </form>
  </body>
</html>