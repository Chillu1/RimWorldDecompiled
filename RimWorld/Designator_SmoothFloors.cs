using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_SmoothFloors : Designator_Smooth
{
	public Designator_SmoothFloors()
	{
		defaultLabel = "DesignatorSmoothFloors".Translate();
		defaultDesc = "DesignatorSmoothFloorsDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/SmoothSurface");
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		SmoothSurfaceDesignatorUtility.DesignateSmoothFloor(base.Map, c);
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		AcceptanceReport result = base.CanDesignateCell(c);
		if (!result.Accepted)
		{
			return result;
		}
		if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor) != null)
		{
			return "SurfaceBeingSmoothed".Translate();
		}
		Building edifice = c.GetEdifice(base.Map);
		if (edifice != null && !SmoothSurfaceDesignatorUtility.CanSmoothFloorUnder(edifice))
		{
			return "MessageMustDesignateSmoothableFloor".Translate();
		}
		if (!c.GetAffordances(base.Map).Contains(TerrainAffordanceDefOf.SmoothableStone))
		{
			return "MessageMustDesignateSmoothableFloor".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}
}
