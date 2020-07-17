
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Globalization;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	public class LinedefActionInfo : INumberedTitle, IComparable<LinedefActionInfo>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Properties
		private readonly int index;
		private readonly string prefix;
		private readonly string category;
		private readonly string name;
		private readonly string title;
		private readonly string id; //mxd. wiki-compatible name 
		private readonly ArgumentInfo[] args;
		private readonly bool isgeneralized;
		private readonly bool isknown;
		private readonly bool requiresactivation; //mxd
        private IDictionary<string, string> flags;
        private readonly string slope;
        private readonly int slopeargs;
        private readonly bool threedfloor;
        private readonly bool threedfloorcustom;
        private readonly int threedfloorflags;
        private readonly IDictionary<string,int> threedfloorflagsadditions;
        #endregion

        #region ================== Properties

        public int Index { get { return index; } }
		public string Prefix { get { return prefix; } }
		public string Category { get { return category; } }
		public string Name { get { return name; } }
		public string Title { get { return title; } }
		public string Id { get { return id; } } //mxd
		public bool IsGeneralized { get { return isgeneralized; } }
		public bool IsKnown { get { return isknown; } }
		public bool IsNull { get { return (index == 0); } }
		public bool RequiresActivation { get { return requiresactivation; } } //mxd
		public ArgumentInfo[] Args { get { return args; } }
        public IDictionary<string, string> Flags { get { return flags; } }
        public bool IsRegularSlope { get { return slope == "regular"; } }
        public bool IsCopySlope { get { return slope == "copy"; } }
        public bool IsVertexSlope { get { return slope == "vertex"; } }
        public int SlopeArgs { get { return slopeargs; } }
        public bool ThreeDFloor { get { return threedfloor; } }
        public bool ThreeDFloorCustom { get { return threedfloorcustom; } }
        public int ThreeDFloorFlags { get { return threedfloorflags; } }
        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal LinedefActionInfo(int index, Configuration cfg, LinedefActionCategory ac, IDictionary<string, EnumList> enums)
		{
			string actionsetting = "linedeftypes." + ac.Name + "." + index.ToString(CultureInfo.InvariantCulture);
			
			// Initialize
			this.index = index;
			this.category = ac.Name;
			this.args = new ArgumentInfo[Linedef.NUM_ARGS];
			this.isgeneralized = false;
			this.isknown = true;
			
			// Read settings
			this.name = cfg.ReadSetting(actionsetting + ".title", "Unnamed");
			this.id = cfg.ReadSetting(actionsetting + ".id", string.Empty); //mxd
			this.prefix = cfg.ReadSetting(actionsetting + ".prefix", "");
			this.requiresactivation = cfg.ReadSetting(actionsetting + ".requiresactivation", true); //mxd
			this.title = this.prefix + " " + this.name;
			this.title = this.title.Trim();
            this.flags = new Dictionary<string, string>(ac.Flags);
            ReadLinedefSpecificFlags(cfg);
            this.slope = cfg.ReadSetting(actionsetting + ".slope", "");
            this.slopeargs = cfg.ReadSetting(actionsetting + ".slopeargs", 0);
            this.threedfloor = cfg.ReadSetting(actionsetting + ".3dfloor", false);
            this.threedfloorcustom = cfg.ReadSetting(actionsetting + ".3dfloorcustom", false);
            try { this.threedfloorflags = Convert.ToInt32(cfg.ReadSetting(actionsetting + ".3dfloorflags", "0"), 16); }
            catch (FormatException) { this.threedfloorflags = 0; }
            this.threedfloorflagsadditions = new Dictionary<string, int>();
            foreach (KeyValuePair<string,string> p in flags)
            {
                int value = 0;
                try { value = Convert.ToInt32(cfg.ReadSetting(actionsetting + ".flags" + p.Key + "3dfloorflagsadd", "0"), 16); }
                catch (FormatException) { }
                try { value -= Convert.ToInt32(cfg.ReadSetting(actionsetting + ".flags" + p.Key + "3dfloorflagsremove", "0"), 16); }
                catch (FormatException) { }
                this.threedfloorflagsadditions.Add(p.Key, value);

            }

            // Read the args
            for (int i = 0; i < Linedef.NUM_ARGS; i++)
				this.args[i] = new ArgumentInfo(cfg, actionsetting, i, enums);
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		// Constructor for generalized type display
		internal LinedefActionInfo(int index, string title, bool isknown, bool isgeneralized)
		{
			this.index = index;
			this.isgeneralized = isgeneralized;
			this.isknown = isknown;
			this.requiresactivation = true; //mxd. Unused, set for consistency sake.
			this.title = title;
            this.flags = new Dictionary<string, string>();
            this.slope = "";
            this.slopeargs = 0;
            this.threedfloor = false;
            this.threedfloorcustom = false;
            this.threedfloorflags = 0;
            this.threedfloorflagsadditions = new Dictionary<string, int>();
            this.args = new ArgumentInfo[Linedef.NUM_ARGS];
			for(int i = 0; i < Linedef.NUM_ARGS; i++)
				this.args[i] = new ArgumentInfo(i);
		}

		#endregion

		#region ================== Methods

		// This presents the item as string
		public override string ToString()
		{
			return index + " - " + title;
		}

		// This compares against another action info
		public int CompareTo(LinedefActionInfo other)
		{
			if(this.index < other.index) return -1;
			else if(this.index > other.index) return 1;
			else return 0;
        }

        private void ReadLinedefSpecificFlags(Configuration cfg)
        {
            Dictionary<string, string> newflags = new Dictionary<string, string>(flags);
            string key = index.ToString(CultureInfo.InvariantCulture);
            foreach (KeyValuePair<string, string> p in flags)
            {
                newflags[p.Key] = cfg.ReadSetting("linedeftypes." + category + "." + key + ".flags" + p.Key + "text", p.Value);
            }
            flags = newflags;
        }

        public int Get3DFloorFlags(IDictionary<string,bool> setflags)
        {
            int value = threedfloorflags;
            foreach (KeyValuePair<string,int> p in threedfloorflagsadditions)
            {
                if (setflags.ContainsKey(p.Key) && setflags[p.Key]) value += p.Value;
            }
            return value;
        }

        #endregion
    }
}
