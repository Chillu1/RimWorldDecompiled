using Verse;

namespace RimWorld;

public class InfectionPathwayDef : Def
{
	private float expiryDays = 30f;

	private bool pawnRequired;

	public float ExpiryDays => expiryDays;

	public int ExpiryTicks => (int)expiryDays * 60000;

	public bool PawnRequired => pawnRequired;
}
