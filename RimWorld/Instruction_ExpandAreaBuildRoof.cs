using Verse;

namespace RimWorld
{
	public class Instruction_ExpandAreaBuildRoof : Instruction_ExpandArea
	{
		protected override Area MyArea => base.Map.areaManager.BuildRoof;
	}
}
