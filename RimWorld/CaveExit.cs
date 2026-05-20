using UnityEngine;
using Verse;

namespace RimWorld;

public class CaveExit : PocketMapExit
{
	private static readonly Vector3 RopeDrawOffset = new Vector3(0f, 1f, 1f);

	[Unsaved(false)]
	private Graphic cachedRopeGraphic;

	private Graphic RopeGraphic
	{
		get
		{
			if (cachedRopeGraphic == null)
			{
				cachedRopeGraphic = GraphicDatabase.Get<Graphic_Single_AgeSecs>("Things/Building/Misc/CaveExit/CaveExit_Rope", ShaderDatabase.CaveExitRope, def.graphicData.drawSize, Color.white);
			}
			return cachedRopeGraphic;
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		RopeGraphic.Draw(DrawPos + RopeDrawOffset, Rot4.North, this);
	}
}
