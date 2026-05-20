using Verse;

namespace RimWorld;

public class StatWorker_MaxPowerOutput : StatWorker
{
	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		if (!(req.Def is ThingDef thingDef))
		{
			Log.ErrorOnce("Tried to get max power output for non-thing " + req.Def, 64352724);
			return 0f;
		}
		CompProperties_Power compProperties_Power = (CompProperties_Power)thingDef.CompDefForAssignableFrom<CompPowerPlant>();
		if (compProperties_Power == null)
		{
			Log.ErrorOnce("Tried to get power output of " + thingDef?.ToString() + " which has no CompPowerPlant.", 756784453);
			return 0f;
		}
		return 0f - compProperties_Power.PowerConsumption;
	}
}
