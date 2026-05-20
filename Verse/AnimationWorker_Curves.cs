using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class AnimationWorker_Curves : BaseAnimationWorker
{
	public override bool Enabled(AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
	{
		if (!def.playWhenDowned)
		{
			return !parms.pawn.Downed;
		}
		return true;
	}

	public override void PostDraw(AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms, Matrix4x4 matrix)
	{
	}

	public override Vector3 OffsetAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
	{
		CurveKeyAnimationPart obj = (CurveKeyAnimationPart)part;
		Pawn pawn = parms.pawn;
		float time = tick;
		if (obj.normalized)
		{
			time = Mathf.InverseLerp(0f, def.durationTicks, tick);
		}
		Vector3 offset = obj.offset?.Evaluate(time) ?? Vector3.zero;
		return AnimationUtility.AdjustOffsetForRotationMode(part.rotationMode, offset, pawn);
	}

	public override float AngleAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
	{
		CurveKeyAnimationPart obj = (CurveKeyAnimationPart)part;
		float time = tick;
		if (obj.normalized)
		{
			time = Mathf.InverseLerp(0f, def.durationTicks, tick);
		}
		return obj.angle?.Evaluate(time) ?? 0f;
	}

	public override Vector3 ScaleAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
	{
		CurveKeyAnimationPart obj = (CurveKeyAnimationPart)part;
		float time = tick;
		if (obj.normalized)
		{
			time = Mathf.InverseLerp(0f, def.durationTicks, tick);
		}
		return obj.scale?.Evaluate(time, 1f) ?? Vector3.one;
	}

	public override GraphicStateDef GraphicStateAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
	{
		return GetKeyframeForTick(tick, part).graphicState;
	}

	protected static CurveKey GetKeyframeForTick(int tick, AnimationPart p)
	{
		CurveKeyAnimationPart curveKeyAnimationPart = (CurveKeyAnimationPart)p;
		if (tick <= curveKeyAnimationPart.keyframes[0].tick)
		{
			return curveKeyAnimationPart.keyframes[0];
		}
		List<CurveKey> keyframes = curveKeyAnimationPart.keyframes;
		if (tick >= keyframes[keyframes.Count - 1].tick)
		{
			List<CurveKey> keyframes2 = curveKeyAnimationPart.keyframes;
			return keyframes2[keyframes2.Count - 1];
		}
		for (int i = 0; i < curveKeyAnimationPart.keyframes.Count; i++)
		{
			if (tick <= curveKeyAnimationPart.keyframes[i].tick)
			{
				return curveKeyAnimationPart.keyframes[i];
			}
		}
		Log.Error($"Attempted to get a curve keyframe for a tick ({tick}) which wasn't handled");
		return null;
	}
}
