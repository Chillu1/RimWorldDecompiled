using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompBreakdownable : ThingComp
{
	private bool brokenDownInt;

	private CompPowerTrader powerComp;

	private const int BreakdownMTBTicks = 13680000;

	public const string BreakdownSignal = "Breakdown";

	private OverlayHandle? overlayBrokenDown;

	public bool BrokenDown => brokenDownInt;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref brokenDownInt, "brokenDown", defaultValue: false);
	}

	private void UpdateOverlays()
	{
		if (parent.Spawned)
		{
			parent.Map.overlayDrawer.Disable(parent, ref overlayBrokenDown);
			if (brokenDownInt)
			{
				overlayBrokenDown = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.BrokenDown);
			}
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		powerComp = parent.GetComp<CompPowerTrader>();
		parent.Map.GetComponent<BreakdownManager>().Register(this);
		UpdateOverlays();
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		map.GetComponent<BreakdownManager>().Deregister(this);
	}

	public void CheckForBreakdown()
	{
		if (CanBreakdownNow() && Rand.MTBEventOccurs(13680000f, 1f, 1041f))
		{
			DoBreakdown();
		}
	}

	protected bool CanBreakdownNow()
	{
		if (!BrokenDown)
		{
			if (powerComp != null)
			{
				return powerComp.PowerOn;
			}
			return true;
		}
		return false;
	}

	public void Notify_Repaired()
	{
		brokenDownInt = false;
		parent.Map.GetComponent<BreakdownManager>().Notify_Repaired(parent);
		if (parent is Building_PowerSwitch)
		{
			parent.Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(parent.GetComp<CompPower>());
		}
		UpdateOverlays();
	}

	public void DoBreakdown()
	{
		brokenDownInt = true;
		parent.BroadcastCompSignal("Breakdown");
		parent.Map.GetComponent<BreakdownManager>().Notify_BrokenDown(parent);
		UpdateOverlays();
	}

	public override string CompInspectStringExtra()
	{
		if (BrokenDown)
		{
			return "BrokenDown".Translate();
		}
		return null;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos && BrokenDown)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Fix breakdown",
				action = Notify_Repaired
			};
		}
	}
}
