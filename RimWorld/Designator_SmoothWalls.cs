using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_SmoothWalls : Designator_Smooth
{
	public Designator_SmoothWalls()
	{
		defaultLabel = "DesignatorSmoothWalls".Translate();
		defaultDesc = "DesignatorSmoothWallsDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/SmoothSurface");
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (t != null && t.def.IsSmoothable && CanDesignateCell(t.Position).Accepted)
		{
			return AcceptanceReport.WasAccepted;
		}
		return false;
	}

	public override void DesignateThing(Thing t)
	{
		DesignateSingleCell(t.Position);
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		SmoothSurfaceDesignatorUtility.DesignateSmoothWall(base.Map, c);
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		AcceptanceReport result = base.CanDesignateCell(c);
		if (!result.Accepted)
		{
			return result;
		}
		if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.SmoothWall) != null)
		{
			return "SurfaceBeingSmoothed".Translate();
		}
		Building edifice = c.GetEdifice(base.Map);
		if (edifice == null || !edifice.def.IsSmoothable)
		{
			return "MessageMustDesignateSmoothableWall".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}
}
