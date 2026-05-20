using RimWorld;

namespace Verse;

public class ConditionalStatAffecter_Child : ConditionalStatAffecter
{
	public override string Label => "Child".Translate();

	public override bool Applies(StatRequest req)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (req.HasThing && req.Thing is Pawn pawn && pawn.RaceProps.Humanlike)
		{
			return pawn.DevelopmentalStage.Child();
		}
		return false;
	}
}
