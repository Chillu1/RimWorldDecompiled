using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class SurgeryOutcomeComp_MedicineQuality : SurgeryOutcomeComp_Curve
{
	protected override float XGetter(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
	{
		int num = 0;
		float num2 = 0f;
		if (bill is Bill_Medical { consumedMedicine: not null } bill_Medical)
		{
			foreach (KeyValuePair<ThingDef, int> item in bill_Medical.consumedMedicine)
			{
				num += item.Value;
				num2 += item.Key.GetStatValueAbstract(StatDefOf.MedicalPotency) * (float)item.Value;
			}
		}
		if (num == 0)
		{
			return 1f;
		}
		return num2 / (float)num;
	}
}
