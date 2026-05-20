using Verse;

namespace RimWorld;

[DefOf]
public static class DesignationDefOf
{
	public static DesignationDef Haul;

	public static DesignationDef Mine;

	public static DesignationDef MineVein;

	public static DesignationDef Deconstruct;

	public static DesignationDef Uninstall;

	public static DesignationDef CutPlant;

	public static DesignationDef HarvestPlant;

	public static DesignationDef Hunt;

	public static DesignationDef SmoothFloor;

	public static DesignationDef RemoveFloor;

	public static DesignationDef RemoveFoundation;

	public static DesignationDef SmoothWall;

	public static DesignationDef Flick;

	public static DesignationDef Plan;

	public static DesignationDef Strip;

	public static DesignationDef Slaughter;

	public static DesignationDef Tame;

	public static DesignationDef Open;

	public static DesignationDef EjectFuel;

	public static DesignationDef ReleaseAnimalToWild;

	public static DesignationDef ExtractTree;

	public static DesignationDef PaintBuilding;

	public static DesignationDef PaintFloor;

	public static DesignationDef RemovePaintBuilding;

	public static DesignationDef RemovePaintFloor;

	public static DesignationDef ExtractSkull;

	public static DesignationDef FillIn;

	static DesignationDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(DesignationDefOf));
	}
}
