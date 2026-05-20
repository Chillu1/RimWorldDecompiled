using Verse;

namespace RimWorld;

public abstract class PawnColumnWorker_Designator : PawnColumnWorker_Checkbox
{
	protected abstract DesignationDef DesignationType { get; }

	protected virtual void Notify_DesignationAdded(Pawn pawn)
	{
	}

	protected override bool GetValue(Pawn pawn)
	{
		return GetDesignation(pawn) != null;
	}

	protected override void SetValue(Pawn pawn, bool value, PawnTable table)
	{
		if (value == GetValue(pawn))
		{
			return;
		}
		if (table.SortingBy == def)
		{
			table.SetDirty();
		}
		if (value)
		{
			if (ShouldConfirmDesignation(pawn, out var title))
			{
				Find.WindowStack.Add(new Dialog_Confirm(title, delegate
				{
					DesignationConfirmed(pawn);
				}));
			}
			else
			{
				DesignationConfirmed(pawn);
			}
		}
		else
		{
			Designation designation = GetDesignation(pawn);
			if (designation != null)
			{
				pawn.MapHeld.designationManager.RemoveDesignation(designation);
			}
		}
	}

	private void DesignationConfirmed(Pawn pawn)
	{
		pawn.MapHeld.designationManager.AddDesignation(new Designation(pawn, DesignationType));
		Notify_DesignationAdded(pawn);
	}

	protected virtual bool ShouldConfirmDesignation(Pawn pawn, out string title)
	{
		title = "";
		return false;
	}

	private Designation GetDesignation(Pawn pawn)
	{
		return pawn.MapHeld?.designationManager.DesignationOn(pawn, DesignationType);
	}
}
