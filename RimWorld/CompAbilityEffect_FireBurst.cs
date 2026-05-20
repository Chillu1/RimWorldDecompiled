using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_FireBurst : CompAbilityEffect
{
	private new CompProperties_AbilityFireBurst Props => (CompProperties_AbilityFireBurst)props;

	private Pawn Pawn => parent.pawn;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		GenExplosion.DoExplosion(Pawn.Position, Pawn.MapHeld, Props.radius, DamageDefOf.Flame, Pawn, -1, -1f, null, null, null, null, ThingDefOf.Filth_Fuel, 1f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 1f, damageFalloff: false, null, null, null, doVisualEffects: false, 0.6f);
		base.Apply(target, dest);
	}

	public override IEnumerable<PreCastAction> GetPreCastActions()
	{
		yield return new PreCastAction
		{
			action = delegate
			{
				parent.AddEffecterToMaintain(EffecterDefOf.Fire_Burst.Spawn(parent.pawn.Position, parent.pawn.Map), parent.pawn.Position, 17, parent.pawn.Map);
			},
			ticksAwayFromCast = 17
		};
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		if (Pawn.Faction == Faction.OfPlayer)
		{
			return false;
		}
		if (target.HasThing && target.Thing is Pawn pawn)
		{
			return pawn.TargetCurrentlyAimingAt == Pawn;
		}
		return false;
	}

	public override void CompTickInterval(int delta)
	{
		if (parent.Casting)
		{
			FireBurstUtility.ThrowFuelTick(Pawn.Position, Props.radius, Pawn.Map);
		}
	}
}
