using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class CompStatue : CompArt
	{
		public class SavedHediffProps : IExposable
		{
			public BodyPartRecord bodyPart;

			public HediffDef hediffDef;

			public float severity;

			public SavedHediffProps(BodyPartRecord bodyPart, HediffDef hediffDef, float severity)
			{
				this.bodyPart = bodyPart;
				this.hediffDef = hediffDef;
				this.severity = severity;
			}

			public SavedHediffProps()
			{
			}

			public void ExposeData()
			{
				Scribe_BodyParts.Look(ref bodyPart, "bodyPart");
				Scribe_Defs.Look(ref hediffDef, "hediffDef");
				Scribe_Values.Look(ref severity, "severity", 0f);
			}
		}

		private static readonly SimpleCurve sculptureTargetWeightByOpinionCurve = new SimpleCurve(new CurvePoint[3]
		{
			new CurvePoint(-100f, 1f),
			new CurvePoint(0f, 2f),
			new CurvePoint(100f, 10f)
		});

		private static List<Pawn> tmpPawnsToSelectFrom = new List<Pawn>();

		private Pawn fakePawn;

		private BodyTypeDef bodyType;

		private HeadTypeDef headType;

		private HairDef hairDef;

		private XenotypeDef xenotype;

		public BeardDef beard;

		private Color hairColor;

		private List<ThingDef> apparel = new List<ThingDef>();

		private Dictionary<int, ThingStyleDef> apparelStyles = new Dictionary<int, ThingStyleDef>();

		private List<GeneDef> nonXenotypeGenes = new List<GeneDef>();

		private List<SavedHediffProps> hediffsWhichAffectRendering = new List<SavedHediffProps>();

		private Name name;

		private Gender gender;

		private int lifestageIndex;

		private int descriptionSeed;

		private Dictionary<string, object> additionalSavedPawnDataForMods = new Dictionary<string, object>();

		public override bool Active => fakePawn != null;

		public new CompProperties_Statue Props => (CompProperties_Statue)props;

		public Graphic StatueBaseGraphic => Props.statueBaseGraphic.GraphicColoredFor(parent);

		public static List<Pawn> GetAllPossiblePawnsToSelectFrom(Pawn crafter)
		{
			tmpPawnsToSelectFrom.Clear();
			tmpPawnsToSelectFrom.AddRange(SocialCardUtility.PawnsForSocialInfo(crafter));
			tmpPawnsToSelectFrom.AddRange(crafter.relations.RelatedPawns.Where((Pawn p) => p.RaceProps.Humanlike && !p.Spawned));
			return tmpPawnsToSelectFrom;
		}

		public static Thing GenerateSpecificStatue(Pawn crafter, Pawn subject)
		{
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Statue, ThingDefOf.Steel);
			CompStatue compStatue = thing.TryGetComp<CompStatue>();
			compStatue.CreateSnapshotOfPawn(subject);
			compStatue.InitFakePawn();
			compStatue.titleInt = compStatue.GenerateTitle(ArtGenerationContext.Colony);
			compStatue.descriptionSeed = Rand.Int;
			subject.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MadeStatueOfMe, crafter);
			return thing;
		}

		protected override void InitializeArtInternal(Thing relatedThing, ArtGenerationContext source)
		{
		}

		private void InitializeAsFactionLeaderStatue()
		{
			List<Pawn> list = Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.FactionLeader).Where(delegate(Pawn pawn)
			{
				if (pawn.RaceProps.Humanlike && !pawn.IsSubhuman)
				{
					Faction faction = pawn.Faction;
					if (faction != null && !faction.temporary)
					{
						return !faction.Hidden;
					}
					return false;
				}
				return false;
			}).ToList();
			if (list.NullOrEmpty())
			{
				Log.Error($"CompStatue at {parent.PositionHeld} could not find any faction leaders to become a statue of. Destroying. This is likely a bug.");
				parent.Destroy();
				return;
			}
			Pawn p = list.RandomElement();
			CreateSnapshotOfPawn(p);
			InitFakePawn();
			titleInt = GenerateTitle(ArtGenerationContext.Outsider);
			descriptionSeed = Rand.Int;
		}

		public override void JustCreatedBy(Pawn pawn)
		{
			base.JustCreatedBy(pawn);
			if (base.CanShowArt)
			{
				Pawn pawn2 = GetAllPossiblePawnsToSelectFrom(pawn).RandomElementByWeight((Pawn p) => sculptureTargetWeightByOpinionCurve.Evaluate(pawn.relations.OpinionOf(p)));
				if (pawn2 != null)
				{
					pawn2.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.MadeStatueOfMe, pawn);
					CreateSnapshotOfPawn(pawn2);
					InitFakePawn();
					titleInt = GenerateTitle(ArtGenerationContext.Colony);
					descriptionSeed = Rand.Int;
				}
			}
		}

		public override void PostPostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
		{
			InitializeAsFactionLeaderStatue();
		}

		private void CreateSnapshotOfPawn(Pawn p)
		{
			bodyType = p.story.bodyType;
			headType = p.story.headType;
			hairDef = p.story.hairDef;
			hairColor = p.story.HairColor;
			beard = p.style?.beardDef;
			name = p.Name;
			gender = p.gender;
			lifestageIndex = p.ageTracker.CurLifeStageIndex;
			xenotype = p.genes.Xenotype;
			apparel.Clear();
			apparelStyles.Clear();
			int num = 0;
			foreach (Apparel item in p.apparel.WornApparel)
			{
				if (!item.def.thingCategories.NotNullAndContains(ThingCategoryDefOf.ApparelUtility) && (!PawnApparelGenerator.IsHeadgear(item.def) || Rand.Bool))
				{
					apparel.Add(item.def);
					if (item.StyleDef != null)
					{
						apparelStyles.Add(num, item.StyleDef);
					}
					num++;
				}
			}
			nonXenotypeGenes.Clear();
			if (ModsConfig.BiotechActive)
			{
				foreach (Gene item2 in p.genes.GenesListForReading)
				{
					if (!xenotype.genes.Contains(item2.def))
					{
						nonXenotypeGenes.Add(item2.def);
					}
				}
			}
			hediffsWhichAffectRendering.Clear();
			foreach (Hediff hediff in p.health.hediffSet.hediffs)
			{
				if (hediff.def.HasDefinedGraphicProperties)
				{
					hediffsWhichAffectRendering.Add(new SavedHediffProps(hediff.Part, hediff.def, hediff.Severity));
				}
			}
			additionalSavedPawnDataForMods.Clear();
			CreateSnapshotOfPawn_HookForMods(p, additionalSavedPawnDataForMods);
		}

		[UsedImplicitly]
		private void CreateSnapshotOfPawn_HookForMods(Pawn p, Dictionary<string, object> dictToStoreDataIn)
		{
		}

		private void InitFakePawn()
		{
			fakePawn = (Pawn)ThingMaker.MakeThing(ThingDefOf.Human);
			PawnComponentsUtility.CreateInitialComponents(fakePawn);
			fakePawn.Name = name;
			fakePawn.gender = gender;
			fakePawn.kindDef = PawnKindDefOf.Colonist;
			if (ModsConfig.BiotechActive)
			{
				fakePawn.genes.SetXenotype(xenotype ?? XenotypeDefOf.Baseliner);
			}
			fakePawn.ageTracker.LockCurrentLifeStageIndex(lifestageIndex);
			fakePawn.story.bodyType = bodyType;
			fakePawn.story.headType = headType;
			fakePawn.story.hairDef = hairDef;
			fakePawn.story.HairColor = hairColor;
			fakePawn.style.beardDef = beard ?? BeardDefOf.NoBeard;
			fakePawn.apparel.DestroyAll();
			for (int i = 0; i < this.apparel.Count; i++)
			{
				ThingDef thingDef = this.apparel[i];
				apparelStyles.TryGetValue(i, out var value);
				Apparel apparel = ((!thingDef.MadeFromStuff) ? ((Apparel)ThingMaker.MakeThing(thingDef)) : ((Apparel)ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef))));
				if (value != null)
				{
					apparel.StyleDef = value;
				}
				fakePawn.apparel.WornApparel.Add(apparel);
			}
			if (!nonXenotypeGenes.NullOrEmpty())
			{
				foreach (GeneDef nonXenotypeGene in nonXenotypeGenes)
				{
					fakePawn.genes.AddGene(nonXenotypeGene, xenogene: true);
				}
			}
			if (!hediffsWhichAffectRendering.NullOrEmpty())
			{
				foreach (SavedHediffProps item in hediffsWhichAffectRendering)
				{
					fakePawn.health.AddHediff(item.hediffDef, item.bodyPart).Severity = item.severity;
				}
			}
			InitFakePawn_HookForMods(fakePawn, additionalSavedPawnDataForMods);
			fakePawn.Drawer.renderer.SetStatue(parent.Stuff);
			fakePawn.Drawer.renderer.SetAllGraphicsDirty();
			fakePawn.Drawer.renderer.EnsureGraphicsInitialized();
			Notify_ColorChanged();
		}

		[UsedImplicitly]
		private void InitFakePawn_HookForMods(Pawn fakePawn, Dictionary<string, object> additionalSavedPawnDataForMods)
		{
		}

		public override TaggedString GenerateImageDescription()
		{
			if (fakePawn == null)
			{
				Log.ErrorOnce($"CompStatue at {parent.PositionHeld} trying to GenerateImageDescription before initializing pawn. This will not work. This message will not appear again for this statue.", parent.thingIDNumber ^ 0x3A2E1B2);
				return null;
			}
			GrammarRequest request = default(GrammarRequest);
			request.Includes.Add(Props.descriptionMaker);
			request.Rules.AddRange(GrammarUtility.RulesForPawn("SUBJECT", fakePawn, request.Constants));
			Rand.PushState();
			Rand.Seed = descriptionSeed;
			string text = GrammarResolver.Resolve("r_statue_description", request, $"statue_description_{name}");
			Rand.PopState();
			return text;
		}

		protected override string GenerateTitle(ArtGenerationContext context)
		{
			GrammarRequest request = default(GrammarRequest);
			request.Includes.Add(Props.nameMaker);
			request.Rules.AddRange(GrammarUtility.RulesForPawn("SUBJECT", fakePawn, request.Constants));
			return GrammarResolver.Resolve("r_statue_name", request, $"statue_name_{name}");
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (text == null)
			{
				return null;
			}
			return string.Concat(text, "\n" + "Subject".Translate() + ": ", name?.ToString());
		}

		public override bool DontDrawParent()
		{
			return true;
		}

		public override void DrawAt(Vector3 drawPos, bool flip = false)
		{
			Vector3 loc = new Vector3(drawPos.x, drawPos.y - 0.03658537f, drawPos.z - 0.15f);
			StatueBaseGraphic.Draw(loc, flip ? parent.Rotation.Opposite : parent.Rotation, parent);
			if (fakePawn == null)
			{
				if (name != null)
				{
					Log.ErrorOnce($"CompStatue at {parent.PositionHeld} has chosen a target pawn but didn't successfully initialize for rendering. This is a bug. The statue will not render. This message will not appear again for this statue.", parent.thingIDNumber ^ 0x3A2F1B2);
					return;
				}
				InitializeAsFactionLeaderStatue();
			}
			float num = fakePawn.ageTracker.CurLifeStage.headSizeFactor ?? 1f;
			float num2 = (1f - num) * 0.5f;
			drawPos += Vector3.back * num2;
			drawPos -= Altitudes.AltIncVect * 0.25f;
			fakePawn.Drawer.renderer.RenderPawnAt(drawPos, Rot4.South, neverAimWeapon: true);
		}

		public override void Notify_ColorChanged()
		{
			if (fakePawn != null)
			{
				fakePawn.Drawer.renderer.SetStatue(parent.Stuff);
				if (!(parent is Building { PaintColorDef: not null } building))
				{
					fakePawn.Drawer.renderer.SetStatuePaintColor(null);
				}
				else
				{
					fakePawn.Drawer.renderer.SetStatuePaintColor(building.PaintColorDef.color);
				}
				fakePawn.Drawer.renderer.SetAllGraphicsDirty();
				fakePawn.Drawer.renderer.EnsureGraphicsInitialized();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look(ref bodyType, "bodyType");
			Scribe_Defs.Look(ref headType, "headType");
			Scribe_Defs.Look(ref hairDef, "hairDef");
			Scribe_Defs.Look(ref xenotype, "xenotype");
			Scribe_Defs.Look(ref beard, "beard");
			Scribe_Collections.Look(ref apparel, "apparel", LookMode.Def);
			Scribe_Collections.Look(ref apparelStyles, "apparelStyles", LookMode.Value, LookMode.Def);
			Scribe_Deep.Look(ref name, "name");
			Scribe_Values.Look(ref gender, "gender", Gender.None);
			Scribe_Values.Look(ref lifestageIndex, "lifestageIndex", 0);
			Scribe_Values.Look(ref hairColor, "hairColor");
			Scribe_Values.Look(ref descriptionSeed, "descriptionSeed", 0);
			Scribe_Collections.Look(ref nonXenotypeGenes, "nonXenotypeGenes", LookMode.Def);
			Scribe_Collections.Look(ref hediffsWhichAffectRendering, "hediffsWhichAffectRendering", LookMode.Deep);
			Scribe_Collections.Look(ref additionalSavedPawnDataForMods, "additionalSavedPawnDataForMods", LookMode.Value, LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (descriptionSeed == 0)
				{
					descriptionSeed = Rand.Int;
				}
				if (additionalSavedPawnDataForMods == null)
				{
					additionalSavedPawnDataForMods = new Dictionary<string, object>();
				}
				if (apparelStyles == null)
				{
					apparelStyles = new Dictionary<int, ThingStyleDef>();
				}
				LongEventHandler.ExecuteWhenFinished(InitFakePawn);
			}
		}
	}
}
