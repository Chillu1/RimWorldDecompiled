using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_GravEngine : Building, IRenameable
{
	public int cooldownCompleteTick = -1;

	public bool silentlyActivate;

	public bool nameHidden = true;

	private string gravshipName = "Gravship".Translate();

	public HashSet<Pawn> pawnsToBoard;

	public HashSet<Pawn> pawnsToLeave;

	public LaunchInfo launchInfo;

	[Unsaved(false)]
	private CompAffectedByFacilities affectedByFacilities;

	[Unsaved(false)]
	private List<CompGravshipFacility> gravshipComponents = new List<CompGravshipFacility>();

	[Unsaved(false)]
	private HashSet<IntVec3> validSubstructure = new HashSet<IntVec3>();

	[Unsaved(false)]
	private HashSet<IntVec3> allConnectedSubstructure = new HashSet<IntVec3>();

	[Unsaved(false)]
	private List<GravshipComponentTypeDef> missingComponents = new List<GravshipComponentTypeDef>();

	[Unsaved(false)]
	public bool substructureDirty = true;

	[Unsaved(false)]
	private bool missingComponentsDirty = true;

	[Unsaved(false)]
	private bool gravshipComponentsDirty = true;

	[Unsaved(false)]
	private bool haveShownNameDialog;

	[Unsaved(false)]
	private bool hasTickedThisSession;

	private static readonly Texture2D InspectCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/Inspect");

	private static readonly CachedMaterial OrbMat = new CachedMaterial("Things/Building/GravEngine/GravEngine_Orb", ShaderDatabase.Cutout);

	private static Graphic onCooldownGraphic;

	private const float BaseFuelPerTile = 10f;

	private const float BobHeight = 0.3f;

	private const int NameDialogSubstructureThreshold = 90;

	private HashSet<Room> tmpGravshipRooms = new HashSet<Room>();

	private static Graphic OnCooldownGraphic => onCooldownGraphic ?? (onCooldownGraphic = GraphicDatabase.Get<Graphic_Single>("Things/Building/GravEngine/GravEngine_Cooldown", ShaderDatabase.Cutout, Vector2.one * 3f, Color.white));

	public string RenamableLabel
	{
		get
		{
			return gravshipName ?? BaseLabel;
		}
		set
		{
			gravshipName = value;
			nameHidden = false;
		}
	}

	public string BaseLabel => def.LabelCap;

	public string InspectLabel => BaseLabel;

	public override Graphic Graphic
	{
		get
		{
			if (Find.TickManager.TicksGame >= cooldownCompleteTick)
			{
				return base.Graphic;
			}
			return OnCooldownGraphic;
		}
	}

	public CompAffectedByFacilities AffectedByFacilities => affectedByFacilities ?? (affectedByFacilities = GetComp<CompAffectedByFacilities>());

	public int MaxLaunchDistance => (int)this.GetStatValue(StatDefOf.GravshipRange);

	public HashSet<IntVec3> ValidSubstructure
	{
		get
		{
			UpdateSubstructureIfNeeded(regenerateSectionLayers: true);
			return validSubstructure;
		}
	}

	public HashSet<IntVec3> AllConnectedSubstructure
	{
		get
		{
			UpdateSubstructureIfNeeded(regenerateSectionLayers: true);
			return allConnectedSubstructure;
		}
	}

	public HashSet<IntVec3> ValidSubstructureNoRegen
	{
		get
		{
			UpdateSubstructureIfNeeded(regenerateSectionLayers: false);
			return validSubstructure;
		}
	}

	public HashSet<IntVec3> AllConnectedSubstructureNoRegen
	{
		get
		{
			UpdateSubstructureIfNeeded(regenerateSectionLayers: false);
			return allConnectedSubstructure;
		}
	}

	public List<CompGravshipFacility> GravshipComponents
	{
		get
		{
			if (gravshipComponentsDirty)
			{
				gravshipComponentsDirty = false;
				gravshipComponents.Clear();
				foreach (Thing item in AffectedByFacilities.LinkedFacilitiesListForReading)
				{
					if (item.TryGetComp(out CompGravshipFacility comp))
					{
						gravshipComponents.Add(comp);
					}
				}
			}
			return gravshipComponents;
		}
	}

	public float TotalFuel
	{
		get
		{
			float num = 0f;
			foreach (CompGravshipFacility gravshipComponent in GravshipComponents)
			{
				if (gravshipComponent.parent.Spawned && gravshipComponent.Props.providesFuel && gravshipComponent.CanBeActive && gravshipComponent.parent.TryGetComp<CompRefuelable>(out var comp))
				{
					num += comp.Fuel;
				}
			}
			return num;
		}
	}

	public float MaxFuel
	{
		get
		{
			float num = 0f;
			foreach (CompGravshipFacility gravshipComponent in GravshipComponents)
			{
				if (gravshipComponent.parent.Spawned && gravshipComponent.Props.providesFuel && gravshipComponent.CanBeActive && gravshipComponent.parent.TryGetComp<CompRefuelable>(out var comp))
				{
					num += comp.Props.fuelCapacity;
				}
			}
			return num;
		}
	}

	public float FuelSavingsPercent
	{
		get
		{
			float num = 0f;
			foreach (CompGravshipFacility gravshipComponent in GravshipComponents)
			{
				if (gravshipComponent.parent.Spawned && gravshipComponent.Props.fuelSavingsPercent > 0f && gravshipComponent.CanBeActive)
				{
					num += gravshipComponent.Props.fuelSavingsPercent;
				}
			}
			return Mathf.Clamp01(num);
		}
	}

	public float FuelPerTile => 10f * FuelUseageFactor;

	public float FuelUseageFactor => 1f - FuelSavingsPercent;

	public List<GravshipComponentTypeDef> MissingComponents
	{
		get
		{
			if (missingComponentsDirty)
			{
				missingComponentsDirty = false;
				missingComponents.Clear();
				foreach (GravshipComponentTypeDef def in DefDatabase<GravshipComponentTypeDef>.AllDefsListForReading)
				{
					if (def.requiredForLaunch && GravshipComponents.Find((CompGravshipFacility c) => c.Props.componentTypeDef == def) == null)
					{
						missingComponents.Add(def);
					}
				}
			}
			return missingComponents;
		}
	}

	public bool HasSignalJammer => GravshipComponents.Any((CompGravshipFacility c) => c.Props.componentTypeDef == GravshipComponentTypeDefOf.SignalJammer);

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			ForceSubstructureDirty();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.DeSpawn(mode);
		missingComponents.Clear();
		ForceSubstructureDirty();
	}

	public override void PostSwapMap()
	{
		base.PostSwapMap();
		ForceSubstructureDirty();
	}

	protected override void Tick()
	{
		base.Tick();
		hasTickedThisSession = true;
		if (silentlyActivate)
		{
			Inspect(silent: true);
			silentlyActivate = false;
		}
		if (Find.TickManager.TicksGame == cooldownCompleteTick)
		{
			Messages.Message("GravshipReadyToLaunch".Translate(RenamableLabel), this, MessageTypeDefOf.NeutralEvent, historical: false);
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		base.DrawAt(drawLoc, flip);
		if (base.Spawned)
		{
			if (Find.TickManager.TicksGame >= cooldownCompleteTick)
			{
				drawLoc.z += 0.5f * (1f + Mathf.Sin(MathF.PI * 2f * (float)GenTicks.TicksGame / 500f)) * 0.3f;
			}
			drawLoc.y += 0.03658537f;
			Vector3 s = new Vector3(def.graphicData.drawSize.x, 1f, def.graphicData.drawSize.y);
			Graphics.DrawMesh(MeshPool.plane10Back, Matrix4x4.TRS(drawLoc, base.Rotation.AsQuat, s), OrbMat.Material, 0, null, 0);
		}
	}

	public void AddComponent(CompGravshipFacility comp)
	{
		ForceSubstructureDirty();
	}

	public void RemoveComponent(CompGravshipFacility comp)
	{
		ForceSubstructureDirty();
	}

	public AcceptanceReport CanLaunch(CompPilotConsole console)
	{
		Map map = base.Map;
		if (map != null && map.generatorDef?.pocketMapProperties?.canLaunchGravship == false)
		{
			return new AcceptanceReport(base.Map.generatorDef.label);
		}
		if (ValidSubstructure.Count == 0)
		{
			return new AcceptanceReport("CannotLaunchNoSubstructure".Translate().CapitalizeFirst());
		}
		if (!console.CanBeActive)
		{
			return new AcceptanceReport("CannotLaunchDisconnected".Translate().CapitalizeFirst());
		}
		if (TotalFuel < 10f)
		{
			return new AcceptanceReport("CannotLaunchNotEnoughFuel".Translate().CapitalizeFirst());
		}
		if (MaxLaunchDistance <= 0)
		{
			return new AcceptanceReport("CannotLaunchNoThrusters".Translate().CapitalizeFirst());
		}
		if (GenTicks.TicksGame < cooldownCompleteTick)
		{
			return new AcceptanceReport("CannotLaunchOnCooldown".Translate((cooldownCompleteTick - GenTicks.TicksGame).ToStringTicksToPeriod()).CapitalizeFirst());
		}
		return AcceptanceReport.WasAccepted;
	}

	public void ConsumeFuel(PlanetTile tile)
	{
		if (!GravshipUtility.TryGetPathFuelCost(base.Map.Tile, tile, out var cost, out var _))
		{
			Log.Error($"Failed to get the fuel cost from tile ({base.Map.Tile}) to {tile}.");
			return;
		}
		float num = cost / TotalFuel;
		foreach (CompGravshipFacility gravshipComponent in GravshipComponents)
		{
			if (gravshipComponent.CanBeActive && gravshipComponent.Props.providesFuel)
			{
				CompRefuelable comp = gravshipComponent.parent.GetComp<CompRefuelable>();
				comp?.ConsumeFuel(comp.Fuel * num);
			}
		}
		cooldownCompleteTick = GenTicks.TicksGame + (int)GravshipUtility.LaunchCooldownFromQuality(launchInfo?.quality ?? 1f);
	}

	public bool ValidSubstructureAt(IntVec3 cell)
	{
		return ValidSubstructure.Contains(cell);
	}

	public bool OnValidSubstructure(Thing thing)
	{
		if (!thing.Spawned)
		{
			return false;
		}
		BuildingProperties building = thing.def.building;
		if (building != null && building.isAttachment)
		{
			Thing wallAttachedTo = GenConstruct.GetWallAttachedTo(thing);
			return ValidSubstructureAt(wallAttachedTo.Position);
		}
		foreach (IntVec3 item in thing.OccupiedRect())
		{
			if (!ValidSubstructureAt(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool LooselyConnectedToGravEngine(Thing thing)
	{
		if (!thing.Spawned)
		{
			return false;
		}
		foreach (IntVec3 item in thing.OccupiedRect())
		{
			if (!AllConnectedSubstructure.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public void Inspect(bool silent = false)
	{
		if (Find.ResearchManager.gravEngineInspected)
		{
			return;
		}
		Find.ResearchManager.gravEngineInspected = true;
		QuestUtility.SendQuestTargetSignals(questTags, "Inspected", this.Named("SUBJECT"));
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.Gravship, OpportunityType.Important);
		if (base.Faction != Faction.OfPlayer)
		{
			SetFaction(Faction.OfPlayer);
		}
		Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.GravEngine, StorytellerUtility.DefaultThreatPointsNow(base.Map));
		if (!silent)
		{
			DiaNode diaNode = new DiaNode("GravEngineInspectedLetterContents".Translate());
			DiaOption item = new DiaOption("ViewQuest".Translate())
			{
				resolveTree = true,
				action = delegate
				{
					Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
					((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
				}
			};
			diaNode.options.Add(item);
			diaNode.options.Add(new DiaOption("Close".Translate())
			{
				resolveTree = true
			});
			Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode);
			dialog_NodeTree.forcePause = true;
			Find.WindowStack.Add(dialog_NodeTree);
		}
	}

	public void ForceSubstructureDirty()
	{
		substructureDirty = true;
		missingComponentsDirty = true;
		gravshipComponentsDirty = true;
	}

	private void UpdateSubstructureIfNeeded(bool regenerateSectionLayers)
	{
		if (!ModsConfig.OdysseyActive || base.Destroyed || !base.Spawned)
		{
			return;
		}
		if (substructureDirty)
		{
			substructureDirty = false;
			GravshipUtility.GetConnectedSubstructure(this, allConnectedSubstructure, int.MaxValue, requireInsideFootprint: false);
			GravshipUtility.GetConnectedSubstructure(this, validSubstructure, (int)this.GetStatValue(StatDefOf.SubstructureSupport));
			base.Map?.substructureGrid?.Drawer.SetDirty();
			if (regenerateSectionLayers && base.Map != null && base.Map.mapDrawer != null)
			{
				base.Map.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_GravshipMask));
				base.Map.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_GravshipHull));
				base.Map.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_SubstructureProps));
			}
		}
		if (hasTickedThisSession && nameHidden && !haveShownNameDialog && Find.ResearchManager.gravEngineInspected && base.Faction == Faction.OfPlayer && validSubstructure.Count > 90)
		{
			Find.WindowStack.Add(new Dialog_NamePlayerGravship(this));
			haveShownNameDialog = true;
		}
	}

	public override AcceptanceReport ClaimableBy(Faction by)
	{
		AcceptanceReport result = base.ClaimableBy(by);
		if (!result.Accepted)
		{
			return result;
		}
		if (!Find.ResearchManager.gravEngineInspected)
		{
			return "GravEngineNotInspected".Translate();
		}
		return true;
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (!base.Spawned)
		{
			return text;
		}
		if (text.Length > 0)
		{
			text += "\n";
		}
		if (!Find.ResearchManager.gravEngineInspected)
		{
			text += "GravEngineNotInspected".Translate() + "\n";
		}
		text = string.Concat(text, "ConnectedSubstructure".Translate() + ": ", AllConnectedSubstructure.Count.ToString(), " / ", StatDefOf.SubstructureSupport.ValueToString(this.GetStatValue(StatDefOf.SubstructureSupport)));
		int count = base.Map.listerThings.ThingsOfDef(ThingDefOf.Blueprint_Substructure).Count;
		count += base.Map.listerThings.ThingsOfDef(ThingDefOf.Frame_Substructure).Count;
		if (count > 0)
		{
			text = string.Concat(text, "\n" + "SubstructureBlueprintsCount".Translate() + ": ", count.ToString());
		}
		if (MissingComponents.Count > 0)
		{
			text = text + "\n" + "Requires".Translate().ToString() + ": " + MissingComponents.Select((GravshipComponentTypeDef d) => d.label).ToCommaList().Colorize(ColorLibrary.RedReadable)
				.CapitalizeFirst();
		}
		return text;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!base.Spawned || Find.ResearchManager.gravEngineInspected)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "CommandInspectGravEngine".Translate(this),
			defaultDesc = "CommandInspectGravEngineDesc".Translate(),
			icon = InspectCommandTex,
			action = delegate
			{
				Find.Targeter.BeginTargeting(TargetingParameters.ForColonist(), delegate(LocalTargetInfo targ)
				{
					if (targ.Thing is Pawn pawn)
					{
						pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.InspectGravEngine, this), JobTag.Misc);
					}
				}, delegate(LocalTargetInfo targ)
				{
					if (ValidateInspectionTarget(targ))
					{
						GenDraw.DrawTargetHighlight(targ);
					}
				}, ValidateInspectionTarget, null, null, InspectCommandTex, playSoundOnAction: true, delegate
				{
					Widgets.MouseAttachedLabel("ChooseWhoShouldInspect".Translate(this));
				});
			}
		};
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Inspect now",
				action = delegate
				{
					Inspect();
				}
			};
		}
		static bool ValidateInspectionTarget(LocalTargetInfo targ)
		{
			if (!(targ.Thing is Pawn pawn))
			{
				return false;
			}
			if (!pawn.IsColonistPlayerControlled)
			{
				return false;
			}
			if (!pawn.CanReach(pawn.Position, PathEndMode.Touch, Danger.Deadly))
			{
				return false;
			}
			if (pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
			{
				return false;
			}
			return true;
		}
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption;
		}
		if (!Find.ResearchManager.gravEngineInspected)
		{
			yield return new FloatMenuOption("CommandInspectGravEngine".Translate(this), delegate
			{
				selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.InspectGravEngine, this), JobTag.Misc);
			});
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats())
		{
			yield return item;
		}
		if (FuelSavingsPercent > 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_GravEngineFuelConsumption".Translate(), (0f - FuelSavingsPercent).ToStringPercentSigned("F0"), "StatsReport_GravEngineFuelConsumption_Desc".Translate(), 520);
		}
	}

	public IEnumerable<TaggedString> GetOrbitalWarnings()
	{
		bool flag = true;
		int heatPusherCount = 0;
		int oxygenPusherCount = 0;
		tmpGravshipRooms.Clear();
		foreach (IntVec3 item in validSubstructure)
		{
			foreach (Thing thing in item.GetThingList(base.Map))
			{
				if (thing.HasComp<CompOxygenPusher>())
				{
					oxygenPusherCount++;
				}
				if (thing is Building_Heater || (thing.TryGetComp(out CompHeatPusher comp) && comp.Props.heatPerSecond > 20f))
				{
					heatPusherCount++;
				}
			}
			Room room = item.GetRoom(base.Map);
			if (room != null && !tmpGravshipRooms.Contains(room) && !room.UsesOutdoorTemperature && !room.IsDoorway)
			{
				tmpGravshipRooms.Add(room);
				if (!VacuumUtility.IsRoomAirtight(room))
				{
					flag = false;
				}
			}
		}
		string baseWarning = ("OrbitalWarning_Warning".Translate() + ": ").Colorize(ColorLibrary.RedReadable);
		if (!flag)
		{
			yield return baseWarning + "OrbitalWarning_Vacuum".Translate();
		}
		if (heatPusherCount == 0)
		{
			yield return baseWarning + "OrbitalWarning_NoHeatSources".Translate();
		}
		else if ((float)heatPusherCount < (float)ValidSubstructure.Count / 250f)
		{
			yield return baseWarning + "OrbitalWarning_FewHeatSources".Translate() + ": " + heatPusherCount.ToString();
		}
		if (oxygenPusherCount == 0)
		{
			yield return baseWarning + "OrbitalWarning_NoOxygenPumps".Translate();
		}
		else if ((float)oxygenPusherCount < (float)ValidSubstructure.Count / 400f)
		{
			yield return baseWarning + "OrbitalWarning_FewOxygenPumps".Translate() + ": " + oxygenPusherCount.ToString();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref cooldownCompleteTick, "cooldownCompleteTick", -1);
		Scribe_Values.Look(ref silentlyActivate, "silentlyActivate", defaultValue: false);
		Scribe_Values.Look(ref nameHidden, "nameHidden", defaultValue: true);
		Scribe_Values.Look(ref gravshipName, "gravshipName");
		Scribe_Deep.Look(ref launchInfo, "launchInfo");
		Scribe_Collections.Look(ref pawnsToBoard, "pawnsToBoard", LookMode.Reference);
		Scribe_Collections.Look(ref pawnsToLeave, "pawnsToLeave", LookMode.Reference);
	}
}
