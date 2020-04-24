using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class QuestPart_DropPods : QuestPart
	{
		public string inSignal;

		public string outSignalResult;

		public IntVec3 dropSpot = IntVec3.Invalid;

		public bool useTradeDropSpot;

		public MapParent mapParent;

		private List<Thing> items = new List<Thing>();

		private List<Pawn> pawns = new List<Pawn>();

		public List<ThingDef> thingsToExcludeFromHyperlinks = new List<ThingDef>();

		public bool joinPlayer;

		public bool makePrisoners;

		public string customLetterText;

		public string customLetterLabel;

		public LetterDef customLetterDef;

		public bool sendStandardLetter = true;

		private Thing importantLookTarget;

		public IEnumerable<Thing> Things
		{
			get
			{
				return items.Concat(pawns.Cast<Thing>());
			}
			set
			{
				items.Clear();
				pawns.Clear();
				if (value != null)
				{
					foreach (Thing item in value)
					{
						if (item.Destroyed)
						{
							Log.Error("Tried to add a destroyed thing to QuestPart_DropPods: " + item.ToStringSafe());
						}
						else
						{
							Pawn pawn = item as Pawn;
							if (pawn != null)
							{
								pawns.Add(pawn);
							}
							else
							{
								items.Add(item);
							}
						}
					}
				}
			}
		}

		public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
		{
			get
			{
				foreach (Dialog_InfoCard.Hyperlink hyperlink in base.Hyperlinks)
				{
					yield return hyperlink;
				}
				foreach (Thing item in items)
				{
					ThingDef def = item.GetInnerIfMinified().def;
					if (!thingsToExcludeFromHyperlinks.Contains(def))
					{
						yield return new Dialog_InfoCard.Hyperlink(def);
					}
				}
			}
		}

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
				{
					yield return questLookTarget;
				}
				if (mapParent != null)
				{
					yield return mapParent;
				}
				foreach (Pawn questLookTarget2 in PawnsArriveQuestPartUtility.GetQuestLookTargets(pawns))
				{
					yield return questLookTarget2;
				}
				if (importantLookTarget != null)
				{
					yield return importantLookTarget;
				}
			}
		}

		public override bool IncreasesPopulation => PawnsArriveQuestPartUtility.IncreasesPopulation(pawns, joinPlayer, makePrisoners);

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal))
			{
				return;
			}
			pawns.RemoveAll((Pawn x) => x.Destroyed);
			items.RemoveAll((Thing x) => x.Destroyed);
			Thing thing = Things.Where((Thing x) => x is Pawn).MaxByWithFallback((Thing x) => x.MarketValue);
			Thing thing2 = Things.MaxByWithFallback((Thing x) => x.MarketValue * (float)x.stackCount);
			if (mapParent != null && mapParent.HasMap && Things.Any())
			{
				Map map = mapParent.Map;
				IntVec3 intVec = dropSpot.IsValid ? dropSpot : GetRandomDropSpot();
				if (sendStandardLetter)
				{
					TaggedString title;
					TaggedString text;
					if (joinPlayer && pawns.Count == 1 && pawns[0].RaceProps.Humanlike)
					{
						text = "LetterRefugeeJoins".Translate(pawns[0].Named("PAWN"));
						title = "LetterLabelRefugeeJoins".Translate(pawns[0].Named("PAWN"));
						PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, pawns[0]);
					}
					else
					{
						text = "LetterQuestDropPodsArrived".Translate(GenLabel.ThingsLabel(Things));
						title = "LetterLabelQuestDropPodsArrived".Translate();
						PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref title, ref text, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
					}
					title = (customLetterLabel.NullOrEmpty() ? title : customLetterLabel.Formatted(title.Named("BASELABEL")));
					text = (customLetterText.NullOrEmpty() ? text : customLetterText.Formatted(text.Named("BASETEXT")));
					Find.LetterStack.ReceiveLetter(title, text, customLetterDef ?? LetterDefOf.PositiveEvent, new TargetInfo(intVec, map), null, quest);
				}
				if (joinPlayer)
				{
					for (int i = 0; i < pawns.Count; i++)
					{
						if (pawns[i].Faction != Faction.OfPlayer)
						{
							pawns[i].SetFaction(Faction.OfPlayer);
						}
					}
				}
				else if (makePrisoners)
				{
					for (int j = 0; j < pawns.Count; j++)
					{
						if (pawns[j].RaceProps.Humanlike)
						{
							if (!pawns[j].IsPrisonerOfColony)
							{
								pawns[j].guest.SetGuestStatus(Faction.OfPlayer, prisoner: true);
							}
							HealthUtility.TryAnesthetize(pawns[j]);
						}
					}
				}
				DropPodUtility.DropThingsNear(intVec, map, Things, 110, canInstaDropDuringInit: false, leaveSlag: false, !useTradeDropSpot, forbid: false);
				importantLookTarget = items.Find((Thing x) => x.GetInnerIfMinified() is MonumentMarker).GetInnerIfMinified();
				items.Clear();
			}
			if (!outSignalResult.NullOrEmpty())
			{
				if (thing != null)
				{
					Find.SignalManager.SendSignal(new Signal(outSignalResult, thing.Named("SUBJECT")));
				}
				else if (thing2 != null)
				{
					Find.SignalManager.SendSignal(new Signal(outSignalResult, thing2.Named("SUBJECT")));
				}
				else
				{
					Find.SignalManager.SendSignal(new Signal(outSignalResult));
				}
			}
		}

		public override bool QuestPartReserves(Pawn p)
		{
			return pawns.Contains(p);
		}

		public override void ReplacePawnReferences(Pawn replace, Pawn with)
		{
			pawns.Replace(replace, with);
		}

		public override void Cleanup()
		{
			base.Cleanup();
			for (int i = 0; i < items.Count; i++)
			{
				if (!items[i].Destroyed)
				{
					items[i].Destroy();
				}
			}
			items.Clear();
		}

		private IntVec3 GetRandomDropSpot()
		{
			Map map = mapParent.Map;
			if (useTradeDropSpot)
			{
				return DropCellFinder.TradeDropSpot(map);
			}
			if (CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => x.Standable(map) && !x.Roofed(map) && !x.Fogged(map) && map.reachability.CanReachColony(x), map, 1000, out IntVec3 result))
			{
				return result;
			}
			return DropCellFinder.RandomDropSpot(map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref outSignalResult, "outSignalResult");
			Scribe_Values.Look(ref dropSpot, "dropSpot");
			Scribe_Values.Look(ref useTradeDropSpot, "useTradeDropSpot", defaultValue: false);
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_Collections.Look(ref items, "items", LookMode.Deep);
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			Scribe_Values.Look(ref joinPlayer, "joinPlayer", defaultValue: false);
			Scribe_Values.Look(ref makePrisoners, "makePrisoners", defaultValue: false);
			Scribe_Values.Look(ref customLetterLabel, "customLetterLabel");
			Scribe_Values.Look(ref customLetterText, "customLetterText");
			Scribe_Defs.Look(ref customLetterDef, "customLetterDef");
			Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: true);
			Scribe_References.Look(ref importantLookTarget, "importantLookTarget");
			Scribe_Collections.Look(ref thingsToExcludeFromHyperlinks, "thingsToExcludeFromHyperlinks", LookMode.Def);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (thingsToExcludeFromHyperlinks == null)
				{
					thingsToExcludeFromHyperlinks = new List<ThingDef>();
				}
				items.RemoveAll((Thing x) => x == null);
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			if (Find.AnyPlayerHomeMap == null)
			{
				return;
			}
			mapParent = Find.RandomPlayerHomeMap.Parent;
			List<Thing> list = ThingSetMakerDefOf.DebugQuestDropPodsContents.root.Generate();
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i] as Pawn;
				if (pawn != null)
				{
					pawn.relations.everSeenByPlayer = true;
					if (!pawn.IsWorldPawn())
					{
						Find.WorldPawns.PassToWorld(pawn);
					}
				}
			}
			Things = list;
		}
	}
}
