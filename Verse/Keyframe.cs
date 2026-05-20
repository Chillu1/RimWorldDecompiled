using UnityEngine;

namespace Verse;

public class Keyframe
{
	public int tick;

	public Vector3 offset = Vector3.zero;

	public float angle;

	public Vector3 scale = Vector3.one;

	public GraphicStateDef graphicState;
}
