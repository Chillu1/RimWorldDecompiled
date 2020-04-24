using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class LordJob_Joinable_Gathering : LordJob_VoluntarilyJoinable
	{
		protected IntVec3 spot;

		protected Pawn organizer;

		protected GatheringDef gatheringDef;

		protected Trigger_TicksPassed timeoutTrigger;

		public Pawn Organizer => organizer;

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
			if (!GatheringsUtility.PawnCanStartOrContinueGathering(organizer))
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
				return VoluntarilyJoinableLordJobJoinPriorities.SocialGathering;
			}
			return 0f;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref spot, "spot");
			Scribe_References.Look(ref organizer, "organizer");
			Scribe_Defs.Look(ref gatheringDef, "gatheringDef");
		}

		private bool IsGatheringAboutToEnd()
		{
			if (timeoutTrigger.TicksLeft < 1200)
			{
				return true;
			}
			return false;
		}

		private bool IsInvited(Pawn p)
		{
			if (lord.faction != null)
			{
				return p.Faction == lord.faction;
			}
			return false;
		}
	}
}
