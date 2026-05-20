using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_SetFaction : QuestPart
	{
		public string inSignal;

		public Faction faction;

		public List<Thing> things = new List<Thing>();

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
			if (!(signal.tag == inSignal))
			{
				return;
			}
			for (int i = 0; i < things.Count; i++)
			{
				if (things[i].Faction != faction)
				{
					things[i].SetFaction(faction);
				}
			}
		}

		public override void Notify_FactionRemoved(Faction f)
		{
			if (f == faction)
			{
				faction = null;
			}
		}

		public override bool QuestPartReserves(Faction f)
		{
			return f == faction;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref faction, "faction");
			Scribe_Collections.Look(ref things, "things", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				things.RemoveAll((Thing x) => x == null);
			}
		}
	}
}
