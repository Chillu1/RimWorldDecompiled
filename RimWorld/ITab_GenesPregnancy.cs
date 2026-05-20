using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_GenesPregnancy : ITab_Genes
{
	public override bool IsVisible => true;

	private GeneSet UnbornBabyHediffGeneset()
	{
		Pawn selPawnForGenes = base.SelPawnForGenes;
		if (selPawnForGenes != null)
		{
			foreach (Hediff hediff in selPawnForGenes.health.hediffSet.hediffs)
			{
				if (hediff is HediffWithParents hediffWithParents)
				{
					return hediffWithParents.geneSet;
				}
			}
		}
		return null;
	}

	protected override void FillTab()
	{
		GeneUIUtility.DrawGenesInfo(new Rect(0f, 20f, size.x, size.y - 20f), null, 550f, ref size, ref scrollPosition, UnbornBabyHediffGeneset());
	}
}
