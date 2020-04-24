using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class QuestPart_AddContentsToShuttle : QuestPart
	{
		public string inSignal;

		public Thing shuttle;

		private List<Thing> items = new List<Thing>();

		private List<Pawn> pawns = new List<Pawn>();

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
							Log.Error("Tried to add a destroyed thing to QuestPart_AddContentsToShuttle: " + item.ToStringSafe());
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
					yield return new Dialog_InfoCard.Hyperlink(def);
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
				foreach (Pawn questLookTarget2 in PawnsArriveQuestPartUtility.GetQuestLookTargets(pawns))
				{
					yield return questLookTarget2;
				}
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal) || shuttle == null)
			{
				return;
			}
			pawns.RemoveAll((Pawn x) => x.Destroyed);
			items.RemoveAll((Thing x) => x.Destroyed);
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].IsWorldPawn())
				{
					Find.WorldPawns.RemovePawn(pawns[i]);
				}
			}
			CompTransporter compTransporter = shuttle.TryGetComp<CompTransporter>();
			compTransporter.innerContainer.TryAddRangeOrTransfer(pawns);
			compTransporter.innerContainer.TryAddRangeOrTransfer(items);
			items.Clear();
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Collections.Look(ref items, "items", LookMode.Deep);
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			Scribe_References.Look(ref shuttle, "shuttle");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				items.RemoveAll((Thing x) => x == null);
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
		}
	}
}
