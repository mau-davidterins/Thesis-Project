﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEngine.Experimental.PlayerLoop;
using System.Linq;

public class RoomBuilder : MonoBehaviour
{
  [SerializeField]
  Tilemap BaseTileLayer = null;

  [SerializeField]
  Tilemap CollideLayer = null;

  [SerializeField]
  UnityEngine.Tilemaps.Tile FloorTile = null;

  [SerializeField]
  UnityEngine.Tilemaps.Tile WallTile = null;

  [SerializeField]
  UnityEngine.Tilemaps.Tile ConnectionDoorTile = null;

  [SerializeField]
  UnityEngine.Tilemaps.Tile DungeonEntranceDoorTile = null;

  [SerializeField]
  GameObject player = null;

  [SerializeField]
  GameObject Enemy = null;

  [SerializeField]
  GameObject TreasureChest = null;

  [SerializeField]
  GameObject ConnectionDoor = null;

  [SerializeField]
  GameObject ExitDoor = null;

  [SerializeField]
  GameObject Room = null;

  [SerializeField]
  GameObject KeyPrefab = null;

  public Tilemap GetBaseLayer() { return BaseTileLayer; }

  private static RoomBuilder Instance;
  public static RoomBuilder Singleton { get { return Instance; } }

  void Awake()
  {
    if (Instance != null)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
  }

  public void BuildRoom(Room room, Vector2 startDoorPosition)
  {
    BuildTileMapLayers(room, startDoorPosition);
  }

  private void BuildTileMapLayers(Room roomToBuild, Vector2 startDoorPosition)
  {
    //För att rensa när rum två laddas in, görs på annat sätt sen. 
    BaseTileLayer.ClearAllTiles();
    CollideLayer.ClearAllTiles();

    Dungeon dungeon = Dungeon.Singleton;

    //Build the room gameObject
    GameObject roomObj = Instantiate(Room, dungeon.gameObject.transform);
    roomObj.name = "Room " + roomToBuild.RoomID;

    //Create its room script and set some variables.
    Room room = roomObj.GetComponent<Room>();
    room = roomToBuild;
    room.RoomGraph = new Graph(room.Width, room.Height);

    List<GameObject> lootableObjects = new List<GameObject>();

    List<Vector2> floorPositions = new List<Vector2>(room.Width * room.Height);

    foreach (TileModel tile in room.Tiles2D)
    {
      Vector3Int tilePos = new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0);

      room.RoomGraph.InsertNode(tilePos, tile.Type);

      var centerOfTile = BaseTileLayer.GetCellCenterWorld(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0));
      switch (tile.Type)
      {
        case TileType.FLOOR:
          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(FloorTile));

          //För att sätta exit dooren på en random position i sista rummet.
          if (room.RoomID == dungeon.RoomLookup.Count - 1)
          {
            floorPositions.Add(centerOfTile);
          }

          break;
        case TileType.WALL:

          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(FloorTile));
          CollideLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(WallTile));

          break;
        case TileType.TREASURE:
          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(FloorTile));

          var center = BaseTileLayer.GetCellCenterWorld(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0));

          lootableObjects.Add(Instantiate(TreasureChest, center, Quaternion.identity, roomObj.transform));
          break;
        case TileType.ENEMY:

          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(FloorTile));

          lootableObjects.Add(Instantiate(Enemy, centerOfTile, Quaternion.identity, roomObj.transform));
          break;
        case TileType.DOOR:

          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(ConnectionDoorTile));
          var doorObj = Instantiate(ConnectionDoor, centerOfTile, Quaternion.identity, roomObj.transform);
          var doorScript = doorObj.GetComponent<Door>();

          RoomEdgeModel roomConnection = dungeon.RoomLookup[room.RoomID].ConnectionLookup[(new Vector2(tilePos.x, tilePos.y))];
          doorScript.TargetRoomID = roomConnection.ToRoomID;
          doorScript.TargetDoorPosition = roomConnection.TargetDoorPosition;
          if ((int)startDoorPosition.x == tilePos.x && (int)startDoorPosition.y == tilePos.y)
          {
            //Lås upp dörren som spelaren har kommit in via.
            doorScript.Unlock();
          }
          else
          {
            //Generra bara nyckel för dörrar som inte spelaren har kommit in via.
            room.requiredKeys.Enqueue(new KeyInfo(doorScript));
          }
          break;
        case TileType.DOORENTER:

          BaseTileLayer.SetTile(tilePos, Instantiate(DungeonEntranceDoorTile));
          if (roomToBuild.RoomID == dungeon.InitialRoomID)
          {
            Camera.main.GetComponent<SmoothCamera>().target = Instantiate(player, centerOfTile, Quaternion.identity, dungeon.transform);
          }

          break;
        case TileType.NONE:
          break;
        default:
          break;
      }

      BaseTileLayer.RefreshAllTiles();
    }

    //Place out keys too lootableItems
    foreach (KeyInfo keyInfo in room.requiredKeys)
    {
      int randomKeyIndex = Random.Range(0, lootableObjects.Count - 1);
      lootableObjects[randomKeyIndex].GetComponentInChildren<Loot>().Items.Add(KeyPrefab);
      lootableObjects[randomKeyIndex].GetComponent<SpriteRenderer>().color = Color.green;
    }

    if (floorPositions.Count > 0)
    {
      int randomExitDoorIndex = Random.Range(0, floorPositions.Count - 1);
      Instantiate(ExitDoor, floorPositions[randomExitDoorIndex], Quaternion.identity, roomObj.transform);
    }


  }

  public void CreateTileLayersOnRoomChange(Room roomToBuild)
  {
    //För att rensa när rum två laddas in, görs på annat sätt sen. 
    BaseTileLayer.ClearAllTiles();
    CollideLayer.ClearAllTiles();


    List<GameObject> lootableObjects = new List<GameObject>();

    foreach (TileModel tile in roomToBuild.Tiles2D)
    {
      Vector3Int tilePos = new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0);

      var centerOfTile = BaseTileLayer.GetCellCenterWorld(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0));
      switch (tile.Type)
      {
        case TileType.FLOOR:
          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(FloorTile));
          break;
        case TileType.WALL:

          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(FloorTile));
          CollideLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(WallTile));

          break;
        case TileType.TREASURE:
          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(FloorTile));

          break;
        case TileType.ENEMY:

          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(FloorTile));

          break;
        case TileType.DOOR:

          BaseTileLayer.SetTile(new Vector3Int((int)tile.Position.x, (int)tile.Position.y, 0), Instantiate(ConnectionDoorTile));


          break;
        case TileType.DOORENTER:

          BaseTileLayer.SetTile(tilePos, Instantiate(DungeonEntranceDoorTile));

          break;
        case TileType.NONE:
          break;
        default:
          break;
      }

      BaseTileLayer.RefreshAllTiles();
    }
  }
}
