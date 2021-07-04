using GlobalEnums;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using HKHKHKHKHK.Utils;
using Logger = Modding.Logger;
using Object = System.Object;
using SFCore.Utils;

namespace HKHKHKHKHK.MonoBehaviours
{
    class VVVVVHandler: MonoBehaviour
    {
        private IEnumerator m_Loop = null;
        private bool m_upsideDown = false;
        private HeroController m_hc = null;
        private Rigidbody2D m_rb2d = null;
        private InputHandler m_ih = null;
        private Collider2D m_col2d = null;

        private IEnumerator Start()
        {
            Awake();
            yield break;
        }
        public void OnEnable()
        {
            Awake();
        }

        public void Awake()
        {
            if (m_Loop != null)
            {
                StopAllCoroutines();
                m_Loop = null;
            }

            m_hc = gameObject.GetComponent<HeroController>();
            m_rb2d = gameObject.GetComponent<Rigidbody2D>();
            m_ih = m_hc.Get<InputHandler>("inputHandler");
            m_col2d = m_hc.Get<Collider2D>("col2d");

			m_Loop = Loop();
            StartCoroutine(m_Loop);
        }

        public void OnDestroy()
        {
            OnDisable();
        }

        public void OnDisable()
        {
            if (m_Loop != null)
            {
                StopAllCoroutines();
                m_Loop = null;
            }
            m_hc = null;
            m_rb2d = null;
            m_ih = null;
            m_col2d = null;

        }

        private void CallBacks()
        {
            //On.HeroController.FixedUpdate += OnHeroControllerFixedUpdate;
			On.HeroController.CancelHeroJump += OnHeroControllerCancelHeroJump;
			On.HeroController.CanDreamNail += OnHeroControllerCanDreamNail;
			On.HeroController.FallCheck += OnHeroControllerFallCheck;
			On.HeroController.JumpReleased += OnHeroControllerJumpReleased;
			On.HeroController.CheckForBump += OnHeroControllerCheckForBump;
			On.HeroController.CheckNearRoof += OnHeroControllerCheckNearRoof;
			On.HeroController.CheckTouchingGround += OnHeroControllerCheckTouchingGround;
        }

