using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class CompGaumakerPod : CompDryadHolder
{
	public bool Full => innerContainer.Count >= 3;

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode != DestroyMode.WillReplace && Find.TickManager.TicksGame < tickComplete)
		{
			innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
		}
	}

	public override void TryAcceptPawn(Pawn p)
	{
		base.TryAcceptPawn(p);
		if (Full)
		{
			tickComplete = Find.TickManager.TicksGame + (int)(60000f * base.Props.daysToComplete);
		}
	}

	protected override void Complete()
	{
		tickComplete = Find.TickManager.TicksGame;
		if (base.TreeComp != null)
		{
			for (int num = innerContainer.Count - 1; num >= 0; num--)
			{
				if (innerContainer[num] is Pawn pawn)
				{
					base.TreeComp.RemoveDryad(pawn);
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
			}
			base.TreeComp.gaumakerPod = null;
			((Plant)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Plant_PodGauranlen), parent.Position, parent.Map)).Growth = 1f;
		}
		parent.Destroy();
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (innerContainer.Count < 3)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text = text + GenLabel.BestKindLabel(PawnKindDefOf.Dryad_Gaumaker, Gender.Male, plural: true).CapitalizeFirst() + ": " + innerContainer.Count + "/" + 3;
		}
		return text;
	}
}
