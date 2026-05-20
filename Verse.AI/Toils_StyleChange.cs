using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse.AI;

public static class Toils_StyleChange
{
	private const float ChanceToChangeHairColor = 0.05f;

	public static Toil SetupLookChangeData()
	{
		Toil toil = ToilMaker.MakeToil("SetupLookChangeData");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Color? hairColor = null;
			if (Rand.Value < 0.05f)
			{
				hairColor = PawnHairColors.RandomHairColor(actor, actor.story.SkinColor, 20);
			}
			actor.style.SetupNextLookChangeData(GetRandomStyleItemWeighted(actor, actor.story.hairDef), GetRandomStyleItemWeighted(actor, actor.style.beardDef), GetRandomStyleItemWeighted(actor, actor.style.FaceTattoo, TattooType.Face), GetRandomStyleItemWeighted(actor, actor.style.BodyTattoo, TattooType.Body), hairColor);
		};
		return toil;
	}

	public static Toil DoLookChange(TargetIndex StationIndex, Pawn pawn)
	{
		return Toils_General.WaitWith(StationIndex, 300, useProgressBar: true, maintainPosture: false, maintainSleep: false, StationIndex).FailOnDespawnedOrNull(StationIndex).PlaySustainerOrSound(SoundDefOf.HairCutting)
			.WithEffect(EffecterDefOf.HairCutting, StationIndex, pawn.story.HairColor);
	}

	public static Toil FinalizeLookChange()
	{
		Toil toil = ToilMaker.MakeToil("FinalizeLookChange");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			bool flag = false;
			if (actor.style.nextHairDef != null && actor.style.nextHairDef != actor.story.hairDef)
			{
				flag = true;
				actor.story.hairDef = actor.style.nextHairDef;
			}
			if (actor.style.CanWantBeard && actor.style.beardDef != null && actor.style.nextBeardDef != actor.style.beardDef)
			{
				flag = true;
				actor.style.beardDef = actor.style.nextBeardDef;
			}
			if (actor.style.nextFaceTattooDef != null)
			{
				actor.style.FaceTattoo = actor.style.nextFaceTattooDef;
			}
			if (actor.style.nextBodyTatooDef != null)
			{
				actor.style.BodyTattoo = actor.style.nextBodyTatooDef;
			}
			actor.style.Notify_StyleItemChanged();
			actor.style.ResetNextStyleChangeAttemptTick();
			if (flag)
			{
				actor.style.MakeHairFilth();
			}
		};
		return toil;
	}

	private static T GetRandomStyleItemWeighted<T>(Pawn pawn, T current, TattooType? tattooType = null) where T : StyleItemDef
	{
		if (DefDatabase<T>.AllDefs.Where((T x) => PawnStyleItemChooser.WantsToUseStyle(pawn, x, tattooType)).TryRandomElementByWeight((T x) => PawnStyleItemChooser.TotalStyleItemLikelihood(x, pawn) * ((x == current) ? 0.05f : 1f), out var result))
		{
			return result;
		}
		return null;
	}
}
