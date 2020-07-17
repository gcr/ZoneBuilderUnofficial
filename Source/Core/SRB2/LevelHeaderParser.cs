#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.ZDoom;
using CodeImp.DoomBuilder.GZBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.SRB2
{
    internal sealed class LevelHeaderParser : ZDTextParser
    {
        #region ================== Delegates

        public delegate void IncludeDelegate(LevelHeaderParser parser, string includefile, bool clearerror);
        public IncludeDelegate OnInclude;

        #endregion

        #region ================== Variables

        private MapInfo mapinfo;
        private string mapname;
        private readonly HashSet<string> parsedlumps;
        private StreamReader streamreader;
        private int linenumber;

        #endregion

        #region ================== Properties

        public MapInfo MapInfo { get { return mapinfo; } }

        #endregion

        #region ================== Constructor

        public LevelHeaderParser()
        {
            // Syntax
            whitespace = "\n \t\r\u00A0";
            specialtokens = "=\n";

            mapinfo = new MapInfo();
            parsedlumps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region ================== Parsing

        override public bool Parse(Stream stream, string sourcefilename, bool clearerrors)
        {
            if (string.IsNullOrEmpty(mapname)) throw new NotSupportedException("Map name required!");
            return Parse(stream, sourcefilename, mapname, clearerrors);
        }

        public bool Parse(Stream stream, string sourcefilename, string mapname, bool clearerrors)
        {
            this.mapname = mapname.ToUpperInvariant();
            if (!base.Parse(stream, sourcefilename, clearerrors)) return false;

            // Keep local data
            streamreader = new StreamReader(stream, Encoding.ASCII);
            linenumber = -1;

            while (!streamreader.EndOfStream)
            {
                string line = RemoveComments(streamreader.ReadLine());
                linenumber++;
                if (String.IsNullOrEmpty(line) || line.StartsWith("\n")) continue;
                string[] tokens = line.Split(new char[] { ' ' });
                switch (tokens[0].ToUpperInvariant())
                {
                    case "LEVEL":
                        if (tokens.Length < 2 || String.IsNullOrEmpty(tokens[1]))
                        {
                            ReportError("Level block is missing a level number");
                            break;
                        }
                        if (GetMapName(tokens[1].ToUpperInvariant()) != mapname) break;
                        if (!ParseLevelHeader(mapname)) return false;
                        break;
                }
            }

            // All done
            return !this.HasError;
        }

        #endregion

        #region ================== Map block parsing

        private bool ParseLevelHeader(string mapname)
        {
            if (mapname == null) return false;
            string levelname = "";
            int act = 0;
            bool zone = true;
            while (!streamreader.EndOfStream)
            {
                string line = streamreader.ReadLine();
                linenumber++;
                if (String.IsNullOrEmpty(line) || line.StartsWith("\n")) break;
                if (line.StartsWith("#")) continue;
                line = RemoveComments(line);
                string[] tokens = line.Split(new char[] { '=' });
                if (tokens.Length != 2)
                {
                    ReportError("Invalid line");
                    return false;
                }
                tokens[0] = tokens[0].Trim().ToUpperInvariant();
                tokens[1] = tokens[1].Trim().ToUpperInvariant();
                switch(tokens[0])
                {
                    case "LEVELNAME":
                        levelname = tokens[1];
                        break;

                    case "ACT":
                        if (!int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out act) || act < 0 || act >= 20)
                        {
                            ReportError("Invalid act number");
                            return false;
                        }
                        break;

                    case "NOZONE":
                        zone = tokens[1][0] == 'T' || tokens[1][0] == 'Y';
                        break;

                    case "SKYNUM":
                        int skyn;
                        if (!int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out skyn))
                        {
                            ReportError("Invalid sky number");
                            return false;
                        }
                        mapinfo.Sky1 = "SKY" + skyn;
                        break;
                }
            }
            mapinfo.Title = levelname + (zone ? " ZONE" : "") + (act > 0 ? " " + act : "");

            return true;
        }

        private string GetMapName(string number)
        {
            int n;
            if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                return ConvertToExtendedMapNum(n);
            else
            {
                if (number.Length != 2 || number[0] < 'A' || number[0] > 'Z' || !((number[1] >= '0' && number[1] <= '9') || (number[1] >= 'A' && number[1] <= 'Z')))
                {
                    ReportError("Invalid level number");
                    return null;
                }
                return "MAP" + number;
            }

        }

        private string ConvertToExtendedMapNum(int n)
        {
            if (n <= 0 || n > 1035)
            {
                ReportError("Invalid level number");
                return null;
            }
            if (n < 10)
                return "MAP0" + n;
            if (n < 100)
                return "MAP" + n.ToString();

            int x = n - 100;
            int p = x / 36;
            int q = x % 36;
            char a = (char)('A' + p);
            char b = (q < 10) ? (char)('0' + q) : (char)('A' + q - 10);
            return "MAP" + String.Concat(a, b);
        }

        #endregion

        #region ================== Methods

        // This reports an error
        protected internal override void ReportError(string message)
        {
            // Set error information
            errordesc = message;
            errorline = (streamreader != null ? linenumber : CompilerError.NO_LINE_NUMBER); //mxd
            errorsource = sourcename;
        }

        protected override string GetLanguageType()
        {
            return "SOC";
        }

        private string RemoveComments(string line)
        {
            string[] tokens = line.Split(new char[] { '#' });
            return tokens[0];
        }

        #endregion
    }
}
