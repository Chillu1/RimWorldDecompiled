using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public class GameInitData
{
	public PlanetTile startingTile = PlanetTile.Invalid;

	public MapGeneratorDef mapGeneratorDef;

	public int mapSize = 250;

	public List<Pawn> startingAndOptionalPawns = new List<Pawn>();

	public Dictionary<Pawn, List<ThingDefCount>> startingPossessions = new Dictionary<Pawn, List<ThingDefCount>>();

	public int startingPawnCount = -1;

	public Faction playerFaction;

	public Season startingSeason;

	public bool permadeathChosen;

	public bool permadeath;

	public PawnKindDef startingPawnKind;

	public List<PawnKindCount> startingPawnsRequired;

	public List<XenotypeCount> startingXenotypesRequired;

	public List<MutantCount> startingMutantsRequired;

	public DevelopmentalStage allowedDevelopmentalStages = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult;

	public List<SkillDef> startingSkillsRequired;

	public bool startedFromEntry;

	public string gameToLoad;

	public const int DefaultMapSize = 250;

	public bool QuickStarted
	{
		get
		{
			if (gameToLoad.NullOrEmpty())
			{
				return !startedFromEntry;
			}
			return false;
		}
	}

	public void ChooseRandomStartingTile()
	{
		startingTile = TileFinder.RandomStartingTile();
	}

	public void ResetWorldRelatedMapInitData()
	{
		Current.Game.World = null;
		startingAndOptionalPawns.Clear();
		playerFaction = null;
		startingTile = PlanetTile.Invalid;
	}

	public override string ToString()
	{
		return $"startedFromEntry: {startedFromEntry}\nstartingAndOptionalPawns: {startingAndOptionalPawns.Count}";
	}

	public void PrepForMapGen()
	{
		while (startingAndOptionalPawns.Count > startingPawnCount)
		{
			PawnComponentsUtility.RemoveComponentsOnDespawned(startingAndOptionalPawns[startingPawnCount]);
			startingAndOptionalPawns[startingPawnCount].wasLeftBehindStartingPawn = true;
			Find.WorldPawns.PassToWorld(startingAndOptionalPawns[startingPawnCount], PawnDiscardDecideMode.KeepForever);
			Pawn pawn = startingAndOptionalPawns[startingPawnCount];
			startingAndOptionalPawns.Remove(pawn);
			startingPossessions.Remove(pawn);
		}
		List<Pawn> list = startingAndOptionalPawns;
		foreach (Pawn item in list)
		{
			item.SetFactionDirect(Faction.OfPlayer);
			PawnComponentsUtility.AddAndRemoveDynamicComponents(item);
		}
		foreach (Pawn item2 in list)
		{
			item2.workSettings.DisableAll();
		}
		foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
		{
			if (w.alwaysStartActive)
			{
				foreach (Pawn item3 in list.Where((Pawn col) => !col.WorkTypeIsDisabled(w)))
				{
					item3.workSettings.SetPriority(w, 3);
				}
				continue;
			}
			bool flag = false;
			foreach (Pawn item4 in list)
			{
				if (!item4.WorkTypeIsDisabled(w) && item4.skills.AverageOfRelevantSkillsFor(w) >= 6f)
				{
					item4.workSettings.SetPriority(w, 3);
					flag = true;
				}
			}
			if (flag)
			{
				continue;
			}
			IEnumerable<Pawn> source = list.Where((Pawn col) => !col.WorkTypeIsDisabled(w));
			if (source.Any())
			{
				source.InRandomOrder().MaxBy((Pawn c) => c.skills.AverageOfRelevantSkillsFor(w)).workSettings.SetPriority(w, 3);
			}
		}
		if (!ModsConfig.BiotechActive)
		{
			return;
		}
		for (int num = 0; num < startingAndOptionalPawns.Count; num++)
		{
			CustomXenotype forcedCustomXenotype = StartingPawnUtility.GetGenerationRequest(num).ForcedCustomXenotype;
			if (forcedCustomXenotype != null && !Current.Game.customXenotypeDatabase.customXenotypes.Contains(forcedCustomXenotype))
			{
				Current.Game.customXenotypeDatabase.customXenotypes.Add(forcedCustomXenotype);
			}
		}
	}
}
