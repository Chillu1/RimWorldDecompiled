using Verse;

namespace RimWorld
{
	public class CompRitualEffect_SpawnOnOrganizer : CompRitualEffect_SpawnOnPawn
	{
		protected override Pawn GetPawn(LordJob_Ritual ritual)
		{
			return ritual.Organizer;
		}
	}
}
