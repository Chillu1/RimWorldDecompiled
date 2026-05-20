using Verse;

namespace RimWorld;

[DefOf]
public static class BodyPartDefOf
{
	public static BodyPartDef Leg;

	public static BodyPartDef Eye;

	public static BodyPartDef Shoulder;

	public static BodyPartDef Arm;

	public static BodyPartDef Hand;

	public static BodyPartDef Head;

	public static BodyPartDef Lung;

	public static BodyPartDef Torso;

	public static BodyPartDef Heart;

	public static BodyPartDef Neck;

	static BodyPartDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartDefOf));
	}
}
