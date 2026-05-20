using Verse;

namespace RimWorld;

public class StageEndTrigger_RoleArrivedAndSpectatorsOnSubstructure : StageEndTrigger_RolesArrived
{
	protected override bool ArrivedCheck(string r, LordJob_Ritual ritual)
	{
		if (!base.ArrivedCheck(r, ritual))
		{
			return false;
		}
		if (Find.TickManager.TicksGame % 60 != 0)
		{
			return false;
		}
		Building_GravEngine building_GravEngine = ritual.selectedTarget.Thing?.TryGetComp<CompPilotConsole>()?.engine;
		if (building_GravEngine == null)
		{
			Log.Error("Engine could not be found in ritual end trigger");
			return true;
		}
		foreach (Pawn item in ritual.assignments.SpectatorsForReading)
		{
			if (!building_GravEngine.ValidSubstructure.Contains(item.Position))
			{
				return false;
			}
		}
		if (building_GravEngine.pawnsToBoard != null)
		{
			foreach (Pawn item2 in building_GravEngine.pawnsToBoard)
			{
				if (!building_GravEngine.ValidSubstructure.Contains(item2.Position))
				{
					return false;
				}
			}
		}
		if (building_GravEngine.pawnsToLeave != null)
		{
			foreach (Pawn item3 in building_GravEngine.pawnsToLeave)
			{
				if (building_GravEngine.ValidSubstructure.Contains(item3.Position))
				{
					return false;
				}
			}
		}
		return true;
	}
}
