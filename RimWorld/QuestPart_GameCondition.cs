using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_GameCondition : QuestPart
{
	public string inSignal;

	public GameCondition gameCondition;

	public bool targetWorld;

	public MapParent mapParent;

	public bool sendStandardLetter = true;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (mapParent != null)
			{
				yield return mapParent;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal) || (!targetWorld && mapParent == null) || gameCondition == null)
		{
			return;
		}
		gameCondition.quest = quest;
		if (targetWorld)
		{
			Find.World.gameConditionManager.RegisterCondition(gameCondition);
		}
		else
		{
			Map map = mapParent.Map ?? quest.TryFindNewSuitableMapParentForRetarget()?.Map ?? Find.AnyPlayerHomeMap;
			if (!gameCondition.CanApplyOnMap(map))
			{
				bool flag = false;
				foreach (Map playerHomeMap in Current.Game.PlayerHomeMaps)
				{
					if (gameCondition.CanApplyOnMap(playerHomeMap))
					{
						map = playerHomeMap;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return;
				}
			}
			map.gameConditionManager.RegisterCondition(gameCondition);
		}
		if (sendStandardLetter)
		{
			Find.LetterStack.ReceiveLetter(gameCondition.LabelCap, gameCondition.LetterText, gameCondition.def.letterDef, LookTargets.Invalid, null, quest);
		}
		gameCondition = null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Deep.Look(ref gameCondition, "gameCondition");
		Scribe_Values.Look(ref targetWorld, "targetWorld", defaultValue: false);
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: true);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (Find.AnyPlayerHomeMap != null)
		{
			gameCondition = GameConditionMaker.MakeCondition(GameConditionDefOf.ColdSnap, Rand.RangeInclusive(2500, 7500));
			mapParent = Find.RandomPlayerHomeMap.Parent;
		}
	}
}
