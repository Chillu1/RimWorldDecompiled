using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_SmoothSurface : Designator_Smooth
{
	public Designator_SmoothSurface()
	{
		defaultLabel = "DesignatorSmoothSurface".Translate();
		defaultDesc = "DesignatorSmoothSurfaceDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/SmoothSurface");
	}

	public override void DesignateThing(Thing t)
	{
		DesignateSingleCell(t.Position);
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		AcceptanceReport result = base.CanDesignateCell(c);
		if (!result.Accepted)
		{
			return result;
		}
		if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor) != null || base.Map.designationManager.DesignationAt(c, DesignationDefOf.SmoothWall) != null)
		{
			return "SurfaceBeingSmoothed".Translate();
		}
		Building edifice = c.GetEdifice(base.Map);
		if (edifice != null && edifice.def.IsSmoothable)
		{
			return AcceptanceReport.WasAccepted;
		}
		if (edifice != null && !SmoothSurfaceDesignatorUtility.CanSmoothFloorUnder(edifice))
		{
			return "MessageMustDesignateSmoothableSurface".Translate();
		}
		if (!c.GetAffordances(base.Map).Contains(TerrainAffordanceDefOf.SmoothableStone))
		{
			return "MessageMustDesignateSmoothableSurface".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		if (!SmoothSurfaceDesignatorUtility.DesignateSmoothWall(base.Map, c))
		{
			SmoothSurfaceDesignatorUtility.DesignateSmoothFloor(base.Map, c);
		}
	}
}
