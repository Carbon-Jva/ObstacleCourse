/*
* Wanderer.cs
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

public class Wanderer : MonoBehaviour
{
    private enum State
    {
        Idle,
        Rotating,
        Moving
    }
    private State state = State.Idle;

    [HideInInspector] public WanderingRegion region;
    
    [Header("References")]
    public Transform trans;
    public Transform modelTrans;

    [Header("Stats")]
    public float movespeed = 18.0f;

    [Tooltip("Minimum wait time before retargeting again.")]
    public float minRetargetingInterval = 4.4f;

    [Tooltip("Maximum wait time before retargeting again.")]
    public float maxRetargetingInterval = 6.2f;

    [Tooltip("Time in seconds taken to rotate after targeting, before moving begins.")]
    public float rotationTime = 0.6f;

    [Tooltip("Time in seconds after rotation finishes before movement starts.")]
    public float postRotationWaitTime = 0.3f;

    private Vector3 currentTarget; //Positiopn we're currently targeting
    private Quaternion initialRotation; //Our rotation when we first retargeted
    private Quaternion targetRotation; //The rotation we're aiming to reach
    private float rotationStartTime; //Time.time at wich we started rotating

    //Called on Start and invokes itself again after each call.
    //Each invoke will a random time within the retarget interval.
    void Retarget()
    {
        //Set our current target to a new random point in the region:
        currentTarget = region.GetRandomPointWithin();

        //Mark our initial rotation:
        initialRotation = modelTrans.rotation;

        //Mark the rotation required to look at the target:
        targetRotation = Quaternion.LookRotation((currentTarget - trans.position).normalized);

        //Start rotating:
        state = State.Rotating;
        rotationStartTime = Time.time;

        //Begin moving again 'postRotationWaitTime' seconds after rotation ends:
        Invoke("BeginMoving", rotationTime + postRotationWaitTime);

    }

    //Called by Retarget to initiate movement.
    void BeginMoving()
    {
        //Make double sure that we're facing the targetRotation
        modelTrans.rotation = targetRotation;

        //Set state to Moving
        state = State.Moving;
    }

    // Start is called before the first frame update
    void Start()
    {
        //On start, call Retarget() immediately
        Retarget();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Moving)
        {
            //Measure the distance we're moving this frame:
            float delta = movespeed * Time.deltaTime;

            //Move towards the target by the delta:
            trans.position = Vector3.MoveTowards(trans.position, currentTarget, delta);

            //Become idle and invoke the next Retarget once we hit the point:
            if (trans.position == currentTarget)
            {
                state = State.Idle;
                Invoke("Retarget", Random.Range(minRetargetingInterval, maxRetargetingInterval));
            }
        }
        else if (state == State.Rotating) 
        {
            //Measure the time we've spent rotating so far, in seconds:
            float timeSpentRotating = Time.time - rotationStartTime;

            //Rotate from initialRotation towards targetRotation:
            modelTrans.rotation = Quaternion.Slerp(initialRotation, targetRotation, timeSpentRotating/rotationTime);
        }
    }
}
