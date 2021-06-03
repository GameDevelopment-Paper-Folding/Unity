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
        public int Triangle1=-1;//�����ߣ���node1��node2������������1��˳ʱ��
        public int Triangle2=-1;//��ʱ��
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
        public void SetCrease() {//���������������
            if (Triangle1 == -1 || Triangle2 == -1)
            {
                return;
            }
            Vector3 norm1 = triangles[Triangle1].GetNorm();
            Vector3 norm2 = triangles[Triangle2].GetNorm();
            float theta = Vector3.Angle(norm1, norm2);
            float k_crease;
            float _target;
            if (type == EdgeType.Face)
            {
                k_crease = init_length * k_facet;
                _target = 0;
            }
            else {
                k_crease = init_length * k_fold;
                if (type == EdgeType.Mountain) _target = -target_angle;
                else _target = target_angle;
                  }
            int p3 = node1;
            int p4 = node2;
            int p1,p2;
            if (triangles[Triangle1].Node1 == p3) p1 = triangles[Triangle1].Node3;
            else if (triangles[Triangle1].Node2 == p3)p1 = triangles[Triangle1].Node1;
            else p1= triangles[Triangle1].Node2;
            if (triangles[Triangle2].Node1 == p3) p2 = triangles[Triangle2].Node2;
            else if (triangles[Triangle2].Node2 == p3) p2 = triangles[Triangle1].Node3;
            else p2 = triangles[Triangle1].Node1;

            float h1 = Utils.DistanceFromPoint2Line(nodes[p1].position, nodes[p3].position, nodes[p4].position);
            float h2 = Utils.DistanceFromPoint2Line(nodes[p2].position, nodes[p3].position, nodes[p4].position);
            Vector3 factor1= (-k_crease) * (theta - _target) * norm1 / h1;
            Vector3 factor2 = (-k_crease) * (theta - _target) * norm2 / h2;
            nodes[p1].f += factor1;
            nodes[p2].f += factor2;
            nodes[p3].f += factor1 * (-Utils.CoTangentFormPoint2Line(nodes[p4].position, nodes[p3].position, nodes[p1].position)) /
                (Utils.CoTangentFormPoint2Line(nodes[p3].position, nodes[p1].position, nodes[p4].position)
                + Utils.CoTangentFormPoint2Line(nodes[p4].position, nodes[p3].position, nodes[p1].position))
                + factor2 * (-Utils.CoTangentFormPoint2Line(nodes[p4].position, nodes[p2].position, nodes[p3].position)) /
                (Utils.CoTangentFormPoint2Line(nodes[p3].position, nodes[p4].position, nodes[p2].position)
                + Utils.CoTangentFormPoint2Line(nodes[p4].position, nodes[p2].position, nodes[p3].position));

            nodes[p4].f += factor1 * (-Utils.CoTangentFormPoint2Line(nodes[p3].position, nodes[p1].position, nodes[p4].position)) /
               (Utils.CoTangentFormPoint2Line(nodes[p3].position, nodes[p1].position, nodes[p4].position)
               + Utils.CoTangentFormPoint2Line(nodes[p4].position, nodes[p3].position, nodes[p1].position))
               + factor2 * (-Utils.CoTangentFormPoint2Line(nodes[p3].position, nodes[p4].position, nodes[p2].position)) /
               (Utils.CoTangentFormPoint2Line(nodes[p3].position, nodes[p4].position, nodes[p2].position)
               + Utils.CoTangentFormPoint2Line(nodes[p4].position, nodes[p2].position, nodes[p3].position));
        }
    }
    class Triangle
    {//��ʼ��ʱ��֤Node1->2->3Ϊ˳ʱ��
        public int Node1;
        public int Node2;
        public int Node3;
        public float init_angle12;
        public float init_angle23;
        public float init_angle13;
        public void SetFaceForce() {
            SetAngleForce(nodes[Node2], nodes[Node1], nodes[Node3], init_angle13);
            SetAngleForce(nodes[Node1], nodes[Node3], nodes[Node2], init_angle23);
            SetAngleForce(nodes[Node3], nodes[Node2], nodes[Node1], init_angle12);
        }
        public Vector3 GetNorm()
        {
            Vector3 e1 = nodes[Node2].position - nodes[Node1].position;
            Vector3 e2 = nodes[Node3].position - nodes[Node1].position;
            return Vector3.Cross(e1, e2).normalized;
        }
        public float GetAngle(Node n,Node n1,Node n2)//nΪ����
        {
            return Vector3.Angle(n1.position - n.position, n2.position - n.position);
        }
        private void SetAngleForce(Node n,Node n1,Node n2,float init_angle)
        {
            float alpha = Vector3.Angle(n1.position - n.position, n2.position - n.position);
            Vector3 factor1 = Vector3.Cross(GetNorm(), n1.position - n.position)/ (n1.position - n.position).sqrMagnitude;
            Vector3 factor2 = Vector3.Cross(GetNorm(), n2.position - n.position) / (n2.position - n.position).sqrMagnitude;
            n1.f += (-k_face) * (alpha - init_angle) * factor1;
            n2.f += (-k_face) * (alpha - init_angle) * factor1;
            n.f+= (-k_face) * (alpha - init_angle) * (factor2-factor1);

        }
    }


    public static float mass = 1.0f;
    public static float damping = 1.0f;

    static Mode mode = Mode.Verlet;
    static Node[] nodes;
    static Edge[] edges;
    static Triangle[] triangles;
    public static float k_axial = 50.0f;//��������
    public static float k_fold = 5.0f;
    public static float k_facet = 5.0f;//�ۺ�Լ���еĳ���
    public static float k_face = 5.0f;//��Լ���еĳ���
    public static float target_angle = 90.0f;//�ǶȾ���degreeΪ��λ

    /*
    ��ʼ��
    ��Ҫ��ɵĵ㣺
    1.Ϊnode��edge��triangle����ռ�
    2.��дedge���������εĶ�Ӧ��ϵ��edge�ķ���Ϊ˳ʱ���������ΪTriangle1,edge�ķ���Ϊ��ʱ���������ΪTriangle2����Ե�ı߽�ȱʧ����������Ϊ-1
    3.��дtriangle�еĽڵ㣬��֤node1->node2->node3Ϊ˳ʱ��
    4.����edge��type���ʼ����init_length��������node���ú����edge.Getlength()��ȡ
    5.�����������еĳ�ʼ�Ƕ�init_angle������ʹ��triangle.getAngle()��ȡ�Ƕ�
    */
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
