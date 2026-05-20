using System.Text;
using Verse;

namespace RimWorld.Planet;

public static class CaravanBonusUtility
{
	public static bool HasCaravanBonus(Pawn pawn)
	{
		if (pawn.RaceProps.Humanlike && !pawn.Downed)
		{
			return pawn.GetStatValue(StatDefOf.CaravanBonusSpeedFactor) > 1f;
		}
		return false;
	}

	public static string GetIconTooltipText(Pawn pawn)
	{
		float statValue = pawn.GetStatValue(StatDefOf.CaravanBonusSpeedFactor);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("CaravanBonusTip".Translate());
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine($"{StatDefOf.CaravanBonusSpeedFactor.LabelCap}: {statValue.ToStringPercent()}");
		return stringBuilder.ToString();
	}
}
