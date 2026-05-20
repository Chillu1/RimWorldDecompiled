using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.AI.Group;

namespace Verse;

public sealed class AutoSlaughterManager : IExposable
{
	private static List<Pawn> tmpAnimals = new List<Pawn>();

	private static List<Pawn> tmpAnimalsMale = new List<Pawn>();

	private static List<Pawn> tmpAnimalsMaleYoung = new List<Pawn>();

	private static List<Pawn> tmpAnimalsFemale = new List<Pawn>();

	private static List<Pawn> tmpAnimalsFemaleYoung = new List<Pawn>();

	private static List<Pawn> tmpAnimalsPregnant = new List<Pawn>();

	public Map map;

	public List<AutoSlaughterConfig> configs = new List<AutoSlaughterConfig>();

	private List<Pawn> animalsToSlaughterCached = new List<Pawn>();

	private bool cacheDirty;

	public List<Pawn> AnimalsToSlaughter
	{
		get
		{
			if (cacheDirty)
			{
				try
				{
					animalsToSlaughterCached.Clear();
					foreach (AutoSlaughterConfig config in configs)
					{
						if (!config.AnyLimit)
						{
							continue;
						}
						tmpAnimals.Clear();
						tmpAnimalsMale.Clear();
						tmpAnimalsMaleYoung.Clear();
						tmpAnimalsFemale.Clear();
						tmpAnimalsFemaleYoung.Clear();
						tmpAnimalsPregnant.Clear();
						foreach (Pawn spawnedColonyAnimal in map.mapPawns.SpawnedColonyAnimals)
						{
							if (spawnedColonyAnimal.def != config.animal || !CanAutoSlaughterNow(spawnedColonyAnimal) || (!config.allowSlaughterBonded && spawnedColonyAnimal.relations.GetDirectRelationsCount(PawnRelationDefOf.Bond) > 0))
							{
								continue;
							}
							if (spawnedColonyAnimal.gender == Gender.Male)
							{
								if (spawnedColonyAnimal.ageTracker.CurLifeStage.reproductive)
								{
									tmpAnimalsMale.Add(spawnedColonyAnimal);
								}
								else
								{
									tmpAnimalsMaleYoung.Add(spawnedColonyAnimal);
								}
								tmpAnimals.Add(spawnedColonyAnimal);
							}
							else if (spawnedColonyAnimal.gender == Gender.Female)
							{
								if (spawnedColonyAnimal.ageTracker.CurLifeStage.reproductive)
								{
									if (!spawnedColonyAnimal.health.hediffSet.HasHediff(HediffDefOf.Pregnant))
									{
										tmpAnimalsFemale.Add(spawnedColonyAnimal);
										tmpAnimals.Add(spawnedColonyAnimal);
									}
									else if (config.allowSlaughterPregnant)
									{
										tmpAnimalsPregnant.Add(spawnedColonyAnimal);
									}
								}
								else
								{
									tmpAnimalsFemaleYoung.Add(spawnedColonyAnimal);
									tmpAnimals.Add(spawnedColonyAnimal);
								}
							}
							else
							{
								tmpAnimals.Add(spawnedColonyAnimal);
							}
						}
						tmpAnimals.SortByDescending((Pawn a) => a.ageTracker.AgeBiologicalTicks);
						tmpAnimalsMale.SortByDescending((Pawn a) => a.ageTracker.AgeBiologicalTicks);
						tmpAnimalsMaleYoung.SortByDescending((Pawn a) => a.ageTracker.AgeBiologicalTicks);
						tmpAnimalsFemale.SortByDescending((Pawn a) => a.ageTracker.AgeBiologicalTicks);
						tmpAnimalsFemaleYoung.SortByDescending((Pawn a) => a.ageTracker.AgeBiologicalTicks);
						if (config.allowSlaughterPregnant)
						{
							tmpAnimalsPregnant.SortByDescending((Pawn a) => a.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant).Severity);
							tmpAnimalsFemale.AddRange(tmpAnimalsPregnant);
							tmpAnimals.AddRange(tmpAnimalsPregnant);
						}
						if (config.maxFemales != -1)
						{
							while (tmpAnimalsFemale.Count > config.maxFemales)
							{
								Pawn item = tmpAnimalsFemale.PopFront();
								tmpAnimals.Remove(item);
								animalsToSlaughterCached.Add(item);
							}
						}
						if (config.maxFemalesYoung != -1)
						{
							while (tmpAnimalsFemaleYoung.Count > config.maxFemalesYoung)
							{
								Pawn item2 = tmpAnimalsFemaleYoung.PopFront();
								tmpAnimals.Remove(item2);
								animalsToSlaughterCached.Add(item2);
							}
						}
						if (config.maxMales != -1)
						{
							while (tmpAnimalsMale.Count > config.maxMales)
							{
								Pawn item3 = tmpAnimalsMale.PopFront();
								tmpAnimals.Remove(item3);
								animalsToSlaughterCached.Add(item3);
							}
						}
						if (config.maxMalesYoung != -1)
						{
							while (tmpAnimalsMaleYoung.Count > config.maxMalesYoung)
							{
								Pawn item4 = tmpAnimalsMaleYoung.PopFront();
								tmpAnimals.Remove(item4);
								animalsToSlaughterCached.Add(item4);
							}
						}
						if (config.maxTotal == -1)
						{
							continue;
						}
						while (tmpAnimals.Count > config.maxTotal)
						{
							Pawn pawn = tmpAnimals.PopFront();
							if (pawn.gender == Gender.Male)
							{
								if (pawn.ageTracker.CurLifeStage.reproductive)
								{
									tmpAnimalsMale.Remove(pawn);
								}
								else
								{
									tmpAnimalsMaleYoung.Remove(pawn);
								}
							}
							else if (pawn.gender == Gender.Female)
							{
								if (pawn.ageTracker.CurLifeStage.reproductive)
								{
									tmpAnimalsFemale.Remove(pawn);
								}
								else
								{
									tmpAnimalsFemaleYoung.Remove(pawn);
								}
							}
							animalsToSlaughterCached.Add(pawn);
						}
					}
					cacheDirty = false;
				}
				finally
				{
					tmpAnimals.Clear();
					tmpAnimalsMale.Clear();
					tmpAnimalsFemale.Clear();
					tmpAnimalsMaleYoung.Clear();
					tmpAnimalsFemaleYoung.Clear();
					tmpAnimalsPregnant.Clear();
				}
			}
			return animalsToSlaughterCached;
		}
	}

	public static bool CanEverAutoSlaughter(Pawn animal)
	{
		if (animal.HomeFaction == Faction.OfPlayer)
		{
			return !animal.RaceProps.Dryad;
		}
		return false;
	}

	public static bool CanAutoSlaughterNow(Pawn animal)
	{
		if (!CanEverAutoSlaughter(animal))
		{
			return false;
		}
		if (animal.GetLord() != null)
		{
			return false;
		}
		if (animal.inventory != null && animal.inventory.UnloadEverything)
		{
			return false;
		}
		return true;
	}

	public AutoSlaughterManager(Map map)
	{
		this.map = map;
		TryPopulateMissingAnimals();
	}

	public void Notify_PawnDespawned()
	{
		cacheDirty = true;
	}

	public void Notify_PawnSpawned()
	{
		cacheDirty = true;
	}

	public void Notify_PawnChangedFaction()
	{
		cacheDirty = true;
	}

	public void Notify_ConfigChanged()
	{
		cacheDirty = true;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref configs, "configs", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (configs.RemoveAll((AutoSlaughterConfig x) => x.animal == null || x.animal.IsCorpse) != 0)
			{
				Log.Warning("Some auto-slaughter configs had null animals after loading.");
			}
			TryPopulateMissingAnimals();
		}
	}

	private void TryPopulateMissingAnimals()
	{
		HashSet<ThingDef> hashSet = new HashSet<ThingDef>();
		hashSet.AddRange(configs.Select((AutoSlaughterConfig c) => c.animal));
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef.race != null && allDef.race.Animal && allDef.GetStatValueAbstract(StatDefOf.Wildness) < 1f && !allDef.race.Dryad && !allDef.IsCorpse && !hashSet.Contains(allDef))
			{
				configs.Add(new AutoSlaughterConfig
				{
					animal = allDef
				});
			}
		}
	}
}
