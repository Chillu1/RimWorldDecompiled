using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;

namespace Verse;

public class BackCompatibilityConverter_1_0 : BackCompatibilityConverter
{
	private struct UpgradedCrashedShipPart
	{
		public string originalDefName;

		public Thing thing;
	}

	private static List<XmlNode> oldCrashedShipParts = new List<XmlNode>();

	private static List<UpgradedCrashedShipPart> upgradedCrashedShipParts = new List<UpgradedCrashedShipPart>();

	public override bool AppliesToVersion(int majorVer, int minorVer)
	{
		if (majorVer <= 1)
		{
			if (majorVer != 0)
			{
				return minorVer == 0;
			}
			return true;
		}
		return false;
	}

	public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
	{
		if (defType == typeof(ThingDef))
		{
			switch (defName)
			{
			case "CrashedPoisonShipPart":
			case "CrashedPsychicEmanatorShipPart":
				return "MechCapsule";
			case "PoisonSpreader":
				return "Defoliator";
			case "PoisonSpreaderShipPart":
				return "DefoliatorShipPart";
			case "MechSerumNeurotrainer":
			{
				XmlNode xmlNode = node?.ParentNode;
				if (xmlNode != null && xmlNode.HasChildNodes)
				{
					foreach (XmlNode childNode in xmlNode.ChildNodes)
					{
						if (childNode.Name == "skill")
						{
							return ThingDefGenerator_Neurotrainer.NeurotrainerDefPrefix + "_" + childNode.InnerText;
						}
					}
				}
				return DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef def) => def.thingCategories != null && def.thingCategories.Contains(ThingCategoryDefOf.Neurotrainers)).RandomElementWithFallback()?.defName;
			}
			}
		}
		else if ((defType == typeof(QuestScriptDef) || defType == typeof(TaleDef)) && defName == "JourneyOffer")
		{
			return "EndGame_ShipEscape";
		}
		return null;
	}

	public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
	{
		if (baseType == typeof(Thing))
		{
			if (providedClassName == "Building_CrashedShipPart" && node["def"] != null)
			{
				oldCrashedShipParts.Add(node);
				return ThingDefOf.MechCapsule.thingClass;
			}
		}
		else if (baseType == typeof(LordJob) && providedClassName == "LordJob_MechanoidsDefendShip")
		{
			XmlElement xmlElement = node["shipPart"];
			if (xmlElement != null)
			{
				xmlElement.InnerText = xmlElement.InnerText.Replace("Thing_CrashedPsychicEmanatorShipPart", "Thing_MechCapsule").Replace("Thing_CrashedPoisonShipPart", "Thing_MechCapsule");
			}
			return typeof(LordJob_MechanoidsDefend);
		}
		return null;
	}

	public override int GetBackCompatibleBodyPartIndex(BodyDef body, int index)
	{
		if (body == BodyDefOf.MechanicalCentipede)
		{
			return index switch
			{
				9 => 10, 
				10 => 12, 
				11 => 14, 
				12 => 15, 
				13 => 9, 
				14 => 11, 
				15 => 13, 
				_ => index, 
			};
		}
		return index;
	}

	public override void PostExposeData(object obj)
	{
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (obj is Game { questManager: null } game)
			{
				game.questManager = new QuestManager();
			}
			if (obj is Zone { ID: -1 } zone)
			{
				zone.ID = Find.UniqueIDsManager.GetNextZoneID();
			}
		}
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (obj is Pawn { royalty: null } pawn)
		{
			pawn.royalty = new Pawn_RoyaltyTracker(pawn);
		}
		if (obj is Pawn_NativeVerbs { verbTracker: null } pawn_NativeVerbs)
		{
			pawn_NativeVerbs.verbTracker = new VerbTracker(pawn_NativeVerbs);
		}
		if (obj is Thing thing)
		{
			if (thing.def.defName == "Sandbags" && thing.Stuff == null)
			{
				thing.SetStuffDirect(ThingDefOf.Cloth);
			}
			if (thing.def == ThingDefOf.MechCapsule)
			{
				foreach (XmlNode oldCrashedShipPart in oldCrashedShipParts)
				{
					XmlElement xmlElement = oldCrashedShipPart["def"];
					XmlElement xmlElement2 = oldCrashedShipPart["id"];
					if (xmlElement != null && xmlElement2 != null && Thing.IDNumberFromThingID(xmlElement2.InnerText) == thing.thingIDNumber)
					{
						upgradedCrashedShipParts.Add(new UpgradedCrashedShipPart
						{
							originalDefName = xmlElement.InnerText,
							thing = thing
						});
					}
				}
			}
		}
		if (obj is StoryWatcher storyWatcher)
		{
			if (storyWatcher.watcherAdaptation == null)
			{
				storyWatcher.watcherAdaptation = new StoryWatcher_Adaptation();
			}
			if (storyWatcher.watcherPopAdaptation == null)
			{
				storyWatcher.watcherPopAdaptation = new StoryWatcher_PopAdaptation();
			}
		}
		if (obj is FoodRestrictionDatabase foodRestrictionDatabase && ScribeMetaHeaderUtility.loadedGameVersionBuild < 2057)
		{
			List<FoodPolicy> allFoodRestrictions = foodRestrictionDatabase.AllFoodRestrictions;
			for (int i = 0; i < allFoodRestrictions.Count; i++)
			{
				allFoodRestrictions[i].filter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, allow: true);
				allFoodRestrictions[i].filter.SetAllow(ThingCategoryDefOf.CorpsesAnimal, allow: true);
			}
		}
		if (obj is SitePart sitePart)
		{
			sitePart.hidden = sitePart.def.defaultHidden;
		}
	}

	public static Quest MakeAndAddWorldObjectQuest(WorldObject destination, string description)
	{
		Quest quest = Quest.MakeRaw();
		quest.SetInitiallyAccepted();
		QuestPartUtility.MakeAndAddEndCondition<QuestPart_NoWorldObject>(quest, quest.InitiateSignal, QuestEndOutcome.Unknown).worldObject = destination;
		quest.description = description;
		Find.QuestManager.Add(quest);
		return quest;
	}

	public static Quest MakeAndAddTradeRequestQuest(WorldObject destination, string description, TradeRequestComp tradeRequest)
	{
		Quest quest = Quest.MakeRaw();
		quest.SetInitiallyAccepted();
		string text = "Quest" + quest.id + ".TradeRequestSite";
		QuestUtility.AddQuestTag(ref destination.questTags, text);
		QuestPartUtility.MakeAndAddEndCondition<QuestPart_NoWorldObject>(quest, quest.InitiateSignal, QuestEndOutcome.Unknown).worldObject = destination;
		QuestPartUtility.MakeAndAddEndCondition<QuestPart_NoWorldObject>(quest, text + ".TradeRequestFulfilled", QuestEndOutcome.Success);
		if (destination.rewards != null)
		{
			QuestPart_GiveToCaravan questPart_GiveToCaravan = new QuestPart_GiveToCaravan
			{
				inSignal = text + ".TradeRequestFulfilled",
				Things = destination.rewards
			};
			foreach (Thing thing in questPart_GiveToCaravan.Things)
			{
				thing.holdingOwner = null;
			}
			quest.AddPart(questPart_GiveToCaravan);
		}
		quest.description = description;
		Find.QuestManager.Add(quest);
		return quest;
	}

	public override void PreLoadSavegame(string loadingVersion)
	{
		oldCrashedShipParts.Clear();
		upgradedCrashedShipParts.Clear();
	}

	public override void PostLoadSavegame(string loadingVersion)
	{
		oldCrashedShipParts.Clear();
		foreach (UpgradedCrashedShipPart upgradedCrashedShipPart in upgradedCrashedShipParts)
		{
			Thing thing = upgradedCrashedShipPart.thing;
			IntVec3 invalid = IntVec3.Invalid;
			Map map = null;
			if (thing.Spawned)
			{
				invalid = thing.Position;
				map = thing.Map;
			}
			else
			{
				Skyfaller obj = thing.ParentHolder as Skyfaller;
				if (obj == null)
				{
					thing.Destroy();
				}
				invalid = obj.Position;
				map = obj.Map;
			}
			if (!(invalid == IntVec3.Invalid))
			{
				invalid = new IntVec3(invalid.x - Mathf.CeilToInt((float)thing.def.size.x / 2f), invalid.y, invalid.z);
				Thing item = null;
				if (upgradedCrashedShipPart.originalDefName == "CrashedPoisonShipPart" || upgradedCrashedShipPart.originalDefName == "PoisonSpreaderShipPart")
				{
					item = ThingMaker.MakeThing(ThingDefOf.DefoliatorShipPart);
				}
				else if (upgradedCrashedShipPart.originalDefName == "CrashedPsychicEmanatorShipPart")
				{
					item = ThingMaker.MakeThing(ThingDefOf.PsychicDronerShipPart);
				}
				ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
				activeTransporterInfo.innerContainer.TryAdd(item, 1);
				activeTransporterInfo.openDelay = 60;
				activeTransporterInfo.leaveSlag = false;
				activeTransporterInfo.despawnPodBeforeSpawningThing = true;
				activeTransporterInfo.spawnWipeMode = WipeMode.Vanish;
				DropPodUtility.MakeDropPodAt(invalid, map, activeTransporterInfo);
			}
		}
		upgradedCrashedShipParts.Clear();
		List<Site> sites = Find.WorldObjects.Sites;
		int i;
		for (i = 0; i < sites.Count; i++)
		{
			if (!Find.QuestManager.QuestsListForReading.Any((Quest x) => x.QuestLookTargets.Contains(sites[i])))
			{
				Quest quest = Quest.MakeRaw();
				QuestUtility.GenerateBackCompatibilityNameFor(quest);
				quest.SetInitiallyAccepted();
				quest.appearanceTick = sites[i].creationGameTicks;
				TimeoutComp component = sites[i].GetComponent<TimeoutComp>();
				if (component != null && component.Active && !sites[i].HasMap)
				{
					QuestPartUtility.MakeAndAddQuestTimeoutDelay(quest, component.TicksLeft, sites[i]);
					component.StopTimeout();
				}
				QuestPartUtility.MakeAndAddEndCondition<QuestPart_NoWorldObject>(quest, quest.InitiateSignal, QuestEndOutcome.Unknown).worldObject = sites[i];
				ChoiceLetter choiceLetter = Find.Archive.ArchivablesListForReading.OfType<ChoiceLetter>().FirstOrDefault((ChoiceLetter x) => x.lookTargets != null && x.lookTargets.targets.Contains(sites[i]));
				if (choiceLetter != null)
				{
					quest.description = choiceLetter.Text;
				}
				Find.QuestManager.Add(quest);
			}
		}
		List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
		int i2;
		for (i2 = 0; i2 < worldObjects.Count; i2++)
		{
			if (worldObjects[i2].def == WorldObjectDefOf.EscapeShip && !Find.QuestManager.QuestsListForReading.Any((Quest x) => x.PartsListForReading.Any((QuestPart y) => y is QuestPart_NoWorldObject && ((QuestPart_NoWorldObject)y).worldObject == worldObjects[i2])))
			{
				MakeAndAddWorldObjectQuest(worldObjects[i2], null);
			}
		}
		int i3;
		for (i3 = 0; i3 < worldObjects.Count; i3++)
		{
			if (worldObjects[i3] is PeaceTalks && !Find.QuestManager.QuestsListForReading.Any((Quest x) => x.PartsListForReading.Any((QuestPart y) => y is QuestPart_NoWorldObject && ((QuestPart_NoWorldObject)y).worldObject == worldObjects[i3])))
			{
				Quest quest2 = MakeAndAddWorldObjectQuest(worldObjects[i3], null);
				ChoiceLetter choiceLetter2 = Find.Archive.ArchivablesListForReading.OfType<ChoiceLetter>().FirstOrDefault((ChoiceLetter x) => x.lookTargets != null && x.lookTargets.targets.Contains(worldObjects[i3]));
				if (choiceLetter2 != null)
				{
					quest2.description = choiceLetter2.Text;
				}
			}
		}
		int i4;
		for (i4 = 0; i4 < worldObjects.Count; i4++)
		{
			TradeRequestComp component2 = worldObjects[i4].GetComponent<TradeRequestComp>();
			if (component2 != null && component2.ActiveRequest && !Find.QuestManager.QuestsListForReading.Any((Quest x) => x.PartsListForReading.Any((QuestPart y) => y is QuestPart_NoWorldObject && ((QuestPart_NoWorldObject)y).worldObject == worldObjects[i4])))
			{
				Quest quest3 = MakeAndAddTradeRequestQuest(worldObjects[i4], null, component2);
				ChoiceLetter choiceLetter3 = Find.Archive.ArchivablesListForReading.OfType<ChoiceLetter>().FirstOrDefault((ChoiceLetter x) => x.lookTargets != null && x.lookTargets.targets.Contains(worldObjects[i4]));
				if (choiceLetter3 != null)
				{
					quest3.description = choiceLetter3.Text;
				}
			}
		}
	}
}
