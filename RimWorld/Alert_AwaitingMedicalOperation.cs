using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_AwaitingMedicalOperation : Alert
{
	private List<Pawn> awaitingMedicalOperationResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> AwaitingMedicalOperation
	{
		get
		{
			awaitingMedicalOperationResult.Clear();
			List<Pawn> list = PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (IsAwaiting(list[i]))
				{
					awaitingMedicalOperationResult.Add(list[i]);
				}
			}
			List<Pawn> allMaps_PrisonersOfColonySpawned = PawnsFinder.AllMaps_PrisonersOfColonySpawned;
			for (int j = 0; j < allMaps_PrisonersOfColonySpawned.Count; j++)
			{
				if (IsAwaiting(allMaps_PrisonersOfColonySpawned[j]) && (!ModsConfig.BiotechActive || allMaps_PrisonersOfColonySpawned[j].health.surgeryBills.Count != 1 || allMaps_PrisonersOfColonySpawned[j].health.surgeryBills[0].recipe != RecipeDefOf.ExtractHemogenPack || allMaps_PrisonersOfColonySpawned[j].guest.IsInteractionDisabled(PrisonerInteractionModeDefOf.HemogenFarm)))
				{
					awaitingMedicalOperationResult.Add(allMaps_PrisonersOfColonySpawned[j]);
				}
			}
			return awaitingMedicalOperationResult;
			bool IsAwaiting(Pawn p)
			{
				if (HealthAIUtility.ShouldHaveSurgeryDoneNow(p) && p.InBed())
				{
					return !awaitingMedicalOperationResult.Contains(p);
				}
				return false;
			}
		}
	}

	public override string GetLabel()
	{
		return "PatientsAwaitingMedicalOperation".Translate(awaitingMedicalOperationResult.Count.ToStringCached());
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (Pawn item in awaitingMedicalOperationResult)
		{
			sb.AppendLine("  - " + item.NameShortColored.Resolve());
		}
		return "PatientsAwaitingMedicalOperationDesc".Translate(sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(AwaitingMedicalOperation);
	}
}
