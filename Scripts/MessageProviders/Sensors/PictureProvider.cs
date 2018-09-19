using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GreenProject.Messages;
using JetBrains.Annotations;
using UnityEngine;

public class PictureProvider : MonoBehaviour
{
    public Camera PictureCamera;
    public GreenWorld GreenWorld;

    public int PictureWidth = 450;
    public int PictureHeight = 450;


    [UsedImplicitly]
    private void Awake()
    {
        GreenWorld.AddMessageListener(PictureRequest, typeof(PictureRequestMessage));
    }

    private void PictureRequest(GreenWorld.AdapterListener adapter, INetworkMessage message)
    {
        Debug.Log("Picture requested");

        PictureCamera.forceIntoRenderTexture = true;
        PictureCamera.targetTexture = RenderTexture.GetTemporary(PictureWidth, PictureHeight, 0, RenderTextureFormat.ARGB32);
        // PictureCamera.enabled = true;
        PictureCamera.Render();
        // PictureCamera.enabled = false;

        RenderTexture.active = PictureCamera.targetTexture;
        Texture2D virtualPhoto = new Texture2D(PictureWidth, PictureHeight, TextureFormat.ARGB32, false);

        virtualPhoto.ReadPixels(new Rect(0, 0, PictureWidth, PictureHeight), 0, 0);
        virtualPhoto.Apply(false, false);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(PictureCamera.targetTexture);

        GreenWorld.SendMessage(adapter, new PictureResponseMessage(virtualPhoto.EncodeToPNG()));
        DestroyImmediate(virtualPhoto, true);
        PictureCamera.targetTexture = null;
    }
}
