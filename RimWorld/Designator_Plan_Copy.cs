using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Plan_Copy : Designator_Plan_Add
{
	private BoolGrid grid;

	private Rot4 rotation;

	private readonly List<IntVec3> cells = new List<IntVec3>();

	private static float middleMouseDownTime;

	public override bool DragDrawMeasurements => false;

	protected override bool CanSelectColor => false;

	public override DrawStyleCategoryDef DrawStyleCategory => null;

	public override bool AlwaysDoGuiControls => true;

	public override bool CanRightClickToggleVisibility => false;

	public float PaneTopY => UI.screenHeight - 35;

	public override Color IconDrawColor => Color.white;

	public Designator_Plan_Copy()
	{
		useMouseIcon = false;
		icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanCopy");
		defaultLabel = "CommandCopyPlanLabel".Translate();
		defaultDesc = "CommandCopyPlanDesc".Translate();
		hotKey = KeyBindingDefOf.Designator_Cancel;
	}

	public override void SelectedUpdate()
	{
		base.SelectedUpdate();
		cells.Clear();
		foreach (IntVec3 currentCell in GetCurrentCells(UI.MouseCell()))
		{
			if (currentCell.InBounds(base.Map) && !currentCell.InNoBuildEdgeArea(base.Map))
			{
				cells.Add(currentCell);
			}
		}
		GenDraw.DrawFieldEdges(cells, colorDef.color, null, null, 3600);
		GenDraw.DrawNoZoneEdgeLines();
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 loc)
	{
		foreach (IntVec3 currentCell in GetCurrentCells(loc))
		{
			if (!currentCell.InBounds(base.Map))
			{
				return false;
			}
			if (currentCell.InNoBuildEdgeArea(base.Map))
			{
				return "TooCloseToMapEdge".Translate();
			}
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		PlanCells(cells);
		base.SelectedPlan = null;
	}

	protected IEnumerable<IntVec3> GetCurrentCells(IntVec3 c)
	{
		IntVec3 root = PrefabUtility.GetRoot(c, new IntVec2(grid.Width, grid.Height), rotation);
		for (int x = 0; x < grid.Width; x++)
		{
			for (int z = 0; z < grid.Height; z++)
			{
				if (grid[x, z])
				{
					IntVec3 adjustedLocalPosition = PrefabUtility.GetAdjustedLocalPosition(new IntVec3(x, 0, z), rotation);
					yield return root + adjustedLocalPosition;
				}
			}
		}
	}

	public override void Selected()
	{
		Plan selectedPlan = base.SelectedPlan;
		CellRect cellRect = CellRect.FromCellList(selectedPlan.Cells);
		colorDef = selectedPlan.Color;
		rotation = Rot4.North;
		grid = new BoolGrid(cellRect.Width, cellRect.Height);
		foreach (IntVec3 cell in selectedPlan.Cells)
		{
			IntVec3 c = cell - cellRect.Min;
			grid.Set(c, value: true);
		}
		base.SelectedPlan = null;
	}

	public override void DoExtraGuiControls(float leftX, float bottomY)
	{
		DesignatorUtility.GUIDoRotationControls(leftX, bottomY, rotation, delegate(Rot4 rot)
		{
			rotation = rot;
		});
	}

	public override void SelectedProcessInput(Event ev)
	{
		RotationDirection rotationDirection = RotationDirection.None;
		if (Event.current.button == 2)
		{
			if (Event.current.type == EventType.MouseDown)
			{
				Event.current.Use();
				middleMouseDownTime = Time.realtimeSinceStartup;
			}
			if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - middleMouseDownTime < 0.15f)
			{
				rotationDirection = RotationDirection.Clockwise;
			}
		}
		if (KeyBindingDefOf.Designator_RotateRight.KeyDownEvent)
		{
			rotationDirection = RotationDirection.Clockwise;
		}
		else if (KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
		{
			rotationDirection = RotationDirection.Counterclockwise;
		}
		if (rotationDirection != RotationDirection.None)
		{
			rotation.Rotate(rotationDirection);
		}
		DesignatorUtility.GUIDoRotationControls(0f, PaneTopY, rotation, delegate(Rot4 rot)
		{
			rotation = rot;
		});
	}
}
