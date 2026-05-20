using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class LifeStageDef : Def
{
	[MustTranslate]
	private string adjective;

	public bool visible = true;

	public bool reproductive;

	public bool milkable;

	public bool shearable;

	public bool caravanRideable;

	public bool alwaysDowned;

	public bool claimable;

	public DevelopmentalStage developmentalStage = DevelopmentalStage.Adult;

	public Type workerClass = typeof(LifeStageWorker);

	public bool involuntarySleepIsNegativeEvent = true;

	public ThinkTreeDef thinkTreeMainOverride;

	public ThinkTreeDef thinkTreeConstantOverride;

	public bool canDoRandomMentalBreaks = true;

	[MustTranslate]
	public string customMoodTipString;

	public bool canSleepWhileHeld;

	public bool canVoluntarilySleep = true;

	public bool canSleepWhenStarving = true;

	public bool canInitiateSocialInteraction = true;

	public float equipmentDrawDistanceFactor = 1f;

	public float soundAttackChance = 0.25f;

	public float voxPitch = 1f;

	public float voxVolume = 1f;

	[NoTranslate]
	public string icon;

	[Unsaved(false)]
	public Texture2D iconTex;

	public GraphicData silhouetteGraphicData;

	public List<StatModifier> statOffsets = new List<StatModifier>();

	public List<StatModifier> statFactors = new List<StatModifier>();

	public float bodySizeFactor = 1f;

	public float healthScaleFactor = 1f;

	public float hungerRateFactor = 1f;

	public float marketValueFactor = 1f;

	public float foodMaxFactor = 1f;

	public float attachPointScaleFactor = 1f;

	public float meleeDamageFactor = 1f;

	public SimpleCurve involuntarySleepMTBDaysFromRest;

	public float? fallAsleepMaxThresholdOverride;

	public float? naturalWakeThresholdOverride;

	public float? bodyWidth;

	public Vector3 bodyDrawOffset = Vector3.zero;

	public float? headSizeFactor;

	public float? eyeSizeFactor;

	public float? sittingOffset;

	public TagFilter hairStyleFilter;

	[Unsaved(false)]
	private LifeStageWorker workerInt;

	public string Adjective => adjective ?? label;

	public LifeStageWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (LifeStageWorker)Activator.CreateInstance(workerClass);
				workerInt.def = this;
			}
			return workerInt;
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (!icon.NullOrEmpty())
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				iconTex = ContentFinder<Texture2D>.Get(icon);
			});
		}
	}
}
