using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public abstract class LordJob_Joinable_Gathering : LordJob_VoluntarilyJoinable
{
	protected IntVec3 spot;

	protected Pawn organizer;

	protected GatheringDef gatheringDef;

	protected int durationTicks;

	protected Trigger_TicksPassed timeoutTrigger;

	public Pawn Organizer => organizer;

	public int DurationTicks => durationTicks;

	public virtual int TicksLeft => timeoutTrigger.TicksLeft;

	public virtual IntVec3 Spot => spot;

	public LordJob_Joinable_Gathering()
	{
	}

	public LordJob_Joinable_Gathering(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
	{
		this.spot = spot;
		this.organizer = organizer;
		this.gatheringDef = gatheringDef;
	}

	protected abstract LordToil CreateGatheringToil(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef);

	protected virtual bool ShouldBeCalledOff()
	{
		if (organizer != null && !GatheringsUtility.PawnCanStartOrContinueGathering(organizer))
		{
			return true;
		}
		if (!GatheringsUtility.AcceptableGameConditionsToContinueGathering(base.Map))
		{
			return true;
		}
		return false;
	}

	public override float VoluntaryJoinPriorityFor(Pawn p)
	{
		if (IsInvited(p))
		{
			if (!GatheringsUtility.ShouldPawnKeepGathering(p, gatheringDef))
			{
				return 0f;
			}
			if (spot.IsForbidden(p))
			{
				return 0f;
			}
			if (!lord.ownedPawns.Contains(p) && IsGatheringAboutToEnd())
			{
				return 0f;
			}
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def.blocksSocialInteraction)
				{
					return 0f;
				}
			}
			return VoluntarilyJoinableLordJobJoinPriorities.SocialGathering;
		}
		return 0f;
	}

	protected virtual Trigger_TicksPassed GetTimeoutTrigger()
	{
		return new Trigger_TicksPassed(durationTicks);
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref spot, "spot");
		Scribe_Values.Look(ref durationTicks, "durationTicks", 0);
		Scribe_References.Look(ref organizer, "organizer");
		Scribe_Defs.Look(ref gatheringDef, "gatheringDef");
	}

	protected bool IsGatheringAboutToEnd()
	{
		if (TicksLeft < 1200)
		{
			return true;
		}
		return false;
	}

	protected virtual bool IsInvited(Pawn p)
	{
		if (lord.faction != null)
		{
			return p.Faction == lord.faction;
		}
		return false;
	}
}
