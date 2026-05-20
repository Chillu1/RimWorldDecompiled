using System;
using Verse;

namespace RimWorld
{
	public class QuestPart_RequirementsToAcceptFactionRelation : QuestPart_RequirementsToAccept
	{
		public Faction otherFaction;

		public FactionRelationKind relationKind;

		public bool acceptIfDefeated;

		public string ReasonText => relationKind switch
		{
			FactionRelationKind.Ally => "QuestAlliedTo".Translate(otherFaction), 
			FactionRelationKind.Neutral => "QuestNeutralTo".Translate(otherFaction), 
			FactionRelationKind.Hostile => "QuestHostileTo".Translate(otherFaction), 
			_ => throw new Exception($"Unknown faction relation kind: {relationKind}"), 
		};

		public override AcceptanceReport CanAccept()
		{
			if (otherFaction != null && ((acceptIfDefeated && otherFaction.defeated) || Faction.OfPlayer.RelationKindWith(otherFaction) == relationKind))
			{
				return true;
			}
			return new AcceptanceReport(ReasonText);
		}

		public override void Notify_FactionRemoved(Faction faction)
		{
			base.Notify_FactionRemoved(faction);
			if (otherFaction == faction)
			{
				otherFaction = null;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref relationKind, "relationKind", FactionRelationKind.Hostile);
			Scribe_References.Look(ref otherFaction, "otherFaction");
			Scribe_Values.Look(ref acceptIfDefeated, "acceptIfDefeated", defaultValue: false);
		}
	}
}
