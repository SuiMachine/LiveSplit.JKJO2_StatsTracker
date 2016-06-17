﻿using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Diagnostics;

namespace LiveSplit.MemoryGraph
{
    public class Component : IComponent
    {
        private Settings settings;

        public string ComponentName => "JKJO2 Stats Tracker";

        public float PaddingTop => 0;
        public float PaddingLeft => 0;
        public float PaddingBottom => 0;
        public float PaddingRight => 0;

        public float VerticalHeight => settings.GraphHeight + 4;
        public float MinimumWidth => 180;
        public float HorizontalWidth => settings.GraphWidth + 4;
        public float MinimumHeight => settings.GraphHeight + 4;

        public IDictionary<string, Action> ContextMenuControls => null;
        bool firstLoad = true;

        private int graphHeight;
        private int graphWidth;

        private int v_SecretsFound;
        private int v_maxSecrets;
        private int v_shotsFired;
        private int v_shotsHit;
        private int v_enemiesKilled;

        private int numberofDisplayedElements = 0;
        private int field1DisplayMode = 0;
        private int field2DisplayMode = 0;
        private int field3DisplayMode = 0;
        private int field4DisplayMode = 0;
        private int field5DisplayMode = 0;
        private bool field1Enabled = true;
        private bool field2Enabled = false;
        private bool field3Enabled = false;
        private bool field4Enabled = false;
        private bool field5Enabled = false;
        private bool overrideTextColorEnabled = false;

        private Color OverrideTextColor;

        private System.Diagnostics.Process process;

        private Bitmap bmpBuffer;
        private Graphics gBuffer;

        private Brush BGBrush;
        private byte BGBrushOpacity;
        private Brush BackgroundColorCompleted;
        private byte BGCompletedOpacity;

        private bool CompletedColorEnabled = false;
        private Brush ComplitionColorIncomplete;
        private Brush ComplitionColorCompleted;

        private StringFormat valueTextFormat;
        private StringFormat descriptiveTextFormat;


        public Component(LiveSplitState state)
        {
            valueTextFormat = new StringFormat(StringFormatFlags.NoWrap);
            valueTextFormat.LineAlignment = StringAlignment.Center;
            valueTextFormat.Alignment = StringAlignment.Far;

            descriptiveTextFormat = new StringFormat(StringFormatFlags.NoWrap);
            descriptiveTextFormat.LineAlignment = StringAlignment.Center;
            descriptiveTextFormat.Alignment = StringAlignment.Near;

            settings = new Settings();
            settings.HandleDestroyed += SettingsUpdated;
            SettingsUpdated(null, null);
        }

        private void SettingsUpdated(object sender, EventArgs e)
        {
            CalculateSize();
            field1DisplayMode = settings.field1DisplayMode;
            field2DisplayMode = settings.field2DisplayMode;
            field3DisplayMode = settings.field3DisplayMode;
            field4DisplayMode = settings.field4DisplayMode;
            field5DisplayMode = settings.field5DisplayMode;
            field1Enabled = settings.field1Enabled;
            field2Enabled = settings.field2Enabled;
            field3Enabled = settings.field3Enabled;
            field4Enabled = settings.field4Enabled;
            field5Enabled = settings.field5Enabled;
            overrideTextColorEnabled = settings.fieldOverrideTextColor;
            CompletedColorEnabled = settings.fieldCompletionColorsEnabled;

            BackgroundColorCompleted = new SolidBrush(settings.BackgroundColorCompleted);
            BGCompletedOpacity = settings.BackgroundColorCompleted.A;
            OverrideTextColor = settings.OverrideTextColor;
            ComplitionColorIncomplete = new SolidBrush(settings.ComplitionColorIncomplete);
            ComplitionColorCompleted = new SolidBrush(settings.ComplitionColorCompleted);
            BGBrush = new SolidBrush(settings.BackgroundColor);
            BGBrushOpacity = settings.BackgroundColor.A;



            if (graphHeight != settings.GraphHeight || graphWidth != settings.GraphWidth)
            {
                graphHeight = settings.GraphHeight;
                graphWidth = settings.GraphWidth;

                bmpBuffer = new Bitmap(graphWidth, graphHeight);
                gBuffer = Graphics.FromImage(bmpBuffer);
                gBuffer.Clear(Color.Transparent);
                gBuffer.CompositingMode = CompositingMode.SourceCopy;
            }
        }

