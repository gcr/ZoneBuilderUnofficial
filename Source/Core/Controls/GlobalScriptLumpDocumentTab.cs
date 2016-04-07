
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

using System.Collections.Generic;
using System.IO;
using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal sealed class GlobalScriptLumpDocumentTab : ScriptDocumentTab
	{
		#region ================== Constants
		
		#endregion

		#region ================== Variables

		private readonly string lumpname;
        private readonly string filepath;
		
		#endregion
		
		#region ================== Properties
		
		public override bool ExplicitSave { get { return true; } }
		public override bool IsSaveAsRequired { get { return false; } }
		public override bool IsClosable { get { return false; } }
		public override bool IsReconfigurable { get { return false; } }
		public override string Filename { get { return lumpname; } } //mxd
		
		#endregion
		
		#region ================== Constructor / Disposer
		
		// Constructor
		public GlobalScriptLumpDocumentTab(ScriptEditorPanel panel, string lumpname, ScriptConfiguration config, string filepath) : base(panel)
		{
    		this.lumpname = lumpname;
			this.filepath = filepath;
			this.config = config;
			editor.SetupStyles(config);

            // Load the lump data
            WAD file = new WAD(filepath);
            Lump lump = file.FindLump(lumpname);
            if (lump != null)
            {
                ClippedStream stream = lump.Stream;
                if (stream != null)
                {
                    editor.SetText(stream.ReadAllBytes());
                    editor.ClearUndoRedo();
                    UpdateNavigator();
                }
                stream.Dispose();
            }
            file.Dispose();

            // Set title
            SetTitle(lumpname.ToUpper());
		}

        #endregion

        #region ================== Methods

        // Compile script
        public override void Compile()
		{
			//mxd. List of errors. UpdateScriptNames can return errors and also updates acs includes list
			List<CompilerError> errors = (config.ScriptType == ScriptType.ACS ? General.Map.UpdateScriptNames() : new List<CompilerError>());

			//mxd. Errors already?..
			if(errors.Count > 0)
			{
				// Feed errors to panel
				panel.ShowErrors(errors);
				return;
			}

			// Compile
			General.Map.CompileLump(lumpname, true);
			
			//mxd. Update script navigator
			UpdateNavigator();

			// Feed errors to panel
			panel.ShowErrors(General.Map.Errors);
		}
		
		// Implicit save
		public override bool Save()
		{
            if (!IsChanged) return true;

			// Store the lump data
			MemoryStream stream = new MemoryStream(editor.GetText());
            WAD file = new WAD(filepath);
            int insertindex = file.Lumps.Count;

            // Remove the lump if it already exists
            int li = file.FindLumpIndex(lumpname);
            if (li > -1)
            {
                insertindex = li;
                file.RemoveAt(li);
            }

            // Insert new lump
            Lump l = file.Insert(lumpname, insertindex, (int)stream.Length);
            l.Stream.Seek(0, SeekOrigin.Begin);
            stream.WriteTo(l.Stream);
            file.Dispose();

            editor.SetSavePoint(); //mxd
            UpdateTitle(); //mxd

            panel.Reload = true;

            return true;
		}

		// This checks if a script error applies to this script
		public override bool VerifyErrorForScript(CompilerError e)
		{
			return (string.Compare(e.filename, "?" + lumpname, true) == 0);
		}
		
		#endregion
		
		#region ================== Events

		#endregion
	}
}
