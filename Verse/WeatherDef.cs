using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class WeatherDef : Def
{
	public IntRange durationRange = new IntRange(16000, 160000);

	public bool repeatable;

	public bool isBad;

	public bool canOccurAsRandomForcedEvent = true;

	public Favorability favorability = Favorability.Neutral;

	public FloatRange temperatureRange = new FloatRange(-999f, 999f);

	public SimpleCurve commonalityRainfallFactor;

	public int transitionTicksOverride = int.MaxValue;

	public int minMonolithLevel;

	public bool canOccurInAmbientHorror;

	[MustTranslate]
	public string letterText;

	[MustTranslate]
	public string letterLabel;

	public LetterDef letterDef;

	public float rainRate;

	public float snowRate;

	public float sandRate;

	public float windSpeedFactor = 1f;

	public float windSpeedOffset;

	public float moveSpeedMultiplier = 1f;

	public float accuracyMultiplier = 1f;

	public float maxRangeCap = -1f;

	public float perceivePriority;

	public bool doToxicBuildup;

	public ThoughtDef weatherThought;

	public float maxGlow = 1f;

	public bool preventSkygaze;

	public bool preventsShuttleLaunch;

	public List<SoundDef> ambientSounds = new List<SoundDef>();

	public List<WeatherEventMaker> eventMakers = new List<WeatherEventMaker>();

	public List<Type> overlayClasses = new List<Type>();

	public SkyColorSet skyColorsNightMid;

	public SkyColorSet skyColorsNightEdge;

	public SkyColorSet skyColorsDay;

	public SkyColorSet skyColorsDusk;

	public Type workerClass = typeof(WeatherWorker);

	[Unsaved(false)]
	private WeatherWorker workerInt;

	public WeatherWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (WeatherWorker)Activator.CreateInstance(workerClass, this);
			}
			return workerInt;
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (skyColorsDay.saturation == 0f || skyColorsDusk.saturation == 0f || skyColorsNightMid.saturation == 0f || skyColorsNightEdge.saturation == 0f)
		{
			yield return "a sky color has saturation of 0";
		}
	}

	public static WeatherDef Named(string defName)
	{
		return DefDatabase<WeatherDef>.GetNamed(defName);
	}
}
