using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsNightForNightOwl : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.Awake() && (GenLocalDate.HourInteger(p) >= 23 || GenLocalDate.HourInteger(p) <= 5);
		}
	}
}
