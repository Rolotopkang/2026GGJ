// ******************************************************************
//       /\ /|       @file       FILENAME
//       \ V/        @brief      
//       | "")       @author     topkang
//       /  |                    
//      /  \\        @Modified   DATE
//    *(__\_\        @Copyright  Copyright (c) YEAR, TOPGAMING
// ******************************************************************

using UnityEngine;

namespace Tools
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;
 
        public static T GetInstance()
        {
            return instance;
        }

        protected virtual void Awake()
        {
            if(instance !=null )
            {
                Destroy(gameObject);
            }
            else
            {
                instance = (T)this;
            }
        }

        public static bool IsInitialized
        {
            get { return instance != null; }
        }

        protected void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}