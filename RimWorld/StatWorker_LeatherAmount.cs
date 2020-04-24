using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StatWorker_LeatherAmount : StatWorker
	{
		public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
		{
			foreach (Dialog_InfoCard.Hyperlink infoCardHyperlink in base.GetInfoCardHyperlinks(statRequest))
			{
				yield return infoCardHyperlink;
			}
			if (statRequest.HasThing && statRequest.Thing.def.race != null && statRequest.Thing.def.race.leatherDef != null)
			{
				yield return new Dialog_InfoCard.Hyperlink(statRequest.Thing.def.race.leatherDef);
			}
		}
	}
}
