using System;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Conditional_FactionRelation : ThingSetMaker_Conditional
	{
		public FactionDef factionDef;

		public bool allowHostile;

		public bool allowNeutral;

		public bool allowAlly;

		protected override bool Condition(ThingSetMakerParams parms)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(factionDef);
			if (faction == null)
			{
				return false;
			}
			switch (faction.RelationKindWith(Faction.OfPlayer))
			{
			case FactionRelationKind.Hostile:
				return allowHostile;
			case FactionRelationKind.Neutral:
				return allowNeutral;
			case FactionRelationKind.Ally:
				return allowAlly;
			default:
				throw new NotImplementedException();
			}
		}
	}
}
