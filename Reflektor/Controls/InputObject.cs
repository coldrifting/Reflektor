using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputObject : InputBase
{
    protected readonly Button InspectBtn = new();
    protected readonly Label InspectLabel = new();
    
    public InputObject(Info info) : base (info)
    {
        Add(InspectBtn);
        Add(InspectLabel);
    }

    public override void PullChanges()
    {
        object? inspect = Getter.Invoke();

        if (inspect == null)
        {
            InspectBtn.text = "null";
            return;
        }
        
        InspectLabel.text = inspect.GetType().ToString();

        InspectBtn.text = !IsInsideCollection && inspect == Key.Target ? "this" : GetName(inspect);

        InspectBtn.clickable = null;
        InspectBtn.clicked += () =>
        {
            SelectKey v = Key.GetSubKey(Name);
            Reflektor.Inspect(v);
        };
    }

    private static string GetName(object obj)
    {
        if (obj is UnityEngine.Object o)
        {
            return o.name;
        }

        string objectString = obj.ToString();
        Type objectType = obj.GetType();
        
        return objectString.Equals(objectType.FullName) || objectString.Contains("\n")
            ? objectType.Name 
            : objectString;
    }
}