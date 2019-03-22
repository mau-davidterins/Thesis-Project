﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InfoBox {
    public static string targetTile = "";
    public static string playerTile = "";
    public static int pathLength = 0;
    public static int stepsLeft = 0;
    private static string memory = "";
    public static int hp = 100;
    public static int coins;

    /// <summary>
    /// Add to memory and trim the "(Clone)"-part
    /// </summary>
    /// <param name="container">Item to add</param>
    public static void UpdateMemory(Dictionary<TileType, List<GameObject>> container) {
        memory = "";
        foreach (KeyValuePair<TileType, List<GameObject>> item in container) {
            foreach(GameObject go in item.Value) {
                memory += "\n" + go.name.Substring(0, go.name.Length - 7);
            }
        }
    }

    public static string GetMemory() {
        return memory;
    }
}