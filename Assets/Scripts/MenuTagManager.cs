using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SocketIO;


public class MenuTagManager : MonoBehaviour
{
    public GameObject menuItem;
    public GameObject categoryItem;
    public GameObject menuItemParent;
    public GameObject categoryItemParent;
    public Text tagName;
    public Text remainTxt;
    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;

    int totalCategoryCnt = -1;
    GameObject[] m_MenuItem;
    GameObject[] m_categoryItem;

    string firscateno = "";
    string oldSelectedCategoryNo = "";
    bool loading = false;

    public Text sel_amount;
    public Text sel_price;
    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Global.categorylist.Clear();
        StartCoroutine(LoadCategoryList());
        LoadCartList();
        tagName.text = Global.cur_tagInfo.tag_name;
        remainTxt.text = Global.GetPriceFormat(Global.cur_tagInfo.remain);
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


    void LoadCartList()
    {
        //이미 잇는 주문카트내역을 조회해서 로드.
        Global.ordercart_totalprice = 0;
        Global.ordercart_amount = 0;
        for (int i = 0; i < Global.mycartlist.Count; i++)
        {
            Global.ordercart_amount += Global.mycartlist[i].amount;
            Global.ordercart_totalprice += Global.mycartlist[i].price * Global.mycartlist[i].amount;
        }
        try
        {
            sel_amount.text = Global.ordercart_amount.ToString();
            sel_price.text = Global.GetPriceFormat(Global.ordercart_totalprice);
        }
        catch (Exception)
        {

        }
    }

