using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using Verse.Profile;
using Verse.Sound;

namespace Verse
{
	public static class DebugActionsMisc
	{
		private static PlanetLayer TestLayer;

		private static readonly int[] TimeIncreases = new int[5] { 2500, 15000, 30000, 60000, 900000 };

		private static List<Pawn> tmpLentColonists = new List<Pawn>();

		private const string NoErrorString = "OK";

		private const string RoyalApparelTag = "Royal";

		private const int PawnsToGenerate = 100;

		[DebugAction("Other", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnWorld, hideInSubMenu = true, requiresOdyssey = true)]
		private static void AddTestPlanetLayer()
		{
			if (TestLayer != null)
			{
				Find.WorldGrid.RemovePlanetLayer(TestLayer);
			}
			PlanetLayer orbit = Find.WorldGrid.Orbit;
			TestLayer = Find.WorldGrid.RegisterPlanetLayer(PlanetLayerDefOf.Orbit, null, 200f);
			TestLayer.AddConnection(orbit, 0f);
			orbit.AddConnection(TestLayer, 0f);
		}

		[DebugAction("Other", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnWorld, hideInSubMenu = true, requiresOdyssey = true)]
		private static void RemoveTestPlanetLayer()
		{
			if (TestLayer != null)
			{
				Find.WorldGrid.RemovePlanetLayer(TestLayer);
			}
		}

