using Verse.AI.Group;

namespace RimWorld;

public class LordToil_MetalhorrorActive : LordToil
{
	private const int UpdateIntervalTicks = 300;

	public override bool AssignsDuties => false;

	public override bool AllowAggressiveTargetingOfRoamers => true;

	public override void UpdateAllDuties()
	{
	}

	public override void LordToilTick()
	{
	}
}
