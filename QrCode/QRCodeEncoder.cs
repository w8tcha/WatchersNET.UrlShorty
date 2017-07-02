// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QRCodeEncoder.cs" company="ThoughtWorks">
//   ThoughtWorks
// </copyright>
// <summary>
//   The qr code encoder.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ThoughtWorks.QRCode
{
    #region

    using System;
    using System.Drawing;
    using System.IO;
    using System.Text;

    using ThoughtWorks.QRCode.Codec.Util;
    using ThoughtWorks.QRCode.Properties;

    using Color = System.Drawing.Color;

    #endregion

    /// <summary>
    /// The qr code encoder.
    /// </summary>
    public class QRCodeEncoder
    {
        #region Constants and Fields

        /// <summary>
        /// The qr code background color.
        /// </summary>
        internal Color qrCodeBackgroundColor;

        /// <summary>
        /// The qr code foreground color.
        /// </summary>
        internal Color qrCodeForegroundColor;

        /// <summary>
        /// The qr code scale.
        /// </summary>
        internal int qrCodeScale;

        /// <summary>
        /// The qrcode encode mode.
        /// </summary>
        internal ENCODE_MODE qrcodeEncodeMode;

        /// <summary>
        /// The qrcode error correct.
        /// </summary>
        internal ERROR_CORRECTION qrcodeErrorCorrect;

        /// <summary>
        /// The qrcode structureappend m.
        /// </summary>
        internal int qrcodeStructureappendM;

        /// <summary>
        /// The qrcode structureappend n.
        /// </summary>
        internal int qrcodeStructureappendN;

        /// <summary>
        /// The qrcode structureappend originaldata.
        /// </summary>
        internal string qrcodeStructureappendOriginaldata;

        /// <summary>
        /// The qrcode structureappend parity.
        /// </summary>
        internal int qrcodeStructureappendParity;

        /// <summary>
        /// The qrcode version.
        /// </summary>
        internal int qrcodeVersion;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QRCodeEncoder"/> class. 
        ///   Constructor
        /// </summary>
        public QRCodeEncoder()
        {
            this.qrcodeErrorCorrect = ERROR_CORRECTION.M;
            this.qrcodeEncodeMode = ENCODE_MODE.BYTE;
            this.qrcodeVersion = 7;

            this.qrcodeStructureappendN = 0;
            this.qrcodeStructureappendM = 0;
            this.qrcodeStructureappendParity = 0;
            this.qrcodeStructureappendOriginaldata = string.Empty;

            this.qrCodeScale = 4;
            this.qrCodeBackgroundColor = Color.White;
            this.qrCodeForegroundColor = Color.Black;

            // QRCODE_DATA_PATH = Environment.CurrentDirectory + @"\" + DATA_PATH;
        }

        #endregion

        #region Enums

        /// <summary>
        /// The encod e_ mode.
        /// </summary>
        public enum ENCODE_MODE
        {
            /// <summary>
            /// The alph a_ numeric.
            /// </summary>
            ALPHA_NUMERIC, 

            /// <summary>
            /// The numeric.
            /// </summary>
            NUMERIC, 

            /// <summary>
            /// The byte.
            /// </summary>
            BYTE
        }

        /// <summary>
        /// The erro r_ correction.
        /// </summary>
        public enum ERROR_CORRECTION
        {
            /// <summary>
            /// The l.
            /// </summary>
            L, 

            /// <summary>
            /// The m.
            /// </summary>
            M, 

            /// <summary>
            /// The q.
            /// </summary>
            Q, 

            /// <summary>
            /// The h.
            /// </summary>
            H
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets QRCodeBackgroundColor.
        /// </summary>
        public virtual Color QRCodeBackgroundColor
        {
            get
            {
                return this.qrCodeBackgroundColor;
            }

            set
            {
                this.qrCodeBackgroundColor = value;
            }
        }

        /// <summary>
        /// Gets or sets QRCodeEncodeMode.
        /// </summary>
        public virtual ENCODE_MODE QRCodeEncodeMode
        {
            get
            {
                return this.qrcodeEncodeMode;
            }

            set
            {
                this.qrcodeEncodeMode = value;
            }
        }

        /// <summary>
        /// Gets or sets QRCodeErrorCorrect.
        /// </summary>
        public virtual ERROR_CORRECTION QRCodeErrorCorrect
        {
            get
            {
                return this.qrcodeErrorCorrect;
            }

            set
            {
                this.qrcodeErrorCorrect = value;
            }
        }

        /// <summary>
        /// Gets or sets QRCodeForegroundColor.
        /// </summary>
        public virtual Color QRCodeForegroundColor
        {
            get
            {
                return this.qrCodeForegroundColor;
            }

            set
            {
                this.qrCodeForegroundColor = value;
            }
        }

        /// <summary>
        /// Gets or sets QRCodeScale.
        /// </summary>
        public virtual int QRCodeScale
        {
            get
            {
                return this.qrCodeScale;
            }

            set
            {
                this.qrCodeScale = value;
            }
        }

        /// <summary>
        /// Gets or sets QRCodeVersion.
        /// </summary>
        public virtual int QRCodeVersion
        {
            get
            {
                return this.qrcodeVersion;
            }

            set
            {
                if (value >= 0 && value <= 40)
                {
                    this.qrcodeVersion = value;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Encode the content using the encoding scheme given
        /// </summary>
        /// <param name="content">
        /// </param>
        /// <param name="encoding">
        /// </param>
        /// <returns>
        /// </returns>
        public virtual Bitmap Encode(string content, Encoding encoding)
        {
            bool[][] matrix = this.calQrcode(encoding.GetBytes(content));
            SolidBrush brush = new SolidBrush(this.qrCodeBackgroundColor);
            Bitmap image = new Bitmap((matrix.Length * this.qrCodeScale) + 1, (matrix.Length * this.qrCodeScale) + 1);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(brush, new Rectangle(0, 0, image.Width, image.Height));
            brush.Color = this.qrCodeForegroundColor;
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix.Length; j++)
                {
                    if (matrix[j][i])
                    {
                        g.FillRectangle(
                            brush, j * this.qrCodeScale, i * this.qrCodeScale, this.qrCodeScale, this.qrCodeScale);
                    }
                }
            }

            return image;
        }

        /// <summary>
        /// Encode the content using the encoding scheme given
        /// </summary>
        /// <param name="content">
        /// </param>
        /// <returns>
        /// </returns>
        public virtual Bitmap Encode(string content)
        {
            return this.Encode(content, QRCodeUtility.IsUniCode(content) ? Encoding.Unicode : Encoding.ASCII);
        }

        /// <summary>
        /// The cal qrcode.
        /// </summary>
        /// <param name="qrcodeData">
        /// The qrcode data.
        /// </param>
        /// <returns>
        /// </returns>
        public virtual bool[][] calQrcode(byte[] qrcodeData)
        {
            int dataCounter = 0;

            int dataLength = qrcodeData.Length;

            int[] dataValue = new int[dataLength + 32];
            sbyte[] dataBits = new sbyte[dataLength + 32];

            if (dataLength <= 0)
            {
                bool[][] ret = new[] { new[] { false } };
                return ret;
            }

            if (this.qrcodeStructureappendN > 1)
            {
                dataValue[0] = 3;
                dataBits[0] = 4;

                dataValue[1] = this.qrcodeStructureappendM - 1;
                dataBits[1] = 4;

                dataValue[2] = this.qrcodeStructureappendN - 1;
                dataBits[2] = 4;

                dataValue[3] = this.qrcodeStructureappendParity;
                dataBits[3] = 8;

                dataCounter = 4;
            }

            dataBits[dataCounter] = 4;

            /*  --- determine encode mode --- */
            int[] codewordNumPlus;
            int codewordNumCounterValue;

            switch (this.qrcodeEncodeMode)
            {
                    /* ---- alphanumeric mode ---  */
                case ENCODE_MODE.ALPHA_NUMERIC:

                    codewordNumPlus = new[]
                        {
                            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4
                            , 4, 4, 4, 4, 4, 4, 4, 4, 4
                        };

                    dataValue[dataCounter] = 2;
                    dataCounter++;
                    dataValue[dataCounter] = dataLength;
                    dataBits[dataCounter] = 9;
                    codewordNumCounterValue = dataCounter;

                    dataCounter++;
                    for (int i = 0; i < dataLength; i++)
                    {
                        char chr = (char)qrcodeData[i];
                        sbyte chrValue = 0;
                        if (chr >= 48 && chr < 58)
                        {
                            chrValue = (sbyte)(chr - 48);
                        }
                        else
                        {
                            if (chr >= 65 && chr < 91)
                            {
                                chrValue = (sbyte)(chr - 55);
                            }
                            else
                            {
                                if (chr == 32)
                                {
                                    chrValue = 36;
                                }

                                if (chr == 36)
                                {
                                    chrValue = 37;
                                }

                                if (chr == 37)
                                {
                                    chrValue = 38;
                                }

                                if (chr == 42)
                                {
                                    chrValue = 39;
                                }

                                if (chr == 43)
                                {
                                    chrValue = 40;
                                }

                                if (chr == 45)
                                {
                                    chrValue = 41;
                                }

                                if (chr == 46)
                                {
                                    chrValue = 42;
                                }

                                if (chr == 47)
                                {
                                    chrValue = 43;
                                }

                                if (chr == 58)
                                {
                                    chrValue = 44;
                                }
                            }
                        }

                        if ((i % 2) == 0)
                        {
                            dataValue[dataCounter] = chrValue;
                            dataBits[dataCounter] = 6;
                        }
                        else
                        {
                            dataValue[dataCounter] = dataValue[dataCounter] * 45 + chrValue;
                            dataBits[dataCounter] = 11;
                            if (i < dataLength - 1)
                            {
                                dataCounter++;
                            }
                        }
                    }

                    dataCounter++;
                    break;

                    /* ---- numeric mode ---- */
                case ENCODE_MODE.NUMERIC:

                    codewordNumPlus = new[]
                        {
                            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4
                            , 4, 4, 4, 4, 4, 4, 4, 4, 4
                        };

                    dataValue[dataCounter] = 1;
                    dataCounter++;
                    dataValue[dataCounter] = dataLength;

                    dataBits[dataCounter] = 10; /* #version 1-9*/
                    codewordNumCounterValue = dataCounter;

                    dataCounter++;
                    for (int i = 0; i < dataLength; i++)
                    {
                        if ((i % 3) == 0)
                        {
                            dataValue[dataCounter] = qrcodeData[i] - 0x30;
                            dataBits[dataCounter] = 4;
                        }
                        else
                        {
                            dataValue[dataCounter] = dataValue[dataCounter] * 10 + (qrcodeData[i] - 0x30);

                            if ((i % 3) == 1)
                            {
                                dataBits[dataCounter] = 7;
                            }
                            else
                            {
                                dataBits[dataCounter] = 10;
                                if (i < dataLength - 1)
                                {
                                    dataCounter++;
                                }
                            }
                        }
                    }

                    dataCounter++;
                    break;

                    /* ---- 8bit byte ---- */
                default:

                    codewordNumPlus = new[]
                        {
                            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8
                            , 8, 8, 8, 8, 8, 8, 8, 8, 8
                        };
                    dataValue[dataCounter] = 4;
                    dataCounter++;
                    dataValue[dataCounter] = dataLength;
                    dataBits[dataCounter] = 8; /* #version 1-9 */
                    codewordNumCounterValue = dataCounter;

                    dataCounter++;

                    for (int i = 0; i < dataLength; i++)
                    {
                        dataValue[i + dataCounter] = qrcodeData[i] & 0xFF;
                        dataBits[i + dataCounter] = 8;
                    }

                    dataCounter += dataLength;

                    break;
            }

            int totalDataBits = 0;
            for (int i = 0; i < dataCounter; i++)
            {
                totalDataBits += dataBits[i];
            }

            int ec;
            switch (this.qrcodeErrorCorrect)
            {
                case ERROR_CORRECTION.L:
                    ec = 1;
                    break;

                case ERROR_CORRECTION.Q:
                    ec = 3;
                    break;

                case ERROR_CORRECTION.H:
                    ec = 2;
                    break;

                default:
                    ec = 0;
                    break;
            }

            int[][] maxDataBitsArray = new[]
                {
                    new[]
                        {
                            0, 128, 224, 352, 512, 688, 864, 992, 1232, 1456, 1728, 2032, 2320, 2672, 2920, 3320, 3624, 
                            4056, 4504, 5016, 5352, 5712, 6256, 6880, 7312, 8000, 8496, 9024, 9544, 10136, 10984, 11640, 
                            12328, 13048, 13800, 14496, 15312, 15936, 16816, 17728, 18672
                        }, 
                    new[]
                        {
                            0, 152, 272, 440, 640, 864, 1088, 1248, 1552, 1856, 2192, 2592, 2960, 3424, 3688, 4184, 4712, 
                            5176, 5768, 6360, 6888, 7456, 8048, 8752, 9392, 10208, 10960, 11744, 12248, 13048, 13880, 
                            14744, 15640, 16568, 17528, 18448, 19472, 20528, 21616, 22496, 23648
                        }, 
                    new[]
                        {
                            0, 72, 128, 208, 288, 368, 480, 528, 688, 800, 976, 1120, 1264, 1440, 1576, 1784, 2024, 2264, 
                            2504, 2728, 3080, 3248, 3536, 3712, 4112, 4304, 4768, 5024, 5288, 5608, 5960, 6344, 6760, 7208
                            , 7688, 7888, 8432, 8768, 9136, 9776, 10208
                        }, 
                    new[]
                        {
                            0, 104, 176, 272, 384, 496, 608, 704, 880, 1056, 1232, 1440, 1648, 1952, 2088, 2360, 2600, 
                            2936, 3176, 3560, 3880, 4096, 4544, 4912, 5312, 5744, 6032, 6464, 6968, 7288, 7880, 8264, 8920
                            , 9368, 9848, 10288, 10832, 11408, 12016, 12656, 13328
                        }
                };

            int maxDataBits = 0;

            if (this.qrcodeVersion == 0)
            {
                /* auto version select */
                this.qrcodeVersion = 1;
                for (int i = 1; i <= 40; i++)
                {
                    if (maxDataBitsArray[ec][i] >= totalDataBits + codewordNumPlus[this.qrcodeVersion])
                    {
                        maxDataBits = maxDataBitsArray[ec][i];
                        break;
                    }

                    this.qrcodeVersion++;
                }
            }
            else
            {
                maxDataBits = maxDataBitsArray[ec][this.qrcodeVersion];
            }

            totalDataBits += codewordNumPlus[this.qrcodeVersion];
            dataBits[codewordNumCounterValue] =
                (sbyte)(dataBits[codewordNumCounterValue] + codewordNumPlus[this.qrcodeVersion]);

            int[] maxCodewordsArray = new[]
                {
                    0, 26, 44, 70, 100, 134, 172, 196, 242, 292, 346, 404, 466, 532, 581, 655, 733, 815, 901, 991, 1085, 
                    1156, 1258, 1364, 1474, 1588, 1706, 1828, 1921, 2051, 2185, 2323, 2465, 2611, 2761, 2876, 3034, 3196, 
                    3362, 3532, 3706
                };

            int maxCodewords = maxCodewordsArray[this.qrcodeVersion];
            int i1 = 17 + (this.qrcodeVersion << 2);

            int[] matrixRemainBit = new[]
                {
                    0, 0, 7, 7, 7, 7, 7, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 
                    3, 0, 0, 0, 0, 0, 0
                };

            /* ---- read version ECC data file */
            int byte_num = matrixRemainBit[this.qrcodeVersion] + (maxCodewords << 3);

            sbyte[] matrixX = new sbyte[byte_num];
            sbyte[] matrixY = new sbyte[byte_num];
            sbyte[] maskArray = new sbyte[byte_num];
            sbyte[] formatInformationX2 = new sbyte[15];
            sbyte[] formatInformationY2 = new sbyte[15];
            sbyte[] rsEccCodewords = new sbyte[1];
            sbyte[] rsBlockOrderTemp = new sbyte[128];

            try
            {
                // String filename = QRCODE_DATA_PATH + @"\qrv" + System.Convert.ToString(qrcodeVersion) + "_" + System.Convert.ToString(ec) + ".dat";
                // StreamReader reader = new StreamReader(filename);
                // BufferedStream bis = new BufferedStream(reader.BaseStream);
                string fileName = "qrv" + Convert.ToString(this.qrcodeVersion) + "_" + Convert.ToString(ec);
                MemoryStream memoryStream = new MemoryStream((byte[])Resources.ResourceManager.GetObject(fileName));
                BufferedStream bis = new BufferedStream(memoryStream);

                SystemUtils.ReadInput(bis, matrixX, 0, matrixX.Length);
                SystemUtils.ReadInput(bis, matrixY, 0, matrixY.Length);
                SystemUtils.ReadInput(bis, maskArray, 0, maskArray.Length);
                SystemUtils.ReadInput(bis, formatInformationX2, 0, formatInformationX2.Length);
                SystemUtils.ReadInput(bis, formatInformationY2, 0, formatInformationY2.Length);
                SystemUtils.ReadInput(bis, rsEccCodewords, 0, rsEccCodewords.Length);
                SystemUtils.ReadInput(bis, rsBlockOrderTemp, 0, rsBlockOrderTemp.Length);

                bis.Close();
                memoryStream.Close();

                // reader.Close();
                /*
                fis.Close();
                */
            }
            catch (Exception e)
            {
                SystemUtils.WriteStackTrace(e, Console.Error);
            }

            sbyte rsBlockOrderLength = 1;
            for (sbyte i = 1; i < 128; i++)
            {
                if (rsBlockOrderTemp[i] != 0)
                {
                    continue;
                }

                rsBlockOrderLength = i;
                break;
            }

            sbyte[] rsBlockOrder = new sbyte[rsBlockOrderLength];
            Array.Copy(rsBlockOrderTemp, 0, rsBlockOrder, 0, (byte)rsBlockOrderLength);

            sbyte[] formatInformationX1 = new sbyte[] { 0, 1, 2, 3, 4, 5, 7, 8, 8, 8, 8, 8, 8, 8, 8 };
            sbyte[] formatInformationY1 = new sbyte[] { 8, 8, 8, 8, 8, 8, 8, 8, 7, 5, 4, 3, 2, 1, 0 };

            int maxDataCodewords = maxDataBits >> 3;

            /* -- read frame data  -- */
            int modules1Side = 4 * this.qrcodeVersion + 17;
            int matrixTotalBits = modules1Side * modules1Side;
            sbyte[] frameData = new sbyte[matrixTotalBits + modules1Side];

            try
            {
                // String filename = QRCODE_DATA_PATH + "/qrvfr" + System.Convert.ToString(qrcodeVersion) + ".dat";
                // StreamReader reader = new StreamReader(filename);
                string fileName = "qrvfr" + Convert.ToString(this.qrcodeVersion);
                MemoryStream memoryStream = new MemoryStream((byte[])Resources.ResourceManager.GetObject(fileName));

                BufferedStream bis = new BufferedStream(memoryStream);
                SystemUtils.ReadInput(bis, frameData, 0, frameData.Length);
                bis.Close();
                memoryStream.Close();

                // reader.Close();
                // fis.Close();
            }
            catch (Exception e)
            {
                SystemUtils.WriteStackTrace(e, Console.Error);
            }

            /*  --- set terminator */
            if (totalDataBits <= maxDataBits - 4)
            {
                dataValue[dataCounter] = 0;
                dataBits[dataCounter] = 4;
            }
            else
            {
                if (totalDataBits < maxDataBits)
                {
                    dataValue[dataCounter] = 0;
                    dataBits[dataCounter] = (sbyte)(maxDataBits - totalDataBits);
                }
                else
                {
                    if (totalDataBits > maxDataBits)
                    {
                        Console.Out.WriteLine("overflow");
                    }
                }
            }

            sbyte[] dataCodewords = divideDataBy8Bits(dataValue, dataBits, maxDataCodewords);
            sbyte[] codewords = calculateRSECC(
                dataCodewords, rsEccCodewords[0], rsBlockOrder, maxDataCodewords, maxCodewords);

            /* ---- flash matrix */
            sbyte[][] matrixContent = new sbyte[modules1Side][];
            for (int i2 = 0; i2 < modules1Side; i2++)
            {
                matrixContent[i2] = new sbyte[modules1Side];
            }

            for (int i = 0; i < modules1Side; i++)
            {
                for (int j = 0; j < modules1Side; j++)
                {
                    matrixContent[j][i] = 0;
                }
            }

            /* --- attach data */
            for (int i = 0; i < maxCodewords; i++)
            {
                sbyte codeword_i = codewords[i];
                for (int j = 7; j >= 0; j--)
                {
                    int codewordBitsNumber = (i * 8) + j;

                    matrixContent[matrixX[codewordBitsNumber] & 0xFF][matrixY[codewordBitsNumber] & 0xFF] =
                        (sbyte)((255 * (codeword_i & 1)) ^ maskArray[codewordBitsNumber]);

                    codeword_i = (sbyte)SystemUtils.URShift((codeword_i & 0xFF), 1);
                }
            }

            for (int matrixRemain = matrixRemainBit[this.qrcodeVersion]; matrixRemain > 0; matrixRemain--)
            {
                int remainBitTemp = matrixRemain + (maxCodewords * 8) - 1;
                matrixContent[matrixX[remainBitTemp] & 0xFF][matrixY[remainBitTemp] & 0xFF] =
                    (sbyte)(255 ^ maskArray[remainBitTemp]);
            }

            /* --- mask select --- */
            sbyte maskNumber = selectMask(matrixContent, matrixRemainBit[this.qrcodeVersion] + maxCodewords * 8);
            sbyte maskContent = (sbyte)(1 << maskNumber);

            /* --- format information --- */
            sbyte formatInformationValue = (sbyte)(ec << 3 | maskNumber);

            string[] formatInformationArray = new[]
                {
                    "101010000010010", "101000100100101", "101111001111100", "101101101001011", "100010111111001", 
                    "100000011001110", "100111110010111", "100101010100000", "111011111000100", "111001011110011", 
                    "111110110101010", "111100010011101", "110011000101111", "110001100011000", "110110001000001", 
                    "110100101110110", "001011010001001", "001001110111110", "001110011100111", "001100111010000", 
                    "000011101100010", "000001001010101", "000110100001100", "000100000111011", "011010101011111", 
                    "011000001101000", "011111100110001", "011101000000110", "010010010110100", "010000110000011", 
                    "010111011011010", "010101111101101"
                };

            for (int i = 0; i < 15; i++)
            {
                sbyte content = SByte.Parse(formatInformationArray[formatInformationValue].Substring(i, (i + 1) - i));

                matrixContent[formatInformationX1[i] & 0xFF][formatInformationY1[i] & 0xFF] = (sbyte)(content * 255);
                matrixContent[formatInformationX2[i] & 0xFF][formatInformationY2[i] & 0xFF] = (sbyte)(content * 255);
            }

            bool[][] out_Renamed = new bool[modules1Side][];
            for (int i3 = 0; i3 < modules1Side; i3++)
            {
                out_Renamed[i3] = new bool[modules1Side];
            }

            int c = 0;
            for (int i = 0; i < modules1Side; i++)
            {
                for (int j = 0; j < modules1Side; j++)
                {
                    if ((matrixContent[j][i] & maskContent) != 0 || frameData[c] == (char)49)
                    {
                        out_Renamed[j][i] = true;
                    }
                    else
                    {
                        out_Renamed[j][i] = false;
                    }

                    c++;
                }

                c++;
            }

            return out_Renamed;
        }

        /// <summary>
        /// The cal structureappend parity.
        /// </summary>
        /// <param name="originaldata">
        /// The originaldata.
        /// </param>
        /// <returns>
        /// The cal structureappend parity.
        /// </returns>
        public virtual int calStructureappendParity(sbyte[] originaldata)
        {
            int i = 0;
            int structureappendParity;

            int originaldataLength = originaldata.Length;

            if (originaldataLength > 1)
            {
                structureappendParity = 0;
                while (i < originaldataLength)
                {
                    structureappendParity = structureappendParity ^ (originaldata[i] & 0xFF);
                    i++;
                }
            }
            else
            {
                structureappendParity = -1;
            }

            return structureappendParity;
        }

        /// <summary>
        /// The set structureappend.
        /// </summary>
        /// <param name="m">
        /// The m.
        /// </param>
        /// <param name="n">
        /// The n.
        /// </param>
        /// <param name="p">
        /// The p.
        /// </param>
        public virtual void setStructureappend(int m, int n, int p)
        {
            if (n > 1 && n <= 16 && m > 0 && m <= 16 && p >= 0 && p <= 255)
            {
                this.qrcodeStructureappendM = m;
                this.qrcodeStructureappendN = n;
                this.qrcodeStructureappendParity = p;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The calculate byte array bits.
        /// </summary>
        /// <param name="xa">
        /// The xa.
        /// </param>
        /// <param name="xb">
        /// The xb.
        /// </param>
        /// <param name="ind">
        /// The ind.
        /// </param>
        /// <returns>
        /// </returns>
        private static sbyte[] calculateByteArrayBits(sbyte[] xa, sbyte[] xb, string ind)
        {
            sbyte[] xl;
            sbyte[] xs;

            if (xa.Length > xb.Length)
            {
                xl = new sbyte[xa.Length];
                xa.CopyTo(xl, 0);
                xs = new sbyte[xb.Length];
                xb.CopyTo(xs, 0);
            }
            else
            {
                xl = new sbyte[xb.Length];
                xb.CopyTo(xl, 0);
                xs = new sbyte[xa.Length];
                xa.CopyTo(xs, 0);
            }

            int ll = xl.Length;
            int ls = xs.Length;
            sbyte[] res = new sbyte[ll];

            for (int i = 0; i < ll; i++)
            {
                if (i < ls)
                {
                    if (ind == "xor")
                    {
                        res[i] = (sbyte)(xl[i] ^ xs[i]);
                    }
                    else
                    {
                        res[i] = (sbyte)(xl[i] | xs[i]);
                    }
                }
                else
                {
                    res[i] = xl[i];
                }
            }

            return res;
        }

        /// <summary>
        /// The calculate rsecc.
        /// </summary>
        /// <param name="codewords">
        /// The codewords.
        /// </param>
        /// <param name="rsEccCodewords">
        /// The rs ecc codewords.
        /// </param>
        /// <param name="rsBlockOrder">
        /// The rs block order.
        /// </param>
        /// <param name="maxDataCodewords">
        /// The max data codewords.
        /// </param>
        /// <param name="maxCodewords">
        /// The max codewords.
        /// </param>
        /// <returns>
        /// </returns>
        private static sbyte[] calculateRSECC(
            sbyte[] codewords, sbyte rsEccCodewords, sbyte[] rsBlockOrder, int maxDataCodewords, int maxCodewords)
        {
            sbyte[][] rsCalTableArray = new sbyte[256][];
            for (int i = 0; i < 256; i++)
            {
                rsCalTableArray[i] = new sbyte[rsEccCodewords];
            }

            try
            {
                // String filename = QRCODE_DATA_PATH + @"\rsc" + rsEccCodewords.ToString() + ".dat";
                // StreamReader reader = new StreamReader(filename);
                string fileName = "rsc" + rsEccCodewords;
                MemoryStream memoryStream = new MemoryStream((byte[])Resources.ResourceManager.GetObject(fileName));
                BufferedStream bis = new BufferedStream(memoryStream);
                for (int i = 0; i < 256; i++)
                {
                    SystemUtils.ReadInput(bis, rsCalTableArray[i], 0, rsCalTableArray[i].Length);
                }

                bis.Close();

                // reader.Close();
                memoryStream.Close();
            }
            catch (Exception e)
            {
                SystemUtils.WriteStackTrace(e, Console.Error);
            }

            /* ---- RS-ECC prepare */
            int j = 0;
            int rsBlockNumber = 0;

            sbyte[][] rsTemp = new sbyte[rsBlockOrder.Length][];

            /*
            for (int i = 0; i < rsBlockOrder.Length; i++)
            {
                rsTemp[i] = new sbyte[rsBlockOrder.Length];
            }
            */
            sbyte[] res = new sbyte[maxCodewords];
            Array.Copy(codewords, 0, res, 0, codewords.Length);

            int i2 = 0;
            while (i2 < rsBlockOrder.Length)
            {
                rsTemp[i2] = new sbyte[(rsBlockOrder[i2] & 0xFF) - rsEccCodewords];
                i2++;
            }

            i2 = 0;
            while (i2 < maxDataCodewords)
            {
                rsTemp[rsBlockNumber][j] = codewords[i2];
                j++;
                if (j >= (rsBlockOrder[rsBlockNumber] & 0xFF) - rsEccCodewords)
                {
                    j = 0;
                    rsBlockNumber++;
                }

                i2++;
            }

            /* ---  RS-ECC main --- */
            rsBlockNumber = 0;
            while (rsBlockNumber < rsBlockOrder.Length)
            {
                sbyte[] rsTempData = new sbyte[rsTemp[rsBlockNumber].Length];
                rsTemp[rsBlockNumber].CopyTo(rsTempData, 0);

                int rsCodewords = rsBlockOrder[rsBlockNumber] & 0xFF;
                int rsDataCodewords = rsCodewords - rsEccCodewords;

                j = rsDataCodewords;
                while (j > 0)
                {
                    sbyte first = rsTempData[0];
                    if (first != 0)
                    {
                        sbyte[] leftChr = new sbyte[rsTempData.Length - 1];
                        Array.Copy(rsTempData, 1, leftChr, 0, rsTempData.Length - 1);
                        sbyte[] cal = rsCalTableArray[first & 0xFF];
                        rsTempData = calculateByteArrayBits(leftChr, cal, "xor");
                    }
                    else
                    {
                        if (rsEccCodewords < rsTempData.Length)
                        {
                            sbyte[] rsTempNew = new sbyte[rsTempData.Length - 1];
                            Array.Copy(rsTempData, 1, rsTempNew, 0, rsTempData.Length - 1);
                            rsTempData = new sbyte[rsTempNew.Length];
                            rsTempNew.CopyTo(rsTempData, 0);
                        }
                        else
                        {
                            sbyte[] rsTempNew = new sbyte[rsEccCodewords];
                            Array.Copy(rsTempData, 1, rsTempNew, 0, rsTempData.Length - 1);
                            rsTempNew[rsEccCodewords - 1] = 0;
                            rsTempData = new sbyte[rsTempNew.Length];
                            rsTempNew.CopyTo(rsTempData, 0);
                        }
                    }

                    j--;
                }

                Array.Copy(rsTempData, 0, res, codewords.Length + rsBlockNumber * rsEccCodewords, (byte)rsEccCodewords);
                rsBlockNumber++;
            }

            return res;
        }

        /// <summary>
        /// The divide data by 8 bits.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="bits">
        /// The bits.
        /// </param>
        /// <param name="maxDataCodewords">
        /// The max data codewords.
        /// </param>
        /// <returns>
        /// </returns>
        private static sbyte[] divideDataBy8Bits(int[] data, sbyte[] bits, int maxDataCodewords)
        {
            /* divide Data By 8bit and add padding char */
            int l1 = bits.Length;
            int codewordsCounter = 0;
            int remainingBits = 8;
            int max = 0;
            bool flag;

            if (l1 != data.Length)
            {
            }

            for (int i = 0; i < l1; i++)
            {
                max += bits[i];
            }

            int l2 = (max - 1) / 8 + 1;
            sbyte[] codewords = new sbyte[maxDataCodewords];
            for (int i = 0; i < l2; i++)
            {
                codewords[i] = 0;
            }

            for (int i = 0; i < l1; i++)
            {
                int buffer = data[i];
                int bufferBits = bits[i];
                flag = true;

                if (bufferBits == 0)
                {
                    break;
                }

                while (flag)
                {
                    if (remainingBits > bufferBits)
                    {
                        codewords[codewordsCounter] = (sbyte)((codewords[codewordsCounter] << bufferBits) | buffer);
                        remainingBits -= bufferBits;
                        flag = false;
                    }
                    else
                    {
                        bufferBits -= remainingBits;
                        codewords[codewordsCounter] =
                            (sbyte)((codewords[codewordsCounter] << remainingBits) | (buffer >> bufferBits));

                        if (bufferBits == 0)
                        {
                            flag = false;
                        }
                        else
                        {
                            buffer = buffer & ((1 << bufferBits) - 1);
                        }

                        codewordsCounter++;
                        remainingBits = 8;
                    }
                }
            }

            if (remainingBits != 8)
            {
                codewords[codewordsCounter] = (sbyte)(codewords[codewordsCounter] << remainingBits);
            }
            else
            {
                codewordsCounter--;
            }

            if (codewordsCounter < maxDataCodewords - 1)
            {
                flag = true;
                while (codewordsCounter < maxDataCodewords - 1)
                {
                    codewordsCounter++;
                    if (flag)
                    {
                        codewords[codewordsCounter] = -20;
                    }
                    else
                    {
                        codewords[codewordsCounter] = 17;
                    }

                    flag = !flag;
                }
            }

            return codewords;
        }

        /// <summary>
        /// The select mask.
        /// </summary>
        /// <param name="matrixContent">
        /// The matrix content.
        /// </param>
        /// <param name="maxCodewordsBitWithRemain">
        /// The max codewords bit with remain.
        /// </param>
        /// <returns>
        /// The select mask.
        /// </returns>
        private static sbyte selectMask(sbyte[][] matrixContent, int maxCodewordsBitWithRemain)
        {
            int l = matrixContent.Length;
            int[] d1 = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] d2 = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] d3 = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] d4 = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            int d2And = 0;
            int d2Or = 0;
            int[] d4Counter = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            for (int y = 0; y < l; y++)
            {
                int[] xData = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                int[] yData = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                bool[] xD1Flag = new[] { false, false, false, false, false, false, false, false };
                bool[] yD1Flag = new[] { false, false, false, false, false, false, false, false };

                for (int x = 0; x < l; x++)
                {
                    if (x > 0 && y > 0)
                    {
                        d2And = matrixContent[x][y] & matrixContent[x - 1][y] & matrixContent[x][y - 1] &
                                matrixContent[x - 1][y - 1] & 0xFF;

                        d2Or = (matrixContent[x][y] & 0xFF) | (matrixContent[x - 1][y] & 0xFF) |
                               (matrixContent[x][y - 1] & 0xFF) | (matrixContent[x - 1][y - 1] & 0xFF);
                    }

                    for (int maskNumber = 0; maskNumber < 8; maskNumber++)
                    {
                        xData[maskNumber] = ((xData[maskNumber] & 63) << 1) |
                                            (SystemUtils.URShift((matrixContent[x][y] & 0xFF), maskNumber) & 1);

                        yData[maskNumber] = ((yData[maskNumber] & 63) << 1) |
                                            (SystemUtils.URShift((matrixContent[y][x] & 0xFF), maskNumber) & 1);

                        if ((matrixContent[x][y] & (1 << maskNumber)) != 0)
                        {
                            d4Counter[maskNumber]++;
                        }

                        if (xData[maskNumber] == 93)
                        {
                            d3[maskNumber] += 40;
                        }

                        if (yData[maskNumber] == 93)
                        {
                            d3[maskNumber] += 40;
                        }

                        if (x > 0 && y > 0)
                        {
                            if (((d2And & 1) != 0) || ((d2Or & 1) == 0))
                            {
                                d2[maskNumber] += 3;
                            }

                            d2And = d2And >> 1;
                            d2Or = d2Or >> 1;
                        }

                        if (((xData[maskNumber] & 0x1F) == 0) || ((xData[maskNumber] & 0x1F) == 0x1F))
                        {
                            if (x > 3)
                            {
                                if (xD1Flag[maskNumber])
                                {
                                    d1[maskNumber]++;
                                }
                                else
                                {
                                    d1[maskNumber] += 3;
                                    xD1Flag[maskNumber] = true;
                                }
                            }
                        }
                        else
                        {
                            xD1Flag[maskNumber] = false;
                        }

                        if (((yData[maskNumber] & 0x1F) == 0) || ((yData[maskNumber] & 0x1F) == 0x1F))
                        {
                            if (x > 3)
                            {
                                if (yD1Flag[maskNumber])
                                {
                                    d1[maskNumber]++;
                                }
                                else
                                {
                                    d1[maskNumber] += 3;
                                    yD1Flag[maskNumber] = true;
                                }
                            }
                        }
                        else
                        {
                            yD1Flag[maskNumber] = false;
                        }
                    }
                }
            }

            int minValue = 0;
            sbyte res = 0;
            int[] d4Value = new[] { 90, 80, 70, 60, 50, 40, 30, 20, 10, 0, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 90 };
            for (int maskNumber = 0; maskNumber < 8; maskNumber++)
            {
                d4[maskNumber] = d4Value[(20 * d4Counter[maskNumber]) / maxCodewordsBitWithRemain];

                int demerit = d1[maskNumber] + d2[maskNumber] + d3[maskNumber] + d4[maskNumber];

                if (demerit < minValue || maskNumber == 0)
                {
                    res = (sbyte)maskNumber;
                    minValue = demerit;
                }
            }

            return res;
        }

        #endregion
    }
}