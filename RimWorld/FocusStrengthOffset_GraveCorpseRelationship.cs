using System.Linq;
using Verse;

namespace RimWorld;

public class FocusStrengthOffset_GraveCorpseRelationship : FocusStrengthOffset
{
	public override bool DependsOnPawn => true;

	public override string GetExplanationAbstract(ThingDef def = null)
	{
		return "StatsReport_GraveCorpseRelatedAbstract".Translate() + ": " + offset.ToStringWithSign("0%");
	}

	public override float GetOffset(Thing parent, Pawn user = null)
	{
		return offset;
	}

	public override bool CanApply(Thing parent, Pawn user = null)
	{
		Building_Grave building_Grave = parent as Building_Grave;
		if (parent.Spawned && building_Grave != null && building_Grave.HasCorpse && building_Grave.Corpse.InnerPawn.RaceProps.Humanlike)
		{
			return building_Grave.Corpse.InnerPawn.relations.PotentiallyRelatedPawns.Contains(user);
		}
		return false;
	}
}
