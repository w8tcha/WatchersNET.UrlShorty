/* WatchersNET.UrlShorty
 * This was originally released as Sourceforge Project http://sourceforge.net/projects/shorturl-dotnet/ by glenarma 
 */

namespace WatchersNET.DNN.Modules.UrlShorty
{
    #region

    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Web;

    using ThoughtWorks.QRCode;

    #endregion

    /// <summary>
    /// The qr code.
    /// </summary>
    public class QrCode : IHttpHandler
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether IsReusable.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Implemented Interfaces

        #region IHttpHandler

        /// <summary>
        /// The process request.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
            string sEncData = !string.IsNullOrEmpty(context.Request["data"])
                                  ? context.Request["data"]
                                  : "http://dnnurlshorty.codeplex.com";

            if (!sEncData.StartsWith(@"http://"))
            {
                sEncData = DataController.RetrieveUrlFromDatabase(sEncData).RealUrl;
            }

            int iScale = !string.IsNullOrEmpty(context.Request["scale"]) ? Convert.ToInt16(context.Request["scale"]) : 3;

            int iVersion = !string.IsNullOrEmpty(context.Request["version"])
                               ? Convert.ToInt16(context.Request["version"])
                               : 7;

            string sErrorCorrect = !string.IsNullOrEmpty(context.Request["errorCorrect"])
                                       ? context.Request["errorCorrect"]
                                       : "M";

            string sEncoding = !string.IsNullOrEmpty(context.Request["encoding"]) ? context.Request["encoding"] : "Byte";

            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder
                {
                   QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE, QRCodeScale = iScale, QRCodeVersion = iVersion, 
                };

            switch (sEncoding)
            {
                case "Byte":
                    qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                    break;
                case "AlphaNumeric":
                    qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.ALPHA_NUMERIC;
                    break;
                case "Numeric":
                    qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.NUMERIC;
                    break;
            }

            switch (sErrorCorrect)
            {
                case "L":
                    qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;
                    break;
                case "M":
                    qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
                    break;
                case "Q":
                    qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.Q;
                    break;
                case "H":
                    qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H;
                    break;
                default:
                    qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
                    break;
            }

            string data = sEncData;
            Image image = qrCodeEncoder.Encode(data);

            context.Response.ContentType = "image/jpeg";
            image.Save(context.Response.OutputStream, ImageFormat.Jpeg);

            image.Dispose();
        }

        #endregion

        #endregion
    }
}