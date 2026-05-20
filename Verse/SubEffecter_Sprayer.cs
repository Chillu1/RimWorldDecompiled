using RimWorld;
using UnityEngine;

namespace Verse;

public abstract class SubEffecter_Sprayer : SubEffecter
{
	private Mote mote;

	private Vector3? lastOffset;

	public SubEffecter_Sprayer(SubEffecterDef def, Effecter parent)
		: base(def, parent)
	{
	}

	public Vector3 GetAttachedSpawnLoc(TargetInfo tgt)
	{
		Vector3 centerVector = tgt.CenterVector3;
		if (def.attachPoint != AttachPointType.RootNone && tgt.HasThing && tgt.Thing.TryGetComp(out CompAttachPoints comp))
		{
			return comp.points.GetWorldPos(def.attachPoint);
		}
		return centerVector;
	}

	protected void MakeMote(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1)
	{
		Vector3 vector = Vector3.zero;
		switch (base.EffectiveSpawnLocType)
		{
		case MoteSpawnLocType.OnSource:
			vector = GetAttachedSpawnLoc(A);
			break;
		case MoteSpawnLocType.OnTarget:
			vector = GetAttachedSpawnLoc(B);
			break;
		case MoteSpawnLocType.BetweenPositions:
		{
			Vector3 vector5 = (A.HasThing ? A.Thing.DrawPos : A.Cell.ToVector3Shifted());
			Vector3 vector6 = (B.HasThing ? B.Thing.DrawPos : B.Cell.ToVector3Shifted());
			vector = ((A.HasThing && !A.Thing.Spawned) ? vector6 : ((!B.HasThing || B.Thing.Spawned) ? (vector5 * def.positionLerpFactor + vector6 * (1f - def.positionLerpFactor)) : vector5));
			break;
		}
		case MoteSpawnLocType.RandomCellOnTarget:
			vector = ((!B.HasThing) ? CellRect.CenteredOn(B.Cell, 0) : B.Thing.OccupiedRect()).RandomCell.ToVector3Shifted();
			break;
		case MoteSpawnLocType.RandomDrawPosOnTarget:
			if (B.Thing.DrawSize != Vector2.one && B.Thing.DrawSize != Vector2.zero)
			{
				Vector2 vector2 = B.Thing.DrawSize.RotatedBy(B.Thing.Rotation);
				Vector3 vector3 = new Vector3(vector2.x * Rand.Value, 0f, vector2.y * Rand.Value);
				vector = B.CenterVector3 + vector3 - new Vector3(vector2.x / 2f, 0f, vector2.y / 2f);
			}
			else
			{
				Vector3 vector4 = new Vector3(Rand.Value, 0f, Rand.Value);
				vector = B.CenterVector3 + vector4 - new Vector3(0.5f, 0f, 0.5f);
			}
			break;
		case MoteSpawnLocType.BetweenTouchingCells:
			vector = A.Cell.ToVector3Shifted() + (B.Cell - A.Cell).ToVector3().normalized * 0.5f;
			break;
		}
		if (parent != null)
		{
			Rand.PushState(parent.GetHashCode());
			if (A.CenterVector3 != B.CenterVector3)
			{
				vector += (B.CenterVector3 - A.CenterVector3).normalized * parent.def.offsetTowardsTarget.RandomInRange;
			}
			Vector3 vector7 = Gen.RandomHorizontalVector(parent.def.positionRadius);
			Rand.PopState();
			if (base.EffectiveDimensions.HasValue)
			{
				vector7 += Gen.Random2DVector(base.EffectiveDimensions.Value);
			}
			vector += vector7 + parent.offset;
		}
		Map map = A.Map ?? B.Map;
		float num = (def.absoluteAngle ? 0f : ((def.useTargetAInitialRotation && A.HasThing) ? A.Thing.Rotation.AsAngle : ((!def.useTargetBInitialRotation || !B.HasThing) ? (B.Cell - A.Cell).AngleFlat : B.Thing.Rotation.AsAngle)));
		float num2 = ((parent != null) ? parent.scale : 1f);
		if (map == null)
		{
			return;
		}
		int randomInRange = def.burstCount.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			Vector3 vector8 = base.EffectiveOffset;
			if (def.useTargetAInitialRotation && A.HasThing)
			{
				vector8 = vector8.RotatedBy(A.Thing.Rotation);
			}
			else if (def.useTargetBInitialRotation && B.HasThing)
			{
				vector8 = vector8.RotatedBy(B.Thing.Rotation);
			}
			else if (def.useTargetABodyAngle && A.HasThing && A.Thing is Pawn pawn)
			{
				vector8 = vector8.RotatedBy(pawn.Drawer.renderer.BodyAngle(PawnRenderFlags.None));
			}
			else if (def.useTargetBBodyAngle && B.HasThing && B.Thing is Pawn pawn2)
			{
				vector8 = vector8.RotatedBy(pawn2.Drawer.renderer.BodyAngle(PawnRenderFlags.None));
			}
			if (!def.perRotationOffsets.NullOrEmpty())
			{
				vector8 += def.perRotationOffsets[((base.EffectiveSpawnLocType == MoteSpawnLocType.OnSource) ? A.Thing.Rotation : B.Thing.Rotation).AsInt];
			}
			for (int j = 0; j < 5; j++)
			{
				vector8 = vector8 * num2 + Rand.InsideAnnulusVector3(def.positionRadiusMin, def.positionRadius) * num2;
				if (def.avoidLastPositionRadius < float.Epsilon || !lastOffset.HasValue || (vector8 - lastOffset.Value).MagnitudeHorizontal() > def.avoidLastPositionRadius)
				{
					break;
				}
			}
			lastOffset = vector8;
			Vector3 vector9 = vector + vector8;
			if (def.rotateTowardsTargetCenter)
			{
				num = (vector9 - B.CenterVector3).AngleFlat();
			}
			if (def.moteDef != null && vector.ShouldSpawnMotesAt(map, def.moteDef.drawOffscreen))
			{
				mote = (Mote)ThingMaker.MakeThing(def.moteDef);
				GenSpawn.Spawn(mote, vector.ToIntVec3(), map);
				mote.Scale = def.scale.RandomInRange * num2;
				mote.exactPosition = vector9;
				mote.rotationRate = def.rotationRate.RandomInRange;
				mote.exactRotation = def.rotation.RandomInRange + num;
				mote.instanceColor = base.EffectiveColor;
				mote.yOffset = vector8.y;
				mote.curvedScale = def.moteDef.mote.scalers?.ScaleAtTime(0f) ?? Vector3.one;
				if (overrideSpawnTick != -1)
				{
					mote.ForceSpawnTick(overrideSpawnTick);
				}
				if (mote is MoteThrown moteThrown)
				{
					moteThrown.airTimeLeft = def.airTime.RandomInRange;
					moteThrown.SetVelocity(def.angle.RandomInRange + num, def.speed.RandomInRange);
				}
				TryAttachMote(A, B, vector8);
				mote.Maintain();
			}
			else if (def.fleckDef != null && vector9.ShouldSpawnMotesAt(map, def.fleckDef.drawOffscreen))
			{
				float velocityAngle = (def.fleckUsesAngleForVelocity ? (def.angle.RandomInRange + num) : 0f);
				FleckAttachLink link = FleckAttachLink.Invalid;
				if (def.fleckDef.useAttachLink && base.EffectiveSpawnLocType == MoteSpawnLocType.OnSource && A.IsValid)
				{
					link = new FleckAttachLink(A);
				}
				if (def.fleckDef.useAttachLink && base.EffectiveSpawnLocType == MoteSpawnLocType.OnTarget && B.IsValid)
				{
					link = new FleckAttachLink(B);
				}
				map.flecks.CreateFleck(new FleckCreationData
				{
					def = def.fleckDef,
					scale = def.scale.RandomInRange * num2,
					spawnPosition = vector9,
					rotationRate = def.rotationRate.RandomInRange,
					rotation = def.rotation.RandomInRange + num,
					instanceColor = base.EffectiveColor,
					velocitySpeed = def.speed.RandomInRange,
					velocityAngle = velocityAngle,
					ageTicksOverride = overrideSpawnTick,
					orbitSpeed = (def.orbitOrigin ? def.orbitSpeed.RandomInRange : 0f),
					orbitSnapStrength = def.orbitSnapStrength,
					link = link
				});
			}
		}
	}

	private void TryAttachMote(TargetInfo A, TargetInfo B, Vector3 posOffset)
	{
		if (!def.attachToSpawnThing)
		{
			return;
		}
		if (mote is MoteAttached moteAttached)
		{
			bool updateOffsetToMatchTargetRotation = def.moteDef.mote.updateOffsetToMatchTargetRotation;
			Vector3 offset = (updateOffsetToMatchTargetRotation ? base.EffectiveOffset : posOffset);
			if (base.EffectiveSpawnLocType == MoteSpawnLocType.OnSource && A.HasThing)
			{
				moteAttached.Attach(A, offset, updateOffsetToMatchTargetRotation);
			}
			else if (base.EffectiveSpawnLocType == MoteSpawnLocType.OnTarget && B.HasThing)
			{
				moteAttached.Attach(B, offset, updateOffsetToMatchTargetRotation);
			}
			if (moteAttached.link1.Linked && moteAttached.link1.rotateWithTarget && moteAttached.link1.Target.HasThing)
			{
				moteAttached.link1.UpdateDrawPos();
				moteAttached.Rotation = moteAttached.link1.Target.Thing.Rotation;
			}
		}
		if (mote is MoteDualAttached moteDualAttached)
		{
			bool updateOffsetToMatchTargetRotation2 = def.moteDef.mote.updateOffsetToMatchTargetRotation;
			Vector3 vector = (updateOffsetToMatchTargetRotation2 ? base.EffectiveOffset : posOffset);
			if (A.HasThing && B.HasThing)
			{
				moteDualAttached.Attach(A, B, vector, Vector3.zero);
			}
			else if (base.EffectiveSpawnLocType == MoteSpawnLocType.OnSource && A.HasThing)
			{
				moteDualAttached.Attach(A, vector, updateOffsetToMatchTargetRotation2);
			}
			else if (base.EffectiveSpawnLocType == MoteSpawnLocType.OnTarget && B.HasThing)
			{
				moteDualAttached.Attach(B, vector, updateOffsetToMatchTargetRotation2);
			}
		}
	}

	public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
	{
		if (def.makeMoteOnSubtrigger)
		{
			MakeMote(A, B);
		}
	}

	public override void SubCleanup()
	{
		if (def.destroyMoteOnCleanup)
		{
			mote?.Destroy();
		}
		base.SubCleanup();
	}

	public override void SubEffectTick(TargetInfo A, TargetInfo B)
	{
		base.SubEffectTick(A, B);
		if (mote != null && mote.def.mote.needsMaintenance)
		{
			mote.Maintain();
		}
	}
}
