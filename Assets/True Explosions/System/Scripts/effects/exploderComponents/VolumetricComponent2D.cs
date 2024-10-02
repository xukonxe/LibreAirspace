using UnityEngine;
using System.Collections;

public class VolumetricComponent2D : ExploderComponent{
	public float duration = 2;
	public float centerEmission = 20000;
	public float centerEmissionDuration = 0.2f; 
	public float radius = 0;
	public int startEmission = 1000;
	public int emission = 1000;
	public int maxParticles = 100000;
	public Gradient colorOverLifetime;
	public AnimationCurve alphaOverLifetime = AnimationCurve.EaseInOut(0, 1, 1, 0);
	public float particleSizeMultiplyer = 4;
	public int teleportationIterations = 4;
	public float teleportationThreshold = 1.5f;
	
	protected Exploder exploder;
	
	protected ParticleSystem.Particle[] particles;
	protected Vector2[] directions;
	protected int[] hitCount;
	protected float speed;
	protected int curCount = 0;

	public override void onExplosionStarted(Exploder exploder) {
		particles = new ParticleSystem.Particle[maxParticles];
		directions = new Vector2[maxParticles];
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
		
		GetComponent<ParticleSystem>().Emit(startEmission);
		GetComponent<ParticleSystem>().GetParticles(particles);
		
		for (int i = 0; i < startEmission; i++) {
			directions[i] = getAllignedDirection(new Vector2(1, 0), Random.Range(0, 180));
			particles[i].position = transform.position;
			particles[i].color = colorOverLifetime.Evaluate(0);
		}
		
		curCount = startEmission;
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
				directions[i] = Random.insideUnitCircle.normalized;
				particles[i].position = transform.position;
				moveParticle(i, Mathf.Max(Time.time - exploder.explosionTime - Time.deltaTime, Time.deltaTime * 0.1f) * speed);
			}
		} else {
			float emitAngle = Random.Range(20, 45);
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
		float copyAngle = Random.Range(20, 45);
		for (int i = 0; i < curCount; i++) {
			int copyId = Random.Range(0, curCount);
			if (hitCount[copyId] * teleportationThreshold < hitCount[i]) {
				directions[i] = getAllignedDirection(copyId, copyAngle);
				particles[i].position = particles[copyId].position;
				hitCount[i] = (int)(hitCount[copyId] * 1.4f);
			}
		}
	}
	
	private void makeStep() {
		exploder.disableCollider();
		float curSize = (8 * speed * (Time.time - exploder.explosionTime) / emission + 0.1f) * particleSizeMultiplyer;
		for (int i = 0; i < curCount; i++) {
			particles[i].size = curSize;
			moveParticle(i, Time.deltaTime * speed);
			particles[i].rotation = Time.time;
		}
		exploder.enableCollider();
		
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

	private Vector2 getAllignedDirection(Vector2 vector, float angle) {
		angle = (Random.Range(0, 2) == 0 ? -1 : 1) * angle;
		return Quaternion.AngleAxis(angle, new Vector3 (0, 0, 1)) * vector;
	}
	
	private Vector2 getAllignedDirection(int id, float angle) {
		return getAllignedDirection(directions[id], angle);
	}
	
	private Ray2D getAllignedRay(int id, float angle) {
		return new Ray2D(particles[id].position, getAllignedDirection(id, angle));
	}
	
	protected void moveParticle(int id, float distance) {
		Ray2D testRay = new Ray2D(particles[id].position, directions[id]);
		
		RaycastHit2D hit = Physics2D.Raycast(testRay.origin, testRay.direction, distance);
		if (hit.collider != null) {
			if (!hit.rigidbody) {
				Vector2 reflectVec = Random.insideUnitCircle.normalized;
				if (Vector2.Dot(reflectVec, hit.normal) < 0) {
					reflectVec *= -1;
				}
				directions[id] = reflectVec;
				particles[id].position = testRay.origin + (hit.point - testRay.origin) * 0.8f;			
			} else {
				hit.collider.enabled = false;
				particles[id].position = hit.point;			
				//moveParticle(id, distance - (hit.point - testRay.origin).magnitude);
				hit.collider.enabled = true;
			}
			hitCount[id]++;
		} else {
			particles[id].position = testRay.origin + testRay.direction * distance;
		}
	}
}
