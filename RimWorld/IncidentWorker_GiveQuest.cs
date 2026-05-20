using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class IncidentWorker_GiveQuest : IncidentWorker
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		QuestScriptDef questScriptDef = def.questScriptDef ?? parms.questScriptDef;
		if (questScriptDef != null && !questScriptDef.CanRun(parms.points, parms.target))
		{
			return false;
		}
		if (!CanQuestOccurOnTile(parms.target.Tile, questScriptDef))
		{
			return false;
		}
		return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended.Any();
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		QuestScriptDef questScriptDef = def.questScriptDef ?? parms.questScriptDef ?? NaturalRandomQuestChooser.ChooseNaturalRandomQuest(parms.points, parms.target);
		if (questScriptDef == null)
		{
			return false;
		}
		if (!CanQuestOccurOnTile(parms.target.Tile, questScriptDef))
		{
			return false;
		}
		parms.questScriptDef = questScriptDef;
		GiveQuest(parms, questScriptDef);
		return true;
	}

	protected virtual void GiveQuest(IncidentParms parms, QuestScriptDef questDef)
	{
		Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, parms.points);
		if (!quest.hidden && questDef.sendAvailableLetter)
		{
			QuestUtility.SendLetterQuestAvailable(quest);
		}
	}

	private static bool CanQuestOccurOnTile(PlanetTile tile, QuestScriptDef quest)
	{
		if (!tile.Valid)
		{
			return true;
		}
		if (quest != null)
		{
			PlanetLayerDef layerDef = tile.LayerDef;
			if (!quest.layerWhitelist.NullOrEmpty() && !quest.layerWhitelist.Contains(layerDef))
			{
				return false;
			}
			if (!quest.layerBlacklist.NullOrEmpty() && quest.layerBlacklist.Contains(layerDef))
			{
				return false;
			}
			if (!quest.canOccurOnAllPlanetLayers && layerDef.onlyAllowWhitelistedIncidents && (quest.layerWhitelist.NullOrEmpty() || !quest.layerWhitelist.Contains(layerDef)))
			{
				return false;
			}
			if (!quest.everAcceptableInSpace && layerDef.isSpace)
			{
				return false;
			}
		}
		return !tile.LayerDef.onlyAllowWhitelistedQuests;
	}
}
