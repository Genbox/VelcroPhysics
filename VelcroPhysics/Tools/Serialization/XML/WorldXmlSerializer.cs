using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Joints;

namespace VelcroPhysics.Tools.Serialization.XML
{
    internal static class WorldXmlSerializer
    {
        private static XmlWriter _writer;

        private static void SerializeShape(Shape shape)
        {
            _writer.WriteStartElement("Shape");
            _writer.WriteAttributeString("Type", shape.ShapeType.ToString());
            _writer.WriteAttributeString("Density", shape.Density.ToString());

            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                {
                    CircleShape circle = (CircleShape)shape;

                    _writer.WriteElementString("Radius", circle.Radius.ToString());

                    WriteElement("Position", circle.Position);
                }
                    break;
                case ShapeType.Polygon:
                {
                    PolygonShape poly = (PolygonShape)shape;

                    _writer.WriteStartElement("Vertices");
                    foreach (Vector2 v in poly.Vertices)
                        WriteElement("Vertex", v);
                    _writer.WriteEndElement();

                    WriteElement("Centroid", poly.MassData.Centroid);
                }
                    break;
                case ShapeType.Edge:
                {
                    EdgeShape poly = (EdgeShape)shape;
                    WriteElement("Vertex1", poly.Vertex1);
                    WriteElement("Vertex2", poly.Vertex2);
                }
                    break;
                case ShapeType.Chain:
                {
                    ChainShape chain = (ChainShape)shape;

                    _writer.WriteStartElement("Vertices");
                    foreach (Vector2 v in chain.Vertices)
                        WriteElement("Vertex", v);
                    _writer.WriteEndElement();

                    WriteElement("NextVertex", chain.NextVertex);
                    WriteElement("PrevVertex", chain.PrevVertex);
                }
                    break;
                default:
                    throw new Exception();
            }

            _writer.WriteEndElement();
        }

        private static void SerializeFixture(Fixture fixture)
        {
            _writer.WriteStartElement("Fixture");
            _writer.WriteAttributeString("Id", fixture.FixtureId.ToString());

            _writer.WriteStartElement("FilterData");
            _writer.WriteElementString("CategoryBits", ((int)fixture.CollisionCategories).ToString());
            _writer.WriteElementString("MaskBits", ((int)fixture.CollidesWith).ToString());
            _writer.WriteElementString("GroupIndex", fixture.CollisionGroup.ToString());
            _writer.WriteElementString("CollisionIgnores", Join("|", fixture._collisionIgnores));
            _writer.WriteEndElement();

            _writer.WriteElementString("Friction", fixture.Friction.ToString());
            _writer.WriteElementString("IsSensor", fixture.IsSensor.ToString());
            _writer.WriteElementString("Restitution", fixture.Restitution.ToString());

            if (fixture.UserData != null)
            {
                _writer.WriteStartElement("UserData");
                WriteDynamicType(fixture.UserData.GetType(), fixture.UserData);
                _writer.WriteEndElement();
            }

            _writer.WriteEndElement();
        }

