using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Data;
using UnityEditor;
using Unity.Collections;

public class AI : MonoBehaviour
{
    public GameObject spawnerOfNN;
    public GameObject aim1;
    bool go = true;
    bool firstSteps = true;
    float firstStepsTime = 0f;
    float damage = 0f;
    
    public List<GameObject> companions = new List<GameObject>();
    public int companionIterator = 0;

    //[NonSerialized] 
    public int addition = 0;

    //NEAT inizialization
    public float speed = 0.01f;
    public int recursionAddLink = 0;
    public float health = 30;
    public float age = 300;

    public int prevNumber = 0;
    public int thisNumber = 0;
    public int outConnections = 4;
    public int initalNeurones = 43;

    public List<float> neurones = new List<float>();
    public List<int> inpInnov = new List<int>();
    public List<int> outInnov = new List<int>();
    public List<float> weights =  new List<float>();
    public List<bool> actConnect = new List<bool>();
    public List<bool> RNNs = new List<bool>();
    public List<float> RNNneurones;
    public List<int> order = new List<int>();
    public List<Dictionary<int, float>> adjList = new List<Dictionary<int, float>>();
    public List<int> innovations = new List<int>();

    public int testing = 0;

    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Adder();
        aim1 = GameObject.FindGameObjectWithTag("Finish");
        RNNneurones = new List<float>(neurones);
        health = UnityEngine.Random.Range(35, 40);
        age = 1000;
        Material AIMaterial = Resources.Load(spawnerOfNN.name, typeof(Material)) as Material;
        gameObject.GetComponent<Renderer>().material = AIMaterial;
        if(order[0] == 1 && order[1] == 0 && order[2] == 0){
            ++testing;
            EditorApplication.isPaused = true;
        }
    }
    void Update()
    {
        if(damage >= 5 && damage <= 15){
            damage += Time.deltaTime;
            if(damage >= 15){
                Material AIMaterial = Resources.Load(spawnerOfNN.name, typeof(Material)) as Material;
                gameObject.GetComponent<Renderer>().material = AIMaterial;
                damage = 0;
            }
        } 
        if(firstSteps == true){
            firstStepsTime += Time.deltaTime;
            if(firstStepsTime >= 1.5f){
                rb.linearVelocity = Vector3.zero;
                firstSteps = false;
                firstStepsTime = 0f;
            }
        }
        //get inputs
        if(!inpInnov.Any()){
            Destroy(gameObject);
        }
        age -= Time.deltaTime;
        health -= Time.deltaTime;
        if(age < 0 || health < 0){
            Destroy(gameObject);
        }
        if(go){
            neurones[0] = 1;
            neurones[outConnections + 1] = health;
            try{
                if(companionIterator < companions.Count){
                    neurones[outConnections + 2] = companions[companionIterator].GetComponent<AI>().neurones[3];
                    neurones[outConnections + 3] = Int32.Parse(companions[companionIterator].name);
                }
            }
            catch{
                companions.RemoveAt(companionIterator);
            }
            for(int i = outConnections + 4; i < initalNeurones - 1; i++){
                RaycastHit hit;
                Ray ray = new Ray(transform.position, transform.TransformDirection(new Vector3(-7 + i, 0, 13)));
                Physics.Raycast(ray, out hit, 160f);
                if(hit.collider != null){
                    if(hit.collider.gameObject.tag == "Player"){
                        neurones[i] = hit.distance;
                        ++i;
                        if(hit.collider.gameObject.GetComponent<AI>().spawnerOfNN == spawnerOfNN){
                            neurones[i] = Int32.Parse(hit.collider.gameObject.name.Substring(0, 3));
                        }
                        else{
                            neurones[i] = 2;
                        }
                    }
                    if(hit.collider.gameObject.tag == "Food"){
                        neurones[i] = hit.distance;
                        ++i;
                        neurones[i] = 3;
                    }
                    if(hit.collider.gameObject.tag == "Tree"){
                        neurones[i] = hit.distance;
                        ++i;
                        if(hit.collider.gameObject.GetComponent<TreeEnviroment>().isSeed == true){
                            neurones[i] = 4;
                        }
                        else{
                            neurones[i] = 5;
                        }
                    }
                    if(hit.collider.gameObject.tag == "Barrier"){
                        neurones[i] = hit.distance;
                        ++i;
                        neurones[i] = 6;
                    }
                }
                else{
                    ++i;
                }
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }

        }
        //Thinking
        for(int i = 0; i < order.Count; i++){
            int thisNeuron = order[i];
            if(thisNeuron != 0 && thisNeuron != 3 && (thisNeuron <= outConnections || thisNeuron > initalNeurones)){
                neurones[thisNeuron] = (float)System.Math.Tanh(neurones[thisNeuron]);
            }
            foreach(var b in adjList[thisNeuron]){
                neurones[b.Key] += b.Value * neurones[thisNeuron];
            }
        }
        if(neurones[1] < -0.5f)
        {
            gameObject.transform.Rotate(0, -1, 0);
        }
        else if(neurones[1] >= 0.5f)
        {
            gameObject.transform.Rotate(0, 1, 0);
        }
        else
        {
            gameObject.transform.Rotate(0, 0, 0);
        }
        if(neurones[2] >= 0.5){
            RaycastHit hit;
            Ray ray = new Ray(transform.position, transform.TransformDirection(transform.forward));
            Physics.Raycast(ray, out hit, 8f);
            if(hit.collider != null && hit.collider.gameObject.tag == "Player" && hit.collider.gameObject.GetComponent<AI>().spawnerOfNN != spawnerOfNN){
                //Debug.Log(gameObject.name + " " + hit.collider.gameObject.name);
                hit.collider.gameObject.GetComponent<AI>().health -= 5;
                hit.collider.gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * 10, ForceMode.Impulse);
                hit.collider.gameObject.GetComponent<AI>().Damaged();
            }
            Debug.DrawLine(ray.origin, hit.point, Color.yellow);
        }
        Math.Round(neurones[3], 2);
        if(neurones[4] >= 0.8f && companionIterator < companions.Count){
            ++companionIterator;
        }
        if(neurones[4] <= 0.2f && companionIterator > 0){
            --companionIterator;
        }
        for(int i = 0; i < inpInnov.Count; i++){
            if(actConnect[i] == true && RNNs[i] == true){
                RNNneurones[outInnov[i]] += neurones[inpInnov[i]] * weights[i];
            }
        }
        for(int i = 0; i < RNNneurones.Count; i++){
            neurones[i] = RNNneurones[i];
            RNNneurones[i] = 0;
        }
    }

    void FixedUpdate()
    {
        if(go){
            rb.MovePosition(rb.position + transform.forward * speed);
        }
    }
    private void OnCollisionEnter(Collision other){
        if(other.gameObject.tag == "Food"){
            health += 35;
            Destroy(other.gameObject);
        }
    }
    //Ability to talk
    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player" && other.gameObject.GetComponent<AI>().spawnerOfNN == spawnerOfNN){
            companions.Add(other.gameObject);
        }
    }
    void OnTriggerExit(Collider other){
        if(other.gameObject.tag == "Player" && other.gameObject.GetComponent<AI>().spawnerOfNN == spawnerOfNN){
            if(companions.IndexOf(other.gameObject) != -1){
                if(companions.IndexOf(other.gameObject) < companionIterator && companionIterator > 0){
                    --companionIterator;
                }
                companions.Remove(other.gameObject);
            }
        }
    }
    public void AddNode(){
        int ind = UnityEngine.Random.Range(0, outInnov.Count);
        int reccurency = 0;
        while(reccurency < 5){
            if(RNNs[ind] == false && actConnect[ind] == true){
                break;
            }
            ind = UnityEngine.Random.Range(0, outInnov.Count);
            ++reccurency;
        }
        if(reccurency >= 5){
            return;
        }
        neurones.Add(0);
        adjList.Add(new Dictionary<int, float>());

        actConnect[ind] = false;

        weights.Add(weights[ind]);
        inpInnov.Add(inpInnov[ind]);
        outInnov.Add(neurones.Count - 1);
        RNNs.Add(false);
        actConnect.Add(true);
        RNNneurones.Add(0);
        innovations.Add(spawnerOfNN.GetComponent<GameManager>().DealInnovations(inpInnov[inpInnov.Count - 1], outInnov[inpInnov.Count - 1], RNNs[inpInnov.Count - 1]));

        weights.Add(1f);
        inpInnov.Add(neurones.Count - 1);
        outInnov.Add(outInnov[ind]);
        RNNs.Add(false);
        actConnect.Add(true);
        innovations.Add(spawnerOfNN.GetComponent<GameManager>().DealInnovations(inpInnov[inpInnov.Count - 1], outInnov[inpInnov.Count - 1], RNNs[inpInnov.Count - 1]));
        makeOrder();
    }
    public void AddLink(){
        bool errorInOut = false;
        weights.Add(UnityEngine.Random.Range(-3f, 3f));
        inpInnov.Add(UnityEngine.Random.Range(0, neurones.Count));
        List<int> TakenConnections = new List<int>();
        int probableOut = UnityEngine.Random.Range(1, neurones.Count - initalNeurones + outConnections + 1);
        if(probableOut > outConnections){
            probableOut += initalNeurones - outConnections - 1;
        }
        for(int i = 0; i < outInnov.Count; i++){
            if(inpInnov[i] == inpInnov[inpInnov.Count - 1]){
                TakenConnections.Add(outInnov[i]);
            }
        }
        foreach(int a in TakenConnections){
            if(a == probableOut || probableOut == inpInnov.Last()){
                errorInOut = true;
                break;
            }
        }
        outInnov.Add(probableOut);
        RNNs.Add(false);
        actConnect.Add(true);
        if(errorInOut == true || correctGen(GenToPh())){
            //Debug.Log(inpInnov.Count - 1);
            inpInnov.RemoveAt(inpInnov.Count - 1);
            outInnov.RemoveAt(outInnov.Count - 1);
            RNNs.RemoveAt(RNNs.Count - 1);
            actConnect.RemoveAt(actConnect.Count - 1);
            weights.RemoveAt(weights.Count - 1);
            makeOrder();
            ++recursionAddLink;
            if(recursionAddLink < 3){
                AddLink();
                recursionAddLink = 0;
            }
        }
        else{
            if(UnityEngine.Random.Range(0, 6) >= 5){
                if (RNNs.Count > 0){
                    RNNs[RNNs.Count - 1] = true;
                    innovations.Add(spawnerOfNN.GetComponent<GameManager>().DealInnovations(inpInnov[inpInnov.Count - 1], outInnov[inpInnov.Count - 1], RNNs[inpInnov.Count - 1]));
                }
            }
            else{
                innovations.Add(spawnerOfNN.GetComponent<GameManager>().DealInnovations(inpInnov[inpInnov.Count - 1], outInnov[inpInnov.Count - 1], RNNs[inpInnov.Count - 1]));
            }
        }
        makeOrder();
    }
    //Transforming genotype to phenotype and convenient format
    public List<int> GenToPh(){
        List<float> _weights = new List<float>(weights);
        List<int> _inpInnov = new List<int>(inpInnov);
        List<int> _outInnov = new List<int>(outInnov);
        List<bool> _actConnect = new List<bool>(actConnect);
        List<bool> _RNNs = new List<bool>(RNNs);
        List<int> nullConn = new List<int>();
        List<int> inDegree = new List<int>();
        List<int> order1 = new List<int>();
        int neuronesCount = neurones.Count;
        for(int i = 0; i < _actConnect.Count; i++){
            if(_actConnect[i] == false || _RNNs[i] == true){
                _actConnect.RemoveAt(i);
                _weights.RemoveAt(i);
                _inpInnov.RemoveAt(i);
                _outInnov.RemoveAt(i);
                _RNNs.RemoveAt(i);
                --i;
            }
        }
        //Creating dictionary of connections
        adjList.Clear();
        for(int i = 0; i < neurones.Count; i++){
           adjList.Add(new Dictionary<int, float>());
        }
        for(int i = 0; i < _inpInnov.Count; i++){
            adjList[_inpInnov[i]].Add(_outInnov[i], _weights[i]);
        }

        for(int i = 0; i < adjList.Count; i++){
            inDegree.Add(0);
        }
        for(int i = 0; i < neurones.Count; i++){
            foreach(var b in adjList[i]){
                ++inDegree[b.Key];
            }
        }
        for(int i = 0; i < inDegree.Count; i++){
            if(inDegree[i] == 0){
                nullConn.Add(i);
                /*
                if(adjList[i].Any()){
                    nullConn.Add(i);
                }
                else{
                    --neuronesCount;
                }*/
            }
        }
        for(int i = 0; i != nullConn.Count; ++i){
            int ie = nullConn[i];
            order1.Add(ie);
            foreach(var b in adjList[ie]){
                int a = b.Key;
                --inDegree[b.Key];
                if(inDegree[a] == 0){
                    nullConn.Add(b.Key);
                }
            }
        }
        //--neuronesCount;
        if(order1.Count != neuronesCount){
            Debug.Log("Not equal " + order1.Count + "  " + neuronesCount + " " + gameObject.name);
            return new List<int>{1, 0, 0};
        }
        if(!order1.Any()){
            Debug.Log("Empty");
            return new List<int> {1, 0, 0, 0};;
        }
        return order1;
    }
    public void makeOrder(){
        List<int> a = GenToPh();
        order = a;
    }
    public void Adder(){
        adjList.Clear();
        for(int i = 0; i < neurones.Count; i++){
            adjList.Add(new Dictionary<int, float>());
        }
        if(addition == 1){
            AddLink();
        }
        if(addition == 2){
            AddNode();
        }
        if(addition == 3){
            AddLink();
            AddLink();
            AddLink();
            AddNode();
            AddLink();
            AddLink();
            AddNode();
            AddNode();
            AddLink();
            AddLink();
            AddLink();
            AddLink();
            AddNode();
            AddLink();
            AddLink();
            AddNode();
            AddNode();
            AddLink();
            AddNode();
            AddNode();
            AddLink();
            AddLink();
            AddLink();
            AddLink();
            AddNode();
        }
        makeOrder();
        //addition = 0;
    }

    public bool correctGen(List<int> a){
        List<int> genes = new(a);
        bool yes = true;
        if(genes[0] == 0 || genes[0] > outConnections){
            genes.RemoveRange(0, genes.Count - 3);
            for(int i = 1; i != outConnections + 1; ++i){
                if(genes.Contains(i)){
                    yes = false;
                    break;
                }
            }
        }
        return yes;
    }

    public void Damaged(){
        Material BloodMaterial = Resources.Load("Blood", typeof(Material)) as Material;
        gameObject.GetComponent<Renderer>().material = BloodMaterial;
        //damage = 5.1f;
    }
}
