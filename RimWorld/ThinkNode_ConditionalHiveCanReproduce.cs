using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalHiveCanReproduce : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return (pawn.mindState.duty.focus.Thing as Hive)?.GetComp<CompSpawnerHives>().canSpawnHives ?? false;
		}
	}
}
