using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MapEditor_RightPanel : MonoBehaviour
{
    public GameObject pnl;
    public Toggle toggle;
    public Transform mapParent;
    public GameObject drop_down;

    Dropdown dropDownItem;
    GameObject lastPlant;

    string m_strPrefabPath = "Assets/Resources/Prefabs/";
    List<string> m_strItemNameList;



    void Start()
    {
        if (toggle && pnl)
        {
            pnl.SetActive(toggle.isOn);

            if (drop_down)
            {
                m_strItemNameList = new List<string>();
                dropDownItem = drop_down.GetComponent<Dropdown>();

                GetAllPrefabsName();
                UpdateDropDownItem(m_strItemNameList);
            }

            toggle.onValueChanged.AddListener(delegate
            {
                ToggleValueChanged(toggle);
            });
        }
    }


    void GetAllPrefabsName()
    {
        if (Directory.Exists(m_strPrefabPath))
        {
            DirectoryInfo direction = new DirectoryInfo(m_strPrefabPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".meta"))
                {
                    continue;
                }
                string strName = GetPrefabsOriginName(files[i].Name);
                if (strName != "")
                {
                    m_strItemNameList.Add(strName);
                }
               
            }
        }  
    }

    string GetPrefabsOriginName( string strName )
    {
        if (strName == "")
            return "";

        int nIndex = 0;
        nIndex = strName.LastIndexOf(".");
        return strName.Substring(0, nIndex);
    }

    public void ToggleValueChanged(Toggle change)
    {
        pnl.SetActive(change.isOn);
        drop_down.SetActive(change.isOn);

        if (!change.isOn && lastPlant)
        {
            Destroy(lastPlant);
        }
    }

    void AddPrefab()
    {
        if (mapParent && dropDownItem.value < m_strItemNameList.Count)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + m_strItemNameList[dropDownItem.value]);
            lastPlant = GameObject.Instantiate(prefab);
            lastPlant.AddComponent<Operate_SceneObj>();
            lastPlant.transform.SetParent(mapParent);
        }
    }


    void UpdateDropDownItem(List<string> showNames)
    {
        dropDownItem.options.Clear();
        Dropdown.OptionData temoData;
        for (int i = 0; i < showNames.Count; i++)
        {
            //给每一个option选项赋值
            temoData = new Dropdown.OptionData();
            temoData.text = showNames[i];
            dropDownItem.options.Add(temoData);
        }
        //初始选项的显示
        dropDownItem.captionText.text = showNames[0];

    }

    public void OnDropDownValueChange()
    {
        if (lastPlant)
        {
            Destroy(lastPlant);
        }
        AddPrefab();
    }


    void Update()
    {
        if (toggle && toggle.isOn)
        {
            if (lastPlant)
            {
                Operate_SceneObj obj = lastPlant.GetComponent<Operate_SceneObj>();
                if (obj.IsFinish())
                {
                    if(!obj.IsInNormalArea())
                    {
                        Destroy(lastPlant);
                    }
                    
                    AddPrefab();
                }
            }
            else
            {
                AddPrefab();
            }
        }
        
    }
}
