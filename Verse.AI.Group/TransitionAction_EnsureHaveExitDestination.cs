using RimWorld;

namespace Verse.AI.Group
{
	public class TransitionAction_EnsureHaveExitDestination : TransitionAction
	{
		public override void DoAction(Transition trans)
		{
			LordToil_Travel lordToil_Travel = (LordToil_Travel)trans.target;
			if (!lordToil_Travel.HasDestination())
			{
				Pawn pawn = lordToil_Travel.lord.ownedPawns.RandomElement();
				if (!CellFinder.TryFindRandomPawnExitCell(pawn, out var result))
				{
					RCellFinder.TryFindRandomPawnEntryCell(out result, pawn.Map, 0f);
				}
				lordToil_Travel.SetDestination(result);
			}
		}
	}
}
