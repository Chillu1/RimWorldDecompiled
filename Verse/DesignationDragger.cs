using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

[StaticConstructorOnStartup]
public class DesignationDragger
{
	private bool dragging;

	private IntVec3 startDragCell;

	private int lastFrameDragCellsDrawn;

	private Sustainer sustainer;

	private float lastDragRealTime = -1000f;

	private readonly List<IntVec3> buffer = new List<IntVec3>();

	private readonly List<IntVec3> dragCells = new List<IntVec3>();

	private string failureReasonInt;

	private int lastUpdateFrame = -1;

	private static readonly Texture2D OutlineTex = SolidColorMaterials.NewSolidColorTexture(new Color32(109, 139, 79, 100));

	private const string TimeSinceDragParam = "TimeSinceDrag";

	private readonly List<IntVec3> tmpHighlightCells = new List<IntVec3>();

	private int numSelectedCells;

	private IntVec3 lastStart;

	private IntVec3 lastEnd;

	private DrawStyleDef lastStyleDef;

	private int lastTick;

	private DrawStyleDef lastStyle;

	public bool Dragging => dragging;

	private Designator SelDes => Find.DesignatorManager.SelectedDesignator;

	private DrawStyleDef SelStyle => Find.DesignatorManager.SelectedStyle;

	public List<IntVec3> CellBuffer => buffer;

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
		dragging = !SelStyle.DrawStyleWorker.SingleCell;
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
		if (!dragging && (SelStyle?.DrawStyleWorker == null || !SelStyle.DrawStyleWorker.SingleCell))
		{
			return;
		}
		tmpHighlightCells.Clear();
		numSelectedCells = 0;
		UpdateCellBuffer();
		CellRect cellRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(3).ClipInsideMap(SelDes.Map);
		for (int num = buffer.Count - 1; num >= 0; num--)
		{
			IntVec3 intVec = buffer[num];
			if (cellRect.Contains(intVec) && (bool)SelDes.CanDesignateCell(intVec))
			{
				if (cellRect.Contains(intVec))
				{
					tmpHighlightCells.Add(intVec);
				}
				numSelectedCells++;
			}
		}
		if (SelDes.DrawHighlight)
		{
			SelDes.RenderHighlight(tmpHighlightCells);
		}
		if (numSelectedCells != lastFrameDragCellsDrawn)
		{
			if (SelDes.soundDragChanged != null)
			{
				SoundInfo info = SoundInfo.OnCamera();
				info.SetParameter("TimeSinceDrag", Time.realtimeSinceStartup - lastDragRealTime);
				SelDes.soundDragChanged.PlayOneShot(info);
			}
			lastDragRealTime = Time.realtimeSinceStartup;
			lastFrameDragCellsDrawn = numSelectedCells;
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
		if (!dragging || SelStyle?.DrawStyleWorker == null || SelStyle.DrawStyleWorker.SingleCell)
		{
			return;
		}
		(IntVec3 start, IntVec3 end) currentBoundary = GetCurrentBoundary();
		IntVec3 item = currentBoundary.start;
		IntVec3 item2 = currentBoundary.end;
		IntVec3 intVec = item - item2;
		intVec.x = Mathf.Abs(intVec.x) + 1;
		intVec.z = Mathf.Abs(intVec.z) + 1;
		if (SelStyle.drawOutline && (intVec.x > 1 || intVec.z > 1))
		{
			Vector3 v = new Vector3(Mathf.Min(item.x, item2.x), 0f, Mathf.Min(item.z, item2.z));
			Vector3 v2 = new Vector3(Mathf.Max(item.x, item2.x) + 1, 0f, Mathf.Max(item.z, item2.z) + 1);
			Vector2 vector = v.MapToUIPosition();
			Vector2 vector2 = v2.MapToUIPosition();
			Widgets.DrawBox(Rect.MinMaxRect(vector.x, vector.y, vector2.x, vector2.y), 1, OutlineTex);
		}
		if (SelDes.DragDrawMeasurements)
		{
			bool flag = intVec.x >= intVec.z;
			if (intVec.x >= 5 && (SelStyle.drawShortSideMeasurement || flag))
			{
				Vector2 screenPos = (item.ToUIPosition() + item2.ToUIPosition()) / 2f;
				screenPos.y = item.ToUIPosition().y;
				Widgets.DrawNumberOnMap(screenPos, intVec.x, Color.white);
			}
			if (intVec.z >= 5 && (SelStyle.drawShortSideMeasurement || !flag))
			{
				Vector2 screenPos2 = (item.ToUIPosition() + item2.ToUIPosition()) / 2f;
				screenPos2.x = item.ToUIPosition().x;
				Widgets.DrawNumberOnMap(screenPos2, intVec.z, Color.white);
			}
		}
		if (intVec.Magnitude >= 3f && intVec.x >= 3 && intVec.z >= 3 && lastFrameDragCellsDrawn > 0 && SelStyle.drawArea)
		{
			Widgets.DrawNumberOnMap((item.ToUIPosition() + item2.ToUIPosition()) / 2f, lastFrameDragCellsDrawn, Color.white);
		}
	}

	public void UpdateCellBuffer()
	{
		var (intVec, intVec2) = GetCurrentBoundary();
		if (lastStart == intVec && lastEnd == intVec2 && lastStyleDef == SelStyle && lastTick == GenTicks.TicksGame)
		{
			return;
		}
		buffer.Clear();
		SelStyle.DrawStyleWorker.Update(intVec, intVec2, buffer);
		lastStart = intVec;
		lastEnd = intVec2;
		lastStyleDef = SelStyle;
		lastTick = GenTicks.TicksGame;
		if (!SelStyle.DrawStyleWorker.CanHaveDuplicates)
		{
			return;
		}
		for (int num = buffer.Count - 1; num >= 0; num--)
		{
			for (int i = 0; i < num; i++)
			{
				if (buffer[num] == buffer[i])
				{
					buffer.RemoveAt(num);
					break;
				}
			}
		}
	}

	private (IntVec3 start, IntVec3 end) GetCurrentBoundary()
	{
		IntVec3 intVec = startDragCell;
		IntVec3 intVec2 = UI.MouseCell();
		if (SelStyle.DrawStyleWorker.SingleCell)
		{
			return (start: intVec2, end: intVec2);
		}
		if (KeyBindingDefOf.Designator_ShapeSnap.IsDown && SelStyle.canSnap)
		{
			IntVec3 intVec3 = intVec2 - intVec;
			int num = ((Mathf.Abs(intVec3.x) >= Mathf.Abs(intVec3.z)) ? intVec3.x : intVec3.z);
			intVec2.x = intVec.x + ((!Mathf.Approximately(Mathf.Sign(intVec3.x), Mathf.Sign(num))) ? (-num) : num);
			intVec2.z = intVec.z + ((!Mathf.Approximately(Mathf.Sign(intVec3.z), Mathf.Sign(num))) ? (-num) : num);
		}
		return (start: intVec, end: intVec2);
	}

	public void UpdateDragCellsIfNeeded()
	{
		if (Time.frameCount == lastUpdateFrame && lastStyle == SelStyle)
		{
			return;
		}
		lastUpdateFrame = Time.frameCount;
		dragCells.Clear();
		failureReasonInt = null;
		lastStyle = SelStyle;
		if (SelStyle == null)
		{
			return;
		}
		UpdateCellBuffer();
		foreach (IntVec3 item in buffer)
		{
			TryAddDragCell(new IntVec3(item.x, startDragCell.y, item.z));
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
