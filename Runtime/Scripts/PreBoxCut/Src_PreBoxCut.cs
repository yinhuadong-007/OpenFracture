using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class Src_PreBoxCut : MonoBehaviour
{
    [Tooltip("每个维度切割的数量")] public Vector3 cutCount;
    [Tooltip("是否显示辅助线")] public bool showHelp = true;
    public SliceOptions sliceOptions;

    public CallbackOptions callbackOptions;

    /// <summary>
    /// The number of times this fragment has been re-sliced.
    /// </summary>
    private int currentSliceCount;

    /// <summary>
    /// Collector object that stores the produced fragments
    /// </summary>
    private static GameObject fragmentRoot;

    public GameObject cube;

    public PreBoxCutOptions rreBoxCutOptions;

    private static int CurCutCount = 0;
    // [Tooltip("从底部往上切割，模型锚点相对于模型底部的位置偏移，默认位置在模型空间（0,0,0）")] public Vector3 offsetToAnchor;
    // [Tooltip("切割道具的范围")] public Vector3 cutterExtents;

    /// <summary>
    /// Compute the fracture and create the fragments
    /// </summary>
    /// <returns></returns>
    [ExecuteInEditMode]
    [ContextMenu("PreBoxCut")]
    public void ComputeBoxCut()
    {
        // This method should only be called from the editor during design time
        if (!Application.isEditor || Application.isPlaying) return;
        CurCutCount = 0;

        //模型中心点和范围
        var mesh = this.GetComponent<MeshFilter>().sharedMesh;
        var center = mesh.bounds.center;
        var extents = mesh.bounds.extents;
        extents = new Vector3(extents.x * this.transform.localScale.x,
                                extents.y * this.transform.localScale.y,
                                extents.z * this.transform.localScale.z);

        Debug.Log("center= " + center + ", extents= " + extents);
        //y方向切割 - 切割物平面与被切割物在y方向垂直
        //每次切割的距离
        float distanceCat = (extents.y / (cutCount.y + 1) * 2);
        for (int i = 0; i < cutCount.y; i++)
        {
            Vector3 originPos = this.transform.position;
            originPos.y += center.y - extents.y + distanceCat * (i + 1);
            originPos.x -= center.x - extents.x;
            originPos.z += center.z;

            Vector3 cutterExtents = extents * 2;
            cutterExtents.y = 0.02f;
            // cutterExtents.x += extents.x * 2;
            // cutterExtents.z += extents.z * 2;
            float distance = extents.x * 4;
            // Cast a ray and find the nearest object
            RaycastHit[] hits = Physics.BoxCastAll(originPos, cutterExtents, Vector3.right, this.transform.rotation, distance);

            CutOnceRaycastHitResult(hits, originPos, Vector3.up);

            Help(i, originPos, cutterExtents, distance, EAxisType.x);
        }

        //z方向切割 - 切割物平面与被切割物在z方向垂直
        //每次切割的距离
        float distanceCat2 = (extents.z / (cutCount.z + 1) * 2);
        for (int i = 0; i < cutCount.z; i++)
        {
            Vector3 originPos = this.transform.position;
            originPos.z += center.z - extents.z + distanceCat2 * (i + 1);
            originPos.y += center.x + extents.y;
            originPos.x -= center.x;

            Vector3 cutterExtents = extents * 2;
            cutterExtents.z = 0.02f;
            // cutterExtents.x += extents.x * 2;
            // cutterExtents.y += extents.y * 2;
            float distance = extents.y * 4;
            // Cast a ray and find the nearest object
            RaycastHit[] hits = Physics.BoxCastAll(originPos, cutterExtents, Vector3.up, this.transform.rotation, distance);

            CutOnceRaycastHitResult(hits, originPos, Vector3.forward);

            Help(i, originPos, cutterExtents, distance, EAxisType.y);
        }

        //x方向切割 - 切割物平面与被切割物在x方向垂直
        //每次切割的距离
        float distanceCat3 = (extents.x / (cutCount.x + 1) * 2);
        for (int i = 0; i < cutCount.x; i++)
        {
            Vector3 originPos = this.transform.position;
            originPos.x += center.x - extents.x + distanceCat3 * (i + 1);
            originPos.z -= extents.z - center.z;
            originPos.y += center.y;

            Vector3 cutterExtents = extents * 2;
            cutterExtents.x = 0.02f;
            // cutterExtents.y += extents.y * 2;
            // cutterExtents.z += extents.z * 2;

            float distance = extents.z * 4;

            // Cast a ray and find the nearest object
            RaycastHit[] hits = Physics.BoxCastAll(originPos, cutterExtents, Vector3.forward, this.transform.rotation, distance);

            CutOnceRaycastHitResult(hits, originPos, Vector3.right);

            Help(i, originPos, cutterExtents, distance, EAxisType.z);
        }

    }

    private void CutOnceRaycastHitResult(RaycastHit[] hits, Vector3 position, Vector3 dir)
    {
        Debug.Log("RaycastHits --> " + hits.Length);
        foreach (RaycastHit hit in hits)
        {
            var obj = hit.collider.gameObject;
            var sliceObj = obj.GetComponent<Src_PreBoxCut>();

            if (sliceObj != null)
            {
                // sliceObj.GetComponent<MeshRenderer>()?.material.SetVector("CutPlaneOrigin", Vector3.positiveInfinity);
                sliceObj.ComputeSlice(dir, position);
            }
        }
    }

    private enum EAxisType
    {
        x,
        y,
        z
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <param name="originPos"></param>
    /// <param name="cutterExtents"></param>
    /// <param name="distance"></param>
    /// <param name="axis">射线方向的轴</param>
    private void Help(int i, Vector3 originPos, Vector3 cutterExtents, float distance, EAxisType axis)
    {
        if (showHelp == false) return;
        Transform trans = Instantiate(cube).transform;
        trans.position = originPos;
        var scale = cutterExtents;
        if (axis == EAxisType.x)
        {
            trans.gameObject.name = "cube_yz_" + i;
            scale.x += distance;
        }
        else if (axis == EAxisType.y)
        {
            trans.gameObject.name = "cube_zx_" + i;
            scale.y += distance;
        }
        else if (axis == EAxisType.z)
        {
            trans.gameObject.name = "cube_xy_" + i;
            scale.z += distance;
        }

        trans.localScale = scale;
    }



    /// <summary>
    /// Slices the attached mesh along the cut plane
    /// </summary>
    /// <param name="sliceNormalWorld">The cut plane normal vector in world coordinates.</param>
    /// <param name="sliceOriginWorld">The cut plane origin in world coordinates.</param>
    public void ComputeSlice(Vector3 sliceNormalWorld, Vector3 sliceOriginWorld)
    {
        Debug.Log("---ComputeSlice---");
        var mesh = this.GetComponent<MeshFilter>().sharedMesh;

        if (mesh != null)
        {
            // If the fragment root object has not yet been created, create it now
            if (fragmentRoot == null)
            {
                // Create a game object to contain the fragments
                fragmentRoot = new GameObject($"{this.name}Slices");
                fragmentRoot.transform.SetParent(this.transform.parent);

                // Each fragment will handle its own scale
                fragmentRoot.transform.position = this.transform.position;
                fragmentRoot.transform.rotation = this.transform.rotation;
                fragmentRoot.transform.localScale = Vector3.one;
            }

            var sliceTemplate = CreateSliceTemplate();
            var sliceNormalLocal = this.transform.InverseTransformDirection(sliceNormalWorld);
            var sliceOriginLocal = this.transform.InverseTransformPoint(sliceOriginWorld);

            BoxFragmenter.Slice(this.gameObject,
                             sliceNormalLocal,
                             sliceOriginLocal,
                             this.sliceOptions,
                             sliceTemplate,
                             fragmentRoot.transform);

            // Done with template, destroy it
            GameObject.DestroyImmediate(sliceTemplate);

            // Deactivate the original object
            if (CurCutCount == 0)
            {
                CurCutCount++;
                this.gameObject.SetActive(false);
            }
            else
            {
                GameObject.DestroyImmediate(this.gameObject);
            }

            // Fire the completion callback
            if (callbackOptions.onCompleted != null)
            {
                callbackOptions.onCompleted.Invoke();
            }
        }
    }
    /// <summary>
    /// Creates a template object which each fragment will derive from
    /// </summary>
    /// <returns></returns>
    private GameObject CreateSliceTemplate()
    {
        // If pre-fracturing, make the fragments children of this object so they can easily be unfrozen later.
        // Otherwise, parent to this object's parent
        GameObject obj = new GameObject();
        obj.name = "Slice";
        obj.tag = this.tag;

        // Update mesh to the new sliced mesh
        obj.AddComponent<MeshFilter>();

        // Add materials. Normal material goes in slot 1, cut material in slot 2
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = new Material[2] {
            this.GetComponent<MeshRenderer>().sharedMaterial,
            this.sliceOptions.insideMaterial
        };

        // Copy collider properties to fragment
        var thisCollider = this.GetComponent<Collider>();
        // var fragmentCollider = obj.AddComponent<MeshCollider>();
        // fragmentCollider.convex = true;
        // fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
        // fragmentCollider.isTrigger = thisCollider.isTrigger;

        var fragmentCollider = obj.AddComponent<BoxCollider>();
        fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
        fragmentCollider.isTrigger = thisCollider.isTrigger;

        // Copy rigid body properties to fragment
        var thisRigidBody = this.GetComponent<Rigidbody>();
        var fragmentRigidBody = obj.AddComponent<Rigidbody>();
        fragmentRigidBody.velocity = thisRigidBody.velocity;
        fragmentRigidBody.angularVelocity = thisRigidBody.angularVelocity;
        fragmentRigidBody.drag = thisRigidBody.drag;
        fragmentRigidBody.angularDrag = thisRigidBody.angularDrag;
        fragmentRigidBody.useGravity = thisRigidBody.useGravity;
        fragmentRigidBody.constraints = RigidbodyConstraints.None;//RigidbodyConstraints.FreezeAll;

        // If refracturing is enabled, create a copy of this component and add it to the template fragment object
        if (this.sliceOptions.enableReslicing &&
           (this.currentSliceCount < this.sliceOptions.maxResliceCount))
        {
            CopySliceComponent(obj);
        }

        return obj;
    }
    /// <summary>
    /// Convenience method for copying this component to another component
    /// </summary>
    /// <param name="obj">The GameObject to copy this component to</param>
    private void CopySliceComponent(GameObject obj)
    {
        var sliceComponent = obj.AddComponent<Src_PreBoxCut>();

        sliceComponent.sliceOptions = this.sliceOptions;
        sliceComponent.callbackOptions = this.callbackOptions;
        sliceComponent.currentSliceCount = this.currentSliceCount + 1;
        // sliceComponent.fragmentRoot = this.fragmentRoot;
    }
}
