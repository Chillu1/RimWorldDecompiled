using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Skyfaller : ThingWithComps, IThingHolder, IRoofCollapseAlert
{
	public ThingOwner innerContainer;

	public int ticksToImpact;

	public int ageTicks;

	public int ticksToDiscard;

	public float angle;

	public float shrapnelDirection;

	private int ticksToImpactMax = 220;

	public Letter impactLetter;

	public bool contentsCanOverlap = true;

	private bool hasHitRoof;

	protected bool hasImpacted;

	private bool hasLeftMap;

	public bool moveAside;

	private Material cachedShadowMaterial;

	private bool anticipationSoundPlayed;

	private Sustainer floatingSoundPlaying;

	private Sustainer anticipationSoundPlaying;

	private static MaterialPropertyBlock shadowPropertyBlock = new MaterialPropertyBlock();

	public const float DefaultAngle = -33.7f;

	private const int RoofHitPreDelay = 15;

	private const int LeaveMapAfterTicksDefault = 220;

	protected CompSkyfallerRandomizeDirection randomizeDirectionComp;

	private static readonly List<IntVec3> usedCells = new List<IntVec3>();

	public int LeaveMapAfterTicks
	{
		get
		{
			if (ticksToDiscard <= 0)
			{
				return 220;
			}
			return ticksToDiscard;
		}
	}

	public bool? OverrideFlightFlippedHorizontal { get; set; }

	public CompSkyfallerRandomizeDirection RandomizeDirectionComp => randomizeDirectionComp;

	public override Graphic Graphic
	{
		get
		{
			Thing thingForGraphic = GetThingForGraphic();
			if (def.skyfaller.fadeInTicks > 0 || def.skyfaller.fadeOutTicks > 0)
			{
				return def.graphicData.GraphicColoredFor(thingForGraphic);
			}
			if (thingForGraphic == this)
			{
				return base.Graphic;
			}
			return thingForGraphic.Graphic.ExtractInnerGraphicFor(thingForGraphic).GetShadowlessGraphic();
		}
	}

	public override Vector3 DrawPos
	{
		get
		{
			switch (def.skyfaller.movementType)
			{
			case SkyfallerMovementType.Accelerate:
				return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, ticksToImpact, angle, CurrentSpeed, OverrideFlightFlippedHorizontal ?? def.skyfaller.flightFlippedHorizontally, randomizeDirectionComp);
			case SkyfallerMovementType.ConstantSpeed:
				return SkyfallerDrawPosUtility.DrawPos_ConstantSpeed(base.DrawPos, ticksToImpact, angle, CurrentSpeed, OverrideFlightFlippedHorizontal ?? def.skyfaller.flightFlippedHorizontally, randomizeDirectionComp);
			case SkyfallerMovementType.Decelerate:
				return SkyfallerDrawPosUtility.DrawPos_Decelerate(base.DrawPos, ticksToImpact, angle, CurrentSpeed, OverrideFlightFlippedHorizontal ?? def.skyfaller.flightFlippedHorizontally, randomizeDirectionComp);
			default:
				Log.ErrorOnce("SkyfallerMovementType not handled: " + def.skyfaller.movementType, thingIDNumber ^ 0x7424EBC7);
				return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, ticksToImpact, angle, CurrentSpeed, OverrideFlightFlippedHorizontal ?? def.skyfaller.flightFlippedHorizontally, randomizeDirectionComp);
			}
		}
	}

	public override Color DrawColor
	{
		get
		{
			if (def.skyfaller.fadeInTicks > 0 && ageTicks < def.skyfaller.fadeInTicks)
			{
				Color drawColor = base.DrawColor;
				drawColor.a *= Mathf.Lerp(0f, 1f, Mathf.Min((float)ageTicks / (float)def.skyfaller.fadeInTicks, 1f));
				return drawColor;
			}
			if (FadingOut)
			{
				Color drawColor2 = base.DrawColor;
				drawColor2.a *= Mathf.Lerp(1f, 0f, Mathf.Max((float)ageTicks - (float)(LeaveMapAfterTicks - def.skyfaller.fadeOutTicks), 0f) / (float)def.skyfaller.fadeOutTicks);
				return drawColor2;
			}
			return base.DrawColor;
		}
		set
		{
			base.DrawColor = value;
		}
	}

	public bool FadingOut
	{
		get
		{
			if (def.skyfaller.fadeOutTicks > 0)
			{
				return ageTicks >= LeaveMapAfterTicks - def.skyfaller.fadeOutTicks;
			}
			return false;
		}
	}

	protected Material ShadowMaterial
	{
		get
		{
			if (cachedShadowMaterial == null && !def.skyfaller.shadow.NullOrEmpty())
			{
				cachedShadowMaterial = MaterialPool.MatFrom(def.skyfaller.shadow, ShaderDatabase.Transparent);
			}
			return cachedShadowMaterial;
		}
	}

	protected float TimeInAnimation
	{
		get
		{
			if (def.skyfaller.reversed)
			{
				return (float)ticksToImpact / (float)LeaveMapAfterTicks;
			}
			return 1f - (float)ticksToImpact / (float)ticksToImpactMax;
		}
	}

	private float CurrentSpeed
	{
		get
		{
			if (def.skyfaller.speedCurve == null)
			{
				return def.skyfaller.speed;
			}
			return def.skyfaller.speedCurve.Evaluate(TimeInAnimation) * def.skyfaller.speed;
		}
	}

	private bool SpawnTimedMotes
	{
		get
		{
			if (def.skyfaller.moteSpawnTime == float.MinValue)
			{
				return false;
			}
			return Mathf.Approximately(def.skyfaller.moteSpawnTime, TimeInAnimation);
		}
	}

	public override void PostPostMake()
	{
		base.PostPostMake();
		randomizeDirectionComp = GetComp<CompSkyfallerRandomizeDirection>();
	}

	public Skyfaller()
	{
		innerContainer = new ThingOwner<Thing>(this);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Deep.Look(ref impactLetter, "impactLetter");
		Scribe_Values.Look(ref ticksToImpact, "ticksToImpact", 0);
		Scribe_Values.Look(ref ticksToDiscard, "ticksToDiscard", 0);
		Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
		Scribe_Values.Look(ref ticksToImpactMax, "ticksToImpactMax", LeaveMapAfterTicks);
		Scribe_Values.Look(ref angle, "angle", 0f);
		Scribe_Values.Look(ref shrapnelDirection, "shrapnelDirection", 0f);
		Scribe_Values.Look(ref hasHitRoof, "hasHitRoof", defaultValue: false);
		Scribe_Values.Look(ref hasImpacted, "hasImpacted", defaultValue: false);
		Scribe_Values.Look(ref hasLeftMap, "hasLeftMap", defaultValue: false);
		Scribe_Values.Look(ref contentsCanOverlap, "contentsCanOverlap", defaultValue: true);
		Scribe_Values.Look(ref moveAside, "moveAside", defaultValue: false);
	}

	public override void PostMake()
	{
		base.PostMake();
		if (def.skyfaller.MakesShrapnel)
		{
			shrapnelDirection = Rand.Range(0f, 360f);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (respawningAfterLoad || base.BeingTransportedOnGravship)
		{
			return;
		}
		ticksToImpact = (ticksToImpactMax = def.skyfaller.ticksToImpactRange.RandomInRange);
		ticksToDiscard = ((def.skyfaller.ticksToDiscardInReverse != IntRange.Zero) ? def.skyfaller.ticksToDiscardInReverse.RandomInRange : (-1));
		if (def.skyfaller.MakesShrapnel)
		{
			float num = GenMath.PositiveMod(shrapnelDirection, 360f);
			if (num < 270f && num >= 90f)
			{
				angle = Rand.Range(0f, 33f);
			}
			else
			{
				angle = Rand.Range(-33f, 0f);
			}
		}
		else if (def.skyfaller.angleCurve != null)
		{
			angle = def.skyfaller.angleCurve.Evaluate(0f);
		}
		else
		{
			angle = -33.7f;
		}
		if (def.rotatable && innerContainer.Any)
		{
			base.Rotation = innerContainer[0].Rotation;
		}
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		base.Destroy(mode);
		innerContainer.ClearAndDestroyContents();
		if (anticipationSoundPlaying != null)
		{
			anticipationSoundPlaying.End();
			anticipationSoundPlaying = null;
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		GetDrawPositionAndRotation(ref drawLoc, out var extraRotation);
		Thing thingForGraphic = GetThingForGraphic();
		if (!WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			Graphic.Draw(drawLoc, flip ? thingForGraphic.Rotation.Opposite : thingForGraphic.Rotation, thingForGraphic, extraRotation);
		}
		DrawDropSpotShadow();
	}

	protected virtual void GetDrawPositionAndRotation(ref Vector3 drawLoc, out float extraRotation)
	{
		extraRotation = 0f;
		if (def.skyfaller.rotateGraphicTowardsDirection)
		{
			extraRotation = angle;
		}
		if (randomizeDirectionComp != null)
		{
			extraRotation += randomizeDirectionComp.ExtraDrawAngle;
		}
		if (def.skyfaller.angleCurve != null)
		{
			angle = def.skyfaller.angleCurve.Evaluate(TimeInAnimation);
		}
		if (def.skyfaller.rotationCurve != null)
		{
			extraRotation += def.skyfaller.rotationCurve.Evaluate(TimeInAnimation);
		}
		if (def.skyfaller.xPositionCurve != null)
		{
			drawLoc.x += def.skyfaller.xPositionCurve.Evaluate(TimeInAnimation);
		}
		if (def.skyfaller.zPositionCurve != null)
		{
			drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(TimeInAnimation);
		}
	}

	public virtual float DrawAngle()
	{
		float num = 0f;
		if (def.skyfaller.rotateGraphicTowardsDirection)
		{
			num = angle;
		}
		num += def.skyfaller.rotationCurve.Evaluate(TimeInAnimation);
		if (randomizeDirectionComp != null)
		{
			num += randomizeDirectionComp.ExtraDrawAngle;
		}
		return num;
	}

	protected override void Tick()
	{
		base.Tick();
		if (SpawnTimedMotes)
		{
			CellRect cellRect = this.OccupiedRect();
			for (int i = 0; i < cellRect.Area * def.skyfaller.motesPerCell; i++)
			{
				FleckMaker.ThrowDustPuff(cellRect.RandomVector3, base.Map, 2f);
			}
		}
		if (def.skyfaller.floatingSound != null && (floatingSoundPlaying == null || floatingSoundPlaying.Ended))
		{
			floatingSoundPlaying = def.skyfaller.floatingSound.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(this), MaintenanceType.PerTick));
		}
		floatingSoundPlaying?.Maintain();
		if (def.skyfaller.reversed)
		{
			ticksToImpact++;
			if (!anticipationSoundPlayed && def.skyfaller.anticipationSound != null && ticksToImpact > def.skyfaller.anticipationSoundTicks)
			{
				anticipationSoundPlayed = true;
				TargetInfo targetInfo = new TargetInfo(base.Position, base.Map);
				if (def.skyfaller.anticipationSound.sustain)
				{
					anticipationSoundPlaying = def.skyfaller.anticipationSound.TrySpawnSustainer(targetInfo);
				}
				else
				{
					def.skyfaller.anticipationSound.PlayOneShot(targetInfo);
				}
			}
			if (ticksToImpact >= LeaveMapAfterTicks && !hasLeftMap)
			{
				LeaveMap();
			}
		}
		else
		{
			ticksToImpact--;
			if (ticksToImpact <= 15 && !hasHitRoof)
			{
				HitRoof();
			}
			if (!anticipationSoundPlayed && def.skyfaller.anticipationSound != null && ticksToImpact < def.skyfaller.anticipationSoundTicks)
			{
				anticipationSoundPlayed = true;
				TargetInfo targetInfo2 = new TargetInfo(base.Position, base.Map);
				if (def.skyfaller.anticipationSound.sustain)
				{
					anticipationSoundPlaying = def.skyfaller.anticipationSound.TrySpawnSustainer(targetInfo2);
				}
				else
				{
					def.skyfaller.anticipationSound.PlayOneShot(targetInfo2);
				}
			}
			anticipationSoundPlaying?.Maintain();
			if (ticksToImpact <= 0 && !hasImpacted)
			{
				Impact();
			}
		}
		ageTicks++;
	}

	protected virtual void HitRoof()
	{
		if (!def.skyfaller.hitRoof)
		{
			return;
		}
		CellRect cr = this.OccupiedRect();
		hasHitRoof = true;
		if (!cr.Cells.Any((IntVec3 x) => x.InBounds(base.Map) && x.Roofed(base.Map)))
		{
			return;
		}
		RoofDef roof = cr.Cells.First((IntVec3 x) => x.InBounds(base.Map) && x.Roofed(base.Map)).GetRoof(base.Map);
		if (!roof.soundPunchThrough.NullOrUndefined())
		{
			roof.soundPunchThrough.PlayOneShot(new TargetInfo(base.Position, base.Map));
		}
		CellRect cellRect = cr.ExpandedBy((!def.skyfaller.minimalRoofDestruction) ? 1 : 0).ClipInsideMap(base.Map);
		Map map = base.Map;
		RoofCollapserImmediate.DropRoofInCells(cellRect.Cells.Where(delegate(IntVec3 c)
		{
			if (!c.InBounds(map))
			{
				return false;
			}
			if (cr.Contains(c))
			{
				return true;
			}
			if (c.GetFirstPawn(map) != null)
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			return (edifice == null || !edifice.def.holdsRoof) ? true : false;
		}), map);
	}

	protected virtual void SpawnThings()
	{
		usedCells.Clear();
		int i;
		for (i = innerContainer.Count - 1; i >= 0; i--)
		{
			GenPlace.TryPlaceThing(innerContainer[i], base.Position, base.Map, ThingPlaceMode.Near, delegate(Thing thing, int count)
			{
				PawnUtility.RecoverFromUnwalkablePositionOrKill(thing.Position, thing.Map);
				if (thing.def.Fillage == FillCategory.Full && def.skyfaller.CausesExplosion && def.skyfaller.explosionDamage.isExplosive && thing.Position.InHorDistOf(base.Position, def.skyfaller.explosionRadius))
				{
					base.Map.terrainGrid.Notify_TerrainDestroyed(thing.Position);
				}
				if (moveAside)
				{
					GenSpawn.CheckMoveItemsAside(thing.Position, thing.Rotation, thing.def, thing.Map);
				}
				if (!contentsCanOverlap)
				{
					foreach (IntVec3 item in thing.OccupiedRect())
					{
						usedCells.Add(item);
					}
				}
			}, delegate(IntVec3 c)
			{
				if (!contentsCanOverlap)
				{
					foreach (IntVec3 item2 in GenAdj.OccupiedRect(c, innerContainer[i].def.defaultPlacingRot, innerContainer[i].def.size))
					{
						if (usedCells.Contains(item2))
						{
							return false;
						}
					}
				}
				return true;
			}, innerContainer[i].Rotation);
		}
	}

	protected virtual void Impact()
	{
		hasImpacted = true;
		if (def.skyfaller.CausesExplosion)
		{
			IntVec3 position = base.Position;
			Map map = base.Map;
			float explosionRadius = def.skyfaller.explosionRadius;
			DamageDef explosionDamage = def.skyfaller.explosionDamage;
			int damAmount = GenMath.RoundRandom((float)def.skyfaller.explosionDamage.defaultDamage * def.skyfaller.explosionDamageFactor);
			List<Thing> ignoredThings = ((!def.skyfaller.damageSpawnedThings) ? innerContainer.ToList() : null);
			GenExplosion.DoExplosion(position, map, explosionRadius, explosionDamage, null, damAmount, -1f, null, null, null, null, null, 0f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 0f, damageFalloff: false, null, ignoredThings);
		}
		SpawnThings();
		innerContainer.ClearAndDestroyContents();
		CellRect cellRect = this.OccupiedRect();
		for (int i = 0; i < cellRect.Area * def.skyfaller.motesPerCell; i++)
		{
			FleckMaker.ThrowDustPuff(cellRect.RandomVector3, base.Map, 2f);
		}
		if (def.skyfaller.MakesShrapnel)
		{
			SkyfallerShrapnelUtility.MakeShrapnel(base.Position, base.Map, shrapnelDirection, def.skyfaller.shrapnelDistanceFactor, def.skyfaller.metalShrapnelCountRange.RandomInRange, def.skyfaller.rubbleShrapnelCountRange.RandomInRange, spawnMotes: true);
		}
		if (def.skyfaller.cameraShake > 0f && base.Map == Find.CurrentMap)
		{
			Find.CameraDriver.shaker.DoShake(def.skyfaller.cameraShake);
		}
		if (def.skyfaller.impactSound != null)
		{
			def.skyfaller.impactSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
		}
		if (impactLetter != null)
		{
			Find.LetterStack.ReceiveLetter(impactLetter);
		}
		Map map2 = base.Map;
		Destroy();
		if (def.skyfaller.spawnThing != null)
		{
			GenSpawn.TrySpawn(def.skyfaller.spawnThing, base.Position, map2, out var _);
		}
	}

	protected virtual void LeaveMap()
	{
		hasLeftMap = true;
		Destroy();
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	private Thing GetThingForGraphic()
	{
		if (def.graphicData != null || !innerContainer.Any)
		{
			return this;
		}
		return innerContainer[0];
	}

	protected virtual void DrawDropSpotShadow()
	{
		Material shadowMaterial = ShadowMaterial;
		if (!(shadowMaterial == null))
		{
			DrawDropSpotShadow(base.DrawPos, base.Rotation, shadowMaterial, def.skyfaller.shadowSize, ticksToImpact);
		}
	}

	public static void DrawDropSpotShadow(Vector3 center, Rot4 rot, Material material, Vector2 shadowSize, int ticksToImpact)
	{
		ticksToImpact = Mathf.Max(ticksToImpact, 0);
		Vector3 pos = center;
		pos.y = AltitudeLayer.Shadows.AltitudeFor();
		float num = 1f + (float)ticksToImpact / 100f;
		Vector3 s = new Vector3(num * shadowSize.x, 1f, num * shadowSize.y);
		Color white = Color.white;
		if (ticksToImpact > 150)
		{
			white.a = Mathf.InverseLerp(200f, 150f, ticksToImpact);
		}
		shadowPropertyBlock.SetColor(ShaderPropertyIDs.Color, white);
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(pos, rot.AsQuat, s);
		Graphics.DrawMesh(MeshPool.plane10Back, matrix, material, 0, null, 0, shadowPropertyBlock);
	}

	public RoofCollapseResponse Notify_OnBeforeRoofCollapse()
	{
		return RoofCollapseResponse.None;
	}
}
