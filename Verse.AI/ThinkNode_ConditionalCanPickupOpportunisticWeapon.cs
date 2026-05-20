namespace Verse.AI
{
	public class ThinkNode_ConditionalCanPickupOpportunisticWeapon : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.duty != null)
			{
				return pawn.mindState.duty.pickupOpportunisticWeapon;
			}
			return false;
		}
	}
}
