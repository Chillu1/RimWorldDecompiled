using System.Linq;
using RimWorld;

namespace Verse.AI.Group;

public class TransitionAction_EnsureHaveExitDestination : TransitionAction
{
	public override void DoAction(Transition trans)
	{
		LordToil_Travel lordToil_Travel = (LordToil_Travel)trans.target;
		if (!lordToil_Travel.HasDestination() && lordToil_Travel.lord.ownedPawns.Where((Pawn x) => x.Spawned).TryRandomElement(out var result))
		{
			if (!CellFinder.TryFindRandomPawnExitCell(result, out var result2))
			{
				RCellFinder.TryFindRandomPawnEntryCell(out result2, result.Map, 0f);
			}
			lordToil_Travel.SetDestination(result2);
		}
	}
}
