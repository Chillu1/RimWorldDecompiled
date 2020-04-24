using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_SetFactionRelations : QuestPart
	{
		public string inSignal;

		public Faction faction;

		public FactionRelationKind relationKind;

		public bool canSendLetter;

		public override IEnumerable<Faction> InvolvedFactions
		{
			get
			{
				foreach (Faction involvedFaction in base.InvolvedFactions)
				{
					yield return involvedFaction;
				}
				if (faction != null)
				{
					yield return faction;
				}
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal && faction != null && faction != Faction.OfPlayer)
			{
				faction.TrySetRelationKind(Faction.OfPlayer, relationKind, canSendLetter);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref relationKind, "relationKind", FactionRelationKind.Hostile);
			Scribe_Values.Look(ref canSendLetter, "canSendLetter", defaultValue: false);
		}
	}
}
