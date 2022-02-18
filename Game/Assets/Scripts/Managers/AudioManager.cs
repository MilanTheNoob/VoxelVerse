using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region instance
    public static AudioManager instance;
    void Awake() { instance = this; }
    #endregion

    public AudioSource OpenMenu;
    public AudioSource CloseMenu;

    [Space]

    public AudioSource GrabItem;
    public AudioSource DropItem;

    [Space]

    public AudioSource[] Steps;

    CharacterController characterController;

    int stepsIndex;
    bool isMoving = false;

    private void Start()
    {
        characterController = FindObjectOfType<CharacterController>();
    }

    public void FixedUpdate()
    {
        if (characterController.moveDirection != Vector3.zero && !isMoving && !characterController.isFalling)
        {
            stepsIndex = Random.Range(0, Steps.Length);
            Steps[stepsIndex].Play();

            isMoving = true;
        }

        if ((characterController.moveDirection == Vector3.zero || characterController.isFalling) && isMoving)
        {
            Steps[stepsIndex].Stop();
            isMoving = false;
        }
    }
}
