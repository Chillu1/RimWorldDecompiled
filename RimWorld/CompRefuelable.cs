using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompRefuelable : ThingComp_VacuumAware, IThingGlower
{
	private float fuel;

	private float configuredTargetFuelLevel = -1f;

	public bool allowAutoRefuel = true;

	private CompFlickable flickComp;

	private CompExplosive explosiveComp;

	public const string RefueledSignal = "Refueled";

	public const string RanOutOfFuelSignal = "RanOutOfFuel";

	private static readonly Texture2D SetTargetFuelLevelCommand = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel");

	private static readonly Vector2 FuelBarSize = new Vector2(1f, 0.2f);

	private static readonly Material FuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f));

	private static readonly Material FuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

	protected override bool FunctionsInVacuum => Props.functionsInVacuum;

	public float TargetFuelLevel
	{
		get
		{
			if (configuredTargetFuelLevel >= 0f)
			{
				return configuredTargetFuelLevel;
			}
			if (Props.targetFuelLevelConfigurable)
			{
				return Props.initialConfigurableTargetFuelLevel;
			}
			return Props.fuelCapacity;
		}
		set
		{
			configuredTargetFuelLevel = Mathf.Clamp(value, 0f, Props.fuelCapacity);
		}
	}

	public CompProperties_Refuelable Props => (CompProperties_Refuelable)props;

	public float Fuel => fuel;

	public float FuelPercentOfTarget => fuel / TargetFuelLevel;

	public float FuelPercentOfMax => fuel / Props.fuelCapacity;

	public bool IsFull => TargetFuelLevel - fuel < 1f * Props.FuelMultiplierCurrentDifficulty;

	public bool HasFuel
	{
		get
		{
			if (fuel > 0f && fuel >= Props.minimumFueledThreshold)
			{
				if (!FunctionsInVacuum)
				{
					return !base.InVacuum;
				}
				return true;
			}
			return false;
		}
	}

	private float ConsumptionRatePerTick => Props.fuelConsumptionRate / 60000f;

	public bool ShouldAutoRefuelNow
	{
		get
		{
			if (FuelPercentOfTarget <= Props.autoRefuelPercent && !IsFull && TargetFuelLevel > 0f)
			{
				return ShouldAutoRefuelNowIgnoringFuelPct;
			}
			return false;
		}
	}

	public bool ShouldAutoRefuelNowIgnoringFuelPct
	{
		get
		{
			if (!parent.IsBurning() && (flickComp == null || flickComp.SwitchIsOn) && parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Flick) == null)
			{
				return parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Deconstruct) == null;
			}
			return false;
		}
	}

	public bool ShouldBeLitNow()
	{
		return HasFuel;
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		allowAutoRefuel = Props.initialAllowAutoRefuel;
		fuel = Props.fuelCapacity * Props.initialFuelPercent;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		flickComp = parent.GetComp<CompFlickable>();
		explosiveComp = parent.GetComp<CompExplosive>();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref fuel, "fuel", 0f);
		Scribe_Values.Look(ref configuredTargetFuelLevel, "configuredTargetFuelLevel", -1f);
		Scribe_Values.Look(ref allowAutoRefuel, "allowAutoRefuel", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && !Props.showAllowAutoRefuelToggle)
		{
			allowAutoRefuel = Props.initialAllowAutoRefuel;
		}
	}

	public override void PostDraw()
	{
		base.PostDraw();
		if (!allowAutoRefuel)
		{
			parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.ForbiddenRefuel);
		}
		else if (!HasFuel && Props.drawOutOfFuelOverlay)
		{
			parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.OutOfFuel);
		}
		if (Props.drawFuelGaugeInMap)
		{
			GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest
			{
				center = parent.DrawPos + Vector3.up * 0.1f,
				size = FuelBarSize,
				fillPercent = FuelPercentOfMax,
				filledMat = FuelBarFilledMat,
				unfilledMat = FuelBarUnfilledMat,
				margin = 0.15f
			};
			Rot4 rotation = parent.Rotation;
			rotation.Rotate(RotationDirection.Clockwise);
			r.rotation = rotation;
			GenDraw.DrawFillableBar(r);
		}
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		if ((!Props.fuelIsMortarBarrel || !Find.Storyteller.difficulty.classicMortars) && mode != DestroyMode.Vanish && previousMap != null && Props.fuelFilter.AllowedDefCount == 1 && Props.initialFuelPercent == 0f)
		{
			ThingDef thingDef = Props.fuelFilter.AllowedThingDefs.First();
			int num = Mathf.FloorToInt(1f * fuel);
			while (num > 0)
			{
				Thing thing = ThingMaker.MakeThing(thingDef);
				thing.stackCount = Mathf.Min(num, thingDef.stackLimit);
				num -= thing.stackCount;
				GenPlace.TryPlaceThing(thing, parent.Position, previousMap, ThingPlaceMode.Near);
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		if (Props.fuelIsMortarBarrel && Find.Storyteller.difficulty.classicMortars)
		{
			return string.Empty;
		}
		string text = base.CompInspectStringExtra();
		text = ((text != null) ? (text + "\n") : string.Empty);
		text = text + Props.FuelLabel + ": " + fuel.ToStringDecimalIfSmall() + " / " + Props.fuelCapacity.ToStringDecimalIfSmall();
		if (!Props.consumeFuelOnlyWhenUsed && HasFuel)
		{
			int numTicks = (int)(fuel / Props.fuelConsumptionRate * 60000f);
			text = text + " (" + numTicks.ToStringTicksToPeriod() + ")";
		}
		if (!HasFuel && !Props.outOfFuelMessage.NullOrEmpty())
		{
			string arg = ((parent.def.building != null && parent.def.building.IsTurret) ? ("CannotShoot".Translate() + ": " + Props.outOfFuelMessage).Resolve() : Props.outOfFuelMessage);
			text += $"\n{arg} ({GetFuelCountToFullyRefuel()}x {Props.fuelFilter.AnyAllowedDef.label})";
		}
		if (Props.targetFuelLevelConfigurable)
		{
			text += "\n" + "ConfiguredTargetFuelLevel".Translate(TargetFuelLevel.ToStringDecimalIfSmall());
		}
		return text;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (parent.def.building != null && parent.def.building.IsTurret)
		{
			TaggedString taggedString = "RearmCostExplanation".Translate();
			if (Props.factorByDifficulty)
			{
				taggedString += " (" + "RearmCostExplanationDifficulty".Translate() + ")";
			}
			taggedString += ".";
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "RearmCost".Translate(), GenLabel.ThingLabel(Props.fuelFilter.AnyAllowedDef, null, GetFuelCountToFullyRefuel()).CapitalizeFirst(), taggedString, 3171);
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		CompPowerTrader comp = parent.GetComp<CompPowerTrader>();
		if (!Props.consumeFuelOnlyWhenUsed && (flickComp == null || flickComp.SwitchIsOn) && (!Props.consumeFuelOnlyWhenPowered || (comp != null && comp.PowerOn)) && !Props.externalTicking)
		{
			ConsumeFuel(ConsumptionRatePerTick);
		}
		if (Props.fuelConsumptionPerTickInRain > 0f && parent.Spawned && parent.Map.weatherManager.RainRate > 0.4f && !parent.Map.roofGrid.Roofed(parent.Position) && !Props.externalTicking)
		{
			ConsumeFuel(Props.fuelConsumptionPerTickInRain);
		}
	}

	public void ConsumeFuel(float amount)
	{
		if ((!Props.fuelIsMortarBarrel || !Find.Storyteller.difficulty.classicMortars) && !(fuel <= 0f))
		{
			fuel -= amount;
			if (fuel <= 0f)
			{
				fuel = 0f;
				Notify_RanOutOfFuel();
			}
		}
	}

	private void Notify_RanOutOfFuel()
	{
		if (Props.destroyOnNoFuel)
		{
			parent.Destroy();
		}
		parent.BroadcastCompSignal("RanOutOfFuel");
	}

	public void Refuel(List<Thing> fuelThings)
	{
		if (Props.atomicFueling && fuelThings.Sum((Thing t) => t.stackCount) < GetFuelCountToFullyRefuel())
		{
			Log.ErrorOnce("Error refueling; not enough fuel available for proper atomic refuel", 19586442);
			return;
		}
		int num = GetFuelCountToFullyRefuel();
		while (num > 0 && fuelThings.Count > 0)
		{
			Thing thing = fuelThings.Pop();
			int num2 = Mathf.Min(num, thing.stackCount);
			Refuel(num2);
			thing.SplitOff(num2).Destroy();
			num -= num2;
		}
	}

	public void Refuel(float amount)
	{
		fuel += amount * Props.FuelMultiplierCurrentDifficulty;
		if (fuel > Props.fuelCapacity)
		{
			fuel = Props.fuelCapacity;
		}
		parent.BroadcastCompSignal("Refueled");
	}

	public AcceptanceReport CanEjectFuel()
	{
		if (!Props.canEjectFuel)
		{
			return false;
		}
		CompExplosive compExplosive = explosiveComp;
		if (compExplosive != null && compExplosive.wickStarted)
		{
			return "AboutToExplode".Translate();
		}
		if (Fuel == 0f)
		{
			return "RefuelableNoFuelToEject".Translate();
		}
		return true;
	}

	public void EjectFuel()
	{
		ThingDef thingDef = Props.fuelFilter.AllowedThingDefs.First();
		int num = Mathf.FloorToInt(fuel);
		while (num > 0)
		{
			Thing thing = ThingMaker.MakeThing(thingDef);
			thing.stackCount = Mathf.Min(num, thingDef.stackLimit);
			num -= thing.stackCount;
			GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
			thing.SetForbidden(value: true);
		}
		fuel = 0f;
		Notify_RanOutOfFuel();
	}

	public void Notify_UsedThisTick()
	{
		ConsumeFuel(ConsumptionRatePerTick);
	}

	public int GetFuelCountToFullyRefuel()
	{
		if (Props.atomicFueling)
		{
			return Mathf.CeilToInt(Props.fuelCapacity / Props.FuelMultiplierCurrentDifficulty);
		}
		return Mathf.Max(Mathf.CeilToInt((TargetFuelLevel - fuel) / Props.FuelMultiplierCurrentDifficulty), 1);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (Props.fuelIsMortarBarrel && Find.Storyteller.difficulty.classicMortars)
		{
			yield break;
		}
		if (!Props.hideGizmosIfNotPlayerFaction || parent.Faction == Faction.OfPlayer)
		{
			if (Find.Selector.SelectedObjects.Count == 1)
			{
				yield return new Gizmo_SetFuelLevel(this);
			}
			else
			{
				if (Props.targetFuelLevelConfigurable)
				{
					Command_SetTargetFuelLevel command_SetTargetFuelLevel = new Command_SetTargetFuelLevel();
					command_SetTargetFuelLevel.refuelable = this;
					command_SetTargetFuelLevel.defaultLabel = "CommandSetTargetFuelLevel".Translate();
					command_SetTargetFuelLevel.defaultDesc = "CommandSetTargetFuelLevelDesc".Translate();
					command_SetTargetFuelLevel.icon = SetTargetFuelLevelCommand;
					yield return command_SetTargetFuelLevel;
				}
				if (Props.showAllowAutoRefuelToggle)
				{
					string str = (allowAutoRefuel ? "On".Translate() : "Off".Translate());
					Command_Toggle command_Toggle = new Command_Toggle();
					command_Toggle.isActive = () => allowAutoRefuel;
					command_Toggle.toggleAction = delegate
					{
						allowAutoRefuel = !allowAutoRefuel;
					};
					command_Toggle.defaultLabel = "CommandToggleAllowAutoRefuel".Translate();
					command_Toggle.defaultDesc = "CommandToggleAllowAutoRefuelDescMult".Translate(str.UncapitalizeFirst().Named("ONOFF"));
					command_Toggle.icon = (allowAutoRefuel ? TexCommand.ForbidOn : TexCommand.ForbidOff);
					command_Toggle.Order = 20f;
					command_Toggle.hotKey = KeyBindingDefOf.Command_ItemForbid;
					yield return command_Toggle;
				}
			}
		}
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Set fuel to 0";
			command_Action.action = delegate
			{
				fuel = 0f;
				parent.BroadcastCompSignal("Refueled");
			};
			yield return command_Action;
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: Set fuel to 0.1";
			command_Action2.action = delegate
			{
				fuel = 0.1f;
				parent.BroadcastCompSignal("Refueled");
			};
			yield return command_Action2;
			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "DEV: Fuel -20%";
			command_Action3.action = delegate
			{
				ConsumeFuel(Props.fuelCapacity * 0.2f);
			};
			yield return command_Action3;
			Command_Action command_Action4 = new Command_Action();
			command_Action4.defaultLabel = "DEV: Set fuel to max";
			command_Action4.action = delegate
			{
				fuel = Props.fuelCapacity;
				parent.BroadcastCompSignal("Refueled");
			};
			yield return command_Action4;
		}
	}
}
