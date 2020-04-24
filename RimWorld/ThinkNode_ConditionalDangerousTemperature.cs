using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalDangerousTemperature : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return !pawn.SafeTemperatureRange().Includes(pawn.AmbientTemperature);
		}
	}
}
