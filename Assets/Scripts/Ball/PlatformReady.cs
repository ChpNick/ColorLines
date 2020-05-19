using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformReady : BaseReady
{
   public override void Operate()
   {
      Debug.Log("Нажата платформа");
      Managers.Scene.PlatformClicked(this);
   }
}
