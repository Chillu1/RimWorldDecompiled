using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_BuildCubeSculpture : JobDriver
{
	private enum CubeMaterial
	{
		Dirt,
		Stone,
		Sand,
		Scrap
	}

	private CubeMaterial cubeMaterial = CubeMaterial.Scrap;

	private float workDone;

	private const TargetIndex Target = TargetIndex.A;

	private const int BuildTimeTicks = 5000;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	public override string GetReport()
	{
		if (base.CurToilIndex == 0)
		{
			return "ReportBuildingCubeEnroute".Translate();
		}
		return job.def.reportString.Formatted(GetMaterialString());
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil toil = Toils_Goto.Goto(TargetIndex.A, PathEndMode.OnCell);
		toil.AddFinishAction(CacheGroundType);
		yield return toil;
		Toil toil2 = Toils_General.Wait(5000);
		toil2.WithProgressBar(TargetIndex.A, () => workDone / 5000f);
		toil2.WithEffect(GetSculptureEffecter, TargetIndex.A);
		toil2.activeSkill = () => SkillDefOf.Construction;
		toil2.handlingFacing = true;
		toil2.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceTarget(base.TargetA);
			float num = 1.7f;
			if (pawn.skills != null)
			{
				pawn.skills.Learn(SkillDefOf.Construction, 0.25f * (float)delta);
				if (!pawn.skills.GetSkill(SkillDefOf.Construction).TotallyDisabled)
				{
					num *= pawn.GetStatValue(StatDefOf.ConstructionSpeed);
				}
			}
			if (pawn.Stuff != null)
			{
				num *= pawn.Stuff.GetStatValueAbstract(StatDefOf.ConstructionSpeedFactor);
			}
			workDone += num * (float)delta;
			if (workDone >= 5000f)
			{
				ReadyForNextToil();
			}
		};
		yield return toil2;
		yield return Toils_General.Do(PlaceAndFinish);
	}

	private void CacheGroundType()
	{
		TerrainDef terrainDef = pawn.MapHeld.terrainGrid.TerrainAt(base.TargetA.Cell);
		if (terrainDef == null)
		{
			cubeMaterial = CubeMaterial.Scrap;
		}
		else if (terrainDef.categoryType == TerrainDef.TerrainCategoryType.Soil)
		{
			cubeMaterial = CubeMaterial.Dirt;
		}
		else if (terrainDef.categoryType == TerrainDef.TerrainCategoryType.Stone)
		{
			cubeMaterial = CubeMaterial.Stone;
		}
		else if (terrainDef.categoryType == TerrainDef.TerrainCategoryType.Sand)
		{
			cubeMaterial = CubeMaterial.Sand;
		}
		else
		{
			cubeMaterial = CubeMaterial.Scrap;
		}
	}

	private void PlaceAndFinish()
	{
		Thing thing = GenSpawn.Spawn(GetSculptureThing(), base.TargetA.Cell, pawn.MapHeld);
		thing.SetFaction(Faction.OfPlayer);
		CompQuality compQuality = thing.TryGetComp<CompQuality>();
		if (compQuality != null)
		{
			compQuality.SetQuality(QualityUtility.GenerateQualityCreatedByPawn(pawn, SkillDefOf.Construction, consumeInspiration: false), ArtGenerationContext.Colony);
			QualityUtility.SendCraftNotification(thing, pawn);
		}
		CompArt compArt = thing.TryGetComp<CompArt>();
		if (compArt != null)
		{
			if (compQuality == null)
			{
				compArt.InitializeArt(ArtGenerationContext.Colony);
			}
			compArt.JustCreatedBy(pawn);
		}
		if (pawn.health.hediffSet.TryGetHediff<Hediff_CubeInterest>(out var hediff))
		{
			hediff.Notify_BuiltSculpture(thing);
		}
		if (pawn.InMentalState && pawn.MentalStateDef == MentalStateDefOf.CubeSculpting)
		{
			pawn.MentalState.RecoverFromState();
		}
	}

	private Thing GetSculptureThing()
	{
		ThingDef def = null;
		if (cubeMaterial == CubeMaterial.Dirt)
		{
			def = ThingDefOf.DirtCubeSculpture;
		}
		else if (cubeMaterial == CubeMaterial.Stone)
		{
			def = ThingDefOf.StoneCubeSculpture;
		}
		else if (cubeMaterial == CubeMaterial.Sand)
		{
			def = ThingDefOf.SandCubeSculpture;
		}
		else if (cubeMaterial == CubeMaterial.Scrap)
		{
			def = ThingDefOf.ScrapCubeSculpture;
		}
		return ThingMaker.MakeThing(def);
	}

	private EffecterDef GetSculptureEffecter()
	{
		EffecterDef result = null;
		if (cubeMaterial == CubeMaterial.Dirt)
		{
			result = EffecterDefOf.ConstructDirt;
		}
		else if (cubeMaterial == CubeMaterial.Stone)
		{
			result = EffecterDefOf.ConstructDirt;
		}
		else if (cubeMaterial == CubeMaterial.Sand)
		{
			result = EffecterDefOf.ConstructDirt;
		}
		else if (cubeMaterial == CubeMaterial.Scrap)
		{
			result = EffecterDefOf.ConstructMetal;
		}
		return result;
	}

	private string GetMaterialString()
	{
		return cubeMaterial switch
		{
			CubeMaterial.Dirt => "CubeMaterialDirt".Translate(), 
			CubeMaterial.Stone => "CubeMaterialStone".Translate(), 
			CubeMaterial.Sand => "CubeMaterialSand".Translate(), 
			CubeMaterial.Scrap => "CubeMaterialScrap".Translate(), 
			_ => "CubeMaterialScrap".Translate(), 
		};
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref cubeMaterial, "cubeMaterial", CubeMaterial.Dirt);
		Scribe_Values.Look(ref workDone, "workDone", 0f);
	}
}
