using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StatPart_Hyperlinks : StatPart
	{
		public List<ThingDef> thingDefs;

		public override string ExplanationPart(StatRequest req)
		{
			return null;
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
		}

		public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
		{
			if (thingDefs.NullOrEmpty())
			{
				yield break;
			}
			foreach (ThingDef thingDef in thingDefs)
			{
				yield return new Dialog_InfoCard.Hyperlink(thingDef);
			}
		}
	}
}
