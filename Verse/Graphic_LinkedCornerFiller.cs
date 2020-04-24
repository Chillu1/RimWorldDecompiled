using UnityEngine;

namespace Verse
{
	public class Graphic_LinkedCornerFiller : Graphic_Linked
	{
		private const float ShiftUp = 0.09f;

		private const float CoverSize = 0.5f;

		private static readonly float CoverSizeCornerCorner = new Vector2(0.5f, 0.5f).magnitude;

		private static readonly float DistCenterCorner = new Vector2(0.5f, 0.5f).magnitude;

		private static readonly float CoverOffsetDist = DistCenterCorner - CoverSizeCornerCorner * 0.5f;

		private static readonly Vector2[] CornerFillUVs = new Vector2[4]
		{
			new Vector2(0.5f, 0.6f),
			new Vector2(0.5f, 0.6f),
			new Vector2(0.5f, 0.6f),
			new Vector2(0.5f, 0.6f)
		};

		public override LinkDrawerType LinkerType => LinkDrawerType.CornerFiller;

		public Graphic_LinkedCornerFiller(Graphic subGraphic)
			: base(subGraphic)
		{
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return new Graphic_LinkedCornerFiller(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
			{
				data = data
			};
		}

		public override void Print(SectionLayer layer, Thing thing)
		{
			base.Print(layer, thing);
			IntVec3 position = thing.Position;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = thing.Position + GenAdj.DiagonalDirectionsAround[i];
				if (!ShouldLinkWith(c, thing) || (i == 0 && (!ShouldLinkWith(position + IntVec3.West, thing) || !ShouldLinkWith(position + IntVec3.South, thing))) || (i == 1 && (!ShouldLinkWith(position + IntVec3.West, thing) || !ShouldLinkWith(position + IntVec3.North, thing))) || (i == 2 && (!ShouldLinkWith(position + IntVec3.East, thing) || !ShouldLinkWith(position + IntVec3.North, thing))) || (i == 3 && (!ShouldLinkWith(position + IntVec3.East, thing) || !ShouldLinkWith(position + IntVec3.South, thing))))
				{
					continue;
				}
				Vector3 center = thing.DrawPos + GenAdj.DiagonalDirectionsAround[i].ToVector3().normalized * CoverOffsetDist + Altitudes.AltIncVect + new Vector3(0f, 0f, 0.09f);
				Vector2 size = new Vector2(0.5f, 0.5f);
				if (!c.InBounds(thing.Map))
				{
					if (c.x == -1)
					{
						center.x -= 1f;
						size.x *= 5f;
					}
					if (c.z == -1)
					{
						center.z -= 1f;
						size.y *= 5f;
					}
					if (c.x == thing.Map.Size.x)
					{
						center.x += 1f;
						size.x *= 5f;
					}
					if (c.z == thing.Map.Size.z)
					{
						center.z += 1f;
						size.y *= 5f;
					}
				}
				Printer_Plane.PrintPlane(layer, center, size, LinkedDrawMatFrom(thing, thing.Position), 0f, flipUv: false, CornerFillUVs);
			}
		}
	}
}
