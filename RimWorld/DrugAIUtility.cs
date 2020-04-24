using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class DrugAIUtility
	{
		public static Job IngestAndTakeToInventoryJob(Thing drug, Pawn pawn, int maxNumToCarry = 9999)
		{
			Job job = JobMaker.MakeJob(JobDefOf.Ingest, drug);
			job.count = Mathf.Min(drug.stackCount, drug.def.ingestible.maxNumToIngestAtOnce, maxNumToCarry);
			if (drug.Spawned && pawn.drugs != null && !pawn.inventory.innerContainer.Contains(drug.def))
			{
				DrugPolicyEntry drugPolicyEntry = pawn.drugs.CurrentPolicy[drug.def];
				if (drugPolicyEntry.allowScheduled)
				{
					job.takeExtraIngestibles = drugPolicyEntry.takeToInventory;
				}
			}
			return job;
		}
	}
}
