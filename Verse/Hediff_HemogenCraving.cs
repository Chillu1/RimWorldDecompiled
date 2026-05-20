namespace Verse
{
	public class Hediff_HemogenCraving : HediffWithComps
	{
		public override string SeverityLabel
		{
			get
			{
				if (Severity == 0f)
				{
					return null;
				}
				return Severity.ToStringPercent();
			}
		}
	}
}
