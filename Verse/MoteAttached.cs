using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class MoteAttached : Mote
{
	private static readonly List<Vector3> animalHeadOffsets = new List<Vector3>
	{
		new Vector3(0f, 0f, 0.4f),
		new Vector3(0.4f, 0f, 0.25f),
		new Vector3(0f, 0f, 0.1f),
		new Vector3(-0.4f, 0f, 0.25f)
	};

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		exactPosition += def.mote.attachedDrawOffset;
	}

	protected override void TimeInterval(float deltaTime)
	{
		base.TimeInterval(deltaTime);
		if (!link1.Linked)
		{
			return;
		}
		bool flag = detachAfterTicks == -1 || Find.TickManager.TicksGame - spawnTick < detachAfterTicks;
		if (!link1.Target.ThingDestroyed && flag)
		{
			link1.UpdateDrawPos();
			if (link1.rotateWithTarget)
			{
				base.Rotation = link1.Target.Thing.Rotation;
			}
		}
		Vector3 vector = def.mote.attachedDrawOffset;
		if (def.mote.attachedToHead && link1.Target.Thing is Pawn pawn)
		{
			bool humanlike = pawn.RaceProps.Humanlike;
			List<Vector3> headPosPerRotation = pawn.RaceProps.headPosPerRotation;
			Rot4 rotation = ((pawn.GetPosture() != PawnPosture.Standing) ? pawn.Drawer.renderer.LayingFacing() : (humanlike ? Rot4.North : pawn.Rotation));
			if (humanlike)
			{
				vector = pawn.Drawer.renderer.BaseHeadOffsetAt(rotation).RotatedBy(pawn.Drawer.renderer.BodyAngle(PawnRenderFlags.None));
			}
			else
			{
				float bodySizeFactor = pawn.ageTracker.CurLifeStage.bodySizeFactor;
				Vector2 vector2 = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize * bodySizeFactor;
				vector = ((!headPosPerRotation.NullOrEmpty()) ? headPosPerRotation[rotation.AsInt].ScaledBy(new Vector3(vector2.x, 1f, vector2.y)) : (animalHeadOffsets[rotation.AsInt] * pawn.BodySize));
			}
		}
		exactPosition = link1.LastDrawPos + vector;
		IntVec3 intVec = exactPosition.ToIntVec3();
		if (base.Spawned && !intVec.InBounds(base.Map))
		{
			Destroy();
		}
		else
		{
			base.Position = intVec;
		}
	}
}
