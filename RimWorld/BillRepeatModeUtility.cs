using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class BillRepeatModeUtility
	{
		public static void MakeConfigFloatMenu(Bill_Production bill)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption(BillRepeatModeDefOf.RepeatCount.LabelCap, delegate
			{
				bill.repeatMode = BillRepeatModeDefOf.RepeatCount;
			}));
			FloatMenuOption item = new FloatMenuOption(BillRepeatModeDefOf.TargetCount.LabelCap, delegate
			{
				if (!bill.recipe.WorkerCounter.CanCountProducts(bill))
				{
					Messages.Message("RecipeCannotHaveTargetCount".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					bill.repeatMode = BillRepeatModeDefOf.TargetCount;
				}
			});
			list.Add(item);
			list.Add(new FloatMenuOption(BillRepeatModeDefOf.Forever.LabelCap, delegate
			{
				bill.repeatMode = BillRepeatModeDefOf.Forever;
			}));
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
