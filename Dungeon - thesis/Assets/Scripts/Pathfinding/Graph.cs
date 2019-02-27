﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Graph with Nodes which can use A* algorithm to obtain the closest path to the target
/// </summary>
public class Graph : MonoBehaviour {
    private int distance;
    private Node[,] nodes;
    private List<Node> finalPath;
    private int gridSizeX, gridSizeY;

    /// <summary>
    /// Creates a Graph that will use A* algorithm to obtain the shortest way to the target
    /// </summary>
    /// <param name="gridSizeX">Grid dimensions X</param>
    /// <param name="gridSizeY">Grid dimensions Y</param>
    public Graph(int gridSizeX, int gridSizeY) {
        this.gridSizeX = gridSizeX;
        this.gridSizeY = gridSizeY;
        nodes = new Node[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                nodes[x, y] = new Node(new Vector3Int(gridSizeX, gridSizeY, 0), TileType.WALL);
            }
        }
    }

    /// <summary>
    /// Insert a new Node into the graph used by the A* algorithm
    /// </summary>
    /// <param name="position">The Node's position in the grid</param>
    /// <param name="tileType">Tile type</param>
    public void InsertNode(Vector3Int position, TileType tileType) {
        if (tileType != TileType.WALL) {
            nodes[position.x, position.y] = new Node(position, tileType);
        }
    }

    /// <summary>
    /// Builds a path to the target tile from the start tile (usually where the agent is)
    /// </summary>
    /// <param name="startTile">Start tile</param>
    /// <param name="targetTile">Target tile</param>
    public List<Node> FindPath(Tile startTile, Tile targetTile) {
        Node startNode  = nodes[(int)startTile.Position.x, (int)startTile.Position.y];
        Node targetNode = nodes[(int)targetTile.Position.x, (int)targetTile.Position.y];

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0) {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++) {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) {
                    currentNode = openSet[i];        
                }
            }
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode) {
                //RetracePath(startNode, targetNode);
                return RetracePath(startNode, targetNode);                         // return;
            }

            foreach(Node neighbour in GetNeighbours(currentNode)) {
                if (neighbour.tileType != TileType.WALL || closedSet.Contains(neighbour)) {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Summarizes the path obtained and sorts it in the correct order, ready for traversal
    /// </summary>
    /// <param name="startNode">Start node</param>
    /// <param name="endNode">End node</param>
    private List<Node> RetracePath(Node startNode, Node endNode) {    // Bara void
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Fetches the neighbours (the surrounding Nodes) in relation to the passed Node
    /// </summary>
    /// <param name="node">Center node</param>
    /// <returns></returns>
    private List<Node> GetNeighbours(Node node) {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.position.x + x;
                int checkY = node.position.y + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY){
                    neighbours.Add(nodes[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    /// <summary>
    /// Fetches the distance between to Nodes in Int
    /// </summary>
    /// <param name="nodeA">Node A</param>
    /// <param name="nodeB">Node B</param>
    /// <returns></returns>
    private int GetDistance(Node nodeA, Node nodeB) {
        int distX = nodeA.position.x - nodeB.position.x;
        int distY = nodeB.position.y - nodeB.position.y;

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        else
            return 14 * distX + 10 * (distY - distX);
    }
}
