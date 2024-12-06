using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace rei
{
    public class CampFireManager : MonoBehaviour
    {
        public List<CampFire> CampFires = new List<CampFire>();
        
        public CampFire lastCampFire;
        
        private void Start()
        {
            foreach (Transform child in transform)
            {
                // 检查子物体是否有 campfire 组件
                CampFire campFire = child.GetComponent<CampFire>();
                if (campFire != null)
                { 
                    // 如果有，就添加到列表中
                    CampFires.Add(campFire);
                }
            }
        }

        
        //获取所有已经点燃过了的火
        public List<CampFire> GetActiveCampFire()
        {
            List<CampFire> activeCampFire = new List<CampFire>();
            foreach (var campfire in CampFires)
            {
                if (campfire.active)
                    activeCampFire.Add(campfire);
            }
            return activeCampFire;
        }

        //获取正在坐的火
        public CampFire GetSittingCampFire()
        {
            CampFire sittingCampFire;
            foreach (var campfire in CampFires)
            {
                if (campfire.sitting)
                {
                    return campfire;
                    break;
                }
            }

            return null;
        }
        
        public static CampFireManager instance;

        void Awake()
        {
            instance = this;
        }
    }
}