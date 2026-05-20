using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_FactionGoodwillLocked : QuestPartActivable
	{
		public Faction faction1;

		public Faction faction2;

		public override IEnumerable<Faction> InvolvedFactions
		{
			get
			{
				foreach (Faction involvedFaction in base.InvolvedFactions)
				{
					yield return involvedFaction;
				}
				if (faction1 != null)
				{
					yield return faction1;
				}
				if (faction2 != null)
				{
					yield return faction2;
				}
			}
		}

		public override void Notify_FactionRemoved(Faction faction)
		{
			if (faction == faction1)
			{
				faction1 = null;
			}
			if (faction == faction2)
			{
				faction2 = null;
			}
		}

		public bool AppliesTo(Faction a, Faction b)
		{
			if (base.State != QuestPartState.Enabled)
			{
				return false;
			}
			if (faction1 != a || faction2 != b)
			{
				if (faction1 == b)
				{
					return faction2 == a;
				}
				return false;
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref faction1, "faction1");
			Scribe_References.Look(ref faction2, "faction2");
		}
	}
}
