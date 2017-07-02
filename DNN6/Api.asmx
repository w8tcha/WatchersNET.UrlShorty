<%@ WebService Language="C#" Class="Api" %>

using System;
using System.Web;
using System.Web.Services;
using WatchersNET.DNN.Modules.UrlShorty;
using WatchersNET.DNN.Modules.UrlShorty.Objects;

[WebService(Namespace = "http://www.my-dnn.de/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class Api : WebService
{

    [WebMethod]
    public Container CreateUrl(string sRealURL, string sUserId, string sPortalId)
    {
        
        string sUserName = "API";

        try
        {
            if (!string.IsNullOrEmpty(sUserId) && !string.IsNullOrEmpty(sPortalId))
            {
                DotNetNuke.Entities.Users.UserInfo userinfo = DotNetNuke.Entities.Users.UserController.GetUserById(
                    int.Parse(sPortalId), int.Parse(sUserId));

                if (userinfo != null)
                {
                    sUserName = userinfo.UserID.ToString();
                }
            }
        }
        catch (Exception)
        {
            sUserName = "API";
        }


        Container oShortUrl = new Container
                                  {
                                      RealUrl = sRealURL,
                                      ShortenedUrl = DataController.UniqueShortUrl(),
                                      CreateDate = DateTime.Now,
                                      CreatedBy = HttpContext.Current.Request.UserHostAddress,
                                      CreatedUser =  sUserName,
                                      Clicked = 0
                                  };

        oShortUrl.ShortenedUrl = DataController.AddUrlToDatabase(oShortUrl, null, false);

        oShortUrl.ShortenedUrl = Utils.PublicShortUrl(oShortUrl.ShortenedUrl, DotNetNuke.Common.Globals.GetDomainName(Context.Request, true));

        return oShortUrl;
    }

    [WebMethod]
    public Container GetUrl(string sShortUrl)
    {
        sShortUrl = Utils.InternalShortUrl(sShortUrl, DotNetNuke.Common.Globals.GetDomainName(Context.Request, true));
        return DataController.RetrieveUrlFromDatabase(sShortUrl);
    }
    
}

