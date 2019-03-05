﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class PlayerMovement : MonoBehaviour
{
  // Normal Movements Variables
  private float walkSpeed;
  private bool hasTarget, interruptPath;
  private List<Node> path;
  private List<Node> memory;
  private TileModel targetTile;
  private int pathIndex = 4;
  private float sightRange = 4.5f;
  private Vector3 offset;
  private float scanRotation = 5f;

  void Start()
  {
    hasTarget = false;
    interruptPath = false;
    walkSpeed = 0.08f;
    memory = new List<Node>();
    offset = new Vector3(-0.5f, -0.5f, 0.0f);
  }


  void TemporaryWalkOnMouseClick()
  {
    if (Input.GetMouseButtonDown(0))
    {
      interruptPath = true;

      var dungeon = GameObject.FindWithTag("Dungeon").GetComponent<Dungeon>();
      Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));

      var mousePos = dungeon.WorldGrid.WorldToCell(new Vector3((int)pos.x, (int)pos.y, 0));
      var tile = dungeon.CurrentRoom.Tiles2D[mousePos.x, mousePos.y];
      if (tile != null)
      {
        //Debug.Log(string.Format("Tile is: {0}", tile.Type));
        // Acquire path if agent has no target and isn't instructed to change path
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

  private void Update()
  {
    TemporaryWalkOnMouseClick();
  }

  void FixedUpdate()
  {
    //// Acquire path if agent has no target and isn't instructed to change path
    //if (!hasTarget || interruptPath) {
    //    //Får vara här så länge för att nå dungeon.
    //    var dungeon = GameObject.FindWithTag("Dungeon").GetComponent<Dungeon>();
    //    var room = dungeon.CurrentRoom;
    //    var cellPos = dungeon.WorldGrid.WorldToCell(transform.position);

    //    path = room.RoomGraph.FindPath(room.Tiles2D[cellPos.y, cellPos.x], room.Tiles2D[6, 4]);
    //    pathIndex = 0;
    //    if (path.Count <= 0)
    //        hasTarget = false;
    //    else
    //        hasTarget = true;
    //    interruptPath = false;          
    //}

    if (hasTarget && !interruptPath)
    {
      transform.position = Vector3.MoveTowards(transform.position, path[pathIndex].GetFloatPosition(), walkSpeed);
    }

    if (hasTarget && !interruptPath && (Vector3.Distance(transform.position, path[pathIndex].GetFloatPosition()) < 0.001f))
    {
      pathIndex++;
      if (pathIndex >= path.Count)
      {
        hasTarget = false;
      }
    }

    if (hasTarget)
    {
      InfoBox.pathLength = path.Count;
      InfoBox.playerTile = transform.position.ToString();
      InfoBox.targetTile = path[pathIndex].position.ToString();
      InfoBox.stepsLeft = path.Count - pathIndex;
    }
    Scan();
  }

  /// <summary>
  /// Scan the area around the agent for memorable tiles.
  /// </summary>
  private void Scan()
  {
    //RaycastHit hitInfo;
    //Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.up) * sightRange);
    //for (int i = 0; i < 360 / scanRotation; i++) {
    //    ray = new Ray(transform.position, ray.direction * sightRange);

    //    if (Physics.Raycast(ray, out hitInfo, sightRange)) {
    //        Debug.DrawLine(ray.origin, hitInfo.point, Color.green);
    //    }
    //    else
    //        Debug.DrawLine(ray.origin, ray.origin + ray.direction * sightRange, Color.red);
    //    ray.direction = Quaternion.AngleAxis(scanRotation, Vector3.forward) * ray.direction;
    //}


    // När den försöker fråga TileModel vilken typ den är så fungerar inte det.
    //RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, Vector2.up * 5f/*, LayerMask.GetMask("Wall")*//* * sightRange*/);
    //for (int i = 0; i < 360 / scanRotation; i++) {
    //if (hitInfo.collider.gameObject.GetComponent<Tile>().Type == TileType.WALL)
    //Debug.Log("Wall hit");
    //}


    //Arrayer som inte borde vara här eller se ut så här sen :)
    var dungeon = GameObject.FindWithTag("Dungeon").GetComponent<Dungeon>();
    Vector2 rayOrigin = transform.position;

    Vector2[] rayDirections = {
      Vector3.left,
     Vector3.right,
      Vector3.up,
      Vector3.down,
      new Vector3(1,1,0),
      new Vector3(-1,-1,0),
      new Vector3(-1,1,0),
      new Vector3(1,-1,0)
     };

    RaycastHit2D[] hits = {
      Physics2D.Raycast(rayOrigin, rayDirections[0], sightRange),
      Physics2D.Raycast(rayOrigin, rayDirections[1], sightRange),
      Physics2D.Raycast(rayOrigin, rayDirections[2], sightRange),
      Physics2D.Raycast(rayOrigin, rayDirections[3], sightRange),
      Physics2D.Raycast(rayOrigin, rayDirections[4], sightRange),
      Physics2D.Raycast(rayOrigin, rayDirections[5], sightRange),
      Physics2D.Raycast(rayOrigin, rayDirections[6], sightRange),
      Physics2D.Raycast(rayOrigin, rayDirections[7], sightRange)
     };
     
    for(int i = 0; i < hits.Length; i++)
    {
      if (hits[i].collider != null)
      {
        GameObject other = hits[i].collider.gameObject;
        if (other)
        {
          var hitpoint = hits[i].point;

          //Detta för att hamna i mitten av en tile, men avrundningen verkar ändå bli fel.
          var offSetHitpoint = hitpoint + (rayDirections[i] / 2);

          if (other.gameObject.name =="CollidingTileLayer")
          {
            //Måste plussa på med 0.5f på hitpointen för att få den till mitten av en tile så int avrundar rätt sen
            //när den ska hämtas ur CurrentRoom.Tiles2D, blir fel ibland. Om du kollar Raysen i debug så ser du att ibland
            //blir den avrundad nedåt och då hamnar rayn på en floor tile.
            TileModel tile = dungeon.CurrentRoom.Tiles2D[(int)offSetHitpoint.x, (int)offSetHitpoint.y];
            //Debug.Log("Hit " + tile.Type);
            Debug.DrawLine(rayOrigin, offSetHitpoint, Color.green);
          }
        }

      }
    }

    ////currentHighlighted = new Vector2Int(outlinesMap.WorldToCell(hit.point).x, outlinesMap.WorldToCell(hit.point).y);
    //else
    //{
    //  Debug.Log("recasting from player");
    //  RaycastHit2D hitFromPlayer = Physics2D.Raycast(playerPos, direction, 50f, outlinesLayer);
    //  if (hitFromPlayer.collider != null)
    //  {

    //  }
    //  //currentHighlighted = new Vector2Int((int)hit.point.x, (int)hit.point.y);
    //}

    ////Debug.Log(currentHighlighted);

    //UpdateHighlighted(outlinesMap, previousHighlighted, currentHighlighted);

  }

  private void RemoveNodeFromMemory(Node node)
  {
    memory.Remove(node);
  }
}

