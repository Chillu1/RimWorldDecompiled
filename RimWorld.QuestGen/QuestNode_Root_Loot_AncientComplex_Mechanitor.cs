using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Loot_AncientComplex_Mechanitor : QuestNode_Root_Loot_AncientComplex
{
	protected override LayoutDef LayoutDef => LayoutDefOf.AncientComplex_Mechanitor_Loot;

	protected override SitePartDef SitePartDef => SitePartDefOf.AncientComplex_Mechanitor;

	protected override bool BeforeRunInt()
	{
		if (!ModLister.CheckBiotech("Ancient mechanitor complex"))
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (!slate.TryGet<bool>("discovered", out var _))
		{
			slate.Set("discovered", var: false);
		}
		base.RunInt();
	}
}
