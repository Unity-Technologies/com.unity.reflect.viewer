using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ReflectViewerCoreEditorTests
{
    public class ObjectNamesTests
    {
        [Test]
        public void ObjectNamesTests_NicifyVariableName_RemovesPrefixes()
        {
            // m_ prefixes
            Assert.That("" == ObjectNames.NicifyVariableName("m_"));

            Assert.That("A" == ObjectNames.NicifyVariableName("m_A"));
            Assert.That("Mesh" == ObjectNames.NicifyVariableName("m_Mesh"));

            // m without _ is not treated as a prefix
            Assert.That("M Mesh" == ObjectNames.NicifyVariableName("mMesh"));

            // _ prefixes
            Assert.That("" == ObjectNames.NicifyVariableName("_"));
            Assert.That("A" == ObjectNames.NicifyVariableName("_A"));
            Assert.That("Mesh" == ObjectNames.NicifyVariableName("_Mesh"));

            // k prefixes
            Assert.That("A" == ObjectNames.NicifyVariableName("kA"));
            Assert.That("Foo" == ObjectNames.NicifyVariableName("kFoo"));
            // should not remove it when k is not followed by uppercase letters
            Assert.That("Ka" == ObjectNames.NicifyVariableName("ka"));
            Assert.That("K0" == ObjectNames.NicifyVariableName("k0"));
            Assert.That("K!" == ObjectNames.NicifyVariableName("k!"));
        }

        [Test]
        public void ObjectNamesTests_NicifyVariableName_MakesFirstCharacterUppercase()
        {
            Assert.That("A" == ObjectNames.NicifyVariableName("a"));
            Assert.That("Aaa" == ObjectNames.NicifyVariableName("aaa"));
            // non-letters
            Assert.That("" == ObjectNames.NicifyVariableName(""));
            Assert.That("0" == ObjectNames.NicifyVariableName("0"));
            Assert.That("!" == ObjectNames.NicifyVariableName("!"));
            Assert.That("{}" == ObjectNames.NicifyVariableName("{}"));
        }

        [Test]
        public void ObjectNamesTests_NicifyVariableName_AddsSpaces_BetweenWords()
        {
            Assert.That("Foo" == ObjectNames.NicifyVariableName("Foo"));
            Assert.That("Foo Bar" == ObjectNames.NicifyVariableName("FooBar"));
            Assert.That("Foo 0" == ObjectNames.NicifyVariableName("Foo0"));
            Assert.That("Foo 0bar" == ObjectNames.NicifyVariableName("Foo0bar"));
        }

        [Test]
        public void ObjectNamesTests_NicifyVariableName_DoesNotAddSpaces_BetweenCapitalLetters()
        {
            Assert.That("AB" == ObjectNames.NicifyVariableName("AB"));
            Assert.That("Foo XYZ" == ObjectNames.NicifyVariableName("FooXYZ"));
            Assert.That("Foo 0AB Cdef G" == ObjectNames.NicifyVariableName("Foo0ABCdefG"));
        }

        [Test]
        public void ObjectNamesTests_NicifyVariableName_DoesNotAddSpaces_BetweenExistingSpaces()
        {
            Assert.That("A B" == ObjectNames.NicifyVariableName("A B"));
            Assert.That("Foo XYZ" == ObjectNames.NicifyVariableName("Foo XYZ"));
            Assert.That("DX 11 Shader" == ObjectNames.NicifyVariableName("DX 11 Shader"));
        }

        [Test]
        public void ObjectNamesTests_NicifyVariableName_DoesNotAddSpace_NxNPatterns()
        {
            Assert.That("1x2" == ObjectNames.NicifyVariableName("m_1x2"));
            Assert.That("3x4" == ObjectNames.NicifyVariableName("3x4"));
            // not NxN patterns
            Assert.That("1xx 2" == ObjectNames.NicifyVariableName("1xx2"));
            Assert.That("X2" == ObjectNames.NicifyVariableName("x2"));
            Assert.That("Ax B" == ObjectNames.NicifyVariableName("AxB"));
            Assert.That("1x" == ObjectNames.NicifyVariableName("1x"));
            Assert.That("X 1x" == ObjectNames.NicifyVariableName("x1x"));
            Assert.That("1#x 1x" == ObjectNames.NicifyVariableName("1#x1x"));
        }

        [Test]
        public void ObjectNamesTests_NicifyVariableName_HandlesKnownPrefixes_Nicely()
        {
            // iDevices
            Assert.That("iOS" == ObjectNames.NicifyVariableName("iOS"));
            Assert.That("Foo iOS" == ObjectNames.NicifyVariableName("foo iOS"));
            Assert.That("iPad + iPhone" == ObjectNames.NicifyVariableName("iPad + iPhone"));
            Assert.That("iPhone, iPod" == ObjectNames.NicifyVariableName("iPhone, iPod"));
            Assert.That("iPhone,iPod" == ObjectNames.NicifyVariableName("iPhone,iPod"));

            // ARM
            Assert.That("ARM" == ObjectNames.NicifyVariableName("ARM"));
            Assert.That("ARMv7" == ObjectNames.NicifyVariableName("ARMv7"));

            // x86/x64
            Assert.That("x86" == ObjectNames.NicifyVariableName("x86"));
            Assert.That("x64" == ObjectNames.NicifyVariableName("x64"));

            // check that names that are similar to known "do not touch" names are still mangled
            Assert.That("Hai Phone" == ObjectNames.NicifyVariableName("haiPhone"));
            Assert.That("Di OS" == ObjectNames.NicifyVariableName("DiOS"));
            Assert.That("I Pooh" == ObjectNames.NicifyVariableName("iPooh"));
            Assert.That("IP" == ObjectNames.NicifyVariableName("iP"));
            Assert.That("IO" == ObjectNames.NicifyVariableName("iO"));
            Assert.That("I Out" == ObjectNames.NicifyVariableName("iOut"));
            Assert.That("IO Source" == ObjectNames.NicifyVariableName("IOSource"));

            Assert.That("POLEARM" == ObjectNames.NicifyVariableName("POLEARM"));
            Assert.That("AR" == ObjectNames.NicifyVariableName("aR"));
            Assert.That("Arm" == ObjectNames.NicifyVariableName("arm"));

            Assert.That("X643" == ObjectNames.NicifyVariableName("x643"));
            Assert.That("Ox 86" == ObjectNames.NicifyVariableName("ox86"));
        }

        [Test]
        public void ObjectNamesTests_NicifyVariableName_HandlesNumberSuffixes_Nicely()
        {
            // all-lowercase suffixes after numbers should not get spaces inserted,
            // (e.g. "720p" should not turn into "72 0p".
            Assert.That("720p" == ObjectNames.NicifyVariableName("720p"));
            Assert.That("100cm" == ObjectNames.NicifyVariableName("100cm"));
            Assert.That("Foo 1{}" == ObjectNames.NicifyVariableName("foo1{}"));
            Assert.That("Foo 10{}" == ObjectNames.NicifyVariableName("foo10{}"));
            Assert.That("HD (1080p)" == ObjectNames.NicifyVariableName("HD (1080p)"));
            Assert.That("10mm (5px)" == ObjectNames.NicifyVariableName("10mm (5px)"));
            Assert.That("16px Margin" == ObjectNames.NicifyVariableName("16pxMargin"));
        }

        [Test]
        public void ObjectNamesTests_NicifyVariableName_WorksNice_OnCommonUnityNames()
        {
            // component names
            Assert.That("Game Object" == ObjectNames.NicifyVariableName("GameObject"));
            Assert.That("Texture 2D" == ObjectNames.NicifyVariableName("Texture2D"));
            Assert.That("Physics 2D Settings" == ObjectNames.NicifyVariableName("Physics2DSettings"));
            Assert.That("Rigidbody 2D" == ObjectNames.NicifyVariableName("Rigidbody2D"));
            Assert.That("GUI Layer" == ObjectNames.NicifyVariableName("GUILayer"));
            Assert.That("GUI Text" == ObjectNames.NicifyVariableName("GUIText"));
            Assert.That("LOD Group" == ObjectNames.NicifyVariableName("LODGroup"));

            // names found in enum popup menus;
            // special case in code for NxN patterns
            Assert.That("PCF 5x5" == ObjectNames.NicifyVariableName("PCF 5x5"));

            // names found in our image effects
            Assert.That("Camera FOV" == ObjectNames.NicifyVariableName("m_CameraFOV"));
            Assert.That("Use DX11" == ObjectNames.NicifyVariableName("m_UseDX11"));
            Assert.That("Ao Shader" == ObjectNames.NicifyVariableName("aoShader"));
            Assert.That("Support HDR Textures" == ObjectNames.NicifyVariableName("supportHDRTextures"));
            Assert.That("Shader FXAA Preset 3" == ObjectNames.NicifyVariableName("shaderFXAAPreset3"));
            Assert.That("Flare Color A" == ObjectNames.NicifyVariableName("m_FlareColorA"));
            Assert.That("Dx 11 Shader" == ObjectNames.NicifyVariableName("dx11Shader"));
            Assert.That("Soft Z Distance" == ObjectNames.NicifyVariableName("softZDistance"));
            Assert.That("Z Curve" == ObjectNames.NicifyVariableName("zCurve"));
        }

        [Test]
        public void ObjectNamesTests_NicifyVariableName_GeneratedName()
        {
            Assert.That("Generated Name" == ObjectNames.NicifyVariableName("<GeneratedName>k__BackingField"));
            Assert.That("Local Function g__Func Name" == ObjectNames.NicifyVariableName("<LocalFunction>g__FuncName"));
            Assert.That("Local Function g__Func Name|0_0" == ObjectNames.NicifyVariableName("<LocalFunction>g__FuncName|0_0"));
        }
    }
}
