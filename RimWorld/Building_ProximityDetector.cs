using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Building_ProximityDetector : Building, IThingGlower
{
	private bool triggered;

	private Effecter detectionEffecter;

	public const float DetectionRadius = 19.9f;

	private const int DetectInterval = 30;

	private CompPowerTrader powerTraderComp;

	private CompGlower glowerComp;

	public CompPowerTrader PowerTraderComp => powerTraderComp ?? (powerTraderComp = GetComp<CompPowerTrader>());

	public CompGlower Glower => glowerComp ?? (glowerComp = GetComp<CompGlower>());

	private TargetInfo TgtInfo => new TargetInfo(base.Position, base.Map);

	public bool ShouldBeLitNow()
	{
		return triggered;
	}

	protected override void Tick()
	{
		base.Tick();
		detectionEffecter?.EffectTick(TgtInfo, TgtInfo);
		if (base.Spawned && this.IsHashIntervalTick(30))
		{
			RunDetection();
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Proximity detector"))
		{
			Destroy();
			return;
		}
		base.SpawnSetup(map, respawningAfterLoad);
		if (respawningAfterLoad)
		{
			TickDetectionEffect();
		}
	}

	private void TickDetectionEffect()
	{
		if (triggered && detectionEffecter == null)
		{
			detectionEffecter = EffecterDefOf.ProximityDetectorAlert.Spawn(base.Position, base.Map);
		}
		else if (!triggered && detectionEffecter != null)
		{
			detectionEffecter.Cleanup();
			detectionEffecter = null;
		}
	}

	private void RunDetection()
	{
		bool flag = false;
		if (PowerTraderComp.PowerOn)
		{
			IReadOnlyList<Pawn> allPawnsSpawned = base.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].Position.InHorDistOf(base.Position, 19.9f) && allPawnsSpawned[i].IsPsychologicallyInvisible())
				{
					flag = true;
					break;
				}
			}
		}
		if (flag != triggered)
		{
			triggered = flag;
			Glower.UpdateLit(base.Map);
			if (triggered)
			{
				Messages.Message("MessageProximityDetectorTriggered".Translate(), this, MessageTypeDefOf.ThreatSmall, historical: false);
			}
		}
		TickDetectionEffect();
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (triggered)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += "ProximityDetectorCreatureDetected".Translate().Colorize(ColorLibrary.RedReadable);
		}
		return text;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref triggered, "triggered", defaultValue: false);
	}
}
