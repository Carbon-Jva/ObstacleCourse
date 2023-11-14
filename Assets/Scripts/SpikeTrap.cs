/*
* SpikeTrap.cs
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

public class SpikeTrap : MonoBehaviour
{
    private enum State
    {
      Lowered,
      Lowering,
      Raising,
      Raised
    }

    private State state = State.Lowered;
    private const float SpikeHeight = 3.6f;
    private const float LoweredSpikeHeight = 0.08f;

    [Header("Stats")]
    [Tooltip("Time in seconds after lowering the spikes before raising them again.")]
    public float interval = 2.0f;

    [Tooltip("Time in seconds after raising the spikes before they start lowering again.")]
    public float raiseWaitTime = 0.3f;

    [Tooltip("Time in seconds taken to fully lower the spikes.")]
    public float lowerTime = 0.6f;

    [Tooltip("Time in seconds taken to fully raise the spikes.")]
    public float raisetime = 0.08f;

    private float lastSwitchTime = Mathf.NegativeInfinity;

    [Header("References")]
    [Tooltip("Reference to the parent of all spikes.")]
    public Transform spikeHolder;

    public GameObject hitboxGameObject;
    public GameObject colliderGameObject;

    void StartRaising()
    {
      lastSwitchTime = Time.time;
      state = State.Raising;
      hitboxGameObject.SetActive(true);
    }

    void StartLowering()
    {
      lastSwitchTime = Time.time;
      state = State.Lowering;
    }
    // Start is called before the first frame update
    void Start()
    {
      //Spikes will be lowered by default.
      //We'll start raising them 'interval' seconds after Start.
      Invoke("StartLowering", interval);
    }

    // Update is called once per frame
    void Update()
    {
      if (state == State.Lowering)
      {
        //Get the spike holder local scale:
        Vector3 scale = spikeHolder.localScale;

        //Update the Y scale by lerping from max height to min height:
        scale.y = Mathf.Lerp(SpikeHeight, LoweredSpikeHeight, (Time.time - lastSwitchTime) / lowerTime);

        //Apply the updated scale to the spike holder:
        spikeHolder.localScale = scale;

        //If the spikes have finished lowering:
        if (scale.y == LoweredSpikeHeight)
        {
          //Update the state and Invoke the next raising in 'interval' seconds:
          Invoke("StartRaising", interval);
          state = State.Lowered;
          colliderGameObject.SetActive(false);
        }
      }
      else if (state == State.Raising)
      {
        //Get the spike holder local scale:
        Vector3 scale = spikeHolder.localScale;

        //Update the Y scale by lerping from min height to max height:
        scale.y = Mathf.Lerp(LoweredSpikeHeight, SpikeHeight, (Time.time - lastSwitchTime) / raisetime);

        //Apply the updated scale to the spike holder:
        spikeHolder.localScale = scale;

        //If the spikes have finished raising:
        if (scale.y == SpikeHeight)
        {
          //Update the state and Invoke the next lowering in 'raiseWaitTime' seconds:
          Invoke("StartLowering", raiseWaitTime);
          state = State.Raised;
          //Activate the collider to block the player:
          colliderGameObject.SetActive(true);

          //Deactivate the hitbox so it no longer kills the player:
          hitboxGameObject.SetActive(false);
        }
      }
    }
}
