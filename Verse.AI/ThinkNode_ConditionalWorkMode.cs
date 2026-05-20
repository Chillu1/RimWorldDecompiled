using RimWorld;

namespace Verse.AI
{
	public class ThinkNode_ConditionalWorkMode : ThinkNode_Conditional
	{
		public MechWorkModeDef workMode;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalWorkMode obj = (ThinkNode_ConditionalWorkMode)base.DeepCopy(resolve);
			obj.workMode = workMode;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			if (!pawn.RaceProps.IsMechanoid || pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			Pawn overseer = pawn.GetOverseer();
			if (overseer == null)
			{
				return false;
			}
			return overseer.mechanitor.GetControlGroup(pawn).WorkMode == workMode;
		}
	}
}
