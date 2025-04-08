using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class UIGradient : BaseMeshEffect
{
    public Color gradientStart = Color.white;
    public Color gradientEnd = new Color(1, 1, 1, 0);
    
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        UIVertex vertex = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            float xPercent = vertex.position.x / GetComponent<RectTransform>().rect.width + 0.5f;
            vertex.color *= Color.Lerp(gradientStart, gradientEnd, xPercent);
            vh.SetUIVertex(vertex, i);
        }
    }
}
