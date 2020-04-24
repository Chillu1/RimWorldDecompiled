namespace Verse.AI.Group
{
	public class TransitionAction_SetDefendLocalGroup : TransitionAction
	{
		public override void DoAction(Transition trans)
		{
			LordToil_DefendPoint obj = (LordToil_DefendPoint)trans.target;
			obj.SetDefendPoint(obj.lord.ownedPawns.RandomElement().Position);
		}
	}
}
