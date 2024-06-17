using Game;
using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

namespace GameFramework.Network.Movement
{
    public class NetworkMovementComponent:NetworkBehaviour
    {
        private Vector3 rVec;
        private Vector3 fVec;
        [SerializeField] private float maxSpeed = 3f;
        [SerializeField] private float maxRotate = 5f;
        public float originalMaxSpeed = 3f;

        private int _tick = 0;
        private float _tickRate = 1f / 60f;
        private float _tickDeltaTime = 0f;

        private const int BUFFER_SIZE = 1024;
        private InputState[] _inputStates=new InputState[BUFFER_SIZE];
        private TransformState[] _transformStates=new TransformState[BUFFER_SIZE];

        public NetworkVariable<TransformState> ServerTransformState= new NetworkVariable<TransformState>();
        public TransformState _previousTransformState;

        [SerializeField]private Animator PinkAnimator;
        [SerializeField] private Animator BlueAnimator;
        private Animator animator;
        private PlayerPickUpDrop playerPickUpDrop;

        private void Start()
        {
            if(GameLobbyManager.Instance.IsHost)
            {
                animator = PinkAnimator;
                BlueAnimator.gameObject.SetActive(false);
                Debug.Log("Player spawn as Host");
            }
            else
            {
                animator = BlueAnimator;
                PinkAnimator.gameObject.SetActive(false);
                BlueAnimator.gameObject.GetComponent<PlayerPickUpDrop>().enabled = false;
                Debug.Log("Player spawn as Client");
            }

            rVec = Camera.main.transform.right;
            Vector3 tempV = Camera.main.transform.forward;
            tempV.y = 0;
            tempV.Normalize();
            fVec = tempV;

            //animator = GetComponent<Animator>();
            playerPickUpDrop = GetComponent<PlayerPickUpDrop>();
        }
        private void OnEnable()
        {
            ServerTransformState.OnValueChanged += OnServerStateChanged;
        }

        private void OnServerStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            _previousTransformState= previousvalue;
        }

        public void ProcessLocalPlayerMovement(float transAmt,float rotAmt)
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                int bufferIndex=_tick % BUFFER_SIZE;

                if (!IsServer)
                {
                    MovePlayerServerRPC(_tick, transAmt, rotAmt);
                    MoveAndRotate(transAmt, rotAmt);
                }
                else
                {
                    MoveAndRotate(transAmt, rotAmt);

                    TransformState state = new TransformState()
                    {
                        Tick = _tick,
                        Position = transform.position,
                        Rotation = transform.rotation,
                        HasStartedMoving = true
                    };

                    _previousTransformState=ServerTransformState.Value;
                    ServerTransformState.Value = state;
                }

                InputState inputState = new InputState()
                {
                    Tick = _tick,
                    TransAmt = transAmt,
                    RotAmt = rotAmt
                };

                TransformState transformState = new TransformState()
                {
                    Tick = _tick,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    HasStartedMoving = true
                };

                _inputStates[bufferIndex] = inputState;
                _transformStates[bufferIndex] = transformState;

                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }

        public void ProcessSimulatedPlayerMovement()
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                if (ServerTransformState.Value.HasStartedMoving)
                {
                    transform.position = ServerTransformState.Value.Position;
                    transform.rotation = ServerTransformState.Value.Rotation;
                }
                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }
        private void MoveAndRotate(float transAmt, float rotAmt)
        {
            Vector3 dir = (rVec * rotAmt) + (fVec * transAmt);

            transform.forward = Vector3.Slerp(transform.forward, dir, maxRotate * Time.deltaTime);

            float moveDist = dir.magnitude;
            Vector3 moveAmt = transform.forward * moveDist * maxSpeed;

            transform.position += moveAmt * _tickRate;

            
            // Call WalkAnimation based on user input
            if (playerPickUpDrop.objectGrabbable == null)
            {
                animator.gameObject.GetComponent<PlayerAnimation>().WalkAnimation(transAmt, rotAmt);
            }

            if (playerPickUpDrop.objectGrabbable != null)
            {
                animator.gameObject.GetComponent<PlayerAnimation>().PickUpRunAnimation(transAmt, rotAmt);
            }
            
        }

        [ServerRpc]
        private void MovePlayerServerRPC(int tick, float transAmt, float rotAmt)
        {
            MoveAndRotate(transAmt, rotAmt);

            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = transform.position,
                Rotation = transform.rotation,
                HasStartedMoving = true
            };
            _previousTransformState=ServerTransformState.Value;
            ServerTransformState.Value = state;
        }

        //==========
        private void OnTriggerEnter(Collider other)
        {
            //when touch water, Player speed slow
            if (other.CompareTag("Water"))
            {
                maxSpeed = maxSpeed / 2f;
                animator.SetBool("isDead", true);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                //back to normal speed
                maxSpeed = originalMaxSpeed;
                animator.SetBool("isDead", false);
            }

            // Ignore collision with LittleBall and BigBall for 3 seconds
            if (other.CompareTag("LittleBall") || other.CompareTag("BigBall"))
            {
                StartCoroutine(IgnoreCollisionWithPlayer(other, 3f));
            }
        }
        private IEnumerator IgnoreCollisionWithPlayer(Collider other, float duration)
        {
            Collider playerCollider = GetComponent<Collider>();
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, other, true);
                yield return new WaitForSeconds(duration);
                Physics.IgnoreCollision(playerCollider, other, false);
            }
        }
        //==========
    }
}
