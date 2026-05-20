using Verse;

namespace RimWorld;

public class JobDriver_FillIn : JobDriver_RemoveBuilding
{
	protected override DesignationDef Designation => DesignationDefOf.FillIn;

	protected override float TotalNeededWork => base.Target.def.building.uninstallWork;

	protected override EffecterDef WorkEffecter
	{
		get
		{
			if (!(base.Target is PitBurrow))
			{
				return EffecterDefOf.FillingInCrater;
			}
			return EffecterDefOf.FillingInPitGate;
		}
	}

	protected override void FinishedRemoving()
	{
		if (base.Target is Crater crater)
		{
			crater.FillIn();
		}
	}
}
