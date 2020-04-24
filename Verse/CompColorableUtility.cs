using UnityEngine;

namespace Verse
{
	public static class CompColorableUtility
	{
		public static void SetColor(this Thing t, Color newColor, bool reportFailure = true)
		{
			ThingWithComps thingWithComps = t as ThingWithComps;
			if (thingWithComps == null)
			{
				if (reportFailure)
				{
					Log.Error("SetColor on non-ThingWithComps " + t);
				}
				return;
			}
			CompColorable comp = thingWithComps.GetComp<CompColorable>();
			if (comp == null)
			{
				if (reportFailure)
				{
					Log.Error("SetColor on Thing without CompColorable " + t);
				}
			}
			else if (comp.Color != newColor)
			{
				comp.Color = newColor;
			}
		}
	}
}
