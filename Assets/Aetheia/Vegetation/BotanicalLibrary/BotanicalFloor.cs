using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Aetheia.EnvironmentArt
{
    // One renderer for the valley. All placement and surface fitting happens in the editor.
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class BotanicalFloor : MonoBehaviour
    {
        public Material foliage;
        public Mesh[] meshes;
        public Matrix4x4[] modelToPlant;
        public TextAsset planting;
        [Range(24,100)] public float viewDistance=61;
        const int BatchSize=511;
        struct Plant {public Matrix4x4 matrix;public int model;public float rank;public Vector3 position;}
        sealed class Cell {public Bounds bounds;public List<Plant> plants=new List<Plant>();}
        List<Cell> cells;
        readonly Plane[] planes=new Plane[6];
        Matrix4x4[][] batches;
        int[] counts;
        void OnEnable(){RenderPipelineManager.beginCameraRendering+=Render;}
        void OnDisable(){RenderPipelineManager.beginCameraRendering-=Render;cells=null;batches=null;}
        void Load(){
            cells=new List<Cell>();var lookup=new Dictionary<Vector2Int,Cell>();
            using(var r=new BinaryReader(new MemoryStream(planting.bytes))){
                int total=r.ReadInt32();
                for(int i=0;i<total;i++){
                    int species=r.ReadByte();var pos=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());
                    var up=new Vector3(r.ReadSByte()/127f,1,r.ReadSByte()/127f).normalized;
                    var rotation=Quaternion.FromToRotation(Vector3.up,up)*Quaternion.Euler(0,r.ReadByte()*360f/255,0);
                    float scale=r.ReadByte()/128f;
                    int family=species/3;
                    Vector3 habit=family==0?new Vector3(2.05f,1.58f,2.05f):family==1?new Vector3(1.72f,1.24f,1.72f):family==2?new Vector3(1.38f,1.10f,1.38f):family==3?new Vector3(1.55f,1.18f,1.55f):Vector3.one;
                    var matrix=Matrix4x4.TRS(pos,rotation,habit*scale)*modelToPlant[species*3];
                    var key=new Vector2Int(Mathf.FloorToInt(pos.x/12),Mathf.FloorToInt(pos.z/12));
                    if(!lookup.TryGetValue(key,out var cell)){cell=new Cell{bounds=new Bounds(pos,Vector3.one)};lookup.Add(key,cell);cells.Add(cell);}
                    cell.bounds.Encapsulate(pos);cell.plants.Add(new Plant{matrix=matrix,position=pos,model=species,rank=r.ReadByte()/255f});
                }
            }
            foreach(var cell in cells)cell.bounds.Expand(1.5f);
            batches=new Matrix4x4[meshes.Length][];counts=new int[meshes.Length];
            for(int i=0;i<batches.Length;i++)batches[i]=new Matrix4x4[BatchSize];
        }
        void Render(ScriptableRenderContext context,Camera camera){
            if(!foliage||!planting||meshes==null||meshes.Length==0||!SystemInfo.supportsInstancing)return;
            if(camera.cameraType!=CameraType.SceneView && camera.cameraType!=CameraType.Game)return;
            if((camera.cullingMask & (1<<gameObject.layer))==0)return;
            if(cells==null)Load();
            GeometryUtility.CalculateFrustumPlanes(camera,planes);Vector3 eye=camera.transform.position;
            var rp=new RenderParams(foliage){camera=camera,layer=gameObject.layer,shadowCastingMode=ShadowCastingMode.Off,receiveShadows=true,lightProbeUsage=LightProbeUsage.Off,worldBounds=new Bounds(eye,Vector3.one*(viewDistance*2+32))};
            foreach(var cell in cells){
                if(cell.bounds.SqrDistance(eye)>viewDistance*viewDistance||!GeometryUtility.TestPlanesAABB(planes,cell.bounds))continue;
                foreach(var p in cell.plants){
                    float d=(eye-p.position).sqrMagnitude;
                    if(d>viewDistance*viewDistance|| (d>18*18 && p.rank<Mathf.Sqrt(Mathf.InverseLerp(18*18,viewDistance*viewDistance,d))*.94f))continue;
                    int level=d<3.8f*3.8f?0:d<13*13?1:2;int k=p.model*3+level;
                    batches[k][counts[k]++]=p.matrix;
                    if(counts[k]==BatchSize){Graphics.RenderMeshInstanced(rp,meshes[k],0,batches[k],counts[k]);counts[k]=0;}
                }
            }
            for(int k=0;k<counts.Length;k++)if(counts[k]>0){Graphics.RenderMeshInstanced(rp,meshes[k],0,batches[k],counts[k]);counts[k]=0;}
        }
    }
}