        private bool OnHeroControllerCheckTouchingGround(On.HeroController.orig_CheckTouchingGround orig, HeroController self)
		{
			Vector2 vector = new Vector2(m_col2d.bounds.min.x, m_col2d.bounds.center.y);
            Vector2 vector2 = m_col2d.bounds.center;
            Vector2 vector3 = new Vector2(m_col2d.bounds.max.x, m_col2d.bounds.center.y);
            float distance = m_col2d.bounds.extents.y + 0.16f;
            UnityEngine.Debug.DrawRay(vector, m_upsideDown ? Vector2.up : Vector2.down, Color.yellow);
            UnityEngine.Debug.DrawRay(vector2, m_upsideDown ? Vector2.up : Vector2.down, Color.yellow);
            UnityEngine.Debug.DrawRay(vector3, m_upsideDown ? Vector2.up : Vector2.down, Color.yellow);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, m_upsideDown ? Vector2.up : Vector2.down, distance, 256);
            RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector2, m_upsideDown ? Vector2.up : Vector2.down, distance, 256);
            RaycastHit2D raycastHit2D3 = Physics2D.Raycast(vector3, m_upsideDown ? Vector2.up : Vector2.down, distance, 256);
            return raycastHit2D.collider != null || raycastHit2D2.collider != null || raycastHit2D3.collider != null;
		}

        private IEnumerator Loop()
        {
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    m_upsideDown = !m_upsideDown;
                    Switch(m_upsideDown);
                }
                yield return null;
            }
        }

        private void Switch(bool nowState)
        {
            m_hc.SetAttr<HeroController, float>("BUMP_VELOCITY", m_hc.GetAttr<HeroController, float>("BUMP_VELOCITY") * -1);
            m_hc.BOUNCE_VELOCITY *= -1;
            m_hc.WALLSLIDE_DECEL *= -1;
            m_hc.WALLSLIDE_SPEED *= -1;
            m_hc.SWIM_ACCEL *= -1;
            m_hc.SWIM_MAX_SPEED *= -1;
            m_hc.JUMP_SPEED_UNDERWATER *= -1;
            m_hc.JUMP_SPEED *= -1;
            m_hc.SHROOM_BOUNCE_VELOCITY *= -1;
            m_hc.RECOIL_DOWN_VELOCITY *= -1;
            m_hc.MAX_FALL_VELOCITY *= -1;
            m_hc.MAX_FALL_VELOCITY_UNDERWATER *= -1;
            //m_rb2d.gravityScale *= -1;
            Physics2D.gravity *= -1;
            Vector3 tmpVec3 = gameObject.transform.localScale;
            tmpVec3.y *= -1;
            gameObject.transform.localScale = tmpVec3;
        }

        #region HeroController Methods Copy Paste for Gravity Flip

        private void OnHeroControllerFixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
		{
            if (self.cState.recoilingLeft || self.cState.recoilingRight)
			{
				if ((float)self.Get<int>("recoilSteps") <= self.RECOIL_HOR_STEPS)
				{
					self.Inc("recoilSteps");
				}
				else
				{
					self.Inv("CancelRecoilHorizontal", null);
				}
			}
			if (self.cState.dead)
			{
				m_rb2d.velocity = new Vector2(0f, 0f);
			}
			if ((self.hero_state == ActorStates.hard_landing && !self.cState.onConveyor) || self.hero_state == ActorStates.dash_landing)
			{
                self.Inv("ResetMotion", null);
			}
			else if (self.hero_state == ActorStates.no_input)
			{
				if (self.cState.transitioning)
				{
					if (self.transitionState == HeroTransitionState.EXITING_SCENE)
					{
						self.AffectedByGravity(false);
						if (!self.Get<bool>("stopWalkingOut"))
						{
							m_rb2d.velocity = new Vector2(self.Get<Vector2>("transition_vel").x, self.Get<Vector2>("transition_vel").y + m_rb2d.velocity.y);
						}
					}
					else if (self.transitionState == HeroTransitionState.ENTERING_SCENE)
					{
						m_rb2d.velocity = self.Get<Vector2>("transition_vel");
					}
					else if (self.transitionState == HeroTransitionState.DROPPING_DOWN)
					{
						m_rb2d.velocity = new Vector2(self.Get<Vector2>("transition_vel").x, m_rb2d.velocity.y);
					}
				}
				else if (self.cState.recoiling)
				{
					self.AffectedByGravity(false);
					m_rb2d.velocity = self.Get<Vector2>("recoilVector");
				}
			}
			else if (self.hero_state != ActorStates.no_input)
			{
				if (self.hero_state == ActorStates.running)
				{
					if (self.move_input > 0f)
					{
						if (self.CheckForBump(CollisionSide.right))
						{
							m_rb2d.velocity = new Vector2(m_rb2d.velocity.x, self.Get<float>("BUMP_VELOCITY"));
						}
					}
					else if (self.move_input < 0f && self.CheckForBump(CollisionSide.left))
					{
						m_rb2d.velocity = new Vector2(m_rb2d.velocity.x, self.Get<float>("BUMP_VELOCITY"));
					}
				}
				if (!self.cState.backDashing && !self.cState.dashing)
				{
                    self.Inv("Move", new object[] { self.move_input });
					if ((!self.cState.attacking || self.Get<float>("attack_time") >= self.ATTACK_RECOVERY_TIME) && !self.cState.wallSliding && !self.wallLocked)
					{
						if (self.move_input > 0f && !self.cState.facingRight)
						{
							self.FlipSprite();
                            self.Inv("CancelAttack", null);
						}
						else if (self.move_input < 0f && self.cState.facingRight)
						{
							self.FlipSprite();
                            self.Inv("CancelAttack", null);
						}
					}
					if (self.cState.recoilingLeft)
					{
						float num;
						if (self.Get<bool>("recoilLarge"))
						{
							num = self.RECOIL_HOR_VELOCITY_LONG;
						}
						else
						{
							num = self.RECOIL_HOR_VELOCITY;
						}
						if (m_rb2d.velocity.x > -num)
						{
							m_rb2d.velocity = new Vector2(-num, m_rb2d.velocity.y);
						}
						else
						{
							m_rb2d.velocity = new Vector2(m_rb2d.velocity.x - num, m_rb2d.velocity.y);
						}
					}
					if (self.cState.recoilingRight)
					{
						float num2;
						if (self.Get<bool>("recoilLarge"))
						{
							num2 = self.RECOIL_HOR_VELOCITY_LONG;
						}
						else
						{
							num2 = self.RECOIL_HOR_VELOCITY;
						}
						if (m_rb2d.velocity.x < num2)
						{
							m_rb2d.velocity = new Vector2(num2, m_rb2d.velocity.y);
						}
						else
						{
							m_rb2d.velocity = new Vector2(m_rb2d.velocity.x + num2, m_rb2d.velocity.y);
						}
					}
				}
				if ((self.cState.lookingUp || self.cState.lookingDown) && Mathf.Abs(self.move_input) > 0.6f)
				{
                    self.Inv("ResetLook", null);
				}
				if (self.cState.jumping)
				{
                    self.Inv("Jump", null);
				}
				if (self.cState.doubleJumping)
				{
                    self.Inv("DoubleJump", null);
				}
				if (self.cState.dashing)
				{
                    self.Inv("Dash", null);
				}
				if (self.cState.casting)
				{
					if (self.cState.castRecoiling)
					{
						if (self.cState.facingRight)
						{
							m_rb2d.velocity = new Vector2(-self.CAST_RECOIL_VELOCITY, 0f);
						}
						else
						{
							m_rb2d.velocity = new Vector2(self.CAST_RECOIL_VELOCITY, 0f);
						}
					}
					else
					{
						m_rb2d.velocity = Vector2.zero;
					}
				}
				if (self.cState.bouncing)
				{
					m_rb2d.velocity = new Vector2(m_rb2d.velocity.x, self.BOUNCE_VELOCITY);
				}
				if (self.cState.shroomBouncing)
				{
				}
				if (self.wallLocked)
				{
					if (self.Get<bool>("wallJumpedR"))
					{
						m_rb2d.velocity = new Vector2(self.Get<float>("currentWalljumpSpeed"), m_rb2d.velocity.y);
					}
					else if (self.Get<bool>("wallJumpedL"))
					{
						m_rb2d.velocity = new Vector2(-self.Get<float>("currentWalljumpSpeed"), m_rb2d.velocity.y);
					}
					self.Inc("wallLockSteps");
					if (self.Get<int>("wallLockSteps") > self.WJLOCK_STEPS_LONG)
					{
						self.wallLocked = false;
					}
					self.Set<float>("currentWalljumpSpeed", self.Get<float>("currentWalljumpSpeed") - self.Get<float>("walljumpSpeedDecel"));
				}
				if (self.cState.wallSliding)
				{
					if (self.wallSlidingL && m_ih.inputActions.right.IsPressed)
					{
						self.Inc("wallUnstickSteps");
					}
					else if (self.wallSlidingR && m_ih.inputActions.left.IsPressed)
					{
						self.Inc("wallUnstickSteps");
					}
					else
					{
						self.Set<int>("wallUnstickSteps", 0);
					}
					if (self.Get<int>("wallUnstickSteps") >= self.WALL_STICKY_STEPS)
					{
                        self.Inv("CancelWallsliding", null);
					}
					if (self.wallSlidingL)
					{
						
						if (!self.Inv<bool>("CheckStillTouchingWall", new object[] { CollisionSide.left, false }))
						{
							self.FlipSprite();
                            self.Inv("CancelWallsliding", null);
						}
					}
					else if (self.wallSlidingR && !self.Inv<bool>("CheckStillTouchingWall", new object[] { CollisionSide.right, false }))
					{
						self.FlipSprite();
                        self.Inv("CancelWallsliding", null);
					}
				}
			}
            if (Mathf.Abs(m_rb2d.velocity.y) > Mathf.Abs(self.MAX_FALL_VELOCITY) && !self.inAcid && !self.controlReqlinquished && !self.cState.shadowDashing && !self.cState.spellQuake)
			{
				m_rb2d.velocity = new Vector2(m_rb2d.velocity.x, -self.MAX_FALL_VELOCITY);
			}
			if (self.Get<bool>("jumpQueuing"))
			{
				self.Inc("jumpQueueSteps");
			}
			if (self.Get<bool>("doubleJumpQueuing"))
			{
				self.Inc("doubleJumpQueueSteps");
			}
			if (self.Get<bool>("dashQueuing"))
			{
				self.Inc("dashQueueSteps");
			}
			if (self.Get<bool>("attackQueuing"))
			{
				self.Inc("attackQueueSteps");
			}
			if (self.cState.wallSliding && !self.cState.onConveyorV)
			{
                if (Mathf.Abs(m_rb2d.velocity.y) < Mathf.Abs(self.WALLSLIDE_SPEED))
				{
					m_rb2d.velocity = new Vector3(m_rb2d.velocity.x, m_rb2d.velocity.y - self.WALLSLIDE_DECEL);
                    if (Mathf.Abs(m_rb2d.velocity.y) > Mathf.Abs(self.WALLSLIDE_SPEED))
					{
						m_rb2d.velocity = new Vector3(m_rb2d.velocity.x, self.WALLSLIDE_SPEED);
					}
				}
				if (Mathf.Abs(m_rb2d.velocity.y) > Mathf.Abs(self.WALLSLIDE_SPEED))
				{
					m_rb2d.velocity = new Vector3(m_rb2d.velocity.x, m_rb2d.velocity.y + self.WALLSLIDE_DECEL);
					if (Mathf.Abs(m_rb2d.velocity.y) > Mathf.Abs(self.WALLSLIDE_SPEED))
					{
						m_rb2d.velocity = new Vector3(m_rb2d.velocity.x, self.WALLSLIDE_SPEED);
					}
				}
			}
			if (self.Get<bool>("nailArt_cyclone"))
			{
				if (m_ih.inputActions.right.IsPressed && !m_ih.inputActions.left.IsPressed)
				{
					m_rb2d.velocity = new Vector3(self.CYCLONE_HORIZONTAL_SPEED, m_rb2d.velocity.y);
				}
				else if (m_ih.inputActions.left.IsPressed && !m_ih.inputActions.right.IsPressed)
				{
					m_rb2d.velocity = new Vector3(-self.CYCLONE_HORIZONTAL_SPEED, m_rb2d.velocity.y);
				}
				else
				{
					m_rb2d.velocity = new Vector3(0f, m_rb2d.velocity.y);
				}
			}
			if (self.cState.swimming)
			{
				m_rb2d.velocity = new Vector3(m_rb2d.velocity.x, m_rb2d.velocity.y + self.SWIM_ACCEL);
                if (Mathf.Abs(m_rb2d.velocity.y) < Mathf.Abs(self.SWIM_MAX_SPEED))
				{
					m_rb2d.velocity = new Vector3(m_rb2d.velocity.x, self.SWIM_MAX_SPEED);
				}
			}
			if (self.cState.superDashOnWall && !self.cState.onConveyorV)
			{
				m_rb2d.velocity = new Vector3(0f, 0f);
			}
			if (self.cState.onConveyor && ((self.cState.onGround && !self.cState.superDashing) || self.hero_state == ActorStates.hard_landing))
			{
				if (self.cState.freezeCharge || self.hero_state == ActorStates.hard_landing || self.controlReqlinquished)
				{
					m_rb2d.velocity = new Vector3(0f, 0f);
				}
				m_rb2d.velocity = new Vector2(m_rb2d.velocity.x + self.conveyorSpeed, m_rb2d.velocity.y);
			}
			if (self.cState.inConveyorZone)
			{
				if (self.cState.freezeCharge || self.hero_state == ActorStates.hard_landing)
				{
					m_rb2d.velocity = new Vector3(0f, 0f);
				}
				m_rb2d.velocity = new Vector2(m_rb2d.velocity.x + self.conveyorSpeed, m_rb2d.velocity.y);
				self.superDash.SendEvent("SLOPE CANCEL");
			}
			if (self.cState.slidingLeft && m_rb2d.velocity.x > -5f)
			{
				m_rb2d.velocity = new Vector2(-5f, m_rb2d.velocity.y);
			}
			if (self.Get<int>("landingBufferSteps") > 0)
			{
				self.Dec("landingBufferSteps");
			}
			if (self.Get<int>("ledgeBufferSteps") > 0)
			{
				self.Dec("ledgeBufferSteps");
			}
			if (self.Get<int>("headBumpSteps") > 0)
			{
				self.Dec("headBumpSteps");
			}
			if (self.Get<int>("jumpReleaseQueueSteps") > 0)
			{
				self.Dec("jumpReleaseQueueSteps");
			}
            Vector2[] tmpVec2 = self.Get<Vector2[]>("positionHistory");
            tmpVec2[1] = tmpVec2[0];
            tmpVec2[0] = self.transform.position;
            self.Set<Vector2[]>("positionHistory", tmpVec2);
			self.cState.wasOnGround = self.cState.onGround;
		}

        private void OnHeroControllerCancelHeroJump(On.HeroController.orig_CancelHeroJump orig, HeroController self)
        {
            if (self.cState.jumping)
            {
                self.Inv("CancelJump", null);
                self.Inv("CancelDoubleJump", null);
                if ((!m_upsideDown && (m_rb2d.velocity.y > 0f)) || (m_upsideDown && (m_rb2d.velocity.y < 0f)))
                {
                    m_rb2d.velocity = new Vector2(m_rb2d.velocity.x, 0f);
                }
            }
        }

        private bool OnHeroControllerCanDreamNail(On.HeroController.orig_CanDreamNail orig, HeroController self)
        {
            return !GameManager.instance.isPaused && self.hero_state != ActorStates.no_input && !self.cState.dashing && !self.cState.backDashing && (!self.cState.attacking || self.Get<float>("attack_time") >= self.ATTACK_RECOVERY_TIME) && !self.controlReqlinquished && !self.cState.hazardDeath && ((!m_upsideDown && (m_rb2d.velocity.y > -0.1f)) || (m_upsideDown && (m_rb2d.velocity.y < 0.1f))) && !self.cState.hazardRespawning && !self.cState.recoilFrozen && !self.cState.recoiling && !self.cState.transitioning && self.playerData.GetBool("hasDreamNail") && self.cState.onGround;
        }

		private void OnHeroControllerFallCheck(On.HeroController.orig_FallCheck orig, HeroController self)
		{
			if (m_rb2d.velocity.y <= -1E-06f)
			{
				if (!self.CheckTouchingGround())
				{
					self.cState.falling = true;
					self.cState.onGround = false;
					self.cState.wallJumping = false;
					self.proxyFSM.SendEvent("HeroCtrl-LeftGround");
					if (self.hero_state != ActorStates.no_input)
					{
						self.Inv("SetState", new object[] { ActorStates.airborne });
					}
					if (self.cState.wallSliding)
					{
						self.Set<float>("fallTimer", 0f);
					}
					else
					{
						self.Add("fallTimer", Time.deltaTime);
					}
					if (self.fallTimer > self.BIG_FALL_TIME)
					{
						if (!self.cState.willHardLand)
						{
							self.cState.willHardLand = true;
						}
						if (!self.Get<bool>("fallRumble"))
						{
							self.Inv("StartFallRumble", null);
						}
					}
					if (self.Get<bool>("fallCheckFlagged"))
					{
						self.Set<bool>("fallCheckFlagged", false);
					}
				}
			}
			else
			{
				self.cState.falling = false;
				self.Set<float>("fallTimer", 0f);
				if (self.transitionState != HeroTransitionState.ENTERING_SCENE)
				{
					self.cState.willHardLand = false;
				}
				if (self.Get<bool>("fallCheckFlagged"))

				{
					self.Set<bool>("fallCheckFlagged", false);
				}
				if (self.Get<bool>("fallRumble"))

				{
					self.Inv("CancelFallEffects", null);
				}
			}
		}

        private void OnHeroControllerJumpReleased(On.HeroController.orig_JumpReleased orig, HeroController self)
        {
            if (m_rb2d.velocity.y > 0f && self.Get<int>("jumped_steps") >= self.JUMP_STEPS_MIN && !self.inAcid && !self.cState.shroomBouncing)
            {
                if (self.Get<bool>("jumpReleaseQueueingEnabled"))

                {
                    if (self.Get<bool>("jumpReleaseQueuing") && self.Get<int>("jumpReleaseQueueSteps") <= 0)

                    {
                        m_rb2d.velocity = new Vector2(m_rb2d.velocity.x, 0f);
                        self.Inv("CancelJump", null);
                    }
                }
                else
                {
                    m_rb2d.velocity = new Vector2(m_rb2d.velocity.x, 0f);
                    self.Inv("CancelJump", null);
                }
            }
            self.Set<bool>("jumpQueuing", false);
            self.Set<bool>("doubleJumpQueuing", false);
            if (self.cState.swimming)
            {
                self.cState.swimming = false;
            }
        }

		private bool OnHeroControllerCheckForBump(On.HeroController.orig_CheckForBump orig, HeroController self, CollisionSide side)
		{
			float numDown = 0.025f * (m_upsideDown ? -1 : 1);
			float numUp = 0.2f * (m_upsideDown ? -1 : 1);
			float yCheck = m_upsideDown ? -m_col2d.bounds.max.y : m_col2d.bounds.min.y;
			float num2 = 0.2f;
			Vector2 vector = new Vector2(m_col2d.bounds.min.x + num2, yCheck + numUp);
			Vector2 vector2 = new Vector2(m_col2d.bounds.min.x + num2, yCheck - numDown);
			Vector2 vector3 = new Vector2(m_col2d.bounds.max.x - num2, yCheck + numUp);
			Vector2 vector4 = new Vector2(m_col2d.bounds.max.x - num2, yCheck - numDown);
			float num3 = 0.32f + num2;
			RaycastHit2D raycastHit2D = default(RaycastHit2D);
			RaycastHit2D raycastHit2D2 = default(RaycastHit2D);
			if (side == CollisionSide.left)
			{
				UnityEngine.Debug.DrawLine(vector2, vector2 + Vector2.left * num3, Color.cyan, 0.15f);
				UnityEngine.Debug.DrawLine(vector, vector + Vector2.left * num3, Color.cyan, 0.15f);
				raycastHit2D2 = Physics2D.Raycast(vector2, Vector2.left, num3, 256);
				raycastHit2D = Physics2D.Raycast(vector, Vector2.left, num3, 256);
			}
			else if (side == CollisionSide.right)
			{
				UnityEngine.Debug.DrawLine(vector4, vector4 + Vector2.right * num3, Color.cyan, 0.15f);
				UnityEngine.Debug.DrawLine(vector3, vector3 + Vector2.right * num3, Color.cyan, 0.15f);
				raycastHit2D2 = Physics2D.Raycast(vector4, Vector2.right, num3, 256);
				raycastHit2D = Physics2D.Raycast(vector3, Vector2.right, num3, 256);
			}
			else
			{
				UnityEngine.Debug.LogError("Invalid CollisionSide specified.");
			}
			if (raycastHit2D2.collider != null && raycastHit2D.collider == null)
			{
				Vector2 down = m_upsideDown ? Vector2.up : Vector2.down;
				Vector2 vector5 = raycastHit2D2.point + new Vector2((side != CollisionSide.right) ? -0.1f : 0.1f, 1f);
				RaycastHit2D raycastHit2D3 = Physics2D.Raycast(vector5, down, 1.5f, 256);
				Vector2 vector6 = raycastHit2D2.point + new Vector2((side != CollisionSide.right) ? 0.1f : -0.1f, 1f);
				RaycastHit2D raycastHit2D4 = Physics2D.Raycast(vector6, down, 1.5f, 256);
				if (raycastHit2D3.collider != null)
				{
					UnityEngine.Debug.DrawLine(vector5, raycastHit2D3.point, Color.cyan, 0.15f);
					if (!(raycastHit2D4.collider != null))
					{
						return true;
					}
					UnityEngine.Debug.DrawLine(vector6, raycastHit2D4.point, Color.cyan, 0.15f);
					float num4 = raycastHit2D3.point.y - raycastHit2D4.point.y;
					if (num4 > 0f)
					{
						UnityEngine.Debug.Log("Bump Height: " + num4);
						return true;
					}
				}
			}
			return false;
		}

        private bool OnHeroControllerCheckNearRoof(On.HeroController.orig_CheckNearRoof orig, HeroController self)
        {
            Vector2 origin = new Vector2(m_col2d.bounds.max.x, (m_upsideDown ? m_col2d.bounds.min : m_col2d.bounds.max).y);
            Vector2 origin2 = new Vector2(m_col2d.bounds.min.x, (m_upsideDown ? m_col2d.bounds.min : m_col2d.bounds.max).y);
            Vector2 vector = new Vector2(m_col2d.bounds.center.x, (m_upsideDown ? m_col2d.bounds.min : m_col2d.bounds.max).y);
            Vector2 origin3 = new Vector2(m_col2d.bounds.center.x + m_col2d.bounds.size.x / 4f, (m_upsideDown ? m_col2d.bounds.min : m_col2d.bounds.max).y);
            Vector2 origin4 = new Vector2(m_col2d.bounds.center.x - m_col2d.bounds.size.x / 4f, (m_upsideDown ? m_col2d.bounds.min : m_col2d.bounds.max).y);
            Vector2 direction = new Vector2(-0.5f, m_upsideDown ? -1f : 1f);
            Vector2 direction2 = new Vector2(0.5f, m_upsideDown ? -1f : 1f);
            Vector2 up = m_upsideDown ? Vector2.down : Vector2.up;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin2, direction, 2f, 256);
            RaycastHit2D raycastHit2D2 = Physics2D.Raycast(origin, direction2, 2f, 256);
            RaycastHit2D raycastHit2D3 = Physics2D.Raycast(origin3, up, 1f, 256);
            RaycastHit2D raycastHit2D4 = Physics2D.Raycast(origin4, up, 1f, 256);
            return raycastHit2D.collider != null || raycastHit2D2.collider != null || raycastHit2D3.collider != null || raycastHit2D4.collider != null;
        }





		#endregion


		private void Log(string message)
        {
            Logger.Log($"[{GetType().FullName?.Replace(".", "]:[")}] - {message}");
        }

        private void Log(object message)
        {
            Logger.Log($"[{GetType().FullName?.Replace(".", "]:[")}] - {message}");
        }
    }
}
