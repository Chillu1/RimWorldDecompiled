using UnityEngine;

namespace RimWorld;

public struct WornGraphicBodyTypeData
{
	public Vector2 offset;

	public Vector2? scale;

	public Vector2 Scale => scale ?? Vector2.one;
}
