using UnityEngine;
using Verse;

namespace RimWorld;

public class Building_MultiTileDoor : Building_SupportedDoor
{
	private bool tmpStuckOpen;

	private const float UpperMoverOffsetStart = 0.25f;

	private const float UpperMoverOffsetEnd = 0.6f;

	private static readonly float UpperMoverAltitude = AltitudeLayer.DoorMoveable.AltitudeFor() + 0.018292684f;

	private static readonly Vector3 MoverDrawScale = new Vector3(0.5f, 1f, 1f);

	protected override bool CanDrawMovers => false;

	protected override void Tick()
	{
		base.Tick();
		tmpStuckOpen = base.StuckOpen;
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		DoorPreDraw();
		if (!tmpStuckOpen)
		{
			float offsetDist = 0.25f + 0.35000002f * OpenPct;
			DrawMovers(drawLoc, offsetDist, Graphic, AltitudeLayer.DoorMoveable.AltitudeFor(), MoverDrawScale, Graphic.ShadowGraphic);
			if (def.building.upperMoverGraphic != null)
			{
				float offsetDist2 = 0.25f + 0.35000002f * Mathf.Clamp01(OpenPct * 2.5f);
				DrawMovers(drawLoc, offsetDist2, def.building.upperMoverGraphic.Graphic, UpperMoverAltitude, MoverDrawScale, null);
			}
		}
		base.DrawAt(drawLoc, flip);
	}
}
