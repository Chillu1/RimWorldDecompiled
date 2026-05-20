using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompAbilityEffect_Teleport : CompAbilityEffect_WithDest
{
	public static string SkipUsedSignalTag = "CompAbilityEffect.SkipUsed";

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
						FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(pawn, FleckDefOf.PsycastSkipFlashEntry, new Vector3(-0.5f, 0f, -0.5f));
						dataAttachedOverlay.link.detachAfterTicks = 5;
						pawn.Map.flecks.CreateFleck(dataAttachedOverlay);
					}
					else
					{
						FleckMaker.Static(t.CenterVector3, parent.pawn.Map, FleckDefOf.PsycastSkipFlashEntry);
					}
					FleckMaker.Static(d.Cell, parent.pawn.Map, FleckDefOf.PsycastSkipInnerExit);
				}
				if (Props.destination != AbilityEffectDestination.RandomInRange)
				{
					FleckMaker.Static(d.Cell, parent.pawn.Map, FleckDefOf.PsycastSkipOuterRingExit);
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
		if (!destination.IsValid)
		{
			return;
		}
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
		if (target.Thing is Pawn pawn2)
		{
			if ((pawn2.Faction == Faction.OfPlayer || pawn2.IsPlayerControlled) && pawn2.Position.Fogged(pawn2.Map))
			{
				FloodFillerFog.FloodUnfog(pawn2.Position, pawn2.Map);
			}
			pawn2.stances.stunner.StunFor(Props.stunTicks.RandomInRange, parent.pawn, addBattleLog: false, showMote: false);
			pawn2.Notify_Teleported();
			SendSkipUsedSignal(pawn2.Position, pawn2);
		}
		if (Props.destClamorType != null)
		{
			GenClamor.DoClamor(pawn, target.Cell, Props.destClamorRadius, Props.destClamorType);
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

	public override bool Valid(LocalTargetInfo target, bool showMessages = true)
	{
		AcceptanceReport acceptanceReport = CanSkipTarget(target);
		if (!acceptanceReport)
		{
			if (showMessages && !acceptanceReport.Reason.NullOrEmpty() && target.Thing is Pawn pawn)
			{
				Messages.Message("CannotSkipTarget".Translate(pawn.Named("PAWN")) + ": " + acceptanceReport.Reason, pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return base.Valid(target, showMessages);
	}

	private AcceptanceReport CanSkipTarget(LocalTargetInfo target)
	{
		if (target.Thing is Pawn pawn)
		{
			if (pawn.BodySize > Props.maxBodySize)
			{
				return "CannotSkipTargetTooLarge".Translate();
			}
			if (pawn.kindDef.skipResistant)
			{
				return "CannotSkipTargetPsychicResistant".Translate();
			}
		}
		return true;
	}

	public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
	{
		return CanSkipTarget(target).Reason;
	}

	public static void SendSkipUsedSignal(LocalTargetInfo target, Thing initiator)
	{
		Find.SignalManager.SendSignal(new Signal(SkipUsedSignalTag, target.Named("POSITION"), initiator.Named("SUBJECT")));
	}
}
