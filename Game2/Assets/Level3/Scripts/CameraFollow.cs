using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform player;
	public float offsetAmount = 2.0f;
	public float smoothSpeed = 0.1f;

	private Vector3 targetPosition;
	private Player playerScript;

    public float minX = -10f; // Set these to match your level bounds
	public float maxX = 10f;
	private float previousSize;
	private float zoomLerpSpeed = 0.5f; // Temporarily increase speed when zoom changes

	void Start()
	{
		playerScript = player.GetComponent<Player>();
	}

	void LateUpdate()
	{
		if (player == null) return;

		// Determine offset based on player's facing direction
		float offset = playerScript.isFacingRight ? offsetAmount : -offsetAmount;
		float targetX = player.position.x + offset;

		// Clamp the camera’s X position within minX and maxX
		targetX = Mathf.Clamp(targetX, minX, maxX);

		// Smoothly move the camera
		Vector3 targetPosition = new Vector3(targetX, player.position.y, transform.position.z);
		transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
	}
}