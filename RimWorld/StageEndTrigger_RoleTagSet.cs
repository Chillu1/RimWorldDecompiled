using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class StageEndTrigger_RoleTagSet : StageEndTrigger
{
	public List<string> roleIds;

	public bool clearTag;

	[NoTranslate]
	public string tag;

	public override Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage)
	{
		return new Trigger_TickCondition(delegate
		{
			foreach (string roleId in roleIds)
			{
				if (!ArrivedCheck(roleId, ritual))
				{
					return false;
				}
			}
			if (clearTag)
			{
				foreach (string roleId2 in roleIds)
				{
					foreach (KeyValuePair<Pawn, PawnTags> perPawnTag in ritual.perPawnTags)
					{
						RitualRole ritualRole = ritual.RoleFor(perPawnTag.Key, includeForced: true);
						if (ritualRole != null && ritualRole.id == roleId2 && perPawnTag.Value.Contains(tag))
						{
							perPawnTag.Value.tags.Remove(tag);
						}
					}
				}
			}
			return true;
		});
	}

	protected virtual bool ArrivedCheck(string r, LordJob_Ritual ritual)
	{
		if (!ritual.RoleFilled(r))
		{
			return true;
		}
		foreach (KeyValuePair<Pawn, PawnTags> perPawnTag in ritual.perPawnTags)
		{
			RitualRole ritualRole = ritual.RoleFor(perPawnTag.Key, includeForced: true);
			if (ritualRole != null && ritualRole.id == r && perPawnTag.Value.Contains(tag))
			{
				return true;
			}
		}
		return false;
	}

	public override void ExposeData()
	{
		Scribe_Collections.Look(ref roleIds, "roleIds", LookMode.Undefined);
		Scribe_Values.Look(ref clearTag, "clearTag", defaultValue: false);
		Scribe_Values.Look(ref tag, "tag");
	}
}
