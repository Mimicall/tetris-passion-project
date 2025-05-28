using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveSprite : MonoBehaviour
{
    public Camera captureCamera;  // Assign your dedicated capture camera here
    public RenderTexture renderTexture; // Assign your RenderTexture
    public Image uiImage; // Assign the UI Image where the sprite will be displayed

    void Start()
    {
        CapturePrefab();
    }

    void CapturePrefab()
    {
        // Set the active RenderTexture
        RenderTexture.active = renderTexture;

        // Create a Texture2D with the same dimensions as the RenderTexture
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);

        // Read the pixels from the RenderTexture into the Texture2D
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Convert the Texture2D to a Sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        // Assign the sprite to the UI Image
        if (uiImage != null)
        {
            uiImage.sprite = sprite;
        }

        // Save the texture as a PNG with transparency
        SaveTextureAsPNG(texture, "CapturedPrefab.png");

        // Clean up
        //RenderTexture.active = null;
    }

    void SaveTextureAsPNG(Texture2D texture, string filename)
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Saved prefab as PNG: {path}");
    }
}