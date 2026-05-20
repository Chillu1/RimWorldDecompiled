using Verse;

namespace RimWorld;

public class PawnColumnWorker_ManhunterOnDamageChance : PawnColumnWorker_Text
{
	protected override string GetTextFor(Pawn pawn)
	{
		float manhunterOnDamageChance = PawnUtility.GetManhunterOnDamageChance(pawn);
		if (manhunterOnDamageChance == 0f)
		{
			return "-";
		}
		return manhunterOnDamageChance.ToStringPercent();
	}

	protected override string GetTip(Pawn pawn)
	{
		return PawnUtility.GetManhunterOnDamageChanceExplanation(pawn.def, pawn);
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return PawnUtility.GetManhunterOnDamageChance(a).CompareTo(PawnUtility.GetManhunterOnDamageChance(b));
	}
}
