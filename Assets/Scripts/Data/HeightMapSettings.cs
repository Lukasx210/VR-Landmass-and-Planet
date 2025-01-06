﻿using UnityEngine;


[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {

	public NoiseConfig noiseConfig;

	public bool useFalloff;

	public float heightMultiplier;
	public AnimationCurve heightCurve;

	public float minHeight {
		get {
			return heightMultiplier * heightCurve.Evaluate (0);
		}
	}

	public float maxHeight {
		get {
			return heightMultiplier * heightCurve.Evaluate (1);
		}
	}

	#if UNITY_EDITOR

	protected override void OnValidate() {
		noiseConfig.EnsureValidValues ();
		base.OnValidate ();
	}
	#endif

}