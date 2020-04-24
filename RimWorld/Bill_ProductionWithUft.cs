using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Bill_ProductionWithUft : Bill_Production
	{
		private UnfinishedThing boundUftInt;

		protected override string StatusString
		{
			get
			{
				if (BoundWorker == null)
				{
					return (base.StatusString ?? "").Trim();
				}
				return ("BoundWorkerIs".Translate(BoundWorker.LabelShort, BoundWorker) + base.StatusString).Trim();
			}
		}

		public Pawn BoundWorker
		{
			get
			{
				if (boundUftInt == null)
				{
					return null;
				}
				Pawn creator = boundUftInt.Creator;
				if (creator == null || creator.Downed || creator.HostFaction != null || creator.Destroyed || !creator.Spawned)
				{
					boundUftInt = null;
					return null;
				}
				Thing thing = billStack.billGiver as Thing;
				if (thing != null)
				{
					WorkTypeDef workTypeDef = null;
					List<WorkGiverDef> allDefsListForReading = DefDatabase<WorkGiverDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].fixedBillGiverDefs != null && allDefsListForReading[i].fixedBillGiverDefs.Contains(thing.def))
						{
							workTypeDef = allDefsListForReading[i].workType;
							break;
						}
					}
					if (workTypeDef != null && !creator.workSettings.WorkIsActive(workTypeDef))
					{
						boundUftInt = null;
						return null;
					}
				}
				return creator;
			}
		}

		public UnfinishedThing BoundUft => boundUftInt;

		public void SetBoundUft(UnfinishedThing value, bool setOtherLink = true)
		{
			if (value == boundUftInt)
			{
				return;
			}
			UnfinishedThing unfinishedThing = boundUftInt;
			boundUftInt = value;
			if (setOtherLink)
			{
				if (unfinishedThing != null && unfinishedThing.BoundBill == this)
				{
					unfinishedThing.BoundBill = null;
				}
				if (value != null && value.BoundBill != this)
				{
					boundUftInt.BoundBill = this;
				}
			}
		}

		public Bill_ProductionWithUft()
		{
		}

		public Bill_ProductionWithUft(RecipeDef recipe)
			: base(recipe)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref boundUftInt, "boundUft");
		}

		public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
			ClearBoundUft();
			base.Notify_IterationCompleted(billDoer, ingredients);
		}

		public void ClearBoundUft()
		{
			boundUftInt = null;
		}

		public override Bill Clone()
		{
			return (Bill_Production)base.Clone();
		}
	}
}
