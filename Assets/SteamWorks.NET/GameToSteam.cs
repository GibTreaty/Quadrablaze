using UnityEngine;
using Steamworks;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace Quadrablaze {
    public class GameToSteam : MonoBehaviour {

        Dictionary<CSteamID, UserInfo> _userInfo = new Dictionary<CSteamID, UserInfo>();

        [SerializeField]
        SteamManager _steamManager;

        [SerializeField]
        Image _smallAvatarImage;

        [SerializeField]
        Image _mediumAvatarImage;

        [SerializeField]
        Image _largeAvatarImage;

        CSteamID _userSteamID;

        IEnumerator Start() {
            _steamManager.Initialize();

            while(!SteamManager.Initialized)
                yield return null;

            Initialize();
        }

        void Initialize() {
            if(_userInfo == null) _userInfo = new Dictionary<CSteamID, UserInfo>();
            else _userInfo.Clear();

            Debug.Log("Steam:" + SteamFriends.GetPersonaName());

            _userSteamID = SteamUser.GetSteamID();

            InitializeUserAvatar();
            InitializeWorkshop();
        }

        void InitializeUserAvatar() {
            GrabAvatar(_userSteamID, AvatarSize.Small);
            GrabAvatar(_userSteamID, AvatarSize.Medium);
            GrabAvatar(_userSteamID, AvatarSize.Large);

            var userInfo = GetUserInfo(_userSteamID);

            _smallAvatarImage.sprite = userInfo.smallAvatar.avatarSprite;
            _mediumAvatarImage.sprite = userInfo.mediumAvatar.avatarSprite;
            _largeAvatarImage.sprite = userInfo.largeAvatar.avatarSprite;
        }

        void InitializeWorkshop() {
            //SteamUGC
            //SteamUGC.CreateItem((AppId_t)661050, EWorkshopFileType.k_EWorkshopFileTypeGameManagedItem);
        }

        public UserInfo GetUserInfo(CSteamID userSteamID) {
            UserInfo userInfo;

            if(!_userInfo.TryGetValue(userSteamID, out userInfo)) {
                userInfo = new UserInfo();
                _userInfo[userSteamID] = userInfo;
            }

            return userInfo;
        }

        public void GrabAvatar(CSteamID userSteamID, AvatarSize avatarSize) {
            int avatarID;

            switch(avatarSize) {
                default: return;
                case AvatarSize.Small: avatarID = SteamFriends.GetSmallFriendAvatar(userSteamID); break;
                case AvatarSize.Medium: avatarID = SteamFriends.GetMediumFriendAvatar(userSteamID); break;
                case AvatarSize.Large: avatarID = SteamFriends.GetLargeFriendAvatar(userSteamID); break;
            }

            var texture = GetSteamImageAsTexture2D(avatarID);
            var avatarTexture = FlipTexture(texture);
            var userInfo = GetUserInfo(userSteamID);
            var avatar = new Avatar {
                avatarTexture = avatarTexture
            };

            Sprite sprite = Sprite.Create(avatarTexture, new Rect(0, 0, avatarTexture.width, avatarTexture.height), Vector2.zero);

            avatar.avatarSprite = sprite;

            switch(avatarSize) {
                case AvatarSize.Small: userInfo.smallAvatar = avatar; break;
                case AvatarSize.Medium: userInfo.mediumAvatar = avatar; break;
                case AvatarSize.Large: userInfo.largeAvatar = avatar; break;
            }

            DestroyImmediate(texture);
        }

        void GrabAvatar2(CSteamID userSteamID, AvatarSize avatarSize) {
            int avatarID;

            switch(avatarSize) {
                default: return;
                case AvatarSize.Small: avatarID = SteamFriends.GetSmallFriendAvatar(userSteamID); break;
                case AvatarSize.Medium: avatarID = SteamFriends.GetMediumFriendAvatar(userSteamID); break;
                case AvatarSize.Large: avatarID = SteamFriends.GetLargeFriendAvatar(userSteamID); break;
            }

            int width;
            int height;

            //if(!SteamUtils.GetImageSize(avatarID, out width, out height))
            //    return;

            switch(avatarSize) {
                default: return;
                case AvatarSize.Small: width = height = 32; break;
                case AvatarSize.Medium: width = height = 64; break;
                case AvatarSize.Large: width = height = 184; break;
            }

            int bufferSize = 4 * width * height * sizeof(char);
            byte[] imageBytes = new byte[bufferSize];

            if(SteamUtils.GetImageRGBA(avatarID, imageBytes, bufferSize)) {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);

                texture.LoadRawTextureData(imageBytes);
                texture.Apply();

                var avatarTexture = FlipTexture(texture);
                var userInfo = GetUserInfo(userSteamID);
                var avatar = new Avatar {
                    avatarTexture = avatarTexture
                };

                Sprite sprite = Sprite.Create(avatarTexture, new Rect(0, 0, width, height), Vector2.zero);

                avatar.avatarSprite = sprite;

                switch(avatarSize) {
                    case AvatarSize.Small: userInfo.smallAvatar = avatar; break;
                    case AvatarSize.Medium: userInfo.mediumAvatar = avatar; break;
                    case AvatarSize.Large: userInfo.largeAvatar = avatar; break;
                }

                DestroyImmediate(texture);
            }
        }

        public static Texture2D GetSteamImageAsTexture2D(int iImage) {
            Texture2D ret = null;
            uint imageWidth;
            uint imageHeight;
            bool bIsValid = SteamUtils.GetImageSize(iImage, out imageWidth, out imageHeight);

            if(bIsValid) {
                byte[] image = new byte[imageWidth * imageHeight * 4];

                bIsValid = SteamUtils.GetImageRGBA(iImage, image, (int)(imageWidth * imageHeight * 4));

                if(bIsValid) {
                    ret = new Texture2D((int)imageWidth, (int)imageHeight, TextureFormat.RGBA32, false, false);
                    ret.LoadRawTextureData(image);
                    ret.Apply();
                }
            }

            return ret;
        }

        static Texture2D FlipTexture(Texture2D original) {
            Texture2D flipped = new Texture2D(original.width, original.height, original.format, false, false);

            int xN = original.width;
            int yN = original.height;

            for(int x = 0; x < xN; x++)
                for(int y = 0; y < yN; y++)
                    flipped.SetPixel(x, yN - y - 1, original.GetPixel(x, y));

            flipped.Apply();

            return flipped;
        }

        public class Avatar {
            public Texture2D avatarTexture;
            public Sprite avatarSprite;
        }

        public class UserInfo {
            public Avatar smallAvatar;
            public Avatar mediumAvatar;
            public Avatar largeAvatar;
        }
    }

    public enum AvatarSize {
        Small,
        Medium,
        Large
    }
}