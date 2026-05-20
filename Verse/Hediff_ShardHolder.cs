using RimWorld;

namespace Verse;

public class Hediff_ShardHolder : HediffWithComps
{
	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Shard holder"))
		{
			pawn.health.RemoveHediff(this);
		}
		else
		{
			base.PostAdd(dinfo);
		}
	}

	public override void Notify_PawnKilled()
	{
		if (pawn.SpawnedOrAnyParentSpawned && GenDrop.TryDropSpawn(ThingMaker.MakeThing(ThingDefOf.Shard), pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near, out var resultingThing))
		{
			resultingThing.SetForbidden(!resultingThing.MapHeld.areaManager.Home[resultingThing.PositionHeld]);
			string text = pawn.LabelShort;
			if (pawn.IsMutant)
			{
				text = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.mutant.Def.label);
			}
			else if (pawn.Name == null)
			{
				text = Find.ActiveLanguageWorker.WithDefiniteArticle(text);
			}
			Messages.Message("MessageShardDropped".Translate(text).CapitalizeFirst(), resultingThing, MessageTypeDefOf.NeutralEvent);
		}
	}
}
