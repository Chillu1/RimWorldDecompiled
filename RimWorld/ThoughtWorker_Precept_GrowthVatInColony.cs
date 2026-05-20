using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_GrowthVatInColony : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.IdeologyActive || !ModsConfig.BiotechActive || !p.IsColonist)
		{
			return ThoughtState.Inactive;
		}
		return p.MapHeld.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.GrowthVat).Any();
	}
}
