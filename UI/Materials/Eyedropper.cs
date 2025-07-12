using UnityEngine;

namespace CarolCustomizer.UI.Materials;
internal class Eyedropper : MonoBehaviour
{
    void Update()
    {
        //if mouse position has changed

        //if 
    }

    void Raycast()
    {
        var mousePos = 
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        Physics.Raycast(ray, out RaycastHit hit, 1000f);
    }
}
