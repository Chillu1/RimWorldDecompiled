using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PawnColumnDefgenerator
	{
		public static IEnumerable<PawnColumnDef> ImpliedPawnColumnDefs()
		{
			PawnTableDef animalsTable = PawnTableDefOf.Animals;
			foreach (TrainableDef item in DefDatabase<TrainableDef>.AllDefsListForReading.OrderByDescending((TrainableDef td) => td.listPriority))
			{
				PawnColumnDef pawnColumnDef = new PawnColumnDef();
				pawnColumnDef.defName = "Trainable_" + item.defName;
				pawnColumnDef.trainable = item;
				pawnColumnDef.headerIcon = item.icon;
				pawnColumnDef.workerClass = typeof(PawnColumnWorker_Trainable);
				pawnColumnDef.sortable = true;
				pawnColumnDef.headerTip = item.LabelCap;
				pawnColumnDef.paintable = true;
				pawnColumnDef.modContentPack = item.modContentPack;
				animalsTable.columns.Insert(animalsTable.columns.FindIndex((PawnColumnDef x) => x.Worker is PawnColumnWorker_Checkbox) - 1, pawnColumnDef);
				yield return pawnColumnDef;
			}
			PawnTableDef workTable = PawnTableDefOf.Work;
			bool moveWorkTypeLabelDown = false;
			foreach (WorkTypeDef item2 in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.Where((WorkTypeDef d) => d.visible).Reverse())
			{
				moveWorkTypeLabelDown = !moveWorkTypeLabelDown;
				PawnColumnDef pawnColumnDef2 = new PawnColumnDef();
				pawnColumnDef2.defName = "WorkPriority_" + item2.defName;
				pawnColumnDef2.workType = item2;
				pawnColumnDef2.moveWorkTypeLabelDown = moveWorkTypeLabelDown;
				pawnColumnDef2.workerClass = typeof(PawnColumnWorker_WorkPriority);
				pawnColumnDef2.sortable = true;
				pawnColumnDef2.modContentPack = item2.modContentPack;
				workTable.columns.Insert(workTable.columns.FindIndex((PawnColumnDef x) => x.Worker is PawnColumnWorker_CopyPasteWorkPriorities) + 1, pawnColumnDef2);
				yield return pawnColumnDef2;
			}
		}
	}
}
