//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using MathDebbuger;
//using CustomMath;
//public class Tester : MonoBehaviour
//{
//    void Start()
//    {
//        List<Vector3> vectors = new List<Vector3>();
//        vectors.Add(new Vec3(10.0f, 0.0f, 0.0f));
//        vectors.Add(new Vec3(10.0f, 10.0f, 0.0f));
//        vectors.Add(new Vec3(20.0f, 10.0f, 0.0f));
//        vectors.Add(new Vec3(20.0f, 20.0f, 0.0f));
//        Vector3Debugger.AddVectorsSecuence(vectors, false, Color.red, "secuencia");
//        Vector3Debugger.EnableEditorView("secuencia");
//        Vector3Debugger.AddVector(new Vector3(10, 10, 0), Color.blue, "elAzul");
//        Vector3Debugger.EnableEditorView("elAzul");
//        Vector3Debugger.AddVector(Vector3.down * 7, Color.green, "elVerde");
//        Vector3Debugger.EnableEditorView("elVerde");
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.L))
//        {
//        }

//        if (Input.GetKeyDown(KeyCode.O))
//        {
//            Vector3Debugger.TurnOffVector("elAzul");
//        }
//        if (Input.GetKeyDown(KeyCode.P))
//        {
//            Vector3Debugger.TurnOnVector("elAzul");
//        }
//    }

//    IEnumerator UpdateVector()
//    {
//        for (int i = 0; i < 100; i++)
//        {
//            //Vector3Debugger.UpdatePosition("elAzul", new Vector3(2.4f, 6.3f, 0.5f) * (i * 0.05f));
//            yield return new WaitForSeconds(0.2f);
//        }
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathDebbuger;
using CustomMath;

public class Tester : MonoBehaviour
{
    [Header("Testing Parameters")]
    public float tolerance = 0.0001f;

    [Header("Visual Debugging (Vectors)")]
    public Vector3 visualA = new Vector3(1, 2, 3);
    public Vector3 visualB = new Vector3(4, 5, 6);

