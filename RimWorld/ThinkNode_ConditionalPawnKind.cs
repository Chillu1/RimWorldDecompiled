using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalPawnKind : ThinkNode_Conditional
	{
		public PawnKindDef pawnKind;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalPawnKind obj = (ThinkNode_ConditionalPawnKind)base.DeepCopy(resolve);
			obj.pawnKind = pawnKind;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.kindDef == pawnKind;
		}
	}
}
