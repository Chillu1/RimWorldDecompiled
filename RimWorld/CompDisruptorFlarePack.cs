using System;
using Verse;

namespace RimWorld;

public class CompDisruptorFlarePack : CompAIUsablePack
{
	public new CompProperties_DisruptorFlarePack Props => (CompProperties_DisruptorFlarePack)props;

	protected override float ChanceToUse(Pawn wearer)
	{
		return 0f;
	}

	protected override void UsePack(Pawn wearer)
	{
		throw new NotImplementedException();
	}
}
