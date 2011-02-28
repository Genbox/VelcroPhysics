using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

public class QuadTreeBroadPhase : IBroadPhase
{
    private const int treeUpdateThresh = 10;
    private Dictionary<int, QTElement<FixtureProxy>> IDRegister;
    public QuadTree<FixtureProxy> QTree;

    private int _currID;

    private List<QTElement<FixtureProxy>> moveBuffer;
    private List<Pair> pairBuffer;

    private int treeMoveNum;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="span">the maximum span of the tree (world size)</param>
    public QuadTreeBroadPhase(AABB span)
    {
        QTree = new QuadTree<FixtureProxy>(span, 5, 10);
        IDRegister = new Dictionary<int, QTElement<FixtureProxy>>();
        moveBuffer = new List<QTElement<FixtureProxy>>();
        pairBuffer = new List<Pair>();
    }

    public int ProxyCount
    {
        get { return IDRegister.Count; }
    }

    private int nextID()
    {
        return _currID++;
    }

    private AABB fatten(ref AABB aabb)
    {
        Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
        return new AABB(aabb.LowerBound - r, aabb.UpperBound + r);
    }

    private Func<QTElement<FixtureProxy>, bool> transformPredicate(Func<int, bool> idPredicate)
    {
        Func<QTElement<FixtureProxy>, bool> qtPred =
            (QTElement<FixtureProxy> qtnode) => idPredicate(qtnode.Value.ProxyId);
        return qtPred;
    }

    private Func<RayCastInput, QTElement<FixtureProxy>, float> transformRayCallback(
        Func<RayCastInput, int, float> callback)
    {
        Func<RayCastInput, QTElement<FixtureProxy>, float> newCallback =
            (RayCastInput input, QTElement<FixtureProxy> qtnode) => callback(input, qtnode.Value.ProxyId);
        return newCallback;
    }

    private AABB getFatAABB(int proxyID)
    {
        if (IDRegister.ContainsKey(proxyID))
            return IDRegister[proxyID].Span;
        else
            throw new KeyNotFoundException("proxyID not found in register");
    }

    public void UpdatePairs(BroadphaseDelegate callback)
    {
        pairBuffer.Clear();
        foreach (var qtnode in moveBuffer)
        {
            // Query tree, create pairs and add them pair buffer.
            Query(proxyID => pairBufferQueryCallback(proxyID, qtnode.Value.ProxyId), ref qtnode.Span);
        }
        moveBuffer.Clear();

        // Sort the pair buffer to expose duplicates.
        pairBuffer.Sort();

        // Send the pairs back to the client.
        int i = 0;
        while (i < pairBuffer.Count)
        {
            Pair primaryPair = pairBuffer[i];
            FixtureProxy userDataA = GetProxy(primaryPair.ProxyIdA);
            FixtureProxy userDataB = GetProxy(primaryPair.ProxyIdB);

            callback(ref userDataA, ref userDataB);
            ++i;

            // Skip any duplicate pairs.
            while (i < pairBuffer.Count && pairBuffer[i].ProxyIdA == primaryPair.ProxyIdA &&
                   pairBuffer[i].ProxyIdB == primaryPair.ProxyIdB)
                ++i;
        }
    }

    private bool pairBufferQueryCallback(int proxyID, int baseID)
    {
        // A proxy cannot form a pair with itself.
        if (proxyID == baseID)
            return true;

        Pair p = new Pair();
        p.ProxyIdA = Math.Min(proxyID, baseID);
        p.ProxyIdB = Math.Max(proxyID, baseID);
        pairBuffer.Add(p);

        return true;
    }

    /// <summary>
    /// Test overlap of fat AABBs.
    /// </summary>
    /// <param name="proxyIdA">The proxy id A.</param>
    /// <param name="proxyIdB">The proxy id B.</param>
    /// <returns></returns>
    public bool TestOverlap(int proxyIdA, int proxyIdB)
    {
        return AABB.TestOverlap(getFatAABB(proxyIdA), getFatAABB(proxyIdB));
    }

    public int AddProxy(ref FixtureProxy proxy)
    {
        int proxyID = nextID();
        proxy.ProxyId = proxyID;
        AABB aabb = fatten(ref proxy.AABB);
        QTElement<FixtureProxy> qtnode = new QTElement<FixtureProxy>(proxy, aabb);

        IDRegister.Add(proxyID, qtnode);
        QTree.AddNode(qtnode);

        return proxyID;
    }

    public void RemoveProxy(int proxyId)
    {
        if (IDRegister.ContainsKey(proxyId))
        {
            var qtnode = IDRegister[proxyId];
            unbufferMove(qtnode);
            IDRegister.Remove(proxyId);
            QTree.RemoveNode(qtnode);
        }
        else
            throw new KeyNotFoundException("proxyID not found in register");
    }

    public void MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement)
    {
        if (getFatAABB(proxyId).Contains(ref aabb)) return; //exit if movement is within fat aabb


        // Extend AABB.
        AABB b = aabb;
        Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
        b.LowerBound = b.LowerBound - r;
        b.UpperBound = b.UpperBound + r;

        // Predict AABB displacement.
        Vector2 d = Settings.AABBMultiplier * displacement;

        if (d.X < 0.0f)
            b.LowerBound.X += d.X;
        else
            b.UpperBound.X += d.X;

        if (d.Y < 0.0f)
            b.LowerBound.Y += d.Y;
        else
            b.UpperBound.Y += d.Y;


        var qtnode = IDRegister[proxyId];
        qtnode.Value.AABB = b; //not neccesary for QTree, but might be accessed externally
        qtnode.Span = b;

        reinsertNode(qtnode);

        bufferMove(qtnode);
    }

    private void reinsertNode(QTElement<FixtureProxy> qtnode)
    {
        QTree.RemoveNode(qtnode);
        QTree.AddNode(qtnode);

        if (++treeMoveNum > treeUpdateThresh)
        {
            QTree.Reconstruct();
            treeMoveNum = 0;
        }
    }

    private void bufferMove(QTElement<FixtureProxy> proxy)
    {
        moveBuffer.Add(proxy);
    }

    private void unbufferMove(QTElement<FixtureProxy> proxy)
    {
        moveBuffer.Remove(proxy);
    }

    public FixtureProxy GetProxy(int proxyId)
    {
        if (IDRegister.ContainsKey(proxyId))
            return IDRegister[proxyId].Value;
        else
            throw new KeyNotFoundException("proxyID not found in register");
    }

    public void TouchProxy(int proxyId)
    {
        if (IDRegister.ContainsKey(proxyId))
            bufferMove(IDRegister[proxyId]);
        else
            throw new KeyNotFoundException("proxyID not found in register");
    }

    public void Query(Func<int, bool> callback, ref AABB query)
    {
        QTree.Query_Callback(transformPredicate(callback), ref query);
    }

    public void RayCast(Func<RayCastInput, int, float> callback, ref RayCastInput input)
    {
        QTree.RayCast(transformRayCallback(callback), ref input);
    }
}