using UnityEngine;

public class GameTile : MonoBehaviour {
	public int x, y;
	public float speed, scale;
	public bool isMergeOrigin, isMergeTarget, moving;
	public GameTile mergeTarget;

	
	public int _index;
	public int index { get { return _index; } }

	private Vector3 _moveTargetPosition;
	public Vector3 moveTargetPosition {
		set {
			if(value != _moveTargetPosition) {
				_moveTargetPosition = value;
				moving = true;
			}
		}
	}

	private Animator animator;
	

	void Awake() {
		scale = 0.5f;

		_index = x = y = 0;
		isMergeOrigin = isMergeTarget = moving = false;
		mergeTarget = null;

		animator = GetComponent<Animator>();
	}

	void Update () {
		if(!isMergeTarget)
			animator.SetInteger("value", _index);
		
		transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(scale, scale, scale), 10 * Time.deltaTime);

		if (transform.position != _moveTargetPosition && isMergeTarget == false) {
			transform.position = Vector3.MoveTowards(transform.position, _moveTargetPosition, speed * Time.deltaTime); //Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
		} else {
			moving = false;
		}
	}


	void OnTriggerEnter2D (Collider2D other) {
		if (isMergeOrigin && other.gameObject.GetComponent<GameTile>().isMergeTarget) {
			isMergeOrigin = false;
			mergeTarget.isMergeTarget = false;
			Merge();
		}
	}


	public void Kill () {
		_index = 0;
		animator.SetTrigger("kill");
		scale = 0.5f;
	}

	public void Spawn () {
		animator.SetTrigger("spawn");
		_index = (UnityEngine.Random.value > 0.5) ? 2 : 4;
		scale = 1f;
	}

	public void Merge () {
		_index *= 2;
		if (_index == 2048)
			Score.Victory();
		animator.SetTrigger("merge");
		transform.localScale = new Vector3(1.5f, 1.5f);

		//Raise score by new value
		Score.Raise(_index);
	}
}