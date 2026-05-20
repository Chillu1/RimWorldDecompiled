using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class PawnColumnDefGenerator
{
	public static IEnumerable<PawnColumnDef> ImpliedPawnColumnDefs(bool hotReload = false)
	{
		PawnTableDef animalsTable = PawnTableDefOf.Animals;
		foreach (TrainableDef item in DefDatabase<TrainableDef>.AllDefsListForReading.OrderByDescending((TrainableDef td) => td.listPriority))
		{
			if (!item.specialTrainable)
			{
				string defName = "Trainable_" + item.defName;
				PawnColumnDef pawnColumnDef = (hotReload ? (DefDatabase<PawnColumnDef>.GetNamed(defName, errorOnFail: false) ?? new PawnColumnDef()) : new PawnColumnDef());
				pawnColumnDef.defName = defName;
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
		}
		PawnTableDef workTable = PawnTableDefOf.Work;
		bool moveWorkTypeLabelDown = false;
		foreach (WorkTypeDef item2 in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.Where((WorkTypeDef d) => d.visible).Reverse())
		{
			moveWorkTypeLabelDown = !moveWorkTypeLabelDown;
			string defName2 = "WorkPriority_" + item2.defName;
			PawnColumnDef pawnColumnDef2 = (hotReload ? (DefDatabase<PawnColumnDef>.GetNamed(defName2, errorOnFail: false) ?? new PawnColumnDef()) : new PawnColumnDef());
			pawnColumnDef2.defName = defName2;
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
