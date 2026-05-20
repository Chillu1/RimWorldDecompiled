namespace RimWorld;

public class CompProperties_Targetable_SingleAnimal : CompProperties_Targetable
{
	public bool allowWildMan = true;

	public CompProperties_Targetable_SingleAnimal()
	{
		compClass = typeof(CompTargetable_SingleAnimal);
	}
}
