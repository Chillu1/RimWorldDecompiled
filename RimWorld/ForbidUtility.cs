using Verse;
using Verse.AI.Group;

namespace RimWorld;

public static class ForbidUtility
{
	public static void TrySetForbidden(this Thing t, bool value)
	{
		if (t is ThingWithComps thing && thing.TryGetComp<CompForbiddable>(out var comp))
		{
			comp.Forbidden = value;
		}
	}

	public static void SetForbidden(this Thing t, bool value, bool warnOnFail = true)
	{
		if (t == null)
		{
			if (warnOnFail)
			{
				Log.Error("Tried to SetForbidden on null Thing.");
			}
			return;
		}
		if (!(t is ThingWithComps thingWithComps))
		{
			if (warnOnFail)
			{
				Log.Error("Tried to SetForbidden on non-ThingWithComps Thing " + t);
			}
			return;
		}
		CompForbiddable comp = thingWithComps.GetComp<CompForbiddable>();
		if (comp == null)
		{
			if (warnOnFail)
			{
				Log.Error("Tried to SetForbidden on non-Forbiddable Thing " + t);
			}
		}
		else
		{
			comp.Forbidden = value;
		}
	}

	public static void SetForbiddenIfOutsideHomeArea(this Thing t)
	{
		if (!t.Spawned)
		{
			Log.Error("SetForbiddenIfOutsideHomeArea unspawned thing " + t);
		}
		if (t.Position.InBounds(t.Map) && !t.Map.areaManager.Home[t.Position])
		{
			t.SetForbidden(value: true, warnOnFail: false);
		}
	}

	public static bool CaresAboutForbidden(Pawn pawn, bool cellTarget, bool bypassDraftedCheck = false)
	{
		if (pawn.HostFaction != null && (pawn.HostFaction != Faction.OfPlayer || !pawn.Spawned || pawn.Map.IsPlayerHome || (pawn.GetRoom() != null && pawn.GetRoom().IsPrisonCell) || (pawn.IsPrisoner && !pawn.guest.PrisonerIsSecure)))
		{
			return false;
		}
		if (!bypassDraftedCheck && pawn.Drafted)
		{
			return false;
		}
		if (pawn.InMentalState)
		{
			return false;
		}
		if (SlaveRebellionUtility.IsRebelling(pawn))
		{
			return false;
		}
		if (cellTarget && ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn))
		{
			return false;
		}
		if (pawn.IsColonyMechRequiringMechanitor())
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && pawn.kindDef == PawnKindDefOf.Revenant)
		{
			return false;
		}
		return true;
	}

	public static bool InAllowedArea(this IntVec3 c, Pawn forPawn)
	{
		if (forPawn.playerSettings != null)
		{
			Area effectiveAreaRestrictionInPawnCurrentMap = forPawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap;
			if (effectiveAreaRestrictionInPawnCurrentMap != null && effectiveAreaRestrictionInPawnCurrentMap.TrueCount > 0 && !effectiveAreaRestrictionInPawnCurrentMap[c])
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsForbiddenHeld(this Thing t, Pawn pawn)
	{
		return t.SpawnedParentOrMe.IsForbidden(pawn);
	}

	public static bool IsForbidden(this Thing t, Pawn pawn)
	{
		if (!CaresAboutForbidden(pawn, cellTarget: false))
		{
			return false;
		}
		if ((t.Spawned || t.SpawnedParentOrMe != pawn) && t.PositionHeld.IsForbidden(pawn))
		{
			return true;
		}
		if (t.IsForbidden(pawn.Faction) || t.IsForbidden(pawn.HostFaction))
		{
			return true;
		}
		Lord lord = pawn.GetLord();
		if (lord != null && lord.extraForbiddenThings.Contains(t))
		{
			return true;
		}
		foreach (Lord lord2 in pawn.MapHeld.lordManager.lords)
		{
			if (lord2.CurLordToil is LordToil_Ritual lordToil_Ritual && lordToil_Ritual.ReservedThings.Contains(t) && lord2 != lord)
			{
				return true;
			}
			if (lord2.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual && lordToil_PsychicRitual.RitualData.psychicRitual.def is PsychicRitualDef_InvocationCircle { TargetRole: not null } psychicRitualDef_InvocationCircle && lordToil_PsychicRitual.RitualData.psychicRitual.assignments.FirstAssignedPawn(psychicRitualDef_InvocationCircle.TargetRole) == t && !(lordToil_PsychicRitual.RitualData.CurPsychicRitualToil is PsychicRitualToil_TargetCleanup) && lord2 != lord)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsForbiddenToPass(this Building_Door t, Pawn pawn)
	{
		if (!CaresAboutForbidden(pawn, cellTarget: false, bypassDraftedCheck: true))
		{
			return false;
		}
		if (t.IsForbidden(pawn.Faction))
		{
			return true;
		}
		return false;
	}

	public static bool IsForbidden(this IntVec3 c, Pawn pawn)
	{
		if (!CaresAboutForbidden(pawn, cellTarget: true))
		{
			return false;
		}
		if (!c.InAllowedArea(pawn))
		{
			return true;
		}
		if (pawn.mindState.maxDistToSquadFlag > 0f && !c.InHorDistOf(pawn.DutyLocation(), pawn.mindState.maxDistToSquadFlag))
		{
			return true;
		}
		return false;
	}

	public static bool IsForbiddenEntirely(this Region r, Pawn pawn)
	{
		if (!CaresAboutForbidden(pawn, cellTarget: true))
		{
			return false;
		}
		if (pawn.playerSettings != null)
		{
			Area effectiveAreaRestrictionInPawnCurrentMap = pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap;
			if (effectiveAreaRestrictionInPawnCurrentMap != null && effectiveAreaRestrictionInPawnCurrentMap.TrueCount > 0 && effectiveAreaRestrictionInPawnCurrentMap.Map == r.Map && r.OverlapWith(effectiveAreaRestrictionInPawnCurrentMap) == AreaOverlap.None)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsForbidden(this Thing t, Faction faction)
	{
		if (faction == null)
		{
			return false;
		}
		if (faction != Faction.OfPlayer)
		{
			return false;
		}
		if (!(t is ThingWithComps { compForbiddable: var compForbiddable }))
		{
			return false;
		}
		return compForbiddable?.Forbidden ?? false;
	}
}
