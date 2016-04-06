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
                    string objname = token.Substring(9);
                    string sprite = DataManager.INTERNAL_PREFIX + "unknownthing";
                    string[] states = new string[8];
                    int mapThingNum = -1;
                    int radius = 0;
                    int height = 0;
                    objname = objname.TrimEnd(new char[] { ']' });

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
                    while (token != null)
                    {
                        if (finished) break;
                        switch (token)
                        {
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
                                ReportError("The sprite \"" + token + "\" assigned by the \"$sprite\" property does not exist");
                                return false;
                            case "doomednum":
                                if (!ReadParameter(out token, out finished)) return false;
                                if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out mapThingNum))
                                {
                                    ReportError("Invalid map thing number");
                                    return false;
                                }
                                break;
                            case "radius":
                                if (!ReadParameter(out token, out finished)) return false;
                                if (!ParseWithArithmetic(token, out radius))
                                {
                                    ReportError("Invalid radius");
                                    return false;
                                }
                                radius /= 65536;
                                break;
                            case "height":
                                if (!ReadParameter(out token, out finished)) return false;
                                if (!ParseWithArithmetic(token, out height))
                                {
                                    ReportError("Invalid height");
                                    return false;
                                }
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
                            case "flags":
                                if (!ReadParameter(out token, out finished)) return false;
                                break;
                            default:
                                ReportError("Unknown object definition parameter " + token);
                                return false;
                        }
                        SkipWhitespace(true);
                        token = ReadToken();
                    }

                    if (token != "}")
                    {
                        ReportError("Invalid object definition, missing }");
                        return false;
                    }

                    if (mapThingNum > 0)
                    {
                        SRB2Object o = new SRB2Object(objname, sprite, states, mapThingNum, radius, height);
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

        protected override string GetLanguageType()
        {
            return "Lua";
        }

        #endregion
    }
}
