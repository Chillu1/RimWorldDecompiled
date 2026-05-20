using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class ShipJob : IExposable, ILoadReferenceable
{
	public ShipJobDef def;

	public int loadID = -1;

	public ShipJobState jobState;

	public TransportShip transportShip;

	protected abstract bool ShouldEnd { get; }

	public virtual bool HasDestination => false;

	public virtual bool Interruptible => true;

	public virtual bool ShowGizmos => true;

	public ShipJob()
	{
	}

	public ShipJob(TransportShip transportShip)
	{
		loadID = Find.UniqueIDsManager.GetNextShipJobID();
		this.transportShip = transportShip;
	}

	public virtual bool TryStart()
	{
		if (transportShip == null)
		{
			Log.Error("Trying to start a ship job with a null ship object.");
			return false;
		}
		if (jobState == ShipJobState.Ended)
		{
			Log.Error("Trying to start an already ended ship job.");
			transportShip.RemoveJob(this);
			return false;
		}
		jobState = ShipJobState.Working;
		return true;
	}

	public virtual void End()
	{
		jobState = ShipJobState.Ended;
	}

	public virtual void TickInterval(int delta)
	{
		if (jobState != ShipJobState.Working)
		{
			Log.Error("Trying to tick " + jobState.ToString() + " job.");
		}
		if (ShouldEnd)
		{
			End();
		}
	}

	public virtual IEnumerable<Gizmo> GetJobGizmos()
	{
		return null;
	}

	public virtual string GetJobInfo()
	{
		return GetType().Name;
	}

	public virtual string ShipThingExtraInspectString()
	{
		return null;
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Values.Look(ref jobState, "jobState", ShipJobState.Uninitialized);
		Scribe_References.Look(ref transportShip, "transportShip");
	}

	public string GetUniqueLoadID()
	{
		return "ShipJob_" + loadID;
	}
}
