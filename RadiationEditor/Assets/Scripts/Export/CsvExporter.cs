using System.IO;
using System.Text;
using UnityEngine;

public class CsvExporter : MonoBehaviour
{
    public void Export()
    {
        var sb = new StringBuilder();
        sb.AppendLine("id,type,material,px,py,pz,sx,sy,sz,rx,ry,rz,radius,radiusX,radiusZ,height");

        int id = 0;
        foreach (var s in ShapeManager.I.shapes)
        {
            if (!s) continue;

            s.RecomputeDerived();

            var p = s.transform.position;
            var sc = s.transform.localScale;
            var e = s.transform.eulerAngles;

            sb.Append(id++).Append(',')
            .Append(s.type).Append(',')
            .Append(Escape(s.materialName)).Append(',')

            // position
            .Append(p.x.ToString("0.######")).Append(',')
            .Append(p.y.ToString("0.######")).Append(',')
            .Append(p.z.ToString("0.######")).Append(',')

            // scale
            .Append(sc.x.ToString("0.######")).Append(',')
            .Append(sc.y.ToString("0.######")).Append(',')
            .Append(sc.z.ToString("0.######")).Append(',')

            // rotation
            .Append(e.x.ToString("0.######")).Append(',')
            .Append(e.y.ToString("0.######")).Append(',')
            .Append(e.z.ToString("0.######")).Append(',')

            // geometry
            .Append(s.radius.ToString("0.######")).Append(',')
            .Append(s.radiusX.ToString("0.######")).Append(',')
            .Append(s.radiusZ.ToString("0.######")).Append(',')
            .Append(s.height.ToString("0.######"))
            .AppendLine();
        }

        string fileName = "radiation_shapes_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

        Debug.Log("CSV exported: " + path);
        Debug.Log("persistentDataPath: " + Application.persistentDataPath);
    }

    // da CSV ne pukne ako materijal ima zarez ili navodnike
    string Escape(string v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        if (v.Contains(",") || v.Contains("\""))
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }
}
