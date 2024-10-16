using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Audioman;

public class MenuMinionController : MonoBehaviour
{

    public List<NavMeshAgent> minions;
    public List<GameObject> destinations;
    public GameObject prefab;
    public int numberOfMinions = 100;
    public EventSystem eventSystem;
   // private int current_destination = 0;
    // Start is called before the first frame update
    private GameObject current_destination;


    public GameObject main_button_group;
    public GameObject main_default_select;
    public GameObject settings_button_group;
    public GameObject settings_default_select;

    public AudioMixer mixer;
    private float volume = 1;
    public float volumeChangeFactor = 0.2f;
    Audioman.LoopHolder loop_holder;
    public Audioman audio_man;

    [Header("GameOver")]
    public float FadeOutTime;
    public float PostFadeOutTime;
    public Image FadeOut;


    public void Awake()
    {
        for(int i = 0; i < numberOfMinions; i++)
        {
            GameObject o = GameObject.Instantiate(prefab, new Vector3(0,0.5f,0), Quaternion.identity, this.transform);
            //o.GetComponent<NavMeshAgent>().obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            minions.Add(o.GetComponent<NavMeshAgent>());
        }

        audio_man = GameObject.FindAnyObjectByType<Audioman>();

    }


     public void startGame()
    {
        // TODO: fade out

        // TODO: load new scene
        scatter();
        Debug.Log("Start_game");
        getLoopHolder()?.Stop();

        IEnumerator Coroutine()
        {
            float t = 0.0f;
            while(t < FadeOutTime)
            {
                t += Time.unscaledDeltaTime;
                Color color = FadeOut.color;
                color.a = t / FadeOutTime;
                FadeOut.color = color;
                yield return null;
            }
            Color c = FadeOut.color;
            c.a = 1.0f;
            FadeOut.color = c;

            yield return new WaitForSecondsRealtime(PostFadeOutTime);
            SceneManager.LoadScene(1);
        }
        StartCoroutine(Coroutine());
    }


    public void quitGame()
    {
        Debug.Log("quits");
        Application.Quit();
        scatter();
    }

    public void settings()
    {
        Debug.Log("Settings");

        scatter();
        settings_button_group.SetActive(true);
        main_button_group.SetActive(false);

        StartCoroutine(callAfterSec(0.5f, () => eventSystem.SetSelectedGameObject(settings_default_select)));

       
    }
    
    public void increaseVolume()
    {
        volume = Mathf.Clamp((float)volume + volumeChangeFactor, 0.01f, 1);
        mixer.SetFloat("master", Mathf.Log10(volume) * 20);
        Debug.Log("Increase volume");

    }

    public void decreaseVolume()
    {
        volume = Mathf.Clamp((float)volume - volumeChangeFactor, 0.01f, 1);
        mixer.SetFloat("master", Mathf.Log10(volume) * 20);
        Debug.Log("Decrease volume");
    }


    public void backToMain() {
        Debug.Log("Bk");
        scatter();
        settings_button_group.SetActive(false);
        main_button_group.SetActive(true);

        StartCoroutine(callAfterSec(1, () => eventSystem.SetSelectedGameObject(main_default_select)));

    }

    IEnumerator callAfterSec(float secs, Action action)
    {
        yield return new WaitForSeconds(secs);
        action.Invoke();
    }

    void Start()
    {
        getLoopHolder();
    }

    public LoopHolder getLoopHolder() {
        if (loop_holder == null)
        {
            loop_holder = Audioman.getInstance()?.PlayLoop(Resources.Load<AudioLoopConfiguration>("object/creature_step_loop"), this.transform.position, true);

        }
        return loop_holder;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit = default;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("UI")))
        {
            GameObject pos_des = destinations.Find(o => o.transform == hit.collider.transform);
            if (pos_des != null)
            {
                eventSystem.SetSelectedGameObject(pos_des);
                if (Input.GetMouseButtonDown(0))
                {
                    ExecuteEvents.Execute(pos_des.gameObject, null, ExecuteEvents.submitHandler);
                   // Debug.Log(pos_des.name);
                }
                
               // setMinionDestination(pos_des);

            }

        }
        var total_velocity_magnitude = 0f;
        foreach (var item in minions)
        {
            total_velocity_magnitude += item.velocity.magnitude;
        }
        var avr_velocity = total_velocity_magnitude / minions.Count;
        // Debug.Log(avr_velocity / 10f);

        getLoopHolder()?.setVolume((avr_velocity / 10f));

    }

    private void scatter()
    {
        if (current_destination == null)
        {
            return;
        }
        current_destination = null;
        foreach (var item in minions)
        {
            item.destination = RandomNavmeshLocation(20);
        }
    }

    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }



    public void setMinionDestination(GameObject destination)
    {

        if (current_destination == destination)
        {
            return;
        }
        current_destination = destination;
        foreach (var item in minions)
        {
            Vector3 result;
            MeshCollider collider = destination.GetComponent<MeshCollider>();
            float margin = 0.0f;
            int limit = 100;
            do
            {

                result = new Vector3(
                        UnityEngine.Random.Range(collider.bounds.min.x + margin, collider.bounds.max.x - margin),
                        collider.bounds.center.y,
                        UnityEngine.Random.Range(collider.bounds.min.z + margin, collider.bounds.max.z - margin)
                    );
                limit = --limit;
            } while (!IsInsideMeshCollider(collider, result) && limit >= 0);
          //  Debug.Log(limit);
            if(IsInsideMeshCollider(collider, result))
            {
                item.destination = result;
            } else
            {
                item.destination = RandomNavmeshLocation(20);
            }
        }
    }



   

    public static bool IsInside(MeshCollider c, Vector3 point)
    {

        var closest = c.ClosestPoint(point);
    // Because closest=point if point is inside - not clear from docs I feel
        Debug.Log(closest == point);
        return closest == point;
    }

    bool IsInsideMeshCollider(MeshCollider col, Vector3 point)
    {
        var temp = Physics.queriesHitBackfaces;
        Ray ray = new Ray(point, Vector3.back);

        bool hitFrontFace = false;
        RaycastHit hit = default;

        Physics.queriesHitBackfaces = true;
        bool hitFrontOrBackFace = col.Raycast(ray, out RaycastHit hit2, 100f);
        if (hitFrontOrBackFace)
        {
            Physics.queriesHitBackfaces = false;
            hitFrontFace = col.Raycast(ray, out hit, 100f);
        }
        Physics.queriesHitBackfaces = temp;

        if (!hitFrontOrBackFace)
        {
            return false;
        }
        else if (!hitFrontFace)
        {
            return true;
        }
        else
        {
            // This can happen when, for instance, the point is inside the torso but there's a part of the mesh (like the tail) that can still be hit on the front
            if (hit.distance > hit2.distance)
            {
                return true;
            }
            else
                return false;
        }

    }
}
