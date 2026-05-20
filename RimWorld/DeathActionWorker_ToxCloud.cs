using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class DeathActionWorker_ToxCloud : DeathActionWorker
{
	public override bool DangerousInMelee => true;

	public override void PawnDied(Corpse corpse, Lord prevLord)
	{
		if (ModsConfig.BiotechActive)
		{
			GenExplosion.DoExplosion(radius: (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0) ? 0.9f : ((corpse.InnerPawn.ageTracker.CurLifeStageIndex != 1) ? 2.9f : 1.9f), center: corpse.Position, map: corpse.Map, damType: DamageDefOf.ToxGas, instigator: corpse.InnerPawn, damAmount: -1, armorPenetration: -1f, explosionSound: null, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnThingDef: null, postExplosionSpawnChance: 0f, postExplosionSpawnThingCount: 1, postExplosionGasType: GasType.ToxGas);
		}
	}
}
