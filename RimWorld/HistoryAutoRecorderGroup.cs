using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class HistoryAutoRecorderGroup : IExposable
{
	public HistoryAutoRecorderGroupDef def;

	public List<HistoryAutoRecorder> recorders = new List<HistoryAutoRecorder>();

	private List<SimpleCurveDrawInfo> curves = new List<SimpleCurveDrawInfo>();

	private int cachedGraphTickCount = -1;

	public float GetMaxDay()
	{
		float num = 0f;
		foreach (HistoryAutoRecorder recorder in recorders)
		{
			int count = recorder.records.Count;
			if (count != 0)
			{
				float num2 = (float)((count - 1) * recorder.def.recordTicksFrequency) / 60000f;
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public void Tick()
	{
		for (int i = 0; i < recorders.Count; i++)
		{
			recorders[i].Tick();
		}
	}

	public void DrawGraph(Rect graphRect, Rect legendRect, FloatRange section, List<CurveMark> marks)
	{
		int ticksGame = Find.TickManager.TicksGame;
		if (ticksGame != cachedGraphTickCount)
		{
			cachedGraphTickCount = ticksGame;
			curves.Clear();
			for (int i = 0; i < recorders.Count; i++)
			{
				HistoryAutoRecorder historyAutoRecorder = recorders[i];
				SimpleCurveDrawInfo simpleCurveDrawInfo = new SimpleCurveDrawInfo();
				simpleCurveDrawInfo.color = historyAutoRecorder.def.graphColor;
				simpleCurveDrawInfo.label = historyAutoRecorder.def.LabelCap;
				simpleCurveDrawInfo.valueFormat = historyAutoRecorder.def.valueFormat;
				simpleCurveDrawInfo.curve = new SimpleCurve();
				for (int j = 0; j < historyAutoRecorder.records.Count; j++)
				{
					simpleCurveDrawInfo.curve.Add(new CurvePoint((float)j * (float)historyAutoRecorder.def.recordTicksFrequency / 60000f, historyAutoRecorder.records[j]), sort: false);
				}
				simpleCurveDrawInfo.curve.SortPoints();
				if (historyAutoRecorder.records.Count == 1)
				{
					simpleCurveDrawInfo.curve.Add(new CurvePoint(1.6666667E-05f, historyAutoRecorder.records[0]));
				}
				curves.Add(simpleCurveDrawInfo);
			}
		}
		if (Mathf.Approximately(section.min, section.max))
		{
			section.max += 1.6666667E-05f;
		}
		SimpleCurveDrawerStyle curveDrawerStyle = Find.History.curveDrawerStyle;
		curveDrawerStyle.FixedSection = section;
		curveDrawerStyle.UseFixedScale = def.useFixedScale;
		curveDrawerStyle.FixedScale = def.fixedScale;
		curveDrawerStyle.YIntegersOnly = def.integersOnly;
		curveDrawerStyle.OnlyPositiveValues = def.onlyPositiveValues;
		SimpleCurveDrawer.DrawCurves(graphRect, curves, curveDrawerStyle, marks, legendRect);
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Collections.Look(ref recorders, "recorders", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			AddOrRemoveHistoryRecorders();
		}
	}

	public void AddOrRemoveHistoryRecorders()
	{
		if (recorders.RemoveAll((HistoryAutoRecorder x) => x == null) != 0)
		{
			Log.Warning("Some history auto recorders were null.");
		}
		foreach (HistoryAutoRecorderDef recorderDef in def.historyAutoRecorderDefs)
		{
			if (!recorders.Any((HistoryAutoRecorder x) => x.def == recorderDef))
			{
				HistoryAutoRecorder historyAutoRecorder = new HistoryAutoRecorder();
				historyAutoRecorder.def = recorderDef;
				recorders.Add(historyAutoRecorder);
			}
		}
		recorders.RemoveAll((HistoryAutoRecorder x) => x.def == null);
	}
}
