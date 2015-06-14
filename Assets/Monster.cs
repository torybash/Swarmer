using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour {

	private Vector2 velocity = Vector3.zero;
	public Vector2 Velocity { get{ return velocity; } }

	private Settings sts = null;
	public Settings Sts { get{ return sts; } }

	private MonsterSettings mSts = null;
	public MonsterSettings MSts { get{ return mSts; } }

	private DebugSettings dbgSts = null;
	public DebugSettings DbgSts { get{ return dbgSts; } }

	[SerializeField] SpriteRenderer bodySR;
	[SerializeField] SpriteRenderer legsSR;



	void Awake(){
		if( sts == null )
		{
			Debug.LogWarning( "Boid initialized with standalone settings copy" );
			sts = new Settings();
		}

		if( dbgSts == null )
		{
			Debug.LogWarning( "Boid initialized with standalone debug settings copy" );
			dbgSts = new DebugSettings();
		}
	}

	public void Init(MonsterSettings mSts)
	{
		this.mSts = mSts;

		bodySR.transform.localScale *= Mathf.Sqrt(mSts.age);
		legsSR.transform.localScale = bodySR.transform.localScale * Mathf.Sqrt(mSts.legStrength);

		sts.OptDistance = 0.2f * mSts.age;
	}


	void FixedUpdate()
	{
		//Bird is affected by 3 base forses:
		// cohesion
		// separation + collisionAvoidance
		// alignmentForce
		
		var sepForce = new BoidTools.SeparationForce(sts);
		var collAvoid = new BoidTools.CollisionAvoidanceForce( sts, sepForce.Calc(sts.OptDistance) );
		
		//Geometric center of visible birds
		Vector2 centeroid = Vector2.zero;
		
		Vector2 collisionAvoidance = Vector2.zero;
		Vector2 avgSpeed = Vector2.zero;
		var neighbourCount = 0;
		
		//Store it as an optimization
		Vector2 direction = transform.rotation * Vector2.up;
		Vector2 curPos = transform.position;

//		Debug.Log("DirCalc - transform.rotation: " + transform.rotation + " direction: " + direction);


		foreach( Collider2D coll in Physics2D.OverlapCircleAll(curPos, sts.ViewRadius) )
		{
			Vector2 collPos = coll.transform.position;
			Monster other;

			if( (other = coll.GetComponent<Monster>()) != null ) //Birds processing
			{
				Vector2 separationForce;
				
				if( !sepForce.Calc(curPos, collPos, out separationForce) )
					continue;
				
				collisionAvoidance += separationForce;
				++neighbourCount;
				centeroid += collPos;
				avgSpeed += other.velocity;
			}
//			else if( (trigger = vis.GetInterface<ITrigger>()) != null )
//			{
//				if( GetComponent<Collider>().bounds.Intersects(vis.bounds) )
//					trigger.OnTouch(this);
//			}
			else //Obstacles processing
			{
				BoidTools.CollisionAvoidanceForce.Force force;
				if( collAvoid.Calc(curPos, direction, coll, out force) )
				{
					collisionAvoidance += force.dir;
					
					if( dbgSts.enableDrawing && dbgSts.obstaclesAvoidanceDraw )
						Drawer.DrawRay( force.pos, force.dir, dbgSts.obstaclesAvoidanceColor );
				}
			}
		}




		if( neighbourCount > 0 )
		{
			//Cohesion force. It makes united formula with BoidTools.SeparationForce
			centeroid = centeroid / neighbourCount - curPos;
			
			//Spherical shape of flock looks unnatural, so let's scale it along y axis
			centeroid.y *= sts.VerticalPriority;
			
			//Difference between current bird speed and average speed of visible birds
			avgSpeed = avgSpeed / neighbourCount - velocity;
		}


		var positionForce = (1.0f - sts.AligmentForcePart) * sts.SpeedMultipliyer * (centeroid + collisionAvoidance);
		var alignmentForce = sts.AligmentForcePart * avgSpeed / Time.deltaTime;
		var attractionForce = CalculateAttractionForce( sts, curPos, velocity );
		var totalForce = sts.TotalForceMultipliyer * ( positionForce + alignmentForce + attractionForce );
		
		var newVelocity = (1 - sts.Inertness) * (totalForce * Time.deltaTime) + sts.Inertness * velocity;
		
		velocity = CalcNewVelocity( sts.MinSpeed, velocity, newVelocity, direction );
		
		var rotation = CalcRotation( sts.InclineFactor, velocity, totalForce );
//		
		if( MathTools.IsValid(rotation) )
			gameObject.transform.rotation = rotation;




		if( dbgSts.enableDrawing )
		{
			if( dbgSts.directionDraw )
				Drawer.DrawRay( curPos, direction, dbgSts.directionColor );

			if( dbgSts.velocityDraw )
				Drawer.DrawRay( curPos, velocity, dbgSts.velocityColor );
			
			if( dbgSts.positionForceDraw )
				Drawer.DrawRay( curPos, positionForce, dbgSts.positionForceColor );
			
			if( dbgSts.alignmentForceDraw )
				Drawer.DrawRay( curPos, alignmentForce, dbgSts.alignmentForceColor );
			
			if( dbgSts.cohesionForceDraw )
				Drawer.DrawRay( curPos, centeroid, dbgSts.cohesionForceColor );
			
			if( dbgSts.collisionsAvoidanceForceDraw )
				Drawer.DrawRay( curPos, collisionAvoidance, dbgSts.collisionsAvoidanceForceColor );
			
			if( dbgSts.attractionForceDraw )
				Drawer.DrawRay( curPos, attractionForce, dbgSts.attractionForceColor );
			
			if( dbgSts.totalForceDraw )
				Drawer.DrawRay( curPos, totalForce, dbgSts.totalForceColor );
		}
		
	}
	

	
	void Update () {
		Vector3 posChange = velocity * Time.deltaTime;
		transform.position += posChange;
	}





	//Force which attracts birds to waypoints
	static Vector2 CalculateAttractionForce( Settings sts, Vector2 curPos, Vector2 curVelocity )
	{
//		if( !sts.Trace )
//			return Vector3.zero;
		
//		var attrPos = sts.Trace.GetAtractionPoint();
		Vector2 attrPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 direction = (attrPos - curPos).normalized;
		
		//The force have an effect only on direction and shouldn't increase speed if bird flies in the right direction
		var factor = sts.AttractrionForce * sts.SpeedMultipliyer * MathTools.AngleToFactor( direction, curVelocity.normalized );
		
		return factor * direction;
	}



	static Vector3 CalcNewVelocity( float minSpeed, Vector2 curVel, Vector2 dsrVel, Vector3 defaultVelocity )
	{
		//We have to take into account that bird can't change their direction instantly. That's why
		//dsrVel (desired velocity) influence first of all on flying direction and after that on
		//velocity magnitude oneself
		
		var curVelLen = curVel.magnitude;
		
		if( curVelLen > MathTools.epsilon )
			curVel /= curVelLen;
		else
		{
			curVel = defaultVelocity;
			curVelLen = 1;
		}
		
		var dsrVelLen = dsrVel.magnitude;
		var resultLen = minSpeed;
		
		if( dsrVelLen > MathTools.epsilon )
		{
			dsrVel /= dsrVelLen;
			
			//We spend a part of velocity magnitude on bird rotation and the rest of it on speed magnitude changing
			
			//Map rotation to factor [0..1]
			var angleFactor = MathTools.AngleToFactor(dsrVel, curVel);
			
			//If dsrVel magnitude is twice bigger than curVelLen then bird can rotate on any angle
			var rotReqLength = 2 * curVelLen * angleFactor;
			
			//Velocity magnitude remained after rotation
			var speedRest = dsrVelLen - rotReqLength;
			
			if( speedRest > 0 )
			{
				curVel = dsrVel;
				resultLen = speedRest;
			}
			else
			{
				curVel = Vector3.Slerp( curVel, dsrVel, dsrVelLen / rotReqLength );
			}
			
			if( resultLen < minSpeed )
				resultLen = minSpeed;
		}
		
		return curVel * resultLen;
	}
	
	//Birds should incline when they turn
	static Quaternion CalcRotation( float inclineFactor, Vector2 velocity, Vector2 totalForce )
	{
		if( velocity.sqrMagnitude < MathTools.sqrEpsilon )
			return new Quaternion( float.NaN, float.NaN, float.NaN, float.NaN );
		
		//We project force on right vector and multiply it by factor
		
		//Instead of true calculation of right vector we use a trick with projection on XZ, but
		//this trick doesn't work if bird flies strictly vertically. In order to fix it we
		//have to know unmodified UP vector of bird.
//		var rightVec = MathTools.RightVectorXZProjected(velocity);
//		var inclineDeg = MathTools.VecProjectedLength( totalForce, rightVec ) * -inclineFactor;
//		return Quaternion.LookRotation( velocity ) * Quaternion.AngleAxis(Mathf.Clamp(inclineDeg, -90, 90), Vector3.forward);



//		float angle = -(180f - ((Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg) + 90f));
		float angle = 270f + Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;


//		Debug.Log("ANGLE CALC - vel: " + velocity + ", angle: " + angle);

		return Quaternion.AngleAxis(angle, Vector3.forward);
	}


	[System.Serializable]
	public class MonsterSettings
	{
		public float age;
		public float legStrength;
//		public float 

	}


	[System.Serializable]
	public class Settings
	{
		public float SpeedMultipliyer = 3.0f;
		public float ViewRadius = 0.5f;
		public float OptDistance = 0.2f;
		public float MinSpeed { get{ return 0.10f * SpeedMultipliyer; } }
		public float InclineFactor { get{ return 300.0f / SpeedMultipliyer; } }
		public float AligmentForcePart = 0.002f;
		public float TotalForceMultipliyer = 12;
		public float Inertness = 0.1f;
		public float VerticalPriority = 1.0f;


		public float AttractrionForce = 0.1f;
	}


	[System.Serializable]
	public class DebugSettings
	{
		public bool enableDrawing = false;

		public bool directionDraw = false;
		public Color directionColor = Color.yellow;

		public bool obstaclesAvoidanceDraw = true;
		public Color obstaclesAvoidanceColor = Color.red;
		
		public bool velocityDraw = false;
		public Color velocityColor = Color.grey;
		
		public bool positionForceDraw = false;
		public Color positionForceColor = Color.cyan;
		
		public bool alignmentForceDraw = false;
		public Color alignmentForceColor = Color.yellow;
		
		public bool cohesionForceDraw = false;
		public Color cohesionForceColor = Color.magenta;
		
		public bool collisionsAvoidanceForceDraw = false;
		public Color collisionsAvoidanceForceColor = Color.green;
		
		public bool attractionForceDraw = false;
		public Color attractionForceColor = Color.green;
		
		public bool totalForceDraw = false;
		public Color totalForceColor = Color.black;
	}
}
