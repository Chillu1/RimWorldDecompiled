using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class LightningBoltMeshPool
	{
		private static List<Mesh> boltMeshes = new List<Mesh>();

		private const int NumBoltMeshesMax = 20;

		public static Mesh RandomBoltMesh
		{
			get
			{
				if (boltMeshes.Count < 20)
				{
					Mesh mesh = LightningBoltMeshMaker.NewBoltMesh();
					boltMeshes.Add(mesh);
					return mesh;
				}
				return boltMeshes.RandomElement();
			}
		}
	}
}
