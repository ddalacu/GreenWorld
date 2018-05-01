using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PictureProvider : WorldMessageProvider
{
    public Camera pictureCamera;

    public override int GetTypeIdentifier()
    {
        return 3;
    }

    public override void HandleMessage(GreenWorld world, GreenWorld.AdapterListener adapterListener, byte[] data)
    {
        Debug.Log(Encoding.ASCII.GetString(data));

        pictureCamera.forceIntoRenderTexture = true;
        pictureCamera.targetTexture =  RenderTexture.GetTemporary(pictureCamera.pixelWidth, pictureCamera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        pictureCamera.enabled = true;
        pictureCamera.Render();
        pictureCamera.enabled = false;

        RenderTexture.active = pictureCamera.targetTexture;
        Texture2D virtualPhoto = new Texture2D(pictureCamera.pixelWidth, pictureCamera.pixelHeight, TextureFormat.RGB24, false);

        virtualPhoto.ReadPixels(new Rect(0, 0, pictureCamera.pixelWidth, pictureCamera.pixelHeight), 0, 0);
        virtualPhoto.Apply(false, false);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(pictureCamera.targetTexture);

       // world.SendWorldMessage(adapterListener, new byte[] { 1}, GetTypeIdentifier());

        world.SendWorldMessage(adapterListener, virtualPhoto.EncodeToPNG(), GetTypeIdentifier());
        DestroyImmediate(virtualPhoto,true);
    }
}
