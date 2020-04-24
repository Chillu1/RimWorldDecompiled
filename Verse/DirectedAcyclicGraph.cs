using System.Collections.Generic;

namespace Verse
{
	public class DirectedAcyclicGraph
	{
		private int numVertices;

		private List<List<int>> adjacencyList = new List<List<int>>();

		public DirectedAcyclicGraph(int numVertices)
		{
			this.numVertices = numVertices;
			for (int i = 0; i < numVertices; i++)
			{
				adjacencyList.Add(new List<int>());
			}
		}

		public void AddEdge(int from, int to)
		{
			adjacencyList[from].Add(to);
		}

		public List<int> TopologicalSort()
		{
			bool[] array = new bool[numVertices];
			for (int i = 0; i < numVertices; i++)
			{
				array[i] = false;
			}
			List<int> result = new List<int>();
			for (int j = 0; j < numVertices; j++)
			{
				if (!array[j])
				{
					TopologicalSortInner(j, array, result);
				}
			}
			return result;
		}

		private void TopologicalSortInner(int v, bool[] visited, List<int> result)
		{
			visited[v] = true;
			foreach (int item in adjacencyList[v])
			{
				if (!visited[item])
				{
					TopologicalSortInner(item, visited, result);
				}
			}
			result.Add(v);
		}

		public bool IsCyclic()
		{
			return FindCycle() != -1;
		}

		public int FindCycle()
		{
			bool[] array = new bool[numVertices];
			bool[] array2 = new bool[numVertices];
			for (int i = 0; i < numVertices; i++)
			{
				array[i] = false;
				array2[i] = false;
			}
			for (int j = 0; j < numVertices; j++)
			{
				if (IsCyclicInner(j, array, array2))
				{
					return j;
				}
			}
			return -1;
		}

		private bool IsCyclicInner(int v, bool[] visited, bool[] history)
		{
			visited[v] = true;
			history[v] = true;
			foreach (int item in adjacencyList[v])
			{
				if (!visited[item] && IsCyclicInner(item, visited, history))
				{
					return true;
				}
				if (history[item])
				{
					return true;
				}
			}
			history[v] = false;
			return false;
		}
	}
}
