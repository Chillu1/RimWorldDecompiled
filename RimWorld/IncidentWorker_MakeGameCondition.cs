using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_MakeGameCondition : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			GameConditionManager gameConditionManager = parms.target.GameConditionManager;
			if (gameConditionManager == null)
			{
				Log.ErrorOnce($"Couldn't find condition manager for incident target {parms.target}", 70849667);
				return false;
			}
			if (gameConditionManager.ConditionIsActive(def.gameCondition))
			{
				return false;
			}
			List<GameCondition> activeConditions = gameConditionManager.ActiveConditions;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				if (!def.gameCondition.CanCoexistWith(activeConditions[i].def))
				{
					return false;
				}
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			GameConditionManager gameConditionManager = parms.target.GameConditionManager;
			GameCondition gameCondition = GameConditionMaker.MakeCondition(duration: Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f), def: def.gameCondition);
			gameConditionManager.RegisterCondition(gameCondition);
			parms.letterHyperlinkThingDefs = gameCondition.def.letterHyperlinks;
			SendStandardLetter(def.letterLabel, gameCondition.LetterText, def.letterDef, parms, LookTargets.Invalid);
			return true;
		}
	}
}
