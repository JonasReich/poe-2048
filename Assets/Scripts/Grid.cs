//-------------------------
// (c) 2017, Jonas Reich
//-------------------------

using UnityEngine;
using System.Collections;
using System;

public class Grid<TTileType> : MonoBehaviour where TTileType : MonoBehaviour
{
	public TTileType Prefab;

	protected int ColumnCount;
	protected int RowCount;

	protected TTileType[,] TileGrid;
	protected int TileCount;

	protected class Coordinates
	{
		public int X, Y;

		public Coordinates(int x, int y)
		{
			X = x;
			Y = y;
		}
	}



	public virtual void Awake()
	{
		TileGrid = new TTileType[ColumnCount, RowCount];
		TileCount = 0;
	}

	public virtual void Start()
	{
		CreateTiles();
	}


	//SETUP GRID

	void CreateTiles()
	{
		for (int x = 0; x < RowCount; x++)
		{
			for (int y = 0; y < ColumnCount; y++)
			{
				TTileType tNew = Instantiate(Prefab, new Vector3(0.5f + y - ColumnCount / 2.0f, 0.5f + x - RowCount / 2.0f), transform.rotation);

				tNew.name = x + " " + y;
				TileGrid[y, x] = tNew;
				tNew.transform.parent = this.transform;
				TileCount++;
			}
		}
	}


	//GRID MANIPULATION

	protected Coordinates GetGridPosition(TTileType originTile)
	{
		for (int x = 0; x < RowCount; x++)
			for (int y = 0; y < ColumnCount; y++)
				if (originTile == TileGrid[y, x])
					return new Coordinates(y, x);

		return new Coordinates(0, 0);
	}

	protected TTileType[] AdjacentTiles(TTileType originTile)
	{
		TTileType[] tileset = new TTileType[9];

		Coordinates pos = GetGridPosition(originTile);
		int tileCount = 0;

		for (int x = -1; x <= +1; x++)
			for (int y = -1; y <= +1; y++)
			{
				int checkX = pos.X + x;
				int checkY = pos.Y + y;

				//clamp to grid bounds
				checkX = Math.Min(Math.Max(checkX, 0), ColumnCount - 1);
				checkY = Math.Min(Math.Max(checkY, 0), RowCount - 1);

				tileset[tileCount] = TileGrid[checkX, checkY];

				tileCount++;
			}

		return CleanArray(tileset);
	}

	protected void SwapGridTiles(TTileType origin, TTileType target)
	{
		Coordinates oPos = GetGridPosition(origin);
		Coordinates tPos = GetGridPosition(target);

		SwapReferences(ref TileGrid[oPos.X, oPos.Y], ref TileGrid[tPos.X, tPos.Y]);
	}


	//ADDITIONAL METHODS

	static TTileType[] CleanArray(TTileType[] inputArray)
	{
		ArrayList outList = new ArrayList();

		foreach (TTileType t in inputArray)
			if (!outList.Contains(t))
				outList.Add(t);

		TTileType[] outArray = new TTileType[outList.Count];

		for (int i = 0; i < outList.Count; i++)
			outArray[i] = (TTileType)outList[i];

		return outArray;
	}

	static void SwapReferences<T>(ref T swap1, ref T swap2)
	{
		T swapVar = swap1;
		swap1 = swap2;
		swap2 = swapVar;
	}
}
