﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Action_Goap
{
  public virtual event EventHandler<ActionFinishedEventArgs> OnActionFinished;

  protected readonly GameObject owner;
  protected bool PreconditionsMet;

  /// <summary>
  /// Dessa tre måste sättas i konstruktorn i varje action.
  /// </summary>
  public int cost = 1;
  public ActionID ID;
  public WorldStateSymbol[] Effects { get; protected set; }
  public WorldStateSymbol[] PreConditions { get; protected set; }

  public Action_Goap(GameObject owner)
  {
    Effects = new WorldStateSymbol[0];
    PreConditions = new WorldStateSymbol[0];
    this.owner = owner;
  }

  public virtual void Enter()
  {

    Debug.Log("Entered: " + ID);

    if (CanExecute())
    {
      Execute();
    }
    else
      Failed();

  }

  protected virtual bool CanExecute()
  {
    if (!PreconditionsSatisfied())
    {
      Debug.Log("All preconditions was not satisfied on entering " + ID);
      //Failed();
      return false;
    }

    return true;
  }

  public virtual void Execute() { Debug.Log("Executing " + ID); }

  protected virtual void Exit() { }

  protected virtual void Failed()
  {
 
    Debug.Log(ID + " was a failure");
    OnActionFinished.Invoke(this, new ActionFinishedEventArgs(ActionCallback.Failed));
  }

  protected virtual void Successfull()
  {
    Debug.Log(ID + " was sucessful");
    OnActionFinished.Invoke(this, new ActionFinishedEventArgs(ActionCallback.Successfull));
  }


  public WorldStateSet ApplyEffects(WorldStateSet worldState)
  {
    WorldStateSet appliedWorldState = (WorldStateSet)worldState.Clone();

    foreach (WorldStateSymbol effect in Effects)
    {
      appliedWorldState[effect] = true;
    }
    return appliedWorldState;
  }

  public bool IsValidInWorldState(WorldStateSet worldState)
  {
    foreach (WorldStateSymbol precondition in PreConditions)
    {
      if (!worldState[precondition])
        return false;
    }
    return true;
  }

  protected bool PreconditionsSatisfied()
  {
    var agentWorldState = owner.GetComponent<Goap_Controller>().PlayerWorldState;
    foreach(WorldStateSymbol symbol in PreConditions)
    {
      if (!agentWorldState[symbol])
      {
        return false;
      }
    }
    return true;
  }

  public virtual float GetCost()
  {
    return cost;
  }
}