        private static void SerializeBody(List<Fixture> fixtures, List<Shape> shapes, Body body)
        {
            _writer.WriteStartElement("Body");
            _writer.WriteAttributeString("Type", body.BodyType.ToString());
            _writer.WriteElementString("Active", body.Enabled.ToString());
            _writer.WriteElementString("AllowSleep", body.SleepingAllowed.ToString());
            _writer.WriteElementString("Angle", body.Rotation.ToString());
            _writer.WriteElementString("AngularDamping", body.AngularDamping.ToString());
            _writer.WriteElementString("AngularVelocity", body.AngularVelocity.ToString());
            _writer.WriteElementString("Awake", body.Awake.ToString());
            _writer.WriteElementString("Bullet", body.IsBullet.ToString());
            _writer.WriteElementString("FixedRotation", body.FixedRotation.ToString());
            _writer.WriteElementString("LinearDamping", body.LinearDamping.ToString());
            WriteElement("LinearVelocity", body.LinearVelocity);
            WriteElement("Position", body.Position);

            if (body.UserData != null)
            {
                _writer.WriteStartElement("UserData");
                WriteDynamicType(body.UserData.GetType(), body.UserData);
                _writer.WriteEndElement();
            }

            _writer.WriteStartElement("Bindings");
            for (int i = 0; i < body.FixtureList.Count; i++)
            {
                _writer.WriteStartElement("Pair");
                _writer.WriteAttributeString("FixtureId", FindIndex(fixtures, body.FixtureList[i]).ToString());
                _writer.WriteAttributeString("ShapeId", FindIndex(shapes, body.FixtureList[i].Shape).ToString());
                _writer.WriteEndElement();
            }

            _writer.WriteEndElement();
            _writer.WriteEndElement();
        }

