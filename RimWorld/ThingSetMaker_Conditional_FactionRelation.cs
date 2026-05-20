using System;
using Verse;

namespace RimWorld;

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
		return faction.RelationKindWith(Faction.OfPlayer) switch
		{
			FactionRelationKind.Hostile => allowHostile, 
			FactionRelationKind.Neutral => allowNeutral, 
			FactionRelationKind.Ally => allowAlly, 
			_ => throw new NotImplementedException(), 
		};
	}
}
