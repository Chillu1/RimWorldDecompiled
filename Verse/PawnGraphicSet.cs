using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class PawnGraphicSet
	{
		public Pawn pawn;

		public Graphic nakedGraphic;

		public Graphic rottingGraphic;

		public Graphic dessicatedGraphic;

		public Graphic packGraphic;

		public DamageFlasher flasher;

		public Graphic headGraphic;

		public Graphic desiccatedHeadGraphic;

		public Graphic skullGraphic;

		public Graphic headStumpGraphic;

		public Graphic desiccatedHeadStumpGraphic;

		public Graphic hairGraphic;

		public List<ApparelGraphicRecord> apparelGraphics = new List<ApparelGraphicRecord>();

		private List<Material> cachedMatsBodyBase = new List<Material>();

		private int cachedMatsBodyBaseHash = -1;

		public static readonly Color RottingColor = new Color(0.34f, 0.32f, 0.3f);

		public static readonly Color DessicatedColorInsect = new Color(0.8f, 0.8f, 0.8f);

		public bool AllResolved => nakedGraphic != null;

		public GraphicMeshSet HairMeshSet
		{
			get
			{
				if (pawn.story.crownType == CrownType.Average)
				{
					return MeshPool.humanlikeHairSetAverage;
				}
				if (pawn.story.crownType == CrownType.Narrow)
				{
					return MeshPool.humanlikeHairSetNarrow;
				}
				Log.Error("Unknown crown type: " + pawn.story.crownType);
				return MeshPool.humanlikeHairSetAverage;
			}
		}

		public List<Material> MatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
		{
			int num = facing.AsInt + 1000 * (int)bodyCondition;
			if (num != cachedMatsBodyBaseHash)
			{
				cachedMatsBodyBase.Clear();
				cachedMatsBodyBaseHash = num;
				if (bodyCondition == RotDrawMode.Fresh)
				{
					cachedMatsBodyBase.Add(nakedGraphic.MatAt(facing));
				}
				else if (bodyCondition == RotDrawMode.Rotting || dessicatedGraphic == null)
				{
					cachedMatsBodyBase.Add(rottingGraphic.MatAt(facing));
				}
				else if (bodyCondition == RotDrawMode.Dessicated)
				{
					cachedMatsBodyBase.Add(dessicatedGraphic.MatAt(facing));
				}
				for (int i = 0; i < apparelGraphics.Count; i++)
				{
					if ((apparelGraphics[i].sourceApparel.def.apparel.shellRenderedBehindHead || apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell) && !PawnRenderer.RenderAsPack(apparelGraphics[i].sourceApparel) && apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead)
					{
						cachedMatsBodyBase.Add(apparelGraphics[i].graphic.MatAt(facing));
					}
				}
			}
			return cachedMatsBodyBase;
		}

		public Material HeadMatAt_NewTemp(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false, bool portrait = false)
		{
			Material material = null;
			switch (bodyCondition)
			{
			case RotDrawMode.Fresh:
				material = ((!stump) ? headGraphic.MatAt(facing) : headStumpGraphic.MatAt(facing));
				break;
			case RotDrawMode.Rotting:
				material = ((!stump) ? desiccatedHeadGraphic.MatAt(facing) : desiccatedHeadStumpGraphic.MatAt(facing));
				break;
			case RotDrawMode.Dessicated:
				if (!stump)
				{
					material = skullGraphic.MatAt(facing);
				}
				break;
			}
			if (material != null)
			{
				if (!portrait && pawn.IsInvisible())
				{
					material = InvisibilityMatPool.GetInvisibleMat(material);
				}
				material = flasher.GetDamagedMat(material);
			}
			return material;
		}

		[Obsolete("Only need this overload to not break mod compatibility.")]
		public Material HeadMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false)
		{
			return HeadMatAt_NewTemp(facing, bodyCondition, stump);
		}

		public Material HairMatAt_NewTemp(Rot4 facing, bool portrait = false)
		{
			Material baseMat = hairGraphic.MatAt(facing);
			if (!portrait && pawn.IsInvisible())
			{
				baseMat = InvisibilityMatPool.GetInvisibleMat(baseMat);
			}
			return flasher.GetDamagedMat(baseMat);
		}

		[Obsolete("Only need this overload to not break mod compatibility.")]
		public Material HairMatAt(Rot4 facing)
		{
			return HairMatAt_NewTemp(facing);
		}

		public PawnGraphicSet(Pawn pawn)
		{
			this.pawn = pawn;
			flasher = new DamageFlasher(pawn);
		}

		public void ClearCache()
		{
			cachedMatsBodyBaseHash = -1;
		}

		public void ResolveAllGraphics()
		{
			ClearCache();
			if (pawn.RaceProps.Humanlike)
			{
				nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, pawn.story.SkinColor);
				rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, RottingColor);
				dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);
				headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);
				desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, RottingColor);
				skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
				headStumpGraphic = GraphicDatabaseHeadRecords.GetStump(pawn.story.SkinColor);
				desiccatedHeadStumpGraphic = GraphicDatabaseHeadRecords.GetStump(RottingColor);
				hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Transparent, Vector2.one, pawn.story.hairColor);
				ResolveApparelGraphics();
				return;
			}
			PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
			if (pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
			{
				nakedGraphic = curKindLifeStage.bodyGraphicData.Graphic;
			}
			else
			{
				nakedGraphic = curKindLifeStage.femaleGraphicData.Graphic;
			}
			if (pawn.RaceProps.packAnimal)
			{
				packGraphic = GraphicDatabase.Get<Graphic_Multi>(nakedGraphic.path + "Pack", ShaderDatabase.Cutout, nakedGraphic.drawSize, Color.white);
			}
			rottingGraphic = nakedGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, RottingColor, RottingColor);
			if (curKindLifeStage.dessicatedBodyGraphicData != null)
			{
				if (pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid)
				{
					if (pawn.gender != Gender.Female || curKindLifeStage.femaleDessicatedBodyGraphicData == null)
					{
						dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.Graphic.GetColoredVersion(ShaderDatabase.Cutout, DessicatedColorInsect, DessicatedColorInsect);
					}
					else
					{
						dessicatedGraphic = curKindLifeStage.femaleDessicatedBodyGraphicData.Graphic.GetColoredVersion(ShaderDatabase.Cutout, DessicatedColorInsect, DessicatedColorInsect);
					}
				}
				else if (pawn.gender != Gender.Female || curKindLifeStage.femaleDessicatedBodyGraphicData == null)
				{
					dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(pawn);
				}
				else
				{
					dessicatedGraphic = curKindLifeStage.femaleDessicatedBodyGraphicData.GraphicColoredFor(pawn);
				}
			}
			if (pawn.kindDef.alternateGraphics.NullOrEmpty())
			{
				return;
			}
			Rand.PushState(pawn.thingIDNumber ^ 0xB415);
			if (Rand.Value <= pawn.kindDef.alternateGraphicChance)
			{
				nakedGraphic = pawn.kindDef.alternateGraphics.RandomElementByWeight((AlternateGraphic x) => x.Weight).GetGraphic(nakedGraphic);
			}
			Rand.PopState();
		}

		public void SetAllGraphicsDirty()
		{
			if (AllResolved)
			{
				ResolveAllGraphics();
			}
		}

		public void ResolveApparelGraphics()
		{
			ClearCache();
			apparelGraphics.Clear();
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				if (ApparelGraphicRecordGetter.TryGetGraphicApparel(item, pawn.story.bodyType, out var rec))
				{
					apparelGraphics.Add(rec);
				}
			}
		}

		public void SetApparelGraphicsDirty()
		{
			if (AllResolved)
			{
				ResolveApparelGraphics();
			}
		}
	}
}
