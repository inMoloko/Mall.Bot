using Mall.Bot.Common.DBHelpers.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Mall.Bot.Common.Helpers
{
    public class Node
    {
        public float x;
        public float y;
        public int num;

        public override bool Equals(object obj)
        {
            Node p = obj as Node;
            return (x == p.x) && (y == p.y);
        }
        public override int GetHashCode()
        {
            return num;
        }
    }
    public class Graph
    {
        public List<Node> Nodes;
        public int[,] _Graph;

    }
    public class PathParserHelper
    {
        public PathPoint[][] Do(string Paths)
        {

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(Paths);
            XmlElement xRoot = xDoc.DocumentElement;
            string temp = "";

            foreach (XmlNode xnode in xRoot)
            {
                foreach (XmlNode childnode in xnode.ChildNodes)  // вытаскиваем из xml файла данные о графе.
                {

                    if (childnode.Name == "Path")
                    {
                        if (childnode.ChildNodes[0].Name == "Path.Data")
                        {
                            if (childnode.ChildNodes[0].ChildNodes[0].Name == "PathGeometry")
                            {
                                XmlNode attr = childnode.ChildNodes[0].ChildNodes[0].Attributes.GetNamedItem("Figures");
                                temp += attr.Value + ";";
                            }
                        }
                    }
                }
            }
            string[] lines = temp.Split(';');
            PathPoint[][] Lines = new PathPoint[lines.Length-1][];
            for (int i = 0; i < Lines.Length; i++)
            {
                Lines[i] = new PathPoint[2];
                //M196.5,1400L196.5,985

                int index = 1;
                string numderFrom = "";
                
                while (lines[i][index] != ',')
                {
                    numderFrom += lines[i][index];
                    index++;
                }

                index++;
                string numderTo = "";
                while (lines[i][index] != 'L')
                {
                    numderTo += lines[i][index];
                    index++;
                }

                Lines[i][0] = new PathPoint { X = double.Parse(numderFrom, CultureInfo.InvariantCulture), Y = double.Parse(numderTo, CultureInfo.InvariantCulture) };

                index++;
                numderFrom = "";
                while (lines[i][index] != ',')
                {
                    numderFrom += lines[i][index];
                    index++;
                }

                index++;
                numderTo = "";
                while (index < lines[i].Length)
                {
                    numderTo += lines[i][index];
                    index++;
                }

                Lines[i][1] = new PathPoint { X = double.Parse(numderFrom, CultureInfo.InvariantCulture), Y = double.Parse(numderTo, CultureInfo.InvariantCulture) };
            }

            return Lines;
        }
    }
}