using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldInspectPane : Window, IInspectPane
{
	private Type openTabType;

	public Type OpenTabType
	{
		get
		{
			return openTabType;
		}
		set
		{
			openTabType = value;
		}
	}

	public float RecentHeight { get; set; }

	protected override float Margin => 0f;

	public override Vector2 InitialSize => InspectPaneUtility.PaneSizeFor(this);

	private List<WorldObject> Selected => Find.WorldSelector.SelectedObjects;

	private int NumSelectedObjects => Find.WorldSelector.NumSelectedObjects;

	public float PaneTopY
	{
		get
		{
			float num = (float)UI.screenHeight - 165f;
			if (Current.ProgramState == ProgramState.Playing)
			{
				num -= 35f;
			}
			return num;
		}
	}

	public bool AnythingSelected => Find.WorldSelector.AnyObjectOrTileSelected;

	private PlanetTile SelectedTile => Find.WorldSelector.SelectedTile;

	private bool SelectedSingleObjectOrTile
	{
		get
		{
			if (NumSelectedObjects != 1)
			{
				if (NumSelectedObjects == 0)
				{
					return SelectedTile.Valid;
				}
				return false;
			}
			return true;
		}
	}

	public bool ShouldShowSelectNextInCellButton => SelectedSingleObjectOrTile;

	public bool ShouldShowPaneContents => SelectedSingleObjectOrTile;

	public IEnumerable<InspectTabBase> CurTabs
	{
		get
		{
			if (NumSelectedObjects == 1)
			{
				return Find.WorldSelector.SingleSelectedObject.GetInspectTabs();
			}
			if (NumSelectedObjects == 0 && SelectedTile.Valid)
			{
				return PlanetLayer.Selected.Def.Tabs;
			}
			return null;
		}
	}

	private string TileInspectString
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			Vector2 vector = Find.WorldGrid.LongLatOf(SelectedTile);
			stringBuilder.Append(vector.y.ToStringLatitude());
			stringBuilder.Append(" ");
			stringBuilder.Append(vector.x.ToStringLongitude());
			Tile tile = Find.WorldGrid[SelectedTile];
			if (!tile.PrimaryBiome.impassable && tile.hilliness != Hilliness.Undefined)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(tile.hilliness.GetLabelCap());
			}
			if (tile is SurfaceTile surfaceTile)
			{
				if (surfaceTile.Roads != null)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(surfaceTile.Roads.Select((SurfaceTile.RoadLink rl) => rl.road).MaxBy((RoadDef road) => road.priority).LabelCap);
				}
				if (!Find.World.Impassable(SelectedTile))
				{
					string arg = (WorldPathGrid.CalculatedMovementDifficultyAt(SelectedTile, perceivedStatic: false) * Find.WorldGrid.GetRoadMovementDifficultyMultiplier(SelectedTile, PlanetTile.Invalid)).ToString("0.#");
					stringBuilder.AppendLine();
					stringBuilder.Append(string.Format("{0}: {1}", "MovementDifficulty".Translate(), arg));
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.Append(string.Format("{0}: {1}", "AvgTemp".Translate(), GenTemperature.GetAverageTemperatureLabel(SelectedTile)));
			if (ModsConfig.BiotechActive && tile.pollution > 0f && tile.OnSurface)
			{
				stringBuilder.AppendLine();
				string pollutionDescription = GenWorld.GetPollutionDescription(tile.pollution);
				pollutionDescription = pollutionDescription + " (" + tile.pollution.ToStringPercent() + ")";
				stringBuilder.Append(string.Format("{0}: {1}", "TilePollution".Translate(), pollutionDescription));
			}
			if (ModsConfig.OdysseyActive && tile.Mutators.Any())
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("TileMutators".Translate() + ": " + (from m in tile.Mutators
					orderby -m.displayPriority
					select m.Label(tile.tile)).ToCommaList().CapitalizeFirst());
			}
			return stringBuilder.ToString();
		}
	}

	public WorldInspectPane()
	{
		layer = WindowLayer.GameUI;
		soundAppear = null;
		soundClose = null;
		closeOnClickedOutside = false;
		closeOnAccept = false;
		closeOnCancel = false;
		preventCameraMotion = false;
	}

	protected override void SetInitialSizeAndPosition()
	{
		base.SetInitialSizeAndPosition();
		windowRect.x = 0f;
		windowRect.y = PaneTopY;
	}

	public string GetLabel(Rect rect)
	{
		if (NumSelectedObjects > 0)
		{
			return WorldInspectPaneUtility.AdjustedLabelFor(Selected, rect);
		}
		if (SelectedTile.Valid)
		{
			if (ModsConfig.OdysseyActive && Find.World.landmarks[SelectedTile] != null)
			{
				return Find.World.landmarks[SelectedTile].name;
			}
			return Find.WorldGrid[SelectedTile].PrimaryBiome.LabelCap;
		}
		return "error";
	}

	public void SelectNextInCell()
	{
		if (AnythingSelected)
		{
			if (NumSelectedObjects > 0)
			{
				Find.WorldSelector.SelectFirstOrNextAt(Selected[0].Tile);
			}
			else
			{
				Find.WorldSelector.SelectFirstOrNextAt(SelectedTile);
			}
		}
	}

	public void DoPaneContents(Rect rect)
	{
		if (NumSelectedObjects > 0)
		{
			InspectPaneFiller.DoPaneContentsFor(Find.WorldSelector.FirstSelectedObject, rect);
		}
		else if (SelectedTile.Valid)
		{
			InspectPaneFiller.DrawInspectString(TileInspectString, rect);
		}
	}

	public void DoInspectPaneButtons(Rect rect, ref float lineEndWidth)
	{
		WorldObject singleSelectedObject = Find.WorldSelector.SingleSelectedObject;
		if (singleSelectedObject != null || SelectedTile.Valid)
		{
			float x = rect.width - 48f;
			if (singleSelectedObject != null)
			{
				Widgets.InfoCardButton(x, 0f, singleSelectedObject);
			}
			else if (Find.WorldGrid[SelectedTile].PrimaryBiome != null)
			{
				Widgets.InfoCardButton(x, 0f, Find.WorldGrid[SelectedTile].PrimaryBiome);
			}
			lineEndWidth += 24f;
		}
	}

	public override void DoWindowContents(Rect rect)
	{
		InspectPaneUtility.InspectPaneOnGUI(rect, this);
	}

	public override void WindowUpdate()
	{
		base.WindowUpdate();
		InspectPaneUtility.UpdateTabs(this);
	}

	public override void ExtraOnGUI()
	{
		base.ExtraOnGUI();
		InspectPaneUtility.ExtraOnGUI(this);
	}

	public void CloseOpenTab()
	{
		openTabType = null;
	}

	public void Reset()
	{
		openTabType = null;
	}
}
