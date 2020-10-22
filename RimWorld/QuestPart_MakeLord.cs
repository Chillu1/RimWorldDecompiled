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
					return mapOfPawn.Map;
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
			if (signal.tag == inSignal && Map != null)
			{
				pawns.RemoveAll((Pawn x) => x.MapHeld != Map);
				for (int i = 0; i < pawns.Count; i++)
				{
					pawns[i].GetLord()?.Notify_PawnLost(pawns[i], PawnLostCondition.ForcedByQuest);
				}
				Lord lord = MakeLord();
				for (int j = 0; j < pawns.Count; j++)
				{
					if (!pawns[j].Dead)
					{
						lord.AddPawn(pawns[j]);
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
