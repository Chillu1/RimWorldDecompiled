using Verse;

namespace RimWorld
{
	public class QuestPart_FactionRelationChange : QuestPart
	{
		public string inSignal;

		public Faction faction;

		public FactionRelationKind relationKind;

		public bool canSendHostilityLetter;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal && faction != null && faction != Faction.OfPlayer)
			{
				faction.SetRelationDirect(Faction.OfPlayer, relationKind, canSendHostilityLetter);
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
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref relationKind, "relationKind", FactionRelationKind.Hostile);
			Scribe_Values.Look(ref canSendHostilityLetter, "canSendHostilityLetter", defaultValue: false);
			Scribe_References.Look(ref faction, "faction");
		}
	}
}