        private void CalculateSize()
        {
            numberofDisplayedElements = 0;
            int size = 0;
            if (settings.field1Enabled)
            {
                size += 24;
                numberofDisplayedElements++;
            }

            if (settings.field2Enabled)
            {
                size += 24;
                numberofDisplayedElements++;
            }

            if (settings.field3Enabled)
            {
                size += 24;
                numberofDisplayedElements++;
            }

            if (settings.field4Enabled)
            {
                size += 24;
                numberofDisplayedElements++;
            }

            if (settings.field5Enabled)
            {
                size += 24;
                numberofDisplayedElements++;
            }

            settings.GraphHeight = size;
        }

        private static Color Blend(Color backColor, Color color, double amount)
        {
            byte a = (byte)((color.A * amount) + backColor.A * (1 - amount));
            byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
            return Color.FromArgb(a, r, g, b);
        }

        public void DrawGraph(Graphics g, LiveSplitState state, float width, float height)
        {
            if (firstLoad)
            {
                SettingsUpdated(null, null);
                firstLoad = false;
            }
            // figure out where to draw the graph
            RectangleF graphRect = new RectangleF();
            graphRect.Y = (height - graphHeight) / 2;
            graphRect.Width = width;
            graphRect.Height = graphHeight;
            graphRect.X = 0;

            // draw descriptive text
            Font font = state.LayoutSettings.TextFont;
            Brush fontBrush;
            if (!overrideTextColorEnabled)
                fontBrush = new SolidBrush(state.LayoutSettings.TextColor);
            else
                fontBrush = new SolidBrush(OverrideTextColor);
            Brush seperatorBrush = new SolidBrush(state.LayoutSettings.ThinSeparatorsColor);
            RectangleF rect = graphRect;
            if (BGBrushOpacity != 0)
                g.FillRectangle(BGBrush, rect);
            rect.X += 5;
            rect.Width -= 10;
            float x = rect.X;
            float y = 2;
            float yDifference = rect.Height / numberofDisplayedElements;


            if (field1Enabled)
            {
                DrawElementInTracker(field1DisplayMode, g, font, fontBrush, x, y, rect.Width, yDifference);
                y += 24;
                g.FillRectangle(seperatorBrush, 0, y, width, 1);

            }
            if(field2Enabled)
            {
                DrawElementInTracker(field2DisplayMode, g, font, fontBrush, x, y, rect.Width, yDifference);
                y += 24;
                g.FillRectangle(seperatorBrush, 0, y, width, 1);
            }
            if (field3Enabled)
            {
                DrawElementInTracker(field3DisplayMode, g, font, fontBrush, x, y, rect.Width, yDifference);
                y += 24;
                g.FillRectangle(seperatorBrush, 0, y, width, 1);
            }
            if (field4Enabled)
            {
                DrawElementInTracker(field4DisplayMode, g, font, fontBrush, x, y, rect.Width, yDifference);
                y += 24;
                g.FillRectangle(seperatorBrush, 0, y, width, 1);
            }
            if (field5Enabled)
            {
                DrawElementInTracker(field5DisplayMode, g, font, fontBrush, x, y, rect.Width, yDifference);
                y += 24;
                g.FillRectangle(seperatorBrush, 0, y, width, 1);
            }

            // draw value text
        }

