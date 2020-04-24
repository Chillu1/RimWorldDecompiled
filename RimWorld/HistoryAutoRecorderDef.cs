using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderDef : Def
	{
		public Type workerClass;

		public int recordTicksFrequency = 60000;

		public Color graphColor = Color.green;

		[MustTranslate]
		public string graphLabelY;

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

		public string GraphLabelY
		{
			get
			{
				if (graphLabelY == null)
				{
					return "Value".TranslateSimple();
				}
				return graphLabelY;
			}
		}

		public static HistoryAutoRecorderDef Named(string defName)
		{
			return DefDatabase<HistoryAutoRecorderDef>.GetNamed(defName);
		}
	}
}
