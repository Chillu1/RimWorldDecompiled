using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Graphic_Linked : Graphic
	{
		protected Graphic subGraphic;

		public virtual LinkDrawerType LinkerType => LinkDrawerType.Basic;

		public override Material MatSingle => MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingle, LinkDirections.None);

		public Graphic_Linked()
		{
		}

		public Graphic_Linked(Graphic subGraphic)
		{
			this.subGraphic = subGraphic;
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return new Graphic_Linked(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
			{
				data = data
			};
		}

		public override void Print(SectionLayer layer, Thing thing)
		{
			Material mat = LinkedDrawMatFrom(thing, thing.Position);
			Printer_Plane.PrintPlane(layer, thing.TrueCenter(), new Vector2(1f, 1f), mat);
		}

		public override Material MatSingleFor(Thing thing)
		{
			return LinkedDrawMatFrom(thing, thing.Position);
		}

		protected Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
		{
			int num = 0;
			int num2 = 1;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = cell + GenAdj.CardinalDirections[i];
				if (ShouldLinkWith(c, parent))
				{
					num += num2;
				}
				num2 *= 2;
			}
			LinkDirections linkSet = (LinkDirections)num;
			return MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingleFor(parent), linkSet);
		}

		public virtual bool ShouldLinkWith(IntVec3 c, Thing parent)
		{
			if (!parent.Spawned)
			{
				return false;
			}
			if (!c.InBounds(parent.Map))
			{
				return (parent.def.graphicData.linkFlags & LinkFlags.MapEdge) != 0;
			}
			return (parent.Map.linkGrid.LinkFlagsAt(c) & parent.def.graphicData.linkFlags) != 0;
		}
	}
}
