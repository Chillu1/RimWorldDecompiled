using System;
using System.Collections.Generic;
using System.Xml;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class BackCompatibilityConverter_Universal : BackCompatibilityConverter
	{
		private Dictionary<Building, ColorInt> lampsToColors = new Dictionary<Building, ColorInt>(128);

		public override bool AppliesToVersion(int majorVer, int minorVer)
		{
			return true;
		}

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (defType == typeof(ThingDef))
			{
				switch (defName)
				{
				case "WoolYak":
					return "WoolSheep";
				case "Plant_TreeAnimus":
				case "Plant_TreeAnimusSmall":
				case "Plant_TreeAnimaSmall":
				case "Plant_TreeAnimaNormal":
				case "Plant_TreeAnimaHardy":
					return "Plant_TreeAnima";
				case "Psytrainer_EntropyLink":
					return "Psytrainer_EntropyDump";
				case "PsylinkNeuroformer":
					return "PsychicAmplifier";
				case "PsychicShockLance":
					return "Apparel_PsychicShockLance";
				case "PsychicInsanityLance":
					return "Apparel_PsychicInsanityLance";
				case "Nutrifungus":
					return "Plant_Nutrifungus";
				case "Mech_Centipede":
					return "Mech_CentipedeBlaster";
				case "Corpse_Mech_Centipede":
					return "Corpse_Mech_CentipedeBlaster";
				case "AncientDiabolusRemains":
					return "AncientUltraDiabolusRemains";
				case "AncientUltraDiabolusRemains":
					return "AncientExostriderRemains";
				case "MegaspiderCocoon":
					return "CocoonMegaspider";
				case "MegascarabCocoon":
					return "CocoonMegascarab";
				case "SpelopedeCocoon":
					return "CocoonSpelopede";
				case "AncientCentipedeShell":
					return "ChunkMechanoidSlag";
				case "BasicSubcore":
					return "SubcoreBasic";
				case "RegularSubcore":
					return "SubcoreRegular";
				case "HighSubcore":
					return "SubcoreHigh";
				case "AncientMechanoidShell":
					return "ChunkMechanoidSlag";
				case "XenogermExtractor":
					return "GeneExtractor";
				case "Mech_Purger":
					return "Mech_Tunneler";
				case "RemoteCharger":
					return "MechBooster";
				case "StandingLamp_Red":
					return "StandingLamp";
				case "StandingLamp_Green":
					return "StandingLamp";
				case "StandingLamp_Blue":
					return "StandingLamp";
				case "Darklamp":
					return "StandingLamp";
				case "MechanitorComplexMap":
					return "MechanoidTransponder";
				}
			}
			if (defType == typeof(HediffDef))
			{
				if (defName == "Psylink")
				{
					return "PsychicAmplifier";
				}
				if (defName == "RemoteCharge")
				{
					return "MechBoost";
				}
			}
			if (defType == typeof(PreceptDef) && defName == "FuneralDestroyed")
			{
				return "FuneralNoCorpse";
			}
			if (defType == typeof(RitualOutcomeEffectDef) && defName == "AttendedFuneralDestroyed")
			{
				return "AttendedFuneralNoCorpse";
			}
			if (defType == typeof(AbilityDef))
			{
				if (defName == "PreachingOfHealing")
				{
					return "PreachHealth";
				}
				if (defName == "HeartenHealth")
				{
					return "PreachHealth";
				}
			}
			if (defType == typeof(IdeoIconDef))
			{
				switch (defName)
				{
				case "PoliticalA":
					return "Eagle";
				case "NatureA":
					return "Treeflat";
				case "PirateA":
					return "Steer";
				case "PirateB":
					return "Skull";
				case "PoliticalB":
					return "OliveBranches";
				case "ReligionA":
					return "DownBurst";
				case "ReligionB":
					return "TripleCross";
				}
			}
			if (defType == typeof(PawnKindDef))
			{
				if (defName == "Mech_Centipede")
				{
					return "Mech_CentipedeBlaster";
				}
				if (defName == "Mech_Purger")
				{
					return "Mech_Tunneler";
				}
			}
			if (defType == typeof(ThoughtDef))
			{
				if (defName == "AteFungus_Prefered")
				{
					return "AteFungus_Preferred";
				}
				if (defName == "AteFungusAsIngredient_Prefered")
				{
					return "AteFungusAsIngredient_Preferred";
				}
			}
			if (defType == typeof(JobDef))
			{
				if (defName == "StudyThing")
				{
					return "StudyInteract";
				}
				if (defName == "StudyBuilding")
				{
					return "StudyInteract";
				}
			}
			if (defType == typeof(ThingStyleDef))
			{
				if (defName.EndsWith("StandingLamp_Red") || defName.EndsWith("StandingLamp_Green") || defName.EndsWith("StandingLamp_Blue"))
				{
					return defName.Substring(0, defName.IndexOf("StandingLamp") + "StandingLamp".Length);
				}
				if (defName.EndsWith("DarklampStanding"))
				{
					return defName.Substring(0, defName.IndexOf("DarklampStanding")) + "StandingLamp";
				}
			}
			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			if (providedClassName == "Hediff_PsychicAmplifier")
			{
				return typeof(Hediff_Psylink);
			}
			if (providedClassName == "Graphic_MotePulse")
			{
				return typeof(Graphic_MoteWithAgeSecs);
			}
			if (node != null && (providedClassName == "ThingWithComps" || providedClassName == "Verse.ThingWithComps"))
			{
				XmlElement xmlElement = node["def"];
				if (xmlElement != null)
				{
					if (xmlElement.InnerText == "PsychicShockLance")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "PsychicInsanityLance")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "OrbitalTargeterBombardment")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "OrbitalTargeterPowerBeam")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "OrbitalTargeterMechCluster")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "TornadoGenerator")
					{
						return typeof(Apparel);
					}
				}
			}
			if (providedClassName == "Building_AncientUltraDiabolusRemains")
			{
				return typeof(Building_AncientMechRemains);
			}
			if (baseType == typeof(WorldObject) && node != null && providedClassName == typeof(WorldObject).FullName && node["def"]?.InnerText == "AbandonedSettlement")
			{
				return typeof(AbandonedSettlement);
			}
			if ((providedClassName == "Precept_Ritual" || providedClassName == "RimWorld.Precept") && node["def"].InnerText == "GravshipLaunch")
			{
				return typeof(Precept_GravshipLaunch);
			}
			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				int loadedGameVersionBuild = ScribeMetaHeaderUtility.loadedGameVersionBuild;
				if (obj is Pawn_RoyaltyTracker pawn_RoyaltyTracker && loadedGameVersionBuild <= 2575)
				{
					foreach (RoyalTitle item in pawn_RoyaltyTracker.AllTitlesForReading)
					{
						item.conceited = RoyalTitleUtility.ShouldBecomeConceitedOnNewTitle(pawn_RoyaltyTracker.pawn);
					}
				}
				if (loadedGameVersionBuild < 3167)
				{
					MealRestrictionsReworkBackCompat(obj);
				}
				if (loadedGameVersionBuild < 3156)
				{
					BiosculpterReworkBackCompat(obj);
				}
				ApplyLampColor(obj);
				if (obj is Pawn_NeedsTracker pawn_NeedsTracker)
				{
					pawn_NeedsTracker.AllNeeds.RemoveAll((Need n) => n.def.defName == "Authority");
				}
				if (obj is History { historyEventsManager: null } history)
				{
					history.historyEventsManager = new HistoryEventsManager();
				}
			}
			if (obj is Pawn pawn)
			{
				if (pawn.abilities == null)
				{
					if (!pawn.RaceProps.ShouldHaveAbilityTracker)
					{
						PawnKindDef kindDef = pawn.kindDef;
						if (kindDef == null || kindDef.abilities.NullOrEmpty())
						{
							goto IL_019b;
						}
					}
					pawn.abilities = new Pawn_AbilityTracker(pawn);
					PawnKindDef kindDef2 = pawn.kindDef;
					if (kindDef2 != null && !kindDef2.abilities.NullOrEmpty())
					{
						Pawn pawn2 = pawn;
						if (pawn2.abilities == null)
						{
							pawn2.abilities = new Pawn_AbilityTracker(pawn);
						}
						for (int num = 0; num < pawn.kindDef.abilities.Count; num++)
						{
							pawn.abilities.GainAbility(pawn.kindDef.abilities[num]);
						}
					}
				}
				goto IL_019b;
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				SaveLampColor(obj);
				if (obj is Map map)
				{
					Map map2 = map;
					if (map2.temporaryThingDrawer == null)
					{
						map2.temporaryThingDrawer = new TemporaryThingDrawer();
					}
					map2 = map;
					if (map2.flecks == null)
					{
						map2.flecks = new FleckManager(map);
					}
					map2 = map;
					if (map2.autoSlaughterManager == null)
					{
						map2.autoSlaughterManager = new AutoSlaughterManager(map);
					}
					map2 = map;
					if (map2.treeDestructionTracker == null)
					{
						map2.treeDestructionTracker = new TreeDestructionTracker(map);
					}
					map2 = map;
					if (map2.gasGrid == null)
					{
						map2.gasGrid = new GasGrid(map);
					}
					if (ModsConfig.BiotechActive && map.pollutionGrid == null)
					{
						map.pollutionGrid = new PollutionGrid(map);
					}
					map2 = map;
					if (map2.deferredSpawner == null)
					{
						map2.deferredSpawner = new DeferredSpawner(map);
					}
					map2 = map;
					if (map2.storageGroups == null)
					{
						map2.storageGroups = new StorageGroupManager(map);
					}
					TerrainGrid terrainGrid = map.terrainGrid;
					if (terrainGrid.colorGrid == null)
					{
						terrainGrid.colorGrid = new ColorDef[map.cellIndices.NumGridCells];
					}
					map2 = map;
					if (map2.tempTerrain == null)
					{
						map2.tempTerrain = new TempTerrainManager(map);
					}
					if (ModsConfig.OdysseyActive)
					{
						map2 = map;
						if (map2.substructureGrid == null)
						{
							map2.substructureGrid = new SubstructureGrid(map);
						}
						map2 = map;
						if (map2.waterBodyTracker == null)
						{
							map2.waterBodyTracker = new WaterBodyTracker(map);
						}
						map2 = map;
						if (map2.freezeManager == null)
						{
							map2.freezeManager = new FreezeManager(map);
						}
					}
				}
				else if (obj is Game game)
				{
					Game game2 = game;
					if (game2.transportShipManager == null)
					{
						game2.transportShipManager = new TransportShipManager();
					}
					game2 = game;
					if (game2.studyManager == null)
					{
						game2.studyManager = new StudyManager();
					}
					game2 = game;
					if (game2.hiddenItemsManager == null)
					{
						game2.hiddenItemsManager = new HiddenItemsManager();
					}
					if (ModsConfig.BiotechActive && game.customXenogermDatabase == null)
					{
						game.customXenogermDatabase = new CustomXenogermDatabase();
					}
					if (ModsConfig.BiotechActive && game.customXenotypeDatabase == null)
					{
						game.customXenotypeDatabase = new CustomXenotypeDatabase();
					}
				}
			}
			else
			{
				if (Scribe.mode != LoadSaveMode.PostLoadInit)
				{
					return;
				}
				Map map3 = obj as Map;
				if (map3 == null || ScribeMetaHeaderUtility.loadedGameVersionBuild >= 4208)
				{
					return;
				}
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					if (map3.listerThings.AnyThingWithDef(ThingDefOf.GravEngine))
					{
						map3.wasSpawnedViaGravShipLanding = true;
					}
				});
			}
			return;
			IL_019b:
			Ability ability = pawn.abilities?.abilities.FirstOrFallback((Ability x) => x.def != null && x.def.defName == "AnimaTreeLinking");
			if (ability != null)
			{
				pawn.abilities.RemoveAbility(ability.def);
			}
			if (pawn.RaceProps.Humanlike)
			{
				if (pawn.surroundings == null)
				{
					pawn.surroundings = new Pawn_SurroundingsTracker(pawn);
				}
				if (ModsConfig.IdeologyActive)
				{
					if (pawn.styleObserver == null)
					{
						pawn.styleObserver = new Pawn_StyleObserverTracker(pawn);
					}
					if (pawn.connections == null)
					{
						pawn.connections = new Pawn_ConnectionsTracker(pawn);
					}
				}
			}
			if (pawn.health != null)
			{
				if (pawn.health.hediffSet.hediffs.RemoveAll((Hediff x) => x == null) != 0)
				{
					Log.Error(pawn.ToStringSafe() + " had some null hediffs.");
				}
				Hediff hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.PsychicHangover);
				if (hediff != null)
				{
					pawn.health.hediffSet.hediffs.Remove(hediff);
				}
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.WakeUpTolerance);
				if (firstHediffOfDef != null)
				{
					pawn.health.hediffSet.hediffs.Remove(firstHediffOfDef);
				}
				Hediff firstHediffOfDef2 = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.GoJuiceTolerance);
				if (firstHediffOfDef2 != null)
				{
					pawn.health.hediffSet.hediffs.Remove(firstHediffOfDef2);
				}
				if (pawn.mechanitor != null)
				{
					Hediff firstHediffOfDef3 = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BandNode);
					if (firstHediffOfDef3 != null && !(firstHediffOfDef3 is Hediff_BandNode))
					{
						pawn.health.RemoveHediff(firstHediffOfDef3);
						pawn.health.AddHediff(HediffDefOf.BandNode, pawn.health.hediffSet.GetBrain());
					}
				}
				if (!pawn.Dead)
				{
					if (pawn.thinker == null)
					{
						pawn.thinker = new Pawn_Thinker(pawn);
					}
					if (pawn.jobs == null)
					{
						pawn.jobs = new Pawn_JobTracker(pawn);
					}
					if (pawn.stances == null)
					{
						pawn.stances = new Pawn_StanceTracker(pawn);
					}
					if (ModsConfig.AnomalyActive && pawn.RaceProps.Humanlike && pawn.infectionVectors == null)
					{
						pawn.infectionVectors = new Pawn_InfectionVectorTracker(pawn);
					}
				}
			}
			if (pawn.equipment != null && pawn.apparel != null && pawn.inventory != null)
			{
				List<ThingWithComps> list = null;
				for (int num2 = 0; num2 < pawn.equipment.AllEquipmentListForReading.Count; num2++)
				{
					ThingWithComps thingWithComps = pawn.equipment.AllEquipmentListForReading[num2];
					if (thingWithComps.def.defName == "OrbitalTargeterBombardment" || thingWithComps.def.defName == "OrbitalTargeterPowerBeam" || thingWithComps.def.defName == "OrbitalTargeterMechCluster" || thingWithComps.def.defName == "TornadoGenerator")
					{
						list = list ?? new List<ThingWithComps>();
						list.Add(thingWithComps);
					}
				}
				if (list != null)
				{
					foreach (Apparel item2 in list)
					{
						pawn.equipment.Remove(item2);
						ResetVerbs(item2);
						if (pawn.apparel.CanWearWithoutDroppingAnything(item2.def))
						{
							pawn.apparel.Wear(item2);
						}
						else
						{
							pawn.inventory.innerContainer.TryAdd(item2);
						}
					}
				}
			}
			if (ModsConfig.BiotechActive && pawn.RaceProps.IsMechanoid && pawn.Faction == Faction.OfPlayer && pawn.Name == null && Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawn.GenerateNecessaryName();
			}
			if (!ModsConfig.BiotechActive || pawn.equipment == null || pawn.def != ThingDefOf.Mech_Constructoid)
			{
				return;
			}
			List<ThingWithComps> allEquipmentListForReading = pawn.equipment.AllEquipmentListForReading;
			for (int num3 = 0; num3 < allEquipmentListForReading.Count; num3++)
			{
				if (allEquipmentListForReading[num3].def.defName == "Gun_Slugthrower")
				{
					pawn.equipment.Remove(allEquipmentListForReading[num3]);
					break;
				}
			}
		}

		private void ResetVerbs(ThingWithComps t)
		{
			(t as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
			foreach (ThingComp allComp in t.AllComps)
			{
				(allComp as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
			}
		}

		public override int GetBackCompatibleBodyPartIndex(BodyDef body, int index)
		{
			if (body == BodyDefOf.Human && ScribeMetaHeaderUtility.loadedGameVersionBuild <= 3094 && index >= 22)
			{
				return index + 1;
			}
			return index;
		}

		private void MealRestrictionsReworkBackCompat(object obj)
		{
			if (obj is FoodRestrictionDatabase foodRestrictionDatabase)
			{
				foodRestrictionDatabase.CreateIdeologyFoodRestrictions();
			}
		}

		private void BiosculpterReworkBackCompat(object obj)
		{
			if (obj is JobDriver_CarryToBiosculpterPod jobDriver_CarryToBiosculpterPod)
			{
				jobDriver_CarryToBiosculpterPod.EndJobWith(JobCondition.Incompletable);
			}
			if (obj is JobDriver_EnterBiosculpterPod jobDriver_EnterBiosculpterPod)
			{
				jobDriver_EnterBiosculpterPod.EndJobWith(JobCondition.Incompletable);
			}
			if (!(obj is Building thing))
			{
				return;
			}
			CompBiosculpterPod compBiosculpterPod = thing.TryGetComp<CompBiosculpterPod>();
			if (compBiosculpterPod != null)
			{
				if (compBiosculpterPod.Occupant == null)
				{
					compBiosculpterPod.ClearCycle();
				}
				compBiosculpterPod.autoLoadNutrition = true;
			}
		}

		private void SaveLampColor(object obj)
		{
			if (obj is Building key)
			{
				ColorInt? colorInt = null;
				switch (Scribe.loader?.curXmlParent?["def"]?.InnerText)
				{
				case "StandingLamp_Red":
					colorInt = new ColorInt(217, 80, 80);
					break;
				case "StandingLamp_Green":
					colorInt = new ColorInt(80, 217, 80);
					break;
				case "StandingLamp_Blue":
					colorInt = new ColorInt(80, 80, 217);
					break;
				case "Darklamp":
					colorInt = new ColorInt(78, 226, 229);
					break;
				}
				if (colorInt.HasValue)
				{
					lampsToColors[key] = colorInt.Value;
				}
			}
		}

		private void ApplyLampColor(object obj)
		{
			if (obj is Building building)
			{
				CompGlower comp = building.GetComp<CompGlower>();
				if (comp != null && lampsToColors.TryGetValue(building, out var value))
				{
					Color.RGBToHSV(value.ToColor, out var H, out var S, out var V);
					Color.RGBToHSV(comp.GlowColor.ToColor, out V, out var _, out var V2);
					comp.GlowColor = new ColorInt(Color.HSVToRGB(H, S, V2));
				}
			}
		}

		public override void PreLoadSavegame(string loadingVersion)
		{
			lampsToColors.Clear();
		}
	}
}
