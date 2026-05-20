using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualRoleDef : Def, ILordJobRole
{
	[Flags]
	public enum Condition : uint
	{
		None = 0u,
		Dead = 1u,
		Downed = 2u,
		Drafted = 4u,
		Sleeping = 8u,
		Bleeding = 0x10u,
		Burning = 0x20u,
		MentalState = 0x40u,
		ExtremeTemperature = 0x80u,
		Humanlike = 0x100u,
		NonHumanlike = 0x200u,
		Prisoner = 0x400u,
		Slave = 0x800u,
		Freeman = 0x1000u,
		InsideBuilding = 0x2000u,
		NonPlayerFaction = 0x4000u,
		NotOnMap = 0x8000u,
		HasAnotherLord = 0x10000u,
		LeftVoluntarily = 0x20000u,
		Vanished = 0x40000u,
		Baby = 0x80000u,
		Child = 0x100000u,
		Adult = 0x200000u,
		NoPath = 0x400000u,
		SubHuman = 0x800000u,
		IdeoUnwilling = 0x1000000u,
		NoPsychicSensitivity = 0x2000000u,
		TemporaryFactionMember = 0x4000000u,
		Error = 0x8000000u,
		All = uint.MaxValue,
		Default = 0x4301900u,
		DefaultInvoker = 0x301900u
	}

	public enum Context
	{
		InvokePsychicRitualGizmo,
		Dialog_BeginPsychicRitual,
		Runtime,
		NonPlayerFaction
	}

	public struct Reason
	{
		public PsychicRitualRoleDef def;

		public Context context;

		public Pawn pawn;

		public TargetInfo target;

		public AnyEnum reasonCode;

		public static Reason None => default(Reason);

		public Reason(PsychicRitualRoleDef def, Context context, Pawn pawn, TargetInfo target, AnyEnum reasonCode)
		{
			this.def = def;
			this.context = context;
			this.pawn = pawn;
			this.target = target;
			this.reasonCode = reasonCode;
		}

		public TaggedString ToPlayerReadable()
		{
			return def?.PawnCannotDoReason(reasonCode, context, pawn, target) ?? TaggedString.Empty;
		}
	}

	public static IReadOnlyDictionary<DevelopmentalStage, Condition> DevelopmentalStageToCondition = new Dictionary<DevelopmentalStage, Condition>
	{
		{
			DevelopmentalStage.None,
			Condition.None
		},
		{
			DevelopmentalStage.Newborn,
			Condition.Baby
		},
		{
			DevelopmentalStage.Baby,
			Condition.Baby
		},
		{
			DevelopmentalStage.Child,
			Condition.Child
		},
		{
			DevelopmentalStage.Adult,
			Condition.Adult
		}
	};

	public static IReadOnlyDictionary<PawnLostCondition, Condition> PawnLostConditionToCondition = new Dictionary<PawnLostCondition, Condition>
	{
		{
			PawnLostCondition.ChangedFaction,
			Condition.NonPlayerFaction
		},
		{
			PawnLostCondition.Drafted,
			Condition.Drafted
		},
		{
			PawnLostCondition.ExitedMap,
			Condition.NotOnMap
		},
		{
			PawnLostCondition.ForcedByPlayerAction,
			Condition.None
		},
		{
			PawnLostCondition.ForcedByQuest,
			Condition.None
		},
		{
			PawnLostCondition.ForcedToJoinOtherLord,
			Condition.HasAnotherLord
		},
		{
			PawnLostCondition.Incapped,
			Condition.Downed
		},
		{
			PawnLostCondition.InMentalState,
			Condition.MentalState
		},
		{
			PawnLostCondition.Killed,
			Condition.Dead
		},
		{
			PawnLostCondition.LeftVoluntarily,
			Condition.LeftVoluntarily
		},
		{
			PawnLostCondition.MadePrisoner,
			Condition.Prisoner
		},
		{
			PawnLostCondition.MadeSlave,
			Condition.Slave
		},
		{
			PawnLostCondition.NoLongerEnteringTransportPods,
			Condition.None
		},
		{
			PawnLostCondition.Undefined,
			Condition.Error
		},
		{
			PawnLostCondition.Vanished,
			Condition.Vanished
		}
	};

	protected IntRange allowedCount = new IntRange(0, 1);

	protected Condition allowedConditions = Condition.Default;

	protected bool canHandleOfferings;

	public bool psychicRitualWaitsForArrival = true;

	public bool showInvocationVfx;

	public bool applyPowerOffset = true;

	public bool removeOnLost = true;

	public virtual int MaxCount => allowedCount.max;

	public virtual int MinCount => allowedCount.min;

	public TaggedString Label => label;

	public TaggedString CategoryLabel => Label;

	public TaggedString CategoryLabelCap => LabelCap;

	public virtual Condition AllowedConditions => allowedConditions;

	public virtual bool CanHandleOfferings => canHandleOfferings;

	public virtual bool ConditionAllowed(PawnLostCondition condition)
	{
		if (condition == PawnLostCondition.LordRejected)
		{
			return false;
		}
		if (!PawnLostConditionToCondition.TryGetValue(condition, out var value))
		{
			throw new InvalidOperationException($"Unknown PawnLostCondition {condition}");
		}
		if (value == Condition.None)
		{
			return true;
		}
		return ConditionAllowed(value);
	}

	public virtual bool ConditionAllowed(Condition condition)
	{
		return (AllowedConditions & condition) != 0;
	}

	public static TaggedString ConditionToReason(Pawn pawn, Condition disqualifier)
	{
		switch (disqualifier)
		{
		case Condition.None:
		case Condition.Error:
			return TaggedString.Empty;
		case Condition.Dead:
			return "PsychicRitualLeaveReason_Killed".Translate(pawn.Named("PAWN"));
		case Condition.Downed:
			return "PsychicRitualLeaveReason_Incapped".Translate(pawn.Named("PAWN"));
		case Condition.Drafted:
			return "PsychicRitualLeaveReason_Drafted".Translate(pawn.Named("PAWN"));
		case Condition.Sleeping:
			return "PsychicRitualLeaveReason_Sleeping".Translate(pawn.Named("PAWN"));
		case Condition.Bleeding:
			return "PsychicRitualLeaveReason_Bleeding".Translate(pawn.Named("PAWN"));
		case Condition.Burning:
			return "PsychicRitualLeaveReason_Burning".Translate(pawn.Named("PAWN"));
		case Condition.MentalState:
			return "PsychicRitualLeaveReason_InMentalState".Translate(pawn.Named("PAWN"));
		case Condition.ExtremeTemperature:
			return "PsychicRitualLeaveReason_ExtremeTemperature".Translate(pawn.Named("PAWN"));
		case Condition.Humanlike:
			return "PsychicRitualLeaveReason_Humanlike".Translate(pawn.Named("PAWN"));
		case Condition.NonHumanlike:
			return "PsychicRitualLeaveReason_NonHumanlike".Translate(pawn.Named("PAWN"));
		case Condition.Prisoner:
			return "PsychicRitualLeaveReason_Prisoner".Translate(pawn.Named("PAWN"));
		case Condition.Slave:
			return "PsychicRitualLeaveReason_Slave".Translate(pawn.Named("PAWN"));
		case Condition.Freeman:
			return "PsychicRitualLeaveReason_Freeman".Translate(pawn.Named("PAWN"));
		case Condition.InsideBuilding:
		{
			Building arg = (Building)pawn.SpawnedParentOrMe;
			return "PsychicRitualLeaveReason_InsideBuilding".Translate(pawn.Named("PAWN"), arg.Named("BUILDING"));
		}
		case Condition.NonPlayerFaction:
			return "PsychicRitualLeaveReason_WrongFaction".Translate(pawn.Named("PAWN"), Faction.OfPlayer.Named("FACTION"));
		case Condition.NotOnMap:
			return "PsychicRitualLeaveReason_ExitedMap".Translate(pawn.Named("PAWN"));
		case Condition.HasAnotherLord:
			return "PsychicRitualLeaveReason_PawnBusyWithLord".Translate(pawn.Named("PAWN"));
		case Condition.LeftVoluntarily:
			return "PsychicRitualLeaveReason_LeftVoluntarily".Translate(pawn.Named("PAWN"));
		case Condition.Vanished:
			return "PsychicRitualLeaveReason_Vanished".Translate(pawn.Named("PAWN"));
		case Condition.Child:
			while (true)
			{
			}
		case Condition.Baby:
		case Condition.Adult:
			return "PsychicRitualLeaveReason_DevelopmentalStage".Translate(pawn.Named("PAWN"), pawn.DevelopmentalStage.ToString().Translate().Named("STAGE"));
		case Condition.NoPath:
			return "PsychicRitualLeaveReason_CannotReachTarget".Translate(pawn.Named("PAWN"));
		case Condition.SubHuman:
			return "PsychicRitualLeaveReason_Mutant".Translate(pawn.Named("PAWN"));
		case Condition.IdeoUnwilling:
			return "PsychicRitualLeaveReason_IdeoUnwilling".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO"));
		case Condition.NoPsychicSensitivity:
			return "PsychicRitualLeaveReason_NoPsychicSensitivity".Translate(pawn.Named("PAWN"));
		case Condition.TemporaryFactionMember:
			return "PsychicRitualLeaveReason_TemporaryFactionMember".Translate(pawn.Named("PAWN"));
		default:
			throw new InvalidOperationException($"Unknown RoleDisqualifiersEnum {disqualifier}");
		}
	}

	public static TaggedString PawnLostConditionToPsychicRitualReason(PsychicRitual psychicRitual, Pawn pawn, PawnLostCondition condition)
	{
		if (condition == PawnLostCondition.LordRejected)
		{
			throw new InvalidOperationException("Already informed player of pawn lost reason.");
		}
		if (!PawnLostConditionToCondition.TryGetValue(condition, out var value))
		{
			throw new InvalidOperationException($"Unknown PawnLostCondition {condition}");
		}
		return ConditionToReason(pawn, value);
	}

	public bool PawnCanDo(Context context, Pawn pawn, TargetInfo target, out Reason reason)
	{
		if (!PawnCanDo(context, pawn, target, out AnyEnum reason2))
		{
			reason = new Reason
			{
				context = context,
				def = this,
				pawn = pawn,
				reasonCode = reason2,
				target = target
			};
			return false;
		}
		reason = Reason.None;
		return true;
	}

	protected virtual bool PawnCanDo(Context context, Pawn pawn, TargetInfo target, out AnyEnum reason)
	{
		if (pawn == null)
		{
			reason = AnyEnum.FromEnum(Condition.Error);
			return false;
		}
		if (!ConditionAllowed(Condition.Dead) && pawn.Dead)
		{
			reason = AnyEnum.FromEnum(Condition.Dead);
			return false;
		}
		if (ModsConfig.AnomalyActive && !ConditionAllowed(Condition.SubHuman) && pawn.IsSubhuman)
		{
			reason = AnyEnum.FromEnum(Condition.SubHuman);
			return false;
		}
		if (!ConditionAllowed(Condition.Humanlike) && pawn.RaceProps.Humanlike)
		{
			reason = AnyEnum.FromEnum(Condition.Humanlike);
			return false;
		}
		if (!ConditionAllowed(Condition.NonHumanlike) && !pawn.RaceProps.Humanlike)
		{
			reason = AnyEnum.FromEnum(Condition.NonHumanlike);
			return false;
		}
		Condition condition = DevelopmentalStageToCondition[pawn.DevelopmentalStage];
		if (!ConditionAllowed(condition))
		{
			switch (context)
			{
			case Context.InvokePsychicRitualGizmo:
				reason = AnyEnum.None;
				return false;
			case Context.Dialog_BeginPsychicRitual:
				reason = AnyEnum.FromEnum(condition);
				return false;
			}
		}
		if (context != Context.NonPlayerFaction && context != Context.Runtime && !pawn.IsPrisoner && !ConditionAllowed(Condition.NonPlayerFaction) && !pawn.Faction.IsPlayerSafe())
		{
			reason = AnyEnum.FromEnum(Condition.NonPlayerFaction);
			return false;
		}
		if (!ConditionAllowed(Condition.HasAnotherLord) && pawn.GetLord() != null && context != Context.Runtime)
		{
			reason = AnyEnum.FromEnum(Condition.HasAnotherLord);
			return false;
		}
		if (!ConditionAllowed(Condition.Slave) && pawn.IsSlave)
		{
			reason = AnyEnum.FromEnum(Condition.Slave);
			return false;
		}
		if (!ConditionAllowed(Condition.Prisoner) && pawn.IsPrisoner)
		{
			reason = AnyEnum.FromEnum(Condition.Prisoner);
			return false;
		}
		if (!ConditionAllowed(Condition.Freeman) && pawn.IsFreeman)
		{
			reason = AnyEnum.FromEnum(Condition.Freeman);
			return false;
		}
		if (!ConditionAllowed(Condition.Downed) && pawn.Downed && (!pawn.health.capacities.CanBeAwake || pawn.Awake()))
		{
			reason = AnyEnum.FromEnum(Condition.Downed);
			return false;
		}
		if (!ConditionAllowed(Condition.MentalState) && pawn.InMentalState)
		{
			reason = AnyEnum.FromEnum(Condition.MentalState);
			return false;
		}
		if (!ConditionAllowed(Condition.Bleeding) && pawn.health.hediffSet.BleedRateTotal > 0f)
		{
			reason = AnyEnum.FromEnum(Condition.Bleeding);
			return false;
		}
		if (!ConditionAllowed(Condition.Burning) && pawn.IsBurning())
		{
			reason = AnyEnum.FromEnum(Condition.Burning);
			return false;
		}
		if (!ConditionAllowed(Condition.ExtremeTemperature) && target.IsValid)
		{
			Map map = target.Map;
			if (map != null && pawn.Faction == Faction.OfPlayer && !pawn.SafeTemperatureAtCell(target.Cell, map))
			{
				reason = AnyEnum.FromEnum(Condition.ExtremeTemperature);
				return false;
			}
		}
		if (!ConditionAllowed(Condition.InsideBuilding) && pawn.SpawnedParentOrMe is Building)
		{
			reason = AnyEnum.FromEnum(Condition.InsideBuilding);
			return false;
		}
		if (!ConditionAllowed(Condition.Sleeping) && !pawn.Awake() && context != Context.Dialog_BeginPsychicRitual)
		{
			reason = AnyEnum.FromEnum(Condition.Sleeping);
			return false;
		}
		if (!ConditionAllowed(Condition.Drafted) && pawn.Drafted && context != Context.Dialog_BeginPsychicRitual)
		{
			reason = AnyEnum.FromEnum(Condition.Drafted);
			return false;
		}
		if (!ConditionAllowed(Condition.NoPath) && target.IsValid && context != Context.Runtime && !CanReach(pawn, target) && !pawn.IsPrisoner)
		{
			reason = AnyEnum.FromEnum(Condition.NoPath);
			return false;
		}
		if (!ConditionAllowed(Condition.IdeoUnwilling) && context == Context.Dialog_BeginPsychicRitual && pawn.Ideo != null && !pawn.Ideo.MemberWillingToDo(new HistoryEvent(HistoryEventDefOf.InvolvedInPsychicRitual, pawn.Named(HistoryEventArgsNames.Doer))))
		{
			reason = AnyEnum.FromEnum(Condition.IdeoUnwilling);
			return false;
		}
		if (!ConditionAllowed(Condition.NoPsychicSensitivity) && pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f)
		{
			reason = AnyEnum.FromEnum(Condition.NoPsychicSensitivity);
			return false;
		}
		if (!ConditionAllowed(Condition.TemporaryFactionMember) && pawn.IsQuestLodger())
		{
			reason = AnyEnum.FromEnum(Condition.TemporaryFactionMember);
			return false;
		}
		reason = AnyEnum.None;
		return true;
	}

	public virtual TaggedString PawnCannotDoReason(AnyEnum reason, Context context, Pawn pawn, TargetInfo target)
	{
		if (reason == AnyEnum.None)
		{
			return TaggedString.Empty;
		}
		Condition disqualifier = reason.As<Condition>() ?? throw new ArgumentException("Unknown status reason type " + reason.enumType.ToStringSafe() + ".  Did you forget to override PawnCannotDoReason in a child class?");
		return ConditionToReason(pawn, disqualifier);
	}

	public IEnumerable<string> BlockingIssues(Pawn pawn, TargetInfo target)
	{
		if (!PawnCanDo(Context.Dialog_BeginPsychicRitual, pawn, target, out Reason reason))
		{
			yield return reason.ToPlayerReadable();
		}
	}

	public virtual IEnumerable<Gizmo> GetPawnGizmos(PsychicRitual psychicRitual, Pawn pawn)
	{
		if (!pawn.IsColonistPlayerControlled)
		{
			yield break;
		}
		if (psychicRitual.def.nonRequiredPawnsMayLeave)
		{
			PsychicRitualRoleDef psychicRitualRoleDef = psychicRitual.assignments.RoleForPawn(pawn);
			if (psychicRitualRoleDef != null && psychicRitual.assignments.RoleAssignedCount(psychicRitualRoleDef) > psychicRitualRoleDef.MinCount)
			{
				yield return PsychicRitualGizmo.LeaveGizmo(psychicRitual, pawn);
			}
		}
		yield return PsychicRitualGizmo.CancelGizmo(psychicRitual);
	}

	public virtual bool CanReach(Pawn pawn, TargetInfo target)
	{
		return pawn.CanReachNonLocal(target, PathEndMode.OnCell, Danger.Deadly);
	}

	public virtual void Notify_PawnJobDone(PsychicRitual psychicRitual, Pawn pawn, JobCondition condition)
	{
		if (condition == JobCondition.ErroredPather && !ConditionAllowed(Condition.NoPath) && !CanReach(pawn, psychicRitual.assignments.Target))
		{
			psychicRitual.LeaveOrCancelPsychicRitual(this, pawn, ConditionToReason(pawn, Condition.NoPath));
		}
	}
}