        private static void SerializeJoint(List<Body> bodies, Joint joint)
        {
            _writer.WriteStartElement("Joint");
            _writer.WriteAttributeString("Type", joint.JointType.ToString());

            WriteElement("BodyA", FindIndex(bodies, joint.BodyA));
            WriteElement("BodyB", FindIndex(bodies, joint.BodyB));

            WriteElement("CollideConnected", joint.CollideConnected);

            WriteElement("Breakpoint", joint.Breakpoint);

            if (joint.UserData != null)
            {
                _writer.WriteStartElement("UserData");
                WriteDynamicType(joint.UserData.GetType(), joint.UserData);
                _writer.WriteEndElement();
            }

            switch (joint.JointType)
            {
                case JointType.Distance:
                {
                    DistanceJoint distanceJoint = (DistanceJoint)joint;
                    WriteElement("DampingRatio", distanceJoint.DampingRatio);
                    WriteElement("FrequencyHz", distanceJoint.Frequency);
                    WriteElement("Length", distanceJoint.Length);
                    WriteElement("LocalAnchorA", distanceJoint.LocalAnchorA);
                    WriteElement("LocalAnchorB", distanceJoint.LocalAnchorB);
                }
                    break;
                case JointType.Friction:
                {
                    FrictionJoint frictionJoint = (FrictionJoint)joint;
                    WriteElement("LocalAnchorA", frictionJoint.LocalAnchorA);
                    WriteElement("LocalAnchorB", frictionJoint.LocalAnchorB);
                    WriteElement("MaxForce", frictionJoint.MaxForce);
                    WriteElement("MaxTorque", frictionJoint.MaxTorque);
                }
                    break;
                case JointType.Gear:
                    throw new Exception("Gear joint not supported by serialization");
                case JointType.Wheel:
                {
                    WheelJoint wheelJoint = (WheelJoint)joint;
                    WriteElement("EnableMotor", wheelJoint.MotorEnabled);
                    WriteElement("LocalAnchorA", wheelJoint.LocalAnchorA);
                    WriteElement("LocalAnchorB", wheelJoint.LocalAnchorB);
                    WriteElement("MotorSpeed", wheelJoint.MotorSpeed);
                    WriteElement("DampingRatio", wheelJoint.DampingRatio);
                    WriteElement("MaxMotorTorque", wheelJoint.MaxMotorTorque);
                    WriteElement("FrequencyHz", wheelJoint.Frequency);
                    WriteElement("Axis", wheelJoint.Axis);
                }
                    break;
                case JointType.Prismatic:
                {
                    //NOTE: Does not conform with Box2DScene

                    PrismaticJoint prismaticJoint = (PrismaticJoint)joint;
                    WriteElement("EnableLimit", prismaticJoint.LimitEnabled);
                    WriteElement("EnableMotor", prismaticJoint.MotorEnabled);
                    WriteElement("LocalAnchorA", prismaticJoint.LocalAnchorA);
                    WriteElement("LocalAnchorB", prismaticJoint.LocalAnchorB);
                    WriteElement("Axis", prismaticJoint.Axis);
                    WriteElement("LowerTranslation", prismaticJoint.LowerLimit);
                    WriteElement("UpperTranslation", prismaticJoint.UpperLimit);
                    WriteElement("MaxMotorForce", prismaticJoint.MaxMotorForce);
                    WriteElement("MotorSpeed", prismaticJoint.MotorSpeed);
                }
                    break;
                case JointType.Pulley:
                {
                    PulleyJoint pulleyJoint = (PulleyJoint)joint;
                    WriteElement("WorldAnchorA", pulleyJoint.WorldAnchorA);
                    WriteElement("WorldAnchorB", pulleyJoint.WorldAnchorB);
                    WriteElement("LengthA", pulleyJoint.LengthA);
                    WriteElement("LengthB", pulleyJoint.LengthB);
                    WriteElement("LocalAnchorA", pulleyJoint.LocalAnchorA);
                    WriteElement("LocalAnchorB", pulleyJoint.LocalAnchorB);
                    WriteElement("Ratio", pulleyJoint.Ratio);
                    WriteElement("Constant", pulleyJoint.Constant);
                }
                    break;
                case JointType.Revolute:
                {
                    RevoluteJoint revoluteJoint = (RevoluteJoint)joint;
                    WriteElement("EnableLimit", revoluteJoint.LimitEnabled);
                    WriteElement("EnableMotor", revoluteJoint.MotorEnabled);
                    WriteElement("LocalAnchorA", revoluteJoint.LocalAnchorA);
                    WriteElement("LocalAnchorB", revoluteJoint.LocalAnchorB);
                    WriteElement("LowerAngle", revoluteJoint.LowerLimit);
                    WriteElement("MaxMotorTorque", revoluteJoint.MaxMotorTorque);
                    WriteElement("MotorSpeed", revoluteJoint.MotorSpeed);
                    WriteElement("ReferenceAngle", revoluteJoint.ReferenceAngle);
                    WriteElement("UpperAngle", revoluteJoint.UpperLimit);
                }
                    break;
                case JointType.Weld:
                {
                    WeldJoint weldJoint = (WeldJoint)joint;
                    WriteElement("LocalAnchorA", weldJoint.LocalAnchorA);
                    WriteElement("LocalAnchorB", weldJoint.LocalAnchorB);
                }
                    break;

                //
                // Not part of Box2DScene
                //
                case JointType.Rope:
                {
                    RopeJoint ropeJoint = (RopeJoint)joint;
                    WriteElement("LocalAnchorA", ropeJoint.LocalAnchorA);
                    WriteElement("LocalAnchorB", ropeJoint.LocalAnchorB);
                    WriteElement("MaxLength", ropeJoint.MaxLength);
                }
                    break;
                case JointType.Angle:
                {
                    AngleJoint angleJoint = (AngleJoint)joint;
                    WriteElement("BiasFactor", angleJoint.BiasFactor);
                    WriteElement("MaxImpulse", angleJoint.MaxImpulse);
                    WriteElement("Softness", angleJoint.Softness);
                    WriteElement("TargetAngle", angleJoint.TargetAngle);
                }
                    break;
                case JointType.Motor:
                {
                    MotorJoint motorJoint = (MotorJoint)joint;
                    WriteElement("AngularOffset", motorJoint.AngularOffset);
                    WriteElement("LinearOffset", motorJoint.LinearOffset);
                    WriteElement("MaxForce", motorJoint.MaxForce);
                    WriteElement("MaxTorque", motorJoint.MaxTorque);
                    WriteElement("CorrectionFactor", motorJoint.CorrectionFactor);
                }
                    break;
                default:
                    throw new Exception("Joint not supported");
            }

            _writer.WriteEndElement();
        }

