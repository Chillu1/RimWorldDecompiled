using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Ship_Pregen : SymbolResolver
	{
		private struct SpawnDescriptor
		{
			public IntVec3 offset;

			public ThingDef def;

			public Rot4 rot;
		}

		public override void Resolve(ResolveParams rp)
		{
			SpawnDescriptor[] array = new SpawnDescriptor[30];
			SpawnDescriptor spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, 0),
				def = ThingDefOf.Ship_Reactor,
				rot = Rot4.North
			};
			array[0] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, 12),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			};
			array[1] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, 6),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			};
			array[2] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-4, 0, -2),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			};
			array[3] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(4, 0, -2),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			};
			array[4] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, -7),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			};
			array[5] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, 16),
				def = ThingDefOf.Ship_SensorCluster,
				rot = Rot4.North
			};
			array[6] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, -7),
				def = ThingDefOf.Ship_ComputerCore,
				rot = Rot4.North
			};
			array[7] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 15),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			};
			array[8] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 13),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			};
			array[9] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 11),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			};
			array[10] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 9),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			};
			array[11] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 7),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			};
			array[12] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 5),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			};
			array[13] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-4, 0, 2),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.North
			};
			array[14] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-5, 0, 1),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			};
			array[15] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 15),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			};
			array[16] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 13),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			};
			array[17] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 11),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			};
			array[18] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 9),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			};
			array[19] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 7),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			};
			array[20] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 5),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			};
			array[21] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(5, 0, 2),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.North
			};
			array[22] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(6, 0, 1),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			};
			array[23] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-6, 0, -3),
				def = ThingDefOf.Ship_Engine,
				rot = Rot4.North
			};
			array[24] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(7, 0, -3),
				def = ThingDefOf.Ship_Engine,
				rot = Rot4.North
			};
			array[25] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(3, 0, -11),
				def = ThingDefOf.Ship_Engine,
				rot = Rot4.North
			};
			array[26] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-2, 0, -11),
				def = ThingDefOf.Ship_Engine,
				rot = Rot4.North
			};
			array[27] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, -8),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			};
			array[28] = spawnDescriptor;
			spawnDescriptor = new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, -6),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			};
			array[29] = spawnDescriptor;
			IntVec3 centerCell = rp.rect.CenterCell;
			IntVec3 b = new IntVec3(-1, 0, -3);
			SpawnDescriptor[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				SpawnDescriptor spawnDescriptor2 = array2[i];
				Thing thing = ThingMaker.MakeThing(spawnDescriptor2.def);
				thing.SetFaction(rp.faction);
				if (rp.hpPercentRange.HasValue)
				{
					thing.HitPoints = Mathf.Clamp(Mathf.RoundToInt((float)thing.MaxHitPoints * rp.hpPercentRange.Value.RandomInRange), 1, thing.MaxHitPoints);
					GenLeaving.DropFilthDueToDamage(thing, thing.MaxHitPoints - thing.HitPoints);
				}
				CompHibernatable compHibernatable = thing.TryGetComp<CompHibernatable>();
				if (compHibernatable != null)
				{
					compHibernatable.State = HibernatableStateDefOf.Hibernating;
				}
				GenSpawn.Spawn(thing, centerCell + b + spawnDescriptor2.offset, BaseGen.globalSettings.map, spawnDescriptor2.rot);
			}
		}
	}
}
