using UnityEngine;

public class Block : MonoBehaviour
{
    public int row;
    public int col;
    public int type;
    public GameObject destroyObject;


    public void SetPosition(int r, int c)
    {
        row = r;
        col = c;
    }

    private void OnDestroy()
    {
        var destoryOb = Instantiate(destroyObject,transform.position, Quaternion.identity);

        var renderers = destoryOb.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in renderers)
        {
            mr.sortingLayerName = "Default";
            mr.sortingOrder = 3;
            Material mat = mr.material;
            mat.mainTexture = GetComponent<SpriteRenderer>().sprite.texture;
        }
    }

    public void OpenCrown()
    {
        Instantiate(destroyObject,new Vector3(transform.position.x, transform.position.y,3), Quaternion.identity);
    }
}