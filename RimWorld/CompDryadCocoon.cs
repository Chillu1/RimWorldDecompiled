using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompDryadCocoon : CompDryadHolder
{
	private int tickExpire = -1;

	private PawnKindDef dryadKind;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad)
		{
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
			tickExpire = Find.TickManager.TicksGame + 600;
		}
	}

	public override void TryAcceptPawn(Pawn p)
	{
		base.TryAcceptPawn(p);
		p.Rotation = Rot4.South;
		tickComplete = Find.TickManager.TicksGame + (int)(60000f * base.Props.daysToComplete);
		tickExpire = -1;
		dryadKind = base.TreeComp.DryadKind;
	}

	protected override void Complete()
	{
		tickComplete = Find.TickManager.TicksGame;
		CompTreeConnection treeComp = base.TreeComp;
		if (treeComp != null && innerContainer.Count > 0)
		{
			Pawn pawn = (Pawn)innerContainer[0];
			long ageBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
			treeComp.RemoveDryad(pawn);
			Pawn pawn2 = treeComp.GenerateNewDryad(dryadKind);
			pawn2.ageTracker.AgeBiologicalTicks = ageBiologicalTicks;
			if (!pawn.Name.Numerical)
			{
				pawn2.Name = pawn.Name;
			}
			pawn.Destroy();
			innerContainer.TryAddOrTransfer(pawn2, 1);
			EffecterDefOf.DryadEmergeFromCocoon.Spawn(parent.Position, parent.Map).Cleanup();
		}
		parent.Destroy();
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode != DestroyMode.WillReplace)
		{
			innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near, delegate(Thing t, int c)
			{
				t.Rotation = Rot4.South;
				SoundDefOf.Pawn_Dryad_Spawn.PlayOneShot(parent);
			}, null, playDropSound: false);
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!parent.Destroyed)
		{
			if (dryadKind != null && dryadKind != base.TreeComp.DryadKind)
			{
				parent.Destroy();
			}
			else if (innerContainer.Count > 0 && tree != null && base.TreeComp.ShouldReturnToTree((Pawn)innerContainer[0]))
			{
				parent.Destroy();
			}
			else if (tickExpire >= 0 && Find.TickManager.TicksGame >= tickExpire)
			{
				tickExpire = -1;
				parent.Destroy();
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (!innerContainer.NullOrEmpty() && dryadKind != null)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += "ChangingDryadIntoType".Translate(innerContainer[0].Named("DRYAD"), NamedArgumentUtility.Named(dryadKind, "TYPE")).Resolve();
		}
		return text;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref tickExpire, "tickExpire", -1);
		Scribe_Defs.Look(ref dryadKind, "dryadKind");
	}
}
