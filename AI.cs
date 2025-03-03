using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Data;
using UnityEditor;
using System.Reflection.Emit;

public class AI : MonoBehaviour
{
    public GameObject gm;
    public GameObject aim1;
    bool go = true;

    //[NonSerialized] 
    public int addition = 0;

    //NEAT inizialization
    public float speed = 0.01f;
    public int recursionAddLink = 0;

    public List<float> neurones = new List<float> {1, 0, 0, 0, 0};
    public List<int> inpInnov = new List<int>();
    public List<int> outInnov = new List<int>();
    public List<float> weights =  new List<float>();
    public List<bool> actConnect = new List<bool>();
    public List<bool> RNNs = new List<bool>();
    public List<float> RNNneurones = new List<float> {1, 0, 0, 0, 0};
    public List<int> order = new List<int>();
    public List<Dictionary<int, float>> adjList = new List<Dictionary<int, float>>();
    public List<int> innovations = new List<int>();

    public int layer = 0;

    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        layer = 0;
        //addition = 2;
        Adder();
        aim1 = GameObject.FindGameObjectWithTag("Finish");
    }
    void Update()
    {
        //get inputs
        if(!inpInnov.Any()){
            Destroy(gameObject);
        }
        if(order[0] == 4){
            gameObject.transform.position = new Vector3(0, 0, -40);
        }
        if(go){
            //neurones[4] = 0;
            neurones[0] = 1;
            rb.MovePosition(rb.position + transform.forward * speed);
            RaycastHit hit;
            Ray ray = new Ray(transform.position, new Vector3(-3, 0, 10));
            //Ray ray = new Ray(transform.position, aim1.transform.position);
            Physics.Raycast(ray, out hit);
            if(hit.collider != null){
                if(hit.collider.gameObject.tag == "Finish"){
                    neurones[1] = hit.distance;
                }
            }
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            ray = new Ray(transform.position, new Vector3(0, 0, 1));
            Physics.Raycast(ray, out hit);
            if(hit.collider != null){
                if(hit.collider.gameObject.tag == "Finish"){
                    neurones[2] = hit.distance;
                }
            }
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            ray = new Ray(transform.position, new Vector3(3, 0, 10));
            Physics.Raycast(ray, out hit);
            if(hit.collider != null){
                if(hit.collider.gameObject.tag == "Finish"){
                    neurones[3] = hit.distance;
                }
            }
            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
        //Thinking
        string str = "";
        for(int i = 0; i < order.Count; i++){
            int thisNeuron = order[i];
            if(thisNeuron > 4){
                neurones[thisNeuron] = (float)System.Math.Tanh(neurones[thisNeuron]);
            }
            try{
                foreach(var b in adjList[thisNeuron]){
                    neurones[b.Key] += b.Value * neurones[thisNeuron];
                }
            }
            catch{
                Debug.Log(neurones.Count + "  " + adjList.Count);
            }
        }
        foreach(float a in neurones){
            str += a + " ";
        }
        /*if(Conn == 1){
            Debug.Log(str);
            Debug.Log(abc1);
            Debug.Log(abc);
            Debug.Log(abc2);
            Debug.Log(neurones[1] + " "+ neurones[2] + " "+ neurones[4]);
        }*/
        if(neurones[4] < -0.5f)
        {
            gameObject.transform.Rotate(0, -1, 0);
        }
        if(neurones[4] >= 0.5f)
        {
            gameObject.transform.Rotate(0, 1, 0);
        }
        else
        {
            gameObject.transform.Rotate(0, 0, 0);
        }
        //neurones[4] = 0;
        for(int i = 0; i < order.Count; i++){
            neurones[i] = RNNneurones[i];
        }
    }
    private void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Finish"){
            go = false;
        }
        if(other.gameObject.tag == "EditorOnly"){
            ++layer;
        }
    }
    public void AddNode(){
        int ind = UnityEngine.Random.Range(0, outInnov.Count);
        while(true){
            if(RNNs[ind] == false){
                break;
            }
            ind = UnityEngine.Random.Range(0, outInnov.Count);
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
        innovations.Add(gm.GetComponent<GameManager>().DealInnovations(inpInnov[inpInnov.Count - 1], outInnov[inpInnov.Count - 1], RNNs[inpInnov.Count - 1]));

        weights.Add(1f);
        inpInnov.Add(neurones.Count - 1);
        outInnov.Add(outInnov[ind]);
        RNNs.Add(false);
        actConnect.Add(true);
        innovations.Add(gm.GetComponent<GameManager>().DealInnovations(inpInnov[inpInnov.Count - 1], outInnov[inpInnov.Count - 1], RNNs[inpInnov.Count - 1]));
    }
    public void AddLink(){
        int reccurency = 0;
        weights.Add(UnityEngine.Random.Range(-3f, 3f));
        //UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        inpInnov.Add(UnityEngine.Random.Range(1, neurones.Count));
        int probableOut = UnityEngine.Random.Range(4, neurones.Count);
        List<int> TakenConnections = new List<int>();
        for(int i = 0; i < outInnov.Count; i++){
            if(inpInnov[i] == inpInnov[inpInnov.Count - 1]){
                TakenConnections.Add(outInnov[i]);
            }
        }
        while(true){
            foreach(var a in TakenConnections){
                if(a == probableOut || probableOut == inpInnov[inpInnov.Count - 1]){
                    ++reccurency;
                    break;
                }
            }
            
            probableOut = UnityEngine.Random.Range(4, neurones.Count);
            if(reccurency > 9){
                reccurency = 1;
                break;
            }
            else{
                break;
            }
        }
        outInnov.Add(probableOut);
        RNNs.Add(false);
        actConnect.Add(true);
        if(GenToPh()[0] == 4 || reccurency == 1 || GenToPh().Last() != 4){
            //UnityEngine.Random.Range(0, 10) < 3
            //transform.position = new Vector3(transform.position.x, transform.position.y, -1500);
            //Debug.Log(RNNs.Count + " RNNs");
            if(UnityEngine.Random.Range(0, 6) == -1 && reccurency == 0){
                if (RNNs.Count > 0){
                    RNNs[this.RNNs.Count - 1] = true;
                }
            }
            else{
                //Debug.Log(inpInnov.Count - 1);
                inpInnov.RemoveAt(inpInnov.Count - 1);
                outInnov.RemoveAt(outInnov.Count - 1);
                RNNs.RemoveAt(RNNs.Count - 1);
                actConnect.RemoveAt(actConnect.Count - 1);
                weights.RemoveAt(weights.Count - 1);
                //Destroy(gameObject);
                makeOrder();
                recursionAddLink++;
                if(recursionAddLink < 7){
                    AddLink();
                    recursionAddLink = 0;
                }
            }
#pragma warning restore CS0164 // This label has not been referenced
        }
        else{
            innovations.Add(gm.GetComponent<GameManager>().DealInnovations(inpInnov[inpInnov.Count - 1], outInnov[inpInnov.Count - 1], RNNs[inpInnov.Count - 1]));
        }
        //Debug.Log("4 " + weights[inpInnov.Count - 1]);
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
        for(int i = 0; i < _actConnect.Count; i++){
            if(_actConnect[i] == false){
                _actConnect.RemoveAt(i);
                _weights.RemoveAt(i);
                _inpInnov.RemoveAt(i);
                _outInnov.RemoveAt(i);
                _RNNs.RemoveAt(i);
                --i;
            }
        }
        for(int i = 0; i < _RNNs.Count; i++){
            if(_RNNs[i] == true){
                _actConnect.RemoveAt(i);
                _weights.RemoveAt(i);
                _inpInnov.RemoveAt(i);
                _outInnov.RemoveAt(i);
                _RNNs.RemoveAt(i);
                --i;
            }
        }
        adjList.Clear();
        for(int i = 0; i < neurones.Count; i++){
           adjList.Add(new Dictionary<int, float>());
        }
        var abc = true;
        try{
            for(int i = 0; i < _inpInnov.Count; i++){
                abc = _actConnect[i];
                adjList[_inpInnov[i]].Add(_outInnov[i], _weights[i]);
            }
        }
        catch{
            //Debug.Log(adjList.Count + "  " + abc);
            order1 = new List<int>{4, 1};
            return order1;
        }

        for(int i = 0; i < adjList.Count; i++){
            inDegree.Add(0);
        }
        for(int i = 0; i < neurones.Count; i++){
            foreach(var b in adjList[i]){
                try{
                    ++inDegree[b.Key];
                }
                catch{
                    Debug.Log(neurones.Count + "  " + inDegree.Count + "   " + b.Key);
                }
            }
        }
        for(int i = 0; i < inDegree.Count; i++){
            if(inDegree[i] == 0){
                nullConn.Add(i);
            }
        }
        string str1 = "";
        for(int i = 0; i < nullConn.Count; i++){
            int ie = nullConn[i];
            order1.Add(ie);
            foreach(var b in adjList[ie]){
                int a = b.Key;
                --inDegree[b.Key];
                if(inDegree[a] == 0){
                    nullConn.Add(b.Key);
                }
                str1 += inDegree[a];
                str1 += " ";
            }
        }
        //Debug.Log(str1);
        if(order1.Count != neurones.Count){
            //Debug.Log("Smaller: " + order1.Count + " " + neurones.Count);
            order1 = new List<int>{4, 0};
            return order1;
        }
        else{
            //Debug.Log("normal");
        }
        if(!order1.Any()){
            List<int> a = new List<int>
            {
                -1
            };
            Debug.Log("Empty");
            return a;
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
        /*adjList = new List<Dictionary<int, float>>
        {
            new Dictionary<int, float>(),
            new Dictionary<int, float>(),
            new Dictionary<int, float>(),
            new Dictionary<int, float>(),
            new Dictionary<int, float>()
        };*/
        if(addition == 1){
            AddLink();
        }
        if(addition == 2){
            AddNode();
        }
        makeOrder();
        addition = 0;
    }

    public float AverageWeight(){
        float AvWeight = 0;
        foreach(float a in weights){
            AvWeight += a;
        }
        AvWeight /= weights.Count;
        return AvWeight;
    }
}
