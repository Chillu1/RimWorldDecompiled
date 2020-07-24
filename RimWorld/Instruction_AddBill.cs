using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_AddBill : Lesson_Instruction
	{
		protected override float ProgressPercent
		{
			get
			{
				int num = def.recipeTargetCount + 1;
				int num2 = 0;
				Bill_Production bill_Production = RelevantBill();
				if (bill_Production != null)
				{
					num2++;
					if (bill_Production.repeatMode == BillRepeatModeDefOf.RepeatCount)
					{
						num2 += bill_Production.repeatCount;
					}
				}
				return (float)num2 / (float)num;
			}
		}

		private Bill_Production RelevantBill()
		{
			if (Find.Selector.SingleSelectedThing != null && Find.Selector.SingleSelectedThing.def == def.thingDef)
			{
				IBillGiver billGiver = Find.Selector.SingleSelectedThing as IBillGiver;
				if (billGiver != null)
				{
					return (Bill_Production)billGiver.BillStack.Bills.FirstOrDefault((Bill b) => b.recipe == def.recipeDef);
				}
			}
			return null;
		}

		private IEnumerable<Thing> ThingsToSelect()
		{
			if (Find.Selector.SingleSelectedThing != null && Find.Selector.SingleSelectedThing.def == def.thingDef)
			{
				yield break;
			}
			foreach (Building item in base.Map.listerBuildings.AllBuildingsColonistOfDef(def.thingDef))
			{
				yield return item;
			}
		}

		public override void LessonOnGUI()
		{
			foreach (Thing item in ThingsToSelect())
			{
				TutorUtility.DrawLabelOnThingOnGUI(item, def.onMapInstruction);
			}
			if (RelevantBill() == null)
			{
				UIHighlighter.HighlightTag("AddBill");
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			foreach (Thing item in ThingsToSelect())
			{
				GenDraw.DrawArrowPointingAt(item.DrawPos);
			}
			if (ProgressPercent > 0.999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
