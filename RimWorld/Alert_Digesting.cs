using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_Digesting : Alert_Critical
{
	private readonly List<Pawn> digesting = new List<Pawn>();

	protected override bool DoMessage => false;

	private List<Pawn> Digesting
	{
		get
		{
			digesting.Clear();
			foreach (Map map in Find.Maps)
			{
				foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
				{
					if (item.TryGetComp<CompDevourer>(out var comp) && comp.Digesting && (comp.DigestingPawn.IsColonist || comp.DigestingPawn.IsColonyMech))
					{
						digesting.Add(comp.DigestingPawn);
					}
				}
			}
			return digesting;
		}
	}

	public Alert_Digesting()
	{
		defaultLabel = "AlertDigestion".Translate();
		requireAnomaly = true;
	}

	public override TaggedString GetExplanation()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Pawn item in digesting)
		{
			int ticksLeftThisToil = ((Pawn)item.SpawnedParentOrMe).jobs.curDriver.ticksLeftThisToil;
			stringBuilder.AppendLine("  - " + item.NameShortColored.Resolve().CapitalizeFirst() + ": " + ticksLeftThisToil.ToStringSecondsFromTicks());
		}
		return string.Format("{0}:\n{1}\n\n{2}", "AlertDigestionDesc".Translate(), stringBuilder.ToString().TrimEndNewlines(), "AlertDigestionDescAppended".Translate());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Digesting);
	}
}
