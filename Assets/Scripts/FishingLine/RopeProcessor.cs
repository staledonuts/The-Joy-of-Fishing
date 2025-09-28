using UnityEngine;

public class RopeProcessor
{
    public void SolveConstraints(RopeState ropeState, float segmentLength, Vector2 rodTipPosition, float reelSettleSpeedEffectTimer, float currentReelOutSettleSpeedFactor)
    {
        if (ropeState.nodes.Count == 0) return;

        RopeNode rootNode = ropeState.nodes[0];
        rootNode.position = rodTipPosition;
        rootNode.prevPosition = rodTipPosition;
        ropeState.nodes[0] = rootNode;

        for (int iter = 0; iter < 15; iter++)
        {
            for (int i = 0; i < ropeState.nodes.Count - 1; i++)
            {
                RopeNode nodeA = ropeState.nodes[i];
                RopeNode nodeB = ropeState.nodes[i + 1];

                Vector2 delta = nodeB.position - nodeA.position;
                float currentDistance = delta.magnitude;
                float error = 0f;

                if (currentDistance > 0.0001f)
                    error = (currentDistance - segmentLength) / currentDistance;
                else
                {
                    error = (currentDistance - segmentLength) / (segmentLength + 0.0001f);
                }

                Vector2 correction = delta * 0.5f * error;

                if (i == 0 && reelSettleSpeedEffectTimer > 0 && ropeState.nodes.Count > 1)
                {
                    if (currentDistance < segmentLength)
                    {
                        correction *= currentReelOutSettleSpeedFactor;
                    }
                }

                if (i != 0)
                {
                    nodeA.position += correction;
                    ropeState.nodes[i] = nodeA;
                }
                nodeB.position -= correction;
                ropeState.nodes[i + 1] = nodeB;
            }

            if (ropeState.nodes.Count > 0)
            {
                RopeNode lastNode = ropeState.nodes[ropeState.nodes.Count - 1];
                Vector2 delta = ropeState.hookPhysicsTargetPosition - lastNode.position;
                float currentDistance = delta.magnitude;
                float error = 0f;

                if (currentDistance > 0.0001f)
                    error = (currentDistance - segmentLength) / currentDistance;
                else
                    error = (currentDistance - segmentLength) / (segmentLength + 0.0001f);

                Vector2 correction = delta * 0.5f * error;

                if (ropeState.nodes.Count > 1)
                {
                    lastNode.position += correction;
                    ropeState.nodes[ropeState.nodes.Count - 1] = lastNode;
                }
                ropeState.hookPhysicsTargetPosition -= correction;
            }
        }
    }
}
