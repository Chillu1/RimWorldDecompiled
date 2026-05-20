using System.Collections.Generic;

namespace Verse;

public sealed class PlanManager : IExposable
{
	public readonly Map map;

	private List<Plan> allPlans = new List<Plan>();

	private Plan[] planGrid;

	public List<Plan> AllPlans => allPlans;

	public PlanManager(Map map)
	{
		this.map = map;
		planGrid = new Plan[map.cellIndices.NumGridCells];
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref allPlans, "allPlans", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			UpdatePlanManagerLinks();
			RebuildPlanGrid();
		}
	}

	private void UpdatePlanManagerLinks()
	{
		for (int i = 0; i < allPlans.Count; i++)
		{
			allPlans[i].planManager = this;
		}
	}

	private void RebuildPlanGrid()
	{
		CellIndices cellIndices = map.cellIndices;
		planGrid = new Plan[cellIndices.NumGridCells];
		foreach (Plan allPlan in allPlans)
		{
			foreach (IntVec3 item in allPlan)
			{
				planGrid[cellIndices.CellToIndex(item)] = allPlan;
			}
		}
	}

	public void RegisterPlan(Plan newPlan)
	{
		allPlans.Add(newPlan);
	}

	public void DeregisterPlan(Plan oldPlan)
	{
		allPlans.Remove(oldPlan);
		if (Find.Selector.SelectedPlan == oldPlan)
		{
			Find.Selector.ClearSelection();
		}
	}

	internal void AddPlanGridCell(Plan plan, IntVec3 c)
	{
		planGrid[map.cellIndices.CellToIndex(c)] = plan;
	}

	internal void ClearPlanGridCell(IntVec3 c)
	{
		planGrid[map.cellIndices.CellToIndex(c)] = null;
	}

	public Plan PlanAt(IntVec3 c)
	{
		return planGrid[map.cellIndices.CellToIndex(c)];
	}

	public bool TryGetPlan(IntVec3 cell, out Plan plan)
	{
		plan = planGrid[map.cellIndices.CellToIndex(cell)];
		return plan != null;
	}

	public string NewPlanName(string nameBase)
	{
		for (int i = 1; i <= 1000; i++)
		{
			string cand = nameBase + " " + i;
			if (!allPlans.Any((Plan z) => z.label == cand))
			{
				return cand;
			}
		}
		Log.Error("Ran out of plan names.");
		return "Plan X";
	}
}
