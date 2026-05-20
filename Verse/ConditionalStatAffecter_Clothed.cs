using RimWorld;

namespace Verse;

public class ConditionalStatAffecter_Clothed : ConditionalStatAffecter
{
	public override string Label => "StatsReport_Clothed".Translate();

	public override bool Applies(StatRequest req)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (req.HasThing && req.Thing is Pawn { apparel: not null } pawn)
		{
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				if (item.def.apparel.countsAsClothingForNudity)
				{
					return true;
				}
			}
		}
		return false;
	}
}
