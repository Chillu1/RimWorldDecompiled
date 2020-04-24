using Verse;

namespace RimWorld
{
	public static class TimetableUtility
	{
		public static TimeAssignmentDef GetTimeAssignment(this Pawn pawn)
		{
			if (pawn.timetable == null)
			{
				return TimeAssignmentDefOf.Anything;
			}
			return pawn.timetable.CurrentAssignment;
		}
	}
}
