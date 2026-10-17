
using UnityEngine;

public class loopingScrolling : MonoBehaviour
{
    public float maxX = 18.24f;
    public float speed = 2f;
    public int direction = 1;

    private float startY;
    private float startZ;

    void Start()
    {
        startY = transform.position.y;
        startZ = transform.position.z;
    }

    void Update()
    {
        transform.position += Vector3.right * speed * direction * Time.deltaTime;

        if (direction == 1 && transform.position.x >= maxX)
        {
            transform.position = new Vector3(0f, startY, startZ);
        }
        else if (direction == -1 && transform.position.x <= 0f)
        {
            transform.position = new Vector3(maxX, startY, startZ);
        }
    }
}
