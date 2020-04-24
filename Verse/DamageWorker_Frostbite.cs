namespace Verse
{
	public class DamageWorker_Frostbite : DamageWorker_AddInjury
	{
		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
		{
			FinalizeAndAddInjury(pawn, totalDamage, dinfo, result);
		}
	}
}
