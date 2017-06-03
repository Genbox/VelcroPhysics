using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using VelcroPhysics.ContentPipelines.SVGImport.Objects;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    [ContentImporter(".svg", DisplayName = "SVG Importer", DefaultProcessor = "PathContainerProcessor")]
    public class SVGImporter : ContentImporter<List<PathDefinition>>
    {
        private const string _isNumber = @"\A[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
        private const string _isCommaWhitespace = @"\A[\s,]*";
        private List<PathDefinition> _parsedSVG;

        private Stack<Matrix> _transformations;

        public override List<PathDefinition> Import(string filename, ContentImporterContext context)
        {
            _transformations = new Stack<Matrix>();
            _transformations.Push(Matrix.Identity);

            _parsedSVG = new List<PathDefinition>();

            XmlDocument input = new XmlDocument();
            input.Load(filename);

            ParseSVGNode(input["svg"]);

            return _parsedSVG;
        }

        private void ParseSVGNode(XmlNode currentNode)
        {
            bool popTransform = false;
            XmlElement currentElement = currentNode as XmlElement;

            if (currentElement != null)
            {
                if (currentElement.HasAttribute("transform"))
                {
                    _transformations.Push(ParseSVGTransformation(currentNode.Attributes["transform"].Value));
                    popTransform = true;
                }

                if (currentElement.Name == "path")
                {
                    PathDefinition path = new PathDefinition();

                    string currentId = currentElement.HasAttribute("velcro_id") ? currentElement.Attributes["velcro_id"].Value : null;
                    XmlElement parent = currentElement.ParentNode as XmlElement;

                    //Take the attribute from the parent if it is a group, otherwise just take it from the current element
                    if (currentId == null && parent != null  && parent.HasAttribute("velcro_id"))
                        path.Id = parent.Attributes["velcro_id"].Value;
                    else
                        path.Id = currentElement.HasAttribute("velcro_id") ? currentElement.Attributes["velcro_id"].Value : "empty_id";

                    path.Path = currentElement.Attributes["d"].Value;
                    path.Transformation = Matrix.Identity;

                    foreach (Matrix m in _transformations)
                    {
                        path.Transformation *= m;
                    }

                    _parsedSVG.Add(path);
                }
            }

            if (currentNode.HasChildNodes)
            {
                foreach (XmlNode child in currentNode.ChildNodes)
                {
                    ParseSVGNode(child);
                }
            }

            if (popTransform)
                _transformations.Pop();
        }

        private Matrix ParseSVGTransformation(string transformation)
        {
            Stack<Matrix> results = new Stack<Matrix>();

            while (!string.IsNullOrEmpty(transformation))
            {
                float[] arguments;
                int argumentCount;
                if (transformation.StartsWith("matrix"))
                {
                    transformation = ParseTransformationArguments(transformation, out arguments, out argumentCount);

                    if (argumentCount != 6)
                        continue;

                    Matrix m = Matrix.Identity;
                    m.M11 = arguments[0];
                    m.M12 = arguments[1];
                    m.M21 = arguments[2];
                    m.M22 = arguments[3];
                    m.M41 = arguments[4];
                    m.M42 = arguments[5];
                    results.Push(m);
                }
                else if (transformation.StartsWith("scale"))
                {
                    transformation = ParseTransformationArguments(transformation, out arguments, out argumentCount);

                    if (argumentCount == 1)
                        arguments[argumentCount++] = arguments[0];

                    if (argumentCount == 2)
                        results.Push(Matrix.CreateScale(arguments[0], arguments[1], 1f));
                }
                else if (transformation.StartsWith("translate"))
                {
                    transformation = ParseTransformationArguments(transformation, out arguments, out argumentCount);

                    if (argumentCount == 1)
                        arguments[argumentCount++] = arguments[0];

                    if (argumentCount == 2)
                        results.Push(Matrix.CreateTranslation(arguments[0], arguments[1], 0f));
                }
                else
                {
                    transformation = transformation.Remove(0, 1);
                }
            }

            Matrix result = Matrix.Identity;
            foreach (Matrix m in results)
            {
                result *= m;
            }
            return result;
        }

        private string ParseTransformationArguments(string operation, out float[] arguments, out int argumentCount)
        {
            arguments = new float[6];
            argumentCount = 0;

            int start = operation.IndexOf('(');
            int end = operation.IndexOf(')', start);

            string parameters = operation.Substring(start + 1, end - start - 1);

            while (!string.IsNullOrEmpty(parameters))
            {
                parameters = Regex.Replace(parameters, _isCommaWhitespace, "");
                if (Regex.IsMatch(parameters, _isNumber))
                {
                    int matchLength = Regex.Match(parameters, _isNumber).Length;
                    arguments[argumentCount++] = float.Parse(parameters.Substring(0, matchLength), CultureInfo.InvariantCulture);
                    parameters = parameters.Remove(0, matchLength);
                }
                else
                {
                    parameters = parameters.Remove(0, 1);
                }
            }
            return operation.Remove(0, end + 1);
        }
    }
}