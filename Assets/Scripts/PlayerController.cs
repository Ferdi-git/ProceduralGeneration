using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 15;
    private Vector3 moveDir;


    private void Update()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"),0, Input.GetAxisRaw("Vertical")).normalized;
    }


    private void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.MovePosition(rb.position + transform.TransformDirection(moveDir) * moveSpeed * Time.deltaTime);
    }

}
