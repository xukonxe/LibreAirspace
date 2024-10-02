using UnityEngine;
using System.Collections;

public class PseudoVolumetricComponent : ExploderComponent {
	public GameObject volumetricExplosion;
	public int count = 30;
	public float scale = 1;
	public float randomness = 1;
	public float duration = 1;
 
	public IEnumerator generateExplosion(Exploder exploder) {
		for (int i = 0; i < count; i++) {
			GameObject explosion = (GameObject) GameObject.Instantiate(volumetricExplosion, Random.insideUnitSphere * scale * randomness + transform.position, Random.rotation);
			explosion.transform.localScale *= scale * (Random.Range(0.5f, 1) * randomness + 1);
			((PseudoVolumetricExplosion) explosion.GetComponent<PseudoVolumetricExplosion>()).timeScale = duration * Random.Range(1 - 0.35f * randomness, 1);
			yield return new WaitForSeconds(Random.Range(0, duration * 0.5f * randomness));
		}
	}

	public override void onExplosionStarted (Exploder exploder) {
		StartCoroutine("generateExplosion", exploder);
	}
}
