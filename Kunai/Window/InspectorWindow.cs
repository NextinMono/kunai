﻿using Kunai.ShurikenRenderer;
using Hexa.NET.ImGui;
using SharpNeedle.Ninja.Csd;
using Shuriken.Rendering;
using Sprite = Shuriken.Rendering.Sprite;
using Hexa.NET.Utilities.Text;

namespace Kunai.Window
{
    public class InspectorWindow : WindowBase
    {
        public enum ESelectionType
        {
            None,
            Scene,
            Node,
            Cast
        }
        public static ESelectionType eSelectionType;

        static List<string> fontNames = new List<string>();
        public static void SelectCast(Cast in_Cast)
        {
            ShurikenRenderHelper.Instance.selectionData.SelectedCast = in_Cast;
            eSelectionType = ESelectionType.Cast;
        }
        public static void SelectScene(KeyValuePair<string, Scene> in_Cast)
        {
            ShurikenRenderHelper.Instance.selectionData.SelectedScene = in_Cast;
            eSelectionType = ESelectionType.Scene;
        }
        static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
        public void DrawSceneInspector()
        {
            var selectedScene = ShurikenRenderHelper.Instance.selectionData.SelectedScene;
            if (selectedScene.Value == null)
                return;
            ImGui.SeparatorText("Scene");
            string name = selectedScene.Key;
            var vers = selectedScene.Value.Version;
            var priority = (int)selectedScene.Value.Priority;
            var aspectRatio = selectedScene.Value.AspectRatio;
            var fps = selectedScene.Value.FrameRate;

            //Show aspect ratio as (width:height) by using greatest common divisor
            int width = 1280;
            int height = (int)(1280 / aspectRatio);
            double decimalRatio = (double)width / height;
            int gcdValue = GCD(width, height);
            int simplifiedWidth = width / gcdValue;
            int simplifiedHeight = height / gcdValue;

            ImGui.InputText("Name", ref name, 256);
            ImGui.InputFloat("Framerate", ref fps);
            ImGui.InputInt("Version", ref vers);
            ImGui.InputInt("Priority", ref priority);
            ImGui.InputFloat("Aspect Ratio", ref aspectRatio);
            ImGui.SameLine();
            ImGui.Text($"({simplifiedWidth}:{simplifiedHeight})");
        }
        public bool EmptyTextureButton(int id)
        {
            bool val = ImGui.Button($"##pattern{id}", new System.Numerics.Vector2(55, 55));
            ImGui.SameLine();
            return val;
        }
        public void DrawCastInspector()
        {
            var selectedCast = ShurikenRenderHelper.Instance.selectionData.SelectedCast;
            if (selectedCast == null)
                return;
            ImGui.SeparatorText("Cast");
            string[] typeStrings = { "No Draw", "Sprite", "Font" };

            var materialFlags = (ElementMaterialFlags)selectedCast.Field38;
            var info = selectedCast.Info;
            var inheritanceFlags = (ElementInheritanceFlags)selectedCast.InheritanceFlags.Value;


            string name = selectedCast.Name;
            int field00 = (int)selectedCast.Field00;
            var type = (int)selectedCast.Field04;
            var enabled = selectedCast.Enabled;
            var hideflag = info.HideFlag == 1;
            var scale = info.Scale;
            var mirrorX = materialFlags.HasFlag(ElementMaterialFlags.MirrorX);
            var mirrorY = materialFlags.HasFlag(ElementMaterialFlags.MirrorY);
            var rectSize = new System.Numerics.Vector2((int)selectedCast.Width, (int)selectedCast.Height);
            var topLeftVert = selectedCast.TopLeft;
            var topRightVert = selectedCast.TopRight;
            var bottomLeftVert = selectedCast.BottomLeft;
            var bottomRightVert = selectedCast.BottomRight;
            var rotation = info.Rotation;
            var origin = selectedCast.Origin;
            var translation = info.Translation;
            var color = info.Color.ToVec4();
            var colorTL = info.GradientTopLeft.ToVec4();
            var colorTR = info.GradientTopRight.ToVec4();
            var colorBL = info.GradientBottomLeft.ToVec4();
            var colorBR = info.GradientBottomRight.ToVec4();

            bool inheritPosX = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritXPosition);
            bool inheritPosY = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritYPosition);
            bool inheritRot = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritRotation);
            bool inheritCol = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritColor);
            bool inheritScaleX = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleX);
            bool inheritScaleY = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleY);
            int spriteIndex = (int)info.SpriteIndex;
            var text = selectedCast.Text;
            var kerning = -BitConverter.ToSingle(BitConverter.GetBytes(selectedCast.Field4C));
            var fontname = selectedCast.FontName;

            int indexFont = fontNames.IndexOf(fontname);

            ImGui.InputText("Room Name", ref name, 100, ImGuiInputTextFlags.AutoSelectAll);
            ImGui.InputInt("Field00", ref field00);
            ImGui.Combo("Type", ref type, typeStrings, 3);
            ImGui.SeparatorText("Status");
            ImGui.Checkbox("Enabled", ref enabled);
            ImGui.SameLine();
            ImGui.Checkbox("Hidden", ref hideflag);

            if (ImGui.CollapsingHeader("Dimensions"))
            {
                ImGui.Text("insert combo here");
                ImGui.SameLine();

                ImGui.BeginGroup();
                ImGui.SeparatorText("Invert UV");
                ImGui.PushID("mirrorH");
                ImGui.Checkbox("H", ref mirrorX);
                ImGui.SetItemTooltip("Mirror the cast horizontally.");
                ImGui.PopID();
                ImGui.SameLine();
                ImGui.Checkbox("V", ref mirrorY);
                ImGui.SetItemTooltip("Mirror the cast vertically.");
                ImGui.EndGroup();

                ImGui.BeginGroup();
                ImGui.SeparatorText("Rect Size");
                ImGui.InputFloat2("Quad Size", ref rectSize);
                ImGui.SetItemTooltip("This does not change any value in the tool, this is a leftover from the CellSprite Editor.");
                ImGui.EndGroup();

                ImGui.SeparatorText("Vertices");
                ImGui.SetItemTooltip("These 4 values determine the 4 points that generate the quad (3D element) that the cast will render on. Use with caution");
                ImGui.InputFloat2("Top Left", ref topLeftVert);
                ImGui.InputFloat2("Top Right", ref topRightVert);
                ImGui.InputFloat2("Bottom Left", ref bottomLeftVert);
                ImGui.InputFloat2("Bottom Right", ref bottomRightVert);

            }
            if (ImGui.CollapsingHeader("Transform"))
            {
                ImGui.InputFloat("Rotation", ref rotation);
                ImGui.SetItemTooltip("Rotation in degrees.");
                ImGui.InputFloat2("Origin", ref origin);
                ImGui.SetItemTooltip("Value used to offset the position of the cast (translation), this cannot be changed by animations.");
                ImGui.InputFloat2("Translation", ref translation);
                ImGui.SetItemTooltip("Position of the cast.");
                ImGui.InputFloat2("Scale", ref scale);
            }
            if (ImGui.CollapsingHeader("Color"))
            {
                ImGui.ColorEdit4("Color", ref color);
                ImGui.ColorEdit4("Top Left", ref colorTL);
                ImGui.ColorEdit4("Top Right", ref colorTR);
                ImGui.ColorEdit4("Bottom Left", ref colorBL);
                ImGui.ColorEdit4("Bottom Right", ref colorBR);
            }
            if (ImGui.CollapsingHeader("Inheritance"))
            {
                ImGui.Checkbox("Inherit Horizontal Position", ref inheritPosX);
                ImGui.Checkbox("Inherit Vertical Position", ref inheritPosY);
                ImGui.Checkbox("Inherit Rotation", ref inheritRot);
                ImGui.Checkbox("Inherit Color", ref inheritCol);
                ImGui.Checkbox("Inherit Width", ref inheritScaleX);
                ImGui.Checkbox("Inherit Height", ref inheritScaleY);
            }

            if (type == 2)
            {
                if (ImGui.CollapsingHeader("Text"))
                {
                    //make combo box eventually
                    if (ImGui.Combo("Font", ref indexFont, fontNames.ToArray(), fontNames.Count))
                    {
                        fontname = fontNames[indexFont];
                    }
                    ImGui.PushID("textInput");
                    ImGui.InputText("Text", ref text, 512);
                    ImGui.PopID();
                    ImGui.DragFloat("Kerning", ref kerning, 0.0005f);
                }
            }
            if (ImGui.CollapsingHeader("Material"))
            {
                ImGui.BeginDisabled(type != (int)DrawType.Sprite);
                ImGui.InputInt("Selected Sprite", ref spriteIndex);
                spriteIndex = Math.Clamp(spriteIndex, -1, 32); //can go over 32 for scu
                if (ImGui.BeginListBox("##listpatterns", new System.Numerics.Vector2(-1, 160)))
                {
                    //Draw Pattern selector
                    for (int i = 0; i < selectedCast.SpriteIndices.Length; i++)
                    {
                        int patternIdx = Math.Min(selectedCast.SpriteIndices.Length - 1, (int)selectedCast.SpriteIndices[i]);

                        //Draw button with a different color if its the currently active pattern
                        if (i == spriteIndex) ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(200, 200, 200, 255));
                        // Draw sprite preview if it isnt set to the default square
                        if (patternIdx == -1)
                        {
                            EmptyTextureButton(i);
                        }
                        else
                        {
                            Sprite spriteReference = SpriteHelper.TryGetSprite(selectedCast.SpriteIndices[i]);
                            if (spriteReference != null)
                            {

                                ShurikenRenderer.Vector2 uvTL = new ShurikenRenderer.Vector2(
                                spriteReference.Start.X / spriteReference.Texture.Width,
                                -(spriteReference.Start.Y / spriteReference.Texture.Height));

                                ShurikenRenderer.Vector2 uvBR = uvTL + new ShurikenRenderer.Vector2(
                                spriteReference.Dimensions.X / spriteReference.Texture.Width,
                                -(spriteReference.Dimensions.Y / spriteReference.Texture.Height));

                                if (spriteReference.Texture.GlTex == null)
                                {
                                    EmptyTextureButton(i);
                                }
                                else
                                {
                                    //This is so stupid, this is how youre supposed to do it according to the HexaNET issues
                                    unsafe
                                    {
                                        const int bufferSize = 256;
                                        byte* buffer = stackalloc byte[bufferSize];
                                        StrBuilder sb = new(buffer, bufferSize);
                                        sb.Append($"##pattern{i}");
                                        sb.End();
                                        //Draw sprite
                                        ImGui.ImageButton(sb, new ImTextureID(spriteReference.Texture.GlTex.ID), new System.Numerics.Vector2(50, 50), uvTL, uvBR);

                                    }
                                }
                            }
                            if (i != selectedCast.SpriteIndices.Length - 1)
                                ImGui.SameLine();
                        }
                        if (i == spriteIndex)
                            ImGui.PopStyleColor();
                    }
                    ImGui.EndListBox();
                }
                ImGui.EndDisabled();
            }
            selectedCast.Name = name;
            selectedCast.Field00 = (uint)field00;
            selectedCast.Field04 = (uint)type;
            selectedCast.Enabled = enabled;
            info.HideFlag = (uint)(hideflag ? 1 : 0);
            //ADD EDIT FOR HIDE FLAG
            if (mirrorX) materialFlags |= ElementMaterialFlags.MirrorX; else materialFlags &= ~ElementMaterialFlags.MirrorX;
            if (mirrorY) materialFlags |= ElementMaterialFlags.MirrorY; else materialFlags &= ~ElementMaterialFlags.MirrorY;
            selectedCast.Width = (uint)rectSize.X;
            selectedCast.Height = (uint)rectSize.Y;
            selectedCast.TopLeft = topLeftVert;
            selectedCast.TopRight = topRightVert;
            selectedCast.BottomLeft = bottomLeftVert;
            selectedCast.BottomRight = bottomRightVert;
            info.Rotation = rotation;
            selectedCast.Origin = origin;
            info.Translation = translation;
            //info.Color = color.ToSharpNeedleColorInverted();
            //info.GradientTopLeft = colorTL.ToSharpNeedleColorInverted();
            //info.GradientTopRight = colorTR.ToSharpNeedleColorInverted();
            //info.GradientBottomLeft = colorBL.ToSharpNeedleColorInverted();
            //info.GradientBottomRight = colorBR.ToSharpNeedleColorInverted();
            info.SpriteIndex = spriteIndex;
            info.Scale = scale;

            if (inheritPosX) inheritanceFlags |= ElementInheritanceFlags.InheritXPosition; else inheritanceFlags &= ~ElementInheritanceFlags.InheritXPosition;
            if (inheritPosY) inheritanceFlags |= ElementInheritanceFlags.InheritYPosition; else inheritanceFlags &= ~ElementInheritanceFlags.InheritYPosition;
            if (inheritRot) inheritanceFlags |= ElementInheritanceFlags.InheritRotation; else inheritanceFlags &= ~ElementInheritanceFlags.InheritRotation;
            if (inheritCol) inheritanceFlags |= ElementInheritanceFlags.InheritColor; else inheritanceFlags &= ~ElementInheritanceFlags.InheritColor;
            if (inheritScaleX) inheritanceFlags |= ElementInheritanceFlags.InheritScaleX; else inheritanceFlags &= ~ElementInheritanceFlags.InheritScaleX;
            if (inheritScaleY) inheritanceFlags |= ElementInheritanceFlags.InheritScaleY; else inheritanceFlags &= ~ElementInheritanceFlags.InheritScaleY;
            selectedCast.InheritanceFlags = (uint)inheritanceFlags;
            selectedCast.Info = info;
            selectedCast.Field38 = (uint)materialFlags;
            selectedCast.FontName = fontname;
            selectedCast.Text = text;
            selectedCast.Field4C = (uint)BitConverter.ToInt32(BitConverter.GetBytes(-kerning), 0);
        }
        public override void Update(ShurikenRenderHelper in_Proj)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2((ImGui.GetWindowViewport().Size.X / 4.5f) * 3.5f, MenuBarWindow.menuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetWindowViewport().Size.X / 4.5f, ImGui.GetWindowViewport().Size.Y - MenuBarWindow.menuBarHeight), ImGuiCond.Always);
            if (ImGui.Begin("Inspector", MainWindow.flags))
            {
                if (in_Proj != null)
                {
                    fontNames.Clear();
                    foreach (var font in in_Proj.WorkProjectCsd.Project.Fonts)
                    {
                        fontNames.Add(font.Key);
                    }
                    switch (eSelectionType)
                    {
                        case ESelectionType.Scene:
                            {
                                DrawSceneInspector();
                                break;
                            }
                        case ESelectionType.Cast:
                            {
                                DrawCastInspector();
                                break;
                            }
                    }
                }
                ImGui.End();
            }
        }
        internal static void Reset()
        {
            ShurikenRenderHelper.Instance.selectionData.SelectedCast = null;
            ShurikenRenderHelper.Instance.selectionData.SelectedScene = new();
        }
    }
}