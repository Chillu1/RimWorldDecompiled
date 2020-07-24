using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse.AI.Group;

namespace Verse
{
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
								return NeurotrainerDefGenerator.NeurotrainerDefPrefix + "_" + childNode.InnerText;
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
				switch (index)
				{
				case 9:
					return 10;
				case 10:
					return 12;
				case 11:
					return 14;
				case 12:
					return 15;
				case 13:
					return 9;
				case 14:
					return 11;
				case 15:
					return 13;
				default:
					return index;
				}
			}
			return index;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Game game = obj as Game;
				if (game != null && game.questManager == null)
				{
					game.questManager = new QuestManager();
				}
				Zone zone = obj as Zone;
				if (zone != null && zone.ID == -1)
				{
					zone.ID = Find.UniqueIDsManager.GetNextZoneID();
				}
			}
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			Pawn pawn = obj as Pawn;
			if (pawn != null && pawn.royalty == null)
			{
				pawn.royalty = new Pawn_RoyaltyTracker(pawn);
			}
			Pawn_NativeVerbs pawn_NativeVerbs = obj as Pawn_NativeVerbs;
			if (pawn_NativeVerbs != null && pawn_NativeVerbs.verbTracker == null)
			{
				pawn_NativeVerbs.verbTracker = new VerbTracker(pawn_NativeVerbs);
			}
			Thing thing = obj as Thing;
			if (thing != null)
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
			StoryWatcher storyWatcher = obj as StoryWatcher;
			if (storyWatcher != null)
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
			FoodRestrictionDatabase foodRestrictionDatabase = obj as FoodRestrictionDatabase;
			if (foodRestrictionDatabase != null && VersionControl.BuildFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) < 2057)
			{
				List<FoodRestriction> allFoodRestrictions = foodRestrictionDatabase.AllFoodRestrictions;
				for (int i = 0; i < allFoodRestrictions.Count; i++)
				{
					allFoodRestrictions[i].filter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, allow: true);
					allFoodRestrictions[i].filter.SetAllow(ThingCategoryDefOf.CorpsesAnimal, allow: true);
				}
			}
			SitePart sitePart = obj as SitePart;
			if (sitePart != null)
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
					ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
					activeDropPodInfo.innerContainer.TryAdd(item, 1);
					activeDropPodInfo.openDelay = 60;
					activeDropPodInfo.leaveSlag = false;
					activeDropPodInfo.despawnPodBeforeSpawningThing = true;
					activeDropPodInfo.spawnWipeMode = WipeMode.Vanish;
					DropPodUtility.MakeDropPodAt(invalid, map, activeDropPodInfo);
				}
			}
			upgradedCrashedShipParts.Clear();
			List<Site> sites = Find.WorldObjects.Sites;
			int l;
			for (l = 0; l < sites.Count; l++)
			{
				if (!Find.QuestManager.QuestsListForReading.Any((Quest x) => x.QuestLookTargets.Contains(sites[l])))
				{
					Quest quest = Quest.MakeRaw();
					QuestUtility.GenerateBackCompatibilityNameFor(quest);
					quest.SetInitiallyAccepted();
					quest.appearanceTick = sites[l].creationGameTicks;
					TimeoutComp component = sites[l].GetComponent<TimeoutComp>();
					if (component != null && component.Active && !sites[l].HasMap)
					{
						QuestPartUtility.MakeAndAddQuestTimeoutDelay(quest, component.TicksLeft, sites[l]);
						component.StopTimeout();
					}
					QuestPartUtility.MakeAndAddEndCondition<QuestPart_NoWorldObject>(quest, quest.InitiateSignal, QuestEndOutcome.Unknown).worldObject = sites[l];
					ChoiceLetter choiceLetter = Find.Archive.ArchivablesListForReading.OfType<ChoiceLetter>().FirstOrDefault((ChoiceLetter x) => x.lookTargets != null && x.lookTargets.targets.Contains(sites[l]));
					if (choiceLetter != null)
					{
						quest.description = choiceLetter.text;
					}
					Find.QuestManager.Add(quest);
				}
			}
			List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
			int k;
			for (k = 0; k < worldObjects.Count; k++)
			{
				if (worldObjects[k].def == WorldObjectDefOf.EscapeShip && !Find.QuestManager.QuestsListForReading.Any((Quest x) => x.PartsListForReading.Any((QuestPart y) => y is QuestPart_NoWorldObject && ((QuestPart_NoWorldObject)y).worldObject == worldObjects[k])))
				{
					MakeAndAddWorldObjectQuest(worldObjects[k], null);
				}
			}
			int j;
			for (j = 0; j < worldObjects.Count; j++)
			{
				if (worldObjects[j] is PeaceTalks && !Find.QuestManager.QuestsListForReading.Any((Quest x) => x.PartsListForReading.Any((QuestPart y) => y is QuestPart_NoWorldObject && ((QuestPart_NoWorldObject)y).worldObject == worldObjects[j])))
				{
					Quest quest2 = MakeAndAddWorldObjectQuest(worldObjects[j], null);
					ChoiceLetter choiceLetter2 = Find.Archive.ArchivablesListForReading.OfType<ChoiceLetter>().FirstOrDefault((ChoiceLetter x) => x.lookTargets != null && x.lookTargets.targets.Contains(worldObjects[j]));
					if (choiceLetter2 != null)
					{
						quest2.description = choiceLetter2.text;
					}
				}
			}
			int i;
			for (i = 0; i < worldObjects.Count; i++)
			{
				TradeRequestComp component2 = worldObjects[i].GetComponent<TradeRequestComp>();
				if (component2 != null && component2.ActiveRequest && !Find.QuestManager.QuestsListForReading.Any((Quest x) => x.PartsListForReading.Any((QuestPart y) => y is QuestPart_NoWorldObject && ((QuestPart_NoWorldObject)y).worldObject == worldObjects[i])))
				{
					Quest quest3 = MakeAndAddTradeRequestQuest(worldObjects[i], null, component2);
					ChoiceLetter choiceLetter3 = Find.Archive.ArchivablesListForReading.OfType<ChoiceLetter>().FirstOrDefault((ChoiceLetter x) => x.lookTargets != null && x.lookTargets.targets.Contains(worldObjects[i]));
					if (choiceLetter3 != null)
					{
						quest3.description = choiceLetter3.text;
					}
				}
			}
		}
	}
}
