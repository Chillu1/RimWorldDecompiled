using Verse;

namespace RimWorld
{
	public class Building_TrapExplosive : Building_Trap
	{
		protected override void SpringSub(Pawn p)
		{
			GetComp<CompExplosive>().StartWick();
		}
	}
}
