using System.Collections.Generic;
using UnityEngine;

public class UnityCubicBezier : MonoBehaviour
{
    public readonly List<Vector2> PointsOnCurve = new List<Vector2>();


    public void BuildCurveWithPairs(List<ControlAnchorPair> pairs, int AmointOfPointsOnLine)
    {
        int pointPerCurvePart = AmointOfPointsOnLine / (pairs.Count - 1);

        for (int i = 1; i < pairs.Count; i++)
            BuildCurveSpace(pairs[i - 1], pairs[i], pointPerCurvePart);
    }


    public void BuildRandomCurve(ControlAnchorPair startPair, ControlAnchorPair endPair,
    float randomClamp, int AmountOfControlPoints, int AmountOfPointsPerCurve, float smoothness, bool isInverse)
    {
        var dif = (startPair.ControlPoint - endPair.ControlPoint).magnitude / AmountOfControlPoints;
        var startDif = dif;

        var AmountOfPointsPerSpace = AmountOfPointsPerCurve / AmountOfControlPoints;
        if (AmountOfPointsPerSpace == 0) AmountOfPointsPerSpace = 1;
        var previousPair = startPair;
        bool prevBool = startPair.isInversed;
        int y = 0;

        for (int i = 0; i < AmountOfControlPoints; i++)
        {
            var pointPos = LerpByDistance(startPair.ControlPoint, endPair.ControlPoint, startDif);

            var newPair = new ControlAnchorPair(pointPos, smoothness, prevBool);
            var randomX = Random.Range(-randomClamp, randomClamp);
            newPair.ControlPoint.x += randomX; newPair.AnchorPoint.x += randomX;

            BuildCurveSpace(previousPair, newPair, AmountOfPointsPerSpace);

            y++;
            if (y == 2)
            {
                prevBool = !prevBool;
                y = 0;
            }
            previousPair = newPair;
            startDif += dif;
        }
    }


    private void BuildCurveSpace(ControlAnchorPair firstPair, ControlAnchorPair secondPair, int Points)
    {
        var firstAnchorX = firstPair.AnchorPoint.x;
        var firstAnchorY = firstPair.AnchorPoint.y;
        var firstControlX = firstPair.ControlPoint.x;
        var firstControlY = firstPair.ControlPoint.y;
        var secondControlX = secondPair.ControlPoint.x;
        var secondControlY = secondPair.ControlPoint.y;
        var secondAnchorX = secondPair.AnchorPoint.x;
        var secondAnchorY = secondPair.AnchorPoint.y;

        double subdiv_step = 1.0 / (Points + 1);
        double subdiv_step2 = subdiv_step * subdiv_step;
        double subdiv_step3 = subdiv_step * subdiv_step * subdiv_step;

        double pre1 = 3.0 * subdiv_step;
        double pre2 = 3.0 * subdiv_step2;
        double pre4 = 6.0 * subdiv_step2;
        double pre5 = 6.0 * subdiv_step3;

        double tmp1x = firstAnchorX - firstControlX * 2.0 + secondControlX;
        double tmp1y = firstAnchorY - firstControlY * 2.0 + secondControlY;

        double tmp2x = (firstControlX - secondControlX) * 3.0 - firstAnchorX + secondAnchorX;
        double tmp2y = (firstControlY - secondControlY) * 3.0 - firstAnchorY + secondAnchorY;

        double fx = firstAnchorX;
        double fy = firstAnchorY;

        double dfx = (firstControlX - firstAnchorX) * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3;
        double dfy = (firstControlY - firstAnchorY) * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;

        double ddfx = tmp1x * pre4 + tmp2x * pre5;
        double ddfy = tmp1y * pre4 + tmp2y * pre5;

        double dddfx = tmp2x * pre5;
        double dddfy = tmp2y * pre5;

        for (int i = 0; i < Points; i++)
        {
            // new X
            fx += dfx;
            // new Y
            fy += dfy;

            dfx += ddfx;
            dfy += ddfy;
            ddfx += dddfx;
            ddfy += dddfy;

            PointsOnCurve.Add(new Vector2((float)fx, (float)fy));
        }
    }


    private Vector3 LerpByDistance(Vector2 A, Vector2 B, float x)
    {
        var P = x * (B - A).normalized + A;
        return P;
    }


    public void CleanBezier() => PointsOnCurve.Clear();
}


public class ControlAnchorPair
{
    public Vector2 ControlPoint;
    public Vector2 AnchorPoint;
    public bool isInversed = false;

    public ControlAnchorPair(Vector2 controlPoint, float smoothness, bool inverse)
    {
        ControlPoint = controlPoint;
        isInversed = inverse;

        switch (inverse)
        {
            case true:
                AnchorPoint = new Vector2(controlPoint.x - smoothness, controlPoint.y - smoothness);
                break;

            case false:
                AnchorPoint = new Vector2(controlPoint.x + smoothness, controlPoint.y + smoothness);
                break;
        }
    }
}
