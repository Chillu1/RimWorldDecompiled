using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowCompSendSignalOnPawnProximityRadius : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			CompProperties_SendSignalOnPawnProximity compProperties = def.GetCompProperties<CompProperties_SendSignalOnPawnProximity>();
			if (compProperties != null)
			{
				GenDraw.DrawRadiusRing(center, compProperties.radius);
			}
		}
	}
}
