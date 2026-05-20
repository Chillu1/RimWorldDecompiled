using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_WorkTableAutonomous : Building_WorkTable, IThingHolder, INotifyHauledTo
{
	public ThingOwner innerContainer;

	protected Bill_Autonomous activeBill;

	public Bill_Autonomous ActiveBill
	{
		get
		{
			return activeBill;
		}
		set
		{
			if (activeBill != value)
			{
				activeBill = value;
			}
		}
	}

	public float CurrentBillFormingPercent
	{
		get
		{
			if (activeBill == null || activeBill.State != FormingState.Forming)
			{
				return 0f;
			}
			return 1f - activeBill.formingTicks / (float)activeBill.recipe.formingTicks;
		}
	}

	public GenDraw.FillableBarRequest BarDrawData => def.building.BarDrawDataFor(base.Rotation);

	public Building_WorkTableAutonomous()
	{
		innerContainer = new ThingOwner<Thing>(this);
	}

	public virtual void Notify_StartForming(Pawn billDoer)
	{
	}

	public virtual void Notify_FormingCompleted()
	{
		Thing thing = activeBill.CreateProducts();
		innerContainer.ClearAndDestroyContents();
		if (thing != null)
		{
			innerContainer.TryAdd(thing);
		}
	}

	public override void Notify_BillDeleted(Bill bill)
	{
		if (activeBill == bill)
		{
			EjectContents();
			activeBill = null;
		}
	}

	public virtual void Notify_HauledTo(Pawn hauler, Thing thing, int count)
	{
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		activeBill?.Reset();
		EjectContents();
		base.Destroy(mode);
	}

	public virtual void EjectContents()
	{
		innerContainer.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near);
		activeBill?.Reset();
	}

	public virtual bool CanWork()
	{
		return true;
	}

	protected override void Tick()
	{
		base.Tick();
		if (activeBill != null && CanWork())
		{
			activeBill.BillTick();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_References.Look(ref activeBill, "activeBill");
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string inspectString = base.GetInspectString();
		if (!inspectString.NullOrEmpty())
		{
			stringBuilder.AppendLine(inspectString);
		}
		string inspectStringExtra = GetInspectStringExtra();
		if (!inspectStringExtra.NullOrEmpty())
		{
			stringBuilder.AppendLine(inspectStringExtra);
		}
		if (CanWork() && activeBill != null)
		{
			activeBill.AppendInspectionData(stringBuilder);
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	protected virtual string GetInspectStringExtra()
	{
		return null;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		Bill_Autonomous bill_Autonomous = ActiveBill;
		if (bill_Autonomous != null && bill_Autonomous.State == FormingState.Forming)
		{
			yield return new Command_Action
			{
				action = delegate
				{
					ActiveBill.formingTicks -= (float)ActiveBill.recipe.formingTicks * 0.25f;
				},
				defaultLabel = "DEV: Forming cycle +25%"
			};
			yield return new Command_Action
			{
				action = delegate
				{
					ActiveBill.formingTicks = 0f;
				},
				defaultLabel = "DEV: Complete cycle"
			};
		}
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}
}
