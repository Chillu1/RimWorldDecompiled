using Verse;

namespace RimWorld
{
	public class ThoughtWorker_HasAddedBodyPart : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			int num = p.health.hediffSet.CountAddedAndImplantedParts();
			if (num > 0)
			{
				return ThoughtState.ActiveAtStage(num - 1);
			}
			return false;
		}
	}
}
