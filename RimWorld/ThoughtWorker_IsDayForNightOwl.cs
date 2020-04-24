using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsDayForNightOwl : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.Awake() && GenLocalDate.HourInteger(p) >= 11 && GenLocalDate.HourInteger(p) <= 17;
		}
	}
}
