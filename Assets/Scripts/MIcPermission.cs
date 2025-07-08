using UnityEngine;
using System.Collections;
using System.Collections.Generic;

# if PlatformAndroid
  
using UnityEngine.Android;

#endif

public class MIcPermission : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
#if PLATFORMANDROID

    if(!Permission.HasUserAuthorizedPermission(Permission.Microphone))
    {
        Permission.RequestUserPermission(Permission.Microphone);
    }

#endif
    }
}
