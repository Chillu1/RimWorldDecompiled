using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Pawn_TimetableTracker : IExposable
{
	private Pawn pawn;

	public List<TimeAssignmentDef> times;

	public TimeAssignmentDef CurrentAssignment
	{
		get
		{
			if (!pawn.IsColonist || pawn.IsPrisonerOfColony)
			{
				return TimeAssignmentDefOf.Anything;
			}
			return times[GenLocalDate.HourOfDay(pawn)];
		}
	}

	public Pawn_TimetableTracker(Pawn pawn)
	{
		this.pawn = pawn;
		times = new List<TimeAssignmentDef>(24);
		for (int i = 0; i < 24; i++)
		{
			TimeAssignmentDef item = ((i > 5 && i <= 21) ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep);
			times.Add(item);
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref times, "times", LookMode.Undefined);
		if (Scribe.mode != LoadSaveMode.PostLoadInit || ModsConfig.RoyaltyActive)
		{
			return;
		}
		for (int i = 0; i < times.Count; i++)
		{
			if (times[i] == TimeAssignmentDefOf.Meditate)
			{
				times[i] = TimeAssignmentDefOf.Anything;
			}
		}
	}

	public TimeAssignmentDef GetAssignment(int hour)
	{
		return times[hour];
	}

	public void SetAssignment(int hour, TimeAssignmentDef ta)
	{
		times[hour] = ta;
	}
}
