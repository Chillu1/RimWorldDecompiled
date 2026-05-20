using Verse;

namespace RimWorld;

public class GameCondition_GillRot : GameCondition
{
	public float fishPopulationOffsetFactorPerDay;

	public override void Init()
	{
		base.Init();
		fishPopulationOffsetFactorPerDay = def.fishPopulationOffsetPerDay;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref fishPopulationOffsetFactorPerDay, "fishPopulationOffsetFactorPerDay", 0f);
	}
}
