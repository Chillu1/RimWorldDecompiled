using Verse;

namespace RimWorld
{
	public class CompRitualEffect_SpawnOnRole : CompRitualEffect_SpawnOnPawn
	{
		protected new CompProperties_RitualEffectSpawnOnRole Props => (CompProperties_RitualEffectSpawnOnRole)props;

		protected override Pawn GetPawn(LordJob_Ritual ritual)
		{
			return ritual.PawnWithRole(Props.roleId);
		}
	}
}
