using RimWorld;
using UnityEngine;

namespace Verse;

public sealed class DeepResourceGrid : ICellBoolGiver, IExposable
{
	private const float LineSpacing = 29f;

	private const float IconPaddingRight = 4f;

	private const float IconSize = 27f;

	private Map map;

	private CellBoolDrawer drawer;

	private ushort[] defGrid;

	private ushort[] countGrid;

	public Color Color => Color.white;

	public DeepResourceGrid(Map map)
	{
		this.map = map;
		defGrid = new ushort[map.cellIndices.NumGridCells];
		countGrid = new ushort[map.cellIndices.NumGridCells];
		drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 3640, 1f);
	}

	public void ExposeData()
	{
		MapExposeUtility.ExposeUshort(map, (IntVec3 c) => defGrid[map.cellIndices.CellToIndex(c)], delegate(IntVec3 c, ushort val)
		{
			defGrid[map.cellIndices.CellToIndex(c)] = val;
		}, "defGrid");
		MapExposeUtility.ExposeUshort(map, (IntVec3 c) => countGrid[map.cellIndices.CellToIndex(c)], delegate(IntVec3 c, ushort val)
		{
			countGrid[map.cellIndices.CellToIndex(c)] = val;
		}, "countGrid");
	}

	public ThingDef ThingDefAt(IntVec3 c)
	{
		return DefDatabase<ThingDef>.GetByShortHash(defGrid[map.cellIndices.CellToIndex(c)]);
	}

	public int CountAt(IntVec3 c)
	{
		return countGrid[map.cellIndices.CellToIndex(c)];
	}

	public void SetAt(IntVec3 c, ThingDef def, int count)
	{
		if (count == 0)
		{
			def = null;
		}
		ushort num = def?.shortHash ?? 0;
		ushort num2 = (ushort)count;
		if (count > 65535)
		{
			Log.Error("Cannot store count " + count + " in DeepResourceGrid: out of ushort range.");
			num2 = ushort.MaxValue;
		}
		if (count < 0)
		{
			Log.Error("Cannot store count " + count + " in DeepResourceGrid: out of ushort range.");
			num2 = 0;
		}
		int num3 = map.cellIndices.CellToIndex(c);
		if (defGrid[num3] != num || countGrid[num3] != num2)
		{
			defGrid[num3] = num;
			countGrid[num3] = num2;
			drawer.SetDirty();
		}
	}

	public void DeepResourceGridUpdate()
	{
		drawer.CellBoolDrawerUpdate();
		if (DebugViewSettings.drawDeepResources)
		{
			MarkForDraw();
		}
	}

	public void MarkForDraw()
	{
		if (map == Find.CurrentMap && !Find.ScreenshotModeHandler.Active)
		{
			drawer.MarkForDraw();
		}
	}

	public void DrawPlacingMouseAttachments(BuildableDef placingDef)
	{
		if (placingDef is ThingDef thingDef && thingDef.CompDefFor<CompDeepDrill>() != null && AnyActiveDeepScannersOnMap())
		{
			RenderMouseAttachments();
		}
	}

	public void DeepResourcesOnGUI()
	{
		Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
		if (singleSelectedThing != null)
		{
			CompDeepScanner compDeepScanner = singleSelectedThing.TryGetComp<CompDeepScanner>();
			CompDeepDrill compDeepDrill = singleSelectedThing.TryGetComp<CompDeepDrill>();
			if ((compDeepScanner != null || compDeepDrill != null) && AnyActiveDeepScannersOnMap())
			{
				RenderMouseAttachments();
			}
		}
	}

	public bool AnyActiveDeepScannersOnMap()
	{
		foreach (Building item in map.listerBuildings.allBuildingsColonist)
		{
			CompDeepScanner compDeepScanner = item.TryGetComp<CompDeepScanner>();
			if (compDeepScanner != null && compDeepScanner.ShouldShowDeepResourceOverlay())
			{
				return true;
			}
		}
		return false;
	}

	private void RenderMouseAttachments()
	{
		IntVec3 c = UI.MouseCell();
		if (!c.InBounds(map))
		{
			return;
		}
		ThingDef thingDef = map.deepResourceGrid.ThingDefAt(c);
		if (thingDef != null)
		{
			int num = map.deepResourceGrid.CountAt(c);
			if (num > 0)
			{
				Vector2 vector = c.ToVector3().MapToUIPosition();
				GUI.color = Color.white;
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;
				float num2 = (UI.CurUICellSize() - 27f) / 2f;
				Rect rect = new Rect(vector.x + num2, vector.y - UI.CurUICellSize() + num2, 27f, 27f);
				Widgets.ThingIcon(rect, thingDef);
				Widgets.Label(new Rect(rect.xMax + 4f, rect.y, 999f, 29f), "DeepResourceRemaining".Translate(NamedArgumentUtility.Named(thingDef, "RESOURCE"), num.Named("COUNT")));
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}
	}

	public bool GetCellBool(int index)
	{
		return CountAt(map.cellIndices.IndexToCell(index)) > 0;
	}

	public Color GetCellExtraColor(int index)
	{
		IntVec3 c = map.cellIndices.IndexToCell(index);
		int num = CountAt(c);
		ThingDef thingDef = ThingDefAt(c);
		return DebugMatsSpectrum.Mat(Mathf.RoundToInt((float)num / (float)thingDef.deepCountPerCell / 2f * 100f) % 100, transparent: true).color;
	}
}
