using UnityEngine;
using UnityEngine.Splines;

public class CardSceneCharacterLeftArm : MonoBehaviour
{
    [SerializeField] Transform shoulderTransform;
    [SerializeField] Transform currentHandTarget;

    [SerializeField] Transform[] cardsTransforms;

    private SplineContainer splineContainer;
    private Spline spline;
    private BezierKnot shoulderKnot;
    private BezierKnot handKnot;
    private int targetCardNum = 2;

    private void Start()
    {
        splineContainer = GetComponent<SplineContainer>();
        spline = splineContainer.Spline;
    }

    void Update()
    {
        if (currentHandTarget == null || shoulderTransform == null) return;        

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            targetCardNum = SelectCard(1);
            Debug.Log(targetCardNum);   
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            targetCardNum = SelectCard(2);
            Debug.Log(targetCardNum);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            targetCardNum = SelectCard(3);
            Debug.Log(targetCardNum);
        }

        shoulderKnot.Position = shoulderTransform.position;
        currentHandTarget.position = Vector3.Lerp(currentHandTarget.position, cardsTransforms[targetCardNum].position, Time.deltaTime * 10f);
        handKnot.Position = splineContainer.transform.InverseTransformPoint(currentHandTarget.position);

        spline.SetKnot(0, shoulderKnot);
        spline.SetKnot(2, handKnot);

        Vector3 mid = Vector3.Lerp(shoulderKnot.Position, handKnot.Position, 0.5f);
        mid += new Vector3(-1, -1) * 1f;
        spline.SetKnot(1, new BezierKnot(mid));
    }

    private int SelectCard(int cardNum)
    {
        if (cardNum == 1) return 0;
        else if (cardNum == 2) return 1;
        else return 2;
    }
}
