using UnityEngine;
using UnityEditor;
using System.Collections;

public class ParticleComponent : ExploderComponent {
	public GameObject explosionEffectsContainer;
	public float scale = 1;
	public float playbackSpeed = 1;
	public override void onExplosionStarted(Exploder exploder)
	{
		GameObject container = (GameObject)GameObject.Instantiate(explosionEffectsContainer, transform.position, Quaternion.identity);
		ParticleSystem[] systems = container.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem system in systems) {
			system.startSpeed *= scale;
			system.startSize *= scale;
			system.transform.localScale *= scale;
			system.playbackSpeed = playbackSpeed;
		}
	}
}
