using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_Incident : QuestPart
{
	public string inSignal;

	public IncidentDef incident;

	private IncidentParms incidentParms;

	private MapParent mapParent;

	public MapParent MapParent
	{
		get
		{
			return mapParent;
		}
		set
		{
			mapParent = value;
		}
	}

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (mapParent != null)
			{
				yield return mapParent;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal) || incidentParms == null)
		{
			return;
		}
		if (!incidentParms.forced)
		{
			Log.Error("QuestPart incident should always be forced but it's not. incident=" + incident);
			incidentParms.forced = true;
		}
		incidentParms.quest = quest;
		if (incidentParms.target == null && (mapParent == null || !mapParent.HasMap || !quest.IsParentSuitableForQuest(mapParent)))
		{
			mapParent = quest.TryFindNewSuitableMapParentForRetarget() ?? Find.AnyPlayerHomeMap?.Parent;
		}
		if (mapParent != null)
		{
			if (mapParent.HasMap)
			{
				incidentParms.target = mapParent.Map;
				if (incident.Worker.CanFireNow(incidentParms))
				{
					incident.Worker.TryExecute(incidentParms);
				}
				incidentParms.target = null;
			}
		}
		else if (incidentParms.target != null && incident.Worker.CanFireNow(incidentParms))
		{
			incident.Worker.TryExecute(incidentParms);
		}
	}

	public void SetIncidentParmsAndRemoveTarget(IncidentParms value)
	{
		incidentParms = value;
		if (incidentParms.target is Map map)
		{
			mapParent = map.Parent;
			incidentParms.target = null;
		}
		else
		{
			mapParent = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Defs.Look(ref incident, "incident");
		Scribe_Deep.Look(ref incidentParms, "incidentParms");
		Scribe_References.Look(ref mapParent, "mapParent");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (Find.AnyPlayerHomeMap != null)
		{
			incident = IncidentDefOf.RaidEnemy;
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = Find.RandomPlayerHomeMap;
			incidentParms.points = 500f;
			SetIncidentParmsAndRemoveTarget(incidentParms);
		}
	}
}
