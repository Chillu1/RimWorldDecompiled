using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StatWorker_MeatAmount : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		if (req.Pawn?.RaceProps?.hasMeat == true)
		{
			return false;
		}
		return base.ShouldShowFor(req);
	}

	public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
	{
		foreach (Dialog_InfoCard.Hyperlink infoCardHyperlink in base.GetInfoCardHyperlinks(statRequest))
		{
			yield return infoCardHyperlink;
		}
		if (statRequest.HasThing && statRequest.Thing.def.race != null && statRequest.Thing.def.race.meatDef != null)
		{
			yield return new Dialog_InfoCard.Hyperlink(statRequest.Thing.def.race.meatDef);
		}
	}
}
