using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class DamageWorker_Flame : DamageWorker_AddInjury
	{
		public override DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			if (pawn != null && pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}
			Map map = victim.Map;
			DamageResult damageResult = base.Apply(dinfo, victim);
			if (!damageResult.deflected && !dinfo.InstantPermanentInjury)
			{
				victim.TryAttachFire(Rand.Range(0.15f, 0.25f));
			}
			if (victim.Destroyed && map != null && pawn == null)
			{
				foreach (IntVec3 item in victim.OccupiedRect())
				{
					FilthMaker.TryMakeFilth(item, map, ThingDefOf.Filth_Ash);
				}
				Plant plant = victim as Plant;
				if (plant != null && victim.def.plant.IsTree && plant.LifeStage != 0 && victim.def != ThingDefOf.BurnedTree)
				{
					((DeadPlant)GenSpawn.Spawn(ThingDefOf.BurnedTree, victim.Position, map)).Growth = plant.Growth;
				}
			}
			return damageResult;
		}

		public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
		{
			base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
			if (def == DamageDefOf.Flame && Rand.Chance(FireUtility.ChanceToStartFireIn(c, explosion.Map)))
			{
				FireUtility.TryStartFireIn(c, explosion.Map, Rand.Range(0.2f, 0.6f));
			}
		}
	}
}
