using RimWorld;

namespace Verse;

public class ConditionalStatAffecter_Unclothed : ConditionalStatAffecter
{
	public override string Label => "StatsReport_Unclothed".Translate();

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
					return false;
				}
			}
		}
		return true;
	}
}
