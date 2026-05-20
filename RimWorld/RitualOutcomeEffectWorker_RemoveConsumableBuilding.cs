using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_RemoveConsumableBuilding : RitualOutcomeEffectWorker_FromQuality
{
	public override bool ApplyOnFailure => true;

	public override bool SupportsAttachableOutcomeEffect => def.allowAttachableOutcome;

	public RitualOutcomeEffectWorker_RemoveConsumableBuilding()
	{
	}

	public RitualOutcomeEffectWorker_RemoveConsumableBuilding(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		if (!jobRitual.cancelled)
		{
			base.Apply(progress, totalPresence, jobRitual);
		}
		if (!jobRitual.selectedTarget.HasThing)
		{
			return;
		}
		Thing thing = jobRitual.selectedTarget.Thing;
		if (def.effecter != null)
		{
			def.effecter.Spawn(thing, jobRitual.selectedTarget.Map).Cleanup();
		}
		if (def.fleckDef != null)
		{
			CellRect cellRect = thing.OccupiedRect();
			for (int i = 0; i < cellRect.Area * def.flecksPerCell; i++)
			{
				FleckCreationData dataStatic = FleckMaker.GetDataStatic(cellRect.RandomVector3, thing.Map, def.fleckDef, def.fleckScaleRange.RandomInRange);
				dataStatic.rotation = def.fleckRotationRange.RandomInRange;
				dataStatic.velocityAngle = def.fleckVelocityAngle.RandomInRange;
				dataStatic.velocitySpeed = def.fleckVelocitySpeed.RandomInRange;
				thing.Map.flecks.CreateFleck(dataStatic);
			}
		}
		if (def.filthDefToSpawn != null)
		{
			foreach (IntVec3 item in thing.OccupiedRect())
			{
				FilthMaker.TryMakeFilth(item, thing.Map, def.filthDefToSpawn, def.filthCountToSpawn.RandomInRange);
			}
		}
		thing.Destroy();
	}
}
