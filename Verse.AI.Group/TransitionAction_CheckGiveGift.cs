using RimWorld;

namespace Verse.AI.Group
{
	public class TransitionAction_CheckGiveGift : TransitionAction
	{
		public override void DoAction(Transition trans)
		{
			if (DebugSettings.instantVisitorsGift || (trans.target.lord.numPawnsLostViolently == 0 && Rand.Chance(VisitorGiftForPlayerUtility.ChanceToLeaveGift(trans.target.lord.faction, trans.Map))))
			{
				VisitorGiftForPlayerUtility.GiveGift(trans.target.lord.ownedPawns, trans.target.lord.faction);
			}
		}
	}
}
