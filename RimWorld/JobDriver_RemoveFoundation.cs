using Verse;

namespace RimWorld;

public class JobDriver_RemoveFoundation : JobDriver_AffectFloor
{
	protected override int BaseWorkAmount => 300;

	protected override DesignationDef DesDef => DesignationDefOf.RemoveFoundation;

	protected override StatDef SpeedStat => StatDefOf.ConstructionSpeed;

	public JobDriver_RemoveFoundation()
	{
		clearSnow = true;
	}

	protected override void DoEffect(IntVec3 c)
	{
		if (base.Map.terrainGrid.CanRemoveFoundationAt(c))
		{
			base.Map.terrainGrid.RemoveFoundation(base.TargetLocA);
			FilthMaker.RemoveAllFilth(c, base.Map);
		}
	}
}
