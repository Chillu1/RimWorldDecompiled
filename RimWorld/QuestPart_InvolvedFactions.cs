using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_InvolvedFactions : QuestPart
	{
		public List<Faction> factions = new List<Faction>();

		public override IEnumerable<Faction> InvolvedFactions
		{
			get
			{
				foreach (Faction involvedFaction in base.InvolvedFactions)
				{
					yield return involvedFaction;
				}
				foreach (Faction faction in factions)
				{
					yield return faction;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref factions, "factions", LookMode.Reference);
		}
	}
}
