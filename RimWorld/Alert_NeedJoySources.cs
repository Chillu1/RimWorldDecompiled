using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedJoySources : Alert
	{
		public Alert_NeedJoySources()
		{
			defaultLabel = "NeedJoySource".Translate();
		}

		public override TaggedString GetExplanation()
		{
			Map map = BadMap();
			int value = JoyUtility.JoyKindsOnMapCount(map);
			string label = map.info.parent.Label;
			ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(map);
			int joyKindsNeeded = expectationDef.joyKindsNeeded;
			string value2 = "AvailableRecreationTypes".Translate() + ":\n\n" + JoyUtility.JoyKindsOnMapString(map);
			string value3 = "MissingRecreationTypes".Translate() + ":\n\n" + JoyUtility.JoyKindsNotOnMapString(map);
			return "NeedJoySourceDesc".Translate(value, label, expectationDef.label, joyKindsNeeded, value2, value3);
		}

		public override AlertReport GetReport()
		{
			return BadMap() != null;
		}

		private Map BadMap()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (NeedJoySource(maps[i]))
				{
					return maps[i];
				}
			}
			return null;
		}

		private bool NeedJoySource(Map map)
		{
			if (!map.IsPlayerHome)
			{
				return false;
			}
			if (!map.mapPawns.AnyColonistSpawned)
			{
				return false;
			}
			int num = JoyUtility.JoyKindsOnMapCount(map);
			int joyKindsNeeded = ExpectationsUtility.CurrentExpectationFor(map).joyKindsNeeded;
			return num < joyKindsNeeded;
		}
	}
}
