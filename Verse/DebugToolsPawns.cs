using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public static class DebugToolsPawns
	{
		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Resurrect()
		{
			foreach (Thing item in UI.MouseCell().GetThingList(Find.CurrentMap).ToList())
			{
				Corpse corpse = item as Corpse;
				if (corpse != null)
				{
					ResurrectionUtility.Resurrect(corpse.InnerPawn);
				}
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DamageUntilDown(Pawn p)
		{
			HealthUtility.DamageUntilDowned(p);
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DamageLegs(Pawn p)
		{
			HealthUtility.DamageLegsUntilIncapableOfMoving(p);
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DamageToDeath(Pawn p)
		{
			HealthUtility.DamageUntilDead(p);
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void CarriedDamageToDeath(Pawn p)
		{
			HealthUtility.DamageUntilDead(p.carryTracker.CarriedThing as Pawn);
		}

		[DebugAction("Pawns", "10 damage until dead", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Do10DamageUntilDead()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				for (int i = 0; i < 1000; i++)
				{
					item.TakeDamage(new DamageInfo(DamageDefOf.Crush, 10f));
					if (item.Destroyed)
					{
						string str = "Took " + (i + 1) + " hits";
						Pawn pawn = item as Pawn;
						if (pawn != null)
						{
							if (pawn.health.ShouldBeDeadFromLethalDamageThreshold())
							{
								str = str + " (reached lethal damage threshold of " + pawn.health.LethalDamageThreshold.ToString("0.#") + ")";
							}
							else if (PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, pawn.RaceProps.body.corePart) <= 0.0001f)
							{
								str += " (core part hp reached 0)";
							}
							else
							{
								PawnCapacityDef pawnCapacityDef = pawn.health.ShouldBeDeadFromRequiredCapacity();
								if (pawnCapacityDef != null)
								{
									str = str + " (incapable of " + pawnCapacityDef.defName + ")";
								}
							}
						}
						Log.Message(str + ".");
						break;
					}
				}
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DamageHeldPawnToDeath()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				Pawn pawn = item as Pawn;
				if (pawn != null && pawn.carryTracker.CarriedThing != null && pawn.carryTracker.CarriedThing is Pawn)
				{
					HealthUtility.DamageUntilDead((Pawn)pawn.carryTracker.CarriedThing);
				}
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SurgeryFailMinor(Pawn p)
		{
			BodyPartRecord bodyPartRecord = (from x in p.health.hediffSet.GetNotMissingParts()
				where !x.def.conceptual
				select x).RandomElement();
			Log.Message("part is " + bodyPartRecord);
			HealthUtility.GiveInjuriesOperationFailureMinor(p, bodyPartRecord);
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SurgeryFailCatastrophic(Pawn p)
		{
			BodyPartRecord bodyPartRecord = (from x in p.health.hediffSet.GetNotMissingParts()
				where !x.def.conceptual
				select x).RandomElement();
			Log.Message("part is " + bodyPartRecord);
			HealthUtility.GiveInjuriesOperationFailureCatastrophic(p, bodyPartRecord);
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SurgeryFailRidiculous(Pawn p)
		{
			HealthUtility.GiveInjuriesOperationFailureRidiculous(p);
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RestoreBodyPart(Pawn p)
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_RestorePart(p)));
		}

		[DebugAction("Pawns", "Apply damage...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ApplyDamage()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_ApplyDamage()));
		}

		[DebugAction("Pawns", "Add Hediff...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AddHediff()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_AddHediff()));
		}

		[DebugAction("Pawns", "Remove Hediff...", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RemoveHediff(Pawn p)
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_RemoveHediff(p)));
		}

		[DebugAction("Pawns", "Heal random injury (10)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void HealRandomInjury10(Pawn p)
		{
			if ((from x in p.health.hediffSet.GetHediffs<Hediff_Injury>()
				where x.CanHealNaturally() || x.CanHealFromTending()
				select x).TryRandomElement(out Hediff_Injury result))
			{
				result.Heal(10f);
			}
		}

		[DebugAction("Pawns", "Activate HediffGiver", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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
						localHdg.TryApply(p);
					}));
				}
			}
			if (list.Any())
			{
				Find.WindowStack.Add(new FloatMenu(list));
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GiveBirth(Pawn p)
		{
			Hediff_Pregnant.DoBirthSpawn(p, null);
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", "Resistance -1", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ResistanceMinus1(Pawn p)
		{
			if (p.guest != null && p.guest.resistance > 0f)
			{
				p.guest.resistance = Mathf.Max(0f, p.guest.resistance - 1f);
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", "Resistance -10", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ResistanceMinus10(Pawn p)
		{
			if (p.guest != null && p.guest.resistance > 0f)
			{
				p.guest.resistance = Mathf.Max(0f, p.guest.resistance - 10f);
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", "+20 psychic entropy", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AddPsychicEntropy()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				if (item is Pawn)
				{
					((Pawn)item).psychicEntropy.TryAddEntropy(20f);
				}
			}
		}

		[DebugAction("Pawns", "-20 psychic entropy", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ReducePsychicEntropy()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				if (item is Pawn)
				{
					((Pawn)item).psychicEntropy.TryAddEntropy(-20f);
				}
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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
						return text;
					}
					return text;
				}),
				new TableDataGetter<Verb>("weight", (Verb v) => VerbUtility.InitialVerbWeight(v, p)),
				new TableDataGetter<Verb>("category", delegate(Verb v)
				{
					switch (v.GetSelectionCategory(p, highestWeight))
					{
					case VerbSelectionCategory.Best:
						return "Best".Colorize(Color.green);
					case VerbSelectionCategory.Worst:
						return "Worst".Colorize(Color.grey);
					default:
						return "Mid";
					}
				}),
				new TableDataGetter<Verb>("sel %", (Verb v) => GetSelectionPercent(v).ToStringPercent("F2"))
			};
			DebugTables.MakeTablesDialog(allMeleeVerbs, list.ToArray());
			float GetSelectionPercent(Verb v)
			{
				return VerbUtility.FinalSelectionWeight(v, p, allMeleeVerbs, highestWeight) / totalSelectionWeight;
			}
		}

		[DebugAction("Pawns", "Add/remove pawn relation", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AddRemovePawnRelation(Pawn p)
		{
			if (p.RaceProps.IsFlesh)
			{
				PawnRelationDef defLocal = default(PawnRelationDef);
				Pawn otherLocal = default(Pawn);
				Action<bool> act = delegate(bool add)
				{
					if (add)
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (PawnRelationDef allDef in DefDatabase<PawnRelationDef>.AllDefs)
						{
							if (!allDef.implied)
							{
								defLocal = allDef;
								list2.Add(new DebugMenuOption(defLocal.defName, DebugMenuOptionMode.Action, delegate
								{
									List<DebugMenuOption> list4 = new List<DebugMenuOption>();
									foreach (Pawn item in from x in PawnsFinder.AllMapsWorldAndTemporary_Alive
										where x.RaceProps.IsFlesh
										orderby x.def == p.def descending, x.IsWorldPawn()
										select x)
									{
										if (p != item && (!defLocal.familyByBloodRelation || item.def == p.def) && !p.relations.DirectRelationExists(defLocal, item))
										{
											otherLocal = item;
											list4.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
											{
												p.relations.AddDirectRelation(defLocal, otherLocal);
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
						DirectPawnRelation rel = default(DirectPawnRelation);
						for (int i = 0; i < directRelations.Count; i++)
						{
							rel = directRelations[i];
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
				list.Add(new DebugMenuOption("Remove", DebugMenuOptionMode.Action, delegate
				{
					act(obj: false);
				}));
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AddOpinionTalksAbout(Pawn p)
		{
			if (p.RaceProps.Humanlike)
			{
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
								for (int i = 0; i < num; i++)
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
		}

		[DebugAction("Pawns", "Force vomit...", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceVomit(Pawn p)
		{
			p.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Vomit), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
		}

		[DebugAction("Pawns", "Authority -20%", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void OffsetAuthortyNegative20()
		{
			OffsetNeed(DefDatabase<NeedDef>.GetNamed("Authority"), -0.2f);
		}

		[DebugAction("Pawns", "Food -20%", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void OffsetFoodNegative20()
		{
			OffsetNeed(NeedDefOf.Food, -0.2f);
		}

		[DebugAction("Pawns", "Rest -20%", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void OffsetRestNegative20()
		{
			OffsetNeed(NeedDefOf.Rest, -0.2f);
		}

		[DebugAction("Pawns", "Joy -20%", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void OffsetJoyNegative20()
		{
			OffsetNeed(NeedDefOf.Joy, -0.2f);
		}

		[DebugAction("Pawns", "Chemical -20%", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void OffsetChemicalNegative20()
		{
			List<NeedDef> allDefsListForReading = DefDatabase<NeedDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (typeof(Need_Chemical).IsAssignableFrom(allDefsListForReading[i].needClass))
				{
					OffsetNeed(allDefsListForReading[i], -0.2f);
				}
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SetSkill()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
			{
				SkillDef localDef = allDef;
				list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					int level = default(int);
					for (int i = 0; i <= 20; i++)
					{
						level = i;
						list2.Add(new DebugMenuOption(level.ToString(), DebugMenuOptionMode.Tool, delegate
						{
							Pawn pawn = (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
								where t is Pawn
								select t).Cast<Pawn>().FirstOrDefault();
							if (pawn != null)
							{
								SkillRecord skill = pawn.skills.GetSkill(localDef);
								skill.Level = level;
								skill.xpSinceLastLevel = skill.XpRequiredForLevelUp / 2f;
								DebugActionsUtility.DustPuffFrom(pawn);
							}
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MaxSkill(Pawn p)
		{
			if (p.skills != null)
			{
				foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
				{
					p.skills.Learn(allDef, 1E+08f);
				}
				DebugActionsUtility.DustPuffFrom(p);
			}
			if (p.training != null)
			{
				foreach (TrainableDef allDef2 in DefDatabase<TrainableDef>.AllDefs)
				{
					Pawn trainer = p.Map.mapPawns.FreeColonistsSpawned.RandomElement();
					if (p.training.CanAssignToTrain(allDef2, out bool _).Accepted)
					{
						p.training.Train(allDef2, trainer);
					}
				}
			}
		}

		[DebugAction("Pawns", "Mental break...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MentalBreak()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("(log possibles)", DebugMenuOptionMode.Tool, delegate
			{
				foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
					where t is Pawn
					select t).Cast<Pawn>())
				{
					item.mindState.mentalBreaker.LogPossibleMentalBreaks();
					DebugActionsUtility.DustPuffFrom(item);
				}
			}));
			list.Add(new DebugMenuOption("(natural mood break)", DebugMenuOptionMode.Tool, delegate
			{
				foreach (Pawn item2 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
					where t is Pawn
					select t).Cast<Pawn>())
				{
					item2.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
					DebugActionsUtility.DustPuffFrom(item2);
				}
			}));
			foreach (MentalBreakDef item3 in DefDatabase<MentalBreakDef>.AllDefs.OrderByDescending((MentalBreakDef x) => x.intensity))
			{
				MentalBreakDef locBrDef = item3;
				string text = locBrDef.defName;
				if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => locBrDef.Worker.BreakCanOccur(x)))
				{
					text += " [NO]";
				}
				list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Tool, delegate
				{
					foreach (Pawn item4 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
					{
						locBrDef.Worker.TryStart(item4, null, causedByMood: false);
						DebugActionsUtility.DustPuffFrom(item4);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", "Mental state...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MentalState()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (MentalStateDef allDef in DefDatabase<MentalStateDef>.AllDefs)
			{
				MentalStateDef locBrDef = allDef;
				string text = locBrDef.defName;
				if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => locBrDef.Worker.StateCanOccur(x)))
				{
					text += " [NO]";
				}
				Pawn locP = default(Pawn);
				list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Tool, delegate
				{
					foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
					{
						locP = item;
						if (locBrDef != MentalStateDefOf.SocialFighting)
						{
							locP.mindState.mentalStateHandler.TryStartMentalState(locBrDef, null, forceWake: true);
							DebugActionsUtility.DustPuffFrom(locP);
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
									locP.interactions.StartSocialFight(pawn);
									DebugTools.curTool = null;
								}
							});
						}
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", "Inspiration...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Inspiration()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (InspirationDef allDef in DefDatabase<InspirationDef>.AllDefs)
			{
				InspirationDef localDef = allDef;
				string text = localDef.defName;
				if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => localDef.Worker.InspirationCanOccur(x)))
				{
					text += " [NO]";
				}
				list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Tool, delegate
				{
					foreach (Pawn item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>())
					{
						item.mindState.inspirationHandler.TryStartInspiration(localDef);
						DebugActionsUtility.DustPuffFrom(item);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", "Give trait...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GiveTrait()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (TraitDef allDef in DefDatabase<TraitDef>.AllDefs)
			{
				TraitDef trDef = allDef;
				for (int j = 0; j < allDef.degreeDatas.Count; j++)
				{
					int i = j;
					list.Add(new DebugMenuOption(trDef.degreeDatas[i].label + " (" + trDef.degreeDatas[j].degree + ")", DebugMenuOptionMode.Tool, delegate
					{
						foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
							where t is Pawn
							select t).Cast<Pawn>())
						{
							if (item.story != null)
							{
								Trait trait = new Trait(trDef, trDef.degreeDatas[i].degree);
								item.story.traits.GainTrait(trait);
								DebugActionsUtility.DustPuffFrom(item);
							}
						}
					}));
				}
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", "Give ability...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GiveAbility()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("*All", DebugMenuOptionMode.Tool, delegate
			{
				foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
					where t is Pawn
					select t).Cast<Pawn>())
				{
					foreach (AbilityDef allDef in DefDatabase<AbilityDef>.AllDefs)
					{
						item.abilities.GainAbility(allDef);
					}
				}
			}));
			foreach (AbilityDef allDef2 in DefDatabase<AbilityDef>.AllDefs)
			{
				AbilityDef localAb = allDef2;
				list.Add(new DebugMenuOption(allDef2.label, DebugMenuOptionMode.Tool, delegate
				{
					foreach (Pawn item2 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
					{
						item2.abilities.GainAbility(localAb);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", "Give PsychicAmplifier...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GivePsychicAmplifier()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			for (int i = 1; i <= 6; i++)
			{
				int level = i;
				list.Add(new DebugMenuOption((string)("Level".Translate() + ": ") + i, DebugMenuOptionMode.Tool, delegate
				{
					foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
					{
						Hediff_ImplantWithLevel hediff_ImplantWithLevel = item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier) as Hediff_ImplantWithLevel;
						if (hediff_ImplantWithLevel == null)
						{
							hediff_ImplantWithLevel = (HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, item, item.health.hediffSet.GetBrain()) as Hediff_ImplantWithLevel);
							item.health.AddHediff(hediff_ImplantWithLevel);
						}
						hediff_ImplantWithLevel.ChangeLevel(level - hediff_ImplantWithLevel.level);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", "Remove psychic entropy", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RemovePsychicEntropy(Pawn p)
		{
			if (p.psychicEntropy != null)
			{
				p.psychicEntropy.TryAddEntropy(-1000f);
			}
		}

		[DebugAction("Pawns", "Give good thought...", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GiveGoodThought(Pawn p)
		{
			if (p.needs.mood != null)
			{
				p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DebugGood);
			}
		}

		[DebugAction("Pawns", "Give bad thought...", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GiveBadThought(Pawn p)
		{
			if (p.needs.mood != null)
			{
				p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DebugBad);
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ClearBoundUnfinishedThings()
		{
			foreach (Building_WorkTable item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
				where t is Building_WorkTable
				select t).Cast<Building_WorkTable>())
			{
				foreach (Bill item2 in item.BillStack)
				{
					(item2 as Bill_ProductionWithUft)?.ClearBoundUft();
				}
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceBirthday(Pawn p)
		{
			p.ageTracker.AgeBiologicalTicks = (p.ageTracker.AgeBiologicalYears + 1) * 3600000 + 1;
			p.ageTracker.DebugForceBirthdayBiological();
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Recruit(Pawn p)
		{
			if (p.Faction != Faction.OfPlayer && p.RaceProps.Humanlike)
			{
				InteractionWorker_RecruitAttempt.DoRecruit(p.Map.mapPawns.FreeColonists.RandomElement(), p, 1f);
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DamageApparel(Pawn p)
		{
			if (p.apparel != null && p.apparel.WornApparelCount > 0)
			{
				p.apparel.WornApparel.RandomElement().TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 30f));
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TameAnimal(Pawn p)
		{
			if (p.AnimalOrWildMan() && p.Faction != Faction.OfPlayer)
			{
				InteractionWorker_RecruitAttempt.DoRecruit(p.Map.mapPawns.FreeColonists.FirstOrDefault(), p, 1f);
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TrainAnimal(Pawn p)
		{
			if (p.RaceProps.Animal && p.Faction == Faction.OfPlayer && p.training != null)
			{
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
				if (!flag)
				{
					foreach (TrainableDef allDef2 in DefDatabase<TrainableDef>.AllDefs)
					{
						if (p.training.CanAssignToTrain(allDef2).Accepted)
						{
							p.training.Train(allDef2, null, complete: true);
						}
					}
				}
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void NameAnimalByNuzzling(Pawn p)
		{
			if ((p.Name == null || p.Name.Numerical) && p.RaceProps.Animal)
			{
				PawnUtility.GiveNameBecauseOfNuzzle(p.Map.mapPawns.FreeColonists.First(), p);
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryDevelopBoundRelation(Pawn p)
		{
			if (p.Faction == null)
			{
				return;
			}
			if (p.RaceProps.Humanlike)
			{
				IEnumerable<Pawn> source = p.Map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.RaceProps.Animal && x.Faction == p.Faction);
				if (source.Any())
				{
					RelationsUtility.TryDevelopBondRelation(p, source.RandomElement(), 999999f);
				}
			}
			else if (p.RaceProps.Animal)
			{
				IEnumerable<Pawn> source2 = p.Map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.RaceProps.Humanlike && x.Faction == p.Faction);
				if (source2.Any())
				{
					RelationsUtility.TryDevelopBondRelation(source2.RandomElement(), p, 999999f);
				}
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void QueueTrainingDecay(Pawn p)
		{
			if (p.RaceProps.Animal && p.Faction == Faction.OfPlayer && p.training != null)
			{
				p.training.Debug_MakeDegradeHappenSoon();
				DebugActionsUtility.DustPuffFrom(p);
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void StartMarriageCeremony(Pawn p)
		{
			if (p.RaceProps.Humanlike)
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Pawn item in p.Map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.RaceProps.Humanlike))
				{
					if (p != item)
					{
						Pawn otherLocal = item;
						list.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
						{
							if (!p.relations.DirectRelationExists(PawnRelationDefOf.Fiance, otherLocal))
							{
								p.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherLocal);
								p.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherLocal);
								p.relations.AddDirectRelation(PawnRelationDefOf.Fiance, otherLocal);
								Messages.Message("Dev: Auto added fiance relation.", p, MessageTypeDefOf.TaskCompletion, historical: false);
							}
							if (!p.Map.lordsStarter.TryStartMarriageCeremony(p, otherLocal))
							{
								Messages.Message("Could not find any valid marriage site.", MessageTypeDefOf.RejectInput, historical: false);
							}
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceInteraction(Pawn p)
		{
			if (p.Faction != null)
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Pawn item in p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction))
				{
					if (item != p)
					{
						Pawn otherLocal = item;
						InteractionDef interactionLocal = default(InteractionDef);
						list.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
						{
							List<DebugMenuOption> list2 = new List<DebugMenuOption>();
							foreach (InteractionDef item2 in DefDatabase<InteractionDef>.AllDefsListForReading)
							{
								interactionLocal = item2;
								list2.Add(new DebugMenuOption(interactionLocal.label, DebugMenuOptionMode.Action, delegate
								{
									p.interactions.TryInteractWith(otherLocal, interactionLocal);
								}));
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void StartRandomGathering()
		{
			if (!Find.CurrentMap.lordsStarter.TryStartRandomGathering(forceStart: true))
			{
				Messages.Message("Could not find any valid gathering spot or organizer.", MessageTypeDefOf.RejectInput, historical: false);
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void StartGathering()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (GatheringDef gatheringDef in DefDatabase<GatheringDef>.AllDefsListForReading)
			{
				list.Add(new DebugMenuOption(gatheringDef.LabelCap + " (" + (gatheringDef.Worker.CanExecute(Find.CurrentMap) ? "Yes" : "No") + ")", DebugMenuOptionMode.Action, delegate
				{
					gatheringDef.Worker.TryExecute(Find.CurrentMap);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void StartPrisonBreak(Pawn p)
		{
			if (p.IsPrisoner)
			{
				PrisonBreakUtility.StartPrisonBreak(p);
			}
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PassToWorld(Pawn p)
		{
			p.DeSpawn();
			Find.WorldPawns.PassToWorld(p, PawnDiscardDecideMode.KeepForever);
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Make1YearOlder(Pawn p)
		{
			p.ageTracker.DebugMake1YearOlder();
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("Pawns", "EndCurrentJob(InterruptForced)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void EndCurrentJobInterruptForced(Pawn p)
		{
			p.jobs.EndCurrentJob(JobCondition.InterruptForced);
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", "CheckForJobOverride", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void CheckForJobOverride(Pawn p)
		{
			p.jobs.CheckForJobOverride();
			DebugActionsUtility.DustPuffFrom(p);
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ToggleJobLogging(Pawn p)
		{
			p.jobs.debugLog = !p.jobs.debugLog;
			DebugActionsUtility.DustPuffFrom(p);
			MoteMaker.ThrowText(p.DrawPos, p.Map, p.LabelShort + "\n" + (p.jobs.debugLog ? "ON" : "OFF"));
		}

		[DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ToggleStanceLogging(Pawn p)
		{
			p.stances.debugLog = !p.stances.debugLog;
			DebugActionsUtility.DustPuffFrom(p);
		}

		private static void OffsetNeed(NeedDef nd, float offsetPct)
		{
			foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
				where t is Pawn
				select t).Cast<Pawn>())
			{
				Need need = item.needs.TryGetNeed(nd);
				if (need != null)
				{
					need.CurLevel += offsetPct * need.MaxLevel;
					DebugActionsUtility.DustPuffFrom(item);
				}
			}
		}

		[DebugAction("Pawns", "Kidnap colonist", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Kidnap(Pawn p)
		{
			if (p.IsColonist)
			{
				Faction faction = Find.FactionManager.RandomEnemyFaction();
				faction?.kidnapped.Kidnap(p, faction.leader);
			}
		}
	}
}
