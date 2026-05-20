using UnityEngine;

namespace LudeonTK;

public static class KeyFrameExtensions
{
	public static Vector2 ToV2(this Keyframe kf)
	{
		return new Vector2(kf.time, kf.value);
	}

	public static Vector4 ToVector4(this Keyframe kf)
	{
		return new Vector4(kf.time, kf.value, kf.inTangent, kf.outTangent);
	}

	public static Keyframe ToKeyframe(this Vector4 kf)
	{
		return new Keyframe(kf.x, kf.y, kf.z, kf.w);
	}

	public static Keyframe ToKeyframe(this Vector2 v2)
	{
		return new Keyframe(v2.x, v2.y);
	}
}
