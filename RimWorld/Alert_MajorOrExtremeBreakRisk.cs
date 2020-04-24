using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_MajorOrExtremeBreakRisk : Alert_Critical
	{
		private List<Pawn> culpritsResult = new List<Pawn>();

		private List<Pawn> Culprits
		{
			get
			{
				culpritsResult.Clear();
				culpritsResult.AddRange(BreakRiskAlertUtility.PawnsAtRiskExtreme);
				culpritsResult.AddRange(BreakRiskAlertUtility.PawnsAtRiskMajor);
				return culpritsResult;
			}
		}

		public override string GetLabel()
		{
			return BreakRiskAlertUtility.AlertLabel;
		}

		public override TaggedString GetExplanation()
		{
			return BreakRiskAlertUtility.AlertExplanation;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(Culprits);
		}
	}
}
