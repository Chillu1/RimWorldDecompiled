using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class GenHostility
{
	public static bool HostileTo(this Thing a, Thing b)
	{
		if (a.Destroyed || b.Destroyed || a == b)
		{
			return false;
		}
		if ((a.Faction == null && a.TryGetComp<CompCauseGameCondition>() != null) || (b.Faction == null && b.TryGetComp<CompCauseGameCondition>() != null))
		{
			return true;
		}
		Pawn pawn = a as Pawn;
		Pawn pawn2 = b as Pawn;
		if (IsActivityDormant(pawn) || IsActivityDormant(pawn2))
		{
			return false;
		}
		if ((pawn != null && pawn.kindDef.hostileToAll) || (pawn2 != null && pawn2.kindDef.hostileToAll))
		{
			return true;
		}
		if (pawn != null && pawn2 != null && ((pawn.story != null && pawn.story.traits.DisableHostilityFrom(pawn2)) || (pawn2.story != null && pawn2.story.traits.DisableHostilityFrom(pawn))))
		{
			return false;
		}
		if ((pawn != null && pawn.MentalState != null && pawn.MentalState.ForceHostileTo(b)) || (pawn2 != null && pawn2.MentalState != null && pawn2.MentalState.ForceHostileTo(a)))
		{
			return true;
		}
		if (pawn != null && pawn2 != null && (IsPredatorHostileTo(pawn, pawn2) || IsPredatorHostileTo(pawn2, pawn)))
		{
			return true;
		}
		if ((a.Faction != null && pawn2 != null && pawn2.HostFaction == a.Faction && (pawn == null || pawn.HostFaction == null) && PrisonBreakUtility.IsPrisonBreaking(pawn2)) || (b.Faction != null && pawn != null && pawn.HostFaction == b.Faction && (pawn2 == null || pawn2.HostFaction == null) && PrisonBreakUtility.IsPrisonBreaking(pawn)))
		{
			return true;
		}
		if ((a.Faction != null && pawn2 != null && pawn2.IsSlave && pawn2.Faction == a.Faction && (pawn == null || !pawn.IsSlave) && SlaveRebellionUtility.IsRebelling(pawn2)) || (b.Faction != null && pawn != null && pawn.IsSlave && pawn.Faction == b.Faction && (pawn2 == null || !pawn2.IsSlave) && SlaveRebellionUtility.IsRebelling(pawn)))
		{
			return true;
		}
		if ((a.Faction != null && pawn2 != null && pawn2.HostFaction == a.Faction) || (b.Faction != null && pawn != null && pawn.HostFaction == b.Faction))
		{
			return false;
		}
		if (pawn != null && pawn.IsPrisoner && pawn2 != null && pawn2.IsPrisoner)
		{
			return false;
		}
		if (pawn != null && pawn.IsSlave && pawn2 != null && pawn2.IsSlave)
		{
			return false;
		}
		if (pawn != null && pawn2 != null && ((pawn.IsPrisoner && pawn.HostFaction == pawn2.HostFaction && !PrisonBreakUtility.IsPrisonBreaking(pawn)) || (pawn2.IsPrisoner && pawn2.HostFaction == pawn.HostFaction && !PrisonBreakUtility.IsPrisonBreaking(pawn2))))
		{
			return false;
		}
		if (pawn != null && pawn2 != null && ((pawn.HostFaction != null && pawn2.Faction != null && !pawn.HostFaction.HostileTo(pawn2.Faction) && !PrisonBreakUtility.IsPrisonBreaking(pawn)) || (pawn2.HostFaction != null && pawn.Faction != null && !pawn2.HostFaction.HostileTo(pawn.Faction) && !PrisonBreakUtility.IsPrisonBreaking(pawn2))))
		{
			return false;
		}
		if ((a.Faction != null && a.Faction.IsPlayer && pawn2 != null && pawn2.mindState.WillJoinColonyIfRescued) || (b.Faction != null && b.Faction.IsPlayer && pawn != null && pawn.mindState.WillJoinColonyIfRescued))
		{
			return false;
		}
		if (pawn != null && pawn2 != null && (pawn.ThreatDisabledBecauseNonAggressiveRoamer(pawn2) || pawn2.ThreatDisabledBecauseNonAggressiveRoamer(pawn)))
		{
			return false;
		}
		if ((pawn != null && MechanitorUtility.IsPlayerOverseerSubject(pawn) && !pawn.IsColonyMechPlayerControlled) || (pawn2 != null && MechanitorUtility.IsPlayerOverseerSubject(pawn2) && !pawn2.IsColonyMechPlayerControlled))
		{
			return false;
		}
		if ((pawn != null && pawn.Faction == null && pawn.RaceProps.Humanlike && b.Faction != null && b.Faction.def.hostileToFactionlessHumanlikes) || (pawn2 != null && pawn2.Faction == null && pawn2.RaceProps.Humanlike && a.Faction != null && a.Faction.def.hostileToFactionlessHumanlikes))
		{
			return true;
		}
		if (pawn != null && pawn2 != null && (pawn.IsShambler || pawn2.IsShambler))
		{
			return MutantUtility.CheckShamblerHostility(pawn, pawn2);
		}
		if ((pawn != null && b is Building_Turret && pawn.IsPsychologicallyInvisible()) || (pawn2 != null && a is Building_Turret && pawn2.IsPsychologicallyInvisible()))
		{
			return false;
		}
		if (a.Faction == null || b.Faction == null)
		{
			return false;
		}
		return a.Faction.HostileTo(b.Faction);
	}

	private static bool IsActivityDormant(Pawn pawn)
	{
		if (pawn == null)
		{
			return false;
		}
		CompCanBeDormant canBeDormant = pawn.canBeDormant;
		if (canBeDormant != null && !canBeDormant.Awake)
		{
			return false;
		}
		if (pawn.needs != null)
		{
			return pawn.activity?.IsDormant ?? false;
		}
		return false;
	}

	public static bool HostileTo(this Thing t, Faction fac)
	{
		if (t.Destroyed)
		{
			return false;
		}
		if (fac == null)
		{
			return false;
		}
		if (t is Pawn { MentalState: var mentalState } pawn)
		{
			if (mentalState != null && mentalState.ForceHostileTo(fac))
			{
				return true;
			}
			if (IsPredatorHostileTo(pawn, fac))
			{
				return true;
			}
			if (pawn.HostFaction == fac && PrisonBreakUtility.IsPrisonBreaking(pawn))
			{
				return true;
			}
			if (pawn.Faction == fac && SlaveRebellionUtility.IsRebelling(pawn))
			{
				return true;
			}
			if (pawn.IsShambler && pawn.Faction == null)
			{
				return true;
			}
			if (pawn.HostFaction == fac)
			{
				return false;
			}
			if (pawn.HostFaction != null && !pawn.HostFaction.HostileTo(fac) && !PrisonBreakUtility.IsPrisonBreaking(pawn))
			{
				return false;
			}
			if (fac.IsPlayer && pawn.mindState.WillJoinColonyIfRescued)
			{
				return false;
			}
			if (IsActivityDormant(pawn))
			{
				return false;
			}
			if (pawn.kindDef.hostileToAll)
			{
				return true;
			}
			if (fac.def.hostileToFactionlessHumanlikes && pawn.Faction == null && pawn.RaceProps.Humanlike)
			{
				return true;
			}
		}
		else if (t.Faction == null && t.TryGetComp<CompCauseGameCondition>() != null)
		{
			return true;
		}
		if (t.Faction == null)
		{
			return false;
		}
		return t.Faction.HostileTo(fac);
	}

	private static bool IsPredatorHostileTo(Pawn predator, Pawn toPawn)
	{
		if (toPawn.Faction == null)
		{
			return false;
		}
		if (toPawn.Faction.HasPredatorRecentlyAttackedAnyone(predator))
		{
			return true;
		}
		Pawn preyOfMyFaction = GetPreyOfMyFaction(predator, toPawn.Faction);
		if (preyOfMyFaction != null && predator.Position.InHorDistOf(preyOfMyFaction.Position, 12f))
		{
			return true;
		}
		return false;
	}

	private static bool IsPredatorHostileTo(Pawn predator, Faction toFaction)
	{
		if (toFaction.HasPredatorRecentlyAttackedAnyone(predator))
		{
			return true;
		}
		if (GetPreyOfMyFaction(predator, toFaction) != null)
		{
			return true;
		}
		return false;
	}

	private static Pawn GetPreyOfMyFaction(Pawn predator, Faction myFaction)
	{
		Job curJob = predator.CurJob;
		if (curJob != null && curJob.def == JobDefOf.PredatorHunt && !predator.jobs.curDriver.ended && curJob.GetTarget(TargetIndex.A).Thing is Pawn { Dead: false } pawn && pawn.Faction == myFaction)
		{
			return pawn;
		}
		return null;
	}

	public static bool AnyHostileActiveThreatToPlayer(Map map, bool countDormantPawnsAsHostile = false, bool canBeFogged = false)
	{
		return AnyHostileActiveThreatTo(map, Faction.OfPlayer, countDormantPawnsAsHostile, canBeFogged);
	}

	public static bool AnyHostileActiveThreatTo(Map map, Faction faction, bool countDormantPawnsAsHostile = false, bool canBeFogged = false)
	{
		IAttackTarget threat;
		return AnyHostileActiveThreatTo(map, faction, out threat, countDormantPawnsAsHostile, canBeFogged);
	}

	public static bool AnyHostileActiveThreatTo(Map map, Faction faction, out IAttackTarget threat, bool countDormantPawnsAsHostile = false, bool canBeFogged = false)
	{
		foreach (IAttackTarget item in map.attackTargetsCache.TargetsHostileToFaction(faction))
		{
			if (IsActiveThreatTo(item, faction, ignoreHives: true, canBeFogged))
			{
				threat = item;
				return true;
			}
			if (countDormantPawnsAsHostile && item.Thing.HostileTo(faction) && (canBeFogged || !item.Thing.Fogged()) && !item.ThreatDisabled(null) && item.Thing is Pawn pawn)
			{
				CompCanBeDormant comp = pawn.GetComp<CompCanBeDormant>();
				if (comp != null && !comp.Awake)
				{
					threat = item;
					return true;
				}
			}
		}
		threat = null;
		return false;
	}

	public static bool IsActiveThreatToPlayer(IAttackTarget target, bool canBeFogged = false)
	{
		return IsActiveThreatTo(target, Faction.OfPlayer, ignoreHives: true, canBeFogged);
	}

	public static bool IsPotentialThreat(IAttackTarget target)
	{
		if (!(target.Thing is IAttackTargetSearcher))
		{
			return false;
		}
		if (target.ThreatDisabled(null))
		{
			return false;
		}
		Pawn pawn = target.Thing as Pawn;
		if (pawn != null && (pawn.MentalStateDef == MentalStateDefOf.PanicFlee || pawn.IsPrisoner))
		{
			return false;
		}
		if (pawn != null && !pawn.Awake())
		{
			return false;
		}
		CompCanBeDormant compCanBeDormant = target.Thing.TryGetComp<CompCanBeDormant>();
		if (compCanBeDormant != null && !compCanBeDormant.Awake)
		{
			return false;
		}
		CompMechanoid compMechanoid = target.Thing.TryGetComp<CompMechanoid>();
		if (compMechanoid != null && compMechanoid.Deactivated)
		{
			return false;
		}
		CompInitiatable compInitiatable = target.Thing.TryGetComp<CompInitiatable>();
		if (compInitiatable != null && !compInitiatable.Initiated)
		{
			return false;
		}
		if (target.Thing.Spawned && target.Thing.Map.generatorDef.defeatRequiresCantReachUnfogged)
		{
			TraverseParms traverseParms = ((pawn != null) ? TraverseParms.For(pawn) : TraverseParms.For(TraverseMode.PassDoors));
			if (!target.Thing.Map.reachability.CanReachUnfogged(target.Thing.Position, traverseParms))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsActiveThreatTo(IAttackTarget target, Faction faction, bool ignoreHives = true, bool canBeFogged = false)
	{
		if (!target.Thing.HostileTo(faction))
		{
			return false;
		}
		if (!canBeFogged && target.Thing.Fogged())
		{
			return false;
		}
		if (ignoreHives && target.Thing is Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			if (lord != null && lord.LordJob is LordJob_DefendAndExpandHive && (pawn.mindState.duty == null || pawn.mindState.duty.def != DutyDefOf.AssaultColony))
			{
				return false;
			}
		}
		if (!IsPotentialThreat(target))
		{
			return false;
		}
		return true;
	}

	public static bool IsDefMechClusterThreat(ThingDef def)
	{
		if (def.building != null && (def.building.IsTurret || def.building.IsMortar))
		{
			return true;
		}
		return def.isMechClusterThreat;
	}

	public static void Notify_PawnLostForTutor(Pawn pawn, Map map)
	{
		if (!map.IsPlayerHome && map.mapPawns.FreeColonistsSpawnedCount != 0 && !AnyHostileActiveThreatToPlayer(map))
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ReformCaravan, OpportunityType.Important);
		}
	}
}
