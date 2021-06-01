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
        public void SetAxial()//���򵯻���
        {
            nodes[node1].f += (nodes[node1].position - nodes[node2].position).normalized * (-k_axial) * (GetLength() - init_length);
            nodes[node2].f += (nodes[node1].position - nodes[node2].position).normalized * (k_axial) * (GetLength() - init_length);
        }
        public void SetCrease() {//����������������������

        }
    }
    class Triangle
    {
        public int Edge1;
        public int Edge2;
        public int Edge3;
        public void SetFaceForce() { }//�����������������
    }


    public static float mass = 1.0f;
    public static float damping = 1.0f;

    static Mode mode = Mode.Verlet;
    static Node[] nodes;
    static Edge[] edges;
    static Triangle[] triangles;
    public static float k_axial = 50.0f;//��������
    public static float k_fold = 5.0f;
    public static float k_face = 5.0f;
    public static float target_angle = 90.0f;


    private void Init_Model()//��ʼ������ȡ���룬����model����Ҫ�ѽڵ�-��-�����εĶ�Ӧ��ϵ���ú�
    {
        nodes = new Node[4];
        edges = new Edge[5];
        triangles = new Triangle[3];
    }
    private void AxialConstraint()//��ÿ��edge����ĵ���
    {
        foreach(Edge edge in edges)
        {
            edge.SetAxial();
        }
    }
    private void CreaseContraint()//ÿ���ۺ۶��������������
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
    }//ÿ�������ε�����

    private void HandleInput() { }//��������
    private void UpdateModel()//����
    {
        foreach (Node node in nodes)
        {
            node.Update();
        }

    }
    private void RenderModel() { }//����

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