        private static void WriteDynamicType(Type type, object val)
        {
            _writer.WriteElementString("Type", type.AssemblyQualifiedName);

            _writer.WriteStartElement("Value");
            XmlSerializer serializer = new XmlSerializer(type);
            XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces();
            xmlnsEmpty.Add("", "");
            serializer.Serialize((XmlWriter)_writer, val, xmlnsEmpty);
            _writer.WriteEndElement();
        }

        private static void WriteElement(string name, Vector2 vec)
        {
            _writer.WriteElementString(name, vec.X + " " + vec.Y);
        }

        private static void WriteElement(string name, int val)
        {
            _writer.WriteElementString(name, val.ToString());
        }

        private static void WriteElement(string name, bool val)
        {
            _writer.WriteElementString(name, val.ToString());
        }

        private static void WriteElement(string name, float val)
        {
            _writer.WriteElementString(name, val.ToString());
        }

        private static int FindIndex(List<Body> list, Body item)
        {
            for (int i = 0; i < list.Count; ++i)
                if (list[i] == item)
                    return i;

            return -1;
        }

        private static int FindIndex(List<Fixture> list, Fixture item)
        {
            for (int i = 0; i < list.Count; ++i)
                if (list[i].CompareTo(item))
                    return i;

            return -1;
        }

        private static int FindIndex(List<Shape> list, Shape item)
        {
            for (int i = 0; i < list.Count; ++i)
                if (list[i].CompareTo(item))
                    return i;

            return -1;
        }

        private static String Join<T>(String separator, IEnumerable<T> values)
        {
            using (IEnumerator<T> en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return String.Empty;

                StringBuilder result = new StringBuilder();
                if (en.Current != null)
                {
                    // handle the case that the enumeration has null entries
                    // and the case where their ToString() override is broken
                    string value = en.Current.ToString();
                    if (value != null)
                        result.Append(value);
                }

                while (en.MoveNext())
                {
                    result.Append(separator);
                    if (en.Current != null)
                    {
                        // handle the case that the enumeration has null entries
                        // and the case where their ToString() override is broken
                        string value = en.Current.ToString();
                        if (value != null)
                            result.Append(value);
                    }
                }
                return result.ToString();
            }
        }

        internal static void Serialize(World world, Stream stream)
        {
            List<Body> bodies = new List<Body>();
            List<Fixture> fixtures = new List<Fixture>();
            List<Shape> shapes = new List<Shape>();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.OmitXmlDeclaration = true;

            using (_writer = XmlWriter.Create(stream, settings))
            {
                _writer.WriteStartElement("World");
                _writer.WriteAttributeString("Version", "3");
                WriteElement("Gravity", world.Gravity);

                _writer.WriteStartElement("Shapes");

                foreach (Body body in world.BodyList)
                {
                    foreach (Fixture fixture in body.FixtureList)
                    {
                        if (!shapes.Any(s2 => fixture.Shape.CompareTo(s2)))
                        {
                            SerializeShape(fixture.Shape);
                            shapes.Add(fixture.Shape);
                        }
                    }
                }

                _writer.WriteEndElement();
                _writer.WriteStartElement("Fixtures");

                foreach (Body body in world.BodyList)
                {
                    foreach (Fixture fixture in body.FixtureList)
                    {
                        if (!fixtures.Any(f2 => fixture.CompareTo(f2)))
                        {
                            SerializeFixture(fixture);
                            fixtures.Add(fixture);
                        }
                    }
                }

                _writer.WriteEndElement();
                _writer.WriteStartElement("Bodies");

                foreach (Body body in world.BodyList)
                {
                    bodies.Add(body);
                    SerializeBody(fixtures, shapes, body);
                }

                _writer.WriteEndElement();
                _writer.WriteStartElement("Joints");

                foreach (Joint joint in world.JointList)
                {
                    SerializeJoint(bodies, joint);
                }

                _writer.WriteEndElement();
                _writer.WriteEndElement();
            }
        }
    }
}