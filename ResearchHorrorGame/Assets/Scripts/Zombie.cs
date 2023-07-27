using UnityEngine;

public class Zombie : MonoBehaviour
{
    public struct RagdollJoint
    {
        public Collider collider { get; private set; }
        public Rigidbody rigidbody { get; private set; }

        public bool enabled { get => m_enabled; set { m_enabled = value;  if(collider) collider.enabled = value; if(rigidbody) rigidbody.useGravity = value; } }
        private bool m_enabled;


        public RagdollJoint(GameObject g)
        {
            collider = g.GetComponent<Collider>();
            rigidbody = g.GetComponent<Rigidbody>();
            m_enabled = true;
        }

        public RagdollJoint(Collider c, Rigidbody r)
        {
            collider = c;
            rigidbody = r;
            m_enabled = false;

            enabled = false;
        }

        public static RagdollJoint[] GetRagdollJoints(Transform parent)
        {
            Rigidbody[] rigidbodies = parent.GetComponentsInChildren<Rigidbody>();
            RagdollJoint[] ragdollJoints = new RagdollJoint[rigidbodies.Length];

            for(int i = 0; i < rigidbodies.Length; i++)
                ragdollJoints[i] = new RagdollJoint(rigidbodies[i].GetComponent<Collider>(), rigidbodies[i]);

            return ragdollJoints;
        }
    }

    public Animator anim { get; private set; }
    public new Collider collider { get; private set; }
    public Color[] colors;
    public RagdollJoint[] joints;


    private void Awake()
    {
        anim = GetComponent<Animator>();
        collider = GetComponent<Collider>();

        joints = RagdollJoint.GetRagdollJoints(transform);

        //anim.SetFloat("speed", 2.25f + Random.Range(-0.05f, 0.05f));

        transform.localScale = transform.localScale.x * Random.Range(0.9f, 1.1f) * Vector3.one;

        Game.OnGameStart += () => { anim.Play("zombie run"); Debug.LogError("Have doors play the loud buzzing sound when opened. See (https://freesound.org/people/kyles/sounds/453724/)"); };

        /*switch(Random.Range(0, 2))
        {
            case 0:
                anim.Play("running crawl");
                break;

            case 1:
                anim.Play("zombie run");
                break;

            default:
                anim.Play("zombie run");
                break;
        }*/

        int i = Random.Range(0, colors.Length);

        GetComponentInChildren<Renderer>().material.color = colors[i];
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if(Player.player.isAttacking)
            {
                Ragdoll();
                GetComponentInChildren<Rigidbody>().AddForce(Player.player.transform.forward * 500f + Vector3.up * 250f);
            }
            else
            {
                Player.player.Stumble();
            }
        }
    }

    private void Ragdoll()
    {
        anim.enabled = false;
        collider.enabled = false;

        for(int i = 0; i < joints.Length; i++)
            joints[i].enabled = true;
    }
}
