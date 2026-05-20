using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class DrugAIUtility
{
	public static Job IngestAndTakeToInventoryJob(Thing drug, Pawn pawn, int maxNumToCarry = 9999)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Ingest, drug);
		job.count = Mathf.Min(drug.stackCount, drug.def.ingestible.defaultNumToIngestAtOnce, maxNumToCarry);
		DrugPolicy drugPolicy = pawn.drugs?.CurrentPolicy;
		if (drugPolicy != null)
		{
			DrugPolicyEntry drugPolicyEntry = drugPolicy[drug.def];
			int num = pawn.inventory.innerContainer.TotalStackCountOfDef(drug.def) - job.count;
			if (drugPolicyEntry.allowScheduled && num <= 0)
			{
				job.takeExtraIngestibles = drugPolicyEntry.takeToInventory;
			}
		}
		return job;
	}
}
