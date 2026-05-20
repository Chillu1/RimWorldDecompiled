using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_CocoonTriggerRadius : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			CompProperties_WakeUpDormant compProperties = def.GetCompProperties<CompProperties_WakeUpDormant>();
			if (compProperties != null)
			{
				GenDraw.DrawRadiusRing(center, compProperties.wakeUpCheckRadius, Color.white);
			}
		}
	}
}
