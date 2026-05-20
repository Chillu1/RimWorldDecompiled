using Verse;
using Verse.AI;

namespace RimWorld;

public class LordToil_PrepareCaravan_GatherAnimals : LordToil_PrepareCaravan_RopeAnimals
{
	public LordToil_PrepareCaravan_GatherAnimals(IntVec3 destinationPoint)
		: base(destinationPoint, null)
	{
	}

	protected override PawnDuty MakeRopeDuty()
	{
		return new PawnDuty(DutyDefOf.PrepareCaravan_GatherAnimals, destinationPoint);
	}

	public override void LordToilTick()
	{
		if (Find.TickManager.TicksGame % 100 == 0)
		{
			GatherAnimalsAndSlavesForCaravanUtility.CheckArrived(lord, lord.ownedPawns, destinationPoint, "AllAnimalsGathered", AnimalPenUtility.NeedsToBeManagedByRope, (Pawn x) => x.roping.RopedToSpot == destinationPoint);
		}
	}
}
