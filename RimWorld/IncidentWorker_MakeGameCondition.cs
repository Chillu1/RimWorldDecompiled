using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IncidentWorker_MakeGameCondition : IncidentWorker
{
	public virtual GameConditionDef GetGameConditionDef(IncidentParms parms)
	{
		return def.gameCondition;
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
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
		GameConditionManager gameConditionManager = parms.target.GameConditionManager;
		if (gameConditionManager == null)
		{
			Log.ErrorOnce($"Couldn't find condition manager for incident target {parms.target}", 70849667);
			return false;
		}
		GameConditionDef gameConditionDef = GetGameConditionDef(parms);
		if (gameConditionDef == null)
		{
			return false;
		}
		if (gameConditionManager.ConditionIsActive(gameConditionDef))
		{
			return false;
		}
		List<GameCondition> activeConditions = gameConditionManager.ActiveConditions;
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (!gameConditionDef.CanCoexistWith(activeConditions[i].def))
			{
				return false;
			}
		}
		if (ModsConfig.OdysseyActive && gameConditionDef.requireFish && (!(parms.target is Map { waterBodyTracker: not null } map2) || !map2.waterBodyTracker.AnyBodyContainsFish))
		{
			return false;
		}
		return true;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		GameConditionManager gameConditionManager = parms.target.GameConditionManager;
		GameConditionDef gameConditionDef = GetGameConditionDef(parms);
		int duration = Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f);
		GameCondition gameCondition = GameConditionMaker.MakeCondition(gameConditionDef, duration);
		gameConditionManager.RegisterCondition(gameCondition);
		TryFireLetter(parms, gameCondition);
		return true;
	}

	private void TryFireLetter(IncidentParms parms, GameCondition condition)
	{
		if (def.letterLabel.NullOrEmpty() || condition.def.letterText.NullOrEmpty() || (parms.target is Map map && condition.HiddenByOtherCondition(map)))
		{
			return;
		}
		bool flag = false;
		foreach (Map map2 in Find.Maps)
		{
			if (condition.CanApplyOnMap(map2))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			parms.letterHyperlinkThingDefs = condition.def.letterHyperlinks;
			SendStandardLetter(def.letterLabel, condition.LetterText, def.letterDef, parms, LookTargets.Invalid);
		}
	}
}
