using UnityEngine;
using System.Collections;

public class VolumetricComponent : ExploderComponent {
	public float duration = 2;
	public float centerEmission = 20000;
	public float centerEmissionDuration = 0.2f; 
	public float radius = 0; 
	public int startEmission = 3000;
	public int emission = 3000;
	public int maxParticles = 100000;
	public Gradient colorOverLifetime;
	public AnimationCurve alphaOverLifetime = AnimationCurve.EaseInOut(0, 1, 1, 0);
	public float particleSizeMultiplyer = 4;
	public int teleportationIterations = 4;
	public float teleportationThreshold = 1.5f;

	protected Exploder exploder;

	protected ParticleSystem.Particle[] particles;
	protected Vector3[] directions;
	protected int[] hitCount;
	protected float speed;
	protected int curCount = 0;

	public override void onExplosionStarted(Exploder exploder) {
		particles = new ParticleSystem.Particle[maxParticles];
		directions = new Vector3[maxParticles];
		hitCount = new int[maxParticles];

		if (GetComponent<ParticleSystem>() == null) {
			gameObject.AddComponent<ParticleSystem>();
		}
		this.exploder = exploder;
		if (radius < 0.0001f) {
			radius = exploder.radius;
		}
		speed = radius / duration;

		initParticleSystem();

		StartCoroutine("emulate");
	}

	private void initParticleSystem() {
		GetComponent<ParticleSystem>().maxParticles = maxParticles;
		GetComponent<ParticleSystem>().emissionRate = 0;
		GetComponent<ParticleSystem>().startSpeed = 0;
		GetComponent<ParticleSystem>().startSize = 1.0f;
		GetComponent<ParticleSystem>().simulationSpace = ParticleSystemSimulationSpace.World;
		GetComponent<ParticleSystem>().startLifetime = duration;

		GetComponent<ParticleSystem>().Emit(startEmission);
		curCount = GetComponent<ParticleSystem>().GetParticles(particles);

		for (int i = 0; i < curCount; i++) {
			directions[i] = Random.onUnitSphere;
			particles[i].position = transform.position;
			particles[i].color = colorOverLifetime.Evaluate(0);
		}

		GetComponent<ParticleSystem>().SetParticles(particles, curCount);
	}

	protected void emitNewParticles() {
		if ((Time.time - exploder.explosionTime) / duration < centerEmissionDuration) {
			GetComponent<ParticleSystem>().Emit(Mathf.Min((int) (centerEmission * Time.deltaTime), maxParticles - curCount));
		} else {
			GetComponent<ParticleSystem>().Emit(Mathf.Min((int) (emission * Time.deltaTime), maxParticles - curCount));
		}
		int nextCount = GetComponent<ParticleSystem>().GetParticles(particles);

		if ((Time.time - exploder.explosionTime) / duration < centerEmissionDuration) {
			for (int i = curCount; i < nextCount; i++) {
				directions[i] = Random.onUnitSphere;
				particles[i].position = transform.position;
				moveParticle(i, Mathf.Max(Time.time - exploder.explosionTime - Time.deltaTime, Time.deltaTime * 0.1f) * speed);
			}
		} else {
			float emitAngle = Random.Range(Mathf.PI / 6, Mathf.PI / 3);
			for (int i = curCount; i < nextCount; i++) {
				int copyId = Random.Range(0, curCount);
				directions[i] = getAllignedDirection(copyId, emitAngle);
				particles[i].position = particles[copyId].position;
				hitCount[i] = hitCount[copyId];
			}
		}

		curCount = nextCount;
	}

	private void teleportBadParticles() {
		float copyAngle = Random.Range(0, Mathf.PI / 6);
		for (int i = 0; i < curCount; i++) {
			int copyId = Random.Range(0, curCount);
			if (hitCount[copyId] * teleportationThreshold < hitCount[i]) {
				directions[i] = getAllignedDirection(copyId, copyAngle);
				particles[i].position = particles[copyId].position;
				hitCount[i] = hitCount[copyId] + 1;
			}
		}
	}

	private void makeStep() {
		float curSize = 2 * speed * Mathf.Sqrt ((Time.time - exploder.explosionTime) / emission) * particleSizeMultiplyer;
		for (int i = 0; i < curCount; i++) {
			particles[i].size = curSize;
			moveParticle(i, Time.deltaTime * speed);
			particles[i].rotation = Time.time;
		}
		
		for (int i = 0; i < teleportationIterations; i++) {
			teleportBadParticles();
		}
	}

	private void resetColors() {
		float alpha = alphaOverLifetime.Evaluate((Time.time - exploder.explosionTime) / duration);
		Color curColor = colorOverLifetime.Evaluate((Time.time - exploder.explosionTime) / duration);
		curColor.a = alpha;
		for (int i = 0; i < curCount; i++) {
			particles[i].color = curColor;
		}
	}
	
	IEnumerator emulate() {
		while (Time.time - exploder.explosionTime < duration) {
			emitNewParticles();
			makeStep();
			resetColors();

			GetComponent<ParticleSystem>().SetParticles(particles, curCount);
			
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}
	
	private Vector3 getAllignedDirection(int id, float angle) {
		float beta;
		beta = Random.Range(0, 2 * Mathf.PI);
		Vector3 randomDir = new Vector3(Mathf.Sin(angle) * Mathf.Cos(beta), Mathf.Sin(angle) * Mathf.Sin(beta), Mathf.Cos(angle));
		Quaternion rotation = Quaternion.LookRotation(randomDir);
		return rotation * directions[id];
	}
	
	private Ray getAllignedRay(int id, float angle) {
		return new Ray(particles[id].position, getAllignedDirection(id, angle));
	}
	
	protected void moveParticle(int id, float distance) {
		Ray testRay = new Ray(particles[id].position, directions[id]);
		
		RaycastHit hit;
		if (Physics.Raycast(testRay, out hit, distance)) {
			if (!hit.rigidbody) {
				Vector3 reflectVec = Random.onUnitSphere;
				if (Vector3.Dot(reflectVec, hit.normal) < 0) {
					reflectVec *= -1;
				}
				directions[id] = reflectVec;	
			}
			particles[id].position = testRay.origin + testRay.direction * hit.distance * 0.95f;		
			hitCount[id]++;
		} else {
			particles[id].position = testRay.origin + testRay.direction * distance;
		}
	}
}
