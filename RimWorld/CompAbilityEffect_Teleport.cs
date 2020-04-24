using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_Teleport : CompAbilityEffect_WithDest
	{
		public new CompProperties_AbilityTeleport Props => (CompProperties_AbilityTeleport)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (!target.HasThing)
			{
				return;
			}
			base.Apply(target, dest);
			LocalTargetInfo destination = GetDestination(dest.IsValid ? dest : target);
			if (destination.IsValid)
			{
				Pawn pawn = parent.pawn;
				Vector3 drawPos = target.Thing.DrawPos;
				target.Thing.Position = destination.Cell;
				Pawn pawn2 = target.Thing as Pawn;
				if (pawn2 != null)
				{
					pawn2.stances.stunner.StunFor(Props.stunTicks.RandomInRange, parent.pawn, addBattleLog: false);
					pawn2.Notify_Teleported();
				}
				if (Props.destClamorType != null)
				{
					GenClamor.DoClamor(pawn, target.Cell, Props.destClamorRadius, Props.destClamorType);
				}
				MoteMaker.MakeConnectingLine(drawPos, target.Thing.DrawPos, ThingDefOf.Mote_PsycastSkipLine, pawn.Map);
				MoteMaker.MakeStaticMote(drawPos, pawn.Map, ThingDefOf.Mote_PsycastSkipEffectSource);
			}
		}
	}
}
