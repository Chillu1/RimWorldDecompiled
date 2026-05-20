using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_Transmute : CompAbilityEffect
{
	public new CompProperties_Transmute Props => (CompProperties_Transmute)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Thing thing = target.Thing;
		if (thing.Stuff != null)
		{
			string text = Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.LabelShort);
			string text2 = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount));
			ThingDef stuff = thing.Stuff;
			ThingDef thingDef;
			using (new RandBlock(thing.thingIDNumber))
			{
				thingDef = Props.outcomeStuff.RandomElement();
			}
			float num = (float)thing.HitPoints / (float)thing.MaxHitPoints;
			thing.SetStuffDirect(thingDef);
			StatDefOf.MaxHitPoints.Worker.ClearCacheForThing(thing);
			thing.Notify_ColorChanged();
			thing.DirtyMapMesh(thing.Map);
			thing.HitPoints = Mathf.CeilToInt((float)thing.MaxHitPoints * num);
			if (thing is UnfinishedThing unfinishedThing)
			{
				int num2 = 0;
				int num3 = 100;
				foreach (Thing ingredient in unfinishedThing.ingredients)
				{
					num2 += ingredient.stackCount;
				}
				unfinishedThing.ingredients.Clear();
				while (num2 > 0 && num3-- > 0)
				{
					Thing thing2 = ThingMaker.MakeThing(thingDef);
					thing2.stackCount = Mathf.Min(num2, thingDef.stackLimit);
					unfinishedThing.ingredients.Add(thing2);
					num2 -= thing2.stackCount;
				}
				if (num3 <= 0)
				{
					Debug.LogError("Attempted to transmute an unfinished thing but could not generate enough replacement items (" + stuff.label + " to " + thingDef.label + "). Check if " + thingDef.label + " is stackable.");
				}
			}
			string text3 = "";
			text3 = ((target.Thing.stackCount <= 1) ? ((string)"MessageTransmutedStuff".Translate(parent.pawn.Named("PAWN"), text, thing.Named("TRANSMUTED"))) : ((string)"MessageTransmutedStuffPlural".Translate(parent.pawn.Named("PAWN"), text2, thing.Named("TRANSMUTED"))));
			Messages.Message(text3, parent.pawn, MessageTypeDefOf.NeutralEvent);
		}
		else
		{
			TryGetElementFor(thing.def, out var ratio);
			int stackCount = thing.stackCount;
			int num4 = Mathf.Max(Mathf.FloorToInt((float)thing.stackCount * ratio.ratio), 1);
			Thing thing3 = ThingMaker.MakeThing(Props.outcomeItems.RandomElement());
			thing3.stackCount = num4;
			IntVec3 positionHeld = thing.PositionHeld;
			thing.Destroy();
			GenPlace.TryPlaceThing(thing3, positionHeld, parent.pawn.Map, ThingPlaceMode.Direct);
			string arg = $"{stackCount} {target.Thing.LabelNoCount}";
			string arg2 = $"{num4} {thing3.LabelNoCount}";
			Messages.Message("MessageTransmutedItem".Translate(parent.pawn.Named("PAWN"), arg.Named("ORIGINAL"), arg2.Named("TRANSMUTED")), parent.pawn, MessageTypeDefOf.NeutralEvent);
		}
	}

	public override bool CanApplyOn(GlobalTargetInfo target)
	{
		CompProperties_Transmute.ElementRatio ratio;
		if (target.HasThing)
		{
			return TryGetElementFor(target.Thing.Stuff ?? target.Thing.def, out ratio);
		}
		return false;
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		int num;
		if (target.HasThing)
		{
			num = (TryGetElementFor(target.Thing.Stuff ?? target.Thing.def, out var _) ? 1 : 0);
			if (num != 0)
			{
				goto IL_0070;
			}
		}
		else
		{
			num = 0;
		}
		Messages.Message(Props.failedMessage.Formatted(target.Thing.Label), target.Thing, MessageTypeDefOf.NeutralEvent);
		goto IL_0070;
		IL_0070:
		return (byte)num != 0;
	}

	private bool TryGetElementFor(ThingDef stuff, out CompProperties_Transmute.ElementRatio ratio)
	{
		ratio = Props.elementRatios.FirstOrDefault((CompProperties_Transmute.ElementRatio x) => x.sourceStuff == stuff);
		return ratio != null;
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		return false;
	}
}
