using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompCauseGameCondition_PsychicEmanation : CompCauseGameCondition
{
	public Gender gender;

	private int ticksToIncreaseDroneLevel;

	private PsychicDroneLevel droneLevel = PsychicDroneLevel.BadHigh;

	public new CompProperties_CausesGameCondition_PsychicEmanation Props => (CompProperties_CausesGameCondition_PsychicEmanation)props;

	public PsychicDroneLevel Level => droneLevel;

	private bool DroneLevelIncreases => Props.droneLevelIncreaseInterval != int.MinValue;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		gender = (Rand.Bool ? Gender.Male : Gender.Female);
		droneLevel = Props.droneLevel;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && DroneLevelIncreases && !parent.BeingTransportedOnGravship)
		{
			ticksToIncreaseDroneLevel = Props.droneLevelIncreaseInterval;
			SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (parent.Spawned && DroneLevelIncreases && Active)
		{
			ticksToIncreaseDroneLevel--;
			if (ticksToIncreaseDroneLevel <= 0)
			{
				IncreaseDroneLevel();
				ticksToIncreaseDroneLevel = Props.droneLevelIncreaseInterval;
			}
		}
	}

	private void IncreaseDroneLevel()
	{
		if (droneLevel != PsychicDroneLevel.BadExtreme)
		{
			droneLevel++;
			TaggedString text = "LetterPsychicDroneLevelIncreased".Translate(gender.GetLabel());
			Find.LetterStack.ReceiveLetter("LetterLabelPsychicDroneLevelIncreased".Translate(), text, LetterDefOf.NegativeEvent);
			SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
			ReSetupAllConditions();
		}
	}

	protected override void SetupCondition(GameCondition condition, Map map)
	{
		base.SetupCondition(condition, map);
		GameCondition_PsychicEmanation obj = (GameCondition_PsychicEmanation)condition;
		obj.gender = gender;
		obj.level = Level;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref gender, "gender", Gender.None);
		Scribe_Values.Look(ref ticksToIncreaseDroneLevel, "ticksToIncreaseDroneLevel", 0);
		Scribe_Values.Look(ref droneLevel, "droneLevel", PsychicDroneLevel.None);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!Prefs.DevMode || !DebugSettings.godMode)
		{
			yield break;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = gender.GetLabel();
		command_Action.action = delegate
		{
			if (gender == Gender.Female)
			{
				gender = Gender.Male;
			}
			else
			{
				gender = Gender.Female;
			}
			ReSetupAllConditions();
		};
		command_Action.hotKey = KeyBindingDefOf.Misc1;
		yield return command_Action;
		Command_Action command_Action2 = new Command_Action();
		command_Action2.defaultLabel = droneLevel.GetLabel();
		command_Action2.action = delegate
		{
			IncreaseDroneLevel();
			ReSetupAllConditions();
		};
		command_Action2.hotKey = KeyBindingDefOf.Misc2;
		yield return command_Action2;
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		return text + ("AffectedGender".Translate() + ": " + gender.GetLabel().CapitalizeFirst() + "\n" + "PsychicDroneLevel".Translate(droneLevel.GetLabelCap()));
	}

	public override void RandomizeSettings(Site site)
	{
		gender = (Rand.Bool ? Gender.Male : Gender.Female);
		if (site.ActualThreatPoints < 800f)
		{
			droneLevel = PsychicDroneLevel.BadLow;
		}
		else if (site.ActualThreatPoints < 2000f)
		{
			droneLevel = PsychicDroneLevel.BadMedium;
		}
		else
		{
			droneLevel = PsychicDroneLevel.BadHigh;
		}
	}
}
