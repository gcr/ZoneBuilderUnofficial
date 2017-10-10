#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.ZDoom;
using CodeImp.DoomBuilder.GZBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.SRB2
{
    internal sealed class LuaObjectParser : ZDTextParser
    {
        #region ================== Delegates

        public delegate void IncludeDelegate(LuaObjectParser parser, string includefile, bool clearerror);
        public IncludeDelegate OnInclude;

        #endregion

        #region ================== Variables

        private Dictionary<string, SRB2Object> objects;
        /*private Dictionary<string, SRB2State> states;
        private List<string> objectfreeslots;
        private List<string> statefreeslots;
        private List<string> spritefreeslots;*/
        private IDictionary<string, int> flagValues = new Dictionary<string,int>
        {
            { "MF_SPECIAL", 0x1 },
            { "MF_SOLID", 0x2 },
            { "MF_SHOOTABLE", 0x4 },
            { "MF_NOSECTOR", 0x8 },
            { "MF_NOBLOCKMAP", 0x10 },
            { "MF_AMBUSH", 0x20 },
            { "MF_PUSHABLE", 0x40 },
            { "MF_BOSS", 0x80 },
            { "MF_SPAWNCEILING", 0x100 },
            { "MF_NOGRAVITY", 0x200 },
            { "MF_AMBIENT", 0x400 },
            { "MF_SLIDEME", 0x800 },
            { "MF_NOCLIP", 0x1000 },
            { "MF_FLOAT", 0x2000 },
            { "MF_BOXICON", 0x4000 },
            { "MF_MISSILE", 0x8000 },
            { "MF_SPRING", 0x10000 },
            { "MF_BOUNCE", 0x20000 },
            { "MF_MONITOR", 0x40000 },
            { "MF_NOTHINK", 0x80000 },
            { "MF_FIRE", 0x100000 },
            { "MF_NOCLIPHEIGHT", 0x200000 },
            { "MF_ENEMY", 0x400000 },
            { "MF_SCENERY", 0x800000 },
            { "MF_PAIN", 0x1000000 },
            { "MF_STICKY", 0x2000000 },
            { "MF_NIGHTSITEM", 0x4000000 },
            { "MF_NOCLIPTHING", 0x8000000 },
            { "MF_GRENADEBOUNCE", 0x10000000 },
            { "MF_RUNSPAWNFUNC", 0x10000000 }
        };

        #endregion

        #region ================== Properties

        public Dictionary<string, SRB2Object> Objects { get { return objects; } }

        #endregion

        #region ================== Constructor

        public LuaObjectParser()
        {
            // Syntax
            whitespace = "\n \t\r\u00A0";
            specialtokens = "=\n";

            objects = new Dictionary<string, SRB2Object>();
            /*states = new Dictionary<string,SRB2State>();
            objectfreeslots = new List<string>();
            statefreeslots = new List<string>();
            spritefreeslots = new List<string>();*/
        }

        // Disposer
        public void Dispose()
        {
            objects = null;
            /*states = null;
            objectfreeslots = null;
            statefreeslots = null;
            spritefreeslots = null;*/
        }

        #endregion

        #region ================== Parsing

        override public bool Parse(Stream stream, string sourcefilename, bool clearerrors)
        {
            if (!base.Parse(stream, sourcefilename, clearerrors)) return false;

            // Keep local data
            Stream localstream = datastream;
            string localsourcename = sourcename;
            BinaryReader localreader = datareader;

            string token;

            while (SkipWhitespace(true))
            {
                token = ReadToken();
                if (!string.IsNullOrEmpty(token))
                {
                    if (!token.StartsWith("mobjinfo[") || !token.EndsWith("]")) continue;
                    string objname = token.Substring(9).TrimEnd(new char[] { ']' });
                    string name = objname;
                    string sprite = DataManager.INTERNAL_PREFIX + "unknownthing";
                    string category = "";
                    string[] states = new string[8];
                    int mapThingNum = -1;
                    int radius = 0;
                    int height = 0;
                    int flags = 0;

                    SkipWhitespace(true);
                    token = ReadToken();
                    if (token != "=")
                    {
                        ReportError("Invalid object definition, missing =");
                        return false;
                    }

                    SkipWhitespace(true);
                    token = ReadToken();
                    if (token != "{")
                    {
                        ReportError("Invalid object definition, missing {");
                        return false;
                    }

                    SkipWhitespace(true);
                    token = ReadToken();
                    bool finished = false;
                    bool blockclosed = false;
                    while (token != null)
                    {
                        if (finished) break;
                        switch (token)
                        {
                            case "$Name":
                                SkipWhitespace(true);
                                token = ReadLine();
                                name = ZDTextParser.StripQuotes(token);
                                break;
                            case "$Sprite":
                                SkipWhitespace(true);
                                token = ReadToken();
                                if (((token.Length > DataManager.INTERNAL_PREFIX.Length) &&
                                    token.ToLowerInvariant().StartsWith(DataManager.INTERNAL_PREFIX)) ||
                                    General.Map.Data.GetSpriteExists(token))
                                {
                                    sprite = token;
                                    break;
                                }
                                LogWarning("The sprite \"" + token + "\" assigned by the \"$sprite\" property does not exist");
                                break;
                            case "$Category":
                                SkipWhitespace(true);
                                token = ReadLine();
                                category = token;
                                break;
                            case "doomednum":
                                if (!ReadParameter(out token, out finished)) return false;
                                if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out mapThingNum))
                                    LogWarning("Could not parse map thing number");
                                break;
                            case "radius":
                                if (!ReadParameter(out token, out finished)) return false;
                                if (!ParseWithArithmetic(token, out radius))
                                    LogWarning("Could not parse radius");
                                radius /= 65536;
                                break;
                            case "height":
                                if (!ReadParameter(out token, out finished)) return false;
                                if (!ParseWithArithmetic(token, out height))
                                    LogWarning("Could not parse height");
                                height /= 65536;
                                break;
                            case "spawnstate":
                                if (!ReadParameter(out token, out finished)) return false;
                                states[0] = token;
                                break;
                            case "seestate":
                                if (!ReadParameter(out token, out finished)) return false;
                                states[1] = token;
                                break;
                            case "painstate":
                                if (!ReadParameter(out token, out finished)) return false;
                                states[2] = token;
                                break;
                            case "meleestate":
                                if (!ReadParameter(out token, out finished)) return false;
                                states[3] = token;
                                break;
                            case "missilestate":
                                if (!ReadParameter(out token, out finished)) return false;
                                states[4] = token;
                                break;
                            case "deathstate":
                                if (!ReadParameter(out token, out finished)) return false;
                                states[5] = token;
                                break;
                            case "xdeathstate":
                                if (!ReadParameter(out token, out finished)) return false;
                                states[6] = token;
                                break;
                            case "raisestate":
                                if (!ReadParameter(out token, out finished)) return false;
                                states[7] = token;
                                break;
                            case "flags":
                                if (!ReadParameter(out token, out finished)) return false;
                                if (!ParseFlags(token, out flags))
                                    LogWarning("Could not parse flags");
                                break;
                            case "spawnhealth":
                            case "seesound":
                            case "reactiontime":
                            case "attacksound":
                            case "painchance":
                            case "painsound":
                            case "deathsound":
                            case "speed":
                            case "dispoffset":
                            case "mass":
                            case "damage":
                            case "activesound":
                                if (!ReadParameter(out token, out finished)) return false;
                                break;
                            case "}":
                                finished = true;
                                blockclosed = true;
                                break;
                            default:
                                ReportError("Unknown object definition parameter " + token);
                                return false;
                        }
                        if (!blockclosed)
                        {
                            SkipWhitespace(true);
                            token = ReadToken();
                        }
                    }

                    if (token != "}")
                    {
                        ReportError("Invalid object definition, missing }");
                        return false;
                    }

                    if (mapThingNum > 0)
                    {
                        SRB2Object o = new SRB2Object(name, sprite, category, states, mapThingNum, radius, height, flags);
                        if (objects.ContainsKey(objname))
                            objects[objname] = o;
                        else
                            objects.Add(objname, o);
                    }
                }
            }
            // All done
            return !this.HasError;
        }

        // This skips whitespace on the stream, placing the read
        // position right before the first non-whitespace character
        // Returns false when the end of the stream is reached
        protected internal override bool SkipWhitespace(bool skipnewline)
        {
            int offset = skipnewline ? 0 : 1;
            char c;
            prevstreamposition = datastream.Position; //mxd

            do
            {
                if (datastream.Position == datastream.Length) return false;
                c = (char)datareader.ReadByte();

                // Check if this is comment
                if (c == '/' || c == '-')
                {
                    if (datastream.Position == datastream.Length) return false;
                    char c2 = (char)datareader.ReadByte();
                    if (c2 == c)
                    {
                        // Check if not a special comment with a token
                        if (datastream.Position == datastream.Length) return false;
                        char c3 = (char)datareader.ReadByte();
                        if (c3 != '$')
                        {
                            // Skip entire line
                            char c4 = ' ';
                            while ((c4 != '\n') && (datastream.Position < datastream.Length)) { c4 = (char)datareader.ReadByte(); }
                            c = c4;
                        }
                        else
                        {
                            // Not a comment
                            c = c3;
                        }
                    }
                    else if (c2 == '*')
                    {
                        // Skip until */
                        char c4, c3 = '\0';
                        prevstreamposition = datastream.Position; //mxd
                        do
                        {
                            if (datastream.Position == datastream.Length) //mxd
                            {
                                // ZDoom doesn't give even a warning message about this, so we shouldn't report error or fail parsing.
                                General.ErrorLogger.Add(ErrorType.Warning, "Lua warning in '" + sourcename + "', line " + GetCurrentLineNumber() + ". Block comment is not closed.");
                                return false;
                            }

                            c4 = c3;
                            c3 = (char)datareader.ReadByte();
                        }
                        while ((c4 != '*') || (c3 != '/'));
                        c = ' ';
                    }
                    else
                    {
                        // Not a comment, rewind from reading c2
                        datastream.Seek(-1, SeekOrigin.Current);
                    }
                }

                if (c == '-')
                {
                    if (datastream.Position == datastream.Length) return false;
                    char c2 = (char)datareader.ReadByte();
                    if (c2 == '-')
                    {
                        if (datastream.Position == datastream.Length) return false;
                        char c3 = (char)datareader.ReadByte();
                        if (c3 == '[')
                        {
                            if (datastream.Position == datastream.Length) return false;
                            char c4 = (char)datareader.ReadByte();
                            if (c4 == '[')
                            {
                                //Skip until ]]
                                char c5 = '\0';
                                char c6 = '\0';
                                prevstreamposition = datastream.Position; //mxd
                                do
                                {
                                    if (datastream.Position == datastream.Length) //mxd
                                    {
                                        General.ErrorLogger.Add(ErrorType.Warning, "Lua warning in '" + sourcename + "', line " + GetCurrentLineNumber() + ". Block comment is not closed.");
                                        return false;
                                    }

                                    c5 = c6;
                                    c6 = (char)datareader.ReadByte();
                                }
                                while ((c6 != ']') || (c5 != ']'));
                                c = ' ';
                            }
                            else
                            {
                                // Not a multiline comment, rewind from reading c4
                                datastream.Seek(-1, SeekOrigin.Current);
                            }
                        }
                        while ((c3 != '\n') && (datastream.Position < datastream.Length)) { c3 = (char)datareader.ReadByte(); }
                        c = c3;
                    }
                    else
                    {
                        // Not a comment, rewind from reading c2
                        datastream.Seek(-1, SeekOrigin.Current);
                    }
                }
            }
            while (whitespace.IndexOf(c, offset) > -1);

            // Go one character back so we can read this non-whitespace character again
            datastream.Seek(-1, SeekOrigin.Current);
            return true;
        }

        protected internal override string ReadToken(bool multiline)
        {
            //mxd. Return empty string when the end of the stream has been reached
            if (datastream.Position == datastream.Length) return string.Empty;

            //mxd. Store starting position 
            prevstreamposition = datastream.Position;

            string token = "";
            bool quotedstring = false;

            // Start reading
            char c = (char)datareader.ReadByte();
            while (!IsWhitespace(c) || quotedstring || IsSpecialToken(c))
            {
                //mxd. Break at newline?
                if (!multiline && c == '\r')
                {
                    // Go one character back so line number is correct
                    datastream.Seek(-1, SeekOrigin.Current);
                    return token;
                }

                // Special token?
                if (!quotedstring && IsSpecialToken(c))
                {
                    // Not reading a token yet?
                    if (token.Length == 0)
                    {
                        // This is our whole token
                        token += c;
                        break;
                    }
                    else
                    {
                        // This is a new token and shouldn't be read now
                        // Go one character back so we can read this token again
                        datastream.Seek(-1, SeekOrigin.Current);
                        break;
                    }
                }
                else
                {
                    // Quote?
                    if (c == '"')
                    {
                        // Quote to end the string?
                        if (quotedstring) quotedstring = false;

                        // First character is a quote?
                        if (token.Length == 0) quotedstring = true;

                        token += c;
                    }
                    // Potential comment?
                    else if ((c == '/') && !quotedstring)
                    {
                        // Check the next byte
                        if (datastream.Position == datastream.Length) return token;
                        char c2 = (char)datareader.ReadByte();
                        if ((c2 == '/') || (c2 == '*'))
                        {
                            // This is a comment start, so the token ends here
                            // Go two characters back so we can read this comment again
                            datastream.Seek(-2, SeekOrigin.Current);
                            break;
                        }
                        else
                        {
                            // Not a comment
                            // Go one character back so we can read this char again
                            datastream.Seek(-1, SeekOrigin.Current);
                            token += c;
                        }
                    }
                    // Potential comment?
                    else if ((c == '-') && !quotedstring)
                    {
                        // Check the next byte
                        if (datastream.Position == datastream.Length) return token;
                        char c2 = (char)datareader.ReadByte();
                        if (c2 == '-')
                        {
                            // This is a comment start, so the token ends here
                            // Go two characters back so we can read this comment again
                            datastream.Seek(-2, SeekOrigin.Current);
                            break;
                        }
                        else
                        {
                            // Not a comment
                            // Go one character back so we can read this char again
                            datastream.Seek(-1, SeekOrigin.Current);
                            token += c;
                        }
                    }
                    else
                    {
                        token += c;
                    }
                }

                // Next character
                if (datastream.Position < datastream.Length)
                    c = (char)datareader.Read();
                else
                    break;
            }

            return token;
        }

        // This reads a token (all sequential non-whitespace characters or a single character) using custom set of special tokens
        // Returns null when the end of the stream has been reached (mxd)
        protected internal override string ReadToken(string specialTokens)
        {
            // Return null when the end of the stream has been reached
            if (datastream.Position == datastream.Length) return null;

            //mxd. Store starting position 
            prevstreamposition = datastream.Position;

            string token = "";
            bool quotedstring = false;

            // Start reading
            char c = (char)datareader.ReadByte();
            while (!IsWhitespace(c) || quotedstring || specialTokens.IndexOf(c) != -1)
            {
                // Special token?
                if (!quotedstring && specialTokens.IndexOf(c) != -1)
                {
                    // Not reading a token yet?
                    if (token.Length == 0)
                    {
                        // This is our whole token
                        token += c;
                        break;
                    }

                    // This is a new token and shouldn't be read now
                    // Go one character back so we can read this token again
                    datastream.Seek(-1, SeekOrigin.Current);
                    break;
                }
                else
                {
                    // Quote?
                    if (c == '"')
                    {
                        // Quote to end the string?
                        if (quotedstring) quotedstring = false;

                        // First character is a quote?
                        if (token.Length == 0) quotedstring = true;

                        token += c;
                    }
                    // Potential comment?
                    else if ((c == '/') && !quotedstring)
                    {
                        // Check the next byte
                        if (datastream.Position == datastream.Length) return token;
                        char c2 = (char)datareader.ReadByte();
                        if ((c2 == '/') || (c2 == '*'))
                        {
                            // This is a comment start, so the token ends here
                            // Go two characters back so we can read this comment again
                            datastream.Seek(-2, SeekOrigin.Current);
                            break;
                        }
                        else
                        {
                            // Not a comment
                            // Go one character back so we can read this char again
                            datastream.Seek(-1, SeekOrigin.Current);
                            token += c;
                        }
                    }
                    // Potential comment?
                    else if ((c == '-') && !quotedstring)
                    {
                        // Check the next byte
                        if (datastream.Position == datastream.Length) return token;
                        char c2 = (char)datareader.ReadByte();
                        if (c2 == '-')
                        {
                            // This is a comment start, so the token ends here
                            // Go two characters back so we can read this comment again
                            datastream.Seek(-2, SeekOrigin.Current);
                            break;
                        }
                        else
                        {
                            // Not a comment
                            // Go one character back so we can read this char again
                            datastream.Seek(-1, SeekOrigin.Current);
                            token += c;
                        }
                    }
                    else
                    {
                        token += c;
                    }
                }

                // Next character
                if (datastream.Position < datastream.Length)
                    c = (char)datareader.Read();
                else
                    break;
            }

            return token;
        }

        #endregion

        #region ================== Methods

        private bool ReadParameter(out string output, out bool finished)
        {
            output = "";
            finished = false;
            SkipWhitespace(true);
            string token = ReadToken();
            if (token != "=")
            {
                ReportError("Invalid parameter definition, missing =");
                return false;
            }
            SkipWhitespace(true);
            token = ReadToken();
            if (!token.EndsWith(","))
            {
                finished = true;
            }
            output = token.TrimEnd(new char[] { ',' });
            return true;
        }

        private bool ParseWithArithmetic(string input, out int output)
        {
            output = 0;
            string[] tokens = input.Split(new char[] { '+' });
            foreach (string t in tokens)
            {
                int val = 0;
                if (!ParseMultiplication(t, out val))
                    return false;
                output += val;
            }
            return true;
        }

        private bool ParseMultiplication(string input, out int output)
        {
            output = 1;
            string[] tokens = input.Split(new char[] { '*' });
            foreach (string t in tokens)
            {
                string trimmed = t.Trim();
                int val = 1;
                if (trimmed == "FRACUNIT") val = 65536;
                else if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out val))
                    return false;
                output *= val;
            }
            return true;
        }

        private bool ParseFlags(string input, out int output)
        {
            output = 0;
            string[] tokens = input.Split(new char[] { '|' });
            foreach (string token in tokens)
            {
                if (flagValues.ContainsKey(token))
                {
                    output |= flagValues[token];
                }
                else
                {
                    int val = 0;
                    if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out val))
                        return false;
                    output |= val;
                }
            }
            return true;
        }

        protected override string GetLanguageType()
        {
            return "Lua";
        }

        #endregion
    }
}
