using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker
	{
		public IncidentDef def;

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

		public bool CanFireNow(IncidentParms parms, bool forced = false)
		{
			if (!parms.forced)
			{
				if (!def.TargetAllowed(parms.target))
				{
					return false;
				}
				if (GenDate.DaysPassed < def.earliestDay)
				{
					return false;
				}
				if (Find.Storyteller.difficulty.difficulty < def.minDifficulty)
				{
					return false;
				}
				if (parms.points >= 0f && parms.points < def.minThreatPoints)
				{
					return false;
				}
				if (def.allowedBiomes != null)
				{
					BiomeDef biome = Find.WorldGrid[parms.target.Tile].biome;
					if (!def.allowedBiomes.Contains(biome))
					{
						return false;
					}
				}
				Scenario scenario = Find.Scenario;
				for (int i = 0; i < scenario.parts.Count; i++)
				{
					ScenPart_DisableIncident scenPart_DisableIncident = scenario.parts[i] as ScenPart_DisableIncident;
					if (scenPart_DisableIncident != null && scenPart_DisableIncident.Incident == def)
					{
						return false;
					}
				}
				if (def.minPopulation > 0 && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Count() < def.minPopulation)
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
			}
			if (!CanFireNowSub(parms))
			{
				return false;
			}
			return true;
		}

		public bool FiredTooRecently(IIncidentTarget target)
		{
			Dictionary<IncidentDef, int> lastFireTicks = target.StoryState.lastFireTicks;
			int ticksGame = Find.TickManager.TicksGame;
			if (lastFireTicks.TryGetValue(def, out int value) && (float)(ticksGame - value) / 60000f < def.minRefireDays)
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
			Map map;
			if ((map = (parms.target as Map)) != null && def.requireColonistsPresent && map.mapPawns.FreeColonistsSpawnedCount == 0)
			{
				return true;
			}
			bool flag = TryExecuteWorker(parms);
			if (flag)
			{
				if (def.tale != null)
				{
					Pawn pawn = null;
					if (parms.target is Caravan)
					{
						pawn = ((Caravan)parms.target).RandomOwner();
					}
					else if (parms.target is Map)
					{
						pawn = ((Map)parms.target).mapPawns.FreeColonistsSpawned.RandomElementWithFallback();
					}
					else if (parms.target is World)
					{
						pawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.RandomElementWithFallback();
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
					}
				}
			}
			return flag;
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
			if (!def.letterHyperlinkHediffDefs.NullOrEmpty())
			{
				if (list3 == null)
				{
					list3 = new List<HediffDef>();
				}
				list3.AddRange(def.letterHyperlinkHediffDefs);
			}
			choiceLetter.hyperlinkHediffDefs = list3;
			Find.LetterStack.ReceiveLetter(choiceLetter);
		}
	}
}
