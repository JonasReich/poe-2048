using UnityEngine;

public class GameTile : MonoBehaviour
{
	public int X, Y;
	public float Speed, Scale;
	internal bool IsMergeOrigin, IsMergeTarget;

	internal bool Moving {get { return transform.position != _moveTargetPosition; }}
	internal GameTile MergeTarget;

	/// <summary>
	/// Value of the individual tile 2, 4, 8, ...
	/// </summary>
	public int Value { get; private set; }

	Vector3 _moveTargetPosition;
	public Vector3 MoveTargetPosition
	{
		set
		{
			if (value == _moveTargetPosition) return;
			_moveTargetPosition = value;
		}
	}

	Animator _animator;


	void Awake()
	{
		Scale = 0.5f;

		Value = X = Y = 0;
		IsMergeOrigin = IsMergeTarget = false;
		MergeTarget = null;

		_animator = GetComponent<Animator>();
	}

	void Update()
	{
		if (!IsMergeTarget)
			_animator.SetInteger("value", Value);

		transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(Scale, Scale, Scale), 10 * Time.deltaTime);

		if (transform.position != _moveTargetPosition && IsMergeTarget == false)
			transform.position = Vector3.MoveTowards(transform.position, _moveTargetPosition, Speed * Time.deltaTime);
	}


	void OnTriggerEnter2D(Collider2D other)
	{
		if (IsMergeOrigin && other.gameObject.GetComponent<GameTile>().IsMergeTarget)
		{
			IsMergeOrigin = false;
			MergeTarget.IsMergeTarget = false;
			Merge();
			other.GetComponent<GameTile>().Kill();
		}
	}

	public void PreKill()
	{
		Value = 0;
		//Scale = 0.5f;
	}

	public void Kill()
	{
		Scale = 0.5f;
		_animator.SetTrigger("kill");
	}

	public void Spawn()
	{
		_animator.SetTrigger("spawn");
		Value = (UnityEngine.Random.value > 0.5) ? 2 : 4;
		Scale = 1f;
	}

	public void Merge()
	{
		Value *= 2;
		if (Value == 2048)
			Score.Victory();
		_animator.SetTrigger("merge");
		transform.localScale = new Vector3(1.5f, 1.5f);

		Score.Raise(Value);
	}
}
