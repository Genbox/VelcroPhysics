using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using VelcroPhysics.ContentPipelines.SVGImport.Objects;
using VelcroPhysics.Dynamics;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    [ContentImporter(".svg", DisplayName = "SVG Importer", DefaultProcessor = "BodyProcessor")]
    public class SVGImporter : ContentImporter<List<BodyTemplateExt>>
    {
        private const string _isNumber = @"\A[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
        private const string _isCommaWhitespace = @"\A[\s,]*";
        private BodyTemplateExt _currentBody;
        private List<BodyTemplateExt> _parsedSVG;

        private Stack<Matrix> _transformations;

        public override List<BodyTemplateExt> Import(string filename, ContentImporterContext context)
        {
            _transformations = new Stack<Matrix>();
            _transformations.Push(Matrix.Identity);

            _parsedSVG = new List<BodyTemplateExt>();
            _parsedSVG.Add(new BodyTemplateExt
            {
                Name = "importer_default_path_container",
                Fixtures = new List<FixtureTemplateExt>(),
                //Mass = 0f
            });

            XmlDocument input = new XmlDocument();
            input.Load(filename);

            _currentBody = null;
            ParseSVGNode(input["svg"]);

            return _parsedSVG;
        }

        private void ParseSVGNode(XmlNode currentNode)
        {
            bool popTransform = false;
            bool killBody = false;
            if (currentNode is XmlElement)
            {
                XmlElement currentElement = currentNode as XmlElement;
                if (currentElement.HasAttribute("transform"))
                {
                    _transformations.Push(ParseSVGTransformation(currentNode.Attributes["transform"].Value));
                    popTransform = true;
                }

                if (currentElement.HasAttribute("fp_body") && _currentBody == null)
                {
                    BodyType type;
                    if (!Enum.TryParse(currentElement.Attributes["fp_body"].Value, true, out type))
                        type = BodyType.Static;

                    string id = "empty_id";
                    if (currentElement.HasAttribute("id"))
                        id = currentElement.Attributes["id"].Value;

                    float bodyMass = 0f;
                    if (currentElement.HasAttribute("fp_mass"))
                        float.TryParse(currentElement.Attributes["fp_mass"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out bodyMass);

                    _currentBody = new BodyTemplateExt
                    {
                        Fixtures = new List<FixtureTemplateExt>(),
                        //Mass = bodyMass,
                        Name = id,
                        Type = type
                    };
                    killBody = true;
                }
                if (currentElement.Name == "path")
                {
                    FixtureTemplateExt fixture = new FixtureTemplateExt();
                    fixture.Path = currentElement.Attributes["d"].Value;
                    fixture.Transformation = Matrix.Identity;

                    foreach (Matrix m in _transformations)
                    {
                        fixture.Transformation *= m;
                    }

                    fixture.Name = currentElement.HasAttribute("id") ? currentElement.Attributes["id"].Value : "empty_id";

                    if (currentElement.HasAttribute("fp_density") && float.TryParse(currentElement.Attributes["fp_density"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float density))
                        fixture.Density = density;
                    else
                        fixture.Density = 1f;

                    if (currentElement.HasAttribute("fp_friction") && float.TryParse(currentElement.Attributes["fp_friction"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float friction))
                        fixture.Friction = friction;
                    else
                        fixture.Friction = 0.5f;

                    if (currentElement.HasAttribute("fp_restitution") && float.TryParse(currentElement.Attributes["fp_restitution"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float restitution))
                        fixture.Restitution = restitution;

                    if (_currentBody != null)
                        _currentBody.Fixtures.Add(fixture);
                    else
                        _parsedSVG[0].Fixtures.Add(fixture);
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

            if (killBody)
            {
                if (_currentBody.Fixtures.Count > 0)
                {
                    _parsedSVG.Add(_currentBody);
                }
                _currentBody = null;
            }
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