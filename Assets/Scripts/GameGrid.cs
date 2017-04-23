using UnityEngine;
using System.Collections;

namespace NONE
{

	public class GameGrid : Grid<GameTile>
	{
		public int gridSize, score;

		public enum Direction { left, right, up, down };

		private Vector2 firstPressPos, secondPressPos, currentSwipe;
		private bool victory, moveInLastFrame;
		private ArrayList _emptyTiles;
		private ArrayList emptyTiles
		{
			get
			{
				_emptyTiles.Clear();
				foreach (GameTile gc in tileGrid)
				{
					if (gc.index == 0)
						_emptyTiles.Add(gc);
				}
				return _emptyTiles;
			}
		}


		new void Awake ()
		{
			rowCount = columnCount = gridSize;
			base.Awake();
			victory = moveInLastFrame = false;

			_emptyTiles = new ArrayList();
		}

		new void Start ()
		{
			base.Start();
			MoveEnd();
			Restart();
		}

		public void Restart ()
		{
			foreach (GameTile gt in tileGrid)
			{
				gt.Kill();
			}
			PlaceRandomTile();
			PlaceRandomTile();

			Score.Clear();
		}

		void Update ()
		{
			if (moveInLastFrame && AreTilesMoving() == false)
			{
				MoveEnd();
			}

			CheckKeyboardInput();
			CheckTouchInput();

			moveInLastFrame = AreTilesMoving();
		}


		//BASIC GRID FUNCTIONS

		private int[,] CreateValueGrid ()
		{
			int[,] valueGrid = new int[columnCount, rowCount];
			for (int x = 0; x < columnCount; x++)
			{
				for (int y = 0; y < rowCount; y++)
				{
					valueGrid[x, y] = tileGrid[x, y].index;
				}
			}
			return valueGrid;
		}

		private void PlaceRandomTile ()
		{
			if (!IsGridFull())
			{
				ArrayList eT = emptyTiles;
				GameTile randomTile = (GameTile)eT[UnityEngine.Random.Range(0, emptyTiles.Count - 1)];
				randomTile.Spawn();
			}

		}

		private bool IsGridFull ()
		{
			foreach (GameTile gt in tileGrid)
			{
				if (gt.index == 0)
					return false;
			}
			return true;
		}


		//MOVE TILES

		private void MoveStart (Direction dir)
		{
			if (AreTilesMoving())
			{  //end any remaining moves before starting a new one
				MoveEnd();
			}

			for (int i = 0; i < columnCount; i++)
			{
				MoveInternally(dir);
			}

			MoveExternally();
		}

		private void MoveInternally (Direction dir)
		{
			int moveCount = 0;

			foreach (TilePair tp in ReturnFromDirection(dir))
			{
				if (tp.origin.index != 0)
				{
					if (tp.target.index == 0)
					{
						SwapGridTiles(tp.origin, tp.target);
						moveCount++;
					}
					else if (tp.origin.index == tp.target.index && tp.target.isMergeOrigin == false && tp.origin.isMergeOrigin == false)
					{
						tp.origin.isMergeOrigin = true;
						tp.origin.mergeTarget = tp.target;
						tp.target.isMergeTarget = true;
						tp.target.Kill();
						SwapGridTiles(tp.origin, tp.target);
						moveCount++;
					}
				}
			}
		}

		private void MoveExternally ()
		{
			for (int x = 0; x < columnCount; x++)
			{
				for (int y = 0; y < rowCount; y++)
				{
					tileGrid[x, y].moveTargetPosition = new Vector3(x - 1.5f, y - 1.5f);
				}
			}
		}

		private void MoveEnd ()
		{
			for (int x = 0; x < columnCount; x++)
			{
				for (int y = 0; y < rowCount; y++)
				{
					GameTile gt = tileGrid[x, y];
					if (gt.isMergeOrigin)
						gt.Merge();
					gt.x = x;
					gt.y = y;
					gt.moveTargetPosition = gt.transform.position = new Vector3(x - 1.5f, y - 1.5f); //cant be changed with position.Set, because position returns a value
					gt.mergeTarget = null;
					gt.isMergeOrigin = false;
					gt.isMergeTarget = false;
					gt.moving = false;
				}
			}

			PlaceRandomTile();
		}

