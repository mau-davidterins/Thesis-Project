﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionID
{
  None,
  PickupItem,
  MeeleAttack,
  RangedAttack,
  ChangeWeapon,
  OpenChest,
  Drink,
  OpenDoor,
  Explore,
  EnterPortal,
}

public enum ActionCallback
{
  Successfull,
  Failed,
  NeedPath,
}
