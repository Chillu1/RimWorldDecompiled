using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Dialog_NamePlayerGravship : Dialog_GiveName
{
	private readonly Building_GravEngine engine;

	protected override int FirstCharLimit => 64;

	public Dialog_NamePlayerGravship(Building_GravEngine engine)
	{
		if (ModLister.CheckOdyssey("Grav engines"))
		{
			this.engine = engine;
			nameGenerator = () => NameGenerator.GenerateName(RulePackDefOf.NamerGravship, IsValidName);
			curName = nameGenerator();
			nameMessageKey = "NamePlayerGravshipMessage";
			gainedNameMessageKey = "PlayerGravshipGainsName";
			invalidNameMessageKey = "PlayerGravshipNameIsInvalid";
		}
	}

	protected override bool IsValidName(string s)
	{
		if (s.Length == 0)
		{
			return false;
		}
		if (s.Length > 64)
		{
			return false;
		}
		if (GrammarResolver.ContainsSpecialChars(s))
		{
			return false;
		}
		return true;
	}

	protected override void Named(string s)
	{
		engine.RenamableLabel = s;
		engine.nameHidden = false;
	}
}
