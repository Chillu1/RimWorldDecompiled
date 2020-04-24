using System;

namespace Verse.AI
{
	public class ThinkNode_ConditionalMentalStateClass : ThinkNode_Conditional
	{
		public Type stateClass;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalMentalStateClass obj = (ThinkNode_ConditionalMentalStateClass)base.DeepCopy(resolve);
			obj.stateClass = stateClass;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			MentalState mentalState = pawn.MentalState;
			if (mentalState != null)
			{
				return stateClass.IsAssignableFrom(mentalState.GetType());
			}
			return false;
		}
	}
}