		private bool AreTilesMoving ()
		{
			foreach (GameTile gt in tileGrid)
			{
				if (gt.moving)
					return true;
			}
			return false;
		}


		//GRID ITERATORS

		private class TilePair
		{
			public GameTile origin;
			public GameTile target;

			public TilePair (GameTile _origin, GameTile _target)
			{
				origin = _origin;
				target = _target;
			}
		}

		public IEnumerable ReturnFromDirection (Direction dir)
		{
			switch (dir)
			{
				case Direction.down:
					return ReturnFromBottom();
				case Direction.left:
					return ReturnFromLeft();
				case Direction.right:
					return ReturnFromRight();
				case Direction.up:
					return ReturnFromTop();
				default:
					return null;
			}
		}

		private IEnumerable ReturnFromLeft ()
		{
			for (int x = 1; x < columnCount; x++)
			{
				for (int y = 0; y < rowCount; y++)
				{
					yield return new TilePair(tileGrid[x, y], tileGrid[x - 1, y]);
				}
			}
		}

		private IEnumerable ReturnFromRight ()
		{
			for (int x = columnCount - 2; x >= 0; x--)
			{
				for (int y = 0; y < rowCount; y++)
				{
					yield return new TilePair(tileGrid[x, y], tileGrid[x + 1, y]);
				}
			}
		}

		private IEnumerable ReturnFromTop ()
		{
			for (int y = rowCount - 2; y >= 0; y--)
			{
				for (int x = 0; x < columnCount; x++)
				{
					yield return new TilePair(tileGrid[x, y], tileGrid[x, y + 1]);
				}
			}
		}

		private IEnumerable ReturnFromBottom ()
		{
			for (int y = 1; y < rowCount; y++)
			{
				for (int x = 0; x < columnCount; x++)
				{
					yield return new TilePair(tileGrid[x, y], tileGrid[x, y - 1]);
				}
			}
		}


		//INPUT

		private void CheckKeyboardInput ()
		{
			if (Input.GetKeyDown(KeyCode.LeftArrow))
				MoveStart(Direction.left);
			if (Input.GetKeyDown(KeyCode.RightArrow))
				MoveStart(Direction.right);
			if (Input.GetKeyDown(KeyCode.DownArrow))
				MoveStart(Direction.down);
			if (Input.GetKeyDown(KeyCode.UpArrow))
				MoveStart(Direction.up);
		}

		private void CheckTouchInput ()
		{
			float swipeLength = 0.8f;

			if (Input.touches.Length > 0)
			{
				Touch t = Input.GetTouch(0);
				if (t.phase == TouchPhase.Began)
				{
					firstPressPos = Camera.main.ScreenToWorldPoint(t.position);
				}
				if (t.phase == TouchPhase.Ended)
				{
					secondPressPos = Camera.main.ScreenToWorldPoint(t.position);

					currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

					if (currentSwipe.magnitude > 1)
					{
						//normalize the 2d vector
						currentSwipe.Normalize();

						//swipe upwards
						if (currentSwipe.y > 0 && currentSwipe.x > -swipeLength && currentSwipe.x < swipeLength)
						{
							MoveStart(Direction.up);
						}
						//swipe down
						if (currentSwipe.y < 0 && currentSwipe.x > -swipeLength && currentSwipe.x < swipeLength)
						{
							MoveStart(Direction.down);
						}
						//swipe left
						if (currentSwipe.x < 0 && currentSwipe.y > -swipeLength && currentSwipe.y < swipeLength)
						{
							MoveStart(Direction.left);
						}
						//swipe right
						if (currentSwipe.x > 0 && currentSwipe.y > -swipeLength && currentSwipe.y < swipeLength)
						{
							MoveStart(Direction.right);
						}
					}
				}
			}
		}
	}
}
