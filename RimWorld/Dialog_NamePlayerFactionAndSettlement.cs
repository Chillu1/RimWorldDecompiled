using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Dialog_NamePlayerFactionAndSettlement : Dialog_GiveName
{
	private Settlement settlement;

	protected override int FirstCharLimit => 64;

	protected override int SecondCharLimit => 64;

	public Dialog_NamePlayerFactionAndSettlement(Settlement settlement)
	{
		this.settlement = settlement;
		if (settlement.HasMap && settlement.Map.mapPawns.FreeColonistsSpawnedCount != 0)
		{
			suggestingPawn = settlement.Map.mapPawns.FreeColonistsSpawned.RandomElement();
		}
		nameGenerator = () => NameGenerator.GenerateName(Faction.OfPlayer.def.factionNameMaker, IsValidName);
		curName = nameGenerator();
		nameMessageKey = "NamePlayerFactionMessage";
		invalidNameMessageKey = "PlayerFactionNameIsInvalid";
		useSecondName = true;
		secondNameGenerator = () => NameGenerator.GenerateName(Faction.OfPlayer.def.settlementNameMaker, IsValidSecondName);
		curSecondName = secondNameGenerator();
		secondNameMessageKey = "NamePlayerFactionBaseMessage_NameFactionContinuation";
		invalidSecondNameMessageKey = "PlayerFactionBaseNameIsInvalid";
		gainedNameMessageKey = "PlayerFactionAndBaseGainsName";
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
		return NamePlayerFactionDialogUtility.IsValidName(s);
	}

	protected override bool IsValidSecondName(string s)
	{
		return NamePlayerSettlementDialogUtility.IsValidName(s);
	}

	protected override void Named(string s)
	{
		NamePlayerFactionDialogUtility.Named(s);
	}

	protected override void NamedSecond(string s)
	{
		NamePlayerSettlementDialogUtility.Named(settlement, s);
	}
}
