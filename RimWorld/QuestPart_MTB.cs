using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class QuestPart_MTB : QuestPartActivable
	{
		private const int CheckIntervalTicks = 10;

		protected abstract float MTBDays { get; }

		public override void QuestPartTick()
		{
			float mTBDays = MTBDays;
			if (mTBDays > 0f && Find.TickManager.TicksGame % 10 == 0 && Rand.MTBEventOccurs(mTBDays, 60000f, 10f))
			{
				Complete();
			}
		}

		public override void DoDebugWindowContents(Rect innerRect, ref float curY)
		{
			if (base.State == QuestPartState.Enabled)
			{
				Rect rect = new Rect(innerRect.x, curY, 500f, 25f);
				if (Widgets.ButtonText(rect, "MTB occurs " + ToString()))
				{
					Complete();
				}
				curY += rect.height + 4f;
			}
		}
	}
}
