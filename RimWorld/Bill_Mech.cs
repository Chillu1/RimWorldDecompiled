using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class Bill_Mech : Bill_Autonomous
{
	private const float StatusStringLineHeight = 20f;

	private Pawn boundPawn;

	private int gestationCycles;

	public Pawn BoundPawn => boundPawn;

	public int GestationCyclesCompleted => gestationCycles;

	public int StartedTick => startedTick;

	public float WorkSpeedMultiplier
	{
		get
		{
			if (recipe.workSpeedStat != null)
			{
				return boundPawn.GetStatValue(recipe.workSpeedStat);
			}
			return 1f;
		}
	}

	public Building_MechGestator Gestator => (Building_MechGestator)billStack.billGiver;

	public abstract float BandwidthCost { get; }

	protected override string StatusString
	{
		get
		{
			switch (base.State)
			{
			case FormingState.Gathering:
			case FormingState.Preparing:
				if (BoundPawn != null)
				{
					return "Worker".Translate() + ": " + BoundPawn.LabelShortCap;
				}
				break;
			case FormingState.Forming:
				return "Gestating".Translate();
			case FormingState.Formed:
				if (BoundPawn != null)
				{
					return "WaitingFor".Translate() + ": " + BoundPawn.LabelShortCap;
				}
				break;
			}
			return null;
		}
	}

	protected override float StatusLineMinHeight => 20f;

	protected override Color BaseColor
	{
		get
		{
			if (suspended)
			{
				return base.BaseColor;
			}
			return Color.white;
		}
	}

	public Bill_Mech()
	{
	}

	public Bill_Mech(RecipeDef recipe, Precept_ThingStyle precept = null)
		: base(recipe, precept)
	{
	}

	protected override Window GetBillDialog()
	{
		return new Dialog_MechBillConfig(this, ((Thing)billStack.billGiver).Position);
	}

	public override bool ShouldDoNow()
	{
		if (BoundPawn?.mechanitor != null && !BoundPawn.mechanitor.HasBandwidthForBill(this))
		{
			JobFailReason.Is("NotEnoughBandwidth".Translate());
			return false;
		}
		return base.ShouldDoNow();
	}

	public override bool PawnAllowedToStartAnew(Pawn p)
	{
		if (!ModLister.CheckBiotech("Mech bill"))
		{
			return false;
		}
		if (!base.PawnAllowedToStartAnew(p))
		{
			return false;
		}
		if (BoundPawn != null && BoundPawn != p)
		{
			JobFailReason.Is("AlreadyAssigned".Translate() + " (" + BoundPawn.LabelShort + ")");
			return false;
		}
		if (!p.mechanitor.HasBandwidthForBill(this))
		{
			JobFailReason.Is("NotEnoughBandwidth".Translate());
			return false;
		}
		return true;
	}

	public override void Notify_DoBillStarted(Pawn billDoer)
	{
		base.Notify_DoBillStarted(billDoer);
		if (boundPawn != billDoer)
		{
			boundPawn = billDoer;
		}
	}

	public override void Reset()
	{
		base.Reset();
		gestationCycles = 0;
		boundPawn = null;
	}

	public void ForceCompleteAllCycles()
	{
		gestationCycles = recipe.gestationCycles;
		formingTicks = 0f;
	}

	public override void BillTick()
	{
		if (suspended || state != FormingState.Forming)
		{
			return;
		}
		formingTicks -= 1f * WorkSpeedMultiplier;
		if (formingTicks <= 0f)
		{
			gestationCycles++;
			if (gestationCycles >= recipe.gestationCycles)
			{
				state = FormingState.Formed;
				Gestator.Notify_FormingCompleted();
			}
			else
			{
				formingTicks = recipe.formingTicks;
				state = FormingState.Preparing;
			}
		}
	}

	public override void AppendInspectionData(StringBuilder sb)
	{
		if (base.State == FormingState.Forming || base.State == FormingState.Preparing)
		{
			sb.AppendLine("CurrentGestationCycle".Translate() + ": " + ((int)(formingTicks * (1f / WorkSpeedMultiplier))).ToStringTicksToPeriod());
			sb.AppendLine(string.Concat(string.Concat("RemainingGestationCycles".Translate() + ": ", (recipe.gestationCycles - GestationCyclesCompleted).ToString(), " (") + "OfLower".Translate() + " ", recipe.gestationCycles.ToString(), ")"));
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref boundPawn, "boundPawn");
		Scribe_Values.Look(ref gestationCycles, "gestationCycles", 0);
	}
}
