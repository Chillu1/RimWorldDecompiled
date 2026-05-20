using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class CompCauseGameCondition_ForceWeather : CompCauseGameCondition
{
	public WeatherDef weather;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		weather = base.Props.conditionDef.weatherDef;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Defs.Look(ref weather, "weather");
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!Prefs.DevMode || !DebugSettings.godMode)
		{
			yield break;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = weather.LabelCap;
		command_Action.action = delegate
		{
			List<WeatherDef> allDefsListForReading = DefDatabase<WeatherDef>.AllDefsListForReading;
			int num = allDefsListForReading.FindIndex((WeatherDef w) => w == weather);
			num++;
			if (num >= allDefsListForReading.Count)
			{
				num = 0;
			}
			weather = allDefsListForReading[num];
			ReSetupAllConditions();
		};
		command_Action.hotKey = KeyBindingDefOf.Misc1;
		yield return command_Action;
	}

	protected override void SetupCondition(GameCondition condition, Map map)
	{
		base.SetupCondition(condition, map);
		((GameCondition_ForceWeather)condition).weather = weather;
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		return text + "Weather".Translate() + ": " + weather.LabelCap;
	}

	public override void RandomizeSettings(Site site)
	{
		weather = DefDatabase<WeatherDef>.AllDefsListForReading.Where((WeatherDef x) => x.isBad && x.canOccurAsRandomForcedEvent).RandomElement();
	}
}