    IEnumerator LoadCategoryList()
    {
        //clear category list
        Debug.Log("clear old ui list");
        while (categoryItemParent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(categoryItemParent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }
        while (menuItemParent.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(menuItemParent.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {

            }
            yield return new WaitForSeconds(0.001f);
        }

        //태블릿에서 이용가능한 모든 카테고리에 대한 메뉴리스트정보 통채로 json구조로 가져오기
        WWWForm form = new WWWForm();
        WWW www = new WWW(Global.api_url + Global.get_categorylist_api, form);
        Debug.Log(Global.api_url + Global.get_categorylist_api);
        StartCoroutine(GetCategorylistFromApi(www));
    }

    IEnumerator GetCategorylistFromApi(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            JSONNode c_list = JSON.Parse(jsonNode["categorylist"].ToString()/*.Replace("\"", "")*/);
            //json구조 해석해서 Global에 값을 대입
            Debug.Log("category list count = " + c_list.Count);
            totalCategoryCnt = c_list.Count;
            try
            {
                firscateno = c_list[0]["id"];
            }
            catch (Exception ex)
            {

            }
            for (int i = 0; i < totalCategoryCnt; i++)
            {
                Debug.Log("loading category list..");
                CategoryInfo cateInfo = new CategoryInfo();
                try
                {
                    cateInfo.name = c_list[i]["name"];
                    cateInfo.id = c_list[i]["id"];
                }
                catch (Exception ex)
                {

                }
                cateInfo.menulist = new List<MenuInfo>();
                JSONNode m_list = JSON.Parse(c_list[i]["menulist"].ToString()/*.Replace("\"", "")*/);
                int menuCnt = m_list.Count;
                for (int j = 0; j < menuCnt; j++)
                {
                    MenuInfo minfo = new MenuInfo();
                    try
                    {
                        minfo.name = m_list[j]["name"];
                        minfo.menu_id = m_list[j]["id"];
                        minfo.price = m_list[j]["price"];
                        minfo.is_best = m_list[j]["is_best"];

                        if (m_list[j]["is_soldout"] == 1)
                        {
                            minfo.is_soldout = true;
                        }
                        else
                        {
                            minfo.is_soldout = false;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    cateInfo.menulist.Add(minfo);
                }
                Global.categorylist.Add(cateInfo);
            }

            //UI에 추가
            m_categoryItem = null;
            m_categoryItem = new GameObject[totalCategoryCnt];
            for (int i = 0; i < totalCategoryCnt; i++)
            {
                m_categoryItem[i] = Instantiate(categoryItem);
                m_categoryItem[i].transform.SetParent(categoryItemParent.transform);

                m_categoryItem[i].transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
                m_categoryItem[i].transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
                float left = 0;
                float right = 0;
                m_categoryItem[i].transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
                m_categoryItem[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
                m_categoryItem[i].transform.localScale = Vector3.one;

                try
                {
                    m_categoryItem[i].GetComponent<Text>().text = Global.categorylist[i].name;
                    m_categoryItem[i].transform.Find("no").GetComponent<Text>().text = Global.categorylist[i].id.ToString();
                    string t_cateNo = Global.categorylist[i].id;
                    m_categoryItem[i].GetComponent<Button>().onClick.AddListener(delegate () { StartCoroutine(LoadMenuList(t_cateNo)); });
                }
                catch (Exception ex)
                {

                }
            }

            if (!loading && totalCategoryCnt > 0 && firscateno != "")
                StartCoroutine(LoadMenuList(firscateno));
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    IEnumerator Destroy_Object(GameObject obj)
    {
        DestroyImmediate(obj);
        yield return null;
    }

    IEnumerator LoadMenuList(string cateno)
    {
        Debug.Log("old = " + oldSelectedCategoryNo + ", new = " + cateno);
        //선택된 카테고리 볼드체로.
        try
        {
            for (int i = 0; i < categoryItemParent.transform.childCount; i++)
            {
                if (categoryItemParent.transform.GetChild(i).Find("no").GetComponent<Text>().text == oldSelectedCategoryNo.ToString())
                {
                    categoryItemParent.transform.GetChild(i).GetComponent<Text>().color = Color.white;
                }
                if (categoryItemParent.transform.GetChild(i).Find("no").GetComponent<Text>().text == cateno.ToString())
                {
                    categoryItemParent.transform.GetChild(i).GetComponent<Text>().color = Color.yellow;
                }
            }
        }
        catch (Exception ex)
        {

        }
        //UI 내역 초기화
        while (menuItemParent.transform.childCount > 0)
        {
            StartCoroutine(Destroy_Object(menuItemParent.transform.GetChild(0).gameObject));
        }
        while(menuItemParent.transform.childCount > 0)
        {
            yield return new WaitForSeconds(0.01f);
        }

        oldSelectedCategoryNo = cateno;

        //카테고리에 한한 메뉴리스트 가져오기
        List<MenuInfo> minfoList = new List<MenuInfo>();
        for (int i = 0; i < Global.categorylist.Count; i++)
        {
            if (Global.categorylist[i].id == cateno)
            {
                minfoList = Global.categorylist[i].menulist; break;
            }
        }
        int menuCnt = minfoList.Count;
        m_MenuItem = new GameObject[menuCnt];
        for (int i = 0; i < menuCnt; i++)
        {
            try
            {
                m_MenuItem[i] = Instantiate(menuItem);
                m_MenuItem[i].transform.SetParent(menuItemParent.transform);
                m_MenuItem[i].transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
                m_MenuItem[i].transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
                float left = 0;
                float right = 0;
                m_MenuItem[i].transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
                m_MenuItem[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
                m_MenuItem[i].transform.localScale = Vector3.one;

                m_MenuItem[i].transform.Find("no").GetComponent<Text>().text = minfoList[i].menu_id.ToString();
                m_MenuItem[i].transform.Find("name").GetComponent<Text>().text = minfoList[i].name;
                m_MenuItem[i].transform.Find("price").GetComponent<Text>().text = Global.GetPriceFormat(minfoList[i].price);

                OrderCartInfo cinfo = new OrderCartInfo();
                cinfo.name = minfoList[i].name;
                cinfo.menu_id = minfoList[i].menu_id;
                cinfo.price = minfoList[i].price;
                cinfo.is_best = minfoList[i].is_best;
                cinfo.amount = 1;
                cinfo.status = 0;
                if (minfoList[i].is_soldout)
                {
                    m_MenuItem[i].transform.Find("name").GetComponent<Text>().color = Color.black;
                    m_MenuItem[i].transform.Find("price").GetComponent<Text>().color = Color.black;
                }
                else
                {
                    m_MenuItem[i].GetComponent<Button>().onClick.AddListener(delegate () { addList(cinfo); });
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
            loading = true;
            yield return new WaitForSeconds(0.001f);
        }
    }

    public void addList(OrderCartInfo cinfo)
    {
        //my cart list에 추가
        Debug.Log("add list");
        Global.addOneCartItem(cinfo);
        sel_amount.text = Global.ordercart_amount.ToString();
        sel_price.text = Global.GetPriceFormat(Global.ordercart_totalprice);
    }

    public void onExit()
    {
        Global.mycartlist.Clear();
        Global.ordercart_amount = 0;
        Global.ordercart_totalprice = 0;
        SceneManager.LoadScene("main");
    }

    public void MyOrder()
    {
        Global.order_type = 0;
        SceneManager.LoadScene("mytagorder");
    }

    public void OrderCart()
    {
        Global.order_type = 0;
        SceneManager.LoadScene("ordercart");
    }
}
