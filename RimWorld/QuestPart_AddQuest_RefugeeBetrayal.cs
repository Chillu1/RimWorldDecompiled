using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld
{
	public class QuestPart_AddQuest_RefugeeBetrayal : QuestPart_AddQuest
	{
		public List<Pawn> lodgers = new List<Pawn>();

		public Pawn factionOpponent;

		public Pawn asker;

		public ExtraFaction refugeeFaction;

		public MapParent mapParent;

		public string inSignalRemovePawn;

		public override QuestScriptDef QuestDef => QuestScriptDefOf.RefugeeBetrayal;

		public override Slate GetSlate()
		{
			Slate slate = new Slate();
			slate.Set("lodgers", lodgers);
			slate.Set("refugeeFaction", refugeeFaction);
			slate.Set("factionOpponent", factionOpponent);
			slate.Set("asker", asker);
			slate.Set("lodgerCount", lodgers.Count);
			slate.Set("faction", refugeeFaction.faction);
			slate.Set("map", mapParent.Map);
			return slate;
		}

		public override void Notify_FactionRemoved(Faction f)
		{
			if (refugeeFaction?.faction == f)
			{
				refugeeFaction = null;
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn arg) && lodgers.Contains(arg))
			{
				lodgers.Remove(arg);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref refugeeFaction, "refugeeFaction");
			Scribe_Collections.Look(ref lodgers, "pawns", LookMode.Reference);
			Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
			Scribe_References.Look(ref asker, "asker");
			Scribe_References.Look(ref factionOpponent, "factionOpponent");
			Scribe_References.Look(ref mapParent, "mapParent");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				lodgers.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
