using System;
using System.Globalization;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using FarseerPhysics.Common;

namespace FarseerPhysics.ContentPipeline
{
  class SVGPathParser
  {
    private const string isNumber = @"\A[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
    private const string isCommaWhitespace = @"\A[\s,]*";

    private int _iterations;

    public SVGPathParser(int bezierIterations)
    {
      _iterations = bezierIterations;
    }

    public List<Polygon> ParseSVGPath(string path, Matrix transformation)
    {
      List<Polygon> result = new List<Polygon>();
      Vertices currentPath = null;
      Vector2 currentPosition = Vector2.Zero;

      char command = '0';
      string item;

      int argumentsPerCommand = 0;
      int argumentCount = 0;

      float[] arguments = new float[6];

      while (!string.IsNullOrEmpty(path))
      {
        path = getNextPathItem(path, out item);
        if (string.IsNullOrEmpty(item))
        {
          break;
        }

        if (Regex.IsMatch(item, isNumber))
        {
          if (argumentCount < 6)
          {
            arguments[argumentCount++] = float.Parse(item, CultureInfo.InvariantCulture);
          }
          if (argumentCount >= argumentsPerCommand)
          {
            switch (command)
            {
              case 'm':
              case 'l':
                {
                  currentPosition.X += arguments[0];
                  currentPosition.Y += arguments[1];
                  currentPath.Add(currentPosition);
                }
                break;
              case 'M':
              case 'L':
                {
                  currentPosition.X = arguments[0];
                  currentPosition.Y = arguments[1];
                  currentPath.Add(currentPosition);
                }
                break;
              case 'h':
                {
                  currentPosition.X += arguments[0];
                  currentPath.Add(currentPosition);
                }
                break;
              case 'H':
                {
                  currentPosition.X = arguments[0];
                  currentPath.Add(currentPosition);
                }
                break;
              case 'v':
                {
                  currentPosition.Y += arguments[0];
                  currentPath.Add(currentPosition);
                }
                break;
              case 'V':
                {
                  currentPosition.Y = arguments[0];
                  currentPath.Add(currentPosition);
                }
                break;
              case 'c':
                {
                  Vector2 end = currentPosition + new Vector2(arguments[4], arguments[5]);
                  cubicBezierRecursive(currentPath, currentPosition, end, currentPosition + new Vector2(arguments[0], arguments[1]), currentPosition + new Vector2(arguments[2], arguments[3]));
                  //cubicBezier(currentPath, currentPosition, end, currentPosition + new Vector2(arguments[0], arguments[1]), currentPosition + new Vector2(arguments[2], arguments[3]), (int)Math.Pow(2.0, BezierIterations));
                  currentPosition = end;
                  currentPath.Add(currentPosition);
                }
                break;
              case 'C':
                {
                  Vector2 end = new Vector2(arguments[4], arguments[5]);
                  cubicBezierRecursive(currentPath, currentPosition, end, new Vector2(arguments[0], arguments[1]), new Vector2(arguments[2], arguments[3]));
                  //cubicBezier(currentPath, currentPosition, end, new Vector2(arguments[0], arguments[1]), new Vector2(arguments[2], arguments[3]), (int)Math.Pow(2.0, BezierIterations));
                  currentPosition = end;
                  currentPath.Add(currentPosition);
                }
                break;
              case 's':
              case 'S':
              case 't':
              case 'T':
              case 'q':
              case 'Q':
              case 'a':
              case 'A':
                {
                  throw new Exception("Path command '" + command + "' is not supported.");
                }
            }
            argumentCount = 0;
          }
        }
        else
        {
          command = item[0];
          switch (char.ToLower(command))
          {
            case 'v':
            case 'h':
              argumentsPerCommand = 1;
              break;
            case 'm':
            case 'l':
              argumentsPerCommand = 2;
              break;
            case 'c':
              argumentsPerCommand = 6;
              break;
            default:
              argumentsPerCommand = 0;
              break;
          }
          if (command == 'M' || command == 'm')
          {
            if (currentPath != null && currentPath.Count > 1)
            {
              result.Add(new Polygon(currentPath, false));
            }
            currentPath = new Vertices();
            argumentCount = 0;
            if (command == 'M')
            {
              currentPosition = Vector2.Zero;
            }
          }
          else if (command == 'Z' || command == 'z')
          {
            if (currentPath != null && currentPath.Count > 1)
            {
              result.Add(new Polygon(currentPath, true));
            }
            currentPath = null;
            argumentCount = 0;
          }
        }
      }
      if (currentPath != null && currentPath.Count > 1)
      {
        result.Add(new Polygon(currentPath, false));
      }

      foreach (Polygon poly in result)
      {
        for (int i = 0; i < poly.vertices.Count; i++)
        {
          poly.vertices[i] = Vector2.Transform(poly.vertices[i], transformation);
        }
      }
      return result;
    }

    private string getNextPathItem(string input, out string item)
    {
      item = "";
      string output = Regex.Replace(input, isCommaWhitespace, "");
      if (!string.IsNullOrEmpty(output))
      {
        if (Regex.IsMatch(output, isNumber))
        {
          int matchLength = Regex.Match(output, isNumber).Length;
          item = output.Substring(0, matchLength);
          output = output.Remove(0, matchLength);
        }
        else
        {
          item = output[0].ToString();
          output = output.Remove(0, 1);
        }
      }
      return output;
    }

    private void cubicBezierRecursive(Vertices path, Vector2 start, Vector2 end, Vector2 controlStart, Vector2 controlEnd, int level = 0)
    {
      if (level >= _iterations)
      {
        return;
      }

      Vector2 s_cS = (start + controlStart) / 2f;
      Vector2 e_cE = (end + controlEnd) / 2f;
      Vector2 cS_cE = (controlStart + controlEnd) / 2f;

      Vector2 s_cS_cS_cE = (s_cS + cS_cE) / 2f;
      Vector2 e_cE_cS_cE = (e_cE + cS_cE) / 2f;

      Vector2 curvePoint = (s_cS_cS_cE + e_cE_cS_cE) / 2f;

      cubicBezierRecursive(path, start, curvePoint, s_cS, s_cS_cS_cE, level + 1);
      path.Add(curvePoint);
      cubicBezierRecursive(path, curvePoint, end, e_cE_cS_cE, e_cE, level + 1);
    }

    private void cubicBezier(Vertices path, Vector2 start, Vector2 end, Vector2 controlStart, Vector2 controlEnd, int steps)
    {
      Vector2 s_cS;
      Vector2 e_cE;
      Vector2 cS_cE;
      Vector2 s_cS_cS_cE;
      Vector2 e_cE_cS_cE;
      Vector2 curvePoint;

      for (int i = 0; i < steps; i++)
      {
        float t = (float)i / (float)steps;
        s_cS = Vector2.Lerp(start, controlStart, t);
        cS_cE = Vector2.Lerp(controlStart, controlEnd, t);
        e_cE = Vector2.Lerp(controlEnd, end, t);
        s_cS_cS_cE = Vector2.Lerp(s_cS, cS_cE, t);
        e_cE_cS_cE = Vector2.Lerp(cS_cE, e_cE, t);
        curvePoint = Vector2.Lerp(s_cS_cS_cE, e_cE_cS_cE, t);
        path.Add(curvePoint);
      }
    }
  }
}
