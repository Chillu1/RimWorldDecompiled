using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public static class QuestGen_Misc
	{
		private const string RootSymbol = "root";

		public static QuestPart_InvolvedFactions AddInvolvedFaction(this Quest quest, Faction faction)
		{
			QuestPart_InvolvedFactions questPart_InvolvedFactions = ((QuestPart_InvolvedFactions)quest.PartsListForReading.First((QuestPart p) => p is QuestPart_InvolvedFactions)) ?? new QuestPart_InvolvedFactions();
			questPart_InvolvedFactions.factions.Add(faction);
			if (!quest.PartsListForReading.Contains(questPart_InvolvedFactions))
			{
				quest.AddPart(questPart_InvolvedFactions);
			}
			return questPart_InvolvedFactions;
		}

		public static QuestPart_SpawnThing SpawnSkyfaller(this Quest quest, Map map, ThingDef skyfallerDef, IEnumerable<Thing> innerThings, Faction factionForSafeSpot = null, IntVec3? cell = null, string inSignal = null, bool lookForSafeSpot = false, bool tryLandInShipLandingZone = false, Thing tryLandNearThing = null, Pawn mapParentOfPawn = null)
		{
			Skyfaller thing = SkyfallerMaker.MakeSkyfaller(skyfallerDef, innerThings);
			QuestPart_SpawnThing questPart_SpawnThing = new QuestPart_SpawnThing();
			questPart_SpawnThing.thing = thing;
			questPart_SpawnThing.mapParent = map?.Parent;
			questPart_SpawnThing.mapParentOfPawn = mapParentOfPawn;
			questPart_SpawnThing.factionForFindingSpot = factionForSafeSpot;
			if (cell.HasValue)
			{
				questPart_SpawnThing.cell = cell.Value;
			}
			questPart_SpawnThing.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart_SpawnThing.lookForSafeSpot = lookForSafeSpot;
			questPart_SpawnThing.tryLandInShipLandingZone = tryLandInShipLandingZone;
			questPart_SpawnThing.tryLandNearThing = tryLandNearThing;
			quest.AddPart(questPart_SpawnThing);
			return questPart_SpawnThing;
		}

		public static QuestPart_SpawnThing SpawnSpaceDrone(this Quest quest, Map map, ThingDef skyfallerDef, IEnumerable<Thing> innerThings, Faction factionForSafeSpot = null, IntVec3? cell = null, string inSignal = null, bool lookForSafeSpot = false, bool tryLandInShipLandingZone = false, Thing tryLandNearThing = null, Pawn mapParentOfPawn = null)
		{
			Skyfaller thing = SkyfallerMaker.MakeSkyfaller(skyfallerDef, innerThings);
			QuestPart_SpawnSpaceDrone questPart_SpawnSpaceDrone = new QuestPart_SpawnSpaceDrone();
			questPart_SpawnSpaceDrone.thing = thing;
			questPart_SpawnSpaceDrone.mapParent = map?.Parent;
			questPart_SpawnSpaceDrone.mapParentOfPawn = mapParentOfPawn;
			questPart_SpawnSpaceDrone.factionForFindingSpot = factionForSafeSpot;
			if (cell.HasValue)
			{
				questPart_SpawnSpaceDrone.cell = cell.Value;
			}
			questPart_SpawnSpaceDrone.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart_SpawnSpaceDrone.lookForSafeSpot = lookForSafeSpot;
			questPart_SpawnSpaceDrone.tryLandInShipLandingZone = tryLandInShipLandingZone;
			questPart_SpawnSpaceDrone.tryLandNearThing = tryLandNearThing;
			quest.AddPart(questPart_SpawnSpaceDrone);
			return questPart_SpawnSpaceDrone;
		}

		public static QuestPart_SpawnThing SpawnThing(this Quest quest, Map map, Thing thing, Faction factionForSafeSpot = null, IntVec3? cell = null, string inSignal = null, bool lookForSafeSpot = false, bool tryLandInShipLandingZone = false, Thing tryLandNearThing = null, Pawn mapParentOfPawn = null, bool questLookTarget = true, EffecterDef spawnEffecter = null)
		{
			QuestPart_SpawnThing questPart_SpawnThing = new QuestPart_SpawnThing();
			questPart_SpawnThing.thing = thing;
			questPart_SpawnThing.mapParent = map?.Parent;
			questPart_SpawnThing.mapParentOfPawn = mapParentOfPawn;
			questPart_SpawnThing.questLookTarget = questLookTarget;
			questPart_SpawnThing.factionForFindingSpot = factionForSafeSpot;
			if (cell.HasValue)
			{
				questPart_SpawnThing.cell = cell.Value;
			}
			questPart_SpawnThing.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart_SpawnThing.lookForSafeSpot = lookForSafeSpot;
			questPart_SpawnThing.tryLandInShipLandingZone = tryLandInShipLandingZone;
			questPart_SpawnThing.tryLandNearThing = tryLandNearThing;
			questPart_SpawnThing.spawnEffecter = spawnEffecter;
			quest.AddPart(questPart_SpawnThing);
			return questPart_SpawnThing;
		}

		public static QuestPart_SetFaction SetFaction(this Quest quest, IEnumerable<Thing> things, Faction faction, string inSignal = null)
		{
			QuestPart_SetFaction questPart_SetFaction = new QuestPart_SetFaction();
			questPart_SetFaction.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart_SetFaction.faction = faction;
			questPart_SetFaction.things.AddRange(things);
			QuestGen.quest.AddPart(questPart_SetFaction);
			return questPart_SetFaction;
		}

		public static QuestPart_JoinPlayer JoinPlayer(this Quest quest, MapParent mapParent, IEnumerable<Pawn> pawns, bool joinPlayer = false, bool makePrisoners = false, string inSignal = null)
		{
			QuestPart_JoinPlayer questPart_JoinPlayer = new QuestPart_JoinPlayer();
			questPart_JoinPlayer.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_JoinPlayer.joinPlayer = joinPlayer;
			questPart_JoinPlayer.makePrisoners = makePrisoners;
			questPart_JoinPlayer.mapParent = mapParent;
			questPart_JoinPlayer.pawns.AddRange(pawns);
			questPart_JoinPlayer.signalListenMode = QuestPart.SignalListenMode.Always;
			quest.AddPart(questPart_JoinPlayer);
			return questPart_JoinPlayer;
		}

		public static QuestPart_LeavePlayer LeavePlayer(this Quest quest, IEnumerable<Pawn> pawns, string inSignal = null, Faction replacementFaction = null, string inSignalRemovePawn = null)
		{
			QuestPart_LeavePlayer questPart_LeavePlayer = new QuestPart_LeavePlayer();
			questPart_LeavePlayer.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_LeavePlayer.pawns.AddRange(pawns);
			questPart_LeavePlayer.replacementFaction = replacementFaction;
			questPart_LeavePlayer.inSignalRemovePawn = inSignalRemovePawn;
			quest.AddPart(questPart_LeavePlayer);
			return questPart_LeavePlayer;
		}

		public static QuestPart_SurpriseReinforcement SurpriseReinforcements(this Quest quest, string inSignalEnabled, MapParent mapParent, Faction faction, float reinforcementChance)
		{
			QuestPart_SurpriseReinforcement questPart_SurpriseReinforcement = new QuestPart_SurpriseReinforcement();
			questPart_SurpriseReinforcement.inSignalEnable = inSignalEnabled;
			questPart_SurpriseReinforcement.mapParent = mapParent;
			questPart_SurpriseReinforcement.faction = faction;
			questPart_SurpriseReinforcement.reinforcementChance = reinforcementChance;
			quest.AddPart(questPart_SurpriseReinforcement);
			return questPart_SurpriseReinforcement;
		}

		public static QuestPart_DropPods DropPods(this Quest quest, MapParent mapParent, IEnumerable<Thing> contents, string customLetterLabel = null, RulePack customLetterLabelRules = null, string customLetterText = null, RulePack customLetterTextRules = null, bool? sendStandardLetter = true, bool useTradeDropSpot = false, bool joinPlayer = false, bool makePrisoners = false, string inSignal = null, IEnumerable<Thing> thingsToExcludeFromHyperlinks = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly, IntVec3? dropSpot = null, bool destroyItemsOnCleanup = true, bool dropAllInSamePod = false, bool allowFogged = false, bool canRetargetAnyMap = false, Faction faction = null)
		{
			QuestPart_DropPods dropPods = new QuestPart_DropPods();
			dropPods.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			dropPods.signalListenMode = signalListenMode;
			if (!customLetterLabel.NullOrEmpty() || customLetterLabelRules != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					dropPods.customLetterLabel = x;
				}, QuestGenUtility.MergeRules(customLetterLabelRules, customLetterLabel, "root"));
			}
			if (!customLetterText.NullOrEmpty() || customLetterTextRules != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					dropPods.customLetterText = x;
				}, QuestGenUtility.MergeRules(customLetterTextRules, customLetterText, "root"));
			}
			dropPods.sendStandardLetter = sendStandardLetter ?? dropPods.sendStandardLetter;
			dropPods.useTradeDropSpot = useTradeDropSpot;
			dropPods.joinPlayer = joinPlayer;
			dropPods.makePrisoners = makePrisoners;
			dropPods.mapParent = mapParent;
			dropPods.Things = contents;
			dropPods.destroyItemsOnCleanup = destroyItemsOnCleanup;
			dropPods.dropAllInSamePod = dropAllInSamePod;
			dropPods.allowFogged = allowFogged;
			dropPods.faction = faction;
			dropPods.canRetargetAnyMap = canRetargetAnyMap;
			if (dropSpot.HasValue)
			{
				dropPods.dropSpot = dropSpot.Value;
			}
			if (thingsToExcludeFromHyperlinks != null)
			{
				dropPods.thingsToExcludeFromHyperlinks.AddRange(thingsToExcludeFromHyperlinks.Select((Thing t) => t.GetInnerIfMinified().def));
			}
			QuestGen.quest.AddPart(dropPods);
			return dropPods;
		}

		public static void AddMemoryThought(this Quest quest, IEnumerable<Pawn> pawns, ThoughtDef def, string inSignal = null, Pawn otherPawn = null, bool? addToLookTargets = null)
		{
			foreach (Pawn pawn in pawns)
			{
				QuestPart_AddMemoryThought questPart_AddMemoryThought = new QuestPart_AddMemoryThought();
				questPart_AddMemoryThought.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
				questPart_AddMemoryThought.def = def;
				questPart_AddMemoryThought.pawn = pawn;
				questPart_AddMemoryThought.otherPawn = otherPawn;
				questPart_AddMemoryThought.addToLookTargets = addToLookTargets ?? true;
				QuestGen.quest.AddPart(questPart_AddMemoryThought);
			}
		}

		public static QuestPart_Letter Letter(this Quest quest, LetterDef letterDef, string inSignal = null, string chosenPawnSignal = null, Faction relatedFaction = null, MapParent useColonistsOnMap = null, bool useColonistsFromCaravanArg = false, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly, IEnumerable<object> lookTargets = null, bool filterDeadPawnsFromLookTargets = false, string text = null, RulePack textRules = null, string label = null, RulePack labelRules = null, string getColonistsFromSignal = null)
		{
			Slate slate = QuestGen.slate;
			QuestPart_Letter questPart_Letter = new QuestPart_Letter();
			questPart_Letter.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? slate.Get<string>("inSignal");
			LetterDef letterDef2 = letterDef ?? LetterDefOf.NeutralEvent;
			if (typeof(ChoiceLetter).IsAssignableFrom(letterDef2.letterClass))
			{
				ChoiceLetter choiceLetter = LetterMaker.MakeLetter("error", "error", letterDef2, QuestGenUtility.ToLookTargets(lookTargets), relatedFaction, QuestGen.quest);
				questPart_Letter.letter = choiceLetter;
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					choiceLetter.Label = x;
				}, QuestGenUtility.MergeRules(labelRules, label, "root"));
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					choiceLetter.Text = x;
				}, QuestGenUtility.MergeRules(textRules, text, "root"));
			}
			else
			{
				questPart_Letter.letter = LetterMaker.MakeLetter(letterDef2);
				questPart_Letter.letter.lookTargets = QuestGenUtility.ToLookTargets(lookTargets);
				questPart_Letter.letter.relatedFaction = relatedFaction;
			}
			questPart_Letter.chosenPawnSignal = QuestGenUtility.HardcodedSignalWithQuestID(chosenPawnSignal);
			questPart_Letter.useColonistsOnMap = useColonistsOnMap;
			questPart_Letter.useColonistsFromCaravanArg = useColonistsFromCaravanArg;
			questPart_Letter.signalListenMode = signalListenMode;
			questPart_Letter.filterDeadPawnsFromLookTargets = filterDeadPawnsFromLookTargets;
			questPart_Letter.getColonistsFromSignal = getColonistsFromSignal;
			QuestGen.quest.AddPart(questPart_Letter);
			return questPart_Letter;
		}

		public static QuestPart_PlayOneShotOnCamera PlayOneShotOnCamera(this Quest quest, SoundDef soundDef, string inSignal = null)
		{
			Slate slate = QuestGen.slate;
			QuestPart_PlayOneShotOnCamera questPart_PlayOneShotOnCamera = new QuestPart_PlayOneShotOnCamera();
			questPart_PlayOneShotOnCamera.soundDef = soundDef;
			questPart_PlayOneShotOnCamera.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? slate.Get<string>("inSignal");
			QuestGen.quest.AddPart(questPart_PlayOneShotOnCamera);
			return questPart_PlayOneShotOnCamera;
		}

		public static void AcceptanceRequirementLayer(this Quest quest, MapParent mapParent = null, bool canBeSpace = false, List<PlanetLayerDef> whitelist = null, List<PlanetLayerDef> blacklist = null)
		{
			quest.AddPart(new QuestPart_RequirementsToAcceptPlanetLayer
			{
				canBeSpace = canBeSpace,
				layerWhitelist = whitelist,
				layerBlacklist = blacklist,
				mapParent = (mapParent ?? QuestGen.slate.Get<Map>("map").Parent)
			});
		}

		public static void AcceptanceRequirementNotSpace(this Quest quest, MapParent mapParent = null)
		{
			MapParent mapParent2 = mapParent ?? QuestGen.slate.Get<Map>("map")?.Parent;
			if (mapParent2 != null)
			{
				quest.AddPart(new QuestPart_RequirementsToAcceptPlanetLayer
				{
					mapParent = mapParent2
				});
			}
		}

		public static void PawnsArrive(this Quest quest, IEnumerable<Pawn> pawns, string inSignal = null, MapParent mapParent = null, PawnsArrivalModeDef arrivalMode = null, bool joinPlayer = false, IntVec3? walkInSpot = null, string customLetterLabel = null, string customLetterText = null, RulePack customLetterLabelRules = null, RulePack customLetterTextRules = null, bool isSingleReward = false, bool rewardDetailsHidden = false, bool sendStandardLetter = true)
		{
			PawnsArrivalModeDef pawnsArrivalModeDef = ResolveArrivalMode(mapParent, arrivalMode);
			QuestPart_PawnsArrive pawnsArrive = new QuestPart_PawnsArrive();
			pawnsArrive.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			pawnsArrive.pawns.AddRange(pawns);
			pawnsArrive.arrivalMode = pawnsArrivalModeDef;
			pawnsArrive.joinPlayer = joinPlayer;
			pawnsArrive.mapParent = mapParent ?? QuestGen.slate.Get<Map>("map").Parent;
			pawnsArrive.sendStandardLetter = sendStandardLetter;
			if (pawnsArrivalModeDef.walkIn)
			{
				pawnsArrive.spawnNear = walkInSpot ?? QuestGen.slate.Get<IntVec3?>("walkInSpot") ?? IntVec3.Invalid;
			}
			if (!customLetterLabel.NullOrEmpty() || customLetterLabelRules != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					pawnsArrive.customLetterLabel = x;
				}, QuestGenUtility.MergeRules(customLetterLabelRules, customLetterLabel, "root"));
			}
			if (!customLetterText.NullOrEmpty() || customLetterTextRules != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					pawnsArrive.customLetterText = x;
				}, QuestGenUtility.MergeRules(customLetterTextRules, customLetterText, "root"));
			}
			QuestGen.quest.AddPart(pawnsArrive);
			if (!isSingleReward)
			{
				return;
			}
			QuestPart_Choice questPart_Choice = new QuestPart_Choice();
			questPart_Choice.inSignalChoiceUsed = pawnsArrive.inSignal;
			QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
			choice.questParts.Add(pawnsArrive);
			foreach (Pawn pawn in pawnsArrive.pawns)
			{
				choice.rewards.Add(new Reward_Pawn
				{
					pawn = pawn,
					detailsHidden = rewardDetailsHidden
				});
			}
			questPart_Choice.choices.Add(choice);
			QuestGen.quest.AddPart(questPart_Choice);
		}

		private static PawnsArrivalModeDef ResolveArrivalMode(MapParent parent, PawnsArrivalModeDef requestedMode)
		{
			PawnsArrivalModeDef pawnsArrivalModeDef = requestedMode ?? PawnsArrivalModeDefOf.EdgeWalkIn;
			if (parent == null || !parent.Tile.Valid || pawnsArrivalModeDef.Worker.CanUseOnTile(parent.Tile))
			{
				return pawnsArrivalModeDef;
			}
			foreach (PawnsArrivalModeDef item in DefDatabase<PawnsArrivalModeDef>.AllDefsListForReading)
			{
				if (item != pawnsArrivalModeDef && item.Worker.CanUseOnTile(parent.Tile))
				{
					pawnsArrivalModeDef = item;
					break;
				}
			}
			return pawnsArrivalModeDef;
		}

		public static QuestPart_AddQuestRefugeeDelayedReward AddQuestRefugeeDelayedReward(this Quest quest, Pawn acceptee, Faction faction, IEnumerable<Pawn> pawns, FloatRange marketValueRange, string inSignalRemovePawn = null)
		{
			QuestPart_AddQuestRefugeeDelayedReward questPart_AddQuestRefugeeDelayedReward = new QuestPart_AddQuestRefugeeDelayedReward();
			questPart_AddQuestRefugeeDelayedReward.acceptee = quest.AccepterPawn;
			questPart_AddQuestRefugeeDelayedReward.inSignal = QuestGen.slate.Get<string>("inSignal");
			questPart_AddQuestRefugeeDelayedReward.inSignalRemovePawn = inSignalRemovePawn;
			questPart_AddQuestRefugeeDelayedReward.faction = faction;
			questPart_AddQuestRefugeeDelayedReward.lodgers.AddRange(pawns);
			questPart_AddQuestRefugeeDelayedReward.marketValueRange = marketValueRange;
			quest.AddPart(questPart_AddQuestRefugeeDelayedReward);
			return questPart_AddQuestRefugeeDelayedReward;
		}

		public static QuestPart_PawnJoinOffer PawnJoinOffer(this Quest quest, Pawn pawn, string letterLabel, string letterTitle, string letterText, Action accepted = null, Action rejected = null, string inSignal = null, string outSignalPawnAccepted = null, string outSignalPawnRejected = null, bool charity = false, bool sendLetterOnEnable = false)
		{
			QuestPart_PawnJoinOffer questPart_PawnJoinOffer = new QuestPart_PawnJoinOffer();
			questPart_PawnJoinOffer.pawn = pawn;
			questPart_PawnJoinOffer.inSignalEnable = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart_PawnJoinOffer.outSignalPawnAccepted = outSignalPawnAccepted ?? QuestGen.GenerateNewSignal("Accepted");
			questPart_PawnJoinOffer.outSignalPawnRejected = outSignalPawnRejected ?? QuestGen.GenerateNewSignal("Rejected");
			questPart_PawnJoinOffer.letterLabel = letterLabel;
			questPart_PawnJoinOffer.letterText = letterText;
			questPart_PawnJoinOffer.letterTitle = letterTitle;
			questPart_PawnJoinOffer.charity = charity;
			questPart_PawnJoinOffer.sendLetterOnEnable = sendLetterOnEnable;
			if (accepted != null)
			{
				QuestGenUtility.RunInner(accepted, questPart_PawnJoinOffer.outSignalPawnAccepted);
			}
			if (rejected != null)
			{
				QuestGenUtility.RunInner(rejected, questPart_PawnJoinOffer.outSignalPawnRejected);
			}
			quest.AddPart(questPart_PawnJoinOffer);
			return questPart_PawnJoinOffer;
		}

		public static QuestPart_Choice RewardChoice(this Quest quest, IEnumerable<QuestPart_Choice.Choice> choices = null, string inSignalChoiceUsed = null)
		{
			QuestPart_Choice questPart_Choice = new QuestPart_Choice();
			questPart_Choice.inSignalChoiceUsed = inSignalChoiceUsed;
			if (choices != null)
			{
				questPart_Choice.choices.AddRange(choices);
			}
			quest.AddPart(questPart_Choice);
			return questPart_Choice;
		}

		public static QuestPart_BetrayalOffer BetrayalOffer(this Quest quest, IEnumerable<Pawn> pawns, ExtraFaction extraFaction, Pawn asker, Action success = null, Action failure = null, Action enabled = null, IEnumerable<string> inSignals = null, string inSignalEnable = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_BetrayalOffer questPart_BetrayalOffer = new QuestPart_BetrayalOffer();
			questPart_BetrayalOffer.pawns.AddRange(pawns);
			questPart_BetrayalOffer.asker = asker;
			questPart_BetrayalOffer.extraFaction = extraFaction;
			questPart_BetrayalOffer.inSignalEnable = inSignalEnable ?? QuestGen.slate.Get<string>("inSignal");
			questPart_BetrayalOffer.signalListenMode = signalListenMode;
			if (inSignals != null)
			{
				questPart_BetrayalOffer.inSignals.AddRange(inSignals);
			}
			if (success != null)
			{
				string text = QuestGen.GenerateNewSignal("BetrayalOfferSuccess");
				QuestGenUtility.RunInner(success, text);
				questPart_BetrayalOffer.outSignalSuccess = text;
			}
			if (failure != null)
			{
				string text2 = QuestGen.GenerateNewSignal("BetrayalOfferFailure");
				QuestGenUtility.RunInner(failure, text2);
				questPart_BetrayalOffer.outSignalFailure = text2;
				string text3 = QuestGen.GenerateNewSignal("BetrayalOfferFailureRecruited");
				QuestGenUtility.RunInner(failure, text3);
				questPart_BetrayalOffer.outSignalFailureRecruited = text3;
			}
			if (enabled != null)
			{
				string text4 = QuestGen.GenerateNewSignal("BetrayalOfferEnabled");
				QuestGenUtility.RunInner(enabled, text4);
				questPart_BetrayalOffer.outSignalEnabled = text4;
			}
			quest.AddPart(questPart_BetrayalOffer);
			return questPart_BetrayalOffer;
		}

		public static QuestPart_InnerFactionFight InnerFactionFight(this Quest quest, IntVec3 meetingPos, MapParent mapParent, IEnumerable<Pawn> pawns, Faction firstFaction, FactionDef secondFactionDef, Action complete = null, IEnumerable<string> inSignals = null, string inSignalEnable = null, string factionBecameHostileSignal = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_InnerFactionFight questPart_InnerFactionFight = new QuestPart_InnerFactionFight();
			questPart_InnerFactionFight.pawns.AddRange(pawns);
			questPart_InnerFactionFight.firstFaction = firstFaction;
			questPart_InnerFactionFight.secondFactionDef = secondFactionDef;
			questPart_InnerFactionFight.inSignalEnable = inSignalEnable ?? QuestGen.slate.Get<string>("inSignal");
			questPart_InnerFactionFight.signalListenMode = signalListenMode;
			questPart_InnerFactionFight.meetingPos = meetingPos;
			questPart_InnerFactionFight.mapParent = mapParent;
			questPart_InnerFactionFight.factionBecameHostileSignal = factionBecameHostileSignal;
			if (inSignals != null)
			{
				questPart_InnerFactionFight.inSignals.AddRange(inSignals);
			}
			if (complete != null)
			{
				string text = QuestGen.GenerateNewSignal("ArgumentCompleted");
				QuestGenUtility.RunInner(complete, text);
				questPart_InnerFactionFight.outSignalComplete = text;
			}
			quest.AddPart(questPart_InnerFactionFight);
			return questPart_InnerFactionFight;
		}

		public static void Leave(this Quest quest, IEnumerable<Pawn> pawns, string inSignal = null, bool sendStandardLetter = true, bool leaveOnCleanup = true, string inSignalRemovePawn = null, bool wakeUp = false)
		{
			QuestPart_Leave questPart_Leave = new QuestPart_Leave();
			questPart_Leave.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Leave.pawns.AddRange(pawns);
			questPart_Leave.sendStandardLetter = sendStandardLetter;
			questPart_Leave.leaveOnCleanup = leaveOnCleanup;
			questPart_Leave.inSignalRemovePawn = inSignalRemovePawn;
			questPart_Leave.wakeUp = wakeUp;
			quest.AddPart(questPart_Leave);
		}

		public static QuestPart_Alert Alert(this Quest quest, string label, string explanation, LookTargets lookTargets = null, bool critical = false, bool getLookTargetsFromSignal = false, string inSignalEnable = null, string inSignalDisable = null)
		{
			QuestPart_Alert questPart = new QuestPart_Alert();
			questPart.label = "error";
			questPart.explanation = "error";
			questPart.critical = critical;
			questPart.getLookTargetsFromSignal = getLookTargetsFromSignal;
			questPart.lookTargets = lookTargets;
			questPart.inSignalEnable = inSignalEnable ?? QuestGen.slate.Get<string>("inSignal");
			questPart.inSignalDisable = inSignalDisable;
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				questPart.label = x;
			}, QuestGenUtility.MergeRules(null, label, "root"));
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				questPart.explanation = x;
			}, QuestGenUtility.MergeRules(null, explanation, "root"));
			quest.AddPart(questPart);
			return questPart;
		}

		public static QuestPart_Message Message(this Quest quest, string message, MessageTypeDef messageType = null, bool getLookTargetsFromSignal = false, RulePack rules = null, LookTargets lookTargets = null, string inSignal = null)
		{
			QuestPart_Message questPart = new QuestPart_Message();
			questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart.messageType = messageType ?? MessageTypeDefOf.NeutralEvent;
			questPart.lookTargets = lookTargets;
			questPart.getLookTargetsFromSignal = getLookTargetsFromSignal;
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				questPart.message = x;
			}, QuestGenUtility.MergeRules(rules, message, "root"));
			QuestGen.quest.AddPart(questPart);
			return questPart;
		}

		public static QuestPart_Notify_PlayerRaidedSomeone Notify_PlayerRaidedSomeone(this Quest quest, Map getRaidersFromMap = null, MapParent getRaidersFromMapParent = null, string inSignal = null)
		{
			QuestPart_Notify_PlayerRaidedSomeone questPart_Notify_PlayerRaidedSomeone = new QuestPart_Notify_PlayerRaidedSomeone();
			questPart_Notify_PlayerRaidedSomeone.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Notify_PlayerRaidedSomeone.getRaidersFromMap = getRaidersFromMap;
			questPart_Notify_PlayerRaidedSomeone.getRaidersFromMapParent = getRaidersFromMapParent;
			questPart_Notify_PlayerRaidedSomeone.signalListenMode = QuestPart.SignalListenMode.Always;
			quest.AddPart(questPart_Notify_PlayerRaidedSomeone);
			return questPart_Notify_PlayerRaidedSomeone;
		}

		public static QuestPart_FactionGoodwillChange_ShuttleSentThings GoodwillChangeShuttleSentThings(this Quest quest, Faction faction, IEnumerable<Pawn> pawns, int changeNotOnShuttle, string inSignalEnable = null, IEnumerable<string> inSignalsShuttleSent = null, string inSignalShuttleDestroyed = null, HistoryEventDef reason = null, bool canSendMessage = true, bool canSendHostilityLetter = false, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_FactionGoodwillChange_ShuttleSentThings questPart_FactionGoodwillChange_ShuttleSentThings = new QuestPart_FactionGoodwillChange_ShuttleSentThings();
			questPart_FactionGoodwillChange_ShuttleSentThings.inSignalEnable = inSignalEnable ?? QuestGen.slate.Get<string>("inSignal");
			questPart_FactionGoodwillChange_ShuttleSentThings.inSignalsShuttleSent.AddRange(inSignalsShuttleSent);
			questPart_FactionGoodwillChange_ShuttleSentThings.inSignalShuttleDestroyed = inSignalShuttleDestroyed;
			questPart_FactionGoodwillChange_ShuttleSentThings.changeNotOnShuttle = changeNotOnShuttle;
			questPart_FactionGoodwillChange_ShuttleSentThings.things.AddRange(pawns);
			questPart_FactionGoodwillChange_ShuttleSentThings.faction = faction;
			questPart_FactionGoodwillChange_ShuttleSentThings.historyEvent = reason;
			questPart_FactionGoodwillChange_ShuttleSentThings.canSendMessage = canSendMessage;
			questPart_FactionGoodwillChange_ShuttleSentThings.canSendHostilityLetter = canSendHostilityLetter;
			questPart_FactionGoodwillChange_ShuttleSentThings.signalListenMode = signalListenMode;
			quest.AddPart(questPart_FactionGoodwillChange_ShuttleSentThings);
			return questPart_FactionGoodwillChange_ShuttleSentThings;
		}

		public static QuestPart_BiocodeWeapons BiocodeWeapons(this Quest quest, IEnumerable<Pawn> pawns, string inSignal = null)
		{
			QuestPart_BiocodeWeapons questPart_BiocodeWeapons = new QuestPart_BiocodeWeapons();
			questPart_BiocodeWeapons.pawns.AddRange(pawns);
			questPart_BiocodeWeapons.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			quest.AddPart(questPart_BiocodeWeapons);
			return questPart_BiocodeWeapons;
		}

		public static QuestPart_DestroyThingsOrPassToWorld DestroyThingsOrPassToWorld(this Quest quest, IEnumerable<Thing> things, string inSignal = null, bool questLookTargets = true, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_DestroyThingsOrPassToWorld questPart_DestroyThingsOrPassToWorld = new QuestPart_DestroyThingsOrPassToWorld();
			questPart_DestroyThingsOrPassToWorld.things.AddRange(things);
			questPart_DestroyThingsOrPassToWorld.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart_DestroyThingsOrPassToWorld.questLookTargets = true;
			questPart_DestroyThingsOrPassToWorld.signalListenMode = signalListenMode;
			return questPart_DestroyThingsOrPassToWorld;
		}

		public static QuestPart_DescriptionPart DescriptionPart(this Quest quest, string descriptionPart, string inSignalEnable = null, string inSignalDisable = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly, RulePack rulePack = null)
		{
			QuestPart_DescriptionPart questPart = new QuestPart_DescriptionPart();
			questPart.inSignalEnable = inSignalEnable ?? QuestGen.slate.Get<string>("inSignal");
			questPart.inSignalDisable = inSignalDisable;
			questPart.descriptionPart = descriptionPart;
			questPart.signalListenMode = signalListenMode;
			quest.AddPart(questPart);
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				questPart.descriptionPart = x;
			}, QuestGenUtility.MergeRules(rulePack, descriptionPart, "root"));
			return questPart;
		}

		public static QuestPart_Dialog Dialog(this Quest quest, string text, string title = null, string inSignal = null, RulePack titleRules = null, RulePack textRules = null, QuestPart.SignalListenMode signalListMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Dialog questPart = new QuestPart_Dialog();
			questPart.title = title;
			questPart.text = text;
			questPart.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart.signalListenMode = signalListMode;
			quest.AddPart(questPart);
			if (!title.NullOrEmpty())
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					questPart.title = x;
				}, QuestGenUtility.MergeRules(titleRules, title, "root"));
			}
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				questPart.text = x;
			}, QuestGenUtility.MergeRules(titleRules, text, "root"));
			return questPart;
		}

		public static QuestPart_Dialog DialogWithCloseBehavior(this Quest quest, string text, string title = null, string inSignal = null, RulePack titleRules = null, RulePack textRules = null, QuestPart.SignalListenMode signalListMode = QuestPart.SignalListenMode.OngoingOnly, QuestPartDialogCloseAction.CloseActionKey closeAction = QuestPartDialogCloseAction.CloseActionKey.None)
		{
			QuestPart_Dialog questPart_Dialog = quest.Dialog(text, title, inSignal, titleRules, textRules, signalListMode);
			questPart_Dialog.closeAction = closeAction;
			return questPart_Dialog;
		}

		public static QuestPart_SetQuestNotYetAccepted SetQuestNotYetAccepted(this Quest quest, string inSignal = null, bool revertOngoingQuestAfterLoad = false)
		{
			QuestPart_SetQuestNotYetAccepted questPart_SetQuestNotYetAccepted = new QuestPart_SetQuestNotYetAccepted();
			questPart_SetQuestNotYetAccepted.inSignal = inSignal;
			questPart_SetQuestNotYetAccepted.revertOngoingQuestAfterLoad = revertOngoingQuestAfterLoad;
			quest.AddPart(questPart_SetQuestNotYetAccepted);
			return questPart_SetQuestNotYetAccepted;
		}

		public static QuestPart_StartWick StartWick(this Quest quest, Thing thing, string inSignal = null)
		{
			QuestPart_StartWick questPart_StartWick = new QuestPart_StartWick();
			questPart_StartWick.explosiveThing = thing;
			questPart_StartWick.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			quest.AddPart(questPart_StartWick);
			return questPart_StartWick;
		}

		public static QuestPart_GiveDiedOrDownedThoughts GiveDiedOrDownedThoughts(this Quest quest, Pawn aboutPawn, PawnDiedOrDownedThoughtsKind thoughtsKind, string inSignal = null)
		{
			QuestPart_GiveDiedOrDownedThoughts questPart_GiveDiedOrDownedThoughts = new QuestPart_GiveDiedOrDownedThoughts();
			questPart_GiveDiedOrDownedThoughts.aboutPawn = aboutPawn;
			questPart_GiveDiedOrDownedThoughts.thoughtsKind = thoughtsKind;
			questPart_GiveDiedOrDownedThoughts.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			quest.AddPart(questPart_GiveDiedOrDownedThoughts);
			return questPart_GiveDiedOrDownedThoughts;
		}

		public static QuestPart_GiveHediff GiveHediff(this Quest quest, Pawn aboutPawn, HediffDef hediff, string inSignal = null)
		{
			QuestPart_GiveHediff questPart_GiveHediff = new QuestPart_GiveHediff
			{
				aboutPawn = aboutPawn,
				hediff = hediff,
				inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal"))
			};
			quest.AddPart(questPart_GiveHediff);
			return questPart_GiveHediff;
		}

		public static QuestPart_LinkUnnaturalCorpse LinkUnnaturalCorpse(this Quest quest, Pawn aboutPawn, UnnaturalCorpse corpse, string inSignal = null)
		{
			QuestPart_LinkUnnaturalCorpse questPart_LinkUnnaturalCorpse = new QuestPart_LinkUnnaturalCorpse
			{
				aboutPawn = aboutPawn,
				corpse = corpse,
				inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal"))
			};
			quest.AddPart(questPart_LinkUnnaturalCorpse);
			return questPart_LinkUnnaturalCorpse;
		}

		public static QuestPart_AssignMechToMechanitor AssignMechToMechanitor(this Quest quest, Pawn mechanitor, Pawn mech, string inSignal = null)
		{
			QuestPart_AssignMechToMechanitor questPart_AssignMechToMechanitor = new QuestPart_AssignMechToMechanitor();
			questPart_AssignMechToMechanitor.mechanitor = mechanitor;
			questPart_AssignMechToMechanitor.mech = mech;
			questPart_AssignMechToMechanitor.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			quest.AddPart(questPart_AssignMechToMechanitor);
			return questPart_AssignMechToMechanitor;
		}

		public static QuestPart_GameCondition GameCondition(this Quest quest, GameConditionDef gameConditionDef, int duration, MapParent mapParent, bool permanent = false, bool forceDisplayAsDuration = false, string inSignal = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly, bool sendStandardLetter = true, bool targetWorld = false)
		{
			GameCondition gameCondition = GameConditionMaker.MakeCondition(gameConditionDef, duration);
			gameCondition.forceDisplayAsDuration = forceDisplayAsDuration;
			gameCondition.Permanent = permanent;
			QuestPart_GameCondition questPart_GameCondition = new QuestPart_GameCondition
			{
				gameCondition = gameCondition,
				inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal")),
				signalListenMode = signalListenMode,
				mapParent = mapParent,
				sendStandardLetter = sendStandardLetter,
				targetWorld = targetWorld
			};
			quest.AddPart(questPart_GameCondition);
			return questPart_GameCondition;
		}

		public static QuestPart_LookTargets LookTargets(this Quest quest, IEnumerable<GlobalTargetInfo> targets)
		{
			QuestPart_LookTargets questPart_LookTargets = new QuestPart_LookTargets();
			questPart_LookTargets.targets.AddRange(targets);
			quest.AddPart(questPart_LookTargets);
			return questPart_LookTargets;
		}
	}
}
