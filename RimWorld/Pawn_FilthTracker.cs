using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Pawn_FilthTracker : IExposable
{
	private readonly Pawn pawn;

	private List<Filth> carriedFilth = new List<Filth>();

	private ThingDef lastTerrainFilthDef;

	private const float FilthPickupChance = 0.1f;

	private const float FilthDropChance = 0.05f;

	private const int MaxCarriedTerrainFilthThickness = 1;

	private const float BaseChanceToSpreadPerCell = 0.005f;

	public string FilthReport
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("FilthOnFeet".Translate());
			if (carriedFilth.Count == 0)
			{
				stringBuilder.Append("(" + "NoneLower".Translate() + ")");
			}
			else
			{
				for (int i = 0; i < carriedFilth.Count; i++)
				{
					stringBuilder.AppendLine(carriedFilth[i].LabelCap);
				}
			}
			return stringBuilder.ToString();
		}
	}

	public List<Filth> CarriedFilthListForReading => carriedFilth;

	private FilthSourceFlags AdditionalFilthSourceFlags
	{
		get
		{
			if (pawn.Faction != null || !pawn.RaceProps.Animal)
			{
				return FilthSourceFlags.Unnatural;
			}
			return FilthSourceFlags.Natural;
		}
	}

	public Pawn_FilthTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref lastTerrainFilthDef, "lastTerrainFilthDef");
		Scribe_Collections.Look(ref carriedFilth, "carriedFilth", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (carriedFilth.RemoveAll((Filth x) => x == null) != 0)
			{
				Log.Error(pawn.ToStringSafe() + " had null carried filth after loading.");
			}
			if (carriedFilth.RemoveAll((Filth x) => x.def == null) != 0)
			{
				Log.Error(pawn.ToStringSafe() + " had carried filth with null def after loading.");
			}
		}
	}

	public void Notify_EnteredNewCell()
	{
		if (pawn.Flying)
		{
			return;
		}
		if (Rand.Value < 0.05f)
		{
			TryDropFilth();
		}
		if (Rand.Value < 0.1f)
		{
			TryPickupFilth();
		}
		if (!(Rand.Value < pawn.GetStatValue(StatDefOf.FilthRate) * 0.005f))
		{
			return;
		}
		if (pawn.RaceProps.Humanlike)
		{
			if (FilthMaker.TryMakeFilth(filthDef: (lastTerrainFilthDef == null || !Rand.Chance(0.66f)) ? ThingDefOf.Filth_Trash : lastTerrainFilthDef, c: pawn.Position, map: pawn.Map, count: 1, additionalFlags: AdditionalFilthSourceFlags | FilthSourceFlags.Pawn))
			{
				FilthMonitor.Notify_FilthHumanGenerated();
			}
		}
		else if (pawn.RaceProps.Insect)
		{
			if (FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, ThingDefOf.Filth_Slime, 1, AdditionalFilthSourceFlags | FilthSourceFlags.Pawn))
			{
				FilthMonitor.Notify_FilthAnimalGenerated();
			}
		}
		else if (pawn.IsAnimal && FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, ThingDefOf.Filth_AnimalFilth, 1, AdditionalFilthSourceFlags | FilthSourceFlags.Pawn))
		{
			FilthMonitor.Notify_FilthAnimalGenerated();
		}
	}

	private void TryPickupFilth()
	{
		TerrainDef terrDef = pawn.Map.terrainGrid.TerrainAt(pawn.Position);
		if (terrDef.generatedFilth != null)
		{
			for (int num = carriedFilth.Count - 1; num >= 0; num--)
			{
				if (carriedFilth[num].def.filth.TerrainSourced && carriedFilth[num].def != terrDef.generatedFilth)
				{
					ThinCarriedFilth(carriedFilth[num]);
				}
			}
			Filth filth = carriedFilth.FirstOrDefault((Filth f) => f.def == terrDef.generatedFilth);
			if (filth == null || filth.thickness < 1)
			{
				GainFilth(terrDef.generatedFilth);
				FilthMonitor.Notify_FilthAccumulated();
			}
		}
		List<Thing> thingList = pawn.Position.GetThingList(pawn.Map);
		for (int num2 = thingList.Count - 1; num2 >= 0; num2--)
		{
			if (thingList[num2] is Filth { CanFilthAttachNow: not false } filth2)
			{
				GainFilth(filth2.def, filth2.sources);
				filth2.ThinFilth();
			}
		}
		if (pawn.Position.Roofed(pawn.Map))
		{
			return;
		}
		foreach (GameCondition activeCondition in pawn.Map.GameConditionManager.ActiveConditions)
		{
			if (activeCondition.def.spreadsFilth != null)
			{
				GainFilth(activeCondition.def.spreadsFilth, Gen.YieldSingle(activeCondition.Label));
			}
		}
	}

	private void TryDropFilth()
	{
		if (carriedFilth.Count == 0)
		{
			return;
		}
		for (int num = carriedFilth.Count - 1; num >= 0; num--)
		{
			if (carriedFilth[num].CanDropAt(pawn.Position, pawn.Map))
			{
				DropCarriedFilth(carriedFilth[num]);
				FilthMonitor.Notify_FilthDropped();
			}
		}
	}

	private void DropCarriedFilth(Filth f)
	{
		if (FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, f.def, f.sources, AdditionalFilthSourceFlags))
		{
			ThinCarriedFilth(f);
		}
	}

	private void ThinCarriedFilth(Filth f)
	{
		f.ThinFilth();
		if (f.thickness <= 0)
		{
			carriedFilth.Remove(f);
		}
	}

	public void GainFilth(ThingDef filthDef)
	{
		if (filthDef.filth.TerrainSourced)
		{
			lastTerrainFilthDef = filthDef;
		}
		GainFilth(filthDef, null);
	}

	public void GainFilth(ThingDef filthDef, IEnumerable<string> sources)
	{
		if (filthDef.filth.TerrainSourced)
		{
			lastTerrainFilthDef = filthDef;
		}
		Filth filth = null;
		for (int i = 0; i < carriedFilth.Count; i++)
		{
			if (carriedFilth[i].def == filthDef)
			{
				filth = carriedFilth[i];
				break;
			}
		}
		if (filth != null)
		{
			if (filth.CanBeThickened)
			{
				filth.ThickenFilth();
				filth.AddSources(sources);
			}
		}
		else
		{
			Filth filth2 = (Filth)ThingMaker.MakeThing(filthDef);
			filth2.AddSources(sources);
			carriedFilth.Add(filth2);
		}
	}
}
