using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetHediff : QuestNode
	{
		public class Option
		{
			public HediffDef def;

			public List<BodyPartDef> partsToAffect;

			public float weight = 1f;
		}

		[NoTranslate]
		public SlateRef<string> storeHediffAs;

		[NoTranslate]
		public SlateRef<string> storePartsToAffectAs;

		public SlateRef<List<Option>> options;

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
			Option option = options.GetValue(slate).RandomElementByWeight((Option x) => x.weight);
			slate.Set(storeHediffAs.GetValue(slate), option.def);
			if (storePartsToAffectAs.GetValue(slate) != null)
			{
				slate.Set(storePartsToAffectAs.GetValue(slate), option.partsToAffect);
			}
		}
	}
}
