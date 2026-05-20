using System.Text.RegularExpressions;
using RimWorld;

namespace Verse;

public class Dialog_RenamePolicy : Dialog_Rename<Policy>
{
	private static readonly Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-]*$");

	public Dialog_RenamePolicy(Policy policy)
		: base(policy)
	{
	}

	protected override AcceptanceReport NameIsValid(string name)
	{
		AcceptanceReport result = base.NameIsValid(name);
		if (!result.Accepted)
		{
			return result;
		}
		if (!ValidNameRegex.IsMatch(name))
		{
			return "InvalidName".Translate();
		}
		return true;
	}
}
