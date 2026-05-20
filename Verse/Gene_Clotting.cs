using System.Collections.Generic;

namespace Verse;

public class Gene_Clotting : Gene
{
	private const int ClotCheckInterval = 360;

	private static readonly FloatRange TendingQualityRange = new FloatRange(0.2f, 0.7f);

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (!pawn.IsHashIntervalTick(360, delta))
		{
			return;
		}
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int num = hediffs.Count - 1; num >= 0; num--)
		{
			if (hediffs[num].Bleeding)
			{
				hediffs[num].Tended(TendingQualityRange.RandomInRange, TendingQualityRange.TrueMax, 1);
			}
		}
	}
}
