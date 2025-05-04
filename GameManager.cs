using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Linq;
using System;
using System.Data;
using UnityEditor;
using Random = UnityEngine.Random;
public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject nn;
    public List<GameObject> AIs = new List<GameObject>();
    public float stopwatch = 0f;
    public float checking = 34f;
    public int factorN = 0;
    //[NonSerialized] 
    public List<int> oInn = new List<int>();
    public List<int> iInn = new List<int>();
    public List<bool> RNN = new List<bool>();
    public List<List<GameObject>> classAIs = new List<List<GameObject>>();
    public List<float> numberClassAIs = new List<float>();

    public int population = 30;
    public int selection = 15;

    public int epoch = 0;
    public float rotationY = 0f;
    public Vector3 VectorDistance;
    void Start()
    {
        List<int> names = new List<int>();
        for(int i = 0; i < 100; i++){
            int name = Random.Range(100, 501);
            while(names.IndexOf(name) != -1){
                name = Random.Range(100, 501);
            }
            names.Add(name);
        }
        Time.timeScale = 3;
        VectorDistance = gameObject.transform.position;
        //creating population
        int initalNeuronesGameManager = 44;
        for(int i = 0; i < population; i++){
            //float b = transform.position.x * Random.Range(0.9f, 1.1f);
            GameObject a = Instantiate(nn, VectorDistance, Quaternion.identity);
            //Add neurones
            a.GetComponent<AI>().neurones.Clear();
            a.GetComponent<AI>().neurones.Add(1);
            a.GetComponent<AI>().initalNeurones = initalNeuronesGameManager;
            for(int j = 0; j < initalNeuronesGameManager - 1; j++){
                a.GetComponent<AI>().neurones.Add(0);
            }
            VectorDistance.x += 1.5f;
            if(VectorDistance.x >= gameObject.transform.position.x + 12f){
                VectorDistance.x = gameObject.transform.position.x;
                VectorDistance.z += 3;
                VectorDistance.x += 1;
            }
            a.transform.Rotate(0f, rotationY, 0f);
            a.GetComponent<AI>().spawnerOfNN = gameObject;
            a.GetComponent<AI>().addition = 1;
            a.name = names[i].ToString();
            AIs.Add(a);
        }
        VectorDistance = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        stopwatch += Time.deltaTime;
        if(stopwatch >= checking){
                selection = 0;
                AIs.Clear();
                foreach(var a in GameObject.FindGameObjectsWithTag("Player")){
                    if(a.GetComponent<AI>().spawnerOfNN.name == gameObject.name){
                        AIs.Add(a);
                        ++selection;
                    }
                }
                List<int> names = new List<int>();
                for(int i = 0; i < 100; i++){
                    int name = Random.Range(100, 201);
                    while(names.IndexOf(name) != -1){
                        name = Random.Range(100, 501);
                    }
                    names.Add(name);
                }
                //selection = population - selection;
                classAIs.Clear();
                stopwatch = 0.1f;
                List<GameObject> copyAI = new List<GameObject>();
                foreach(GameObject a in AIs){
                    copyAI.Add(a);
                    if(factorN < a.GetComponent<AI>().inpInnov.Count){
                        factorN = a.GetComponent<AI>().inpInnov.Count;
                    }
                }
                List<GameObject> SortedList = new List<GameObject>();
                {
                    int i = 0;
                    List<GameObject> b = AIs.OrderByDescending(o => o.GetComponent<AI>().health).ToList();
                    foreach(var a in b){
                        if(i >= 8){
                            break;
                        }
                        ++i;
                        SortedList.Add(a);
                    }
                }
                AIs.Clear();
                List<GameObject> NewAIs = new List<GameObject>();
                //Speciation
                classAIs.Add(new List<GameObject>());
                classAIs[0].Add(SortedList[0]);
                for(int k = 0; k < SortedList.Count; k++){
                    GameObject thisNN = SortedList[k];
                    List<int> thisNNInnvoations = new(thisNN.GetComponent<AI>().innovations);
                    thisNNInnvoations.Sort();
                    float fit = 0;
                    float disjoint = 0;
                    float excess = 0;
                    float differenceWeight = 0;
                    int differenceWeightLength = 0;
                    for(int i = 0; i < classAIs.Count; i++){
                        GameObject c = classAIs[i][0];
                        List<int> cInnvoations = new(c.GetComponent<AI>().innovations);
                        cInnvoations.Sort();
                        int countInnov = cInnvoations.Count - 1;
                        for(int j = 0; j < thisNNInnvoations.Count; j++){
                            if(j > countInnov){
                                excess += countInnov + 1;
                                break;
                            }
                            else{
                                if(cInnvoations.IndexOf(thisNNInnvoations[j]) == -1){
                                    ++disjoint;
                                }
                                else{
                                    differenceWeight += Mathf.Abs(thisNNInnvoations[j] - cInnvoations[cInnvoations.IndexOf(thisNNInnvoations[j])]);
                                    ++differenceWeightLength;
                                }
                            }
                        }
                        fit = (disjoint + excess)/factorN + differenceWeight/differenceWeightLength;
                        if(fit > 0.8){
                            classAIs[i].Add(SortedList[k]);
                            break;
                        }
                        else if(i == (classAIs.Count - 1)){
                            classAIs.Add(new List<GameObject>());
                            classAIs[classAIs.Count - 1].Add(SortedList[k]);
                            break;
                        }
                    }
                }
                
                //for(int i = 0; i < population; i++) 
                for(int i = 0; i < (population - selection); i++){
                    int FirstInd = Random.Range(0, classAIs.Count);
                    GameObject offspring = Instantiate(classAIs[FirstInd][Random.Range(0, classAIs[FirstInd].Count)], VectorDistance, Quaternion.identity);
                    GameObject c = classAIs[FirstInd][Random.Range(0, classAIs[FirstInd].Count)];
                    //Random inheritance of weights
                    if(offspring.GetComponent<AI>().innovations.Any()){
                        if(c.GetComponent<AI>().innovations.Any()){
                            for(int ii = 0; ii < offspring.GetComponent<AI>().innovations.Count; ii++){
                                if(Random.Range(0, 2) == 0){
                                        int tryFind = c.GetComponent<AI>().innovations.IndexOf(offspring.GetComponent<AI>().innovations[ii]);
                                        if(tryFind != -1){
                                            {
                                                offspring.GetComponent<AI>().weights[ii] = c.GetComponent<AI>().weights[tryFind];
                                            }
                                        }
                                }
                            }
                        }
                    }
                    if(Random.Range(0, 5) == 0){
                        offspring.GetComponent<AI>().addition = 1;
                    }
                    else if(Random.Range(0, 5) == 0){
                        offspring.GetComponent<AI>().addition = 2;
                    }
                    else{
                        offspring.GetComponent<AI>().addition = 0;
                    }
                    //Reactivate and deactivate
                    if(offspring.GetComponent<AI>().actConnect.Any()){
                        if(Random.Range(0, 5) < 1){
                            offspring.GetComponent<AI>().actConnect[Random.Range(0, offspring.GetComponent<AI>().actConnect.Count - 1)] = false;
                            //offspring.GetComponent<AI>().actConnect.RemoveAt(Random.Range(0, offspring.GetComponent<AI>().actConnect.Count - 1));
                        }
                        else if(Random.Range(0, 5) < 1){
                            List<int> check = new List<int>();
                            for(int ii = 0; ii < offspring.GetComponent<AI>().actConnect.Count; ii++){
                                if(offspring.GetComponent<AI>().actConnect[ii] == false){
                                    check.Add(ii);
                                }
                            }
                            if(check.Any()){
                                int reactivate = Random.Range(0, check.Count);
                                offspring.GetComponent<AI>().actConnect[check[reactivate]] = true;
                                if(offspring.GetComponent<AI>().correctGen(offspring.GetComponent<AI>().GenToPh()) == true){
                                    offspring.GetComponent<AI>().RNNs[check[reactivate]] = true;
                                }
                            }
                        }
                    }
                    for(int ie = 0; ie < offspring.GetComponent<AI>().weights.Count; ie++){
                        if(Random.Range(0, 5) < 4){
                            offspring.GetComponent<AI>().weights[ie] += Random.Range(-0.5f, 0.5f);
                        }
                    }
                    NewAIs.Add(offspring);
                    /*GameObject a = (GameObject) Instantiate(NewAIs[i], new Vector3(b, transform.position.y, transform.position.z), Quaternion.identity);
                    a.GetComponent<AI>().Adder();*/
                    //float b = transform.position.x * Random.Range(0.9f, 1.1f);
                    //float b = transform.position.x; 
                    VectorDistance.x += 1.5f;
                    if(VectorDistance.x >= gameObject.transform.position.x + 12f && VectorDistance.z <= 6 + gameObject.transform.position.z){
                        VectorDistance.x = gameObject.transform.position.x;
                        VectorDistance.z += 4f;
                        VectorDistance.x += 1.5f;
                    }
                    else if(VectorDistance.x >= gameObject.transform.position.x + 12f){
                        VectorDistance.x = gameObject.transform.position.x;
                        VectorDistance.z -= 8f;
                        VectorDistance.x -= 3f;
                    }
                    offspring.transform.Rotate(0f, rotationY, 0f);
                    offspring.name = names[i].ToString();
                    offspring.GetComponent<AI>().spawnerOfNN = gameObject;
                }
                ++epoch;
        }
    }
    public int DealInnovations(int inoV, int outV, bool rnnV){
    {
        for(int i = 0; i < iInn.Count; i++){
            if (iInn[i] == inoV)
                if(oInn[i] == outV){ 
                    if(RNN[i] == rnnV){
                        return i;
                    }
                }
            }
        }
        iInn.Add(inoV);
        oInn.Add(outV);
        RNN.Add(rnnV);
        return iInn.Count - 1;
    }
}


