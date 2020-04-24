using Verse;

namespace RimWorld
{
	public class Alert_Custom : Alert
	{
		public string label;

		public string explanation;

		public AlertReport report;

		public override string GetLabel()
		{
			return label;
		}

		public override TaggedString GetExplanation()
		{
			return explanation;
		}

		public override AlertReport GetReport()
		{
			return report;
		}
	}
}
