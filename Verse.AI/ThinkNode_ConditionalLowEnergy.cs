namespace Verse.AI
{
	public class ThinkNode_ConditionalLowEnergy : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.needs?.energy != null)
			{
				return pawn.needs.energy.IsLowEnergySelfShutdown;
			}
			return false;
		}
	}
}
