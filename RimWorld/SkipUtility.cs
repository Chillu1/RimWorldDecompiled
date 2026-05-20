using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class SkipUtility
{
	public static Thing SkipTo(Thing thing, IntVec3 cell, Map dest)
	{
		if (thing.Spawned)
		{
			SkipDeSpawn(thing);
		}
		Thing thing2 = SkipSpawn(thing, cell, dest);
		if (thing2 is Pawn pawn)
		{
			pawn.Notify_Teleported();
		}
		return thing2;
	}

	public static Job GetEntitySkipJob(Pawn pawn, IntVec3 cell, AbilityDef abilityDef = null)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return null;
		}
		return pawn.abilities.GetAbility(abilityDef ?? AbilityDefOf.EntitySkip, includeTemporary: true)?.GetJob(pawn, cell);
	}

	public static bool CanEntitySkipNow(Pawn pawn, AbilityDef abilityDef = null)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		Ability ability = pawn.abilities.GetAbility(abilityDef ?? AbilityDefOf.EntitySkip, includeTemporary: true);
		if (ability == null)
		{
			return false;
		}
		return !ability.OnCooldown;
	}

	public static void SkipDeSpawn(Thing thing)
	{
		if (!thing.Spawned)
		{
			Log.ErrorOnce("Cannot skip despawn spawn a thing which is already despawned.", 89689431);
			return;
		}
		if (thing is Pawn pawn)
		{
			if (pawn.carryTracker.CarriedThing != null && !pawn.Drafted)
			{
				pawn.carryTracker.TryDropCarriedThing(thing.Position, ThingPlaceMode.Direct, out var _);
			}
			if (pawn.drafter != null)
			{
				pawn.wasDraftedBeforeSkip = pawn.drafter.Drafted;
			}
		}
		EffecterDefOf.Skip_EntryNoDelay.Spawn(thing, thing.MapHeld).Cleanup();
		thing.DeSpawnOrDeselect();
	}

	private static Thing SkipSpawn(Thing thing, IntVec3 cell, Map dest)
	{
		if (thing.Spawned)
		{
			Log.ErrorOnce("Cannot skip spawn a thing which is already spawned, use SkipTo or call SkipDeSpawn first.", 47256283);
			return null;
		}
		Thing thing2 = GenSpawn.Spawn(thing, cell, dest, thing.def.defaultPlacingRot);
		EffecterDefOf.Skip_Exit.Spawn(thing2, dest).Cleanup();
		if (thing is Pawn pawn)
		{
			if (pawn.TryGetFormingCaravanLord(out var lord) && lord.Map != pawn.Map)
			{
				CaravanFormingUtility.RemovePawnFromCaravan(pawn, pawn.GetLord(), removeFromDowned: false);
			}
			if (pawn.drafter != null && pawn.wasDraftedBeforeSkip)
			{
				pawn.drafter.Drafted = true;
			}
		}
		return thing2;
	}
}
