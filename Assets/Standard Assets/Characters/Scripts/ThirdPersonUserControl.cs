//TO DO!!!!!
//to edit file so only one is needed by every thrid person controler for optimisation of inputs
//using pre calculated velocity makes it easier to store movement --less space used too
//move pickup here --controls currently in different place
//timing between phusics updates could cause issues - BIG ISSUES!!!
//pickup is still must definately fucked --cd/cr --pickup on replay very finecky

/** FIX FOR TIMING??? just an  idea which probably only works in theory
 * flatten - set input for next fixed update in update and wait till it happens until doing again
 * all inputs handled in update
 * all functionality handled in fixed update

 * Slow
 * flatten - runs update - waits for next fixed to apply inputs - runs update 
 * more fixed - every next fiex ignored

 * Fast
 * flatten - runs update - waits for next fixed to apply inputs - runs update
 * more updates - every update between ignored
**/

using UnityEngine;
using System;
using System.IO;
using StaticValuesNamespace;
using UnityEngine.SceneManagement;
using System.Collections;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        public int player;
        private ThirdPersonCharacter m_Character;
        public Transform m_Cam;
        private Vector3 m_CamForward;
        private Vector3 m_Move;
        private bool m_Jump, m_Crouch, m_hasFixedUpdateRun = true;
        private bool m_PickupAction;
        private bool m_InteractAction;
        private float m_h = 0, m_v = 0;
        Rigidbody m_Rigidbody;
        StreamWriter m_sw;
        StreamReader m_sr;

        private void Start()
        {
            if (StaticValues.IsReplay)
            {
                if (File.Exists(StaticValues.ReplayBaseDir + "/" + StaticValues.ReplayName + "/input" + this.player + ".txt"))
                {
                    m_sr = File.OpenText(StaticValues.ReplayBaseDir + "/" + StaticValues.ReplayName + "/input" + this.player + ".txt");
                }
                else {
                    SceneManager.LoadScene(0);
                }
            }
            else {
                if (!Directory.Exists(StaticValues.ReplayBaseDir))
                {
                    Directory.CreateDirectory(StaticValues.ReplayBaseDir);
                }
                if (!Directory.Exists(StaticValues.ReplayBaseDir + "/" + StaticValues.ReplayName))
                {
                    Directory.CreateDirectory(StaticValues.ReplayBaseDir + "/" + StaticValues.ReplayName);
                }
                if (File.Exists(StaticValues.ReplayBaseDir + "/" + StaticValues.ReplayName + "/input" + this.player + ".txt"))
                {
                    File.Delete(StaticValues.ReplayBaseDir + "/" + StaticValues.ReplayName + "/input" + this.player + ".txt");
                    m_sw = File.CreateText(StaticValues.ReplayBaseDir + "/" + StaticValues.ReplayName + "/input" + this.player + ".txt");
                }
                else
                {
                    m_sw = File.CreateText(StaticValues.ReplayBaseDir + "/" + StaticValues.ReplayName + "/input" + this.player + ".txt");
                }
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
        }


        private void Update()
        {
            if (!StaticValues.IsReplay) {
                if (m_hasFixedUpdateRun)
                {
                    m_Crouch = false;
                    m_PickupAction = false;
                    m_InteractAction = false;
                    m_Jump = false;
                    m_hasFixedUpdateRun = false;
                    m_h = 0;
                    m_v = 0;
                }

                m_Crouch = Input.GetButtonDown("P" + this.player + "_Crouch");
                m_PickupAction = Input.GetButtonDown("P" + this.player + "_Pickup");
                m_InteractAction = Input.GetButtonDown("P" + this.player + "_Interact");
                m_Jump = Input.GetButtonDown("P" + this.player + "_Jump");
                m_h = Input.GetAxis("P" + this.player + "_Horizontal");
                m_v = Input.GetAxis("P" + this.player + "_Vertical");

                if (m_Cam != null)
                {
                    // calculate camera relative direction to move:
                    m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                    m_Move = m_v * m_CamForward + m_h * m_Cam.right;
                }
                else
                {
                    // we use world-relative directions in the case of no main camera
                    m_Move = m_v * Vector3.forward + m_h * Vector3.right;
                }
            }
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            if (StaticValues.IsReplay)
            {
                var line = m_sr.ReadLine();
                if (line != null)
                {
                    String[] inputs = line.Split('|');
                    m_Move.x = float.Parse(inputs[1]);
                    m_Move.y = float.Parse(inputs[2]);
                    m_Move.z = float.Parse(inputs[3]);
                    m_Crouch = (inputs[4] == "True");
                    m_Jump = (inputs[5] == "True");
                    m_PickupAction = (inputs[6] == "True");
                    m_InteractAction = (inputs[7] == "True");
                }
                else
                {
                    StartCoroutine(WaitBeforeExit(5));
                }
            }
            else
            {
                m_sw.WriteLine(this.player + "|" + m_Move.x + "|" + m_Move.y + "|" + m_Move.z + "|" + m_Crouch + "|" + m_Jump + "|" + m_PickupAction + "|" + m_InteractAction);
            }
            m_Character.Move(m_Move, m_Crouch, m_Jump);
            m_hasFixedUpdateRun = true;
        }

        void OnDestroy()
        {
            if (StaticValues.IsReplay) {
                m_sr.Close();
            }
            else {
                m_sw.Close();
            }
        }

        public bool getPickupActionState() {
            return m_PickupAction;
        }

        public bool getInteractionActionState()
        {
            return m_InteractAction;
        }

        IEnumerator WaitBeforeExit(int seconds)
        {
            yield return new WaitForSeconds(seconds);
            SceneManager.LoadScene(0);
        }
    }
}
