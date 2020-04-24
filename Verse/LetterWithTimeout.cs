using UnityEngine;

namespace Verse
{
	public abstract class LetterWithTimeout : Letter
	{
		public int disappearAtTick = -1;

		public bool TimeoutActive => disappearAtTick >= 0;

		public bool TimeoutPassed
		{
			get
			{
				if (TimeoutActive)
				{
					return Find.TickManager.TicksGame >= disappearAtTick;
				}
				return false;
			}
		}

		public override bool CanShowInLetterStack
		{
			get
			{
				if (!base.CanShowInLetterStack)
				{
					return false;
				}
				if (TimeoutPassed)
				{
					return false;
				}
				return true;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref disappearAtTick, "disappearAtTick", -1);
		}

		public void StartTimeout(int duration)
		{
			disappearAtTick = Find.TickManager.TicksGame + duration;
		}

		protected override string PostProcessedLabel()
		{
			string text = base.PostProcessedLabel();
			if (TimeoutActive)
			{
				int num = Mathf.RoundToInt((float)(disappearAtTick - Find.TickManager.TicksGame) / 2500f);
				text += " (" + num + "LetterHour".Translate() + ")";
			}
			return text;
		}
	}
}
