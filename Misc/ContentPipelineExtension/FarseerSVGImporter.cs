using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace FarseerPhysics.ContentPipeline
{
    [ContentImporter(".svg", DisplayName = "Farseer SVG Importer", DefaultProcessor = "FarseerBodyProcessor")]
    class FarseerSVGImporter : ContentImporter<List<RawBodyTemplate>>
    {
        private const string IsNumber = @"\A[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
        private const string IsCommaWhitespace = @"\A[\s,]*";

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
                Name = "importer_default_path_container",
                Fixtures = new List<RawFixtureTemplate>(),
                Mass = 0f
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
                        Fixtures = new List<RawFixtureTemplate>(),
                        Mass = bodyMass,
                        Name = ID,
                        BodyType = type
                    };
                    killBody = true;
                }
                if (currentElement.Name == "path")
                {
                    RawFixtureTemplate fixture = new RawFixtureTemplate();
                    fixture.Path = currentElement.Attributes["d"].Value;
                    fixture.Transformation = Matrix.Identity;
                    foreach (Matrix m in _transformations)
                    {
                        fixture.Transformation *= m;
                    }
                    if (currentElement.HasAttribute("id"))
                    {
                        fixture.Name = currentElement.Attributes["id"].Value;
                    }
                    else
                    {
                        fixture.Name = "empty_id";
                    }
                    if (!(currentElement.HasAttribute("fp_density") && float.TryParse(currentElement.Attributes["fp_density"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out fixture.Density)))
                    {
                        fixture.Density = 1f;
                    }
                    if (!(currentElement.HasAttribute("fp_friction") && float.TryParse(currentElement.Attributes["fp_friction"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out fixture.Friction)))
                    {
                        fixture.Friction = 0.5f;
                    }
                    if (!(currentElement.HasAttribute("fp_restitution") && float.TryParse(currentElement.Attributes["fp_restitution"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out fixture.Restitution)))
                    {
                        fixture.Restitution = 0f;
                    }
                    if (_currentBody.HasValue)
                    {
                        _currentBody.Value.Fixtures.Add(fixture);
                    }
                    else
                    {
                        _parsedSVG[0].Fixtures.Add(fixture);
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
                if (_currentBody.Value.Fixtures.Count > 0)
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
                parameters = Regex.Replace(parameters, IsCommaWhitespace, "");
                if (Regex.IsMatch(parameters, IsNumber))
                {
                    int matchLength = Regex.Match(parameters, IsNumber).Length;
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