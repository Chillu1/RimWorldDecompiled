using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class DesignationDragger
	{
		private bool dragging;

		private IntVec3 startDragCell;

		private int lastFrameDragCellsDrawn;

		private Sustainer sustainer;

		private float lastDragRealTime = -1000f;

		private List<IntVec3> dragCells = new List<IntVec3>();

		private string failureReasonInt;

		private int lastUpdateFrame = -1;

		private const int MaxSquareWidth = 50;

		public bool Dragging => dragging;

		private Designator SelDes => Find.DesignatorManager.SelectedDesignator;

		public List<IntVec3> DragCells
		{
			get
			{
				UpdateDragCellsIfNeeded();
				return dragCells;
			}
		}

		public string FailureReason
		{
			get
			{
				UpdateDragCellsIfNeeded();
				return failureReasonInt;
			}
		}

		public void StartDrag()
		{
			dragging = true;
			startDragCell = UI.MouseCell();
		}

		public void EndDrag()
		{
			dragging = false;
			lastDragRealTime = -99999f;
			lastFrameDragCellsDrawn = 0;
			if (sustainer != null)
			{
				sustainer.End();
				sustainer = null;
			}
		}

		public void DraggerUpdate()
		{
			if (!dragging)
			{
				return;
			}
			List<IntVec3> list = DragCells;
			SelDes.RenderHighlight(list);
			if (list.Count != lastFrameDragCellsDrawn)
			{
				lastDragRealTime = Time.realtimeSinceStartup;
				lastFrameDragCellsDrawn = list.Count;
				if (SelDes.soundDragChanged != null)
				{
					SelDes.soundDragChanged.PlayOneShotOnCamera();
				}
			}
			if (sustainer == null || sustainer.Ended)
			{
				if (SelDes.soundDragSustain != null)
				{
					sustainer = SelDes.soundDragSustain.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerFrame));
				}
			}
			else
			{
				sustainer.externalParams["TimeSinceDrag"] = Time.realtimeSinceStartup - lastDragRealTime;
				sustainer.Maintain();
			}
		}

		public void DraggerOnGUI()
		{
			if (dragging && SelDes != null && SelDes.DragDrawMeasurements)
			{
				IntVec3 intVec = startDragCell - UI.MouseCell();
				intVec.x = Mathf.Abs(intVec.x) + 1;
				intVec.z = Mathf.Abs(intVec.z) + 1;
				if (intVec.x >= 3)
				{
					Vector2 screenPos = (startDragCell.ToUIPosition() + UI.MouseCell().ToUIPosition()) / 2f;
					screenPos.y = startDragCell.ToUIPosition().y;
					Widgets.DrawNumberOnMap(screenPos, intVec.x, Color.white);
				}
				if (intVec.z >= 3)
				{
					Vector2 screenPos2 = (startDragCell.ToUIPosition() + UI.MouseCell().ToUIPosition()) / 2f;
					screenPos2.x = startDragCell.ToUIPosition().x;
					Widgets.DrawNumberOnMap(screenPos2, intVec.z, Color.white);
				}
			}
		}

		[Obsolete]
		private void DrawNumber(Vector2 screenPos, int number)
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(screenPos.x - 20f, screenPos.y - 15f, 40f, 30f);
			GUI.DrawTexture(rect, TexUI.GrayBg);
			rect.y += 3f;
			Widgets.Label(rect, number.ToStringCached());
		}

		private void UpdateDragCellsIfNeeded()
		{
			if (Time.frameCount == lastUpdateFrame)
			{
				return;
			}
			lastUpdateFrame = Time.frameCount;
			dragCells.Clear();
			failureReasonInt = null;
			IntVec3 intVec = startDragCell;
			IntVec3 intVec2 = UI.MouseCell();
			if (SelDes.DraggableDimensions == 1)
			{
				bool flag = true;
				if (Mathf.Abs(intVec.x - intVec2.x) < Mathf.Abs(intVec.z - intVec2.z))
				{
					flag = false;
				}
				if (flag)
				{
					int z = intVec.z;
					if (intVec.x > intVec2.x)
					{
						IntVec3 intVec3 = intVec;
						intVec = intVec2;
						intVec2 = intVec3;
					}
					for (int i = intVec.x; i <= intVec2.x; i++)
					{
						TryAddDragCell(new IntVec3(i, intVec.y, z));
					}
				}
				else
				{
					int x = intVec.x;
					if (intVec.z > intVec2.z)
					{
						IntVec3 intVec4 = intVec;
						intVec = intVec2;
						intVec2 = intVec4;
					}
					for (int j = intVec.z; j <= intVec2.z; j++)
					{
						TryAddDragCell(new IntVec3(x, intVec.y, j));
					}
				}
			}
			if (SelDes.DraggableDimensions != 2)
			{
				return;
			}
			IntVec3 intVec5 = intVec;
			IntVec3 intVec6 = intVec2;
			if (intVec6.x > intVec5.x + 50)
			{
				intVec6.x = intVec5.x + 50;
			}
			if (intVec6.z > intVec5.z + 50)
			{
				intVec6.z = intVec5.z + 50;
			}
			if (intVec6.x < intVec5.x)
			{
				if (intVec6.x < intVec5.x - 50)
				{
					intVec6.x = intVec5.x - 50;
				}
				int x2 = intVec5.x;
				intVec5 = new IntVec3(intVec6.x, intVec5.y, intVec5.z);
				intVec6 = new IntVec3(x2, intVec6.y, intVec6.z);
			}
			if (intVec6.z < intVec5.z)
			{
				if (intVec6.z < intVec5.z - 50)
				{
					intVec6.z = intVec5.z - 50;
				}
				int z2 = intVec5.z;
				intVec5 = new IntVec3(intVec5.x, intVec5.y, intVec6.z);
				intVec6 = new IntVec3(intVec6.x, intVec6.y, z2);
			}
			for (int k = intVec5.x; k <= intVec6.x; k++)
			{
				for (int l = intVec5.z; l <= intVec6.z; l++)
				{
					TryAddDragCell(new IntVec3(k, intVec5.y, l));
				}
			}
		}

		private void TryAddDragCell(IntVec3 c)
		{
			AcceptanceReport acceptanceReport = SelDes.CanDesignateCell(c);
			if (acceptanceReport.Accepted)
			{
				dragCells.Add(c);
			}
			else if (!acceptanceReport.Reason.NullOrEmpty())
			{
				failureReasonInt = acceptanceReport.Reason;
			}
		}
	}
}
