// SeparatorAttribute.cs
using System;
using UnityEditor;
using UnityEngine;

public class SeparatorAttribute : PropertyAttribute
{
    public float thickness;
    public float padding;
    public Color color;

    public SeparatorAttribute(float thickness = 1f, float padding = 30f, float r = 0.6f, float g = 0.6f, float b = 0.6f)
    {
        this.thickness = thickness;
        this.padding = padding;
        this.color = new Color(r, g, b);
    }
}


