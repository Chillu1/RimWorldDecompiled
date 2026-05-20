using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_GlowRadius : PlaceWorker
{
	private static readonly Color RingColor = new Color(0.8f, 0.8f, 0.4f);

	private const float RadiusOffset = -2.1f;

	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		if (thing != null)
		{
			return;
		}
		CompProperties_Glower compProperties = def.GetCompProperties<CompProperties_Glower>();
		if (compProperties != null)
		{
			float num = compProperties.glowRadius + -2.1f;
			if (num > 0f)
			{
				GenDraw.DrawRadiusRing(center, num, RingColor);
			}
		}
	}
}
