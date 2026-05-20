using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_IdeoDiversity_Social : ThoughtWorker_Precept_Social
	{
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			return p.Faction == otherPawn.Faction && p.Ideo != otherPawn.Ideo && !otherPawn.DevelopmentalStage.Baby();
		}
	}
}
