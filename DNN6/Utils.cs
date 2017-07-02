/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty
{
    #region

    using System;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Web;

    #endregion

    /// <summary>
    /// Range of utility methods
    /// </summary>
    public static class Utils
    {
        #region Constants and Fields

        /// <summary>
        /// The shorturl chars lcase.
        /// </summary>
        private const string ShorturlCharsLcase = "abcdefgijkmnopqrstwxyz";

        /// <summary>
        /// The shorturl chars numeric.
        /// </summary>
        private const string ShorturlCharsNumeric = "23456789";

        /// <summary>
        /// The shorturl chars ucase.
        /// </summary>
        private const string ShorturlCharsUcase = "ABCDEFGHJKLMNPQRSTWXYZ";

        #endregion

        #region Public Methods

        /// <summary>
        /// Clean Url.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// Returns the Cleaned Url
        /// </returns>
        public static string Clean(string url)
        {
            const string Filter = @"((https?):((//)|(\\\\))+[\w\d:#@%/;$()~_?\+-=\\\.&]*)";
            new Regex(Filter);

            // rx.Match(url);
            return url;
        }

        /// <summary>
        /// Remove all unwanted Stuff from a Custom Url
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The clean custom url.
        /// </returns>
        public static string CleanCustomUrl(string url)
        {
            if (url.EndsWith(".aspx") || url.EndsWith(".asp") || url.EndsWith(".ascx") || url.EndsWith(".html") ||
                url.EndsWith(".htm"))
            {
                url = url.Remove(url.LastIndexOf(".", StringComparison.Ordinal));
            }

            return Regex.Replace(url, @"\W*", string.Empty);
        }

        /// <summary>
        /// The clean string.
        /// </summary>
        /// <param name="input">
        /// The input string.
        /// </param>
        /// <returns>
        /// Returns the clean string.
        /// </returns>
        public static string CleanString(string input)
        {
            var output = input;

            if (output.Contains("'"))
            {
                output = output.Replace("'", string.Empty);
            }

            if (output.Contains("\""))
            {
                output = output.Replace("\"", string.Empty);
            }

            output = output.Replace(Environment.NewLine, string.Empty);

            output = output.Replace("\r", string.Empty);

            output = output.Replace("\n", string.Empty);

            return output.Trim();
        }

        /// <summary>
        /// Check if Object has a Value
        /// </summary>
        /// <param name="o">
        /// The object .
        /// </param>
        /// <returns>
        /// Returns if object has a value.
        /// </returns>
        public static bool HasValue(object o)
        {
            if (o == null)
            {
                return false;
            }

            if (o == DBNull.Value)
            {
                return false;
            }

            if (o is string)
            {
                if (((string)o).Trim() == string.Empty)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The internal short url.
        /// </summary>
        /// <param name="shortURL">
        /// The short url.
        /// </param>
        /// <param name="domainName">
        /// The s domain name.
        /// </param>
        /// <returns>
        /// Returns internal short url.
        /// </returns>
        public static string InternalShortUrl(string shortURL, string domainName)
        {
            return shortURL.Replace(string.Format("http://{0}/", domainName), string.Empty);
        }

        /// <summary>
        /// Check if Url exists already exists inside DNN 
        ///   (but its not really necessary because this Module Handles only 404s)
        /// </summary>
        /// <param name="sCustUrl">
        /// Url To Check
        /// </param>
        /// <returns>
        /// The is valid custom url.
        /// </returns>
        public static bool IsValidCustomUrl(string sCustUrl)
        {
            if (string.IsNullOrEmpty(sCustUrl))
            {
                return false;
            }

            bool bIsValid = false;

            try
            {
                HttpWebRequest urlReq = (HttpWebRequest)WebRequest.Create(sCustUrl);
                urlReq.Timeout = 6000;
                HttpWebResponse urlRes = (HttpWebResponse)urlReq.GetResponse();

                switch (urlRes.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        bIsValid = true;
                        break;
                }
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    bIsValid = true;
                }
            }

            return bIsValid;
        }

        /// <summary>
        /// Formats a MapPath into relative MapUrl
        /// </summary>
        /// <param name="sPath">
        /// MapPath Input string
        /// </param>
        /// <returns>
        /// The output URL string
        /// </returns>
        public static string MapUrl(string sPath)
        {
            string sAppPath = HttpContext.Current.Server.MapPath("~");

            string sUrl = string.Format(
                "{0}", HttpContext.Current.Request.ApplicationPath + sPath.Replace(sAppPath, string.Empty).Replace("\\", "/"));

            return sUrl;
        }

        /// <summary>
        /// The public short url.
        /// </summary>
        /// <param name="shortURL">
        /// The short url.
        /// </param>
        /// <param name="sDomainName">
        /// The s domain name.
        /// </param>
        /// <returns>
        /// Returns the public short url.
        /// </returns>
        public static string PublicShortUrl(string shortURL, string sDomainName)
        {
            return string.Format("http://{0}/{1}", sDomainName, shortURL);
        }

        /// <summary>
        /// The random characters.
        /// </summary>
        /// <returns>
        /// Returns random characters.
        /// </returns>
        public static string RandomCharacters()
        {
            // Create a local array containing supported short-url characters
            // grouped by types.
            char[][] charGroups = new[]
                {
                    ShorturlCharsLcase.ToCharArray(), ShorturlCharsUcase.ToCharArray(), ShorturlCharsNumeric.ToCharArray()
                };

            // Use this array to track the number of unused characters in each
            // character group.
            int[] charsLeftInGroup = new int[charGroups.Length];

            // Initially, all characters in each group are not used.
            for (int i = 0; i < charsLeftInGroup.Length; i++)
            {
                charsLeftInGroup[i] = charGroups[i].Length;
            }

            // Use this array to track (iterate through) unused character groups.
            int[] leftGroupsOrder = new int[charGroups.Length];

            // Initially, all character groups are not used.
            for (int i = 0; i < leftGroupsOrder.Length; i++)
            {
                leftGroupsOrder[i] = i;
            }

            // Because we cannot use the default randomizer, which is based on the
            // current time (it will produce the same "random" number within a
            // second), we will use a random number generator to seed the
            // randomizer.

            // Use a 4-byte array to fill it with random bytes and convert it then
            // to an integer value.
            byte[] randomBytes = new byte[4];

            // Generate 4 random bytes.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);

            // Convert 4 bytes into a 32-bit integer value.
            int seed = (randomBytes[0] & 0x7f) << 24 | randomBytes[1] << 16 | randomBytes[2] << 8 | randomBytes[3];

            // Now, this is real randomization.
            Random random = new Random(seed);

            // This array will hold short-url characters.

            // Allocate appropriate memory for the short-url.
            char[] shortURL = new char[random.Next(5, 5)];

            // Index of the next character to be added to short-url.

            // Index of the next character group to be processed.

            // Index which will be used to track not processed character groups.

            // Index of the last non-processed character in a group.

            // Index of the last non-processed group.
            int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;

            // Generate short-url characters one at a time.
            for (int i = 0; i < shortURL.Length; i++)
            {
                // If only one character group remained unprocessed, process it;
                // otherwise, pick a random character group from the unprocessed
                // group list. To allow a special character to appear in the
                // first position, increment the second parameter of the Next
                // function call by one, i.e. lastLeftGroupsOrderIdx + 1.
                int nextLeftGroupsOrderIdx = lastLeftGroupsOrderIdx == 0 ? 0 : random.Next(0, lastLeftGroupsOrderIdx);

                // Get the actual index of the character group, from which we will
                // pick the next character.
                int nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];

                // Get the index of the last unprocessed characters in this group.
                int lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

                // If only one unprocessed character is left, pick it; otherwise,
                // get a random character from the unused character list.
                int nextCharIdx = lastCharIdx == 0 ? 0 : random.Next(0, lastCharIdx + 1);

                // Add this character to the short-url.
                shortURL[i] = charGroups[nextGroupIdx][nextCharIdx];

                // If we processed the last character in this group, start over.
                if (lastCharIdx == 0)
                {
                    charsLeftInGroup[nextGroupIdx] = charGroups[nextGroupIdx].Length;
                }
                else
                {
                    // There are more unprocessed characters left.
                    // Swap processed character with the last unprocessed character
                    // so that we don't pick it until we process all characters in
                    // this group.
                    if (lastCharIdx != nextCharIdx)
                    {
                        char temp = charGroups[nextGroupIdx][lastCharIdx];
                        charGroups[nextGroupIdx][lastCharIdx] = charGroups[nextGroupIdx][nextCharIdx];
                        charGroups[nextGroupIdx][nextCharIdx] = temp;
                    }

                    // Decrement the number of unprocessed characters in
                    // this group.
                    charsLeftInGroup[nextGroupIdx]--;
                }

                // If we processed the last group, start all over.
                if (lastLeftGroupsOrderIdx == 0)
                {
                    lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                }
                else
                {
                    // There are more unprocessed groups left.
                    // Swap processed group with the last unprocessed group
                    // so that we don't pick it until we process all groups.
                    if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                    {
                        int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                        leftGroupsOrder[lastLeftGroupsOrderIdx] = leftGroupsOrder[nextLeftGroupsOrderIdx];
                        leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                    }

                    // Decrement the number of unprocessed groups.
                    lastLeftGroupsOrderIdx--;
                }
            }

            // Convert password characters into a string and return the result.
            return new string(shortURL);
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// ReFormat a DateTime to something more usefull
        /// </summary>
        /// <param name="oldDateTime">
        /// The old Date Time.
        /// </param>
        /// <returns>
        /// Returns the formated date time.
        /// </returns>
        internal static string ReFormatDateTime(DateTime oldDateTime)
        {
            DateTime now = DateTime.Now;

            TimeSpan ts = now.Subtract(oldDateTime);

            string sCreated;

            if (ts.Days > 0)
            {
                if (ts.Days >= 30)
                {
                    // More then one Month old
                    sCreated = oldDateTime.ToString("MMMM dd yyyy");
                }
                else if (ts.Days > 1)
                {
                    // More then one Day old
                    sCreated = string.Format("{0} days ago", ts.Days);
                }
                else
                {
                    // One Day old
                    sCreated = string.Format("{0} day ago", ts.Days);
                }
            }
            else
            {
                if (ts.Hours > 0)
                {
                    sCreated = string.Format(ts.Hours.Equals(1) ? "{0} hour ago" : "{0} hours ago", ts.Hours);
                }
                else
                {
                    sCreated = ts.Minutes > 0
                                   ? string.Format(
                                       ts.Hours.Equals(1) ? "{0} minute ago" : "{0} minutes ago", ts.Minutes)
                                   : string.Format("{0} seconds ago", ts.Seconds);
                }
            }

            return sCreated;
        }

        #endregion
    }
}