using System.Collections.Generic;
using System.Linq;

namespace Verse.AI.Group
{
	public class Trigger_TicksPassedWithoutHarmOrMemos : Trigger_TicksPassed
	{
		private List<string> memos;

		public Trigger_TicksPassedWithoutHarmOrMemos(int tickLimit, params string[] memos)
			: base(tickLimit)
		{
			this.memos = memos.ToList();
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (Trigger_PawnHarmed.SignalIsHarm(signal) || memos.Contains(signal.memo))
			{
				base.Data.ticksPassed = 0;
			}
			return base.ActivateOn(lord, signal);
		}
	}
}
