#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Map
{
    public class BSP
	{
		#region ================== Constants		
		#endregion
		
		#region ================== Variables

		private Seg[] segs;
		private Node[] nodes;
		private Vector2D[] verts;
		private Subsector[] ssectors;
        private bool deactivate;
        private bool isdisposed;
        private string errormessage;
        #endregion

        #region ================== Properties

        public Seg[] Segs { get { return segs; } }
		public Node[] Nodes { get { return nodes; } }
		public Vector2D[] Vertices { get { return verts; } }
		public Subsector[] Subsectors { get { return ssectors; } }
        public bool IsDeactivated { get { return deactivate; } }
        public string ErrorMessage { get { return errormessage; } }
		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public BSP(bool deactivate)
        {
            this.deactivate = deactivate;
            this.errormessage = "";
            if (!deactivate && !General.Map.IsChanged) LoadStructures();
        }

        // Disposer
        public void Dispose()
        {
            // Not already disposed?
            if (!isdisposed)
            {
                // Clean up
                segs = null;
                nodes = null;
                verts = null;
                ssectors = null;

                // Done
                isdisposed = true;
            }
        }
        #endregion

        #region ================== Methods

        public void Build()
        {
            if (!deactivate && General.Map.IsChanged)
            {
                if (!General.Map.RebuildNodes(General.Map.ConfigSettings.NodebuilderTest, true)) return;
                LoadStructures();
            }
        }

		/// <summary>
		/// This loads all nodes structures data from the lumps
		/// </summary>
		private bool LoadStructures()
		{
            errormessage = "";
			// Load the nodes structure
			MemoryStream nodesstream = General.Map.GetLumpData("NODES");
            if (nodesstream == null)
            {
                deactivate = true;
                return false;
            }
            int numnodes = (int)nodesstream.Length / 28;

			//mxd. Boilerplate!
			if(numnodes < 1)
			{
                errormessage = "The map has only one subsector.";
                deactivate = true;
				return false;
			}

			BinaryReader nodesreader = new BinaryReader(nodesstream);
			nodes = new Node[numnodes];
			for(int i = 0; i < nodes.Length; i++)
			{
				nodes[i].linestart.x = nodesreader.ReadInt16();
				nodes[i].linestart.y = nodesreader.ReadInt16();
				nodes[i].linedelta.x = nodesreader.ReadInt16();
				nodes[i].linedelta.y = nodesreader.ReadInt16();
				float top = nodesreader.ReadInt16();
				float bot = nodesreader.ReadInt16();
				float left = nodesreader.ReadInt16();
				float right = nodesreader.ReadInt16();
				nodes[i].rightbox = new RectangleF(left, top, (right - left), (bot - top));
				top = nodesreader.ReadInt16();
				bot = nodesreader.ReadInt16();
				left = nodesreader.ReadInt16();
				right = nodesreader.ReadInt16();
				nodes[i].leftbox = new RectangleF(left, top, (right - left), (bot - top));
				int rightindex = nodesreader.ReadInt16();
				int leftindex = nodesreader.ReadInt16();
				nodes[i].rightchild = rightindex & 0x7FFF;
				nodes[i].leftchild = leftindex & 0x7FFF;
				nodes[i].rightsubsector = (rightindex & 0x8000) != 0;
				nodes[i].leftsubsector = (leftindex & 0x8000) != 0;
			}
			nodesreader.Close();
			nodesstream.Close();
			nodesstream.Dispose();

			// Add additional properties to nodes
			nodes[nodes.Length - 1].parent = -1;
			RecursiveSetupNodes(nodes.Length - 1);

			// Load the segs structure
			MemoryStream segsstream = General.Map.GetLumpData("SEGS");
			BinaryReader segsreader = new BinaryReader(segsstream);
			int numsegs = (int)segsstream.Length / 12;

			//mxd. Boilerplate!
			if(numsegs < 1) 
			{
				errormessage = "The map has an empty SEGS lump.";
                deactivate = true;
                return false;
			}

			segs = new Seg[numsegs];
			for(int i = 0; i < segs.Length; i++)
			{
				segs[i].startvertex = segsreader.ReadInt16();
				segs[i].endvertex = segsreader.ReadInt16();
				segs[i].angle = Angle2D.DoomToReal(segsreader.ReadInt16());
				segs[i].lineindex = segsreader.ReadInt16();
				segs[i].leftside = segsreader.ReadInt16() != 0;
				segs[i].offset = segsreader.ReadInt16();
			}
			segsreader.Close();
			segsstream.Close();
			segsstream.Dispose();

			// Load the vertexes structure
			MemoryStream vertsstream = General.Map.GetLumpData("VERTEXES");
			BinaryReader vertsreader = new BinaryReader(vertsstream);
			int numverts = (int)vertsstream.Length / 4;

			//mxd. Boilerplate!
			if(numverts < 1) 
			{
                errormessage = "The map has an empty VERTEXES lump.";
                deactivate = true;
                return false;
			}

			verts = new Vector2D[numverts];
			for(int i = 0; i < verts.Length; i++)
			{
				verts[i].x = vertsreader.ReadInt16();
				verts[i].y = vertsreader.ReadInt16();
			}
			vertsreader.Close();
			vertsstream.Close();
			vertsstream.Dispose();

			// Load the subsectors structure
			MemoryStream ssecstream = General.Map.GetLumpData("SSECTORS");
			BinaryReader ssecreader = new BinaryReader(ssecstream);
			int numssec = (int)ssecstream.Length / 4;

			//mxd. Boilerplate!
			if(numssec < 1) 
			{
                errormessage = "The map has an empty SSECTORS lump.";
                deactivate = true;
                return false;
			}

			ssectors = new Subsector[numssec];
			for(int i = 0; i < ssectors.Length; i++)
			{
				ssectors[i].numsegs = ssecreader.ReadInt16();
				ssectors[i].firstseg = ssecreader.ReadUInt16();
			}
			ssecreader.Close();
			ssecstream.Close();
			ssecstream.Dispose();

			// Link all segs to their subsectors
			for(int i = 0; i < ssectors.Length; i++)
			{
				int lastseg = ssectors[i].firstseg + ssectors[i].numsegs - 1;
				for(int sg = ssectors[i].firstseg; sg <= lastseg; sg++)
				{
					segs[sg].ssector = i;
				}
			}

			return true;
		}

		/// <summary>
		/// This recursively sets up the nodes structure with additional properties
		/// </summary>
		private void RecursiveSetupNodes(int nodeindex)
		{
			Node n = nodes[nodeindex];
			if(!n.leftsubsector)
			{
				nodes[n.leftchild].parent = nodeindex;
				RecursiveSetupNodes(n.leftchild);
			}
			if(!n.rightsubsector)
			{
				nodes[n.rightchild].parent = nodeindex;
				RecursiveSetupNodes(n.rightchild);
			}
		}

        private bool PointOnSide(Vector2D p, Node node)
        {
            if (node.linedelta.x == 0)
                return p.x <= node.linestart.x ? node.linedelta.y > 0 : node.linedelta.y < 0;

            if (node.linedelta.y == 0)
                return p.y <= node.linestart.y ? node.linedelta.x < 0 : node.linedelta.x > 0;

            float dx = p.x - node.linestart.x;
            float dy = p.y - node.linestart.y;

            // Try to quickly decide by looking at sign bits.
            if (((node.linedelta.y < 0) ^ (node.linedelta.x < 0) ^ (dx < 0) ^ (dy < 0)))
                return (node.linedelta.y < 0) ^ (dx < 0);  // (left is negative)
            return dy*node.linedelta.x/65536 >= dx*node.linedelta.y/65536;
        }

        private Subsector PointInSubsector(Vector2D p)
        {
            int nodenum = nodes.Length - 1;
            bool reachedsubsector = false;

            while (!reachedsubsector)
            {
                bool side = !PointOnSide(p, nodes[nodenum]);
                reachedsubsector = side ? nodes[nodenum].rightsubsector : nodes[nodenum].leftsubsector;
                nodenum = side ? nodes[nodenum].rightchild : nodes[nodenum].leftchild;
            }

            return ssectors[nodenum];
        }

        public Sector GetSector(Vector2D p)
        {
            Subsector ss = PointInSubsector(p);
            Seg seg = segs[ss.firstseg];
            Linedef line = General.Map.Map.GetLinedefByIndex(seg.lineindex);
            Sidedef sidedef = seg.leftside ? line.Back : line.Front;
            return sidedef.Sector;
        }
        #endregion
    }
}
