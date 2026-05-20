using RimWorld;

namespace Verse.AI;

public class ThinkNode_ChancePerHour_Nuzzle : ThinkNode_ChancePerHour
{
	protected override float MtbHours(Pawn pawn)
	{
		return NuzzleUtility.GetNuzzleMTBHours(pawn);
	}
}
