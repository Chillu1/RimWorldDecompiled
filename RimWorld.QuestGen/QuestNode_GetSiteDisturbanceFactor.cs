using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetSiteDisturbanceFactor : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<IEnumerable<SitePartDef>> sitePartDefs;

		protected override bool TestRunInt(Slate slate)
		{
			SetVars(slate);
			return true;
		}

		protected override void RunInt()
		{
			SetVars(QuestGen.slate);
		}

		private void SetVars(Slate slate)
		{
			float num = 1f;
			IEnumerable<SitePartDef> value = sitePartDefs.GetValue(slate);
			if (value != null)
			{
				foreach (SitePartDef item in value)
				{
					num *= item.activeThreatDisturbanceFactor;
				}
			}
			slate.Set(storeAs.GetValue(slate), num);
		}
	}
}
