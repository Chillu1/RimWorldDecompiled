using Verse;

namespace RimWorld
{
	public class Instruction_ExpandAreaHome : Instruction_ExpandArea
	{
		protected override Area MyArea => base.Map.areaManager.Home;
	}
}
