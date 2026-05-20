using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GenerateThing : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		[NoTranslate]
		public SlateRef<string> addToList;

		public SlateRef<ThingDef> def;

		public SlateRef<int?> stackCount;

		public SlateRef<IEnumerable<Thing>> contents;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Thing thing = ThingMaker.MakeThing(def.GetValue(slate));
			thing.stackCount = stackCount.GetValue(slate) ?? 1;
			if (contents.GetValue(slate) != null)
			{
				thing.TryGetInnerInteractableThingOwner()?.TryAddRangeOrTransfer(contents.GetValue(slate));
			}
			if (storeAs.GetValue(slate) != null)
			{
				QuestGen.slate.Set(storeAs.GetValue(slate), thing);
			}
			if (addToList.GetValue(slate) != null)
			{
				QuestGenUtility.AddToOrMakeList(slate, addToList.GetValue(slate), thing);
			}
		}
	}
}
