using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class AnimationWorker_Keyframes : BaseAnimationWorker
{
	private static readonly Func<Vector3, Vector3, float, Vector3> cachedLerpVector3 = Vector3.Lerp;

	private static readonly Func<float, float, float, float> cachedLerpMathf = Mathf.Lerp;

	private readonly Func<Keyframe, Vector3> cachedGetKeyframeOffset = GetKeyframeOffset;

	private readonly Func<Keyframe, float> cachedGetKeyframeAngle = GetKeyframeAngle;

	private readonly Func<Keyframe, Vector3> cachedGetKeyframeScale = GetKeyframeScale;

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
		Pawn pawn = parms.pawn;
		Vector3 keyframeData = GetKeyframeData(tick, part, cachedGetKeyframeOffset, cachedLerpVector3);
		return AnimationUtility.AdjustOffsetForRotationMode(part.rotationMode, keyframeData, pawn);
	}

	public override float AngleAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
	{
		return GetKeyframeData(tick, part, cachedGetKeyframeAngle, cachedLerpMathf);
	}

	public override Vector3 ScaleAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
	{
		return GetKeyframeData(tick, part, cachedGetKeyframeScale, cachedLerpVector3);
	}

	public override GraphicStateDef GraphicStateAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
	{
		return GetKeyframeForTick(tick, part).graphicState;
	}

	protected static T GetKeyframeData<T>(int tick, AnimationPart p, Func<Keyframe, T> func, Func<T, T, float, T> lerpFunc) where T : struct
	{
		KeyframeAnimationPart keyframeAnimationPart = (KeyframeAnimationPart)p;
		if (tick <= keyframeAnimationPart.keyframes[0].tick)
		{
			return func(keyframeAnimationPart.keyframes[0]);
		}
		List<Keyframe> keyframes = keyframeAnimationPart.keyframes;
		if (tick >= keyframes[keyframes.Count - 1].tick)
		{
			List<Keyframe> keyframes2 = keyframeAnimationPart.keyframes;
			return func(keyframes2[keyframes2.Count - 1]);
		}
		Keyframe keyframe = keyframeAnimationPart.keyframes[0];
		List<Keyframe> keyframes3 = keyframeAnimationPart.keyframes;
		Keyframe keyframe2 = keyframes3[keyframes3.Count - 1];
		for (int i = 0; i < keyframeAnimationPart.keyframes.Count; i++)
		{
			if (tick <= keyframeAnimationPart.keyframes[i].tick)
			{
				keyframe2 = keyframeAnimationPart.keyframes[i];
				if (i > 0)
				{
					keyframe = keyframeAnimationPart.keyframes[i - 1];
				}
				break;
			}
		}
		float arg = (float)(tick - keyframe.tick) / (float)(keyframe2.tick - keyframe.tick);
		return lerpFunc(func(keyframe), func(keyframe2), arg);
	}

	protected static Keyframe GetKeyframeForTick(int tick, AnimationPart p)
	{
		KeyframeAnimationPart keyframeAnimationPart = (KeyframeAnimationPart)p;
		if (tick <= keyframeAnimationPart.keyframes[0].tick)
		{
			return keyframeAnimationPart.keyframes[0];
		}
		List<Keyframe> keyframes = keyframeAnimationPart.keyframes;
		if (tick >= keyframes[keyframes.Count - 1].tick)
		{
			List<Keyframe> keyframes2 = keyframeAnimationPart.keyframes;
			return keyframes2[keyframes2.Count - 1];
		}
		for (int i = 0; i < keyframeAnimationPart.keyframes.Count; i++)
		{
			if (tick <= keyframeAnimationPart.keyframes[i].tick)
			{
				return keyframeAnimationPart.keyframes[i];
			}
		}
		Log.Error($"Attempted to get a keyframe for a tick ({tick}) which wasn't handled");
		return null;
	}

	private static Vector3 GetKeyframeScale(Keyframe keyframe)
	{
		return keyframe.scale;
	}

	private static Vector3 GetKeyframeOffset(Keyframe keyframe)
	{
		return keyframe.offset;
	}

	private static float GetKeyframeAngle(Keyframe keyframe)
	{
		return keyframe.angle;
	}
}
