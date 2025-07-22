using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RopePhysics : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] int segmentCount = 15;
    [SerializeField] int constraintLoop = 15;
    [SerializeField] float segmentLength = 0.1f;
    [SerializeField] float ropeWidth = 0.1f;

    [Header("·ÎÇÁ ÁÂÇ¥")]
    [SerializeField] Transform startTransform;
    [SerializeField] GameObject endObject;

    private List<Segment> segments = new List<Segment>();
    private Vector2 gravity = new Vector2(0, -9.81f);

    private void Awake()
    {
        Vector2 segmentPos = startTransform.position;

        Init();

        for(int i = 0; i < segmentCount; i++)
        {
            segments.Add(new Segment(segmentPos));
            segmentPos.y -= segmentLength;
        }
    }

    private void Init()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
        UpdateSegments();
        for(int i = 0; i < constraintLoop; i++)
        {
            ApplyConstraint();
        }        
        DrawRope();
    }

    private void DrawRope()
    {
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;

        Vector3[] segmentPositions = new Vector3[segments.Count];
        for(int i =0; i<segments.Count; i++)
        {
            segmentPositions[i] = segments[i].Position;
        }

        lineRenderer.positionCount = segmentPositions.Length;
        lineRenderer.SetPositions(segmentPositions);
    }

    private void UpdateSegments()
    {
        for(int i = 0;i < segments.Count; i++)
        {
            segments[i].Velocity = segments[i].Position - segments[i].PreviousPos;
            segments[i].PreviousPos = segments[i].Position;
            segments[i].Position += gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
            segments[i].Position += segments[i].Velocity;
        }
    }

    private void ApplyConstraint()
    {
        segments[0].Position = startTransform.position;
        segments[segmentCount - 1].Position = endObject.transform.position;

        for(int i = 0; i < segments.Count - 1; i++)
        {
            float distance = (segments[i].Position - segments[i + 1].Position).magnitude;
            float difference = segmentLength - distance;
            Vector2 dir = (segments[i + 1].Position - segments[i].Position).normalized;

            Vector2 movement = dir * difference;

            if(i == 0) segments[i + 1].Position += movement;
            else if(i == segments.Count - 2) segments[i].Position -= movement;
            else
            {
                segments[i].Position -= movement * 0.5f;
                segments[i + 1].Position += movement * 0.5f;
            }
        }
    }

    public class Segment
    {
        public Vector2 PreviousPos;
        public Vector2 Position;
        public Vector2 Velocity;

        public Segment(Vector2 position)
        {
            PreviousPos = position;
            Position = position;
            Velocity = Vector2.zero;
        }
    }
}
