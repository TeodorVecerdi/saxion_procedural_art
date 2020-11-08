using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketGenerator : BuildingGenerator {
    [HideInInspector] public MarketSettings MarketSettings;
    public List<GameObject> FountainPrefabs;
    public List<GameObject> StallPrefabs;
    public List<GameObject> DetailPrefabs;
    public List<GameObject> TreePrefabs;

    public static new bool DoneOnceField;

    public override MeshData Generate(PlotData plot, BuildingTypeSettings settings, float heightAdjustment, Vector3 offset, int LOD) {
        DoOnce(ref DoneOnceField);

        // Don't do LOD
        if (LOD >= 1) return new MeshData();

        var container = new GameObject();
        var (leftDown, rightDown, rightUp, leftUp) = MathUtils.RotatedRectangle(plot);
        var center = (leftDown + rightDown + rightUp + leftUp) / 4;
        SetLocalPosition(container, transform, -new Vector3(plot.Bounds.width / 2.0f, transform.localPosition.y, plot.Bounds.height / 2.0f));
        container.transform.localRotation = Quaternion.identity;

        var fountainPrefab = RandUtils.ListItem(FountainPrefabs);
        var fountain = Instantiate(fountainPrefab, Vector3.zero, Quaternion.identity);
        var fountainPosition = PlaceObject(plot.Bounds, new Vector3(plot.Bounds.width / 2.0f, 0, plot.Bounds.height / 2.0f));
        SetLocalPosition(fountain, container.transform, fountainPosition);

        var stalls = RandUtils.RandomBetween(MarketSettings.MinMaxStalls);
        var details = RandUtils.RandomBetween(MarketSettings.MinMaxDetails);
        var treeClumps = RandUtils.RandomBetween(MarketSettings.MinMaxTreeClumps);

        for (var i = 0; i < stalls; i++) {
            var stall = Instantiate(RandUtils.ListItem(StallPrefabs), Vector3.zero, Quaternion.Euler(0, Rand.Range(360f), 0));
            var stallPosition = PlaceObject(plot.Bounds, GetPositionAwayFromBorder(plot.Bounds, MarketSettings.BorderDistance).ToVec3());
            SetLocalPosition(stall, container.transform, stallPosition);
        }

        for (var i = 0; i < details; i++) {
            var detail = Instantiate(RandUtils.ListItem(DetailPrefabs), Vector3.zero, Quaternion.Euler(0, Rand.Range(360f), 0));
            var detailPosition = PlaceObject(plot.Bounds, GetPositionAwayFromBorder(plot.Bounds, MarketSettings.BorderDistance).ToVec3());
            SetLocalPosition(detail, container.transform, detailPosition);
        }

        for (var i = 0; i < treeClumps; i++) {
            var treeCount = RandUtils.RandomBetween(MarketSettings.MinMaxTreesPerClump);
            var clumpPosition = GetPositionNearBorder(plot.Bounds, MarketSettings.BorderDistance);
            for (var j = 0; j < treeCount; j++) {
                var tree = Instantiate(RandUtils.ListItem(TreePrefabs), Vector3.zero, Quaternion.Euler(0, Rand.Range(360f), 0));
                var treePosition = PlaceObject(plot.Bounds, GetOffset(clumpPosition, MarketSettings.MaxTreeDistance, MarketSettings.MinTreeDistance).ToVec3());
                SetLocalPosition(tree, container.transform, treePosition);
            }
        }

        return new MeshData();
    }

    public override void DoOnce(ref bool doneOnceField) {
        if (doneOnceField) return;
        doneOnceField = true;
    }

    public override void Setup(BuildingTypeSettings settings) {
        MarketSettings = settings.GeneratorSettings as MarketSettings;
    }

    private Vector3 PlaceObject(Rect plotBounds, Vector3 position) {
        if (Physics.Raycast(position + plotBounds.position.ToVec3() + Vector3.up * 200, Vector3.down, out var hitInfo, LayerMask.GetMask("Terrain"))) {
            position.y = hitInfo.point.y;
            return position;
        }

        return position;
    }

    private Vector2 GetPosition(Rect plotBounds) {
        var randomizer = new WeightedRandom(0.25f, 0.25f, 0.25f, 0.25f);
        randomizer.NormalizeWeights();
        randomizer.CalculateAdditiveWeights();
        var choice = randomizer.Value();
        var left = new Vector2(Rand.Range(0f, plotBounds.width / 2.0f - MarketSettings.MinDistanceFromCenter), Rand.Range(0f, plotBounds.height));
        var right = new Vector2(Rand.Range(plotBounds.width / 2.0f + MarketSettings.MinDistanceFromCenter, plotBounds.width), Rand.Range(0f, plotBounds.height));
        var top = new Vector2(Rand.Range(0, plotBounds.width), Rand.Range(plotBounds.height / 2.0f + MarketSettings.MinDistanceFromCenter, plotBounds.height));
        var bottom = new Vector2(Rand.Range(0, plotBounds.width), Rand.Range(0f, plotBounds.height / 2.0f - MarketSettings.MinDistanceFromCenter));
        switch (choice) {
            case 0: return left;
            case 1: return right;
            case 2: return top;
            default: return bottom;
        }
    }
    
    private Vector2 GetPositionAwayFromBorder(Rect plotBounds, float borderDistance) {
        var randomizer = new WeightedRandom(0.25f, 0.25f, 0.25f, 0.25f);
        randomizer.NormalizeWeights();
        randomizer.CalculateAdditiveWeights();
        var choice = randomizer.Value();
        var left = new Vector2(Rand.Range(borderDistance, plotBounds.width / 2.0f - MarketSettings.MinDistanceFromCenter), Rand.Range(borderDistance, plotBounds.height - borderDistance));
        var right = new Vector2(Rand.Range(plotBounds.width / 2.0f + MarketSettings.MinDistanceFromCenter, plotBounds.width - borderDistance), Rand.Range(borderDistance, plotBounds.height - borderDistance));
        var top = new Vector2(Rand.Range(borderDistance, plotBounds.width - borderDistance), Rand.Range(plotBounds.height / 2.0f + MarketSettings.MinDistanceFromCenter, plotBounds.height - borderDistance));
        var bottom = new Vector2(Rand.Range(borderDistance, plotBounds.width - borderDistance), Rand.Range(borderDistance, plotBounds.height / 2.0f - MarketSettings.MinDistanceFromCenter));
        switch (choice) {
            case 0: return left;
            case 1: return right;
            case 2: return top;
            default: return bottom;
        }
    }
    
    private Vector2 GetPositionNearBorder(Rect plotBounds, float borderDistance) {
        var randomizer = new WeightedRandom(0.25f, 0.25f, 0.25f, 0.25f);
        randomizer.NormalizeWeights();
        randomizer.CalculateAdditiveWeights();
        var choice = randomizer.Value();
        var left = new Vector2(Rand.Range(0f, borderDistance), Rand.Range(0f, plotBounds.height));
        var right = new Vector2(Rand.Range(plotBounds.width - borderDistance, plotBounds.width), Rand.Range(0f, plotBounds.height));
        var top = new Vector2(Rand.Range(0, plotBounds.width), Rand.Range(plotBounds.height - borderDistance, plotBounds.height));
        var bottom = new Vector2(Rand.Range(0, plotBounds.width), Rand.Range(0f, borderDistance));
        switch (choice) {
            case 0: return left;
            case 1: return right;
            case 2: return top;
            default: return bottom;
        }
    }

    private Vector2 GetOffset(Vector2 point, float maxDistance, float minDistance) {
        var unit = Rand.UnitVector2;
        var minDist = unit.Map(0f, 1f, minDistance, 1f + minDistance);
        var maxDist = minDist.WithMagnitude(maxDistance - minDistance);
        return point + maxDist;
    }

    private void SetLocalPosition(GameObject @object, Transform parent, Vector3 position) {
        @object.transform.parent = parent;
        @object.transform.localPosition = position;
    }
}