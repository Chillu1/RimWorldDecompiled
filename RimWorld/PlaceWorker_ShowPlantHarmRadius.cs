using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowPlantHarmRadius : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			if (thing == null)
			{
				return;
			}
			CompPlantHarmRadius compPlantHarmRadius = thing.TryGetComp<CompPlantHarmRadius>();
			if (compPlantHarmRadius != null)
			{
				float currentRadius = compPlantHarmRadius.CurrentRadius;
				if (currentRadius < 50f)
				{
					GenDraw.DrawRadiusRing(center, currentRadius);
				}
			}
		}
	}
}
