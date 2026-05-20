using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_WorkDisabled : QuestPartActivable
{
	public List<Pawn> pawns = new List<Pawn>();

	public WorkTags disabledWorkTags;

	public IEnumerable<WorkTypeDef> DisabledWorkTypes
	{
		get
		{
			if (base.State != QuestPartState.Enabled)
			{
				yield break;
			}
			List<WorkTypeDef> list = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < list.Count; i++)
			{
				if ((disabledWorkTags & list[i].workTags) != WorkTags.None)
				{
					yield return list[i];
				}
			}
		}
	}

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		ClearPawnWorkTypesAndSkillsCache();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		ClearPawnWorkTypesAndSkillsCache();
	}

	private void ClearPawnWorkTypesAndSkillsCache()
	{
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i] != null)
			{
				pawns[i].Notify_DisabledWorkTypesChanged();
				if (pawns[i].skills != null)
				{
					pawns[i].skills.Notify_SkillDisablesChanged();
				}
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Values.Look(ref disabledWorkTags, "disabledWorkTags", WorkTags.None);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
