using Verse;

namespace RimWorld
{
	public class ThoughtWorker_DeathrestChamber : ThoughtWorker
	{
		private const int DullIndex = 2;

		private const int MediocreIndex = 3;

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			Gene_Deathrest gene_Deathrest = p.genes?.GetFirstGeneOfType<Gene_Deathrest>();
			if (gene_Deathrest == null || gene_Deathrest.chamberThoughtIndex < 0)
			{
				return false;
			}
			if (p.Deathresting)
			{
				return false;
			}
			int chamberThoughtIndex = gene_Deathrest.chamberThoughtIndex;
			if (chamberThoughtIndex == 2 || chamberThoughtIndex == 3)
			{
				return false;
			}
			return ThoughtState.ActiveAtStage(chamberThoughtIndex);
		}
	}
}
