using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_NeedJoySources : Alert
{
	private Map badMap;

	public Alert_NeedJoySources()
	{
		defaultLabel = "NeedJoySource".Translate();
	}

	public override TaggedString GetExplanation()
	{
		Map map = badMap;
		int num = JoyUtility.JoyKindsOnMapCount(map);
		string label = map.info.parent.Label;
		ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(map);
		int joyKindsNeeded = expectationDef.joyKindsNeeded;
		string text = "AvailableRecreationTypes".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + JoyUtility.JoyKindsOnMapString(map);
		string text2 = "MissingRecreationTypes".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + JoyUtility.JoyKindsNotOnMapString(map);
		return "NeedJoySourceDesc".Translate(num, label, expectationDef.label, joyKindsNeeded, text, text2);
	}

	public override AlertReport GetReport()
	{
		return BadMap() != null;
	}

	private Map BadMap()
	{
		badMap = null;
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (NeedJoySource(maps[i]))
			{
				badMap = maps[i];
				break;
			}
		}
		return badMap;
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
