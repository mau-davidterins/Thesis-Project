﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour {
    // Normal Movements Variables
    private float walkSpeed;
    private bool hasTarget, interruptPath;
    private List<Node> path;
    private TileModel targetTile;
    private int pathIndex;
    private Vector3 offset;

    void Start() {
        hasTarget = false;
        interruptPath = false;
        walkSpeed = 0.05f;
        offset = new Vector3(-0.5f, -0.5f, 0.0f);
    }

    void FixedUpdate() {
        // Acquire path if agent has no target and isn't instructed to change path
        if (!hasTarget || interruptPath) {
            //Får vara här så länge för att nå dungeon.
            var dungeon = GameObject.FindWithTag("Dungeon").GetComponent<Dungeon>();
            var room = dungeon.CurrentRoom;
            var cellPos = dungeon.WorldGrid.WorldToCell(transform.position);

            path = room.RoomGraph.FindPath(room.Tiles2D[cellPos.y, cellPos.x], room.Tiles2D[6, 4]);
            pathIndex = 0;
            if (path.Count <= 0)
                hasTarget = false;
            else
                hasTarget = true;
            interruptPath = false;          
        }

        if (hasTarget) {
            transform.position = Vector3.MoveTowards(transform.position, path[pathIndex].GetFloatPosition(), walkSpeed);           
        }

        if (hasTarget && (Vector3.Distance(transform.position, path[pathIndex].GetFloatPosition()) < 0.001f)) {
            pathIndex++;
            if (pathIndex >= path.Count) {
                hasTarget = false;
            }
        }

        if (hasTarget) {
            InfoBox.pathLength = path.Count;
            InfoBox.playerTile = transform.position.ToString();
            InfoBox.targetTile = path[pathIndex].position.ToString();
            InfoBox.stepsLeft = path.Count - pathIndex;
        }
    }
}

