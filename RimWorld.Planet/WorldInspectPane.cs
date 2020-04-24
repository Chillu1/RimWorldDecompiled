using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldInspectPane : Window, IInspectPane
	{
		private static readonly WITab[] TileTabs = new WITab[2]
		{
			new WITab_Terrain(),
			new WITab_Planet()
		};

		private Type openTabType;

		private float recentHeight;

		public Gizmo mouseoverGizmo;

		private static List<object> tmpObjectsList = new List<object>();

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

		public float RecentHeight
		{
			get
			{
				return recentHeight;
			}
			set
			{
				recentHeight = value;
			}
		}

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

		private int SelectedTile => Find.WorldSelector.selectedTile;

		private bool SelectedSingleObjectOrTile
		{
			get
			{
				if (NumSelectedObjects != 1)
				{
					if (NumSelectedObjects == 0)
					{
						return SelectedTile >= 0;
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
				if (NumSelectedObjects == 0 && SelectedTile >= 0)
				{
					return TileTabs;
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
				if (!tile.biome.impassable)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(tile.hilliness.GetLabelCap());
				}
				if (tile.Roads != null)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(tile.Roads.Select((Tile.RoadLink rl) => rl.road).MaxBy((RoadDef road) => road.priority).LabelCap);
				}
				if (!Find.World.Impassable(SelectedTile))
				{
					string t = (WorldPathGrid.CalculatedMovementDifficultyAt(SelectedTile, perceivedStatic: false) * Find.WorldGrid.GetRoadMovementDifficultyMultiplier(SelectedTile, -1)).ToString("0.#");
					stringBuilder.AppendLine();
					stringBuilder.Append("MovementDifficulty".Translate() + ": " + t);
				}
				stringBuilder.AppendLine();
				stringBuilder.Append("AvgTemp".Translate() + ": " + GenTemperature.GetAverageTemperatureLabel(SelectedTile));
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

		public void DrawInspectGizmos()
		{
			tmpObjectsList.Clear();
			WorldRoutePlanner worldRoutePlanner = Find.WorldRoutePlanner;
			List<WorldObject> selected = Selected;
			for (int i = 0; i < selected.Count; i++)
			{
				if (!worldRoutePlanner.Active || selected[i] is RoutePlannerWaypoint)
				{
					tmpObjectsList.Add(selected[i]);
				}
			}
			InspectGizmoGrid.DrawInspectGizmoGridFor(tmpObjectsList, out mouseoverGizmo);
			tmpObjectsList.Clear();
		}

		public string GetLabel(Rect rect)
		{
			if (NumSelectedObjects > 0)
			{
				return WorldInspectPaneUtility.AdjustedLabelFor(Selected, rect);
			}
			if (SelectedTile >= 0)
			{
				return Find.WorldGrid[SelectedTile].biome.LabelCap;
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
			else if (SelectedTile >= 0)
			{
				InspectPaneFiller.DrawInspectString(TileInspectString, rect);
			}
		}

		public void DoInspectPaneButtons(Rect rect, ref float lineEndWidth)
		{
			WorldObject singleSelectedObject = Find.WorldSelector.SingleSelectedObject;
			if (singleSelectedObject != null || SelectedTile >= 0)
			{
				float x = rect.width - 48f;
				if (singleSelectedObject != null)
				{
					Widgets.InfoCardButton(x, 0f, singleSelectedObject);
				}
				else
				{
					Widgets.InfoCardButton(x, 0f, Find.WorldGrid[SelectedTile].biome);
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
			if (mouseoverGizmo != null)
			{
				mouseoverGizmo.GizmoUpdateOnMouseover();
			}
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
}
