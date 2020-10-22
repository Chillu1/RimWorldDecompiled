using Verse;

namespace RimWorld
{
	public class FocusStrengthOffset_GraveFull : FocusStrengthOffset
	{
		public override string GetExplanation(Thing parent)
		{
			if (CanApply(parent))
			{
				Building_Grave building_Grave = parent as Building_Grave;
				return "StatsReport_GraveFull".Translate(building_Grave.Corpse.InnerPawn.LabelShortCap) + ": " + GetOffset(parent).ToStringWithSign("0%");
			}
			return GetExplanationAbstract();
		}

		public override string GetExplanationAbstract(ThingDef def = null)
		{
			return "StatsReport_GraveFullAbstract".Translate() + ": " + offset.ToStringWithSign("0%");
		}

		public override float GetOffset(Thing parent, Pawn user = null)
		{
			return offset;
		}

		public override bool CanApply(Thing parent, Pawn user = null)
		{
			Building_Grave building_Grave;
			if (parent.Spawned && (building_Grave = parent as Building_Grave) != null && building_Grave.HasCorpse)
			{
				return building_Grave.Corpse.InnerPawn.RaceProps.Humanlike;
			}
			return false;
		}
	}
}
