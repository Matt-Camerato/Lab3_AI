using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class CamRig : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float verticalSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform camTransform;
    [SerializeField] private TMP_Text valueText;

    private CharacterController cc;

    private float verticalRotation = 0f;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //check if escape key was pressed
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            //toggle cursor lock state
            if(Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Locked;
        }

        //move cam rig forward, backward, left and right using WASD
        transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime);
        transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime);

        //move cam rig up and down using space and LShift
        float verticalMovement = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalMovement = verticalSpeed;
        else if (Input.GetKey(KeyCode.LeftShift)) verticalMovement = -verticalSpeed;
        transform.Translate(Vector3.up * verticalMovement * Time.deltaTime);

        //rotate cam rig using mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80, 80);
        camTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up, mouseX);
    }

    //called by slider to change mouse sensitivity
    public void OnSensitivityChanged(float value)
    {
        valueText.text = value.ToString();
        mouseSensitivity = value;
    }
}
