using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalRandom : ThinkNode_Conditional
	{
		public float chance = 0.5f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalRandom obj = (ThinkNode_ConditionalRandom)base.DeepCopy(resolve);
			obj.chance = chance;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return Rand.Value < chance;
		}
	}
}
