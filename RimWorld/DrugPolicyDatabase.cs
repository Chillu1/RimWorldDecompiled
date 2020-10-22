using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class DrugPolicyDatabase : IExposable
	{
		private List<DrugPolicy> policies = new List<DrugPolicy>();

		public List<DrugPolicy> AllPolicies => policies;

		public DrugPolicyDatabase()
		{
			GenerateStartingDrugPolicies();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref policies, "policies", LookMode.Deep);
		}

		public DrugPolicy DefaultDrugPolicy()
		{
			if (policies.Count == 0)
			{
				MakeNewDrugPolicy();
			}
			return policies[0];
		}

		public AcceptanceReport TryDelete(DrugPolicy policy)
		{
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
			{
				if (item.drugs != null && item.drugs.CurrentPolicy == policy)
				{
					return new AcceptanceReport("DrugPolicyInUse".Translate(item));
				}
			}
			foreach (Pawn item2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				if (item2.drugs != null && item2.drugs.CurrentPolicy == policy)
				{
					item2.drugs.CurrentPolicy = null;
				}
			}
			policies.Remove(policy);
			return AcceptanceReport.WasAccepted;
		}

		public DrugPolicy MakeNewDrugPolicy()
		{
			int uniqueId = ((!policies.Any()) ? 1 : (policies.Max((DrugPolicy o) => o.uniqueId) + 1));
			DrugPolicy drugPolicy = new DrugPolicy(uniqueId, "DrugPolicy".Translate() + " " + uniqueId.ToString());
			policies.Add(drugPolicy);
			return drugPolicy;
		}

		private void GenerateStartingDrugPolicies()
		{
			DrugPolicy drugPolicy = MakeNewDrugPolicy();
			drugPolicy.label = "SocialDrugs".Translate();
			drugPolicy[ThingDefOf.Beer].allowedForJoy = true;
			drugPolicy[ThingDefOf.SmokeleafJoint].allowedForJoy = true;
			MakeNewDrugPolicy().label = "NoDrugs".Translate();
			DrugPolicy drugPolicy2 = MakeNewDrugPolicy();
			drugPolicy2.label = "Unrestricted".Translate();
			for (int i = 0; i < drugPolicy2.Count; i++)
			{
				if (drugPolicy2[i].drug.IsPleasureDrug)
				{
					drugPolicy2[i].allowedForJoy = true;
				}
			}
			DrugPolicy drugPolicy3 = MakeNewDrugPolicy();
			drugPolicy3.label = "OneDrinkPerDay".Translate();
			drugPolicy3[ThingDefOf.Beer].allowedForJoy = true;
			drugPolicy3[ThingDefOf.Beer].allowScheduled = true;
			drugPolicy3[ThingDefOf.Beer].takeToInventory = 1;
			drugPolicy3[ThingDefOf.Beer].daysFrequency = 1f;
			drugPolicy3[ThingDefOf.SmokeleafJoint].allowedForJoy = true;
		}
	}
}
