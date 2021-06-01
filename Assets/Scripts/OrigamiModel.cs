using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrigamiModel : MonoBehaviour
{
    enum Mode
    {
        Verlet,Euler
    }
    enum EdgeType
    {
        Mountain,Valley,Face
    }
    class Node
    {
        public Vector3 prev_pos;
        public Vector3 position;
        public Vector3 f;
        public Vector3 a;
        public Vector3 v;
        public void Update()
        {
            if (mode == Mode.Verlet)
            {
                a = f / mass;
                Vector3 new_pos = position + damping * (position - prev_pos) + a * Time.deltaTime * Time.deltaTime;
                prev_pos = position;
                position = new_pos;
                f = Vector3.zero;
            }
        }

    }
    class Edge
    {
        public int node1;//index of node
        public int node2;
        public int Triangle1;
        public int Triangle2;
        public EdgeType type;
        public float init_length;
        public float GetLength()
        {
            return (nodes[node1].position - nodes[node2].position).magnitude;
        }
        public void SetAxial()//轴向弹簧力
        {
            nodes[node1].f += (nodes[node1].position - nodes[node2].position).normalized * (-k_axial) * (GetLength() - init_length);
            nodes[node2].f += (nodes[node1].position - nodes[node2].position).normalized * (k_axial) * (GetLength() - init_length);
        }
        public void SetCrease() {//对相邻两个面的力，待完成

        }
    }
    class Triangle
    {
        public int Edge1;
        public int Edge2;
        public int Edge3;
        public void SetFaceForce() { }//三角形内力，待完成
    }


    public static float mass = 1.0f;
    public static float damping = 1.0f;

    static Mode mode = Mode.Verlet;
    static Node[] nodes;
    static Edge[] edges;
    static Triangle[] triangles;
    public static float k_axial = 50.0f;//弹力常量
    public static float k_fold = 5.0f;
    public static float k_face = 5.0f;
    public static float target_angle = 90.0f;


    private void Init_Model()//初始化，读取输入，生成model，需要把节点-边-三角形的对应关系设置好
    {
        nodes = new Node[4];
        edges = new Edge[5];
        triangles = new Triangle[3];
    }
    private void AxialConstraint()//沿每个edge方向的弹力
    {
        foreach(Edge edge in edges)
        {
            edge.SetAxial();
        }
    }
    private void CreaseContraint()//每个折痕对相邻两个面的力
    {
        foreach(Edge edge in edges)
        {
            edge.SetCrease();
        }
    }
    private void FaceConstraint() {
        foreach(Triangle t in triangles)
        {
            t.SetFaceForce();
        }
    }//每个三角形的内力

    private void HandleInput() { }//处理输入
    private void UpdateModel()//更新
    {
        foreach (Node node in nodes)
        {
            node.Update();
        }

    }
    private void RenderModel() { }//绘制

    void Start()
    {
        Init_Model();
    }


    void FixedUpdate()
    {
        HandleInput();
        AxialConstraint();
        CreaseContraint();
        FaceConstraint();
        UpdateModel();
        RenderModel();
    }
}
