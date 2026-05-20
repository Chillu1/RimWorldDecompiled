using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_HoldingPlatform : Building, IThingHolderWithDrawnPawn, IThingHolder, IRoofCollapseAlert, ISearchableContents
{
	public struct Chain
	{
		public Vector3 from;

		public Vector3 to;

		public Graphic graphic;

		public Graphic baseFastenerGraphic;

		public Graphic targetFastenerGraphic;

		public float rotation;
	}

	public ThingOwner innerContainer;

	private int lastDamaged;

	private Graphic chainsUntetheredGraphic;

	private List<Chain> chains;

	private CompAffectedByFacilities facilitiesComp;

	private CompAttachPoints attachPointsComp;

	private AttachPointTracker targetPoints;

	private List<Chain> defaultPointMapping;

	[Unsaved(false)]
	private int debugEscapeTick = -1;

	private int heldPawnStartTick = -1;

	private const float ChainsUntetheredYOffset = 0.05f;

	private const float ChainsTetheredYOffset = 0.13658537f;

	private const float LurchMTBTicks = 100f;

	private const float DamageMTBDays = 2f;

	private static readonly FloatRange Damage = new FloatRange(1f, 3f);

	private const float LungeAnimationChance = 0.25f;

	private Dictionary<AttachPointType, Vector3> platformPoints;

	public float HeldPawnDrawPos_Y => DrawPos.y + 0.03658537f;

	public float HeldPawnBodyAngle => base.Rotation.AsAngle;

	public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

	public Rot4 HeldPawnRotation => base.Rotation;

	public Vector3 PawnDrawOffset => new Vector3(0f, 0f, 0.15f);

	public Pawn HeldPawn => innerContainer.FirstOrDefault((Thing x) => x is Pawn) as Pawn;

	public bool Occupied => HeldPawn != null;

	public float AnimationAlpha => Mathf.Clamp01((float)(Find.TickManager.TicksGame - heldPawnStartTick) / 20f);

	private CompAffectedByFacilities FacilitiesComp => facilitiesComp ?? (facilitiesComp = GetComp<CompAffectedByFacilities>());

	private CompAttachPoints AttachPointsComp => attachPointsComp ?? (attachPointsComp = GetComp<CompAttachPoints>());

	public ThingOwner SearchableContents => innerContainer;

	private AttachPointTracker TargetPawnAttachPoints
	{
		get
		{
			if (targetPoints != null && targetPoints.ThingId != HeldPawn.ThingID)
			{
				targetPoints = null;
			}
			bool num = targetPoints == null;
			targetPoints = targetPoints ?? HeldPawn.TryGetComp<CompAttachPoints>()?.points;
			if (num)
			{
				foreach (HediffComp_AttachPoints hediffComp in HeldPawn.health.hediffSet.GetHediffComps<HediffComp_AttachPoints>())
				{
					if (hediffComp.Points != null)
					{
						if (targetPoints == null)
						{
							targetPoints = hediffComp.Points;
						}
						else
						{
							targetPoints.Add(hediffComp.Points);
						}
					}
				}
			}
			return targetPoints;
		}
	}

	public bool HasAttachedElectroharvester
	{
		get
		{
			foreach (Thing item in FacilitiesComp.LinkedFacilitiesListForReading)
			{
				CompPowerPlantElectroharvester compPowerPlantElectroharvester = item.TryGetComp<CompPowerPlantElectroharvester>();
				if (compPowerPlantElectroharvester != null && compPowerPlantElectroharvester.PowerOn)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool HasAttachedBioferriteHarvester
	{
		get
		{
			foreach (Thing item in FacilitiesComp.LinkedFacilitiesListForReading)
			{
				if (item is Building_BioferriteHarvester building_BioferriteHarvester && building_BioferriteHarvester.Power.PowerOn)
				{
					return true;
				}
			}
			return false;
		}
	}

	private Graphic ChainsUntetheredGraphic
	{
		get
		{
			if (chainsUntetheredGraphic == null)
			{
				chainsUntetheredGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.untetheredGraphicTexPath, ShaderDatabase.Cutout, def.graphicData.drawSize, Color.white);
			}
			return chainsUntetheredGraphic;
		}
	}

	private CompProperties_EntityHolderPlatform PlatformProps => GetComp<CompEntityHolderPlatform>().Props;

	public PawnDrawParms HeldPawnDrawParms => new PawnDrawParms
	{
		pawn = HeldPawn,
		facing = HeldPawn.Rotation,
		rotDrawMode = RotDrawMode.Fresh,
		posture = HeldPawn.GetPosture(),
		flags = (PawnRenderFlags.Headgear | PawnRenderFlags.Clothes),
		tint = Color.white
	};

	public List<Chain> DefaultPointMapping
	{
		get
		{
			if (defaultPointMapping == null)
			{
				defaultPointMapping = new List<Chain>();
				Vector3 worldPos = AttachPointsComp.points.GetWorldPos(AttachPointType.PlatformRestraint0);
				Vector3 worldPos2 = AttachPointsComp.points.GetWorldPos(AttachPointType.PlatformRestraint1);
				Vector3 worldPos3 = AttachPointsComp.points.GetWorldPos(AttachPointType.PlatformRestraint2);
				Vector3 worldPos4 = AttachPointsComp.points.GetWorldPos(AttachPointType.PlatformRestraint3);
				Vector2 vector = new Vector2(Vector3.Distance(worldPos, worldPos3), 1f);
				defaultPointMapping.Add(new Chain
				{
					from = worldPos,
					to = worldPos3,
					graphic = (GraphicDatabase.Get<Graphic_Tiling>(PlatformProps.tilingChainTexPath, ShaderTypeDefOf.Cutout.Shader, vector, Color.white) as Graphic_Tiling).WithTiling(vector),
					baseFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.baseChainFastenerTexPath, ShaderTypeDefOf.Cutout.Shader, Vector2.one, Color.white),
					targetFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.targetChainFastenerTexPath, ShaderTypeDefOf.Cutout.Shader, Vector2.one, Color.white),
					rotation = (worldPos3.WithY(0f) - worldPos.WithY(0f)).normalized.ToAngleFlat()
				});
				vector = new Vector2(Vector3.Distance(worldPos2, worldPos4), 1f);
				defaultPointMapping.Add(new Chain
				{
					from = worldPos2,
					to = worldPos4,
					graphic = (GraphicDatabase.Get<Graphic_Tiling>(PlatformProps.tilingChainTexPath, ShaderTypeDefOf.Cutout.Shader, vector, Color.white) as Graphic_Tiling).WithTiling(vector),
					baseFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.baseChainFastenerTexPath, ShaderTypeDefOf.Cutout.Shader, Vector2.one, Color.white),
					targetFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.targetChainFastenerTexPath, ShaderTypeDefOf.Cutout.Shader, Vector2.one, Color.white),
					rotation = (worldPos4.WithY(0f) - worldPos2.WithY(0f)).normalized.ToAngleFlat()
				});
			}
			return defaultPointMapping;
		}
	}

	public Building_HoldingPlatform()
	{
		innerContainer = new ThingOwner<Thing>(this);
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Holding platform"))
		{
			Destroy();
			return;
		}
		base.SpawnSetup(map, respawningAfterLoad);
		Find.StudyManager.UpdateStudiableCache(this, base.Map);
		Find.Anomaly.hasBuiltHoldingPlatform = true;
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode != DestroyMode.WillReplace)
		{
			EjectContents();
		}
		platformPoints = null;
		base.DeSpawn(mode);
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	private bool TryGetFirstColonistDirection(out Vector2 direction)
	{
		foreach (Thing item in GenRadial.RadialDistinctThingsAround(base.Position, base.Map, 4f, useCenter: false))
		{
			if (item is Pawn { IsColonist: not false } pawn)
			{
				direction = pawn.Position.ToVector2() - base.Position.ToVector2();
				return true;
			}
		}
		direction = Vector2.zero;
		return false;
	}

	protected override void Tick()
	{
		base.Tick();
		if (Occupied && chains == null && AttachPointsComp != null)
		{
			chains = ((TargetPawnAttachPoints != null) ? BuildTargetPointMapping() : DefaultPointMapping);
		}
		if (!Occupied && chains != null)
		{
			chains = null;
		}
		if (Occupied && HasAttachedElectroharvester && Rand.MTBEventOccurs(2f, 60000f, 1f))
		{
			HeldPawn.TakeDamage(new DamageInfo(DamageDefOf.ElectricalBurn, Damage.RandomInRange));
		}
		if (Occupied && Rand.MTBEventOccurs(100f, 1f, 1f))
		{
			UpdateAnimation();
		}
		if (debugEscapeTick > 0 && Find.TickManager.TicksGame == debugEscapeTick && HeldPawn != null)
		{
			HeldPawn.TryGetComp<CompHoldingPlatformTarget>()?.Escape(initiator: false);
		}
		if (heldPawnStartTick == -1 && HeldPawn != null)
		{
			heldPawnStartTick = Find.TickManager.TicksGame;
		}
		else if (HeldPawn == null)
		{
			heldPawnStartTick = -1;
		}
	}

	private void UpdateAnimation()
	{
		if (HeldPawn.TryGetComp<CompHoldingPlatformTarget>(out var comp) && (!comp.Props.hasAnimation || HeldPawn.health.Downed))
		{
			HeldPawn.Drawer.renderer.SetAnimation(null);
			return;
		}
		SoundDef soundDef = PlatformProps.entityLungeSoundLow;
		AnimationDef animationDef = AnimationDefOf.HoldingPlatformWiggleLight;
		if (TryGetFirstColonistDirection(out var direction))
		{
			if (TargetPawnAttachPoints != null && Rand.Chance(0.25f))
			{
				Vector2 vector = direction.normalized.Cardinalize();
				if (vector == Vector2.up)
				{
					animationDef = AnimationDefOf.HoldingPlatformLungeUp;
				}
				if (vector == Vector2.right)
				{
					animationDef = AnimationDefOf.HoldingPlatformLungeRight;
				}
				if (vector == Vector2.left)
				{
					animationDef = AnimationDefOf.HoldingPlatformLungeLeft;
				}
				if (vector == Vector2.down)
				{
					animationDef = AnimationDefOf.HoldingPlatformLungeDown;
				}
				soundDef = PlatformProps.entityLungeSoundHi;
			}
			else
			{
				animationDef = AnimationDefOf.HoldingPlatformWiggleIntense;
			}
		}
		if (HeldPawn.Drawer.renderer.CurAnimation != animationDef)
		{
			soundDef?.PlayOneShot(this);
			HeldPawn.Drawer.renderer.SetAnimation(animationDef);
		}
	}

	public List<Chain> BuildTargetPointMapping()
	{
		if (chains == null)
		{
			chains = new List<Chain>();
		}
		else
		{
			chains.Clear();
		}
		HeldPawn.Drawer.renderer.renderTree.GetRootTPRS(HeldPawnDrawParms, out var offset, out var _, out var rotation, out var _);
		Vector3 vector = DrawPos + PawnDrawOffset;
		Dictionary<AttachPointType, Vector3> dictionary = new Dictionary<AttachPointType, Vector3>();
		int num = 5;
		int num2 = 8;
		foreach (AttachPointType item in TargetPawnAttachPoints.PointTypes(num, num2))
		{
			Vector3 value = vector + rotation * (offset + TargetPawnAttachPoints.GetRotatedOffset(item, base.Rotation));
			dictionary.Add(item, value);
		}
		for (int i = num; i <= num2; i++)
		{
			Vector3 vector2 = GetPlatformPoints()[(AttachPointType)i];
			Vector3 vector3 = dictionary[(AttachPointType)i];
			Vector3 vector4 = Vector3.Lerp(vector2, vector3, AnimationAlpha);
			float x = Vector3.Distance(vector4, vector2);
			Vector2 vector5 = new Vector2(x, 1f);
			chains.Add(new Chain
			{
				from = vector2,
				to = vector4,
				graphic = (GraphicDatabase.Get<Graphic_Tiling>(PlatformProps.tilingChainTexPath, ShaderTypeDefOf.CutoutTiling.Shader, vector5, Color.white) as Graphic_Tiling).WithTiling(vector5),
				baseFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.baseChainFastenerTexPath, ShaderTypeDefOf.CutoutTiling.Shader, Vector2.one, Color.white),
				targetFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.targetChainFastenerTexPath, ShaderTypeDefOf.CutoutTiling.Shader, Vector2.one, Color.white),
				rotation = (vector3.WithY(0f) - vector2.WithY(0f)).normalized.ToAngleFlat()
			});
		}
		return chains;
	}

	private Dictionary<AttachPointType, Vector3> GetPlatformPoints()
	{
		if (platformPoints == null)
		{
			platformPoints = new Dictionary<AttachPointType, Vector3>();
			int min = 5;
			int max = 8;
			foreach (AttachPointType item in AttachPointsComp.points.PointTypes(min, max))
			{
				platformPoints.Add(item, AttachPointsComp.points.GetWorldPos(item));
			}
		}
		return platformPoints;
	}

	public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
	{
		base.DynamicDrawPhaseAt(phase, drawLoc, flip);
		Pawn heldPawn = HeldPawn;
		if (heldPawn != null)
		{
			Rot4 value = Rot4.South;
			if (heldPawn.IsShambler && heldPawn.RaceProps.Animal)
			{
				value = Rot4.East;
			}
			heldPawn.Drawer.renderer.DynamicDrawPhaseAt(phase, DrawPos + PawnDrawOffset, value, neverAimWeapon: true);
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		base.DrawAt(drawLoc, flip);
		if (HeldPawn != null)
		{
			DrawChains();
		}
		else
		{
			ChainsUntetheredGraphic.Draw(drawLoc + Vector3.up * 0.05f, base.Rotation, this);
		}
	}

	private void DrawChains()
	{
		if (chains == null)
		{
			return;
		}
		chains = ((TargetPawnAttachPoints != null) ? BuildTargetPointMapping() : DefaultPointMapping);
		Vector3 vector = Vector3.up * 0.13658537f;
		foreach (Chain chain in chains)
		{
			Vector3 v = (chain.from + chain.to) / 2f;
			chain.graphic.Draw(v.WithY(DrawPos.y) + vector, base.Rotation, this, chain.rotation + 180f);
			chain.targetFastenerGraphic.Draw(chain.to + 2f * vector, base.Rotation, this, chain.rotation + 90f);
			chain.baseFastenerGraphic.Draw(chain.from + 2f * vector, base.Rotation, this, chain.rotation + 90f);
		}
	}

	public void EjectContents()
	{
		defaultPointMapping = null;
		chains = null;
		HeldPawn?.Drawer.renderer.SetAnimation(null);
		HeldPawn?.GetComp<CompHoldingPlatformTarget>()?.Notify_ReleasedFromPlatform();
		innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption;
		}
		if (!Occupied)
		{
			yield break;
		}
		foreach (FloatMenuOption floatMenuOption2 in HeldPawn.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption2;
		}
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		Pawn heldPawn = HeldPawn;
		if (heldPawn != null)
		{
			TaggedString ts = "HoldingThing".Translate() + ": " + heldPawn.NameShortColored.CapitalizeFirst();
			bool flag = this.SafelyContains(heldPawn);
			if (!flag)
			{
				ts += " (" + "HoldingPlatformRequiresStrength".Translate(StatDefOf.MinimumContainmentStrength.Worker.ValueToString(heldPawn.GetStatValue(StatDefOf.MinimumContainmentStrength), finalized: false)) + ")";
			}
			text += ts.Colorize(flag ? Color.white : ColorLibrary.RedReadable);
		}
		else
		{
			text += "HoldingThing".Translate() + ": " + "Nothing".Translate().CapitalizeFirst();
		}
		if (heldPawn != null && heldPawn.def.IsStudiable)
		{
			string inspectStringExtraFor = CompStudiable.GetInspectStringExtraFor(heldPawn);
			if (!inspectStringExtraFor.NullOrEmpty())
			{
				text = text + "\n" + inspectStringExtraFor;
			}
		}
		if (heldPawn != null && heldPawn.TryGetComp<CompProducesBioferrite>(out var comp))
		{
			string text2 = comp.CompInspectStringExtra();
			if (!text2.NullOrEmpty())
			{
				text = text + "\n" + text2;
			}
		}
		return text;
	}

	public override IEnumerable<InspectTabBase> GetInspectTabs()
	{
		foreach (InspectTabBase inspectTab in base.GetInspectTabs())
		{
			yield return inspectTab;
		}
		if (HeldPawn != null && HeldPawn.def.inspectorTabs.Contains(typeof(ITab_StudyNotes)))
		{
			yield return HeldPawn.GetInspectTabs().FirstOrDefault((InspectTabBase tab) => tab is ITab_StudyNotes);
		}
	}

	public void Notify_PawnDied(Pawn pawn, DamageInfo? dinfo)
	{
		if (pawn == HeldPawn)
		{
			innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
			if (!dinfo.HasValue || !dinfo.Value.Def.execution)
			{
				Messages.Message("EntityDiedOnHoldingPlatform".Translate(pawn), pawn, MessageTypeDefOf.NegativeEvent);
			}
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo2 in base.GetGizmos())
		{
			yield return gizmo2;
		}
		if (HeldPawn != null && HeldPawn.TryGetComp<CompActivity>(out var comp))
		{
			foreach (Gizmo item in comp.CompGetGizmosExtra())
			{
				yield return item;
			}
		}
		if (HeldPawn != null && HeldPawn.TryGetComp<CompStudiable>(out var comp2))
		{
			foreach (Gizmo item2 in comp2.CompGetGizmosExtra())
			{
				yield return item2;
			}
		}
		if (HeldPawn != null && HeldPawn.TryGetComp<CompHoldingPlatformTarget>(out var comp3))
		{
			foreach (Gizmo item3 in comp3.CompGetGizmosExtra())
			{
				yield return item3;
			}
		}
		foreach (Thing item4 in (IEnumerable<Thing>)innerContainer)
		{
			Gizmo gizmo = Building.SelectContainedItemGizmo(this, item4);
			if (gizmo != null)
			{
				yield return gizmo;
			}
		}
		if (!DebugSettings.ShowDevGizmos || HeldPawn == null)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Timed escape",
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				for (int i = 1; i < 21; i++)
				{
					int delay = i * 60;
					list.Add(new FloatMenuOption(delay.TicksToSeconds() + "s", delegate
					{
						debugEscapeTick = Find.TickManager.TicksGame + delay;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
	}

	public RoofCollapseResponse Notify_OnBeforeRoofCollapse()
	{
		if (!Occupied)
		{
			return RoofCollapseResponse.None;
		}
		if (HeldPawn is IRoofCollapseAlert roofCollapseAlert)
		{
			roofCollapseAlert.Notify_OnBeforeRoofCollapse();
		}
		foreach (IRoofCollapseAlert comp in HeldPawn.GetComps<IRoofCollapseAlert>())
		{
			comp.Notify_OnBeforeRoofCollapse();
		}
		return RoofCollapseResponse.None;
	}

	public override void Notify_DefsHotReloaded()
	{
		base.Notify_DefsHotReloaded();
		chains = null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastDamaged, "lastDamaged", 0);
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Values.Look(ref heldPawnStartTick, "heldPawnStartTick", 0);
	}
}
