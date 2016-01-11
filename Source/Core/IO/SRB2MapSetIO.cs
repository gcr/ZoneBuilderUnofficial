
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
using System.IO;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.IO
{
    internal class SRB2MapSetIO : DoomMapSetIO
    {
        #region ================== Constants
        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        public SRB2MapSetIO(WAD wad, MapManager manager) : base(wad, manager)
        {
            //Dictionary contents: Type, flags, translucency, flags when noclimb is active
            //Type: 1 = solid, 2 = water, 3 = intangible, +4 = render insides
            //Flags: 1 = disable lighting effects (e.g. shadows), 2 = restrict lighting effects to insides, 4 = fog
            //Translucency: 0 = invisible, 1 = read from front upper texture, 2 = opaque
            threeDFloorTypes = new Dictionary<int, int[]>() {
                { 100, new int[4] { 1, 0, 2, 0} },
                { 101, new int[4] { 1, 1, 2, 1} },
                { 102, new int[4] { 1, 1, 1, 1} },
                { 103, new int[4] { 1, 1, 2, 1} },
                { 104, new int[4] { 1, 1, 2, 0} },
                { 105, new int[4] { 1, 1, 0, 1} },
                { 120, new int[4] { 6, 0, 2, 2} },
                { 121, new int[4] { 6, 0, 1, 2} },
                { 122, new int[4] { 6, 0, 2, 2} },
                { 123, new int[4] { 6, 0, 1, 2} },
                { 124, new int[4] { 6, 0, 1, 2} },
                { 125, new int[4] { 6, 0, 1, 2} },
                { 140, new int[4] { 1, 0, 2, 1} },
                { 141, new int[4] { 1, 0, 1, 1} },
                { 142, new int[4] { 1, 0, 1, 1} },
                { 143, new int[4] { 1, 0, 2, 1} },
                { 144, new int[4] { 1, 0, 1, 1} },
                { 145, new int[4] { 1, 0, 1, 1} },
                { 146, new int[4] { 1, 0, 2, 1} },
                { 150, new int[4] { 1, 0, 2, 0} },
                { 151, new int[4] { 1, 0, 2, 0} },
                { 152, new int[4] { 1, 0, 2, 0} },
                { 160, new int[4] { 1, 0, 2, 0} },
                { 170, new int[4] { 1, 0, 2, 0} },
                { 171, new int[4] { 1, 0, 2, 0} },
                { 172, new int[4] { 1, 0, 2, 1} },
                { 173, new int[4] { 1, 0, 2, 1} },
                { 174, new int[4] { 1, 0, 1, 1} },
                { 175, new int[4] { 1, 0, 1, 1} },
                { 176, new int[4] { 1, 0, 2, 0} },
                { 177, new int[4] { 1, 0, 2, 0} },
                { 178, new int[4] { 1, 0, 2, 0} },
                { 179, new int[4] { 1, 0, 2, 0} },
                { 180, new int[4] { 1, 0, 2, 0} },
                { 190, new int[4] { 1, 0, 2, 0} },
                { 191, new int[4] { 1, 1, 2, 1} },
                { 192, new int[4] { 1, 1, 1, 1} },
                { 193, new int[4] { 1, 1, 0, 1} },
                { 194, new int[4] { 1, 0, 2, 1} },
                { 195, new int[4] { 1, 0, 1, 1} },
                { 200, new int[4] { 3, 1, 0, 1} },
                { 201, new int[4] { 3, 0, 0, 0} },
                { 202, new int[4] { 7, 5, 2, 5} },
                { 220, new int[4] { 3, 0, 2, 0} },
                { 221, new int[4] { 3, 1, 1, 0} },
                { 222, new int[4] { 3, 1, 2, 0} },
                { 223, new int[4] { 3, 1, 0, 1} },
                { 250, new int[4] { 1, 0, 2, 0} },
                { 251, new int[4] { 1, 0, 2, 0} },
                { 252, new int[4] { 1, 0, 2, 0} },
                { 253, new int[4] { 1, 0, 1, 0} },
                { 254, new int[4] { 1, 0, 2, 0} },
                { 255, new int[4] { 1, 0, 2, 0} },
                { 256, new int[4] { 1, 0, 1, 0} },
                { 257, new int[4] { 5, 0, 2, 0} },
                { 258, new int[4] { 1, 1, 2, 0} },
                { 259, new int[4] { 1, 0, 2, 0} }
            };

            //Dictionary contents: floor, ceiling (0 = no slope, 1 = slope front, 2 = slope back)
            slopeTypes = new Dictionary<int, int[]>() {
                { 700, new int[2] { 1, 0 } },
                { 701, new int[2] { 0, 1 } },
                { 702, new int[2] { 1, 1 } },
                { 703, new int[2] { 1, 2 } },
                { 710, new int[2] { 2, 0 } },
                { 711, new int[2] { 0, 2 } },
                { 712, new int[2] { 2, 2 } },
                { 713, new int[2] { 2, 1 } }
            };

            //Dictionary contents: floor, ceiling (0 = no slope, 1 = slope front, 2 = slope back)
            slopeCopyTypes = new Dictionary<int, int[]>() {
                { 720, new int[2] { 1, 0 } },
                { 721, new int[2] { 0, 1 } },
                { 722, new int[2] { 1, 1 } },
            };

            //Dictionary contents:
            //1. 0 = slope front, 1 = slope back
            //2. 0 = slope floor, 1 = slope ceiling
            vertexSlopeTypes = new Dictionary<int, int[]>() {
                { 704, new int[2] { 0, 0 } },
                { 705, new int[2] { 0, 1 } },
                { 714, new int[2] { 1, 0 } },
                { 715, new int[2] { 1, 1 } },
            };

            translucentLineTypes = new Dictionary<int, float>() {
                { 900, 0.9f },
                { 901, 0.8f },
                { 902, 0.7f },
                { 903, 0.6f },
                { 904, 0.5f },
                { 905, 0.4f },
                { 906, 0.3f },
                { 907, 0.2f },
                { 908, 0.1f },
            };

        }

        #endregion

        #region ================== Properties
        public override bool HasThingHeight { get { return true; } }
        public override bool HasLinedefParameters { get { return false; } }
        public override bool HasTranslucent3DFloors { get { return true; } }
        public override int Custom3DFloorType { get { return 259; } }
        public override int SlopeVertexType { get { return 750; } }
        public override int MaxThingHeight { get { return 4095; } }
        public override int MinThingHeight { get { return 0; } }
        public override int ColormapType { get { return 606; } }
        public override int FlatAlignmentType { get { return 7; } }
        public override int AxisType { get { return 1700; } }
        public override int AxisTransferLineType { get { return 1702; } }
        #endregion

        #region ================== Reading

        // This reads the THINGS from WAD file
        protected override void ReadThings(MapSet map, int firstindex)
        {
            // Get the lump from wad file
            Lump lump = wad.FindLump("THINGS", firstindex);
            if (lump == null) throw new Exception("Could not find required lump THINGS!");

            // Prepare to read the items
            MemoryStream mem = new MemoryStream(lump.Stream.ReadAllBytes());
            int num = (int)lump.Stream.Length / 10;
            BinaryReader reader = new BinaryReader(mem);

            // Read items from the lump
            map.SetCapacity(0, 0, 0, 0, map.Things.Count + num);
            for (int i = 0; i < num; i++)
            {
                // Read properties from stream
                int x = reader.ReadInt16();
                int y = reader.ReadInt16();
                int angle = reader.ReadInt16();
                int type = reader.ReadUInt16();
                int flags = reader.ReadUInt16();

                // Make string flags
                Dictionary<string, bool> stringflags = new Dictionary<string, bool>(StringComparer.Ordinal);
                foreach (KeyValuePair<string, string> f in manager.Config.ThingFlags)
                {
                    int fnum;
                    if (int.TryParse(f.Key, out fnum)) stringflags[f.Key] = ((flags & fnum) == fnum);
                }

                // MascaraSnake: SRB2 stores Z position in upper 12 bits of flags. Read Z position and remove it from flags.
                int z = flags >> 4;
                flags &= 0xF;

                // Create new item
                Thing t = map.CreateThing();
                t.Update(type, x, y, z, angle, 0, 0, 1.0f, 1.0f, stringflags, 0, 0, new int[Thing.NUM_ARGS]);
            }

            // Done
            mem.Dispose();
        }
        #endregion

        #region ================== Writing
        // This writes the THINGS to WAD file
        protected override void WriteThings(MapSet map, int position, Dictionary<string, MapLumpInfo> maplumps)
        {
            // Create memory to write to
            MemoryStream mem = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mem, WAD.ENCODING);

            // Go for all things
            foreach (Thing t in map.Things)
            {
                // Convert flags
                int flags = 0;
                foreach (KeyValuePair<string, bool> f in t.Flags)
                {
                    int fnum;
                    if (f.Value && int.TryParse(f.Key, out fnum)) flags |= fnum;
                }

                // MascaraSnake: SRB2 stores Z position in upper 12 bits of flags. Add Z position to flags.
                flags |= (UInt16)t.Position.z << 4;

                // Write properties to stream
                writer.Write((Int16)t.Position.x);
                writer.Write((Int16)t.Position.y);
                writer.Write((Int16)t.AngleDoom);
                writer.Write((UInt16)t.Type);
                writer.Write((UInt16)flags);
            }

            // Find insert position and remove old lump
            int insertpos = MapManager.RemoveSpecificLump(wad, "THINGS", position, MapManager.TEMP_MAP_HEADER, maplumps);
            if (insertpos == -1) insertpos = position + 1;
            if (insertpos > wad.Lumps.Count) insertpos = wad.Lumps.Count;

            // Create the lump from memory
            Lump lump = wad.Insert("THINGS", insertpos, (int)mem.Length);
            lump.Stream.Seek(0, SeekOrigin.Begin);
            mem.WriteTo(lump.Stream);
            mem.Flush();
        }
        #endregion
    }
}