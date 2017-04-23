using UnityEngine;
using System.Collections;
using System;

public class Grid<TileType> : MonoBehaviour where TileType : MonoBehaviour {
	public TileType prefab;
	
	protected int columnCount;
	protected int rowCount;

	protected TileType[,] tileGrid;
	protected int tileCount;

	protected class Coordinates {
		public int x, y;

		public Coordinates(int _x, int _y) {
			x = _x;
			y = _y;
		}
	}
	


	public virtual void Awake () {
		tileGrid = new TileType[columnCount, rowCount];
		tileCount = 0;
	}

	public virtual void Start () {
		CreateTiles();
	}


	//SETUP GRID

	private void CreateTiles () {
		for (int x = 0; x < rowCount; x++) {
			for (int y = 0; y < columnCount; y++) {
				TileType tNew = (TileType)Instantiate(prefab, new Vector3(0.5f + y - columnCount / 2, 0.5f + x - rowCount / 2), transform.rotation);

				tNew.name = x.ToString() + " " + y.ToString();
				tileGrid[y, x] = tNew;
				tNew.transform.parent = this.transform;
				tileCount++;
			}
		}
	}
	

	//GRID MANIPULATION

	protected Coordinates getGridPosition (TileType originTile) {
		for (int x = 0; x < rowCount; x++) {
			for (int y = 0; y < columnCount; y++) {
				if (originTile == tileGrid[y, x]) {
					return new Coordinates(y, x);
				}
			}
		}
		return new Coordinates(0, 0);
	}

	protected TileType[] AdjacentTiles (TileType originTile) {
		TileType[] tileset = new TileType[9];

		Coordinates pos = getGridPosition(originTile);
		int tileCount = 0;

		for (int x = -1; x <= +1; x++) {
			for (int y = -1; y <= +1; y++) {
				int checkX = pos.x + x;
				int checkY = pos.y + y;

				//clamp to grid bounds
				checkX = Math.Min(Math.Max(checkX, 0), columnCount - 1);
				checkY = Math.Min(Math.Max(checkY, 0), rowCount - 1);

				tileset[tileCount] = tileGrid[checkX, checkY];

				tileCount++;
			}
		}

		return cleanArray(tileset);
	}

	protected void SwapGridTiles (TileType origin, TileType target) {
		Coordinates oPos = getGridPosition(origin);
		Coordinates tPos = getGridPosition(target);

		SwapReferences(ref tileGrid[oPos.x, oPos.y], ref tileGrid[tPos.x, tPos.y]);
	}
	
	
	//ADDITIONAL METHODS

	private TileType[] cleanArray (TileType[] inputArray) {
		ArrayList outList = new ArrayList();

		for (int i = 0; i < inputArray.Length; i++) {
			if (!outList.Contains(inputArray[i])) {
				outList.Add(inputArray[i]);
			}
		}

		TileType[] outArray = new TileType[outList.Count];

		for (int i = 0; i < outList.Count; i++) {
			outArray[i] = (TileType)outList[i];
		}
		return outArray;
	}

	private void SwapReferences<T> (ref T swap1, ref T swap2) {
		T swapVar = swap1;
		swap1 = swap2;
		swap2 = swapVar;
	}
}
