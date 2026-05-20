using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_PsychicSlaughter : CompAbilityEffect
{
	private static readonly IntRange MeatPieces = new IntRange(3, 4);

	private static readonly IntRange BloodFilth = new IntRange(3, 4);

	public new CompProperties_PsychicSlaughter Props => (CompProperties_PsychicSlaughter)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		Pawn pawn = target.Pawn;
		Map map = pawn.MapHeld;
		IntVec3 positionHeld = pawn.PositionHeld;
		base.Apply(target, dest);
		int num = Mathf.Max(GenMath.RoundRandom(pawn.GetStatValue(StatDefOf.MeatAmount)), 3);
		pawn.Kill(new DamageInfo(DamageDefOf.Psychic, 99999f, 0f, -1f, parent.pawn));
		if (pawn.Faction != null && pawn.HomeFaction != null && pawn.HomeFaction != Faction.OfPlayer && pawn.HomeFaction.def.humanlikeFaction && !pawn.Faction.def.PermanentlyHostileTo(FactionDefOf.PlayerColony))
		{
			pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -100, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.MemberKilled);
		}
		pawn.Corpse?.Destroy();
		EffecterDefOf.MeatExplosion.Spawn(positionHeld, map).Cleanup();
		FleshbeastUtility.MeatSplatter(BloodFilth.RandomInRange, positionHeld, map, FleshbeastUtility.MeatExplosionSize.Large);
		int num2 = MeatPieces.RandomInRange;
		int num3 = Mathf.CeilToInt((float)num / (float)ThingDefOf.Meat_Twisted.stackLimit);
		if (num2 < num3)
		{
			num2 = num3;
		}
		for (int i = 0; i < num2; i++)
		{
			if (RCellFinder.TryFindRandomCellNearWith(positionHeld, (IntVec3 x) => x.Walkable(map), map, out var result, 1, 4))
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Meat_Twisted);
				thing.stackCount = num / num2;
				GenSpawn.Spawn(thing, result, map);
			}
		}
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		return false;
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		return Valid(target);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn == null)
		{
			return false;
		}
		if (!pawn.RaceProps.IsFlesh)
		{
			if (throwMessages)
			{
				Messages.Message("MessageSlaughterNoFlesh".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.RejectInput);
			}
			return false;
		}
		if ((double)pawn.BodySize > 2.5)
		{
			if (throwMessages)
			{
				Messages.Message("MessageSlaughterTooBig".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.RejectInput);
			}
			return false;
		}
		return true;
	}
}
