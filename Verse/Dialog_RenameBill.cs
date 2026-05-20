using RimWorld;

namespace Verse;

public class Dialog_RenameBill : Dialog_Rename<Bill_Production>
{
	public Dialog_RenameBill(Bill_Production renaming)
		: base(renaming)
	{
	}
}
