using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class History : IExposable
	{
		public Archive archive = new Archive();

		private List<HistoryAutoRecorderGroup> autoRecorderGroups;

		public SimpleCurveDrawerStyle curveDrawerStyle;

		public History()
		{
			autoRecorderGroups = new List<HistoryAutoRecorderGroup>();
			AddOrRemoveHistoryRecorderGroups();
			curveDrawerStyle = new SimpleCurveDrawerStyle();
			curveDrawerStyle.DrawMeasures = true;
			curveDrawerStyle.DrawPoints = false;
			curveDrawerStyle.DrawBackground = true;
			curveDrawerStyle.DrawBackgroundLines = false;
			curveDrawerStyle.DrawLegend = true;
			curveDrawerStyle.DrawCurveMousePoint = true;
			curveDrawerStyle.OnlyPositiveValues = true;
			curveDrawerStyle.UseFixedSection = true;
			curveDrawerStyle.UseAntiAliasedLines = true;
			curveDrawerStyle.PointsRemoveOptimization = true;
			curveDrawerStyle.MeasureLabelsXCount = 10;
			curveDrawerStyle.MeasureLabelsYCount = 5;
			curveDrawerStyle.XIntegersOnly = true;
			curveDrawerStyle.LabelX = "Day".Translate();
		}

		public void HistoryTick()
		{
			for (int i = 0; i < autoRecorderGroups.Count; i++)
			{
				autoRecorderGroups[i].Tick();
			}
		}

		public List<HistoryAutoRecorderGroup> Groups()
		{
			return autoRecorderGroups;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref archive, "archive");
			Scribe_Collections.Look(ref autoRecorderGroups, "autoRecorderGroups", LookMode.Deep);
			BackCompatibility.PostExposeData(this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				AddOrRemoveHistoryRecorderGroups();
			}
		}

		private void AddOrRemoveHistoryRecorderGroups()
		{
			if (autoRecorderGroups.RemoveAll((HistoryAutoRecorderGroup x) => x == null) != 0)
			{
				Log.Warning("Some history auto recorder groups were null.");
			}
			foreach (HistoryAutoRecorderGroupDef def in DefDatabase<HistoryAutoRecorderGroupDef>.AllDefs)
			{
				if (!autoRecorderGroups.Any((HistoryAutoRecorderGroup x) => x.def == def))
				{
					HistoryAutoRecorderGroup historyAutoRecorderGroup = new HistoryAutoRecorderGroup();
					historyAutoRecorderGroup.def = def;
					historyAutoRecorderGroup.AddOrRemoveHistoryRecorders();
					autoRecorderGroups.Add(historyAutoRecorderGroup);
				}
			}
			autoRecorderGroups.RemoveAll((HistoryAutoRecorderGroup x) => x.def == null);
		}
	}
}
