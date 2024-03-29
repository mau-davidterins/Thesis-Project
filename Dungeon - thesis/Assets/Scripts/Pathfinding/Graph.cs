﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Graph with Nodes which can use A* algorithm to obtain the closest path to the target
/// </summary>
public class Graph : MonoBehaviour
{
 
  private int distance;
  private Node[,] nodes;
  private List<Node> unexploredNodes;
  private List<Node> finalPath;
  private int gridSizeX, gridSizeY;
  private int totalUnexplored = 0;
  public int TotalTraversableTiles { get { return totalUnexplored; } }

  private float unexploredPercentage;
  public float UnexploredPercentage
  {
    get
    {
      return (float)unexploredNodes.Count / (float)totalUnexplored;
    }

  }

  /// <summary>
  /// Creates a Graph that will use A* algorithm to obtain the shortest way to the target
  /// </summary>
  /// <param name="gridSizeX">Grid dimensions X</param>
  /// <param name="gridSizeY">Grid dimensions Y</param>
  public Graph(int gridSizeX, int gridSizeY)
  {
    this.gridSizeX = gridSizeX;
    this.gridSizeY = gridSizeY;
    nodes = new Node[gridSizeX, gridSizeY];
    unexploredNodes = new List<Node>();
    for (int x = 0; x < gridSizeX; x++)
    {
      for (int y = 0; y < gridSizeY; y++)
      {
        nodes[x, y] = new Node(new Vector3Int(x, y, 0), TileType.WALL);
      }
    }
  }

  /// <summary>
  /// Sets the nearby nodes to be explored
  /// </summary>
  /// <param name="agentPosition"></param>
  /// <param name="range"></param>
  public void ExploreNodes(Vector2 agentPosition, int range)
  {
    int xmin = (int)Mathf.Clamp(agentPosition.x - range, 0f, gridSizeX);
    int ymin = (int)Mathf.Clamp(agentPosition.y - range, 0f, gridSizeY);
    int xmax = (int)Mathf.Clamp(agentPosition.x + range, 0f, gridSizeX);
    int ymax = (int)Mathf.Clamp(agentPosition.y + range, 0f, gridSizeY);
    for (int x = xmin; x < xmax; x++)
    {
      for (int y = ymin; y < ymax; y++)
      {
        if (nodes[x, y].tileType != TileType.WALL && unexploredNodes.Contains(nodes[x, y]))
        {
          unexploredNodes.Remove(nodes[x, y]);
        }
      }
    }
  }

  // TODO: Prefer to explore a position close to the player? YE
  // TODO använd position parametern för att kolla distance för närmaste nod.
  public Vector2 GetUnexploredPosition(Vector2 position)
  {
    if (unexploredNodes.Count <= 0)
      return Vector2.zero;
    //int index = Random.Range(0, unexploredNodes.Count);
    //return new Vector2(unexploredNodes[index].position.x, unexploredNodes[index].position.y);
    Node closestNode = new Node(1000);
    float shortestDistance = float.MaxValue;

    foreach(Node n in unexploredNodes)
    {
      if(n.gCost < closestNode.gCost)
      {//Börjar med att räkna på nodes som inte är fiender.
        float distanceToNode = Vector2.Distance(position, closestNode.GetFloatPosition());
        if (distanceToNode < shortestDistance)
        {
          closestNode = n;
          shortestDistance = distanceToNode;
        }
      }
    }
    return closestNode.GetFloatPosition();
  }

  /// <summary>
  /// Inserts a new Node into the graph used by the A* algorithm when the room is loaded
  /// </summary>
  /// <param name="position">The Node's position in the grid</param>
  /// <param name="tileType">Tile type</param>
  public void InsertNode(Vector3Int position, TileType tileType)
  {
    if (tileType != TileType.WALL)
    {
      Node node = new Node(position, tileType);
      if(tileType == TileType.ENEMY )
      {
        node.gCost = 30;
      }
      if(tileType == TileType.DOORENTER)
      {
        node.gCost = 5;
      }
      nodes[position.x, position.y] = node;
      unexploredNodes.Add(node);
      totalUnexplored++;
    }

  }

  /// <summary>
  /// Builds a path to the target tile from the start tile (usually where the agent is)
  /// </summary>
  /// <param name="startTile">Start tile</param>
  /// <param name="targetTile">Target tile</param>
  public List<Node> FindPath(TileModel startTile, TileModel targetTile)
  {
    Node startNode = nodes[(int)startTile.Position.x, (int)startTile.Position.y];
    Node targetNode = nodes[(int)targetTile.Position.x, (int)targetTile.Position.y];

    List<Node> openSet = new List<Node>();
    HashSet<Node> closedSet = new HashSet<Node>();
    openSet.Add(startNode);

    while (openSet.Count > 0)
    {
      Node currentNode = openSet[0];
      for (int i = 1; i < openSet.Count; i++)
      {
        if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
        {
          currentNode = openSet[i];
        }
      }
      openSet.Remove(currentNode);
      closedSet.Add(currentNode);

      if (currentNode == targetNode)
      {
        return RetracePath(startNode, targetNode);
      }

      foreach (Node neighbour in GetNeighbours(currentNode))
      {
        if (closedSet.Contains(neighbour))
        {
          continue;
        }

        float newMovementCostToNeighbour = currentNode.gCost + Vector3.Distance(currentNode.GetFloatPosition(), neighbour.GetFloatPosition());// GetDistance(currentNode, neighbour);
        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
        {
          neighbour.gCost = newMovementCostToNeighbour;
          neighbour.hCost = Vector3.Distance(currentNode.GetFloatPosition(), neighbour.GetFloatPosition());// GetDistance(neighbour, targetNode);
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
  private List<Node> RetracePath(Node startNode, Node endNode)
  {    // Bara void
    List<Node> path = new List<Node>();
    Node currentNode = endNode;

    while (currentNode != startNode)
    {
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
  private List<Node> GetNeighbours(Node node)
  {
    List<Node> neighbours = new List<Node>();

    for (int x = -1; x <= 1; x++)
    {
      for (int y = -1; y <= 1; y++)
      {
        if (x == 0 && y == 0)
          continue;

        int checkX = node.position.x + x;
        int checkY = node.position.y + y;


        if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
        {
          if (nodes[checkX, checkY].tileType != TileType.WALL)
            neighbours.Add(nodes[checkX, checkY]);
        }
      }
    }
    //Debug.Log("Node at [" + node.position.x + "," + node.position.y + "] " + "has " + neighbours.Count + " neighbours");
    return neighbours;
  }

  /// <summary>
  /// Fetches the distance between two Nodes in Int
  /// </summary>
  /// <param name="nodeA">Node A</param>
  /// <param name="nodeB">Node B</param>
  /// <returns></returns>
  private float GetDistance(Node nodeA, Node nodeB)
  {
    int distX = nodeA.position.x - nodeB.position.x;
    int distY = nodeB.position.y - nodeB.position.y;

    if (distX > distY)
      return 14 * distY + 10 * (distX - distY);
    else
      return 14 * distX + 10 * (distY - distX);
  }
}
