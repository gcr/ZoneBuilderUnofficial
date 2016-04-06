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
    internal sealed class SOCObjectParser : ZDTextParser
    {
        #region ================== Delegates

        public delegate void IncludeDelegate(SOCObjectParser parser, string includefile, bool clearerror);
        public IncludeDelegate OnInclude;

        #endregion

        #region ================== Variables

        private Dictionary<string, SRB2Object> objects;
        /*private Dictionary<string, SRB2State> states;
        private List<string> objectfreeslots;
        private List<string> statefreeslots;
        private List<string> spritefreeslots;*/
        private StreamReader streamreader;
        private int linenumber;

        #endregion

        #region ================== Properties

        public Dictionary<string, SRB2Object> Objects { get { return objects; } }

        #endregion

        #region ================== Constructor

        public SOCObjectParser()
        {
            // Syntax
            whitespace = "\n \t\r\u00A0";
            specialtokens = "=\n";

            objects = new Dictionary<string,SRB2Object>();
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
                    /*case "FREESLOT":
                        if (!ParseFreeslots()) return false;
                        break;*/
                    case "OBJECT":
                    case "MOBJ":
                    case "THING":
                        if (tokens.Length < 2 || String.IsNullOrEmpty(tokens[1]))
                        {
                            ReportError("Object block is missing an object name");
                            break;
                        }
                        if (!ParseObject(tokens[1].ToUpperInvariant())) return false;
                        break;
                    /*case "STATE":
                    case "FRAME":
                        if (tokens.Length < 2 || String.IsNullOrEmpty(tokens[1]))
                        {
                            ReportError("State block is missing an state name");
                            break;
                        }
                        if (!ParseState(tokens[1].ToUpperInvariant())) return false;
                        break;*/
                }
            }

            // All done
            return !this.HasError;
        }

        #endregion

        #region ================== Map block parsing

        /*private bool ParseFreeslots()
        {
            while (!streamreader.EndOfStream)
            {
                string line = streamreader.ReadLine();
                linenumber++;
                if (String.IsNullOrEmpty(line) || line.StartsWith("\n")) break;
                if (line.StartsWith("#")) continue;
                line = RemoveComments(line).Trim();
                if (line.StartsWith("MT_")) objectfreeslots.Add(line);
                else if (line.StartsWith("S_")) statefreeslots.Add(line);
                else if (line.StartsWith("SPR_")) spritefreeslots.Add(line);
            }
            return true;
        }*/

        private bool ParseObject(string name)
        {
            if (name == null) return false;
            string sprite = DataManager.INTERNAL_PREFIX + "unknownthing";
            string[] states = new string[8];
            int mapThingNum = -1;
            int radius = 0;
            int height = 0;
            while (!streamreader.EndOfStream)
            {
                string line = streamreader.ReadLine();
                linenumber++;
                if (String.IsNullOrEmpty(line) || line.StartsWith("\n")) break;
                if (line.StartsWith("#$Sprite "))
                {
                    string spritename = line.Substring(9);
                    if (((spritename.Length > DataManager.INTERNAL_PREFIX.Length) &&
                        spritename.ToLowerInvariant().StartsWith(DataManager.INTERNAL_PREFIX)) ||
                        General.Map.Data.GetSpriteExists(spritename))
                    {
                        sprite = spritename;
                        continue;
                    }
                    ReportError("The sprite \"" + spritename + "\" assigned by the \"$sprite\" property does not exist");
                }
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
                    case "MAPTHINGNUM":
                        if (!int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out mapThingNum))
                        {
                            ReportError("Invalid map thing number");
                            return false;
                        }
                        break;
                    case "RADIUS":
                        if (!ParseWithArithmetic(tokens[1], out radius))
                        {
                            ReportError("Invalid radius");
                            return false;
                        }
                        radius /= 65536;
                        break;
                    case "HEIGHT":
                        if (!ParseWithArithmetic(tokens[1], out height))
                        {
                            ReportError("Invalid height");
                            return false;
                        }
                        height /= 65536;
                        break;

                    case "SPAWNSTATE":
                        states[0] = tokens[1];
                        break;
                    case "SEESTATE":
                        states[1] = tokens[1];
                        break;
                    case "PAINSTATE":
                        states[2] = tokens[1];
                        break;
                    case "MELEESTATE":
                        states[3] = tokens[1];
                        break;
                    case "MISSILESTATE":
                        states[4] = tokens[1];
                        break;
                    case "DEATHSTATE":
                        states[5] = tokens[1];
                        break;
                    case "XDEATHSTATE":
                        states[6] = tokens[1];
                        break;
                    case "RAISESTATE":
                        states[7] = tokens[1];
                        break;
                }
            }
            if (mapThingNum > 0)
            {
                SRB2Object o = new SRB2Object(name, sprite, states, mapThingNum, radius, height);
                if (objects.ContainsKey(name))
                    objects[name] = o;
                else
                    objects.Add(name, o);
            }

            return true;
        }

        /*private bool ParseState(string name)
        {
            if (name == null) return false;
            string spritename = "";
            int spriteframe = 0;
            string next = "";
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
                switch (tokens[0])
                {
                    case "SPRITENAME":
                    case "SPRITENUMBER":
                        spritename = tokens[1];
                        break;
                    case "SPRITEFRAME":
                    case "SPRITESUBNUMBER":
                    //TODO: Strip flags
                        spriteframe = ParseSpriteFrame(tokens[1]);
                        break;
                    case "NEXT":
                        next = tokens[1];
                        break;
                }
            }
            states.Add(new SRB2State(name, spritename, spriteframe, next));

            return true;
        }*/

        #endregion

        #region ================== Methods

        private bool ParseWithArithmetic(string input, out int output)
        {
            output = 1;
            string[] tokens = input.Split(new char[] { '*' });
            foreach (string t in tokens)
            {
                string trimmed = t.Trim();
                int val = 1;
                if (trimmed == "FRACUNIT") val = 65536;
                else if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out val))
                {
                    ReportError("Invalid radius");
                    return false;
                }
                output *= val;
            }
            return true;
        }

        // This reports an error
        protected internal override void ReportError(string message)
        {
            // Set error information
            errordesc = message;
            errorline = (streamreader != null ? linenumber : CompilerError.NO_LINE_NUMBER); //mxd
            errorsource = sourcename;
        }

        private string RemoveComments(string line)
        {
            string[] tokens = line.Split(new char[] { '#' });
            return tokens[0];
        }

        protected override string GetLanguageType()
        {
            return "SOC";
        }

        #endregion
    }
}
