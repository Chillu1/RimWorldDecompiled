using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class CompTerrainPump : ThingComp
{
	private CompPowerTrader powerComp;

	private int progressTicks;

	private CompProperties_TerrainPump Props => (CompProperties_TerrainPump)props;

	private float ProgressDays => (float)progressTicks / 60000f;

	protected float CurrentRadius => Mathf.Min(Props.radius, ProgressDays / Props.daysToRadius * Props.radius);

	protected bool Working
	{
		get
		{
			if (powerComp != null)
			{
				return powerComp.PowerOn;
			}
			return true;
		}
	}

	private int TicksUntilRadiusInteger
	{
		get
		{
			float num = Mathf.Ceil(CurrentRadius) - CurrentRadius;
			if (num < 1E-05f)
			{
				num = 1f;
			}
			float num2 = Props.radius / Props.daysToRadius;
			return (int)(num / num2 * 60000f);
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		powerComp = parent.TryGetComp<CompPowerTrader>();
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		progressTicks = 0;
	}

	public override void CompTickRare()
	{
		if (Working)
		{
			progressTicks += 250;
			int num = GenRadial.NumCellsInRadius(CurrentRadius);
			for (int i = 0; i < num; i++)
			{
				AffectCell(parent.Position + GenRadial.RadialPattern[i]);
			}
		}
	}

	protected abstract void AffectCell(IntVec3 c);

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref progressTicks, "progressTicks", 0);
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		if (CurrentRadius < Props.radius - 0.0001f)
		{
			GenDraw.DrawRadiusRing(parent.Position, CurrentRadius);
		}
	}

	public override string CompInspectStringExtra()
	{
		string text = "TimePassed".Translate().CapitalizeFirst() + ": " + progressTicks.ToStringTicksToPeriod() + "\n" + "CurrentRadius".Translate().CapitalizeFirst() + ": " + CurrentRadius.ToString("F1");
		if (ProgressDays < Props.daysToRadius && Working)
		{
			text += "\n" + "RadiusExpandsIn".Translate().CapitalizeFirst() + ": " + TicksUntilRadiusInteger.ToStringTicksToPeriod();
		}
		return text;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Progress 1 day";
			command_Action.action = delegate
			{
				progressTicks += 60000;
			};
			yield return command_Action;
		}
	}
}
