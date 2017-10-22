//-------------------------
// (c) 2017, Jonas Reich
//-------------------------

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NONE
{

	public class GameGrid : Grid<GameTile>
	{
		public int GridSize, Score;

		public enum Direction { Left, Right, Up, Down };

		Vector2 _firstPressPos;
		bool _moveInLastFrame;
		List<GameTile> _emptyTiles;

		List<GameTile> EmptyTiles
		{
			get
			{
				_emptyTiles.Clear();
				foreach (GameTile gc in TileGrid)
					if (gc.Value == 0)
						_emptyTiles.Add(gc);

				return _emptyTiles;
			}
		}


		new void Awake()
		{
			RowCount = ColumnCount = GridSize;
			base.Awake();
			_moveInLastFrame = false;

			_emptyTiles = new List<GameTile>();
		}

		new void Start()
		{
			base.Start();
			foreach (GameTile t in TileGrid)
				t.MoveTargetPosition = t.transform.position = GridCoordniatesToWorldPosition(t.X, t.Y);
			Restart();
		}

		public void Restart()
		{
			foreach (GameTile gt in TileGrid)
				gt.Kill();

			PlaceRandomTile();
			PlaceRandomTile();

			global::Score.Clear();
		}

		void Update()
		{
			if (_moveInLastFrame && AreTilesMoving() == false)
				MoveEnd();

			CheckKeyboardInput();
			CheckTouchInput();

			_moveInLastFrame = AreTilesMoving();
		}


		//BASIC GRID FUNCTIONS

		void PlaceRandomTile()
		{
			if (!IsGridFull())
			{
				GameTile randomTile = (GameTile)EmptyTiles[UnityEngine.Random.Range(0, EmptyTiles.Count - 1)];
				randomTile.Spawn();
			}

		}

		bool IsGridFull()
		{
			foreach (GameTile gt in TileGrid)
				if (gt.Value == 0)
					return false;

			return true;
		}


		//MOVE TILES

		void MoveStart(Direction dir)
		{
			//end any remaining moves before starting a new one
			if (AreTilesMoving())
				MoveEnd();

			for (int i = 0; i < ColumnCount; i++)
				MoveInternally(dir);

			MoveExternally();
		}

		void MoveInternally(Direction dir)
		{
			foreach (TilePair tp in ReturnFromDirection(dir))
			{
				if (tp.Origin.Value == 0) continue;
				if (tp.Target.Value == 0)
				{
					SwapGridTiles(tp.Origin, tp.Target);
				}
				else if (tp.Origin.Value == tp.Target.Value
					&& tp.Target.IsMergeOrigin == false && tp.Origin.IsMergeOrigin == false
					&& tp.Origin.IsMergeTarget == false && tp.Target.IsMergeTarget == false)
				{
					tp.Origin.IsMergeOrigin = true;
					tp.Origin.MergeTarget = tp.Target;
					tp.Target.IsMergeTarget = true;
					tp.Target.PreKill();
					SwapGridTiles(tp.Origin, tp.Target);
				}
			}
		}

		void MoveExternally()
		{
			for (int x = 0; x < ColumnCount; x++)
				for (int y = 0; y < RowCount; y++)
					TileGrid[x, y].MoveTargetPosition = GridCoordniatesToWorldPosition(x, y);
		}

		void MoveEnd()
		{
			for (int x = 0; x < ColumnCount; x++)
				for (int y = 0; y < RowCount; y++)
				{
					GameTile gt = TileGrid[x, y];
					if (gt.IsMergeOrigin)
						gt.Merge();
					if (gt.IsMergeTarget)
						gt.Kill();
					gt.X = x;
					gt.Y = y;
					gt.MoveTargetPosition = gt.transform.position = GridCoordniatesToWorldPosition(x, y);
					gt.MergeTarget = null;
					gt.IsMergeOrigin = false;
					gt.IsMergeTarget = false;
				}

			PlaceRandomTile();
		}

		bool AreTilesMoving()
		{
			foreach (GameTile gt in TileGrid)
				if (gt.Moving)
					return true;

			return false;
		}


		//GRID ITERATORS

		class TilePair
		{
			public GameTile Origin;
			public GameTile Target;

			public TilePair(GameTile origin, GameTile target)
			{
				Origin = origin;
				Target = target;
			}
		}

		public IEnumerable ReturnFromDirection(Direction dir)
		{
			switch (dir)
			{
				case Direction.Down:
					return ReturnFromBottom();
				case Direction.Left:
					return ReturnFromLeft();
				case Direction.Right:
					return ReturnFromRight();
				case Direction.Up:
					return ReturnFromTop();
				default:
					return null;
			}
		}

		IEnumerable ReturnFromLeft()
		{
			for (int x = 1; x < ColumnCount; x++)
				for (int y = 0; y < RowCount; y++)
					yield return new TilePair(TileGrid[x, y], TileGrid[x - 1, y]);
		}

		IEnumerable ReturnFromRight()
		{
			for (int x = ColumnCount - 2; x >= 0; x--)
				for (int y = 0; y < RowCount; y++)
					yield return new TilePair(TileGrid[x, y], TileGrid[x + 1, y]);
		}

		IEnumerable ReturnFromTop()
		{
			for (int y = RowCount - 2; y >= 0; y--)
				for (int x = 0; x < ColumnCount; x++)
					yield return new TilePair(TileGrid[x, y], TileGrid[x, y + 1]);
		}

		IEnumerable ReturnFromBottom()
		{
			for (int y = 1; y < RowCount; y++)
				for (int x = 0; x < ColumnCount; x++)
					yield return new TilePair(TileGrid[x, y], TileGrid[x, y - 1]);
		}


		//INPUT

		void CheckKeyboardInput()
		{
			if (Input.GetKeyDown(KeyCode.LeftArrow))
				MoveStart(Direction.Left);
			if (Input.GetKeyDown(KeyCode.RightArrow))
				MoveStart(Direction.Right);
			if (Input.GetKeyDown(KeyCode.DownArrow))
				MoveStart(Direction.Down);
			if (Input.GetKeyDown(KeyCode.UpArrow))
				MoveStart(Direction.Up);
		}

		const float SwipeDeadZoneRadius = 0.8f;
		const float MinimumSwipeLength = 1;

		void CheckTouchInput()
		{

			if (Input.touches.Length <= 0) return;

			Touch t = Input.GetTouch(0);
			switch (t.phase)
			{
				case TouchPhase.Began:
					_firstPressPos = Camera.main.ScreenToWorldPoint(t.position);
					break;
				case TouchPhase.Ended:
					{
						Vector3 secondPressPos = Camera.main.ScreenToWorldPoint(t.position);

						Vector3 currentSwipe = new Vector3(secondPressPos.x - _firstPressPos.x, secondPressPos.y - _firstPressPos.y);

						if (currentSwipe.magnitude <= MinimumSwipeLength) return;

						currentSwipe.Normalize();

						if (IsSwipe(currentSwipe.y, currentSwipe.x))
							MoveStart(Direction.Up);

						if (IsSwipe(currentSwipe.y * -1, currentSwipe.x))
							MoveStart(Direction.Down);

						if (IsSwipe(currentSwipe.x * -1, currentSwipe.y))
							MoveStart(Direction.Left);

						if (IsSwipe(currentSwipe.x, currentSwipe.y))
							MoveStart(Direction.Right);
					}
					break;
			}
		}

		bool IsSwipe(float correctAxis, float wrongAxis)
		{
			return correctAxis > 0 && wrongAxis > -SwipeDeadZoneRadius && wrongAxis < SwipeDeadZoneRadius;
		}
	}
}
