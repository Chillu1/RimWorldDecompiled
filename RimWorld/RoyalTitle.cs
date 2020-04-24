using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoyalTitle : IExposable
	{
		public Faction faction;

		public RoyalTitleDef def;

		public int receivedTick = -1;

		public bool wasInherited;

		public bool conceited;

		private const int DecreeCheckInterval = 833;

		private const int RoomRequirementsGracePeriodTicks = 180000;

		public float RoomRequirementGracePeriodDaysLeft => Mathf.Max((180000 - (GenTicks.TicksGame - receivedTick)).TicksToDays(), 0f);

		public bool RoomRequirementGracePeriodActive(Pawn pawn)
		{
			if (GenTicks.TicksGame - receivedTick < 180000)
			{
				return !pawn.IsQuestLodger();
			}
			return false;
		}

		public RoyalTitle()
		{
		}

		public RoyalTitle(RoyalTitle other)
		{
			faction = other.faction;
			def = other.def;
			receivedTick = other.receivedTick;
		}

		public void RoyalTitleTick(Pawn pawn)
		{
			if (pawn.IsHashIntervalTick(833) && conceited && pawn.Spawned && pawn.IsFreeColonist && !pawn.IsQuestHelper() && def.decreeMtbDays > 0f && pawn.Awake() && Rand.MTBEventOccurs(def.decreeMtbDays, 60000f, 833f) && (float)(Find.TickManager.TicksGame - pawn.royalty.lastDecreeTicks) >= def.decreeMinIntervalDays * 60000f)
			{
				pawn.royalty.IssueDecree(causedByMentalBreak: false);
			}
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref faction, "faction");
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref receivedTick, "receivedTick", 0);
			Scribe_Values.Look(ref wasInherited, "wasInherited", defaultValue: false);
			Scribe_Values.Look(ref conceited, "conceited", defaultValue: false);
		}
	}
}
