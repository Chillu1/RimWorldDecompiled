using Verse;

namespace RimWorld;

public abstract class ThoughtWorker_Precept_ChildLabor : ThoughtWorker_Precept
{
	protected abstract TimeAssignmentDef AssignmentDef { get; }

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.IdeologyActive || !ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		foreach (Pawn item in p.MapHeld.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
		{
			if (!item.RaceProps.Humanlike || !item.DevelopmentalStage.Child() || item.timetable == null)
			{
				continue;
			}
			for (int i = 0; i < 24; i++)
			{
				if (item.timetable.GetAssignment(i) == AssignmentDef)
				{
					return true;
				}
			}
		}
		return false;
	}
}
