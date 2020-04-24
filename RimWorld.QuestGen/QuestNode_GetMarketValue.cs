using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetMarketValue : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<IEnumerable<Thing>> things;

		protected override bool TestRunInt(Slate slate)
		{
			return DoWork(slate);
		}

		protected override void RunInt()
		{
			DoWork(QuestGen.slate);
		}

		private bool DoWork(Slate slate)
		{
			float num = 0f;
			if (things.GetValue(slate) != null)
			{
				foreach (Thing item in things.GetValue(slate))
				{
					num += item.MarketValue * (float)item.stackCount;
				}
			}
			slate.Set(storeAs.GetValue(slate), num);
			return true;
		}
	}
}
