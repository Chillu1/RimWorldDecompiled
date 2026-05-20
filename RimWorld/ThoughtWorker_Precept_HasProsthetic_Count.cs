using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_HasProsthetic_Count : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		int num = ProstheticsCount(p);
		if (num > 0)
		{
			return ThoughtState.ActiveAtStage(num - 1);
		}
		return false;
	}

	public static int ProstheticsCount(Pawn p)
	{
		return GeneUtility.AddedAndImplantedPartsWithXenogenesCount(p);
	}
}
