using RimWorld;
using System.Collections.Generic;

namespace Verse.AI.Group
{
	public class TransitionAction_WakeAll : TransitionAction
	{
		public override void DoAction(Transition trans)
		{
			List<Pawn> ownedPawns = trans.target.lord.ownedPawns;
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				RestUtility.WakeUp(ownedPawns[i]);
			}
		}
	}
}
