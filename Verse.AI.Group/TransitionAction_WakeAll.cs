using System.Collections.Generic;
using RimWorld;

namespace Verse.AI.Group;

public class TransitionAction_WakeAll : TransitionAction
{
	public override void DoAction(Transition trans)
	{
		List<Pawn> ownedPawns = trans.target.lord.ownedPawns;
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			RestUtility.WakeUp(ownedPawns[i]);
		}
		List<Building> ownedBuildings = trans.target.lord.ownedBuildings;
		for (int j = 0; j < ownedBuildings.Count; j++)
		{
			ownedBuildings[j].TryGetComp<CompCanBeDormant>()?.WakeUpWithDelay();
		}
	}
}
