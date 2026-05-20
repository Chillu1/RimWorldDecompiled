using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalHasAbility : ThinkNode_Conditional
	{
		public AbilityDef ability;

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.abilities?.GetAbility(ability) != null;
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalHasAbility obj = (ThinkNode_ConditionalHasAbility)base.DeepCopy(resolve);
			obj.ability = ability;
			return obj;
		}
	}
}
