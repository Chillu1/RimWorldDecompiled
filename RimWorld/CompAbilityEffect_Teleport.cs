using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompAbilityEffect_Teleport : CompAbilityEffect_WithDest
	{
		public new CompProperties_AbilityTeleport Props => (CompProperties_AbilityTeleport)props;

		public override IEnumerable<PreCastAction> GetPreCastActions()
		{
			yield return new PreCastAction
			{
				action = delegate(LocalTargetInfo t, LocalTargetInfo d)
				{
					if (!parent.def.HasAreaOfEffect)
					{
						Pawn pawn = t.Pawn;
						if (pawn != null)
						{
							MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_PsycastSkipFlashEntry, Vector3.zero).detachAfterTicks = 5;
						}
						else
						{
							MoteMaker.MakeStaticMote(t.CenterVector3, parent.pawn.Map, ThingDefOf.Mote_PsycastSkipFlashEntry);
						}
						MoteMaker.MakeStaticMote(d.Cell, parent.pawn.Map, ThingDefOf.Mote_PsycastSkipInnerExit);
					}
					if (Props.destination != AbilityEffectDestination.RandomInRange)
					{
						MoteMaker.MakeStaticMote(d.Cell, parent.pawn.Map, ThingDefOf.Mote_PsycastSkipOuterRingExit);
					}
					if (!parent.def.HasAreaOfEffect)
					{
						SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(t.Cell, parent.pawn.Map));
						SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(d.Cell, parent.pawn.Map));
					}
				},
				ticksAwayFromCast = 5
			};
		}

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
				if (!parent.def.HasAreaOfEffect)
				{
					parent.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(target.Thing, pawn.Map), target.Thing.Position, 60);
				}
				else
				{
					parent.AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(target.Thing, pawn.Map), target.Thing.Position, 60);
				}
				if (Props.destination == AbilityEffectDestination.Selected)
				{
					parent.AddEffecterToMaintain(EffecterDefOf.Skip_Exit.Spawn(destination.Cell, pawn.Map), destination.Cell, 60);
				}
				else
				{
					parent.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(destination.Cell, pawn.Map), destination.Cell, 60);
				}
				target.Thing.TryGetComp<CompCanBeDormant>()?.WakeUp();
				target.Thing.Position = destination.Cell;
				Pawn pawn2 = target.Thing as Pawn;
				if (pawn2 != null)
				{
					pawn2.stances.stunner.StunFor_NewTmp(Props.stunTicks.RandomInRange, parent.pawn, addBattleLog: false, showMote: false);
					pawn2.Notify_Teleported();
				}
				if (Props.destClamorType != null)
				{
					GenClamor.DoClamor(pawn, target.Cell, Props.destClamorRadius, Props.destClamorType);
				}
			}
		}

		public override bool CanHitTarget(LocalTargetInfo target)
		{
			if (!CanPlaceSelectedTargetAt(target))
			{
				return false;
			}
			return base.CanHitTarget(target);
		}
	}
}
