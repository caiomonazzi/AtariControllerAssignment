using UnityEngine;

public class Parallax : MonoBehaviour
{
    #region Variables

    [SerializeField] private float speed;

    private MeshRenderer mesh;

    #endregion

    #region Unity Methods

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        MoveTexture();
    }

    #endregion

    #region Private Methods

    private void InitializeComponents()
    {
        mesh = GetComponent<MeshRenderer>();
    }

    private void MoveTexture()
    {
        mesh.material.mainTextureOffset = new Vector2(Time.time * speed, 0);
    }

    #endregion
}
