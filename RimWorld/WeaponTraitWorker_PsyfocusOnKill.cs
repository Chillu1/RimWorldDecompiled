using Verse;

namespace RimWorld;

public class WeaponTraitWorker_PsyfocusOnKill : WeaponTraitWorker
{
	private const float PsyfocusOffset = 0.2f;

	public override void Notify_KilledPawn(Pawn pawn)
	{
		base.Notify_KilledPawn(pawn);
		pawn.psychicEntropy?.OffsetPsyfocusDirectly(0.2f);
	}
}
