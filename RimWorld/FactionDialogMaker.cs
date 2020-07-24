using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class FactionDialogMaker
	{
		public static DiaNode FactionDialogFor(Pawn negotiator, Faction faction)
		{
			Map map = negotiator.Map;
			Pawn pawn;
			string value;
			if (faction.leader != null)
			{
				pawn = faction.leader;
				value = faction.leader.Name.ToStringFull.Colorize(ColoredText.NameColor);
			}
			else
			{
				Log.Error(string.Concat("Faction ", faction, " has no leader."));
				pawn = negotiator;
				value = faction.Name;
			}
			DiaNode root;
			if (faction.PlayerRelationKind == FactionRelationKind.Hostile)
			{
				string key = (faction.def.permanentEnemy || !"FactionGreetingHostileAppreciative".CanTranslate()) ? "FactionGreetingHostile" : "FactionGreetingHostileAppreciative";
				root = new DiaNode(key.Translate(value).AdjustedFor(pawn));
			}
			else if (faction.PlayerRelationKind == FactionRelationKind.Neutral)
			{
				root = new DiaNode("FactionGreetingWary".Translate(value, negotiator.LabelShort, negotiator.Named("NEGOTIATOR"), pawn.Named("LEADER")).AdjustedFor(pawn));
			}
			else
			{
				root = new DiaNode("FactionGreetingWarm".Translate(value, negotiator.LabelShort, negotiator.Named("NEGOTIATOR"), pawn.Named("LEADER")).AdjustedFor(pawn));
			}
			if (map != null && map.IsPlayerHome)
			{
				AddAndDecorateOption(RequestTraderOption(map, faction, negotiator), needsSocial: true);
				AddAndDecorateOption(RequestMilitaryAidOption(map, faction, negotiator), needsSocial: true);
				Pawn_RoyaltyTracker royalty = negotiator.royalty;
				if (royalty != null && royalty.HasAnyTitleIn(faction))
				{
					foreach (RoyalTitle item in royalty.AllTitlesInEffectForReading)
					{
						if (item.def.permits == null)
						{
							continue;
						}
						foreach (RoyalTitlePermitDef permit in item.def.permits)
						{
							IEnumerable<DiaOption> factionCommDialogOptions = permit.Worker.GetFactionCommDialogOptions(map, negotiator, faction);
							if (factionCommDialogOptions == null)
							{
								continue;
							}
							foreach (DiaOption item2 in factionCommDialogOptions)
							{
								AddAndDecorateOption(item2, needsSocial: true);
							}
						}
					}
					if (royalty.GetCurrentTitle(faction).canBeInherited && !negotiator.IsQuestLodger())
					{
						AddAndDecorateOption(RequestRoyalHeirChangeOption(map, faction, pawn, negotiator), needsSocial: false);
					}
				}
				if (DefDatabase<ResearchProjectDef>.AllDefsListForReading.Any((ResearchProjectDef rp) => rp.HasTag(ResearchProjectTagDefOf.ShipRelated) && rp.IsFinished))
				{
					AddAndDecorateOption(RequestAICoreQuest(map, faction, negotiator), needsSocial: true);
				}
			}
			if (Prefs.DevMode)
			{
				foreach (DiaOption item3 in DebugOptions(faction, negotiator))
				{
					AddAndDecorateOption(item3, needsSocial: false);
				}
			}
			AddAndDecorateOption(new DiaOption("(" + "Disconnect".Translate() + ")")
			{
				resolveTree = true
			}, needsSocial: false);
			return root;
			void AddAndDecorateOption(DiaOption opt, bool needsSocial)
			{
				if (needsSocial && negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
				{
					opt.Disable("WorkTypeDisablesOption".Translate(SkillDefOf.Social.label));
				}
				root.options.Add(opt);
			}
		}

		private static IEnumerable<DiaOption> DebugOptions(Faction faction, Pawn negotiator)
		{
			Faction faction2 = faction;
			Pawn negotiator2 = negotiator;
			DiaOption diaOption = new DiaOption("(Debug) Goodwill +10");
			diaOption.action = delegate
			{
				faction2.TryAffectGoodwillWith(Faction.OfPlayer, 10, canSendMessage: false);
			};
			diaOption.linkLateBind = (() => FactionDialogFor(negotiator2, faction2));
			yield return diaOption;
			DiaOption diaOption2 = new DiaOption("(Debug) Goodwill -10");
			diaOption2.action = delegate
			{
				faction2.TryAffectGoodwillWith(Faction.OfPlayer, -10, canSendMessage: false);
			};
			diaOption2.linkLateBind = (() => FactionDialogFor(negotiator2, faction2));
			yield return diaOption2;
		}

		private static int AmountSendableSilver(Map map)
		{
			return (from t in TradeUtility.AllLaunchableThingsForTrade(map)
				where t.def == ThingDefOf.Silver
				select t).Sum((Thing t) => t.stackCount);
		}

		private static DiaOption RequestAICoreQuest(Map map, Faction faction, Pawn negotiator)
		{
			TaggedString taggedString = "RequestAICoreInformation".Translate(ThingDefOf.AIPersonaCore.label, 1500.ToString());
			if (faction.PlayerGoodwill < 40)
			{
				DiaOption diaOption = new DiaOption(taggedString);
				diaOption.Disable("NeedGoodwill".Translate(40.ToString("F0")));
				return diaOption;
			}
			bool num = PlayerItemAccessibilityUtility.ItemStashHas(ThingDefOf.AIPersonaCore);
			Slate slate = new Slate();
			slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(Find.World));
			slate.Set("asker", faction.leader);
			slate.Set("itemStashSingleThing", ThingDefOf.AIPersonaCore);
			bool flag = QuestScriptDefOf.OpportunitySite_ItemStash.CanRun(slate);
			if (num || !flag)
			{
				DiaOption diaOption2 = new DiaOption(taggedString);
				diaOption2.Disable("NoKnownAICore".Translate(1500));
				return diaOption2;
			}
			if (AmountSendableSilver(map) < 1500)
			{
				DiaOption diaOption3 = new DiaOption(taggedString);
				diaOption3.Disable("NeedSilverLaunchable".Translate(1500));
				return diaOption3;
			}
			return new DiaOption(taggedString)
			{
				action = delegate
				{
					QuestUtility.SendLetterQuestAvailable(QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.OpportunitySite_ItemStash, slate));
					TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, 1500, map, null);
					Current.Game.GetComponent<GameComponent_OnetimeNotification>().sendAICoreRequestReminder = false;
				},
				link = new DiaNode("RequestAICoreInformationResult".Translate(faction.leader).CapitalizeFirst())
				{
					options = 
					{
						OKToRoot(faction, negotiator)
					}
				}
			};
		}

		private static DiaOption RequestTraderOption(Map map, Faction faction, Pawn negotiator)
		{
			TaggedString taggedString = "RequestTrader".Translate(15);
			if (faction.PlayerRelationKind != FactionRelationKind.Ally)
			{
				DiaOption diaOption = new DiaOption(taggedString);
				diaOption.Disable("MustBeAlly".Translate());
				return diaOption;
			}
			if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
			{
				DiaOption diaOption2 = new DiaOption(taggedString);
				diaOption2.Disable("BadTemperature".Translate());
				return diaOption2;
			}
			int num = faction.lastTraderRequestTick + 240000 - Find.TickManager.TicksGame;
			if (num > 0)
			{
				DiaOption diaOption3 = new DiaOption(taggedString);
				diaOption3.Disable("WaitTime".Translate(num.ToStringTicksToPeriod()));
				return diaOption3;
			}
			DiaOption diaOption4 = new DiaOption(taggedString);
			DiaNode diaNode = new DiaNode("TraderSent".Translate(faction.leader).CapitalizeFirst());
			diaNode.options.Add(OKToRoot(faction, negotiator));
			DiaNode diaNode2 = new DiaNode("ChooseTraderKind".Translate(faction.leader));
			foreach (TraderKindDef item in faction.def.caravanTraderKinds.Where((TraderKindDef x) => x.requestable))
			{
				TraderKindDef localTk = item;
				DiaOption diaOption5 = new DiaOption(localTk.LabelCap);
				if (localTk.TitleRequiredToTrade != null && (negotiator.royalty == null || localTk.TitleRequiredToTrade.seniority > negotiator.GetCurrentTitleSeniorityIn(faction)))
				{
					DiaNode diaNode3 = new DiaNode("TradeCaravanRequestDeniedDueTitle".Translate(negotiator.Named("NEGOTIATOR"), localTk.TitleRequiredToTrade.GetLabelCapFor(negotiator).Named("TITLE"), faction.Named("FACTION")));
					DiaOption diaOption6 = new DiaOption("GoBack".Translate());
					diaNode3.options.Add(diaOption6);
					diaOption5.link = diaNode3;
					diaOption6.link = diaNode2;
				}
				else
				{
					diaOption5.action = delegate
					{
						IncidentParms parms = new IncidentParms
						{
							target = map,
							faction = faction,
							traderKind = localTk,
							forced = true
						};
						Find.Storyteller.incidentQueue.Add(IncidentDefOf.TraderCaravanArrival, Find.TickManager.TicksGame + 120000, parms, 240000);
						faction.lastTraderRequestTick = Find.TickManager.TicksGame;
						faction.TryAffectGoodwillWith(Faction.OfPlayer, -15, canSendMessage: false, canSendHostilityLetter: true, "GoodwillChangedReason_RequestedTrader".Translate());
					};
					diaOption5.link = diaNode;
				}
				diaNode2.options.Add(diaOption5);
			}
			DiaOption diaOption7 = new DiaOption("GoBack".Translate());
			diaOption7.linkLateBind = ResetToRoot(faction, negotiator);
			diaNode2.options.Add(diaOption7);
			diaOption4.link = diaNode2;
			return diaOption4;
		}

		private static DiaOption RequestMilitaryAidOption(Map map, Faction faction, Pawn negotiator)
		{
			string text = "RequestMilitaryAid".Translate(25);
			if (faction.PlayerRelationKind != FactionRelationKind.Ally)
			{
				DiaOption diaOption = new DiaOption(text);
				diaOption.Disable("MustBeAlly".Translate());
				return diaOption;
			}
			if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
			{
				DiaOption diaOption2 = new DiaOption(text);
				diaOption2.Disable("BadTemperature".Translate());
				return diaOption2;
			}
			int num = faction.lastMilitaryAidRequestTick + 60000 - Find.TickManager.TicksGame;
			if (num > 0)
			{
				DiaOption diaOption3 = new DiaOption(text);
				diaOption3.Disable("WaitTime".Translate(num.ToStringTicksToPeriod()));
				return diaOption3;
			}
			if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, faction))
			{
				DiaOption diaOption4 = new DiaOption(text);
				diaOption4.Disable("HostileVisitorsPresent".Translate());
				return diaOption4;
			}
			DiaOption diaOption5 = new DiaOption(text);
			if ((int)faction.def.techLevel < 4)
			{
				diaOption5.link = CantMakeItInTime(faction, negotiator);
			}
			else
			{
				IEnumerable<Faction> source = (from x in map.attackTargetsCache.TargetsHostileToColony
					where GenHostility.IsActiveThreatToPlayer(x)
					select ((Thing)x).Faction into x
					where x != null && !x.HostileTo(faction)
					select x).Distinct();
				if (source.Any())
				{
					DiaNode diaNode = new DiaNode("MilitaryAidConfirmMutualEnemy".Translate(faction.Name, source.Select((Faction fa) => fa.Name).ToCommaList(useAnd: true)));
					DiaOption diaOption6 = new DiaOption("CallConfirm".Translate());
					diaOption6.action = delegate
					{
						CallForAid(map, faction);
					};
					diaOption6.link = FightersSent(faction, negotiator);
					DiaOption diaOption7 = new DiaOption("CallCancel".Translate());
					diaOption7.linkLateBind = ResetToRoot(faction, negotiator);
					diaNode.options.Add(diaOption6);
					diaNode.options.Add(diaOption7);
					diaOption5.link = diaNode;
				}
				else
				{
					diaOption5.action = delegate
					{
						CallForAid(map, faction);
					};
					diaOption5.link = FightersSent(faction, negotiator);
				}
			}
			return diaOption5;
		}

		private static DiaOption RequestRoyalHeirChangeOption(Map map, Faction faction, Pawn factionRepresentative, Pawn negotiator)
		{
			RoyalTitleDef currentTitle = negotiator.royalty.GetCurrentTitle(faction);
			Pawn heir = negotiator.royalty.GetHeir(faction);
			DiaOption diaOption = new DiaOption((heir != null) ? "RequestChangeRoyalHeir".Translate(negotiator.Named("HOLDER"), currentTitle.GetLabelCapFor(negotiator).Named("TITLE"), heir.Named("HEIR")) : "RequestSetRoyalHeir".Translate(negotiator.Named("HOLDER"), currentTitle.GetLabelCapFor(negotiator).Named("TITLE")));
			bool num = Find.QuestManager.QuestsListForReading.Any((Quest q) => q.root == QuestScriptDefOf.ChangeRoyalHeir && q.State == QuestState.Ongoing && q.PartsListForReading.Any(delegate(QuestPart p)
			{
				QuestPart_ChangeHeir questPart_ChangeHeir = p as QuestPart_ChangeHeir;
				return questPart_ChangeHeir != null && !questPart_ChangeHeir.done && questPart_ChangeHeir.holder == negotiator;
			}));
			diaOption.link = RoyalHeirChangeCandidates(faction, factionRepresentative, negotiator);
			if (num)
			{
				diaOption.Disable("RequestChangeRoyalHeirAlreadyInProgress".Translate(negotiator.Named("PAWN")));
			}
			return diaOption;
		}

		public static DiaNode RoyalHeirChangeCandidates(Faction faction, Pawn factionRepresentative, Pawn negotiator)
		{
			DiaNode diaNode = new DiaNode("ChooseHeir".Translate(negotiator.Named("HOLDER")));
			RoyalTitleDef title = negotiator.royalty.GetCurrentTitle(faction);
			Pawn heir2 = negotiator.royalty.GetHeir(faction);
			foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsAndPrisonersSpawned)
			{
				DiaOption diaOption = new DiaOption(item.Name.ToStringFull);
				if (item == negotiator || item == heir2)
				{
					continue;
				}
				if (item.royalty != null)
				{
					RoyalTitleDef currentTitle = item.royalty.GetCurrentTitle(faction);
					if (currentTitle != null && currentTitle.seniority >= title.seniority)
					{
						continue;
					}
				}
				if (!item.IsQuestLodger())
				{
					Pawn heir = item;
					Action confirmedAct = delegate
					{
						QuestScriptDef changeRoyalHeir = QuestScriptDefOf.ChangeRoyalHeir;
						Slate slate = new Slate();
						slate.Set("points", title.changeHeirQuestPoints);
						slate.Set("asker", factionRepresentative);
						slate.Set("titleHolder", negotiator);
						slate.Set("titleHeir", heir);
						slate.Set("titlePreviousHeir", negotiator.royalty.GetHeir(faction));
						QuestUtility.SendLetterQuestAvailable(QuestUtility.GenerateQuestAndMakeAvailable(changeRoyalHeir, slate));
					};
					diaOption.link = RoyalHeirChangeConfirm(faction, negotiator, heir2, confirmedAct);
					diaNode.options.Add(diaOption);
				}
			}
			DiaOption diaOption2 = new DiaOption("GoBack".Translate());
			diaOption2.linkLateBind = ResetToRoot(faction, negotiator);
			diaNode.options.Add(diaOption2);
			return diaNode;
		}

		public static DiaNode RoyalHeirChangeConfirm(Faction faction, Pawn negotiator, Pawn currentHeir, Action confirmedAct)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("MakeHeirConfirm".Translate(faction, negotiator.Named("HOLDER")));
			if (currentHeir != null)
			{
				stringBuilder.Append(" " + "MakeHeirPreviousHeirWarning".Translate(negotiator.Named("HOLDER"), currentHeir.Named("HEIR")));
			}
			stringBuilder.Append(" " + "AreYouSure".Translate());
			DiaNode diaNode = new DiaNode(stringBuilder.ToString());
			DiaOption item = new DiaOption("Confirm".Translate())
			{
				action = confirmedAct,
				linkLateBind = ResetToRoot(faction, negotiator)
			};
			diaNode.options.Add(item);
			DiaOption item2 = new DiaOption("GoBack".Translate())
			{
				linkLateBind = ResetToRoot(faction, negotiator)
			};
			diaNode.options.Add(item2);
			return diaNode;
		}

		public static DiaNode CantMakeItInTime(Faction faction, Pawn negotiator)
		{
			return new DiaNode("CantSendMilitaryAidInTime".Translate(faction.leader).CapitalizeFirst())
			{
				options = 
				{
					OKToRoot(faction, negotiator)
				}
			};
		}

		public static DiaNode FightersSent(Faction faction, Pawn negotiator)
		{
			return new DiaNode("MilitaryAidSent".Translate(faction.leader).CapitalizeFirst())
			{
				options = 
				{
					OKToRoot(faction, negotiator)
				}
			};
		}

		private static void CallForAid(Map map, Faction faction)
		{
			faction.TryAffectGoodwillWith(Faction.OfPlayer, -25, canSendMessage: false, canSendHostilityLetter: true, "GoodwillChangedReason_RequestedMilitaryAid".Translate());
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = map;
			incidentParms.faction = faction;
			incidentParms.raidArrivalModeForQuickMilitaryAid = true;
			incidentParms.points = DiplomacyTuning.RequestedMilitaryAidPointsRange.RandomInRange;
			faction.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
			IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
		}

		private static DiaOption OKToRoot(Faction faction, Pawn negotiator)
		{
			return new DiaOption("OK".Translate())
			{
				linkLateBind = ResetToRoot(faction, negotiator)
			};
		}

		public static Func<DiaNode> ResetToRoot(Faction faction, Pawn negotiator)
		{
			return () => FactionDialogFor(negotiator, faction);
		}
	}
}
