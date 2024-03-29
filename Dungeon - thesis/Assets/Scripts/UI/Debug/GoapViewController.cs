﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GoapViewController : MonoBehaviour
{
  [SerializeField]
  private Text GoalText = null;

  [SerializeField]
  private Text GoalRelevancyText = null;

  [SerializeField]
  GameObject PlanList = null;

  [SerializeField]
  Text PlanStepTextPrefab = null;

  [SerializeField]
  private Text WSVariableText = null;


  int actionIndex = 0;

  public void SetGoalRelevancies(List<Goal_Goap> goals)
  {
    GoalRelevancyText.text = "Goal Relevancies:\n";
    foreach (Goal_Goap goal in goals)
    {
      string relevancy = goal.Relevancy.ToString();
      if(relevancy.Length > 4)
      {
        relevancy = goal.Relevancy.ToString().Substring(0, 3);
      }
      GoalRelevancyText.text += goal.GetType() + " " + relevancy + "\n";
    }
  }

  public void SetGoal(Goal_Goap goal)
  {
    GoalText.text = "Selected Goal:\n" + goal.GetType() + " " + goal.Relevancy;

  }

  public void SetPlan(Queue<ActionID> actions)
  {
    foreach (Transform child in PlanList.transform)
    {
      Destroy(child.gameObject);
    }
    if (actions.Count > 0)
    {
      actionIndex = 0;
      int index = 1;
      foreach (ActionID action in actions)
      {
        var planStep = Instantiate(PlanStepTextPrefab, PlanList.transform);
        planStep.color = Color.yellow;
        planStep.text += index++ + ". " + action;
      }
    }
  }

  public void UpdateActionStatus(ActionCallback actionResult)
  {
    //Debug.Log("Current acttionIndex " + actionIndex);
    try
    {
      var updatedplanStep = PlanList.transform.GetChild(actionIndex).gameObject;
      switch (actionResult)
      {
        case ActionCallback.Successfull:
          updatedplanStep.GetComponent<Text>().color = Color.green;
          break;
        case ActionCallback.Failed:
          updatedplanStep.GetComponent<Text>().color = Color.red;
          break;
      }
      actionIndex++;
    }
    catch (System.Exception ex)
    {
      Debug.Log(ex.Message);
    }

  }

  public void UpdateWSVariables(WorldStateSet currentWS)
  {
    WSVariableText.text = "WorldState";
    foreach (var keyVal in currentWS)
    {
      WSVariableText.text += "\n" + keyVal.Key + ": " + keyVal.Value;
    }
  }
}

