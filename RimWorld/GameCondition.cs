using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class GameCondition : IExposable, ILoadReferenceable
{
	public GameConditionManager gameConditionManager;

	public Thing conditionCauser;

	public bool hideSource;

	public GameConditionDef def;

	public int uniqueID = -1;

	public int startTick;

	public bool suppressEndMessage;

	private int duration = -1;

	private bool permanent;

	public bool forceDisplayAsDuration;

	private List<Map> cachedAffectedMaps = new List<Map>();

	private List<Map> cachedAffectedMapsForMaps = new List<Map>();

	public Quest quest;

	public PsychicRitualDef psychicRitualDef;

	private static List<GameConditionManager> tmpGameConditionManagers = new List<GameConditionManager>();

	protected Map SingleMap => gameConditionManager.ownerMap;

	public virtual string Label => def.label;

	public virtual string LabelCap => Label.CapitalizeFirst(def);

	public virtual string LetterText => def.letterText;

	public virtual bool Expired
	{
		get
		{
			if (!Permanent)
			{
				return Find.TickManager.TicksGame > startTick + Duration;
			}
			return false;
		}
	}

	public virtual bool ElectricityDisabled => false;

	public int TicksPassed => Find.TickManager.TicksGame - startTick;

	public virtual string Description => def.description;

	public virtual int TransitionTicks => 300;

	public int TicksLeft
	{
		get
		{
			if (Permanent)
			{
				Log.ErrorOnce("Trying to get ticks left of a permanent condition.", 384767654);
				return 360000000;
			}
			return Duration - TicksPassed;
		}
		set
		{
			Duration = TicksPassed + value;
		}
	}

	public bool Permanent
	{
		get
		{
			return permanent;
		}
		set
		{
			if (value)
			{
				duration = -1;
			}
			permanent = value;
		}
	}

	public int Duration
	{
		get
		{
			if (Permanent)
			{
				Log.ErrorOnce("Trying to get duration of a permanent condition.", 100394867);
				return 360000000;
			}
			return duration;
		}
		set
		{
			permanent = false;
			duration = value;
		}
	}

	public virtual string TooltipString
	{
		get
		{
			string text = def.LabelCap.ToString();
			Map map = null;
			map = ((SingleMap != null) ? SingleMap : ((Find.CurrentMap == null) ? Find.AnyPlayerHomeMap : Find.CurrentMap));
			if (Permanent && !forceDisplayAsDuration && def.showPermanentInTooltip)
			{
				text += "\n" + "Permanent".Translate().CapitalizeFirst();
			}
			else
			{
				Vector2 location = ((map != null) ? Find.WorldGrid.LongLatOf(map.Tile) : Vector2.zero);
				text = string.Concat(text, "\n", "Started".Translate(), ": ", GenDate.DateFullStringAt(GenDate.TickGameToAbs(startTick), location).Colorize(ColoredText.DateTimeColor));
				text = string.Concat(text, "\n", "Lasted".Translate(), ": ", TicksPassed.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor));
			}
			text += "\n";
			text = text + "\n" + Description.ResolveTags();
			if (conditionCauser != null && !hideSource && CameraJumper.CanJump(conditionCauser))
			{
				text = text + "\n\n" + def.jumpToSourceKey.Translate().Resolve();
			}
			else
			{
				Quest quest = this.quest;
				if (quest != null && !quest.hidden)
				{
					text = text + "\n\n" + "CausedByQuest".Translate(this.quest.name).Resolve();
				}
				else if (psychicRitualDef != null)
				{
					text += string.Format("\n\n{0}: {1}", "CausedByPsychicRitual".Translate(), psychicRitualDef.label.CapitalizeFirst());
				}
				else if (!def.natural)
				{
					text += string.Format("\n\n{0}", "SourceUnknown".Translate());
				}
			}
			if (map != null && MapExcludedByFilter(def, map))
			{
				text += string.Format("\n\n{0}", "ThisWillNotAffectLayer".Translate(map.Tile.LayerDef.gerundLabel.Named("GERUND"), map.Tile.LayerDef.label.Named("LAYER")));
			}
			return text;
		}
	}

	public List<Map> AffectedMaps
	{
		get
		{
			if (!GenCollection.ListsEqual(cachedAffectedMapsForMaps, Find.Maps))
			{
				cachedAffectedMapsForMaps.Clear();
				cachedAffectedMapsForMaps.AddRange(Find.Maps);
				cachedAffectedMaps.Clear();
				if (CanApplyOnMap(gameConditionManager.ownerMap))
				{
					cachedAffectedMaps.Add(gameConditionManager.ownerMap);
				}
				tmpGameConditionManagers.Clear();
				gameConditionManager.GetChildren(tmpGameConditionManagers);
				for (int i = 0; i < tmpGameConditionManagers.Count; i++)
				{
					if (CanApplyOnMap(tmpGameConditionManagers[i].ownerMap))
					{
						cachedAffectedMaps.Add(tmpGameConditionManagers[i].ownerMap);
					}
				}
				tmpGameConditionManagers.Clear();
			}
			return cachedAffectedMaps;
		}
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref uniqueID, "uniqueID", -1);
		Scribe_Values.Look(ref suppressEndMessage, "suppressEndMessage", defaultValue: false);
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref startTick, "startTick", 0);
		Scribe_Values.Look(ref duration, "duration", 0);
		Scribe_Values.Look(ref permanent, "permanent", defaultValue: false);
		Scribe_References.Look(ref quest, "quest");
		Scribe_Values.Look(ref forceDisplayAsDuration, "forceDisplayAsDuration", defaultValue: false);
		Scribe_Values.Look(ref hideSource, "hideSource", defaultValue: false);
		Scribe_Defs.Look(ref psychicRitualDef, "psychicRitualDef");
		BackCompatibility.PostExposeData(this);
	}

	public virtual void GameConditionTick()
	{
	}

	public virtual void GameConditionDraw(Map map)
	{
	}

	public virtual void Init()
	{
		if (!def.startMessage.NullOrEmpty())
		{
			Messages.Message(def.startMessage, MessageTypeDefOf.NeutralEvent);
		}
	}

	public virtual void End()
	{
		if (!suppressEndMessage && def.endMessage != null && !cachedAffectedMaps.Any(HiddenByOtherCondition))
		{
			Messages.Message(def.endMessage, MessageTypeDefOf.NeutralEvent);
		}
		gameConditionManager.OnConditionEnd(this);
	}

	public bool CanApplyOnMap(Map map)
	{
		if (map == null)
		{
			return false;
		}
		if (map.generatorDef.isUnderground && !def.allowUnderground)
		{
			return false;
		}
		if (MapExcludedByFilter(def, map))
		{
			return false;
		}
		if (ModsConfig.OdysseyActive && def.requireFish)
		{
			WaterBodyTracker waterBodyTracker = map.waterBodyTracker;
			if (waterBodyTracker == null || !waterBodyTracker.AnyBodyContainsFish)
			{
				return false;
			}
		}
		return true;
	}

	public static bool MapExcludedByFilter(GameConditionDef def, Map map)
	{
		if (!map.Tile.Valid)
		{
			return false;
		}
		if (!def.layerWhitelist.NullOrEmpty() && !def.layerWhitelist.Contains(map.Tile.LayerDef))
		{
			return true;
		}
		if (!def.layerBlacklist.NullOrEmpty() && def.layerBlacklist.Contains(map.Tile.LayerDef))
		{
			return true;
		}
		if (!def.canAffectAllPlanetLayers && map.Tile.LayerDef.onlyAllowWhitelistedGameConditions && (def.layerWhitelist.NullOrEmpty() || !def.layerWhitelist.Contains(map.Tile.LayerDef)))
		{
			return true;
		}
		return false;
	}

	public bool HiddenByOtherCondition(Map map)
	{
		if (def.silencedByConditions.NullOrEmpty())
		{
			return false;
		}
		for (int i = 0; i < def.silencedByConditions.Count; i++)
		{
			if (map.gameConditionManager.ConditionIsActive(def.silencedByConditions[i]))
			{
				return true;
			}
		}
		return false;
	}

	public virtual float SkyGazeChanceFactor(Map map)
	{
		return 1f;
	}

	public virtual float SkyGazeJoyGainFactor(Map map)
	{
		return 1f;
	}

	public virtual float TemperatureOffset()
	{
		return 0f;
	}

	public virtual float SkyTargetLerpFactor(Map map)
	{
		return 0f;
	}

	public virtual SkyTarget? SkyTarget(Map map)
	{
		return null;
	}

	public virtual float AnimalDensityFactor(Map map)
	{
		return 1f;
	}

	public virtual float PlantDensityFactor(Map map)
	{
		return 1f;
	}

	public virtual bool AllowEnjoyableOutsideNow(Map map)
	{
		return true;
	}

	public virtual List<SkyOverlay> SkyOverlays(Map map)
	{
		return null;
	}

	public virtual void DoCellSteadyEffects(IntVec3 c, Map map)
	{
	}

	public virtual WeatherDef ForcedWeather()
	{
		return null;
	}

	public virtual float MinWindSpeed()
	{
		return 0f;
	}

	public virtual float WeatherCommonalityFactor(WeatherDef weather, Map map)
	{
		return 1f;
	}

	public virtual void PostMake()
	{
		uniqueID = Find.UniqueIDsManager.GetNextGameConditionID();
	}

	public virtual void RandomizeSettings(float points, Map map, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
	}

	public string GetUniqueLoadID()
	{
		return GetType().Name + "_" + uniqueID;
	}
}
