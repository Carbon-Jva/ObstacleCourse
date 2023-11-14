/*
* Patroller.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroller : MonoBehaviour
{
    //Consts
    private const float rotationSlerpAmount = .68f;

    [Header("References")]
    public Transform trans;
    public Transform modelHolder;

    [Header("Stats")]
    public float moveSpeed = 30.0f;

    //private variables
    private int currentPointIndex;
    private Transform currentPoint;
    private Transform[] patrolPoints;

    private void SetCurrentPatrolPoint(int index)
    {
        currentPointIndex = index;
        currentPoint = patrolPoints[index];
    }
    //Returns a list containing the Transform of each child
    // with a name that starts with "Patrol Point"
    private List<Transform> GetUnsortedPatrolPoints()
    {
        //Get the Transform of each child in the Patroller
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();

        //Declare a local List storing Transforms
        List<Transform> points = new List<Transform>();

        for (int i = 0; i < children.Length; i++)
        {
            //Check if the child's name starts with "Patrol Point ("
            if (children[i].gameObject.name.StartsWith("Patrol Point ("))
            {
                //If so, add it to the 'points' List
                points.Add(children[i]);
            }
        }

        //Return point list
        return points;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Get an unsorted list of patrol points
        List<Transform> points = GetUnsortedPatrolPoints();

        //Only continue if we found att least 1 patrol point
        if (points.Count > 0)
        {
            //Prepare our array of patrol points
            patrolPoints = new Transform[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                //Quick reference to the current point
                Transform point = points[i];

                //Isolate just the patrol point number within the name
                int closingParathesisIndex = point.gameObject.name.IndexOf(')');

                string indexSubstring = point.gameObject.name.Substring(14, closingParathesisIndex - 14);

                //Convert the number from a string to an integer
                int index = Convert.ToInt32(indexSubstring);

                //Set a reference in our script patrolPoints array
                patrolPoints[index] = point;

                //Unparent each patrol poiny so it doesn't move with us
                point.SetParent(null);

                //Hide patrol points in the Hierarchy
                point.gameObject.hideFlags = HideFlags.HideInHierarchy;

            }
            //Start patrooling at first point in the array
            SetCurrentPatrolPoint(0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Only operate if we have a currentPoint
        if (currentPoint != null)
        {
            //Move root GameObject towards the current point
            trans.position = Vector3.MoveTowards(trans.position, currentPoint.position, moveSpeed * Time.deltaTime);

            //If we're on top of the point already, change the current point
            if (trans.position == currentPoint.position)
            {
                //If we're at the last patrol point ...
                if (currentPointIndex >= patrolPoints.Length -1)
                {
                    SetCurrentPatrolPoint(0);
                }
                // Else if we're not at the last patrol point
                else
                {
                    //Go to the index after the current
                    SetCurrentPatrolPoint(currentPointIndex + 1);
                }
            }
            //Else if we're not on the point yet, rotate the model towards it
            else
            {
                Quaternion lookRotation = Quaternion.LookRotation((currentPoint.position - trans.position).normalized);

                modelHolder.rotation = Quaternion.Slerp(modelHolder.rotation, lookRotation, rotationSlerpAmount);
            }
        }
    }
}
