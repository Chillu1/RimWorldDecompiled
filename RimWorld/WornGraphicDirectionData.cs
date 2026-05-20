using UnityEngine;

namespace RimWorld;

public struct WornGraphicDirectionData
{
	public Vector2 offset;

	public Vector2? scale;

	public WornGraphicBodyTypeData male;

	public WornGraphicBodyTypeData female;

	public WornGraphicBodyTypeData thin;

	public WornGraphicBodyTypeData hulk;

	public WornGraphicBodyTypeData fat;

	public Vector2 Scale => scale ?? Vector2.one;
}
