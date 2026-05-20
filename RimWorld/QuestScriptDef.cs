using System;
using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class QuestScriptDef : Def
{
	public QuestNode root;

	public float rootSelectionWeight;

	public SimpleCurve rootSelectionWeightFactorFromPointsCurve;

	public bool randomlySelectable = true;

	public float rootMinPoints;

	public float rootMinProgressScore;

	public int rootEarliestDay;

	public bool rootIncreasesPopulation;

	public float minRefireDays;

	public float decreeSelectionWeight;

	public List<string> decreeTags;

	public RulePack questDescriptionRules;

	public RulePack questNameRules;

	public RulePack questDescriptionAndNameRules;

	public RulePack questContentRules;

	public RulePack questSubjectRules;

	public bool autoAccept;

	public bool hideOnCleanup;

	public FloatRange expireDaysRange = new FloatRange(-1f, -1f);

	public bool nameMustBeUnique;

	public int defaultChallengeRating = -1;

	public bool defaultHidden;

	public bool isRootSpecial;

	public bool canGiveRoyalFavor;

	public string questAvailableLetterLabel;

	public LetterDef questAvailableLetterDef;

	public bool questAvailableLetterTextIsDescription;

	public bool hideInvolvedFactionsInfo;

	public bool affectedByPopulation;

	public bool affectedByPoints = true;

	public bool defaultCharity;

	public HistoryEventDef successHistoryEvent;

	public HistoryEventDef failedOrExpiredHistoryEvent;

	public bool sendAvailableLetter = true;

	public bool epic;

	public QuestScriptDef epicParent;

	public bool endOnColonyMove = true;

	public bool everAcceptableInSpace;

	public bool neverPossibleInSpace;

	public List<QuestGiverTag> givenBy = new List<QuestGiverTag>();

	public List<PlanetLayerDef> layerWhitelist;

	public List<PlanetLayerDef> layerBlacklist;

	public bool canOccurOnAllPlanetLayers;

	[Unsaved(false)]
	private int lastCheckCanRunTick;

	[Unsaved(false)]
	private int lastCheckCanRunPoints;

	[Unsaved(false)]
	private bool lastCanRunResult;

	public bool IsRootRandomSelected
	{
		get
		{
			if (rootSelectionWeight != 0f)
			{
				return randomlySelectable;
			}
			return false;
		}
	}

	public bool IsRootDecree => decreeSelectionWeight != 0f;

	public bool IsRootAny
	{
		get
		{
			if (!IsRootRandomSelected && !IsRootDecree)
			{
				return isRootSpecial;
			}
			return true;
		}
	}

	public bool IsEpic => epic;

	public void Run()
	{
		InitializeRules();
		root.Run();
	}

	public void InitializeRules()
	{
		if (questDescriptionRules != null)
		{
			RimWorld.QuestGen.QuestGen.AddQuestDescriptionRules(questDescriptionRules);
		}
		if (questNameRules != null)
		{
			RimWorld.QuestGen.QuestGen.AddQuestNameRules(questNameRules);
		}
		if (questDescriptionAndNameRules != null)
		{
			RimWorld.QuestGen.QuestGen.AddQuestDescriptionRules(questDescriptionAndNameRules);
			RimWorld.QuestGen.QuestGen.AddQuestNameRules(questDescriptionAndNameRules);
		}
		if (questContentRules != null)
		{
			RimWorld.QuestGen.QuestGen.AddQuestContentRules(questContentRules);
		}
	}

	public bool CanRun(Slate slate, IIncidentTarget target)
	{
		using (new ProfilerBlock("CanRun()"))
		{
			if (!slate.TryGet<int>("points", out var var))
			{
				var = 0;
			}
			if (lastCheckCanRunTick == Find.TickManager.TicksGame && lastCheckCanRunPoints == var)
			{
				return lastCanRunResult;
			}
			lastCheckCanRunTick = Find.TickManager.TicksGame;
			lastCheckCanRunPoints = var;
			try
			{
				lastCanRunResult = target != null && CanQuestOccurOnTile(target.Tile) && root.TestRun(slate.DeepCopy());
				Scenario scenario = Find.Scenario;
				for (int i = 0; i < scenario.parts.Count; i++)
				{
					if (!lastCanRunResult)
					{
						break;
					}
					if (scenario.parts[i] is ScenPart_DisableQuest scenPart_DisableQuest && scenPart_DisableQuest.questDef == this)
					{
						lastCanRunResult = false;
					}
				}
			}
			catch (Exception arg)
			{
				lastCanRunResult = false;
				Log.Error($"Error while checking if can generate quest: {arg}");
				throw;
			}
			return lastCanRunResult;
		}
	}

	private bool CanQuestOccurOnTile(PlanetTile tile)
	{
		if (!tile.Valid)
		{
			return true;
		}
		PlanetLayerDef layerDef = tile.LayerDef;
		if (!layerWhitelist.NullOrEmpty() && !layerWhitelist.Contains(layerDef))
		{
			return false;
		}
		if (!layerBlacklist.NullOrEmpty() && layerBlacklist.Contains(layerDef))
		{
			return false;
		}
		if (!autoAccept && !everAcceptableInSpace && layerDef.isSpace)
		{
			return false;
		}
		if (neverPossibleInSpace && layerDef.isSpace)
		{
			return false;
		}
		return true;
	}

	public MapParent TryFindNewSuitableMapParentForRetarget()
	{
		foreach (Map playerHomeMap in Current.Game.PlayerHomeMaps)
		{
			if (CanQuestOccurOnTile(playerHomeMap.Tile))
			{
				return playerHomeMap.Parent;
			}
		}
		foreach (Map map in Current.Game.Maps)
		{
			if (CanQuestOccurOnTile(map.Tile) && GravshipUtility.GetPlayerGravEngine_NewTemp(map) != null)
			{
				return map.Parent;
			}
		}
		return null;
	}

	public bool IsParentSuitableForQuest(MapParent mapParent)
	{
		return CanQuestOccurOnTile(mapParent.Tile);
	}

	public bool CanRun(float points, IIncidentTarget target)
	{
		Slate slate = new Slate();
		slate.Set("points", points);
		return CanRun(slate, target);
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (rootSelectionWeight > 0f && !autoAccept && expireDaysRange.TrueMax <= 0f)
		{
			yield return "rootSelectionWeight > 0 but expireDaysRange not set";
		}
		if (autoAccept && expireDaysRange.TrueMax > 0f)
		{
			yield return "autoAccept but there is an expireDaysRange set";
		}
		if (defaultChallengeRating > 0 && !IsRootAny)
		{
			yield return "non-root quest has defaultChallengeRating";
		}
	}
}
