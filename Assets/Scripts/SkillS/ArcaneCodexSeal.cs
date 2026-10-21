using UnityEngine;
using UnityEngine.UI;

/// <summary>Static UI mesh: etched concentric rings, eight rays and an inset crystal. No textures, Update or raycasts.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ArcaneCodexSeal : MaskableGraphic
{
    protected override void Awake(){base.Awake();raycastTarget=false;}
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();var r=rectTransform.rect;var c=r.center;float radius=Mathf.Min(r.width,r.height)*.46f;
        Ring(mesh,c,radius,.7f);Ring(mesh,c,radius*.84f,.55f);Ring(mesh,c,radius*.36f,.7f);
        for(int i=0;i<8;i++)
        {
            float a=i*Mathf.PI/4;var direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Line(mesh,c+direction*radius*.42f,c+direction*radius*1.06f,i%2==0?.9f:.55f);
            var tangent=new Vector2(-direction.y,direction.x);Line(mesh,c+direction*radius*.92f-tangent*radius*.045f,c+direction*radius*.92f+tangent*radius*.045f,.8f);
        }
        var top=c+Vector2.up*radius*.26f;var bottom=c+Vector2.down*radius*.26f;var left=c+Vector2.left*radius*.16f;var right=c+Vector2.right*radius*.16f;
        Line(mesh,top,right,.8f);Line(mesh,right,bottom,.8f);Line(mesh,bottom,left,.8f);Line(mesh,left,top,.8f);Line(mesh,top,bottom,.5f);
    }
    private void Ring(VertexHelper mesh,Vector2 center,float radius,float width){for(int i=0;i<96;i++){float a=i*Mathf.PI*2/96,b=(i+1)*Mathf.PI*2/96;Line(mesh,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,width);}}
    private void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width){var normal=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;int n=mesh.currentVertCount;mesh.AddVert(a-normal,color,Vector2.zero);mesh.AddVert(a+normal,color,Vector2.zero);mesh.AddVert(b+normal,color,Vector2.zero);mesh.AddVert(b-normal,color,Vector2.zero);mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);}
}
