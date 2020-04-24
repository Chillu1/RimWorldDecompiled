using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_FactionRelationKind : QuestPartActivable
	{
		public Faction faction1;

		public Faction faction2;

		public FactionRelationKind relationKind;

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

		public override void QuestPartTick()
		{
			base.QuestPartTick();
			if (faction1 != null && faction2 != null && faction1.RelationKindWith(faction2) == relationKind)
			{
				Complete(faction1.Named("SUBJECT"));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref faction1, "faction1");
			Scribe_References.Look(ref faction2, "faction2");
			Scribe_Values.Look(ref relationKind, "relationKind", FactionRelationKind.Hostile);
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			faction1 = Find.FactionManager.RandomEnemyFaction();
			faction2 = Faction.OfPlayer;
			relationKind = FactionRelationKind.Neutral;
		}
	}
}
