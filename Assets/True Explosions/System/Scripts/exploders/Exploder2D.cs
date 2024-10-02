using UnityEngine;
using System.Collections;

public class Exploder2D : Exploder {
	public override void disableCollider() {
		if (GetComponent<Collider2D>()) {
			wasTrigger = GetComponent<Collider2D>().isTrigger;
			GetComponent<Collider2D>().isTrigger = true;
		}
	}
	
	public override void enableCollider() {
		if (GetComponent<Collider2D>()) {
			GetComponent<Collider2D>().isTrigger = wasTrigger;
		}
	}


	protected override void shootFromCurrentPosition() {
		Vector2 probeDir = Random.insideUnitCircle;
		probeDir.Normalize();
		Vector2 start = new Vector2(transform.position.x, transform.position.y);
		Ray2D testRay = new Ray2D(start, probeDir);
		shootRay(testRay, radius);
	}

	void Start() {
		init();
		power *= 10;
	}

	private void shootRay(Ray2D testRay, float estimatedRadius) {
		RaycastHit2D hit = Physics2D.Raycast(testRay.origin, testRay.direction, estimatedRadius);
		if (hit.collider != null) {
			if (hit.rigidbody != null) {
				hit.rigidbody.AddForceAtPosition(power * Time.deltaTime * testRay.direction / probeCount, hit.point);
				estimatedRadius /= 2;
			} else {
				Vector2 reflectVec = Random.insideUnitCircle.normalized;
				if (Vector2.Dot(reflectVec, hit.normal) < 0) {
					reflectVec *= -1;
				}
				Ray2D emittedRay = new Ray2D(hit.point, reflectVec);
				if (Random.Range(0, 20) > 1) shootRay(emittedRay, estimatedRadius - hit.fraction);
			}
		}
	}

}

