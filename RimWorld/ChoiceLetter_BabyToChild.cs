using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ChoiceLetter_BabyToChild : ChoiceLetter
{
	private const int TimeoutTicks = 30000;

	private Pawn pawn;

	private bool bornSlave;

	private TaggedString ChoseColonistLabel;

	private TaggedString ChoseSlaveLabel;

	public override bool CanShowInLetterStack => pawn.Faction?.IsPlayer ?? false;

	public override IEnumerable<DiaOption> Choices
	{
		get
		{
			if (!base.ArchivedOnly)
			{
				if (pawn.Faction?.IsPlayer ?? false)
				{
					yield return new DiaOption(ChoseColonistLabel)
					{
						action = ChoseColonist,
						disabled = (bornSlave != pawn.IsSlave),
						disabledReason = "CannotChangeChildStatusReason".Translate(pawn),
						resolveTree = true
					};
					yield return new DiaOption(ChoseSlaveLabel)
					{
						action = ChoseSlave,
						disabled = (bornSlave != pawn.IsSlave),
						disabledReason = "CannotChangeChildStatusReason".Translate(pawn),
						resolveTree = true
					};
				}
				if (bornSlave != pawn.IsSlave)
				{
					yield return base.Option_Close;
				}
				else
				{
					yield return base.Option_Postpone;
				}
			}
			else
			{
				yield return base.Option_Close;
			}
			yield return base.Option_JumpToLocationAndPostpone;
		}
	}

	public void Start()
	{
		StartTimeout(30000);
		pawn = lookTargets.TryGetPrimaryTarget().Thing as Pawn;
		bornSlave = pawn.IsSlave;
		if (bornSlave)
		{
			ChoseColonistLabel = "Emancipate".Translate().CapitalizeFirst();
			ChoseSlaveLabel = "RemainX".Translate(pawn.LegalStatus).CapitalizeFirst();
		}
		else
		{
			ChoseColonistLabel = "RemainX".Translate(pawn.LegalStatus).CapitalizeFirst();
			ChoseSlaveLabel = "Enslave".Translate().CapitalizeFirst();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref bornSlave, "bornSlave", defaultValue: false);
		Scribe_Values.Look(ref ChoseColonistLabel, "ChoseColonistLabel");
		Scribe_Values.Look(ref ChoseSlaveLabel, "ChoseSlaveLabel");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawn = lookTargets.TryGetPrimaryTarget().Thing as Pawn;
		}
	}

	private void ChoseColonist()
	{
		if (bornSlave)
		{
			GenGuest.SlaveRelease(pawn);
		}
		Find.LetterStack.RemoveLetter(this);
	}

	private void ChoseSlave()
	{
		if (!bornSlave)
		{
			pawn.guest.SetGuestStatus(pawn.Faction, GuestStatus.Slave);
		}
		Find.LetterStack.RemoveLetter(this);
	}
}
