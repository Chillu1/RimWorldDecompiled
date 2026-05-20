using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class IncidentWorker
{
	public IncidentDef def;

	[Unsaved(false)]
	private int lastCheckCanRunTick;

	[Unsaved(false)]
	private bool lastCanRunResult;

	public virtual float BaseChanceThisGame
	{
		get
		{
			if (ModsConfig.RoyaltyActive && def.baseChanceWithRoyalty >= 0f)
			{
				return def.baseChanceWithRoyalty;
			}
			return def.baseChance;
		}
	}

	public virtual float ChanceFactorNow(IIncidentTarget target)
	{
		return 1f;
	}

	public bool CanFireNow(IncidentParms parms)
	{
		if (!parms.forced)
		{
			if (!def.TargetAllowed(parms.target))
			{
				return false;
			}
			if (!parms.bypassStorytellerSettings)
			{
				if (GenDate.DaysPassedSinceSettle < def.earliestDay)
				{
					return false;
				}
				if (!Find.Storyteller.difficulty.AllowedBy(def.disabledWhen) || (def.category == IncidentCategoryDefOf.ThreatBig && !Find.Storyteller.difficulty.allowBigThreats))
				{
					return false;
				}
			}
			if (parms.points >= 0f && parms.points < def.minThreatPoints)
			{
				return false;
			}
			if (parms.points >= 0f && parms.points > def.maxThreatPoints)
			{
				return false;
			}
			if (!def.allowedBiomes.NullOrEmpty())
			{
				BiomeDef primaryBiome = Find.WorldGrid[parms.target.Tile].PrimaryBiome;
				if (!def.allowedBiomes.Contains(primaryBiome))
				{
					return false;
				}
			}
			if (!def.disallowedBiomes.NullOrEmpty())
			{
				BiomeDef primaryBiome2 = Find.WorldGrid[parms.target.Tile].PrimaryBiome;
				if (def.disallowedBiomes.Contains(primaryBiome2))
				{
					return false;
				}
			}
			if (parms.target.Tile.Valid)
			{
				PlanetLayerDef layerDef = parms.target.Tile.LayerDef;
				if (!def.layerWhitelist.NullOrEmpty() && !def.layerWhitelist.Contains(layerDef))
				{
					return false;
				}
				if (!def.layerBlacklist.NullOrEmpty() && def.layerBlacklist.Contains(layerDef))
				{
					return false;
				}
				if (!def.canOccurOnAllPlanetLayers && layerDef.onlyAllowWhitelistedIncidents && (def.layerWhitelist.NullOrEmpty() || !def.layerWhitelist.Contains(layerDef)))
				{
					return false;
				}
			}
			if (!parms.bypassStorytellerSettings)
			{
				Scenario scenario = Find.Scenario;
				for (int i = 0; i < scenario.parts.Count; i++)
				{
					if (scenario.parts[i] is ScenPart_DisableIncident scenPart_DisableIncident && scenPart_DisableIncident.Incident == def)
					{
						return false;
					}
				}
			}
			if (def.minPopulation > 0 && PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count() < def.minPopulation)
			{
				return false;
			}
			if (FiredTooRecently(parms.target))
			{
				return false;
			}
			if (def.minGreatestPopulation > 0 && Find.StoryWatcher.statsRecord.greatestPopulation < def.minGreatestPopulation)
			{
				return false;
			}
			if (ModsConfig.AnomalyActive && def.IsAnomalyIncident && Find.Anomaly.LevelDef.anomalyThreatTier < def.minAnomalyThreatLevel && Find.Anomaly.GenerateMonolith)
			{
				return false;
			}
			if (parms.target is Map map)
			{
				foreach (GameCondition activeCondition in map.gameConditionManager.ActiveConditions)
				{
					if (activeCondition.def.preventIncidents)
					{
						return false;
					}
				}
			}
			if (Find.GameEnder.gameEnding && (def.category == IncidentCategoryDefOf.ThreatBig || def.category == IncidentCategoryDefOf.ThreatSmall))
			{
				return false;
			}
			if (Find.TickManager.TicksGame < Find.GameEnder.newWanderersCreatedTick + 300000 && def.category == IncidentCategoryDefOf.ThreatBig)
			{
				return false;
			}
		}
		if (lastCheckCanRunTick == Find.TickManager.TicksGame)
		{
			return lastCanRunResult;
		}
		lastCheckCanRunTick = Find.TickManager.TicksGame;
		if (ModsConfig.AnomalyActive && Find.Anomaly.VoidAwakeningActive())
		{
			lastCanRunResult = false;
		}
		else
		{
			lastCanRunResult = CanFireNowSub(parms);
		}
		return lastCanRunResult;
	}

	public bool FiredTooRecently(IIncidentTarget target)
	{
		Dictionary<IncidentDef, int> lastFireTicks = target.StoryState.lastFireTicks;
		int ticksGame = Find.TickManager.TicksGame;
		if (lastFireTicks.TryGetValue(def, out var value) && (float)(ticksGame - value) / 60000f < def.minRefireDays)
		{
			return true;
		}
		List<IncidentDef> refireCheckIncidents = def.RefireCheckIncidents;
		if (refireCheckIncidents != null)
		{
			for (int i = 0; i < refireCheckIncidents.Count; i++)
			{
				if (lastFireTicks.TryGetValue(refireCheckIncidents[i], out value) && (float)(ticksGame - value) / 60000f < def.minRefireDays)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual bool CanFireNowSub(IncidentParms parms)
	{
		return true;
	}

	public bool TryExecute(IncidentParms parms)
	{
		if (parms.target is Map map && def.requireColonistsPresent && map.mapPawns.FreeColonistsSpawnedCount == 0)
		{
			return true;
		}
		bool flag = TryExecuteWorker(parms);
		if (flag)
		{
			if (def.tale != null)
			{
				Pawn pawn = null;
				if (parms.target is Caravan caravan)
				{
					pawn = caravan.RandomOwner();
				}
				else if (parms.target is Map map2)
				{
					pawn = map2.mapPawns.FreeColonistsSpawned.RandomElementWithFallback();
				}
				else if (parms.target is World)
				{
					pawn = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended.RandomElementWithFallback();
				}
				if (pawn != null)
				{
					TaleRecorder.RecordTale(def.tale, pawn);
				}
			}
			if (def.category.tale != null)
			{
				Tale tale = TaleRecorder.RecordTale(def.category.tale);
				if (tale != null)
				{
					tale.customLabel = def.label;
					if (def.hidden)
					{
						tale.hidden = true;
					}
				}
			}
		}
		Find.Storyteller.RecordIncidentFired(def);
		if (ModsConfig.AnomalyActive && def.codexEntry != null)
		{
			Find.EntityCodex.SetDiscovered(def.codexEntry);
		}
		return flag;
	}

	public static void SendIncidentLetter(TaggedString baseLetterLabel, TaggedString baseLetterText, LetterDef baseLetterDef, IncidentParms parms, LookTargets lookTargets, IncidentDef def, params NamedArgument[] textArgs)
	{
		if (parms.silent || !parms.sendLetter)
		{
			return;
		}
		if (baseLetterLabel.NullOrEmpty() || baseLetterText.NullOrEmpty())
		{
			Log.Error("Sending standard incident letter with no label or text.");
		}
		TaggedString taggedString = baseLetterText.Formatted(textArgs);
		TaggedString text;
		if (parms.customLetterText.NullOrEmpty())
		{
			text = taggedString;
		}
		else
		{
			List<NamedArgument> list = new List<NamedArgument>();
			if (textArgs != null)
			{
				list.AddRange(textArgs);
			}
			list.Add(taggedString.Named("BASETEXT"));
			text = parms.customLetterText.Formatted(list.ToArray());
		}
		TaggedString taggedString2 = baseLetterLabel.Formatted(textArgs);
		TaggedString label;
		if (parms.customLetterLabel.NullOrEmpty())
		{
			label = taggedString2;
		}
		else
		{
			List<NamedArgument> list2 = new List<NamedArgument>();
			if (textArgs != null)
			{
				list2.AddRange(textArgs);
			}
			list2.Add(taggedString2.Named("BASELABEL"));
			label = parms.customLetterLabel.Formatted(list2.ToArray());
		}
		ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, parms.customLetterDef ?? baseLetterDef, lookTargets, parms.faction, parms.quest, parms.letterHyperlinkThingDefs);
		List<HediffDef> list3 = new List<HediffDef>();
		if (!parms.letterHyperlinkHediffDefs.NullOrEmpty())
		{
			list3.AddRange(parms.letterHyperlinkHediffDefs);
		}
		if (def != null && !def.letterHyperlinkHediffDefs.NullOrEmpty())
		{
			list3.AddRange(def.letterHyperlinkHediffDefs);
		}
		choiceLetter.hyperlinkHediffDefs = list3;
		Find.LetterStack.ReceiveLetter(choiceLetter);
	}

	protected virtual bool TryExecuteWorker(IncidentParms parms)
	{
		Log.Error("Unimplemented incident " + this);
		return false;
	}

	protected void SendStandardLetter(IncidentParms parms, LookTargets lookTargets, params NamedArgument[] textArgs)
	{
		SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, lookTargets, textArgs);
	}

	protected void SendStandardLetter(TaggedString baseLetterLabel, TaggedString baseLetterText, LetterDef baseLetterDef, IncidentParms parms, LookTargets lookTargets, params NamedArgument[] textArgs)
	{
		SendIncidentLetter(baseLetterLabel, baseLetterText, baseLetterDef, parms, lookTargets, def, textArgs);
	}
}
