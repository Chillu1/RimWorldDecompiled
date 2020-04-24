using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_LifeThreateningHediff : Alert_Critical
	{
		private List<Pawn> sickPawnsResult = new List<Pawn>();

		private List<Pawn> SickPawns
		{
			get
			{
				sickPawnsResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep)
				{
					for (int i = 0; i < item.health.hediffSet.hediffs.Count; i++)
					{
						Hediff hediff = item.health.hediffSet.hediffs[i];
						if (hediff.CurStage != null && hediff.CurStage.lifeThreatening && !hediff.FullyImmune())
						{
							sickPawnsResult.Add(item);
							break;
						}
					}
				}
				return sickPawnsResult;
			}
		}

		public override string GetLabel()
		{
			return "PawnsWithLifeThreateningDisease".Translate();
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			foreach (Pawn sickPawn in SickPawns)
			{
				stringBuilder.AppendLine("  - " + sickPawn.NameShortColored.Resolve());
				foreach (Hediff hediff in sickPawn.health.hediffSet.hediffs)
				{
					if (hediff.CurStage != null && hediff.CurStage.lifeThreatening && hediff.Part != null && hediff.Part != sickPawn.RaceProps.body.corePart)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				return "PawnsWithLifeThreateningDiseaseAmputationDesc".Translate(stringBuilder.ToString());
			}
			return "PawnsWithLifeThreateningDiseaseDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(SickPawns);
		}
	}
}
