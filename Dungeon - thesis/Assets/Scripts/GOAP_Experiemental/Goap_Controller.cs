﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Goap_Controller : MonoBehaviour
{

  Dictionary<ActionID, Action_Goap> playerActionLookup = new Dictionary<ActionID, Action_Goap>();
  Queue<ActionID> plan;

  List<Action_Goap> playerActions;
  List<Goal_Goap> playerGoals;

  Goal_Goap currentGoal;
  public Action_Goap currentAction;

  BlackBoard blackBoard;
  Planner_Goap planner;
  Persona persona;
  public Persona Persona { get { return persona; } }

  private readonly float ActionDelay = Settings.FetchSpeed();

  GoapViewController viewControl;

  public WorldStateSet PlayerWorldState { get { return playerWorldState; } }
  WorldStateSet playerWorldState = new WorldStateSet()
    {

    {WorldStateSymbol.EnemyDead, false},
    {WorldStateSymbol.MeleeEquiped, true },
    {WorldStateSymbol.AvailableEnemy, false },

    {WorldStateSymbol.LootableItem, false},
    {WorldStateSymbol.ImportantLoot, false},
    {WorldStateSymbol.AvailableChest, false },

    {WorldStateSymbol.IsHealed, false},
    {WorldStateSymbol.RoomExplored, false },
    {WorldStateSymbol.RoomUnexplored, true },
    {WorldStateSymbol.Progress, false},

    {WorldStateSymbol.HasItem, false},
    {WorldStateSymbol.HasPotion, false},
    {WorldStateSymbol.HasKey, false},
    {WorldStateSymbol.HasMeleeWeapon, true },

    {WorldStateSymbol.CanWin, false },
    {WorldStateSymbol.PortalLocated, false },
  };

  void Awake()
  {

    viewControl = GameObject.FindWithTag("GoapViewController").GetComponent<GoapViewController>();

    blackBoard = GetComponent<BlackBoard>();
    blackBoard.WorldStateVariableChanged += BlackBoard_WorldStateVariableChanged;

    SetPersona();
    planner = new Planner_Goap();

    playerActions = new List<Action_Goap>{
          new PickupItem_Action(gameObject),
          new MeeleAttack_Action(gameObject),
          new EnterPortal_Action(gameObject),
          new OpenChest_Action(gameObject),
          new Drink_Action(gameObject),
          new OpenDoor_Action(gameObject),
          new Explore_Action(gameObject),
          new Action_Goap(gameObject),
        };

    playerGoals = new List<Goal_Goap>
        {
      new Loot_Goal(gameObject, planner),
      new KillEnemy_Goal(gameObject, planner),
      new Heal_Goal(gameObject, planner),
      new Explore_Goal(gameObject, planner),
      new Progress_Goal(gameObject, planner),
      new Win_Goal(gameObject, planner),
    };

    foreach (Action_Goap action in playerActions)
    {
      playerActionLookup.Add(action.ID, action);
      action.OnActionFinished += Action_ActionCallback;
    }

    viewControl.UpdateWSVariables(playerWorldState);
  }

  private void Start()
  {
    StartCoroutine(NewPlanfrst());
  }

  void BlackBoard_WorldStateVariableChanged(object sender, WsSymbolChangedEventArgs e)
  {
    //   Debug.Log("WorlstateSymbol changed " + e.Symbol + " to value " + e.Value);
    viewControl.UpdateWSVariables(playerWorldState);
    playerWorldState[e.Symbol] = e.Value;
  }


  /// <summary>
  /// Tänker att denna ska användas efter att en action är klar för att byta
  /// action/ge info om hur det gick att utföra den actionen.
  /// </summary>
  /// <param name="sender">Sender.</param>
  /// <param name="e">E.</param>
  void Action_ActionCallback(object sender, ActionFinishedEventArgs e)
  {
    viewControl.UpdateActionStatus(e.Result);
    switch (e.Result)
    {
      case ActionCallback.Successfull:
        if (plan.Count < 1)
        {
          Debug.Log("PlanQueue count was 0, need new plan!");
          //Kommentera ut StartCoroutine(NewPlan()) om du bestämma själv när agenten ska
          //göra ny plan med P.
          StartCoroutine(NewPlan());
        }
        else
        {
          Debug.Log("ActionCallback " + e.Result);
          StartCoroutine(NextAction());
        }
        break;
      case ActionCallback.Failed:
        Debug.Log("Plan failed, need new plan!");
        //Kommentera ut StartCoroutine(NewPlan()) om du bestämma själv när agenten ska
        //göra ny plan med P.
        StartCoroutine(NewPlan());
        break;
    }
  }

  /// <summary>
  /// Blir tydligare för debugningen att kolla med en paus på 0.5s
  /// </summary>
  /// <returns>The action.</returns>
  private IEnumerator NextAction()
  {
    yield return new WaitForSecondsRealtime(ActionDelay);
    if (plan.Count > 0)
    {
      if (currentAction.GetType() == typeof(MovingAction_Goap))
      {
        ((MovingAction_Goap)currentAction).Unload();
      };
      currentAction = playerActionLookup[plan.Dequeue()];
      currentAction.Enter();
    }
  }

  private IEnumerator NewPlanfrst()
  {
    yield return new WaitForSecondsRealtime(2);

    CreateNewPlan();
  }

  private IEnumerator NewPlan()
  {
    blackBoard.UpdateTargets();
    currentGoal = GetNewGoal();
    viewControl.SetGoal(currentGoal);
    plan = currentGoal.TryGetPlan(playerWorldState, playerActions);
    if (plan == null)
      Debug.LogWarning("Plan war null for: " + currentGoal.GetType());

    yield return new WaitForSecondsRealtime(ActionDelay);

    viewControl.SetPlan(plan);
    if (plan.Count > 0)
    {
      if (currentAction.GetType() == typeof(MovingAction_Goap))
      {
        ((MovingAction_Goap)currentAction).Unload();
      };
      currentAction = playerActionLookup[plan.Dequeue()];
      currentAction.Enter();
    }
  }

  void Update()
  {
    CreatePlanOnP();
  }

  void CreatePlanOnP()
  {
    if (Input.GetKeyDown(KeyCode.P))
    {
      blackBoard.UpdateTargets();
      CreateNewPlan();
    }
  }

  private void CreateNewPlan()
  {
    currentGoal = GetNewGoal();
    viewControl.SetGoal(currentGoal);

    plan = currentGoal.TryGetPlan(playerWorldState, playerActions);


    viewControl.SetPlan(plan);

    if (plan.Count > 0)
    {
      currentAction = playerActionLookup[plan.Dequeue()];
      currentAction.Enter();
    }

  }

  /// <summary>
  /// TODO Gör så att målet räknas ut från goals relevans funktion när minnet är
  /// klart.
  /// Att använda när agenten behöver ett nytt mål.
  /// </summary>
  /// <returns>The new goal.</returns>
  public Goal_Goap GetNewGoal()
  {
    Goal_Goap relevantGoal = null;
    float highestRelevance = -1;
    foreach (Goal_Goap goal in playerGoals)
    {
      float relevance = goal.CalculateRelevancy(blackBoard);

      if (relevance > highestRelevance)
      {
        highestRelevance = relevance;
        relevantGoal = goal;
      }
    }
    viewControl.SetGoalRelevancies(playerGoals);
    Debug.Log("Most Relevant goal: " + relevantGoal.GetType());
    return relevantGoal;
  }



  /// <summary>
  /// signa av från actionCallbacken
  /// </summary>
  private void OnDestroy()
  {
    blackBoard.WorldStateVariableChanged -= BlackBoard_WorldStateVariableChanged;
    foreach (var action in playerActions)
    {
      action.OnActionFinished -= Action_ActionCallback;
      if (action.GetType() == typeof(MovingAction_Goap))
      {
        ((MovingAction_Goap)action).Unload();
      }
    }
  }

  private void SetPersona()
  {
    Destroy(GetComponent<DefaultPersona>());
    switch (Settings.FetchPersona())
    {
      case Settings.Persona.Custom:
        gameObject.AddComponent<CustomPersona>();
        gameObject.GetComponent<CustomPersona>().SetValues(Settings.FetchCustomModifiers());
        persona = gameObject.GetComponent<CustomPersona>();
        InfoBox.persona = "Custom";
        break;
      case Settings.Persona.Explorer:
        //gameObject.AddComponent<ExplorerPersona>();
        InfoBox.persona = "Explorer";
        break;
      case Settings.Persona.MonsterSlayer:
        gameObject.AddComponent<MonsterSlayerPersona>();
        persona = gameObject.GetComponent<MonsterSlayerPersona>();
        InfoBox.persona = "Monster Slayer";
        break;
      case Settings.Persona.Rusher:
        gameObject.AddComponent<RusherPersona>();
        persona = gameObject.GetComponent<RusherPersona>();
        InfoBox.persona = "Rusher";
        break;
      case Settings.Persona.TreasureHunter:
        gameObject.AddComponent<TreasureHunterPersona>();
        persona = gameObject.GetComponent<TreasureHunterPersona>();
        InfoBox.persona = "Treasure Hunter";
        break;
      default:
        gameObject.AddComponent<DefaultPersona>();
        persona = gameObject.GetComponent<DefaultPersona>();
        InfoBox.persona = "Default";
        break;
    };
  }
}