        private void DrawElementInTracker(int DisplayMode, Graphics g, Font font, Brush brush, float x, float y, float width, float height)
        {
            switch(DisplayMode)
            {
                case (int)elementType.Secrets:
                    if (BGCompletedOpacity != 0 && v_SecretsFound == v_maxSecrets)
                        g.FillRectangle(BackgroundColorCompleted, x, y, width, height);

                    g.DrawString("Secrets found:", font, brush, new RectangleF(x, y, width, height), descriptiveTextFormat);
                    if (CompletedColorEnabled)
                    {
                        if(v_SecretsFound != v_maxSecrets)
                            g.DrawString(v_SecretsFound.ToString() + " / " + v_maxSecrets.ToString(), font, ComplitionColorIncomplete, new RectangleF(x, y, width, height), valueTextFormat);
                        else
                            g.DrawString(v_SecretsFound.ToString() + " / " + v_maxSecrets.ToString(), font, ComplitionColorCompleted, new RectangleF(x, y, width, height), valueTextFormat);
                    }
                    else
                        g.DrawString(v_SecretsFound.ToString() + " / " + v_maxSecrets.ToString(), font, brush, new RectangleF(x, y, width, height), valueTextFormat);


                    break;
                case (int)elementType.Accuracy:
                    g.DrawString("Accuracy:", font, brush, new RectangleF(x, y, width, height), descriptiveTextFormat);
                    if(v_shotsFired > 0)
                        g.DrawString(((int)(v_shotsHit/(v_shotsFired*1.0f)*100)).ToString() + " %", font, brush, new RectangleF(x, y, width, height), valueTextFormat);
                    else
                        g.DrawString("∞", font, brush, new RectangleF(x, y, width, height), valueTextFormat);
                    break;
                case (int)elementType.EnemiesHit:
                    g.DrawString("Shots hit:", font, brush, new RectangleF(x, y, width, height), descriptiveTextFormat);
                    g.DrawString(v_shotsHit.ToString(), font, brush, new RectangleF(x, y, width, height), valueTextFormat);
                    break;
                case (int)elementType.EnemiesKilled:
                    g.DrawString("Enemies killed:", font, brush, new RectangleF(x, y, width, height), descriptiveTextFormat);
                    g.DrawString(v_enemiesKilled.ToString(), font, brush, new RectangleF(x, y, width, height), valueTextFormat);
                    break;
                case (int)elementType.ShotsFired:
                    g.DrawString("Shots fired:", font, brush, new RectangleF(x, y, width, height), descriptiveTextFormat);
                    g.DrawString(v_shotsFired.ToString(), font, brush, new RectangleF(x, y, width, height), valueTextFormat);
                    break;
                case (int)elementType.HitToFiredRatio:
                    g.DrawString("Hit / Fired:", font, brush, new RectangleF(x, y, width, height), descriptiveTextFormat);
                    g.DrawString(v_shotsHit + " / " + v_shotsFired, font, brush, new RectangleF(x, y, width, height), valueTextFormat);
                    break;
            }
        }

        private void DrawBackground(Graphics g, LiveSplitState state, float width, float height)
        {
            if (settings.BackgroundColor.A == 0 && settings.BackgroundColorCompleted.A == 0)
            {
                Brush gradientBrush = new SolidBrush(settings.BackgroundColor);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            DrawBackground(g, state, width, VerticalHeight);
            DrawGraph(g, state, width, VerticalHeight);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            DrawBackground(g, state, HorizontalWidth, height);
            DrawGraph(g, state, HorizontalWidth, height);
        }
        
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (process != null && settings.p_currentSecrets != null && !process.HasExited && 
                process.ProcessName == "jk2sp")
            {
                v_SecretsFound = settings.p_currentSecrets.Deref<int>(process);
                v_maxSecrets = settings.p_maxSecrets.Deref<int>(process);
                v_enemiesKilled = settings.p_levelKills.Deref<int>(process);
                v_shotsFired = settings.p_shotsFired.Deref<int>(process);
                v_shotsHit = settings.p_shotsHit.Deref<int>(process);

                if (invalidator != null)
                {
                    invalidator.Invalidate(0, 0, width, height);
                }
            }
            else
            {
                process = System.Diagnostics.Process.GetProcessesByName("jk2sp").FirstOrDefault();
            }            
        }

        public System.Windows.Forms.Control GetSettingsControl(LayoutMode mode)
        {
            return settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            this.settings.SetSettings(settings);
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return settings.GetSettings(document);
        }

        public int GetSettingsHashCode()
        {
            return settings.GetSettingsHashCode();
        }

        protected virtual void Dispose(bool disposing)
        {
            bmpBuffer.Dispose();
            valueTextFormat.Dispose();
            descriptiveTextFormat.Dispose();
            settings.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}