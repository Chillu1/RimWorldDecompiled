using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class Building_FleshmassHeart : Building
	{
		private const int BeatDurationTicks = 15;

		private static readonly Vector3 PartDrawSize = new Vector3(3.5f, 3.5f, 3.5f);

		private const int RequiredAnalysis = 3;

		private int bpm = 30;

		private int bpmAccel;

		private Lord defendHeartLord;

		private int overloadTick = -99999;

		[Unsaved(false)]
		private int lastBeatTick = -99999;

		[Unsaved(false)]
		private Graphic cachedCenterPartGraphic;

		[Unsaved(false)]
		private Graphic cachedLeftPartGraphic;

		[Unsaved(false)]
		private Graphic cachedRightPartGraphic;

		[Unsaved(false)]
		private CompBiosignatureOwner biosignatureCompInt;

		[Unsaved(false)]
		private List<Pawn> cachedLordPawns;

		private Graphic CenterPartGraphic => cachedCenterPartGraphic ?? (cachedCenterPartGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/Fleshmass/FleshmassHeart/FleshmassHeart_CentralPart", ShaderDatabase.Cutout, PartDrawSize, Color.white));

		private Graphic LeftPartGraphic => cachedLeftPartGraphic ?? (cachedLeftPartGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/Fleshmass/FleshmassHeart/FleshmassHeart_LeftPart", ShaderDatabase.Cutout, PartDrawSize, Color.white));

		private Graphic RightPartGraphic => cachedRightPartGraphic ?? (cachedRightPartGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/Fleshmass/FleshmassHeart/FleshmassHeart_RightPart", ShaderDatabase.Cutout, PartDrawSize, Color.white));

		public CompBiosignatureOwner BiosignatureComp => biosignatureCompInt ?? (biosignatureCompInt = GetComp<CompBiosignatureOwner>());

		public int Biosignature => BiosignatureComp.biosignature;

		public object BiosignatureName => BiosignatureComp.BiosignatureName;

		public CompFleshmassHeart Comp => GetComp<CompFleshmassHeart>();

		public Lord DefendHeartLord
		{
			get
			{
				if (defendHeartLord == null)
				{
					defendHeartLord = LordMaker.MakeNewLord(base.Faction, new LordJob_DefendFleshmassHeart(this), base.Map);
				}
				return defendHeartLord;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref bpmAccel, "bpmAccel", 0);
			Scribe_Values.Look(ref bpm, "bpm", 0);
			Scribe_References.Look(ref defendHeartLord, "defendHeartLord");
			Scribe_Values.Look(ref overloadTick, "overloadTick", 0);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad && !base.BeingTransportedOnGravship)
			{
				Find.AnalysisManager.AddAnalysisTask(Biosignature, 3);
				if (base.Faction != Faction.OfEntities)
				{
					SetFaction(Faction.OfEntities);
				}
				EffecterDefOf.ImpactDustCloud.Spawn(base.Position, base.Map).Cleanup();
			}
		}

		public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
		{
			IntVec3 positionHeld = base.PositionHeld;
			Map mapHeld = base.MapHeld;
			base.Kill(dinfo, exactCulprit);
			if (!GenDrop.TryDropSpawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.FleshmassNucleus, Faction.OfEntities)), positionHeld, mapHeld, ThingPlaceMode.Near, out var resultingThing))
			{
				Log.ErrorOnce("Failed to place fleshmass nucleus", 9685694);
				return;
			}
			TaggedString label = "LetterLabelFleshmassHeartDestroyed".Translate();
			TaggedString text = "LetterFleshmassHeartDestroyed".Translate();
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, resultingThing);
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Map.GetComponent<FleshmassMapComponent>()?.DestroyFleshmass(60000, 1f, destroyInChunks: false, this);
			base.Destroy(mode);
		}

		protected override void Tick()
		{
			base.Tick();
			if ((float)Find.TickManager.TicksGame > (float)lastBeatTick + 60f / (float)bpm * 60f)
			{
				Beat();
			}
			Thing.allowDestroyNonDestroyable = true;
			if (overloadTick > 0 && Find.TickManager.TicksGame > overloadTick)
			{
				Kill();
			}
			Thing.allowDestroyNonDestroyable = false;
		}

		private void Beat()
		{
			lastBeatTick = Find.TickManager.TicksGame;
			bpm += bpmAccel;
			SoundDefOf.FleshmassHeart_Throb.PlayOneShot(this);
		}

		public void StartTachycardiacOverload()
		{
			bpm *= 2;
			bpmAccel = 15;
			overloadTick = Find.TickManager.TicksGame + EffecterDefOf.TachycardiacArrest.maintainTicks;
			EffecterDefOf.TachycardiacArrest.SpawnMaintained(base.Position, base.Map);
			Messages.Message("MessageHeartAttack".Translate(), this, MessageTypeDefOf.PositiveEvent);
		}

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			Matrix4x4 matrix = Matrix4x4.TRS(drawLoc + new Vector3(0f, 0.35f, -0.35f), Quaternion.AngleAxis(0f, Vector3.up), new Vector3(GetBeatScale(1.15f), 1f, GetBeatScale(1.2f)));
			GenDraw.DrawMeshNowOrLater(CenterPartGraphic.MeshAt(Rot4.South), matrix, CenterPartGraphic.MatSouth, drawNow: false);
			matrix = Matrix4x4.TRS(drawLoc + new Vector3(-0.875f, 0.3f, 0f), Quaternion.AngleAxis(30f, Vector3.up), new Vector3(GetBeatScale(1.1f, 3), 1f, GetBeatScale(1.1f, 3)));
			GenDraw.DrawMeshNowOrLater(LeftPartGraphic.MeshAt(Rot4.South), matrix, LeftPartGraphic.MatSouth, drawNow: false);
			matrix = Matrix4x4.TRS(drawLoc + new Vector3(0.875f, 0.3f, 0f), Quaternion.AngleAxis(10f, Vector3.up), new Vector3(GetBeatScale(1.1f, 7), 1f, GetBeatScale(1.1f, 7)));
			GenDraw.DrawMeshNowOrLater(RightPartGraphic.MeshAt(Rot4.South), matrix, RightPartGraphic.MatSouth, drawNow: false);
		}

		private float GetBeatScale(float maxScale, int delay = 0)
		{
			float num = (float)(Find.TickManager.TicksGame - (lastBeatTick + delay)) / 15f;
			if (num < 1f)
			{
				return Mathf.Lerp(1f, maxScale, Mathf.Sin(MathF.PI * num));
			}
			return 1f;
		}

		public override void PreSwapMap()
		{
			base.PreSwapMap();
			cachedLordPawns = new List<Pawn>();
			if (defendHeartLord != null)
			{
				cachedLordPawns.AddRange(defendHeartLord.ownedPawns);
			}
		}

		public override void PostSwapMap()
		{
			base.PostSwapMap();
			defendHeartLord?.RemovePawns(cachedLordPawns);
			cachedLordPawns.RemoveAll((Pawn p) => !p.Spawned);
			defendHeartLord = null;
			DefendHeartLord.AddPawns(cachedLordPawns);
			cachedLordPawns = null;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLineIfNotEmpty().AppendLine("FleshmassHeartInvulnerable".Translate());
			stringBuilder.AppendLine(base.GetInspectString());
			stringBuilder.Append((Comp.GrowthPoints > 0) ? "FleshmassHeartGrowing".Translate() : "FleshmassHeartCollecting".Translate());
			return stringBuilder.ToString();
		}
	}
}
