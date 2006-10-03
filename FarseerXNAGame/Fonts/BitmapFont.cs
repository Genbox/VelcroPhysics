// BitmapFont.cs
// Bitmap Font class for XNA
// Copyright 2006 Microsoft Corp.
// Gary Kacmarcik (garykac@microsoft.com)
// Revision: 2006-Sep-16

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerXNAGame.Components {
    /// <summary>
    /// Bitmap font class for XNA
    /// </summary>
    public class BitmapFont {
        public enum TextAlignment {
            Left,
            Center,
            Right,
        }

        /// <summary>
        /// Save font rendering state so that it can be saved/restored
        /// </summary>
        public struct SaveStateInfo {
            public bool fKern;
            public float fpDepth;
            public TextAlignment align;
            public Vector2 vPen;
            public Color color;
        }

        enum GlyphFlags {
            None = 0,
            ForceWhite = 1,		// force the drawing color for this glyph to be white.
        }

        /// <summary>
        /// Info for each glyph in the font - where to find the glyph image and 
        /// other properties
        /// </summary>
        struct GlyphInfo {
            public ushort nBitmapID;
            public byte pxLocX;
            public byte pxLocY;
            public byte pxWidth;
            public byte pxHeight;
            public byte pxAdvanceWidth;
            public sbyte pxLeftSideBearing;
            public GlyphFlags nFlags;
        }

        /// <summary>
        /// Info for each font bitmap
        /// </summary>
        struct BitmapInfo {
            public string strFilename;
            public int nX, nY;
        }

        private SpriteBatch m_sb;
        private SpriteBatch m_sbOverride;
        private string m_strName;
        private string m_strPath;
        private string m_strFilename;
        private bool m_fLoadFromResource;
        private Dictionary<int, BitmapInfo> m_dictBitmapID2BitmapInfo;
        private Dictionary<int, Texture2D> m_dictBitmapID2Texture;
        private Dictionary<char, GlyphInfo> m_dictUnicode2GlyphInfo;
        private Dictionary<char, Dictionary<char, sbyte>> m_dictKern;
        private int m_nBase = 0;
        private int m_nHeight = 0;
        private float m_fpDepth = 0.0f;
        private TextAlignment m_eAlign = TextAlignment.Left;
        private static Dictionary<string, BitmapFont> m_dictBitmapFonts = new Dictionary<string, BitmapFont>();

        /// <summary>
        /// Create a new font from the info in the specified font descriptor (XML) file
        /// </summary>
        /// <param name="strFontFilename">Font descriptor file (.xml)</param>
        public BitmapFont(string strFontFilename) {
            m_sb = null;
            m_sbOverride = null;

            m_dictBitmapID2BitmapInfo = new Dictionary<int, BitmapInfo>();
            m_dictBitmapID2Texture = new Dictionary<int, Texture2D>();

            m_dictUnicode2GlyphInfo = new Dictionary<char, GlyphInfo>();
            m_dictKern = new Dictionary<char, Dictionary<char, sbyte>>();

            if (System.IO.File.Exists(strFontFilename)) {
                // all files mentioned in the font descriptor file are relative to the parent directory of the font file.
                // record the path to this directory.
                m_strPath = System.IO.Path.GetDirectoryName(strFontFilename) + @"/";
                m_strFilename = strFontFilename;
                m_fLoadFromResource = false;

                XmlDocument xd = new XmlDocument();
                xd.Load(strFontFilename);
                LoadFontXML(xd.ChildNodes);
            }
            else {
                // look in the assembly resources
                bool fFoundResource = false;
                Assembly a = Assembly.GetExecutingAssembly();
                System.IO.Stream s = a.GetManifestResourceStream(strFontFilename);
                if (s != null) {
                    // calc the resource path to this font (strip off <fontname>.xml)
                    string[] aPath = strFontFilename.Split('.');
                    m_strPath = "";
                    for (int i = 0; i < aPath.Length - 2; i++)
                        m_strPath += aPath[i] + ".";
                    m_strFilename = strFontFilename;
                    m_fLoadFromResource = true;
                    fFoundResource = true;

                    XmlDocument xd = new XmlDocument();
                    xd.Load(s);
                    LoadFontXML(xd.ChildNodes);
                }
                if (!fFoundResource)
                    throw new System.Exception(String.Format("Unable to find font named '{0}'.", strFontFilename));
            }

            // if the font doesn't define a name, create one from the filename
            if (m_strName == "")
                m_strName = System.IO.Path.GetFileNameWithoutExtension(strFontFilename);

            // add this font to the list of active fonts
            m_dictBitmapFonts.Add(m_strName, this);
        }

        /// <summary>
        /// Destructor for BitmapFont. Remove font from list of active fonts.
        /// </summary>
        ~BitmapFont() {
            Dispose();
            m_dictBitmapFonts.Remove(m_strName);
        }

        /// <summary>
        /// Dispose of all of the non-managed resources for this object
        /// </summary>
        public void Dispose() {
            m_sb.Dispose();
            foreach (int key in m_dictBitmapID2Texture.Keys)
                m_dictBitmapID2Texture[key].Dispose();
        }

        /// <summary>
        /// Reset the font when the device has changed
        /// </summary>
        /// <param name="device">The new device</param>
        public void Reset(GraphicsDevice device) {
            Assembly a = Assembly.GetExecutingAssembly();
            m_sb = new SpriteBatch(device);
            foreach (KeyValuePair<int, BitmapInfo> kv in m_dictBitmapID2BitmapInfo) {
                Texture2D tex;
                TextureCreationParameters tcp = TextureCreationParameters.Default;
                tcp.Width = kv.Value.nX;
                tcp.Height = kv.Value.nY;
                if (m_fLoadFromResource) {
                    System.IO.Stream s = a.GetManifestResourceStream(m_strPath + kv.Value.strFilename);
                    tex = Texture2D.FromFile(device, s, tcp) as Texture2D;
                }
                else
                    tex = Texture2D.FromFile(device, m_strPath + kv.Value.strFilename, tcp);


                m_dictBitmapID2Texture[kv.Key] = tex;
            }
        }

        /// <summary>
        /// The name of this font
        /// </summary>
        public string Name {
            get { return m_strName; }
        }

        /// <summary>
        /// The name of the font file
        /// </summary>
        public string Filename {
            get { return m_strFilename; }
        }

        /// <summary>
        /// Should we kern adjacent characters?
        /// </summary>
        private bool m_fKern = true;

        /// <summary>
        /// Enable/disable kerning
        /// </summary>
        public bool KernEnable {
            get { return m_fKern; }
            set { m_fKern = value; }
        }

        /// <summary>
        /// Distance from top of font to the baseline
        /// </summary>
        public int Baseline {
            get { return m_nBase; }
        }

        /// <summary>
        /// Distance from top to bottom of the font
        /// </summary>
        public int LineHeight {
            get { return m_nHeight; }
        }

        /// <summary>
        /// The depth at which to draw the font
        /// </summary>
        public float Depth {
            get { return m_fpDepth; }
            set { m_fpDepth = value; }
        }

        /// <summary>
        /// The text alignment. This is only used by the TextBox routines
        /// </summary>
        public TextAlignment Alignment {
            get { return m_eAlign; }
            set { m_eAlign = value; }
        }

        /// <summary>
        /// Calculate the width of the given string.
        /// </summary>
        /// <param name="format">String format</param>
        /// <param name="args">String format arguments</param>
        /// <returns>Width (in pixels) of the string</returns>
        public int MeasureString(string format, params object[] args) {
            string str = string.Format(format, args);
            int pxWidth = 0;
            char cLast = '\0';

            foreach (char c in str) {
                if (!m_dictUnicode2GlyphInfo.ContainsKey(c)) {
                    //TODO: print out undefined char glyph
                    continue;
                }

                GlyphInfo ginfo = m_dictUnicode2GlyphInfo[c];

                // if kerning is enabled, get the kern adjustment for this char pair
                if (m_fKern) {
                    pxWidth += CalcKern(cLast, c);
                    cLast = c;
                }

                // update the string width
                pxWidth += ginfo.pxAdvanceWidth;
            }

            return pxWidth;
        }

        /// <summary>
        /// Calculate the number of characters that fit in the given width.
        /// </summary>
        /// <param name="pxMaxWidth">Maximum string width</param>
        /// <param name="str">String</param>
        /// <param name="nChars">Number of characters that fit</param>
        /// <param name="pxWidth">Width of substring</param>
        private void CountCharWidth(int pxMaxWidth, string str, out int nChars, out int pxWidth) {
            int nLastWordBreak = 0;
            int pxLastWordBreakWidth = 0;
            int pxLastWidth = 0;
            char cLast = '\0';

            nChars = 0;
            pxWidth = 0;

            foreach (char c in str) {
                // if this is a newline, then return. the width is set correctly
                if (c == '\n') {
                    nChars++;
                    return;
                }

                if (!m_dictUnicode2GlyphInfo.ContainsKey(c)) {
                    //TODO: print out undefined char glyph
                    continue;
                }

                GlyphInfo ginfo = m_dictUnicode2GlyphInfo[c];

                // if kerning is enabled, get the kern adjustment for this char pair
                if (m_fKern) {
                    int pxKern = CalcKern(cLast, c);
                    pxWidth += pxKern;
                    cLast = c;
                }

                // update the string width and char count
                pxLastWidth = pxWidth;
                pxWidth += ginfo.pxAdvanceWidth;
                nChars++;

                // record the end of the previous word if this is a whitespace char
                if (Char.IsWhiteSpace(c)) {
                    nLastWordBreak = nChars;			// include space in char count
                    pxLastWordBreakWidth = pxLastWidth;	// don't include space in width
                }

                // if we've exceeded the max, then return the chars up to the last complete word
                if (pxWidth > pxMaxWidth) {
                    pxWidth = pxLastWordBreakWidth;
                    if (pxWidth == 0) {
                        // fallback to last char if we haven't seen a complete word
                        pxWidth = pxLastWidth;
                        nChars--;
                    }
                    else
                        nChars = nLastWordBreak;
                    return;
                }
            }
        }

        /// <summary>
        /// Current pen position
        /// </summary>
        private Vector2 m_vPen = new Vector2(0, 0);

        /// <summary>
        /// Current pen position
        /// </summary>
        public Vector2 Pen {
            get { return m_vPen; }
            set { m_vPen = value; }
        }

        /// <summary>
        /// Set the current pen position
        /// </summary>
        /// <param name="x">X-coord</param>
        /// <param name="y">Y-coord</param>
        public void SetPen(int x, int y) {
            m_vPen = new Vector2(x, y);
        }


        /// <summary>
        /// Current color used for drawing text
        /// </summary>
        private Color m_color = Color.White;

        /// <summary>
        /// Current color used for drawing text
        /// </summary>
        public Color TextColor {
            get { return m_color; }
            set { m_color = value; }
        }


        /// <summary>
        /// Draw the given string at (x,y).
        /// The text color is inherited from the last draw command (default=White).
        /// </summary>
        /// <param name="x">X-coord</param>
        /// <param name="y">Y-coord</param>
        /// <param name="format">String format</param>
        /// <param name="args">String format args</param>
        /// <returns>Width of string (in pixels)</returns>
        public int DrawString(int x, int y, string format, params object[] args) {
            Vector2 v = new Vector2(x, y);
            return DrawString(v, m_color, format, args);
        }

        /// <summary>
        /// Draw the given string at (x,y) using the specified color
        /// </summary>
        /// <param name="x">X-coord</param>
        /// <param name="y">Y-coord</param>
        /// <param name="color">Text color</param>
        /// <param name="format">String format</param>
        /// <param name="args">String format args</param>
        /// <returns>Width of string (in pixels)</returns>
        public int DrawString(int x, int y, Color color, string format, params object[] args) {
            Vector2 v = new Vector2(x, y);
            return DrawString(v, color, format, args);
        }

        /// <summary>
        /// Draw the given string using the specified color.
        /// The text drawing location is immediately after the last drawn text (default=0,0).
        /// </summary>
        /// <param name="color">Text color</param>
        /// <param name="format">String format</param>
        /// <param name="args">String format args</param>
        /// <returns>Width of string (in pixels)</returns>
        public int DrawString(Color color, string format, params object[] args) {
            return DrawString(m_vPen, color, format, args);
        }

        /// <summary>
        /// Draw the given string at (x,y).
        /// The text drawing location is immediately after the last drawn text (default=0,0).
        /// The text color is inherited from the last draw command (default=White).
        /// </summary>
        /// <param name="format">String format</param>
        /// <param name="args">String format args</param>
        /// <returns>Width of string (in pixels)</returns>
        public int DrawString(string format, params object[] args) {
            return DrawString(m_vPen, m_color, format, args);
        }

        /// <summary>
        /// Draw the given string at vOrigin using the specified color
        /// </summary>
        /// <param name="vAt">(x,y) coord</param>
        /// <param name="cText">Text color</param>
        /// <param name="strFormat">String format</param>
        /// <param name="args">String format args</param>
        /// <returns>Width of string (in pixels)</returns>
        public int DrawString(Vector2 vAt, Color cText, string strFormat, params object[] args) {
            string str = string.Format(strFormat, args);

            return DrawString_internal(vAt, cText, str);
        }

        /// <summary>
        /// Private version of DrawString that expects the string to be formatted already
        /// </summary>
        /// <param name="vAt">(x,y) coord</param>
        /// <param name="cText">Text color</param>
        /// <param name="str">String</param>
        /// <returns>Width of string (in pixels)</returns>
        private int DrawString_internal(Vector2 vAt, Color cText, string str) {
            Vector2 vOrigin = new Vector2(0, 0);
            int pxWidth = 0;
            char cLast = '\0';

            // are we using our local SpriteBatch, or an override?
            bool fSBOverride = (m_sbOverride != null);
            SpriteBatch sb = (fSBOverride ? m_sbOverride : m_sb);

            if (!fSBOverride)
                sb.Begin(SpriteBlendMode.AlphaBlend);

            // draw each character in the string
            foreach (char c in str) {
                if (!m_dictUnicode2GlyphInfo.ContainsKey(c)) {
                    //TODO: print out undefined char glyph
                    continue;
                }

                GlyphInfo ginfo = m_dictUnicode2GlyphInfo[c];

                // if kerning is enabled, get the kern adjustment for this char pair
                if (m_fKern) {
                    int pxKern = CalcKern(cLast, c);
                    vAt.X += pxKern;
                    pxWidth += pxKern;
                    cLast = c;
                }

                // draw the glyph
                vAt.X += ginfo.pxLeftSideBearing;
                if (ginfo.pxWidth != 0 && ginfo.pxHeight != 0) {
                    Rectangle rSource = new Rectangle(ginfo.pxLocX, ginfo.pxLocY, ginfo.pxWidth, ginfo.pxHeight);
                    Color color = (((ginfo.nFlags & GlyphFlags.ForceWhite) != 0) ? Color.White : cText);
                    sb.Draw(m_dictBitmapID2Texture[ginfo.nBitmapID], vAt, rSource, color, 0.0f, vOrigin, 1.0f, SpriteEffects.None, m_fpDepth);
                }

                // update the string width and advance the pen to the next drawing position
                pxWidth += ginfo.pxAdvanceWidth;
                vAt.X += ginfo.pxAdvanceWidth - ginfo.pxLeftSideBearing;
            }

            if (!fSBOverride)
                sb.End();

            // record final pen position and color
            m_vPen = vAt;
            m_color = cText;

            return pxWidth;
        }

        /// <summary>
        /// Get the kern value for the given pair of characters
        /// </summary>
        /// <param name="chLeft">Left character</param>
        /// <param name="chRight">Right character</param>
        /// <returns>Amount to kern (in pixels)</returns>
        private int CalcKern(char chLeft, char chRight) {
            if (m_dictKern.ContainsKey(chLeft)) {
                Dictionary<char, sbyte> kern2 = m_dictKern[chLeft];
                if (kern2.ContainsKey(chRight))
                    return kern2[chRight];
            }
            return 0;
        }

        /// <summary>
        /// Draw text formatted to fit in the specified rectangle
        /// </summary>
        /// <param name="r">The rectangle to fit the text</param>
        /// <param name="cText">Text color</param>
        /// <param name="strFormat">String format</param>
        /// <param name="args">String format args</param>
        public void TextBox(Rectangle r, Color cText, string strFormat, params object[] args) {
            string str = string.Format(strFormat, args);

            int nChars;
            int pxWidth;
            Vector2 vAt = new Vector2(r.Left, r.Top);

            while (str.Length != 0) {
                // stop drawing if there isn't room for this line
                if (vAt.Y + m_nHeight > r.Bottom)
                    return;

                CountCharWidth(r.Width, str, out nChars, out pxWidth);

                switch (m_eAlign) {
                    case TextAlignment.Left:
                        vAt.X = r.Left;
                        break;
                    case TextAlignment.Center:
                        vAt.X = r.Left + ((r.Width - pxWidth) / 2);
                        break;
                    case TextAlignment.Right:
                        vAt.X = r.Left + (r.Width - pxWidth);
                        break;
                }
                DrawString_internal(vAt, cText, str.Substring(0, nChars));
                str = str.Substring(nChars);
                vAt.Y += m_nHeight;
            }
        }

        /// <summary>
        /// Save the current font rendering state
        /// </summary>
        /// <param name="bfss">Struct to store the save state</param>
        public void SaveState(out SaveStateInfo bfss) {
            bfss.fKern = m_fKern;
            bfss.fpDepth = m_fpDepth;
            bfss.align = m_eAlign;
            bfss.vPen = m_vPen;
            bfss.color = m_color;
        }

        /// <summary>
        /// Restore the font rendering state
        /// </summary>
        /// <param name="bfss">Previously saved font state</param>
        public void RestoreState(SaveStateInfo bfss) {
            m_fKern = bfss.fKern;
            m_fpDepth = bfss.fpDepth;
            m_eAlign = bfss.align;
            m_vPen = bfss.vPen;
            m_color = bfss.color;
        }

        /// <summary>
        /// Temporarily override the font's SpriteBatch with the given SpriteBatch.
        /// </summary>
        /// <param name="sb">The new SpriteBatch (or null to reset)</param>
        /// <remarks>
        /// When drawing text using the SpriteBatch override, Begin/End will not be called on the SpriteBatch.
        /// Use null to reset back to the font's original SpriteBatch.
        /// </remarks>
        public void SpriteBatchOverride(SpriteBatch sb) {
            m_sbOverride = sb;
        }

        /// <summary>
        /// Return the font associated with the given name.
        /// </summary>
        /// <param name="strName">Name of the font</param>
        /// <returns>The font</returns>
        public static BitmapFont GetNamedFont(string strName) {
            return m_dictBitmapFonts[strName];
        }

        #region Load Font from XML

        /// <summary>
        /// Load the font data from an XML font descriptor file
        /// </summary>
        /// <param name="xnl">XML node list containing the entire font descriptor file</param>
        private void LoadFontXML(XmlNodeList xnl) {
            foreach (XmlNode xn in xnl) {
                if (xn.Name == "font") {
                    m_strName = GetXMLAttribute(xn, "name");
                    m_nBase = Int32.Parse(GetXMLAttribute(xn, "base"));
                    m_nHeight = Int32.Parse(GetXMLAttribute(xn, "height"));

                    LoadFontXML_font(xn.ChildNodes);
                }
            }
        }

        /// <summary>
        /// Load the data from the "font" node
        /// </summary>
        /// <param name="xnl">XML node list containing the "font" node's children</param>
        private void LoadFontXML_font(XmlNodeList xnl) {
            foreach (XmlNode xn in xnl) {
                if (xn.Name == "bitmaps")
                    LoadFontXML_bitmaps(xn.ChildNodes);
                if (xn.Name == "glyphs")
                    LoadFontXML_glyphs(xn.ChildNodes);
                if (xn.Name == "kernpairs")
                    LoadFontXML_kernpairs(xn.ChildNodes);
            }
        }

        /// <summary>
        /// Load the data from the "bitmaps" node
        /// </summary>
        /// <param name="xnl">XML node list containing the "bitmaps" node's children</param>
        private void LoadFontXML_bitmaps(XmlNodeList xnl) {
            foreach (XmlNode xn in xnl) {
                if (xn.Name == "bitmap") {
                    string strID = GetXMLAttribute(xn, "id");
                    string strFilename = GetXMLAttribute(xn, "name");
                    string strSize = GetXMLAttribute(xn, "size");
                    string[] aSize = strSize.Split('x');

                    BitmapInfo bminfo;
                    bminfo.strFilename = strFilename;
                    bminfo.nX = Int32.Parse(aSize[0]);
                    bminfo.nY = Int32.Parse(aSize[1]);

                    m_dictBitmapID2BitmapInfo[Int32.Parse(strID)] = bminfo;
                }
            }
        }

        /// <summary>
        /// Load the data from the "glyphs" node
        /// </summary>
        /// <param name="xnl">XML node list containing the "glyphs" node's children</param>
        private void LoadFontXML_glyphs(XmlNodeList xnl) {
            foreach (XmlNode xn in xnl) {
                if (xn.Name == "glyph") {
                    string strChar = GetXMLAttribute(xn, "ch");
                    string strBitmapID = GetXMLAttribute(xn, "bm");
                    string strLoc = GetXMLAttribute(xn, "loc");
                    string strSize = GetXMLAttribute(xn, "size");
                    string strAW = GetXMLAttribute(xn, "aw");
                    string strLSB = GetXMLAttribute(xn, "lsb");
                    string strForceWhite = GetXMLAttribute(xn, "forcewhite");

                    if (strLoc == "")
                        strLoc = GetXMLAttribute(xn, "origin");	// obsolete - use loc instead

                    string[] aLoc = strLoc.Split(',');
                    string[] aSize = strSize.Split('x');

                    GlyphInfo ginfo = new GlyphInfo();
                    ginfo.nBitmapID = UInt16.Parse(strBitmapID);
                    ginfo.pxLocX = Byte.Parse(aLoc[0]);
                    ginfo.pxLocY = Byte.Parse(aLoc[1]);
                    ginfo.pxWidth = Byte.Parse(aSize[0]);
                    ginfo.pxHeight = Byte.Parse(aSize[1]);
                    ginfo.pxAdvanceWidth = Byte.Parse(strAW);
                    ginfo.pxLeftSideBearing = SByte.Parse(strLSB);
                    ginfo.nFlags = 0;
                    ginfo.nFlags |= (strForceWhite == "true" ? GlyphFlags.ForceWhite : GlyphFlags.None);

                    m_dictUnicode2GlyphInfo[strChar[0]] = ginfo;
                }
            }
        }

        /// <summary>
        /// Load the data from the "kernpairs" node
        /// </summary>
        /// <param name="xnl">XML node list containing the "kernpairs" node's children</param>
        private void LoadFontXML_kernpairs(XmlNodeList xnl) {
            foreach (XmlNode xn in xnl) {
                if (xn.Name == "kernpair") {
                    string strLeft = GetXMLAttribute(xn, "left");
                    string strRight = GetXMLAttribute(xn, "right");
                    string strAdjust = GetXMLAttribute(xn, "adjust");

                    char chLeft = strLeft[0];
                    char chRight = strRight[0];

                    // create a kern dict for the left char if needed
                    if (!m_dictKern.ContainsKey(chLeft))
                        m_dictKern[chLeft] = new Dictionary<char, sbyte>();

                    // add the right char to the left char's kern dict
                    Dictionary<char, sbyte> kern2 = m_dictKern[chLeft];
                    kern2[chRight] = SByte.Parse(strAdjust);
                }
            }
        }

        /// <summary>
        /// Get the XML attribute value
        /// </summary>
        /// <param name="n">XML node</param>
        /// <param name="strAttr">Attribute name</param>
        /// <returns>Attribute value, or the empty string if the attribute doesn't exist</returns>
        private static string GetXMLAttribute(XmlNode n, string strAttr) {
            XmlAttribute attr = n.Attributes.GetNamedItem(strAttr) as XmlAttribute;
            if (attr != null)
                return attr.Value;
            return "";
        }

        #endregion
    }
}
