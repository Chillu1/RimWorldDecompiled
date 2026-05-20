using UnityEngine;
using Verse;

namespace RimWorld;

public class CompReleaseGas : ThingComp
{
	private int remainingGas;

	private bool started;

	[Unsaved(false)]
	private Effecter effecter;

	private const int ReleaseGasInterval = 30;

	private CompProperties_ReleaseGas Props => (CompProperties_ReleaseGas)props;

	private int TotalGas => Mathf.CeilToInt(Props.cellsToFill * 255f);

	private float GasReleasedPerTick => (float)TotalGas / Props.durationSeconds / 60f;

	private Thing EffecterSourceThing
	{
		get
		{
			ThingWithComps pawn = parent;
			if (!parent.Spawned)
			{
				IThingHolder parentHolder = parent.ParentHolder;
				if (parentHolder != null)
				{
					if (parentHolder is Pawn_ApparelTracker pawn_ApparelTracker)
					{
						pawn = pawn_ApparelTracker.pawn;
					}
					else if (parentHolder is Pawn_CarryTracker pawn_CarryTracker)
					{
						pawn = pawn_CarryTracker.pawn;
					}
					else if (parentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker)
					{
						pawn = pawn_EquipmentTracker.pawn;
					}
				}
			}
			return pawn;
		}
	}

	public override void PostPostMake()
	{
		remainingGas = TotalGas;
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		effecter?.Cleanup();
		effecter = null;
	}

	public void StartRelease()
	{
		started = true;
	}

	public override void Notify_WearerDied()
	{
		started = false;
		remainingGas = TotalGas;
	}

	public override void CompTick()
	{
		if (!started || parent.MapHeld == null)
		{
			return;
		}
		if (Props.effecterReleasing != null)
		{
			if (effecter == null)
			{
				effecter = Props.effecterReleasing.Spawn(EffecterSourceThing, TargetInfo.Invalid);
			}
			effecter.EffectTick(EffecterSourceThing, TargetInfo.Invalid);
		}
		if (remainingGas > 0 && parent.IsHashIntervalTick(30))
		{
			int num = Mathf.Min(remainingGas, Mathf.RoundToInt(GasReleasedPerTick * 30f));
			GasUtility.AddGas(parent.PositionHeld, parent.MapHeld, Props.gasType, num);
			remainingGas -= num;
			if (remainingGas <= 0)
			{
				started = false;
				remainingGas = TotalGas;
			}
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref remainingGas, "remainingGas", 0);
		Scribe_Values.Look(ref started, "started", defaultValue: false);
	}
}
