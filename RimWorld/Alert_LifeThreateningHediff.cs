using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_LifeThreateningHediff : Alert_Critical
{
	private List<Pawn> sickPawnsResult = new List<Pawn>();

	private List<Pawn> SickPawns
	{
		get
		{
			sickPawnsResult.Clear();
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_NoCryptosleep)
			{
				bool flag = ModsConfig.BiotechActive && item.genes != null && item.genes.HasActiveGene(GeneDefOf.Deathless);
				for (int i = 0; i < item.health.hediffSet.hediffs.Count; i++)
				{
					Hediff hediff = item.health.hediffSet.hediffs[i];
					if (!hediff.IsCurrentlyLifeThreatening || hediff.FullyImmune())
					{
						continue;
					}
					if (flag)
					{
						HediffStage curStage = hediff.CurStage;
						if (curStage == null || !curStage.mtbDeathDestroysBrain)
						{
							continue;
						}
					}
					sickPawnsResult.Add(item);
					break;
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
				if (hediff.CurStage != null && hediff.IsCurrentlyLifeThreatening && hediff.Part != null && hediff.Part != sickPawn.RaceProps.body.corePart)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			return "PawnsWithLifeThreateningDiseaseAmputationDesc".Translate(stringBuilder.ToString().TrimEndNewlines());
		}
		return "PawnsWithLifeThreateningDiseaseDesc".Translate(stringBuilder.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(SickPawns);
	}
}
