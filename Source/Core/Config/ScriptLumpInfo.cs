
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

using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	public struct ScriptLumpInfo
	{
		// Members
		public readonly string Name;
        public readonly bool IsPrefix;
		internal readonly ScriptConfiguration Script;
		
		// Construct from IDictionary
		internal ScriptLumpInfo(string name, Configuration cfg)
		{
			// Apply settings
			this.Name = name;
			this.Script = this.Script = new ScriptConfiguration();
            this.IsPrefix = cfg.ReadSetting("scriptlumpnames." + name + ".isprefix", false);
            string scriptconfig = cfg.ReadSetting("scriptlumpnames." + name + ".script", "");
			
			// Find script configuration
			if(scriptconfig.Length > 0)
			{
				if(General.ScriptConfigs.ContainsKey(scriptconfig.ToLowerInvariant()))
				{
					this.Script = General.ScriptConfigs[scriptconfig.ToLowerInvariant()];
				}
				else
				{
					General.ErrorLogger.Add(ErrorType.Warning, "Script lump '" + name + "' in the current game configuration specifies an unknown script configuration '" + scriptconfig + "'. Using plain text instead.");
				}
			}
		}
	}
}

