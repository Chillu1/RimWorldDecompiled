using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class ScenPart_PlayerFaction : ScenPart
{
	internal FactionDef factionDef;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref factionDef, "factionDef");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && factionDef == null)
		{
			Randomize();
			Log.Error("ScenPart had null faction after loading. Changing to " + factionDef.ToStringSafe());
		}
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		if (!Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), factionDef.LabelCap))
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (FactionDef item in DefDatabase<FactionDef>.AllDefs.Where((FactionDef d) => d.isPlayer))
		{
			FactionDef localFd = item;
			list.Add(new FloatMenuOption(localFd.LabelCap, delegate
			{
				factionDef = localFd;
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public override string Summary(Scenario scen)
	{
		return "ScenPart_PlayerFaction".Translate(factionDef.label);
	}

	public override void Randomize()
	{
		factionDef = DefDatabase<FactionDef>.AllDefs.Where((FactionDef fd) => fd.isPlayer).RandomElement();
	}

	public override void PostWorldGenerate()
	{
		Find.GameInitData.playerFaction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(factionDef));
		Find.FactionManager.Add(Find.GameInitData.playerFaction);
	}

	public override void PreMapGenerate()
	{
		PlanetTile startingTile = Find.GameInitData.startingTile;
		Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(startingTile.LayerDef.SettlementWorldObjectDef);
		settlement.SetFaction(Find.GameInitData.playerFaction);
		settlement.Tile = startingTile;
		settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, Find.GameInitData.playerFaction.def.playerInitialSettlementNameMaker);
		Find.WorldObjects.Add(settlement);
	}

	public override void PostGameStart()
	{
		Find.GameInitData.playerFaction = null;
		TaleRecorder.RecordTale(TaleDefOf.TileSettled).customLabel = "NewSettlement".Translate();
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (factionDef == null)
		{
			yield return "factionDef is null";
		}
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((factionDef != null) ? factionDef.GetHashCode() : 0);
	}
}
