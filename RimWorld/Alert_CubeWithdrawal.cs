using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_CubeWithdrawal : Alert_Critical
{
	private readonly List<Pawn> inWithdrawal = new List<Pawn>();

	private const float AlertPercentage = 0.8f;

	protected override bool DoMessage => false;

	private List<Pawn> Withdrawal
	{
		get
		{
			inWithdrawal.Clear();
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_NoCryptosleep)
			{
				if ((!ModsConfig.BiotechActive || !item.Deathresting) && InWithdrawal(item))
				{
					inWithdrawal.Add(item);
				}
			}
			return inWithdrawal;
		}
	}

	public Alert_CubeWithdrawal()
	{
		defaultLabel = "AlertCubeWithdrawal".Translate();
		requireAnomaly = true;
	}

	private static bool InWithdrawal(ThingWithComps thing)
	{
		if (thing is Pawn { health: not null } pawn && pawn.health.hediffSet.TryGetHediff(HediffDefOf.CubeWithdrawal, out var hediff))
		{
			return hediff.Severity >= 0.8f;
		}
		return false;
	}

	public override string GetLabel()
	{
		if (inWithdrawal.Count == 1)
		{
			return inWithdrawal[0].Label + ": " + GetSeverity(inWithdrawal[0]).ToStringPercent("0");
		}
		return defaultLabel;
	}

	public override TaggedString GetExplanation()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Pawn item in inWithdrawal)
		{
			stringBuilder.AppendLine("  - " + item.NameShortColored.Resolve() + ": " + GetSeverity(item).ToStringPercent("0"));
		}
		return string.Format("{0}:\n{1}\n\n{2}", "AlertCubeWithdrawalDesc".Translate(), stringBuilder.ToString().TrimEndNewlines(), "AlertCubeWithdrawalDescAppended".Translate());
	}

	private static float GetSeverity(Pawn pawn)
	{
		return pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.CubeWithdrawal).Severity;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Withdrawal);
	}
}
