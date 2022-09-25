using UnityEngine;

namespace DG.Util
{
    public class ScreenshotTool : MonoBehaviour
    {
        // Wait untile end of frame before calling
        public static Texture2D TakeScreenshot()
        {

            // Create a texture the size of the screen, RGBA32 format
            int width = Screen.width;
            int height = Screen.height;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            //tex.alphaIsTransparency = true;
            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            tex.Apply(false);
            return tex;
        }
    }
}