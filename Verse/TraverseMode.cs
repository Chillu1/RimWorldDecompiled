namespace Verse
{
	public enum TraverseMode : byte
	{
		ByPawn,
		PassDoors,
		NoPassClosedDoors,
		PassAllDestroyableThings,
		NoPassClosedDoorsOrWater,
		PassAllDestroyableThingsNotWater
	}
}
