using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SocketIO;

public class TagSceneManager : MonoBehaviour
{
    public GameObject tagItem;
    public GameObject tagItemParent;
    public Text tableName;
    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;

    GameObject[] m_tagItem;
    List<TagInfo> tagList;
    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        tagList = Global.tableGroupList[Global.cur_tInfo.tgNo].tablelist[Global.cur_tInfo.tNo].taglist;
        StartCoroutine(LoadTaglist());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float time = 0f;

    void FixedUpdate()
    {
        if (!Input.anyKey)
        {
            time += Time.deltaTime;
        }
        else
        {
            if (time != 0f)
            {
                GameObject.Find("touch").GetComponent<AudioSource>().Play();
                time = 0f;
            }
        }
    }


    IEnumerator LoadTaglist()
    {
        while (tagItemParent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(tagItemParent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }
        //UI
        m_tagItem = new GameObject[tagList.Count];
        for (int i = 0; i < m_tagItem.Length; i++)
        {
            m_tagItem[i] = Instantiate(tagItem);
            m_tagItem[i].transform.SetParent(tagItemParent.transform);
            m_tagItem[i].transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
            m_tagItem[i].transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
            float left = 0;
            float right = 0;
            m_tagItem[i].transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
            m_tagItem[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
            m_tagItem[i].transform.localScale = Vector3.one;
            try
            {
                m_tagItem[i].transform.Find("name").GetComponent<Text>().text = tagList[i].name;
                m_tagItem[i].transform.Find("no").GetComponent<Text>().text = tagList[i].id.ToString();
            }
            catch (Exception ex)
            {

            }
        }
    }

    public void onRegTag()
    {
        SceneManager.LoadScene("add_tag");
    }

    public void moveTable()
    {
        SceneManager.LoadScene("table_move");
    }

    public void onExit()
    {
        //추가된 태그 보관 조작

        SceneManager.LoadScene("main");
    }
}
