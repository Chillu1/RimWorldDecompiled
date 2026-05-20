using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_AssaultColony : LordToil
{
	private bool attackDownedIfStarving;

	private bool canPickUpOpportunisticWeapons;

	public override bool ForceHighStoryDanger => true;

	public override bool AllowSatisfyLongNeeds => false;

	public LordToil_AssaultColony(bool attackDownedIfStarving = false, bool canPickUpOpportunisticWeapons = false)
	{
		this.attackDownedIfStarving = attackDownedIfStarving;
		this.canPickUpOpportunisticWeapons = canPickUpOpportunisticWeapons;
	}

	public override void Init()
	{
		base.Init();
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].mindState != null)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
				lord.ownedPawns[i].mindState.duty.attackDownedIfStarving = attackDownedIfStarving;
				lord.ownedPawns[i].mindState.duty.pickupOpportunisticWeapon = canPickUpOpportunisticWeapons;
				lord.ownedPawns[i].TryGetComp<CompCanBeDormant>()?.WakeUp();
			}
		}
	}
}
