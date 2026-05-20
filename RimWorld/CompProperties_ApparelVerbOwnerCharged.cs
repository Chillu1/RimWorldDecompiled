using Verse;

namespace RimWorld;

public class CompProperties_ApparelVerbOwnerCharged : CompProperties_ApparelVerbOwner
{
	public int maxCharges;

	public bool destroyOnEmpty;

	[MustTranslate]
	public string chargeNoun = "charge";

	public NamedArgument ChargeNounArgument => chargeNoun.Named("CHARGENOUN");

	public CompProperties_ApparelVerbOwnerCharged()
	{
		compClass = typeof(CompApparelVerbOwner_Charged);
	}
}
