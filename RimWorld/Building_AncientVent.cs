using Verse;

namespace RimWorld;

public class Building_AncientVent : Building
{
	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		CompFleckEmitterLongTerm comp = GetComp<CompFleckEmitterLongTerm>();
		if (comp != null)
		{
			GameConditionDef conditionToCause = GetComp<CompAncientVent>().Props.conditionToCause;
			bool enabled = map.GameConditionManager.ConditionIsActive(conditionToCause);
			comp.Enabled = enabled;
		}
		base.SpawnSetup(map, respawningAfterLoad);
	}
}
