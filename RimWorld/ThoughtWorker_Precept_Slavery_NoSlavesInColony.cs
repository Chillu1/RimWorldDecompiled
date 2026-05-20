using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_Slavery_NoSlavesInColony : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return p.IsColonist && !p.IsSlave && !p.IsPrisoner && FactionUtility.GetSlavesInFactionCount(p.Faction) <= 0;
		}
	}
}
