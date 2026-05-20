using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Plan_CopySelectionPaste : Designator_Plan_Add
{
	private readonly List<ColorDef> grid = new List<ColorDef>();

	private readonly HashSet<ColorDef> colors = new HashSet<ColorDef>();

	private CellIndices indices;

	private Rot4 rotation;

	private readonly List<IntVec3> cells = new List<IntVec3>();

	private static readonly List<IntVec3> tmpSelected = new List<IntVec3>();

	private static float middleMouseDownTime;

	public override bool DragDrawMeasurements => true;

	public override DrawStyleCategoryDef DrawStyleCategory => null;

	public override bool AlwaysDoGuiControls => true;

	public float PaneTopY => UI.screenHeight - 35;

	public override bool CanRightClickToggleVisibility => false;

	public Designator_Plan_CopySelectionPaste()
	{
		useMouseIcon = true;
		hideMouseIcon = true;
		mouseText = "CommandCopyPlanSelectionPasteMouse".Translate();
	}

	public override void SelectedUpdate()
	{
		base.SelectedUpdate();
		foreach (ColorDef color in colors)
		{
			foreach (IntVec3 currentCell in GetCurrentCells(UI.MouseCell(), color))
			{
				if (currentCell.InBounds(base.Map) && !currentCell.InNoBuildEdgeArea(base.Map))
				{
					cells.Add(currentCell);
				}
			}
			GenDraw.DrawFieldEdges(cells, color.color, null, null, 3600);
			cells.Clear();
		}
		GenDraw.DrawNoZoneEdgeLines();
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 loc)
	{
		foreach (ColorDef color in colors)
		{
			foreach (IntVec3 currentCell in GetCurrentCells(loc, color))
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
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		foreach (ColorDef color in colors)
		{
			base.SelectedPlan = null;
			colorDef = color;
			PlanCells(GetCurrentCells(c, color));
		}
		base.SelectedPlan = null;
	}

	protected IEnumerable<IntVec3> GetCurrentCells(IntVec3 c, ColorDef color)
	{
		IntVec3 root = PrefabUtility.GetRoot(c, new IntVec2(indices.SizeX, indices.SizeZ), rotation);
		for (int i = 0; i < indices.NumGridCells; i++)
		{
			if (grid[i] == color)
			{
				yield return root + PrefabUtility.GetAdjustedLocalPosition(indices[i], rotation);
			}
		}
	}

	public void Initialize(IEnumerable<IntVec3> selected)
	{
		rotation = Rot4.North;
		tmpSelected.Clear();
		tmpSelected.AddRange(selected);
		CellRect cellRect = CellRect.FromCellList(tmpSelected);
		indices = new CellIndices(cellRect.Size.x, cellRect.Size.z);
		grid.Clear();
		colors.Clear();
		grid.Capacity = Mathf.Max(grid.Capacity, cellRect.Area);
		for (int i = 0; i < indices.NumGridCells; i++)
		{
			IntVec3 c = cellRect.Min + indices[i];
			ColorDef colorDef = base.Map.planManager.PlanAt(c)?.Color;
			grid.Add(colorDef);
			if (colorDef != null)
			{
				colors.Add(colorDef);
			}
		}
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
