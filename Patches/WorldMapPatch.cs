using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using Lamb.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ForgottenSins.Patches;

[HarmonyPatch]
public class WorldMapPatch
{
    [HarmonyPatch(typeof(UIWorldMapMenuController), "Start")]
    [HarmonyPrefix]
    public static void StartPrefix(UIWorldMapMenuController __instance)
    {
        Transform artLayer = __instance.transform.Find("WorldMapMenuContainer").Find("Map Mask").Find("Map Container")
            .Find("Layers");
        
        // Create parent object
        GameObject hole = new GameObject("Hole");
        hole.transform.SetParent(artLayer, false);
        hole.transform.SetSiblingIndex(1);
        
        GameObject holeArt = new GameObject("HoleArt");
        holeArt.transform.SetParent(hole.transform, false);
        
        // Load image
        try 
        {
            // Load texture from your plugin folder
            string imagePath = Path.Combine(Plugin.PluginPath, "hole.png");
    
            if (File.Exists(imagePath))
            {
                byte[] imageData = File.ReadAllBytes(imagePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
        
                // Create sprite from texture
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        
                // Add to image
                Image image = holeArt.AddComponent<Image>();
                image.sprite = sprite;
                image.color = Color.white; // White to show actual image colors
            }
            else
            {
                Plugin.Log.LogError($"Image not found at: {imagePath}");
                // Fallback to colored square
                holeArt.AddComponent<Image>().color = Color.red;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to load image: {ex.Message}");
            holeArt.AddComponent<Image>().color = Color.red; // Fallback
        }
        
        // Set size and position
        RectTransform squareTransform = holeArt.GetComponent<RectTransform>();
        // Image is 200x121
        float scaleFactor = 1.3f;
        squareTransform.sizeDelta = new Vector2(200f * scaleFactor, 121f * scaleFactor);
        squareTransform.anchoredPosition = new Vector2(-1260f, 300f);
        
        // Add parallax to parent
        GameObject parallaxController = __instance.transform.Find("WorldMapMenuContainer").Find("Map Mask")
            .Find("Parallax Controller").gameObject;
        ParallaxLayer parallaxLayer = hole.AddComponent<ParallaxLayer>();
        parallaxLayer._distance = 30;
        parallaxLayer._rectTransform = hole.GetComponent<RectTransform>();
        WorldMapParallax mapParallax = parallaxController.GetComponent<WorldMapParallax>();
        mapParallax._layers = mapParallax._layers.Concat([parallaxLayer]).ToArray();
        
    }
}