using RimWorld;

namespace Verse;

public class Hediff_RotStinkExposure : HediffWithComps
{
	public override bool Visible
	{
		get
		{
			if (base.Visible)
			{
				return !pawn.health.hediffSet.HasHediff(HediffDefOf.LungRot);
			}
			return false;
		}
	}

	public override string SeverityLabel
	{
		get
		{
			if (Severity <= 0f)
			{
				return null;
			}
			return Severity.ToStringPercent("F0");
		}
	}
}
