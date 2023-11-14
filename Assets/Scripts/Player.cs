/*
* Player.cs
* Description of the content and purpose of the file.
*
* Copyright (c) 2023 Jimmy Vall
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    //References
    [Header("References")]
    public Transform trans;
    public Transform modelTrans;
    public CharacterController characterController;

    //Movement
    [Header("Movement")]
    [Tooltip("Units moved per second at maximum speed.")]
    public float moveSpeed = 24;

    [Tooltip("Time, in seconds, to reach maximum speed.")]
    public float timeToMaxSpeed = .26f;

    //Death and Respawning
    [Header("Death and Respawning")]
    [Tooltip("How long after the player's death, in seconds, before they are respawned?")]
    public float respawnWaitTime = 2f;
    private bool dead = false;
    private Vector3 spawnPoint;
    private Quaternion spawnRotation;

    //Dashing
    [Header("Dashing")]
    [Tooltip("Total number of units traveled when performing a dash.")]
    public float dashDistance = 17.0f;

    [Tooltip("Time taken to perform a dash, in seconds.")]
    public float dashTime = 0.26f;

    [Tooltip("Time after dashing finishes before it can be performed again.")]
    public float dashCoolDown = 1.8f;

    [Tooltip("Multiplier for momentum when attempting to move in a direction opposite the current traveling direction (e.g. trying to move right when already moving left).")]
    public float reverseMomentumMultiplier = 2.2f;

    [Tooltip("Time, in seconds, to go from maximum speed to stationary.")]
    public float timeToLoseMaxSpeed = .2f;

    private Vector3 movementVelocity = Vector3.zero;

    private Vector3 dashDirection;
    private float dashBeginTime = Mathf.NegativeInfinity;

    public GameObject cam;

    private bool paused = false;
    
    private bool IsDashing
    {
        get
        {
            return(Time.time < dashBeginTime +  dashTime);
        }
    }

    private float VelocityGainPerSecond {
        get { 
            return moveSpeed / timeToMaxSpeed;
        }
    }

    private float VelocityLossPerSecond
    {
        get
        {
            return moveSpeed / timeToLoseMaxSpeed;
        }
    }

    private bool CanDashNow
    {
        get 
        {
            return (Time.time > (dashBeginTime + dashTime + dashCoolDown));
        }
    }

    private void Pausing()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Toggle pause status:
            paused = !paused;

            //If we're now paused, set timeScale to 0:
            if (paused)
            {
                Time.timeScale = 0;
            }
            //Otherwise if we're no longer paused, revert timeScale to 1:
            else
            {
                Time.timeScale = 1;
            }
        }
    }
    private void Movement()
    {
        //Only move if we aren't dashing:
        if (!IsDashing && CanDashNow)
        {
            //if W or the up arrow key is held
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                //if we're already moving forward increase Z velocity by VelocityGainPerSecond, but don't go higher then moveSpeed
                if (movementVelocity.z >= 0)
                {
                    movementVelocity.z = Mathf.Min(moveSpeed, movementVelocity.z + VelocityGainPerSecond * Time.deltaTime);
                }
                //else were are moving back increase Z velocity by VelocityPerSecond, using the reversMomentumMultiplier, but don't raise higher than 0
                else
                {  
                    movementVelocity.z = Mathf.Min(0, movementVelocity.z + VelocityGainPerSecond * reverseMomentumMultiplier * Time.deltaTime);
                }
            }
            //if S or the down arrow key is held
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                //if we're already moving forward
                if (movementVelocity.z > 0) 
                { 
                    movementVelocity.z = Mathf.Max(0, movementVelocity.z - VelocityGainPerSecond * reverseMomentumMultiplier * Time.deltaTime);
                }
                //if we're moving back or not moving at all 
                else
                {
                    movementVelocity.z = Mathf.Max(-moveSpeed, movementVelocity.z - VelocityGainPerSecond * Time.deltaTime);
                }
            }
            //if neither forward nor back are being held
            else
            {
                //We must bring the Z velocity back to 0 over time

                //if we moving up, decrease Z velocity by VelocityLossPerSecond, but don't go any lower then 0
                if (movementVelocity.z > 0)
                { 
                    movementVelocity.z = Mathf.Max(0, movementVelocity.z - VelocityLossPerSecond * Time.deltaTime);                              
                }
                // if we're moving down
                else
                {
                    //Increase Z velocity (back towards 0) by VelocityLossPerSecond, but don't go any higher than 0
                    movementVelocity.z = Mathf.Min(0, movementVelocity.z + VelocityLossPerSecond * Time.deltaTime);
                }
            }

            //if D or the up arrow key is held
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                //if we're already moving right increase Z velocity by VelocityGainPerSecond, but don't go higher then moveSpeed
                if (movementVelocity.x >= 0)
                {
                    movementVelocity.x = Mathf.Min(moveSpeed, movementVelocity.x + VelocityGainPerSecond * Time.deltaTime);
                }
                //else were are moving left increase Z velocity by VelocityPerSecond, using the reversMomentumMultiplier, but don't raise higher than 0
                else
                {
                    movementVelocity.x = Mathf.Min(0, movementVelocity.x + VelocityGainPerSecond * reverseMomentumMultiplier * Time.deltaTime);
                }
            }
            //if A or the down arrow key is held
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                //if we're already moving right
                if (movementVelocity.x > 0)
                {
                    movementVelocity.x = Mathf.Max(0, movementVelocity.x - VelocityGainPerSecond * reverseMomentumMultiplier * Time.deltaTime);
                }
                //if we're moving left or not moving at all 
                else
                {
                    movementVelocity.x = Mathf.Max(-moveSpeed, movementVelocity.x - VelocityGainPerSecond * Time.deltaTime);
                }
            }
            //if neither right nor left are being held
            else
            {
                //We must bring the X velocity back to 0 over time

                //if we moving right, decrease X velocity by VelocityLossPerSecond, but don't go any lower then 0
                if (movementVelocity.x > 0)
                {
                    movementVelocity.x = Mathf.Max(0, movementVelocity.x - VelocityLossPerSecond * Time.deltaTime);
                }
                // if we're moving left
                else
                {
                    //Increase X velocity (back towards 0) by VelocityLossPerSecond, but don't go any higher than 0
                    movementVelocity.x = Mathf.Min(0, movementVelocity.x + VelocityLossPerSecond * Time.deltaTime);
                }
            }
            //if the player is moving in either direction (left/right or up/down)
            if (movementVelocity.x != 0 || movementVelocity.z != 0)
            {
                //applying the movement velocity
                characterController.Move(movementVelocity * Time.deltaTime);

                //keeping the model holder rotated towards the last movement direction
                modelTrans.rotation = Quaternion.Slerp(modelTrans.rotation, Quaternion.LookRotation(movementVelocity), .18f);
            }
        }
    }

    public void Respawn()
    {
        dead = false;
        trans.position = spawnPoint;
        enabled = true;
        characterController.enabled = true;
        modelTrans.gameObject.SetActive(true);
        modelTrans.rotation = spawnRotation;
    }

    public void Die()
    {
        if (!dead)
        {
            dead = true;
            Invoke("Respawn", respawnWaitTime);
            movementVelocity = Vector3.zero;
            enabled = false;
            characterController.enabled = false;
            modelTrans.gameObject.SetActive(false);
            dashBeginTime = Mathf.NegativeInfinity;
        }
    }

    private void Dashing()
    {
        if (!IsDashing) //If we aren't dashing right now
        {
            //If the space key is pressed
            if (Input.GetKey(KeyCode.Space))
            {
                //Find the direction we're holding with the movement keys:
                Vector3 movementDir = Vector3.zero;

                //If holding W or up key set z to 1:
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
                    movementDir.z = 1;
                }
                //Else if holding S or down arrow, set z to -1:
                else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                {
                    movementDir.z = -1;
                }
                //If holdind D or right arrow set x to 1:
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                {
                    movementDir.x = 1;
                }
                //Else if holding A or left arrow set x to -1:
                else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                {
                    movementDir.x = -1;
                }
                //If at least one movement key was held:
                if (movementDir.x != 0 || movementDir.z != 0)
                {
                    //Start dashing
                    dashDirection = movementDir;
                    dashBeginTime = Time.time;
                    movementVelocity = dashDirection * moveSpeed;
                    modelTrans.forward = dashDirection;
                }
            }
        }
        else //If were are dashing
        {
            characterController.Move(dashDirection * (dashDistance / dashTime) * Time.deltaTime);
        }
    }

    void OnGUI()
    {
        if (paused)
        {
            float boxWidth = Screen.width * 0.4f;
            float boxHeight = Screen.height * 0.4f;
            GUILayout.BeginArea(new Rect((Screen.width * 0.5f) - (boxWidth * 0.5f),
                                           (Screen.height * 0.5f) - (boxHeight * 0.5f),
                                           boxWidth,
                                           boxHeight));
            
            if (GUILayout.Button("RESUME GAME", GUILayout.Height(boxHeight * 0.5f)))
            {
                paused = false;
                Time.timeScale = 1;
            }

            if (GUILayout.Button("RETURN TO MAIN MENU", GUILayout.Height(boxHeight * 0.5f)))
            {
                Time.timeScale = 1;
                SceneManager.LoadScene(0);
            }

            GUILayout.EndArea();

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        spawnPoint = trans.position;
        spawnRotation = modelTrans.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused)
        {
            Movement();

            Dashing();
        }

        Pausing();

        if (Input.GetKeyDown(KeyCode.T))
            Die();
    }
}
