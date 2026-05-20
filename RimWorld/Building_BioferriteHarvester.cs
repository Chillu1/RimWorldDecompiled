using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Building_BioferriteHarvester : Building
{
	public float containedBioferrite;

	public bool unloadingEnabled = true;

	private bool initalized;

	private const int HaulingThreshold = 30;

	private const float MaxCapacity = 60f;

	private CompFacility facilityComp;

	private CompPowerTrader powerComp;

	private CompCableConnection cableConnection;

	private Sustainer workingSustainer;

	public CompFacility FacilityComp => facilityComp ?? (facilityComp = GetComp<CompFacility>());

	public CompPowerTrader Power => powerComp ?? (powerComp = GetComp<CompPowerTrader>());

	public List<Thing> Platforms => FacilityComp.LinkedBuildings;

	public bool ReadyForHauling => Mathf.FloorToInt(containedBioferrite) >= 30;

	private float BioferritePerDay
	{
		get
		{
			if (!Power.PowerOn)
			{
				return 0f;
			}
			float num = 0f;
			foreach (Thing platform in Platforms)
			{
				if (platform is Building_HoldingPlatform { Occupied: not false } building_HoldingPlatform)
				{
					num += CompProducesBioferrite.BioferritePerDay(building_HoldingPlatform.HeldPawn);
				}
			}
			return num;
		}
	}

	public CompCableConnection CableConnection => cableConnection ?? (cableConnection = GetComp<CompCableConnection>());

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			Initialize();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.DeSpawn(mode);
		FacilityComp.OnLinkAdded -= OnLinkAdded;
		FacilityComp.OnLinkRemoved -= OnLinkRemoved;
		initalized = false;
		workingSustainer?.End();
		workingSustainer = null;
	}

	private void Initialize()
	{
		if (initalized)
		{
			return;
		}
		initalized = true;
		FacilityComp.OnLinkAdded += OnLinkAdded;
		FacilityComp.OnLinkRemoved += OnLinkRemoved;
		foreach (Thing platform in Platforms)
		{
			if (platform is Building_HoldingPlatform building_HoldingPlatform)
			{
				building_HoldingPlatform.innerContainer.OnContentsChanged += RebuildCables;
			}
		}
		RebuildCables();
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		base.DrawAt(drawLoc, flip);
		if (!initalized)
		{
			Initialize();
		}
	}

	private void OnLinkRemoved(CompFacility facility, Thing thing)
	{
		if (thing is Building_HoldingPlatform building_HoldingPlatform)
		{
			building_HoldingPlatform.innerContainer.OnContentsChanged -= RebuildCables;
			RebuildCables();
		}
	}

	private void OnLinkAdded(CompFacility facility, Thing thing)
	{
		if (thing is Building_HoldingPlatform building_HoldingPlatform)
		{
			building_HoldingPlatform.innerContainer.OnContentsChanged += RebuildCables;
			RebuildCables();
		}
	}

	protected override void Tick()
	{
		base.Tick();
		if (this.IsHashIntervalTick(250))
		{
			containedBioferrite = Mathf.Min(containedBioferrite + BioferritePerDay / 60000f * 250f, 60f);
		}
		if (IsWorking())
		{
			if (workingSustainer == null)
			{
				workingSustainer = SoundDefOf.BioferriteHarvester_Ambient.TrySpawnSustainer(SoundInfo.InMap(this));
			}
			workingSustainer.Maintain();
		}
		else
		{
			workingSustainer?.End();
			workingSustainer = null;
		}
	}

	public override bool IsWorking()
	{
		return BioferritePerDay != 0f;
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.GetInspectString());
		if (stringBuilder.Length != 0)
		{
			stringBuilder.AppendLine();
		}
		stringBuilder.Append("BioferriteHarvesterContained".Translate());
		stringBuilder.Append($": {containedBioferrite:F2} / {60f.ToString()} (+{BioferritePerDay:F2} ");
		stringBuilder.Append(string.Format("{0})", "BioferriteHarvesterPerDay".Translate()));
		return stringBuilder.ToString();
	}

	public Thing TakeOutBioferrite()
	{
		int num = Mathf.FloorToInt(containedBioferrite);
		if (num == 0)
		{
			return null;
		}
		containedBioferrite -= num;
		Thing thing = ThingMaker.MakeThing(ThingDefOf.Bioferrite);
		thing.stackCount = num;
		return thing;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		Command_Toggle command_Toggle = new Command_Toggle();
		command_Toggle.defaultLabel = "BioferriteHarvesterToggleUnloading".Translate();
		command_Toggle.defaultDesc = "BioferriteHarvesterToggleUnloadingDesc".Translate();
		command_Toggle.isActive = () => unloadingEnabled;
		command_Toggle.toggleAction = delegate
		{
			unloadingEnabled = !unloadingEnabled;
		};
		command_Toggle.activateSound = SoundDefOf.Tick_Tiny;
		command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/BioferriteUnloading");
		yield return command_Toggle;
		if (containedBioferrite >= 1f)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "BioferriteHarvesterEjectContents".Translate();
			command_Action.defaultDesc = "BioferriteHarvesterEjectContentsDesc".Translate(Find.ActiveLanguageWorker.Pluralize(ThingDefOf.Bioferrite.label));
			command_Action.action = delegate
			{
				EjectContents();
			};
			command_Action.Disabled = containedBioferrite == 0f;
			command_Action.activateSound = SoundDefOf.Tick_Tiny;
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/EjectBioferrite");
			yield return command_Action;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: Add +1 bioferrite";
			command_Action2.action = delegate
			{
				containedBioferrite = Mathf.Min(containedBioferrite + 1f, 60f);
			};
			yield return command_Action2;
		}
	}

	private void EjectContents()
	{
		Thing thing = TakeOutBioferrite();
		if (thing != null)
		{
			GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near);
		}
	}

	public override void Notify_DefsHotReloaded()
	{
		base.Notify_DefsHotReloaded();
		RebuildCables();
	}

	private void RebuildCables()
	{
		CableConnection.RebuildCables(Platforms, (Thing thing) => thing is Building_HoldingPlatform building_HoldingPlatform && building_HoldingPlatform.Occupied);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref containedBioferrite, "containedBioferrite", 0f);
		Scribe_Values.Look(ref unloadingEnabled, "unloadingEnabled", defaultValue: false);
	}
}
