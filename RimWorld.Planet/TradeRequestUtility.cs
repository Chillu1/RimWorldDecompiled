using Verse;

namespace RimWorld.Planet
{
	public static class TradeRequestUtility
	{
		public static string RequestedThingLabel(ThingDef def, int count)
		{
			string text = GenLabel.ThingLabel(def, null, count);
			if (def.HasComp(typeof(CompQuality)))
			{
				text += " (" + "NormalQualityOrBetter".Translate() + ")";
			}
			if (def.IsApparel)
			{
				text += " (" + "NotTainted".Translate() + ")";
			}
			return text;
		}
	}
}
