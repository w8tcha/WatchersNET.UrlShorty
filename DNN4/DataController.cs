/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Data;

    using WatchersNET.DNN.Modules.UrlShorty.Objects;

    #endregion

    /// <summary>
    /// DataController to Handle all SQL Stuff for the Module
    /// </summary>
    public class DataController
    {
        #region Public Methods

        /// <summary>
        /// Add 404 Error as Log Entry to Database
        /// </summary>
        /// <param name="oErrorLog">
        /// The Error Entry
        /// </param>
        public static void AddErrorToDatabase(ErrorLog oErrorLog)
        {
            DataProvider.Instance().ExecuteNonQuery(
                "ShortyUrls_AddErrorLog", 
                oErrorLog.ReqTime, 
                oErrorLog.RequestUrl, 
                oErrorLog.UserHostAddress, 
                oErrorLog.UserAgent, 
                oErrorLog.UrlReferrer, 
                oErrorLog.Browser, 
                oErrorLog.Platform);
        }

        /// <summary>
        /// Add Url to Database
        /// </summary>
        /// <param name="oShortUrl">
        /// Url Container
        /// </param>
        /// <param name="sHashReuseMode">
        /// String How To Handle ReUse Hashes when Real url is already in DB
        /// </param>
        /// <param name="bCustom">
        /// If short Url must be a Custom Url
        /// </param>
        /// <returns>
        /// The add url to database.
        /// </returns>
        public static string AddUrlToDatabase(Container oShortUrl, string sHashReuseMode, bool bCustom)
        {
            string sOldShortUrl = null;
            string sCustomUrl = null;

            if (bCustom)
            {
                sCustomUrl = oShortUrl.ShortenedUrl;
            }

            using (
                IDataReader dr = DataProvider.Instance().ExecuteReader(
                    "ShortyUrls_GetRealUrl", Utils.Clean(oShortUrl.RealUrl)))
            {
                while (dr.Read())
                {
                    Container container = new Container
                        {
                            CreateDate = DateTime.Parse(Convert.ToString(dr["create_date"])), 
                            CreatedBy = Convert.ToString(dr["created_by"]), 
                            RealUrl = Convert.ToString(dr["real_url"]), 
                            ShortenedUrl = Convert.ToString(dr["short_url"]), 
                            CreatedUser = Convert.ToString(dr["created_user"]), 
                        };

                    sOldShortUrl = container.ShortenedUrl;
                }
            }

            if (!string.IsNullOrEmpty(sHashReuseMode) && !string.IsNullOrEmpty(sOldShortUrl))
            {
                switch (sHashReuseMode)
                {
                    case "user":
                        {
                            sOldShortUrl = null;

                            using (
                                IDataReader dr = DataProvider.Instance().ExecuteReader(
                                    "ShortyUrls_GetRealUrlByUser", Utils.Clean(oShortUrl.RealUrl), oShortUrl.CreatedUser))
                            {
                                while (dr.Read())
                                {
                                    Container container = new Container
                                        {
                                            CreateDate = DateTime.Parse(Convert.ToString(dr["create_date"])), 
                                            CreatedBy = Convert.ToString(dr["created_by"]), 
                                            RealUrl = Convert.ToString(dr["real_url"]), 
                                            ShortenedUrl = Convert.ToString(dr["short_url"]), 
                                            CreatedUser = Convert.ToString(dr["created_user"]), 
                                        };

                                    sOldShortUrl = container.ShortenedUrl;
                                }
                            }
                        }

                        break;
                    case "new":
                        {
                            sOldShortUrl = null;
                        }

                        break;
                    case "reuse":
                        {
                            oShortUrl.ShortenedUrl = sOldShortUrl;
                        }

                        break;
                }

                if (bCustom)
                {
                    if (sCustomUrl.Equals(oShortUrl.ShortenedUrl))
                    {
                        sOldShortUrl = oShortUrl.ShortenedUrl;
                    }
                    else
                    {
                        oShortUrl.ShortenedUrl = sCustomUrl;
                        sOldShortUrl = null;
                    }
                }
            }

            if (string.IsNullOrEmpty(sOldShortUrl) || bCustom)
            {
                DataProvider.Instance().ExecuteNonQuery(
                    "ShortyUrls_AddUrl", 
                    oShortUrl.ShortenedUrl, 
                    oShortUrl.CreateDate, 
                    oShortUrl.CreatedBy, 
                    Utils.Clean(oShortUrl.RealUrl), 
                    oShortUrl.CreatedUser, 
                    oShortUrl.Clicked);

                return bCustom ? sCustomUrl : oShortUrl.ShortenedUrl;
            }

            return sOldShortUrl;
        }

        /// <summary>
        /// Empty Error Table
        /// </summary>
        public static void DeleteAllErrorsFromDb()
        {
            DataProvider.Instance().ExecuteNonQuery("ShortyUrls_DeleteErrors");
        }

        /// <summary>
        /// Delete Short Url From Database
        /// </summary>
        /// <param name="internalURL">
        /// Short Url
        /// </param>
        public static void DeleteUrlFromDatabase(string internalURL)
        {
            DataProvider.Instance().ExecuteNonQuery("ShortyUrls_DeleteUrl", Utils.Clean(internalURL));
        }

        /// <summary>
        /// Get all Url Container from SQL
        /// </summary>
        /// <returns>
        /// Returns the Error List
        /// </returns>
        public static List<ErrorLog> RetrieveErrorList()
        {
            List<ErrorLog> errorLogList = new List<ErrorLog>();

            using (IDataReader dr = DataProvider.Instance().ExecuteReader("ShortyUrls_GetAllErrors"))
            {
                while (dr.Read())
                {
                    ErrorLog oErrorLog = new ErrorLog
                        {
                            ReqTime = DateTime.Parse(Convert.ToString(dr["ReqTime"])), 
                            RequestUrl = Convert.ToString(dr["RequestUrl"]), 
                            UserHostAddress = Convert.ToString(dr["UserHostAdress"]), 
                            UserAgent = Convert.ToString(dr["UserAgent"]), 
                            UrlReferrer = Convert.ToString(dr["UrlReferrer"]), 
                            Browser = Convert.ToString(dr["Browser"]), 
                            Platform = Convert.ToString(dr["Platform"])
                        };

                    errorLogList.Add(oErrorLog);
                }
            }

            return errorLogList;
        }

        /// <summary>
        /// Get Short Url From Database
        /// </summary>
        /// <param name="internalURL">
        /// Short Url
        /// </param>
        /// <returns>
        /// The Url Container from the Database
        /// </returns>
        public static Container RetrieveUrlFromDatabase(string internalURL)
        {
            Container oShortUrl = new Container { ShortenedUrl = internalURL };

            using (IDataReader dr = DataProvider.Instance().ExecuteReader("ShortyUrls_GetUrl", Utils.Clean(internalURL)))
            {
                while (dr.Read())
                {
                    int iClicked;
                    try
                    {
                        iClicked = Convert.ToInt32(dr["short_clicks"]);
                    }
                    catch (Exception)
                    {
                        iClicked = 0;
                    }

                    oShortUrl.CreateDate = DateTime.Parse(Convert.ToString(dr["create_date"]));
                    oShortUrl.CreatedBy = Convert.ToString(dr["created_by"]);
                    oShortUrl.CreatedUser = Convert.ToString(dr["created_user"]);
                    oShortUrl.RealUrl = Convert.ToString(dr["real_url"]);
                    oShortUrl.Clicked = iClicked;
                }
            }

            if (oShortUrl.RealUrl != null)
            {
                AddClickCount(oShortUrl);
            }

            return oShortUrl;
        }

        /// <summary>
        /// Get all Url Container from SQL
        /// </summary>
        /// <returns>
        /// Returns The Url List
        /// </returns>
        public static List<Container> RetrieveUrlList()
        {
            // Container oShortUrl = new Container { ShortenedUrl = internalURL };
            List<Container> urlList = new List<Container>();

            using (IDataReader dr = DataProvider.Instance().ExecuteReader("ShortyUrls_GetAllUrls"))
            {
                while (dr.Read())
                {
                    int iClicked;
                    try
                    {
                        iClicked = Convert.ToInt32(dr["short_clicks"]);
                    }
                    catch (Exception)
                    {
                        iClicked = 0;
                    }

                    Container oShortUrl = new Container
                        {
                            CreateDate = DateTime.Parse(Convert.ToString(dr["create_date"])), 
                            CreatedBy = Convert.ToString(dr["created_by"]), 
                            RealUrl = Convert.ToString(dr["real_url"]), 
                            ShortenedUrl = Convert.ToString(dr["short_url"]), 
                            CreatedUser = Convert.ToString(dr["created_user"]), 
                            Clicked = iClicked
                        };

                    urlList.Add(oShortUrl);
                }
            }

            return urlList;
        }

        /// <summary>
        /// Get all Url Container by UserID from SQL
        /// </summary>
        /// <param name="iUserId">
        /// The i User Id.
        /// </param>
        /// <returns>
        /// Returns the User Url List
        /// </returns>
        public static List<Container> RetrieveUserUrlList(int iUserId)
        {
            List<Container> urlList = new List<Container>();

            using (IDataReader dr = DataProvider.Instance().ExecuteReader("ShortyUrls_GetAllUserUrls", iUserId))
            {
                while (dr.Read())
                {
                    int iClicked;
                    try
                    {
                        iClicked = Convert.ToInt32(dr["short_clicks"]);
                    }
                    catch (Exception)
                    {
                        iClicked = 0;
                    }

                    Container oShortUrl = new Container
                        {
                            CreateDate = DateTime.Parse(Convert.ToString(dr["create_date"])), 
                            CreatedBy = Convert.ToString(dr["created_by"]), 
                            RealUrl = Convert.ToString(dr["real_url"]), 
                            ShortenedUrl = Convert.ToString(dr["short_url"]), 
                            CreatedUser = Convert.ToString(dr["created_user"]), 
                            Clicked = iClicked
                        };

                    urlList.Add(oShortUrl);
                }
            }

            return urlList;
        }

        /// <summary>
        /// Get Unique Short Url
        /// </summary>
        /// <returns>
        /// Unique ID
        /// </returns>
        public static string UniqueShortUrl()
        {
            string shortURL = Utils.RandomCharacters();

            int iUrlCount = 0;

            using (IDataReader dr = DataProvider.Instance().ExecuteReader("ShortyUrls_CountUrls", shortURL))
            {
                while (dr.Read())
                {
                    iUrlCount++;
                }
            }

            return iUrlCount.Equals(0) ? shortURL : Utils.RandomCharacters();
        }

        /// <summary>
        /// Updates an Existing Short Url
        /// </summary>
        /// <param name="oShortUrl">
        /// The Updated Url Container
        /// </param>
        public static void UpdateUrl(Container oShortUrl)
        {
            DataProvider.Instance().ExecuteReader(
                "ShortyUrls_UpdateUrl", 
                oShortUrl.ShortenedUrl, 
                oShortUrl.CreateDate, 
                oShortUrl.CreatedBy, 
                Utils.Clean(oShortUrl.RealUrl), 
                oShortUrl.CreatedUser, 
                oShortUrl.Clicked);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add click count.
        /// </summary>
        /// <param name="oShortUrl">
        /// The o short url.
        /// </param>
        private static void AddClickCount(Container oShortUrl)
        {
            DataProvider.Instance().ExecuteReader("ShortyUrls_AddClick", oShortUrl.ShortenedUrl, oShortUrl.Clicked + 1);
        }

        #endregion
    }
}