using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using UnityEngine.Profiling;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Verse.Steam;

namespace Verse
{
	public class EditWindow_DebugInspector : EditWindow
	{
		private StringBuilder debugStringBuilder = new StringBuilder();

		public bool fullMode;

		private float columnWidth = 360f;

		public override Vector2 InitialSize => new Vector2(400f, 600f);

		public override bool IsDebug => true;

		public EditWindow_DebugInspector()
		{
			optionalTitle = "Debug inspector";
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			if (Current.ProgramState == ProgramState.Playing)
			{
				GenUI.RenderMouseoverBracket();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			if (KeyBindingDefOf.Dev_ToggleDebugInspector.KeyDownEvent)
			{
				Event.current.Use();
				Close();
			}
			Text.Font = GameFont.Tiny;
			WidgetRow widgetRow = new WidgetRow(0f, 0f);
			widgetRow.ToggleableIcon(ref fullMode, TexButton.InspectModeToggle, "Toggle deep inspection mode for things on the map.");
			widgetRow.ToggleableIcon(ref DebugViewSettings.writeCellContents, TexButton.InspectModeToggle, "Toggle shallow inspection for things on the map.");
			if (widgetRow.ButtonText("Visibility", "Toggle what information should be reported by the inspector."))
			{
				Find.WindowStack.Add(new Dialog_DebugSettingsMenu());
			}
			if (widgetRow.ButtonText("Column Width +", "Make the columns wider."))
			{
				columnWidth += 20f;
				columnWidth = Mathf.Clamp(columnWidth, 200f, 1600f);
			}
			if (widgetRow.ButtonText("Column Width -", "Make the columns narrower."))
			{
				columnWidth -= 20f;
				columnWidth = Mathf.Clamp(columnWidth, 200f, 1600f);
			}
			inRect.yMin += 30f;
			Listing_Standard listing_Standard = new Listing_Standard(GameFont.Tiny);
			listing_Standard.ColumnWidth = Mathf.Min(columnWidth, inRect.width);
			listing_Standard.Begin(inRect);
			string[] array = debugStringBuilder.ToString().Split('\n');
			foreach (string label in array)
			{
				listing_Standard.Label(label);
				listing_Standard.Gap(-9f);
			}
			listing_Standard.End();
			if (Event.current.type == EventType.Repaint)
			{
				debugStringBuilder = new StringBuilder();
				debugStringBuilder.Append(CurrentDebugString());
			}
		}

		public void AppendDebugString(string str)
		{
			debugStringBuilder.AppendLine(str);
		}

		private string CurrentDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (DebugViewSettings.writeGame)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine((Current.Game == null) ? "Current.Game = null" : Current.Game.DebugString());
			}
			if (DebugViewSettings.writeMusicManagerPlay)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.MusicManagerPlay.DebugString());
			}
			if (DebugViewSettings.writePlayingSounds)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine("Sustainers:");
				foreach (Sustainer allSustainer in Find.SoundRoot.sustainerManager.AllSustainers)
				{
					stringBuilder.AppendLine(allSustainer.DebugString());
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("OneShots:");
				foreach (SampleOneShot playingOneShot in Find.SoundRoot.oneShotManager.PlayingOneShots)
				{
					stringBuilder.AppendLine(playingOneShot.ToString());
				}
			}
			if (DebugViewSettings.writeSoundEventsRecord)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine("Recent sound events:\n       ...");
				stringBuilder.AppendLine(DebugSoundEventsLog.EventsListingDebugString);
			}
			if (DebugViewSettings.writeSteamItems)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(WorkshopItems.DebugOutput());
			}
			if (DebugViewSettings.writeConcepts)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(LessonAutoActivator.DebugString());
			}
			if (DebugViewSettings.writeReservations && Find.CurrentMap != null)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(string.Join("\r\n", Find.CurrentMap.reservationManager.ReservationsReadOnly.Select((ReservationManager.Reservation r) => r.ToString()).ToArray()));
			}
			if (DebugViewSettings.writeMemoryUsage)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine("Total allocated: " + Profiler.GetTotalAllocatedMemoryLong().ToStringBytes());
				stringBuilder.AppendLine("Total reserved: " + Profiler.GetTotalReservedMemoryLong().ToStringBytes());
				stringBuilder.AppendLine("Total reserved unused: " + Profiler.GetTotalUnusedReservedMemoryLong().ToStringBytes());
				stringBuilder.AppendLine("Mono heap size: " + Profiler.GetMonoHeapSizeLong().ToStringBytes());
				stringBuilder.AppendLine("Mono used size: " + Profiler.GetMonoUsedSizeLong().ToStringBytes());
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				stringBuilder.AppendLine("Tick " + Find.TickManager.TicksGame);
				if (DebugViewSettings.writeStoryteller)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.Storyteller.DebugString());
				}
			}
			if (Current.ProgramState == ProgramState.Playing && Find.CurrentMap != null)
			{
				if (DebugViewSettings.writeMapGameConditions)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.gameConditionManager.DebugString());
				}
				if (DebugViewSettings.drawPawnDebug)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.reservationManager.DebugString());
				}
				if (DebugViewSettings.writeMoteSaturation)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Mote count: " + Find.CurrentMap.moteCounter.MoteCount);
					stringBuilder.AppendLine("Mote saturation: " + Find.CurrentMap.moteCounter.Saturation);
				}
				if (DebugViewSettings.writeEcosystem)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.wildAnimalSpawner.DebugString());
				}
				if (DebugViewSettings.writeTotalSnowDepth)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Total snow depth: " + Find.CurrentMap.snowGrid.TotalDepth);
				}
				if (DebugViewSettings.writeWind)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.windManager.DebugString());
				}
				if (DebugViewSettings.writeRecentStrikes)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.mineStrikeManager.DebugStrikeRecords());
				}
				if (DebugViewSettings.writeListRepairableBldgs)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.listerBuildingsRepairable.DebugString());
				}
				if (DebugViewSettings.writeListFilthInHomeArea)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.listerFilthInHomeArea.DebugString());
				}
				if (DebugViewSettings.writeListHaulables)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.listerHaulables.DebugString());
				}
				if (DebugViewSettings.writeListMergeables)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.listerMergeables.DebugString());
				}
				if (DebugViewSettings.drawLords)
				{
					foreach (Lord lord in Find.CurrentMap.lordManager.lords)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine(lord.DebugString());
					}
				}
				IntVec3 intVec = UI.MouseCell();
				if (intVec.InBounds(Find.CurrentMap))
				{
					stringBuilder.AppendLine("Inspecting " + intVec.ToString());
					if (DebugViewSettings.writeTerrain)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine(Find.CurrentMap.terrainGrid.DebugStringAt(intVec));
					}
					if (DebugViewSettings.writeAttackTargets)
					{
						foreach (Pawn item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>())
						{
							stringBuilder.AppendLine("---");
							stringBuilder.AppendLine("Potential attack targets for " + item.LabelShort + ":");
							List<IAttackTarget> potentialTargetsFor = Find.CurrentMap.attackTargetsCache.GetPotentialTargetsFor(item);
							for (int i = 0; i < potentialTargetsFor.Count; i++)
							{
								Thing thing = (Thing)potentialTargetsFor[i];
								stringBuilder.AppendLine(string.Concat(thing.LabelShort, ", ", thing.Faction, potentialTargetsFor[i].ThreatDisabled(null) ? " (threat disabled)" : ""));
							}
						}
					}
					if (DebugViewSettings.writeSnowDepth)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("Snow depth: " + Find.CurrentMap.snowGrid.GetDepth(intVec));
					}
					if (DebugViewSettings.drawDeepResources)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("Deep resource def: " + Find.CurrentMap.deepResourceGrid.ThingDefAt(intVec));
						stringBuilder.AppendLine("Deep resource count: " + Find.CurrentMap.deepResourceGrid.CountAt(intVec));
					}
					if (DebugViewSettings.writeCanReachColony)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("CanReachColony: " + Find.CurrentMap.reachability.CanReachColony(UI.MouseCell()));
					}
					if (DebugViewSettings.writeMentalStateCalcs)
					{
						stringBuilder.AppendLine("---");
						foreach (Pawn item2 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
							where t is Pawn
							select t).Cast<Pawn>())
						{
							stringBuilder.AppendLine(item2.mindState.mentalBreaker.DebugString());
						}
					}
					if (DebugViewSettings.writeWorkSettings)
					{
						foreach (Pawn item3 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
							where t is Pawn
							select t).Cast<Pawn>())
						{
							if (item3.workSettings != null)
							{
								stringBuilder.AppendLine("---");
								stringBuilder.AppendLine(item3.workSettings.DebugString());
							}
						}
					}
					if (DebugViewSettings.writeApparelScore)
					{
						stringBuilder.AppendLine("---");
						if (intVec.InBounds(Find.CurrentMap))
						{
							foreach (Thing thing3 in intVec.GetThingList(Find.CurrentMap))
							{
								Apparel apparel = thing3 as Apparel;
								if (apparel != null)
								{
									stringBuilder.AppendLine(apparel.Label + ": " + JobGiver_OptimizeApparel.ApparelScoreRaw(null, apparel).ToString("F2"));
								}
							}
						}
					}
					if (DebugViewSettings.writeCellContents || fullMode)
					{
						stringBuilder.AppendLine("---");
						if (intVec.InBounds(Find.CurrentMap))
						{
							foreach (Designation item4 in Find.CurrentMap.designationManager.AllDesignationsAt(intVec))
							{
								stringBuilder.AppendLine(item4.ToString());
							}
							foreach (Thing item5 in Find.CurrentMap.thingGrid.ThingsAt(intVec))
							{
								if (!fullMode)
								{
									stringBuilder.AppendLine(item5.LabelCap + " - " + item5.ToString());
									continue;
								}
								stringBuilder.AppendLine(Scribe.saver.DebugOutputFor(item5));
								stringBuilder.AppendLine();
							}
						}
					}
					if (DebugViewSettings.debugApparelOptimize)
					{
						stringBuilder.AppendLine("---");
						foreach (Thing item6 in Find.CurrentMap.thingGrid.ThingsAt(intVec))
						{
							Apparel apparel2 = item6 as Apparel;
							if (apparel2 == null)
							{
								continue;
							}
							stringBuilder.AppendLine(apparel2.LabelCap);
							stringBuilder.AppendLine("   raw: " + JobGiver_OptimizeApparel.ApparelScoreRaw(null, apparel2).ToString("F2"));
							Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
							if (pawn != null)
							{
								List<float> list = new List<float>();
								for (int j = 0; j < pawn.apparel.WornApparel.Count; j++)
								{
									list.Add(JobGiver_OptimizeApparel.ApparelScoreRaw(pawn, pawn.apparel.WornApparel[j]));
								}
								stringBuilder.AppendLine("  Pawn: " + pawn);
								stringBuilder.AppendLine("  gain: " + JobGiver_OptimizeApparel.ApparelScoreGain_NewTmp(pawn, apparel2, list).ToString("F2"));
							}
						}
					}
					if (DebugViewSettings.drawRegions)
					{
						stringBuilder.AppendLine("---");
						Region regionAt_NoRebuild_InvalidAllowed = Find.CurrentMap.regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(intVec);
						stringBuilder.AppendLine("Region:\n" + ((regionAt_NoRebuild_InvalidAllowed != null) ? regionAt_NoRebuild_InvalidAllowed.DebugString : "null"));
					}
					if (DebugViewSettings.drawRooms)
					{
						stringBuilder.AppendLine("---");
						Room room = intVec.GetRoom(Find.CurrentMap, RegionType.Set_All);
						if (room != null)
						{
							stringBuilder.AppendLine(room.DebugString());
						}
						else
						{
							stringBuilder.AppendLine("(no room)");
						}
					}
					if (DebugViewSettings.drawRoomGroups)
					{
						stringBuilder.AppendLine("---");
						RoomGroup roomGroup = intVec.GetRoomGroup(Find.CurrentMap);
						if (roomGroup != null)
						{
							stringBuilder.AppendLine(roomGroup.DebugString());
						}
						else
						{
							stringBuilder.AppendLine("(no room group)");
						}
					}
					if (DebugViewSettings.drawGlow)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("Game glow: " + Find.CurrentMap.glowGrid.GameGlowAt(intVec));
						stringBuilder.AppendLine("Psych glow: " + Find.CurrentMap.glowGrid.PsychGlowAt(intVec));
						stringBuilder.AppendLine("Visual Glow: " + Find.CurrentMap.glowGrid.VisualGlowAt(intVec));
						stringBuilder.AppendLine("GlowReport:\n" + ((SectionLayer_LightingOverlay)Find.CurrentMap.mapDrawer.SectionAt(intVec).GetLayer(typeof(SectionLayer_LightingOverlay))).GlowReportAt(intVec));
						stringBuilder.AppendLine("SkyManager.CurSkyGlow: " + Find.CurrentMap.skyManager.CurSkyGlow);
					}
					if (DebugViewSettings.writePathCosts)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("Perceived path cost: " + Find.CurrentMap.pathGrid.PerceivedPathCostAt(intVec));
						stringBuilder.AppendLine("Real path cost: " + Find.CurrentMap.pathGrid.CalculatedCostAt(intVec, perceivedStatic: false, IntVec3.Invalid));
					}
					if (DebugViewSettings.writeFertility)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("\nFertility: " + Find.CurrentMap.fertilityGrid.FertilityAt(intVec).ToString("##0.00"));
					}
					if (DebugViewSettings.writeLinkFlags)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("\nLinkFlags: ");
						foreach (object value in Enum.GetValues(typeof(LinkFlags)))
						{
							if ((Find.CurrentMap.linkGrid.LinkFlagsAt(intVec) & (LinkFlags)value) != 0)
							{
								stringBuilder.Append(" " + value);
							}
						}
					}
					if (DebugViewSettings.writeSkyManager)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine(Find.CurrentMap.skyManager.DebugString());
					}
					if (DebugViewSettings.writeCover)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.Append("Cover: ");
						Thing thing2 = Find.CurrentMap.coverGrid[intVec];
						if (thing2 == null)
						{
							stringBuilder.AppendLine("null");
						}
						else
						{
							stringBuilder.AppendLine(thing2.ToString());
						}
					}
					if (DebugViewSettings.drawPower)
					{
						stringBuilder.AppendLine("---");
						foreach (Thing item7 in Find.CurrentMap.thingGrid.ThingsAt(intVec))
						{
							ThingWithComps thingWithComps = item7 as ThingWithComps;
							if (thingWithComps != null && thingWithComps.GetComp<CompPowerTrader>() != null)
							{
								stringBuilder.AppendLine(" " + thingWithComps.GetComp<CompPowerTrader>().DebugString);
							}
						}
						PowerNet powerNet = Find.CurrentMap.powerNetGrid.TransmittedPowerNetAt(intVec);
						if (powerNet != null)
						{
							stringBuilder.AppendLine(powerNet.DebugString() ?? "");
						}
						else
						{
							stringBuilder.AppendLine("(no PowerNet here)");
						}
					}
					if (DebugViewSettings.drawPreyInfo)
					{
						Pawn pawn2 = Find.Selector.SingleSelectedThing as Pawn;
						if (pawn2 != null)
						{
							List<Thing> thingList = intVec.GetThingList(Find.CurrentMap);
							for (int k = 0; k < thingList.Count; k++)
							{
								Pawn pawn3 = thingList[k] as Pawn;
								if (pawn3 != null)
								{
									stringBuilder.AppendLine("---");
									if (FoodUtility.IsAcceptablePreyFor(pawn2, pawn3))
									{
										stringBuilder.AppendLine("Prey score: " + FoodUtility.GetPreyScoreFor(pawn2, pawn3));
									}
									else
									{
										stringBuilder.AppendLine("Prey score: None");
									}
									break;
								}
							}
						}
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
