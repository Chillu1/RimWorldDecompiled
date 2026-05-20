using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class QuestPart_MakeLord : QuestPart
	{
		public List<Pawn> pawns = new List<Pawn>();

		public string inSignal;

		public Faction faction;

		public MapParent mapParent;

		public string inSignalRemovePawn;

		public Pawn mapOfPawn;

		public bool excludeFromLookTargets;

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
				{
					yield return questLookTarget;
				}
				if (!excludeFromLookTargets)
				{
					for (int i = 0; i < pawns.Count; i++)
					{
						yield return pawns[i];
					}
				}
			}
		}

		public Map Map
		{
			get
			{
				if (mapOfPawn != null)
				{
					return mapOfPawn.MapHeld;
				}
				if (mapParent != null)
				{
					return mapParent.Map;
				}
				return null;
			}
		}

		protected abstract Lord MakeLord();

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				if (Map == null)
				{
					mapOfPawn = null;
					mapParent = quest.TryFindNewSuitableMapParentForRetarget();
				}
				if (Map != null)
				{
					pawns.RemoveAll((Pawn x) => x.MapHeld != Map);
					for (int num = 0; num < pawns.Count; num++)
					{
						pawns[num].GetLord()?.Notify_PawnLost(pawns[num], PawnLostCondition.ForcedByQuest);
					}
					Lord lord = MakeLord();
					for (int num2 = 0; num2 < pawns.Count; num2++)
					{
						if (!pawns[num2].Dead)
						{
							lord.AddPawn(pawns[num2]);
						}
					}
				}
			}
			if (signal.tag == inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
			{
				pawns.Remove(arg);
			}
		}

		public override void Notify_FactionRemoved(Faction f)
		{
			if (faction == f)
			{
				faction = null;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
			Scribe_References.Look(ref faction, "faction");
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_References.Look(ref mapOfPawn, "mapOfPawn");
			Scribe_Values.Look(ref excludeFromLookTargets, "excludeFromLookTargets", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			pawns.Add(PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
			mapParent = pawns[0].Map.Parent;
		}
	}
}
