﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The Actual room that will be used in the game.
/// </summary>
public class Room : MonoBehaviour
{
  public TileModel[,] Tiles2D { get; private set; }

  public Dictionary<Vector2, RoomEdgeModel> ConnectionLookup { get; private set; }

  public int RoomID { get; private set; }

  public int Width { get; private set; }

  public int Height { get; private set; }

  public Graph RoomGraph { get; set; }

  public double EntranceSafety { get; private set; }

  public bool IsExplored { get { return RoomGraph.UnexploredPercentage < 0.1f; } }

  public Queue<KeyInfo> requiredKeys = new Queue<KeyInfo>();

  public int StartingTreasureCount { get; private set; }

  public int StartingEnemyCount { get; private set; }

  /// <summary>
  /// Used as a contructor to build the acutal room from its model.
  /// </summary>
  /// <param name="model">Model.</param>
  public void CreateFromModel(RoomModel model)
  {
    RoomID = model.RoomID;
    Width = model.Width;
    Height = model.Height;
    EntranceSafety = model.EntranceSafety;
    Tiles2D = new TileModel[Width, Height];
    int index = 0;
    for (int x = 0; x < Width; x++)
    {
      for (int y = 0; y < Height; y++)
      {
        if (model.Tiles[index].Type == TileType.ENEMY)
        {
          StartingEnemyCount++;
        }
        else if (model.Tiles[index].Type == TileType.TREASURE)
        {
          StartingTreasureCount++;
        }
        Tiles2D[x, y] = model.Tiles[index++];
      }
    }
    
    ConnectionLookup = new Dictionary<Vector2, RoomEdgeModel>();
    foreach (RoomEdgeModel edge in model.ConnectingRoomIDs)
    {
      ConnectionLookup[edge.StartDoorPosition] = edge;
    }
  }


}
