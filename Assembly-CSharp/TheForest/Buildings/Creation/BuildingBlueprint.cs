using System;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	[DoNotSerializePublic]
	[Serializable]
	public class BuildingBlueprint
	{
		public BuildingTypes _type;

		public PlacerDistance _placerDistance;

		public GameObject _ghostPrefab;

		public GameObject _ghostPrefabMP;

		public GameObject _builtPrefab;

		public float _cornersRaycastDistanceAlpha = 0.75f;

		public bool _ignoreLookAtCollision;

		public bool _ignoreBlock;

		public bool _preventHoleCutting;

		[Header("Rendering", order = 0)]
		public bool _useFlatMeshMaterial;

		[Header("Tree", order = 0)]
		public bool _allowInTree;

		public bool _allowInTreeAtFloorLevel;

		[Header("Legacy (basic)", order = 0)]
		public bool _isPlateform;

		public bool _isWallPiece;

		public bool _isStairPiece;

		[Header("Procedural", order = 0)]
		public bool _allowFoundation;

		public bool _airBorne;

		[Header("Anchors", order = 0)]
		public bool _showAnchors;

		public bool _showSupportAnchor;

		[Header("Dynamic", order = 0)]
		public bool _isDynamic;

		public bool _allowParentingToDynamic;

		public bool _skipParentingToDynamicChecks;

		public ExclusionGroups _exclusionGroup;

		[Header("Small", order = 0)]
		public bool _isSmall;

		public bool _skipSmallStructureChecks;

		[Header("Water", order = 0)]
		public bool _waterborne;

		public bool _waterborneExclusive;

		public bool _hydrophobic;
	}
}
