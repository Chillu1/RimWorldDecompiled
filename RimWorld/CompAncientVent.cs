using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public abstract class CompAncientVent : ThingComp
{
	private static Dictionary<int, int> sharedLastCheckMtbTickPerMap = new Dictionary<int, int>();

	private static Dictionary<int, bool> sharedLastMtbCheckDidOccurPerMap = new Dictionary<int, bool>();

	private const int PushHeatInterval = 20;

	private bool ventOn;

	private Sustainer sustainer;

	public CompProperties_AncientVentEmitter Props => (CompProperties_AncientVentEmitter)props;

	protected virtual bool AppliesEffectsToPawns => false;

	public bool VentOn => ventOn;

	public override void CompTick()
	{
		base.CompTick();
		if (parent.Destroyed)
		{
			return;
		}
		if (CheckShouldToggleAndHandleMapState())
		{
			ventOn = !ventOn;
			ToggleIndividualVent(ventOn);
		}
		if (!ventOn)
		{
			sustainer?.End();
			sustainer = null;
			return;
		}
		if (sustainer == null)
		{
			sustainer = Props.activeSound.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
		}
		sustainer.Maintain();
		if (parent.IsHashIntervalTick(20) && Props.heatToPush != 0)
		{
			GenTemperature.PushHeat(parent, Props.heatToPush);
		}
		if (!AppliesEffectsToPawns)
		{
			return;
		}
		Map map = parent.Map;
		IntVec3 position = parent.Position;
		int num = Mathf.FloorToInt((float)parent.def.size.x / 2f);
		for (int i = 0; i < map.mapPawns.AllPawnsSpawned.Count; i++)
		{
			Pawn pawn = map.mapPawns.AllPawnsSpawned[i];
			if (!(pawn.Position.DistanceTo(position) > (float)num))
			{
				ApplyPawnEffect(pawn, 1);
			}
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref ventOn, "ventOn", defaultValue: false);
	}

	private bool CheckShouldToggleAndHandleMapState()
	{
		bool value2;
		if ((sharedLastCheckMtbTickPerMap.TryGetValue(parent.Map.uniqueID, out var value) ? value : 0) == GenTicks.TicksGame)
		{
			return sharedLastMtbCheckDidOccurPerMap.TryGetValue(parent.Map.uniqueID, out value2) && value2;
		}
		int num = (ventOn ? Props.onDurationMtbDays : Props.offDurationMtbDays);
		sharedLastCheckMtbTickPerMap[parent.Map.uniqueID] = GenTicks.TicksGame;
		bool flag = (sharedLastMtbCheckDidOccurPerMap[parent.Map.uniqueID] = Rand.MTBEventOccurs(num, 60000f, 1f));
		if (flag)
		{
			ToggleMapState();
		}
		return flag;
	}

	private void ToggleMapState()
	{
		if (ventOn)
		{
			if (Props.conditionToCause != null)
			{
				parent.Map.gameConditionManager.GetActiveCondition(Props.conditionToCause)?.End();
			}
			SendToggleAlert(goingToBeOn: false);
		}
		else if (Props.conditionToCause != null)
		{
			if (!parent.Map.gameConditionManager.ConditionIsActive(Props.conditionToCause))
			{
				GameCondition gameCondition = GameConditionMaker.MakeConditionPermanent(Props.conditionToCause);
				gameCondition.suppressEndMessage = true;
				gameCondition.conditionCauser = parent;
				gameCondition.forceDisplayAsDuration = true;
				parent.Map.gameConditionManager.RegisterCondition(gameCondition);
			}
			SendToggleAlert(goingToBeOn: true);
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!ventOn)
		{
			return "DormantCompInactive".Translate();
		}
		return Props.activeInspectStringKey.Translate().CapitalizeFirst();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				action = DevToggleVent,
				defaultLabel = "DEV: Toggle vents on map",
				defaultDesc = "Toggles all vents on the map on or off"
			};
		}
	}

	private void DevToggleVent()
	{
		ToggleMapState();
		bool flag = !ventOn;
		foreach (Building item in parent.Map.listerBuildings.AllBuildingsNonColonistOfDef(parent.def))
		{
			CompAncientVent compAncientVent = item.TryGetComp<CompAncientVent>();
			if (compAncientVent != null)
			{
				compAncientVent.ventOn = flag;
				compAncientVent.ToggleIndividualVent(flag);
			}
		}
	}

	protected void SendToggleAlert(bool goingToBeOn)
	{
		Messages.Message((goingToBeOn ? Props.startupMessageKey : Props.shutdownMessageKey).Translate(), new LookTargets(parent.Map.listerBuildings.AllBuildingsNonColonistOfDef(parent.def)), MessageTypeDefOf.NeutralEvent, historical: false);
	}

	protected abstract void ToggleIndividualVent(bool on);

	protected virtual void ApplyPawnEffect(Pawn pawn, int delta)
	{
	}
}
