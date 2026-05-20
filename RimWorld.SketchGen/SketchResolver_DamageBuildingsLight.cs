namespace RimWorld.SketchGen;

public class SketchResolver_DamageBuildingsLight : SketchResolver_DamageBuildings
{
	protected override float MaxPctOfTotalDestroyed => 0.15f;

	protected override float HpRandomFactor => 1.5f;

	protected override float DestroyChanceExp => 2f;
}
