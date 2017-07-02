// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QRCodeDecoder.cs" company="ThoughtWorks">
//   ThoughtWorks
// </copyright>
// <summary>
//   The qr code decoder.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ThoughtWorks.QRCode.Codec
{
    #region

    using System;
    using System.Collections;
    using System.Text;

    using ThoughtWorks.QRCode.Codec.Data;
    using ThoughtWorks.QRCode.Codec.Ecc;
    using ThoughtWorks.QRCode.Codec.Reader;
    using ThoughtWorks.QRCode.Codec.Util;
    using ThoughtWorks.QRCode.ExceptionHandler;
    using ThoughtWorks.QRCode.Geom;

    #endregion

    /// <summary>
    /// The qr code decoder.
    /// </summary>
    public class QRCodeDecoder
    {
        #region Constants and Fields

        /// <summary>
        /// The canvas.
        /// </summary>
        internal static DebugCanvas canvas;

        /// <summary>
        /// The correction succeeded.
        /// </summary>
        internal bool correctionSucceeded;

        /// <summary>
        /// The image reader.
        /// </summary>
        internal QRCodeImageReader imageReader;

        /// <summary>
        /// The last results.
        /// </summary>
        internal ArrayList lastResults = ArrayList.Synchronized(new ArrayList(10));

        /// <summary>
        /// The num last corrections.
        /// </summary>
        internal int numLastCorrections;

        /// <summary>
        /// The num try decode.
        /// </summary>
        internal int numTryDecode;

        /// <summary>
        /// The qr code symbol.
        /// </summary>
        internal QRCodeSymbol qrCodeSymbol;

        /// <summary>
        /// The results.
        /// </summary>
        internal ArrayList results;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QRCodeDecoder"/> class.
        /// </summary>
        public QRCodeDecoder()
        {
            this.numTryDecode = 0;
            this.results = ArrayList.Synchronized(new ArrayList(10));
            canvas = new DebugCanvasAdapter();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Canvas.
        /// </summary>
        public static DebugCanvas Canvas
        {
            get
            {
                return canvas;
            }

            set
            {
                canvas = value;
            }
        }

        /// <summary>
        /// Gets AdjustPoints.
        /// </summary>
        internal virtual Point[] AdjustPoints
        {
            get
            {
                // note that adjusts affect dependently
                // i.e. below means (0,0), (2,3), (3,4), (1,2), (2,1), (1,1), (-1,-1)

                // 		Point[] adjusts = {new Point(0,0), new Point(2,3), new Point(1,1), 
                // 				new Point(-2,-2), new Point(1,-1), new Point(-1,0), new Point(-2,-2)};
                ArrayList adjustPoints = ArrayList.Synchronized(new ArrayList(10));
                for (int d = 0; d < 4; d++)
                {
                    adjustPoints.Add(new Point(1, 1));
                }

                int lastX = 0, lastY = 0;
                for (int y = 0; y > -4; y--)
                {
                    for (int x = 0; x > -4; x--)
                    {
                        if (x == y || ((x + y) % 2 != 0))
                        {
                            continue;
                        }

                        adjustPoints.Add(new Point(x - lastX, y - lastY));
                        lastX = x;
                        lastY = y;
                    }
                }

                Point[] adjusts = new Point[adjustPoints.Count];
                for (int i = 0; i < adjusts.Length; i++)
                {
                    adjusts[i] = (Point)adjustPoints[i];
                }

                return adjusts;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The decode.
        /// </summary>
        /// <param name="qrCodeImage">
        /// The qr code image.
        /// </param>
        /// <param name="encoding">
        /// The encoding.
        /// </param>
        /// <returns>
        /// The decode.
        /// </returns>
        public virtual string decode(QRCodeImage qrCodeImage, Encoding encoding)
        {
            sbyte[] data = this.decodeBytes(qrCodeImage);
            byte[] byteData = new byte[data.Length];

            Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);

            /*
            char[] decodedData = new char[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                decodedData[i] = Convert.to(data[i]);

            }
            return new String(decodedData);
            */
            string decodedData = encoding.GetString(byteData);
            return decodedData;
        }

        /// <summary>
        /// The decode.
        /// </summary>
        /// <param name="qrCodeImage">
        /// The qr code image.
        /// </param>
        /// <returns>
        /// The decode.
        /// </returns>
        public virtual string decode(QRCodeImage qrCodeImage)
        {
            sbyte[] data = this.decodeBytes(qrCodeImage);
            byte[] byteData = new byte[data.Length];
            Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);

            Encoding encoding = QRCodeUtility.IsUnicode(byteData) ? Encoding.Unicode : Encoding.ASCII;

            string decodedData = encoding.GetString(byteData);
            return decodedData;
        }

        /// <summary>
        /// The decode bytes.
        /// </summary>
        /// <param name="qrCodeImage">
        /// The qr code image.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="DecodingFailedException">
        /// </exception>
        /// <exception cref="DecodingFailedException">
        /// </exception>
        public virtual sbyte[] decodeBytes(QRCodeImage qrCodeImage)
        {
            Point[] adjusts = this.AdjustPoints;
            ArrayList results = ArrayList.Synchronized(new ArrayList(10));
            while (this.numTryDecode < adjusts.Length)
            {
                try
                {
                    DecodeResult result = decode(qrCodeImage, adjusts[this.numTryDecode]);
                    if (result.CorrectionSucceeded)
                    {
                        return result.DecodedBytes;
                    }
                    else
                    {
                        results.Add(result);
                        canvas.println("Decoding succeeded but could not correct");
                        canvas.println("all errors. Retrying..");
                    }
                }
                catch (DecodingFailedException dfe)
                {
                    if (dfe.Message.IndexOf("Finder Pattern") >= 0)
                    {
                        throw dfe;
                    }
                }
                finally
                {
                    this.numTryDecode += 1;
                }
            }

            if (results.Count == 0)
            {
                throw new DecodingFailedException("Give up decoding");
            }

            int lowestErrorIndex = -1;
            int lowestError = Int32.MaxValue;
            for (int i = 0; i < results.Count; i++)
            {
                DecodeResult result = (DecodeResult)results[i];

                if (result.NumErrors >= lowestError)
                {
                    continue;
                }

                lowestError = result.NumErrors;
                lowestErrorIndex = i;
            }

            canvas.println("All trials need for correct error");
            canvas.println("Reporting #" + lowestErrorIndex + " that,");
            canvas.println("corrected minimum errors (" + lowestError + ")");

            canvas.println("Decoding finished.");
            return ((DecodeResult)results[lowestErrorIndex]).DecodedBytes;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The correct data blocks.
        /// </summary>
        /// <param name="blocks">
        /// The blocks.
        /// </param>
        /// <returns>
        /// </returns>
        internal virtual int[] correctDataBlocks(int[] blocks)
        {
            int numCorrections = 0;
            int dataCapacity = this.qrCodeSymbol.DataCapacity;
            int[] dataBlocks = new int[dataCapacity];
            int numErrorCollectionCode = this.qrCodeSymbol.NumErrorCollectionCode;
            int numRSBlocks = this.qrCodeSymbol.NumRSBlocks;
            int eccPerRSBlock = numErrorCollectionCode / numRSBlocks;
            if (numRSBlocks == 1)
            {
                ReedSolomon corrector = new ReedSolomon(blocks, eccPerRSBlock);
                corrector.correct();
                numCorrections += corrector.NumCorrectedErrors;
                if (numCorrections > 0)
                {
                    canvas.println(Convert.ToString(numCorrections) + " data errors corrected.");
                }
                else
                {
                    canvas.println("No errors found.");
                }

                this.numLastCorrections = numCorrections;
                this.correctionSucceeded = corrector.CorrectionSucceeded;
                return blocks;
            }

            // we have to interleave data blocks because symbol has 2 or more RS blocks
            int numLongerRSBlocks = dataCapacity % numRSBlocks;
            if (numLongerRSBlocks == 0)
            {
                // symbol has only 1 type of RS block
                int lengthRSBlock = dataCapacity / numRSBlocks;
                int[][] tmpArray = new int[numRSBlocks][];
                for (int i = 0; i < numRSBlocks; i++)
                {
                    tmpArray[i] = new int[lengthRSBlock];
                }

                int[][] RSBlocks = tmpArray;

                // obtain RS blocks
                for (int i = 0; i < numRSBlocks; i++)
                {
                    for (int j = 0; j < lengthRSBlock; j++)
                    {
                        RSBlocks[i][j] = blocks[j * numRSBlocks + i];
                    }

                    ReedSolomon corrector = new ReedSolomon(RSBlocks[i], eccPerRSBlock);
                    corrector.correct();
                    numCorrections += corrector.NumCorrectedErrors;
                    this.correctionSucceeded = corrector.CorrectionSucceeded;
                }

                // obtain only data part
                int p = 0;
                for (int i = 0; i < numRSBlocks; i++)
                {
                    for (int j = 0; j < lengthRSBlock - eccPerRSBlock; j++)
                    {
                        dataBlocks[p++] = RSBlocks[i][j];
                    }
                }
            }
            else
            {
                // symbol has 2 types of RS blocks
                int lengthShorterRSBlock = dataCapacity / numRSBlocks;
                int lengthLongerRSBlock = dataCapacity / numRSBlocks + 1;
                int numShorterRSBlocks = numRSBlocks - numLongerRSBlocks;
                int[][] tmpArray2 = new int[numShorterRSBlocks][];
                for (int i2 = 0; i2 < numShorterRSBlocks; i2++)
                {
                    tmpArray2[i2] = new int[lengthShorterRSBlock];
                }

                int[][] shorterRSBlocks = tmpArray2;
                int[][] tmpArray3 = new int[numLongerRSBlocks][];
                for (int i3 = 0; i3 < numLongerRSBlocks; i3++)
                {
                    tmpArray3[i3] = new int[lengthLongerRSBlock];
                }

                int[][] longerRSBlocks = tmpArray3;
                for (int i = 0; i < numRSBlocks; i++)
                {
                    if (i < numShorterRSBlocks)
                    {
                        // get shorter RS Block(s)
                        int mod = 0;
                        for (int j = 0; j < lengthShorterRSBlock; j++)
                        {
                            if (j == lengthShorterRSBlock - eccPerRSBlock)
                            {
                                mod = numLongerRSBlocks;
                            }

                            shorterRSBlocks[i][j] = blocks[j * numRSBlocks + i + mod];
                        }

                        ReedSolomon corrector = new ReedSolomon(shorterRSBlocks[i], eccPerRSBlock);
                        corrector.correct();
                        numCorrections += corrector.NumCorrectedErrors;
                        this.correctionSucceeded = corrector.CorrectionSucceeded;
                    }
                    else
                    {
                        // get longer RS Blocks
                        int mod = 0;
                        for (int j = 0; j < lengthLongerRSBlock; j++)
                        {
                            if (j == lengthShorterRSBlock - eccPerRSBlock)
                            {
                                mod = numShorterRSBlocks;
                            }

                            longerRSBlocks[i - numShorterRSBlocks][j] = blocks[j * numRSBlocks + i - mod];
                        }

                        ReedSolomon corrector = new ReedSolomon(
                            longerRSBlocks[i - numShorterRSBlocks], eccPerRSBlock);
                        corrector.correct();
                        numCorrections += corrector.NumCorrectedErrors;
                        this.correctionSucceeded = corrector.CorrectionSucceeded;
                    }
                }

                int p = 0;
                for (int i = 0; i < numRSBlocks; i++)
                {
                    if (i < numShorterRSBlocks)
                    {
                        for (int j = 0; j < lengthShorterRSBlock - eccPerRSBlock; j++)
                        {
                            dataBlocks[p++] = shorterRSBlocks[i][j];
                        }
                    }
                    else
                    {
                        for (int j = 0; j < lengthLongerRSBlock - eccPerRSBlock; j++)
                        {
                            dataBlocks[p++] = longerRSBlocks[i - numShorterRSBlocks][j];
                        }
                    }
                }
            }

            if (numCorrections > 0)
            {
                canvas.println(Convert.ToString(numCorrections) + " data errors corrected.");
            }
            else
            {
                canvas.println("No errors found.");
            }

            this.numLastCorrections = numCorrections;
            return dataBlocks;
        }

        /// <summary>
        /// The decode.
        /// </summary>
        /// <param name="qrCodeImage">
        /// The qr code image.
        /// </param>
        /// <param name="adjust">
        /// The adjust.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="DecodingFailedException">
        /// </exception>
        /// <exception cref="DecodingFailedException">
        /// </exception>
        internal virtual DecodeResult decode(QRCodeImage qrCodeImage, Point adjust)
        {
            try
            {
                if (this.numTryDecode == 0)
                {
                    canvas.println("Decoding started");
                    int[][] intImage = this.imageToIntArray(qrCodeImage);
                    this.imageReader = new QRCodeImageReader();
                    this.qrCodeSymbol = this.imageReader.getQRCodeSymbol(intImage);
                }
                else
                {
                    canvas.println("--");
                    canvas.println("Decoding restarted #" + this.numTryDecode);
                    this.qrCodeSymbol = this.imageReader.getQRCodeSymbolWithAdjustedGrid(adjust);
                }
            }
            catch (SymbolNotFoundException e)
            {
                throw new DecodingFailedException(e.Message);
            }

            canvas.println("Created QRCode symbol.");
            canvas.println("Reading symbol.");
            canvas.println("Version: " + this.qrCodeSymbol.VersionReference);
            canvas.println("Mask pattern: " + this.qrCodeSymbol.MaskPatternRefererAsString);

            // blocks contains all (data and RS) blocks in QR Code symbol
            int[] blocks = this.qrCodeSymbol.Blocks;
            canvas.println("Correcting data errors.");

            // now blocks turn to data blocks (corrected and extracted from original blocks)
            blocks = this.correctDataBlocks(blocks);
            try
            {
                sbyte[] decodedByteArray = this.getDecodedByteArray(
                    blocks, this.qrCodeSymbol.Version, this.qrCodeSymbol.NumErrorCollectionCode);
                return new DecodeResult(this, decodedByteArray, this.numLastCorrections, this.correctionSucceeded);
            }
            catch (InvalidDataBlockException e)
            {
                canvas.println(e.Message);
                throw new DecodingFailedException(e.Message);
            }
        }

        /// <summary>
        /// The get decoded byte array.
        /// </summary>
        /// <param name="blocks">
        /// The blocks.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <param name="numErrorCorrectionCode">
        /// The num error correction code.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="InvalidDataBlockException">
        /// </exception>
        internal virtual sbyte[] getDecodedByteArray(int[] blocks, int version, int numErrorCorrectionCode)
        {
            sbyte[] byteArray;
            QRCodeDataBlockReader reader = new QRCodeDataBlockReader(blocks, version, numErrorCorrectionCode);
            try
            {
                byteArray = reader.DataByte;
            }
            catch (InvalidDataBlockException e)
            {
                throw e;
            }

            return byteArray;
        }

        /// <summary>
        /// The get decoded string.
        /// </summary>
        /// <param name="blocks">
        /// The blocks.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <param name="numErrorCorrectionCode">
        /// The num error correction code.
        /// </param>
        /// <returns>
        /// The get decoded string.
        /// </returns>
        /// <exception cref="InvalidDataBlockException">
        /// </exception>
        internal virtual string getDecodedString(int[] blocks, int version, int numErrorCorrectionCode)
        {
            string dataString;
            QRCodeDataBlockReader reader = new QRCodeDataBlockReader(blocks, version, numErrorCorrectionCode);
            try
            {
                dataString = reader.DataString;
            }
            catch (IndexOutOfRangeException e)
            {
                throw new InvalidDataBlockException(e.Message);
            }

            return dataString;
        }

        /// <summary>
        /// The image to int array.
        /// </summary>
        /// <param name="image">
        /// The image.
        /// </param>
        /// <returns>
        /// </returns>
        internal virtual int[][] imageToIntArray(QRCodeImage image)
        {
            int width = image.Width;
            int height = image.Height;
            int[][] intImage = new int[width][];
            for (int i = 0; i < width; i++)
            {
                intImage[i] = new int[height];
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    intImage[x][y] = image.getPixel(x, y);
                }
            }

            return intImage;
        }

        #endregion

        /// <summary>
        /// The decode result.
        /// </summary>
        internal class DecodeResult
        {
            #region Constants and Fields

            /// <summary>
            /// The correction succeeded.
            /// </summary>
            internal bool correctionSucceeded;

            /// <summary>
            /// The decoded bytes.
            /// </summary>
            internal sbyte[] decodedBytes;

            /// <summary>
            /// The num corrections.
            /// </summary>
            internal int numCorrections;

            /// <summary>
            /// The enclosing instance.
            /// </summary>
            private QRCodeDecoder enclosingInstance;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="DecodeResult"/> class.
            /// </summary>
            /// <param name="enclosingInstance">
            /// The enclosing instance.
            /// </param>
            /// <param name="decodedBytes">
            /// The decoded bytes.
            /// </param>
            /// <param name="numErrors">
            /// The num errors.
            /// </param>
            /// <param name="correctionSucceeded">
            /// The correction succeeded.
            /// </param>
            public DecodeResult(
                QRCodeDecoder enclosingInstance, sbyte[] decodedBytes, int numErrors, bool correctionSucceeded)
            {
                this.InitBlock(enclosingInstance);
                this.decodedBytes = decodedBytes;
                this.numCorrections = numErrors;
                this.correctionSucceeded = correctionSucceeded;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets a value indicating whether CorrectionSucceeded.
            /// </summary>
            public virtual bool CorrectionSucceeded
            {
                get
                {
                    return this.correctionSucceeded;
                }
            }

            /// <summary>
            /// Gets DecodedBytes.
            /// </summary>
            public virtual sbyte[] DecodedBytes
            {
                get
                {
                    return this.decodedBytes;
                }
            }

            /// <summary>
            /// Gets Enclosing_Instance.
            /// </summary>
            public QRCodeDecoder Enclosing_Instance
            {
                get
                {
                    return this.enclosingInstance;
                }
            }

            /// <summary>
            /// Gets NumErrors.
            /// </summary>
            public virtual int NumErrors
            {
                get
                {
                    return this.numCorrections;
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// The init block.
            /// </summary>
            /// <param name="_enclosingInstance">
            /// The enclosing instance.
            /// </param>
            private void InitBlock(QRCodeDecoder _enclosingInstance)
            {
                this.enclosingInstance = _enclosingInstance;
            }

            #endregion
        }
    }
}