using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class CompScanner : ThingComp
{
	protected float daysWorkingSinceLastFinding;

	protected float lastUserSpeed = 1f;

	protected float lastScanTick = -1f;

	protected CompPowerTrader powerComp;

	protected CompForbiddable forbiddable;

	public CompProperties_Scanner Props => (CompProperties_Scanner)props;

	public virtual AcceptanceReport CanUseNow
	{
		get
		{
			if (!parent.Spawned)
			{
				return false;
			}
			if (powerComp != null && !powerComp.PowerOn)
			{
				return false;
			}
			if (RoofUtility.IsAnyCellUnderRoof(parent))
			{
				return "CannotUseScannerRoofed".Translate();
			}
			if (forbiddable != null && forbiddable.Forbidden)
			{
				return false;
			}
			return parent.Faction == Faction.OfPlayer;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref daysWorkingSinceLastFinding, "daysWorkingSinceLastFinding", 0f);
		Scribe_Values.Look(ref lastUserSpeed, "lastUserSpeed", 0f);
		Scribe_Values.Look(ref lastScanTick, "lastScanTick", 0f);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		powerComp = parent.GetComp<CompPowerTrader>();
		forbiddable = parent.GetComp<CompForbiddable>();
	}

	public void Used(Pawn worker)
	{
		if (!CanUseNow)
		{
			Log.Error("Used while CanUseNow is false.");
		}
		lastScanTick = Find.TickManager.TicksGame;
		lastUserSpeed = 1f;
		if (Props.scanSpeedStat != null)
		{
			lastUserSpeed = worker.GetStatValue(Props.scanSpeedStat);
		}
		daysWorkingSinceLastFinding += lastUserSpeed / 60000f;
		if (TickDoesFind(lastUserSpeed))
		{
			DoFind(worker);
			daysWorkingSinceLastFinding = 0f;
		}
	}

	protected virtual bool TickDoesFind(float scanSpeed)
	{
		if (parent.IsHashIntervalTick(59) && (Rand.MTBEventOccurs(Props.scanFindMtbDays / scanSpeed, 60000f, 59f) || (Props.scanFindGuaranteedDays > 0f && daysWorkingSinceLastFinding >= Props.scanFindGuaranteedDays)))
		{
			return true;
		}
		return false;
	}

	public override string CompInspectStringExtra()
	{
		string text = "";
		if (lastScanTick > (float)(Find.TickManager.TicksGame - 30))
		{
			text += "UserScanAbility".Translate() + ": " + lastUserSpeed.ToStringPercent() + "\n" + "ScanAverageInterval".Translate() + ": " + "PeriodDays".Translate((Props.scanFindMtbDays / lastUserSpeed).ToString("F1")) + "\n";
		}
		return text + "ScanningProgressToGuaranteedFind".Translate() + ": " + (daysWorkingSinceLastFinding / Props.scanFindGuaranteedDays).ToStringPercent();
	}

	protected abstract void DoFind(Pawn worker);

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Find now";
			command_Action.action = delegate
			{
				DoFind(PawnsFinder.AllMaps_FreeColonists.RandomElement());
			};
			yield return command_Action;
		}
	}
}