		[DebugAction("Other", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnWorld, hideInSubMenu = true)]
		private static void LogPlanetLayerConnections()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<int, PlanetLayer> planetLayer3 in Find.WorldGrid.PlanetLayers)
			{
				planetLayer3.Deconstruct(out var _, out var value);
				PlanetLayer planetLayer = value;
				stringBuilder.AppendLine($"{planetLayer.LayerID}: {planetLayer.Def.label}:");
				foreach (KeyValuePair<PlanetLayer, PlanetLayerConnection> connection in planetLayer.Connections)
				{
					connection.Deconstruct(out value, out var value2);
					PlanetLayer planetLayer2 = value;
					PlanetLayerConnection planetLayerConnection = value2;
					stringBuilder.AppendLine($"   -> {planetLayer2.Def.label}, cost: {planetLayerConnection.fuelCost}");
				}
				stringBuilder.AppendLine();
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DestroyAllPlants()
		{
			foreach (Thing item in Find.CurrentMap.listerThings.AllThings.ToList())
			{
				if (item is Plant)
				{
					item.Destroy();
				}
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DestroyAllThings()
		{
			foreach (Thing item in Find.CurrentMap.listerThings.AllThings.ToList())
			{
				item.Destroy();
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DestroyClutter()
		{
			foreach (Thing item in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Chunk).ToList())
			{
				item.Destroy();
			}
			foreach (Thing item2 in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Filth).ToList())
			{
				item2.Destroy();
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DestroyAllHats()
		{
			foreach (Pawn allMap in PawnsFinder.AllMaps)
			{
				if (!allMap.RaceProps.Humanlike)
				{
					continue;
				}
				for (int num = allMap.apparel.WornApparel.Count - 1; num >= 0; num--)
				{
					Apparel apparel = allMap.apparel.WornApparel[num];
					if (apparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || apparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead))
					{
						apparel.Destroy();
					}
				}
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DestroyAllCorpses()
		{
			List<Thing> list = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].Destroy();
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FinishAllResearch()
		{
			Find.ResearchManager.DebugSetAllProjectsFinished();
			Find.EntityCodex.debug_UnhideAllResearch = true;
			Messages.Message("All research finished.", MessageTypeDefOf.TaskCompletion, historical: false);
		}

		[DebugAction("General", "Add techprint to project", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AddTechprintsForProject()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ResearchProjectDef item in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef p) => !p.TechprintRequirementMet))
			{
				ResearchProjectDef localProject = item;
				list.Add(new DebugMenuOption(localProject.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					Find.ResearchManager.AddTechprints(localProject, localProject.TechprintCount - Find.ResearchManager.GetTechprints(localProject));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "Apply techprint on project", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ApplyTechprintsForProject()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ResearchProjectDef item in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef p) => !p.TechprintRequirementMet))
			{
				ResearchProjectDef localProject = item;
				list.Add(new DebugMenuOption(localProject.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>
					{
						new DebugMenuOption("None", DebugMenuOptionMode.Action, delegate
						{
							Find.ResearchManager.ApplyTechprint(localProject, null);
						})
					};
					foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
					{
						Pawn localColonist = allMapsCaravansAndTravellingTransporters_Alive_Colonist;
						list2.Add(new DebugMenuOption(localColonist.LabelCap, DebugMenuOptionMode.Action, delegate
						{
							Find.ResearchManager.ApplyTechprint(localProject, localColonist);
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> AddTradeShipOfKind()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (TraderKindDef traderKind in DefDatabase<TraderKindDef>.AllDefs.Where((TraderKindDef t) => t.orbital))
			{
				list.Add(new DebugActionNode(traderKind.label, DebugActionType.Action, delegate
				{
					Find.CurrentMap.passingShipManager.DebugSendAllShipsAway();
					IncidentParms parms = new IncidentParms
					{
						target = Find.CurrentMap,
						traderKind = traderKind
					};
					IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(parms);
				}));
			}
			return list;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ReplaceAllTradeShips()
		{
			Find.CurrentMap.passingShipManager.DebugSendAllShipsAway();
			for (int i = 0; i < 5; i++)
			{
				IncidentParms incidentParms = new IncidentParms();
				incidentParms.target = Find.CurrentMap;
				IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(incidentParms);
			}
		}

		[DebugAction("General", "Change weather...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> ChangeWeather()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (WeatherDef allDef in DefDatabase<WeatherDef>.AllDefs)
			{
				WeatherDef localWeather = allDef;
				list.Add(new DebugActionNode(localWeather.LabelCap, DebugActionType.Action, delegate
				{
					Find.CurrentMap.weatherManager.TransitionTo(localWeather);
				}));
			}
			return list;
		}

		[DebugAction("General", "Celestial debugger", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void OpenCelestialDebugger()
		{
			if (Find.WindowStack.TryGetWindow<Dialog_DevCelestial>(out var window))
			{
				window.Close();
			}
			Find.WindowStack.Add(new Dialog_DevCelestial());
		}

		[DebugAction("Sound", "Music debugger", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void OpenMusicDebugger()
		{
			if (Find.WindowStack.TryGetWindow<Dialog_DevMusic>(out var window))
			{
				window.Close();
			}
			Find.WindowStack.Add(new Dialog_DevMusic());
		}

		[DebugAction("General", "World noise visualizer", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.WorldRenderedNow)]
		private static void OpenWorldNoiseDebugger()
		{
			if (Find.WindowStack.TryGetWindow<Dialog_DevNoiseWorld>(out var window))
			{
				window.Close();
			}
			Find.WindowStack.Add(new Dialog_DevNoiseWorld());
		}

		[DebugAction("General", "Map noise visualizer", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap)]
		private static void OpenMapNoiseDebugger()
		{
			if (Find.WindowStack.TryGetWindow<Dialog_DevNoiseMap>(out var window))
			{
				window.Close();
			}
			Find.WindowStack.Add(new Dialog_DevNoiseMap());
		}

		[DebugAction("Sound", "Test music fadeout and silence", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TestFadeoutAndSilence()
		{
			Find.MusicManagerPlay.ForceFadeoutAndSilenceFor(120f, 5f, preventDangerTransition: true);
		}

		[DebugAction("Sound", "Play song...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> PlaySong()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (SongDef allDef in DefDatabase<SongDef>.AllDefs)
			{
				SongDef localSong = allDef;
				list.Add(new DebugActionNode(localSong.defName, DebugActionType.Action, delegate
				{
					Find.MusicManagerPlay.ForcePlaySong(localSong, ignorePrefsVolume: false);
				}));
			}
			return list;
		}

		[DebugAction("Sound", "Trigger transition...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> TriggerTransition()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (MusicTransitionDef allDef in DefDatabase<MusicTransitionDef>.AllDefs)
			{
				MusicTransitionDef local = allDef;
				list.Add(new DebugActionNode(local.defName, DebugActionType.Action, delegate
				{
					Find.MusicManagerPlay.ForceTriggerTransition(local);
				}));
			}
			return list;
		}

		[DebugAction("Sound", "Play sound...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> PlaySound()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (SoundDef item in DefDatabase<SoundDef>.AllDefs.Where((SoundDef s) => !s.sustain))
			{
				SoundDef localSd = item;
				list.Add(new DebugActionNode(localSd.defName, DebugActionType.Action, delegate
				{
					if (localSd.subSounds.Any((SubSoundDef sub) => sub.onCamera))
					{
						localSd.PlayOneShotOnCamera();
					}
					else
					{
						localSd.PlayOneShot(SoundInfo.InMap(new TargetInfo(Find.CameraDriver.MapPosition, Find.CurrentMap)));
					}
				}));
			}
			return list;
		}

		[DebugAction("General", "End game condition...", false, false, false, false, false, 0, false, allowedGameStates = (AllowedGameStates.PlayingOnMap | AllowedGameStates.HasGameCondition))]
		private static void EndGameCondition()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (GameCondition activeCondition in Find.CurrentMap.gameConditionManager.ActiveConditions)
			{
				GameCondition localMc = activeCondition;
				list.Add(new DebugMenuOption(localMc.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					localMc.End();
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true)]
		private static List<DebugActionNode> SimulateSanguophageMeeting()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			for (int i = 1; i <= 8; i++)
			{
				int num = i;
				list.Add(new DebugActionNode(num + " sanguophages", DebugActionType.ToolMap, delegate
				{
					List<FactionRelation> list2 = new List<FactionRelation>();
					foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
					{
						if (!item.def.PermanentlyHostileTo(FactionDefOf.Sanguophages))
						{
							list2.Add(new FactionRelation(item, FactionRelationKind.Neutral));
						}
					}
					Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(FactionDefOf.Sanguophages, list2, hidden: true);
					faction.temporary = true;
					Find.FactionManager.Add(faction);
					List<Pawn> list3 = new List<Pawn>();
					PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Sanguophage, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true);
					for (int j = 0; j < num; j++)
					{
						list3.Add(PawnGenerator.GeneratePawn(request));
					}
					IncidentParms parms = new IncidentParms
					{
						target = Find.CurrentMap
					};
					PawnsArrivalModeDef edgeWalkIn = PawnsArrivalModeDefOf.EdgeWalkIn;
					edgeWalkIn.Worker.TryResolveRaidSpawnCenter(parms);
					edgeWalkIn.Worker.Arrive(list3, parms);
					LordMaker.MakeNewLord(faction, new LordJob_SanguophageMeeting(UI.MouseCell(), new List<Thing>(), 60000, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty), Find.CurrentMap, list3);
				}));
			}
			return list;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceEnemyAssault()
		{
			foreach (Lord lord in Find.CurrentMap.lordManager.lords)
			{
				if (!(lord.CurLordToil is LordToil_Stage item))
				{
					continue;
				}
				foreach (Transition transition in lord.Graph.transitions)
				{
					if (transition.sources.Contains(item) && (transition.target is LordToil_AssaultColony || transition.target is LordToil_AssaultColonyBreaching || transition.target is LordToil_AssaultColonyPrisoners || transition.target is LordToil_AssaultColonySappers || transition.target is LordToil_AssaultColonyBossgroup || transition.target is LordToil_MoveInBossgroup))
					{
						Messages.Message("Debug forcing to assault toil: " + lord.faction, MessageTypeDefOf.TaskCompletion, historical: false);
						lord.GotoToil(transition.target);
						return;
					}
				}
			}
			foreach (Quest item2 in Find.QuestManager.QuestsListForReading)
			{
				if (item2.State != QuestState.Ongoing)
				{
					continue;
				}
				foreach (QuestPart item3 in item2.PartsListForReading)
				{
					if (item3 is QuestPart_BossgroupArrives { State: QuestPartState.Enabled } questPart_BossgroupArrives)
					{
						questPart_BossgroupArrives.DebugForceComplete();
						Messages.Message("Debug forcing bossgroup assault.", MessageTypeDefOf.TaskCompletion, historical: false);
					}
				}
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceEnemyFlee()
		{
			foreach (Lord lord in Find.CurrentMap.lordManager.lords)
			{
				if (lord.faction != null && lord.faction.HostileTo(Faction.OfPlayer) && lord.faction.def.autoFlee)
				{
					LordToil lordToil = lord.Graph.lordToils.FirstOrDefault((LordToil st) => st is LordToil_PanicFlee);
					if (lordToil != null)
					{
						lord.GotoToil(lordToil);
					}
				}
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AdaptionProgress10Days()
		{
			Find.StoryWatcher.watcherAdaptation.Debug_OffsetAdaptDays(10f);
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void UnloadUnusedAssets()
		{
			MemoryUtility.UnloadUnusedUnityAssets();
		}

		[DebugAction("General", "Name settlement...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void NameSettlement()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("Faction", DebugMenuOptionMode.Action, delegate
			{
				Find.WindowStack.Add(new Dialog_NamePlayerFaction());
			}));
			if (Find.CurrentMap != null && Find.CurrentMap.IsPlayerHome && Find.CurrentMap.Parent is Settlement)
			{
				Settlement factionBase = (Settlement)Find.CurrentMap.Parent;
				list.Add(new DebugMenuOption("Faction base", DebugMenuOptionMode.Action, delegate
				{
					Find.WindowStack.Add(new Dialog_NamePlayerSettlement(factionBase));
				}));
				list.Add(new DebugMenuOption("Faction and faction base", DebugMenuOptionMode.Action, delegate
				{
					Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(factionBase));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void NextLesson()
		{
			LessonAutoActivator.DebugForceInitiateBestLessonNow();
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 900)]
		private static List<DebugActionNode> ChangeCameraConfig()
		{
			List<DebugActionNode> list = new List<DebugActionNode>
			{
				new DebugActionNode("Open editor", DebugActionType.Action, delegate
				{
					Find.WindowStack.Add(new Dialog_CameraConfig());
				})
			};
			foreach (Type item in typeof(CameraMapConfig).AllSubclasses())
			{
				Type localType = item;
				string text = localType.Name;
				if (text.StartsWith("CameraMapConfig_"))
				{
					text = text.Substring("CameraMapConfig_".Length);
				}
				list.Add(new DebugActionNode(text, DebugActionType.Action, delegate
				{
					Find.CameraDriver.config = (CameraMapConfig)Activator.CreateInstance(localType);
				}));
			}
			return list;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void ForceShipCountdown()
		{
			ShipCountdown.InitiateCountdown((Building)null);
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void ForceStartShip()
		{
			Map currentMap = Find.CurrentMap;
			if (currentMap != null)
			{
				Building_ShipComputerCore obj = (Building_ShipComputerCore)currentMap.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.Ship_ComputerCore).FirstOrDefault();
				if (obj == null)
				{
					Messages.Message("Could not find any compute core on current map!", MessageTypeDefOf.NeutralEvent);
				}
				obj.ForceLaunch();
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void FlashTradeDropSpot()
		{
			IntVec3 intVec = DropCellFinder.TradeDropSpot(Find.CurrentMap);
			Find.CurrentMap.debugDrawer.FlashCell(intVec);
			IntVec3 intVec2 = intVec;
			Log.Message("trade drop spot: " + intVec2.ToString());
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MakeFactionLeader(Pawn p)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction faction in Find.FactionManager.AllFactionsVisible)
			{
				list.Add(new DebugMenuOption(faction.Name, DebugMenuOptionMode.Action, delegate
				{
					if (faction.leader != p)
					{
						faction.leader = p;
						if (ModsConfig.IdeologyActive)
						{
							foreach (Precept item in faction.ideos.PrimaryIdeo.PreceptsListForReading)
							{
								if (item is Precept_Role precept_Role && precept_Role.def.leaderRole)
								{
									precept_Role.Assign(p, addThoughts: false);
									break;
								}
							}
						}
						DebugActionsUtility.DustPuffFrom(p);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void KillFactionLeader()
		{
			Pawn leader = Find.FactionManager.AllFactions.Where((Faction x) => x.leader != null).RandomElement().leader;
			int num = 0;
			while (!leader.Dead)
			{
				if (++num > 1000)
				{
					Log.Warning("Could not kill faction leader.");
					break;
				}
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Bullet, 30f, 999f);
				dinfo.SetIgnoreInstantKillProtection(ignore: true);
				leader.TakeDamage(dinfo);
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void KillKidnappedPawn()
		{
			IEnumerable<Pawn> pawnsBySituation = Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.Kidnapped);
			if (pawnsBySituation.Any())
			{
				Pawn pawn = pawnsBySituation.RandomElement();
				pawn.Kill(null, null);
				Messages.Message("Killed " + pawn.LabelCap, MessageTypeDefOf.NeutralEvent, historical: false);
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void KillWorldPawn()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Pawn item in Find.WorldPawns.AllPawnsAlive)
			{
				Pawn pLocal = item;
				list.Add(new DebugMenuOption(item.LabelShort + "(" + item.kindDef.label + ")", DebugMenuOptionMode.Action, delegate
				{
					pLocal.Kill(null, null);
					Messages.Message("Killed " + pLocal.LabelCap, MessageTypeDefOf.NeutralEvent, historical: false);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SetFactionRelations()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (Faction item2 in Find.FactionManager.AllFactionsVisibleInViewOrder)
			{
				Faction localFac = item2;
				foreach (FactionRelationKind value in Enum.GetValues(typeof(FactionRelationKind)))
				{
					FactionRelationKind localRk = value;
					FloatMenuOption item = new FloatMenuOption(localFac?.ToString() + " - " + localRk, delegate
					{
						if (localRk == FactionRelationKind.Hostile)
						{
							Faction.OfPlayer.TryAffectGoodwillWith(localFac, -100, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.DebugGoodwill);
						}
						else if (localRk == FactionRelationKind.Ally)
						{
							Faction.OfPlayer.TryAffectGoodwillWith(localFac, 100, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.DebugGoodwill);
						}
					});
					list.Add(item);
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void VisitorGift()
		{
			List<Pawn> list = new List<Pawn>();
			foreach (Pawn item in Find.CurrentMap.mapPawns.AllPawnsSpawned)
			{
				if (item.Faction != null && !item.Faction.IsPlayer && !item.Faction.HostileTo(Faction.OfPlayer))
				{
					list.Add(item);
					break;
				}
			}
			VisitorGiftForPlayerUtility.GiveRandomGift(list, list[0].Faction);
		}

		[DebugAction("General", "Increment time", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 500)]
		private static List<DebugActionNode> IncrementTime()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			for (int i = 0; i < TimeIncreases.Length; i++)
			{
				int durationLocal = TimeIncreases[i];
				list.Add(new DebugActionNode(durationLocal.ToStringTicksToPeriod(), DebugActionType.Action, delegate
				{
					Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + durationLocal);
				}));
			}
			return list;
		}

		[DebugAction("General", "Storywatcher tick 1 day", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void StorywatcherTick1Day()
		{
			for (int i = 0; i < 60000; i++)
			{
				Find.StoryWatcher.StoryWatcherTick();
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1);
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void KillRandomLentColonist()
		{
			if (QuestUtility.TotalBorrowedColonistCount() <= 0)
			{
				return;
			}
			tmpLentColonists.Clear();
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if (questsListForReading[i].State != QuestState.Ongoing)
				{
					continue;
				}
				List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
				for (int j = 0; j < partsListForReading.Count; j++)
				{
					if (!(partsListForReading[j] is QuestPart_LendColonistsToFaction { LentColonistsListForReading: var lentColonistsListForReading }))
					{
						continue;
					}
					for (int k = 0; k < lentColonistsListForReading.Count; k++)
					{
						if (lentColonistsListForReading[k] is Pawn { Dead: false } pawn)
						{
							tmpLentColonists.Add(pawn);
						}
					}
				}
			}
			Pawn pawn2 = tmpLentColonists.RandomElement();
			bool flag = pawn2.health.hediffSet.hediffs.Any((Hediff x) => x.def.isBad);
			pawn2.Kill(null, flag ? pawn2.health.hediffSet.hediffs.RandomElement() : null);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void ClearPrisonerInteractionSchedule(Pawn p)
		{
			if (p.IsPrisonerOfColony)
			{
				p.mindState.lastAssignedInteractTime = -1;
				p.mindState.interactionsToday = 0;
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -100)]
		private static void GlowAtPosition()
		{
			Map currentMap = Find.CurrentMap;
			foreach (IntVec3 item in GenRadial.RadialCellsAround(UI.MouseCell(), 10f, useCenter: true))
			{
				if (item.InBounds(currentMap))
				{
					float num = Find.CurrentMap.glowGrid.GroundGlowAt(item);
					currentMap.debugDrawer.FlashCell(item, 0f, num.ToString("F1"), 100);
				}
			}
		}

		[DebugAction("General", "HSV At Position", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -100)]
		private static void HSVAtPosition()
		{
			Map currentMap = Find.CurrentMap;
			foreach (IntVec3 item in GenRadial.RadialCellsAround(UI.MouseCell(), 10f, useCenter: true))
			{
				if (item.InBounds(currentMap))
				{
					Color.RGBToHSV(Find.CurrentMap.glowGrid.VisualGlowAt(item), out var H, out var S, out var V);
					currentMap.debugDrawer.FlashCell(item, 0.5f, $"HSV({H:.0#},{S:.0#},{V:.0#})", 100);
				}
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void FlashBlockedLandingCells()
		{
			Map currentMap = Find.CurrentMap;
			foreach (IntVec3 allCell in currentMap.AllCells)
			{
				if (!allCell.Fogged(currentMap) && !DropCellFinder.IsGoodDropSpot(allCell, currentMap, allowFogged: false, canRoofPunch: false, allowIndoors: false))
				{
					currentMap.debugDrawer.FlashCell(allCell, 0f, "bl");
				}
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static List<DebugActionNode> PawnKindApparelCheck()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (PawnKindDef item in from kd in DefDatabase<PawnKindDef>.AllDefs
				where kd.race == ThingDefOf.Human
				orderby kd.defName
				select kd)
			{
				PawnKindDef localKindDef = item;
				list.Add(new DebugActionNode(localKindDef.defName, DebugActionType.Action, delegate
				{
					Faction faction = FactionUtility.DefaultFactionFrom(localKindDef.defaultFactionDef);
					bool flag = false;
					for (int i = 0; i < 100; i++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(localKindDef, faction);
						if (pawn.royalty != null)
						{
							RoyalTitle mostSeniorTitle = pawn.royalty.MostSeniorTitle;
							if (mostSeniorTitle != null && !mostSeniorTitle.def.requiredApparel.NullOrEmpty())
							{
								for (int j = 0; j < mostSeniorTitle.def.requiredApparel.Count; j++)
								{
									ApparelRequirement apparelRequirement = mostSeniorTitle.def.requiredApparel[j];
									if (apparelRequirement.IsActive(pawn) && !apparelRequirement.IsMet(pawn))
									{
										Log.Error(localKindDef?.ToString() + " (" + mostSeniorTitle.def.label + ")  does not have its title requirements met. index=" + j + logApparel(pawn));
										flag = true;
									}
								}
							}
						}
						List<Apparel> wornApparel = pawn.apparel.WornApparel;
						for (int k = 0; k < wornApparel.Count; k++)
						{
							string text = apparelOkayToWear(pawn, wornApparel[k]);
							if (text != "OK")
							{
								Log.Error(text + " - " + wornApparel[k].Label + logApparel(pawn));
								flag = true;
							}
						}
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
					}
					if (!flag)
					{
						Log.Message("No errors for " + localKindDef.defName);
					}
				}));
			}
			return list;
			static string apparelOkayToWear(Pawn pawn, Apparel apparel)
			{
				ApparelProperties app = apparel.def.apparel;
				if (!pawn.kindDef.apparelRequired.NullOrEmpty() && pawn.kindDef.apparelRequired.Contains(apparel.def))
				{
					return "OK";
				}
				if (!app.PawnCanWear(pawn))
				{
					return "Pawn cannot wear.";
				}
				List<SpecificApparelRequirement> specificApparelRequirements = pawn.kindDef.specificApparelRequirements;
				if (specificApparelRequirements != null)
				{
					for (int i = 0; i < specificApparelRequirements.Count; i++)
					{
						if (PawnApparelGenerator.ApparelRequirementHandlesThing(specificApparelRequirements[i], apparel.def) && PawnApparelGenerator.ApparelRequirementTagsMatch(specificApparelRequirements[i], apparel.def))
						{
							return "OK";
						}
					}
				}
				if (!pawn.kindDef.apparelTags.NullOrEmpty())
				{
					if (!app.tags.Any((string tag) => pawn.kindDef.apparelTags.Contains(tag)))
					{
						return "Required tag missing.";
					}
					if ((pawn.royalty == null || pawn.royalty.MostSeniorTitle == null) && app.tags.Contains("Royal") && !pawn.kindDef.apparelTags.Any((string tag) => app.tags.Contains(tag)))
					{
						return "Royal apparel on non-royal pawn.";
					}
				}
				if (!pawn.kindDef.apparelDisallowTags.NullOrEmpty() && pawn.kindDef.apparelDisallowTags.Any((string t) => app.tags.Contains(t)))
				{
					return "Has a disallowed tag.";
				}
				if (pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Any())
				{
					RoyalTitle mostSeniorTitle = pawn.royalty.MostSeniorTitle;
					if (apparel.TryGetQuality(out var qc) && (int)qc < (int)mostSeniorTitle.def.requiredMinimumApparelQuality)
					{
						return "Quality too low.";
					}
				}
				return "OK";
			}
			static string logApparel(Pawn p)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Apparel of " + p.LabelShort + ":");
				List<Apparel> wornApparel = p.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					stringBuilder.AppendLine("  - " + wornApparel[i].Label);
				}
				return stringBuilder.ToString();
			}
		}

		[DebugAction("General", "Edit effecter...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> EditEffecter()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (EffecterDef allDef in DefDatabase<EffecterDef>.AllDefs)
			{
				EffecterDef localDef = allDef;
				list.Add(new DebugActionNode(localDef.defName, DebugActionType.Action, delegate
				{
					if (!Find.WindowStack.TryRemove(typeof(EditWindow_DefEditor)))
					{
						Find.WindowStack.Add(new EditWindow_DefEditor(localDef));
					}
				}));
			}
			return list;
		}

		[DebugAction("General", "Edit Animation...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> EditAnimation()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (AnimationDef allDef in DefDatabase<AnimationDef>.AllDefs)
			{
				AnimationDef localDef = allDef;
				list.Add(new DebugActionNode(localDef.defName, DebugActionType.Action, delegate
				{
					if (!Find.WindowStack.TryRemove(typeof(EditWindow_DefEditor)))
					{
						Find.WindowStack.Add(new EditWindow_DefEditor(localDef));
					}
				}));
			}
			return list;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static List<DebugActionNode> PawnKindAbilityCheck()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (PawnKindDef item in from kd in DefDatabase<PawnKindDef>.AllDefs
				where kd.titleRequired != null || !kd.titleSelectOne.NullOrEmpty()
				orderby kd.defName
				select kd)
			{
				PawnKindDef localKindDef = item;
				list.Add(new DebugActionNode(localKindDef.defName, DebugActionType.Action, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					Faction faction = FactionUtility.DefaultFactionFrom(localKindDef.defaultFactionDef);
					for (int i = 0; i < 100; i++)
					{
						RoyalTitleDef royalTitleDef = null;
						if (localKindDef.titleRequired != null)
						{
							royalTitleDef = localKindDef.titleRequired;
						}
						else if (!localKindDef.titleSelectOne.NullOrEmpty() && Rand.Chance(localKindDef.royalTitleChance))
						{
							royalTitleDef = localKindDef.titleSelectOne.RandomElementByWeight((RoyalTitleDef t) => t.commonality);
						}
						PawnKindDef kind = localKindDef;
						RoyalTitleDef fixedTitle = royalTitleDef;
						Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, fixedTitle));
						RoyalTitle mostSeniorTitle = pawn.royalty.MostSeniorTitle;
						if (mostSeniorTitle != null)
						{
							Hediff_Psylink mainPsylinkSource = pawn.GetMainPsylinkSource();
							if (mainPsylinkSource == null)
							{
								if (mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def) > 0)
								{
									string text = mostSeniorTitle.def.LabelCap + " - No psylink.";
									if (pawn.abilities.abilities.Any((Ability x) => x.def.IsPsycast && x.def.level > 0))
									{
										text += " Has psycasts without psylink.";
									}
									stringBuilder.AppendLine(text);
								}
							}
							else if (mainPsylinkSource.level < mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def))
							{
								stringBuilder.AppendLine("Psylink at level " + mainPsylinkSource.level + ", but requires " + mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def));
							}
							else if (mainPsylinkSource.level > mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def))
							{
								stringBuilder.AppendLine("Psylink at level " + mainPsylinkSource.level + ". Max is " + mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def));
							}
						}
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
					}
					if (stringBuilder.Length == 0)
					{
						Log.Message("No errors for " + localKindDef.defName);
					}
					else
					{
						Log.Error("Errors:\n" + stringBuilder);
					}
				}));
			}
			return list;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		public static void AtlasRebuild()
		{
			GlobalTextureAtlasManager.rebakeAtlas = true;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DumpPawnAtlases()
		{
			string text = Application.dataPath + "\\atlasDump_Pawn";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			GlobalTextureAtlasManager.DumpPawnAtlases(text);
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DumpStaticAtlases()
		{
			string text = Application.dataPath + "\\atlasDump_Static";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			GlobalTextureAtlasManager.DumpStaticAtlases(text);
		}

		[DebugAction("Anomaly", null, false, false, false, false, false, 0, false, name = "Set Anomaly level...", allowedGameStates = AllowedGameStates.Playing, requiresAnomaly = true)]
		private static List<DebugActionNode> SetMonolithLevel()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (MonolithLevelDef def in DefDatabase<MonolithLevelDef>.AllDefs)
			{
				list.Add(new DebugActionNode(def.defName, DebugActionType.Action, delegate
				{
					Find.Anomaly.SetLevel(def);
				}));
			}
			return list;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, name = "Hot reload Defs", allowedGameStates = AllowedGameStates.Playing, displayPriority = 9999)]
		private static void HotReloadDefs()
		{
			PlayDataLoader.HotReloadDefs();
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void SetGraphicsDirty(Pawn p)
		{
			p.Drawer.renderer.SetAllGraphicsDirty();
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void ToggleMovement(Pawn p)
		{
			p.pather.debugDisabled = !p.pather.debugDisabled;
			MoteMaker.ThrowText(p.Position.ToVector3(), Find.CurrentMap, "Movement " + (p.pather.debugDisabled ? "Disabled" : "Enabled"));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void ToggleMaxMoveSpeed(Pawn p)
		{
			p.debugMaxMoveSpeed = !p.debugMaxMoveSpeed;
			MoteMaker.ThrowText(p.Position.ToVector3(), Find.CurrentMap, "Max MoveSpeed " + (p.debugMaxMoveSpeed ? "Enabled" : "Disabled"));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static List<DebugActionNode> LockRotation()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (Rot4 allRotation in Rot4.AllRotations)
			{
				Rot4 lRot = allRotation;
				list.Add(new DebugActionNode(allRotation.ToStringHuman() ?? "", DebugActionType.ToolMapForPawns)
				{
					pawnAction = delegate(Pawn pawn)
					{
						if (pawn.debugRotLocked)
						{
							pawn.debugRotLocked = false;
						}
						pawn.Rotation = lRot;
						pawn.debugRotLocked = true;
						MoteMaker.ThrowText(pawn.Position.ToVector3(), Find.CurrentMap, "Rot Locked");
					}
				});
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void UnlockRotation(Pawn p)
		{
			if (p.debugRotLocked)
			{
				p.debugRotLocked = false;
				MoteMaker.ThrowText(p.Position.ToVector3(), Find.CurrentMap, "Rot Unlocked");
			}
		}

		[DebugAction("General", "Reveal Hidden Defs", false, false, false, false, false, 0, false)]
		private static void RevealHiddenDefs()
		{
			Find.HiddenItemsManager.ClearHiddenDefs();
		}

		[DebugAction("Anomaly", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, name = "End revenant hypnosis", requiresAnomaly = true)]
		private static void EndRevenantHypnosis(Pawn p)
		{
			Hediff hediff = p.health.hediffSet.hediffs.Find((Hediff hd) => hd.def == HediffDefOf.RevenantHypnosis);
			if (hediff != null)
			{
				p.health.RemoveHediff(hediff);
			}
			p.stances.stunner.StopStun();
			Find.Anomaly.EndHypnotize(p);
		}

		[DebugAction("General", "Layer pathfinder...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
		private static void LayerPathfinder()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (KeyValuePair<int, PlanetLayer> planetLayer3 in Find.WorldGrid.PlanetLayers)
			{
				planetLayer3.Deconstruct(out var _, out var value);
				PlanetLayer planetLayer = value;
				PlanetLayer origin = planetLayer;
				list.Add(new DebugMenuOption(origin.Def.label, DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (KeyValuePair<int, PlanetLayer> planetLayer4 in Find.WorldGrid.PlanetLayers)
					{
						planetLayer4.Deconstruct(out var _, out var value2);
						PlanetLayer planetLayer2 = value2;
						PlanetLayer dest = planetLayer2;
						if (dest != origin)
						{
							list2.Add(new DebugMenuOption(dest.Def.label, DebugMenuOptionMode.Action, delegate
							{
								List<PlanetLayerConnection> list3 = new List<PlanetLayerConnection>();
								string text;
								if (origin.TryGetPath(dest, list3, out var cost))
								{
									text = $"Found path between {origin.Def.label} and {dest.Def.label}, cost: {cost}";
									foreach (PlanetLayerConnection item in list3)
									{
										text += $"\n {item.origin.Def.label} ({item.fuelCost}) -> {item.target.Def.label}";
									}
								}
								else
								{
									text = "No path between " + origin.Def.label + " and " + dest.Def.label;
								}
								Log.Message(text);
							}));
						}
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2, "Select destination"));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list, "Select origin"));
		}

		[DebugAction("Other", "Clear cached materials", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Invalid, hideInSubMenu = true)]
		private static void ClearCachedMaterials()
		{
			MatLoader.ClearCache();
		}

		[DebugAction("Pathing", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void LogCellMapData()
		{
			Find.CurrentMap.pathFinder.MapData.LogCell(UI.MouseCell());
		}

		[DebugAction("Pathing", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap)]
		private static void LogPathfinderState()
		{
			Find.CurrentMap.pathFinder.LogPathfinderState();
		}

		[DebugAction("Pathing", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void LogCellGridResult()
		{
			Find.CurrentMap.pathFinder.LogGridCellResult(UI.MouseCell());
		}

		[DebugAction("Pathing", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Breach()
		{
			IntVec3 start;
			DebugTools.curTool = new DebugTool("start...", delegate
			{
				start = UI.MouseCell();
				DebugTools.curTool = new DebugTool("end...", delegate
				{
					IntVec3 end = UI.MouseCell();
					new BreachingGrid(Find.CurrentMap, null).CreateBreachPath(start, end, 1, 3, useAvoidGrid: true);
					DebugTools.curTool = null;
				});
			});
		}

		[DebugAction("Pathing", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Goto(Pawn pawn)
		{
			DebugTools.curTool = new DebugTool("Select destination...", delegate
			{
				IntVec3 intVec = UI.MouseCell();
				if (!pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly))
				{
					MoteMaker.ThrowText(intVec.ToVector3(), Find.CurrentMap, "Cannot reach cell");
				}
				else
				{
					Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
					job.locomotionUrgency = LocomotionUrgency.Jog;
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
					DebugTools.curTool = null;
				}
			});
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing, requiresOdyssey = true)]
		private static void RetroactivelyAddLandmarksToWorld()
		{
			new WorldGenStep_Landmarks().GenerateFresh(Find.World.info.seedString, Find.WorldGrid.Surface);
			Find.World.renderer.GetLayer<WorldDrawLayer_Terrain>(Find.WorldGrid.Surface).RegenerateNow();
			Find.World.renderer.GetLayer<WorldDrawLayer_Landmarks>(Find.WorldGrid.Surface).RegenerateNow();
			Find.World.renderer.GetLayer<WorldDrawLayer_Hills>(Find.WorldGrid.Surface).RegenerateNow();
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing, requiresOdyssey = true)]
		private static void RegenerateMapFeatures()
		{
			foreach (Tile tile in Find.WorldGrid.Surface.Tiles)
			{
				tile.mutatorsNullable?.Clear();
			}
			WorldGenStep_Mutators.AddMutatorsFromTile(Find.WorldGrid.Surface);
		}
	}
}
