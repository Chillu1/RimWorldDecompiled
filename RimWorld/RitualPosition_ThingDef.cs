using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class RitualPosition_ThingDef : RitualPosition_NearbyThing
{
	protected abstract ThingDef ThingDef { get; }

	public override IEnumerable<Thing> CandidateThings(LordJob_Ritual ritual)
	{
		foreach (Thing item in ritual.Map.listerThings.ThingsOfDef(ThingDef))
		{
			if (item.Position.InHorDistOf(ritual.selectedTarget.Cell, 50f))
			{
				yield return item;
			}
		}
	}

	public override IntVec3 PositionForThing(Thing t)
	{
		return t.Position + t.Rotation.Opposite.FacingCell;
	}

	public override bool IsUsableThing(Thing thing, IntVec3 spot, TargetInfo ritualTarget)
	{
		IntVec3 c = PositionForThing(thing);
		Map map = ritualTarget.Map;
		Building edifice = c.GetEdifice(map);
		if (!c.Standable(map) || (edifice != null && edifice.def.Fillage != FillCategory.None))
		{
			return false;
		}
		return base.IsUsableThing(thing, spot, ritualTarget);
	}

	protected override Rot4 FacingDir(Thing t)
	{
		return t.Rotation;
	}
}
