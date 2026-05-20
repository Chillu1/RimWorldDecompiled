using Verse;

namespace RimWorld;

public class Hediff_High : HediffWithComps
{
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
