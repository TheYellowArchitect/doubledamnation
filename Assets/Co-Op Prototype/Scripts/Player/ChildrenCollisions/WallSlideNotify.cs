using NaughtyAttributes;
using UnityEngine;

public class WallSlideNotify : MonoBehaviour
{
	[ReadOnlyAttribute]
	public WallSlideCheck parentWallSlideCheck;
	public WallSlideCheck.WallSlidePosition position;

	// Use this for initialization
	void Start ()
	{
		parentWallSlideCheck = GetComponentInParent<WallSlideCheck>();
	}

	private void OnTriggerEnter2D(Collider2D wallToRegister)
	{
		parentWallSlideCheck.SideWallEnterTrigger(wallToRegister, position);
	}

	private void OnTriggerExit2D(Collider2D wallToRegister)
	{
		parentWallSlideCheck.SideWallExitTrigger(wallToRegister, position);
	}

}
