namespace RimWorld;

public class CompProperties_AbilityTransferEntropy : CompProperties_AbilityEffect
{
	public bool targetReceivesEntropy = true;

	public CompProperties_AbilityTransferEntropy()
	{
		compClass = typeof(CompAbilityEffect_TransferEntropy);
	}
}
