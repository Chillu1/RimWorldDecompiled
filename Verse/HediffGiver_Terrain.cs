using RimWorld;
using UnityEngine;

namespace Verse;

public class HediffGiver_Terrain : HediffGiver
{
	public override void OnIntervalPassed(Pawn pawn, Hediff cause)
	{
		if (pawn.Spawned)
		{
			TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
			if (terrain.ignitePawnsIntervalTicks > 0f && Rand.MTBEventOccurs(terrain.ignitePawnsIntervalTicks, 1f, 60f) && Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(pawn)))
			{
				pawn.TryAttachFire(Rand.Range(0.15f, 0.25f), null);
			}
			if (terrain.burnDamage > 0 && Rand.MTBEventOccurs(terrain.burnIntervalTicks, 1f, 60f))
			{
				int num = Mathf.Max(terrain.burnDamage, 3);
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Burn, num);
				dinfo.SetBodyRegion(BodyPartHeight.Bottom, BodyPartDepth.Outside);
				pawn.TakeDamage(dinfo);
			}
		}
	}
}
