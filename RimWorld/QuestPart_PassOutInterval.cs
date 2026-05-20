using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class QuestPart_PassOutInterval : QuestPartActivable
	{
		public IntRange ticksInterval;

		public List<string> outSignals = new List<string>();

		public List<string> inSignalsDisable = new List<string>();

		private int currentInterval;

		public override void QuestPartTick()
		{
			if (currentInterval < 0)
			{
				foreach (string outSignal in outSignals)
				{
					Find.SignalManager.SendSignal(new Signal(outSignal));
				}
				currentInterval = ticksInterval.RandomInRange;
			}
			currentInterval--;
		}

		protected override void ProcessQuestSignal(Signal signal)
		{
			if (inSignalsDisable.Contains(signal.tag))
			{
				Disable();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref outSignals, "outSignals", LookMode.Value);
			Scribe_Collections.Look(ref inSignalsDisable, "inSignalsDisable", LookMode.Value);
			Scribe_Values.Look(ref currentInterval, "currentInterval", 0);
			Scribe_Values.Look(ref ticksInterval, "ticksInterval");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				outSignals = outSignals ?? new List<string>();
				inSignalsDisable = inSignalsDisable ?? new List<string>();
			}
		}

		public override void DoDebugWindowContents(Rect innerRect, ref float curY)
		{
			if (base.State == QuestPartState.Enabled)
			{
				Rect rect = new Rect(innerRect.x, curY, 500f, 25f);
				if (Widgets.ButtonText(rect, "Reset Interval " + ToString()))
				{
					currentInterval = 0;
				}
				curY += rect.height + 4f;
			}
		}
	}
}