    void Start()
    {
        RunAllTests();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Vec3 cross = Vec3.Cross(new Vec3(visualA.x, visualA.y, visualA.z), new Vec3(visualB.x, visualB.y, visualB.z));
            Vector3Debugger.AddVector(visualA, Color.white, "VisualA");
            Vector3Debugger.AddVector(visualB, Color.gray, "VisualB");
            Vector3Debugger.AddVector(cross, Color.cyan, "VisualCross");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearConsole();
            RunAllTests();
        }
    }

    private void ClearConsole()
    {
#if UNITY_EDITOR
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method?.Invoke(new object(), null);
#endif
    }

    private void RunAllTests()
    {
        Debug.Log("<color=yellow>--- INICIANDO TESTS DINÁMICOS: VEC3 vs VECTOR3 ---</color>");

        Vector3 uA = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        Vector3 uB = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        Vector3 uC = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));

        float scalar = Random.Range(-5f, 5f);
        float t = Random.Range(-2f, 2f);

        Vector3 norm = Random.onUnitSphere;

        Debug.Log($"<color=cyan><b>[Valores Aleatorios Generados para el Test]</b></color>\n" +
                  $"<b>Vector A:</b> {uA:F4}\n" +
                  $"<b>Vector B:</b> {uB:F4}\n" +
                  $"<b>Vector C:</b> {uC:F4}\n" +
                  $"<b>Escalar:</b> {scalar:F4}\n" +
                  $"<b>T (Interpolación):</b> {t:F4}\n" +
                  $"<b>Normal plana:</b> {norm:F4}");

        Vec3 vA = new Vec3(uA);
        Vec3 vB = new Vec3(uB);
        Vec3 vC = new Vec3(uC);
        Vec3 vNorm = new Vec3(norm);

        int failedTests = 0;

        if (!TestAddition(uA, uB, vA, vB)) failedTests++;
        if (!TestSubtraction(uA, uB, vA, vB)) failedTests++;
        if (!TestUnaryMinus(uA, vA)) failedTests++;
        if (!TestMultiplication(uA, vA, scalar)) failedTests++;
        if (!TestDivision(uA, vA, scalar)) failedTests++;
        if (!TestDotProduct(uA, uB, vA, vB)) failedTests++;
        if (!TestCrossProduct(uA, uB, vA, vB)) failedTests++;
        if (!TestMagnitude(uA, vA)) failedTests++;
        if (!TestDistance(uA, uB, vA, vB)) failedTests++;
        if (!TestLerp(uA, uB, vA, vB, t)) failedTests++;
        if (!TestNormalization(uA, vA)) failedTests++;
        if (!TestAngle(uA, uB, vA, vB)) failedTests++;
        if (!TestClampMagnitude(uA, vA, Mathf.Abs(scalar))) failedTests++;
        if (!TestMinMax(uA, uB, vA, vB)) failedTests++;
        if (!TestProject(uA, norm, vA, vNorm)) failedTests++;
        if (!TestReflect(uA, norm, vA, vNorm)) failedTests++;

        Debug.Log("<color=orange>--- INICIANDO TESTS DINÁMICOS: MYPLANE vs PLANE ---</color>");

        if (!TestPlaneConstructorPointNormal(norm, uA, vNorm, vA)) failedTests++;
        if (!TestPlaneConstructor3Points(uA, uB, uC, vA, vB, vC)) failedTests++;
        if (!TestPlaneGetSideAndDistance(norm, uA, uB, uC, vNorm, vA, vB, vC)) failedTests+=2;
        if (!TestPlaneClosestPoint(norm, uA, uB, vNorm, vA, vB)) failedTests++;
        if (!TestPlaneFlip(norm, uA, vNorm, vA)) failedTests++;
        if (!TestPlaneTranslate(norm, uA, uB, vNorm, vA, vB)) failedTests++;

        if (failedTests > 0) Debug.Log($"<color=red>--- NO TODOS LOS TEST PASARON {failedTests} ERRORES---</color>");
        else Debug.Log("<color=green>--- TODOS LOS TESTS FINALIZADOS ---</color>");

    }

    bool TestAddition(Vector3 uA, Vector3 uB, Vec3 vA, Vec3 vB)
    {
        return AssertVectorMatch(uA + uB, vA + vB, "Suma (+)");
    }

    bool TestSubtraction(Vector3 uA, Vector3 uB, Vec3 vA, Vec3 vB)
    {
        return AssertVectorMatch(uA - uB, vA - vB, "Resta (-)");
    }

    bool TestUnaryMinus(Vector3 uA, Vec3 vA)
    {
        return AssertVectorMatch(-uA, -vA, "Negación Unaria (-Vec)");
    }

    bool TestMultiplication(Vector3 uA, Vec3 vA, float scalar)
    {
        bool result = true;
        result &= AssertVectorMatch(uA * scalar, vA * scalar, "Multiplicación por Escalar (*)");
        result &= AssertVectorMatch(scalar * uA, scalar * vA, "Multiplicación Escalar inverso (*)");
        return result;
    }

    bool TestDivision(Vector3 uA, Vec3 vA, float scalar)
    {
        return AssertVectorMatch(uA / scalar, vA / scalar, "División por Escalar (/)");
    }

    bool TestDotProduct(Vector3 uA, Vector3 uB, Vec3 vA, Vec3 vB)
    {
        return AssertFloatMatch(Vector3.Dot(uA, uB), Vec3.Dot(vA, vB), "Producto Punto (Dot)");
    }

    bool TestCrossProduct(Vector3 uA, Vector3 uB, Vec3 vA, Vec3 vB)
    {
        bool result = true;
        result &= AssertVectorMatch(Vector3.Cross(Vector3.right, Vector3.up), Vec3.Cross(Vec3.Right, Vec3.Up), "Producto Cruz (Ejes estáticos)");
        result &= AssertVectorMatch(Vector3.Cross(uA, uB), Vec3.Cross(vA, vB), "Producto Cruz (Aleatorio)");
        return result;
    }

    bool TestMagnitude(Vector3 uA, Vec3 vA)
    {
        bool result = true;
        result &= AssertFloatMatch(uA.magnitude, vA.magnitude, "Magnitud");
        result &= AssertFloatMatch(uA.sqrMagnitude, vA.sqrMagnitude, "Magnitud al Cuadrado");
        return result;
    }

    bool TestDistance(Vector3 uA, Vector3 uB, Vec3 vA, Vec3 vB)
    {
        return AssertFloatMatch(Vector3.Distance(uA, uB), Vec3.Distance(vA, vB), "Distancia");
    }

    bool TestLerp(Vector3 uA, Vector3 uB, Vec3 vA, Vec3 vB, float t)
    {
        bool result = true;
        result &= AssertVectorMatch(Vector3.Lerp(uA, uB, 0.5f), Vec3.Lerp(vA, vB, 0.5f), "Lerp (t = 0.5)");
        result &= AssertVectorMatch(Vector3.Lerp(uA, uB, t), Vec3.Lerp(vA, vB, t), "Lerp (t Aleatorio Clamped)");
        result &= AssertVectorMatch(Vector3.LerpUnclamped(uA, uB, t), Vec3.LerpUnclamped(vA, vB, t), "LerpUnclamped (t Aleatorio)");
        return result;
    }

    bool TestNormalization(Vector3 uA, Vec3 vA)
    {
        return AssertVectorMatch(uA.normalized, vA.normalized, "Vector Normalizado (normalized)");
    }

    bool TestAngle(Vector3 uA, Vector3 uB, Vec3 vA, Vec3 vB)
    {
        return AssertFloatMatch(Vector3.Angle(uA, uB), Vec3.Angle(vA, vB), "Ángulo (Angle)");
    }

    bool TestClampMagnitude(Vector3 uA, Vec3 vA, float maxLength)
    {
        return AssertVectorMatch(Vector3.ClampMagnitude(uA, maxLength), Vec3.ClampMagnitude(vA, maxLength), "ClampMagnitude");
    }

    bool TestMinMax(Vector3 uA, Vector3 uB, Vec3 vA, Vec3 vB)
    {
        bool result = true;
        result &= AssertVectorMatch(Vector3.Min(uA, uB), Vec3.Min(vA, vB), "Mínimo (Min)");
        result &= AssertVectorMatch(Vector3.Max(uA, uB), Vec3.Max(vA, vB), "Máximo (Max)");
        return result;
    }

    bool TestProject(Vector3 uA, Vector3 norm, Vec3 vA, Vec3 vNorm)
    {
        return AssertVectorMatch(Vector3.Project(uA, norm), Vec3.Project(vA, vNorm), "Proyección (Project)");
    }

    bool TestReflect(Vector3 uDir, Vector3 uNorm, Vec3 vDir, Vec3 vNorm)
    {
        return AssertVectorMatch(Vector3.Reflect(uDir, uNorm), Vec3.Reflect(vDir, vNorm), "Reflexión (Reflect)");
    }

    bool TestPlaneConstructorPointNormal(Vector3 norm, Vector3 pt, Vec3 vNorm, Vec3 vPt)
    {
        Plane uPlane = new Plane(norm, pt);
        MyPlane vPlane = new MyPlane(vNorm, vPt);

        return AssertPlaneMatch(uPlane, vPlane, "Plano: Constructor (Normal, Punto)");
    }

    bool TestPlaneConstructor3Points(Vector3 a, Vector3 b, Vector3 c, Vec3 vA, Vec3 vB, Vec3 vC)
    {
        Plane uPlane = new Plane(a, b, c);
        MyPlane vPlane = new MyPlane(vA, vB, vC);

        return AssertPlaneMatch(uPlane, vPlane, "Plano: Constructor (3 Puntos)");
    }

    bool TestPlaneGetSideAndDistance(Vector3 norm, Vector3 pt, Vector3 testPt1, Vector3 testPt2, Vec3 vNorm, Vec3 vPt, Vec3 vTest1, Vec3 vTest2)
    {
        Plane uPlane = new Plane(norm, pt);
        MyPlane vPlane = new MyPlane(vNorm, vPt);

        bool result = true;
        result &= AssertFloatMatch(uPlane.GetDistanceToPoint(testPt1), vPlane.GetDistanceToPoint(vTest1), "Plano: DistanceToPoint 1");
        result &= AssertFloatMatch(uPlane.GetDistanceToPoint(testPt2), vPlane.GetDistanceToPoint(vTest2), "Plano: DistanceToPoint 2");

        if (uPlane.GetSide(testPt1) != vPlane.GetSide(vTest1))
        {
            Debug.LogError($"[FAILED] Plano: GetSide");
            result = false;
        }
        else
        {
            Debug.Log("[PASSED] Plano: GetSide");
        }

        return result;
    }

    bool TestPlaneClosestPoint(Vector3 norm, Vector3 pt, Vector3 testPt, Vec3 vNorm, Vec3 vPt, Vec3 vTest)
    {
        Plane uPlane = new Plane(norm, pt);
        MyPlane vPlane = new MyPlane(vNorm, vPt);

        return AssertVectorMatch(uPlane.ClosestPointOnPlane(testPt), vPlane.ClosestPointOnPlane(vTest), "Plano: ClosestPointOnPlane");
    }

    bool TestPlaneFlip(Vector3 norm, Vector3 pt, Vec3 vNorm, Vec3 vPt)
    {
        Plane uPlane = new Plane(norm, pt);
        MyPlane vPlane = new MyPlane(vNorm, vPt);

        uPlane.Flip();
        vPlane.Flip();

        return AssertPlaneMatch(uPlane, vPlane, "Plano: Flip()");
    }

    bool TestPlaneTranslate(Vector3 norm, Vector3 pt, Vector3 translation, Vec3 vNorm, Vec3 vPt, Vec3 vTranslation)
    {
        Plane uPlane = new Plane(norm, pt);
        MyPlane vPlane = new MyPlane(vNorm, vPt);

        uPlane.Translate(-translation);
        vPlane.Translate(vTranslation);

        return AssertPlaneMatch(uPlane, vPlane, "Plano: Translate()");
    }

    private bool AssertPlaneMatch(Plane expected, MyPlane actual, string testName)
    {
        bool passed = true;

        if (Mathf.Abs(expected.normal.x - actual.normal.x) > tolerance ||
            Mathf.Abs(expected.normal.y - actual.normal.y) > tolerance ||
            Mathf.Abs(expected.normal.z - actual.normal.z) > tolerance)
        {
            Debug.LogError($"[FAILED] {testName} (NORMAL): Esperado {expected.normal.ToString("F4")}, pero se obtuvo {actual.normal.ToString()}");
            passed = false;
        }

        if (Mathf.Abs(expected.distance - actual.distance) > tolerance)
        {
            Debug.LogError($"[FAILED] {testName} (DISTANCE): Esperado {expected.distance:F4}, pero se obtuvo {actual.distance:F4}");
            passed = false;
        }

        if (passed) Debug.Log($"[PASSED] {testName}");
        return passed;
    }

    private bool AssertVectorMatch(Vector3 expected, Vec3 actual, string testName)
    {
        Vector3 actualV3 = new Vector3(actual.x, actual.y, actual.z);

        float diffX = Mathf.Abs(expected.x - actual.x);
        float diffY = Mathf.Abs(expected.y - actual.y);
        float diffZ = Mathf.Abs(expected.z - actual.z);

        if (diffX > tolerance || diffY > tolerance || diffZ > tolerance)
        {
            Debug.LogError($"[FAILED] {testName}: Esperado {expected.ToString("F4")}, pero se obtuvo {actualV3.ToString("F4")}");
            return false;
        }

        Debug.Log($"[PASSED] {testName}");
        return true;
    }

    private bool AssertFloatMatch(float expected, float actual, string testName)
    {
        if (float.IsNaN(actual) && !float.IsNaN(expected))
        {
            Debug.LogError($"[FAILED] {testName}: Esperado {expected:F4}, pero se obtuvo NaN (Error Matemático)");
            return false;
        }

        if (Mathf.Abs(expected - actual) > tolerance)
        {
            Debug.LogError($"[FAILED] {testName}: Esperado {expected:F4}, pero se obtuvo {actual:F4}");
            return false;
        }

        Debug.Log($"[PASSED] {testName}");
        return true;
    }
}