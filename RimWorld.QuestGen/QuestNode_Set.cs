using System;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Set : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> name;

		public SlateRef<object> value;

		public SlateRef<Type> convertTo;

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
			object obj = value.GetValue(slate);
			if (convertTo.GetValue(slate) != null)
			{
				obj = ConvertHelper.Convert(obj, convertTo.GetValue(slate));
			}
			slate.Set(name.GetValue(slate), obj);
		}
	}
}
