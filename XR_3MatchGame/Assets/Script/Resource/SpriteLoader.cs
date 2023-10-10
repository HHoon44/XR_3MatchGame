using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using XR_3MatchGame.Util;

namespace XR_3MatchGame_Resource
{
    public static class SpriteLoader
    {
        /// <summary>
        /// 모든 아틀라스를 담아 놓을 저장소
        /// </summary>
        private static Dictionary<AtlasType, SpriteAtlas> atlasDic =
            new Dictionary<AtlasType, SpriteAtlas>();

        /// <summary>
        /// 폴더에 존재하는 아틀라스를 전부 가져와서
        /// 저장소에 저장하는 메서드
        /// </summary>
        /// <param name="atlases">폴더에 존재하는 아틀라스</param>
        public static void SetAtlas(SpriteAtlas[] atlases)
        {
            for (int i = 0; i < atlases.Length; i++)
            {
                var key = (AtlasType)Enum.Parse(typeof(AtlasType), atlases[i].name);

                atlasDic.Add(key, atlases[i]);
            }
        }

        /// <summary>
        /// 요청한 아틀라에서 원하는 스프라이트를
        /// 찾아 반환하는 메서드
        /// </summary>
        /// <param name="type">원하는 아틀라스</param>
        /// <param name="spriteName">원하는 스프라이트 이름</param>
        /// <returns></returns>
        public static Sprite GetSprite(AtlasType type, string spriteName)
        {
            if (!atlasDic.ContainsKey(type))
            {
                // 원하는 스프라이트가 없다면 종료한다
                return null;
            }

            // 원하는 스프라이트를 가져오기 위해서
            // 저장소에 접근하여 아틀라스를 가져와
            // 아틀라스가 지닌 GetSprite라는 함수를 이용해서
            // 스프라이트를 가져온다
            return atlasDic[type].GetSprite(spriteName);
        }

    }
}