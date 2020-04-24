using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Building_WorkTable : Building, IBillGiver, IBillGiverWithTickAction
	{
		public BillStack billStack;

		private CompPowerTrader powerComp;

		private CompRefuelable refuelableComp;

		private CompBreakdownable breakdownableComp;

		public bool CanWorkWithoutPower
		{
			get
			{
				if (powerComp == null)
				{
					return true;
				}
				if (def.building.unpoweredWorkTableWorkSpeedFactor > 0f)
				{
					return true;
				}
				return false;
			}
		}

		public bool CanWorkWithoutFuel => refuelableComp == null;

		public BillStack BillStack => billStack;

		public IntVec3 BillInteractionCell => InteractionCell;

		public IEnumerable<IntVec3> IngredientStackCells => GenAdj.CellsOccupiedBy(this);

		public Building_WorkTable()
		{
			billStack = new BillStack(this);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref billStack, "billStack", this);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			refuelableComp = GetComp<CompRefuelable>();
			breakdownableComp = GetComp<CompBreakdownable>();
			foreach (Bill item in billStack)
			{
				item.ValidateSettings();
			}
		}

		public virtual void UsedThisTick()
		{
			if (refuelableComp != null)
			{
				refuelableComp.Notify_UsedThisTick();
			}
		}

		public bool CurrentlyUsableForBills()
		{
			if (!UsableForBillsAfterFueling())
			{
				return false;
			}
			if (!CanWorkWithoutPower && (powerComp == null || !powerComp.PowerOn))
			{
				return false;
			}
			if (!CanWorkWithoutFuel && (refuelableComp == null || !refuelableComp.HasFuel))
			{
				return false;
			}
			return true;
		}

		public bool UsableForBillsAfterFueling()
		{
			if (!CanWorkWithoutPower && (powerComp == null || !powerComp.PowerOn))
			{
				return false;
			}
			if (breakdownableComp != null && breakdownableComp.BrokenDown)
			{
				return false;
			}
			return true;
		}
	}
}
