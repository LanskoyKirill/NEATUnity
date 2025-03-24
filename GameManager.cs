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
    public float checking = 11f;
    public int factorN = 0;
    //[NonSerialized] 
    public List<int> oInn = new List<int>();
    public List<int> iInn = new List<int>();
    public List<bool> RNN = new List<bool>();
    public List<List<GameObject>> classAIs = new List<List<GameObject>>();
    public List<float> numberClassAIs = new List<float>();

    public int population = 100;
    public int selection = 6;

    public int epoch = 0;
    void Start()
    {
        //creating population
        for(int i = 0; i < population; i++){
            float b = transform.position.x * Random.Range(0.9f, 1.1f);
            GameObject a = Instantiate(nn, new Vector3(b, transform.position.y, transform.position.z), Quaternion.identity);
            a.GetComponent<AI>().gm = gameObject;
            a.GetComponent<AI>().addition = 1;
            AIs.Add(a);
        }
    }

    // Update is called once per frame
    void Update()
    {
        stopwatch += Time.deltaTime;
        if(stopwatch >= checking){
            AIs.Clear();
            AIs.AddRange(GameObject.FindGameObjectsWithTag("Player"));
            classAIs.Clear();
            stopwatch = 0.1f;
            List<GameObject> copyAI = new List<GameObject>();
            foreach(GameObject a in AIs){
                copyAI.Add(a);
                if(factorN < a.GetComponent<AI>().neurones.Count){
                    factorN = a.GetComponent<AI>().neurones.Count;
                }
            }
            List<GameObject> SortedList = new List<GameObject>(AIs.OrderByDescending(o=>o.GetComponent<AI>().layer + o.transform.position.z * 0.001).ToList());
            //List<GameObject> SortedList = new List<GameObject>(AIs.OrderByDescending(o=>o.transform.position.z).ToList());
            AIs.Clear();
            List<GameObject> NewAIs = new List<GameObject>(SortedList.GetRange(0, selection));
            //Debug.Log(SortedList[0].GetComponent<AI>().layer);
            //Speciation
            classAIs.Add(new List<GameObject>());
            classAIs[0].Add(SortedList[0]);
            for(int k = 0; k < SortedList.Count; k++){
                GameObject thisNN = SortedList[k];
                thisNN.GetComponent<AI>().innovations.Sort();
                float fit = 0;
                float disjoint = 0;
                float excess = 0;
                for(int i = 0; i < classAIs.Count; i++){
                    GameObject c = classAIs[i][0];
                    c.GetComponent<AI>().innovations.Sort();
                    int countInnov = c.GetComponent<AI>().innovations.Count - 1;
                    for(int j = 0; j < thisNN.GetComponent<AI>().innovations.Count; j++){
                        if(j > countInnov){
                            excess += countInnov + 1;
                            break;
                        }
                        else{
                            if(c.GetComponent<AI>().innovations.IndexOf(thisNN.GetComponent<AI>().innovations[j]) == -1){
                                ++disjoint;
                            }
                        }
                    }
                    fit = disjoint / factorN + excess / factorN + thisNN.GetComponent<AI>().AverageWeight();
                    if(1 - fit < 0.2){
                        try{
                            classAIs[i].Add(SortedList[k]);
                        }
                        catch{
                            Debug.Log(classAIs[i]);
                        }
                        break;
                    }
                    /*else{
                        classAIs.Add(new List<GameObject>());
                        classAIs[classAIs.Count - 1].Add(SortedList[k]);
                    }*/
                }
            }

            foreach(var a in NewAIs){
                float c = transform.position.x * Random.Range(0.9f, 1.1f);
                var b = Instantiate(a, new Vector3(c, transform.position.y, transform.position.z), Quaternion.identity);
                b.name = "nn";
            }
            
            //for(int i = 0; i < population; i++)
            {
                for(int ii = 0; ii < population - selection; ii++){
                    int FirstInd = Random.Range(0, classAIs.Count);
                    GameObject offspring = classAIs[FirstInd][Random.Range(0, classAIs[FirstInd].Count)];
                    GameObject c = classAIs[FirstInd][Random.Range(0, classAIs[FirstInd].Count)];

                    //Random inheritance of weights
                    if(offspring.GetComponent<AI>().innovations.Any()){
                        if(c.GetComponent<AI>().innovations.Any()){
                            for(int iii = 0; iii < offspring.GetComponent<AI>().innovations.Count; iii++){
                                if(Random.Range(0, 2) == 0){
                                        int tryFind = c.GetComponent<AI>().innovations.IndexOf(offspring.GetComponent<AI>().innovations[iii]);
                                        if(tryFind != -1){
                                            try{
                                                offspring.GetComponent<AI>().weights[iii] = c.GetComponent<AI>().weights[tryFind];
                                            }
                                            catch{
                                                Debug.Log(offspring.GetComponent<AI>().weights.Count + "  " + c.GetComponent<AI>().weights.Count + "  " + tryFind);
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
                            for(int i = 0; i < offspring.GetComponent<AI>().actConnect.Count; i++){
                                if(offspring.GetComponent<AI>().actConnect[i] == false){
                                    check.Add(i);
                                }
                            }
                            if(check.Any()){
                                offspring.GetComponent<AI>().actConnect[check[Random.Range(0, check.Count)]] = true;
                            }
                        }
                    }
                    for(int ie = 0; ie < offspring.GetComponent<AI>().weights.Count; ie++){
                        if(Random.Range(0, 5) < 4){
                            offspring.GetComponent<AI>().weights[ie] += Random.Range(-0.5f, 0.5f);
                        }
                    }
                    NewAIs.Add(offspring);
                    /*GameObject a = (GameObject) Instantiate(NewAIs[ii], new Vector3(b, transform.position.y, transform.position.z), Quaternion.identity);
                    a.GetComponent<AI>().Adder();*/
                    float b = transform.position.x * Random.Range(0.9f, 1.1f);
                    //float b = transform.position.x;
                    var a = Instantiate(NewAIs[ii], new Vector3(b, transform.position.y, transform.position.z), Quaternion.identity);
                    a.name = "nn";
                }
            }
            Debug.Log(copyAI.Count);
            foreach(var a in copyAI){
                Destroy(a);
            }
            ++epoch;
            //checking += 0.08f;
        }
    }
    public int DealInnovations(int inoV, int outV, bool rnnV){
        {
            for(int i = 0; i < iInn.Count; i++){
                /*if(iInn[i] == inoV){
                    if(oInn[i] == outV){
                        if(RNN[i] == rnnV){
                            return i;
                        }
                    } 
                }*/
                if (iInn[i] == inoV && oInn[i] == outV && RNN[i] == rnnV){
                    return i;
                }
            }
        }
        iInn.Add(inoV);
        oInn.Add(outV);
        RNN.Add(rnnV);
        return iInn.Count - 1;
    }
}
