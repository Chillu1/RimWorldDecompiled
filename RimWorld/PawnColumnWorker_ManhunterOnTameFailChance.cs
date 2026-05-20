using Verse;

namespace RimWorld;

public class PawnColumnWorker_ManhunterOnTameFailChance : PawnColumnWorker_Text
{
	protected override string GetTextFor(Pawn pawn)
	{
		float manhunterOnTameFailChance = PawnUtility.GetManhunterOnTameFailChance(pawn);
		if (manhunterOnTameFailChance == 0f)
		{
			return "-";
		}
		return manhunterOnTameFailChance.ToStringPercent();
	}

	protected override string GetTip(Pawn pawn)
	{
		return PawnUtility.GetManhunterOnTameFailChanceExplanation(pawn.def, pawn);
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return PawnUtility.GetManhunterOnTameFailChance(a).CompareTo(PawnUtility.GetManhunterOnTameFailChance(b));
	}
}
