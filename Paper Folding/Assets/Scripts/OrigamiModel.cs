using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrigamiModel : MonoBehaviour
{
    public Camera cam;
    public Material material;

    public bool back = true;

    public static float mass = 1.0f;
    public static float damping = 0.45f;

    static Mode mode = Mode.Verlet;
    static Node[] nodes;
    static Edge[] edges;
    static Triangle[] triangles;
    public static float k_axial = 20.0f;//��������
    public static float k_fold = 0.7f;
    public static float k_face = 0.2f;
    public static float target_angle = 45.0f;

    Mesh mesh;

    enum Mode
    {
        Verlet,Euler
    }
    enum EdgeType
    {
        Mountain,Valley,Face,Boundary
    }
    class Node
    {
        public Vector3 prev_pos;
        public Vector3 position;
        public Vector3 f = Vector3.zero;
        public Vector3 a = Vector3.zero;
        public Vector3 v = Vector3.zero;
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
            else if (mode == Mode.Euler)
            {
                a = f / mass;
                position += v * Time.deltaTime;
                v += a * Time.deltaTime;
            }
        }

    }
    class Edge
    {
        public int node1;//index of node
        public int node2;
        public int Triangle1 = -1;
        public int Triangle2 = -1;
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
                k_crease = init_length * k_face;
                _target = 0;
            }
            else
            {
                k_crease = init_length * k_fold;
                if (type == EdgeType.Mountain) _target = 180-target_angle;
                else _target = target_angle;
            }
            int p3 = node1;
            int p4 = node2;
            int p1, p2;

            if (triangles[Triangle1].Node1 == p3) p1 = triangles[Triangle1].Node3;
            else if (triangles[Triangle1].Node2 == p3) p1 = triangles[Triangle1].Node1;
            else p1 = triangles[Triangle1].Node2;
            if (triangles[Triangle2].Node1 == p3) p2 = triangles[Triangle2].Node2;
            else if (triangles[Triangle2].Node2 == p3) p2 = triangles[Triangle1].Node3;
            else p2 = triangles[Triangle1].Node1;

            float h1 = Utils.DistanceFromPoint2Line(nodes[p1].position, nodes[p3].position, nodes[p4].position);
            float h2 = Utils.DistanceFromPoint2Line(nodes[p2].position, nodes[p3].position, nodes[p4].position);
            Vector3 factor1 = (-k_crease) * (theta - _target) * norm1 / h1;
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
    {
        public int Node1;
        public int Node2;
        public int Node3;
        public float init_angle12;
        public float init_angle23;
        public float init_angle13;
        public void SetFaceForce() { }//�����������������
        public Vector3 GetNorm()
        {
            Vector3 e1 = nodes[Node2].position - nodes[Node1].position;
            Vector3 e2 = nodes[Node3].position - nodes[Node1].position;
            return Vector3.Cross(e1, e2).normalized;
        }
    }


    private void Init_Model()//��ʼ������ȡ���룬����model����Ҫ�ѽڵ�-��-�����εĶ�Ӧ��ϵ���ú�
    {
        nodes = new Node[5];
        edges = new Edge[7];
        triangles = new Triangle[3];

        for (int i = 0; i < 5; ++i)
            nodes[i] = new Node();

        for (int i = 0; i < 7; ++i)
            edges[i] = new Edge();

        for (int i = 0; i < 3; ++i)
            triangles[i] = new Triangle();
        
        nodes[0].position = new Vector3(0, 1, 0);
        nodes[1].position = new Vector3(0, 0, 0);
        nodes[2].position = new Vector3(1, 0, 0);
        nodes[3].position = new Vector3(1, 1, 0);
        nodes[4].position = new Vector3(1, 2, 0);

        edges[0].node1 = 0;
        edges[0].node2 = 1;
        edges[1].node1 = 0;
        edges[1].node2 = 2;
        edges[2].node1 = 0;
        edges[2].node2 = 3;
        edges[3].node1 = 1;
        edges[3].node2 = 2;
        edges[4].node1 = 2;
        edges[4].node2 = 3;
        edges[5].node1 = 0;
        edges[5].node2 = 4;
        edges[6].node1 = 3;
        edges[6].node2 = 4;

        edges[0].Triangle2 = 0;
        edges[1].Triangle1 = 0;
        edges[1].Triangle2 = 1;
        edges[2].Triangle1 = 1;
        edges[2].Triangle2 = 2;
        edges[3].Triangle2 = 0;
        edges[4].Triangle2 = 1;
        edges[5].Triangle1 = 2;
        edges[6].Triangle2 = 2;

        edges[0].type = EdgeType.Boundary;
        edges[1].type = EdgeType.Valley;
        edges[2].type = EdgeType.Mountain;
        edges[3].type = EdgeType.Boundary;
        edges[4].type = EdgeType.Boundary;
        edges[5].type = EdgeType.Boundary;
        edges[6].type = EdgeType.Boundary;

        for (int i = 0; i < 7; i++)
            edges[i].init_length = edges[i].GetLength();

        triangles[0].Node1 = 0;
        triangles[0].Node2 = 2;
        triangles[0].Node3 = 1;
        triangles[1].Node1 = 0;
        triangles[1].Node2 = 3;
        triangles[1].Node3 = 2;
        triangles[2].Node1 = 0;
        triangles[2].Node2 = 4;
        triangles[2].Node3 = 3;

        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();

        for (int i = 0; i < nodes.Length; ++i)
            vertices.Add(nodes[i].position);
        
        for (int i = 0; i < triangles.Length; ++i)
        {
            int node1 = triangles[i].Node1, node2 = triangles[i].Node2, node3 = triangles[i].Node3;            
            if (!back)
            {
                tris.Add(node1);
                tris.Add(node2);
                tris.Add(node3);
            }
            else
            {
                tris.Add(node3);
                tris.Add(node2);
                tris.Add(node1);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
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
    private void RenderModel() 
    {
        List<Vector3> vertices = new List<Vector3>();

        for (int i = 0; i < nodes.Length; ++i)
        {
            vertices.Add(nodes[i].position);
            // Debug.Log(vertices[i]);
        }

        mesh.vertices = vertices.ToArray();
    }//����

    void Start()
    {
        mesh = new Mesh();
        Init_Model();
        
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.GetComponent<MeshRenderer>().material = material;
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }


    void FixedUpdate()
    {
        //HandleInput();
        AxialConstraint();
        CreaseContraint();
        //FaceConstraint();
        UpdateModel();
        RenderModel();
        target_angle = PlayerPrefs.GetInt("FoldPercent", 90) * 180 / 100;
    }
}
