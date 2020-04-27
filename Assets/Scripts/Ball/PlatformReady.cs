using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformReady : MonoBehaviour
{
   private void OnMouseDown()
   {
      Debug.Log("Нажата платформа");
      Managers.Scene.PlatformClicked(this);
   }
}
