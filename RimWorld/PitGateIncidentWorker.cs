using Verse;

namespace RimWorld;

public abstract class PitGateIncidentWorker : IExposable, ILoadReferenceable
{
	private int loadID = -1;

	public PitGateIncidentDef def;

	public Building pitGate;

	public int fireTick = -99999;

	protected PitGateIncidentWorker()
	{
		loadID = Find.UniqueIDsManager.GetNextPitGateIncidentID();
	}

	public virtual void Setup(float points)
	{
		fireTick = Find.TickManager.TicksGame;
	}

	public virtual bool CanFireNow()
	{
		if (pitGate == null || pitGate.Destroyed)
		{
			return false;
		}
		return true;
	}

	public virtual void Tick()
	{
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Defs.Look(ref def, "def");
		Scribe_References.Look(ref pitGate, "pitGate");
	}

	public string GetUniqueLoadID()
	{
		return "PitGateIncident_" + def.defName + "_" + loadID;
	}
}
