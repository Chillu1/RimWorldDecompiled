using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;
using Verse.Profile;
using Verse.Sound;

namespace Verse
{
	public static class DebugActionsMisc
	{
		private static List<Pawn> tmpLentColonists = new List<Pawn>();

		private const string NoErrorString = "OK";

		private const string RoyalApparelTag = "Royal";

		private const int PawnsToGenerate = 100;

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DestroyAllThings()
		{
			foreach (Thing item in Find.CurrentMap.listerThings.AllThings.ToList())
			{
				item.Destroy();
			}
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FinishAllResearch()
		{
			Find.ResearchManager.DebugSetAllProjectsFinished();
			Messages.Message("All research finished.", MessageTypeDefOf.TaskCompletion, historical: false);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", "Change weather...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ChangeWeather()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (WeatherDef allDef in DefDatabase<WeatherDef>.AllDefs)
			{
				WeatherDef localWeather = allDef;
				list.Add(new DebugMenuOption(localWeather.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					Find.CurrentMap.weatherManager.TransitionTo(localWeather);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "Play song...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PlaySong()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (SongDef allDef in DefDatabase<SongDef>.AllDefs)
			{
				SongDef localSong = allDef;
				list.Add(new DebugMenuOption(localSong.defName, DebugMenuOptionMode.Action, delegate
				{
					Find.MusicManagerPlay.ForceStartSong(localSong, ignorePrefsVolume: false);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "Play sound...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PlaySound()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (SoundDef item in DefDatabase<SoundDef>.AllDefs.Where((SoundDef s) => !s.sustain))
			{
				SoundDef localSd = item;
				list.Add(new DebugMenuOption(localSd.defName, DebugMenuOptionMode.Action, delegate
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
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "End game condition...", allowedGameStates = (AllowedGameStates.PlayingOnMap | AllowedGameStates.HasGameCondition))]
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AddPrisoner()
		{
			AddGuest(prisoner: true);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AddGuest()
		{
			AddGuest(prisoner: false);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceEnemyAssault()
		{
			foreach (Lord lord in Find.CurrentMap.lordManager.lords)
			{
				LordToil_Stage lordToil_Stage = lord.CurLordToil as LordToil_Stage;
				if (lordToil_Stage == null)
				{
					continue;
				}
				foreach (Transition transition in lord.Graph.transitions)
				{
					if (transition.sources.Contains(lordToil_Stage) && transition.target is LordToil_AssaultColony)
					{
						Messages.Message("Debug forcing to assault toil: " + lord.faction, MessageTypeDefOf.TaskCompletion, historical: false);
						lord.GotoToil(transition.target);
						return;
					}
				}
			}
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AdaptionProgress10Days()
		{
			Find.StoryWatcher.watcherAdaptation.Debug_OffsetAdaptDays(10f);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void UnloadUnusedAssets()
		{
			MemoryUtility.UnloadUnusedUnityAssets();
		}

		[DebugAction("General", "Name settlement...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void NextLesson()
		{
			LessonAutoActivator.DebugForceInitiateBestLessonNow();
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RegenAllMapMeshSections()
		{
			Find.CurrentMap.mapDrawer.RegenerateEverythingNow();
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ChangeCameraConfig()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type item in typeof(CameraMapConfig).AllSubclasses())
			{
				Type localType = item;
				string text = localType.Name;
				if (text.StartsWith("CameraMapConfig_"))
				{
					text = text.Substring("CameraMapConfig_".Length);
				}
				list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
				{
					Find.CameraDriver.config = (CameraMapConfig)Activator.CreateInstance(localType);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceShipCountdown()
		{
			ShipCountdown.InitiateCountdown((Building)null);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FlashTradeDropSpot()
		{
			IntVec3 intVec = DropCellFinder.TradeDropSpot(Find.CurrentMap);
			Find.CurrentMap.debugDrawer.FlashCell(intVec);
			Log.Message("trade drop spot: " + intVec);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SetFactionRelations()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (Faction item2 in Find.FactionManager.AllFactionsVisibleInViewOrder)
			{
				Faction localFac = item2;
				foreach (FactionRelationKind value in Enum.GetValues(typeof(FactionRelationKind)))
				{
					FactionRelationKind localRk = value;
					FloatMenuOption item = new FloatMenuOption(string.Concat(localFac, " - ", localRk), delegate
					{
						localFac.TrySetRelationKind(Faction.OfPlayer, localRk);
					});
					list.Add(item);
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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
			VisitorGiftForPlayerUtility.GiveGift(list, list[0].Faction);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RefogMap()
		{
			FloodFillerFog.DebugRefogMap(Find.CurrentMap);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void UseGenStep()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type item in typeof(GenStep).AllSubclassesNonAbstract())
			{
				Type localGenStep = item;
				list.Add(new DebugMenuOption(localGenStep.Name, DebugMenuOptionMode.Action, delegate
				{
					((GenStep)Activator.CreateInstance(localGenStep)).Generate(Find.CurrentMap, default(GenStepParams));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void IncrementTime1Hour()
		{
			Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 2500);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void IncrementTime6Hours()
		{
			Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 15000);
		}

		[DebugAction("General", "Increment time 1 day", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void IncrementTime1Day()
		{
			Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 60000);
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void IncrementTime1Season()
		{
			Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 900000);
		}

		[DebugAction("General", "Storywatcher tick 1 day", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void StorywatcherTick1Day()
		{
			for (int i = 0; i < 60000; i++)
			{
				Find.StoryWatcher.StoryWatcherTick();
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1);
			}
		}

		[DebugAction("General", "Add techprint to project", allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", "Apply techprint on project", allowedGameStates = AllowedGameStates.PlayingOnMap)]
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
					Pawn localColonist = default(Pawn);
					foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
					{
						localColonist = allMapsCaravansAndTravelingTransportPods_Alive_Colonist;
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

		private static void AddGuest(bool prisoner)
		{
			foreach (Building_Bed item in Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>())
			{
				if (item.ForPrisoners != prisoner || (item.OwnersForReading.Any() && (!prisoner || !item.AnyUnownedSleepingSlot)))
				{
					continue;
				}
				PawnKindDef pawnKindDef = (prisoner ? DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef pk) => pk.defaultFactionType != null && !pk.defaultFactionType.isPlayer && pk.RaceProps.Humanlike).RandomElement() : PawnKindDefOf.SpaceRefugee);
				Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionType);
				Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
				GenSpawn.Spawn(pawn, item.Position, Find.CurrentMap);
				foreach (ThingWithComps item2 in pawn.equipment.AllEquipmentListForReading.ToList())
				{
					if (pawn.equipment.TryDropEquipment(item2, out var resultingEq, pawn.Position))
					{
						resultingEq.Destroy();
					}
				}
				pawn.inventory.innerContainer.Clear();
				pawn.ownership.ClaimBedIfNonMedical(item);
				pawn.guest.SetGuestStatus(Faction.OfPlayer, prisoner);
				break;
			}
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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
					QuestPart_LendColonistsToFaction questPart_LendColonistsToFaction;
					if ((questPart_LendColonistsToFaction = partsListForReading[j] as QuestPart_LendColonistsToFaction) == null)
					{
						continue;
					}
					List<Thing> lentColonistsListForReading = questPart_LendColonistsToFaction.LentColonistsListForReading;
					for (int k = 0; k < lentColonistsListForReading.Count; k++)
					{
						Pawn pawn;
						if ((pawn = lentColonistsListForReading[k] as Pawn) != null && !pawn.Dead)
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PawnKindApparelCheck()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (PawnKindDef item in from kd in DefDatabase<PawnKindDef>.AllDefs
				where kd.race == ThingDefOf.Human
				orderby kd.defName
				select kd)
			{
				PawnKindDef localKindDef = item;
				list.Add(new DebugMenuOption(localKindDef.defName, DebugMenuOptionMode.Action, delegate
				{
					Faction faction = FactionUtility.DefaultFactionFrom(localKindDef.defaultFactionType);
					bool flag = false;
					for (int k = 0; k < 100; k++)
					{
						Pawn pawn2 = PawnGenerator.GeneratePawn(localKindDef, faction);
						if (pawn2.royalty != null)
						{
							RoyalTitle mostSeniorTitle2 = pawn2.royalty.MostSeniorTitle;
							if (mostSeniorTitle2 != null && !mostSeniorTitle2.def.requiredApparel.NullOrEmpty())
							{
								for (int l = 0; l < mostSeniorTitle2.def.requiredApparel.Count; l++)
								{
									if (!mostSeniorTitle2.def.requiredApparel[l].IsMet(pawn2))
									{
										Log.Error(string.Concat(localKindDef, " (", mostSeniorTitle2.def.label, ")  does not have its title requirements met. index=", l, logApparel(pawn2)));
										flag = true;
									}
								}
							}
						}
						List<Apparel> wornApparel2 = pawn2.apparel.WornApparel;
						for (int m = 0; m < wornApparel2.Count; m++)
						{
							string text = apparelOkayToWear(pawn2, wornApparel2[m]);
							if (text != "OK")
							{
								Log.Error(text + " - " + wornApparel2[m].Label + logApparel(pawn2));
								flag = true;
							}
						}
						Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
					}
					if (!flag)
					{
						Log.Message("No errors for " + localKindDef.defName);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			static string apparelOkayToWear(Pawn pawn, Apparel apparel)
			{
				ApparelProperties app = apparel.def.apparel;
				if (!pawn.kindDef.apparelRequired.NullOrEmpty() && pawn.kindDef.apparelRequired.Contains(apparel.def))
				{
					return "OK";
				}
				if (!app.CorrectGenderForWearing(pawn.gender))
				{
					return "Wrong gender.";
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
				stringBuilder.AppendLine($"Apparel of {p.LabelShort}:");
				List<Apparel> wornApparel = p.apparel.WornApparel;
				for (int j = 0; j < wornApparel.Count; j++)
				{
					stringBuilder.AppendLine("  - " + wornApparel[j].Label);
				}
				return stringBuilder.ToString();
			}
		}

		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PawnKindAbilityCheck()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			StringBuilder sb = new StringBuilder();
			foreach (PawnKindDef item in from kd in DefDatabase<PawnKindDef>.AllDefs
				where kd.titleRequired != null || !kd.titleSelectOne.NullOrEmpty()
				orderby kd.defName
				select kd)
			{
				PawnKindDef localKindDef = item;
				list.Add(new DebugMenuOption(localKindDef.defName, DebugMenuOptionMode.Action, delegate
				{
					Faction faction = FactionUtility.DefaultFactionFrom(localKindDef.defaultFactionType);
					for (int i = 0; i < 100; i++)
					{
						RoyalTitleDef fixedTitle = null;
						if (localKindDef.titleRequired != null)
						{
							fixedTitle = localKindDef.titleRequired;
						}
						else if (!localKindDef.titleSelectOne.NullOrEmpty() && Rand.Chance(localKindDef.royalTitleChance))
						{
							fixedTitle = localKindDef.titleSelectOne.RandomElementByWeight((RoyalTitleDef t) => t.commonality);
						}
						Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(localKindDef, faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, fixedTitle));
						RoyalTitle mostSeniorTitle = pawn.royalty.MostSeniorTitle;
						if (mostSeniorTitle != null)
						{
							Hediff_Psylink mainPsylinkSource = pawn.GetMainPsylinkSource();
							if (mainPsylinkSource == null)
							{
								if (mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def) > 0)
								{
									string text = mostSeniorTitle.def.LabelCap + " - No psylink.";
									if (pawn.abilities.abilities.Any((Ability x) => x.def.level > 0))
									{
										text += " Has psycasts without psylink.";
									}
									sb.AppendLine(text);
								}
							}
							else if (mainPsylinkSource.level < mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def))
							{
								sb.AppendLine("Psylink at level " + mainPsylinkSource.level + ", but requires " + mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def));
							}
							else if (mainPsylinkSource.level > mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def))
							{
								sb.AppendLine("Psylink at level " + mainPsylinkSource.level + ". Max is " + mostSeniorTitle.def.MaxAllowedPsylinkLevel(faction.def));
							}
						}
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
					}
					if (sb.Length == 0)
					{
						Log.Message("No errors for " + localKindDef.defName);
					}
					else
					{
						Log.Error("Errors:\n" + sb.ToString());
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}
	}
}
