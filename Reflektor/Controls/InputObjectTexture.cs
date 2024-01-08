using System.Reflection;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputObjectTexture : InputObject
{
    private readonly VisualElement _container = new();
    
    private readonly VisualElement _imageSaveControls = new();
    private readonly TextField _imageSaveTextField = new();
    private readonly Button _imageSaveBtn = new();
    
    private readonly VisualElement _imagePreview = new();

    private Texture2D? _image;

    public InputObjectTexture(Info info) : base(info)
    {
        Remove(InspectBtn);
        Remove(InspectLabel);

        _container.Add(InspectBtn);
        
        _container.Add(_imageSaveControls);
        _imageSaveControls.AddToClassList("image-save-controls");
        _imageSaveControls.Add(_imageSaveTextField);
        _imageSaveTextField.label = "Save as filename";
        _imageSaveControls.Add(_imageSaveBtn);
        _imageSaveBtn.text = "Save Texture";
        _imageSaveBtn.clicked += SaveImage;
        
        _container.Add(_imagePreview);
        _imagePreview.AddToClassList("image-preview");
        
        Add(_container );
        
        _imageSaveControls.Hide();
        _imagePreview.Hide();
    }

    public override void PullChanges()
    {
        base.PullChanges();

        _image = Getter.Invoke() switch
        {
            Sprite s => s.texture,
            Texture2D t => t,
            _ => _image
        };

        _imagePreview.style.backgroundImage = _image;

        InspectBtn.text = "Preview";
        if (_image is not null)
        {
            _imageSaveTextField.value = _image.name;
            if (_image.name is not "")
            {
                InspectBtn.text += $" | {_image.name}";
            }
        }
        
        InspectBtn.clickable = null;
        InspectBtn.RegisterCallback((MouseDownEvent evt) =>
        {
            if (evt.button == 1)
            {
                SelectKey v = Key.GetSubKey(Name);
                Reflektor.Inspect(v);
            }
        });
        
        InspectBtn.clicked += () =>
        {
            _imageSaveControls.ToggleDisplay();
            _imagePreview.ToggleDisplay();
        };
    }

    private void SaveImage()
    {
        if (_image is not null)
        {
            string saveFileName = _imageSaveTextField.value is not "" ? _imageSaveTextField.value : "Untitled";
            
            string savePath =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "images");
            Directory.CreateDirectory(savePath);
            
            File.WriteAllBytes(Path.Combine(savePath, saveFileName + ".png"),  GetReadableTexture(_image).EncodeToPNG());
        }
    }

    private static Texture2D GetReadableTexture(Texture source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}