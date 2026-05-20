using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

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

	public void SetDefault(DrugPolicy policy)
	{
		int index = policies.IndexOf(policy);
		DrugPolicy value = policies[0];
		policies[0] = policy;
		policies[index] = value;
	}

	public void MakePolicyDefault(DrugPolicyDef policyDef)
	{
		if (DefaultDrugPolicy().sourceDef != policyDef)
		{
			DrugPolicy drugPolicy = policies.FirstOrDefault((DrugPolicy x) => x.sourceDef == policyDef);
			if (drugPolicy != null)
			{
				policies.Remove(drugPolicy);
				policies.Insert(0, drugPolicy);
			}
		}
	}

	public AcceptanceReport TryDelete(DrugPolicy policy)
	{
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
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
		int id = ((!policies.Any()) ? 1 : (policies.Max((DrugPolicy o) => o.id) + 1));
		DrugPolicy drugPolicy = new DrugPolicy(id, "DrugPolicy".Translate() + " " + id.ToString());
		policies.Add(drugPolicy);
		return drugPolicy;
	}

	public DrugPolicy NewDrugPolicyFromDef(DrugPolicyDef def)
	{
		DrugPolicy drugPolicy = MakeNewDrugPolicy();
		drugPolicy.label = def.LabelCap;
		drugPolicy.sourceDef = def;
		if (def.allowPleasureDrugs)
		{
			for (int i = 0; i < drugPolicy.Count; i++)
			{
				if (drugPolicy[i].drug.IsPleasureDrug)
				{
					drugPolicy[i].allowedForJoy = true;
				}
			}
		}
		if (def.entries != null)
		{
			for (int j = 0; j < def.entries.Count; j++)
			{
				drugPolicy[def.entries[j].drug].CopyFrom(def.entries[j]);
			}
		}
		return drugPolicy;
	}

	private void GenerateStartingDrugPolicies()
	{
		foreach (DrugPolicyDef allDef in DefDatabase<DrugPolicyDef>.AllDefs)
		{
			NewDrugPolicyFromDef(allDef);
		}
	}
}
