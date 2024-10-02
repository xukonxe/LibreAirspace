using UnityEngine;
using System.Collections;

public class LightComponent : ExploderComponent {
	
	public Light lightning; 
	public float radius = 0;

	IEnumerator lightUp(Exploder exploder) {
		Light light = (Light) GameObject.Instantiate(lightning, transform.position, transform.rotation);
		light.transform.parent = transform;
		light.range = radius;
		float startIntencity = light.intensity;
		float startTime = Time.time;
		while (exploder.explodeDuration > Time.time - startTime) {
			light.intensity = Mathf.Lerp(startIntencity, 0, (Time.time - startTime) / exploder.explodeDuration); 
			yield return new WaitForEndOfFrame();
		}
		GameObject.Destroy(light);
		yield return null;
	}

	public override void onExplosionStarted(Exploder exploder) {
		if (radius < 0.0001f) {
			radius = exploder.radius;
		}
		StartCoroutine("lightUp", exploder);
	}
}
