using System.Collections.Generic;

namespace Verse.AI
{
	public class ThinkNode_ConditionalMentalStates : ThinkNode_Conditional
	{
		public List<MentalStateDef> states;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalMentalStates obj = (ThinkNode_ConditionalMentalStates)base.DeepCopy(resolve);
			obj.states = states;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return states.Contains(pawn.MentalStateDef);
		}
	}
}
