using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI.Group;

public class TransitionAction_SetDefendTrader : TransitionAction
{
	public override void DoAction(Transition trans)
	{
		LordToil_DefendPoint lordToil_DefendPoint = (LordToil_DefendPoint)trans.target;
		Pawn pawn = TraderCaravanUtility.FindTrader(lordToil_DefendPoint.lord);
		if (pawn != null)
		{
			lordToil_DefendPoint.SetDefendPoint(pawn.Position);
			return;
		}
		IEnumerable<Pawn> source = lordToil_DefendPoint.lord.ownedPawns.Where((Pawn x) => x.GetTraderCaravanRole() == TraderCaravanRole.Carrier);
		if (source.Any())
		{
			lordToil_DefendPoint.SetDefendPoint(source.RandomElement().Position);
		}
		else
		{
			lordToil_DefendPoint.SetDefendPoint(lordToil_DefendPoint.lord.ownedPawns.RandomElement().Position);
		}
	}
}
