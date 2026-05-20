using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_HypothermicAnimals : Alert
{
	private List<Pawn> hypothermicAnimalsResult = new List<Pawn>();

	private List<Pawn> HypothermicAnimals
	{
		get
		{
			hypothermicAnimalsResult.Clear();
			IReadOnlyList<Pawn> allMaps_Spawned = PawnsFinder.AllMaps_Spawned;
			for (int i = 0; i < allMaps_Spawned.Count; i++)
			{
				if (allMaps_Spawned[i].IsAnimal && allMaps_Spawned[i].Faction == null && allMaps_Spawned[i].health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia) != null)
				{
					hypothermicAnimalsResult.Add(allMaps_Spawned[i]);
				}
			}
			return hypothermicAnimalsResult;
		}
	}

	public override string GetLabel()
	{
		return "Hypothermic wild animals (debug)";
	}

	public override TaggedString GetExplanation()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Debug alert:\n\nThese wild animals are hypothermic. This may indicate a bug (but it may not, if the animals are trapped or in some other wierd but legitimate situation):");
		foreach (Pawn hypothermicAnimal in HypothermicAnimals)
		{
			stringBuilder.AppendLine("    " + hypothermicAnimal?.ToString() + " at " + hypothermicAnimal.Position.ToString());
		}
		return stringBuilder.ToString();
	}

	public override AlertReport GetReport()
	{
		if (!Prefs.DevMode)
		{
			return false;
		}
		return AlertReport.CulpritsAre(HypothermicAnimals);
	}
}
