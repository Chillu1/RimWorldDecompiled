using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualBehaviorDef : Def
{
	public Type workerClass = typeof(RitualBehaviorWorker);

	public List<RitualRole> roles;

	public List<RitualStage> stages;

	public IntRange durationTicks = new IntRange(7500, 7500);

	public List<PreceptRequirement> preceptRequirements;

	public List<RitualCancellationTrigger> cancellationTriggers;

	public bool displayCompletedMessage = true;

	[NoTranslate]
	public string useVisualEffectsFromRoleIdIdeo;

	public int displayOrder;

	[MustTranslate]
	public string letterTitle;

	[MustTranslate]
	public string letterText;

	[MustTranslate]
	public string spectatorsLabel;

	[MustTranslate]
	public string spectatorGerund;

	public bool spectatorsIgnoreBleeding;

	public List<SoundDef> soundDefsPerEnhancerCount;

	public int maxEnhancerDistance;

	public RitualSpectatorFilter spectatorFilter;

	public RitualPosition_Lectern FirstLecternPosition
	{
		get
		{
			foreach (RitualStage stage in stages)
			{
				if (stage.roleBehaviors == null)
				{
					continue;
				}
				foreach (RitualRoleBehavior roleBehavior in stage.roleBehaviors)
				{
					if (roleBehavior.CustomPositionsForReading == null)
					{
						continue;
					}
					foreach (RitualPosition item in roleBehavior.CustomPositionsForReading)
					{
						if (item is RitualPosition_Lectern result)
						{
							return result;
						}
					}
				}
			}
			return null;
		}
	}

	public bool UsesLectern => FirstLecternPosition != null;

	public RitualBehaviorWorker GetInstance()
	{
		return (RitualBehaviorWorker)Activator.CreateInstance(workerClass, this);
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		foreach (RitualStage stage in stages)
		{
			if (stage.endTriggers.NullOrEmpty())
			{
				yield return "ritual stage with no endTrigger";
			}
		}
		if (!string.IsNullOrEmpty(spectatorsLabel) && string.IsNullOrEmpty(spectatorGerund))
		{
			yield return "ritual has spectatorLabel but no spectatorGerund";
		}
		if (!string.IsNullOrEmpty(spectatorGerund) && string.IsNullOrEmpty(spectatorsLabel))
		{
			yield return "ritual has spectatorGerund but no spectatorLabel";
		}
		if (!roles.NullOrEmpty() && roles.Count((RitualRole x) => x.defaultForSelectedColonist) > 1)
		{
			yield return ">1 default role for selected pawn";
		}
	}

	public override void PostLoad()
	{
		base.PostLoad();
		if (!stages.NullOrEmpty())
		{
			for (int i = 0; i < stages.Count; i++)
			{
				stages[i].parent = this;
			}
		}
	}

	public IEnumerable<RitualRole> RequiredRoles()
	{
		if (roles.NullOrEmpty())
		{
			yield break;
		}
		for (int i = 0; i < roles.Count; i++)
		{
			if (roles[i].required)
			{
				yield return roles[i];
			}
		}
	}
}
