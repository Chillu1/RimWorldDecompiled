using System;
using UnityEngine;
using Verse;

namespace RimWorld;

public class HistoryAutoRecorderDef : Def
{
	public Type workerClass;

	public int recordTicksFrequency = 60000;

	public Color graphColor = Color.green;

	[MustTranslate]
	public string graphLabelY;

	public string valueFormat;

	[Unsaved(false)]
	private HistoryAutoRecorderWorker workerInt;

	public HistoryAutoRecorderWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (HistoryAutoRecorderWorker)Activator.CreateInstance(workerClass);
			}
			return workerInt;
		}
	}

	public static HistoryAutoRecorderDef Named(string defName)
	{
		return DefDatabase<HistoryAutoRecorderDef>.GetNamed(defName);
	}
}
