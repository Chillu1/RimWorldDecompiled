using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowExplosionRadius : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			CompProperties_Explosive compProperties = def.GetCompProperties<CompProperties_Explosive>();
			if (compProperties != null)
			{
				GenDraw.DrawRadiusRing(center, compProperties.explosiveRadius);
			}
		}
	}
}
