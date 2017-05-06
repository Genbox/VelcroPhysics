using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Joints;
using VelcroPhysics.Shared;

namespace VelcroPhysics.Tools.Serialization.XML
{
    internal static class WorldXmlDeserializer
    {
        internal static World Deserialize(Stream stream)
        {
            World world = new World(Vector2.Zero);
            Deserialize(world, stream);
            return world;
        }

        private static void Deserialize(World world, Stream stream)
        {
            List<Body> bodies = new List<Body>();
            List<Fixture> fixtures = new List<Fixture>();
            List<Joint> joints = new List<Joint>();
            List<Shape> shapes = new List<Shape>();

            XMLFragmentElement root = XMLFragmentParser.LoadFromStream(stream);

            if (root.Name.ToLower() != "world")
                throw new Exception();

            //Read gravity
            foreach (XMLFragmentElement element in root.Elements)
            {
                if (element.Name.ToLower() == "gravity")
                {
                    world.Gravity = ReadVector(element);
                    break;
                }
            }

            //Read shapes
            foreach (XMLFragmentElement shapeElement in root.Elements)
            {
                if (shapeElement.Name.ToLower() == "shapes")
                {
                    foreach (XMLFragmentElement element in shapeElement.Elements)
                    {
                        if (element.Name.ToLower() != "shape")
                            throw new Exception();

                        ShapeType type = (ShapeType)Enum.Parse(typeof(ShapeType), element.Attributes[0].Value, true);
                        float density = float.Parse(element.Attributes[1].Value);

                        switch (type)
                        {
                            case ShapeType.Circle:
                                {
                                    CircleShape shape = new CircleShape();
                                    shape._density = density;

                                    foreach (XMLFragmentElement sn in element.Elements)
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "radius":
                                                shape.Radius = float.Parse(sn.Value);
                                                break;
                                            case "position":
                                                shape.Position = ReadVector(sn);
                                                break;
                                            default:
                                                throw new Exception();
                                        }
                                    }

                                    shapes.Add(shape);
                                }
                                break;
                            case ShapeType.Polygon:
                                {
                                    PolygonShape shape = new PolygonShape();
                                    shape._density = density;

                                    foreach (XMLFragmentElement sn in element.Elements)
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "vertices":
                                                {
                                                    List<Vector2> verts = new List<Vector2>(sn.Elements.Count);

                                                    foreach (XMLFragmentElement vert in sn.Elements)
                                                        verts.Add(ReadVector(vert));

                                                    shape.Vertices = new Vertices(verts);
                                                }
                                                break;
                                            case "centroid":
                                                shape.MassData.Centroid = ReadVector(sn);
                                                break;
                                        }
                                    }

                                    shapes.Add(shape);
                                }
                                break;
                            case ShapeType.Edge:
                                {
                                    EdgeShape shape = new EdgeShape();
                                    shape._density = density;

                                    foreach (XMLFragmentElement sn in element.Elements)
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "hasvertex0":
                                                shape.HasVertex0 = bool.Parse(sn.Value);
                                                break;
                                            case "hasvertex3":
                                                shape.HasVertex0 = bool.Parse(sn.Value);
                                                break;
                                            case "vertex0":
                                                shape.Vertex0 = ReadVector(sn);
                                                break;
                                            case "vertex1":
                                                shape.Vertex1 = ReadVector(sn);
                                                break;
                                            case "vertex2":
                                                shape.Vertex2 = ReadVector(sn);
                                                break;
                                            case "vertex3":
                                                shape.Vertex3 = ReadVector(sn);
                                                break;
                                            default:
                                                throw new Exception();
                                        }
                                    }
                                    shapes.Add(shape);
                                }
                                break;
                            case ShapeType.Chain:
                                {
                                    ChainShape shape = new ChainShape();
                                    shape._density = density;

                                    foreach (XMLFragmentElement sn in element.Elements)
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "vertices":
                                                {
                                                    List<Vector2> verts = new List<Vector2>(sn.Elements.Count);

                                                    foreach (XMLFragmentElement vert in sn.Elements)
                                                        verts.Add(ReadVector(vert));

                                                    shape.Vertices = new Vertices(verts);
                                                }
                                                break;
                                            case "nextvertex":
                                                shape.NextVertex = ReadVector(sn);
                                                break;
                                            case "prevvertex":
                                                shape.PrevVertex = ReadVector(sn);
                                                break;

                                            default:
                                                throw new Exception();
                                        }
                                    }
                                    shapes.Add(shape);
                                }
                                break;
                        }
                    }
                }
            }

            //Read fixtures
            foreach (XMLFragmentElement fixtureElement in root.Elements)
            {
                if (fixtureElement.Name.ToLower() == "fixtures")
                {
                    foreach (XMLFragmentElement element in fixtureElement.Elements)
                    {
                        Fixture fixture = new Fixture();

                        if (element.Name.ToLower() != "fixture")
                            throw new Exception();

                        fixture.FixtureId = int.Parse(element.Attributes[0].Value);

                        foreach (XMLFragmentElement sn in element.Elements)
                        {
                            switch (sn.Name.ToLower())
                            {
                                case "filterdata":
                                    foreach (XMLFragmentElement ssn in sn.Elements)
                                    {
                                        switch (ssn.Name.ToLower())
                                        {
                                            case "categorybits":
                                                fixture._collisionCategories = (Category)int.Parse(ssn.Value);
                                                break;
                                            case "maskbits":
                                                fixture._collidesWith = (Category)int.Parse(ssn.Value);
                                                break;
                                            case "groupindex":
                                                fixture._collisionGroup = short.Parse(ssn.Value);
                                                break;
                                            case "CollisionIgnores":
                                                string[] split = ssn.Value.Split('|');
                                                foreach (string s in split)
                                                {
                                                    fixture._collisionIgnores.Add(int.Parse(s));
                                                }
                                                break;
                                        }
                                    }

                                    break;
                                case "friction":
                                    fixture.Friction = float.Parse(sn.Value);
                                    break;
                                case "issensor":
                                    fixture.IsSensor = bool.Parse(sn.Value);
                                    break;
                                case "restitution":
                                    fixture.Restitution = float.Parse(sn.Value);
                                    break;
                                case "userdata":
                                    fixture.UserData = ReadSimpleType(sn, null, false);
                                    break;
                            }
                        }

                        fixtures.Add(fixture);
                    }
                }
            }

            //Read bodies
            foreach (XMLFragmentElement bodyElement in root.Elements)
            {
                if (bodyElement.Name.ToLower() == "bodies")
                {
                    foreach (XMLFragmentElement element in bodyElement.Elements)
                    {
                        Body body = new Body(world);

                        if (element.Name.ToLower() != "body")
                            throw new Exception();

                        body.BodyType = (BodyType)Enum.Parse(typeof(BodyType), element.Attributes[0].Value, true);

                        foreach (XMLFragmentElement sn in element.Elements)
                        {
                            switch (sn.Name.ToLower())
                            {
                                case "active":
                                    bool enabled = bool.Parse(sn.Value);
                                    if (enabled)
                                        body._flags |= BodyFlags.Enabled;
                                    else
                                        body._flags &= ~BodyFlags.Enabled;
                                    break;
                                case "allowsleep":
                                    body.SleepingAllowed = bool.Parse(sn.Value);
                                    break;
                                case "angle":
                                    {
                                        Vector2 position = body.Position;
                                        body.SetTransformIgnoreContacts(ref position, float.Parse(sn.Value));
                                    }
                                    break;
                                case "angulardamping":
                                    body.AngularDamping = float.Parse(sn.Value);
                                    break;
                                case "angularvelocity":
                                    body.AngularVelocity = float.Parse(sn.Value);
                                    break;
                                case "awake":
                                    body.Awake = bool.Parse(sn.Value);
                                    break;
                                case "bullet":
                                    body.IsBullet = bool.Parse(sn.Value);
                                    break;
                                case "fixedrotation":
                                    body.FixedRotation = bool.Parse(sn.Value);
                                    break;
                                case "lineardamping":
                                    body.LinearDamping = float.Parse(sn.Value);
                                    break;
                                case "linearvelocity":
                                    body.LinearVelocity = ReadVector(sn);
                                    break;
                                case "position":
                                    {
                                        float rotation = body.Rotation;
                                        Vector2 position = ReadVector(sn);
                                        body.SetTransformIgnoreContacts(ref position, rotation);
                                    }
                                    break;
                                case "userdata":
                                    body.UserData = ReadSimpleType(sn, null, false);
                                    break;
                                case "bindings":
                                    {
                                        foreach (XMLFragmentElement pair in sn.Elements)
                                        {
                                            Fixture fix = fixtures[int.Parse(pair.Attributes[0].Value)];
                                            fix.Shape = shapes[int.Parse(pair.Attributes[1].Value)].Clone();
                                            fix.CloneOnto(body);
                                        }
                                        break;
                                    }
                            }
                        }

                        bodies.Add(body);
                    }
                }
            }

            //Read joints
            foreach (XMLFragmentElement jointElement in root.Elements)
            {
                if (jointElement.Name.ToLower() == "joints")
                {
                    foreach (XMLFragmentElement n in jointElement.Elements)
                    {
                        Joint joint;

                        if (n.Name.ToLower() != "joint")
                            throw new Exception();

                        JointType type = (JointType)Enum.Parse(typeof(JointType), n.Attributes[0].Value, true);

                        int bodyAIndex = -1, bodyBIndex = -1;
                        bool collideConnected = false;
                        object userData = null;

                        foreach (XMLFragmentElement sn in n.Elements)
                        {
                            switch (sn.Name.ToLower())
                            {
                                case "bodya":
                                    bodyAIndex = int.Parse(sn.Value);
                                    break;
                                case "bodyb":
                                    bodyBIndex = int.Parse(sn.Value);
                                    break;
                                case "collideconnected":
                                    collideConnected = bool.Parse(sn.Value);
                                    break;
                                case "userdata":
                                    userData = ReadSimpleType(sn, null, false);
                                    break;
                            }
                        }

                        Body bodyA = bodies[bodyAIndex];
                        Body bodyB = bodies[bodyBIndex];

                        switch (type)
                        {
                            //case JointType.FixedMouse:
                            //    joint = new FixedMouseJoint();
                            //    break;
                            //case JointType.FixedRevolute:
                            //    break;
                            //case JointType.FixedDistance:
                            //    break;
                            //case JointType.FixedLine:
                            //    break;
                            //case JointType.FixedPrismatic:
                            //    break;
                            //case JointType.FixedAngle:
                            //    break;
                            //case JointType.FixedFriction:
                            //    break;
                            case JointType.Distance:
                                joint = new DistanceJoint();
                                break;
                            case JointType.Friction:
                                joint = new FrictionJoint();
                                break;
                            case JointType.Wheel:
                                joint = new WheelJoint();
                                break;
                            case JointType.Prismatic:
                                joint = new PrismaticJoint();
                                break;
                            case JointType.Pulley:
                                joint = new PulleyJoint();
                                break;
                            case JointType.Revolute:
                                joint = new RevoluteJoint();
                                break;
                            case JointType.Weld:
                                joint = new WeldJoint();
                                break;
                            case JointType.Rope:
                                joint = new RopeJoint();
                                break;
                            case JointType.Angle:
                                joint = new AngleJoint();
                                break;
                            case JointType.Motor:
                                joint = new MotorJoint();
                                break;
                            case JointType.Gear:
                                throw new Exception("GearJoint is not supported.");
                            default:
                                throw new Exception("Invalid or unsupported joint.");
                        }

                        joint.CollideConnected = collideConnected;
                        joint.UserData = userData;
                        joint.BodyA = bodyA;
                        joint.BodyB = bodyB;
                        joints.Add(joint);
                        world.AddJoint(joint);

                        foreach (XMLFragmentElement sn in n.Elements)
                        {
                            // check for specific nodes
                            switch (type)
                            {
                                case JointType.Distance:
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "dampingratio":
                                                ((DistanceJoint)joint).DampingRatio = float.Parse(sn.Value);
                                                break;
                                            case "frequencyhz":
                                                ((DistanceJoint)joint).Frequency = float.Parse(sn.Value);
                                                break;
                                            case "length":
                                                ((DistanceJoint)joint).Length = float.Parse(sn.Value);
                                                break;
                                            case "localanchora":
                                                ((DistanceJoint)joint).LocalAnchorA = ReadVector(sn);
                                                break;
                                            case "localanchorb":
                                                ((DistanceJoint)joint).LocalAnchorB = ReadVector(sn);
                                                break;
                                        }
                                    }
                                    break;
                                case JointType.Friction:
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "localanchora":
                                                ((FrictionJoint)joint).LocalAnchorA = ReadVector(sn);
                                                break;
                                            case "localanchorb":
                                                ((FrictionJoint)joint).LocalAnchorB = ReadVector(sn);
                                                break;
                                            case "maxforce":
                                                ((FrictionJoint)joint).MaxForce = float.Parse(sn.Value);
                                                break;
                                            case "maxtorque":
                                                ((FrictionJoint)joint).MaxTorque = float.Parse(sn.Value);
                                                break;
                                        }
                                    }
                                    break;
                                case JointType.Wheel:
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "enablemotor":
                                                ((WheelJoint)joint).MotorEnabled = bool.Parse(sn.Value);
                                                break;
                                            case "localanchora":
                                                ((WheelJoint)joint).LocalAnchorA = ReadVector(sn);
                                                break;
                                            case "localanchorb":
                                                ((WheelJoint)joint).LocalAnchorB = ReadVector(sn);
                                                break;
                                            case "motorspeed":
                                                ((WheelJoint)joint).MotorSpeed = float.Parse(sn.Value);
                                                break;
                                            case "dampingratio":
                                                ((WheelJoint)joint).DampingRatio = float.Parse(sn.Value);
                                                break;
                                            case "maxmotortorque":
                                                ((WheelJoint)joint).MaxMotorTorque = float.Parse(sn.Value);
                                                break;
                                            case "frequencyhz":
                                                ((WheelJoint)joint).Frequency = float.Parse(sn.Value);
                                                break;
                                            case "axis":
                                                ((WheelJoint)joint).Axis = ReadVector(sn);
                                                break;
                                        }
                                    }
                                    break;
                                case JointType.Prismatic:
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "enablelimit":
                                                ((PrismaticJoint)joint).LimitEnabled = bool.Parse(sn.Value);
                                                break;
                                            case "enablemotor":
                                                ((PrismaticJoint)joint).MotorEnabled = bool.Parse(sn.Value);
                                                break;
                                            case "localanchora":
                                                ((PrismaticJoint)joint).LocalAnchorA = ReadVector(sn);
                                                break;
                                            case "localanchorb":
                                                ((PrismaticJoint)joint).LocalAnchorB = ReadVector(sn);
                                                break;
                                            case "axis":
                                                ((PrismaticJoint)joint).Axis = ReadVector(sn);
                                                break;
                                            case "maxmotorforce":
                                                ((PrismaticJoint)joint).MaxMotorForce = float.Parse(sn.Value);
                                                break;
                                            case "motorspeed":
                                                ((PrismaticJoint)joint).MotorSpeed = float.Parse(sn.Value);
                                                break;
                                            case "lowertranslation":
                                                ((PrismaticJoint)joint).LowerLimit = float.Parse(sn.Value);
                                                break;
                                            case "uppertranslation":
                                                ((PrismaticJoint)joint).UpperLimit = float.Parse(sn.Value);
                                                break;
                                            case "referenceangle":
                                                ((PrismaticJoint)joint).ReferenceAngle = float.Parse(sn.Value);
                                                break;
                                        }
                                    }
                                    break;
                                case JointType.Pulley:
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "worldanchora":
                                                ((PulleyJoint)joint).WorldAnchorA = ReadVector(sn);
                                                break;
                                            case "worldanchorb":
                                                ((PulleyJoint)joint).WorldAnchorB = ReadVector(sn);
                                                break;
                                            case "lengtha":
                                                ((PulleyJoint)joint).LengthA = float.Parse(sn.Value);
                                                break;
                                            case "lengthb":
                                                ((PulleyJoint)joint).LengthB = float.Parse(sn.Value);
                                                break;
                                            case "localanchora":
                                                ((PulleyJoint)joint).LocalAnchorA = ReadVector(sn);
                                                break;
                                            case "localanchorb":
                                                ((PulleyJoint)joint).LocalAnchorB = ReadVector(sn);
                                                break;
                                            case "ratio":
                                                ((PulleyJoint)joint).Ratio = float.Parse(sn.Value);
                                                break;
                                            case "constant":
                                                ((PulleyJoint)joint).Constant = float.Parse(sn.Value);
                                                break;
                                        }
                                    }
                                    break;
                                case JointType.Revolute:
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "enablelimit":
                                                ((RevoluteJoint)joint).LimitEnabled = bool.Parse(sn.Value);
                                                break;
                                            case "enablemotor":
                                                ((RevoluteJoint)joint).MotorEnabled = bool.Parse(sn.Value);
                                                break;
                                            case "localanchora":
                                                ((RevoluteJoint)joint).LocalAnchorA = ReadVector(sn);
                                                break;
                                            case "localanchorb":
                                                ((RevoluteJoint)joint).LocalAnchorB = ReadVector(sn);
                                                break;
                                            case "maxmotortorque":
                                                ((RevoluteJoint)joint).MaxMotorTorque = float.Parse(sn.Value);
                                                break;
                                            case "motorspeed":
                                                ((RevoluteJoint)joint).MotorSpeed = float.Parse(sn.Value);
                                                break;
                                            case "lowerangle":
                                                ((RevoluteJoint)joint).LowerLimit = float.Parse(sn.Value);
                                                break;
                                            case "upperangle":
                                                ((RevoluteJoint)joint).UpperLimit = float.Parse(sn.Value);
                                                break;
                                            case "referenceangle":
                                                ((RevoluteJoint)joint).ReferenceAngle = float.Parse(sn.Value);
                                                break;
                                        }
                                    }
                                    break;
                                case JointType.Weld:
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "localanchora":
                                                ((WeldJoint)joint).LocalAnchorA = ReadVector(sn);
                                                break;
                                            case "localanchorb":
                                                ((WeldJoint)joint).LocalAnchorB = ReadVector(sn);
                                                break;
                                        }
                                    }
                                    break;
                                case JointType.Rope:
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "localanchora":
                                                ((RopeJoint)joint).LocalAnchorA = ReadVector(sn);
                                                break;
                                            case "localanchorb":
                                                ((RopeJoint)joint).LocalAnchorB = ReadVector(sn);
                                                break;
                                            case "maxlength":
                                                ((RopeJoint)joint).MaxLength = float.Parse(sn.Value);
                                                break;
                                        }
                                    }
                                    break;
                                case JointType.Gear:
                                    throw new Exception("Gear joint is unsupported");
                                case JointType.Angle:
                                    {
                                        switch (sn.Name.ToLower())
                                        {
                                            case "biasfactor":
                                                ((AngleJoint)joint).BiasFactor = float.Parse(sn.Value);
                                                break;
                                            case "maximpulse":
                                                ((AngleJoint)joint).MaxImpulse = float.Parse(sn.Value);
                                                break;
                                            case "softness":
                                                ((AngleJoint)joint).Softness = float.Parse(sn.Value);
                                                break;
                                            case "targetangle":
                                                ((AngleJoint)joint).TargetAngle = float.Parse(sn.Value);
                                                break;
                                        }
                                    }
                                    break;
                                case JointType.Motor:
                                    switch (sn.Name.ToLower())
                                    {
                                        case "angularoffset":
                                            ((MotorJoint)joint).AngularOffset = float.Parse(sn.Value);
                                            break;
                                        case "linearoffset":
                                            ((MotorJoint)joint).LinearOffset = ReadVector(sn);
                                            break;
                                        case "maxforce":
                                            ((MotorJoint)joint).MaxForce = float.Parse(sn.Value);
                                            break;
                                        case "maxtorque":
                                            ((MotorJoint)joint).MaxTorque = float.Parse(sn.Value);
                                            break;
                                        case "correctionfactor":
                                            ((MotorJoint)joint).CorrectionFactor = float.Parse(sn.Value);
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            world.ProcessChanges();
        }

        private static Vector2 ReadVector(XMLFragmentElement node)
        {
            string[] values = node.Value.Split(' ');
            return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
        }

        private static object ReadSimpleType(XMLFragmentElement node, Type type, bool outer)
        {
            if (type == null)
                return ReadSimpleType(node.Elements[1], Type.GetType(node.Elements[0].Value), outer);

            XmlSerializer serializer = new XmlSerializer(type);
            XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces();
            xmlnsEmpty.Add("", "");

            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                {
                    writer.Write((string)((outer) ? node.OuterXml : node.InnerXml));
                    writer.Flush();
                    stream.Position = 0;
                }
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;

                return serializer.Deserialize(XmlReader.Create(stream, settings));
            }
        }
    }
}