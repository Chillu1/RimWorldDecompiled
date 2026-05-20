using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Building_PassengerShuttle : Building, IRenameable
{
	private string shuttleName;

	private CompRefuelable cachedRefuelableComp;

	private CompLaunchable cachedLaunchableComp;

	private CompTransporter cachedTransporterComp;

	private CompShuttle cachedShuttleComp;

	public static CachedTexture RefuelFromCargoIcon = new CachedTexture("UI/Commands/RefuelPassengerShuttle");

	private static List<Thing> tmpContainedThings = new List<Thing>();

	public CompRefuelable RefuelableComp => cachedRefuelableComp ?? (cachedRefuelableComp = GetComp<CompRefuelable>());

	public CompLaunchable LaunchableComp => cachedLaunchableComp ?? (cachedLaunchableComp = GetComp<CompLaunchable>());

	public CompTransporter TransporterComp => cachedTransporterComp ?? (cachedTransporterComp = GetComp<CompTransporter>());

	public CompShuttle ShuttleComp => cachedShuttleComp ?? (cachedShuttleComp = GetComp<CompShuttle>());

	public string RenamableLabel
	{
		get
		{
			return shuttleName ?? BaseLabel;
		}
		set
		{
			shuttleName = value;
		}
	}

	public string BaseLabel => def.LabelCap;

	public string InspectLabel => RenamableLabel;

	public override string Label => RenamableLabel;

	public float FuelLevel => RefuelableComp.Fuel;

	public float MaxFuelLevel => RefuelableComp.Props.fuelCapacity;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			ShuttleComp.shipParent.Start();
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		int num = 0;
		foreach (object selectedObject in Find.Selector.SelectedObjects)
		{
			if (selectedObject is ThingWithComps thing && thing.HasComp<CompTransporter>())
			{
				num++;
			}
		}
		if (num > 1)
		{
			yield break;
		}
		float fuelInShuttle = FuelInShuttle();
		string text = null;
		if (fuelInShuttle <= 0f)
		{
			text = "NoFuelInShuttle".Translate();
		}
		if (Mathf.Approximately(FuelLevel, MaxFuelLevel))
		{
			text = "ShuttleFullyFueled".Translate();
		}
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "CommandRefuelShuttleFromCargo".Translate();
		command_Action.defaultDesc = "CommandRefuelShuttleFromCargoDesc".Translate();
		command_Action.icon = RefuelFromCargoIcon.Texture;
		command_Action.action = delegate
		{
			int to = Mathf.FloorToInt(Mathf.Min(fuelInShuttle, MaxFuelLevel - FuelLevel));
			Dialog_Slider window = new Dialog_Slider((int val) => "RefuelShuttleCount".Translate(val), 1, to, delegate(int count)
			{
				ConsumeFuelFromInventory(count);
				RefuelableComp.Refuel(count);
			});
			Find.WindowStack.Add(window);
		};
		command_Action.Disabled = !text.NullOrEmpty();
		command_Action.disabledReason = text;
		yield return command_Action;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref shuttleName, "shuttleName");
	}

	private float FuelInShuttle()
	{
		float num = 0f;
		foreach (Thing item in (IEnumerable<Thing>)TransporterComp.innerContainer)
		{
			if (RefuelableComp.Props.fuelFilter.Allows(item))
			{
				num += (float)item.stackCount;
			}
		}
		return num;
	}

	private void ConsumeFuelFromInventory(int fuelAmount)
	{
		tmpContainedThings.Clear();
		tmpContainedThings.AddRange(TransporterComp.innerContainer);
		int num = fuelAmount;
		int num2 = tmpContainedThings.Count - 1;
		while (num2 >= 0)
		{
			Thing thing = tmpContainedThings[num2];
			if (RefuelableComp.Props.fuelFilter.Allows(thing))
			{
				Thing thing2 = thing.SplitOff(Mathf.Min(num, thing.stackCount));
				num -= thing2.stackCount;
			}
			if (num > 0)
			{
				num2--;
				continue;
			}
			break;
		}
	}
}
