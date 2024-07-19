using GlobalEnums;
using System.Collections;
using UnityEngine;
using HKHKHKHKHK.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Logger = Modding.Logger;
using SFCore.Utils;

namespace HKHKHKHKHK.MonoBehaviours
{
    class VvvvvHandler: MonoBehaviour
    {
        private IEnumerator _mLoop = null;
        private bool _mUpsideDown = false;
        private HeroController _mHc = null;
        private Rigidbody2D _mRb2d = null;
        private InputHandler _mIh = null;
        private Collider2D _mCol2d = null;

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
            if (_mLoop != null)
            {
                StopAllCoroutines();
                _mLoop = null;
            }

            _mHc = gameObject.GetComponent<HeroController>();
            _mRb2d = gameObject.GetComponent<Rigidbody2D>();
            _mIh = _mHc.Get<InputHandler>("inputHandler");
            _mCol2d = _mHc.Get<Collider2D>("col2d");

			_mLoop = Loop();
            StartCoroutine(_mLoop);
        }

        public void OnDestroy()
        {
            OnDisable();
        }

        public void OnDisable()
        {
            if (_mLoop != null)
            {
                StopAllCoroutines();
                _mLoop = null;
            }
            _mHc = null;
            _mRb2d = null;
            _mIh = null;
            _mCol2d = null;

        }

        private void CallBacks()
        {
			IL.HeroController.FixedUpdate += HeroControllerOnFixedUpdate;
        }

        private void HeroControllerOnFixedUpdate(ILContext il)
        {
	        var cursor = new ILCursor(il);

	        cursor.Goto(0);

	        cursor.GotoNext(MoveType.After,
		        i => i.MatchLdfld<float>("MAX_FALL_VELOCITY"),
		        i => i.MatchNeg());
	        // found the comparison of fall speed
	        if (!_mUpsideDown)
	        {
		        // no need to do anything
	        }
	        else
	        {
		        // remove the negative
		        //cursor.Next.OpCode = OpCodes.Blt_Un_S;
		        var operand = cursor.Instrs.ToArray()[cursor.Index].Operand;
		        cursor.Remove();
		        cursor.Emit(OpCodes.Blt_Un_S, operand);
	        }
	        cursor.GotoNext(MoveType.After,
		        i => i.MatchLdfld<float>("MAX_FALL_VELOCITY"));
	        // found the setting of fall speed
	        if (!_mUpsideDown)
	        {
		        // no need to do anything
	        }
	        else
	        {
		        // remove the negative
		        //cursor.Remove();
	        }
        }

        private IEnumerator Loop()
        {
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    _mUpsideDown = !_mUpsideDown;
                    Switch(_mUpsideDown);
                }
                yield return null;
            }
        }

        private void Switch(bool nowState)
        {
            //_mHc.SetAttr("BUMP_VELOCITY", _mHc.GetAttr<HeroController, float>("BUMP_VELOCITY") * -1);
            //_mHc.BOUNCE_VELOCITY *= -1;
            //_mHc.WALLSLIDE_DECEL *= -1;
            //_mHc.WALLSLIDE_SPEED *= -1;
            //_mHc.SWIM_ACCEL *= -1;
            //_mHc.SWIM_MAX_SPEED *= -1;
            _mHc.JUMP_SPEED_UNDERWATER *= -1;
            _mHc.JUMP_SPEED *= -1;
            //_mHc.SHROOM_BOUNCE_VELOCITY *= -1;
            //_mHc.RECOIL_DOWN_VELOCITY *= -1;
            _mHc.MAX_FALL_VELOCITY *= -1;
            _mHc.MAX_FALL_VELOCITY_UNDERWATER *= -1;
            _mRb2d.gravityScale *= -1;
            //Physics2D.gravity *= -1;
            Vector3 tmpVec3 = gameObject.transform.localScale;
            tmpVec3.y *= -1;
            gameObject.transform.localScale = tmpVec3;
        }

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
