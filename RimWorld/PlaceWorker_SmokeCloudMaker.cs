using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_SmokeCloudMaker : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		CompProperties_SmokeCloudMaker compProperties_SmokeCloudMaker = def.GetCompProperties<CompProperties_SmokeCloudMaker>() ?? (def.entityDefToBuild as ThingDef)?.GetCompProperties<CompProperties_SmokeCloudMaker>();
		if (compProperties_SmokeCloudMaker != null)
		{
			GenDraw.DrawCircleOutline(center.ToVector3Shifted(), compProperties_SmokeCloudMaker.cloudRadius);
		}
	}
}
