using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Page_SelectStartingSite : Page
	{
		private const float GapBetweenBottomButtons = 10f;

		private const float UseTwoRowsIfScreenWidthBelow = 1340f;

		private static List<Vector3> tmpTileVertices = new List<Vector3>();

		private int? tutorialStartTilePatch;

		public override string PageTitle => "SelectStartingSite".TranslateWithBackup("SelectLandingSite");

		public override Vector2 InitialSize => Vector2.zero;

		protected override float Margin => 0f;

		public Page_SelectStartingSite()
		{
			absorbInputAroundWindow = false;
			shadowAlpha = 0f;
			preventCameraMotion = false;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.Planet;
			Find.WorldInterface.Reset();
			((MainButtonWorker_ToggleWorld)MainButtonDefOf.World.Worker).resetViewNextTime = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			Find.GameInitData.ChooseRandomStartingTile();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.WorldCameraMovement, OpportunityType.Important);
			TutorSystem.Notify_Event("PageStart-SelectStartingSite");
			tutorialStartTilePatch = null;
			if (!TutorSystem.TutorialMode || Find.Tutor.activeLesson == null || Find.Tutor.activeLesson.Current == null || Find.Tutor.activeLesson.Current.Instruction != InstructionDefOf.ChooseLandingSite)
			{
				return;
			}
			Find.WorldCameraDriver.ResetAltitude();
			Find.WorldCameraDriver.Update();
			List<int> list = new List<int>();
			float[] array = new float[Find.WorldGrid.TilesCount];
			WorldGrid worldGrid = Find.WorldGrid;
			Vector2 a = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
			float num = Vector2.Distance(a, Vector2.zero);
			for (int i = 0; i < worldGrid.TilesCount; i++)
			{
				Tile tile = worldGrid[i];
				if (TutorSystem.AllowAction("ChooseBiome-" + tile.biome.defName + "-" + tile.hilliness.ToString()))
				{
					tmpTileVertices.Clear();
					worldGrid.GetTileVertices(i, tmpTileVertices);
					Vector3 zero = Vector3.zero;
					for (int j = 0; j < tmpTileVertices.Count; j++)
					{
						zero += tmpTileVertices[j];
					}
					zero /= (float)tmpTileVertices.Count;
					Vector3 v = Find.WorldCamera.WorldToScreenPoint(zero) / Prefs.UIScale;
					v.y = (float)UI.screenHeight - v.y;
					v.x = Mathf.Clamp(v.x, 0f, UI.screenWidth);
					v.y = Mathf.Clamp(v.y, 0f, UI.screenHeight);
					float num2 = 1f - Vector2.Distance(a, v) / num;
					Vector3 normalized = (zero - Find.WorldCamera.transform.position).normalized;
					float num3 = Vector3.Dot(Find.WorldCamera.transform.forward, normalized);
					array[i] = num2 * num3;
				}
				else
				{
					array[i] = float.NegativeInfinity;
				}
			}
			for (int k = 0; k < 16; k++)
			{
				for (int l = 0; l < array.Length; l++)
				{
					list.Clear();
					worldGrid.GetTileNeighbors(l, list);
					float num4 = array[l];
					if (num4 < 0f)
					{
						continue;
					}
					for (int m = 0; m < list.Count; m++)
					{
						float num5 = array[list[m]];
						if (!(num5 < 0f))
						{
							num4 += num5;
						}
					}
					array[l] = num4 / (float)list.Count;
				}
			}
			float num6 = float.NegativeInfinity;
			int num7 = -1;
			for (int n = 0; n < array.Length; n++)
			{
				if (array[n] > 0f && num6 < array[n])
				{
					num6 = array[n];
					num7 = n;
				}
			}
			if (num7 != -1)
			{
				tutorialStartTilePatch = num7;
			}
		}

		public override void PostClose()
		{
			base.PostClose();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}

		public override void DoWindowContents(Rect rect)
		{
			if (Find.WorldInterface.SelectedTile >= 0)
			{
				Find.GameInitData.startingTile = Find.WorldInterface.SelectedTile;
			}
			else if (Find.WorldSelector.FirstSelectedObject != null)
			{
				Find.GameInitData.startingTile = Find.WorldSelector.FirstSelectedObject.Tile;
			}
		}

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			Text.Anchor = TextAnchor.UpperCenter;
			DrawPageTitle(new Rect(0f, 5f, UI.screenWidth, 300f));
			Text.Anchor = TextAnchor.UpperLeft;
			DoCustomBottomButtons();
			if (tutorialStartTilePatch.HasValue)
			{
				tmpTileVertices.Clear();
				Find.WorldGrid.GetTileVertices(tutorialStartTilePatch.Value, tmpTileVertices);
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < tmpTileVertices.Count; i++)
				{
					zero += tmpTileVertices[i];
				}
				Color color = GUI.color;
				GUI.color = Color.white;
				GenUI.DrawArrowPointingAtWorldspace(zero / tmpTileVertices.Count, Find.WorldCamera);
				GUI.color = color;
			}
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			int selectedTile = Find.WorldInterface.SelectedTile;
			if (selectedTile < 0)
			{
				Messages.Message("MustSelectStartingSite".TranslateWithBackup("MustSelectLandingSite"), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (!TileFinder.IsValidTileForNewSettlement(selectedTile, stringBuilder))
			{
				Messages.Message(stringBuilder.ToString(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			Tile tile = Find.WorldGrid[selectedTile];
			if (!TutorSystem.AllowAction("ChooseBiome-" + tile.biome.defName + "-" + tile.hilliness.ToString()))
			{
				return false;
			}
			return true;
		}

		protected override void DoNext()
		{
			int selTile = Find.WorldInterface.SelectedTile;
			SettlementProximityGoodwillUtility.CheckConfirmSettle(selTile, delegate
			{
				Find.GameInitData.startingTile = selTile;
				base.DoNext();
			});
		}

		private void DoCustomBottomButtons()
		{
			int num = TutorSystem.TutorialMode ? 4 : 5;
			int num2 = (num < 4 || !((float)UI.screenWidth < 1340f)) ? 1 : 2;
			int num3 = Mathf.CeilToInt((float)num / (float)num2);
			float num4 = Page.BottomButSize.x * (float)num3 + 10f * (float)(num3 + 1);
			float num5 = (float)num2 * Page.BottomButSize.y + 10f * (float)(num2 + 1);
			Rect rect = new Rect(((float)UI.screenWidth - num4) / 2f, (float)UI.screenHeight - num5 - 4f, num4, num5);
			WorldInspectPane worldInspectPane = Find.WindowStack.WindowOfType<WorldInspectPane>();
			if (worldInspectPane != null && rect.x < InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f)
			{
				rect.x = InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f;
			}
			Widgets.DrawWindowBackground(rect);
			float num6 = rect.xMin + 10f;
			float num7 = rect.yMin + 10f;
			Text.Font = GameFont.Small;
			if (Widgets.ButtonText(new Rect(num6, num7, Page.BottomButSize.x, Page.BottomButSize.y), "Back".Translate()) && CanDoBack())
			{
				DoBack();
			}
			num6 += Page.BottomButSize.x + 10f;
			if (!TutorSystem.TutorialMode)
			{
				if (Widgets.ButtonText(new Rect(num6, num7, Page.BottomButSize.x, Page.BottomButSize.y), "Advanced".Translate()))
				{
					Find.WindowStack.Add(new Dialog_AdvancedGameConfig(Find.WorldInterface.SelectedTile));
				}
				num6 += Page.BottomButSize.x + 10f;
			}
			if (Widgets.ButtonText(new Rect(num6, num7, Page.BottomButSize.x, Page.BottomButSize.y), "SelectRandomSite".Translate()))
			{
				SoundDefOf.Click.PlayOneShotOnCamera();
				Find.WorldInterface.SelectedTile = TileFinder.RandomStartingTile();
				Find.WorldCameraDriver.JumpTo(Find.WorldGrid.GetTileCenter(Find.WorldInterface.SelectedTile));
			}
			num6 += Page.BottomButSize.x + 10f;
			if (num2 == 2)
			{
				num6 = rect.xMin + 10f;
				num7 += Page.BottomButSize.y + 10f;
			}
			if (Widgets.ButtonText(new Rect(num6, num7, Page.BottomButSize.x, Page.BottomButSize.y), "WorldFactionsTab".Translate()))
			{
				Find.WindowStack.Add(new Dialog_FactionDuringLanding());
			}
			num6 += Page.BottomButSize.x + 10f;
			if (Widgets.ButtonText(new Rect(num6, num7, Page.BottomButSize.x, Page.BottomButSize.y), "Next".Translate()) && CanDoNext())
			{
				DoNext();
			}
			num6 += Page.BottomButSize.x + 10f;
			GenUI.AbsorbClicksInRect(rect);
		}
	}
}
