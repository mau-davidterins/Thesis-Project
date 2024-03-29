﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class Movement : MonoBehaviour
{
  // Normal Movements Variables
  private float walkSpeed;
  private bool hasTarget, interruptPath;
  private List<Node> path;
  private int pathIndex = 4;
  private Vector3 offset;

  public event EventHandler AtDestination;

  public TileModel Target { get; private set; }
  public bool PathInterrupted { get { return interruptPath; } }
  public bool HasTarget { get { return hasTarget; } }

  float targetOffsetDistance = 0;

  void Start()
  {
    hasTarget = false;
    interruptPath = false;
    walkSpeed = Settings.FetchWalkSpeed();
    offset = new Vector3(-0.5f, -0.5f, 0.0f);
  }

  //TODO Lägga till så att man kan hitta en path till en target i ett annat rum.
  public bool TryMoveToTarget(Vector2 targetPosition, float targetOffsetDistance = 0.001f)
  {
    var dungeon = Dungeon.Singleton;
    interruptPath = true;

    if (!hasTarget || interruptPath)
    {
      var room = dungeon.CurrentRoom;
      var cellPos = dungeon.WorldGrid.WorldToCell(transform.position);

      try
      {
        path = room.RoomGraph.FindPath(room.Tiles2D[cellPos.y, cellPos.x], room.Tiles2D[(int)targetPosition.y, (int)targetPosition.x]);
      }
      catch (Exception ex)
      {
        //TODO Detta händer ibland.. behövs kollas in.
        Debug.LogError(
        "Could not find path from: " + cellPos.y + ", " + cellPos.x + " to " + (int)targetPosition.y + ", " + (int)targetPosition.x);
        throw ex;
      }

      pathIndex = 0;
      if (path != null && path.Count <= 0)
      {
        hasTarget = false;
      }
      else
      {
        hasTarget = true;
        this.targetOffsetDistance = targetOffsetDistance;
        interruptPath = false;
      }
    }
    return hasTarget;
  }


  private void Update()
  {
    if (hasTarget && path != null)
    {
      //Moving between nodes
      if (hasTarget && !interruptPath)
      {
        transform.position = Vector2.MoveTowards(transform.position, path[pathIndex].GetFloatPosition(), walkSpeed);
      }

      //At a node
      if (hasTarget && !interruptPath && (Vector2.Distance(transform.position, path[pathIndex].GetFloatPosition()) < 0.001f + targetOffsetDistance))
      {
        pathIndex++;
        if (pathIndex >= path.Count)
        {//At final node
          hasTarget = false;
          targetOffsetDistance = 0;
          if (AtDestination != null)
          {
            //PrintAtTargetInvocationList();
            AtDestination.Invoke(this, new EventArgs());
          }

        }
        GetComponent<Vision>().Scan();
      }

      if (hasTarget)
      {
        InfoBox.pathLength = path.Count;
        InfoBox.playerTile = transform.position.ToString();
        InfoBox.targetTile = path[pathIndex].position.ToString();
        InfoBox.stepsLeft = path.Count - pathIndex;
      }
    }
    TemporaryWalkOnMouseClick();
  }

  void TemporaryWalkOnMouseClick()
  {
    if (Input.GetMouseButtonDown(0))
    {
      interruptPath = true;

      var dungeon = Dungeon.Singleton;
      Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));

      var mousePos = dungeon.WorldGrid.WorldToCell(new Vector3((int)pos.x, (int)pos.y, 0));
      var tile = dungeon.CurrentRoom.Tiles2D[mousePos.x, mousePos.y];
      if (tile != null)
      {
        if (!hasTarget || interruptPath)
        {
          var room = dungeon.CurrentRoom;
          var cellPos = dungeon.WorldGrid.WorldToCell(transform.position);

          path = room.RoomGraph.FindPath(room.Tiles2D[cellPos.y, cellPos.x], room.Tiles2D[mousePos.y, mousePos.x]);
          pathIndex = 0;
          if (path.Count <= 0)
            hasTarget = false;
          else
            hasTarget = true;
          interruptPath = false;
        }
      }
    }
  }

  public void PrintAtTargetInvocationList()
  {
    if (AtDestination != null)
      Debug.LogWarning("Invocation count movement: " + AtDestination.GetInvocationList().Length);
  }

  public int GetTargetInvocationCount()
  {
    if (AtDestination != null)
    {
      return AtDestination.GetInvocationList().Length;
    }
    return -1;
  }
}

