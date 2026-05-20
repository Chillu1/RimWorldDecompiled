namespace RimWorld;

public class Building_KidOutfitStand : Building_OutfitStand
{
	protected override BodyTypeDef BodyTypeDefForRendering { get; } = BodyTypeDefOf.Child;

	protected override float WeaponDrawDistanceFactor => 0.55f;
}
