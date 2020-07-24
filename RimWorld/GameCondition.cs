using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class GameCondition : IExposable, ILoadReferenceable
	{
		public GameConditionManager gameConditionManager;

		public Thing conditionCauser;

		public GameConditionDef def;

		public int uniqueID = -1;

		public int startTick;

		public bool suppressEndMessage;

		private int duration = -1;

		private bool permanent;

		private List<Map> cachedAffectedMaps = new List<Map>();

		private List<Map> cachedAffectedMapsForMaps = new List<Map>();

		public Quest quest;

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
				string t = def.LabelCap;
				if (Permanent)
				{
					t += "\n" + "Permanent".Translate().CapitalizeFirst();
				}
				else
				{
					Vector2 location = (SingleMap != null) ? Find.WorldGrid.LongLatOf(SingleMap.Tile) : ((Find.CurrentMap != null) ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile) : ((Find.AnyPlayerHomeMap == null) ? Vector2.zero : Find.WorldGrid.LongLatOf(Find.AnyPlayerHomeMap.Tile)));
					t += "\n" + "Started".Translate() + ": " + GenDate.DateFullStringAt(GenDate.TickGameToAbs(startTick), location);
					t += "\n" + "Lasted".Translate() + ": " + TicksPassed.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
				}
				t += "\n";
				t = t + "\n" + Description;
				t += "\n";
				t += "\n";
				if (conditionCauser != null && CameraJumper.CanJump(conditionCauser))
				{
					return t + def.jumpToSourceKey.Translate();
				}
				if (quest != null)
				{
					return t + "CausedByQuest".Translate(quest.name);
				}
				return t + "SourceUnknown".Translate();
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
					if (gameConditionManager.ownerMap != null)
					{
						cachedAffectedMaps.Add(gameConditionManager.ownerMap);
					}
					tmpGameConditionManagers.Clear();
					gameConditionManager.GetChildren(tmpGameConditionManagers);
					for (int i = 0; i < tmpGameConditionManagers.Count; i++)
					{
						if (tmpGameConditionManagers[i].ownerMap != null)
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
		}

		public virtual void End()
		{
			if (!suppressEndMessage && def.endMessage != null)
			{
				Messages.Message(def.endMessage, MessageTypeDefOf.NeutralEvent);
			}
			gameConditionManager.ActiveConditions.Remove(this);
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

		public virtual void PostMake()
		{
			uniqueID = Find.UniqueIDsManager.GetNextGameConditionID();
		}

		public virtual void RandomizeSettings(float points, Map map, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
		}

		public string GetUniqueLoadID()
		{
			return $"{GetType().Name}_{uniqueID.ToString()}";
		}
	}
}
