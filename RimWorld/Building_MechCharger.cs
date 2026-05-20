using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_MechCharger : Building
{
	private Pawn currentlyChargingMech;

	private float wasteProduced;

	private int wireExtensionTicks = 70;

	private CompWasteProducer wasteProducer;

	private CompThingContainer container;

	private Sustainer sustainerCharging;

	private Mote moteCharging;

	private Mote moteCablePulse;

	public const float ChargePerDay = 50f;

	private const float ChargePerTick = 0.00083333335f;

	private static readonly Material WasteBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f));

	private static readonly Material WasteBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 1f));

	private const int TicksToExtendWire = 70;

	private const float MinWireExtension = 0.32f;

	private Material wireMaterial;

	public CompPowerTrader Power => this.TryGetComp<CompPowerTrader>();

	public bool IsPowered => Power.PowerOn;

	public bool IsFullOfWaste
	{
		get
		{
			if (wasteProduced >= (float)WasteProducedPerChargingCycle)
			{
				return Container.innerContainer.Any;
			}
			return false;
		}
	}

	private CompWasteProducer WasteProducer
	{
		get
		{
			if (wasteProducer == null)
			{
				wasteProducer = GetComp<CompWasteProducer>();
			}
			return wasteProducer;
		}
	}

	public CompThingContainer Container
	{
		get
		{
			if (container == null)
			{
				container = GetComp<CompThingContainer>();
			}
			return container;
		}
	}

	public GenDraw.FillableBarRequest BarDrawData => def.building.BarDrawDataFor(base.Rotation);

	private Material WireMaterial
	{
		get
		{
			if (wireMaterial == null)
			{
				wireMaterial = MaterialPool.MatFrom("Other/BundledWires", ShaderDatabase.Transparent, Color.white);
			}
			return wireMaterial;
		}
	}

	private bool IsAttachedToMech
	{
		get
		{
			if (currentlyChargingMech != null)
			{
				return wireExtensionTicks >= 70;
			}
			return false;
		}
	}

	private int WasteProducedPerChargingCycle => Container.Props.stackLimit;

	private float WasteProducedPercentFull => wasteProduced / (float)WasteProducedPerChargingCycle;

	private float WasteProducedPerTick => currentlyChargingMech.GetStatValue(StatDefOf.WastepacksPerRecharge) * (0.00083333335f / currentlyChargingMech.needs.energy.MaxLevel);

	public Pawn CurrentlyChargingMech => currentlyChargingMech;

	public override void PostPostMake()
	{
		if (!ModLister.CheckBiotech("Mech recharger"))
		{
			Destroy();
		}
		else
		{
			base.PostPostMake();
		}
	}

	public bool CanPawnChargeCurrently(Pawn pawn)
	{
		if (Power.PowerNet == null)
		{
			return false;
		}
		if (IsFullOfWaste)
		{
			return false;
		}
		if (!IsCompatibleWithCharger(pawn.kindDef))
		{
			return false;
		}
		if (IsPowered)
		{
			if (currentlyChargingMech == null)
			{
				return true;
			}
			if (currentlyChargingMech == pawn)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCompatibleWithCharger(PawnKindDef kindDef)
	{
		return IsCompatibleWithCharger(def, kindDef);
	}

	public static bool IsCompatibleWithCharger(ThingDef chargerDef, PawnKindDef kindDef)
	{
		return IsCompatibleWithCharger(chargerDef, kindDef.race);
	}

	public static bool IsCompatibleWithCharger(ThingDef chargerDef, ThingDef mechRace)
	{
		if (mechRace.race.IsMechanoid && mechRace.GetCompProperties<CompProperties_OverseerSubject>() != null && mechRace.race.mechWeightClass != null)
		{
			return chargerDef.building.requiredMechWeightClasses.NotNullAndContains(mechRace.race.mechWeightClass);
		}
		return false;
	}

	protected override void Tick()
	{
		base.Tick();
		if (currentlyChargingMech != null && (currentlyChargingMech.CurJobDef != JobDefOf.MechCharge || currentlyChargingMech.CurJob.targetA.Thing != this))
		{
			Log.Warning("Mech did not clean up his charging job properly");
			StopCharging();
		}
		if (currentlyChargingMech != null && Power.PowerOn)
		{
			currentlyChargingMech.needs.energy.CurLevel += 0.00083333335f;
			wasteProduced += WasteProducedPerTick;
			wasteProduced = Mathf.Clamp(wasteProduced, 0f, WasteProducedPerChargingCycle);
			if (wasteProduced >= (float)WasteProducedPerChargingCycle && !Container.innerContainer.Any)
			{
				wasteProduced = 0f;
				GenerateWastePack();
			}
			if (moteCablePulse == null || moteCablePulse.Destroyed)
			{
				moteCablePulse = MoteMaker.MakeInteractionOverlay(ThingDefOf.Mote_ChargingCablesPulse, this, new TargetInfo(InteractionCell, base.Map));
			}
			moteCablePulse?.Maintain();
		}
		if (currentlyChargingMech != null && Power.PowerOn && IsAttachedToMech)
		{
			if (sustainerCharging == null)
			{
				sustainerCharging = SoundDefOf.MechChargerCharging.TrySpawnSustainer(SoundInfo.InMap(this));
			}
			sustainerCharging.Maintain();
			if (moteCharging == null || moteCharging.Destroyed)
			{
				moteCharging = MoteMaker.MakeAttachedOverlay(currentlyChargingMech, ThingDefOf.Mote_MechCharging, Vector3.zero);
			}
			moteCharging?.Maintain();
		}
		else if (sustainerCharging != null && (currentlyChargingMech == null || !Power.PowerOn))
		{
			sustainerCharging.End();
			sustainerCharging = null;
		}
		if (wireExtensionTicks < 70)
		{
			wireExtensionTicks++;
		}
	}

	public void GenerateWastePack()
	{
		WasteProducer.ProduceWaste(WasteProducedPerChargingCycle);
		EffecterDefOf.MechChargerWasteProduced.Spawn(base.Position, base.Map).Cleanup();
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			action = delegate
			{
				wasteProduced = WasteProducedPerChargingCycle;
			},
			defaultLabel = "DEV: Waste 100%"
		};
		yield return new Command_Action
		{
			action = delegate
			{
				wasteProduced += 0.25f * (float)WasteProducedPerChargingCycle;
				wasteProduced = Mathf.Clamp(wasteProduced, 0f, WasteProducedPerChargingCycle);
			},
			defaultLabel = "DEV: Waste +25%"
		};
		yield return new Command_Action
		{
			action = delegate
			{
				wasteProduced = 0f;
			},
			defaultLabel = "DEV: Waste 0%"
		};
		if (!Container.innerContainer.Any)
		{
			yield return new Command_Action
			{
				action = GenerateWastePack,
				defaultLabel = "DEV: Generate waste"
			};
		}
		if (currentlyChargingMech != null)
		{
			yield return new Command_Action
			{
				action = delegate
				{
					currentlyChargingMech.needs.TryGetNeed(out Need_MechEnergy need);
					need.CurLevelPercentage = 1f;
				},
				defaultLabel = "DEV: Charge 100%"
			};
		}
	}

	public void StartCharging(Pawn mech)
	{
		if (ModLister.CheckBiotech("Mech charging"))
		{
			if (currentlyChargingMech != null)
			{
				Log.Error("Tried charging on already charging mech charger!");
				return;
			}
			if (!mech.IsColonyMech)
			{
				mech.jobs.EndCurrentJob(JobCondition.Incompletable);
				return;
			}
			currentlyChargingMech = mech;
			mech.needs.energy.currentCharger = this;
			wireExtensionTicks = 0;
			SoundDefOf.MechChargerStart.PlayOneShot(this);
		}
	}

	public void StopCharging()
	{
		if (currentlyChargingMech == null)
		{
			Log.Error("Tried stopping charging on currently not charging mech charger!");
			return;
		}
		if (currentlyChargingMech.needs.energy != null)
		{
			currentlyChargingMech.needs.energy.currentCharger = null;
		}
		currentlyChargingMech = null;
		wireExtensionTicks = 0;
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		if (base.BeingTransportedOnGravship)
		{
			base.DeSpawn(mode);
			return;
		}
		if (currentlyChargingMech != null && mode != DestroyMode.WillReplace)
		{
			Messages.Message("MessageMechChargerDestroyedMechGoesBerserk".Translate(currentlyChargingMech.Named("PAWN")), new LookTargets(currentlyChargingMech), MessageTypeDefOf.NegativeEvent);
			currentlyChargingMech.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.BerserkMechanoid);
		}
		WasteProducer.ProduceWaste(Mathf.CeilToInt(wasteProduced));
		base.DeSpawn(mode);
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		base.DrawAt(drawLoc, flip);
		GenDraw.FillableBarRequest barDrawData = BarDrawData;
		barDrawData.center = DrawPos + Vector3.up * 0.1f;
		barDrawData.fillPercent = WasteProducedPercentFull;
		barDrawData.filledMat = WasteBarFilledMat;
		barDrawData.unfilledMat = WasteBarUnfilledMat;
		barDrawData.rotation = base.Rotation;
		GenDraw.DrawFillableBar(barDrawData);
		Vector3 a = drawLoc;
		float num = EaseInOutQuart((float)wireExtensionTicks / 70f);
		if (currentlyChargingMech == null)
		{
			num = 1f - num;
		}
		num = Mathf.Max(num, 0.32f);
		Vector3 b = Vector3.Lerp(drawLoc, InteractionCell.ToVector3Shifted(), num);
		b.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
		a.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
		GenDraw.DrawLineBetween(a, b, WireMaterial, 1f);
	}

	private float EaseInOutQuart(float val)
	{
		if (!((double)val < 0.5))
		{
			return 1f - Mathf.Pow(-2f * val + 2f, 4f) / 2f;
		}
		return 8f * val * val * val * val;
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.GetInspectString());
		stringBuilder.AppendLineIfNotEmpty();
		stringBuilder.Append("WasteLevel".Translate() + ": " + WasteProducedPercentFull.ToStringPercent());
		return stringBuilder.ToString();
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		IEnumerable<PawnKindDef> source = DefDatabase<PawnKindDef>.AllDefs.Where(IsCompatibleWithCharger);
		string text = source.Select((PawnKindDef pk) => pk.LabelCap.Resolve()).ToLineList("  - ");
		yield return new StatDrawEntry(StatCategoryDefOf.Basics, "StatsReport_RechargerWeightClass".Translate(), def.building.requiredMechWeightClasses.Select((MechWeightClassDef w) => w.label).ToCommaList().CapitalizeFirst(), "StatsReport_RechargerWeightClass_Desc".Translate() + ": " + "\n\n" + text, 99999, null, source.Select((PawnKindDef pk) => new Dialog_InfoCard.Hyperlink(pk.race)));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref currentlyChargingMech, "currentlyChargingMech");
		Scribe_Values.Look(ref wasteProduced, "wasteProduced", 0f);
		Scribe_Values.Look(ref wireExtensionTicks, "wireExtensionTicks", 0);
	}
}
