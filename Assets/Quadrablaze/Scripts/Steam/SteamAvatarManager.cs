using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using Quadrablaze;

public class SteamAvatarManager : MonoBehaviour {

    public static SteamAvatarManager Current { get; private set; }

    Dictionary<CSteamID, UserAvatars> _userInfo = new Dictionary<CSteamID, UserAvatars>();

    private void OnEnable() {
        Current = this;
    }

    public void ClearAvatars() {
        foreach(var user in _userInfo.Values) {
            user.SmallAvatar?.DestroyAvatar();
            user.MediumAvatar?.DestroyAvatar();
            user.LargeAvatar?.DestroyAvatar();
        }
    }

    public UserAvatars GetAvatars(CSteamID steamId, SteamAvatarSize reloadAvatar = SteamAvatarSize.None) {
        UserAvatars user;

        if(!_userInfo.TryGetValue(steamId, out user)) {
            user = new UserAvatars();
            _userInfo[steamId] = user;
        }

        GrabAvatar(user, steamId, reloadAvatar);

        return user;
    }

    void GrabAvatar(UserAvatars avatars, CSteamID userSteamID, SteamAvatarSize avatarSize) {
        if(avatarSize.HasFlag(SteamAvatarSize.Small)) {
            avatars.SmallAvatar?.DestroyAvatar();
            avatars.SmallAvatar = MakeAvatar(SteamFriends.GetSmallFriendAvatar(userSteamID));
        }

        if(avatarSize.HasFlag(SteamAvatarSize.Medium)) {
            avatars.MediumAvatar?.DestroyAvatar();
            avatars.MediumAvatar = MakeAvatar(SteamFriends.GetMediumFriendAvatar(userSteamID));
        }

        if(avatarSize.HasFlag(SteamAvatarSize.Large)) {
            avatars.LargeAvatar?.DestroyAvatar();
            avatars.LargeAvatar = MakeAvatar(SteamFriends.GetLargeFriendAvatar(userSteamID));
        }
    }

    SteamAvatar MakeAvatar(int avatarId) {
        var texture = GetSteamImageAsTexture2D(avatarId);

        if(texture == null) return null;

        var avatarTexture = FlipTexture(texture);
        var avatar = new SteamAvatar {
            avatarTexture = avatarTexture
        };

        var sprite = Sprite.Create(avatarTexture, new Rect(0, 0, avatarTexture.width, avatarTexture.height), Vector2.zero);

        avatar.avatarSprite = sprite;

        DestroyImmediate(texture);

        return avatar;
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

    static Texture2D GetSteamImageAsTexture2D(int iImage) {
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

    [System.Flags]
    public enum SteamAvatarSize {
        None = 0,
        Small = 1,
        Medium = 2,
        Large = 4
    }
}

public class SteamAvatar {
    public Texture2D avatarTexture;
    public Sprite avatarSprite;

    public void DestroyAvatar() {
        Object.Destroy(avatarSprite);
        Object.Destroy(avatarTexture);
    }
}

public class UserAvatars {
    SteamAvatar _smallAvatar = null;
    SteamAvatar _mediumAvatar = null;
    SteamAvatar _largeAvatar = null;

    #region Properties
    public SteamAvatar LargeAvatar {
        get { return _largeAvatar; }
        set { _largeAvatar = value; }
    }

    public SteamAvatar MediumAvatar {
        get { return _mediumAvatar; }
        set { _mediumAvatar = value; }
    }

    public SteamAvatar SmallAvatar {
        get { return _smallAvatar; }
        set { _smallAvatar = value; }
    }
    #endregion
}