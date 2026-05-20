using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class MonolithLevelDef : Def
{
	public int level;

	public int graphicIndex;

	public bool monolithGlows;

	public int anomalyThreatTier;

	public bool useInactiveAnomalyThreatFraction;

	public bool useActiveAnomalyThreatFraction;

	public float anomalyThreatFractionFactor = 1f;

	public float anomalyThreatFraction;

	public List<IncidentDef> incidentsOnReached;

	public bool triggersGrayPall;

	public List<GameConditionDef> unreachableDuringConditions;

	public bool advanceThroughActivation;

	public int desiredHarbingerTreeCount;

	public bool postEndgame;

	public int monolithGlowRadiusOverride = -1;

	public KnowledgeCategoryDef monolithStudyCategory;

	public EntityCategoryDef entityCatagoryCompletionRequired;

	public int entityCountCompletionRequired;

	public float anomalyMentalBreakChance;

	public SoundDef activateSound;

	public SoundDef activatedSound;

	public List<MonolithAttachment> attachments;

	public IntVec2? sizeIncludingAttachments;

	public string uiIconPath;

	[MustTranslate]
	public string monolithLabel;

	[MustTranslate]
	public string monolithDescription;

	[MustTranslate]
	public string levelInspectText;

	[MustTranslate]
	public string extraQuestDescription;

	[MustTranslate]
	public string activateGizmoText;

	[MustTranslate]
	public string activateFloatMenuText;

	[MustTranslate]
	public string activateGizmoDescription;

	[MustTranslate]
	public string pawnSentToActivateMessage;

	[MustTranslate]
	public string monolithCanBeActivatedText;

	[MustTranslate]
	public string activateQuestText;

	[MustTranslate]
	public string activatableLetterLabel;

	[MustTranslate]
	public string activatableLetterText;

	[MustTranslate]
	public string activatedLetterText;

	private Texture2D uiIcon;

	public Texture2D UIIcon => uiIcon;

	public override void PostLoad()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			if (!uiIconPath.NullOrEmpty())
			{
				uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
			}
		});
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (entityCatagoryCompletionRequired != null && entityCountCompletionRequired <= 0)
		{
			yield return "entityCatagoryCompletionRequired is set but entityCountCompletionRequired is not";
		}
	}
}
