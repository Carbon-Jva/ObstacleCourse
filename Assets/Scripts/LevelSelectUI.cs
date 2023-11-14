/*
* LevelSelectUI.cs
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
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelSelectUI : MonoBehaviour
{
  //Build index of the currently-loaded scene.
  private int currentScene = 0;

  // Current scene's level view camera, if any.
  private GameObject levelViewCamera;

  //Current ongoing scene loading operation, if any.
  private AsyncOperation currentLoadOperation;

  void OnGUI()
  {
    GUILayout.Label("OBSTACLE COURSE");

    //If this isn't the main menu:
    if (currentScene != 0)
    {
      GUILayout.Label("Currently viewing Level " + currentScene);

      //Show a PLAY button:
      if (GUILayout.Button("PLAY"))
      {
        //If the button is clicked, start playing the level:
        PlayCurrentLevel();
      }
    }
    else //If this is the main menu
    {
      GUILayout.Label("Sellect a level to preview it.");
    }
    //Starting at scene build index 1, loop through the remaining scene indexes:
    for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
    {
      //Show a button with text "Level [level number]"
      if (GUILayout.Button("Level " + i))
      {
        //If that button is pressed, and we aren't already waiting for a scene to load:
        if (currentLoadOperation == null)
        {
          //Start loading the level asynchronously:
          currentLoadOperation = SceneManager.LoadSceneAsync(i);

          //Set current scene:
          currentScene = i;
        }
      }
    }
  }

  private void PlayCurrentLevel()
  {
    //Deactive the level view camera:
    levelViewCamera.SetActive(false);

    //Try to find the Player GameObject:
    var playerGobj = GameObject.Find("Player");

    //Throw an error if we couldn't find it:
    if (playerGobj == null)
    {
      Debug.LogError("Couldn't find a Player in the level!");
    }
    else // If we did find the player:
    {
      //Get the Player script attached and enable it:
      var playerScript = playerGobj.GetComponent<Player>();
      playerScript.enabled = true;

      //Through the player script, access the camera GameObject and activate it:
      playerScript.cam.SetActive(true);

      //Destroy self; we'll come back when the main scene is loaded again:
      Destroy(this.gameObject);
    }
  }
  // Start is called before the first frame update
  void Start()
  {
    //Make sure this object persists when the scene changes:
    DontDestroyOnLoad(gameObject);
  }

  // Update is called once per frame
  void Update()
  {
    //If we have a current load operation and it's done:
    if (currentLoadOperation != null && currentLoadOperation.isDone)
    {
      //Null out the load operation:
      currentLoadOperation = null;

      //Find the level view camera in the scene:
      levelViewCamera = GameObject.Find("Level View Camera");

      //Log an error if we couldn't find the camera:
      if (levelViewCamera == null)
      {
        Debug.LogError("No level view camera was found in this scene!");
      }
    }
  }
}
