using Verse;

namespace RimWorld;

public abstract class ScenPart_ConfigPage_ConfigureStartingPawnsBase : ScenPart_ConfigPage
{
	public int pawnChoiceCount = 10;

	protected const int MaxPawnCount = 10;

	protected abstract int TotalPawnCount { get; }

	protected abstract void GenerateStartingPawns();

	public override void PostIdeoChosen()
	{
		Find.GameInitData.startingPawnCount = TotalPawnCount;
		if (ModsConfig.BiotechActive)
		{
			Current.Game.customXenotypeDatabase.customXenotypes.Clear();
			foreach (Ideo item in Find.IdeoManager.IdeosListForReading)
			{
				foreach (Precept item2 in item.PreceptsListForReading)
				{
					if (item2 is Precept_Xenotype { customXenotype: not null } precept_Xenotype && !Current.Game.customXenotypeDatabase.customXenotypes.Contains(precept_Xenotype.customXenotype))
					{
						Current.Game.customXenotypeDatabase.customXenotypes.Add(precept_Xenotype.customXenotype);
					}
				}
			}
		}
		if (ModsConfig.IdeologyActive && Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo != null)
		{
			foreach (Precept item3 in Faction.OfPlayerSilentFail.ideos.PrimaryIdeo.PreceptsListForReading)
			{
				if (item3.def.defaultDrugPolicyOverride != null)
				{
					Current.Game.drugPolicyDatabase.MakePolicyDefault(item3.def.defaultDrugPolicyOverride);
				}
			}
		}
		GenerateStartingPawns();
		while (Find.GameInitData.startingAndOptionalPawns.Count < pawnChoiceCount)
		{
			StartingPawnUtility.AddNewPawn();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref pawnChoiceCount, "pawnChoiceCount", 0);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ pawnChoiceCount;
	}
}
