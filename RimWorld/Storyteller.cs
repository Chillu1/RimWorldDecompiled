using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Storyteller : IExposable
	{
		public StorytellerDef def;

		public DifficultyDef difficulty;

		public Difficulty difficultyValues = new Difficulty();

		public List<StorytellerComp> storytellerComps;

		public IncidentQueue incidentQueue = new IncidentQueue();

		public static readonly Vector2 PortraitSizeTiny = new Vector2(116f, 124f);

		public static readonly Vector2 PortraitSizeLarge = new Vector2(580f, 620f);

		public const int IntervalsPerDay = 60;

		public const int CheckInterval = 1000;

		private static List<IIncidentTarget> tmpAllIncidentTargets = new List<IIncidentTarget>();

		private string debugStringCached = "Generating data...";

		public List<IIncidentTarget> AllIncidentTargets
		{
			get
			{
				tmpAllIncidentTargets.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					tmpAllIncidentTargets.Add(maps[i]);
				}
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int j = 0; j < caravans.Count; j++)
				{
					if (caravans[j].IsPlayerControlled)
					{
						tmpAllIncidentTargets.Add(caravans[j]);
					}
				}
				tmpAllIncidentTargets.Add(Find.World);
				return tmpAllIncidentTargets;
			}
		}

		public static void StorytellerStaticUpdate()
		{
			tmpAllIncidentTargets.Clear();
		}

		public Storyteller()
		{
		}

		public Storyteller(StorytellerDef def, DifficultyDef difficulty)
			: this(def, difficulty, new Difficulty(difficulty))
		{
		}

		public Storyteller(StorytellerDef def, DifficultyDef difficulty, Difficulty difficultyValues)
		{
			this.def = def;
			this.difficulty = difficulty;
			this.difficultyValues = difficultyValues;
			InitializeStorytellerComps();
		}

		private void InitializeStorytellerComps()
		{
			storytellerComps = new List<StorytellerComp>();
			for (int i = 0; i < def.comps.Count; i++)
			{
				if (def.comps[i].Enabled)
				{
					StorytellerComp storytellerComp = (StorytellerComp)Activator.CreateInstance(def.comps[i].compClass);
					storytellerComp.props = def.comps[i];
					storytellerComp.Initialize();
					storytellerComps.Add(storytellerComp);
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Defs.Look(ref difficulty, "difficulty");
			Scribe_Deep.Look(ref incidentQueue, "incidentQueue");
			if (difficulty == null)
			{
				Log.Error("Loaded storyteller without difficulty");
				difficulty = DifficultyDefOf.Rough;
			}
			if (difficulty.isCustom)
			{
				Scribe_Deep.Look(ref difficultyValues, "customDifficulty", difficulty);
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				difficultyValues = new Difficulty(difficulty);
			}
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				InitializeStorytellerComps();
			}
		}

		public void StorytellerTick()
		{
			incidentQueue.IncidentQueueTick();
			if (Find.TickManager.TicksGame % 1000 != 0 || !DebugSettings.enableStoryteller)
			{
				return;
			}
			foreach (FiringIncident item in MakeIncidentsForInterval())
			{
				TryFire(item);
			}
		}

		public bool TryFire(FiringIncident fi)
		{
			if (fi.def.Worker.CanFireNow(fi.parms) && fi.def.Worker.TryExecute(fi.parms))
			{
				fi.parms.target.StoryState.Notify_IncidentFired(fi);
				return true;
			}
			return false;
		}

		public IEnumerable<FiringIncident> MakeIncidentsForInterval()
		{
			List<IIncidentTarget> targets = AllIncidentTargets;
			for (int j = 0; j < storytellerComps.Count; j++)
			{
				foreach (FiringIncident item in MakeIncidentsForInterval(storytellerComps[j], targets))
				{
					yield return item;
				}
			}
			List<Quest> quests = Find.QuestManager.QuestsListForReading;
			for (int j = 0; j < quests.Count; j++)
			{
				if (quests[j].State != QuestState.Ongoing)
				{
					continue;
				}
				List<QuestPart> parts = quests[j].PartsListForReading;
				for (int k = 0; k < parts.Count; k++)
				{
					IIncidentMakerQuestPart incidentMakerQuestPart = parts[k] as IIncidentMakerQuestPart;
					if (incidentMakerQuestPart == null || ((QuestPartActivable)parts[k]).State != QuestPartState.Enabled)
					{
						continue;
					}
					foreach (FiringIncident item2 in incidentMakerQuestPart.MakeIntervalIncidents())
					{
						item2.sourceQuestPart = parts[k];
						item2.parms.quest = quests[j];
						yield return item2;
					}
				}
			}
		}

		public IEnumerable<FiringIncident> MakeIncidentsForInterval(StorytellerComp comp, List<IIncidentTarget> targets)
		{
			if (GenDate.DaysPassedFloat <= comp.props.minDaysPassed)
			{
				yield break;
			}
			for (int i = 0; i < targets.Count; i++)
			{
				IIncidentTarget incidentTarget = targets[i];
				bool flag = false;
				bool flag2 = comp.props.allowedTargetTags.NullOrEmpty();
				foreach (IncidentTargetTagDef item in incidentTarget.IncidentTargetTags())
				{
					if (!comp.props.disallowedTargetTags.NullOrEmpty() && comp.props.disallowedTargetTags.Contains(item))
					{
						flag = true;
						break;
					}
					if (!flag2 && comp.props.allowedTargetTags.Contains(item))
					{
						flag2 = true;
					}
				}
				if (flag || !flag2)
				{
					continue;
				}
				foreach (FiringIncident item2 in comp.MakeIntervalIncidents(incidentTarget))
				{
					if (Find.Storyteller.difficultyValues.allowBigThreats || item2.def.category != IncidentCategoryDefOf.ThreatBig)
					{
						yield return item2;
					}
				}
			}
		}

		public void Notify_PawnEvent(Pawn pawn, AdaptationEvent ev, DamageInfo? dinfo = null)
		{
			Find.StoryWatcher.watcherAdaptation.Notify_PawnEvent(pawn, ev, dinfo);
			for (int i = 0; i < storytellerComps.Count; i++)
			{
				storytellerComps[i].Notify_PawnEvent(pawn, ev, dinfo);
			}
		}

		public void Notify_DefChanged()
		{
			InitializeStorytellerComps();
		}

		public string DebugString()
		{
			if (Time.frameCount % 60 == 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("GLOBAL STORYTELLER STATS");
				stringBuilder.AppendLine("------------------------");
				stringBuilder.AppendLine("Storyteller: ".PadRight(40) + def.label);
				stringBuilder.AppendLine("Adaptation days: ".PadRight(40) + Find.StoryWatcher.watcherAdaptation.AdaptDays.ToString("F1"));
				stringBuilder.AppendLine("Adapt points factor: ".PadRight(40) + Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor.ToString("F2"));
				stringBuilder.AppendLine("Time points factor: ".PadRight(40) + Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate(GenDate.DaysPassed).ToString("F2"));
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Ally incident fraction (neutral or ally): ".PadRight(40) + StorytellerUtility.AllyIncidentFraction(fullAlliesOnly: false).ToString("F2"));
				stringBuilder.AppendLine("Ally incident fraction (ally only): ".PadRight(40) + StorytellerUtility.AllyIncidentFraction(fullAlliesOnly: true).ToString("F2"));
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(StorytellerUtilityPopulation.DebugReadout().TrimEndNewlines());
				IIncidentTarget incidentTarget = Find.WorldSelector.SingleSelectedObject as IIncidentTarget;
				if (incidentTarget == null)
				{
					incidentTarget = Find.CurrentMap;
				}
				if (incidentTarget != null)
				{
					Map map = incidentTarget as Map;
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("STATS FOR INCIDENT TARGET: " + incidentTarget);
					stringBuilder.AppendLine("------------------------");
					stringBuilder.AppendLine("Progress score: ".PadRight(40) + StorytellerUtility.GetProgressScore(incidentTarget).ToString("F2"));
					stringBuilder.AppendLine("Base points: ".PadRight(40) + StorytellerUtility.DefaultThreatPointsNow(incidentTarget).ToString("F0"));
					stringBuilder.AppendLine("Points factor random range: ".PadRight(40) + incidentTarget.IncidentPointsRandomFactorRange);
					stringBuilder.AppendLine("Wealth: ".PadRight(40) + incidentTarget.PlayerWealthForStoryteller.ToString("F0"));
					if (map != null)
					{
						if (Find.Storyteller.difficultyValues.fixedWealthMode)
						{
							stringBuilder.AppendLine($"- Wealth calculated using fixed model curve, time factor: {Find.Storyteller.difficultyValues.fixedWealthTimeFactor:F1}");
							stringBuilder.AppendLine("- Map age: ".PadRight(40) + map.AgeInDays.ToString("F1"));
						}
						stringBuilder.AppendLine("- Items: ".PadRight(40) + map.wealthWatcher.WealthItems.ToString("F0"));
						stringBuilder.AppendLine("- Buildings: ".PadRight(40) + map.wealthWatcher.WealthBuildings.ToString("F0"));
						stringBuilder.AppendLine("- Floors: ".PadRight(40) + map.wealthWatcher.WealthFloorsOnly.ToString("F0"));
						stringBuilder.AppendLine("- Pawns: ".PadRight(40) + map.wealthWatcher.WealthPawns.ToString("F0"));
					}
					stringBuilder.AppendLine("Pawn count human: ".PadRight(40) + incidentTarget.PlayerPawnsForStoryteller.Where((Pawn p) => p.def.race.Humanlike).Count());
					stringBuilder.AppendLine("Pawn count animal: ".PadRight(40) + incidentTarget.PlayerPawnsForStoryteller.Where((Pawn p) => p.def.race.Animal).Count());
					if (map != null)
					{
						stringBuilder.AppendLine("StoryDanger: ".PadRight(40) + map.dangerWatcher.DangerRating);
						stringBuilder.AppendLine("FireDanger: ".PadRight(40) + map.fireWatcher.FireDanger.ToString("F2"));
						stringBuilder.AppendLine("LastThreatBigTick days ago: ".PadRight(40) + (Find.TickManager.TicksGame - map.storyState.LastThreatBigTick).ToStringTicksToDays());
					}
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("LIST OF ALL INCIDENT TARGETS");
				stringBuilder.AppendLine("------------------------");
				for (int i = 0; i < AllIncidentTargets.Count; i++)
				{
					stringBuilder.AppendLine(i + ". " + AllIncidentTargets[i].ToString());
				}
				debugStringCached = stringBuilder.ToString();
			}
			return debugStringCached;
		}
	}
}
