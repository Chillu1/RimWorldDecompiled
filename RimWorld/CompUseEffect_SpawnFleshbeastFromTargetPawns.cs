using System.Linq;
using Verse;

namespace RimWorld;

public class CompUseEffect_SpawnFleshbeastFromTargetPawns : CompUseEffect
{
	private CompTargetable targetable;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		targetable = parent.GetComp<CompTargetable>();
	}

	public override void DoEffect(Pawn usedBy)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return;
		}
		if (targetable == null)
		{
			Log.Error("CompUseEffect_SpawnFleshbeastFromTargetPawns requires a CompTargetable");
			return;
		}
		Thing[] array = targetable.GetTargets().ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is Pawn pawn)
			{
				FleshbeastUtility.SpawnFleshbeastFromPawn(pawn, false, false);
			}
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		if (Scribe.mode == LoadSaveMode.PostLoadInit && targetable == null)
		{
			targetable = parent.GetComp<CompTargetable>();
		}
	}
}
