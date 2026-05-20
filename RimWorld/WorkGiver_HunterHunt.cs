using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_HunterHunt : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.OnCell;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Hunt))
		{
			yield return item.target.Thing;
		}
	}

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (!HasHuntingWeapon(pawn))
		{
			return true;
		}
		if (HasShieldAndRangedWeapon(pawn))
		{
			return true;
		}
		if (!pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Hunt))
		{
			return true;
		}
		return false;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn pawn2) || !pawn2.AnimalOrWildMan())
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Hunt) == null)
		{
			return false;
		}
		if (HistoryEventUtility.IsKillingInnocentAnimal(pawn, pawn2) && !new HistoryEvent(HistoryEventDefOf.KilledInnocentAnimal, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return false;
		}
		if (pawn.Ideo != null && pawn.Ideo.IsVeneratedAnimal(pawn2) && !new HistoryEvent(HistoryEventDefOf.HuntedVeneratedAnimal, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return false;
		}
		if (!CanFindHuntingPosition(pawn, pawn2))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.Hunt, t);
	}

	public static bool HasHuntingWeapon(Pawn p)
	{
		if (p.equipment.Primary != null && p.equipment.Primary.def.IsRangedWeapon && p.equipment.PrimaryEq.PrimaryVerb.HarmsHealth() && !p.equipment.PrimaryEq.PrimaryVerb.UsesExplosiveProjectiles())
		{
			return true;
		}
		return false;
	}

	private bool CanFindHuntingPosition(Pawn hunter, Pawn animal)
	{
		CastPositionRequest newReq = new CastPositionRequest
		{
			caster = hunter,
			target = animal,
			verb = hunter.TryGetAttackVerb(animal),
			wantCoverFromTarget = false
		};
		newReq.maxRangeFromTarget = (animal.Downed ? Mathf.Min(newReq.verb.EffectiveRange, animal.RaceProps.executionRange) : Mathf.Max(newReq.verb.EffectiveRange * 0.95f, 1.42f));
		IntVec3 dest;
		return CastPositionFinder.TryFindCastPosition(newReq, out dest);
	}

	public static bool HasShieldAndRangedWeapon(Pawn p)
	{
		if (p.equipment.Primary != null && p.equipment.Primary.def.IsWeaponUsingProjectiles)
		{
			List<Apparel> wornApparel = p.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (wornApparel[i].def.IsShieldThatBlocksRanged)
				{
					return true;
				}
			}
		}
		return false;
	}
}
