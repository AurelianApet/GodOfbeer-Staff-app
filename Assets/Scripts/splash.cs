using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
public class splash : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        if(PlayerPrefs.GetString("ip") == "")
        {
            yield return new WaitForSeconds(0.1f);
            SceneManager.LoadScene("setting");
        }
        Global.setInfo.bus_id = PlayerPrefs.GetString("bus_id");
        Global.setInfo.staff_no = PlayerPrefs.GetInt("staff_no");
        Global.setInfo.market_name = PlayerPrefs.GetString("mark_name");
        Global.server_address = PlayerPrefs.GetString("ip");
        Global.api_url = "http://" + Global.server_address + ":" + Global.api_server_port + "/";
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("main");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
