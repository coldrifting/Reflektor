using UnityEngine.UIElements;

namespace Reflektor.Extensions;

public static class WindowExtensions
{
    public static bool IsVisible(this VisualElement element)
    {
        return element.style.display == new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
    }
    
    public static void SetVisible(this VisualElement element, bool setVisible)
    {
        element.style.display = new StyleEnum<DisplayStyle>(setVisible ? DisplayStyle.Flex : DisplayStyle.None);
    }

    public static void ToggleVisible(this VisualElement element)
    {
        element.SetVisible(!element.IsVisible());
    }
    
    public static bool IsVisible(this UIDocument document)
    {
        return document.rootVisualElement.visible;
    }
    
    public static void SetVisible(this UIDocument document, bool setVisible)
    {
        document.rootVisualElement.visible = setVisible;
    }

    public static void ToggleVisible(this UIDocument document)
    {
        document.rootVisualElement.visible = !document.rootVisualElement.visible;
    }
}