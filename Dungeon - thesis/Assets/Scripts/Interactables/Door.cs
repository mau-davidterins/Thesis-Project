﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Door : InteractableObject, IMemorizable
{
  public static event Action OnRoomEnter = delegate { };

  [SerializeField]
  Sprite OpenSprite = null;

  public Type MemorizableType { get { return GetType(); } }

  bool ofInterest = true;
  public bool OfInterest { get { return ofInterest; } set { ofInterest = value; } }


  public Vector2 TargetDoorPosition { get; set; }

  public int TargetRoomID { get; set; }

  public bool IsOpen { get; set; }

  public int AgentHealthOnEnter { get; set; }

  //private void OnTriggerEnter2D(Collision2D collision)
  //{
  //  if (collision.gameObject.tag == "Player")
  //    DoorEnter(collision.gameObject);
  //}

  public void Unlock()
  {
    IsOpen = true;
    GetComponent<SpriteRenderer>().sprite = OpenSprite;
  }

  void Enter(GameObject player)
  {
    var dungeon = Dungeon.Singleton;
    var room = dungeon.RoomLookup[TargetRoomID];
    dungeon.ChangeRoom(TargetRoomID, TargetDoorPosition);
    Debug.Log("Entered room: " + TargetRoomID);
    player.transform.position = new Vector2(TargetDoorPosition.x + 0.5f, TargetDoorPosition.y + 0.5f);
    player.GetComponent<BlackBoard>().TargetLoot = null;
  }

  public override void Interact(GameObject player)
  {
    Enter(player);
    OnRoomEnter.Invoke();
  }
}
