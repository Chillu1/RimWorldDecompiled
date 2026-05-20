using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class RitualStage : IExposable
{
	public RitualBehaviorDef parent;

	public DutyDef defaultDuty;

	public List<StageEndTrigger> endTriggers;

	public List<StageFailTrigger> failTriggers;

	public RitualStageAction preAction;

	public RitualStageAction postAction;

	public RitualStageAction interruptedAction;

	public RitualStageAction pawnLeaveAction;

	[NoTranslate]
	public List<string> highlightRolePositions = new List<string>();

	[NoTranslate]
	public List<string> highlightRolePawns = new List<string>();

	public RitualStageTickActionMaker tickActionMaker;

	public RitualVisualEffectDef visualEffectDef;

	public bool spectatorsRequired;

	public bool essential;

	public bool ignoreDurationToFinishAfterStage;

	public SpectateRectSide allowedSpectateSidesOverride;

	public IntRange spectateDistanceOverride = IntRange.Zero;

	public bool showProgressBar = true;

	public List<RitualRoleBehavior> roleBehaviors;

	private Dictionary<string, RitualRoleBehavior> behaviorForRole = new Dictionary<string, RitualRoleBehavior>();

	public RitualRoleBehavior BehaviorForRole(string roleId)
	{
		if (roleId == null)
		{
			return null;
		}
		if (!behaviorForRole.TryGetValue(roleId, out var value) && roleBehaviors != null)
		{
			for (int i = 0; i < roleBehaviors.Count; i++)
			{
				RitualRoleBehavior ritualRoleBehavior = roleBehaviors[i];
				if (ritualRoleBehavior.roleId == roleId)
				{
					behaviorForRole[roleId] = ritualRoleBehavior;
					return ritualRoleBehavior;
				}
			}
		}
		return value;
	}

	public DutyDef GetDuty(Pawn pawn, RitualRole forcedRole = null, LordJob_Ritual ritual = null)
	{
		if (parent.roles != null)
		{
			if (forcedRole != null)
			{
				return BehaviorForRole(forcedRole.id)?.dutyDef;
			}
			RitualRole ritualRole = ritual.assignments.RoleForPawn(pawn);
			if (ritualRole != null)
			{
				RitualRoleBehavior ritualRoleBehavior = BehaviorForRole(ritualRole.id);
				if (ritualRoleBehavior != null)
				{
					return ritualRoleBehavior.dutyDef;
				}
			}
		}
		return defaultDuty;
	}

	public bool HasRole(Pawn p)
	{
		List<RitualRole> roles = parent.roles;
		for (int i = 0; i < roles.Count; i++)
		{
			if (roles[i].AppliesToPawn(p, out var _, TargetInfo.Invalid, null, null, null, skipReason: true))
			{
				return true;
			}
		}
		return false;
	}

	public bool Applies(Precept_Ritual ritual, List<Pawn> participants)
	{
		foreach (RitualRole item in parent.RequiredRoles())
		{
			bool flag = false;
			Precept_Role precept_Role = (Precept_Role)Faction.OfPlayer.ideos.GetPrecept(item.precept);
			if (precept_Role == null)
			{
				return false;
			}
			foreach (Pawn participant in participants)
			{
				if (precept_Role.Active && precept_Role.IsAssigned(participant))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public PawnStagePosition GetPawnPosition(IntVec3 spot, Pawn pawn, LordJob_Ritual ritual, RitualRole forcedRole = null)
	{
		if (parent.roles != null)
		{
			if (forcedRole != null)
			{
				RitualPosition ritualPosition = BehaviorForRole(forcedRole.id)?.GetPosition(spot, pawn, ritual);
				if (ritualPosition != null)
				{
					return ritualPosition.GetCell(spot, pawn, ritual);
				}
			}
			RitualRole ritualRole = ritual.assignments.RoleForPawn(pawn);
			if (ritualRole != null)
			{
				RitualRoleBehavior ritualRoleBehavior = BehaviorForRole(ritualRole.id);
				if (ritualRoleBehavior != null)
				{
					RitualPosition position = ritualRoleBehavior.GetPosition(spot, pawn, ritual);
					if (position != null)
					{
						return position.GetCell(spot, pawn, ritual);
					}
				}
			}
		}
		return new PawnStagePosition(spot, null, Rot4.Invalid, highlight: false);
	}

	public virtual float ProgressPerTick(LordJob_Ritual ritual)
	{
		return 1f;
	}

	public virtual TargetInfo GetSecondFocus(LordJob_Ritual ritual)
	{
		return TargetInfo.Invalid;
	}

	public virtual IEnumerable<RitualStagePawnSecondFocus> GetPawnSecondFoci(LordJob_Ritual ritual)
	{
		return null;
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref parent, "parent");
		Scribe_Defs.Look(ref defaultDuty, "defaultDuty");
		Scribe_Collections.Look(ref endTriggers, "endTriggers", LookMode.Deep);
		Scribe_Values.Look(ref spectatorsRequired, "spectatorsRequired", defaultValue: false);
		Scribe_Values.Look(ref allowedSpectateSidesOverride, "allowedSpectateSidesOverride", SpectateRectSide.None);
		Scribe_Values.Look(ref spectateDistanceOverride, "spectateDistanceOverride");
		Scribe_Values.Look(ref showProgressBar, "showProgressBar", defaultValue: false);
		Scribe_Collections.Look(ref failTriggers, "failTriggers", LookMode.Deep);
		Scribe_Collections.Look(ref roleBehaviors, "roleBehaviors", LookMode.Deep);
		Scribe_Collections.Look(ref highlightRolePositions, "highlightRolePositions", LookMode.Value);
		Scribe_Defs.Look(ref visualEffectDef, "visualEffectDef");
		Scribe_Values.Look(ref ignoreDurationToFinishAfterStage, "ignoreDurationToFinishAfterStage", defaultValue: false);
		Scribe_Deep.Look(ref postAction, "postAction");
		Scribe_Deep.Look(ref preAction, "preAction");
		Scribe_Deep.Look(ref interruptedAction, "interruptedAction");
		Scribe_Deep.Look(ref pawnLeaveAction, "pawnLeaveAction");
	}
}
