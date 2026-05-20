using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNode_ConditionalHiveCanReproduce : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		if (pawn.mindState.duty.focus.Thing is Hive hive)
		{
			return hive.GetComp<CompSpawnerHives>().canSpawnHives;
		}
		return false;
	}
}
