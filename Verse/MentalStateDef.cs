using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public class MentalStateDef : Def
{
	public Type stateClass = typeof(MentalState);

	public Type workerClass = typeof(MentalStateWorker);

	public MentalStateCategory category;

	public bool prisonersCanDo = true;

	public bool slavesCanDo = true;

	public bool inCaravanCanDo;

	public bool colonistsOnly;

	public bool slavesOnly;

	public List<PawnCapacityDef> requiredCapacities = new List<PawnCapacityDef>();

	public bool downedCanDo;

	public bool unspawnedNotInCaravanCanDo;

	public bool blockNormalThoughts;

	public bool stopsJobs = true;

	public List<InteractionDef> blockInteractionInitiationExcept;

	public List<InteractionDef> blockInteractionRecipientExcept;

	public bool blockRandomInteraction;

	public EffecterDef stateEffecter;

	public TaleDef tale;

	public bool allowBeatfire;

	public DrugCategory drugCategory = DrugCategory.Any;

	public bool ignoreDrugPolicy;

	public IntRange ticksBetweenRamblingRange;

	public float recoveryMtbDays = 1f;

	public int minTicksBeforeRecovery = 500;

	public int maxTicksBeforeRecovery = 99999999;

	public bool recoverFromSleep;

	public bool recoverFromDowned = true;

	public bool recoverFromCaptured = true;

	public bool recoverFromCollapsingExhausted = true;

	public ThoughtDef moodRecoveryThought;

	public bool allowGuilty = true;

	[MustTranslate]
	public string beginLetter;

	[MustTranslate]
	public string beginLetterLabel;

	public LetterDef beginLetterDef;

	public Color nameColor = Color.green;

	[MustTranslate]
	public string recoveryMessage;

	[MustTranslate]
	public string baseInspectLine;

	public bool escapingPrisonersIgnore;

	public bool blocksDefendAndExpandHive;

	private MentalStateWorker workerInt;

	public MentalStateWorker Worker
	{
		get
		{
			if (workerInt == null && workerClass != null)
			{
				workerInt = (MentalStateWorker)Activator.CreateInstance(workerClass);
				workerInt.def = this;
			}
			return workerInt;
		}
	}

	public bool IsAggro => category == MentalStateCategory.Aggro;

	public bool IsExtreme
	{
		get
		{
			List<MentalBreakDef> allDefsListForReading = DefDatabase<MentalBreakDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].intensity == MentalBreakIntensity.Extreme && allDefsListForReading[i].mentalState == this)
				{
					return true;
				}
			}
			return false;
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (beginLetterDef == null)
		{
			beginLetterDef = LetterDefOf.NegativeEvent;
		}
	}
}
