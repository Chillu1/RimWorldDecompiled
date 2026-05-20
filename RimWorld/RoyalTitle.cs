using UnityEngine;
using Verse;

namespace RimWorld;

public class RoyalTitle : IExposable
{
	public Faction faction;

	public RoyalTitleDef def;

	public Pawn pawn;

	public int receivedTick = -1;

	public bool wasInherited;

	public bool conceited;

	private const int DecreeCheckInterval = 833;

	private const int RoomRequirementsGracePeriodTicks = 180000;

	public string Label => def.GetLabelFor(pawn);

	private int RoomRequirementsGracePeriodTicksLeft => Mathf.Max(180000 - (GenTicks.TicksGame - receivedTick), MoveColonyUtility.TitleAndRoleRequirementGracePeriodTicksLeft, 0);

	public float RoomRequirementGracePeriodDaysLeft => RoomRequirementsGracePeriodTicksLeft.TicksToDays();

	public bool RoomRequirementGracePeriodActive(Pawn pawn)
	{
		if (RoomRequirementsGracePeriodTicksLeft > 0)
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
		pawn = other.pawn;
	}

	public void RoyalTitleTick(int delta)
	{
		if (pawn.IsHashIntervalTick(833, delta) && conceited && pawn.Spawned && pawn.IsFreeColonist && (!pawn.IsQuestLodger() || pawn.LodgerAllowedDecrees()) && def.decreeMtbDays > 0f && pawn.Awake() && Rand.MTBEventOccurs(def.decreeMtbDays, 60000f, 833f) && (float)(Find.TickManager.TicksGame - pawn.royalty.lastDecreeTicks) >= def.decreeMinIntervalDays * 60000f)
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
