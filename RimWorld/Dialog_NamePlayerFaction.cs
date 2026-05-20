namespace RimWorld;

public class Dialog_NamePlayerFaction : Dialog_GiveName
{
	protected override int FirstCharLimit => 64;

	public Dialog_NamePlayerFaction()
	{
		nameGenerator = () => NameGenerator.GenerateName(Faction.OfPlayer.def.factionNameMaker, IsValidName);
		curName = nameGenerator();
		nameMessageKey = "NamePlayerFactionMessage";
		gainedNameMessageKey = "PlayerFactionGainsName";
		invalidNameMessageKey = "PlayerFactionNameIsInvalid";
	}

	protected override bool IsValidName(string s)
	{
		return NamePlayerFactionDialogUtility.IsValidName(s);
	}

	protected override void Named(string s)
	{
		NamePlayerFactionDialogUtility.Named(s);
	}
}
