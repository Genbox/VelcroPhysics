using System;
using System.Globalization;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.ContentPipeline
{
    [ContentImporter(".svg", DisplayName = "Farseer SVG Importer", DefaultProcessor = "FarseerBodyProcessor")]
    class FarseerSVGImporter : ContentImporter<List<RawBodyTemplate>>
    {
        private const string isNumber = @"\A[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
        private const string isCommaWhitespace = @"\A[\s,]*";

        private Stack<Matrix> _transformations;
        private List<RawBodyTemplate> _parsedSVG;
        private RawBodyTemplate? _currentBody;

        public override List<RawBodyTemplate> Import(string filename, ContentImporterContext context)
        {
            _transformations = new Stack<Matrix>();
            _transformations.Push(Matrix.Identity);

            _parsedSVG = new List<RawBodyTemplate>();
            _parsedSVG.Add(new RawBodyTemplate()
            {
                name = "importer_default_path_container",
                fixtures = new List<RawFixtureTemplate>(),
                mass = 0f
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
                    if (!Enum.TryParse<BodyType>(currentElement.Attributes["fp_body"].Value, true, out type))
                    {
                        type = BodyType.Static;
                    }
                    string ID = "empty_id";
                    if (currentElement.HasAttribute("id"))
                    {
                        ID = currentElement.Attributes["id"].Value;
                    }
                    float bodyMass = 0f;
                    if (currentElement.HasAttribute("fp_mass"))
                    {
                        float.TryParse(currentElement.Attributes["fp_mass"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out bodyMass);
                    }
                    _currentBody = new RawBodyTemplate()
                    {
                        fixtures = new List<RawFixtureTemplate>(),
                        mass = bodyMass,
                        name = ID,
                        bodyType = type
                    };
                    killBody = true;
                }
                if (currentElement.Name == "path")
                {
                    RawFixtureTemplate fixture = new RawFixtureTemplate();
                    fixture.path = currentElement.Attributes["d"].Value;
                    fixture.transformation = Matrix.Identity;
                    foreach (Matrix m in _transformations)
                    {
                        fixture.transformation *= m;
                    }
                    if (currentElement.HasAttribute("id"))
                    {
                        fixture.name = currentElement.Attributes["id"].Value;
                    }
                    else
                    {
                        fixture.name = "empty_id";
                    }
                    if (!(currentElement.HasAttribute("fp_density") && float.TryParse(currentElement.Attributes["fp_density"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out fixture.density)))
                    {
                        fixture.density = 1f;
                    }
                    if (!(currentElement.HasAttribute("fp_friction") && float.TryParse(currentElement.Attributes["fp_friction"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out fixture.friction)))
                    {
                        fixture.friction = 0.5f;
                    }
                    if (!(currentElement.HasAttribute("fp_restitution") && float.TryParse(currentElement.Attributes["fp_restitution"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out fixture.restitution)))
                    {
                        fixture.restitution = 0f;
                    }
                    if (_currentBody.HasValue)
                    {
                        _currentBody.Value.fixtures.Add(fixture);
                    }
                    else
                    {
                        _parsedSVG[0].fixtures.Add(fixture);
                    }
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
            {
                _transformations.Pop();
            }
            if (killBody)
            {
                if (_currentBody.Value.fixtures.Count > 0)
                {
                    _parsedSVG.Add(_currentBody.Value);
                }
                _currentBody = null;
            }
        }

        private Matrix ParseSVGTransformation(string transformation)
        {
            Stack<Matrix> results = new Stack<Matrix>();
            float[] arguments;
            int argumentCount;

            while (!string.IsNullOrEmpty(transformation))
            {
                if (transformation.StartsWith("matrix"))
                {
                    transformation = parseTransformationArguments(transformation, out arguments, out argumentCount);
                    if (argumentCount == 6)
                    {
                        Matrix m = Matrix.Identity;
                        m.M11 = arguments[0];
                        m.M12 = arguments[1];
                        m.M21 = arguments[2];
                        m.M22 = arguments[3];
                        m.M41 = arguments[4];
                        m.M42 = arguments[5];
                        results.Push(m);
                    }
                }
                else if (transformation.StartsWith("scale"))
                {
                    transformation = parseTransformationArguments(transformation, out arguments, out argumentCount);
                    if (argumentCount == 1)
                    {
                        arguments[argumentCount++] = arguments[0];
                    }
                    if (argumentCount == 2)
                    {
                        results.Push(Matrix.CreateScale(arguments[0], arguments[1], 1f));
                    }
                }
                else if (transformation.StartsWith("translate"))
                {
                    transformation = parseTransformationArguments(transformation, out arguments, out argumentCount);
                    if (argumentCount == 1)
                    {
                        arguments[argumentCount++] = arguments[0];
                    }
                    if (argumentCount == 2)
                    {
                        results.Push(Matrix.CreateTranslation(arguments[0], arguments[1], 0f));
                    }
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

        private string parseTransformationArguments(string operation, out float[] arguments, out int argumentCount)
        {
            arguments = new float[6];
            argumentCount = 0;

            int start = operation.IndexOf('(');
            int end = operation.IndexOf(')', start);

            string parameters = operation.Substring(start + 1, end - start - 1);

            while (!string.IsNullOrEmpty(parameters))
            {
                parameters = Regex.Replace(parameters, isCommaWhitespace, "");
                if (Regex.IsMatch(parameters, isNumber))
                {
                    int matchLength = Regex.Match(parameters, isNumber).Length;
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