using Verse;

namespace RimWorld;

public class RitualOutcomeComp_BuildingOfDefUsed : RitualOutcomeComp_QualitySingleOffset
{
	public ThingDef def;

	public int maxHorDistFromTarget;

	protected override string LabelForDesc => "Used".Translate(def.LabelCap);

	public override bool Applies(LordJob_Ritual ritual)
	{
		return ritual.usedThings.Any((Thing t) => t.def == def);
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		bool flag = false;
		foreach (Thing item in ritualTarget.Map.listerThings.ThingsOfDef(def))
		{
			if (GatheringsUtility.InGatheringArea(item.Position, ritualTarget.Cell, ritualTarget.Map) && (maxHorDistFromTarget == 0 || item.Position.InHorDistOf(ritualTarget.Cell, maxHorDistFromTarget)))
			{
				flag = true;
				break;
			}
		}
		return new QualityFactor
		{
			label = def.LabelCap,
			present = flag,
			qualityChange = ExpectedOffsetDesc(flag, -1f),
			quality = (flag ? qualityOffset : 0f),
			positive = flag,
			priority = 1f
		};
	}
}
