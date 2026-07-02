using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[Serializable]
public class MessageBase
{
  public string type;
}

[Serializable]
public class PlayVfxMessage : MessageBase
{
  public string vfx;
}

public class WebSocketMsgHandler : MonoBehaviour
{

  private List<VisualEffect> particleSystems;

  void Start()
  {
    var foundParticleSystems = FindObjectsOfType<VisualEffect>(true);
    this.particleSystems = new List<VisualEffect>(foundParticleSystems);
  }

  public void OnMessage(string msg)
  {
    var msgWithType = JsonUtility.FromJson<MessageBase>(msg);
    if (msgWithType == null || string.IsNullOrEmpty(msgWithType.type))
    {
      Debug.LogError($"WebSocket message does not contain 'type' field: '{msg}'");
      return;
    }
    var type = msgWithType.type;

    switch (type)
    {
      case "play-vfx":
        {
          PlayVfx(msg);
          break;
        }
      default:
        {
          Debug.LogError($"WebSocket message contains unknown 'type' field: '{type}'");
          break;
        }
    }
  }

  public void PlayVfx(string msg)
  {
    var vfxMsg = JsonUtility.FromJson<PlayVfxMessage>(msg);
    var vfxName = vfxMsg.vfx;
    Debug.Log($"Playing vfx: '{vfxName}'");

    // FIX: Execute Play() on the intended system without destroying ambient/background effects
    foreach (var x in particleSystems)
    {
      if (x.name == vfxName)
      {
        x.gameObject.SetActive(true);
        x.Play(); 
      }
    }
  }
}