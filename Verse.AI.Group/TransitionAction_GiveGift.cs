using System.Collections.Generic;
using RimWorld;

namespace Verse.AI.Group
{
	public class TransitionAction_GiveGift : TransitionAction
	{
		public List<Thing> gifts;

		public override void DoAction(Transition trans)
		{
			if (!gifts.NullOrEmpty())
			{
				VisitorGiftForPlayerUtility.GiveGift(trans.target.lord.ownedPawns, trans.target.lord.faction, gifts);
				gifts.Clear();
			}
		}
	}
}
