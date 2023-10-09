using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using XR_3MatchGame.Util;

namespace XR_3MatchGame_Resource
{
    public class SpriteLoader : MonoBehaviour
    {
        /// <summary>
        /// 모든 아틀라스를 담아 놓을 저장소
        /// </summary>
        private static Dictionary<AtlasType, SpriteAtlas> atlasDic = 
            new Dictionary<AtlasType, SpriteAtlas>(); 

    }
}