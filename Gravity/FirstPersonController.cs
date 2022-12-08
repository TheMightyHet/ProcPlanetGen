using System.Collections;
using UnityEngine;

[RequireComponent (typeof (GravBody))]
public class FirstPersonController : MonoBehaviour
{
	public float mouseSensitivityX = 1;
	public float mouseSensitivityY = 1;
	public float walkSpeed = 5;
	public float jumpHeight = 2f;

    Vector3 moveAmount;
	Vector3 smoothMoveVelocity;
	float verticalLookRotation;
	Transform cameraTransform;
	Rigidbody rb;


	void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		cameraTransform = Camera.main.transform;
		rb = GetComponent<Rigidbody>();
	}

	void Update()
	{
		transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
		verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60, 60);
		cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;

		float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");

		Vector3 moveDir = new Vector3(inputX, 0, inputY).normalized;
		Vector3 targetMoveAmount = moveDir * walkSpeed;
		moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(100 * jumpHeight * transform.up);
        }
    }

	void FixedUpdate()
	{
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rb.MovePosition(rb.position + localMove);
	}

	bool IsGrounded()
	{
		return Physics.Raycast(transform.position, -transform.up, 1.2f);
	}
}