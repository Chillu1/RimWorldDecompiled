using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Verse
{
	public static class DebugToolsPawns
	{
		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true, displayPriority = 1000)]
		private static void AddSlave()
		{
			AddGuest(GuestStatus.Slave);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void AddPrisoner()
		{
			AddGuest(GuestStatus.Prisoner);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void AddGuest()
		{
			AddGuest(GuestStatus.Guest);
		}

		private static void AddGuest(GuestStatus guestStatus)
		{
			foreach (Building_Bed item in Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>())
			{
				if ((item.OwnersForReading.Any() && !item.AnyUnownedSleepingSlot) || (guestStatus == GuestStatus.Prisoner && !item.ForPrisoners) || (guestStatus == GuestStatus.Slave && !item.ForSlaves))
				{
					continue;
				}
				PawnKindDef pawnKindDef = ((guestStatus != GuestStatus.Guest) ? DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef pk) => pk.defaultFactionDef != null && !pk.defaultFactionDef.isPlayer && pk.RaceProps.Humanlike && pk.mutant == null).RandomElement() : PawnKindDefOf.SpaceRefugee);
				Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionDef);
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
				pawn.guest.SetGuestStatus(Faction.OfPlayer, guestStatus);
				break;
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void DamageUntilDown(Pawn p)
		{
			HealthUtility.DamageUntilDowned(p);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DamageLegs(Pawn p)
		{
			HealthUtility.DamageLegsUntilIncapableOfMoving(p);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DamageUntilIncapableOfManipulation(Pawn p)
		{
			HealthUtility.DamageLimbsUntilIncapableOfManipulation(p);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DamageToDeath(Pawn p)
		{
			HealthUtility.DamageUntilDead(p);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void CarriedDamageToDeath(Pawn p)
		{
			HealthUtility.DamageUntilDead(p.carryTracker.CarriedThing as Pawn);
		}

		[DebugAction("Pawns", "10 damage until dead", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void Do10DamageUntilDead()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				for (int i = 0; i < 1000; i++)
				{
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Crush, 10f);
					dinfo.SetIgnoreInstantKillProtection(ignore: true);
					item.TakeDamage(dinfo);
					if (!item.Destroyed)
					{
						continue;
					}
					string text = "Took " + (i + 1) + " hits";
					if (item is Pawn pawn)
					{
						if (pawn.health.ShouldBeDeadFromLethalDamageThreshold())
						{
							text = text + " (reached lethal damage threshold of " + pawn.health.LethalDamageThreshold.ToString("0.#") + ")";
						}
						else if (PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, pawn.RaceProps.body.corePart) <= 0.0001f)
						{
							text += " (core part hp reached 0)";
						}
						else
						{
							PawnCapacityDef pawnCapacityDef = pawn.health.ShouldBeDeadFromRequiredCapacity();
							if (pawnCapacityDef != null)
							{
								text = text + " (incapable of " + pawnCapacityDef.defName + ")";
							}
						}
					}
					Log.Message(text + ".");
					break;
				}
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DamageHeldPawnToDeath()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				if (item is Pawn pawn && pawn.carryTracker.CarriedThing != null && pawn.carryTracker.CarriedThing is Pawn)
				{
					HealthUtility.DamageUntilDead((Pawn)pawn.carryTracker.CarriedThing);
				}
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RestoreBodyPart(Pawn p)
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_RestorePart(p)));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SetHeadType(Pawn p)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (HeadTypeDef headTypeDef in DefDatabase<HeadTypeDef>.AllDefsListForReading)
			{
				list.Add(new FloatMenuOption(headTypeDef.defName, delegate
				{
					p.story.headType = headTypeDef;
					p.Drawer.renderer.SetAllGraphicsDirty();
				}));
			}
			if (list.Any())
			{
				Find.WindowStack.Add(new FloatMenu(list, p.LabelShort));
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SetBodyType(Pawn p)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (BodyTypeDef bodyTypeDef in DefDatabase<BodyTypeDef>.AllDefsListForReading)
			{
				list.Add(new FloatMenuOption(bodyTypeDef.defName, delegate
				{
					p.story.bodyType = bodyTypeDef;
					p.Drawer.renderer.SetAllGraphicsDirty();
				}));
			}
			if (list.Any())
			{
				Find.WindowStack.Add(new FloatMenu(list, p.LabelShort));
			}
		}

		[DebugAction("Pawns", "Apply damage", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> ApplyDamage()
		{
			return DebugTools_Health.Options_ApplyDamage();
		}

		[DebugAction("Pawns", "Heal random injury (10)", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void HealRandomInjury10(Pawn p)
		{
			List<Hediff_Injury> resultHediffs = new List<Hediff_Injury>();
			p.health.hediffSet.GetHediffs(ref resultHediffs, (Hediff_Injury x) => x.CanHealNaturally() || x.CanHealFromTending());
			if (resultHediffs.TryRandomElement(out var result))
			{
				result.Heal(10f);
			}
		}

		[DebugAction("Pawns", "Make injuries permanent", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MakeInjuryPermanent(Pawn p)
		{
			foreach (Hediff hediff in p.health.hediffSet.hediffs)
			{
				HediffComp_GetsPermanent hediffComp_GetsPermanent = hediff.TryGetComp<HediffComp_GetsPermanent>();
				if (hediffComp_GetsPermanent != null)
				{
					hediffComp_GetsPermanent.IsPermanent = true;
				}
			}
		}

		[DebugAction("Pawns", "Toggle immunity", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ToggleImmunity(Pawn p)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (Hediff hediff2 in p.health.hediffSet.hediffs)
			{
				Hediff hediff = hediff2;
				ImmunityRecord immunityRecord = p.health.immunity.GetImmunityRecord(hediff.def);
				if (immunityRecord == null)
				{
					continue;
				}
				Texture2D iconTex = ((immunityRecord.immunity < 1f) ? Widgets.CheckboxOffTex : Widgets.CheckboxOnTex);
				list.Add(new FloatMenuOption(hediff.LabelCap, delegate
				{
					if (immunityRecord.immunity < 1f)
					{
						immunityRecord.immunity = 1f;
					}
					else
					{
						immunityRecord.immunity = 0f;
					}
				}, iconTex, Color.white));
			}
			if (list.Any())
			{
				Find.WindowStack.Add(new FloatMenu(list, p.LabelShort));
			}
		}

		[DebugAction("Pawns", "Activate HediffGiver", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ActivateHediffGiver(Pawn p)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (p.RaceProps.hediffGiverSets != null)
			{
				foreach (HediffGiver item in p.RaceProps.hediffGiverSets.SelectMany((HediffGiverSetDef set) => set.hediffGivers))
				{
					HediffGiver localHdg = item;
					list.Add(new FloatMenuOption(localHdg.hediff.defName, delegate
					{
						if (localHdg.TryApply(p))
						{
							Messages.Message(localHdg.hediff.defName + " applied to " + p.Label, MessageTypeDefOf.NeutralEvent, historical: false);
						}
						else
						{
							Messages.Message("failed to apply " + localHdg.hediff.defName + " to " + p.Label, MessageTypeDefOf.NegativeEvent, historical: false);
						}
					}));
				}
			}
			if (list.Any())
			{
				Find.WindowStack.Add(new FloatMenu(list));
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", "Activate HediffGiver World Pawn", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void ActivateHediffGiverWorldPawn()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Pawn item in Find.WorldPawns.AllPawnsAlive.Where((Pawn p) => p.RaceProps.Humanlike))
			{
				Pawn pawnLocal = item;
				list.Add(new DebugMenuOption(pawnLocal.Label, DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (HediffGiver item2 in pawnLocal.RaceProps.hediffGiverSets.SelectMany((HediffGiverSetDef s) => s.hediffGivers))
					{
						HediffGiver hediffGiverLocal = item2;
						list2.Add(new DebugMenuOption(hediffGiverLocal.hediff.defName, DebugMenuOptionMode.Action, delegate
						{
							if (hediffGiverLocal.TryApply(pawnLocal))
							{
								Messages.Message(hediffGiverLocal.hediff.defName + " applied to " + pawnLocal.Label, MessageTypeDefOf.NeutralEvent, historical: false);
							}
							else
							{
								Messages.Message("failed to apply " + hediffGiverLocal.hediff.defName + " to " + pawnLocal.Label, MessageTypeDefOf.NegativeEvent, historical: false);
							}
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DiscoverHediffs(Pawn p)
		{
			foreach (Hediff hediff in p.health.hediffSet.hediffs)
			{
				if (!hediff.Visible)
				{
					hediff.Severity = Mathf.Max(hediff.Severity, hediff.def.stages.First((HediffStage s) => s.becomeVisible).minSeverity);
				}
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GrantImmunities(Pawn p)
		{
			foreach (Hediff hediff in p.health.hediffSet.hediffs)
			{
				ImmunityRecord immunityRecord = p.health.immunity.GetImmunityRecord(hediff.def);
				if (immunityRecord != null)
				{
					immunityRecord.immunity = 1f;
				}
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void GiveBirth(Pawn p)
		{
			Hediff_Pregnant.DoBirthSpawn(p, null);
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", "Resistance -1", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void ResistanceMinus1(Pawn p)
		{
			if (p.guest != null && p.guest.resistance > 0f)
			{
				p.guest.resistance = Mathf.Max(0f, p.guest.resistance - 1f);
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", "Resistance -10", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void ResistanceMinus10(Pawn p)
		{
			if (p.guest != null && p.guest.resistance > 0f)
			{
				p.guest.resistance = Mathf.Max(0f, p.guest.resistance - 10f);
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", "Add/remove pawn relation", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AddRemovePawnRelation(Pawn p)
		{
			if (!p.RaceProps.IsFlesh)
			{
				return;
			}
			Action<bool> act = delegate(bool add)
			{
				if (add)
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (PawnRelationDef allDef in DefDatabase<PawnRelationDef>.AllDefs)
					{
						if (!allDef.implied)
						{
							PawnRelationDef defLocal = allDef;
							list2.Add(new DebugMenuOption(defLocal.defName, DebugMenuOptionMode.Action, delegate
							{
								List<DebugMenuOption> list4 = new List<DebugMenuOption>();
								foreach (Pawn item in from x in PawnsFinder.AllMapsWorldAndTemporary_Alive
									where x.RaceProps.IsFlesh || (x.RaceProps.IsMechanoid && x.Faction == Faction.OfPlayer)
									orderby x.def == p.def descending, x.IsWorldPawn()
									select x)
								{
									if (p != item && (!defLocal.familyByBloodRelation || item.def == p.def) && !p.relations.DirectRelationExists(defLocal, item))
									{
										Pawn otherLocal = item;
										list4.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
										{
											p.relations.AddDirectRelation(defLocal, otherLocal);
											if (defLocal == PawnRelationDefOf.Fiance)
											{
												otherLocal.relations.nextMarriageNameChange = (p.relations.nextMarriageNameChange = SpouseRelationUtility.Roll_NameChangeOnMarriage(p));
											}
										}));
									}
								}
								Find.WindowStack.Add(new Dialog_DebugOptionListLister(list4));
							}));
						}
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}
				else
				{
					List<DebugMenuOption> list3 = new List<DebugMenuOption>();
					List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
					for (int num = 0; num < directRelations.Count; num++)
					{
						DirectPawnRelation rel = directRelations[num];
						list3.Add(new DebugMenuOption(rel.def.defName + " - " + rel.otherPawn.LabelShort, DebugMenuOptionMode.Action, delegate
						{
							p.relations.RemoveDirectRelation(rel);
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
				}
			};
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("Add", DebugMenuOptionMode.Action, delegate
			{
				act(obj: true);
			}));
			if (!p.relations.DirectRelations.NullOrEmpty())
			{
				list.Add(new DebugMenuOption("Remove", DebugMenuOptionMode.Action, delegate
				{
					act(obj: false);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void AddOpinionTalksAbout(Pawn p)
		{
			if (!p.RaceProps.Humanlike)
			{
				return;
			}
			Action<bool> act = delegate(bool good)
			{
				foreach (Pawn item in p.Map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.RaceProps.Humanlike))
				{
					if (p != item)
					{
						IEnumerable<ThoughtDef> source = DefDatabase<ThoughtDef>.AllDefs.Where((ThoughtDef x) => typeof(Thought_MemorySocial).IsAssignableFrom(x.thoughtClass) && ((good && x.stages[0].baseOpinionOffset > 0f) || (!good && x.stages[0].baseOpinionOffset < 0f)));
						if (source.Any())
						{
							int num = Rand.Range(2, 5);
							for (int num2 = 0; num2 < num; num2++)
							{
								ThoughtDef def = source.RandomElement();
								item.needs.mood.thoughts.memories.TryGainMemory(def, p);
							}
						}
					}
				}
			};
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("Good", DebugMenuOptionMode.Action, delegate
			{
				act(obj: true);
			}));
			list.Add(new DebugMenuOption("Bad", DebugMenuOptionMode.Action, delegate
			{
				act(obj: false);
			}));
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> SetSkill()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
			{
				SkillDef localDef = allDef;
				DebugActionNode debugActionNode = new DebugActionNode(localDef.defName);
				for (int i = 0; i <= 20; i++)
				{
					int level = i;
					debugActionNode.AddChild(new DebugActionNode(level.ToString(), DebugActionType.ToolMapForPawns)
					{
						pawnAction = delegate(Pawn p)
						{
							if (p.skills != null)
							{
								SkillRecord skill = p.skills.GetSkill(localDef);
								skill.Level = level;
								skill.xpSinceLastLevel = skill.XpRequiredForLevelUp / 2f;
								DebugActionsUtility.DustPuffFrom(p);
							}
						}
					});
				}
				list.Add(debugActionNode);
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> MaxSkill()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
			{
				SkillDef localDef = allDef;
				list.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMapForPawns, null, delegate(Pawn p)
				{
					p.skills?.Learn(localDef, 100000000f);
				}));
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MaxAllSkills(Pawn p)
		{
			if (p.skills != null)
			{
				foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
				{
					p.skills.Learn(allDef, 100000000f);
				}
				DebugActionsUtility.DustPuffFrom(p);
			}
			if (p.training == null)
			{
				return;
			}
			foreach (TrainableDef allDef2 in DefDatabase<TrainableDef>.AllDefs)
			{
				Pawn trainer = p.Map.mapPawns.FreeColonistsSpawned.RandomElement();
				if (p.training.CanAssignToTrain(allDef2, out var _).Accepted)
				{
					p.training.Train(allDef2, trainer);
				}
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> SetPassion()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
			{
				SkillDef localDef = allDef;
				DebugActionNode debugActionNode = new DebugActionNode(localDef.defName);
				foreach (object passion in Enum.GetValues(typeof(Passion)))
				{
					debugActionNode.AddChild(new DebugActionNode(passion.ToString(), DebugActionType.ToolMapForPawns)
					{
						pawnAction = delegate(Pawn p)
						{
							if (p.skills != null)
							{
								p.skills.GetSkill(localDef).passion = (Passion)passion;
								DebugActionsUtility.DustPuffFrom(p);
							}
						}
					});
				}
				list.Add(debugActionNode);
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> MaxPassion()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
			{
				SkillDef localDef = allDef;
				list.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMapForPawns, null, delegate(Pawn p)
				{
					if (p.skills != null)
					{
						p.skills.GetSkill(localDef).passion = Passion.Major;
					}
				}));
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MaxAllPassions(Pawn p)
		{
			if (p.skills == null)
			{
				return;
			}
			foreach (SkillRecord skill in p.skills.skills)
			{
				skill.passion = Passion.Major;
			}
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", "Mental break", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> MentalBreak()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			list.Add(new DebugActionNode("(log possibles)", DebugActionType.ToolMapForPawns)
			{
				pawnAction = delegate(Pawn p)
				{
					p.mindState.mentalBreaker.LogPossibleMentalBreaks();
					DebugActionsUtility.DustPuffFrom(p);
				}
			});
			list.Add(new DebugActionNode("(natural mood break)", DebugActionType.ToolMapForPawns)
			{
				pawnAction = delegate(Pawn p)
				{
					p.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
					DebugActionsUtility.DustPuffFrom(p);
				}
			});
			foreach (MentalBreakDef item in DefDatabase<MentalBreakDef>.AllDefs.OrderByDescending((MentalBreakDef x) => x.intensity))
			{
				MentalBreakDef locBrDef = item;
				list.Add(new DebugActionNode(locBrDef.defName, DebugActionType.ToolMapForPawns)
				{
					pawnAction = delegate(Pawn p)
					{
						locBrDef.Worker.TryStart(p, null, causedByMood: false);
						DebugActionsUtility.DustPuffFrom(p);
					},
					labelGetter = delegate
					{
						string text = locBrDef.defName;
						if (Find.CurrentMap != null && !Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => locBrDef.Worker.BreakCanOccur(x)))
						{
							text += " [NO]";
						}
						return text;
					}
				});
			}
			return list;
		}

		[DebugAction("Pawns", "Mental state...", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> MentalState()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (MentalStateDef allDef in DefDatabase<MentalStateDef>.AllDefs)
			{
				MentalStateDef locBrDef = allDef;
				list.Add(new DebugActionNode(locBrDef.defName, DebugActionType.ToolMapForPawns)
				{
					pawnAction = delegate(Pawn p)
					{
						if (locBrDef != MentalStateDefOf.SocialFighting)
						{
							p.mindState.mentalStateHandler.TryStartMentalState(locBrDef, null, forced: false, forceWake: true);
							DebugActionsUtility.DustPuffFrom(p);
						}
						else
						{
							DebugTools.curTool = new DebugTool("...with", delegate
							{
								Pawn pawn = (Pawn)(from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
									where t is Pawn
									select t).FirstOrDefault();
								if (pawn != null)
								{
									p.interactions.StartSocialFight(pawn);
									DebugTools.curTool = null;
								}
							});
						}
					},
					labelGetter = delegate
					{
						string text = locBrDef.defName;
						if (Find.CurrentMap == null)
						{
							return text;
						}
						if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => locBrDef.Worker.StateCanOccur(x)))
						{
							text += " [NO]";
						}
						return text;
					}
				});
			}
			return list;
		}

		[DebugAction("Pawns", "Stop mental state", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void StopMentalState(Pawn p)
		{
			if (p.InMentalState)
			{
				p.MentalState.RecoverFromState();
				p.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> Inspiration()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (InspirationDef allDef in DefDatabase<InspirationDef>.AllDefs)
			{
				InspirationDef localDef = allDef;
				list.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMapForPawns)
				{
					pawnAction = delegate(Pawn p)
					{
						p.mindState.inspirationHandler?.TryStartInspiration(localDef, "Debug gain");
						DebugActionsUtility.DustPuffFrom(p);
					},
					labelGetter = delegate
					{
						string text = localDef.defName;
						if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => localDef.Worker.InspirationCanOccur(x)))
						{
							text += " [NO]";
						}
						return text;
					}
				});
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static List<DebugActionNode> GiveTrait()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (TraitDef allDef in DefDatabase<TraitDef>.AllDefs)
			{
				TraitDef trDef = allDef;
				for (int i = 0; i < allDef.degreeDatas.Count; i++)
				{
					int i2 = i;
					list.Add(new DebugActionNode(trDef.degreeDatas[i2].label + " (" + trDef.degreeDatas[i].degree + ")", DebugActionType.ToolMapForPawns)
					{
						pawnAction = delegate(Pawn p)
						{
							if (p.story != null)
							{
								p.story.traits.GainTrait(new Trait(trDef, trDef.degreeDatas[i2].degree), suppressConflicts: true);
								DebugActionsUtility.DustPuffFrom(p);
							}
						}
					});
				}
			}
			return list;
		}

		[DebugAction("Pawns", "Remove all traits", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RemoveAllTraits(Pawn p)
		{
			if (p.story != null)
			{
				for (int num = p.story.traits.allTraits.Count - 1; num >= 0; num--)
				{
					p.story.traits.RemoveTrait(p.story.traits.allTraits[num]);
				}
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static List<DebugActionNode> SetBackstory()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			DebugActionNode debugActionNode = new DebugActionNode("Adulthood");
			foreach (DebugActionNode item in BackstoryOptionNodes(BackstorySlot.Adulthood))
			{
				debugActionNode.AddChild(item);
			}
			list.Add(debugActionNode);
			DebugActionNode debugActionNode2 = new DebugActionNode("Childhood");
			foreach (DebugActionNode item2 in BackstoryOptionNodes(BackstorySlot.Childhood))
			{
				debugActionNode2.AddChild(item2);
			}
			list.Add(debugActionNode2);
			return list;
			static List<DebugActionNode> BackstoryOptionNodes(BackstorySlot slot)
			{
				List<DebugActionNode> list2 = new List<DebugActionNode>();
				foreach (BackstoryDef outerBackstory in DefDatabase<BackstoryDef>.AllDefs.Where((BackstoryDef b) => b.slot == slot))
				{
					list2.Add(new DebugActionNode(outerBackstory.defName, DebugActionType.ToolMapForPawns)
					{
						pawnAction = delegate(Pawn p)
						{
							if (p.story != null)
							{
								if (slot == BackstorySlot.Adulthood)
								{
									p.story.Adulthood = outerBackstory;
								}
								else
								{
									p.story.Childhood = outerBackstory;
								}
								MeditationFocusTypeAvailabilityCache.ClearFor(p);
								DebugActionsUtility.DustPuffFrom(p);
							}
						}
					});
				}
				return list2;
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static List<DebugActionNode> GiveAbility()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			list.Add(new DebugActionNode("*All", DebugActionType.ToolMapForPawns)
			{
				pawnAction = delegate(Pawn p)
				{
					if (p.abilities != null)
					{
						foreach (AbilityDef allDef in DefDatabase<AbilityDef>.AllDefs)
						{
							p.abilities.GainAbility(allDef);
						}
					}
				}
			});
			foreach (AbilityDef allDef2 in DefDatabase<AbilityDef>.AllDefs)
			{
				AbilityDef localAb = allDef2;
				list.Add(new DebugActionNode(allDef2.label, DebugActionType.ToolMapForPawns, null, delegate(Pawn p)
				{
					p.abilities?.GainAbility(localAb);
				}));
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000, requiresRoyalty = true)]
		private static List<DebugActionNode> GivePsylink()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			if (!ModsConfig.RoyaltyActive)
			{
				return list;
			}
			for (int i = 1; i <= (int)HediffDefOf.PsychicAmplifier.maxSeverity; i++)
			{
				int level = i;
				list.Add(new DebugActionNode("Level " + i, DebugActionType.ToolMapForPawns)
				{
					pawnAction = delegate(Pawn p)
					{
						Hediff_Level hediff_Level = p.GetMainPsylinkSource();
						if (hediff_Level == null)
						{
							hediff_Level = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, p, p.health.hediffSet.GetBrain()) as Hediff_Level;
							p.health.AddHediff(hediff_Level);
						}
						hediff_Level.ChangeLevel(level - hediff_Level.level);
					}
				});
			}
			return list;
		}

		[DebugAction("Pawns", "Play Animation...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> PlayAnimation()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (AnimationDef allDef in DefDatabase<AnimationDef>.AllDefs)
			{
				AnimationDef localDef = allDef;
				list.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMapForPawns)
				{
					pawnAction = delegate(Pawn p)
					{
						p.Drawer.renderer.SetAnimation(localDef);
					}
				});
			}
			return list;
		}

		[DebugAction("Pawns", "Give good thought", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void GiveGoodThought(Pawn p)
		{
			if (p.needs.mood != null)
			{
				p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DebugGood);
			}
		}

		[DebugAction("Pawns", "Give bad thought", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void GiveBadThought(Pawn p)
		{
			if (p.needs.mood != null)
			{
				p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DebugBad);
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void ClearBoundUnfinishedThings()
		{
			foreach (Building_WorkTable item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
				where t is Building_WorkTable
				select t).Cast<Building_WorkTable>())
			{
				foreach (Bill item2 in item.BillStack)
				{
					if (item2 is Bill_ProductionWithUft bill_ProductionWithUft)
					{
						bill_ProductionWithUft.ClearBoundUft();
					}
				}
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceBirthday(Pawn p)
		{
			p.ageTracker.AgeBiologicalTicks = (p.ageTracker.AgeBiologicalYears + 1) * 3600000 + 1;
			p.ageTracker.DebugForceBirthdayBiological();
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void Recruit(Pawn p)
		{
			if (p.Faction != Faction.OfPlayer)
			{
				if (p.RaceProps.Humanlike)
				{
					InteractionWorker_RecruitAttempt.DoRecruit(p.Map.mapPawns.FreeColonists.RandomElement(), p);
					DebugActionsUtility.DustPuffFrom(p);
				}
				else if (p.RaceProps.IsMechanoid)
				{
					p.SetFaction(Faction.OfPlayer);
					DebugActionsUtility.DustPuffFrom(p);
				}
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true, displayPriority = 1000)]
		private static void Enslave(Pawn p)
		{
			if (p.Faction != Faction.OfPlayer && p.RaceProps.Humanlike)
			{
				GenGuest.EnslavePrisoner(p.Map.mapPawns.FreeColonists.RandomElement(), p);
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ToggleRecruitable(Pawn p)
		{
			if (p.guest != null)
			{
				p.guest.Recruitable = !p.guest.Recruitable;
				DebugActionsUtility.DustPuffFrom(p);
				MoteMaker.ThrowText(p.DrawPos, p.MapHeld, "Recruitable:\n" + p.guest.Recruitable.ToStringYesNo());
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GrowPawnToMaturity()
		{
			Pawn firstPawn = UI.MouseCell().GetFirstPawn(Find.CurrentMap);
			if (firstPawn != null)
			{
				firstPawn.ageTracker.AgeBiologicalTicks += Mathf.FloorToInt(firstPawn.ageTracker.AdultMinAge * 3600000f);
			}
		}

		[DebugAction("Pawns", "Wear apparel (selected)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> WearApparel_ToSelected()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			list.Add(new DebugActionNode("*Remove all apparel")
			{
				action = delegate
				{
					foreach (object item in Find.Selector.SelectedObjectsListForReading)
					{
						if (item is Pawn pawn)
						{
							pawn.apparel?.DestroyAll();
						}
					}
				}
			});
			foreach (ThingDef item2 in from d in DefDatabase<ThingDef>.AllDefs
				where d.IsApparel
				orderby d.defName
				select d)
			{
				ThingDef localDef = item2;
				list.Add(new DebugActionNode(localDef.defName)
				{
					action = delegate
					{
						foreach (object item3 in Find.Selector.SelectedObjectsListForReading)
						{
							if (item3 is Pawn { apparel: not null } pawn)
							{
								ThingDef stuff = GenStuff.RandomStuffFor(localDef);
								Apparel newApparel = (Apparel)ThingMaker.MakeThing(localDef, stuff);
								pawn.apparel.Wear(newApparel, dropReplacedApparel: false);
							}
						}
					}
				});
			}
			return list;
		}

		[DebugAction("Pawns", "Equip primary (selected)...", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> EquipPrimary_ToSelected()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			list.Add(new DebugActionNode("*Remove primary")
			{
				action = delegate
				{
					foreach (object item in Find.Selector.SelectedObjectsListForReading)
					{
						if (item is Pawn pawn && pawn.equipment?.Primary != null)
						{
							pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
						}
					}
				}
			});
			foreach (ThingDef def in from d in DefDatabase<ThingDef>.AllDefs
				where d.equipmentType == EquipmentType.Primary
				orderby d.defName
				select d)
			{
				ThingDef thingDef = def;
				list.Add(new DebugActionNode(thingDef.defName)
				{
					action = delegate
					{
						foreach (object item2 in Find.Selector.SelectedObjectsListForReading)
						{
							if (item2 is Pawn { equipment: not null } pawn)
							{
								if (pawn.equipment.Primary != null)
								{
									pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
								}
								ThingDef stuff = GenStuff.RandomStuffFor(def);
								ThingWithComps newEq = (ThingWithComps)ThingMaker.MakeThing(def, stuff);
								pawn.equipment.AddEquipment(newEq);
							}
						}
					}
				});
			}
			return list;
		}

		public static List<FloatMenuOption> PawnGearDevOptions(Pawn pawn)
		{
			return new List<FloatMenuOption>
			{
				new FloatMenuOption("Set primary", delegate
				{
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_SetPrimary(pawn)));
				}),
				new FloatMenuOption("Wear", delegate
				{
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_Wear(pawn)));
				}),
				new FloatMenuOption("Add to inventory", delegate
				{
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_GiveToInventory(pawn)));
				}),
				new FloatMenuOption("Damage random apparel", delegate
				{
					pawn.apparel.WornApparel.RandomElement().TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 30f));
				})
			};
		}

		private static List<DebugMenuOption> Options_Wear(Pawn pawn)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("*Remove all apparel", DebugMenuOptionMode.Action, delegate
			{
				pawn.apparel.DestroyAll();
			}));
			foreach (ThingDef def in from d in DefDatabase<ThingDef>.AllDefs
				where d.IsApparel && d.apparel.developmentalStageFilter.Has(pawn.DevelopmentalStage)
				orderby d.defName
				select d)
			{
				list.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Action, delegate
				{
					ThingDef stuff = GenStuff.RandomStuffFor(def);
					Apparel newApparel = (Apparel)ThingMaker.MakeThing(def, stuff);
					pawn.apparel.Wear(newApparel, dropReplacedApparel: false);
				}));
			}
			return list;
		}

		private static List<DebugMenuOption> Options_SetPrimary(Pawn pawn)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("*Remove primary", DebugMenuOptionMode.Action, delegate
			{
				if (pawn.equipment != null && pawn.equipment.Primary != null)
				{
					pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
				}
			}));
			foreach (ThingDef def in from d in DefDatabase<ThingDef>.AllDefs
				where d.equipmentType == EquipmentType.Primary
				orderby d.defName
				select d)
			{
				list.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Action, delegate
				{
					if (pawn.equipment.Primary != null)
					{
						pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
					}
					ThingDef stuff = GenStuff.RandomStuffFor(def);
					ThingWithComps newEq = (ThingWithComps)ThingMaker.MakeThing(def, stuff);
					pawn.equipment.AddEquipment(newEq);
				}));
			}
			return list;
		}

		private static List<DebugMenuOption> Options_GiveToInventory(Pawn pawn)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("*Clear all", DebugMenuOptionMode.Action, delegate
			{
				pawn.inventory.DestroyAll();
			}));
			foreach (ThingDef def in from d in DefDatabase<ThingDef>.AllDefs
				where d.category == ThingCategory.Item
				orderby d.defName
				select d)
			{
				list.Add(new DebugMenuOption(def.label, DebugMenuOptionMode.Action, delegate
				{
					pawn.inventory.TryAddItemNotForSale(ThingMaker.MakeThing(def));
				}));
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void TameAnimal(Pawn p)
		{
			if (p.AnimalOrWildMan() && p.Faction != Faction.OfPlayer)
			{
				InteractionWorker_RecruitAttempt.DoRecruit(p.Map.mapPawns.FreeColonists.FirstOrDefault(), p);
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TrainAnimal(Pawn p)
		{
			if (!p.IsAnimal || p.Faction != Faction.OfPlayer || p.training == null)
			{
				return;
			}
			DebugActionsUtility.DustPuffFrom(p);
			bool flag = false;
			foreach (TrainableDef allDef in DefDatabase<TrainableDef>.AllDefs)
			{
				if (p.training.GetWanted(allDef))
				{
					p.training.Train(allDef, null, complete: true);
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			foreach (TrainableDef allDef2 in DefDatabase<TrainableDef>.AllDefs)
			{
				if (p.training.CanAssignToTrain(allDef2).Accepted)
				{
					p.training.Train(allDef2, null, complete: true);
				}
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryDevelopBoundRelation(Pawn p)
		{
			if (p.Faction == null)
			{
				return;
			}
			Pawn result2;
			if (p.RaceProps.Humanlike)
			{
				if (p.Map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.IsAnimal && x.Faction == p.Faction).TryRandomElement(out var result))
				{
					RelationsUtility.TryDevelopBondRelation(p, result, 999999f);
				}
			}
			else if (p.IsAnimal && p.Map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.RaceProps.Humanlike && x.Faction == p.Faction).TryRandomElement(out result2))
			{
				RelationsUtility.TryDevelopBondRelation(result2, p, 999999f);
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void QueueTrainingDecay(Pawn p)
		{
			if (p.IsAnimal && p.Faction == Faction.OfPlayer && p.training != null)
			{
				p.training.Debug_MakeDegradeHappenSoon();
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DisplayRelationsInfo(Pawn pawn)
		{
			List<TableDataGetter<Pawn>> list = new List<TableDataGetter<Pawn>>
			{
				new TableDataGetter<Pawn>("name", (Pawn p) => p.LabelCap),
				new TableDataGetter<Pawn>("kind label", (Pawn p) => p.KindLabel),
				new TableDataGetter<Pawn>("gender", (Pawn p) => p.gender.GetLabel()),
				new TableDataGetter<Pawn>("age", (Pawn p) => p.ageTracker.AgeBiologicalYears),
				new TableDataGetter<Pawn>("my compat", (Pawn p) => pawn.relations.CompatibilityWith(p).ToString("F2")),
				new TableDataGetter<Pawn>("their compat", (Pawn p) => p.relations.CompatibilityWith(pawn).ToString("F2")),
				new TableDataGetter<Pawn>("my 2nd\nrom chance", (Pawn p) => pawn.relations.SecondaryRomanceChanceFactor(p).ToStringPercent("F0")),
				new TableDataGetter<Pawn>("their 2nd\nrom chance", (Pawn p) => p.relations.SecondaryRomanceChanceFactor(pawn).ToStringPercent("F0")),
				new TableDataGetter<Pawn>("lovin mtb", (Pawn p) => LovePartnerRelationUtility.GetLovinMtbHours(pawn, p).ToString("F1") + " h")
			};
			DebugTables.MakeTablesDialog(from x in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
				where x != pawn && x.RaceProps.Humanlike
				select x, list.ToArray());
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DisplayInteractionsInfo(Pawn pawn)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Pawn p in from x in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
				where x != pawn && x.RaceProps.Humanlike
				select x)
			{
				float totalWeight = DefDatabase<InteractionDef>.AllDefs.Sum((InteractionDef x) => x.Worker.RandomSelectionWeight(pawn, p));
				list.Add(new DebugMenuOption(p.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					List<TableDataGetter<InteractionDef>> list2 = new List<TableDataGetter<InteractionDef>>
					{
						new TableDataGetter<InteractionDef>("defName", (InteractionDef i) => i.defName),
						new TableDataGetter<InteractionDef>("sel weight", (InteractionDef i) => i.Worker.RandomSelectionWeight(pawn, p)),
						new TableDataGetter<InteractionDef>("sel chance", (InteractionDef i) => (i.Worker.RandomSelectionWeight(pawn, p) / totalWeight).ToStringPercent()),
						new TableDataGetter<InteractionDef>("fight\nchance", (InteractionDef i) => p.interactions.SocialFightChance(i, pawn).ToStringPercent()),
						new TableDataGetter<InteractionDef>("success\nchance", delegate(InteractionDef i)
						{
							if (i == InteractionDefOf.RomanceAttempt)
							{
								return InteractionWorker_RomanceAttempt.SuccessChance(pawn, p).ToStringPercent();
							}
							return (i == InteractionDefOf.MarriageProposal) ? InteractionWorker_MarriageProposal.AcceptanceChance(pawn, p).ToStringPercent() : "";
						})
					};
					DebugTables.MakeTablesDialog(DefDatabase<InteractionDef>.AllDefs, list2.ToArray());
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void StartMarriageCeremony(Pawn p)
		{
			if (!p.RaceProps.Humanlike)
			{
				return;
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Pawn item in p.Map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.RaceProps.Humanlike))
			{
				if (p == item)
				{
					continue;
				}
				Pawn otherLocal = item;
				list.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
				{
					if (!p.relations.DirectRelationExists(PawnRelationDefOf.Fiance, otherLocal))
					{
						p.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherLocal);
						p.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherLocal);
						p.relations.AddDirectRelation(PawnRelationDefOf.Fiance, otherLocal);
						Messages.Message("DEV: Auto added fiance relation.", p, MessageTypeDefOf.TaskCompletion, historical: false);
					}
					if (!p.Map.lordsStarter.TryStartMarriageCeremony(p, otherLocal))
					{
						Messages.Message("Could not find any valid marriage site.", MessageTypeDefOf.RejectInput, historical: false);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceInteraction(Pawn p)
		{
			if (p.Faction == null)
			{
				return;
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Pawn item in p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction))
			{
				if (item == p)
				{
					continue;
				}
				Pawn otherLocal = item;
				list.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (InteractionDef item2 in DefDatabase<InteractionDef>.AllDefsListForReading)
					{
						InteractionDef interactionLocal = item2;
						list2.Add(new DebugMenuOption(interactionLocal.label, DebugMenuOptionMode.Action, delegate
						{
							p.interactions.TryInteractWith(otherLocal, interactionLocal);
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> StartGathering()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			list.Add(new DebugActionNode("*Random")
			{
				action = delegate
				{
					if (!Find.CurrentMap.lordsStarter.TryStartRandomGathering(forceStart: true))
					{
						Messages.Message("Could not find any valid gathering spot or organizer.", MessageTypeDefOf.RejectInput, historical: false);
					}
				}
			});
			foreach (GatheringDef item in DefDatabase<GatheringDef>.AllDefsListForReading)
			{
				GatheringDef gatheringDef = item;
				list.Add(new DebugActionNode(gatheringDef.defName)
				{
					action = delegate
					{
						gatheringDef.Worker.TryExecute(Find.CurrentMap);
					},
					labelGetter = () => gatheringDef.LabelCap + " (" + ((Find.World?.factionManager != null && gatheringDef.Worker.CanExecute(Find.CurrentMap)) ? "Yes" : "No") + ")"
				});
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void StartPrisonBreak(Pawn p)
		{
			if (p.IsPrisoner)
			{
				PrisonBreakUtility.StartPrisonBreak(p);
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void PassToWorld(Pawn p)
		{
			p.DeSpawn();
			Find.WorldPawns.PassToWorld(p, PawnDiscardDecideMode.KeepForever);
		}

		[DebugAction("Spawning", "Remove world pawn...", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void RemoveWorldPawn()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Pawn item in Find.WorldPawns.AllPawnsAliveOrDead)
			{
				Pawn pLocal = item;
				string text = item.LabelShort;
				WorldPawnSituation situation = Find.WorldPawns.GetSituation(item);
				if (situation != WorldPawnSituation.Free)
				{
					text = text + " [" + situation.ToString() + "]";
				}
				list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
				{
					Find.WorldPawns.RemovePawn(pLocal);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", "Make +1 year older", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Make1YearOlder(Pawn p)
		{
			float num = p.ageTracker.BiologicalTicksPerTick;
			if (num == 0f)
			{
				num = 1f;
			}
			p.ageTracker.AgeTickMothballed(Mathf.RoundToInt(3600000f / num));
		}

		[DebugAction("Pawns", "Make +1 day older", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Make1DayOlder(Pawn p)
		{
			p.ageTracker.AgeTickMothballed(60000);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryJobGiver(Pawn p)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type item in typeof(ThinkNode_JobGiver).AllSubclasses())
			{
				Type localType = item;
				list.Add(new DebugMenuOption(localType.Name, DebugMenuOptionMode.Action, delegate
				{
					ThinkNode_JobGiver obj = (ThinkNode_JobGiver)Activator.CreateInstance(localType);
					obj.ResolveReferences();
					ThinkResult thinkResult = obj.TryIssueJobPackage(p, default(JobIssueParams));
					if (thinkResult.Job != null)
					{
						p.jobs.StartJob(thinkResult.Job);
					}
					else
					{
						Messages.Message("Failed to give job", MessageTypeDefOf.RejectInput, historical: false);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryJoyGiver(Pawn p)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (JoyGiverDef def in DefDatabase<JoyGiverDef>.AllDefsListForReading)
			{
				list.Add(new DebugMenuOption(def.Worker.CanBeGivenTo(p) ? def.defName : (def.defName + " [NO]"), DebugMenuOptionMode.Action, delegate
				{
					Job job = def.Worker.TryGiveJob(p);
					if (job != null)
					{
						p.jobs.StartJob(job, JobCondition.InterruptForced);
					}
					else
					{
						Messages.Message("Failed to give job", MessageTypeDefOf.RejectInput, historical: false);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", "EndCurrentJob(InterruptForced)", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void EndCurrentJobInterruptForced(Pawn p)
		{
			p.jobs.EndCurrentJob(JobCondition.InterruptForced);
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", "CheckForJobOverride", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void CheckForJobOverride(Pawn p)
		{
			p.jobs.CheckForJobOverride();
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ToggleJobLogging(Pawn p)
		{
			if (p?.jobs != null)
			{
				p.jobs.debugLog = !p.jobs.debugLog;
				DebugActionsUtility.DustPuffFrom(p);
				MoteMaker.ThrowText(p.DrawPosHeld.Value, p.MapHeld, p.LabelShort + "\n" + (p.jobs.debugLog ? "ON" : "OFF"));
			}
		}

		[DebugAction("Pathing", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TogglePathDebugging(Pawn p)
		{
			if (p?.jobs != null)
			{
				p.pather.debugLog = !p.pather.debugLog;
				DebugActionsUtility.DustPuffFrom(p);
				MoteMaker.ThrowText(p.DrawPosHeld.Value, p.MapHeld, p.LabelShort + "\n" + (p.pather.debugLog ? "ON" : "OFF"));
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void LogJobDetails(Pawn p)
		{
			if (p.CurJob != null)
			{
				p.CurJob.LogDetails(p);
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void ToggleStanceLogging(Pawn p)
		{
			p.stances.debugLog = !p.stances.debugLog;
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", "Kidnap colonist", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void Kidnap(Pawn p)
		{
			if (p.IsColonist)
			{
				Faction faction = Find.FactionManager.RandomEnemyFaction();
				faction?.kidnapped.Kidnap(p, faction.leader);
			}
		}

		[DebugAction("Pawns", "Face cell (selected)...", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Selected_SetFacing()
		{
			foreach (object item in Find.Selector.SelectedObjectsListForReading)
			{
				if (item is Pawn pawn)
				{
					pawn.rotationTracker.FaceTarget(UI.MouseCell());
				}
			}
		}

		[DebugAction("Pawns", "Set enemy target for (selected)", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Selected_SetTarget(Pawn p)
		{
			foreach (object item in Find.Selector.SelectedObjectsListForReading)
			{
				if (item is Pawn pawn)
				{
					pawn.mindState.enemyTarget = p;
					pawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
				}
			}
		}

		[DebugAction("Pawns", "Progress life stage", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ProgressLifeStage(Pawn p)
		{
			int curLifeStageIndex = p.ageTracker.CurLifeStageIndex;
			if (curLifeStageIndex < p.ageTracker.MaxRaceLifeStageIndex)
			{
				float minAge = p.RaceProps.lifeStageAges[curLifeStageIndex + 1].minAge;
				float minAge2 = p.RaceProps.lifeStageAges[p.RaceProps.lifeStageAges.Count - 1].minAge;
				if (p.RaceProps.Humanlike)
				{
					p.ageTracker.DebugSetAge((long)minAge * 3600000);
				}
				else
				{
					p.ageTracker.DebugSetGrowth(minAge / minAge2);
				}
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", "Make guilty", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void MakeGuilty(Pawn p)
		{
			p.guilt?.Notify_Guilty();
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", "Force age reversal demand now", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true, requiresIdeology = true)]
		private static void ForceAgeReversalDemandNow(Pawn p)
		{
			p.ageTracker.DebugForceAgeReversalDemandNow();
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", "Reset age reversal demand", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true, requiresIdeology = true)]
		private static void ResetAgeReversalDemandNow(Pawn p)
		{
			p.ageTracker.DebugResetAgeReversalDemand();
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Resurrect()
		{
			foreach (Thing item in UI.MouseCell().GetThingList(Find.CurrentMap).ToList())
			{
				if (item is Corpse corpse)
				{
					ResurrectionUtility.TryResurrect(corpse.InnerPawn);
				}
			}
		}

		public static List<DebugMenuOption> Options_AddGene(Action<GeneDef> callback)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (GeneDef item in DefDatabase<GeneDef>.AllDefs.OrderBy((GeneDef x) => x.defName))
			{
				GeneDef localDef = item;
				list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
				{
					callback(localDef);
				}));
			}
			return list;
		}

		public static List<DebugMenuOption> Options_RemoveGene(Pawn pawn)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			if (pawn.genes != null)
			{
				foreach (Gene item in pawn.genes.GenesListForReading)
				{
					Gene localG = item;
					list.Add(new DebugMenuOption(localG.LabelCap, DebugMenuOptionMode.Action, delegate
					{
						pawn.genes.RemoveGene(localG);
					}));
				}
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true, displayPriority = 1000)]
		private static List<DebugActionNode> AddGene()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			DebugActionNode debugActionNode = new DebugActionNode("Xenogene");
			foreach (DebugActionNode item in GeneOptionNodes(xenogene: true, heritableOnly: false))
			{
				debugActionNode.AddChild(item);
			}
			list.Add(debugActionNode);
			DebugActionNode debugActionNode2 = new DebugActionNode("Endogene");
			foreach (DebugActionNode item2 in GeneOptionNodes(xenogene: false, heritableOnly: false))
			{
				debugActionNode2.AddChild(item2);
			}
			list.Add(debugActionNode2);
			DebugActionNode debugActionNode3 = new DebugActionNode("Heritable");
			foreach (DebugActionNode item3 in GeneOptionNodes(xenogene: false, heritableOnly: true))
			{
				debugActionNode3.AddChild(item3);
			}
			list.Add(debugActionNode3);
			return list;
			static List<DebugActionNode> GeneOptionNodes(bool xenogene, bool heritableOnly)
			{
				List<DebugActionNode> list2 = new List<DebugActionNode>();
				foreach (GeneDef item4 in DefDatabase<GeneDef>.AllDefs.OrderBy((GeneDef x) => x.defName))
				{
					GeneDef localDef = item4;
					if ((xenogene || localDef.biostatArc <= 0) && (!heritableOnly || localDef.endogeneCategory != EndogeneCategory.None || !localDef.forcedTraits.NullOrEmpty() || DefDatabase<XenotypeDef>.AllDefs.Any((XenotypeDef x) => x.genes.Contains(localDef) && x.inheritable)))
					{
						list2.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMapForPawns)
						{
							pawnAction = delegate(Pawn p)
							{
								p.genes?.AddGene(localDef, xenogene);
								DebugActionsUtility.DustPuffFrom(p);
							}
						});
					}
				}
				return list2;
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true, displayPriority = 1000)]
		private static void RemoveGene(Pawn p)
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_RemoveGene(p)));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true, displayPriority = 1000)]
		private static List<DebugActionNode> SetXenotype()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (XenotypeDef item in DefDatabase<XenotypeDef>.AllDefs.OrderBy((XenotypeDef x) => x.defName))
			{
				XenotypeDef localDef = item;
				list.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMapForPawns)
				{
					pawnAction = delegate(Pawn p)
					{
						p.genes?.SetXenotype(localDef);
						DebugActionsUtility.DustPuffFrom(p);
					}
				});
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true)]
		private static List<DebugActionNode> AddLearningDesire()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (LearningDesireDef allDef in DefDatabase<LearningDesireDef>.AllDefs)
			{
				LearningDesireDef defLocal = allDef;
				list.Add(new DebugActionNode(defLocal.defName, DebugActionType.ToolMapForPawns, null, delegate(Pawn p)
				{
					p.learning?.Debug_SetLearningDesire(defLocal);
					DebugActionsUtility.DustPuffFrom(p);
				}));
			}
			return list;
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true)]
		private static void TryLearningGiver(Pawn p)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (LearningDesireDef def in DefDatabase<LearningDesireDef>.AllDefsListForReading)
			{
				list.Add(new DebugMenuOption(def.Worker.CanDo(p) ? def.defName : (def.defName + " [NO]"), DebugMenuOptionMode.Action, delegate
				{
					Job job = def.Worker.TryGiveJob(p);
					if (job != null)
					{
						p.jobs.StartJob(job, JobCondition.InterruptForced);
					}
					else
					{
						Messages.Message("Failed to give job", MessageTypeDefOf.RejectInput, historical: false);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void ListMeleeVerbs(Pawn p)
		{
			List<Verb> allMeleeVerbs = (from x in p.meleeVerbs.GetUpdatedAvailableVerbsList(terrainTools: false)
				select x.verb).ToList();
			float highestWeight = 0f;
			foreach (Verb item in allMeleeVerbs)
			{
				float num = VerbUtility.InitialVerbWeight(item, p);
				if (num > highestWeight)
				{
					highestWeight = num;
				}
			}
			float totalSelectionWeight = 0f;
			foreach (Verb item2 in allMeleeVerbs)
			{
				totalSelectionWeight += VerbUtility.FinalSelectionWeight(item2, p, allMeleeVerbs, highestWeight);
			}
			allMeleeVerbs.SortBy((Verb x) => 0f - VerbUtility.InitialVerbWeight(x, p));
			List<TableDataGetter<Verb>> list = new List<TableDataGetter<Verb>>
			{
				new TableDataGetter<Verb>("verb", (Verb v) => v.ToString().Split('/')[1].TrimEnd(')')),
				new TableDataGetter<Verb>("source", delegate(Verb v)
				{
					if (v.HediffSource != null)
					{
						return v.HediffSource.Label;
					}
					return (v.tool != null) ? v.tool.label : "";
				}),
				new TableDataGetter<Verb>("damage", (Verb v) => v.verbProps.AdjustedMeleeDamageAmount(v, p)),
				new TableDataGetter<Verb>("cooldown", (Verb v) => v.verbProps.AdjustedCooldown(v, p) + "s"),
				new TableDataGetter<Verb>("dmg/sec", (Verb v) => VerbUtility.DPS(v, p)),
				new TableDataGetter<Verb>("armor pen", (Verb v) => v.verbProps.AdjustedArmorPenetration(v, p)),
				new TableDataGetter<Verb>("hediff", delegate(Verb v)
				{
					string text = "";
					if (v.verbProps.meleeDamageDef != null && !v.verbProps.meleeDamageDef.additionalHediffs.NullOrEmpty())
					{
						foreach (DamageDefAdditionalHediff additionalHediff in v.verbProps.meleeDamageDef.additionalHediffs)
						{
							text = text + additionalHediff.hediff.label + " ";
						}
					}
					return text;
				}),
				new TableDataGetter<Verb>("weight", (Verb v) => VerbUtility.InitialVerbWeight(v, p)),
				new TableDataGetter<Verb>("category", (Verb v) => v.GetSelectionCategory(p, highestWeight) switch
				{
					VerbSelectionCategory.Best => "Best".Colorize(Color.green), 
					VerbSelectionCategory.Worst => "Worst".Colorize(Color.grey), 
					_ => "Mid", 
				}),
				new TableDataGetter<Verb>("sel %", (Verb v) => GetSelectionPercent(v).ToStringPercent("F2"))
			};
			DebugTables.MakeTablesDialog(allMeleeVerbs, list.ToArray());
			float GetSelectionPercent(Verb v)
			{
				return VerbUtility.FinalSelectionWeight(v, p, allMeleeVerbs, highestWeight) / totalSelectionWeight;
			}
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void DoVoiceCall(Pawn p)
		{
			p.caller?.DoCall();
		}

		[DebugAction("Pawns", "Force vomit", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void ForceVomit(Pawn p)
		{
			p.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Vomit), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
		}

		[DebugAction("Pawns", "Reset pawn render cache", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void ResetRenderCache(Pawn p)
		{
			p.Drawer.renderer.SetAllGraphicsDirty();
		}

		[DebugAction("Anomaly", "Max ghoul upgrades", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresAnomaly = true)]
		private static void MaxGhoulUpgrades(Pawn p)
		{
			p.health.AddHediff(HediffDefOf.GhoulBarbs, p.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso));
			p.health.AddHediff(HediffDefOf.GhoulPlating, p.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso));
			p.health.AddHediff(HediffDefOf.AdrenalHeart, p.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Heart));
		}

		[DebugAction("Anomaly", "Raise corpse as shambler", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresAnomaly = true)]
		private static void RaiseAsShambler()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
			{
				if (item is Corpse corpse)
				{
					MutantUtility.ResurrectAsShambler(corpse.InnerPawn);
					break;
				}
			}
		}

		[DebugAction("Anomaly", "Set mutant...", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> SetMutant()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (MutantDef mutant in DefDatabase<MutantDef>.AllDefs)
			{
				list.Add(new DebugActionNode(mutant.LabelCap, DebugActionType.ToolMapForPawns, null, delegate(Pawn p)
				{
					MutantUtility.SetPawnAsMutantInstantly(p, mutant);
				}));
			}
			return list;
		}

		[DebugAction("Anomaly", "Revert mutant", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresAnomaly = true)]
		private static void RevertMutant(Pawn p)
		{
			if (p.IsMutant)
			{
				p.mutant.Revert();
				p.mutant = null;
			}
		}

		[DebugAction("Pawns", "Create baby from parents", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true)]
		private static void CreateBabyFromParents()
		{
			DebugTool tool = null;
			Pawn parent1 = null;
			tool = new DebugTool("First parent...", delegate
			{
				parent1 = PawnAt(UI.MouseCell());
				if (parent1 != null)
				{
					DebugTools.curTool = new DebugTool("Second parent...", delegate
					{
						Pawn pawn = PawnAt(UI.MouseCell());
						if (pawn != null)
						{
							PregnancyUtility.ApplyBirthOutcome(RitualOutcomeEffectDefOf.ChildBirth.BestOutcome, 1f, null, null, parent1, pawn, pawn);
						}
						DebugTools.curTool = tool;
					});
				}
			});
			DebugTools.curTool = tool;
			static Pawn PawnAt(IntVec3 c)
			{
				foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(c))
				{
					if (item is Pawn result)
					{
						return result;
					}
				}
				return null;
			}
		}

		[DebugAction("Pawns", "Infection pathway debugger", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void OpenInfectionPathwayDebugger()
		{
			if (Find.WindowStack.TryGetWindow<Dialog_DevInfectionPathways>(out var window))
			{
				window.Close();
			}
			Find.WindowStack.Add(new Dialog_DevInfectionPathways());
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static List<DebugActionNode> AddInfectionPathway()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (InfectionPathwayDef def in DefDatabase<InfectionPathwayDef>.AllDefs)
			{
				string label = ((!def.PawnRequired) ? def.defName : (def.defName + " [NO]"));
				list.Add(new DebugActionNode(label, DebugActionType.ToolMapForPawns, null, delegate(Pawn p)
				{
					if (!def.PawnRequired)
					{
						p.infectionVectors.AddInfectionVector(def);
						SoundDefOf.Click.PlayOneShotOnCamera();
					}
				}));
			}
			return list;
		}

		[DebugAction("Pawns", "Destroy factionless animals", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DestroyAnimals()
		{
			List<Pawn> list = new List<Pawn>();
			list.AddRange(Find.CurrentMap.mapPawns.AllPawnsSpawned);
			foreach (Pawn item in list)
			{
				if (item.RaceProps.Animal && item.Faction == null)
				{
					item.Destroy();
				}
			}
		}

		[DebugAction("Pawns", "Destroy player animals", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DestroyPlayerAnimals()
		{
			List<Pawn> list = new List<Pawn>();
			list.AddRange(Find.CurrentMap.mapPawns.SpawnedColonyAnimals);
			foreach (Pawn item in list)
			{
				item.Destroy();
			}
		}

		[DebugAction("Pawns", "Destroy non-colonists", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DestroyAllNonColonists()
		{
			IReadOnlyList<Pawn> allPawnsSpawned = Find.CurrentMap.mapPawns.AllPawnsSpawned;
			for (int num = allPawnsSpawned.Count - 1; num >= 0; num--)
			{
				Pawn pawn = allPawnsSpawned[num];
				if (!pawn.IsColonist || !pawn.RaceProps.Humanlike)
				{
					pawn.Destroy();
				}
			}
		}

		[DebugAction("Other", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, hideInSubMenu = true)]
		private static void GarbageCollectWorldPawn()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			WorldPawnSituation[] array = (WorldPawnSituation[])Enum.GetValues(typeof(WorldPawnSituation));
			for (int i = 0; i < array.Length; i++)
			{
				WorldPawnSituation worldPawnSituation = array[i];
				if (worldPawnSituation == WorldPawnSituation.None)
				{
					continue;
				}
				foreach (Pawn item in from x in Find.WorldPawns.GetPawnsBySituation(worldPawnSituation)
					orderby x.Faction?.loadID ?? (-1)
					select x)
				{
					if (item.relations != null)
					{
						Pawn local = item;
						list.Add(new DebugMenuOption(item.LabelShort + " [" + worldPawnSituation.ToString() + "]", DebugMenuOptionMode.Action, delegate
						{
							local.markedForDiscard = true;
							Find.WorldPawns.RemoveAndDiscardPawnViaGC(local);
							int num = Find.RelationshipRecords.CleanupUnusedRecords();
							Log.Message($"{num} records found and removed.");
						}));
					}
				}
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void UnlockAllApparel(Pawn p)
		{
			p.apparel.UnlockAll();
		}

		[DebugAction("Pawns", "Try make animal fish for food", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryFishForFood(Pawn p)
		{
			if (!p.IsAnimal)
			{
				Messages.Message("Must select animal to fish for food.", MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			Job job = JobGiver_GetFood.TryFindFishJob(p);
			if (job != null)
			{
				p.jobs.StartJob(job, JobCondition.InterruptForced);
			}
			else
			{
				Messages.Message("Unable to find fish job", MessageTypeDefOf.RejectInput, historical: false);
			}
		}
	}
}
