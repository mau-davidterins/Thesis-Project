﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot_Goal : Goal_Goap
{
  private float baseValue;

  public Loot_Goal(GameObject owner, Planner_Goap planner) : base(owner, planner)
  {
    GoalWorldstates.Add(WorldStateSymbol.HasItem, true);

    //baseValue = persona.personalityModifer[Personality.GREED] * 0.33f;
    //var s = 1;
  }

  /// <summary>
  /// Calculate relevancy to loot something. Does not take distance into account.
  /// </summary>
  /// <param name="blackBoard">The blackboard</param>
  /// <returns></returns>
  public override float CalculateRelevancy(BlackBoard blackBoard)
  {

    if (blackBoard.ImportantItemDrop)
      return relevancy = 1f;
    else if (!blackBoard.TargetTreasureChest)
      return relevancy = 0f;

    // TODO: Right now coins are set to the value 10. This value is hardcoded and needs to be x10 the value of 1 coin.
    relevancy = persona.personalityModifer[Personality.GREED] - owner.GetComponent<Player>().Coins / 100;

    relevancy = Mathf.Clamp(relevancy, 0f, 1f);
    return relevancy;
  }
}
