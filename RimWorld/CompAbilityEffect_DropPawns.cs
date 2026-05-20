using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_DropPawns : CompAbilityEffect_WithDest
	{
		public new CompProperties_DropPawns Props => (CompProperties_DropPawns)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < Props.amount; i++)
			{
				Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Props.pawnKindDef));
				list.Add(item);
			}
			DropPodUtility.DropThingsNear(target.Cell, parent.pawn.Map, list);
		}
	}
}
