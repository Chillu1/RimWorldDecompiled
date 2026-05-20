using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class PlaceWorker_SpectatorPreview : PlaceWorker
{
	private static Graphic spectatingPawnsGraphic;

	private static Graphic SpectatingPawnsGraphic
	{
		get
		{
			if (spectatingPawnsGraphic == null)
			{
				spectatingPawnsGraphic = GraphicDatabase.Get<Graphic_Multi>("UI/Overlays/PawnsSpectating", ShaderDatabase.Transparent);
			}
			return spectatingPawnsGraphic;
		}
	}

	public static void DrawSpectatorPreview(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, bool useArrow, out CellRect rect, Thing thing = null)
	{
		rect = GenAdj.OccupiedRect(center, rot, def.size);
		RitualFocusProperties ritualFocus = def.ritualFocus;
		if (ritualFocus == null && def.entityDefToBuild is ThingDef thingDef)
		{
			ritualFocus = thingDef.ritualFocus;
		}
		if (ritualFocus == null)
		{
			return;
		}
		foreach (SpectateRectSide allSelectedItem in ritualFocus.allowedSpectateSides.GetAllSelectedItems<SpectateRectSide>())
		{
			if (allSelectedItem.ValidSingleSide())
			{
				Rot4 rot2 = allSelectedItem.Rotated(rot).AsRot4();
				if (useArrow)
				{
					GenDraw.DrawArrowRotated(allSelectedItem.GraphicOffsetForRect(center, rect, rot, Vector2.zero), rot2.AsAngle, ghost: true);
				}
				Vector2 vector = (rot2.IsHorizontal ? new Vector2(2f, 4f) : new Vector2(4f, 2f));
				bool flag = (allSelectedItem & SpectateRectSide.Horizontal) != SpectateRectSide.Horizontal;
				Vector2 vector2 = vector - new Vector2(0.5f, 0.5f);
				GenDraw.DrawMeshNowOrLater(SpectatingPawnsGraphic.MeshAt(rot2.Opposite), Matrix4x4.TRS(allSelectedItem.GraphicOffsetForRect(center, rect, rot, flag ? vector2 : (-vector2)) + new Vector3(0f, 8f, 0f), Quaternion.identity, new Vector3(vector.x, 1f, vector.y)), SpectatingPawnsGraphic.MatAt(rot2.Opposite), drawNow: false);
			}
		}
	}

	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		DrawSpectatorPreview(def, center, rot, ghostCol, useArrow: true, out var _, thing);
	}
}
