using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class SelectionDrawer
{
	private static Dictionary<object, float> selectTimes = new Dictionary<object, float>();

	private static HashSet<StorageGroup> drawnStorageGroupBrackets = new HashSet<StorageGroup>();

	private static readonly Material SelectionBracketMat = MaterialPool.MatFrom("UI/Overlays/SelectionBracket", ShaderDatabase.MetaOverlay);

	private static Vector3[] bracketLocs = new Vector3[4];

	public static Dictionary<object, float> SelectTimes => selectTimes;

	public static void Notify_Selected(object t)
	{
		selectTimes[t] = Time.realtimeSinceStartup;
	}

	public static void Clear()
	{
		selectTimes.Clear();
	}

	public static void Notify_DrawnStorageGroup(StorageGroup storageGroup)
	{
		drawnStorageGroupBrackets.Add(storageGroup);
	}

	public static bool DrawnStorageGroupThisFrame(StorageGroup storageGroup)
	{
		return drawnStorageGroupBrackets.Contains(storageGroup);
	}

	public static void DrawSelectionOverlays()
	{
		drawnStorageGroupBrackets.Clear();
		if (Find.ScreenshotModeHandler.Active)
		{
			return;
		}
		foreach (object selectedObject in Find.Selector.SelectedObjects)
		{
			DrawSelectionBracketFor(selectedObject);
			if (selectedObject is Thing thing)
			{
				thing.DrawExtraSelectionOverlays();
			}
		}
	}

	public static void DrawSelectionBracketFor(object obj, Material overrideMat = null)
	{
		if (obj is Zone zone)
		{
			GenDraw.DrawFieldEdges(zone.Cells);
		}
		else if (obj is Plan plan)
		{
			GenDraw.DrawFieldEdges(plan.Cells, 3600);
		}
		else
		{
			if (!(obj is Thing { CustomRectForSelector: var customRectForSelector, DrawPos: var carryDrawPos } thing))
			{
				return;
			}
			if (customRectForSelector.HasValue)
			{
				SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, customRectForSelector.Value.CenterVector3, new Vector2(customRectForSelector.Value.Width, customRectForSelector.Value.Height), selectTimes, Vector2.one, 1f, thing.def.deselectedSelectionBracketFactor);
			}
			else if (thing.SpawnedParentOrMe is Pawn pawn && pawn != thing)
			{
				carryDrawPos = pawn.DrawPos;
				PawnRenderUtility.CalculateCarriedDrawPos(pawn, thing, ref carryDrawPos, out var _);
				SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, carryDrawPos, thing.RotatedSize.ToVector2(), selectTimes, Vector2.one, 1f, thing.def.deselectedSelectionBracketFactor);
			}
			else if (thing.SpawnedParentOrMe is Building_Enterable building_Enterable && building_Enterable != thing)
			{
				SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, building_Enterable.DrawPos + building_Enterable.PawnDrawOffset, thing.RotatedSize.ToVector2(), selectTimes, Vector2.one, 1f, thing.def.deselectedSelectionBracketFactor);
			}
			else
			{
				if (!thing.DrawPosHeld.HasValue)
				{
					return;
				}
				carryDrawPos = thing.DrawPosHeld.Value;
				SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, carryDrawPos, thing.RotatedSize.ToVector2(), selectTimes, Vector2.one, 1f, thing.def.deselectedSelectionBracketFactor);
			}
			float num = (thing.MultipleItemsPerCellDrawn() ? 0.8f : 1f);
			float num2 = 1f;
			CameraDriver cameraDriver = Find.CameraDriver;
			float num3 = Mathf.Clamp01(Mathf.InverseLerp(cameraDriver.config.sizeRange.max * 0.84999996f, cameraDriver.config.sizeRange.max, cameraDriver.ZoomRootSize));
			if (thing is Pawn)
			{
				if (thing.def.Size == IntVec2.One)
				{
					num *= Mathf.Min(1f + num3 / 2f, 2f);
				}
				else
				{
					num2 = Mathf.Min(1f + num3 / 2f, 2f);
				}
			}
			int num4 = 0;
			for (int i = 0; i < 4; i++)
			{
				Quaternion q = Quaternion.AngleAxis(num4, Vector3.up);
				Vector3 pos = (bracketLocs[i] - carryDrawPos) * num + carryDrawPos;
				Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(pos, q, new Vector3(num, 1f, num) * num2), overrideMat ?? SelectionBracketMat, 0);
				num4 -= 90;
			}
		}
	}
}
