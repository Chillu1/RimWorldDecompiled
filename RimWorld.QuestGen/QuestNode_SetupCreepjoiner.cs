using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_SetupCreepjoiner : QuestNode
{
	public SlateRef<CreepJoinerFormKindDef> form;

	public SlateRef<CreepJoinerBenefitDef> benefit;

	public SlateRef<CreepJoinerDownsideDef> downside;

	public SlateRef<CreepJoinerAggressiveDef> aggressive;

	public SlateRef<CreepJoinerRejectionDef> rejection;

	[NoTranslate]
	public SlateRef<string> storeFormAs;

	[NoTranslate]
	public SlateRef<string> storeBenefitAs;

	[NoTranslate]
	public SlateRef<string> storeDownsideAs;

	[NoTranslate]
	public SlateRef<string> storeAggressiveAs;

	[NoTranslate]
	public SlateRef<string> storeRejectionAs;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		CreepJoinerFormKindDef value = form.GetValue(slate);
		CreepJoinerBenefitDef value2 = benefit.GetValue(slate);
		CreepJoinerDownsideDef value3 = downside.GetValue(slate);
		CreepJoinerAggressiveDef value4 = aggressive.GetValue(slate);
		CreepJoinerRejectionDef value5 = rejection.GetValue(slate);
		CreepJoinerUtility.GetCreepjoinerSpecifics(map, ref value, ref value2, ref value3, ref value4, ref value5);
		if (storeFormAs.GetValue(slate) != null)
		{
			slate.Set(storeFormAs.GetValue(slate), value);
		}
		if (storeBenefitAs.GetValue(slate) != null)
		{
			slate.Set(storeBenefitAs.GetValue(slate), value2);
		}
		if (storeDownsideAs.GetValue(slate) != null)
		{
			slate.Set(storeDownsideAs.GetValue(slate), value3);
		}
		if (storeAggressiveAs.GetValue(slate) != null)
		{
			slate.Set(storeAggressiveAs.GetValue(slate), value4);
		}
		if (storeRejectionAs.GetValue(slate) != null)
		{
			slate.Set(storeRejectionAs.GetValue(slate), value5);
		}
	}

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}
}
