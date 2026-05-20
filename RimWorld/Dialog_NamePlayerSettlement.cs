using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Dialog_NamePlayerSettlement : Dialog_GiveName
{
	private Settlement settlement;

	protected override int FirstCharLimit => 64;

	public Dialog_NamePlayerSettlement(Settlement settlement)
	{
		this.settlement = settlement;
		if (settlement.HasMap && settlement.Map.mapPawns.FreeColonistsSpawnedCount != 0)
		{
			suggestingPawn = settlement.Map.mapPawns.FreeColonistsSpawned.RandomElement();
		}
		nameGenerator = () => NameGenerator.GenerateName(Faction.OfPlayer.def.settlementNameMaker, IsValidName);
		curName = nameGenerator();
		nameMessageKey = "NamePlayerFactionBaseMessage";
		gainedNameMessageKey = "PlayerFactionBaseGainsName";
		invalidNameMessageKey = "PlayerFactionBaseNameIsInvalid";
	}

	public override void PostOpen()
	{
		base.PostOpen();
		if (settlement.Map != null)
		{
			Current.Game.CurrentMap = settlement.Map;
		}
	}

	protected override bool IsValidName(string s)
	{
		return NamePlayerSettlementDialogUtility.IsValidName(s);
	}

	protected override void Named(string s)
	{
		NamePlayerSettlementDialogUtility.Named(settlement, s);
	}
}
