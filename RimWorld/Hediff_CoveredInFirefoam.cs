using Verse;

namespace RimWorld;

public class Hediff_CoveredInFirefoam : HediffWithComps
{
	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		pawn.Drawer.renderer.FirefoamOverlays.coveredInFoam = true;
	}

	public override void PostRemoved()
	{
		base.PostRemoved();
		pawn.Drawer.renderer.FirefoamOverlays.coveredInFoam = pawn.health.hediffSet.HasHediff(HediffDefOf.CoveredInFirefoam);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawn.Drawer.renderer.FirefoamOverlays.coveredInFoam = true;
		}
	}
}
