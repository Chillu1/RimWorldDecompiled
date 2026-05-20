using UnityEngine;
using Verse;

namespace RimWorld;

public class Blueprint_Door : Blueprint_Build
{
	public override Graphic Graphic => base.DefaultGraphic;

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (def.entityDefToBuild.Size == IntVec2.One)
		{
			base.Rotation = DoorUtility.DoorRotationAt(base.Position, base.Map, base.BuildDef.building.preferConnectingToFences);
		}
		base.DrawAt(drawLoc, flip);
	}
}
